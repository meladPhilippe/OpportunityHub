namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents the association between an opportunity version
/// and a sector.
/// </summary>
public sealed class OpportunityVersionSector : CreationTrackedObject
{
    private OpportunityVersionSector(
        Guid sectorId,
        string createdBy,
        DateTime createdAtUtc)
        : base(createdBy, createdAtUtc)
    {
        if (sectorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Sector ID is required.",
                nameof(sectorId));
        }

        SectorId = sectorId;
    }

    private OpportunityVersionSector()
    {
        
    }

    #region Properties

    public Guid SectorId { get; private set; }

    #endregion

    #region Factory

    internal static OpportunityVersionSector Create(
        Guid sectorId,
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
    {
        return new OpportunityVersionSector(
            sectorId,
            createdBy,
            createdAtUtc ?? DateTime.UtcNow);
    }

    #endregion
}