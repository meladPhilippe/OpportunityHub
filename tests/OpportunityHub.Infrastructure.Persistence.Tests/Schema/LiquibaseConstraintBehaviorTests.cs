using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Schema;

[Collection("Infrastructure Integration Tests")]
public sealed class LiquibaseConstraintBehaviorTests
{
    private readonly OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture _fixture;

    public LiquibaseConstraintBehaviorTests(
        OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OpportunityVersion_should_reject_duplicate_version_number()
    {
        await using var db = _fixture.CreateDbContext();

        var opportunityId = Guid.NewGuid();
        var version1Id = Guid.NewGuid();
        var version2Id = Guid.NewGuid();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Opportunity
                (Id, StatusCode, IsActive, CreatedBy)
            VALUES
                ({0}, 1, 1, 'test')
            """,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OpportunityVersion
                (Id, OpportunityId, VersionNumber, IsCurrent,
                 IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
            VALUES
                ({0}, {1}, 1, 1, 0, 'Version 1', 'test')
            """,
            version1Id,
            opportunityId);

        Func<Task> act = async () =>
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO dbo.OpportunityVersion
                    (Id, OpportunityId, VersionNumber, IsCurrent,
                     IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
                VALUES
                    ({0}, {1}, 1, 0, 0, 'Duplicate Version', 'test')
                """,
                version2Id,
                opportunityId);

        var exception = await act.Should().ThrowAsync<SqlException>();

        exception.Which
            .Number.Should().Be(2627);
    }

    [Fact]
    public async Task OpportunityVersion_should_reject_multiple_current_versions()
    {
        await using var db = _fixture.CreateDbContext();

        var opportunityId = Guid.NewGuid();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Opportunity
                (Id, StatusCode, IsActive, CreatedBy)
            VALUES
                ({0}, 1, 1, 'test')
            """,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OpportunityVersion
                (Id, OpportunityId, VersionNumber, IsCurrent,
                 IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
            VALUES
                ({0}, {1}, 1, 1, 0, 'Version 1', 'test')
            """,
            Guid.NewGuid(),
            opportunityId);

        Func<Task> act = async () =>
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO dbo.OpportunityVersion
                    (Id, OpportunityId, VersionNumber, IsCurrent,
                     IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
                VALUES
                    ({0}, {1}, 2, 1, 0, 'Version 2', 'test')
                """,
                Guid.NewGuid(),
                opportunityId);

        var exception = await act.Should().ThrowAsync<SqlException>();

        exception.Which
            .Number.Should().Be(2601);
    }

    [Fact]
    public async Task OpportunityVersion_should_reject_multiple_published_snapshots()
    {
        await using var db = _fixture.CreateDbContext();

        var opportunityId = Guid.NewGuid();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Opportunity
                (Id, StatusCode, IsActive, CreatedBy)
            VALUES
                ({0}, 1, 1, 'test')
            """,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OpportunityVersion
                (Id, OpportunityId, VersionNumber, IsCurrent,
                 IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
            VALUES
                ({0}, {1}, 1, 1, 1, 'Version 1', 'test')
            """,
            Guid.NewGuid(),
            opportunityId);

        Func<Task> act = async () =>
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO dbo.OpportunityVersion
                    (Id, OpportunityId, VersionNumber, IsCurrent,
                     IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
                VALUES
                    ({0}, {1}, 2, 0, 1, 'Version 2', 'test')
                """,
                Guid.NewGuid(),
                opportunityId);

        var exception = await act.Should().ThrowAsync<SqlException>();

        exception.Which
            .Number.Should().Be(2601);
    }

    [Fact]
    public async Task Submission_should_reject_duplicate_sequence_for_version()
    {
        await using var db = _fixture.CreateDbContext();

        var opportunityId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Opportunity
                (Id, StatusCode, IsActive, CreatedBy)
            VALUES
                ({0}, 1, 1, 'test')
            """,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OpportunityVersion
                (Id, OpportunityId, VersionNumber, IsCurrent,
                 IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
            VALUES
                ({0}, {1}, 1, 1, 0, 'Version 1', 'test')
            """,
            versionId,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Submission
                (Id, OpportunityId, OpportunityVersionId, SequenceNumber,
                 SubmissionType, PreviousStatusCode, SubmittedBy)
            VALUES
                ({0}, {1}, {2}, 1, 1, 1, 'test')
            """,
            Guid.NewGuid(),
            opportunityId,
            versionId);

        Func<Task> act = async () =>
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO dbo.Submission
                    (Id, OpportunityId, OpportunityVersionId, SequenceNumber,
                     SubmissionType, PreviousStatusCode, SubmittedBy)
                VALUES
                    ({0}, {1}, {2}, 1, 1, 1, 'test')
                """,
                Guid.NewGuid(),
                opportunityId,
                versionId);

        var exception = await act.Should().ThrowAsync<SqlException>();

        exception.Which
            .Number.Should().Be(2627);
    }

    [Fact]
    public async Task Submission_should_reject_zero_sequence_number()
    {
        await using var db = _fixture.CreateDbContext();

        var opportunityId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Opportunity
                (Id, StatusCode, IsActive, CreatedBy)
            VALUES
                ({0}, 1, 1, 'test')
            """,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OpportunityVersion
                (Id, OpportunityId, VersionNumber, IsCurrent,
                 IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
            VALUES
                ({0}, {1}, 1, 1, 0, 'Version 1', 'test')
            """,
            versionId,
            opportunityId);

        Func<Task> act = async () =>
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO dbo.Submission
                    (Id, OpportunityId, OpportunityVersionId, SequenceNumber,
                     SubmissionType, PreviousStatusCode, SubmittedBy)
                VALUES
                    ({0}, {1}, {2}, 0, 1, 1, 'test')
                """,
                Guid.NewGuid(),
                opportunityId,
                versionId);

        var exception = await act.Should().ThrowAsync<SqlException>();

        exception.Which
            .Number.Should().Be(547);
    }

    [Fact]
    public async Task FinalRejection_should_reject_unknown_rejection_reason()
    {
        await using var db = _fixture.CreateDbContext();

        var opportunityId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Opportunity
                (Id, StatusCode, IsActive, CreatedBy)
            VALUES
                ({0}, 1, 1, 'test')
            """,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.OpportunityVersion
                (Id, OpportunityId, VersionNumber, IsCurrent,
                 IsPublishedSnapshot, OpportunityNameEn, CreatedBy)
            VALUES
                ({0}, {1}, 1, 1, 0, 'Version 1', 'test')
            """,
            versionId,
            opportunityId);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.Submission
                (Id, OpportunityId, OpportunityVersionId, SequenceNumber,
                 SubmissionType, PreviousStatusCode, SubmittedBy)
            VALUES
                ({0}, {1}, {2}, 1, 1, 1, 'test')
            """,
            submissionId,
            opportunityId,
            versionId);

        Func<Task> act = async () =>
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO dbo.FinalRejection
                    (Id, SubmissionId, RejectionReasonId, Comment, CreatedBy)
                VALUES
                    ({0}, {1}, 999999, 'Invalid reason', 'test')
                """,
                Guid.NewGuid(),
                submissionId);

        var exception = await act.Should().ThrowAsync<SqlException>();

        exception.Which
            .Number.Should().Be(547);
    }

}