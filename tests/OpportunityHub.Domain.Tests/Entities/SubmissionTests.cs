using OpportunityHub.Domain.Entities.Submissions;
using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Tests.Entities;

public sealed class SubmissionTests
{
    private static readonly Guid OpportunityVersionId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private const string SubmittedBy = "user-1";

    #region Creation

    /// <summary>
    /// Verifies that a first publication submission is created
    /// with all provided values.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateSubmission()
    {
        // Arrange
        var sequenceNumber = 1L;

        var submittedAt = new DateTime(
            2026,
            8,
            21,
            10,
            0,
            0,
            DateTimeKind.Utc);

        // Act
        var submission = Submission.Create(
            OpportunityVersionId,
            sequenceNumber,
            SubmissionType.FirstPublication,
            null,
            OpportunityStatusCode.Draft,
            null,
            SubmittedBy,
            submittedAt);

        // Assert
        Assert.NotEqual(Guid.Empty, submission.Id);
        Assert.Equal(OpportunityVersionId, submission.OpportunityVersionId);
        Assert.Equal(sequenceNumber, submission.SequenceNumber);

        Assert.Equal(
            SubmissionType.FirstPublication,
            submission.SubmissionType);

        Assert.Null(submission.EditSummary);

        Assert.Equal(
            OpportunityStatusCode.Draft,
            submission.PreviousStatusCode);

        Assert.Null(submission.PreviousSubStatusCode);

        Assert.Equal(
            SubmittedBy,
            submission.SubmittedBy);

        Assert.Equal(
            submittedAt,
            submission.SubmittedAtUtc);
    }

    /// <summary>
    /// Verifies that a non-first-publication submission stores
    /// the provided edit summary.
    /// </summary>
    [Fact]
    public void Create_ShouldStoreEditSummary_ForNonFirstPublication()
    {
        // Arrange
        const string editSummary = "Updated Opportunity information.";

        // Act
        var submission = Submission.Create(
            OpportunityVersionId,
            1,
            SubmissionType.PublishedModification,
            editSummary,
            OpportunityStatusCode.PublishedUnderReview,
            null,
            SubmittedBy);

        // Assert
        Assert.Equal(
            editSummary,
            submission.EditSummary);
    }

