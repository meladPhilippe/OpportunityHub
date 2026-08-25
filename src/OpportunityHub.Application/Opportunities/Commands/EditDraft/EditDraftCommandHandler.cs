using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Application.Opportunities.Mappers;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.EditDraft;

public sealed class EditDraftCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<EditDraftCommand>
{
    public async Task Handle(
        EditDraftCommand request,
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

        opportunity.UpdateDraft(
            content,
            currentUser.UserId,
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
