using OpportunityHub.Application.Opportunities.Mappers;
using OpportunityHub.Application.Opportunities.Models;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Application.Tests.Opportunities.Mappers;

public sealed class OpportunityVersionContentMapperTests
{
    [Fact]
    public void ToDomain_WhenRequestContainsValues_MapsProperties()
    {
        // Arrange
        var channelId1 = Guid.NewGuid();
        var channelId2 = Guid.NewGuid();

        var sectorId1 = Guid.NewGuid();
        var sectorId2 = Guid.NewGuid();

        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            NationalImpact = new LocalizedTextRequest(
                "National impact",
                "الأثر الوطني"),

            Description = new LocalizedTextRequest(
                "Description",
                "الوصف"),

            WebsiteUrl = new LocalizedTextRequest(
                "https://example.com",
                "https://example.com/ar"),

            LogoReference = new LocalizedTextRequest(
                "logo-en",
                "logo-ar"),

            BannerReference = new LocalizedTextRequest(
                "banner-en",
                "banner-ar"),

            CompanyName = new LocalizedTextRequest(
                "Company",
                "الشركة"),

            CompanyWebsiteUrl = new LocalizedTextRequest(
                "company-en",
                "company-ar"),

            AdoptedBy = new LocalizedTextRequest(
                "Adopter",
                "الجهة المتبنية"),

            Beneficiaries = new LocalizedTextRequest(
                "Beneficiaries",
                "المستفيدون"),

            KsaAdoptingEntitiesCount = 10,

            OpportunityOwnerName = new LocalizedTextRequest(
                "Owner",
                "المالك"),

            OpportunityOwnerEmail = "owner@example.com",
            OpportunityOwnerPhone = "+966500000000",

            ChannelIds =
            [
                channelId1,
                channelId2
            ],

            SectorIds =
            [
                sectorId1,
                sectorId2
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        AssertLocalizedText(
            result.OpportunityName,
            "Opportunity",
            "فرصة");

        AssertLocalizedText(
            result.NationalImpact,
            "National impact",
            "الأثر الوطني");

        AssertLocalizedText(
            result.Description,
            "Description",
            "الوصف");

        AssertLocalizedText(
            result.WebsiteUrl,
            "https://example.com",
            "https://example.com/ar");

        AssertLocalizedText(
            result.LogoReference,
            "logo-en",
            "logo-ar");

        AssertLocalizedText(
            result.BannerReference,
            "banner-en",
            "banner-ar");

        AssertLocalizedText(
            result.CompanyName,
            "Company",
            "الشركة");

        AssertLocalizedText(
            result.CompanyWebsiteUrl,
            "company-en",
            "company-ar");

        AssertLocalizedText(
            result.AdoptedBy,
            "Adopter",
            "الجهة المتبنية");

        AssertLocalizedText(
            result.Beneficiaries,
            "Beneficiaries",
            "المستفيدون");

        Assert.Equal(
            10,
            result.KsaAdoptingEntitiesCount);

        AssertLocalizedText(
            result.OpportunityOwnerName,
            "Owner",
            "المالك");

        Assert.Equal(
            "owner@example.com",
            result.OpportunityOwnerEmail);

        Assert.Equal(
            "+966500000000",
            result.OpportunityOwnerPhone);

        Assert.Equal(
            [channelId1, channelId2],
            result.ChannelIds);

        Assert.Equal(
            [sectorId1, sectorId2],
            result.SectorIds);
    }

    [Fact]
    public void ToDomain_WhenOptionalValuesAreNull_MapsThemAsNull()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة")
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        AssertLocalizedText(
            result.OpportunityName,
            "Opportunity",
            "فرصة");

        Assert.Null(result.NationalImpact);
        Assert.Null(result.Description);
        Assert.Null(result.WebsiteUrl);
        Assert.Null(result.LogoReference);
        Assert.Null(result.BannerReference);
        Assert.Null(result.CompanyName);
        Assert.Null(result.CompanyWebsiteUrl);
        Assert.Null(result.AdoptedBy);
        Assert.Null(result.Beneficiaries);
        Assert.Null(result.OpportunityOwnerName);

        Assert.Null(result.KsaAdoptingEntitiesCount);
        Assert.Null(result.OpportunityOwnerEmail);
        Assert.Null(result.OpportunityOwnerPhone);

        Assert.Empty(result.ChannelIds);
        Assert.Empty(result.SectorIds);
        Assert.Empty(result.Features);
        Assert.Empty(result.KeyAchievements);
        Assert.Empty(result.Kpis);
    }

