using OpportunityHub.Application.Opportunities.Queries.GetFinalRejection;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Opportunities.Queries.GetFinalRejection;

public sealed class GetFinalRejectionQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenFinalRejectionExists_ReturnsDetails()
    {
        // Arrange
        var finalRejectionId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        const int rejectionReasonId = 42;

        var createdAtUtc =
            new DateTime(
                2026,
                8,
                23,
                10,
                30,
                0,
                DateTimeKind.Utc);

        var repository =
            new FakeFinalRejectionRepository();

        repository.Set(
            new FinalRejectionDetails(
                finalRejectionId,
                submissionId,
                rejectionReasonId,
                "The opportunity does not meet the required criteria.",
                "reviewer-user",
                createdAtUtc));

        var handler =
            new GetFinalRejectionQueryHandler(
                repository);

        var query =
            new GetFinalRejectionQuery(
                finalRejectionId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            finalRejectionId,
            result.Id);

        Assert.Equal(
            submissionId,
            result.SubmissionId);

        Assert.Equal(
            rejectionReasonId,
            result.RejectionReasonId);

        Assert.Equal(
            "The opportunity does not meet the required criteria.",
            result.Comment);

        Assert.Equal(
            "reviewer-user",
            result.CreatedBy);

        Assert.Equal(
            createdAtUtc,
            result.CreatedAtUtc);
    }

    [Fact]
    public async Task Handle_WhenFinalRejectionDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository =
            new FakeFinalRejectionRepository();

        var handler =
            new GetFinalRejectionQueryHandler(
                repository);

        var query =
            new GetFinalRejectionQuery(
                Guid.NewGuid());

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var repository =
            new FakeFinalRejectionRepository();

        var handler =
            new GetFinalRejectionQueryHandler(
                repository);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var query =
            new GetFinalRejectionQuery(
                Guid.NewGuid());

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
