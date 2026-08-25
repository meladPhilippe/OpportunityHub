using OpportunityHub.Application.Opportunities.Commands.EditDraft;
using OpportunityHub.Application.Opportunities.Models;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Application.Tests.TestData;
using OpportunityHub.Domain.Exceptions;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.EditDraft;

public sealed class EditDraftCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpportunityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("editor-user");

        var handler =
            new EditDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var opportunityId =
            Guid.NewGuid();

        var command =
            new EditDraftCommand(
                opportunityId,
                CreateContentRequest());

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
    public async Task Handle_WhenOpportunityIsDraft_UpdatesContentAndSaves()
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
            new FakeCurrentUser("editor-user");

        var handler =
            new EditDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new EditDraftCommand(
                opportunity.Id,
                CreateContentRequest(
                    "Updated Opportunity",
                    "فرصة محدثة",
                    "Updated description",
                    "وصف محدث"));

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        var version =
            opportunity.GetCurrentVersion();

        Assert.Equal(
            "Updated Opportunity",
            version.OpportunityName.En);

        Assert.Equal(
            "فرصة محدثة",
            version.OpportunityName.Ar);

        Assert.Equal(
            "Updated description",
            version.Description!.En);

        Assert.Equal(
            "وصف محدث",
            version.Description.Ar);

        Assert.Equal(
            OpportunityStatusCode.Draft,
            opportunity.StatusCode);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
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

        var currentUser =
            new FakeCurrentUser("editor-user");

        var handler =
            new EditDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new EditDraftCommand(
                opportunity.Id,
                CreateContentRequest());

        // Act
        var exception =
            await Assert.ThrowsAsync<WorkflowDomainException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

        // Assert
        Assert.Equal(
            "Only a draft opportunity can be edited.",
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DoesNotCreateSubmissionOrAuditHistory()
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
            new FakeCurrentUser("editor-user");

        var handler =
            new EditDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new EditDraftCommand(
                opportunity.Id,
                CreateContentRequest());

        var submissionCountBefore =
            opportunity.Submissions.Count;

        var auditHistoryCountBefore =
            opportunity.AuditHistories.Count;

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            submissionCountBefore,
            opportunity.Submissions.Count);

        Assert.Equal(
            auditHistoryCountBefore,
            opportunity.AuditHistories.Count);
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

        var currentUser =
            new FakeCurrentUser("editor-user");

        var handler =
            new EditDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new EditDraftCommand(
                opportunity.Id,
                CreateContentRequest());

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

    private static OpportunityVersionContentRequest CreateContentRequest(
        string opportunityNameEn = "Test Opportunity",
        string opportunityNameAr = "فرصة اختبار",
        string descriptionEn = "Test description",
        string descriptionAr = "وصف الاختبار")
    {
        return new OpportunityVersionContentRequest
        {
            OpportunityName =
                new LocalizedTextRequest(
                    opportunityNameEn,
                    opportunityNameAr),

            Description =
                new LocalizedTextRequest(
                    descriptionEn,
                    descriptionAr)
        };
    }
}
