using MediatR;

namespace OpportunityHub.Application.Opportunities.Commands.DeleteDraft;

public sealed record DeleteDraftCommand(
    Guid OpportunityId) : IRequest;
