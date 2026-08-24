namespace OpportunityHub.Domain;

/// <summary>
/// Base type for entities that have identity, creation tracking,
/// and update tracking.
/// </summary>
public abstract class ChangeTrackedEntity<TId>
    : CreationTrackedEntity<TId>
    where TId : notnull
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

/// <summary>
/// Default change-tracked entity using Guid identity.
/// </summary>
public abstract class ChangeTrackedEntity
    : ChangeTrackedEntity<Guid>
{
    protected ChangeTrackedEntity(
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
        : base(createdBy,createdAtUtc)
    {
    }

}