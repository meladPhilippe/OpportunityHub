using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a reference-data rejection reason that can be
/// associated with a final rejection.
/// </summary>
public sealed class RejectionReason : ChangeTrackedEntity<int>
{
    private RejectionReason(
        string code,
        LocalizedText name,
        int sortOrder,
        string createdBy,
        DateTime createdAtUtc)
        : base(createdBy, createdAtUtc)
    {
        Code = code;
        Name = name;
        SortOrder = sortOrder;
        IsActive = true;
    }

    private RejectionReason()
    {
    }

    #region Properties

    public string Code { get; private set; } = string.Empty;

    public LocalizedText Name { get; private set; } = null!;

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    #endregion

    #region Factory

    public static RejectionReason Create(
        string code,
        LocalizedText name,
        int sortOrder,
        string createdBy,
        DateTime? createdAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");
        }

        return new RejectionReason(
            code,
            name,
            sortOrder,
            createdBy,
            createdAtUtc ?? DateTime.UtcNow);
    }

    #endregion

    #region State

    public void Activate(
        string updatedBy,
        DateTime? updatedAtUtc = null)
    {
        ValidateUpdatedBy(updatedBy);
        IsActive = true;

        TrackUpdate(
            updatedBy,
            updatedAtUtc);
    }

    public void Deactivate(
        string updatedBy,
        DateTime? updatedAtUtc = null)
    {
        ValidateUpdatedBy(updatedBy);
        IsActive = false;

        TrackUpdate(
            updatedBy,
            updatedAtUtc);
    }

    private static void ValidateUpdatedBy(string updatedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);
    }

    #endregion
}