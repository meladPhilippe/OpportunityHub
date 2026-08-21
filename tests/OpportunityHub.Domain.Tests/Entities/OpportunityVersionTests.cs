using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Tests.Entities;

public sealed class OpportunityVersionTests
{
    private static readonly Guid OpportunityId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid ChannelId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid SectorId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    private const string CreatedBy = "user-1";
    private const string UpdatedBy = "user-2";

    private static readonly DateTime CreatedAtUtc =
        new(
            2026,
            8,
            21,
            10,
            0,
            0,
            DateTimeKind.Utc);

    private static readonly DateTime UpdatedAtUtc =
        new(
            2026,
            8,
            21,
            11,
            0,
            0,
            DateTimeKind.Utc);

    #region Creation

    /// <summary>
    /// Verifies that an initial opportunity version is created with
    /// version number one and the supplied content and creation information.
    /// </summary>
    [Fact]
    public void CreateInitial_ShouldCreateVersion()
    {
        // Arrange
        var content = CreateContent();

        // Act
        var version = OpportunityVersion.CreateInitial(
            OpportunityId,
            content,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.NotEqual(Guid.Empty, version.Id);
        Assert.Equal(OpportunityId, version.OpportunityId);
        Assert.Equal(1, version.VersionNumber);
        Assert.True(version.IsCurrent);
        Assert.False(version.IsPublishedSnapshot);
        Assert.Null(version.PublishedAtUtc);

        Assert.Equal(CreatedBy, version.CreatedBy);
        Assert.Equal(CreatedAtUtc, version.CreatedAtUtc);

        AssertContent(version, content);
    }

    /// <summary>
    /// Verifies that the initial version uses the current UTC time
    /// when no creation timestamp is provided.
    /// </summary>
    [Fact]
    public void CreateInitial_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var version = OpportunityVersion.CreateInitial(
            OpportunityId,
            CreateContent(),
            CreatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            version.CreatedAtUtc,
            before,
            after);
    }

