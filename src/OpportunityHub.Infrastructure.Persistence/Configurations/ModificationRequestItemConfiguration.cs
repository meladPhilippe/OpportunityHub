using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class ModificationRequestItemConfiguration
    : IEntityTypeConfiguration<ModificationRequestItem>
{
    public void Configure(
        EntityTypeBuilder<ModificationRequestItem> builder)
    {
        builder.ToTable("ModificationRequestItem", "dbo");

        #region Key

        builder.HasKey(
            "ModificationRequestId",
            nameof(ModificationRequestItem.FieldName));

        #endregion

        #region Properties

        builder.Property<Guid>("ModificationRequestId")
            .IsRequired();

        builder.Property(x => x.FieldName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(4000)
            .IsRequired();

        #endregion

        #region Modification Request Relationship

        builder.HasOne<ModificationRequest>()
            .WithMany(x => x.Items)
            .HasForeignKey("ModificationRequestId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}