namespace OpportunityHub.Application.Opportunities.Models;

public sealed class OpportunityVersionContentRequest
{
    public LocalizedTextRequest OpportunityName { get; init; } = null!;

    public LocalizedTextRequest? NationalImpact { get; init; }

    public LocalizedTextRequest? Description { get; init; }

    public LocalizedTextRequest? WebsiteUrl { get; init; }

    public LocalizedTextRequest? LogoReference { get; init; }

    public LocalizedTextRequest? BannerReference { get; init; }

    public LocalizedTextRequest? CompanyName { get; init; }

    public LocalizedTextRequest? CompanyWebsiteUrl { get; init; }

    public LocalizedTextRequest? AdoptedBy { get; init; }

    public LocalizedTextRequest? Beneficiaries { get; init; }

    public int? KsaAdoptingEntitiesCount { get; init; }

    public LocalizedTextRequest? OpportunityOwnerName { get; init; }

    public string? OpportunityOwnerEmail { get; init; }

    public string? OpportunityOwnerPhone { get; init; }

    public IReadOnlyCollection<Guid> ChannelIds { get; init; } = [];

    public IReadOnlyCollection<Guid> SectorIds { get; init; } = [];

    public IReadOnlyCollection<FeatureRequest> Features { get; init; } = [];

    public IReadOnlyCollection<KeyAchievementRequest> KeyAchievements { get; init; } = [];

    public IReadOnlyCollection<KpiRequest> Kpis { get; init; } = [];
}