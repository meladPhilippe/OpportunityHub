using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Application.Opportunities.Mappers;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.SubmitForManagerReview;

public sealed class SubmitForManagerReviewCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<SubmitForManagerReviewCommand, Guid>
{
    public async Task<Guid> Handle(
        SubmitForManagerReviewCommand request,
        CancellationToken cancellationToken)
    {
        var opportunity =
            await opportunityRepository.GetByIdAsync(
                request.OpportunityId,
                cancellationToken);

        if (opportunity is null)
        {
            throw new InvalidOperationException(
                $"Opportunity '{request.OpportunityId}' was not found.");
        }

        var content =
            OpportunityVersionContentMapper.ToDomain(
                request.Content);

        var submission =
            opportunity.SubmitForManagerReview(
                content,
                currentUser.UserId,
                request.EditSummary,
                DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return submission.Id;
    }
}