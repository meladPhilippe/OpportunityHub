using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a reference-data channel that can be associated
/// with one or more opportunity versions.
/// </summary>
public sealed class Channel : ChangeTrackedEntity
{
    private Channel(
        int code,
        LocalizedText name,
        int sortOrder,
        string createdBy,
        DateTime createdAtUtc)
        : base(createdBy, createdAtUtc)
    {
        if (code <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                "Channel code must be greater than zero.");
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");
        }

        Code = code;
        Name = name;
        SortOrder = sortOrder;
        IsActive = true;
    }

    private Channel()
    {
    }

    #region Properties

    public int Code { get; private set; }

    public LocalizedText Name { get; private set; } = null!;

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    #endregion

    #region Factory

    public static Channel Create(
        int code,
        LocalizedText name,
        int sortOrder,
        string createdBy,
        DateTime? createdAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        return new Channel(
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

    private void ValidateUpdatedBy(string updatedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);
    }

    #endregion
}