namespace OpportunityHub.Domain.Entities;

public sealed class RejectionReason : EntityIdentity
{
    private RejectionReason()
    {
    }

    public RejectionReason(
        string code,
        string name,
        int displayOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder));
        }

        Code = code;
        Name = name;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    #region Properties

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public int DisplayOrder { get; private set; }

    #endregion

    #region State

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    #endregion
}