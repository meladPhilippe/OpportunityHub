--liquibase formatted sql

--changeset melad:001-seed-rejection-reason

INSERT INTO dbo.RejectionReason
(
    Id,
    Code,
    NameEn,
    NameAr,
    SortOrder,
    IsActive,
    CreatedBy
)
VALUES
(
    1,
    N'OPPORTUNITY_NOT_ELIGIBLE',
    N'Opportunity does not meet the required criteria',
    N'الفرصة لا تستوفي المعايير المطلوبة',
    1,
    1,
    N'liquibase'
);
