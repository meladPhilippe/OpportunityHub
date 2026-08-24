using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

public sealed class LiquibaseSchemaTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public LiquibaseSchemaTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Liquibase_should_create_all_expected_tables()
    {
        await using var db = _fixture.CreateDbContext();

        var tables = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT TABLE_NAME AS [Value]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_TYPE = 'BASE TABLE'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        var expectedTables = new[]
        {
            "Opportunity",
            "OpportunityVersion",
            "Channel",
            "OpportunityVersionChannel",
            "Sector",
            "OpportunityVersionSector",
            "OpportunityVersionFeature",
            "OpportunityVersionKeyAchievement",
            "OpportunityVersionKpi",
            "Submission",
            "ModificationRequest",
            "ModificationRequestItem",
            "ModificationRejection",
            "RejectionReason",
            "FinalRejection",
            "AuditHistory"
        };

        tables.Should().Contain(expectedTables);
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

    [Fact]
    public async Task Liquibase_should_have_change_log_lock_table()
    {
        await using var db = _fixture.CreateDbContext();

        var exists = await db.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS [Value]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = 'DATABASECHANGELOGLOCK'
                """)
            .SingleAsync(TestContext.Current.CancellationToken);

        exists.Should().Be(1);
    }

    [Fact]
    public async Task Liquibase_should_create_expected_primary_keys()
    {
        await using var db = _fixture.CreateDbContext();

        var primaryKeys = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT tc.TABLE_NAME + '.' + tc.CONSTRAINT_NAME AS [Value]
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                  AND tc.TABLE_SCHEMA = 'dbo'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        primaryKeys.Should().Contain(x =>
            x.StartsWith("Opportunity.", StringComparison.OrdinalIgnoreCase));

        primaryKeys.Should().Contain(x =>
            x.StartsWith("OpportunityVersion.", StringComparison.OrdinalIgnoreCase));

        primaryKeys.Should().Contain(x =>
            x.StartsWith("Submission.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Liquibase_should_create_foreign_keys()
    {
        await using var db = _fixture.CreateDbContext();

        var foreignKeys = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT fk.name AS [Value]
                FROM sys.foreign_keys fk
                INNER JOIN sys.tables t
                    ON t.object_id = fk.parent_object_id
                WHERE SCHEMA_NAME(t.schema_id) = 'dbo'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        foreignKeys.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Liquibase_should_create_unique_constraints_or_indexes()
    {
        await using var db = _fixture.CreateDbContext();

        var uniqueIndexes = await db.Database
            .SqlQueryRaw<string>(
                """
                SELECT i.name AS [Value]
                FROM sys.indexes i
                INNER JOIN sys.tables t
                    ON t.object_id = i.object_id
                WHERE i.is_unique = 1
                  AND SCHEMA_NAME(t.schema_id) = 'dbo'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        uniqueIndexes.Should().NotBeEmpty();
    }
}
