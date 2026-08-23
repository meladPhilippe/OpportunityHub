namespace OpportunityHub.Application.Opportunities.Models;

public sealed record KeyAchievementRequest(
    int? IconReference,
    LocalizedTextRequest? Title,
    LocalizedTextRequest? Description,
    int SortOrder,
    bool DisplayOnWebsite);