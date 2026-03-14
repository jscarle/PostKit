using LightResults;
using PostKit.Bounces;
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
    Task<Result<EmailSubmission>> SendEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>Sends a batch of email messages asynchronously.</summary>
    /// <param name="emails">The collection of emails to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the batch send response or error information.</returns>
    Task<Result<EmailBatchSubmission>> SendEmailBatchAsync(IReadOnlyCollection<Email> emails, CancellationToken cancellationToken = default);

    /// <summary>Sends a bulk email request asynchronously.</summary>
    /// <param name="bulkEmail">The bulk email request to submit.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the current bulk email request status or error information.</returns>
    Task<Result<BulkEmailJob>> SendBulkEmailAsync(BulkEmail bulkEmail, CancellationToken cancellationToken = default);

    /// <summary>Gets the current status of a bulk email request asynchronously.</summary>
    /// <param name="id">The identifier of the bulk email request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the current bulk email request status or error information.</returns>
    Task<Result<BulkEmailJob>> GetBulkEmailStatusAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of bounces asynchronously.</summary>
    /// <param name="query">The bounce query to execute.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the bounce search response or error information.</returns>
    Task<Result<BouncePage>> GetBouncesAsync(BounceQuery query, CancellationToken cancellationToken = default);

    /// <summary>Gets a single bounce asynchronously.</summary>
    /// <param name="id">The identifier of the bounce to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the bounce details or error information.</returns>
    Task<Result<BounceDetails>> GetBounceAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Gets delivery statistics for the current server asynchronously.</summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing delivery statistics or error information.</returns>
    Task<Result<DeliveryStats>> GetDeliveryStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the raw dump for a bounce asynchronously.</summary>
    /// <param name="id">The identifier of the bounce dump to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the raw bounce dump or error information.</returns>
    Task<Result<BounceDump>> GetBounceDumpAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Reactivates a bounce asynchronously.</summary>
    /// <param name="id">The identifier of the bounce to reactivate.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the activation response or error information.</returns>
    Task<Result<BounceActivation>> ActivateBounceAsync(long id, CancellationToken cancellationToken = default);
}
