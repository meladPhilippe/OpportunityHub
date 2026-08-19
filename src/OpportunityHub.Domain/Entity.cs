namespace OpportunityHub.Domain;

public abstract class Entity : CreationTrackedEntity
{
    public Guid Id { get; protected set; }

    protected Entity(string createdBy = "SYS", DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
        Id = Guid.NewGuid();
    }
}