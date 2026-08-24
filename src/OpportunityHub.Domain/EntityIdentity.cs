namespace OpportunityHub.Domain;

/// <summary>
/// Base type for entities that have a strongly typed identity.
/// Guid is the default identity type.
/// </summary>
public abstract class EntityIdentity<TId> : DomainObject
    where TId : notnull
{
    public TId Id { get; protected set; }

    protected EntityIdentity(
        Func<TId>? idFactory = null)
    {
        Id = idFactory is not null
            ? idFactory()
            : CreateDefaultId();
    }

    protected EntityIdentity(TId id)
    {
        Id = id;
    }

    private static TId CreateDefaultId()
    {
        if (typeof(TId) == typeof(Guid))
        {
            return (TId)(object)Guid.NewGuid();
        }

        return default!;
    }
}
public abstract class EntityIdentity : EntityIdentity<Guid>
{
    protected EntityIdentity()
    {
    }

    protected EntityIdentity(Guid id)
    {
        Id = Guid.NewGuid();        
    }
}