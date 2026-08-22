--liquibase formatted sql

--changeset melad:009-create-opportunity-version-kpi

CREATE TABLE dbo.OpportunityVersionKpi
(
    Id UNIQUEIDENTIFIER NOT NULL,

    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,

    TitleEn NVARCHAR(1000) NULL,
    TitleAr NVARCHAR(1000) NULL,

    ValueEn NVARCHAR(1000) NULL,
    ValueAr NVARCHAR(1000) NULL,

    SortOrder INT NOT NULL,

    CONSTRAINT PK_OpportunityVersionKpi
        PRIMARY KEY (Id),

    CONSTRAINT FK_OpportunityVersionKpi_OpportunityVersion
        FOREIGN KEY (OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion(Id),

    CONSTRAINT CK_OpportunityVersionKpi_SortOrder
        CHECK (SortOrder >= 0)
);

--rollback DROP TABLE dbo.OpportunityVersionKpi;