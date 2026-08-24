using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class OpportunityRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public OpportunityRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_returns_persisted_opportunity()
    {
        var opportunityId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Test Opportunity",
                    "فرصة اختبار")
            },
            "integration-test",
            createdAtUtc);

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new OpportunityRepository(db);

        var result = await repository.GetByIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(opportunityId);
        result.StatusCode.Should().Be(
            OpportunityStatusCode.Draft);
        result.SubStatusCode.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.LastSubmissionSequenceNumber.Should().Be(0);
        result.LastActivitySequenceNumber.Should().Be(0);
        result.CreatedBy.Should().Be("integration-test");
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_opportunity()
    {
        await using var db = _fixture.CreateDbContext();

        var repository = new OpportunityRepository(db);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_preserves_opportunity_workflow_state()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Workflow Test Opportunity")
            },
            "integration-test");

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new OpportunityRepository(db);

        var result = await repository.GetByIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(
            OpportunityStatusCode.Draft);
        result.IsDraft.Should().BeTrue();
        result.IsPublished.Should().BeFalse();
        result.IsApproved.Should().BeFalse();
        result.IsRejected.Should().BeFalse();
        result.IsUnderReview.Should().BeFalse();
    }
}
