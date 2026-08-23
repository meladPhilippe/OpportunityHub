using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.RequestModification;

public sealed class RequestModificationCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<RequestModificationCommand>
{
    public async Task Handle(
        RequestModificationCommand request,
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

        var items = request.Items
            .Select(x => (
                FieldName: x.FieldName,
                Comment: x.Comment));

        opportunity.RequestModification(
            items,
            currentUser.UserId,
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}