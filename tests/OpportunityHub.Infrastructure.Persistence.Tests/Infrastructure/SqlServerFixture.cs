using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpportunityHub.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private const string SqlServerAlias = "sqlserver";

    private readonly INetwork _network;
    private readonly MsSqlContainer _sqlServer;
    private readonly IContainer _liquibase;

    public SqlServerFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SqlServerFixture>()
            .AddEnvironmentVariables()
            .Build();

        var password = configuration["TestDatabase:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "TestDatabase:Password is not configured.");
        }

        var repositoryRoot = FindRepositoryRoot();

        var dbManagerPath = Path.Combine(
            repositoryRoot,
            "src",
            "OpportunityHub.DbManager");

        if (!Directory.Exists(dbManagerPath))
        {
            throw new DirectoryNotFoundException(
                $"OpportunityHub.DbManager directory was not found: {dbManagerPath}");
        }

        _network = new NetworkBuilder()
            .WithName($"opportunityhub-tests-{Guid.NewGuid():N}")
            .Build();

        _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(password)
            .WithDatabase("OpportunityHubDb")
            .WithNetwork(_network)
            .WithNetworkAliases(SqlServerAlias)
            .Build();

        _liquibase = new ContainerBuilder("opportunityhub-liquibase:5.0.2")
            .WithNetwork(_network)
            .WithBindMount(
                dbManagerPath,
                "/liquibase/changelog")
            .WithCommand(
                "--classpath=/liquibase/lib/mssql-jdbc.jar",
                "--url=jdbc:sqlserver://sqlserver:1433;databaseName=OpportunityHubDb;encrypt=false;trustServerCertificate=true",
                "--username=sa",
                $"--password={password}",
                "--search-path=/liquibase/changelog",
                "--changelog-file=master.xml",
                "update")
            .Build();
    }

    public string ConnectionString =>
        _sqlServer.GetConnectionString();

    public OpportunityHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OpportunityHubDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new OpportunityHubDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _network.CreateAsync();

        await _sqlServer.StartAsync();

        await _liquibase.StartAsync();

        var exitCode = await _liquibase.GetExitCodeAsync();

        if (exitCode != 0)
        {
            var (stdout, stderr) = await _liquibase.GetLogsAsync();

            Console.WriteLine("===== LIQUIBASE STDOUT =====");
            Console.WriteLine(stdout);

            Console.WriteLine("===== LIQUIBASE STDERR =====");
            Console.WriteLine(stderr);

            throw new InvalidOperationException(
                $"Liquibase failed with exit code {exitCode}.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _liquibase.DisposeAsync();
        await _sqlServer.DisposeAsync();
        await _network.DeleteAsync();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionDirectory = Path.Combine(
                directory.FullName,
                "src",
                "OpportunityHub.DbManager");

            if (Directory.Exists(solutionDirectory))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the OpportunityHub repository root.");
    }
}
