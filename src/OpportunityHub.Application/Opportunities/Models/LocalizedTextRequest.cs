namespace OpportunityHub.Application.Opportunities.Models;

public sealed record LocalizedTextRequest(
    string En,
    string? Ar = null);