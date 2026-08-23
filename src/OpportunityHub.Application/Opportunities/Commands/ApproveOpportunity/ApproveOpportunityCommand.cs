using MediatR;

namespace OpportunityHub.Application.Opportunities.Commands.ApproveOpportunity;

public sealed record ApproveOpportunityCommand(
    Guid OpportunityId) : IRequest;