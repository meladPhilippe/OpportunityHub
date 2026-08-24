using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Schema;

public sealed class LiquibaseSchemaTests
    : IClassFixture<OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture>
{
    private readonly OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture _fixture;

    public LiquibaseSchemaTests(
        OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Liquibase_should_create_expected_tables()
    {
        await using var db = _fixture.CreateDbContext();

        var tables = await db.Database
            .SqlQuery<string>($"""
                SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS [Value]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        tables.Should().Contain("dbo.Opportunity");
    }
}
