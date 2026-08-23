using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Application.Tests.TestData;

public static class OpportunityFactory
{
    public static Opportunity CreateDraft(
        Guid? opportunityId = null,
        string createdBy = "test-user",
        DateTime? createdAtUtc = null)
    {
        return Opportunity.CreateDraft(
            opportunityId ?? Guid.NewGuid(),
            CreateContent(),
            createdBy,
            createdAtUtc ?? DateTime.UtcNow);
    }

    public static Opportunity CreatePendingManagerReview(
        Guid? opportunityId = null,
        string createdBy = "test-user",
        DateTime? createdAtUtc = null)
    {
        var opportunity = CreateDraft(
            opportunityId,
            createdBy,
            createdAtUtc);

        opportunity.SubmitForManagerReview(
            CreateContent(),
            createdBy);

        return opportunity;
    }

    public static Opportunity CreateApproved(
        Guid? opportunityId = null,
        string createdBy = "test-user",
        DateTime? createdAtUtc = null)
    {
        var opportunity = CreatePendingManagerReview(
            opportunityId,
            createdBy,
            createdAtUtc);

        opportunity.Approve(createdBy);

        return opportunity;
    }

    public static Opportunity CreatePublished(
        Guid? opportunityId = null,
        string createdBy = "test-user",
        DateTime? createdAtUtc = null)
    {
        var opportunity = CreateApproved(
            opportunityId,
            createdBy,
            createdAtUtc);

        opportunity.Publish(createdBy);

        return opportunity;
    }

    public static OpportunityVersionContent CreateContent()
    {
        return new OpportunityVersionContent
        {
            OpportunityName = new LocalizedText(
                "Test Opportunity",
                "فرصة اختبار")
        };
    }

    public static Opportunity CreatePublishedModificationPendingManagerReview()
{
    var opportunity = CreatePublished();

    opportunity.SubmitForManagerReview(
        CreateContent(),
        "specialist-user",
        "Test modification");

    opportunity.RequestModification(
        [
            (
                "Description",
                "Please provide additional information.")
        ],
        "manager-user");

    opportunity.SubmitForManagerReview(
        CreateContent(),
        "specialist-user",
        "Test modification");

    return opportunity;
}
}