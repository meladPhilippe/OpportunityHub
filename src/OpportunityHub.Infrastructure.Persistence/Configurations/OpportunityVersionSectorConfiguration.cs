using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class OpportunityVersionSectorConfiguration
    : IEntityTypeConfiguration<OpportunityVersionSector>
{
    public void Configure(
        EntityTypeBuilder<OpportunityVersionSector> builder)
    {
        builder.ToTable("OpportunityVersionSector", "dbo");

        #region Key

        builder.HasKey(
            "OpportunityVersionId",
            nameof(OpportunityVersionSector.SectorId));

        #endregion

        #region Shadow Foreign Key

        builder.Property<Guid>("OpportunityVersionId")
            .IsRequired();

        #endregion

        #region Properties

        builder.Property(x => x.SectorId)
            .IsRequired();

        #endregion

        #region Relationships

        builder.HasOne<OpportunityVersion>()
            .WithMany(x => x.Sectors)
            .HasForeignKey("OpportunityVersionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Sector>()
            .WithMany()
            .HasForeignKey(x => x.SectorId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region Creation Tracking

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        #endregion
    }
}