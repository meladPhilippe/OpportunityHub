using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Domain.ValueObjects;

/// <summary>
/// Represents the content required to create or update
/// a key achievement within an opportunity version.
/// </summary>
public sealed class KeyAchievementContent
{
    #region Properties

    public int? IconReference { get; init; }

    public LocalizedText? Title { get; init; }

    public LocalizedText? Description { get; init; }

    public int SortOrder { get; init; }

    public bool DisplayOnWebsite { get; init; }

    #endregion

    #region Factory

    internal static KeyAchievementContent From(
        OpportunityVersionKeyAchievement achievement)
    {
        ArgumentNullException.ThrowIfNull(achievement);

        return new KeyAchievementContent
        {
            IconReference = achievement.IconReference,
            Title = achievement.Title,
            Description = achievement.Description,
            SortOrder = achievement.SortOrder,
            DisplayOnWebsite = achievement.DisplayOnWebsite
        };
    }

    #endregion
} 