using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.PublishOpportunity;

public sealed class PublishOpportunityCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<PublishOpportunityCommand>
{
    public async Task Handle(
        PublishOpportunityCommand request,
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

        opportunity.Publish(
            currentUser.UserId,
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}