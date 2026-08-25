using MediatR;
using OpportunityHub.Application.Opportunities.Models;

namespace OpportunityHub.Application.Opportunities.Commands.EditDraft;

public sealed record EditDraftCommand(
    Guid OpportunityId,
    OpportunityVersionContentRequest Content) : IRequest;
