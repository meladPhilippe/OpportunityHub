namespace OpportunityHub.Domain.Entities;
/// <summary>
/// Represents the final rejection of the opportunity. After this decision, 
/// the opportunity cannot continue through the current submission workflow.
/// </summary>
public sealed class FinalRejection : CreationTrackedEntity
{
    internal FinalRejection(
        int rejectionReasonId,
        string comment,
        string rejectedBy,
        DateTime? rejectedAtUtc = null)
        : base(rejectedBy, rejectedAtUtc)
    {
        if (rejectionReasonId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rejectionReasonId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        RejectionReasonId = rejectionReasonId;
        Comment = comment;
    }

    #region Properties

    public int RejectionReasonId { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    #endregion
}