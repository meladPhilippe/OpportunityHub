namespace OpportunityHub.Application.Opportunities.Models;

public sealed record ModificationRequestItem(
    string FieldName,
    string Comment);