using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Workflow;

/// <summary>
/// Describes the workflow state that an opportunity enters
/// after a successful workflow action.
/// </summary>
public sealed record WorkflowTransition(
    OpportunityStatusCode StatusCode,
    OpportunitySubStatusCode? SubStatusCode);