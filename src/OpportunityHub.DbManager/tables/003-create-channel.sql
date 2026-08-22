--liquibase formatted sql

--changeset melad:003-create-channel

CREATE TABLE dbo.Channel
(
    Id UNIQUEIDENTIFIER NOT NULL,

    Code INT NOT NULL,

    NameEn NVARCHAR(500) NOT NULL,
    NameAr NVARCHAR(500) NULL,

    SortOrder INT NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Channel_IsActive DEFAULT (1),

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_Channel_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    UpdatedAtUtc DATETIME2(7) NULL,
    UpdatedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Channel
        PRIMARY KEY (Id),

    CONSTRAINT UQ_Channel_Code
        UNIQUE (Code),

    CONSTRAINT CK_Channel_Code
        CHECK (Code > 0),

    CONSTRAINT CK_Channel_SortOrder
        CHECK (SortOrder >= 0)
);

CREATE INDEX IX_Channel_IsActive_SortOrder
    ON dbo.Channel (IsActive, SortOrder);

--rollback DROP TABLE dbo.Channel;