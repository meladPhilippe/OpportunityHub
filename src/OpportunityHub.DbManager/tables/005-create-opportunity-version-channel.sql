--liquibase formatted sql

--changeset melad:005-create-opportunity-version-channel

CREATE TABLE dbo.OpportunityVersionChannel
(
    OpportunityVersionId UNIQUEIDENTIFIER NOT NULL,
    ChannelId UNIQUEIDENTIFIER NOT NULL,

    CreatedAtUtc DATETIME2(7) NOT NULL
        CONSTRAINT DF_OpportunityVersionChannel_CreatedAtUtc
        DEFAULT (SYSUTCDATETIME()),

    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_OpportunityVersionChannel
        PRIMARY KEY (OpportunityVersionId, ChannelId),

    CONSTRAINT FK_OpportunityVersionChannel_OpportunityVersion
        FOREIGN KEY (OpportunityVersionId)
        REFERENCES dbo.OpportunityVersion(Id),

    CONSTRAINT FK_OpportunityVersionChannel_Channel
        FOREIGN KEY (ChannelId)
        REFERENCES dbo.Channel(Id)
);

--rollback DROP TABLE dbo.OpportunityVersionChannel;