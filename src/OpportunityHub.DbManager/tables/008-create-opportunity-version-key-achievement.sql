--liquibase formatted sql

--changeset melad:008-create-opportunity-version-key-achievement

CREATE TABLE dbo.OpportunityVersionKeyAchievement
(
    Id UNIQUEIDENTIFIER NOT NULL,

    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,

    IconReference INT NULL,

    TitleEn NVARCHAR(1000) NULL,
    TitleAr NVARCHAR(1000) NULL,

    DescriptionEn NVARCHAR(MAX) NULL,
    DescriptionAr NVARCHAR(MAX) NULL,

    SortOrder INT NOT NULL,

    DisplayOnWebsite BIT NOT NULL
        CONSTRAINT DF_OpportunityVersionKeyAchievement_DisplayOnWebsite
        DEFAULT (1),

    CONSTRAINT PK_OpportunityVersionKeyAchievement
        PRIMARY KEY (Id),

    CONSTRAINT FK_OpportunityVersionKeyAchievement_OpportunityVersion
        FOREIGN KEY (OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion(Id),

    CONSTRAINT CK_OpportunityVersionKeyAchievement_SortOrder
        CHECK (SortOrder >= 0)
);

--rollback DROP TABLE dbo.OpportunityVersionKeyAchievement;