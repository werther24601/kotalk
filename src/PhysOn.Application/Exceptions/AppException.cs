using System.Net;

namespace PhysOn.Application.Exceptions;

public sealed class AppException : Exception
{
    public AppException(
        string code,
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        bool retryable = false,
        IReadOnlyDictionary<string, string>? fieldErrors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Retryable = retryable;
        FieldErrors = fieldErrors;
    }

    public string Code { get; }
    public HttpStatusCode StatusCode { get; }
    public bool Retryable { get; }
    public IReadOnlyDictionary<string, string>? FieldErrors { get; }
}
