using OpportunityHub.Application.Opportunities.Commands.SubmitForManagerReview;
using OpportunityHub.Application.Opportunities.Models;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.SubmitForManagerReview;

public sealed class SubmitForManagerReviewCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("test-user");

        var handler = new SubmitForManagerReviewCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var opportunityId = Guid.NewGuid();

        var command = new SubmitForManagerReviewCommand(
            opportunityId,
            CreateContentRequest());

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
    public async Task Handle_WhenOpportunityIsDraft_SubmitsForManagerReviewAndSaves()
    {
        // Arrange
        var opportunity = OpportunityFactory.CreateDraft();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("reviewer-user");

        var handler = new SubmitForManagerReviewCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new SubmitForManagerReviewCommand(
            opportunity.Id,
            CreateContentRequest());

        // Act
        var submissionId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            submissionId);

        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.Equal(
            1,
            opportunity.LastSubmissionSequenceNumber);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenFirstPublicationModificationIsPendingSpecialistModification_SubmitsForManagerReview()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        opportunity.RequestModification(
            [
                (
                    "Description",
                    "Please provide additional information.")
            ],
            "manager-user");

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("specialist-user");

        var handler = new SubmitForManagerReviewCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new SubmitForManagerReviewCommand(
            opportunity.Id,
            CreateContentRequest());

        // Act
        var submissionId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            submissionId);

        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.Equal(
            2,
            opportunity.LastSubmissionSequenceNumber);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPublishedModificationIsPendingSpecialistModification_SubmitsForManagerReview()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublished();

        opportunity.SubmitForManagerReview(
            OpportunityFactory.CreateContent(),
            "specialist-user",
            "Update published opportunity");

        opportunity.RequestModification(
            [
                (
                    "Description",
                    "Please provide additional information.")
            ],
            "manager-user");

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("specialist-user");

        var handler = new SubmitForManagerReviewCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new SubmitForManagerReviewCommand(
            opportunity.Id,
            CreateContentRequest());

        // Act
        var submissionId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            submissionId);

        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PendingManagerReview,
            opportunity.SubStatusCode);

        Assert.Equal(
            3,
            opportunity.LastSubmissionSequenceNumber);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunity = OpportunityFactory.CreateDraft();

        var repository = new FakeOpportunityRepository();
        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("test-user");

        var handler = new SubmitForManagerReviewCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var command = new SubmitForManagerReviewCommand(
            opportunity.Id,
            CreateContentRequest());

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

    private static OpportunityVersionContentRequest CreateContentRequest()
    {
        return new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Updated Opportunity",
                "فرصة محدثة")
        };
    }
}
