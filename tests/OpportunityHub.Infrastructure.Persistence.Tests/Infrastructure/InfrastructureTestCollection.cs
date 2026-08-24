using Xunit;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

[CollectionDefinition("Infrastructure Integration Tests")]
public sealed class InfrastructureTestCollection
    : ICollectionFixture<SqlServerFixture>
{
}
