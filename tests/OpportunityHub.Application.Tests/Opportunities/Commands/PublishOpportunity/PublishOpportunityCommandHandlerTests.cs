using OpportunityHub.Application.Opportunities.Commands.PublishOpportunity;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.PublishOpportunity;

public sealed class PublishOpportunityCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("publisher-user");

        var handler = new PublishOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var opportunityId = Guid.NewGuid();

        var command = new PublishOpportunityCommand(
            opportunityId);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

        // Assert
        Assert.Equal(
            $"Opportunity '{opportunityId}' was not found.",
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOpportunityIsApproved_PublishesOpportunityAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateApproved();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("publisher-user");

        var handler = new PublishOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new PublishOpportunityCommand(
            opportunity.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Published,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.True(
            opportunity.IsPublished);

        Assert.NotNull(
            opportunity.PublishedAtUtc);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPublishedModificationIsApproved_PublishesModificationAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublishedModificationPendingManagerReview();

        opportunity.Approve(
            "manager-user");

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("publisher-user");

        var handler = new PublishOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new PublishOpportunityCommand(
            opportunity.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Published,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PublishedModified,
            opportunity.SubStatusCode);

        Assert.True(
            opportunity.IsPublished);

        Assert.NotNull(
            opportunity.PublishedAtUtc);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOpportunityIsDraft_ThrowsWorkflowTransitionNotAllowedException()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateDraft();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("publisher-user");

        var handler = new PublishOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new PublishOpportunityCommand(
            opportunity.Id);

        // Act & Assert
        await Assert.ThrowsAsync<WorkflowTransitionNotAllowedException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(
            OpportunityStatusCode.Draft,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.False(
            opportunity.IsPublished);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateApproved();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("publisher-user");

        var handler = new PublishOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new PublishOpportunityCommand(
            opportunity.Id);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act
        await handler.Handle(
            command,
            cancellationTokenSource.Token);

        // Assert
        Assert.Equal(
            cancellationTokenSource.Token,
            repository.LastCancellationToken);
    }
}