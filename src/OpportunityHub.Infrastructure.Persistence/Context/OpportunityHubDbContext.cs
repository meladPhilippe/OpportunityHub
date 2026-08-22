using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Entities.Submissions;

namespace OpportunityHub.Infrastructure.Persistence;

public sealed class OpportunityHubDbContext : DbContext
{
    public OpportunityHubDbContext(
        DbContextOptions<OpportunityHubDbContext> options)
        : base(options)
    {
    }

    #region Aggregate Roots

    public DbSet<Opportunity> Opportunities =>
        Set<Opportunity>();

    #endregion

    #region Reference Data

    public DbSet<Channel> Channels =>
        Set<Channel>();

    public DbSet<Sector> Sectors =>
        Set<Sector>();

    public DbSet<RejectionReason> RejectionReasons =>
        Set<RejectionReason>();

    #endregion

    #region On Model Creating

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OpportunityHubDbContext).Assembly);
    }

    #endregion
}