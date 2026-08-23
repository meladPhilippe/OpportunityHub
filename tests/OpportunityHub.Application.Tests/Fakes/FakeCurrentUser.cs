using OpportunityHub.Application.Abstractions.Identity;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(string userId)
    {
        UserId = userId;
    }

    public string UserId { get; }
}