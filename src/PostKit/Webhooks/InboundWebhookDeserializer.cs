using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark inbound webhook messages.</summary>
public static class InboundWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark inbound webhook JSON could not be deserialized. Ensure the input contains one valid Postmark inbound webhook JSON object.";

    /// <summary>Deserializes a Postmark inbound webhook from a JSON string.</summary>
    public static Result<InboundWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark inbound webhook JSON cannot be null.");

        InboundWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<InboundWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark inbound webhook from JSON characters.</summary>
    public static Result<InboundWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        InboundWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<InboundWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark inbound webhook from UTF-8 JSON bytes.</summary>
    public static Result<InboundWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        InboundWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<InboundWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark inbound webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<InboundWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark inbound webhook JSON stream cannot be null.");

        InboundWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<InboundWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark inbound webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<InboundWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark inbound webhook JSON stream cannot be null.");

        InboundWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<InboundWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark inbound webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<InboundWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark inbound webhook JSON pipe reader cannot be null.");

        InboundWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<InboundWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<InboundWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#endif
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes one Postmark inbound webhook value from a UTF-8 JSON reader.</summary>
    public static Result<InboundWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        InboundWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<InboundWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark inbound webhook from the root element of a JSON document.</summary>
    public static Result<InboundWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark inbound webhook JSON document cannot be null.");

        JsonElement rootElement;
        try
        {
            rootElement = document.RootElement;
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(rootElement);
    }

    /// <summary>Deserializes a Postmark inbound webhook from a JSON element.</summary>
    public static Result<InboundWebhookMessage> Deserialize(JsonElement element)
    {
        InboundWebhookMessage? message;
        try
        {
            message = element.Deserialize<InboundWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark inbound webhook from a JSON node.</summary>
    public static Result<InboundWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark inbound webhook JSON node cannot be null.");

        InboundWebhookMessage? message;
        try
        {
            message = node.Deserialize<InboundWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<InboundWebhookMessage> Validate(InboundWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<InboundWebhookMessage>("The Postmark inbound webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.FromName is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.FromName));

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.MessageStream));

        if (message.From is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.From));

        if (message.FromFull is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.FromFull));

        var fromFullError = ValidateAddress(message.FromFull, nameof(InboundWebhookMessage.FromFull));
        if (fromFullError is not null)
            return Result.Failure<InboundWebhookMessage>(fromFullError);

        if (message.To is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.To));

        var toFullError = ValidateAddresses(message.ToFull, nameof(InboundWebhookMessage.ToFull));
        if (toFullError is not null)
            return Result.Failure<InboundWebhookMessage>(toFullError);

        if (message.Cc is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Cc));

        var ccFullError = ValidateAddresses(message.CcFull, nameof(InboundWebhookMessage.CcFull));
        if (ccFullError is not null)
            return Result.Failure<InboundWebhookMessage>(ccFullError);

        if (message.Bcc is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Bcc));

        var bccFullError = ValidateAddresses(message.BccFull, nameof(InboundWebhookMessage.BccFull));
        if (bccFullError is not null)
            return Result.Failure<InboundWebhookMessage>(bccFullError);

        if (message.OriginalRecipient is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.OriginalRecipient));

        if (message.Subject is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Subject));

        if (message.ReplyTo is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.ReplyTo));

        if (message.MailboxHash is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.MailboxHash));

        if (message.Date is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Date));

        if (message.TextBody is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.TextBody));

        if (message.HtmlBody is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.HtmlBody));

        if (message.StrippedTextReply is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.StrippedTextReply));

        if (message.Tag is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Tag));

        if (message.Headers is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Headers));

        for (var index = 0; index < message.Headers.Count; index++)
        {
            var header = message.Headers[index];
            if (header is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Headers[{index}]' cannot be null.");

            if (header.Name is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Headers[{index}].Name' is required and cannot be null.");

            if (header.Value is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Headers[{index}].Value' is required and cannot be null.");
        }

        if (message.Attachments is null)
            return MissingRequiredProperty(nameof(InboundWebhookMessage.Attachments));

        for (var index = 0; index < message.Attachments.Count; index++)
        {
            var attachment = message.Attachments[index];
            if (attachment is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Attachments[{index}]' cannot be null.");

            if (attachment.Name is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Attachments[{index}].Name' is required and cannot be null.");

            if (attachment.Content is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Attachments[{index}].Content' is required and cannot be null.");

            if (attachment.ContentType is null)
                return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property 'Attachments[{index}].ContentType' is required and cannot be null.");

            if (attachment.ContentLength < 0)
                return Result.Failure<InboundWebhookMessage>(
                    $"The Postmark inbound webhook property 'Attachments[{index}].ContentLength' cannot be negative. Received {attachment.ContentLength}.");
        }

        return Result.Success(message);
    }

    private static string? ValidateAddresses(IReadOnlyList<InboundWebhookAddress?>? addresses, string propertyName)
    {
        if (addresses is null)
            return $"The Postmark inbound webhook property '{propertyName}' is required and cannot be null.";

        for (var index = 0; index < addresses.Count; index++)
        {
            var error = ValidateAddress(addresses[index], $"{propertyName}[{index}]");
            if (error is not null)
                return error;
        }

        return null;
    }

    private static string? ValidateAddress(InboundWebhookAddress? address, string propertyPath)
    {
        if (address is null)
            return $"The Postmark inbound webhook property '{propertyPath}' cannot be null.";

        if (address.Email is null)
            return $"The Postmark inbound webhook property '{propertyPath}.Email' is required and cannot be null.";

        if (address.Name is null)
            return $"The Postmark inbound webhook property '{propertyPath}.Name' is required and cannot be null.";

        if (address.MailboxHash is null)
            return $"The Postmark inbound webhook property '{propertyPath}.MailboxHash' is required and cannot be null.";

        return null;
    }

    private static Result<InboundWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<InboundWebhookMessage>($"The Postmark inbound webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<InboundWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<InboundWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
