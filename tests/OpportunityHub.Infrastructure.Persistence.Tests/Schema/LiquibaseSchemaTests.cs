using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace OpportunityHub.Infrastructure.Persistence.Tests.Schema;

[Collection("Infrastructure Integration Tests")]
public sealed class LiquibaseSchemaTests
{
    private readonly OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture _fixture;

    public LiquibaseSchemaTests(
        OpportunityHub.Infrastructure.Persistence.Tests.Infrastructure.SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Liquibase_should_create_expected_tables()
    {
        await using var db = _fixture.CreateDbContext();

        var tables = await db.Database
            .SqlQuery<string>($"""
                SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS [Value]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                  AND TABLE_SCHEMA = 'dbo'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        var expectedTables = new[]
        {
            "dbo.Opportunity",
            "dbo.OpportunityVersion",
            "dbo.Channel",
            "dbo.Sector",
            "dbo.OpportunityVersionChannel",
            "dbo.OpportunityVersionSector",
            "dbo.OpportunityVersionFeature",
            "dbo.OpportunityVersionKeyAchievement",
            "dbo.OpportunityVersionKpi",
            "dbo.Submission",
            "dbo.ModificationRequest",
            "dbo.ModificationRequestItem",
            "dbo.ModificationRejection",
            "dbo.RejectionReason",
            "dbo.FinalRejection",
            "dbo.AuditHistory"
        };

        tables.Should().Contain(expectedTables);
    }

    [Fact]
    public async Task Liquibase_should_create_expected_unique_constraints_and_indexes()
    {
        await using var db = _fixture.CreateDbContext();

        var indexes = await db.Database
            .SqlQuery<string>($"""
                SELECT i.name AS [Value]
                FROM sys.indexes i
                INNER JOIN sys.tables t
                    ON t.object_id = i.object_id
                INNER JOIN sys.schemas s
                    ON s.schema_id = t.schema_id
                WHERE s.name = 'dbo'
                  AND i.name IS NOT NULL
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        var expectedIndexes = new[]
        {
            "UX_OpportunityVersion_Current",
            "UX_OpportunityVersion_Published",

            "UQ_OpportunityVersion_Opportunity_Id",
            "UQ_OpportunityVersion_Opportunity_VersionNumber",

            "UQ_Submission_OpportunityVersion_SequenceNumber",

            "UQ_ModificationRequest_Submission",
            "UQ_ModificationRejection_Submission",
            "UQ_FinalRejection_Submission",

            "UQ_Channel_Code",
            "UQ_Sector_Code",
            "UQ_RejectionReason_Code"
        };

        indexes.Should().Contain(expectedIndexes);
    }

    [Fact]
    public async Task Liquibase_should_create_expected_foreign_keys()
    {
        await using var db = _fixture.CreateDbContext();

        var foreignKeys = await db.Database
            .SqlQuery<string>($"""
                SELECT fk.name AS [Value]
                FROM sys.foreign_keys fk
                INNER JOIN sys.tables t
                    ON t.object_id = fk.parent_object_id
                INNER JOIN sys.schemas s
                    ON s.schema_id = t.schema_id
                WHERE s.name = 'dbo'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        var expectedForeignKeys = new[]
        {
            "FK_OpportunityVersion_Opportunity",

            "FK_OpportunityVersionChannel_OpportunityVersion",
            "FK_OpportunityVersionChannel_Channel",

            "FK_OpportunityVersionSector_OpportunityVersion",
            "FK_OpportunityVersionSector_Sector",

            "FK_Submission_Opportunity",
            "FK_Submission_OpportunityVersion",

            "FK_ModificationRequest_Submission",
            "FK_ModificationRequestItem_Request",

            "FK_ModificationRejection_Submission",

            "FK_FinalRejection_Submission",
            "FK_FinalRejection_RejectionReason",

            "FK_AuditHistory_Opportunity",
            "FK_AuditHistory_OpportunityVersion",
            "FK_AuditHistory_Submission"
        };

        foreignKeys.Should().Contain(expectedForeignKeys);
    }

    [Fact]
    public async Task Liquibase_should_create_expected_check_constraints()
    {
        await using var db = _fixture.CreateDbContext();

        var constraints = await db.Database
            .SqlQuery<string>($"""
                SELECT cc.name AS [Value]
                FROM sys.check_constraints cc
                INNER JOIN sys.tables t
                    ON t.object_id = cc.parent_object_id
                INNER JOIN sys.schemas s
                    ON s.schema_id = t.schema_id
                WHERE s.name = 'dbo'
                """)
            .ToListAsync(TestContext.Current.CancellationToken);

        var expectedConstraints = new[]
        {
            "CK_Opportunity_LastSubmissionSequenceNumber",
            "CK_Opportunity_LastActivitySequenceNumber",

            "CK_OpportunityVersion_VersionNumber",

            "CK_Channel_Code",
            "CK_Channel_SortOrder",

            "CK_Sector_Code",
            "CK_Sector_SortOrder",

            "CK_OpportunityVersionFeature_SortOrder",
            "CK_OpportunityVersionKeyAchievement_SortOrder",
            "CK_OpportunityVersionKpi_SortOrder",

            "CK_Submission_SequenceNumber",

            "CK_AuditHistory_ActivitySequenceNumber",
            "CK_AuditHistory_RelatedEntityType",
            "CK_AuditHistory_RelatedEntityReference"
        };

        constraints.Should().Contain(expectedConstraints);
    }
}
