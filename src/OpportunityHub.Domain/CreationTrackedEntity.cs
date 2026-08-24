namespace OpportunityHub.Domain;

/// <summary>
/// Base type for entities that have identity and creation tracking.
/// </summary>
public abstract class CreationTrackedEntity<TId> : EntityIdentity<TId>
    where TId : notnull
{
    public DateTime CreatedAtUtc { get; protected set; }
    public string CreatedBy { get; protected set; } = "SYS";

    protected CreationTrackedEntity(
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
        : base()
    {
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}

public abstract class CreationTrackedEntity : EntityIdentity<Guid>
{
    public DateTime CreatedAtUtc { get; protected set; }
    public string CreatedBy { get; protected set; } = "SYS";

    protected CreationTrackedEntity(
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
        : base()
    {
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}
