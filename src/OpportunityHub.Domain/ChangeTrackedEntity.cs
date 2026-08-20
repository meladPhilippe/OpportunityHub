namespace OpportunityHub.Domain;

/// <summary>
/// Base type for entities that have identity, creation tracking,
/// and update tracking.
/// </summary>
public abstract class ChangeTrackedEntity : CreationTrackedEntity
{
    public DateTime? UpdatedAtUtc { get; protected set; }

    public string? UpdatedBy { get; protected set; }

    protected ChangeTrackedEntity(
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
    }

    protected void TrackUpdate(
        string updatedBy,
        DateTime? updatedAtUtc = null)
    {
        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
    }
}