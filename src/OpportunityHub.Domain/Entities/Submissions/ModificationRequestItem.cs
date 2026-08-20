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

    #region Properties

    public string FieldName { get; private set; }

    public string Comment { get; private set; }

    #endregion
}