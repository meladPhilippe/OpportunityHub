using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.ApproveOpportunity;

public sealed class ApproveOpportunityCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<ApproveOpportunityCommand>
{
    public async Task Handle(
        ApproveOpportunityCommand request,
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

        opportunity.Approve(
            currentUser.UserId,
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}