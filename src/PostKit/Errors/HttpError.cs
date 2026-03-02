using System.Net;
using LightResults;

namespace PostKit.Errors;

/// <summary>Represents an error returned by the HTTP client.</summary>
public class HttpError : Error
{
    /// <summary>The HTTP status code returned by the server.</summary>
    public HttpStatusCode StatusCode { get; }

    internal HttpError(HttpStatusCode httpStatusCode)
    {
        StatusCode = httpStatusCode;
    }

    internal HttpError(HttpStatusCode httpStatusCode, string message)
        : base(message)
    {
        StatusCode = httpStatusCode;
    }

    internal HttpError(HttpStatusCode httpStatusCode, string message, IReadOnlyDictionary<string, object?> metadata)
        : base(message, metadata)
    {
        StatusCode = httpStatusCode;
    }
}
