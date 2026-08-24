namespace OpportunityHub.Application.Opportunities.Queries.GetFinalRejection;

public sealed record FinalRejectionResponse(
    Guid Id,
    Guid SubmissionId,
    int RejectionReasonId,
    string Comment,
    string CreatedBy,
    DateTime CreatedAtUtc);
