using FluentAssertions;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Infrastructure.Persistence.Repositories;
using OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Repositories;

[Collection("Infrastructure Integration Tests")]
public sealed class OpportunityRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public OpportunityRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_returns_persisted_opportunity()
    {
        var opportunityId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            CreateContent(),
            "integration-test",
            createdAtUtc);

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new OpportunityRepository(db);

        var result = await repository.GetByIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be(opportunityId);
        result.StatusCode.Should().Be(
            OpportunityStatusCode.Draft);

        result.SubStatusCode.Should().BeNull();
        result.IsActive.Should().BeTrue();

        result.LastSubmissionSequenceNumber.Should().Be(0);
        result.LastActivitySequenceNumber.Should().Be(0);

        result.CreatedBy.Should().Be("integration-test");

        result.Versions.Should().ContainSingle();

        var version = result.Versions.Single();

        version.VersionNumber.Should().Be(1);
        version.IsCurrent.Should().BeTrue();
        version.IsPublishedSnapshot.Should().BeFalse();

        version.OpportunityName.En.Should().Be(
            "Test Opportunity");

        version.OpportunityName.Ar.Should().Be(
            "فرصة اختبار");

        version.Channels.Should().HaveCount(2);
        version.Channels
            .Select(x => x.ChannelId)
            .Should()
            .Contain(
                new[]
                {
                    new Guid("11111111-1111-1111-1111-111111111111"),
                    new Guid("22222222-2222-2222-2222-222222222222")
                });

        version.Sectors.Should().HaveCount(2);
        version.Sectors
            .Select(x => x.SectorId)
            .Should()
            .Contain(
                new[]
                {
                    new Guid("33333333-3333-3333-3333-333333333333"),
                    new Guid("44444444-4444-4444-4444-444444444444")
                });

        version.Features.Should().ContainSingle();

        var feature = version.Features.Single();

        feature.Title!.En.Should().Be("Feature 1");
        feature.Title.Ar.Should().Be("الميزة 1");
        feature.IconReference.Should().Be(10);
        feature.SortOrder.Should().Be(1);
        feature.DisplayOnWebsite.Should().BeTrue();

        version.KeyAchievements.Should().ContainSingle();

        var achievement = version.KeyAchievements.Single();

        achievement.Title!.En.Should().Be(
            "Achievement 1");

        achievement.Title.Ar.Should().Be(
            "الإنجاز 1");

        achievement.Description!.En.Should().Be(
            "Achievement description");

        achievement.IconReference.Should().Be(20);
        achievement.SortOrder.Should().Be(1);
        achievement.DisplayOnWebsite.Should().BeTrue();

        version.Kpis.Should().ContainSingle();

        var kpi = version.Kpis.Single();

        kpi.Title!.En.Should().Be("Revenue");
        kpi.Title.Ar.Should().Be("الإيرادات");

        kpi.Value!.En.Should().Be("$1M");
        kpi.Value.Ar.Should().Be("1 مليون دولار");

        kpi.SortOrder.Should().Be(1);

        result.Submissions.Should().BeEmpty();
        result.AuditHistories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_unknown_opportunity()
    {
        await using var db = _fixture.CreateDbContext();

        var repository = new OpportunityRepository(db);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_preserves_opportunity_workflow_state()
    {
        var opportunityId = Guid.NewGuid();

        await using var db = _fixture.CreateDbContext();

        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            new OpportunityVersionContent
            {
                OpportunityName = new LocalizedText(
                    "Workflow Test Opportunity")
            },
            "integration-test");

        db.Opportunities.Add(opportunity);

        await db.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var repository = new OpportunityRepository(db);

        var result = await repository.GetByIdAsync(
            opportunityId,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(
            OpportunityStatusCode.Draft);

        result.IsDraft.Should().BeTrue();
        result.IsPublished.Should().BeFalse();
        result.IsApproved.Should().BeFalse();
        result.IsRejected.Should().BeFalse();
        result.IsUnderReview.Should().BeFalse();
    }

    private static OpportunityVersionContent CreateContent()
    {
        return new OpportunityVersionContent
        {
            OpportunityName = new LocalizedText(
                "Test Opportunity",
                "فرصة اختبار"),

            NationalImpact = new LocalizedText(
                "National impact",
                "الأثر الوطني"),

            Description = new LocalizedText(
                "Opportunity description",
                "وصف الفرصة"),

            WebsiteUrl = new LocalizedText(
                "https://example.com",
                "https://example.com/ar"),

            CompanyName = new LocalizedText(
                "Test Company",
                "شركة الاختبار"),

            CompanyWebsiteUrl = new LocalizedText(
                "https://company.example.com"),

            AdoptedBy = new LocalizedText(
                "Test Entity",
                "الجهة المختبرة"),

            Beneficiaries = new LocalizedText(
                "Citizens",
                "المواطنون"),

            KsaAdoptingEntitiesCount = 5,

            OpportunityOwnerName = new LocalizedText(
                "Test Owner",
                "مالك الاختبار"),

            OpportunityOwnerEmail =
                "owner@example.com",

            OpportunityOwnerPhone =
                "+966500000000",

            ChannelIds =
            [
                new Guid("11111111-1111-1111-1111-111111111111"),
                new Guid("22222222-2222-2222-2222-222222222222")
            ],

            SectorIds =
            [
                new Guid("33333333-3333-3333-3333-333333333333"),
                new Guid("44444444-4444-4444-4444-444444444444")
            ],

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
                }
            ],

            KeyAchievements =
            [
                new KeyAchievementContent
                {
                    IconReference = 20,

                    Title = new LocalizedText(
                        "Achievement 1",
                        "الإنجاز 1"),

                    Description = new LocalizedText(
                        "Achievement description",
                        "وصف الإنجاز"),

                    SortOrder = 1,
                    DisplayOnWebsite = true
                }
            ],

            Kpis =
            [
                new KpiContent
                {
                    Title = new LocalizedText(
                        "Revenue",
                        "الإيرادات"),

                    Value = new LocalizedText(
                        "$1M",
                        "1 مليون دولار"),

                    SortOrder = 1
                }
            ]
        };
    }
}
