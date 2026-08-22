--liquibase formatted sql

--changeset melad:006-create-opportunity-version-sector

CREATE TABLE dbo.OpportunityVersionSector
(
    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,
    SectorId UNIQUEIDENTIFIER NOT NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_OpportunityVersionSector_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_OpportunityVersionSector
        PRIMARY KEY (OpportunityVersionId, SectorId),

    CONSTRAINT FK_OpportunityVersionSector_OpportunityVersion
        FOREIGN KEY (OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion(Id),

    CONSTRAINT FK_OpportunityVersionSector_Sector
        FOREIGN KEY (SectorId)
        REFERENCES dbo.Sector(Id)
);

--rollback DROP TABLE dbo.OpportunityVersionSector;