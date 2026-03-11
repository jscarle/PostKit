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

    private static readonly int[] ValidErrorCodes = (int[])Enum.GetValuesAsUnderlyingType<PostmarkErrorCode>();

    internal PostmarkError(PostmarkResponse response)
        : this(response.ErrorCode, response.Message)
    {
    }

    internal PostmarkError(int errorCode, string message)
        : base(HttpStatusCode.UnprocessableEntity, message)
    {
        if (!ValidErrorCodes.Contains(errorCode))
            throw new InvalidOperationException("The Postmark error code is not defined.");

        ErrorCode = (PostmarkErrorCode)errorCode;
    }
}
