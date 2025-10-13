using LightResults;
using PostKit.Responses;

namespace PostKit;

/// <summary>
/// Defines operations for interacting with the PostKit service.
/// </summary>
public interface IPostKitClient
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="email">The email to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the send response or error information.</returns>
    Task<Result<SendEmailResponse>> SendEmailAsync(Email email, CancellationToken cancellationToken = default);
}
