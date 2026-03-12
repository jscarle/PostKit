using System.Net;
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

    internal PostmarkError(PostmarkResponse response)
        : this(HttpStatusCode.UnprocessableEntity, response)
    {
    }

    internal PostmarkError(HttpStatusCode statusCode, PostmarkResponse response)
        : this(statusCode, response.ErrorCode, response.Message)
    {
    }

    internal PostmarkError(int errorCode, string message)
        : this(HttpStatusCode.UnprocessableEntity, errorCode, message)
    {
    }

    internal PostmarkError(HttpStatusCode statusCode, int errorCode, string message)
        : base(statusCode, message)
    {
        ErrorCode = (PostmarkErrorCode)errorCode;
    }
}
