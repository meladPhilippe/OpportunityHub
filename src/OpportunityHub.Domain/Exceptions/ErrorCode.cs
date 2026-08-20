namespace OpportunityHub.Domain.Exceptions;

public static class ErrorCode
{
    public const string WorkflowTransitionNotAllowed =
        "OPPORTUNITY_WORKFLOW_TRANSITION_NOT_ALLOWED";

    public const string EditSummaryRequired =
        "OPPORTUNITY_EDIT_SUMMARY_REQUIRED";

    public const string AlreadySubmittedForManagerReview =
        "OPPORTUNITY_ALREADY_SUBMITTED_FOR_MANAGER_REVIEW";
}