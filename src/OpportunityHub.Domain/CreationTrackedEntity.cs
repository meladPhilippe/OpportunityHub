namespace OpportunityHub.Domain;

public abstract class CreationTrackedEntity
{
    public DateTime CreatedAtUtc { get; protected set; }

    public string CreatedBy { get; protected set; } = "SYS";

    protected CreationTrackedEntity(string createdBy = "SYS", DateTime? createdAtUtc = null)
    {
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}