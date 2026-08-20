
namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a manager's request for changes to a submitted opportunity. 
/// It contains one or more requested modifications and is created and controlled by the Submission.
/// </summary>
public sealed class ModificationRequest : CreationTrackedEntity
{
    private readonly List<ModificationRequestItem> _items = new();

    internal ModificationRequest(
        string requestedBy,
        DateTime? requestedAtUtc = null)
        : base(requestedBy, requestedAtUtc)
    {
    }

    #region Properties

    public IReadOnlyCollection<ModificationRequestItem> Items =>
        _items.AsReadOnly();

    #endregion

    #region Items

    internal void AddItem(
        string fieldName,
        string comment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        if (_items.Any(x =>
                string.Equals(
                    x.FieldName.Trim(),
                    fieldName.Trim(),
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"A modification request already exists for field '{fieldName}'.");
        }

        _items.Add(
            new ModificationRequestItem(
                fieldName.Trim(),
                comment.Trim()));
    }

    #endregion
}