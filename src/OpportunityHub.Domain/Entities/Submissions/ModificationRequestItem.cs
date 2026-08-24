namespace OpportunityHub.Domain.Entities;

public sealed class ModificationRequestItem : DomainObject
{
    internal ModificationRequestItem(
        string fieldName,
        string comment)
    {
        FieldName = fieldName;
        Comment = comment;
    }

    private ModificationRequestItem()
    {
        
    }

    #region Properties

    public string FieldName { get; private set; } = string.Empty;

    public string Comment { get; private set; } = string.Empty;

    #endregion
}