using OpportunityHub.Application.Opportunities.Queries.GetOpportunityById;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.Opportunities.Queries.GetOpportunityById;

public sealed class GetOpportunityByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityExists_ReturnsOpportunityResponse()
    {
        // Arrange
        var opportunity = OpportunityFactory.CreateDraft();

        var repository = new FakeOpportunityRepository();

        repository.Add(opportunity);

        var handler = new GetOpportunityByIdQueryHandler(
            repository);

        var query = new GetOpportunityByIdQuery(
            opportunity.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            opportunity.Id,
            result.Id);

        Assert.Equal(
            OpportunityStatusCode.Draft,
            result.StatusCode);

        Assert.Null(result.SubStatusCode);

        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();

        var handler = new GetOpportunityByIdQueryHandler(
            repository);

        var query = new GetOpportunityByIdQuery(
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenPublishedOpportunityExists_ReturnsPublishedOpportunityResponse()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublished();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var handler = new GetOpportunityByIdQueryHandler(
            repository);

        var query = new GetOpportunityByIdQuery(
            opportunity.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            opportunity.Id,
            result.Id);

        Assert.Equal(
            OpportunityStatusCode.Published,
            result.StatusCode);

        Assert.Null(
            result.SubStatusCode);

        Assert.Equal(
            opportunity.QrCodeReference,
            result.QrCodeReference);

        Assert.NotNull(
            result.PublishedAtUtc);

        Assert.Equal(
            opportunity.PublishedAtUtc,
            result.PublishedAtUtc);

        Assert.True(
            result.IsActive);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateDraft();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var handler = new GetOpportunityByIdQueryHandler(
            repository);

        var query = new GetOpportunityByIdQuery(
            opportunity.Id);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act
        await handler.Handle(
            query,
            cancellationTokenSource.Token);

        // Assert
        Assert.Equal(
            cancellationTokenSource.Token,
            repository.LastCancellationToken);
    }
}