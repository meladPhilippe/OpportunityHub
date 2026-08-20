using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a key achievement belonging to an opportunity version.
/// Its lifecycle is controlled by the opportunity version.
/// </summary> 
public sealed class OpportunityVersionKeyAchievement : EntityIdentity
{
    private OpportunityVersionKeyAchievement(
        KeyAchievementContent content)
    {
        IconReference = content.IconReference;
        Title = content.Title;
        Description = content.Description;
        SortOrder = content.SortOrder;
        DisplayOnWebsite = content.DisplayOnWebsite;
    }

    #region Properties

    public int? IconReference { get; private set; }

    public LocalizedText? Title { get; private set; }

    public LocalizedText? Description { get; private set; }

    public int SortOrder { get; private set; }

    public bool DisplayOnWebsite { get; private set; }

    #endregion

    #region Factory

    internal static OpportunityVersionKeyAchievement Create(
        KeyAchievementContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new OpportunityVersionKeyAchievement(content);
    }

    #endregion
}