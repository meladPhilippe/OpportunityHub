using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class OpportunityConfiguration
    : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(
        EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("Opportunity", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Workflow State

        builder.Property(x => x.StatusCode)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.SubStatusCode)
            .HasConversion<int?>()
            .IsRequired(false);

        #endregion

        #region Properties

        builder.Property(x => x.QrCodeReference)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.PublishedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.LastSubmissionSequenceNumber)
            .IsRequired();

        builder.Property(x => x.LastActivitySequenceNumber)
            .IsRequired();

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

        #region Indexes

        builder.HasIndex(x => x.StatusCode)
            .HasDatabaseName("IX_Opportunity_StatusCode");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Opportunity_IsActive");

        #endregion
    }
}