using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class OpportunityVersionKpiConfiguration
    : IEntityTypeConfiguration<OpportunityVersionKpi>
{
    public void Configure(
        EntityTypeBuilder<OpportunityVersionKpi> builder)
    {
        builder.ToTable("OpportunityVersionKpi", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        #endregion

        #region Identity

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Shadow Foreign Key

        builder.Property<Guid>("OpportunityVersionId")
            .IsRequired();

        #endregion

        #region Properties

        builder.Property(x => x.SortOrder)
            .IsRequired();

        #endregion

        #region Localized Text - Title

        builder.OwnsOne(
            x => x.Title,
            title =>
            {
                title.Property(x => x.En)
                    .HasColumnName("TitleEn")
                    .HasMaxLength(1000)
                    .IsRequired(false);

                title.Property(x => x.Ar)
                    .HasColumnName("TitleAr")
                    .HasMaxLength(1000)
                    .IsRequired(false);
            });

        #endregion

        #region Localized Text - Value

        builder.OwnsOne(
            x => x.Value,
            value =>
            {
                value.Property(x => x.En)
                    .HasColumnName("ValueEn")
                    .HasMaxLength(1000)
                    .IsRequired(false);

                value.Property(x => x.Ar)
                    .HasColumnName("ValueAr")
                    .HasMaxLength(1000)
                    .IsRequired(false);
            });

        #endregion

        #region Relationship

        builder.HasOne<OpportunityVersion>()
            .WithMany(x => x.Kpis)
            .HasForeignKey("OpportunityVersionId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}