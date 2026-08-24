using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeSubmissionRepository
    : ISubmissionRepository
{
    private readonly List<SubmissionDetails> _submissions = [];

    public CancellationToken LastCancellationToken { get; private set; }

    public void Add(SubmissionDetails submission)
    {
        _submissions.Add(submission);
    }

    public Task<SubmissionDetails?> GetByIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;

        SubmissionDetails? result =
            _submissions
                .SingleOrDefault(x => x.Id == submissionId);

        return Task.FromResult(result);
    }
}
