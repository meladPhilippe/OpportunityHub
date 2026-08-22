--liquibase formatted sql

--changeset melad:007-create-opportunity-version-feature

CREATE TABLE dbo.OpportunityVersionFeature
(
    Id UNIQUEIDENTIFIER NOT NULL,

    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,

    IconReference INT NULL,

    TitleEn NVARCHAR(1000) NULL,
    TitleAr NVARCHAR(1000) NULL,

    SortOrder INT NOT NULL,

    DisplayOnWebsite BIT NOT NULL
        CONSTRAINT DF_OpportunityVersionFeature_DisplayOnWebsite
        DEFAULT (1),

    CONSTRAINT PK_OpportunityVersionFeature
        PRIMARY KEY (Id),

    CONSTRAINT FK_OpportunityVersionFeature_OpportunityVersion
        FOREIGN KEY (OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion(Id),

    CONSTRAINT CK_OpportunityVersionFeature_SortOrder
        CHECK (SortOrder >= 0)
);

--rollback DROP TABLE dbo.OpportunityVersionFeature;