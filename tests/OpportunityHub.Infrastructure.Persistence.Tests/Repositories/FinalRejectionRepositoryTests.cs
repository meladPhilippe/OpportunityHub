using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class FinalRejectionRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public FinalRejectionRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_returns_persisted_final_rejection()
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

        var submission = opportunity.SubmitForManagerReview(
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Test Opportunity",
                    "فرصة اختبار")
            },
            "test-user");

        const int rejectionReasonId = 1;

        opportunity.Reject(
            rejectionReasonId,
            "The opportunity does not meet the required criteria.",
            "manager-user");

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var finalRejectionId =
            submission.FinalRejection!.Id;

        var repository =
            new FinalRejectionRepository(db);

        var result = await repository.GetByIdAsync(
            finalRejectionId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(finalRejectionId);
        result.SubmissionId.Should().Be(submission.Id);
        result.RejectionReasonId.Should().Be(rejectionReasonId);
        result.Comment.Should()
            .Be("The opportunity does not meet the required criteria.");
        result.CreatedBy.Should().Be("manager-user");
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_final_rejection()
    {
        await using var db = _fixture.CreateDbContext();

        var repository =
            new FinalRejectionRepository(db);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}
