using OpportunityHub.Domain;
using OpportunityHub.Domain.ValueObjects;

public sealed class VersionKpi : Entity
{
    public Guid VersionId { get; private set; }

    public LocalizedText? Title { get; private set; }

    public LocalizedText? Value { get; private set; }

    public int SortOrder { get; private set; }
    private VersionKpi()
    {
    }
    internal VersionKpi(Guid versionId, LocalizedText? title, LocalizedText? value, int sortOrder, string createdBy,
     DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
        VersionId = versionId;
        Title = title;
        Value = value;
        SortOrder = sortOrder;
    }
   
}