using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Entities.Submissions;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Domain.Workflow;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Aggregate root for an opportunity.
///
/// Opportunity is the consistency boundary for:
/// - opportunity workflow state
/// - versions
/// - submissions
/// - workflow audit history
/// - interests
///
/// Changes to these objects must be coordinated through the
/// Opportunity aggregate.
/// </summary>
public sealed class Opportunity : ChangeTrackedEntity
{
    #region Fields
    private readonly List<OpportunityVersion> _versions = new();
    private readonly List<Submission> _submissions = new();
    private readonly List<AuditHistory> _auditHistories = new();

    #endregion

    #region State

    private OpportunityStatusCode _statusCode;
    private OpportunitySubStatusCode? _subStatusCode;

    #endregion

    #region Properties

    public OpportunityStatusCode StatusCode =>
        _statusCode;

    public OpportunitySubStatusCode? SubStatusCode =>
        _subStatusCode;

    public IReadOnlyCollection<OpportunityVersion> Versions =>
        _versions.AsReadOnly();

    public IReadOnlyCollection<Submission> Submissions =>
        _submissions.AsReadOnly();

    public IReadOnlyCollection<AuditHistory> AuditHistories =>
        _auditHistories.AsReadOnly();

    public string? QrCodeReference { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public long LastSubmissionSequenceNumber { get; private set; }

    public long LastActivitySequenceNumber { get; private set; }

    #endregion

    #region State Helpers

    public bool IsDraft =>
        _statusCode == OpportunityStatusCode.Draft;

    public bool IsPublished =>
        _statusCode == OpportunityStatusCode.Published;

    public bool IsUnderReview =>
        _statusCode == OpportunityStatusCode.PendingManagerReview ||
        _statusCode == OpportunityStatusCode.PendingSpecialistModification ||
        _statusCode == OpportunityStatusCode.PublishedUnderReview;

    public bool IsRejected =>
        _statusCode == OpportunityStatusCode.Rejected;

    public bool IsApproved =>
        _statusCode == OpportunityStatusCode.Approved;

    #endregion

    #region Constructor

    private Opportunity()
    {
    }

    #endregion

    #region Factory

    /// <summary>
    /// Creates a new opportunity in Draft state with its initial version.
    /// </summary>
    public static Opportunity CreateDraft(
        Guid opportunityId,
        OpportunityVersionContent content,
        string createdBy,
        DateTime? createdAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var opportunity = new Opportunity
        {
            Id = opportunityId,
            _statusCode = OpportunityStatusCode.Draft,
            _subStatusCode = null,
            IsActive = true,
            LastSubmissionSequenceNumber = 0,
            LastActivitySequenceNumber = 0
        };

        var timestamp = createdAtUtc ?? DateTime.UtcNow;

        opportunity.CreatedAtUtc = timestamp;
        opportunity.CreatedBy = createdBy;

        var version = OpportunityVersion.CreateInitial(
            opportunity.Id,
            content,
            createdBy,
            timestamp);

        opportunity._versions.Add(version);

        return opportunity;
    }

    #endregion

    #region Version Access

    /// <summary>
    /// Returns the current working version.
    /// </summary>
    public OpportunityVersion GetCurrentVersion()
    {
        return _versions.SingleOrDefault(x => x.IsCurrent)
            ?? throw new WorkflowDomainException(
                "The opportunity does not have a current version.");
    }

    /// <summary>
    /// Returns the currently published version, if one exists.
    /// </summary>
    public OpportunityVersion? GetPublishedVersion()
    {
        return _versions.SingleOrDefault(
            x => x.IsPublishedSnapshot);
    }

    /// <summary>
    /// Returns the latest review submission.
    /// </summary>
    public Submission GetLatestReviewSubmission()
    {
        return _submissions
            .Where(x =>
                x.SubmissionType !=
                SubmissionType.ManagerDirectEdit)
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefault()
            ?? throw new WorkflowDomainException(
                "The opportunity does not have a review submission.");
    }

    /// <summary>
    /// Finds the submission that started the current modification cycle.
    /// A new cycle starts when the opportunity was submitted from its
    /// published state rather than from PublishedUnderReview.
    /// </summary>
    private Submission GetCurrentModificationCycleStartSubmission(
        Submission currentSubmission)
    {
        return _submissions
            .Where(x =>
                x.SubmissionType == currentSubmission.SubmissionType &&
                x.SequenceNumber <= currentSubmission.SequenceNumber)
            .Where(x =>
                x.PreviousStatusCode !=
                OpportunityStatusCode.PublishedUnderReview)
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefault()
            ?? throw new WorkflowDomainException(
                "The current modification cycle start submission could not be found.");
    }

    #endregion

    #region Workflow

    /// <summary>
    /// Submits the current opportunity version for manager review.
    ///
    /// For a published opportunity, a new working version is created
    /// before applying the requested changes.
    /// </summary>
    public Submission SubmitForManagerReview(
        OpportunityVersionContent content,
        string submittedBy,
        string? editSummary = null,
        DateTime? submittedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var timestamp = submittedAtUtc ?? DateTime.UtcNow;

        var previousStatus = _statusCode;
        var previousSubStatus = _subStatusCode;

        EnsureWorkflowActionAllowed(
            WorkflowAction.SubmitForManagerReview);

        var submissionType = ResolveSubmissionType();

        var normalizedSummary =
            NormalizeEditSummary(
                editSummary,
                submissionType);

        var version = GetCurrentVersion();

        // Published opportunities are immutable snapshots.
        // Create a new working version before modifying them.
        if (_statusCode == OpportunityStatusCode.Published)
        {
            version = CreateWorkingVersion(
                version,
                submittedBy,
                timestamp);
        }

        if (_statusCode == OpportunityStatusCode.PublishedUnderReview)
        {
            if (_subStatusCode ==
                OpportunitySubStatusCode.PendingSpecialistModification)
            {
                version = GetCurrentVersion();
            }
        }

        version.ApplyContent(
            content,
            submittedBy,
            timestamp);

        var transition = WorkflowDefinition.GetTransition(
            new WorkflowKey(
                _statusCode,
                _subStatusCode,
                WorkflowAction.SubmitForManagerReview));

        ApplyTransition(
            transition,
            submittedBy,
            timestamp);

        var submission = Submission.Create(
            version.Id,
            GetNextSubmissionSequenceNumber(),
            submissionType,
            normalizedSummary,
            previousStatus,
            previousSubStatus,
            submittedBy,
            timestamp);

        _submissions.Add(submission);

        AddAuditHistory(
            version.Id,
            submission.Id,
            WorkflowActivityType.SubmittedForManagerReview,
            AuditRelatedEntityType.None,
            null,
            submittedBy,
            timestamp);

        return submission;
    }

    /// <summary>
    /// Requests specialist modifications for the current submission.
    /// </summary>
    public void RequestModification(
        IEnumerable<(string FieldName, string Comment)> items,
        string requestedBy,
        DateTime? requestedAtUtc = null)
    {
        var timestamp = requestedAtUtc ?? DateTime.UtcNow;

        EnsureWorkflowActionAllowed(
            WorkflowAction.RequestModification);

        var submission = GetLatestReviewSubmission();

        submission.RequestModification(
            items,
            requestedBy,
            timestamp);

        ApplyWorkflowAction(
            WorkflowAction.RequestModification,
            requestedBy,
            timestamp);

        AddAuditHistory(
            GetCurrentVersion().Id,
            submission.Id,
            WorkflowActivityType.ModificationRequested,
            AuditRelatedEntityType.ModificationRequest,
            submission.ModificationRequest?.Id,
            requestedBy,
            timestamp);
    }

    /// <summary>
    /// Approves the current submission.
    ///
    /// For a first publication, the opportunity becomes Approved.
    ///
    /// For a published modification, the opportunity remains
    /// PublishedUnderReview but its sub-status becomes Approved.
    /// It is not published until Publish is called.
    /// </summary>
    public void Approve(
        string approvedBy,
        DateTime? approvedAtUtc = null)
    {
        var timestamp = approvedAtUtc ?? DateTime.UtcNow;

        EnsureWorkflowActionAllowed(
            WorkflowAction.Approve);

        var submission = GetLatestReviewSubmission();

        ApplyWorkflowAction(
            WorkflowAction.Approve,
            approvedBy,
            timestamp);

        AddAuditHistory(
            GetCurrentVersion().Id,
            submission.Id,
            WorkflowActivityType.Approved,
            AuditRelatedEntityType.None,
            null,
            approvedBy,
            timestamp);
    }

    /// <summary>
    /// Permanently rejects the opportunity during the first-publication
    /// workflow.
    /// </summary>
    public void Reject(
        int rejectionReasonId,
        string comment,
        string rejectedBy,
        DateTime? rejectedAtUtc = null)
    {
        var timestamp = rejectedAtUtc ?? DateTime.UtcNow;

        EnsureWorkflowActionAllowed(
            WorkflowAction.Reject);

        var submission = GetLatestReviewSubmission();

        submission.RejectOpportunity(
            rejectionReasonId,
            comment,
            rejectedBy,
            timestamp);

        ApplyWorkflowAction(
            WorkflowAction.Reject,
            rejectedBy,
            timestamp);

        AddAuditHistory(
            GetCurrentVersion().Id,
            submission.Id,
            WorkflowActivityType.OpportunityRejected,
            AuditRelatedEntityType.FinalRejection,
            submission.FinalRejection?.Id,
            rejectedBy,
            timestamp);
    }

    /// <summary>
    /// Rejects the current modification cycle.
    ///
    /// This is not a final rejection. The opportunity is restored to
    /// the state stored by the submission.
    /// </summary>
    public void RejectModification(
        string comment,
        string rejectedBy,
        DateTime? rejectedAtUtc = null)
    {
        var timestamp = rejectedAtUtc ?? DateTime.UtcNow;

        EnsureWorkflowActionAllowed(
            WorkflowAction.RejectModification);

        var submission = GetLatestReviewSubmission();

        submission.RejectModification(
            comment,
            rejectedBy,
            timestamp);

        var submissionStartedTheModificationCycle = GetCurrentModificationCycleStartSubmission(submission);

        RestorePreviousStatus(
            submissionStartedTheModificationCycle,
            rejectedBy,
            timestamp);

        AddAuditHistory(
            GetCurrentVersion().Id,
            submission.Id,
            WorkflowActivityType.ModificationRejected,
            AuditRelatedEntityType.ModificationRejection,
            submission.ModificationRejection?.Id,
            rejectedBy,
            timestamp);
    }

    #endregion

    #region Publishing

    /// <summary>
    /// Publishes the currently approved version.
    ///
    /// First publication:
    ///     Approved → Published
    ///
    /// Published modification:
    ///     PublishedUnderReview + Approved
    ///         → Published + PublishedModified
    /// </summary>
    public void Publish(
        string publishedBy,
        DateTime? publishedAtUtc = null)
    {
        var timestamp = publishedAtUtc ?? DateTime.UtcNow;

        EnsureWorkflowActionAllowed(
            WorkflowAction.Publish);

        var version = GetCurrentVersion();

        version.Publish(
            publishedBy,
            timestamp);

        var transition = WorkflowDefinition.GetTransition(
            new WorkflowKey(
                _statusCode,
                _subStatusCode,
                WorkflowAction.Publish));

        ApplyTransition(
            transition,
            publishedBy,
            timestamp);

        PublishedAtUtc = timestamp;

        AddAuditHistory(
            version.Id,
            null,
            WorkflowActivityType.Published,
            AuditRelatedEntityType.None,
            null,
            publishedBy,
            timestamp);
    }

    /// <summary>
    /// Unpublishes the currently published opportunity.
    /// </summary>
    public void Unpublish(
        string unpublishedBy,
        DateTime? unpublishedAtUtc = null)
    {
        var timestamp = unpublishedAtUtc ?? DateTime.UtcNow;

        EnsureWorkflowActionAllowed(
            WorkflowAction.Unpublish);

        var version = GetCurrentVersion();

        ApplyWorkflowAction(
            WorkflowAction.Unpublish,
            unpublishedBy,
            timestamp);

        AddAuditHistory(
            version.Id,
            null,
            WorkflowActivityType.Unpublished,
            AuditRelatedEntityType.None,
            null,
            unpublishedBy,
            timestamp);
    }

    #endregion

    #region Version Management

    /// <summary>
    /// Creates a new working version from the supplied version.
    ///
    /// The previous version becomes non-current.
    /// </summary>
    private OpportunityVersion CreateWorkingVersion(
        OpportunityVersion sourceVersion,
        string createdBy,
        DateTime occurredAtUtc)
    {
        var nextVersionNumber =
            _versions.Max(x => x.VersionNumber) + 1;

        var newVersion = sourceVersion.CloneForEditing(
            nextVersionNumber,
            createdBy,
            occurredAtUtc);

        _versions.Add(newVersion);

        return newVersion;
    }

    #endregion

    #region Audit

    /// <summary>
    /// Adds an audit record to the opportunity aggregate.
    ///
    /// Audit history is not owned by Submission because activities
    /// can occur outside a submission workflow.
    /// </summary>
    private void AddAuditHistory(
        Guid opportunityVersionId,
        Guid? submissionId,
        WorkflowActivityType activityType,
        AuditRelatedEntityType relatedEntityType,
        Guid? relatedEntityId,
        string createdBy = null!,
        DateTime? occurredAtUtc = null!)
    {
        var audit = AuditHistory.Create(
            Id,
            opportunityVersionId,
            submissionId,
            GetNextActivitySequenceNumber(),
            activityType,
            relatedEntityType,
            relatedEntityId,
            createdBy,
            occurredAtUtc);

        _auditHistories.Add(audit);
    }

    #endregion
    #region Workflow Sequences

    public long GetNextSubmissionSequenceNumber()
    {
        return ++LastSubmissionSequenceNumber;
    }

    public long GetNextActivitySequenceNumber()
    {
        return ++LastActivitySequenceNumber;
    }

    #endregion

    #region Workflow Helpers

    private void ApplyWorkflowAction(
        WorkflowAction action,
        string updatedBy,
        DateTime occurredAtUtc)
    {
        var transition = WorkflowDefinition.GetTransition(
            new WorkflowKey(
                _statusCode,
                _subStatusCode,
                action));

        ApplyTransition(
            transition,
            updatedBy,
            occurredAtUtc);
    }

    private void ApplyTransition(
        WorkflowTransition transition,
        string updatedBy,
        DateTime occurredAtUtc)
    {
        _statusCode = transition.StatusCode;
        _subStatusCode = transition.SubStatusCode;

        TrackUpdate(
            updatedBy,
            occurredAtUtc);
    }

    private void EnsureWorkflowActionAllowed(
        WorkflowAction action)
    {
        var key = new WorkflowKey(
            _statusCode,
            _subStatusCode,
            action);

        if (!WorkflowDefinition.IsAllowed(key))
        {
            throw new WorkflowTransitionNotAllowedException(key);
        }
    }

    private void RestorePreviousStatus(
        Submission submission,
        string updatedBy,
        DateTime occurredAtUtc)
    {
        _statusCode =
            submission.PreviousStatusCode;

        _subStatusCode =
            submission.PreviousSubStatusCode;

        TrackUpdate(
            updatedBy,
            occurredAtUtc);
    }

    #endregion

    #region Submission

    private SubmissionType ResolveSubmissionType()
    {
        return _statusCode switch
        {
            OpportunityStatusCode.Draft =>
                SubmissionType.FirstPublication,

            OpportunityStatusCode.Approved =>
                SubmissionType.ApprovedModification,

            OpportunityStatusCode.Published =>
                SubmissionType.PublishedModification,

            OpportunityStatusCode.PendingSpecialistModification =>
                ResolvePreviousSubmissionType(),

            OpportunityStatusCode.PublishedUnderReview
                when _subStatusCode ==
                     OpportunitySubStatusCode.PendingSpecialistModification =>
                ResolvePreviousSubmissionType(),

            _ => throw new WorkflowDomainException(
                $"A submission cannot be created from the current " +
                $"opportunity state: {_statusCode}/{_subStatusCode}.")
        };
    }

    private SubmissionType ResolvePreviousSubmissionType()
    {
        var submission =
            GetLatestReviewSubmission();

        return submission.SubmissionType switch
        {
            SubmissionType.FirstPublication =>
                SubmissionType.FirstPublication,

            SubmissionType.ApprovedModification =>
                SubmissionType.ApprovedModification,

            SubmissionType.PublishedModification =>
                SubmissionType.PublishedModification,

            _ => throw new WorkflowDomainException(
                $"The previous submission type '{submission.SubmissionType}' " +
                "cannot be used for the current workflow transition.")
        };
    }

    #endregion

    #region Validation

    private static string? NormalizeEditSummary(
        string? editSummary,
        SubmissionType submissionType)
    {
        var normalized =
            string.IsNullOrWhiteSpace(editSummary)
                ? null
                : editSummary.Trim();

        var requiresSummary =
            submissionType is
                SubmissionType.ApprovedModification or
                SubmissionType.PublishedModification;

        if (requiresSummary && normalized is null)
        {
            throw new WorkflowDomainException(
                "An edit summary is required for this submission.");
        }

        if (normalized?.Length > 2000)
        {
            throw new WorkflowDomainException(
                "The edit summary cannot exceed 2,000 characters.");
        }

        return normalized;
    }

    #endregion
}