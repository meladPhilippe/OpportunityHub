using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Submissions;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class SubmissionConfiguration
    : IEntityTypeConfiguration<Submission>
{
    public void Configure(
        EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submission", "dbo");

        #region Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Shadow Properties

        builder.Property<Guid>("OpportunityId")
            .IsRequired();

        #endregion

        #region Properties

        builder.Property(x => x.OpportunityVersionId)
            .IsRequired();

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.SubmissionType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.EditSummary)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.PreviousStatusCode)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PreviousSubStatusCode)
            .HasConversion<int?>()
            .IsRequired(false);

        builder.Property(x => x.SubmittedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.SubmittedAtUtc)
            .IsRequired();

        #endregion

        #region Opportunity Relationship

        builder.HasOne<Opportunity>()
            .WithMany(x => x.Submissions)
            .HasForeignKey("OpportunityId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region Opportunity Version Relationship

        builder.HasOne<OpportunityVersion>()
            .WithMany()
            .HasForeignKey(
                "OpportunityId",
                nameof(Submission.OpportunityVersionId))
            .HasPrincipalKey(
                nameof(OpportunityVersion.OpportunityId),
                nameof(OpportunityVersion.Id))
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region Indexes

        builder.HasIndex(
            nameof(Submission.OpportunityVersionId),
            nameof(Submission.SequenceNumber))
            .IsUnique()
            .HasDatabaseName(
                "UQ_Submission_Version_Sequence");

        #endregion
    }
}