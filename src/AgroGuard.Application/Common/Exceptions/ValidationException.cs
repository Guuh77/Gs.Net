namespace AgroGuard.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            ["Request"] = [message]
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }

    public static ValidationException For(string field, string message)
    {
        return new ValidationException(new Dictionary<string, string[]>
        {
            [field] = [message]
        });
    }
}
