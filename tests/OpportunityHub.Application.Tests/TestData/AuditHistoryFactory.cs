using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.TestData;

public static class AuditHistoryFactory
{
    public static AuditHistory Create(
        Guid opportunityId,
        Guid opportunityVersionId,
        Guid? submissionId,
        long activitySequenceNumber,
        WorkflowActivityType activityType,
        AuditRelatedEntityType relatedEntityType = AuditRelatedEntityType.None,
        Guid? relatedEntityId = null,
        string createdBy = "test-user",
        DateTime? occurredAtUtc = null)
    {
        return AuditHistory.Create(
            opportunityId,
            opportunityVersionId,
            submissionId,
            activitySequenceNumber,
            activityType,
            relatedEntityType,
            relatedEntityId,
            createdBy,
            occurredAtUtc);
    }
}
