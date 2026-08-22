--liquibase formatted sql

--changeset melad:012-create-modification-request-item

CREATE TABLE dbo.ModificationRequestItem
(
    ModificationRequestId UNIQUEIDENTIFIER NOT NULL,

    FieldName NVARCHAR(200) NOT NULL,

    Comment NVARCHAR(4000) NOT NULL,


    CONSTRAINT PK_ModificationRequestItem_FieldName
        PRIMARY KEY (
            ModificationRequestId,
            FieldName
        ),

    CONSTRAINT FK_ModificationRequestItem_Request
        FOREIGN KEY (ModificationRequestId)
        REFERENCES dbo.ModificationRequest(Id)
);

--rollback DROP TABLE dbo.ModificationRequestItem;