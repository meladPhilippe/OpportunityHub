using OpportunityHub.Application.Opportunities.Commands.RejectOpportunity;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.RejectOpportunity;

public sealed class RejectOpportunityCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var opportunityId = Guid.NewGuid();

        var command = new RejectOpportunityCommand(
            opportunityId,
            1,
            "The opportunity does not meet the requirements.");

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
    public async Task Handle_WhenOpportunityIsPendingManagerReview_RejectsAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("manager-user");

        var handler =
            new RejectOpportunityCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        const int rejectionReasonId = 5;

        const string comment =
            "The opportunity does not meet the requirements.";

        var command = new RejectOpportunityCommand(
         opportunity.Id,
         rejectionReasonId,
         comment);


        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Rejected,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);

        var submission = Assert.Single(opportunity.Submissions);

        Assert.NotNull(
            submission.FinalRejection);

        Assert.Equal(
            rejectionReasonId,
            submission.FinalRejection.RejectionReasonId);

        Assert.Equal(
            comment,
            submission.FinalRejection.Comment);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenWorkflowActionIsNotAllowed_ThrowsDomainException()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateDraft();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("manager-user");

        var handler =
            new RejectOpportunityCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new RejectOpportunityCommand(
                opportunity.Id,
                1,
                "Invalid rejection.");

        // Act & Assert
        await Assert.ThrowsAsync<WorkflowTransitionNotAllowedException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

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

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("manager-user");

        var handler =
            new RejectOpportunityCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new RejectOpportunityCommand(
                opportunity.Id,
                1,
                "The opportunity does not meet the requirements.");

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

    [Fact]
    public async Task Handle_WhenPublishedModificationIsPendingManagerReview_ThrowsWorkflowTransitionNotAllowedException()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublishedModificationPendingManagerReview();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RejectOpportunityCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new RejectOpportunityCommand(
            opportunity.Id,
            5,
            "The published modification does not meet the requirements.");

        // Act & Assert
        await Assert.ThrowsAsync<WorkflowTransitionNotAllowedException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        // The published opportunity must remain under review.
        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PendingManagerReview,
            opportunity.SubStatusCode);

        // No final rejection should have been created.
        var submission =
            opportunity.Submissions
                .OrderByDescending(s => s.SequenceNumber)
                .First();

        Assert.Null(
    submission.FinalRejection);
        Assert.Null(
            submission.FinalRejection);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

}