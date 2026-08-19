using OpportunityHub.Domain;

public class Rejection : CreationTrackedEntity
{
    public Guid SubmissionId { get; set; }
    public int RejectionReasonId { get; set; }
    public string Comment { get; set; } = string.Empty;

}