using LightResults;

namespace PostKit.Errors;

/// <summary>Represents an unsafe Postmark API request that may have succeeded even though its outcome could not be confirmed.</summary>
public sealed class PostmarkIndeterminateError : Error
{
    internal PostmarkIndeterminateError(HttpMethod method, string endpoint, string details) : base(CreateMessage(method, endpoint, details))
    {
        Method = method;
        Endpoint = endpoint;
    }

    internal PostmarkIndeterminateError(HttpMethod method, string endpoint, string details, Exception exception) : base(CreateMessage(method, endpoint, details), exception)
    {
        Method = method;
        Endpoint = endpoint;
    }

    /// <summary>Gets the HTTP method used for the request.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets the Postmark API endpoint targeted by the request.</summary>
    public string Endpoint { get; }

    private static string CreateMessage(HttpMethod method, string endpoint, string details)
    {
        return $"Postmark may have accepted the '{method.Method} {endpoint}' request, but PostKit could not confirm the outcome. Do not retry it unless the application can prevent duplicate effects. {details}";
    }
}
