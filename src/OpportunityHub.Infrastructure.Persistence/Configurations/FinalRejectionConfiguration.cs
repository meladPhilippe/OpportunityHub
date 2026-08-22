using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Submissions;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class FinalRejectionConfiguration
    : IEntityTypeConfiguration<FinalRejection>
{
    public void Configure(
        EntityTypeBuilder<FinalRejection> builder)
    {
        builder.ToTable("FinalRejection", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Properties

        builder.Property(x => x.RejectionReasonId)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(4000)
            .IsRequired();

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
            .WithOne(x => x.FinalRejection)
            .HasForeignKey<FinalRejection>("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region Rejection Reason Relationship

        builder.HasOne<RejectionReason>()
            .WithMany()
            .HasForeignKey(x => x.RejectionReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}