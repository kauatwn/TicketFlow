namespace TicketFlow.Domain.Exceptions;

public sealed class ValidationException : Exception
{
    public const string DefaultErrorMessage = "Validation failed";

    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors) : base(DefaultErrorMessage)
    {
        Errors = errors;
    }

    public ValidationException(string field, string message) : base(DefaultErrorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, [message] }
        };
    }
}