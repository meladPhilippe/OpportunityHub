using MediatR;
using OpportunityHub.Application.Opportunities.Models;

namespace OpportunityHub.Application.Opportunities.Commands.CreateDraft;

public sealed record CreateDraftCommand(
    OpportunityVersionContentRequest Content) : IRequest<Guid>;
