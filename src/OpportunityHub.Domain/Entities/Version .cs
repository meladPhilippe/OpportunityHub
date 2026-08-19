using OpportunityHub.Domain;
using OpportunityHub.Domain.ValueObjects;

public class Version : ChangeTrackedEntity
{
   #region Fields

    private readonly List<VersionChannel> _channels = [];
    private readonly List<VersionSector> _sectors = [];
    private readonly List<VersionFeature> _features = [];
    private readonly List<VersionKeyAchievement> _keyAchievements = [];
    private readonly List<VersionKpi> _kpis = [];

    #endregion

   #region Properties

    public Guid OpportunityId { get; private set; }

    // Version
    public int VersionNumber { get; private set; }

    public bool IsCurrent { get; private set; }

    public bool IsPublishedSnapshot { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    // Content
    public LocalizedText ProductName { get; private set; } = null!;

    public LocalizedText? NationalImpact { get; private set; }

    public LocalizedText? Description { get; private set; }

    public LocalizedText? WebsiteUrl { get; private set; }

    public LocalizedText? LogoReference { get; private set; }

    public LocalizedText? BannerReference { get; private set; }

    public LocalizedText? CompanyName { get; private set; }

    public LocalizedText? CompanyWebsiteUrl { get; private set; }

    public LocalizedText? AdoptedBy { get; private set; }

    public LocalizedText? Beneficiaries { get; private set; }

    public int? KsaAdoptingEntitiesCount { get; private set; }

    public LocalizedText? ProductOwnerName { get; private set; }

    public string? ProductOwnerEmail { get; private set; }

    public string? ProductOwnerPhone { get; private set; }

    public LocalizedText? ProductOwnerPageUrl { get; private set; }

    // Children
    public IReadOnlyCollection<VersionChannel> Channels =>
        _channels.AsReadOnly();

    public IReadOnlyCollection<VersionSector> Sectors =>
        _sectors.AsReadOnly();

    public IReadOnlyCollection<VersionFeature> Features =>
        _features.AsReadOnly();

    public IReadOnlyCollection<VersionKeyAchievement> KeyAchievements =>
        _keyAchievements.AsReadOnly();

    public IReadOnlyCollection<VersionKpi> Kpis =>
        _kpis.AsReadOnly();

    #endregion

    #region Factory

    protected Version()
    {
    }
    public static Version CreateInitial(
        Guid opportunityId,
        VersionContent content,
        string userId,
        string? userName,
        DateTime? occurredAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(content.ProductName);

        var timestamp = occurredAtUtc ?? DateTime.UtcNow;
        var user = NormalizeUserName(userId, userName);

        var version = new Version
        {
            OpportunityId = opportunityId,
            VersionNumber = 1,
            IsCurrent = true,
            IsPublishedSnapshot = false,
            CreatedAtUtc = timestamp,
            CreatedBy = user
        };

        version.ApplyContentCore(
            content,
            user,
            timestamp);

        return version;
    }

    #endregion

    #region Content

    public void ApplyContent(
        VersionContent content,
        string userId,
        string? userName,
        DateTime? occurredAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(content.ProductName);

        var timestamp = occurredAtUtc ?? DateTime.UtcNow;
        var user = NormalizeUserName(userId, userName);

        ApplyContentCore(content, user, timestamp);
        TrackUpdate(user, timestamp);
    }
    private void ApplyContentCore(
        VersionContent content,
        string user,
        DateTime occurredAtUtc)
    {
        ProductName = content.ProductName;
        NationalImpact = content.NationalImpact;
        Description = content.Description;
        WebsiteUrl = content.WebsiteUrl;
        LogoReference = content.LogoReference;
        BannerReference = content.BannerReference;

        CompanyName = content.CompanyName;
        CompanyWebsiteUrl = content.CompanyWebsiteUrl;
        AdoptedBy = content.AdoptedBy;
        Beneficiaries = content.Beneficiaries;

        KsaAdoptingEntitiesCount =
            content.KsaAdoptingEntitiesCount;

        ProductOwnerName = content.ProductOwnerName;
        ProductOwnerEmail = content.ProductOwnerEmail;
        ProductOwnerPhone = content.ProductOwnerPhone;
        ProductOwnerPageUrl = content.ProductOwnerPageUrl;

        ReplaceChannels(content.ChannelIds,  user,  occurredAtUtc);
        ReplaceSectors( content.SectorIds, user,occurredAtUtc);
        ReplaceFeatures(content.Features, user, occurredAtUtc);
        ReplaceKeyAchievements(content.KeyAchievements, user, occurredAtUtc);
        ReplaceKpis(content.Kpis, user, occurredAtUtc);
    }
    private void ReplaceChannels( IEnumerable<Guid> channelIds,  string createdBy, DateTime occurredAtUtc)
    {
        _channels.Clear();

        foreach (var channelId in channelIds.Distinct())
        {
            _channels.Add(
                new VersionChannel(
                    Id,
                    channelId,
                    createdBy,
                    occurredAtUtc));
        }
    }
    private void ReplaceSectors(IEnumerable<Guid> sectorIds,string createdBy,DateTime occurredAtUtc)
    {
        _sectors.Clear();

        foreach (var sectorId in sectorIds.Distinct())
        {
            _sectors.Add(
                new VersionSector(
                    Id,
                    sectorId,
                    createdBy,
                    occurredAtUtc));
        }
    }
    private void ReplaceFeatures( IEnumerable<VersionFeature> features, string createdBy, DateTime occurredAtUtc)
    {
        _features.Clear();

        foreach (var feature in features)
        {
            _features.Add(new VersionFeature(
                Id,
                feature.Title,
                feature.IconReference,
                feature.SortOrder,
                feature.DisplayOnWebsite,
                createdBy,
                occurredAtUtc));
        }
    }
    private void ReplaceKeyAchievements( IEnumerable<VersionKeyAchievement> keyAchievements, string createdBy, DateTime occurredAtUtc)
    {
        _keyAchievements.Clear();

        foreach (var keyAchievement in keyAchievements)
        {
            _keyAchievements.Add(new VersionKeyAchievement(
                Id,
                keyAchievement.IconReference,
                keyAchievement.Title,
                keyAchievement.Description,
                keyAchievement.SortOrder,
                keyAchievement.DisplayOnWebsite,
                createdBy,
                occurredAtUtc));
        }
    }
    private void ReplaceKpis(   IEnumerable<VersionKpi> kpis,   string createdBy,  DateTime occurredAtUtc)
    {
        _kpis.Clear();

        foreach (var kpi in kpis)
        {
            _kpis.Add(new VersionKpi(
                Id,
                kpi.Title,
                kpi.Value,
                kpi.SortOrder,
                createdBy,
                occurredAtUtc));
        }
    }
    
    #endregion
    
    #region Versioning
    public Version CloneForEditing(
        int nextVersionNumber,
        string userId,
        string? userName,
        DateTime? occurredAtUtc = null)
    {
        var timestamp = occurredAtUtc ?? DateTime.UtcNow;
        var user = NormalizeUserName(userId, userName);

        MarkNotCurrent(
            user,
            timestamp);

        var clone = new Version
        {
            OpportunityId = OpportunityId,
            VersionNumber = nextVersionNumber,
            IsCurrent = true,
            IsPublishedSnapshot = false,
            PublishedAtUtc = null,

            CreatedAtUtc = timestamp,
            CreatedBy = user,

            ProductName = ProductName,
            NationalImpact = NationalImpact,
            Description = Description,
            WebsiteUrl = WebsiteUrl,
            LogoReference = LogoReference,
            BannerReference = BannerReference,

            CompanyName = CompanyName,
            CompanyWebsiteUrl = CompanyWebsiteUrl,
            AdoptedBy = AdoptedBy,
            Beneficiaries = Beneficiaries,

            KsaAdoptingEntitiesCount =
                KsaAdoptingEntitiesCount,

            ProductOwnerName = ProductOwnerName,
            ProductOwnerEmail = ProductOwnerEmail,
            ProductOwnerPhone = ProductOwnerPhone,
            ProductOwnerPageUrl = ProductOwnerPageUrl
        };

        foreach (var channel in _channels)
        {
            clone._channels.Add(
                new VersionChannel(
                    clone.Id,
                    channel.ChannelId,
                    user,
                    timestamp));
        }

        foreach (var sector in _sectors)
        {
            clone._sectors.Add(
                new VersionSector(
                    clone.Id,
                    sector.SectorId,
                    user,
                    timestamp));
        }

        foreach (var feature in _features)
        {
            clone._features.Add(
                new VersionFeature(
                    clone.Id,
                        feature.Title,
                        feature.IconReference,
                        feature.SortOrder,
                        feature.DisplayOnWebsite,
                    user,
                    timestamp));
        }

        foreach (var achievement in _keyAchievements)
        {
            clone._keyAchievements.Add(
        new VersionKeyAchievement(
            clone.Id,
            achievement.IconReference,
            achievement.Title,
            achievement.Description,
            achievement.SortOrder,
            achievement.DisplayOnWebsite,
            user,
            timestamp));
        }

        foreach (var kpi in _kpis)
        {
            clone._kpis.Add(
                   new VersionKpi(
                            clone.Id,
                            kpi.Title,
                            kpi.Value,
                            kpi.SortOrder,
                            user,
                            timestamp));
        }

        return clone;
    }

    public void MarkNotCurrent(
        string userId,
        DateTime occurredAtUtc)
    {
        IsCurrent = false;

        UpdatedAtUtc = occurredAtUtc;
        UpdatedBy = userId;
    }

    #endregion

    #region Publishing

    public void Publish(
        string userId,
        DateTime occurredAtUtc)
    {
        IsPublishedSnapshot = true;
        IsCurrent = true;
        PublishedAtUtc = occurredAtUtc;

        UpdatedAtUtc = occurredAtUtc;
        UpdatedBy = userId;
    }

    #endregion

    #region Helpers
    private static string NormalizeUserName(
        string userId,
        string? userName)
    {
        return string.IsNullOrWhiteSpace(userName)
            ? userId
            : userName;
    }

    #endregion

}