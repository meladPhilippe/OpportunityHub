using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Workflow;

/// <summary>
/// Identifies a workflow decision by combining the current
/// opportunity state with the requested business action.
/// </summary>
public readonly record struct WorkflowKey(
    OpportunityStatusCode StatusCode,
    OpportunitySubStatusCode? SubStatusCode,
    WorkflowAction Action);