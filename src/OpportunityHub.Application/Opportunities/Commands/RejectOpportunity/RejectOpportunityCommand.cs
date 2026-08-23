using MediatR;

namespace OpportunityHub.Application.Opportunities.Commands.RejectOpportunity;

public sealed record RejectOpportunityCommand(
    Guid OpportunityId,
    int RejectionReasonId,
    string Comment) : IRequest;