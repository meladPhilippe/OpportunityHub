namespace OpportunityHub.Domain.Repositories.Models;

public sealed record ModificationRequestDetails(
    Guid Id,
    Guid SubmissionId,
    string CreatedBy,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<ModificationRequestItemDetails> Items);
