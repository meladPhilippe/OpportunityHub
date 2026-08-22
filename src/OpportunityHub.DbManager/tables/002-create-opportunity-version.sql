--liquibase formatted sql

--changeset melad:002-create-opportunity-version

CREATE TABLE dbo.OpportunityVersion
(
    Id UNIQUEIDENTIFIER NOT NULL,

    OpportunityId UNIQUEIDENTIFIER NOT NULL,

    VersionNumber INT NOT NULL,

    IsCurrent BIT NOT NULL
        CONSTRAINT DF_OpportunityVersion_IsCurrent DEFAULT (1),

    IsPublishedSnapshot BIT NOT NULL
        CONSTRAINT DF_OpportunityVersion_IsPublishedSnapshot DEFAULT (0),

    PublishedAtUtc DATETIME2(7) NULL,

    -- Opportunity name
    OpportunityNameEn NVARCHAR(1000) NOT NULL,
    OpportunityNameAr NVARCHAR(1000) NULL,

    -- National impact
    NationalImpactEn NVARCHAR(MAX) NULL,
    NationalImpactAr NVARCHAR(MAX) NULL,

    -- Description
    DescriptionEn NVARCHAR(MAX) NULL,
    DescriptionAr NVARCHAR(MAX) NULL,

    -- Website
    WebsiteUrlEn NVARCHAR(2000) NULL,
    WebsiteUrlAr NVARCHAR(2000) NULL,

    -- Logo
    LogoReferenceEn NVARCHAR(2000) NULL,
    LogoReferenceAr NVARCHAR(2000) NULL,

    -- Banner
    BannerReferenceEn NVARCHAR(2000) NULL,
    BannerReferenceAr NVARCHAR(2000) NULL,

    -- Company
    CompanyNameEn NVARCHAR(1000) NULL,
    CompanyNameAr NVARCHAR(1000) NULL,

    CompanyWebsiteUrlEn NVARCHAR(2000) NULL,
    CompanyWebsiteUrlAr NVARCHAR(2000) NULL,

    -- Adoption
    AdoptedByEn NVARCHAR(MAX) NULL,
    AdoptedByAr NVARCHAR(MAX) NULL,

    -- Beneficiaries
    BeneficiariesEn NVARCHAR(MAX) NULL,
    BeneficiariesAr NVARCHAR(MAX) NULL,

    KsaAdoptingEntitiesCount INT NULL,

    -- Opportunity owner
    OpportunityOwnerNameEn NVARCHAR(500) NULL,
    OpportunityOwnerNameAr NVARCHAR(500) NULL,

    OpportunityOwnerEmail NVARCHAR(320) NULL,
    OpportunityOwnerPhone NVARCHAR(100) NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_OpportunityVersion_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    UpdatedAtUtc DATETIME2(7) NULL,
    UpdatedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_OpportunityVersion
        PRIMARY KEY (Id),

    CONSTRAINT UQ_OpportunityVersion_Opportunity_Id
    UNIQUE (OpportunityId, Id),

    CONSTRAINT FK_OpportunityVersion_Opportunity
        FOREIGN KEY (OpportunityId)
        REFERENCES dbo.Opportunity(Id),

    CONSTRAINT UQ_OpportunityVersion_Opportunity_VersionNumber
        UNIQUE (OpportunityId, VersionNumber),

    CONSTRAINT CK_OpportunityVersion_VersionNumber
        CHECK (VersionNumber > 0)
);

--rollback DROP TABLE dbo.OpportunityVersion;