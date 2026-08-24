using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Extensions;

namespace OpportunityHub.Domain.Entities.Audit;

/// <summary>
/// Records a workflow activity performed against an opportunity,
/// optionally identifying the submission or another related domain object.
/// </summary>
public sealed class AuditHistory : CreationTrackedObject
{
    private AuditHistory(
        Guid opportunityId,
        Guid opportunityVersionId,
        Guid? submissionId,
        long activitySequenceNumber,
        WorkflowActivityType activityType,
        AuditRelatedEntityType relatedEntityType,
        Guid? relatedEntityId,
        string createdBy = null!,
        DateTime? occurredAtUtc = null!)
        : base(createdBy, occurredAtUtc ?? DateTime.UtcNow)
    {
        
        OpportunityId = opportunityId;
        OpportunityVersionId = opportunityVersionId;
        SubmissionId = submissionId;
        ActivitySequenceNumber = activitySequenceNumber;
        ActivityType = activityType;
        RelatedEntityType = relatedEntityType.ToDatabaseValue();
        RelatedEntityId = relatedEntityId;
    }
    private AuditHistory()
    {
    }

    #region Properties

    public Guid OpportunityId { get; private set; }

    public Guid OpportunityVersionId { get; private set; }

    public Guid? SubmissionId { get; private set; }

    public long ActivitySequenceNumber { get; private set; }

    public WorkflowActivityType ActivityType { get; private set; }

    public string RelatedEntityType { get; private set; }  = string.Empty;

    public Guid? RelatedEntityId { get; private set; }

    #endregion

    #region Factory

    internal static AuditHistory Create(
        Guid opportunityId,
        Guid opportunityVersionId,
        Guid? submissionId,
        long activitySequenceNumber,
        WorkflowActivityType activityType,
        AuditRelatedEntityType relatedEntityType,
        Guid? relatedEntityId,
        string createdBy = null!,
        DateTime? occurredAtUtc = null!)
    {
        return new AuditHistory(
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

    #endregion
}