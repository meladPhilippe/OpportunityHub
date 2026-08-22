--liquibase formatted sql

--changeset melad:001-create-opportunity

CREATE TABLE dbo.Opportunity
(
    Id UNIQUEIDENTIFIER NOT NULL,

    StatusCode INT NOT NULL,
    SubStatusCode INT NULL,

    QrCodeReference NVARCHAR(500) NULL,
    PublishedAtUtc DATETIME2(7) NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Opportunity_IsActive DEFAULT (1),

    LastSubmissionSequenceNumber BIGINT NOT NULL
        CONSTRAINT DF_Opportunity_LastSubmissionSequenceNumber DEFAULT (0),

    LastActivitySequenceNumber BIGINT NOT NULL
        CONSTRAINT DF_Opportunity_LastActivitySequenceNumber DEFAULT (0),

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_Opportunity_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    UpdatedAtUtc DATETIME2(7) NULL,
    UpdatedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Opportunity
        PRIMARY KEY (Id),

    CONSTRAINT CK_Opportunity_LastSubmissionSequenceNumber
        CHECK (LastSubmissionSequenceNumber >= 0),

    CONSTRAINT CK_Opportunity_LastActivitySequenceNumber
        CHECK (LastActivitySequenceNumber >= 0)
    );

CREATE INDEX IX_Opportunity_StatusCode
    ON dbo.Opportunity (StatusCode);

CREATE INDEX IX_Opportunity_IsActive
    ON dbo.Opportunity (IsActive);

--rollback DROP TABLE dbo.Opportunity;