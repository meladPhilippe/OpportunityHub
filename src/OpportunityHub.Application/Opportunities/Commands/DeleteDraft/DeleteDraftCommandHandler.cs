using MediatR;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Application.Opportunities.Commands.DeleteDraft;

public sealed class DeleteDraftCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDraftCommand>
{
    public async Task Handle(
        DeleteDraftCommand request,
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

        if (!opportunity.IsDraft)
        {
            throw new WorkflowDomainException(
                "Only a draft opportunity can be deleted.");
        }

        opportunityRepository.Delete(opportunity);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
