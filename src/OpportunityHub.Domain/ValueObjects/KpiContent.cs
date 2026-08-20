using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Domain.ValueObjects;

/// <summary>
/// Represents the content required to create or update
/// a KPI within an opportunity version.
/// </summary>
public sealed class KpiContent
{
    #region Properties

    public LocalizedText? Title { get; init; }

    public LocalizedText? Value { get; init; }

    public int SortOrder { get; init; }

    #endregion

    #region Factory

    internal static KpiContent From(
        OpportunityVersionKpi kpi)
    {
        ArgumentNullException.ThrowIfNull(kpi);

        return new KpiContent
        {
            Title = kpi.Title,
            Value = kpi.Value,
            SortOrder = kpi.SortOrder
        };
    }

    #endregion
}