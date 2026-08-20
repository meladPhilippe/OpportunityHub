using OpportunityHub.Domain.Enums;
using OpportunityHub.Domain.Exceptions;

namespace OpportunityHub.Domain.Workflow;

/// <summary>
/// Defines the valid workflow transitions for an opportunity.
///
/// The workflow definition contains rules only. It does not modify
/// opportunity state or own workflow data.
///
/// The Opportunity aggregate remains responsible for applying
/// transitions and enforcing rules that depend on aggregate data.
/// </summary>
public static class WorkflowDefinition
{
    private static readonly IReadOnlyDictionary<
        WorkflowKey,
        WorkflowTransition> Transitions =
        new Dictionary<WorkflowKey, WorkflowTransition>
        {
            // =========================================================
            // FIRST PUBLICATION
            // =========================================================

            // Draft
            //   → Pending Manager Review
            [new(
                OpportunityStatusCode.Draft,
                null,
                WorkflowAction.SubmitForManagerReview)]
                = new(
                    OpportunityStatusCode.PendingManagerReview,
                    null),

            // Manager requests changes to the first-publication draft.
            //
            // Pending Manager Review
            //   → Pending Specialist Modification
            [new(
                OpportunityStatusCode.PendingManagerReview,
                null,
                WorkflowAction.RequestModification)]
                = new(
                    OpportunityStatusCode.PendingSpecialistModification,
                    null),

            // Specialist submits the requested changes.
            //
            // Pending Specialist Modification
            //   → Pending Manager Review
            [new(
                OpportunityStatusCode.PendingSpecialistModification,
                null,
                WorkflowAction.SubmitForManagerReview)]
                = new(
                    OpportunityStatusCode.PendingManagerReview,
                    null),

            // Manager approves the first publication.
            //
            // Pending Manager Review
            //   → Approved
            [new(
                OpportunityStatusCode.PendingManagerReview,
                null,
                WorkflowAction.Approve)]
                = new(
                    OpportunityStatusCode.Approved,
                    null),

            // Manager permanently rejects the opportunity.
            //
            // This is a FINAL rejection during the first-publication
            // workflow.
            //
            // Pending Manager Review
            //   → Rejected
            [new(
                OpportunityStatusCode.PendingManagerReview,
                null,
                WorkflowAction.Reject)]
                = new(
                    OpportunityStatusCode.Rejected,
                    null),

            // Publish the approved first publication.
            //
            // Approved
            //   → Published
            [new(
                OpportunityStatusCode.Approved,
                null,
                WorkflowAction.Publish)]
                = new(
                    OpportunityStatusCode.Published,
                    null),


            // =========================================================
            // PUBLISHED OPPORTUNITY — START MODIFICATION
            // =========================================================

            // A published opportunity enters the modification review
            // workflow.
            //
            // Published
            //   → Published Under Review
            //   + Pending Manager Review
            [new(
                OpportunityStatusCode.Published,
                null,
                WorkflowAction.SubmitForManagerReview)]
                = new(
                    OpportunityStatusCode.PublishedUnderReview,
                    OpportunitySubStatusCode.PendingManagerReview),

            // A previously modified published opportunity can enter
            // another modification cycle.
            //
            // Published
            // + PublishedModified
            //   → Published Under Review
            //   + Pending Manager Review
            [new(
                OpportunityStatusCode.Published,
                OpportunitySubStatusCode.PublishedModified,
                WorkflowAction.SubmitForManagerReview)]
                = new(
                    OpportunityStatusCode.PublishedUnderReview,
                    OpportunitySubStatusCode.PendingManagerReview),


            // =========================================================
            // PUBLISHED MODIFICATION — MANAGER REVIEW
            // =========================================================

            // Manager requests changes to the published modification.
            //
            // Published Under Review
            // + Pending Manager Review
            //   → Published Under Review
            //   + Pending Specialist Modification
            [new(
                OpportunityStatusCode.PublishedUnderReview,
                OpportunitySubStatusCode.PendingManagerReview,
                WorkflowAction.RequestModification)]
                = new(
                    OpportunityStatusCode.PublishedUnderReview,
                    OpportunitySubStatusCode.PendingSpecialistModification),

            // Specialist submits the revised modification.
            //
            // Published Under Review
            // + Pending Specialist Modification
            //   → Published Under Review
            //   + Pending Manager Review
            [new(
                OpportunityStatusCode.PublishedUnderReview,
                OpportunitySubStatusCode.PendingSpecialistModification,
                WorkflowAction.SubmitForManagerReview)]
                = new(
                    OpportunityStatusCode.PublishedUnderReview,
                    OpportunitySubStatusCode.PendingManagerReview),

            // Manager approves the published modification.
            //
            // The modification is approved but NOT published yet.
            //
            // Published Under Review
            // + Pending Manager Review
            //   → Published Under Review
            //   + Approved
            [new(
                OpportunityStatusCode.PublishedUnderReview,
                OpportunitySubStatusCode.PendingManagerReview,
                WorkflowAction.Approve)]
                = new(
                    OpportunityStatusCode.PublishedUnderReview,
                    OpportunitySubStatusCode.Approved),

            // Manager rejects the published modification.
            //
            // This is NOT a final opportunity rejection.
            //
            // The aggregate restores the state stored in:
            //
            // Submission.PreviousStatusCode
            // Submission.PreviousSubStatusCode
            //
            // Therefore the transition target here is only a placeholder
            // for the transition definition. The aggregate restores the
            // actual previous state.
            [new(
                OpportunityStatusCode.PublishedUnderReview,
                OpportunitySubStatusCode.PendingManagerReview,
                WorkflowAction.RejectModification)]
                = new(
                    OpportunityStatusCode.PublishedUnderReview,
                    OpportunitySubStatusCode.PendingManagerReview),


            // =========================================================
            // PUBLISHED MODIFICATION — PUBLICATION
            // =========================================================

            // Publish an approved published modification.
            //
            // Published Under Review
            // + Approved
            //   → Published
            //   + PublishedModified
            [new(
                OpportunityStatusCode.PublishedUnderReview,
                OpportunitySubStatusCode.Approved,
                WorkflowAction.Publish)]
                = new(
                    OpportunityStatusCode.Published,
                    OpportunitySubStatusCode.PublishedModified),


            // =========================================================
            // UNPUBLICATION
            // =========================================================

            // Unpublish a normally published opportunity.
            //
            // Published
            // + null
            //   → Unpublished
            [new(
                OpportunityStatusCode.Published,
                null,
                WorkflowAction.Unpublish)]
                = new(
                    OpportunityStatusCode.Unpublished,
                    null),

            // Unpublish an opportunity whose current published version
            // is the result of an approved modification.
            //
            // Published
            // + PublishedModified
            //   → Unpublished
            [new(
                OpportunityStatusCode.Published,
                OpportunitySubStatusCode.PublishedModified,
                WorkflowAction.Unpublish)]
                = new(
                    OpportunityStatusCode.Unpublished,
                    null)
        };

    /// <summary>
    /// Returns the transition associated with the requested workflow action.
    /// </summary>
    public static WorkflowTransition GetTransition(
        WorkflowKey key)
    {
        if (Transitions.TryGetValue(key, out var transition))
        {
            return transition;
        }

        throw new WorkflowTransitionNotAllowedException(key);
    }

    /// <summary>
    /// Determines whether the requested workflow action is allowed
    /// from the supplied state.
    /// </summary>
    public static bool IsAllowed(
        WorkflowKey key)
    {
        return Transitions.ContainsKey(key);
    }

    /// <summary>
    /// Attempts to resolve a workflow transition without throwing.
    /// </summary>
    public static bool TryGetTransition(
        WorkflowKey key,
        out WorkflowTransition transition)
    {
        return Transitions.TryGetValue(
            key,
            out transition!);
    }
}