--liquibase formatted sql

--changeset melad:016-create-audit-history

CREATE TABLE dbo.AuditHistory
(
    OpportunityId UNIQUEIDENTIFIER NOT NULL,

    ActivitySequenceNumber BIGINT NOT NULL,

    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,

    SubmissionId UNIQUEIDENTIFIER NULL,

    ActivityType INT NOT NULL,

    RelatedEntityType NVARCHAR(200) NULL,

    RelatedEntityId UNIQUEIDENTIFIER NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_AuditHistory_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_AuditHistory
        PRIMARY KEY (OpportunityId, ActivitySequenceNumber),

    CONSTRAINT FK_AuditHistory_Opportunity
        FOREIGN KEY (OpportunityId)
        REFERENCES dbo.Opportunity(Id),

    CONSTRAINT FK_AuditHistory_OpportunityVersion
        FOREIGN KEY (OpportunityId, OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion(OpportunityId, Id),

    CONSTRAINT FK_AuditHistory_Submission
        FOREIGN KEY (SubmissionId)
        REFERENCES dbo.Submission(Id),

    CONSTRAINT CK_AuditHistory_ActivitySequenceNumber
        CHECK (ActivitySequenceNumber > 0)
);

--rollback DROP TABLE dbo.AuditHistory;