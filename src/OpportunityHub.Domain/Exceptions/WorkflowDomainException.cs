namespace OpportunityHub.Domain.Exceptions;
public sealed class WorkflowDomainException : Exception
{
    public WorkflowDomainException(string message, string errorCode = null!) : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}