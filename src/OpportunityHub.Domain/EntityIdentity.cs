namespace OpportunityHub.Domain;

/// <summary>
/// Base type for domain objects that have a distinct identity
/// throughout their lifecycle.
/// </summary>
public abstract class EntityIdentity : DomainObject
{
    public Guid Id { get; protected set; }

    protected EntityIdentity()
    {
        Id = Guid.NewGuid();
    }

    protected EntityIdentity(Guid id)
    {
        Id = id;
    }
}