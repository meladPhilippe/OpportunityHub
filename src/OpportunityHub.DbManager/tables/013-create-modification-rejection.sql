--liquibase formatted sql

--changeset melad:013-create-modification-rejection

CREATE TABLE dbo.ModificationRejection
(
    Id UNIQUEIDENTIFIER NOT NULL,

    SubmissionId UNIQUEIDENTIFIER NOT NULL,

    Comment NVARCHAR(4000) NOT NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_ModificationRejection_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_ModificationRejection
        PRIMARY KEY (Id),

    CONSTRAINT UQ_ModificationRejection_Submission
        UNIQUE (SubmissionId),

    CONSTRAINT FK_ModificationRejection_Submission
        FOREIGN KEY (SubmissionId)
        REFERENCES dbo.Submission(Id)
);

--rollback DROP TABLE dbo.ModificationRejection;