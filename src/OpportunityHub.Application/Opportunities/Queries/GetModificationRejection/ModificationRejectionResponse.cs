namespace OpportunityHub.Application.Opportunities.Queries.GetModificationRejection;

public sealed record ModificationRejectionResponse(
    Guid Id,
    Guid SubmissionId,
    string Comment,
    string CreatedBy,
    DateTime CreatedAtUtc);
