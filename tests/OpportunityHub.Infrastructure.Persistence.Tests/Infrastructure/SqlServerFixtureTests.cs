using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OpportunityHub.Infrastructure.Persistence;
using Xunit;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

[Collection("Infrastructure Integration Tests")]
public sealed class SqlServerFixtureTests
{
    private readonly SqlServerFixture _fixture;

    public SqlServerFixtureTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DbContext_should_connect_to_liquibase_schema()
    {
        await using var db = _fixture.CreateDbContext();

        var canConnect = await db.Database.CanConnectAsync(
            TestContext.Current.CancellationToken);

        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_should_have_expected_tables()
    {
        await using var db = _fixture.CreateDbContext();

        var tables = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS [Value]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                ORDER BY TABLE_SCHEMA, TABLE_NAME
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        tables.Should().Contain("dbo.AuditHistory");
    }

    [Fact]
    public async Task Database_should_have_expected_liquibase_tables()
    {
        await using var db = _fixture.CreateDbContext();

        var tables = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS [Value]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        tables.Should().Contain("dbo.DATABASECHANGELOG");
        tables.Should().Contain("dbo.DATABASECHANGELOGLOCK");
    }

    [Fact]
    public async Task Liquibase_should_have_applied_changesets()
    {
        await using var db = _fixture.CreateDbContext();

        var count = await db.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS [Value]
                FROM dbo.DATABASECHANGELOG
                """)
            .SingleAsync(TestContext.Current.CancellationToken);

        count.Should().BeGreaterThan(0);
    }
}
