using OpportunityHub.Domain;

public sealed class VersionChannel : CreationTrackedEntity
{
    public Guid VersionId { get; private set; }

    public Guid ChannelId { get; private set; }

    private VersionChannel()
    {
    }

    internal VersionChannel(Guid versionId, Guid channelId, string createdBy, DateTime? createdAtUtc = null)
        : base(createdBy, createdAtUtc)
    {
        VersionId = versionId;
        ChannelId = channelId;
    }
}