using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Entities.Audit;

/// <summary>
/// Records a workflow activity performed against an opportunity,
/// optionally identifying the submission or another related domain object.
/// </summary>
public sealed class AuditHistory : CreationTrackedObject
{
    private AuditHistory(
        Guid productId,
        Guid productVersionId,
        Guid? submissionId,
        long activitySequenceNumber,
        WorkflowActivityType activityType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string createdBy = null!,
        DateTime? occurredAtUtc = null!)
        : base(createdBy, occurredAtUtc ?? DateTime.UtcNow)
    {
        ProductId = productId;
        ProductVersionId = productVersionId;
        SubmissionId = submissionId;
        ActivitySequenceNumber = activitySequenceNumber;
        ActivityType = activityType;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
    }

    #region Properties

    public Guid ProductId { get; private set; }

    public Guid ProductVersionId { get; private set; }

    public Guid? SubmissionId { get; private set; }

    public long ActivitySequenceNumber { get; private set; }

    public WorkflowActivityType ActivityType { get; private set; }

    public string? RelatedEntityType { get; private set; }

    public Guid? RelatedEntityId { get; private set; }

    #endregion

    #region Factory

    internal static AuditHistory Create(
        Guid productId,
        Guid productVersionId,
        Guid? submissionId,
        long activitySequenceNumber,
        WorkflowActivityType activityType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string createdBy = null!,
        DateTime? occurredAtUtc = null!)
    {
        return new AuditHistory(
            productId,
            productVersionId,
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