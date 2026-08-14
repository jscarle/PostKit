using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark outbound webhook messages by their record type.</summary>
public static class OutboundWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark outbound webhook JSON could not be deserialized. Ensure the input contains one valid Postmark outbound webhook JSON object.";

    /// <summary>Deserializes a Postmark outbound webhook from a JSON string.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark outbound webhook JSON cannot be null.");

        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    /// <summary>Deserializes a Postmark outbound webhook from JSON characters.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    /// <summary>Deserializes a Postmark outbound webhook from UTF-8 JSON bytes.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    /// <summary>Deserializes a Postmark outbound webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark outbound webhook JSON stream cannot be null.");

        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    /// <summary>Asynchronously deserializes a Postmark outbound webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<OutboundWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark outbound webhook JSON stream cannot be null.");

        JsonElement element;
        try
        {
            element = await JsonSerializer.DeserializeAsync<JsonElement>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    /// <summary>Asynchronously deserializes a Postmark outbound webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<OutboundWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark outbound webhook JSON pipe reader cannot be null.");

        JsonElement element;
        try
        {
#if NET10_0_OR_GREATER
            element = await JsonSerializer.DeserializeAsync<JsonElement>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            element = await JsonSerializer.DeserializeAsync<JsonElement>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

        return Deserialize(element);
    }

    /// <summary>Deserializes one Postmark outbound webhook value from a UTF-8 JSON reader.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        JsonElement element;
        try
        {
            element = JsonSerializer.Deserialize<JsonElement>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    /// <summary>Deserializes a Postmark outbound webhook from the root element of a JSON document.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark outbound webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark outbound webhook from a JSON element.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(JsonElement element)
    {
        string? recordType;
        try
        {
            if (element.ValueKind != JsonValueKind.Object)
                return Result.Failure<OutboundWebhookMessage>("The Postmark outbound webhook JSON root must be an object.");

            if (!element.TryGetProperty("RecordType", out var recordTypeElement))
                return Result.Failure<OutboundWebhookMessage>("The Postmark outbound webhook property 'RecordType' is required.");

            if (recordTypeElement.ValueKind == JsonValueKind.Null)
                return Result.Failure<OutboundWebhookMessage>("The Postmark outbound webhook property 'RecordType' cannot be null.");

            if (recordTypeElement.ValueKind != JsonValueKind.String)
                return Result.Failure<OutboundWebhookMessage>("The Postmark outbound webhook property 'RecordType' must be a JSON string.");

            recordType = recordTypeElement.GetString();
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return recordType switch
        {
            "Bounce" => Convert(BounceWebhookDeserializer.Deserialize(element)),
            "Click" => Convert(ClickWebhookDeserializer.Deserialize(element)),
            "Delivery" => Convert(DeliveryWebhookDeserializer.Deserialize(element)),
            "Open" => Convert(OpenWebhookDeserializer.Deserialize(element)),
            "SpamComplaint" => Convert(SpamComplaintWebhookDeserializer.Deserialize(element)),
            "SubscriptionChange" => Convert(SubscriptionChangeWebhookDeserializer.Deserialize(element)),
            _ => Result.Failure<OutboundWebhookMessage>($"The Postmark outbound webhook RecordType value '{recordType}' is not supported.")
        };
    }

    /// <summary>Deserializes a Postmark outbound webhook from a JSON node.</summary>
    public static Result<OutboundWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark outbound webhook JSON node cannot be null.");

        JsonElement element;
        try
        {
            element = node.Deserialize<JsonElement>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Deserialize(element);
    }

    private static Result<OutboundWebhookMessage> Convert<TMessage>(Result<TMessage> result) where TMessage : OutboundWebhookMessage
    {
        if (result.IsFailure(out var error, out var message))
            return Result.Failure<OutboundWebhookMessage>(error);

        return Result.Success<OutboundWebhookMessage>(message);
    }

    private static Result<OutboundWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<OutboundWebhookMessage>(DeserializationFailureMessage, exception);
    }
}
