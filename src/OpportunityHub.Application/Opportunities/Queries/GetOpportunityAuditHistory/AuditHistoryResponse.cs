using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Opportunities.Queries.GetOpportunityAuditHistory;

public sealed record AuditHistoryResponse(
    Guid OpportunityId,
    Guid OpportunityVersionId,
    Guid? SubmissionId,
    long ActivitySequenceNumber,
    WorkflowActivityType ActivityType,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    string CreatedBy,
    DateTime CreatedAtUtc);
