namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a review decision that the requested modifications were rejected and the submission cannot proceed 
/// through that modification request.
/// </summary>
public sealed class ModificationRejection : CreationTrackedEntity
{
    internal ModificationRejection(
        string comment,
        string rejectedBy,
        DateTime? rejectedAtUtc = null)
        : base(rejectedBy, rejectedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        Comment = comment;
    }

    #region Properties

    public string Comment { get; private set; } = string.Empty;

    #endregion
}