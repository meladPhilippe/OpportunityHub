using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class OpportunityVersionFeatureConfiguration
    : IEntityTypeConfiguration<OpportunityVersionFeature>
{
    public void Configure(
        EntityTypeBuilder<OpportunityVersionFeature> builder)
    {
        builder.ToTable("OpportunityVersionFeature", "dbo");

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

        builder.Property(x => x.IconReference)
            .IsRequired(false);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.DisplayOnWebsite)
            .IsRequired();

        #endregion

        #region Localized Text

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

        #region Relationship

        builder.HasOne<OpportunityVersion>()
            .WithMany(x => x.Features)
            .HasForeignKey("OpportunityVersionId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}