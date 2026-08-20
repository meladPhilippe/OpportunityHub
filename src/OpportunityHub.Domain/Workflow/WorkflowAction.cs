namespace OpportunityHub.Domain.Workflow;

/// <summary>
/// Represents a business operation that can cause an opportunity
/// to move from one workflow state to another.
/// </summary>
public enum WorkflowAction
{
    SubmitForManagerReview,
    RequestModification,
    Reject,
    Approve,
    RejectModification,
    Publish,
    Unpublish
}