namespace OpportunityHub.Domain.ValueObjects;

public sealed class LocalizedText
{
    public string En { get; private set; } = string.Empty;
    public string? Ar { get; private set; }

    private LocalizedText()
    {
    }

    public LocalizedText(string en, string? ar = null)
    {
         ArgumentException.ThrowIfNullOrWhiteSpace(en);
        En = en;
        Ar = ar;
    }

    public void Set(string en, string? ar)
    {
        En = en;
        Ar = ar;
    }
}