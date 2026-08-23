using OpportunityHub.Application.Opportunities.Models;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Application.Opportunities.Mappers;

public static class OpportunityVersionContentMapper
{
    public static OpportunityVersionContent ToDomain(
        OpportunityVersionContentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new OpportunityVersionContent
        {
            OpportunityName = ToRequiredDomain(request.OpportunityName),

            NationalImpact = ToOptionalDomain(request.NationalImpact),
            Description = ToOptionalDomain(request.Description),
            WebsiteUrl = ToOptionalDomain(request.WebsiteUrl),
            LogoReference = ToOptionalDomain(request.LogoReference),
            BannerReference = ToOptionalDomain(request.BannerReference),

            CompanyName = ToOptionalDomain(request.CompanyName),
            CompanyWebsiteUrl = ToOptionalDomain(request.CompanyWebsiteUrl),
            AdoptedBy = ToOptionalDomain(request.AdoptedBy),
            Beneficiaries = ToOptionalDomain(request.Beneficiaries),

            KsaAdoptingEntitiesCount =
                request.KsaAdoptingEntitiesCount,

            OpportunityOwnerName =
                ToOptionalDomain(request.OpportunityOwnerName),

            OpportunityOwnerEmail =
                request.OpportunityOwnerEmail,

            OpportunityOwnerPhone =
                request.OpportunityOwnerPhone,

            ChannelIds = request.ChannelIds,
            SectorIds = request.SectorIds,

            Features = request.Features
                .Select(ToDomain)
                .ToArray(),

            KeyAchievements = request.KeyAchievements
                .Select(ToDomain)
                .ToArray(),

            Kpis = request.Kpis
                .Select(ToDomain)
                .ToArray()
        };
    }

    private static LocalizedText ToRequiredDomain(
        LocalizedTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new LocalizedText(
            request.En,
            request.Ar);
    }

    private static LocalizedText? ToOptionalDomain(
        LocalizedTextRequest? request)
    {
        return request is null
            ? null
            : new LocalizedText(
                request.En,
                request.Ar);
    }

    private static FeatureContent ToDomain(
        FeatureRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new FeatureContent
        {
            Title = ToOptionalDomain(request.Title),
            IconReference = request.IconReference,
            SortOrder = request.SortOrder,
            DisplayOnWebsite = request.DisplayOnWebsite
        };
    }

    private static KeyAchievementContent ToDomain(
        KeyAchievementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new KeyAchievementContent
        {
            IconReference = request.IconReference,
            Title = ToOptionalDomain(request.Title),
            Description = ToOptionalDomain(request.Description),
            SortOrder = request.SortOrder,
            DisplayOnWebsite = request.DisplayOnWebsite
        };
    }

    private static KpiContent ToDomain(
        KpiRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new KpiContent
        {
            Title = ToOptionalDomain(request.Title),
            Value = ToOptionalDomain(request.Value),
            SortOrder = request.SortOrder
        };
    }
}