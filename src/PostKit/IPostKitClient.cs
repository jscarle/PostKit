using LightResults;
using PostKit.BulkEmails;
using PostKit.Emails;

namespace PostKit;

/// <summary>Defines operations for interacting with the PostKit service.</summary>
public interface IPostKitClient
{
    /// <summary>Sends an email message asynchronously.</summary>
    /// <param name="email">The email to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the send response or error information.</returns>
    Task<Result<SendEmailResponse>> SendEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>Sends a batch of email messages asynchronously.</summary>
    /// <param name="emails">The collection of emails to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the batch send response or error information.</returns>
    Task<Result<SendEmailBatchResponse>> SendEmailBatchAsync(IReadOnlyCollection<Email> emails, CancellationToken cancellationToken = default);

    /// <summary>Sends a bulk email request asynchronously.</summary>
    /// <param name="bulkEmail">The bulk email request to submit.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the current bulk email request status or error information.</returns>
    Task<Result<BulkEmailStatusResponse>> SendBulkEmailAsync(BulkEmail bulkEmail, CancellationToken cancellationToken = default);

    /// <summary>Gets the current status of a bulk email request asynchronously.</summary>
    /// <param name="id">The identifier of the bulk email request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the current bulk email request status or error information.</returns>
    Task<Result<BulkEmailStatusResponse>> GetBulkEmailStatusAsync(Guid id, CancellationToken cancellationToken = default);
}
