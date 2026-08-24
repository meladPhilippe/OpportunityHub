using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class AuditHistoryRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public AuditHistoryRepositoryTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByOpportunityIdAsync_returns_persisted_audit_history()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = CreateOpportunity(opportunityId);

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var opportunityVersionId =
            opportunity.GetCurrentVersion().Id;

        var auditHistory = CreateAuditHistory(
            opportunityId,
            opportunityVersionId,
            activitySequenceNumber: 1);

        db.AuditHistories.Add(auditHistory);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new AuditHistoryRepository(db);

        var result = await repository.GetByOpportunityIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result.Should().ContainSingle();

        var persisted = result.Single();

        persisted.OpportunityId.Should().Be(opportunityId);
        persisted.OpportunityVersionId.Should().Be(opportunityVersionId);
        persisted.ActivitySequenceNumber.Should().Be(1);
        persisted.ActivityType.Should()
            .Be(WorkflowActivityType.SubmittedForManagerReview);
        persisted.RelatedEntityType.Should()
            .Be(AuditRelatedEntityType.None.ToString());
        persisted.RelatedEntityId.Should().BeNull();
        persisted.CreatedBy.Should().Be("test-user");
    }

    [Fact]
    public async Task GetByOpportunityIdAsync_returns_empty_collection_for_unknown_opportunity()
    {
        await using var db = _fixture.CreateDbContext();

        var repository = new AuditHistoryRepository(db);

        var result = await repository.GetByOpportunityIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByOpportunityIdAsync_returns_only_history_for_requested_opportunity()
    {
        var opportunityId = Guid.NewGuid();
        var otherOpportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = CreateOpportunity(opportunityId);
        var otherOpportunity = CreateOpportunity(otherOpportunityId);

        db.Opportunities.AddRange(
            opportunity,
            otherOpportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var opportunityVersionId =
            opportunity.GetCurrentVersion().Id;

        var otherOpportunityVersionId =
            otherOpportunity.GetCurrentVersion().Id;

        db.AuditHistories.AddRange(
            CreateAuditHistory(
                opportunityId,
                opportunityVersionId,
                activitySequenceNumber: 1),
            CreateAuditHistory(
                otherOpportunityId,
                otherOpportunityVersionId,
                activitySequenceNumber: 2));

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new AuditHistoryRepository(db);

        var result = await repository.GetByOpportunityIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result.Should().ContainSingle();
        result.Single().OpportunityId.Should().Be(opportunityId);
    }

    [Fact]
    public async Task GetByOpportunityIdAsync_returns_history_ordered_by_activity_sequence_number()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = CreateOpportunity(opportunityId);

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var opportunityVersionId =
            opportunity.GetCurrentVersion().Id;

        db.AuditHistories.AddRange(
            CreateAuditHistory(
                opportunityId,
                opportunityVersionId,
                activitySequenceNumber: 3),
            CreateAuditHistory(
                opportunityId,
                opportunityVersionId,
                activitySequenceNumber: 1),
            CreateAuditHistory(
                opportunityId,
                opportunityVersionId,
                activitySequenceNumber: 2));

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new AuditHistoryRepository(db);

        var result = await repository.GetByOpportunityIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result
            .Select(x => x.ActivitySequenceNumber)
            .Should()
            .Equal(1, 2, 3);
    }

    private static Opportunity CreateOpportunity(
        Guid opportunityId)
    {
        return Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Test Opportunity",
                    "فرصة اختبار")
            },
            "test-user");
    }

    private static AuditHistory CreateAuditHistory(
        Guid opportunityId,
        Guid opportunityVersionId,
        long activitySequenceNumber)
    {
        return AuditHistory.Create(
            opportunityId,
            opportunityVersionId,
            submissionId: null,
            activitySequenceNumber,
            WorkflowActivityType.SubmittedForManagerReview,
            AuditRelatedEntityType.None,
            relatedEntityId: null,
            createdBy: "test-user",
            occurredAtUtc: DateTime.UtcNow);
    }
}
