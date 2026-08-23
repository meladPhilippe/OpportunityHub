using MediatR;
using OpportunityHub.Application.Opportunities.Models;

namespace OpportunityHub.Application.Opportunities.Commands.SubmitForManagerReview;

public sealed record SubmitForManagerReviewCommand(
    Guid OpportunityId,
    OpportunityVersionContentRequest Content,
    string? EditSummary = null) : IRequest<Guid>;   