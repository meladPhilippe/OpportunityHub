--liquibase formatted sql

--changeset melad:010-create-submission

CREATE TABLE dbo.Submission
(
    Id UNIQUEIDENTIFIER NOT NULL,

    OpportunityId UNIQUEIDENTIFIER NOT NULL,
    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,

    SequenceNumber BIGINT NOT NULL,

    SubmissionType INT NOT NULL,

    EditSummary NVARCHAR(2000) NULL,

    PreviousStatusCode INT NOT NULL,
    PreviousSubStatusCode INT NULL,

    SubmittedBy NVARCHAR(256) NOT NULL,

    SubmittedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_Submission_SubmittedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Submission
        PRIMARY KEY (Id),

    CONSTRAINT FK_Submission_Opportunity
        FOREIGN KEY (OpportunityId)
        REFERENCES dbo.Opportunity(Id),

    CONSTRAINT FK_Submission_OpportunityVersion
        FOREIGN KEY (OpportunityId, OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion( OpportunityId, Id),

    CONSTRAINT UQ_Submission_OpportunityVersion_SequenceNumber
        UNIQUE (OpportunityVersionId, SequenceNumber),

    CONSTRAINT CK_Submission_SequenceNumber
        CHECK (SequenceNumber > 0)
);

--rollback DROP TABLE dbo.Submission;