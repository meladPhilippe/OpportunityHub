using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a versioned snapshot of an opportunity's content.
/// Controls the version's content, associated reference data,
/// child content, editing lifecycle, and publication state.
/// </summary>
public sealed class OpportunityVersion : ChangeTrackedEntity
{
    #region Fields

    private readonly List<OpportunityVersionChannel> _channels = new();
    private readonly List<OpportunityVersionSector> _sectors = new();
    private readonly List<OpportunityVersionFeature> _features = new();
    private readonly List<OpportunityVersionKeyAchievement> _keyAchievements = new();
    private readonly List<OpportunityVersionKpi> _kpis = new();

    #endregion

    #region Properties

    public Guid OpportunityId { get; private set; }

    public int VersionNumber { get; private set; }

    public bool IsCurrent { get; private set; }

    public bool IsPublishedSnapshot { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    #region Content

    public LocalizedText OpportunityName { get; private set; } = null!;

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

    public LocalizedText? OpportunityOwnerName { get; private set; }

    public string? OpportunityOwnerEmail { get; private set; }

    public string? OpportunityOwnerPhone { get; private set; }

    #endregion

    #region Children

    public IReadOnlyCollection<OpportunityVersionChannel> Channels =>
        _channels.AsReadOnly();

    public IReadOnlyCollection<OpportunityVersionSector> Sectors =>
        _sectors.AsReadOnly();

    public IReadOnlyCollection<OpportunityVersionFeature> Features =>
        _features.AsReadOnly();

    public IReadOnlyCollection<OpportunityVersionKeyAchievement> KeyAchievements =>
        _keyAchievements.AsReadOnly();

    public IReadOnlyCollection<OpportunityVersionKpi> Kpis =>
        _kpis.AsReadOnly();

    #endregion

    #endregion

    #region Constructor

    private OpportunityVersion(
        Guid opportunityId,
        int versionNumber,
        string createdBy,
        DateTime createdAtUtc)
        : base(createdBy, createdAtUtc)
    {
        if (opportunityId == Guid.Empty)
        {
            throw new ArgumentException(
                "Opportunity ID is required.",
                nameof(opportunityId));
        }

        if (versionNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versionNumber));
        }

