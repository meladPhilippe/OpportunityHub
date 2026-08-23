using MediatR;
using OpportunityHub.Application.Opportunities.Models;

namespace OpportunityHub.Application.Opportunities.Commands.RequestModification;

public sealed record RequestModificationCommand(
    Guid OpportunityId,
    IReadOnlyCollection<ModificationRequestItem> Items) : IRequest;