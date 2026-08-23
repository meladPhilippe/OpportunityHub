namespace OpportunityHub.Application.Opportunities.Models;

public sealed record FeatureRequest(
    LocalizedTextRequest? Title,
    int? IconReference,
    int SortOrder,
    bool DisplayOnWebsite);