    [Fact]
    public void ToDomain_WhenFeaturesProvided_MapsFeatures()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            Features =
            [
                new FeatureRequest(
                    new LocalizedTextRequest(
                        "Feature 1",
                        "الميزة 1"),
                    10,
                    2,
                    true)
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        var feature = Assert.Single(result.Features);

        AssertLocalizedText(
            feature.Title,
            "Feature 1",
            "الميزة 1");

        Assert.Equal(
            10,
            feature.IconReference);

        Assert.Equal(
            2,
            feature.SortOrder);

        Assert.True(
            feature.DisplayOnWebsite);
    }

    [Fact]
    public void ToDomain_WhenKeyAchievementsProvided_MapsKeyAchievements()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            KeyAchievements =
            [
                new KeyAchievementRequest(
                    20,
                    new LocalizedTextRequest(
                        "Achievement",
                        "الإنجاز"),
                    new LocalizedTextRequest(
                        "Achievement description",
                        "وصف الإنجاز"),
                    1,
                    true)
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        var achievement =
            Assert.Single(result.KeyAchievements);

        Assert.Equal(
            20,
            achievement.IconReference);

        AssertLocalizedText(
            achievement.Title,
            "Achievement",
            "الإنجاز");

        AssertLocalizedText(
            achievement.Description,
            "Achievement description",
            "وصف الإنجاز");

        Assert.Equal(
            1,
            achievement.SortOrder);

        Assert.True(
            achievement.DisplayOnWebsite);
    }

    [Fact]
    public void ToDomain_WhenKpisProvided_MapsKpis()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            Kpis =
            [
                new KpiRequest(
                    new LocalizedTextRequest(
                        "Revenue",
                        "الإيرادات"),
                    new LocalizedTextRequest(
                        "100",
                        "١٠٠"),
                    1)
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        var kpi = Assert.Single(result.Kpis);

        AssertLocalizedText(
            kpi.Title,
            "Revenue",
            "الإيرادات");

        AssertLocalizedText(
            kpi.Value,
            "100",
            "١٠٠");

        Assert.Equal(
            1,
            kpi.SortOrder);
    }

    [Fact]
    public void ToDomain_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () =>
                OpportunityVersionContentMapper.ToDomain(null!));
    }

    [Fact]
    public void ToDomain_WhenFeatureTitleIsNull_MapsTitleAsNull()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            Features =
            [
                new FeatureRequest(
                    null,
                    null,
                    1,
                    false)
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        var feature = Assert.Single(result.Features);

        Assert.Null(feature.Title);
        Assert.Null(feature.IconReference);
        Assert.Equal(1, feature.SortOrder);
        Assert.False(feature.DisplayOnWebsite);
    }

    [Fact]
    public void ToDomain_WhenKeyAchievementOptionalValuesAreNull_MapsThemAsNull()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            KeyAchievements =
            [
                new KeyAchievementRequest(
                    null,
                    null,
                    null,
                    1,
                    false)
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        var achievement =
            Assert.Single(result.KeyAchievements);

        Assert.Null(achievement.IconReference);
        Assert.Null(achievement.Title);
        Assert.Null(achievement.Description);
        Assert.Equal(1, achievement.SortOrder);
        Assert.False(achievement.DisplayOnWebsite);
    }

    [Fact]
    public void ToDomain_WhenKpiOptionalValuesAreNull_MapsThemAsNull()
    {
        // Arrange
        var request = new OpportunityVersionContentRequest
        {
            OpportunityName = new LocalizedTextRequest(
                "Opportunity",
                "فرصة"),

            Kpis =
            [
                new KpiRequest(
                    null,
                    null,
                    1)
            ]
        };

        // Act
        var result =
            OpportunityVersionContentMapper.ToDomain(request);

        // Assert
        var kpi = Assert.Single(result.Kpis);

        Assert.Null(kpi.Title);
        Assert.Null(kpi.Value);
        Assert.Equal(1, kpi.SortOrder);
    }

    private static void AssertLocalizedText(
        LocalizedText? actual,
        string expectedEn,
        string expectedAr)
    {
        Assert.NotNull(actual);

        Assert.Equal(
            expectedEn,
            actual.En);

        Assert.Equal(
            expectedAr,
            actual.Ar);
    }
}