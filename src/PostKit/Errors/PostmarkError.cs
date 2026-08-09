using System.Net;
using System.Text.Json.Nodes;
using PostKit.Postmark.Email;

namespace PostKit.Errors;

/// <summary>Represents an error returned by the Postmark API.</summary>
public sealed class PostmarkError : HttpError
{
    internal PostmarkError(PostmarkResponse response) : this(HttpStatusCode.UnprocessableEntity, response)
    {
    }

    internal PostmarkError(HttpStatusCode statusCode, PostmarkResponse response, TimeSpan? retryAfter = null) : this(statusCode, response.ErrorCode, response.Message, response.Errors, retryAfter)
    {
    }

    internal PostmarkError(int errorCode, string message) : this(HttpStatusCode.UnprocessableEntity, errorCode, message, null)
    {
    }

    internal PostmarkError(HttpStatusCode statusCode, int errorCode, string message, JsonNode? errors, TimeSpan? retryAfter = null) : base(statusCode, message, retryAfter)
    {
        ErrorCode = (PostmarkErrorCode)errorCode;
        Errors = errors?.DeepClone();
    }

    /// <summary>The error code returned by the Postmark API.</summary>
    public PostmarkErrorCode ErrorCode { get; }

    /// <summary>Additional field-level error details returned by the Postmark API when available.</summary>
    public JsonNode? Errors { get; }
}
