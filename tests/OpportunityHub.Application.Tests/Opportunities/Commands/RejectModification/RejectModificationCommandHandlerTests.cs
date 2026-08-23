using OpportunityHub.Application.Opportunities.Commands.RejectModification;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.RejectModification;

public sealed class RejectModificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectModificationCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var opportunityId = Guid.NewGuid();

        var command = new RejectModificationCommand(
            opportunityId,
            "The requested modification is not acceptable.");

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
    public async Task Handle_WhenFirstPublicationModificationIsPendingManagerReview_RejectModificationIsNotAllowed()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        // Manager requests changes to the first publication.
        opportunity.RequestModification(
            [
                (
                    "Description",
                    "Please provide additional information.")
            ],
            "manager-user");

        Assert.Equal(
            OpportunityStatusCode.PendingSpecialistModification,
            opportunity.StatusCode);

        // Specialist submits the changes for manager review.
        opportunity.SubmitForManagerReview(
            OpportunityFactory.CreateContent(),
            "specialist-user");

        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectModificationCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new RejectModificationCommand(
            opportunity.Id,
            "The proposed modification is rejected.");

        // Act & Assert
        await Assert.ThrowsAsync<WorkflowTransitionNotAllowedException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenModificationOfPublishedOpportunityIsRejected_RestoresPublishedStateAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublishedModificationPendingManagerReview();

        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PendingManagerReview,
            opportunity.SubStatusCode);

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectModificationCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new RejectModificationCommand(
            opportunity.Id,
            "The proposed modification is rejected.");

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

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPublishedModifiedOpportunityModificationIsRejected_RestoresPublishedModifiedState()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublished();

        // Complete and publish the first modification cycle.
        opportunity.SubmitForManagerReview(
            OpportunityFactory.CreateContent(),
            "specialist-user",
            "First modification");

        opportunity.Approve("manager-user");
        opportunity.Publish("manager-user");

        Assert.Equal(
            OpportunityStatusCode.Published,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PublishedModified,
            opportunity.SubStatusCode);

        // Start a second modification cycle.
        opportunity.SubmitForManagerReview(
            OpportunityFactory.CreateContent(),
            "specialist-user",
            "Second modification");

        opportunity.RequestModification(
            [
                (
                    "Description",
                    "Please provide additional information.")
            ],
            "manager-user");

        opportunity.SubmitForManagerReview(
            OpportunityFactory.CreateContent(),
            "specialist-user",
            "Second modification");

        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PendingManagerReview,
            opportunity.SubStatusCode);

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectModificationCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new RejectModificationCommand(
            opportunity.Id,
            "The second modification is rejected.");

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

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublishedModificationPendingManagerReview();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectModificationCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new RejectModificationCommand(
            opportunity.Id,
            "Modification rejected.");

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