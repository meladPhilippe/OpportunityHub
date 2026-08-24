using OpportunityHub.Application.Opportunities.Queries.GetSubmissionDetails;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Opportunities.Queries.GetSubmissionDetails;

public sealed class GetSubmissionDetailsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSubmissionDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository =
            new FakeSubmissionRepository();

        var handler =
            new GetSubmissionDetailsQueryHandler(
                repository);

        var submissionId =
            Guid.NewGuid();

        var query =
            new GetSubmissionDetailsQuery(
                submissionId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenSubmissionExists_ReturnsDetails()
    {
        // Arrange
        var submissionId =
            Guid.NewGuid();

        var opportunityVersionId =
            Guid.NewGuid();

        var modificationRequestId =
            Guid.NewGuid();

        var modificationRejectionId =
            Guid.NewGuid();

        var finalRejectionId =
            Guid.NewGuid();

        var submittedAtUtc =
            new DateTime(
                2026,
                8,
                23,
                10,
                30,
                0,
                DateTimeKind.Utc);

        var submission =
            new SubmissionDetails(
                submissionId,
                opportunityVersionId,
                3,
                SubmissionType.PublishedModification,
                "Update published opportunity",
                OpportunityStatusCode.PublishedUnderReview,
                OpportunitySubStatusCode.Approved,
                "specialist-user",
                submittedAtUtc,
                modificationRequestId,
                modificationRejectionId,
                finalRejectionId);

        var repository =
            new FakeSubmissionRepository();

        repository.Add(submission);

        var handler =
            new GetSubmissionDetailsQueryHandler(
                repository);

        var query =
            new GetSubmissionDetailsQuery(
                submissionId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            submissionId,
            result.Id);

        Assert.Equal(
            opportunityVersionId,
            result.OpportunityVersionId);

        Assert.Equal(
            3,
            result.SequenceNumber);

        Assert.Equal(
            SubmissionType.PublishedModification,
            result.SubmissionType);

        Assert.Equal(
            "Update published opportunity",
            result.EditSummary);

        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            result.PreviousStatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.Approved,
            result.PreviousSubStatusCode);

        Assert.Equal(
            "specialist-user",
            result.SubmittedBy);

        Assert.Equal(
            submittedAtUtc,
            result.SubmittedAtUtc);

        Assert.Equal(
            modificationRequestId,
            result.ModificationRequestId);

        Assert.Equal(
            modificationRejectionId,
            result.ModificationRejectionId);

        Assert.Equal(
            finalRejectionId,
            result.FinalRejectionId);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var submission =
            new SubmissionDetails(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                SubmissionType.FirstPublication,
                null,
                OpportunityStatusCode.Draft,
                null,
                "test-user",
                DateTime.UtcNow,
                null,
                null,
                null);

        var repository =
            new FakeSubmissionRepository();

        repository.Add(submission);

        var handler =
            new GetSubmissionDetailsQueryHandler(
                repository);

        var query =
            new GetSubmissionDetailsQuery(
                submission.Id);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        // Act
        await handler.Handle(
            query,
            cancellationToken);

        // Assert
        Assert.Equal(
            cancellationToken,
            repository.LastCancellationToken);
    }
}
