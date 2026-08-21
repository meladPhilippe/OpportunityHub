using OpportunityHub.Domain.Enums;

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
        string? relatedEntityType,
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
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
    }

    #region Properties

    public Guid OpportunityId { get; private set; }

    public Guid OpportunityVersionId { get; private set; }

    public Guid? SubmissionId { get; private set; }

    public long ActivitySequenceNumber { get; private set; }

    public WorkflowActivityType ActivityType { get; private set; }

    public string? RelatedEntityType { get; private set; }

    public Guid? RelatedEntityId { get; private set; }

    #endregion

    #region Factory

    internal static AuditHistory Create(
        Guid opportunityId,
        Guid opportunityVersionId,
        Guid? submissionId,
        long activitySequenceNumber,
        WorkflowActivityType activityType,
        string? relatedEntityType,
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