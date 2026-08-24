using OpportunityHub.Application.Opportunities.Queries.GetOpportunityAuditHistory;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.Opportunities.Queries.GetOpportunityAuditHistory;

public sealed class GetOpportunityAuditHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenAuditHistoryExists_ReturnsAuditHistory()
    {
        // Arrange
        var opportunityId = Guid.NewGuid();
        var opportunityVersionId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var relatedEntityId = Guid.NewGuid();

        var firstAudit =
            AuditHistoryFactory.Create(
                opportunityId,
                opportunityVersionId,
                submissionId,
                1,
                WorkflowActivityType.SubmittedForManagerReview,
                createdBy: "specialist-user");

        var secondAudit =
            AuditHistoryFactory.Create(
                opportunityId,
                opportunityVersionId,
                submissionId,
                2,
                WorkflowActivityType.ModificationRequested,
                relatedEntityType: AuditRelatedEntityType.ModificationRequest,
                relatedEntityId: relatedEntityId,
                createdBy: "manager-user");

        var repository =
            new FakeAuditHistoryRepository();

        // Add in reverse order to verify repository ordering.
        repository.Add(secondAudit);
        repository.Add(firstAudit);

        var handler =
            new GetOpportunityAuditHistoryQueryHandler(
                repository);

        var query =
            new GetOpportunityAuditHistoryQuery(
                opportunityId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal(
                    opportunityId,
                    first.OpportunityId);

                Assert.Equal(
                    opportunityVersionId,
                    first.OpportunityVersionId);

                Assert.Equal(
                    submissionId,
                    first.SubmissionId);

                Assert.Equal(
                    1,
                    first.ActivitySequenceNumber);

                Assert.Equal(
                    WorkflowActivityType.SubmittedForManagerReview,
                                        first.ActivityType);
                    Assert.Equal(
                        "None",
                        first.RelatedEntityType);

                Assert.Null(
                    first.RelatedEntityId);

                Assert.Equal(
                    "specialist-user",
                    first.CreatedBy);
            },
            second =>
            {
                Assert.Equal(
                    opportunityId,
                    second.OpportunityId);

                Assert.Equal(
                    2,
                    second.ActivitySequenceNumber);

                Assert.Equal(
                    WorkflowActivityType.ModificationRequested,
                    second.ActivityType);

                Assert.Equal(
                    "ModificationRequest",
                    second.RelatedEntityType);

                Assert.Equal(
                    relatedEntityId,
                    second.RelatedEntityId);

                Assert.Equal(
                    "manager-user",
                    second.CreatedBy);
            });
    }

    [Fact]
    public async Task Handle_WhenAuditHistoryDoesNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var repository =
            new FakeAuditHistoryRepository();

        var handler =
            new GetOpportunityAuditHistoryQueryHandler(
                repository);

        var query =
            new GetOpportunityAuditHistoryQuery(
                Guid.NewGuid());

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_DoesNotReturnAuditHistoryForAnotherOpportunity()
    {
        // Arrange
        var opportunityId = Guid.NewGuid();

        var matchingAudit =
            AuditHistoryFactory.Create(
                opportunityId,
                Guid.NewGuid(),
                null,
                1,
                WorkflowActivityType.SubmittedForManagerReview);

        var otherAudit =
            AuditHistoryFactory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                1,
                WorkflowActivityType.SubmittedForManagerReview);

        var repository =
            new FakeAuditHistoryRepository();

        repository.Add(matchingAudit);
        repository.Add(otherAudit);

        var handler =
            new GetOpportunityAuditHistoryQueryHandler(
                repository);

        var query =
            new GetOpportunityAuditHistoryQuery(
                opportunityId);

        // Act
        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        // Assert
        var audit =
            Assert.Single(result);

        Assert.Equal(
            opportunityId,
            audit.OpportunityId);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunityId = Guid.NewGuid();

        var repository =
            new FakeAuditHistoryRepository();

        repository.Add(
            AuditHistoryFactory.Create(
                opportunityId,
                Guid.NewGuid(),
                null,
                1,
                WorkflowActivityType.SubmittedForManagerReview));

        var handler =
            new GetOpportunityAuditHistoryQueryHandler(
                repository);

        var query =
            new GetOpportunityAuditHistoryQuery(
                opportunityId);

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
