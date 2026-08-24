using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a feature belonging to a specific opportunity version.
/// Its lifecycle is controlled by the opportunity version.
/// </summary>
public sealed class OpportunityVersionFeature : EntityIdentity
{
    private OpportunityVersionFeature(
        FeatureContent content)
    {
        Title = content.Title;
        IconReference = content.IconReference;
        SortOrder = content.SortOrder;
        DisplayOnWebsite = content.DisplayOnWebsite;
    }
    private OpportunityVersionFeature()
    {
        
    }

    #region Properties

    public int? IconReference { get; private set; }

    public LocalizedText? Title { get; private set; }

    public int SortOrder { get; private set; }

    public bool DisplayOnWebsite { get; private set; }

    #endregion

    #region Factory

    internal static OpportunityVersionFeature Create(
        FeatureContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new OpportunityVersionFeature(content);
    }

    #endregion
}