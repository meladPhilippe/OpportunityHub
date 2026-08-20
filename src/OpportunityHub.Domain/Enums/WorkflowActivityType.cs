namespace OpportunityHub.Domain.Enums;
public enum WorkflowActivityType
{
    DraftCreated = 1,
    SubmittedForManagerReview = 2,
    ModificationRequested = 3,
    SpecialistModificationSubmitted = 4,
    Approved = 5,
    ModificationRejected = 6,
    OpportunityRejected = 7,
    Published = 8,
    Unpublished = 9,
    PublishedUnderReview = 10
}
