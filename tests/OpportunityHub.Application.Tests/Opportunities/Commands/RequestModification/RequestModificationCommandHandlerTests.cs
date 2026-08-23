using OpportunityHub.Application.Opportunities.Commands.RequestModification;
using OpportunityHub.Application.Opportunities.Models;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.RequestModification;

public sealed class RequestModificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeOpportunityRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser("manager-user");

        var handler = new RequestModificationCommandHandler(
            repository,
            unitOfWork,
            currentUser);

        var opportunityId = Guid.NewGuid();

        var command = new RequestModificationCommand(
            opportunityId,
            CreateItems());

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
    }

    [Fact]
    public async Task Handle_WhenOpportunityIsPendingManagerReview_RequestsModificationAndSaves()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        var repository = new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork = new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("manager-user");

        var handler =
            new RequestModificationCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new RequestModificationCommand(
                opportunity.Id,
                CreateItems());

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PendingSpecialistModification,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenWorkflowActionIsNotAllowed_ThrowsDomainException()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreateDraft();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("manager-user");

        var handler =
            new RequestModificationCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new RequestModificationCommand(
                opportunity.Id,
                CreateItems());

        // Act & Assert
        await Assert.ThrowsAsync<WorkflowTransitionNotAllowedException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var opportunity =
            OpportunityFactory.CreatePendingManagerReview();

        var repository =
            new FakeOpportunityRepository();

        repository.Add(opportunity);

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("manager-user");

        var handler =
            new RequestModificationCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new RequestModificationCommand(
                opportunity.Id,
                CreateItems());

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act
        await handler.Handle(
            command,
            cancellationTokenSource.Token);

        // Assert
        Assert.Equal(
            cancellationTokenSource.Token,
            repository.LastCancellationToken);
    }

    private static IReadOnlyCollection<ModificationRequestItem>
        CreateItems()
    {
        return
        [
            new ModificationRequestItem(
                "Description",
                "Please provide more details."),

            new ModificationRequestItem(
                "OpportunityName",
                "Please update the opportunity name.")
        ];
    }
}