    /// <summary>
    /// Verifies that the submission timestamp defaults to the current
    /// UTC time when no timestamp is provided.
    /// </summary>
    [Fact]
    public void Create_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var submission = Submission.Create(
            OpportunityVersionId,
            1,
            SubmissionType.FirstPublication,
            null,
            OpportunityStatusCode.Draft,
            null,
            SubmittedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            submission.SubmittedAtUtc,
            before,
            after);
    }

    /// <summary>
    /// Verifies that creating a submission fails when the submitter is empty.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenSubmittedByIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Submission.Create(
                OpportunityVersionId,
                1,
                SubmissionType.FirstPublication,
                null,
                OpportunityStatusCode.Draft,
                null,
                string.Empty));
    }

    /// <summary>
    /// Verifies that first publication submissions cannot have an edit summary.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEditSummary_ForFirstPublication()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Submission.Create(
                OpportunityVersionId,
                1,
                SubmissionType.FirstPublication,
                "Some edit summary",
                OpportunityStatusCode.Draft,
                null,
                SubmittedBy));

        Assert.Equal(
            "Edit summary must not be provided for first publication submissions. (Parameter 'editSummary')",
            exception.Message);
    }

    /// <summary>
    /// Verifies that published modification submissions require an edit summary.
    /// </summary>
    [Fact]
    public void Create_ShouldRequireEditSummary_ForPublishedModification()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Submission.Create(
                OpportunityVersionId,
                1,
                SubmissionType.PublishedModification,
                null,
                OpportunityStatusCode.PublishedUnderReview,
                null,
                SubmittedBy));

        Assert.Equal(
            "Edit summary is required for non-first publication submissions. (Parameter 'editSummary')",
            exception.Message);
    }

    /// <summary>
    /// Verifies that approved modification submissions require an edit summary.
    /// </summary>
    [Fact]
    public void Create_ShouldRequireEditSummary_ForApprovedModification()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Submission.Create(
                OpportunityVersionId,
                1,
                SubmissionType.ApprovedModification,
                null,
                OpportunityStatusCode.PublishedUnderReview,
                null,
                SubmittedBy));

        Assert.Equal(
            "Edit summary is required for non-first publication submissions. (Parameter 'editSummary')",
            exception.Message);
    }

    /// <summary>
    /// Verifies that manager direct edit submissions require an edit summary.
    /// </summary>
    [Fact]
    public void Create_ShouldRequireEditSummary_ForManagerDirectEdit()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Submission.Create(
                OpportunityVersionId,
                1,
                SubmissionType.ManagerDirectEdit,
                null,
                OpportunityStatusCode.PublishedUnderReview,
                null,
                SubmittedBy));

        Assert.Equal(
            "Edit summary is required for non-first publication submissions. (Parameter 'editSummary')",
            exception.Message);
    }

    /// <summary>
    /// Verifies that whitespace-only edit summaries are rejected
    /// for non-first-publication submissions.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectWhitespaceEditSummary_ForNonFirstPublication()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Submission.Create(
                OpportunityVersionId,
                1,
                SubmissionType.PublishedModification,
                "   ",
                OpportunityStatusCode.PublishedUnderReview,
                null,
                SubmittedBy));

        Assert.Equal(
            "Edit summary is required for non-first publication submissions. (Parameter 'editSummary')",
            exception.Message);
    }

    #endregion

    #region Modification Request

    /// <summary>
    /// Verifies that a submission can create a modification request
    /// containing the requested modification items.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldCreateModificationRequest()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        var items = new[]
        {
            ("OpportunityName", "Please update the Opportunity name."),
            ("Description", "Please provide a more detailed description.")
        };

        // Act
        submission.RequestModification(
            items,
            "manager-1");

        // Assert
        Assert.NotNull(submission.ModificationRequest);
        Assert.Null(submission.ModificationRejection);
        Assert.Null(submission.FinalRejection);

        Assert.Equal(
            2,
            submission.ModificationRequest!.Items.Count);
    }

    /// <summary>
    /// Verifies that modification field names and comments are trimmed
    /// before being stored.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldTrimFieldAndComment()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act
        submission.RequestModification(
            new[]
            {
                (
                    "  OpportunityName  ",
                    "  Please update the name.  ")
            },
            "manager-1");

        // Assert
        var item = Assert.Single(
            submission.ModificationRequest!.Items);

        Assert.Equal(
            "OpportunityName",
            item.FieldName);

        Assert.Equal(
            "Please update the name.",
            item.Comment);
    }

    /// <summary>
    /// Verifies that modification requests cannot contain duplicate
    /// field names, ignoring case and surrounding whitespace.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldRejectDuplicateFields()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        var items = new[]
        {
            ("OpportunityName", "First comment"),
            (" Opportunityname ", "Second comment")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RequestModification(
                items,
                "manager-1"));

        Assert.Equal(
        "A modification request already exists for field 'OpportunityName'.",
        exception.Message,
        StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that a modification request must contain at least one item.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldRejectEmptyRequest()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RequestModification(
                [],
                "manager-1"));

        Assert.Equal(
            "A modification request must contain at least one item.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a modification item cannot have an empty field name.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldRejectEmptyFieldName()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            submission.RequestModification(
                new[]
                {
                    ("", "Some comment")
                },
                "manager-1"));
    }

    /// <summary>
    /// Verifies that a modification item cannot have an empty comment.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldRejectEmptyComment()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            submission.RequestModification(
                new[]
                {
                    ("OpportunityName", "")
                },
                "manager-1"));
    }

    /// <summary>
    /// Verifies that a modification request cannot be created after
    /// another workflow decision has already been made.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldRejectWhenSubmissionAlreadyHasDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        submission.RejectOpportunity(
            1,
            "Final rejection",
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RequestModification(
                new[]
                {
                    ("OpportunityName", "Change it")
                },
                "manager-1"));

        Assert.Equal(
            "The submission already has a workflow decision.",
            exception.Message);
    }

    #endregion

    #region Modification Rejection

    /// <summary>
    /// Verifies that a published modification submission can be rejected
    /// through modification rejection.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldCreateModificationRejection_ForPublishedModification()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.PublishedModification);

        // Act
        submission.RejectModification(
            "The requested changes were not acceptable.",
            "manager-1");

        // Assert
        Assert.NotNull(
            submission.ModificationRejection);

        Assert.Equal(
            "The requested changes were not acceptable.",
            submission.ModificationRejection!.Comment);

        Assert.Null(submission.ModificationRequest);
        Assert.Null(submission.FinalRejection);
    }

    /// <summary>
    /// Verifies that an approved modification submission can be rejected
    /// through modification rejection.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldCreateModificationRejection_ForApprovedModification()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.ApprovedModification);

        // Act
        submission.RejectModification(
            "The requested changes were not acceptable.",
            "manager-1");

        // Assert
        Assert.NotNull(
            submission.ModificationRejection);

        Assert.Equal(
            "The requested changes were not acceptable.",
            submission.ModificationRejection!.Comment);
    }

    /// <summary>
    /// Verifies that manager direct edit submissions cannot be rejected
    /// through modification rejection.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldRejectManagerDirectEditSubmission()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.ManagerDirectEdit);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectModification(
                "Rejected",
                "manager-1"));

        Assert.Equal(
            "Modification rejection is not allowed for submission type 'ManagerDirectEdit'.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that first publication submissions cannot be rejected
    /// through modification rejection.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldRejectFirstPublicationSubmission()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectModification(
                "Rejected",
                "manager-1"));

        Assert.Equal(
            "Modification rejection is not allowed for submission type 'FirstPublication'.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a submission cannot receive a second decision
    /// after modification rejection.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldNotAllowSecondDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.PublishedModification);

        submission.RejectModification(
            "Rejected",
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectModification(
                "Rejected again",
                "manager-2"));

        Assert.Equal(
            "The submission already has a workflow decision.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that modification rejection requires a non-empty comment.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldRejectEmptyComment()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.PublishedModification);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            submission.RejectModification(
                "",
                "manager-1"));
    }

    #endregion

    #region Final Rejection

    /// <summary>
    /// Verifies that a first publication submission can be finally rejected
    /// with a valid rejection reason and comment.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldCreateFinalRejection()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act
        submission.RejectOpportunity(
            10,
            "The opportunity does not meet the requirements.",
            "manager-1");

        // Assert
        Assert.NotNull(
            submission.FinalRejection);

        Assert.Equal(
            10,
            submission.FinalRejection!.RejectionReasonId);

        Assert.Equal(
            "The opportunity does not meet the requirements.",
            submission.FinalRejection.Comment);

        Assert.Null(submission.ModificationRequest);
        Assert.Null(submission.ModificationRejection);
    }

    /// <summary>
    /// Verifies that final rejection is not allowed for published
    /// modification submissions.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldRejectPublishedModificationSubmission()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.PublishedModification);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectOpportunity(
                1,
                "Final rejection",
                "manager-1"));

        Assert.Equal(
            "An opportunity can only be rejected during first publication.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that final rejection is not allowed for approved
    /// modification submissions.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldRejectApprovedModificationSubmission()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.ApprovedModification);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectOpportunity(
                1,
                "Final rejection",
                "manager-1"));

        Assert.Equal(
            "An opportunity can only be rejected during first publication.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that final rejection is not allowed for manager direct
    /// edit submissions.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldRejectManagerDirectEditSubmission()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.ManagerDirectEdit);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectOpportunity(
                1,
                "Final rejection",
                "manager-1"));

        Assert.Equal(
            "An opportunity can only be rejected during first publication.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that final rejection requires a valid rejection reason.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldRejectInvalidReason()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            submission.RejectOpportunity(
                0,
                "Invalid rejection reason",
                "manager-1"));
    }

    /// <summary>
    /// Verifies that final rejection requires a non-empty comment.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldRejectEmptyComment()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            submission.RejectOpportunity(
                1,
                "",
                "manager-1"));
    }

    /// <summary>
    /// Verifies that a submission cannot receive a second final rejection
    /// after a final rejection has already been recorded.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldRejectWhenSubmissionAlreadyHasDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        submission.RejectOpportunity(
            1,
            "First rejection",
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectOpportunity(
                2,
                "Second rejection",
                "manager-2"));

        Assert.Equal(
            "The submission already has a workflow decision.",
            exception.Message);
    }

    #endregion

    #region Decision Exclusivity

    /// <summary>
    /// Verifies that final rejection cannot be created after a modification
    /// request has already been recorded.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldPreventFinalRejectionAfterDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        submission.RequestModification(
            new[]
            {
                ("OpportunityName", "Change the name.")
            },
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectOpportunity(
                1,
                "Final rejection",
                "manager-1"));

        Assert.Equal(
            "The submission already has a workflow decision.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that modification rejection cannot be created after
    /// a modification request has already been recorded.
    /// </summary>
    [Fact]
    public void RequestModification_ShouldPreventModificationRejectionAfterDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.PublishedModification);

        submission.RequestModification(
            new[]
            {
                ("OpportunityName", "Change the name.")
            },
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectModification(
                "Rejected",
                "manager-1"));

        Assert.Equal(
            "The submission already has a workflow decision.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a final rejection cannot be created after
    /// a modification rejection has already been recorded.
    /// </summary>
    [Fact]
    public void RejectModification_ShouldPreventFinalRejectionAfterDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.PublishedModification);

        submission.RejectModification(
            "Rejected",
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RejectOpportunity(
                1,
                "Final rejection",
                "manager-1"));

        Assert.Equal(
            "An opportunity can only be rejected during first publication.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a modification request cannot be created after
    /// a final rejection has already been recorded.
    /// </summary>
    [Fact]
    public void RejectOpportunity_ShouldPreventModificationRequestAfterDecision()
    {
        // Arrange
        var submission = CreateSubmission(
            SubmissionType.FirstPublication);

        submission.RejectOpportunity(
            1,
            "Final rejection",
            "manager-1");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            submission.RequestModification(
                new[]
                {
                    ("OpportunityName", "Change it")
                },
                "manager-1"));

        Assert.Equal(
            "The submission already has a workflow decision.",
            exception.Message);
    }

    #endregion

    #region Helpers

    private static Submission CreateSubmission(
        SubmissionType submissionType,
        OpportunityStatusCode previousStatus =
            OpportunityStatusCode.PendingManagerReview,
        OpportunitySubStatusCode? previousSubStatus = null)
    {
        var editSummary =
            submissionType == SubmissionType.FirstPublication
                ? null
                : "Updated opportunity information.";

        return Submission.Create(
            OpportunityVersionId,
            1,
            submissionType,
            editSummary,
            previousStatus,
            previousSubStatus,
            SubmittedBy,
            new DateTime(
                2026,
                8,
                21,
                10,
                0,
                0,
                DateTimeKind.Utc));
    }

    #endregion
}