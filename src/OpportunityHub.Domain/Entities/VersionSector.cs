using OpportunityHub.Domain;

public sealed class VersionSector : CreationTrackedEntity
{
    public Guid VersionId { get; private set; }

    public Guid SectorId { get; private set; }

    private VersionSector()
    {
    }
    internal VersionSector(Guid versionId, Guid sectorId, string createdBy, DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
        VersionId = versionId;
        SectorId = sectorId;
    }
}