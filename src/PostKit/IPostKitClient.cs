using LightResults;
using PostKit.Bounces;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.DataRemovals;
using PostKit.Domains;
using PostKit.Emails;
using PostKit.InboundRules;
using PostKit.Messages;
using PostKit.MessageStreams;
using PostKit.SenderSignatures;
using PostKit.Servers;
using PostKit.Stats;
using PostKit.Suppressions;
using PostKit.Templates;
using PostKit.Webhooks;

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

    /// <summary>Searches inbound messages asynchronously.</summary>
    Task<Result<InboundMessagePage>> SearchInboundMessagesAsync(int count = 500, int offset = 0, InboundMessageQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a single inbound message asynchronously.</summary>
    Task<Result<InboundMessageDetails>> GetInboundMessageDetailsAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Bypasses inbound rules for a blocked inbound message asynchronously.</summary>
    Task<Result<InboundMessageAction>> BypassInboundMessageRulesAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Retries processing for a failed inbound message asynchronously.</summary>
    Task<Result<InboundMessageAction>> RetryInboundMessageAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Searches message open events asynchronously.</summary>
    Task<Result<MessageOpenPage>> SearchMessageOpensAsync(MessageStream messageStream, int count = 500, int offset = 0, MessageTrackingQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Searches message open events asynchronously.</summary>
    Task<Result<MessageOpenPage>> SearchMessageOpensAsync(string messageStream, int count = 500, int offset = 0, MessageTrackingQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets open events for a single outbound message asynchronously.</summary>
    Task<Result<MessageOpenPage>> GetMessageOpensAsync(Guid messageId, int count = 500, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>Searches message click events asynchronously.</summary>
    Task<Result<MessageClickPage>> SearchMessageClicksAsync(MessageStream messageStream, int count = 500, int offset = 0, MessageTrackingQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Searches message click events asynchronously.</summary>
    Task<Result<MessageClickPage>> SearchMessageClicksAsync(string messageStream, int count = 500, int offset = 0, MessageTrackingQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets click events for a single outbound message asynchronously.</summary>
    Task<Result<MessageClickPage>> GetMessageClicksAsync(Guid messageId, int count = 500, int offset = 0, CancellationToken cancellationToken = default);

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

    /// <summary>Gets a Postmark template by ID asynchronously.</summary>
    Task<Result<Template>> GetTemplateAsync(long templateId, CancellationToken cancellationToken = default);

    /// <summary>Gets a Postmark template by alias asynchronously.</summary>
    Task<Result<Template>> GetTemplateAsync(string templateAlias, CancellationToken cancellationToken = default);

    /// <summary>Creates a Postmark template asynchronously.</summary>
    Task<Result<TemplateSummary>> CreateTemplateAsync(TemplateCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a Postmark template by ID asynchronously.</summary>
    Task<Result<TemplateSummary>> EditTemplateAsync(long templateId, TemplateEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a Postmark template by alias asynchronously.</summary>
    Task<Result<TemplateSummary>> EditTemplateAsync(string templateAlias, TemplateEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of Postmark templates asynchronously.</summary>
    Task<Result<TemplatePage>> ListTemplatesAsync(int count = 100, int offset = 0, TemplateQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a Postmark template by ID asynchronously.</summary>
    Task<Result<TemplateDeletion>> DeleteTemplateAsync(long templateId, CancellationToken cancellationToken = default);

    /// <summary>Deletes a Postmark template by alias asynchronously.</summary>
    Task<Result<TemplateDeletion>> DeleteTemplateAsync(string templateAlias, CancellationToken cancellationToken = default);

    /// <summary>Validates Postmark template content asynchronously.</summary>
    Task<Result<TemplateValidation>> ValidateTemplateAsync(TemplateValidationParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Pushes template changes between Postmark servers asynchronously.</summary>
    Task<Result<TemplatePush>> PushTemplatesAsync(TemplatePushParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Lists webhooks for the current server asynchronously.</summary>
    Task<Result<WebhookList>> ListWebhooksAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists webhooks for a message stream asynchronously.</summary>
    Task<Result<WebhookList>> ListWebhooksAsync(MessageStream messageStream, CancellationToken cancellationToken = default);

    /// <summary>Lists webhooks for a message stream ID asynchronously.</summary>
    Task<Result<WebhookList>> ListWebhooksAsync(string messageStream, CancellationToken cancellationToken = default);

    /// <summary>Gets a webhook asynchronously.</summary>
    Task<Result<Webhook>> GetWebhookAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Creates a webhook asynchronously.</summary>
    Task<Result<Webhook>> CreateWebhookAsync(WebhookCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a webhook asynchronously.</summary>
    Task<Result<Webhook>> EditWebhookAsync(long id, WebhookEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Deletes a webhook asynchronously.</summary>
    Task<Result<WebhookDeletion>> DeleteWebhookAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists inbound rule triggers asynchronously.</summary>
    Task<Result<InboundRuleTriggerPage>> ListInboundRuleTriggersAsync(int count = 500, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>Creates an inbound rule trigger asynchronously.</summary>
    Task<Result<InboundRuleTrigger>> CreateInboundRuleTriggerAsync(InboundRuleTriggerCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Creates an inbound rule trigger asynchronously.</summary>
    Task<Result<InboundRuleTrigger>> CreateInboundRuleTriggerAsync(string rule, CancellationToken cancellationToken = default);

    /// <summary>Deletes an inbound rule trigger asynchronously.</summary>
    Task<Result<InboundRuleTriggerDeletion>> DeleteInboundRuleTriggerAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound statistics overview asynchronously.</summary>
    Task<Result<OutboundOverviewStats>> GetOutboundStatsOverviewAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound sent-count statistics asynchronously.</summary>
    Task<Result<OutboundSentStats>> GetOutboundSentStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound bounce-count statistics asynchronously.</summary>
    Task<Result<OutboundBounceStats>> GetOutboundBounceStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound spam-complaint statistics asynchronously.</summary>
    Task<Result<OutboundSpamComplaintStats>> GetOutboundSpamComplaintStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound tracked-email statistics asynchronously.</summary>
    Task<Result<OutboundTrackedEmailStats>> GetOutboundTrackedEmailStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound open statistics asynchronously.</summary>
    Task<Result<OutboundOpenStats>> GetOutboundOpenStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound email-platform statistics asynchronously.</summary>
    Task<Result<OutboundEmailPlatformStats>> GetOutboundEmailPlatformStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound email-client statistics asynchronously.</summary>
    Task<Result<OutboundEmailClientStats>> GetOutboundEmailClientStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound click statistics asynchronously.</summary>
    Task<Result<OutboundClickStats>> GetOutboundClickStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound click-browser statistics asynchronously.</summary>
    Task<Result<OutboundClickBrowserStats>> GetOutboundClickBrowserStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound click-platform statistics asynchronously.</summary>
    Task<Result<OutboundClickPlatformStats>> GetOutboundClickPlatformStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets outbound click-location statistics asynchronously.</summary>
    Task<Result<OutboundClickLocationStats>> GetOutboundClickLocationStatsAsync(OutboundStatsQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a data removal request asynchronously.</summary>
    Task<Result<DataRemoval>> CreateDataRemovalAsync(DataRemovalCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Gets a data removal request asynchronously.</summary>
    Task<Result<DataRemoval>> GetDataRemovalAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Gets the current Postmark server using the configured server token.</summary>
    Task<Result<PostmarkServer>> GetServerAsync(CancellationToken cancellationToken = default);

    /// <summary>Edits the current Postmark server using the configured server token.</summary>
    Task<Result<PostmarkServer>> EditServerAsync(ServerEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Gets a Postmark server by ID using the configured account token.</summary>
    Task<Result<PostmarkServer>> GetServerAsync(long serverId, CancellationToken cancellationToken = default);

    /// <summary>Creates a Postmark server using the configured account token.</summary>
    Task<Result<PostmarkServer>> CreateServerAsync(ServerCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a Postmark server by ID using the configured account token.</summary>
    Task<Result<PostmarkServer>> EditServerAsync(long serverId, ServerEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Lists Postmark servers using the configured account token.</summary>
    Task<Result<ServerPage>> ListServersAsync(int count = 100, int offset = 0, ServerQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a Postmark server by ID using the configured account token.</summary>
    Task<Result<ServerDeletion>> DeleteServerAsync(long serverId, CancellationToken cancellationToken = default);

    /// <summary>Lists message streams for the current server.</summary>
    Task<Result<MessageStreamList>> ListMessageStreamsAsync(MessageStreamQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a message stream by ID.</summary>
    Task<Result<MessageStreamInfo>> GetMessageStreamAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>Creates a message stream.</summary>
    Task<Result<MessageStreamInfo>> CreateMessageStreamAsync(MessageStreamCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a message stream.</summary>
    Task<Result<MessageStreamInfo>> EditMessageStreamAsync(string streamId, MessageStreamEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Archives a message stream.</summary>
    Task<Result<MessageStreamArchive>> ArchiveMessageStreamAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>Unarchives a message stream.</summary>
    Task<Result<MessageStreamInfo>> UnarchiveMessageStreamAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>Lists sender domains using the configured account token.</summary>
    Task<Result<DomainPage>> ListDomainsAsync(int count = 100, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets a sender domain by ID using the configured account token.</summary>
    Task<Result<PostmarkDomain>> GetDomainAsync(long domainId, CancellationToken cancellationToken = default);

    /// <summary>Creates a sender domain using the configured account token.</summary>
    Task<Result<PostmarkDomain>> CreateDomainAsync(DomainCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a sender domain using the configured account token.</summary>
    Task<Result<PostmarkDomain>> EditDomainAsync(long domainId, DomainEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sender domain using the configured account token.</summary>
    Task<Result<DomainDeletion>> DeleteDomainAsync(long domainId, CancellationToken cancellationToken = default);

    /// <summary>Verifies DKIM for a sender domain using the configured account token.</summary>
    Task<Result<PostmarkDomain>> VerifyDomainDkimAsync(long domainId, CancellationToken cancellationToken = default);

    /// <summary>Verifies the return-path domain using the configured account token.</summary>
    Task<Result<PostmarkDomain>> VerifyDomainReturnPathAsync(long domainId, CancellationToken cancellationToken = default);

    /// <summary>Verifies SPF for a sender domain using the configured account token.</summary>
    Task<Result<DomainSpfVerification>> VerifyDomainSpfAsync(long domainId, CancellationToken cancellationToken = default);

    /// <summary>Rotates DKIM for a sender domain using the configured account token.</summary>
    Task<Result<DomainDkimRotation>> RotateDomainDkimAsync(long domainId, CancellationToken cancellationToken = default);

    /// <summary>Lists sender signatures using the configured account token.</summary>
    Task<Result<SenderSignaturePage>> ListSenderSignaturesAsync(int count = 100, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets a sender signature by ID using the configured account token.</summary>
    Task<Result<SenderSignature>> GetSenderSignatureAsync(long signatureId, CancellationToken cancellationToken = default);

    /// <summary>Creates a sender signature using the configured account token.</summary>
    Task<Result<SenderSignature>> CreateSenderSignatureAsync(SenderSignatureCreateParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Edits a sender signature using the configured account token.</summary>
    Task<Result<SenderSignature>> EditSenderSignatureAsync(long signatureId, SenderSignatureEditParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sender signature using the configured account token.</summary>
    Task<Result<SenderSignatureDeletion>> DeleteSenderSignatureAsync(long signatureId, CancellationToken cancellationToken = default);

    /// <summary>Resends confirmation for a sender signature using the configured account token.</summary>
    Task<Result<SenderSignatureAction>> ResendSenderSignatureConfirmationAsync(long signatureId, CancellationToken cancellationToken = default);

    /// <summary>Verifies SPF for a sender signature using the configured account token.</summary>
    Task<Result<SenderSignature>> VerifySenderSignatureSpfAsync(long signatureId, CancellationToken cancellationToken = default);

    /// <summary>Requests new DKIM keys for a sender signature using the configured account token.</summary>
    Task<Result<SenderSignatureAction>> RequestNewDkimForSenderSignatureAsync(long signatureId, CancellationToken cancellationToken = default);
}