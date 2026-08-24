using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;
using OpportunityHub.Domain.ValueObjects;
using OpportunityHub.Domain.Workflow;

namespace OpportunityHub.Domain.Tests.Entities;

public sealed class OpportunityTests
{
    #region CreateDraft

    /// <summary>
    /// Verifies that a draft opportunity is created in the correct initial state.
    /// </summary>
    [Fact]
    public void CreateDraft_ShouldCreateDraftOpportunity()
    {
        // Arrange
        var opportunityId = Guid.NewGuid();
        var content = CreateContent();
        const string createdBy = "creator";
        var createdAtUtc = CreatedAtUtc;

        // Act
        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            content,
            createdBy,
            createdAtUtc);

        // Assert
        Assert.Equal(opportunityId, opportunity.Id);
        Assert.Equal(
            OpportunityStatusCode.Draft,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);
        Assert.True(opportunity.IsDraft);
        Assert.False(opportunity.IsPublished);
        Assert.False(opportunity.IsUnderReview);
        Assert.False(opportunity.IsRejected);
        Assert.False(opportunity.IsApproved);

        Assert.True(opportunity.IsActive);

        Assert.Equal(
            createdBy,
            opportunity.CreatedBy);

        Assert.Equal(
            createdAtUtc,
            opportunity.CreatedAtUtc);

        Assert.Null(opportunity.UpdatedBy);
        Assert.Null(opportunity.UpdatedAtUtc);

        Assert.Equal(
            0,
            opportunity.LastSubmissionSequenceNumber);

