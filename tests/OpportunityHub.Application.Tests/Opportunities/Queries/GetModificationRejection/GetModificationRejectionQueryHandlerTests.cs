using OpportunityHub.Application.Opportunities.Queries.GetModificationRejection;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Opportunities.Queries.GetModificationRejection;

public sealed class GetModificationRejectionQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenModificationRejectionExists_ReturnsDetails()
    {
        // Arrange
        var modificationRejectionId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();

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
            new FakeModificationRejectionRepository();

        repository.Set(
            new ModificationRejectionDetails(
                modificationRejectionId,
                submissionId,
                "The requested modification cannot be accepted.",
                "reviewer-user",
                createdAtUtc));

        var handler =
            new GetModificationRejectionQueryHandler(
                repository);

        var query =
            new GetModificationRejectionQuery(
                modificationRejectionId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            modificationRejectionId,
            result.Id);

        Assert.Equal(
            submissionId,
            result.SubmissionId);

        Assert.Equal(
            "The requested modification cannot be accepted.",
            result.Comment);

        Assert.Equal(
            "reviewer-user",
            result.CreatedBy);

        Assert.Equal(
            createdAtUtc,
            result.CreatedAtUtc);
    }

    [Fact]
    public async Task Handle_WhenModificationRejectionDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository =
            new FakeModificationRejectionRepository();

        var handler =
            new GetModificationRejectionQueryHandler(
                repository);

        var query =
            new GetModificationRejectionQuery(
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
            new FakeModificationRejectionRepository();

        var handler =
            new GetModificationRejectionQueryHandler(
                repository);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var query =
            new GetModificationRejectionQuery(
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
