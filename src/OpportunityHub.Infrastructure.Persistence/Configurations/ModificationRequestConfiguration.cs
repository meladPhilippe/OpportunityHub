using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Submissions;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class ModificationRequestConfiguration
    : IEntityTypeConfiguration<ModificationRequest>
{
    public void Configure(
        EntityTypeBuilder<ModificationRequest> builder)
    {
        builder.ToTable("ModificationRequest", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Creation Tracking

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        #endregion

        #region Submission Relationship

        builder.Property<Guid>("SubmissionId")
            .IsRequired();

        builder.HasOne<Submission>()
            .WithOne(x => x.ModificationRequest)
            .HasForeignKey<ModificationRequest>("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region Items

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("ModificationRequestId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}