        Assert.Equal(
            0,
            opportunity.LastActivitySequenceNumber);
    }

    /// <summary>
    /// Verifies that creating a draft creates exactly one current version.
    /// </summary>
    [Fact]
    public void CreateDraft_ShouldCreateInitialVersion()
    {
        // Arrange
        var opportunityId = Guid.NewGuid();
        var content = CreateContent();

        // Act
        var opportunity = Opportunity.CreateDraft(
            opportunityId,
            content,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        var version = Assert.Single(
            opportunity.Versions);

        Assert.Equal(
            opportunityId,
            version.OpportunityId);

        Assert.Equal(
            1,
            version.VersionNumber);

        Assert.True(version.IsCurrent);
        Assert.False(version.IsPublishedSnapshot);

        Assert.Equal(
            content.OpportunityName.En,
            version.OpportunityName.En);

        Assert.Equal(
            content.OpportunityName.Ar,
            version.OpportunityName.Ar);
    }

    /// <summary>
    /// Verifies that the initial version contains the supplied reference data.
    /// </summary>
    [Fact]
    public void CreateDraft_ShouldCreateInitialVersionWithReferenceData()
    {
        // Arrange
        var channelId = Guid.NewGuid();
        var secondChannelId = Guid.NewGuid();
        var sectorId = Guid.NewGuid();

        var content = CreateContent(
            channelIds:
            [
                channelId,
                secondChannelId,
                channelId
            ],
            sectorIds:
            [
                sectorId
            ]);

        // Act
        var opportunity = Opportunity.CreateDraft(
            Guid.NewGuid(),
            content,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        var version = opportunity.GetCurrentVersion();

        Assert.Equal(
            2,
            version.Channels.Count);

        Assert.Contains(
            version.Channels,
            x => x.ChannelId == channelId);

        Assert.Contains(
            version.Channels,
            x => x.ChannelId == secondChannelId);

        Assert.Single(version.Sectors);

        Assert.Equal(
            sectorId,
            version.Sectors.Single().SectorId);
    }

    /// <summary>
    /// Verifies that a null content value is rejected.
    /// </summary>
    [Fact]
    public void CreateDraft_ShouldRejectNullContent()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Opportunity.CreateDraft(
                Guid.NewGuid(),
                null!,
                CreatedBy,
                CreatedAtUtc));
    }

    #endregion

    #region Version Access

    /// <summary>
    /// Verifies that the current version can be retrieved.
    /// </summary>
    [Fact]
    public void GetCurrentVersion_ShouldReturnCurrentVersion()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var version = opportunity.GetCurrentVersion();

        // Assert
        Assert.True(version.IsCurrent);
        Assert.Equal(
            1,
            version.VersionNumber);
    }

    /// <summary>
    /// Verifies that no published version exists before publication.
    /// </summary>
    [Fact]
    public void GetPublishedVersion_ShouldReturnNullBeforePublication()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var version = opportunity.GetPublishedVersion();

        // Assert
        Assert.Null(version);
    }

    /// <summary>
    /// Verifies that the latest review submission cannot be retrieved
    /// before a submission exists.
    /// </summary>
    [Fact]
    public void GetLatestReviewSubmission_ShouldThrowWhenNoSubmissionExists()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowDomainException>(() =>
                opportunity.GetLatestReviewSubmission());

        Assert.StartsWith(
            "The opportunity does not have a review submission.",
            exception.Message);
    }

    #endregion

    #region First Publication - Submission

    /// <summary>
    /// Verifies that submitting a draft moves it to manager review.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromDraft_ShouldMoveToPendingManagerReview()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);
        Assert.True(opportunity.IsUnderReview);

        Assert.Equal(
            1,
            opportunity.LastSubmissionSequenceNumber);

        Assert.Equal(
            1,
            opportunity.LastActivitySequenceNumber);

        Assert.Equal(
            "manager",
            opportunity.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            opportunity.UpdatedAtUtc);

        Assert.Equal(
            SubmissionType.FirstPublication,
            submission.SubmissionType);
    }

    /// <summary>
    /// Verifies the state stored on a first-publication submission.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromDraft_ShouldStorePreviousState()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Draft,
            submission.PreviousStatusCode);

        Assert.Null(
            submission.PreviousSubStatusCode);

        Assert.Equal(
            1,
            submission.SequenceNumber);

        Assert.Equal(
            "manager",
            submission.SubmittedBy);

        Assert.Equal(
            UpdatedAtUtc,
            submission.SubmittedAtUtc);
    }

    /// <summary>
    /// Verifies that submitting for review creates the corresponding audit history.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_ShouldCreateAuditHistory()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: UpdatedAtUtc);

        // Assert
        var audit = Assert.Single(
            opportunity.AuditHistories);

        Assert.Equal(
            opportunity.Id,
            audit.OpportunityId);

        Assert.Equal(
            opportunity.GetCurrentVersion().Id,
            audit.OpportunityVersionId);

        Assert.Equal(
            submission.Id,
            audit.SubmissionId);

        Assert.Equal(
            1,
            audit.ActivitySequenceNumber);

        Assert.Equal(
            WorkflowActivityType.SubmittedForManagerReview,
            audit.ActivityType);

        Assert.Equal(
            "None",
            audit.RelatedEntityType);

        Assert.Null(audit.RelatedEntityId);

        Assert.Equal(
            "manager",
            audit.CreatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            audit.CreatedAtUtc);
    }

    #endregion

    #region Modification Request

    /// <summary>
    /// Verifies that a manager can request specialist modifications
    /// during first publication.
    /// </summary>
    [Fact]
    public void RequestModification_FromPendingManagerReview_ShouldMoveToPendingSpecialistModification()
    {
        // Arrange
        var opportunity = CreateDraft();

        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "specialist",
            submittedAtUtc: CreatedAtUtc);

        var items =
            new[]
            {
                ("OpportunityName", "Please update the Opportunity name.")
            };

        // Act
        opportunity.RequestModification(
            items,
            "manager",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PendingSpecialistModification,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);

        Assert.NotNull(
            submission.ModificationRequest);

        Assert.Single(
            submission.ModificationRequest!.Items);

        var item =
            submission.ModificationRequest.Items.Single();

        Assert.Equal(
            "OpportunityName",
            item.FieldName);

        Assert.Equal(
            "Please update the Opportunity name.",
            item.Comment);
    }

    /// <summary>
    /// Verifies that the modification request is referenced by the audit history.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldReferenceModificationRequestInAuditHistory()
    {
        // Arrange
        var opportunity = CreateDraft();

        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "specialist",
            submittedAtUtc: CreatedAtUtc);

        opportunity.RequestModification(
            new[]
            {
                ("OpportunityName", "Please update the name.")
            },
            "manager",
            UpdatedAtUtc);

        // Act
        var audit = opportunity.AuditHistories
            .Single(x =>
                x.ActivityType ==
                WorkflowActivityType.ModificationRequested);

        // Assert
        Assert.Equal(
            opportunity.Id,
            audit.OpportunityId);

        Assert.Equal(
            submission.Id,
            audit.SubmissionId);

        Assert.Equal(
            nameof(ModificationRequest),
            audit.RelatedEntityType);

        Assert.Equal(
            submission.ModificationRequest!.Id,
            audit.RelatedEntityId);

        Assert.Equal(
            2,
            audit.ActivitySequenceNumber);
    }

    /// <summary>
    /// Verifies that specialist submission returns the first-publication
    /// workflow to manager review.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_AfterModificationRequest_ShouldReturnToPendingManagerReview()
    {
        // Arrange
        var opportunity = CreateDraft();

        opportunity.SubmitForManagerReview(
            CreateContent(),
            "specialist",
            submittedAtUtc: CreatedAtUtc);

        opportunity.RequestModification(
            new[]
            {
                ("OpportunityName", "Please update the name.")
            },
            "manager",
            UpdatedAtUtc);

        // Act
        var submission = opportunity.SubmitForManagerReview(
            CreateContent("Updated Opportunity"),
            "specialist",
            submittedAtUtc: PublishedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);

        Assert.Equal(
            2,
            submission.SequenceNumber);

        Assert.Equal(
            2,
            opportunity.LastSubmissionSequenceNumber);
    }

    #endregion

    #region Approval

    /// <summary>
    /// Verifies that approving a first-publication submission moves
    /// the opportunity to Approved.
    /// </summary>
    [Fact]
    public void Approve_FromPendingManagerReview_ShouldMoveToApproved()
    {
        // Arrange
        var opportunity = CreateDraft();

        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        // Act
        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Approved,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);
        Assert.True(opportunity.IsApproved);

        var audit = opportunity.AuditHistories
            .Single(x =>
                x.ActivityType ==
                WorkflowActivityType.Approved);

        Assert.Equal(
            submission.Id,
            audit.SubmissionId);

        Assert.Equal(
            "None",
            audit.RelatedEntityType);
        Assert.Null(audit.RelatedEntityId);
    }

    #endregion

    #region Final Rejection

    /// <summary>
    /// Verifies that rejecting a first-publication submission permanently
    /// rejects the opportunity.
    /// </summary>
    [Fact]
    public void Reject_FromPendingManagerReview_ShouldMoveToRejected()
    {
        // Arrange
        var opportunity = CreateDraft();

        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        // Act
        opportunity.Reject(
            10,
            "The opportunity does not meet the required criteria.",
            "manager",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Rejected,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);
        Assert.True(opportunity.IsRejected);

        Assert.NotNull(
            submission.FinalRejection);

        Assert.Equal(
            10,
            submission.FinalRejection!.RejectionReasonId);

        Assert.Equal(
            "The opportunity does not meet the required criteria.",
            submission.FinalRejection.Comment);

        Assert.Equal(
            "manager",
            submission.FinalRejection.CreatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            submission.FinalRejection.CreatedAtUtc);
    }

    /// <summary>
    /// Verifies that the final rejection is referenced by audit history.
    /// </summary>
    [Fact]
    public void Reject_ShouldReferenceFinalRejectionInAuditHistory()
    {
        // Arrange
        var opportunity = CreateDraft();

        var submission = opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        opportunity.Reject(
            10,
            "Rejected by manager.",
            "manager",
            UpdatedAtUtc);

        // Act
        var audit = opportunity.AuditHistories
            .Single(x =>
                x.ActivityType ==
                WorkflowActivityType.OpportunityRejected);

        // Assert
        Assert.Equal(
            opportunity.Id,
            audit.OpportunityId);

        Assert.Equal(
            submission.Id,
            audit.SubmissionId);

        Assert.Equal(
            nameof(FinalRejection),
            audit.RelatedEntityType);

        Assert.Equal(
            submission.FinalRejection!.Id,
            audit.RelatedEntityId);
    }

    /// <summary>
    /// Verifies that modification rejection is not available during
    /// the first-publication workflow.
    /// </summary>
    [Fact]
    public void RejectModification_FromFirstPublicationWorkflow_ShouldNotBeAllowed()
    {
        // Arrange
        var opportunity = CreateDraft();

        opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowTransitionNotAllowedException>(() =>
                opportunity.RejectModification(
                    "Reject modification.",
                    "manager",
                    UpdatedAtUtc));

        Assert.Equal(
            WorkflowAction.RejectModification,
            exception.Key.Action);

        Assert.Equal(
            OpportunityStatusCode.PendingManagerReview,
            exception.Key.StatusCode);

        Assert.Null(
            exception.Key.SubStatusCode);
    }

    #endregion

    #region Publishing

    /// <summary>
    /// Verifies that an approved first-publication opportunity can be published.
    /// </summary>
    [Fact]
    public void Publish_FromApproved_ShouldMoveToPublished()
    {
        // Arrange
        var opportunity = CreateDraft();

        opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        // Act
        opportunity.Publish(
            "publisher",
            PublishedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Published,
            opportunity.StatusCode);

        Assert.Null(opportunity.SubStatusCode);

        Assert.True(opportunity.IsPublished);

        Assert.Equal(
            PublishedAtUtc,
            opportunity.PublishedAtUtc);

        var version = opportunity.GetCurrentVersion();

        Assert.True(version.IsCurrent);
        Assert.True(version.IsPublishedSnapshot);

        Assert.Equal(
            PublishedAtUtc,
            version.PublishedAtUtc);
    }

    /// <summary>
    /// Verifies that the published version can be retrieved after publication.
    /// </summary>
    [Fact]
    public void GetPublishedVersion_AfterPublish_ShouldReturnPublishedVersion()
    {
        // Arrange
        var opportunity = CreatePublishedOpportunity();

        // Act
        var version = opportunity.GetPublishedVersion();

        // Assert
        Assert.NotNull(version);

        Assert.True(version!.IsPublishedSnapshot);
        Assert.True(version.IsCurrent);

        Assert.Equal(
            1,
            version.VersionNumber);
    }

    /// <summary>
    /// Verifies that publishing creates the expected audit history.
    /// </summary>
    [Fact]
    public void Publish_ShouldCreateAuditHistory()
    {
        // Arrange
        var opportunity = CreateDraft();

        opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        // Act
        opportunity.Publish(
            "publisher",
            PublishedAtUtc);

        // Assert
        var audit = opportunity.AuditHistories
            .Single(x =>
                x.ActivityType ==
                WorkflowActivityType.Published);

        Assert.Equal(
            opportunity.GetCurrentVersion().Id,
            audit.OpportunityVersionId);

        Assert.Null(audit.SubmissionId);
        Assert.Equal(
            "None",
            audit.RelatedEntityType);
        Assert.Null(audit.RelatedEntityId);

        Assert.Equal(
            "publisher",
            audit.CreatedBy);

        Assert.Equal(
            PublishedAtUtc,
            audit.CreatedAtUtc);
    }

    #endregion

    #region Published Modification

    /// <summary>
    /// Verifies that submitting a published opportunity creates a new
    /// working version.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromPublished_ShouldCreateNewWorkingVersion()
    {
        // Arrange
        var opportunity = CreatePublishedOpportunity();

        var publishedVersion =
            opportunity.GetCurrentVersion();

        var newContent =
            CreateContent("Modified Opportunity");

        // Act
        var submission = opportunity.SubmitForManagerReview(
            newContent,
            "specialist",
            "Update published Opportunity.",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PendingManagerReview,
            opportunity.SubStatusCode);

        Assert.Equal(
            SubmissionType.PublishedModification,
            submission.SubmissionType);

        Assert.Equal(
            2,
            opportunity.Versions.Count);

        var currentVersion =
            opportunity.GetCurrentVersion();

        Assert.NotEqual(
            publishedVersion.Id,
            currentVersion.Id);

        Assert.Equal(
            2,
            currentVersion.VersionNumber);

        Assert.True(currentVersion.IsCurrent);
        Assert.False(currentVersion.IsPublishedSnapshot);

        Assert.False(publishedVersion.IsCurrent);
        Assert.True(publishedVersion.IsPublishedSnapshot);

        Assert.Equal(
            "Modified Opportunity",
            currentVersion.OpportunityName.En);

        Assert.Equal(
            "Opportunity",
            publishedVersion.OpportunityName.En);
    }

    /// <summary>
    /// Verifies that the previous published version remains unchanged
    /// when a modification starts.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromPublished_ShouldPreservePublishedVersion()
    {
        // Arrange
        var opportunity = CreatePublishedOpportunity();

        var publishedVersion =
            opportunity.GetCurrentVersion();

        var publishedVersionId =
            publishedVersion.Id;

        var publishedAt =
            publishedVersion.PublishedAtUtc;

        // Act
        opportunity.SubmitForManagerReview(
            CreateContent("Modified Opportunity"),
            "specialist",
            "Update Opportunity.",
            UpdatedAtUtc);

        // Assert
        var preservedVersion =
            opportunity.Versions.Single(
                x => x.Id == publishedVersionId);

        Assert.True(
            preservedVersion.IsPublishedSnapshot);

        Assert.False(
            preservedVersion.IsCurrent);

        Assert.Equal(
            publishedAt,
            preservedVersion.PublishedAtUtc);

        Assert.Equal(
            "Opportunity",
            preservedVersion.OpportunityName.En);
    }

    /// <summary>
    /// Verifies that a published modification can be approved without
    /// publishing it immediately.
    /// </summary>
    [Fact]
    public void Approve_PublishedModification_ShouldSetApprovedSubStatus()
    {
        // Arrange
        var opportunity =
            CreatePublishedModificationPendingManagerReview();

        // Act
        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.PublishedUnderReview,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.Approved,
            opportunity.SubStatusCode);

        Assert.False(opportunity.IsPublished);

        var currentVersion =
            opportunity.GetCurrentVersion();

        Assert.True(currentVersion.IsCurrent);
        Assert.False(currentVersion.IsPublishedSnapshot);
    }

    /// <summary>
    /// Verifies that an approved published modification can be published.
    /// </summary>
    [Fact]
    public void Publish_PublishedModification_ShouldPublishNewVersion()
    {
        // Arrange
        var opportunity =
            CreatePublishedModificationPendingManagerReview();

        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        // Act
        opportunity.Publish(
            "publisher",
            PublishedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Published,
            opportunity.StatusCode);

        Assert.Equal(
            OpportunitySubStatusCode.PublishedModified,
            opportunity.SubStatusCode);

        Assert.True(opportunity.IsPublished);

        Assert.Equal(
            PublishedAtUtc,
            opportunity.PublishedAtUtc);

        var currentVersion =
            opportunity.GetCurrentVersion();

        Assert.True(currentVersion.IsCurrent);
        Assert.True(currentVersion.IsPublishedSnapshot);

        Assert.Equal(
            PublishedAtUtc,
            currentVersion.PublishedAtUtc);

        Assert.Equal(
            2,
            currentVersion.VersionNumber);
    }

    #endregion

    #region Published Modification Rejection

    /// <summary>
    /// Verifies that rejecting a published modification restores the
    /// state that existed before the modification cycle.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldRestorePreviousPublishedState()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        var originalVersion =
            opportunity.GetCurrentVersion();

        var originalVersionId =
            originalVersion.Id;

        opportunity.SubmitForManagerReview(
            CreateContent("Modified Opportunity"),
            "specialist",
            "Update Opportunity.",
            CreatedAtUtc);

        var submission =
            opportunity.GetLatestReviewSubmission();

        // Act
        opportunity.RejectModification(
            "Modification was rejected.",
            "manager",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Published,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.True(
            opportunity.IsPublished);

        Assert.NotNull(
            submission.ModificationRejection);

        Assert.Equal(
            "Modification was rejected.",
            submission.ModificationRejection!.Comment);

        Assert.Equal(
            "manager",
            submission.ModificationRejection.CreatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            submission.ModificationRejection.CreatedAtUtc);

        var publishedVersion =
            opportunity.Versions.Single(
                x => x.Id == originalVersionId);

        Assert.True(
            publishedVersion.IsPublishedSnapshot);
    }

    /// <summary>
    /// Verifies that the modification rejection is referenced by audit history.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldReferenceModificationRejectionInAuditHistory()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        opportunity.SubmitForManagerReview(
            CreateContent("Modified Opportunity"),
            "specialist",
            "Update Opportunity.",
            CreatedAtUtc);

        var submission =
            opportunity.GetLatestReviewSubmission();

        // Act
        opportunity.RejectModification(
            "Modification rejected.",
            "manager",
            UpdatedAtUtc);

        // Assert
        var audit = opportunity.AuditHistories
            .Single(x =>
                x.ActivityType ==
                WorkflowActivityType.ModificationRejected);

        Assert.Equal(
            submission.Id,
            audit.SubmissionId);

        Assert.Equal(
            nameof(ModificationRejection),
            audit.RelatedEntityType);

        Assert.Equal(
            submission.ModificationRejection!.Id,
            audit.RelatedEntityId);
    }

    #endregion

    #region Unpublishing

    /// <summary>
    /// Verifies that a published opportunity can be unpublished.
    /// </summary>
    [Fact]
    public void Unpublish_FromPublished_ShouldMoveToUnpublished()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        // Act
        opportunity.Unpublish(
            "manager",
            UpdatedAtUtc);

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Unpublished,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);

        Assert.False(
            opportunity.IsPublished);

        Assert.Equal(
            "manager",
            opportunity.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            opportunity.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that an opportunity published from a modification cycle
    /// can also be unpublished.
    /// </summary>
    [Fact]
    public void Unpublish_FromPublishedModified_ShouldMoveToUnpublished()
    {
        // Arrange
        var opportunity =
            CreatePublishedModificationPendingManagerReview();

        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        opportunity.Publish(
            "publisher",
            PublishedAtUtc);

        // Act
        opportunity.Unpublish(
            "manager",
            DateTime.UtcNow.AddMinutes(1));

        // Assert
        Assert.Equal(
            OpportunityStatusCode.Unpublished,
            opportunity.StatusCode);

        Assert.Null(
            opportunity.SubStatusCode);
    }

    /// <summary>
    /// Verifies that unpublishing creates the expected audit history.
    /// </summary>
    [Fact]
    public void Unpublish_ShouldCreateAuditHistory()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        var version =
            opportunity.GetCurrentVersion();

        // Act
        opportunity.Unpublish(
            "manager",
            UpdatedAtUtc);

        // Assert
        var audit = opportunity.AuditHistories
            .Single(x =>
                x.ActivityType ==
                WorkflowActivityType.Unpublished);

        Assert.Equal(
            opportunity.Id,
            audit.OpportunityId);

        Assert.Equal(
            version.Id,
            audit.OpportunityVersionId);

        Assert.Null(audit.SubmissionId);
        Assert.Equal(
            "None",
            audit.RelatedEntityType);
        Assert.Null(audit.RelatedEntityId);

        Assert.Equal(
            "manager",
            audit.CreatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            audit.CreatedAtUtc);
    }

    #endregion

    #region Sequence Numbers

    /// <summary>
    /// Verifies that submission sequence numbers are generated
    /// sequentially by the opportunity aggregate.
    /// </summary>
    [Fact]
    public void GetNextSubmissionSequenceNumber_ShouldIncrementSequence()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var first =
            opportunity.GetNextSubmissionSequenceNumber();

        var second =
            opportunity.GetNextSubmissionSequenceNumber();

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);

        Assert.Equal(
            2,
            opportunity.LastSubmissionSequenceNumber);
    }

    /// <summary>
    /// Verifies that activity sequence numbers are generated sequentially.
    /// </summary>
    [Fact]
    public void GetNextActivitySequenceNumber_ShouldIncrementSequence()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act
        var first =
            opportunity.GetNextActivitySequenceNumber();

        var second =
            opportunity.GetNextActivitySequenceNumber();

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);

        Assert.Equal(
            2,
            opportunity.LastActivitySequenceNumber);
    }

    #endregion

    #region Invalid Workflow Transitions

    /// <summary>
    /// Verifies that approval is not allowed from Draft.
    /// </summary>
    [Fact]
    public void Approve_FromDraft_ShouldThrowWorkflowTransitionNotAllowedException()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowTransitionNotAllowedException>(() =>
                opportunity.Approve(
                    "manager",
                    UpdatedAtUtc));

        Assert.Equal(
            OpportunityStatusCode.Draft,
            exception.Key.StatusCode);

        Assert.Null(
            exception.Key.SubStatusCode);

        Assert.Equal(
            WorkflowAction.Approve,
            exception.Key.Action);
    }

    /// <summary>
    /// Verifies that publication is not allowed from Draft.
    /// </summary>
    [Fact]
    public void Publish_FromDraft_ShouldThrowWorkflowTransitionNotAllowedException()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowTransitionNotAllowedException>(() =>
                opportunity.Publish(
                    "publisher",
                    PublishedAtUtc));

        Assert.Equal(
            OpportunityStatusCode.Draft,
            exception.Key.StatusCode);

        Assert.Equal(
            WorkflowAction.Publish,
            exception.Key.Action);
    }

    /// <summary>
    /// Verifies that final rejection is not allowed from Draft.
    /// </summary>
    [Fact]
    public void Reject_FromDraft_ShouldThrowWorkflowTransitionNotAllowedException()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowTransitionNotAllowedException>(() =>
                opportunity.Reject(
                    1,
                    "Rejected.",
                    "manager",
                    UpdatedAtUtc));

        Assert.Equal(
            OpportunityStatusCode.Draft,
            exception.Key.StatusCode);

        Assert.Equal(
            WorkflowAction.Reject,
            exception.Key.Action);
    }

    /// <summary>
    /// Verifies that an unpublished opportunity cannot be published
    /// without a workflow transition allowing it.
    /// </summary>
    [Fact]
    public void Publish_FromUnpublished_ShouldThrowWorkflowTransitionNotAllowedException()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        opportunity.Unpublish(
            "manager",
            UpdatedAtUtc);

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowTransitionNotAllowedException>(() =>
                opportunity.Publish(
                    "publisher",
                    PublishedAtUtc));

        Assert.Equal(
            OpportunityStatusCode.Unpublished,
            exception.Key.StatusCode);

        Assert.Equal(
            WorkflowAction.Publish,
            exception.Key.Action);
    }

    #endregion

    #region Edit Summary

    /// <summary>
    /// Verifies that an edit summary is required for a published modification.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromPublished_ShouldRequireEditSummary()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowDomainException>(() =>
                opportunity.SubmitForManagerReview(
                    CreateContent("Modified Opportunity"),
                    "specialist",
                    null,
                    UpdatedAtUtc));

        Assert.Equal(
            "An edit summary is required for this submission.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that an edit summary is trimmed before it is stored.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromPublished_ShouldTrimEditSummary()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        // Act
        var submission =
            opportunity.SubmitForManagerReview(
                CreateContent("Modified Opportunity"),
                "specialist",
                "   Update Opportunity.   ",
                UpdatedAtUtc);

        // Assert
        Assert.Equal(
            "Update Opportunity.",
            submission.EditSummary);
    }

    /// <summary>
    /// Verifies that an edit summary longer than 2,000 characters is rejected.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromPublished_ShouldRejectEditSummaryOver2000Characters()
    {
        // Arrange
        var opportunity =
            CreatePublishedOpportunity();

        var editSummary =
            new string('x', 2001);

        // Act & Assert
        var exception =
            Assert.Throws<WorkflowDomainException>(() =>
                opportunity.SubmitForManagerReview(
                    CreateContent("Modified Opportunity"),
                    "specialist",
                    editSummary,
                    UpdatedAtUtc));

        Assert.Equal(
            "The edit summary cannot exceed 2,000 characters.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that an edit summary is not allowed for first publication.
    /// </summary>
    [Fact]
    public void SubmitForManagerReview_FromDraft_ShouldRejectEditSummary()
    {
        // Arrange
        var opportunity = CreateDraft();

        // Act & Assert
        var exception =
            Assert.Throws<ArgumentException>(() =>
                opportunity.SubmitForManagerReview(
                    CreateContent(),
                    "manager",
                    "This should not be provided.",
                    UpdatedAtUtc));

        Assert.Equal(
            "editSummary",
            exception.ParamName);
    }

    #endregion

    #region Helpers

    private static readonly DateTime CreatedAtUtc =
        new(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime UpdatedAtUtc =
        new(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime PublishedAtUtc =
        new(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

    private const string CreatedBy = "creator";

    private static Opportunity CreateDraft(
        OpportunityVersionContent? content = null)
    {
        return Opportunity.CreateDraft(
            Guid.NewGuid(),
            content ?? CreateContent(),
            CreatedBy,
            CreatedAtUtc);
    }

    private static Opportunity CreatePublishedOpportunity()
    {
        var opportunity = CreateDraft();

        opportunity.SubmitForManagerReview(
            CreateContent(),
            "manager",
            submittedAtUtc: CreatedAtUtc);

        opportunity.Approve(
            "manager",
            UpdatedAtUtc);

        opportunity.Publish(
            "publisher",
            PublishedAtUtc);

        return opportunity;
    }

    private static Opportunity CreatePublishedModificationPendingManagerReview()
    {
        var opportunity =
            CreatePublishedOpportunity();

        opportunity.SubmitForManagerReview(
            CreateContent("Modified Opportunity"),
            "specialist",
            "Update published Opportunity.",
            UpdatedAtUtc);

        return opportunity;
    }

    private static OpportunityVersionContent CreateContent(
        string OpportunityName = "Opportunity",
        IReadOnlyCollection<Guid>? channelIds = null,
        IReadOnlyCollection<Guid>? sectorIds = null)
    {
        return new OpportunityVersionContent
        {
            OpportunityName =
                new LocalizedText(
                    OpportunityName,
                    "المنتج"),

            NationalImpact =
                new LocalizedText(
                    "National impact",
                    "الأثر الوطني"),

            Description =
                new LocalizedText(
                    "Opportunity description",
                    "وصف المنتج"),

            CompanyName =
                new LocalizedText(
                    "Company",
                    "الشركة"),

            OpportunityOwnerName =
                new LocalizedText(
                    "Opportunity Owner",
                    "مالك المنتج"),

            OpportunityOwnerEmail =
                "owner@example.com",

            OpportunityOwnerPhone =
                "+201000000000",

            ChannelIds =
                channelIds ?? [],

            SectorIds =
                sectorIds ?? []
        };
    }

    #endregion
}
