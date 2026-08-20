namespace OpportunityHub.Domain.Enums;
public enum OpportunityStatusCode
{
    Draft = 1,
    PendingManagerReview = 2,
    PendingSpecialistModification = 3,
    Approved = 4,
    Published = 5,
    Unpublished = 6,
    Rejected = 7,
    PublishedUnderReview = 8
}
