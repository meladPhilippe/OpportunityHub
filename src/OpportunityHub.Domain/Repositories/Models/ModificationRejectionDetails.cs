namespace OpportunityHub.Domain.Repositories.Models;

public sealed record ModificationRejectionDetails(
    Guid Id,
    Guid SubmissionId,
    string Comment,
    string CreatedBy,
    DateTime CreatedAtUtc);
