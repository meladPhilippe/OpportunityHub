using OpportunityHub.Domain;
using OpportunityHub.Domain.ValueObjects;
public class OrganizationType : ChangeTrackedEntity
{
    public int Code { get; set; }
    public LocalizedText Name { get; private set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}