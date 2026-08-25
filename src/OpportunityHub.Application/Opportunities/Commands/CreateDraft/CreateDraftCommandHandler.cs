using MediatR;
using OpportunityHub.Application.Abstractions.Identity;
using OpportunityHub.Application.Opportunities.Mappers;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Commands.CreateDraft;

public sealed class CreateDraftCommandHandler(
    IOpportunityRepository opportunityRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateDraftCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateDraftCommand request,
        CancellationToken cancellationToken)
    {
        var content =
            OpportunityVersionContentMapper.ToDomain(
                request.Content);

        var opportunityId = Guid.NewGuid();

        var opportunity =
            Opportunity.CreateDraft(
                opportunityId,
                content,
                currentUser.UserId,
                DateTime.UtcNow);

        opportunityRepository.Add(opportunity);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return opportunity.Id;
    }
}
