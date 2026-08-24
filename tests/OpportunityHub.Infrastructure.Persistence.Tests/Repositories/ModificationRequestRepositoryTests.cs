using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class ModificationRequestRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public ModificationRequestRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_returns_persisted_modification_request()
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

        opportunity.RequestModification(
            new[]
            {
                ("OpportunityName", "Please update the opportunity name."),
                ("Description", "Please provide a better description.")
            },
            "manager-user");

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var modificationRequestId =
            submission.ModificationRequest!.Id;

        var repository =
            new ModificationRequestRepository(db);

        var result = await repository.GetByIdAsync(
            modificationRequestId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(modificationRequestId);
        result.SubmissionId.Should().Be(submission.Id);
        result.CreatedBy.Should().Be("manager-user");

        result.Items.Should().HaveCount(2);

        result.Items.Should().ContainSingle(x =>
            x.FieldName == "OpportunityName" &&
            x.Comment == "Please update the opportunity name.");

        result.Items.Should().ContainSingle(x =>
            x.FieldName == "Description" &&
            x.Comment == "Please provide a better description.");
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_modification_request()
    {
        await using var db = _fixture.CreateDbContext();

        var repository =
            new ModificationRequestRepository(db);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}
