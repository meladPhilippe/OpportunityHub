using MediatR;

namespace OpportunityHub.Application.Opportunities.Commands.RejectModification;

public sealed record RejectModificationCommand(
    Guid OpportunityId,
    string Comment) : IRequest;