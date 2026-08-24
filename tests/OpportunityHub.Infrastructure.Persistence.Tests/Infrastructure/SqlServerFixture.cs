using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        _liquibase = new ContainerBuilder("liquibase/liquibase:5.0.3")
            .WithNetwork(_network)
            .WithBindMount(
                dbManagerPath,
                "/liquibase/changelog")
            .WithBindMount(
                Path.Combine(dbManagerPath, "mssql-jdbc.jar"),
                "/liquibase/lib/mssql-jdbc.jar")
            .WithCommand(
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
                $"Liquibase failed with exit code {exitCode}.\\n" +
                $"===== LIQUIBASE STDOUT =====\\n{stdout}\\n" +
                $"===== LIQUIBASE STDERR =====\\n{stderr}");
        }

        await SeedReferenceDataAsync();

        Console.WriteLine("===== REFERENCE DATA SEEDED =====");

        Console.WriteLine("===== REFERENCE DATA SEEDED =====");
    }

    private async Task SeedReferenceDataAsync()
    {
        await using var db = CreateDbContext();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Channel
            (
                Id,
                Code,
                NameEn,
                NameAr,
                SortOrder,
                IsActive,
                CreatedBy
            )
            VALUES
            (
                '11111111-1111-1111-1111-111111111111',
                1,
                N'Test Channel 1',
                N'قناة اختبار 1',
                1,
                1,
                N'integration-test'
            ),
            (
                '22222222-2222-2222-2222-222222222222',
                2,
                N'Test Channel 2',
                N'قناة اختبار 2',
                2,
                1,
                N'integration-test'
            );

            INSERT INTO dbo.Sector
            (
                Id,
                Code,
                NameEn,
                NameAr,
                SortOrder,
                IsActive,
                CreatedBy
            )
            VALUES
            (
                '33333333-3333-3333-3333-333333333333',
                1,
                N'Test Sector 1',
                N'قطاع اختبار 1',
                1,
                1,
                N'integration-test'
            ),
            (
                '44444444-4444-4444-4444-444444444444',
                2,
                N'Test Sector 2',
                N'قطاع اختبار 2',
                2,
                1,
                N'integration-test'
            );
            """,
            TestContext.Current.CancellationToken);
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
