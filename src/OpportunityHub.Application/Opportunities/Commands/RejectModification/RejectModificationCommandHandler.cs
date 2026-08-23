using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.RejectModification;

public sealed class RejectModificationCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<RejectModificationCommand>
{
    public async Task Handle(
        RejectModificationCommand request,
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

        opportunity.RejectModification(
            request.Comment,
            currentUser.UserId,
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}