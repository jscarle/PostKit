using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark click webhook messages.</summary>
public static class ClickWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark click webhook JSON could not be deserialized. Ensure the input contains one valid Postmark click webhook JSON object.";

    /// <summary>Deserializes a Postmark click webhook from a JSON string.</summary>
    public static Result<ClickWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark click webhook JSON cannot be null.");

        ClickWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<ClickWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark click webhook from JSON characters.</summary>
    public static Result<ClickWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        ClickWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<ClickWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark click webhook from UTF-8 JSON bytes.</summary>
    public static Result<ClickWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        ClickWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<ClickWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark click webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<ClickWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark click webhook JSON stream cannot be null.");

        ClickWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<ClickWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark click webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<ClickWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark click webhook JSON stream cannot be null.");

        ClickWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<ClickWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Asynchronously deserializes a Postmark click webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<ClickWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark click webhook JSON pipe reader cannot be null.");

        ClickWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<ClickWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<ClickWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Deserializes one Postmark click webhook value from a UTF-8 JSON reader.</summary>
    public static Result<ClickWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        ClickWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<ClickWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark click webhook from the root element of a JSON document.</summary>
    public static Result<ClickWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark click webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark click webhook from a JSON element.</summary>
    public static Result<ClickWebhookMessage> Deserialize(JsonElement element)
    {
        ClickWebhookMessage? message;
        try
        {
            message = element.Deserialize<ClickWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark click webhook from a JSON node.</summary>
    public static Result<ClickWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark click webhook JSON node cannot be null.");

        ClickWebhookMessage? message;
        try
        {
            message = node.Deserialize<ClickWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<ClickWebhookMessage> Validate(ClickWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<ClickWebhookMessage>("The Postmark click webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.RecordType is null)
            return MissingRequiredProperty(nameof(OutboundWebhookMessage.RecordType));

        if (!string.Equals(message.RecordType, "Click", StringComparison.Ordinal))
            return Result.Failure<ClickWebhookMessage>($"The Postmark click webhook property 'RecordType' must be 'Click'. Received '{message.RecordType}'.");

        if (message.ClickLocation is null)
            return MissingRequiredProperty(nameof(ClickWebhookMessage.ClickLocation));

        if (message.UserAgent is null)
            return MissingRequiredProperty(nameof(ClickWebhookMessage.UserAgent));

        if (message.OriginalLink is null)
            return MissingRequiredProperty(nameof(ClickWebhookMessage.OriginalLink));

        if (message.Metadata is null)
            return MissingRequiredProperty(nameof(ClickWebhookMessage.Metadata));

        foreach (var metadata in message.Metadata)
        {
            if (metadata.Value is null)
                return Result.Failure<ClickWebhookMessage>($"The Postmark click webhook property 'Metadata[\"{metadata.Key}\"]' cannot be null.");
        }

        if (message.Recipient is null)
            return MissingRequiredProperty(nameof(ClickWebhookMessage.Recipient));

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(ClickWebhookMessage.MessageStream));

        return Result.Success(message);
    }

    private static Result<ClickWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<ClickWebhookMessage>($"The Postmark click webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<ClickWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<ClickWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
