using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark delivery webhook messages.</summary>
public static class DeliveryWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark delivery webhook JSON could not be deserialized. Ensure the input contains one valid Postmark delivery webhook JSON object.";

    /// <summary>Deserializes a Postmark delivery webhook from a JSON string.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark delivery webhook JSON cannot be null.");

        DeliveryWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark delivery webhook from JSON characters.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        DeliveryWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark delivery webhook from UTF-8 JSON bytes.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        DeliveryWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark delivery webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark delivery webhook JSON stream cannot be null.");

        DeliveryWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark delivery webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<DeliveryWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark delivery webhook JSON stream cannot be null.");

        DeliveryWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<DeliveryWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Asynchronously deserializes a Postmark delivery webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<DeliveryWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark delivery webhook JSON pipe reader cannot be null.");

        DeliveryWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<DeliveryWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<DeliveryWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Deserializes one Postmark delivery webhook value from a UTF-8 JSON reader.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        DeliveryWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark delivery webhook from the root element of a JSON document.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark delivery webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark delivery webhook from a JSON element.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(JsonElement element)
    {
        DeliveryWebhookMessage? message;
        try
        {
            message = element.Deserialize<DeliveryWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark delivery webhook from a JSON node.</summary>
    public static Result<DeliveryWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark delivery webhook JSON node cannot be null.");

        DeliveryWebhookMessage? message;
        try
        {
            message = node.Deserialize<DeliveryWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<DeliveryWebhookMessage> Validate(DeliveryWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<DeliveryWebhookMessage>("The Postmark delivery webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.RecordType is null)
            return MissingRequiredProperty(nameof(OutboundWebhookMessage.RecordType));

        if (!string.Equals(message.RecordType, "Delivery", StringComparison.Ordinal))
            return Result.Failure<DeliveryWebhookMessage>($"The Postmark delivery webhook property 'RecordType' must be 'Delivery'. Received '{message.RecordType}'.");

        if (message.Recipient is null)
            return MissingRequiredProperty(nameof(DeliveryWebhookMessage.Recipient));

        if (message.Details is null)
            return MissingRequiredProperty(nameof(DeliveryWebhookMessage.Details));

        if (message.ServerId < 0)
            return Result.Failure<DeliveryWebhookMessage>($"The Postmark delivery webhook property 'ServerId' cannot be negative. Received {message.ServerId}.");

        if (message.Metadata is null)
            return MissingRequiredProperty(nameof(DeliveryWebhookMessage.Metadata));

        foreach (var metadata in message.Metadata)
        {
            if (metadata.Value is null)
                return Result.Failure<DeliveryWebhookMessage>($"The Postmark delivery webhook property 'Metadata[\"{metadata.Key}\"]' cannot be null.");
        }

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(DeliveryWebhookMessage.MessageStream));

        return Result.Success(message);
    }

    private static Result<DeliveryWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<DeliveryWebhookMessage>($"The Postmark delivery webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<DeliveryWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<DeliveryWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
