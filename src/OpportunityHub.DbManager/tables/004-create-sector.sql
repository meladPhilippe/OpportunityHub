--liquibase formatted sql

--changeset melad:004-create-sector

CREATE TABLE dbo.Sector
(
    Id UNIQUEIDENTIFIER NOT NULL,

    Code INT NOT NULL,

    NameEn NVARCHAR(500) NOT NULL,
    NameAr NVARCHAR(500) NULL,

    SortOrder INT NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Sector_IsActive DEFAULT (1),

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_Sector_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    UpdatedAtUtc DATETIME2(7) NULL,
    UpdatedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Sector
        PRIMARY KEY (Id),

    CONSTRAINT UQ_Sector_Code
        UNIQUE (Code),

    CONSTRAINT CK_Sector_Code
        CHECK (Code > 0),

    CONSTRAINT CK_Sector_SortOrder
        CHECK (SortOrder >= 0)
);

CREATE INDEX IX_Sector_IsActive_SortOrder
    ON dbo.Sector (IsActive, SortOrder);

--rollback DROP TABLE dbo.Sector;