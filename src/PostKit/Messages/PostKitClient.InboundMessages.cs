using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Messages;
using PostKit.Postmark;
using InboundActionModel = PostKit.Postmark.Messages.InboundMessageActionResponse;
using InboundAddressModel = PostKit.Postmark.Messages.InboundMessageAddressResponse;
using InboundAttachmentModel = PostKit.Postmark.Messages.InboundMessageAttachmentResponse;
using InboundDetailsModel = PostKit.Postmark.Messages.InboundMessageDetailsResponse;
using InboundHeaderModel = PostKit.Postmark.Messages.InboundMessageHeaderResponse;
using InboundMessageModel = PostKit.Postmark.Messages.InboundMessageResponse;
using InboundSearchModel = PostKit.Postmark.Messages.InboundSearchResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<InboundMessagePage>> SearchInboundMessagesAsync(int count = ValidationExtensions.MaxInboundMessageCount, int offset = 0, InboundMessageQuery? query = null, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateInboundMessageQuery(count, offset, query);
        if (validationError is not null)
            return Result.Failure<InboundMessagePage>(validationError);

        Result<InboundSearchModel> response;
        try
        {
            var endpoint = BuildInboundMessageSearchEndpoint(count, offset, query);
            response = await postmark.GetAsync<InboundSearchModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogInboundMessagesException(ex);
            return Result.Failure<InboundMessagePage>(ex);
        }

        if (response.IsFailure(out var error, out var inboundSearchModel))
        {
            LogInboundMessagesError(error.Message, error);
            return Result.Failure<InboundMessagePage>(error);
        }

        var mappedResponse = CreateInboundMessagePage(inboundSearchModel);
        if (mappedResponse.IsFailure(out var mappingError, out var inboundMessages))
        {
            LogInboundMessagesError(mappingError.Message, mappingError);
            return Result.Failure<InboundMessagePage>(mappingError);
        }

        return Result.Success(inboundMessages);
    }

    public async Task<Result<InboundMessageDetails>> GetInboundMessageDetailsAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
            return Result.Failure<InboundMessageDetails>("The inbound message ID must not be empty.");

        Result<InboundDetailsModel> response;
        try
        {
            response = await postmark.GetAsync<InboundDetailsModel>(PostmarkTokenScope.Server, $"/messages/inbound/{messageId:D}/details", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogInboundMessageDetailsException(ex);
            return Result.Failure<InboundMessageDetails>(ex);
        }

        if (response.IsFailure(out var error, out var inboundDetailsModel))
        {
            LogInboundMessageDetailsError(error.Message, error);
            return Result.Failure<InboundMessageDetails>(error);
        }

        var mappedResponse = CreateInboundMessageDetails(inboundDetailsModel);
        if (mappedResponse.IsFailure(out var mappingError, out var inboundMessageDetails))
        {
            LogInboundMessageDetailsError(mappingError.Message, mappingError);
            return Result.Failure<InboundMessageDetails>(mappingError);
        }

        return Result.Success(inboundMessageDetails);
    }

    public Task<Result<InboundMessageAction>> BypassInboundMessageRulesAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return ChangeInboundMessageProcessingAsync(messageId, "bypass", "bypass inbound message rules", LogBypassInboundMessageException, LogBypassInboundMessageError, cancellationToken);
    }

    public Task<Result<InboundMessageAction>> RetryInboundMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return ChangeInboundMessageProcessingAsync(messageId, "retry", "retry inbound message processing", LogRetryInboundMessageException, LogRetryInboundMessageError, cancellationToken);
    }

    private async Task<Result<InboundMessageAction>> ChangeInboundMessageProcessingAsync(Guid messageId, string operationPath, string operationName, Action<Exception> logException, Action<string, IError> logError,
        CancellationToken cancellationToken)
    {
        if (messageId == Guid.Empty)
            return Result.Failure<InboundMessageAction>($"The inbound message ID for {operationName} must not be empty.");

        Result<InboundActionModel> response;
        try
        {
            response = await postmark.PutAsync<InboundActionModel>(PostmarkTokenScope.Server, $"/messages/inbound/{messageId:D}/{operationPath}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logException(ex);
            return Result.Failure<InboundMessageAction>(ex);
        }

        if (response.IsFailure(out var error, out var actionModel))
        {
            logError(error.Message, error);
            return Result.Failure<InboundMessageAction>(error);
        }

        var mapped = CreateInboundMessageAction(actionModel);
        if (mapped.IsFailure(out var mappingError, out var action))
        {
            logError(mappingError.Message, mappingError);
            return Result.Failure<InboundMessageAction>(mappingError);
        }

        return Result.Success(action);
    }

    private static string BuildInboundMessageSearchEndpoint(int count, int offset, InboundMessageQuery? query)
    {
        var parameters = new List<string>(10)
        {
            $"count={count.ToString(CultureInfo.InvariantCulture)}",
            $"offset={offset.ToString(CultureInfo.InvariantCulture)}"
        };

        if (query?.Recipient is not null)
            parameters.Add($"recipient={Uri.EscapeDataString(query.Recipient.Address)}");

        if (query?.FromEmail is not null)
            parameters.Add($"fromemail={Uri.EscapeDataString(query.FromEmail.Address)}");

        if (query?.Tag is not null)
            parameters.Add($"tag={Uri.EscapeDataString(query.Tag)}");

        if (query?.Subject is not null)
            parameters.Add($"subject={Uri.EscapeDataString(query.Subject)}");

        if (query?.MailboxHash is not null)
            parameters.Add($"mailboxhash={Uri.EscapeDataString(query.MailboxHash)}");

        if (query?.Status.HasValue == true)
            parameters.Add($"status={Uri.EscapeDataString(GetInboundMessageStatusQueryValue(query.Status.Value))}");

        if (query?.ToDate.HasValue == true)
            parameters.Add($"todate={Uri.EscapeDataString(FormatPostmarkDateQueryValue(query.ToDate.Value))}");

        if (query?.FromDate.HasValue == true)
            parameters.Add($"fromdate={Uri.EscapeDataString(FormatPostmarkDateQueryValue(query.FromDate.Value))}");

        return $"/messages/inbound?{string.Join("&", parameters)}";
    }

    private static Result<InboundMessagePage> CreateInboundMessagePage(InboundSearchModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<InboundMessagePage>("TotalCount was not returned from the Postmark Messages API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<InboundMessagePage>($"TotalCount returned from the Postmark Messages API was invalid. Received {response.TotalCount.Value}.");

        if (response.InboundMessages is null)
            return Result.Failure<InboundMessagePage>("InboundMessages were not returned from the Postmark Messages API.");

        var messages = new List<InboundMessage>(response.InboundMessages.Count);
        for (var index = 0; index < response.InboundMessages.Count; index++)
        {
            var inboundMessageModel = response.InboundMessages[index];
            if (inboundMessageModel is null)
                return Result.Failure<InboundMessagePage>($"Inbound message item {index} returned from the Postmark Messages API was null.");

            var mappedMessage = CreateInboundMessage(inboundMessageModel);
            if (mappedMessage.IsFailure(out var error, out var message))
                return Result.Failure<InboundMessagePage>($"Inbound message item {index} could not be mapped: {error.Message}");

            messages.Add(message);
        }

        return Result.Success(new InboundMessagePage { TotalCount = response.TotalCount.Value, Messages = messages });
    }

    private static Result<InboundMessage> CreateInboundMessage(InboundMessageModel response)
    {
        var mappedCore = CreateInboundMessageCore(response);
        if (mappedCore.IsFailure(out var error, out var core))
            return Result.Failure<InboundMessage>(error);

        return Result.Success(new InboundMessage(core.From, core.FromName, core.FromFull, core.To, core.ToFull, core.CcFull, core.Cc, core.ReplyTo, core.OriginalRecipient, core.Subject, core.Date, core.MailboxHash, core.Tag,
            core.Attachments, core.MessageId, core.Status));
    }

    private static Result<InboundMessageDetails> CreateInboundMessageDetails(InboundDetailsModel response)
    {
        var mappedMessage = CreateInboundMessage(response);
        if (mappedMessage.IsFailure(out var error, out var message))
            return Result.Failure<InboundMessageDetails>(error);

        if (response.Headers is null)
            return Result.Failure<InboundMessageDetails>("Headers were not returned from the Postmark Messages API.");

        var headers = CreateInboundMessageHeaders(response.Headers);
        if (headers.IsFailure(out var headerError, out var mappedHeaders))
            return Result.Failure<InboundMessageDetails>(headerError);

        return Result.Success(new InboundMessageDetails(message, response.TextBody, response.HtmlBody, mappedHeaders, NormalizeOptionalInboundString(response.BlockedReason)));
    }

    private static Result<InboundMessageCore> CreateInboundMessageCore(InboundMessageModel response)
    {
        if (string.IsNullOrWhiteSpace(response.From))
            return Result.Failure<InboundMessageCore>("From was not returned from the Postmark Messages API.");

        var fromFull = CreateInboundMessageAddress(response.FromFull, "FromFull");
        if (fromFull.IsFailure(out var fromFullError, out var mappedFromFull))
            return Result.Failure<InboundMessageCore>(fromFullError);

        if (string.IsNullOrWhiteSpace(response.To))
            return Result.Failure<InboundMessageCore>("To was not returned from the Postmark Messages API.");

        if (response.ToFull is null)
            return Result.Failure<InboundMessageCore>("ToFull was not returned from the Postmark Messages API.");

        var toFull = CreateInboundMessageAddresses(response.ToFull, "ToFull");
        if (toFull.IsFailure(out var toFullError, out var mappedToFull))
            return Result.Failure<InboundMessageCore>(toFullError);

        if (response.CcFull is null)
            return Result.Failure<InboundMessageCore>("CcFull was not returned from the Postmark Messages API.");

        var ccFull = CreateInboundMessageAddresses(response.CcFull, "CcFull");
        if (ccFull.IsFailure(out var ccFullError, out var mappedCcFull))
            return Result.Failure<InboundMessageCore>(ccFullError);

        if (string.IsNullOrWhiteSpace(response.OriginalRecipient))
            return Result.Failure<InboundMessageCore>("OriginalRecipient was not returned from the Postmark Messages API.");

        if (response.Attachments is null)
            return Result.Failure<InboundMessageCore>("Attachments were not returned from the Postmark Messages API.");

        var attachments = CreateInboundMessageAttachments(response.Attachments);
        if (attachments.IsFailure(out var attachmentError, out var mappedAttachments))
            return Result.Failure<InboundMessageCore>(attachmentError);

        if (string.IsNullOrWhiteSpace(response.MessageId))
            return Result.Failure<InboundMessageCore>("MessageID was not returned from the Postmark Messages API.");

        if (!Guid.TryParse(response.MessageId, out var messageId))
            return Result.Failure<InboundMessageCore>("MessageID returned from the Postmark Messages API was not a valid GUID.");

        if (string.IsNullOrWhiteSpace(response.Status))
            return Result.Failure<InboundMessageCore>("Status was not returned from the Postmark Messages API.");

        var status = TryMapInboundMessageStatus(response.Status);
        if (status.IsFailure(out var statusError, out var mappedStatus))
            return Result.Failure<InboundMessageCore>(statusError);

        return Result.Success(new InboundMessageCore(response.From, NormalizeOptionalInboundString(response.FromName), mappedFromFull, response.To, mappedToFull, mappedCcFull, NormalizeOptionalInboundString(response.Cc),
            NormalizeOptionalInboundString(response.ReplyTo), response.OriginalRecipient, response.Subject, response.Date, NormalizeOptionalInboundString(response.MailboxHash), NormalizeOptionalInboundString(response.Tag),
            mappedAttachments, messageId, mappedStatus));
    }

    private static Result<InboundMessageAddress?> CreateInboundMessageAddress(InboundAddressModel? response, string propertyName)
    {
        if (response is null)
            return Result.Success<InboundMessageAddress?>(null);

        if (string.IsNullOrWhiteSpace(response.Email))
            return Result.Failure<InboundMessageAddress?>($"Email was not returned from the Postmark Messages API for {propertyName}.");

        return Result.Success<InboundMessageAddress?>(new InboundMessageAddress { Email = response.Email, Name = NormalizeOptionalInboundString(response.Name) });
    }

    private static Result<IReadOnlyList<InboundMessageAddress>> CreateInboundMessageAddresses(List<InboundAddressModel?> response, string propertyName)
    {
        var addresses = new List<InboundMessageAddress>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var mappedAddress = CreateInboundMessageAddress(response[index], $"{propertyName} item {index}");
            if (mappedAddress.IsFailure(out var error, out var address))
                return Result.Failure<IReadOnlyList<InboundMessageAddress>>(error);

            if (address is null)
                return Result.Failure<IReadOnlyList<InboundMessageAddress>>($"{propertyName} item {index} returned from the Postmark Messages API was null.");

            addresses.Add(address);
        }

        return Result.Success<IReadOnlyList<InboundMessageAddress>>(addresses);
    }

    private static Result<IReadOnlyList<InboundMessageAttachment>> CreateInboundMessageAttachments(List<InboundAttachmentModel?> response)
    {
        var attachments = new List<InboundMessageAttachment>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var attachment = response[index];
            if (attachment is null)
                return Result.Failure<IReadOnlyList<InboundMessageAttachment>>($"Attachments item {index} returned from the Postmark Messages API was null.");

            if (string.IsNullOrWhiteSpace(attachment.Name))
                return Result.Failure<IReadOnlyList<InboundMessageAttachment>>($"Name was not returned from the Postmark Messages API for Attachments item {index}.");

            if (string.IsNullOrWhiteSpace(attachment.ContentType))
                return Result.Failure<IReadOnlyList<InboundMessageAttachment>>($"ContentType was not returned from the Postmark Messages API for Attachments item {index}.");

            if (attachment.ContentLength is null)
                return Result.Failure<IReadOnlyList<InboundMessageAttachment>>($"ContentLength was not returned from the Postmark Messages API for Attachments item {index}.");

            if (attachment.ContentLength.Value < 0)
                return Result.Failure<IReadOnlyList<InboundMessageAttachment>>($"ContentLength returned from the Postmark Messages API for Attachments item {index} was invalid. Received {attachment.ContentLength.Value}.");

            attachments.Add(new InboundMessageAttachment
            {
                Name = attachment.Name,
                ContentId = NormalizeOptionalInboundString(attachment.ContentId),
                ContentType = attachment.ContentType,
                ContentLength = attachment.ContentLength.Value
            });
        }

        return Result.Success<IReadOnlyList<InboundMessageAttachment>>(attachments);
    }

    private static Result<IReadOnlyList<InboundMessageHeader>> CreateInboundMessageHeaders(List<InboundHeaderModel?> response)
    {
        var headers = new List<InboundMessageHeader>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var header = response[index];
            if (header is null)
                return Result.Failure<IReadOnlyList<InboundMessageHeader>>($"Headers item {index} returned from the Postmark Messages API was null.");

            if (string.IsNullOrWhiteSpace(header.Name))
                return Result.Failure<IReadOnlyList<InboundMessageHeader>>($"Name was not returned from the Postmark Messages API for Headers item {index}.");

            if (header.Value is null)
                return Result.Failure<IReadOnlyList<InboundMessageHeader>>($"Value was not returned from the Postmark Messages API for Headers item {index}.");

            headers.Add(new InboundMessageHeader { Name = header.Name, Value = header.Value });
        }

        return Result.Success<IReadOnlyList<InboundMessageHeader>>(headers);
    }

    private static Result<InboundMessageAction> CreateInboundMessageAction(InboundActionModel response)
    {
        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<InboundMessageAction>("Message was not returned from the Postmark Messages API.");

        return Result.Success(new InboundMessageAction { Message = response.Message });
    }

    private static string GetInboundMessageStatusQueryValue(InboundMessageStatus status)
    {
        return status switch
        {
            InboundMessageStatus.Blocked => "blocked",
            InboundMessageStatus.Processed => "processed",
            InboundMessageStatus.Queued => "queued",
            InboundMessageStatus.Failed => "failed",
            InboundMessageStatus.Scheduled => "scheduled",
            _ => throw new NotImplementedException(
                $"Inbound message status enum value of '{nameof(InboundMessageStatus)}.{status}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<InboundMessageStatus> TryMapInboundMessageStatus(string status)
    {
        var mappedStatus = status switch
        {
            "Blocked" => InboundMessageStatus.Blocked,
            "Processed" => InboundMessageStatus.Processed,
            "Queued" => InboundMessageStatus.Queued,
            "Failed" => InboundMessageStatus.Failed,
            "Scheduled" => InboundMessageStatus.Scheduled,
            _ => (InboundMessageStatus?)null
        };

        if (mappedStatus is null)
            return Result.Failure<InboundMessageStatus>($"Status value '{status}' returned from the Postmark Messages API is not supported.");

        return Result.Success(mappedStatus.Value);
    }

    private static string? NormalizeOptionalInboundString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to search inbound messages.")]
    private partial void LogInboundMessagesException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to search inbound messages. {Message}")]
    private partial void LogInboundMessagesError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve inbound message details.")]
    private partial void LogInboundMessageDetailsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve inbound message details. {Message}")]
    private partial void LogInboundMessageDetailsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to bypass inbound message rules.")]
    private partial void LogBypassInboundMessageException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to bypass inbound message rules. {Message}")]
    private partial void LogBypassInboundMessageError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retry inbound message processing.")]
    private partial void LogRetryInboundMessageException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retry inbound message processing. {Message}")]
    private partial void LogRetryInboundMessageError(string message, [LogProperties] IError error);

    private readonly record struct InboundMessageCore(
        string From,
        string? FromName,
        InboundMessageAddress? FromFull,
        string To,
        IReadOnlyList<InboundMessageAddress> ToFull,
        IReadOnlyList<InboundMessageAddress> CcFull,
        string? Cc,
        string? ReplyTo,
        string OriginalRecipient,
        string? Subject,
        string? Date,
        string? MailboxHash,
        string? Tag,
        IReadOnlyList<InboundMessageAttachment> Attachments,
        Guid MessageId,
        InboundMessageStatus Status);
}