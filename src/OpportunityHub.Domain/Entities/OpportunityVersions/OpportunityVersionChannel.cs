namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents the association between an opportunity version
/// and a channel.
/// </summary>
public sealed class OpportunityVersionChannel : CreationTrackedObject
{
    private OpportunityVersionChannel(
        Guid channelId,
        string createdBy,
        DateTime createdAtUtc)
        : base(createdBy, createdAtUtc)
    {
        if (channelId == Guid.Empty)
        {
            throw new ArgumentException(
                "Channel ID is required.",
                nameof(channelId));
        }

        ChannelId = channelId;
    }

    #region Properties

    public Guid ChannelId { get; private set; }

    #endregion

    #region Factory

    internal static OpportunityVersionChannel Create(
        Guid channelId,
        string createdBy = "SYS",
        DateTime? createdAtUtc = null)
    {
        return new OpportunityVersionChannel(
            channelId,
            createdBy,
            createdAtUtc ?? DateTime.UtcNow);
    }

    #endregion
}