using MediatR;

namespace OpportunityHub.Application.Opportunities.Commands.PublishOpportunity;

public sealed record PublishOpportunityCommand(
    Guid OpportunityId) : IRequest;