    /// <summary>
    /// Verifies that creating an initial version fails when
    /// the opportunity ID is empty.
    /// </summary>
    [Fact]
    public void CreateInitial_ShouldRejectEmptyOpportunityId()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            OpportunityVersion.CreateInitial(
                Guid.Empty,
                CreateContent(),
                CreatedBy,
                CreatedAtUtc));

        Assert.StartsWith(
            "Opportunity ID is required.",
            exception.Message);

        Assert.Equal(
            "opportunityId",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creating an initial version fails when
    /// the content is null.
    /// </summary>
    [Fact]
    public void CreateInitial_ShouldRejectNullContent()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            OpportunityVersion.CreateInitial(
                OpportunityId,
                null!,
                CreatedBy,
                CreatedAtUtc));
    }

    /// <summary>
    /// Verifies that creating an initial version fails when
    /// the creator is empty.
    /// </summary>
    [Fact]
    public void CreateInitial_ShouldRejectEmptyCreatedBy()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OpportunityVersion.CreateInitial(
                OpportunityId,
                CreateContent(),
                string.Empty,
                CreatedAtUtc));
    }

    /// <summary>
    /// Verifies that creating an initial version fails when
    /// the product name is not provided.
    /// </summary>
    [Fact]
    public void CreateInitial_ShouldRejectMissingProductName()
    {
        // Arrange
        var content = new OpportunityVersionContent
        {
            NationalImpact = new LocalizedText("Impact"),
            Description = new LocalizedText("Description"),
            WebsiteUrl = new LocalizedText("https://example.com"),
            ChannelIds = [ChannelId],
            SectorIds = [SectorId]
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            OpportunityVersion.CreateInitial(
                OpportunityId,
                content,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "Product name is required.",
            exception.Message);
    }

    #endregion

    #region Content

    /// <summary>
    /// Verifies that applying content replaces the version content
    /// and updates the update-tracking information.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceContentAndTrackUpdate()
    {
        // Arrange
        var version = CreateVersion();
        var content = CreateContent("Updated Product");

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        AssertContent(version, content);

        Assert.Equal(
            UpdatedBy,
            version.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            version.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that applying content fails when the content is null.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldRejectNullContent()
    {
        // Arrange
        var version = CreateVersion();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            version.ApplyContent(
                null!,
                UpdatedBy,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that applying content fails when the updater is empty.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldRejectEmptyUpdatedBy()
    {
        // Arrange
        var version = CreateVersion();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            version.ApplyContent(
                CreateContent(),
                string.Empty,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that applying content fails when the product name
    /// is not provided.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldRejectMissingProductName()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            version.ApplyContent(
                content,
                UpdatedBy,
                UpdatedAtUtc));

        Assert.Equal(
            "Product name is required.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that applying content fails when a feature entry is null.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldRejectNullFeature()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Features =
            [
                null!
            ]
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            version.ApplyContent(
                content,
                UpdatedBy,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that applying content fails when a key achievement entry is null.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldRejectNullKeyAchievement()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            KeyAchievements =
            [
                null!
            ]
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            version.ApplyContent(
                content,
                UpdatedBy,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that applying content fails when a KPI entry is null.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldRejectNullKpi()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Kpis =
            [
                null!
            ]
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            version.ApplyContent(
                content,
                UpdatedBy,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that applying content with empty collections
    /// removes all existing child content.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldClearExistingChildren_WhenCollectionsAreEmpty()
    {
        // Arrange
        var version = CreateVersion();

        Assert.NotEmpty(version.Channels);
        Assert.NotEmpty(version.Sectors);
        Assert.NotEmpty(version.Features);
        Assert.NotEmpty(version.KeyAchievements);
        Assert.NotEmpty(version.Kpis);

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Updated Product"),
            ChannelIds = [],
            SectorIds = [],
            Features = [],
            KeyAchievements = [],
            Kpis = []
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Empty(version.Channels);
        Assert.Empty(version.Sectors);
        Assert.Empty(version.Features);
        Assert.Empty(version.KeyAchievements);
        Assert.Empty(version.Kpis);
    }

    #endregion

    #region Channels

    /// <summary>
    /// Verifies that channel associations are created from the supplied
    /// channel IDs and duplicate IDs are ignored.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceChannelsAndRemoveDuplicates()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            ChannelIds =
            [
                ChannelId,
                ChannelId,
                Guid.NewGuid()
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            2,
            version.Channels.Count);

        Assert.Contains(
            version.Channels,
            channel => channel.ChannelId == ChannelId);
    }

    /// <summary>
    /// Verifies that empty channel IDs are ignored when
    /// channel associations are created.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldIgnoreEmptyChannelIds()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            ChannelIds =
            [
                Guid.Empty,
                ChannelId
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.Channels);

        Assert.Equal(
            ChannelId,
            version.Channels.Single().ChannelId);
    }

    /// <summary>
    /// Verifies that applying new content replaces existing channel
    /// associations instead of appending to them.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceExistingChannels()
    {
        // Arrange
        var version = CreateVersion();

        var firstContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            ChannelIds = [ChannelId]
        };

        version.ApplyContent(
            firstContent,
            UpdatedBy,
            UpdatedAtUtc);

        var secondChannelId = Guid.NewGuid();

        var secondContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            ChannelIds = [secondChannelId]
        };

        // Act
        version.ApplyContent(
            secondContent,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.Channels);

        Assert.Equal(
            secondChannelId,
            version.Channels.Single().ChannelId);
    }

    #endregion

    #region Sectors

    /// <summary>
    /// Verifies that sector associations are created from the supplied
    /// sector IDs and duplicate IDs are ignored.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceSectorsAndRemoveDuplicates()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            SectorIds =
            [
                SectorId,
                SectorId,
                Guid.NewGuid()
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            2,
            version.Sectors.Count);

        Assert.Contains(
            version.Sectors,
            sector => sector.SectorId == SectorId);
    }

    /// <summary>
    /// Verifies that empty sector IDs are ignored when
    /// sector associations are created.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldIgnoreEmptySectorIds()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            SectorIds =
            [
                Guid.Empty,
                SectorId
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.Sectors);

        Assert.Equal(
            SectorId,
            version.Sectors.Single().SectorId);
    }

    /// <summary>
    /// Verifies that applying new content replaces existing sector
    /// associations instead of appending to them.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceExistingSectors()
    {
        // Arrange
        var version = CreateVersion();

        var firstContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            SectorIds = [SectorId]
        };

        version.ApplyContent(
            firstContent,
            UpdatedBy,
            UpdatedAtUtc);

        var secondSectorId = Guid.NewGuid();

        var secondContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            SectorIds = [secondSectorId]
        };

        // Act
        version.ApplyContent(
            secondContent,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.Sectors);

        Assert.Equal(
            secondSectorId,
            version.Sectors.Single().SectorId);
    }

    #endregion

    #region Features

    /// <summary>
    /// Verifies that feature content is converted into
    /// opportunity version feature entities.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldCreateFeatures()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Features =
            [
                new FeatureContent
                {
                    Title = new LocalizedText(
                        "Feature 1",
                        "الميزة 1"),
                    IconReference = 10,
                    SortOrder = 1,
                    DisplayOnWebsite = true
                },
                new FeatureContent
                {
                    Title = new LocalizedText("Feature 2"),
                    IconReference = 20,
                    SortOrder = 2,
                    DisplayOnWebsite = false
                }
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            2,
            version.Features.Count);

        var first = version.Features
            .OrderBy(x => x.SortOrder)
            .First();

        Assert.Equal(
            "Feature 1",
            first.Title!.En);

        Assert.Equal(
            "الميزة 1",
            first.Title.Ar);

        Assert.Equal(
            10,
            first.IconReference);

        Assert.Equal(
            1,
            first.SortOrder);

        Assert.True(first.DisplayOnWebsite);
    }

    /// <summary>
    /// Verifies that applying content replaces the existing
    /// feature collection.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceFeatures()
    {
        // Arrange
        var version = CreateVersion();

        var firstContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Features =
            [
                new FeatureContent
                {
                    Title = new LocalizedText("Feature 1"),
                    SortOrder = 1
                }
            ]
        };

        version.ApplyContent(
            firstContent,
            UpdatedBy,
            UpdatedAtUtc);

        var secondContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Features =
            [
                new FeatureContent
                {
                    Title = new LocalizedText("Feature 2"),
                    SortOrder = 2
                }
            ]
        };

        // Act
        version.ApplyContent(
            secondContent,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.Features);

        Assert.Equal(
            "Feature 2",
            version.Features.Single().Title!.En);
    }

    #endregion

    #region Key Achievements

    /// <summary>
    /// Verifies that key achievement content is converted into
    /// opportunity version key achievement entities.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldCreateKeyAchievements()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            KeyAchievements =
            [
                new KeyAchievementContent
                {
                    IconReference = 10,
                    Title = new LocalizedText(
                        "Achievement 1",
                        "الإنجاز 1"),
                    Description = new LocalizedText(
                        "Description"),
                    SortOrder = 1,
                    DisplayOnWebsite = true
                }
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        var achievement =
            Assert.Single(version.KeyAchievements);

        Assert.Equal(
            10,
            achievement.IconReference);

        Assert.Equal(
            "Achievement 1",
            achievement.Title!.En);

        Assert.Equal(
            "الإنجاز 1",
            achievement.Title.Ar);

        Assert.Equal(
            "Description",
            achievement.Description!.En);

        Assert.Equal(
            1,
            achievement.SortOrder);

        Assert.True(achievement.DisplayOnWebsite);
    }

    /// <summary>
    /// Verifies that applying content replaces the existing
    /// key achievement collection.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceKeyAchievements()
    {
        // Arrange
        var version = CreateVersion();

        var firstContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            KeyAchievements =
            [
                new KeyAchievementContent
                {
                    Title = new LocalizedText("Achievement 1"),
                    SortOrder = 1
                }
            ]
        };

        version.ApplyContent(
            firstContent,
            UpdatedBy,
            UpdatedAtUtc);

        var secondContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            KeyAchievements =
            [
                new KeyAchievementContent
                {
                    Title = new LocalizedText("Achievement 2"),
                    SortOrder = 2
                }
            ]
        };

        // Act
        version.ApplyContent(
            secondContent,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.KeyAchievements);

        Assert.Equal(
            "Achievement 2",
            version.KeyAchievements.Single().Title!.En);
    }

    #endregion

    #region KPIs

    /// <summary>
    /// Verifies that KPI content is converted into
    /// opportunity version KPI entities.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldCreateKpis()
    {
        // Arrange
        var version = CreateVersion();

        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Kpis =
            [
                new KpiContent
                {
                    Title = new LocalizedText("Revenue"),
                    Value = new LocalizedText("100M"),
                    SortOrder = 1
                }
            ]
        };

        // Act
        version.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        var kpi = Assert.Single(version.Kpis);

        Assert.Equal(
            "Revenue",
            kpi.Title!.En);

        Assert.Equal(
            "100M",
            kpi.Value!.En);

        Assert.Equal(
            1,
            kpi.SortOrder);
    }

    /// <summary>
    /// Verifies that applying content replaces the existing KPI collection.
    /// </summary>
    [Fact]
    public void ApplyContent_ShouldReplaceKpis()
    {
        // Arrange
        var version = CreateVersion();

        var firstContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Kpis =
            [
                new KpiContent
                {
                    Title = new LocalizedText("Revenue"),
                    Value = new LocalizedText("100M"),
                    SortOrder = 1
                }
            ]
        };

        version.ApplyContent(
            firstContent,
            UpdatedBy,
            UpdatedAtUtc);

        var secondContent = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Product"),
            Kpis =
            [
                new KpiContent
                {
                    Title = new LocalizedText("Users"),
                    Value = new LocalizedText("1M"),
                    SortOrder = 2
                }
            ]
        };

        // Act
        version.ApplyContent(
            secondContent,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Single(version.Kpis);

        Assert.Equal(
            "Users",
            version.Kpis.Single().Title!.En);
    }

    #endregion

    #region Versioning

    /// <summary>
    /// Verifies that cloning a version creates the next version
    /// with the same content and makes the original version non-current.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldCreateNextVersionAndCloneContent()
    {
        // Arrange
        var version = CreateVersion();

        // Act
        var clone = version.CloneForEditing(
            2,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.NotEqual(
            version.Id,
            clone.Id);

        Assert.Equal(
            version.OpportunityId,
            clone.OpportunityId);

        Assert.Equal(
            2,
            clone.VersionNumber);

        Assert.False(version.IsCurrent);
        Assert.True(clone.IsCurrent);

        Assert.Equal(
            UpdatedBy,
            version.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            version.UpdatedAtUtc);

        Assert.Equal(
            UpdatedBy,
            clone.CreatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            clone.CreatedAtUtc);

        AssertContent(
            clone,
            CreateContent());

        Assert.Equal(
            version.Channels.Select(x => x.ChannelId),
            clone.Channels.Select(x => x.ChannelId));

        Assert.Equal(
            version.Sectors.Select(x => x.SectorId),
            clone.Sectors.Select(x => x.SectorId));

        Assert.Equal(
            version.Features.Count,
            clone.Features.Count);

        Assert.Equal(
            version.KeyAchievements.Count,
            clone.KeyAchievements.Count);

        Assert.Equal(
            version.Kpis.Count,
            clone.Kpis.Count);
    }

    /// <summary>
    /// Verifies that cloning fails when the next version number
    /// is not greater than the current version number.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldRejectInvalidVersionNumber()
    {
        // Arrange
        var version = CreateVersion();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            version.CloneForEditing(
                1,
                UpdatedBy,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that cloning fails when the creator is empty.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldRejectEmptyCreatedBy()
    {
        // Arrange
        var version = CreateVersion();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            version.CloneForEditing(
                2,
                string.Empty,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that cloning supports any version number greater
    /// than the current version number.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldAllowGreaterVersionNumber()
    {
        // Arrange
        var version = CreateVersion();

        // Act
        var clone = version.CloneForEditing(
            5,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            5,
            clone.VersionNumber);

        Assert.False(version.IsCurrent);
        Assert.True(clone.IsCurrent);
    }

    /// <summary>
    /// Verifies that cloning a published version creates an unpublished
    /// editable version.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldCreateUnpublishedVersion()
    {
        // Arrange
        var version = CreateVersion();

        version.Publish(
            CreatedBy,
            CreatedAtUtc);

        Assert.True(version.IsPublishedSnapshot);
        Assert.NotNull(version.PublishedAtUtc);

        // Act
        var clone = version.CloneForEditing(
            2,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.False(clone.IsPublishedSnapshot);
        Assert.Null(clone.PublishedAtUtc);
        Assert.True(clone.IsCurrent);
    }

    /// <summary>
    /// Verifies that cloned child entities receive new identities
    /// while preserving their content.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldCreateNewChildEntities()
    {
        // Arrange
        var version = CreateVersion();

        // Act
        var clone = version.CloneForEditing(
            2,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        var originalChannel =
            Assert.Single(version.Channels);

        var clonedChannel =
            Assert.Single(clone.Channels);

        Assert.Equal(
            originalChannel.ChannelId,
            clonedChannel.ChannelId);

        var originalSector =
            Assert.Single(version.Sectors);

        var clonedSector =
            Assert.Single(clone.Sectors);

        Assert.Equal(
            originalSector.SectorId,
            clonedSector.SectorId);

        var originalFeature =
            Assert.Single(version.Features);

        var clonedFeature =
            Assert.Single(clone.Features);

        Assert.NotEqual(
            originalFeature.Id,
            clonedFeature.Id);

        var originalAchievement =
            Assert.Single(version.KeyAchievements);

        var clonedAchievement =
            Assert.Single(clone.KeyAchievements);

        Assert.NotEqual(
            originalAchievement.Id,
            clonedAchievement.Id);

        var originalKpi =
            Assert.Single(version.Kpis);

        var clonedKpi =
            Assert.Single(clone.Kpis);

        Assert.NotEqual(
            originalKpi.Id,
            clonedKpi.Id);
    }

    /// <summary>
    /// Verifies that modifying the cloned version does not modify
    /// the child collections of the original version.
    /// </summary>
    [Fact]
    public void CloneForEditing_ShouldCreateIndependentChildCollections()
    {
        // Arrange
        var version = CreateVersion();

        var clone = version.CloneForEditing(
            2,
            UpdatedBy,
            UpdatedAtUtc);

        // Act
        var content = new OpportunityVersionContent
        {
            ProductName = new LocalizedText("Changed"),
            ChannelIds = [Guid.NewGuid()],
            SectorIds = [Guid.NewGuid()],
            Features =
            [
                new FeatureContent
                {
                    Title = new LocalizedText("New Feature")
                }
            ],
            KeyAchievements =
            [
                new KeyAchievementContent
                {
                    Title = new LocalizedText("New Achievement")
                }
            ],
            Kpis =
            [
                new KpiContent
                {
                    Title = new LocalizedText("New KPI")
                }
            ]
        };

        clone.ApplyContent(
            content,
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            "Product",
            version.ProductName.En);

        Assert.Single(version.Channels);
        Assert.Single(version.Sectors);
        Assert.Single(version.Features);
        Assert.Single(version.KeyAchievements);
        Assert.Single(version.Kpis);

        Assert.Equal(
            "Changed",
            clone.ProductName.En);
    }

    #endregion

    #region Publishing

    /// <summary>
    /// Verifies that publishing a version marks it as a published snapshot,
    /// keeps it current, records the publication timestamp, and tracks the publisher.
    /// </summary>
    [Fact]
    public void Publish_ShouldPublishVersion()
    {
        // Arrange
        var version = CreateVersion();

        // Act
        version.Publish(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.True(version.IsPublishedSnapshot);
        Assert.True(version.IsCurrent);

        Assert.Equal(
            UpdatedAtUtc,
            version.PublishedAtUtc);

        Assert.Equal(
            UpdatedBy,
            version.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            version.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that publishing a version uses the current UTC time
    /// when no publication timestamp is provided.
    /// </summary>
    [Fact]
    public void Publish_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var version = CreateVersion();

        var before = DateTime.UtcNow;

        // Act
        version.Publish(UpdatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            version.PublishedAtUtc!.Value,
            before,
            after);
    }

    /// <summary>
    /// Verifies that publishing fails when the publisher is empty.
    /// </summary>
    [Fact]
    public void Publish_ShouldRejectEmptyPublishedBy()
    {
        // Arrange
        var version = CreateVersion();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            version.Publish(
                string.Empty,
                UpdatedAtUtc));
    }

    /// <summary>
    /// Verifies that publishing an already published version
    /// updates its publication information.
    /// </summary>
    [Fact]
    public void Publish_ShouldUpdatePublicationInformation_WhenAlreadyPublished()
    {
        // Arrange
        var version = CreateVersion();

        version.Publish(
            CreatedBy,
            CreatedAtUtc);

        // Act
        version.Publish(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.True(version.IsPublishedSnapshot);
        Assert.True(version.IsCurrent);

        Assert.Equal(
            UpdatedAtUtc,
            version.PublishedAtUtc);

        Assert.Equal(
            UpdatedBy,
            version.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            version.UpdatedAtUtc);
    }

    #endregion

    #region Helpers

    private static OpportunityVersion CreateVersion()
    {
        return OpportunityVersion.CreateInitial(
            OpportunityId,
            CreateContent(),
            CreatedBy,
            CreatedAtUtc);
    }

    private static OpportunityVersionContent CreateContent(
        string productName = "Product")
    {
        return new OpportunityVersionContent
        {
            ProductName =
                new LocalizedText(
                    productName,
                    "المنتج"),

            NationalImpact =
                new LocalizedText(
                    "National impact",
                    "الأثر الوطني"),

            Description =
                new LocalizedText(
                    "Product description",
                    "وصف المنتج"),

            WebsiteUrl =
                new LocalizedText(
                    "https://example.com"),

            LogoReference =
                new LocalizedText(
                    "logo-reference"),

            BannerReference =
                new LocalizedText(
                    "banner-reference"),

            CompanyName =
                new LocalizedText(
                    "Company",
                    "الشركة"),

            CompanyWebsiteUrl =
                new LocalizedText(
                    "https://company.example.com"),

            AdoptedBy =
                new LocalizedText(
                    "Government Entity"),

            Beneficiaries =
                new LocalizedText(
                    "Citizens"),

            KsaAdoptingEntitiesCount = 5,

            ProductOwnerName =
                new LocalizedText(
                    "Product Owner"),

            ProductOwnerEmail =
                "owner@example.com",

            ProductOwnerPhone =
                "+966500000000",

            ChannelIds =
            [
                ChannelId
            ],

            SectorIds =
            [
                SectorId
            ],

            Features =
            [
                new FeatureContent
                {
                    Title =
                        new LocalizedText(
                            "Feature",
                            "الميزة"),

                    IconReference = 1,
                    SortOrder = 1,
                    DisplayOnWebsite = true
                }
            ],

            KeyAchievements =
            [
                new KeyAchievementContent
                {
                    IconReference = 2,

                    Title =
                        new LocalizedText(
                            "Achievement",
                            "الإنجاز"),

                    Description =
                        new LocalizedText(
                            "Achievement description"),

                    SortOrder = 1,
                    DisplayOnWebsite = true
                }
            ],

            Kpis =
            [
                new KpiContent
                {
                    Title =
                        new LocalizedText(
                            "KPI"),

                    Value =
                        new LocalizedText(
                            "100"),

                    SortOrder = 1
                }
            ]
        };
    }

    private static void AssertContent(
        OpportunityVersion version,
        OpportunityVersionContent content)
    {
        Assert.Equal(
            content.ProductName.En,
            version.ProductName.En);

        Assert.Equal(
            content.ProductName.Ar,
            version.ProductName.Ar);

        Assert.Equal(
            content.NationalImpact?.En,
            version.NationalImpact?.En);

        Assert.Equal(
            content.Description?.En,
            version.Description?.En);

        Assert.Equal(
            content.WebsiteUrl?.En,
            version.WebsiteUrl?.En);

        Assert.Equal(
            content.LogoReference?.En,
            version.LogoReference?.En);

        Assert.Equal(
            content.BannerReference?.En,
            version.BannerReference?.En);

        Assert.Equal(
            content.CompanyName?.En,
            version.CompanyName?.En);

        Assert.Equal(
            content.CompanyWebsiteUrl?.En,
            version.CompanyWebsiteUrl?.En);

        Assert.Equal(
            content.AdoptedBy?.En,
            version.AdoptedBy?.En);

        Assert.Equal(
            content.Beneficiaries?.En,
            version.Beneficiaries?.En);

        Assert.Equal(
            content.KsaAdoptingEntitiesCount,
            version.KsaAdoptingEntitiesCount);

        Assert.Equal(
            content.ProductOwnerName?.En,
            version.ProductOwnerName?.En);

        Assert.Equal(
            content.ProductOwnerEmail,
            version.ProductOwnerEmail);

        Assert.Equal(
            content.ProductOwnerPhone,
            version.ProductOwnerPhone);

        Assert.Equal(
            content.ChannelIds.Count,
            version.Channels.Count);

        Assert.Equal(
            content.SectorIds.Count,
            version.Sectors.Count);

        Assert.Equal(
            content.Features.Count,
            version.Features.Count);

        Assert.Equal(
            content.KeyAchievements.Count,
            version.KeyAchievements.Count);

        Assert.Equal(
            content.Kpis.Count,
            version.Kpis.Count);
    }

    #endregion
}