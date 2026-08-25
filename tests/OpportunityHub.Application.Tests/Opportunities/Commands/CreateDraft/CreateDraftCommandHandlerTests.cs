using OpportunityHub.Application.Opportunities.Commands.CreateDraft;
using OpportunityHub.Application.Opportunities.Models;
using OpportunityHub.Application.Tests.Fakes;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Tests.Opportunities.Commands.CreateDraft;

public sealed class CreateDraftCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesDraftOpportunityAndSaves()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("creator-user");

        var handler =
            new CreateDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new CreateDraftCommand(
                CreateContentRequest(
                    "New Opportunity",
                    "فرصة جديدة",
                    "New description",
                    "وصف جديد"));

        // Act
        var opportunityId =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            opportunityId);

        var opportunity =
            await repository.GetByIdAsync(
                opportunityId,
                CancellationToken.None);

        Assert.NotNull(opportunity);

        Assert.Equal(
            opportunityId,
            opportunity.Id);

        Assert.Equal(
            OpportunityStatusCode.Draft,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.Equal(
            "New Opportunity",
            opportunity.GetCurrentVersion().OpportunityName.En);

        Assert.Equal(
            "فرصة جديدة",
            opportunity.GetCurrentVersion().OpportunityName.Ar);

        var description =
            opportunity.GetCurrentVersion().Description;

        Assert.NotNull(description);

        Assert.Equal(
            "New description",
            description.En);

        Assert.Equal(
            "وصف جديد",
            description.Ar);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_CreatesOpportunityWithCurrentUserAsCreator()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("creator-user");

        var handler =
            new CreateDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new CreateDraftCommand(
                CreateContentRequest());

        // Act
        var opportunityId =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        var opportunity =
            await repository.GetByIdAsync(
                opportunityId,
                CancellationToken.None);

        Assert.NotNull(opportunity);

        var createdVersion =
            opportunity!.GetCurrentVersion();

        Assert.Equal(
            "creator-user",
            createdVersion.CreatedBy);
    }

    [Fact]
    public async Task Handle_CreatesSingleInitialVersion()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("creator-user");

        var handler =
            new CreateDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new CreateDraftCommand(
                CreateContentRequest());

        // Act
        var opportunityId =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        var opportunity =
            await repository.GetByIdAsync(
                opportunityId,
                CancellationToken.None);

        Assert.NotNull(opportunity);

        Assert.Single(
            opportunity.Versions);

        Assert.Equal(
            1,
            opportunity.GetCurrentVersion().VersionNumber);
    }

    [Fact]
    public async Task Handle_DoesNotCreateSubmissionOrAuditHistory()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("creator-user");

        var handler =
            new CreateDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new CreateDraftCommand(
                CreateContentRequest());

        // Act
        var opportunityId =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        var opportunity =
            await repository.GetByIdAsync(
                opportunityId,
                CancellationToken.None);

        Assert.NotNull(opportunity);

        Assert.Empty(
            opportunity.Submissions);

        Assert.Empty(
            opportunity.AuditHistories);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToUnitOfWork()
    {
        // Arrange
        var repository =
            new FakeOpportunityRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            new FakeCurrentUser("creator-user");

        var handler =
            new CreateDraftCommandHandler(
                repository,
                unitOfWork,
                currentUser);

        var command =
            new CreateDraftCommand(
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
