using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark subscription change webhook messages.</summary>
public static class SubscriptionChangeWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark subscription change webhook JSON could not be deserialized. Ensure the input contains one valid Postmark subscription change webhook JSON object.";

    /// <summary>Deserializes a Postmark subscription change webhook from a JSON string.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark subscription change webhook JSON cannot be null.");

        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SubscriptionChangeWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark subscription change webhook from JSON characters.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SubscriptionChangeWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark subscription change webhook from UTF-8 JSON bytes.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SubscriptionChangeWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark subscription change webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark subscription change webhook JSON stream cannot be null.");

        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SubscriptionChangeWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark subscription change webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<SubscriptionChangeWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark subscription change webhook JSON stream cannot be null.");

        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<SubscriptionChangeWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Asynchronously deserializes a Postmark subscription change webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<SubscriptionChangeWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark subscription change webhook JSON pipe reader cannot be null.");

        SubscriptionChangeWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<SubscriptionChangeWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<SubscriptionChangeWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Deserializes one Postmark subscription change webhook value from a UTF-8 JSON reader.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SubscriptionChangeWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark subscription change webhook from the root element of a JSON document.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark subscription change webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark subscription change webhook from a JSON element.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(JsonElement element)
    {
        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = element.Deserialize<SubscriptionChangeWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark subscription change webhook from a JSON node.</summary>
    public static Result<SubscriptionChangeWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark subscription change webhook JSON node cannot be null.");

        SubscriptionChangeWebhookMessage? message;
        try
        {
            message = node.Deserialize<SubscriptionChangeWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<SubscriptionChangeWebhookMessage> Validate(SubscriptionChangeWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<SubscriptionChangeWebhookMessage>("The Postmark subscription change webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.RecordType is null)
            return MissingRequiredProperty(nameof(OutboundWebhookMessage.RecordType));

        if (!string.Equals(message.RecordType, "SubscriptionChange", StringComparison.Ordinal))
            return Result.Failure<SubscriptionChangeWebhookMessage>(
                $"The Postmark subscription change webhook property 'RecordType' must be 'SubscriptionChange'. Received '{message.RecordType}'.");

        if (message.ServerId < 0)
            return Result.Failure<SubscriptionChangeWebhookMessage>($"The Postmark subscription change webhook property 'ServerId' cannot be negative. Received {message.ServerId}.");

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(SubscriptionChangeWebhookMessage.MessageStream));

        if (message.Recipient is null)
            return MissingRequiredProperty(nameof(SubscriptionChangeWebhookMessage.Recipient));

        if (message.Origin is null)
            return MissingRequiredProperty(nameof(SubscriptionChangeWebhookMessage.Origin));

        if (message.Metadata is null)
            return MissingRequiredProperty(nameof(SubscriptionChangeWebhookMessage.Metadata));

        foreach (var metadata in message.Metadata)
        {
            if (metadata.Value is null)
                return Result.Failure<SubscriptionChangeWebhookMessage>($"The Postmark subscription change webhook property 'Metadata[\"{metadata.Key}\"]' cannot be null.");
        }

        if (message.SuppressSending && message.SuppressionReason is null)
            return Result.Failure<SubscriptionChangeWebhookMessage>(
                "The Postmark subscription change webhook property 'SuppressionReason' cannot be null when 'SuppressSending' is true.");

        if (!message.SuppressSending && message.SuppressionReason is not null)
            return Result.Failure<SubscriptionChangeWebhookMessage>(
                "The Postmark subscription change webhook property 'SuppressionReason' must be null when 'SuppressSending' is false.");

        if (!message.SuppressSending && message.Tag is not null)
            return Result.Failure<SubscriptionChangeWebhookMessage>("The Postmark subscription change webhook property 'Tag' must be null when 'SuppressSending' is false.");

        if (!message.SuppressSending && message.Metadata.Count != 0)
            return Result.Failure<SubscriptionChangeWebhookMessage>("The Postmark subscription change webhook property 'Metadata' must be empty when 'SuppressSending' is false.");

        return Result.Success(message);
    }

    private static Result<SubscriptionChangeWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<SubscriptionChangeWebhookMessage>($"The Postmark subscription change webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<SubscriptionChangeWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<SubscriptionChangeWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
