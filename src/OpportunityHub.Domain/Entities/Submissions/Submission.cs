using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Entities.Submissions;

/// <summary>
/// Represents a workflow submission of an opportunity version for review.
/// The submission records the workflow state before the submission and
/// contains the decision made during the review process.
/// </summary>
public sealed class Submission : EntityIdentity
{
    private Submission(
        Guid opportunityVersionId,
        long sequenceNumber,
        SubmissionType submissionType,
        string? editSummary,
        OpportunityStatusCode previousStatusCode,
        OpportunitySubStatusCode? previousSubStatusCode,
        string submittedBy,
        DateTime submittedAtUtc)
    {
        OpportunityVersionId = opportunityVersionId;
        SequenceNumber = sequenceNumber;
        SubmissionType = submissionType;
        EditSummary = editSummary;
        PreviousStatusCode = previousStatusCode;
        PreviousSubStatusCode = previousSubStatusCode;
        SubmittedBy = submittedBy;
        SubmittedAtUtc = submittedAtUtc;
    }

    #region Properties

    public Guid OpportunityVersionId { get; private set; }

    public long SequenceNumber { get; private set; }

    public SubmissionType SubmissionType { get; private set; }

    public string? EditSummary { get; private set; }

    public OpportunityStatusCode PreviousStatusCode { get; private set; }

    public OpportunitySubStatusCode? PreviousSubStatusCode { get; private set; }

    public string SubmittedBy { get; private set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; private set; }

    public ModificationRequest? ModificationRequest { get; private set; }

    public ModificationRejection? ModificationRejection { get; private set; }

    public FinalRejection? FinalRejection { get; private set; }

    #endregion

    #region Factory

    public static Submission Create(
        Guid opportunityVersionId,
        long sequenceNumber,
        SubmissionType submissionType,
        string? editSummary,
        OpportunityStatusCode previousStatusCode,
        OpportunitySubStatusCode? previousSubStatusCode,
        string submittedBy,
        DateTime? submittedAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(submittedBy);
        ValidateEditSummary(submissionType, editSummary);
        return new Submission(
            opportunityVersionId,
            sequenceNumber,
            submissionType,
            editSummary,
            previousStatusCode,
            previousSubStatusCode,
            submittedBy,
            submittedAtUtc ?? DateTime.UtcNow);
    }
    private Submission()
    {
        
    }

    #endregion

    #region Workflow

    public void RequestModification(
        IEnumerable<(string FieldName, string Comment)> items,
        string requestedBy,
        DateTime? requestedAtUtc = null)
    {
        EnsureNoDecision();

        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedBy);

        var request = new ModificationRequest(
            requestedBy,
            requestedAtUtc);

        foreach (var item in items)
        {
            request.AddItem(
                item.FieldName,
                item.Comment);
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A modification request must contain at least one item.");
        }

        ModificationRequest = request;
    }

    public void RejectModification(
        string comment,
        string rejectedBy,
        DateTime? rejectedAtUtc = null)
    {
        EnsureModificationRejectionAllowed();
        EnsureNoDecision();

        ModificationRejection = new ModificationRejection(
            comment,
            rejectedBy,
            rejectedAtUtc);
    }

    public void RejectOpportunity(
        int rejectionReasonId,
        string comment,
        string rejectedBy,
        DateTime? rejectedAtUtc = null)
    {
        EnsureFirstPublication();
        EnsureNoDecision();

        FinalRejection = new FinalRejection(
            rejectionReasonId,
            comment,
            rejectedBy,
            rejectedAtUtc);
    }

    #endregion

    #region Validation

    private void EnsureNoDecision()
    {
        if (ModificationRequest is not null ||
            ModificationRejection is not null ||
            FinalRejection is not null)
        {
            throw new InvalidOperationException(
                "The submission already has a workflow decision.");
        }
    }

    private void EnsureFirstPublication()
    {
        if (SubmissionType != SubmissionType.FirstPublication)
        {
            throw new InvalidOperationException(
                "An opportunity can only be rejected during first publication.");
        }
    }

    private void EnsureModificationRejectionAllowed()
{
    if (SubmissionType is SubmissionType.FirstPublication
        or SubmissionType.ManagerDirectEdit)
    {
        throw new InvalidOperationException(
            $"Modification rejection is not allowed for submission type '{SubmissionType}'.");
    }
}

    private static void ValidateEditSummary(
    SubmissionType submissionType,
    string? editSummary)
    {
        if (submissionType == SubmissionType.FirstPublication)
        {
            if (!string.IsNullOrWhiteSpace(editSummary))
            {
                throw new ArgumentException(
                    "Edit summary must not be provided for first publication submissions.",
                    nameof(editSummary));
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(editSummary))
        {
            throw new ArgumentException(
                "Edit summary is required for non-first publication submissions.",
                nameof(editSummary));
        }
    }

    #endregion
}