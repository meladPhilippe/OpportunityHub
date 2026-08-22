using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class RejectionReasonConfiguration
    : IEntityTypeConfiguration<RejectionReason>
{
    public void Configure(
        EntityTypeBuilder<RejectionReason> builder)
    {
        builder.ToTable("RejectionReason", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Properties

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        #endregion

        #region Indexes 

        builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasDatabaseName("UQ_RejectionReason_Code");

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
                .HasDatabaseName("IX_RejectionReason_IsActive_DisplayOrder");
        #endregion
    }
}