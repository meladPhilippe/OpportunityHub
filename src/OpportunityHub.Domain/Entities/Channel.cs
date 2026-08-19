using OpportunityHub.Domain;
using OpportunityHub.Domain.ValueObjects;

public class Channel : ChangeTrackedEntity
{
    public int Code { get; private set; }

    public LocalizedText Name { get; private set; } = null!;

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }
}