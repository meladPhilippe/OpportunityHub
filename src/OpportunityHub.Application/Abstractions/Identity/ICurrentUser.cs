namespace OpportunityHub.Application.Abstractions.Identity;

public interface ICurrentUser
{
    string UserId { get; }
}