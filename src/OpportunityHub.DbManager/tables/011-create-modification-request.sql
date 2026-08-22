--liquibase formatted sql

--changeset melad:011-create-modification-request

CREATE TABLE dbo.ModificationRequest
(
    Id UNIQUEIDENTIFIER NOT NULL,

    SubmissionId UNIQUEIDENTIFIER NOT NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_ModificationRequest_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_ModificationRequest
        PRIMARY KEY (Id),

    CONSTRAINT UQ_ModificationRequest_Submission
        UNIQUE (SubmissionId),

    CONSTRAINT FK_ModificationRequest_Submission
        FOREIGN KEY (SubmissionId)
        REFERENCES dbo.Submission(Id)
);

--rollback DROP TABLE dbo.ModificationRequest;