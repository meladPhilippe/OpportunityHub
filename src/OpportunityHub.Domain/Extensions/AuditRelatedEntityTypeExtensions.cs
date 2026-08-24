using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Extensions;

public static class AuditRelatedEntityTypeExtensions
{
    public static string ToDatabaseValue(
        this AuditRelatedEntityType entityType)
    {
        return entityType switch
        {
            AuditRelatedEntityType.None =>
                nameof(AuditRelatedEntityType.None),

            AuditRelatedEntityType.ModificationRequest =>
                nameof(AuditRelatedEntityType.ModificationRequest),

            AuditRelatedEntityType.ModificationRejection =>
                nameof(AuditRelatedEntityType.ModificationRejection),

            AuditRelatedEntityType.FinalRejection =>
                nameof(AuditRelatedEntityType.FinalRejection),

            _ => throw new ArgumentOutOfRangeException(
                nameof(entityType),
                entityType,
                "Unknown audit related entity type.")
        };
    }
}
