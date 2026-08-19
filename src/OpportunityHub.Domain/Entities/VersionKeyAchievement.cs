using OpportunityHub.Domain;
using OpportunityHub.Domain.ValueObjects;

public sealed class VersionKeyAchievement : Entity
{
    public Guid VersionId { get; private set; }

    public int? IconReference { get; private set; }

    public LocalizedText? Title { get; private set; }

    public LocalizedText? Description { get; private set; }

    public int SortOrder { get; private set; }

    public bool DisplayOnWebsite { get; private set; }
    private VersionKeyAchievement()
    {
    }
    internal VersionKeyAchievement(Guid versionId, int? iconReference, LocalizedText? title, LocalizedText? description,
                 int sortOrder, bool displayOnWebsite, string createdBy, DateTime? createdAtUtc = null)
                 : base(createdBy, createdAtUtc)
    {
        VersionId = versionId;
        IconReference = iconReference;
        Title = title;
        Description = description;
        SortOrder = sortOrder;
        DisplayOnWebsite = displayOnWebsite;
    }
   
}