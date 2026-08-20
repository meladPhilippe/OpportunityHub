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
        Guid productVersionId,
        long sequenceNumber,
        SubmissionType submissionType,
        string? editSummary,
        OpportunityStatusCode previousStatusCode,
        OpportunitySubStatusCode? previousSubStatusCode,
        string submittedBy,
        DateTime submittedAtUtc)
    {
        ProductVersionId = productVersionId;
        SequenceNumber = sequenceNumber;
        SubmissionType = submissionType;
        EditSummary = editSummary;
        PreviousStatusCode = previousStatusCode;
        PreviousSubStatusCode = previousSubStatusCode;
        SubmittedBy = submittedBy;
        SubmittedAtUtc = submittedAtUtc;
    }

    #region Properties

    public Guid ProductVersionId { get; private set; }

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
        Guid productVersionId,
        long sequenceNumber,
        SubmissionType submissionType,
        string? editSummary,
        OpportunityStatusCode previousStatusCode,
        OpportunitySubStatusCode? previousSubStatusCode,
        string submittedBy,
        DateTime? submittedAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(submittedBy);

        return new Submission(
            productVersionId,
            sequenceNumber,
            submissionType,
            editSummary,
            previousStatusCode,
            previousSubStatusCode,
            submittedBy,
            submittedAtUtc ?? DateTime.UtcNow);
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

    #endregion
}