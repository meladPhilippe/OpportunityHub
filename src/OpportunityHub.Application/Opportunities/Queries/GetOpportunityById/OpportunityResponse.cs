using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Opportunities.Queries.GetOpportunityById;

public sealed record OpportunityResponse(
    Guid Id,
    OpportunityStatusCode StatusCode,
    OpportunitySubStatusCode? SubStatusCode,
    string? QrCodeReference,
    DateTime? PublishedAtUtc,
    bool IsActive);