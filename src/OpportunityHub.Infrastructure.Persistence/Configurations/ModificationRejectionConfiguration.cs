using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Submissions;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class ModificationRejectionConfiguration
    : IEntityTypeConfiguration<ModificationRejection>
{
    public void Configure(
        EntityTypeBuilder<ModificationRejection> builder)
    {
        builder.ToTable("ModificationRejection", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Properties

        builder.Property(x => x.Comment)
            .HasMaxLength(4000)
            .IsRequired();

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
            .WithOne(x => x.ModificationRejection)
            .HasForeignKey<ModificationRejection>("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}