using System.Net;
using System.Text.Json.Nodes;
using PostKit.Postmark.Email;

namespace PostKit.Errors;

/// <summary>
/// Represents an error returned by the Postmark API.
/// </summary>
public sealed class PostmarkError : HttpError
{
    /// <summary>
    /// The error code returned by the Postmark API.
    /// </summary>
    public PostmarkErrorCode ErrorCode { get; }

    /// <summary>
    /// Additional field-level error details returned by the Postmark API when available.
    /// </summary>
    public JsonNode? Errors { get; }

    internal PostmarkError(PostmarkResponse response)
        : this(HttpStatusCode.UnprocessableEntity, response)
    {
    }

    internal PostmarkError(HttpStatusCode statusCode, PostmarkResponse response)
        : this(statusCode, response.ErrorCode, response.Message, response.Errors)
    {
    }

    internal PostmarkError(int errorCode, string message)
        : this(HttpStatusCode.UnprocessableEntity, errorCode, message, errors: null)
    {
    }

    internal PostmarkError(HttpStatusCode statusCode, int errorCode, string message, JsonNode? errors)
        : base(statusCode, message)
    {
        ErrorCode = (PostmarkErrorCode)errorCode;
        Errors = errors?.DeepClone();
    }
}
