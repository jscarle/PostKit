using System.Net;
using LightResults;

namespace PostKit.Errors;

/// <summary>Represents a response from the Postmark API that does not match the documented response contract.</summary>
/// <remarks>Repeated occurrences most likely indicate a change in the Postmark API and should be reported to the PostKit repository.</remarks>
public sealed class PostmarkInvalidResponseError : Error
{
    internal PostmarkInvalidResponseError(HttpMethod method, string endpoint, string details, HttpStatusCode? statusCode = null) : base(CreateMessage(method, endpoint, details))
    {
        Method = method;
        Endpoint = endpoint;
        StatusCode = statusCode;
    }

    internal PostmarkInvalidResponseError(HttpMethod method, string endpoint, string details, HttpStatusCode? statusCode, Exception exception) : base(CreateMessage(method, endpoint, details), exception)
    {
        Method = method;
        Endpoint = endpoint;
        StatusCode = statusCode;
    }

    /// <summary>Gets the HTTP method used for the request.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets the Postmark API endpoint targeted by the request.</summary>
    public string Endpoint { get; }

    /// <summary>Gets the HTTP status code returned with the invalid response when it is available.</summary>
    public HttpStatusCode? StatusCode { get; }

    private static string CreateMessage(HttpMethod method, string endpoint, string details)
    {
        return $"Postmark returned an invalid response for the '{method.Method} {endpoint}' request. {details} " +
               "If this error occurs repeatedly, it most likely indicates a change in the Postmark API. " +
               "Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).";
    }
}
