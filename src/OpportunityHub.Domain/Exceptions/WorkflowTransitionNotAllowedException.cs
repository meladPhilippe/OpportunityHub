using OpportunityHub.Domain.Workflow;

namespace OpportunityHub.Domain.Exceptions;
public sealed class WorkflowTransitionNotAllowedException : DomainException
{
    public WorkflowTransitionNotAllowedException(
        WorkflowKey key)
        : base(
            $"Workflow action '{key.Action}' is not allowed " +
            $"from status '{key.StatusCode}' " +
            $"and sub-status '{key.SubStatusCode}'.")
    {
        Key = key;
    }
    public WorkflowKey Key { get; }
}
