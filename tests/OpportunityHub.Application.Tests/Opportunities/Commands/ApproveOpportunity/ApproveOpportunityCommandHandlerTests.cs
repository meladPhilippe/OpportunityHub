using OpportunityHub.Application.Opportunities.Commands.ApproveOpportunity;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.ApproveOpportunity;

public sealed class ApproveOpportunityCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new ApproveOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var opportunityId = Guid.NewGuid();

        var command = new ApproveOpportunityCommand(
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
    public async Task Handle_WhenFirstPublicationIsPendingManagerReview_ApprovesAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new ApproveOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new ApproveOpportunityCommand(
            opportunity.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Approved,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPublishedModificationIsPendingManagerReview_ApprovesModificationAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublishedModificationPendingManagerReview();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new ApproveOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new ApproveOpportunityCommand(
            opportunity.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.Approved,
            opportunity.SubStatusCode);

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
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new ApproveOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new ApproveOpportunityCommand(
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

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new ApproveOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new ApproveOpportunityCommand(
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