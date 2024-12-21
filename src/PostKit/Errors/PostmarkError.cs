using System.Net;
using PostKit.Postmark.Email;

namespace PostKit.Errors;

internal sealed class PostmarkError : HttpError
{
    public PostmarkErrorCode ErrorCode { get; }

    private static readonly int[] ValidErrorCodes = (int[])Enum.GetValuesAsUnderlyingType<PostmarkErrorCode>();

    public PostmarkError(ErrorResponse response)
        : base(HttpStatusCode.UnprocessableEntity, response.Message)
    {
        if (!ValidErrorCodes.Contains(response.ErrorCode))
            throw new InvalidOperationException("The Postmark error code is not defined.");

        ErrorCode = (PostmarkErrorCode)response.ErrorCode;
    }
}
