using FluentAssertions;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

public sealed class SqlServerFixtureTests : IClassFixture<SqlServerFixture>
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

        var canConnect = await db.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }
}
