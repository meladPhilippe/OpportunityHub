namespace OpportunityHub.Application.Opportunities.Queries.GetModificationRequest;

public sealed record ModificationRequestResponse(
    Guid Id,
    Guid SubmissionId,
    string CreatedBy,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<ModificationRequestItemResponse> Items);

public sealed record ModificationRequestItemResponse(
    string FieldName,
    string Comment);
