namespace OpportunityHub.Domain;

/// <summary>
/// Base type for entities that have identity and creation tracking.
/// </summary>
public abstract class CreationTrackedEntity : EntityIdentity
{
    public DateTime CreatedAtUtc { get; protected set; }

    public string CreatedBy { get; protected set; } = "SYS";

    protected CreationTrackedEntity(string createdBy = "SYS", DateTime? createdAtUtc = null)
    {
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}