namespace OpportunityHub.Domain;

public abstract class ChangeTrackedEntity : Entity
{
    public DateTime? UpdatedAtUtc { get; protected set; }

    public string? UpdatedBy { get; protected set; }

    protected ChangeTrackedEntity(string createdBy = "SYS", DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
    }

    protected void TrackUpdate(string updatedBy, DateTime? updatedAtUtc = null)
    {
        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
    }
}