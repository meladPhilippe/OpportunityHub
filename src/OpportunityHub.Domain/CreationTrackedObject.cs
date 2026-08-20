namespace OpportunityHub.Domain;

/// <summary>
/// Base type for domain objects that record who and when
/// the object was created, without requiring domain identity.
/// </summary>
public abstract class CreationTrackedObject : DomainObject
{
    public DateTime CreatedAtUtc { get; protected set; }

    public string CreatedBy { get; protected set; } = "SYS";

    protected CreationTrackedObject(
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
    {
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}