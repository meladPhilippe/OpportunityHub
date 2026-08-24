using OpportunityHub.Application.Opportunities.Queries.GetModificationRequest;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Opportunities.Queries.GetModificationRequest;

public sealed class GetModificationRequestQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenModificationRequestExists_ReturnsDetails()
    {
        // Arrange
        var modificationRequestId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();

        var repository =
            new FakeModificationRequestRepository();

        repository.Set(
            new ModificationRequestDetails(
                modificationRequestId,
                submissionId,
                "manager-user",
                new DateTime(
                    2026,
                    8,
                    23,
                    10,
                    30,
                    0,
                    DateTimeKind.Utc),
                [
                    new ModificationRequestItemDetails(
                        "OpportunityName",
                        "Please update the opportunity name."),

                    new ModificationRequestItemDetails(
                        "Description",
                        "Please provide more details.")
                ]));

        var handler =
            new GetModificationRequestQueryHandler(
                repository);

        var query =
            new GetModificationRequestQuery(
                modificationRequestId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            modificationRequestId,
            result.Id);

        Assert.Equal(
            submissionId,
            result.SubmissionId);

        Assert.Equal(
            "manager-user",
            result.CreatedBy);

        Assert.Equal(
            new DateTime(
                2026,
                8,
                23,
                10,
                30,
                0,
                DateTimeKind.Utc),
            result.CreatedAtUtc);

        Assert.Equal(
            2,
            result.Items.Count);

        var first =
            result.Items.ElementAt(0);

        Assert.Equal(
            "OpportunityName",
            first.FieldName);

        Assert.Equal(
            "Please update the opportunity name.",
            first.Comment);

        var second =
            result.Items.ElementAt(1);

        Assert.Equal(
            "Description",
            second.FieldName);

        Assert.Equal(
            "Please provide more details.",
            second.Comment);
    }

    [Fact]
    public async Task Handle_WhenModificationRequestDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository =
            new FakeModificationRequestRepository();

        var handler =
            new GetModificationRequestQueryHandler(
                repository);

        var query =
            new GetModificationRequestQuery(
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
            new FakeModificationRequestRepository();

        var handler =
            new GetModificationRequestQueryHandler(
                repository);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var query =
            new GetModificationRequestQuery(
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
