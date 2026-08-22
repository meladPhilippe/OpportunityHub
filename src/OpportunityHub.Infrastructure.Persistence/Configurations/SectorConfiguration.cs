using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class SectorConfiguration
    : IEntityTypeConfiguration<Sector>
{
    public void Configure(
        EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sector", "dbo");

        #region Key

        builder.HasKey(x => x.Id)
            .HasName("PK_Sector");

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Properties

        builder.Property(x => x.Code)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        #endregion

        #region Name

        builder.OwnsOne(
            x => x.Name,
            name =>
            {
                name.Property(x => x.En)
                    .HasColumnName("NameEn")
                    .HasMaxLength(1000)
                    .IsRequired();

                name.Property(x => x.Ar)
                    .HasColumnName("NameAr")
                    .HasMaxLength(1000)
                    .IsRequired(false);
            });

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

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_Sector_Code");

        builder.HasIndex(
            x => new
            {
                x.IsActive,
                x.SortOrder
            })
            .HasDatabaseName(
                "IX_Sector_IsActive_SortOrder");

        #endregion
    }
}