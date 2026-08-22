--liquibase formatted sql

--changeset melad:015-create-rejection-reason

CREATE TABLE dbo.RejectionReason
(
    Id UNIQUEIDENTIFIER NOT NULL,

    Code NVARCHAR(100) NOT NULL,

    NameEn NVARCHAR(500) NOT NULL,
    NameAr NVARCHAR(500) NULL,

    SortOrder INT NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_RejectionReason_IsActive
        DEFAULT (1),

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_RejectionReason_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    UpdatedAtUtc DATETIME2(7) NULL,
    UpdatedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_RejectionReason
        PRIMARY KEY (Id),

    CONSTRAINT UQ_RejectionReason_Code
        UNIQUE (Code),

    CONSTRAINT CK_RejectionReason_SortOrder
        CHECK (SortOrder >= 0)
);

--rollback DROP TABLE dbo.RejectionReason;