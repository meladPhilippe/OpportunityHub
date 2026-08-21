namespace OpportunityHub.Domain.ValueObjects;

/// <summary>
/// Represents the complete content used to create or update
/// an opportunity version.
/// </summary>
public sealed class OpportunityVersionContent
{
    #region Properties

    public LocalizedText ProductName { get; init; } = null!;

    public LocalizedText? NationalImpact { get; init; }

    public LocalizedText? Description { get; init; }

    public LocalizedText? WebsiteUrl { get; init; }

    public LocalizedText? LogoReference { get; init; }

    public LocalizedText? BannerReference { get; init; }

    public LocalizedText? CompanyName { get; init; }

    public LocalizedText? CompanyWebsiteUrl { get; init; }

    public LocalizedText? AdoptedBy { get; init; }

    public LocalizedText? Beneficiaries { get; init; }

    public int? KsaAdoptingEntitiesCount { get; init; }

    public LocalizedText? ProductOwnerName { get; init; }

    public string? ProductOwnerEmail { get; init; }

    public string? ProductOwnerPhone { get; init; }

    public IReadOnlyCollection<Guid> ChannelIds { get; init; } = [];

    public IReadOnlyCollection<Guid> SectorIds { get; init; } = [];

    public IReadOnlyCollection<FeatureContent> Features { get; init; } = [];

    public IReadOnlyCollection<KeyAchievementContent> KeyAchievements { get; init; } = [];

    public IReadOnlyCollection<KpiContent> Kpis { get; init; } = [];

    #endregion
}