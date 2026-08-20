using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Domain.ValueObjects;

/// <summary>
/// Represents the content required to create or update
/// a feature within an opportunity version.
/// </summary>
public sealed class FeatureContent
{
    #region Properties

    public LocalizedText? Title { get; init; }

    public int? IconReference { get; init; }

    public int SortOrder { get; init; }

    public bool DisplayOnWebsite { get; init; }

    #endregion

    #region Factory

    internal static FeatureContent From(
        OpportunityVersionFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);

        return new FeatureContent
        {
            Title = feature.Title,
            IconReference = feature.IconReference,
            SortOrder = feature.SortOrder,
            DisplayOnWebsite = feature.DisplayOnWebsite
        };
    }

    #endregion
}