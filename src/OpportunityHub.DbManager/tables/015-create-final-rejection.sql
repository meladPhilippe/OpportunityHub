--liquibase formatted sql

--changeset melad:014-create-final-rejection

CREATE TABLE dbo.FinalRejection
(
    Id UNIQUEIDENTIFIER NOT NULL,

    SubmissionId UNIQUEIDENTIFIER NOT NULL,

    RejectionReasonId UNIQUEIDENTIFIER NOT NULL,

    Comment NVARCHAR(4000) NOT NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_FinalRejection_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_FinalRejection
        PRIMARY KEY (Id),

    CONSTRAINT UQ_FinalRejection_Submission
        UNIQUE (SubmissionId),

    CONSTRAINT FK_FinalRejection_Submission
        FOREIGN KEY (SubmissionId)
        REFERENCES dbo.Submission(Id),

    CONSTRAINT FK_FinalRejection_RejectionReason
        FOREIGN KEY (RejectionReasonId)
        REFERENCES dbo.RejectionReason(Id)
);

--rollback DROP TABLE dbo.FinalRejection;