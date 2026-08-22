using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Infrastructure.Persistence.Configurations;

public sealed class OpportunityVersionChannelConfiguration
    : IEntityTypeConfiguration<OpportunityVersionChannel>
{
    public void Configure(
        EntityTypeBuilder<OpportunityVersionChannel> builder)
    {
        builder.ToTable("OpportunityVersionChannel", "dbo");

        #region Key

        builder.HasKey(
            "OpportunityVersionId",
            nameof(OpportunityVersionChannel.ChannelId));

        #endregion

        #region Shadow Foreign Key

        builder.Property<Guid>("OpportunityVersionId")
            .IsRequired();

        #endregion

        #region Properties

        builder.Property(x => x.ChannelId)
            .IsRequired();

        #endregion

        #region Relationships

        builder.HasOne<OpportunityVersion>()
            .WithMany(x => x.Channels)
            .HasForeignKey("OpportunityVersionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Channel>()
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
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