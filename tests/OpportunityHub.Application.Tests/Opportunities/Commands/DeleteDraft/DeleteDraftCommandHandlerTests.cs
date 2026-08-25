using OpportunityHub.Application.Opportunities.Commands.DeleteDraft;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Exceptions;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.DeleteDraft;

public sealed class DeleteDraftCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            new DeleteDraftCommandHandler(
                repository,
                unitOfWork);

        var opportunityId =
            Guid.NewGuid();

        var command =
            new DeleteDraftCommand(
                opportunityId);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

        // Assert
        Assert.Equal(
            $"Opportunity '{opportunityId}' was not found.",
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);

        Assert.Null(
            repository.DeletedOpportunity);
    }

    [Fact]
    public async Task Handle_WhenOpportunityIsDraft_DeletesOpportunityAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateDraft();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            new DeleteDraftCommandHandler(
                repository,
                unitOfWork);

        var command =
            new DeleteDraftCommand(
                opportunity.Id);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Same(
            opportunity,
            repository.DeletedOpportunity);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);

        var deletedOpportunity =
            await repository.GetByIdAsync(
                opportunity.Id,
                CancellationToken.None);

        Assert.Null(
            deletedOpportunity);
    }

    [Fact]
    public async Task Handle_WhenOpportunityIsNotDraft_ThrowsWorkflowDomainException()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            new DeleteDraftCommandHandler(
                repository,
                unitOfWork);

        var command =
            new DeleteDraftCommand(
                opportunity.Id);

        // Act
        var exception =
            await Assert.ThrowsAsync<WorkflowDomainException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

        // Assert
        Assert.Equal(
            "Only a draft opportunity can be deleted.",
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);

        Assert.Null(
            repository.DeletedOpportunity);
    }

    [Fact]
    public async Task Handle_WhenOpportunityIsPublished_DoesNotDeleteOpportunity()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePublished();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            new DeleteDraftCommandHandler(
                repository,
                unitOfWork);

        var command =
            new DeleteDraftCommand(
                opportunity.Id);

        // Act
        var exception =
            await Assert.ThrowsAsync<WorkflowDomainException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

        // Assert
        Assert.Equal(
            "Only a draft opportunity can be deleted.",
            exception.Message);

        Assert.Null(
            repository.DeletedOpportunity);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepositoryAndUnitOfWork()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateDraft();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            new DeleteDraftCommandHandler(
                repository,
                unitOfWork);

        var command =
            new DeleteDraftCommand(
                opportunity.Id);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        // Act
        await handler.Handle(
            command,
            cancellationToken);

        // Assert
        Assert.Equal(
            cancellationToken,
            repository.LastCancellationToken);

        Assert.Equal(
            cancellationToken,
            unitOfWork.LastCancellationToken);
    }
}
