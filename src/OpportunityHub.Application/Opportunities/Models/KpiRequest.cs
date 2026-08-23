namespace OpportunityHub.Application.Opportunities.Models;

public sealed record KpiRequest(
    LocalizedTextRequest? Title,
    LocalizedTextRequest? Value,
    int SortOrder);