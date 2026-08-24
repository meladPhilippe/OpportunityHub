using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class ModificationRejectionRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public ModificationRejectionRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_returns_persisted_modification_rejection()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Test Opportunity",
                    "فرصة اختبار")
            },
            "test-user");

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        // First publication.
        opportunity.SubmitForManagerReview(
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Test Opportunity",
                    "فرصة اختبار")
            },
            "test-user");

        opportunity.Approve("manager-user");
        opportunity.Publish("publisher-user");

        // Start a modification cycle for the published opportunity.
        var modificationSubmission =
            opportunity.SubmitForManagerReview(
                new OpportunityVersionContent
                {
                    OpportunityName = new LocalizedText(
                        "Modified Opportunity",
                        "فرصة معدلة")
                },
                "specialist-user",
                "Update opportunity information.");

        // Manager requests changes.
        opportunity.RequestModification(
            new[]
            {
                (
                    "OpportunityName",
                    "Please update the opportunity name.")
            },
            "manager-user");

        // Specialist submits the requested changes back for manager review.
        var resubmission =
            opportunity.SubmitForManagerReview(
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Modified Opportunity",
                    "فرصة معدلة")
            },
            "specialist-user",
            "Updated opportunity information.");

        // Manager rejects the modification cycle.
        opportunity.RejectModification(
            "The requested modification cannot be accepted.",
            "manager-user");

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var modificationRejectionId =
            resubmission.ModificationRejection!.Id;

        var repository =
            new ModificationRejectionRepository(db);

        var result = await repository.GetByIdAsync(
            modificationRejectionId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(modificationRejectionId);
        result.SubmissionId.Should().Be(resubmission.Id);
        result.Comment.Should()
            .Be("The requested modification cannot be accepted.");
        result.CreatedBy.Should().Be("manager-user");
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_modification_rejection()
    {
        await using var db = _fixture.CreateDbContext();

        var repository =
            new ModificationRejectionRepository(db);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}
