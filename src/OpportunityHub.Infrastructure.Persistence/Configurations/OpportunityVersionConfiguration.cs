using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class OpportunityVersionConfiguration
    : IEntityTypeConfiguration<OpportunityVersion>
{
    public void Configure(
        EntityTypeBuilder<OpportunityVersion> builder)
    {
        builder.ToTable("OpportunityVersion", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion
        
        #region Principal Key for Composite Relationships

        builder.HasAlternateKey(x => new
        {
            x.OpportunityId,
            x.Id
        })
        .HasName("AK_OpportunityVersion_OpportunityId_Id");

        #endregion

        #region Opportunity

        builder.Property(x => x.OpportunityId)
            .IsRequired();

        builder.HasOne<Opportunity>()
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region Version

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.IsCurrent)
            .IsRequired();

        builder.Property(x => x.IsPublishedSnapshot)
            .IsRequired();

        builder.Property(x => x.PublishedAtUtc)
            .IsRequired(false);

        #endregion

        #region Opportunity Name

        builder.OwnsOne(
            x => x.OpportunityName,
            name =>
            {
                name.Property(x => x.En)
                    .HasColumnName("OpportunityNameEn")
                    .HasMaxLength(1000)
                    .IsRequired();

                name.Property(x => x.Ar)
                    .HasColumnName("OpportunityNameAr")
                    .HasMaxLength(1000)
                    .IsRequired(false);
            });

        #endregion

        #region National Impact

        builder.OwnsOne(
            x => x.NationalImpact,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("NationalImpactEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("NationalImpactAr")
                    .IsRequired(false);
            });

        #endregion

        #region Description

        builder.OwnsOne(
            x => x.Description,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("DescriptionEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("DescriptionAr")
                    .IsRequired(false);
            });

        #endregion

        #region Website

        builder.OwnsOne(
            x => x.WebsiteUrl,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("WebsiteUrlEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("WebsiteUrlAr")
                    .IsRequired(false);
            });

        #endregion

        #region Logo

        builder.OwnsOne(
            x => x.LogoReference,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("LogoReferenceEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("LogoReferenceAr")
                    .IsRequired(false);
            });

        #endregion

        #region Banner

        builder.OwnsOne(
            x => x.BannerReference,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("BannerReferenceEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("BannerReferenceAr")
                    .IsRequired(false);
            });

        #endregion

        #region Company Name

        builder.OwnsOne(
            x => x.CompanyName,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("CompanyNameEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("CompanyNameAr")
                    .IsRequired(false);
            });

        #endregion

        #region Company Website

        builder.OwnsOne(
            x => x.CompanyWebsiteUrl,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("CompanyWebsiteUrlEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("CompanyWebsiteUrlAr")
                    .IsRequired(false);
            });

        #endregion

        #region Adopted By

        builder.OwnsOne(
            x => x.AdoptedBy,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("AdoptedByEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("AdoptedByAr")
                    .IsRequired(false);
            });

        #endregion

        #region Beneficiaries

        builder.OwnsOne(
            x => x.Beneficiaries,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("BeneficiariesEn")
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("BeneficiariesAr")
                    .IsRequired(false);
            });

        #endregion

        #region KSA Adopting Entities

        builder.Property(x => x.KsaAdoptingEntitiesCount)
            .IsRequired(false);

        #endregion

        #region Opportunity Owner Name

        builder.OwnsOne(
            x => x.OpportunityOwnerName,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("OpportunityOwnerNameEn")
                    .HasMaxLength(500)
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("OpportunityOwnerNameAr")
                    .HasMaxLength(500)
                    .IsRequired(false);
            });

        #endregion

        #region Opportunity Owner Contact

        builder.Property(x => x.OpportunityOwnerEmail)
            .HasMaxLength(320)
            .IsRequired(false);

        builder.Property(x => x.OpportunityOwnerPhone)
            .HasMaxLength(100)
            .IsRequired(false);

        #endregion

        #region Indexes

        // One version number per opportunity.
        builder.HasIndex(x => new
        {
            x.OpportunityId,
            x.VersionNumber
        })
        .IsUnique()
        .HasDatabaseName(
            "UQ_OpportunityVersion_Opportunity_VersionNumber");

        // Only one current version per opportunity.
        builder.HasIndex(x => x.OpportunityId)
            .IsUnique()
            .HasDatabaseName(
                "UX_OpportunityVersion_Current")
            .HasFilter("[IsCurrent] = 1");

        // Only one published snapshot per opportunity.
        builder.HasIndex(x => x.OpportunityId)
            .IsUnique()
            .HasDatabaseName(
                "UX_OpportunityVersion_Published")
            .HasFilter("[IsPublishedSnapshot] = 1");

        #endregion

        #region Change Tracking

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(256)
            .IsRequired(false);

        #endregion
    }
}