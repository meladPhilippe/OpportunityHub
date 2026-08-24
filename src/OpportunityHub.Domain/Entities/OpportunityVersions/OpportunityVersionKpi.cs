using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Entities;

/// <summary>
/// Represents a KPI belonging to an opportunity version.
/// Its lifecycle is controlled by the opportunity version.
/// </summary>
public sealed class OpportunityVersionKpi : EntityIdentity
{
    private OpportunityVersionKpi(
        KpiContent content)
    {
        Title = content.Title;
        Value = content.Value;
        SortOrder = content.SortOrder;
    }

    private OpportunityVersionKpi()
    {
        
    }
    #region Properties

    public LocalizedText? Title { get; private set; }

    public LocalizedText? Value { get; private set; }

    public int SortOrder { get; private set; }

    #endregion

    #region Factory

    internal static OpportunityVersionKpi Create(
        KpiContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new OpportunityVersionKpi(content);
    }

    #endregion
}