using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark bounce webhook messages.</summary>
public static class BounceWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark bounce webhook JSON could not be deserialized. Ensure the input contains one valid Postmark bounce webhook JSON object.";

    /// <summary>Deserializes a Postmark bounce webhook from a JSON string.</summary>
    public static Result<BounceWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark bounce webhook JSON cannot be null.");

        BounceWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<BounceWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark bounce webhook from JSON characters.</summary>
    public static Result<BounceWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        BounceWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<BounceWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark bounce webhook from UTF-8 JSON bytes.</summary>
    public static Result<BounceWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        BounceWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<BounceWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark bounce webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<BounceWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark bounce webhook JSON stream cannot be null.");

        BounceWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<BounceWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark bounce webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<BounceWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark bounce webhook JSON stream cannot be null.");

        BounceWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<BounceWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Asynchronously deserializes a Postmark bounce webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<BounceWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark bounce webhook JSON pipe reader cannot be null.");

        BounceWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<BounceWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<BounceWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Deserializes one Postmark bounce webhook value from a UTF-8 JSON reader.</summary>
    public static Result<BounceWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        BounceWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<BounceWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark bounce webhook from the root element of a JSON document.</summary>
    public static Result<BounceWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark bounce webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark bounce webhook from a JSON element.</summary>
    public static Result<BounceWebhookMessage> Deserialize(JsonElement element)
    {
        BounceWebhookMessage? message;
        try
        {
            message = element.Deserialize<BounceWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark bounce webhook from a JSON node.</summary>
    public static Result<BounceWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark bounce webhook JSON node cannot be null.");

        BounceWebhookMessage? message;
        try
        {
            message = node.Deserialize<BounceWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<BounceWebhookMessage> Validate(BounceWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<BounceWebhookMessage>("The Postmark bounce webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.RecordType is null)
            return MissingRequiredProperty(nameof(OutboundWebhookMessage.RecordType));

        if (!string.Equals(message.RecordType, "Bounce", StringComparison.Ordinal))
            return Result.Failure<BounceWebhookMessage>($"The Postmark bounce webhook property 'RecordType' must be 'Bounce'. Received '{message.RecordType}'.");

        if (message.Id < 0)
            return Result.Failure<BounceWebhookMessage>($"The Postmark bounce webhook property 'Id' cannot be negative. Received {message.Id}.");

        if (message.Type is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Type));

        if (message.TypeCode < 0)
            return Result.Failure<BounceWebhookMessage>($"The Postmark bounce webhook property 'TypeCode' cannot be negative. Received {message.TypeCode}.");

        if (message.Name is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Name));

        if (message.Metadata is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Metadata));

        foreach (var metadata in message.Metadata)
        {
            if (metadata.Value is null)
                return Result.Failure<BounceWebhookMessage>($"The Postmark bounce webhook property 'Metadata[\"{metadata.Key}\"]' cannot be null.");
        }

        if (message.ServerId < 0)
            return Result.Failure<BounceWebhookMessage>($"The Postmark bounce webhook property 'ServerId' cannot be negative. Received {message.ServerId}.");

        if (message.Description is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Description));

        if (message.Details is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Details));

        if (message.Email is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Email));

        if (message.Subject is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.Subject));

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(BounceWebhookMessage.MessageStream));

        return Result.Success(message);
    }

    private static Result<BounceWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<BounceWebhookMessage>($"The Postmark bounce webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<BounceWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<BounceWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
