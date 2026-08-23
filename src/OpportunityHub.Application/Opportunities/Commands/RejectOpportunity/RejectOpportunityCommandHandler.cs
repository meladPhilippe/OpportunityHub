using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.RejectOpportunity;

public sealed class RejectOpportunityCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<RejectOpportunityCommand>
{
    public async Task Handle(
        RejectOpportunityCommand request,
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

        opportunity.Reject(
            request.RejectionReasonId,
            request.Comment,
            currentUser.UserId,
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}