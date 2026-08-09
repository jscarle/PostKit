using LightResults;

namespace PostKit.Errors;

/// <summary>Represents a Postmark API request for which no conclusive HTTP response was received.</summary>
public sealed class PostmarkUnknownOutcomeError : Error
{
    internal PostmarkUnknownOutcomeError(HttpMethod method, string endpoint, Exception exception) : base(CreateMessage(method, endpoint), exception)
    {
        Method = method;
        Endpoint = endpoint;
    }

    /// <summary>Gets the HTTP method used for the request.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets the Postmark API endpoint targeted by the request.</summary>
    public string Endpoint { get; }

    /// <summary>Gets whether repeating the HTTP method is safe from duplicate effects.</summary>
    public bool IsRetrySafe => Method == HttpMethod.Get || Method == HttpMethod.Head || Method == HttpMethod.Options || Method == HttpMethod.Trace;

    private static string CreateMessage(HttpMethod method, string endpoint)
    {
        var retryGuidance = method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options || method == HttpMethod.Trace
            ? "The request used a safe HTTP method and can be retried without duplicate effects."
            : "Postmark may have processed the request. Do not retry it unless the application can prevent duplicate effects.";

        return $"PostKit did not receive a conclusive response for the '{method.Method} {endpoint}' request, so its outcome is unknown. {retryGuidance}";
    }
}
