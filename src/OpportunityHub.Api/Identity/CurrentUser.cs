using System.Security.Claims;
using OpportunityHub.Application.Abstractions.Identity;

namespace OpportunityHub.Api.Identity;

public sealed class CurrentUser(
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string UserId =>
        httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException(
            "The current user is not authenticated.");
}