using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Entities.Submissions;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class AuditHistoryConfiguration
    : IEntityTypeConfiguration<AuditHistory>
{
    public void Configure(
        EntityTypeBuilder<AuditHistory> builder)
    {
        builder.ToTable("AuditHistory", "dbo");

        #region Key

        builder.HasKey(
            x => new
            {
                x.OpportunityId,
                x.ActivitySequenceNumber
            })
            .HasName("PK_AuditHistory");

        #endregion

        #region Properties

        builder.Property(x => x.OpportunityId)
            .IsRequired();

        builder.Property(x => x.ActivitySequenceNumber)
            .IsRequired();

        builder.Property(x => x.OpportunityVersionId)
            .IsRequired();

        builder.Property(x => x.SubmissionId)
            .IsRequired(false);

        builder.Property(x => x.ActivityType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RelatedEntityType)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.RelatedEntityId)
            .IsRequired(false);

        #endregion

        #region Creation Tracking

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        #endregion

        #region Opportunity Relationship

        builder.HasOne<Opportunity>()
            .WithMany(x => x.AuditHistories)
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region Opportunity Version Relationship

        builder.HasOne<OpportunityVersion>()
            .WithMany()
            .HasForeignKey(
                x => new
                {
                    x.OpportunityId,
                    x.OpportunityVersionId
                })
            .HasPrincipalKey(
                x => new
                {
                    x.OpportunityId,
                    x.Id
                })
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region Submission Relationship

        builder.HasOne<Submission>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region Constraints

        builder.ToTable(
            "AuditHistory",
            "dbo",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_AuditHistory_ActivitySequenceNumber",
                    "[ActivitySequenceNumber] > 0");
            });

        #endregion
    }
}