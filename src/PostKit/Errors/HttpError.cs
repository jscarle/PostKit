using System.Net;
using LightResults;

namespace PostKit.Errors;

/// <summary>Represents an error returned by the HTTP client.</summary>
public class HttpError : Error
{
    internal HttpError(HttpStatusCode httpStatusCode, TimeSpan? retryAfter = null)
    {
        StatusCode = httpStatusCode;
        RetryAfter = retryAfter;
    }

    internal HttpError(HttpStatusCode httpStatusCode, string message, TimeSpan? retryAfter = null) : base(message)
    {
        StatusCode = httpStatusCode;
        RetryAfter = retryAfter;
    }

    internal HttpError(HttpStatusCode httpStatusCode, string message, IReadOnlyDictionary<string, object?> metadata, TimeSpan? retryAfter = null) : base(message, metadata)
    {
        StatusCode = httpStatusCode;
        RetryAfter = retryAfter;
    }

    /// <summary>The HTTP status code returned by the server.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Gets the server-requested delay before another request should be attempted, when one was provided.</summary>
    public TimeSpan? RetryAfter { get; }
}