        OpportunityId = opportunityId;
        VersionNumber = versionNumber;
        IsCurrent = true;
    }

    private OpportunityVersion()
    {
    }

    #endregion

    #region Factory

    public static OpportunityVersion CreateInitial(
        Guid opportunityId,
        OpportunityVersionContent content,
        string createdBy,
        DateTime? createdAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        var timestamp = createdAtUtc ?? DateTime.UtcNow;

        var version = new OpportunityVersion(
            opportunityId,
            1,
            createdBy,
            timestamp);

        version.ApplyContentCore(content);

        return version;
    }

    #endregion

    #region Content

    public void ApplyContent(
        OpportunityVersionContent content,
        string updatedBy,
        DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);

        var timestamp = updatedAtUtc ?? DateTime.UtcNow;

        ApplyContentCore(content);

        TrackUpdate(
            updatedBy,
            timestamp);
    }

    private void ApplyContentCore(
        OpportunityVersionContent content)
    {
        if (content.OpportunityName is null)
        {
            throw new InvalidOperationException(
                "Opportunity name is required.");
        }

        OpportunityName = content.OpportunityName;
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

        OpportunityOwnerName = content.OpportunityOwnerName;
        OpportunityOwnerEmail = content.OpportunityOwnerEmail;
        OpportunityOwnerPhone = content.OpportunityOwnerPhone;
        ReplaceChannels(content.ChannelIds);
        ReplaceSectors(content.SectorIds);
        ReplaceFeatures(content.Features);
        ReplaceKeyAchievements(content.KeyAchievements);
        ReplaceKpis(content.Kpis);
    }

    #endregion

    #region Channels

    private void ReplaceChannels(
        IEnumerable<Guid> channelIds)
    {
        _channels.Clear();

        foreach (var channelId in channelIds.Distinct())
        {
            if (channelId == Guid.Empty)
            {
                continue;
            }

            _channels.Add(
                OpportunityVersionChannel.Create(channelId));
        }
    }

    #endregion

    #region Sectors

    private void ReplaceSectors(
        IEnumerable<Guid> sectorIds)
    {
        _sectors.Clear();

        foreach (var sectorId in sectorIds.Distinct())
        {
            if (sectorId == Guid.Empty)
            {
                continue;
            }

            _sectors.Add(
                OpportunityVersionSector.Create(sectorId));
        }
    }

    #endregion

    #region Features

    private void ReplaceFeatures(
        IEnumerable<FeatureContent> features)
    {
        _features.Clear();

        foreach (var feature in features)
        {
            ArgumentNullException.ThrowIfNull(feature);

            _features.Add(
                OpportunityVersionFeature.Create(feature));
        }
    }

    #endregion

    #region Key Achievements

    private void ReplaceKeyAchievements(
        IEnumerable<KeyAchievementContent> achievements)
    {
        _keyAchievements.Clear();

        foreach (var achievement in achievements)
        {
            ArgumentNullException.ThrowIfNull(achievement);

            _keyAchievements.Add(
                OpportunityVersionKeyAchievement.Create(achievement));
        }
    }

    #endregion

    #region KPIs

    private void ReplaceKpis(
        IEnumerable<KpiContent> kpis)
    {
        _kpis.Clear();

        foreach (var kpi in kpis)
        {
            ArgumentNullException.ThrowIfNull(kpi);

            _kpis.Add(
                OpportunityVersionKpi.Create(kpi));
        }
    }

    #endregion

    #region Versioning

    public OpportunityVersion CloneForEditing(
        int nextVersionNumber,
        string createdBy,
        DateTime? createdAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        if (nextVersionNumber <= VersionNumber)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextVersionNumber));
        }

        var timestamp = createdAtUtc ?? DateTime.UtcNow;

        MarkNotCurrent(
            createdBy,
            timestamp);

        var clone = new OpportunityVersion(
            OpportunityId,
            nextVersionNumber,
            createdBy,
            timestamp)
        {
            OpportunityName = OpportunityName,
            NationalImpact = NationalImpact,
            Description = Description,
            WebsiteUrl = WebsiteUrl,
            LogoReference = LogoReference,
            BannerReference = BannerReference,
            CompanyName = CompanyName,
            CompanyWebsiteUrl = CompanyWebsiteUrl,
            AdoptedBy = AdoptedBy,
            Beneficiaries = Beneficiaries,
            KsaAdoptingEntitiesCount = KsaAdoptingEntitiesCount,
            OpportunityOwnerName = OpportunityOwnerName,
            OpportunityOwnerEmail = OpportunityOwnerEmail,
            OpportunityOwnerPhone = OpportunityOwnerPhone
        };

        foreach (var channel in _channels)
        {
            clone._channels.Add(
                OpportunityVersionChannel.Create(
                    channel.ChannelId));
        }

        foreach (var sector in _sectors)
        {
            clone._sectors.Add(
                OpportunityVersionSector.Create(
                    sector.SectorId));
        }

        foreach (var feature in _features)
        {
            clone._features.Add(
                OpportunityVersionFeature.Create(
                    FeatureContent.From(feature)));
        }

        foreach (var achievement in _keyAchievements)
        {
            clone._keyAchievements.Add(
                OpportunityVersionKeyAchievement.Create(
                    KeyAchievementContent.From(achievement)));
        }

        foreach (var kpi in _kpis)
        {
            clone._kpis.Add(
                OpportunityVersionKpi.Create(
                    KpiContent.From(kpi)));
        }

        return clone;
    }

    private void MarkNotCurrent(
        string updatedBy,
        DateTime updatedAtUtc)
    {
        IsCurrent = false;

        TrackUpdate(
            updatedBy,
            updatedAtUtc);
    }

    #endregion

    #region Publishing

    public void Publish(
        string publishedBy,
        DateTime? publishedAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publishedBy);

        var timestamp = publishedAtUtc ?? DateTime.UtcNow;

        IsPublishedSnapshot = true;
        IsCurrent = true;
        PublishedAtUtc = timestamp;

        TrackUpdate(
            publishedBy,
            timestamp);
    }

    #endregion
}      