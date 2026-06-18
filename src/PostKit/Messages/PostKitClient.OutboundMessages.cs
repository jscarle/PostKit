using System.Globalization;
using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Messages;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using OutboundMessageAttachmentModel = PostKit.Postmark.Messages.OutboundMessageAttachmentResponse;
using OutboundMessageDetailsModel = PostKit.Postmark.Messages.OutboundMessageDetailsResponse;
using OutboundMessageDumpModel = PostKit.Postmark.Messages.OutboundMessageDumpResponse;
using OutboundMessageEventModel = PostKit.Postmark.Messages.OutboundMessageEventResponse;
using OutboundMessageModel = PostKit.Postmark.Messages.OutboundMessageResponse;
using OutboundMessageRecipientModel = PostKit.Postmark.Messages.OutboundMessageRecipientResponse;
using OutboundSearchModel = PostKit.Postmark.Messages.OutboundSearchResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public Task<Result<OutboundMessagePage>> SearchOutboundMessagesAsync(MessageStream messageStream, int count = ValidationExtensions.MaxOutboundMessageCount, int offset = 0, OutboundMessageQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The outbound message query message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<OutboundMessagePage>(error));

        return SearchOutboundMessagesAsync(messageStreamId, count, offset, query, cancellationToken);
    }

    public async Task<Result<OutboundMessagePage>> SearchOutboundMessagesAsync(string messageStream, int count = ValidationExtensions.MaxOutboundMessageCount, int offset = 0, OutboundMessageQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateOutboundMessageQuery(messageStream, count, offset, query);
        if (validationError is not null)
            return Result.Failure<OutboundMessagePage>(validationError);

        Result<OutboundSearchModel> response;
        try
        {
            var endpoint = BuildOutboundMessageSearchEndpoint(messageStream, count, offset, query);
            response = await postmark.GetAsync<OutboundSearchModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogOutboundMessagesException(ex);
            return Result.Failure<OutboundMessagePage>(ex);
        }

        if (response.IsFailure(out var error, out var outboundSearchModel))
        {
            LogOutboundMessagesError(error.Message, error);
            return Result.Failure<OutboundMessagePage>(error);
        }

        var mappedResponse = CreateOutboundMessagePage(outboundSearchModel);
        if (mappedResponse.IsFailure(out var mappingError, out var outboundMessages))
        {
            LogOutboundMessagesError(mappingError.Message, mappingError);
            return Result.Failure<OutboundMessagePage>(mappingError);
        }

        return Result.Success(outboundMessages);
    }

    public async Task<Result<OutboundMessageDetails>> GetOutboundMessageDetailsAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
            return Result.Failure<OutboundMessageDetails>("The outbound message ID must not be empty.");

        Result<OutboundMessageDetailsModel> response;
        try
        {
            response = await postmark.GetAsync<OutboundMessageDetailsModel>(PostmarkTokenScope.Server, $"/messages/outbound/{messageId:D}/details", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogOutboundMessageDetailsException(ex);
            return Result.Failure<OutboundMessageDetails>(ex);
        }

        if (response.IsFailure(out var error, out var outboundMessageModel))
        {
            LogOutboundMessageDetailsError(error.Message, error);
            return Result.Failure<OutboundMessageDetails>(error);
        }

        var mappedResponse = CreateOutboundMessageDetails(outboundMessageModel);
        if (mappedResponse.IsFailure(out var mappingError, out var outboundMessageDetails))
        {
            LogOutboundMessageDetailsError(mappingError.Message, mappingError);
            return Result.Failure<OutboundMessageDetails>(mappingError);
        }

        return Result.Success(outboundMessageDetails);
    }

    public async Task<Result<OutboundMessageDump>> GetOutboundMessageDumpAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
            return Result.Failure<OutboundMessageDump>("The outbound message ID must not be empty.");

        Result<OutboundMessageDumpModel> response;
        try
        {
            response = await postmark.GetAsync<OutboundMessageDumpModel>(PostmarkTokenScope.Server, $"/messages/outbound/{messageId:D}/dump", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogOutboundMessageDumpException(ex);
            return Result.Failure<OutboundMessageDump>(ex);
        }

        if (response.IsFailure(out var error, out var outboundMessageDumpModel))
        {
            LogOutboundMessageDumpError(error.Message, error);
            return Result.Failure<OutboundMessageDump>(error);
        }

        var mappedResponse = CreateOutboundMessageDump(outboundMessageDumpModel);
        if (mappedResponse.IsFailure(out var mappingError, out var outboundMessageDump))
        {
            LogOutboundMessageDumpError(mappingError.Message, mappingError);
            return Result.Failure<OutboundMessageDump>(mappingError);
        }

        return Result.Success(outboundMessageDump);
    }

    private static string BuildOutboundMessageSearchEndpoint(string messageStream, int count, int offset, OutboundMessageQuery? query)
    {
        var parameters = new List<string>(11) { $"count={count.ToString(CultureInfo.InvariantCulture)}", $"offset={offset.ToString(CultureInfo.InvariantCulture)}" };

        if (query?.Recipient is not null)
            parameters.Add($"recipient={Uri.EscapeDataString(query.Recipient.Address)}");

        if (query?.FromEmail is not null)
            parameters.Add($"fromemail={Uri.EscapeDataString(query.FromEmail.Address)}");

        if (query?.Tag is not null)
            parameters.Add($"tag={Uri.EscapeDataString(query.Tag)}");

        if (query?.Status.HasValue == true)
            parameters.Add($"status={Uri.EscapeDataString(GetOutboundMessageStatusQueryValue(query.Status.Value))}");

        if (query?.ToDate.HasValue == true)
            parameters.Add($"todate={Uri.EscapeDataString(FormatPostmarkDateQueryValue(query.ToDate.Value))}");

        if (query?.FromDate.HasValue == true)
            parameters.Add($"fromdate={Uri.EscapeDataString(FormatPostmarkDateQueryValue(query.FromDate.Value))}");

        if (query?.Subject is not null)
            parameters.Add($"subject={Uri.EscapeDataString(query.Subject)}");

        parameters.Add($"messagestream={Uri.EscapeDataString(messageStream)}");

        if (query?.Metadata is not null)
            parameters.Add($"metadata_{Uri.EscapeDataString(query.Metadata.Name)}={Uri.EscapeDataString(query.Metadata.Value)}");

        return $"/messages/outbound?{string.Join("&", parameters)}";
    }

    private static Result<OutboundMessagePage> CreateOutboundMessagePage(OutboundSearchModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<OutboundMessagePage>("TotalCount was not returned from the Postmark Messages API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<OutboundMessagePage>($"TotalCount returned from the Postmark Messages API was invalid. Received {response.TotalCount.Value}.");

        if (response.Messages is null)
            return Result.Failure<OutboundMessagePage>("Messages were not returned from the Postmark Messages API.");

        var messages = new List<OutboundMessage>(response.Messages.Count);
        for (var index = 0; index < response.Messages.Count; index++)
        {
            var outboundMessageModel = response.Messages[index];
            if (outboundMessageModel is null)
                return Result.Failure<OutboundMessagePage>($"Outbound message item {index} returned from the Postmark Messages API was null.");

            var mappedMessage = CreateOutboundMessage(outboundMessageModel);
            if (mappedMessage.IsFailure(out var error, out var message))
                return Result.Failure<OutboundMessagePage>($"Outbound message item {index} could not be mapped: {error.Message}");

            messages.Add(message);
        }

        return Result.Success(new OutboundMessagePage(response.TotalCount.Value, messages));
    }

    private static Result<OutboundMessage> CreateOutboundMessage(OutboundMessageModel response)
    {
        var mappedCore = CreateOutboundMessageCore(response);
        if (mappedCore.IsFailure(out var error, out var outboundMessageCore))
            return Result.Failure<OutboundMessage>(error);

        var message = new OutboundMessage(outboundMessageCore.Tag, outboundMessageCore.MessageId, outboundMessageCore.MessageStream, outboundMessageCore.To, outboundMessageCore.Cc, outboundMessageCore.Bcc, outboundMessageCore.Recipients,
            outboundMessageCore.ReceivedAt, outboundMessageCore.From, outboundMessageCore.Subject, outboundMessageCore.Attachments, outboundMessageCore.Status, outboundMessageCore.TrackOpens, outboundMessageCore.TrackLinks,
            outboundMessageCore.Metadata, outboundMessageCore.Sandboxed);
        return Result.Success(message);
    }

    private static Result<OutboundMessageDetails> CreateOutboundMessageDetails(OutboundMessageDetailsModel response)
    {
        var mappedCore = CreateOutboundMessageCore(response);
        if (mappedCore.IsFailure(out var error, out var outboundMessageCore))
            return Result.Failure<OutboundMessageDetails>(error);

        if (response.MessageEvents is null)
            return Result.Failure<OutboundMessageDetails>("MessageEvents were not returned from the Postmark Messages API.");

        var mappedMessageEvents = CreateOutboundMessageEvents(response.MessageEvents);
        if (mappedMessageEvents.IsFailure(out var messageEventsError, out var messageEvents))
            return Result.Failure<OutboundMessageDetails>(messageEventsError);

        var messageDetails = new OutboundMessageDetails(outboundMessageCore.Tag, outboundMessageCore.MessageId, outboundMessageCore.MessageStream, outboundMessageCore.To, outboundMessageCore.Cc, outboundMessageCore.Bcc,
            outboundMessageCore.Recipients, outboundMessageCore.ReceivedAt, outboundMessageCore.From, outboundMessageCore.Subject, outboundMessageCore.Attachments, outboundMessageCore.Status, outboundMessageCore.TrackOpens,
            outboundMessageCore.TrackLinks, outboundMessageCore.Metadata, outboundMessageCore.Sandboxed, response.TextBody, response.HtmlBody, response.Body, messageEvents);
        return Result.Success(messageDetails);
    }

    private static Result<OutboundMessageDump> CreateOutboundMessageDump(OutboundMessageDumpModel response)
    {
        if (response.Body is null)
            return Result.Failure<OutboundMessageDump>("Body was not returned from the Postmark Messages API.");

        return Result.Success(new OutboundMessageDump(response.Body));
    }

    private static Result<OutboundMessageCore> CreateOutboundMessageCore(OutboundMessageModel response)
    {
        if (response.Tag is null)
            return Result.Failure<OutboundMessageCore>("Tag was not returned from the Postmark Messages API.");

        if (string.IsNullOrWhiteSpace(response.MessageId))
            return Result.Failure<OutboundMessageCore>("MessageID was not returned from the Postmark Messages API.");

        if (!Guid.TryParse(response.MessageId, out var messageId))
            return Result.Failure<OutboundMessageCore>("MessageID returned from the Postmark Messages API was not a valid GUID.");

        if (string.IsNullOrWhiteSpace(response.MessageStream))
            return Result.Failure<OutboundMessageCore>("MessageStream was not returned from the Postmark Messages API.");

        if (response.To is null)
            return Result.Failure<OutboundMessageCore>("To was not returned from the Postmark Messages API.");

        var mappedTo = CreateOutboundMessageRecipients(response.To, "To");
        if (mappedTo.IsFailure(out var toError, out var to))
            return Result.Failure<OutboundMessageCore>(toError);

        if (response.Cc is null)
            return Result.Failure<OutboundMessageCore>("Cc was not returned from the Postmark Messages API.");

        var mappedCc = CreateOutboundMessageRecipients(response.Cc, "Cc");
        if (mappedCc.IsFailure(out var ccError, out var cc))
            return Result.Failure<OutboundMessageCore>(ccError);

        if (response.Bcc is null)
            return Result.Failure<OutboundMessageCore>("Bcc was not returned from the Postmark Messages API.");

        var mappedBcc = CreateOutboundMessageRecipients(response.Bcc, "Bcc");
        if (mappedBcc.IsFailure(out var bccError, out var bcc))
            return Result.Failure<OutboundMessageCore>(bccError);

        if (response.Recipients is null)
            return Result.Failure<OutboundMessageCore>("Recipients were not returned from the Postmark Messages API.");

        var mappedRecipients = CreateOutboundMessageRecipientAddresses(response.Recipients);
        if (mappedRecipients.IsFailure(out var recipientError, out var recipients))
            return Result.Failure<OutboundMessageCore>(recipientError);

        if (response.ReceivedAt is null)
            return Result.Failure<OutboundMessageCore>("ReceivedAt was not returned from the Postmark Messages API.");

        if (string.IsNullOrWhiteSpace(response.From))
            return Result.Failure<OutboundMessageCore>("From was not returned from the Postmark Messages API.");

        if (response.Subject is null)
            return Result.Failure<OutboundMessageCore>("Subject was not returned from the Postmark Messages API.");

        if (response.Attachments is null)
            return Result.Failure<OutboundMessageCore>("Attachments were not returned from the Postmark Messages API.");

        var mappedAttachments = CreateOutboundMessageAttachments(response.Attachments);
        if (mappedAttachments.IsFailure(out var attachmentError, out var attachments))
            return Result.Failure<OutboundMessageCore>(attachmentError);

        if (string.IsNullOrWhiteSpace(response.Status))
            return Result.Failure<OutboundMessageCore>("Status was not returned from the Postmark Messages API.");

        var mappedStatus = TryMapOutboundMessageStatus(response.Status);
        if (mappedStatus.IsFailure(out var statusError, out var status))
            return Result.Failure<OutboundMessageCore>(statusError);

        if (response.TrackOpens is null)
            return Result.Failure<OutboundMessageCore>("TrackOpens was not returned from the Postmark Messages API.");

        if (string.IsNullOrWhiteSpace(response.TrackLinks))
            return Result.Failure<OutboundMessageCore>("TrackLinks was not returned from the Postmark Messages API.");

        var mappedTrackLinks = TryMapOutboundMessageLinkTracking(response.TrackLinks);
        if (mappedTrackLinks.IsFailure(out var trackLinksError, out var trackLinks))
            return Result.Failure<OutboundMessageCore>(trackLinksError);

        var mappedMetadata = CreateOutboundMessageMetadata(response.Metadata);
        if (mappedMetadata.IsFailure(out var metadataError, out var metadata))
            return Result.Failure<OutboundMessageCore>(metadataError);

        if (response.Sandboxed is null)
            return Result.Failure<OutboundMessageCore>("Sandboxed was not returned from the Postmark Messages API.");

        return Result.Success(new OutboundMessageCore(response.Tag, messageId, response.MessageStream, to, cc, bcc, recipients, response.ReceivedAt.Value, response.From, response.Subject, attachments, status, response.TrackOpens.Value,
            trackLinks, metadata, response.Sandboxed.Value));
    }

    private static Result<List<OutboundMessageRecipient>> CreateOutboundMessageRecipients(List<OutboundMessageRecipientModel?> response, string propertyName)
    {
        var recipients = new List<OutboundMessageRecipient>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var recipient = response[index];
            if (recipient is null)
                return Result.Failure<List<OutboundMessageRecipient>>($"{propertyName} item {index} returned from the Postmark Messages API was null.");

            if (string.IsNullOrWhiteSpace(recipient.Email))
                return Result.Failure<List<OutboundMessageRecipient>>($"Email was not returned from the Postmark Messages API for {propertyName} item {index}.");

            recipients.Add(new OutboundMessageRecipient(recipient.Email, recipient.Name));
        }

        return Result.Success(recipients);
    }

    private static Result<List<string>> CreateOutboundMessageRecipientAddresses(List<string?> response)
    {
        var recipients = new List<string>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var recipient = response[index];
            if (string.IsNullOrWhiteSpace(recipient))
                return Result.Failure<List<string>>($"Recipients item {index} returned from the Postmark Messages API was empty.");

            recipients.Add(recipient);
        }

        return Result.Success(recipients);
    }

    private static Result<List<OutboundMessageAttachment>> CreateOutboundMessageAttachments(List<JsonElement> response)
    {
        var attachments = new List<OutboundMessageAttachment>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var mappedAttachment = CreateOutboundMessageAttachment(response[index], index);
            if (mappedAttachment.IsFailure(out var error, out var attachment))
                return Result.Failure<List<OutboundMessageAttachment>>(error);

            attachments.Add(attachment);
        }

        return Result.Success(attachments);
    }

    private static Result<OutboundMessageAttachment> CreateOutboundMessageAttachment(JsonElement response, int index)
    {
        if (response.ValueKind == JsonValueKind.String)
        {
            var name = response.GetString();
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<OutboundMessageAttachment>($"Attachment item {index} returned from the Postmark Messages API had an empty name.");

            return Result.Success(new OutboundMessageAttachment(name, null, null, null));
        }

        if (response.ValueKind != JsonValueKind.Object)
            return Result.Failure<OutboundMessageAttachment>($"Attachment item {index} returned from the Postmark Messages API had an unexpected JSON type '{response.ValueKind}'.");

        OutboundMessageAttachmentModel? attachment;
        try
        {
            attachment = response.Deserialize<OutboundMessageAttachmentModel>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (JsonException)
        {
            return Result.Failure<OutboundMessageAttachment>($"Attachment item {index} returned from the Postmark Messages API could not be deserialized because the response JSON did not match the expected shape.");
        }

        if (attachment is null)
            return Result.Failure<OutboundMessageAttachment>($"Attachment item {index} returned from the Postmark Messages API was empty.");

        if (string.IsNullOrWhiteSpace(attachment.Name))
            return Result.Failure<OutboundMessageAttachment>($"Name was not returned from the Postmark Messages API for attachment item {index}.");

        return Result.Success(new OutboundMessageAttachment(attachment.Name, attachment.ContentType, attachment.Content, attachment.ContentId));
    }

    private static Result<IReadOnlyDictionary<string, string>> CreateOutboundMessageMetadata(IReadOnlyDictionary<string, string?>? response)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (response is null)
            return Result.Success<IReadOnlyDictionary<string, string>>(metadata);

        foreach (var entry in response)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                return Result.Failure<IReadOnlyDictionary<string, string>>("Metadata returned from the Postmark Messages API contained an empty name.");

            if (entry.Value is null)
                return Result.Failure<IReadOnlyDictionary<string, string>>($"Metadata value for '{entry.Key}' returned from the Postmark Messages API was null.");

            if (ValidationExtensions.TryGetExistingKey(metadata, entry.Key, out var existingKey))
                return Result.Failure<IReadOnlyDictionary<string, string>>(
                    $"Metadata returned from the Postmark Messages API contained duplicate names when compared case-insensitively. Metadata name '{entry.Key}' duplicates '{existingKey}'.");

            metadata.Add(entry.Key, entry.Value);
        }

        return Result.Success<IReadOnlyDictionary<string, string>>(metadata);
    }

    private static Result<List<OutboundMessageEvent>> CreateOutboundMessageEvents(List<OutboundMessageEventModel?> response)
    {
        var events = new List<OutboundMessageEvent>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var messageEvent = response[index];
            if (messageEvent is null)
                return Result.Failure<List<OutboundMessageEvent>>($"MessageEvents item {index} returned from the Postmark Messages API was null.");

            var mappedEvent = CreateOutboundMessageEvent(messageEvent, index);
            if (mappedEvent.IsFailure(out var error, out var outboundMessageEvent))
                return Result.Failure<List<OutboundMessageEvent>>(error);

            events.Add(outboundMessageEvent);
        }

        return Result.Success(events);
    }

    private static Result<OutboundMessageEvent> CreateOutboundMessageEvent(OutboundMessageEventModel response, int index)
    {
        if (string.IsNullOrWhiteSpace(response.Recipient))
            return Result.Failure<OutboundMessageEvent>($"Recipient was not returned from the Postmark Messages API for MessageEvents item {index}.");

        if (string.IsNullOrWhiteSpace(response.Type))
            return Result.Failure<OutboundMessageEvent>($"Type was not returned from the Postmark Messages API for MessageEvents item {index}.");

        var mappedType = TryMapOutboundMessageEventType(response.Type);
        if (mappedType.IsFailure(out var typeError, out var eventType))
            return Result.Failure<OutboundMessageEvent>($"MessageEvents item {index} could not be mapped: {typeError.Message}");

        if (response.ReceivedAt is null)
            return Result.Failure<OutboundMessageEvent>($"ReceivedAt was not returned from the Postmark Messages API for MessageEvents item {index}.");

        if (response.Details is null)
            return Result.Failure<OutboundMessageEvent>($"Details were not returned from the Postmark Messages API for MessageEvents item {index}.");

        var mappedDetails = CreateOutboundMessageEventDetails(response.Details, index);
        if (mappedDetails.IsFailure(out var detailsError, out var details))
            return Result.Failure<OutboundMessageEvent>(detailsError);

        return Result.Success(new OutboundMessageEvent(response.Recipient, eventType, response.ReceivedAt.Value, details));
    }

    private static Result<OutboundMessageEventDetails> CreateOutboundMessageEventDetails(Dictionary<string, JsonElement> response, int eventIndex)
    {
        var values = new Dictionary<string, string>(response.Count, StringComparer.Ordinal);
        foreach (var entry in response)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                return Result.Failure<OutboundMessageEventDetails>($"Details returned from the Postmark Messages API for MessageEvents item {eventIndex} contained an empty name.");

            values.Add(entry.Key, FormatOutboundMessageEventDetailValue(entry.Value));
        }

        return Result.Success(new OutboundMessageEventDetails(values));
    }

    private static string FormatOutboundMessageEventDetailValue(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();
    }

    private static string GetOutboundMessageStatusQueryValue(OutboundMessageStatus status)
    {
        return status switch
        {
            OutboundMessageStatus.Queued => "queued",
            OutboundMessageStatus.Sent => "sent",
            OutboundMessageStatus.Processed => "processed",
            _ => throw new NotImplementedException(
                $"Outbound message status enum value of '{nameof(OutboundMessageStatus)}.{status}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<OutboundMessageStatus> TryMapOutboundMessageStatus(string status)
    {
        var mappedStatus = status switch
        {
            "Queued" => OutboundMessageStatus.Queued,
            "Sent" => OutboundMessageStatus.Sent,
            "Processed" => OutboundMessageStatus.Processed,
            _ => (OutboundMessageStatus?)null
        };

        if (mappedStatus is null)
            return Result.Failure<OutboundMessageStatus>($"Status value '{status}' returned from the Postmark Messages API is not supported.");

        return Result.Success(mappedStatus.Value);
    }

    private static Result<LinkTracking> TryMapOutboundMessageLinkTracking(string trackLinks)
    {
        var mappedTrackLinks = trackLinks switch
        {
            "None" => LinkTracking.None,
            "HtmlAndText" => LinkTracking.HtmlAndText,
            "HtmlOnly" => LinkTracking.HtmlOnly,
            "TextOnly" => LinkTracking.TextOnly,
            _ => (LinkTracking?)null
        };

        if (mappedTrackLinks is null)
            return Result.Failure<LinkTracking>($"TrackLinks value '{trackLinks}' returned from the Postmark Messages API is not supported.");

        return Result.Success(mappedTrackLinks.Value);
    }

    private static Result<OutboundMessageEventType> TryMapOutboundMessageEventType(string type)
    {
        var mappedType = type switch
        {
            "SubscriptionChanged" => OutboundMessageEventType.SubscriptionChanged,
            "Delivered" => OutboundMessageEventType.Delivered,
            "Transient" => OutboundMessageEventType.Transient,
            "Opened" => OutboundMessageEventType.Opened,
            "LinkClicked" => OutboundMessageEventType.LinkClicked,
            "Bounced" => OutboundMessageEventType.Bounced,
            _ => (OutboundMessageEventType?)null
        };

        if (mappedType is null)
            return Result.Failure<OutboundMessageEventType>($"Type value '{type}' returned from the Postmark Messages API is not supported.");

        return Result.Success(mappedType.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to search outbound messages.")]
    private partial void LogOutboundMessagesException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to search outbound messages. {Message}")]
    private partial void LogOutboundMessagesError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve outbound message details.")]
    private partial void LogOutboundMessageDetailsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve outbound message details. {Message}")]
    private partial void LogOutboundMessageDetailsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve outbound message dump.")]
    private partial void LogOutboundMessageDumpException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve outbound message dump. {Message}")]
    private partial void LogOutboundMessageDumpError(string message, [LogProperties] IError error);

    private readonly record struct OutboundMessageCore(
        string Tag,
        Guid MessageId,
        string MessageStream,
        IReadOnlyCollection<OutboundMessageRecipient> To,
        IReadOnlyCollection<OutboundMessageRecipient> Cc,
        IReadOnlyCollection<OutboundMessageRecipient> Bcc,
        IReadOnlyCollection<string> Recipients,
        DateTimeOffset ReceivedAt,
        string From,
        string Subject,
        IReadOnlyCollection<OutboundMessageAttachment> Attachments,
        OutboundMessageStatus Status,
        bool TrackOpens,
        LinkTracking TrackLinks,
        IReadOnlyDictionary<string, string> Metadata,
        bool Sandboxed);
}