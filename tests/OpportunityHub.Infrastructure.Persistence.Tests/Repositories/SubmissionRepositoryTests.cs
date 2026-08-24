using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class SubmissionRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public SubmissionRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_returns_persisted_submission()
    {
        var opportunityId = Guid.NewGuid();
        var submittedAtUtc = DateTime.UtcNow;

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Submission Test Opportunity",
                    "فرصة اختبار الإرسال")
            },
            "integration-test");

        var submission = opportunity.SubmitForManagerReview(
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Submission Test Opportunity Updated",
                    "فرصة اختبار الإرسال المحدثة")
            },
            "submitter",
            submittedAtUtc: submittedAtUtc);

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new SubmissionRepository(db);

        var result = await repository.GetByIdAsync(
            submission.Id,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(submission.Id);
        result.OpportunityVersionId.Should().Be(
            submission.OpportunityVersionId);
        result.SequenceNumber.Should().Be(1);
        result.SubmissionType.Should().Be(
            SubmissionType.FirstPublication);
        result.EditSummary.Should().BeNull();
        result.PreviousStatusCode.Should().Be(
            OpportunityStatusCode.Draft);
        result.PreviousSubStatusCode.Should().BeNull();
        result.SubmittedBy.Should().Be("submitter");
        result.SubmittedAtUtc.Should().Be(submittedAtUtc);
        result.ModificationRequestId.Should().BeNull();
        result.ModificationRejectionId.Should().BeNull();
        result.FinalRejectionId.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_modification_request_id()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Modification Request Test",
                    "اختبار طلب التعديل")
            },
            "test-user");

        var submission = opportunity.SubmitForManagerReview(
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Modification Request Test",
                    "اختبار طلب التعديل")
            },
            "test-user");

        opportunity.RequestModification(
            new[]
            {
                (
                    "OpportunityName",
                    "Please update the opportunity name.")
            },
            "manager-user");

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var modificationRequestId =
            submission.ModificationRequest!.Id;

        var repository = new SubmissionRepository(db);

        var result = await repository.GetByIdAsync(
            submission.Id,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(submission.Id);
        result.ModificationRequestId.Should()
            .Be(modificationRequestId);
        result.ModificationRejectionId.Should().BeNull();
        result.FinalRejectionId.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_modification_rejection_id()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Modification Rejection Test",
                    "اختبار رفض التعديل")
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
                    "Modification Rejection Test",
                    "اختبار رفض التعديل")
            },
            "test-user");

        opportunity.Approve("manager-user");
        opportunity.Publish("publisher-user");

        // Start published modification cycle.
        var submission = opportunity.SubmitForManagerReview(
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

        // Specialist submits changes back for manager review.
        var rejectionSubmission =
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
            rejectionSubmission.ModificationRejection!.Id;

        var repository = new SubmissionRepository(db);

        var result = await repository.GetByIdAsync(
            rejectionSubmission.Id,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(rejectionSubmission.Id);
        result.ModificationRequestId.Should().BeNull();
        result.ModificationRejectionId.Should()
            .Be(modificationRejectionId);
        result.FinalRejectionId.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_final_rejection_id()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Final Rejection Test",
                    "اختبار الرفض النهائي")
            },
            "test-user");

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var submission = opportunity.SubmitForManagerReview(
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Final Rejection Test",
                    "اختبار الرفض النهائي")
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

        var repository = new SubmissionRepository(db);

        var result = await repository.GetByIdAsync(
            submission.Id,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(submission.Id);
        result.ModificationRequestId.Should().BeNull();
        result.ModificationRejectionId.Should().BeNull();
        result.FinalRejectionId.Should().Be(finalRejectionId);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_submission()
    {
        await using var db = _fixture.CreateDbContext();

        var repository = new SubmissionRepository(db);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}
