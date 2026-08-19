using OpportunityHub.Domain;
using OpportunityHub.Domain.ValueObjects;

public sealed class VersionFeature : Entity
{
    public Guid VersionId { get; private set; }

    public int? IconReference { get; private set; }

    public LocalizedText? Title { get; private set; }

    public int SortOrder { get; private set; }

    public bool DisplayOnWebsite { get; private set; }

    internal VersionFeature(
        Guid versionId,
        LocalizedText? title,
        int? iconReference,
        int sortOrder,
        bool displayOnWebsite,
        string createdBy,
        DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
        VersionId = versionId;
        Title = title;
        IconReference = iconReference;
        SortOrder = sortOrder;
        DisplayOnWebsite = displayOnWebsite;
    }
   
}