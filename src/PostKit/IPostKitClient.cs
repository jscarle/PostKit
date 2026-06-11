using LightResults;
using PostKit.Bounces;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Messages;
using PostKit.Suppressions;

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
    Task<Result<EmailBatchSubmission>> SendEmailBatchAsync(IEnumerable<Email> emails, CancellationToken cancellationToken = default);

    /// <summary>Sends a bulk email request asynchronously.</summary>
    /// <param name="email">The bulk email request to submit.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the current bulk email request status or error information.</returns>
    Task<Result<BulkEmailJob>> SendBulkEmailAsync(BulkEmail email, CancellationToken cancellationToken = default);

    /// <summary>Gets the current status of a bulk email request asynchronously.</summary>
    /// <param name="id">The identifier of the bulk email request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the current bulk email request status or error information.</returns>
    Task<Result<BulkEmailJob>> GetBulkEmailStatusAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of bounces asynchronously.</summary>
    /// <param name="messageStream">The message stream whose bounces should be queried.</param>
    /// <param name="count">The number of bounces to return.</param>
    /// <param name="offset">The number of bounces to skip before returning results.</param>
    /// <param name="query">The optional bounce filters to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the bounce search response or error information.</returns>
    Task<Result<BouncePage>> GetBouncesAsync(MessageStream messageStream, int count = 500, int offset = 0, BounceQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of bounces asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose bounces should be queried.</param>
    /// <param name="count">The number of bounces to return.</param>
    /// <param name="offset">The number of bounces to skip before returning results.</param>
    /// <param name="query">The optional bounce filters to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the bounce search response or error information.</returns>
    Task<Result<BouncePage>> GetBouncesAsync(string messageStream, int count = 500, int offset = 0, BounceQuery? query = null, CancellationToken cancellationToken = default);

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

    /// <summary>Searches outbound messages asynchronously.</summary>
    /// <param name="messageStream">The message stream whose outbound messages should be searched.</param>
    /// <param name="count">The number of messages to return.</param>
    /// <param name="offset">The number of messages to skip before returning results.</param>
    /// <param name="query">The optional outbound message filters to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the outbound message search response or error information.</returns>
    Task<Result<OutboundMessagePage>> SearchOutboundMessagesAsync(MessageStream messageStream, int count = 500, int offset = 0, OutboundMessageQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Searches outbound messages asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose outbound messages should be searched.</param>
    /// <param name="count">The number of messages to return.</param>
    /// <param name="offset">The number of messages to skip before returning results.</param>
    /// <param name="query">The optional outbound message filters to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the outbound message search response or error information.</returns>
    Task<Result<OutboundMessagePage>> SearchOutboundMessagesAsync(string messageStream, int count = 500, int offset = 0, OutboundMessageQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a single outbound message asynchronously.</summary>
    /// <param name="messageId">The identifier of the outbound message to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the outbound message details or error information.</returns>
    Task<Result<OutboundMessageDetails>> GetOutboundMessageDetailsAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Gets the raw dump for an outbound message asynchronously.</summary>
    /// <param name="messageId">The identifier of the outbound message dump to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the raw outbound message dump or error information.</returns>
    Task<Result<OutboundMessageDump>> GetOutboundMessageDumpAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Gets suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream whose suppressions should be queried.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the suppression dump or error information.</returns>
    Task<Result<SuppressionDump>> GetSuppressionsAsync(MessageStream messageStream, CancellationToken cancellationToken = default);

    /// <summary>Gets suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose suppressions should be queried.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the suppression dump or error information.</returns>
    Task<Result<SuppressionDump>> GetSuppressionsAsync(string messageStream, CancellationToken cancellationToken = default);

    /// <summary>Gets suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream whose suppressions should be queried.</param>
    /// <param name="query">The optional suppression filters to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the suppression dump or error information.</returns>
    Task<Result<SuppressionDump>> GetSuppressionsAsync(MessageStream messageStream, SuppressionQuery? query, CancellationToken cancellationToken = default);

    /// <summary>Gets suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose suppressions should be queried.</param>
    /// <param name="query">The optional suppression filters to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the suppression dump or error information.</returns>
    Task<Result<SuppressionDump>> GetSuppressionsAsync(string messageStream, SuppressionQuery? query, CancellationToken cancellationToken = default);

    /// <summary>Creates a suppression for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream whose suppression list should be updated.</param>
    /// <param name="emailAddress">The email address to suppress.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression result or error information.</returns>
    Task<Result<SuppressionBatch>> CreateSuppressionsAsync(MessageStream messageStream, string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>Creates a suppression for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose suppression list should be updated.</param>
    /// <param name="emailAddress">The email address to suppress.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression result or error information.</returns>
    Task<Result<SuppressionBatch>> CreateSuppressionsAsync(string messageStream, string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>Creates suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream whose suppression list should be updated.</param>
    /// <param name="emailAddresses">The email addresses to suppress.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression results or error information.</returns>
    Task<Result<SuppressionBatch>> CreateSuppressionsAsync(MessageStream messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default);

    /// <summary>Creates suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose suppression list should be updated.</param>
    /// <param name="emailAddresses">The email addresses to suppress.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression results or error information.</returns>
    Task<Result<SuppressionBatch>> CreateSuppressionsAsync(string messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default);

    /// <summary>Deletes a suppression for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream whose suppression list should be updated.</param>
    /// <param name="emailAddress">The email address to reactivate or remove from the suppression list.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression result or error information.</returns>
    Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(MessageStream messageStream, string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>Deletes a suppression for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose suppression list should be updated.</param>
    /// <param name="emailAddress">The email address to reactivate or remove from the suppression list.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression result or error information.</returns>
    Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(string messageStream, string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>Deletes suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream whose suppression list should be updated.</param>
    /// <param name="emailAddresses">The email addresses to reactivate or remove from the suppression list.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression results or error information.</returns>
    Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(MessageStream messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default);

    /// <summary>Deletes suppressions for a message stream asynchronously.</summary>
    /// <param name="messageStream">The message stream ID whose suppression list should be updated.</param>
    /// <param name="emailAddresses">The email addresses to reactivate or remove from the suppression list.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A result containing the per-address suppression results or error information.</returns>
    Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(string messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default);
}
