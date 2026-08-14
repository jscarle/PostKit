using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark open webhook messages.</summary>
public static class OpenWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark open webhook JSON could not be deserialized. Ensure the input contains one valid Postmark open webhook JSON object.";

    /// <summary>Deserializes a Postmark open webhook from a JSON string.</summary>
    public static Result<OpenWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark open webhook JSON cannot be null.");

        OpenWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<OpenWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark open webhook from JSON characters.</summary>
    public static Result<OpenWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        OpenWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<OpenWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark open webhook from UTF-8 JSON bytes.</summary>
    public static Result<OpenWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        OpenWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<OpenWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark open webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<OpenWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark open webhook JSON stream cannot be null.");

        OpenWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<OpenWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark open webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<OpenWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark open webhook JSON stream cannot be null.");

        OpenWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<OpenWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Asynchronously deserializes a Postmark open webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<OpenWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark open webhook JSON pipe reader cannot be null.");

        OpenWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<OpenWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<OpenWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Deserializes one Postmark open webhook value from a UTF-8 JSON reader.</summary>
    public static Result<OpenWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        OpenWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<OpenWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark open webhook from the root element of a JSON document.</summary>
    public static Result<OpenWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark open webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark open webhook from a JSON element.</summary>
    public static Result<OpenWebhookMessage> Deserialize(JsonElement element)
    {
        OpenWebhookMessage? message;
        try
        {
            message = element.Deserialize<OpenWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark open webhook from a JSON node.</summary>
    public static Result<OpenWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark open webhook JSON node cannot be null.");

        OpenWebhookMessage? message;
        try
        {
            message = node.Deserialize<OpenWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<OpenWebhookMessage> Validate(OpenWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<OpenWebhookMessage>("The Postmark open webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.RecordType is null)
            return MissingRequiredProperty(nameof(OutboundWebhookMessage.RecordType));

        if (!string.Equals(message.RecordType, "Open", StringComparison.Ordinal))
            return Result.Failure<OpenWebhookMessage>($"The Postmark open webhook property 'RecordType' must be 'Open'. Received '{message.RecordType}'.");

        if (message.UserAgent is null)
            return MissingRequiredProperty(nameof(OpenWebhookMessage.UserAgent));

        if (message.Metadata is null)
            return MissingRequiredProperty(nameof(OpenWebhookMessage.Metadata));

        foreach (var metadata in message.Metadata)
        {
            if (metadata.Value is null)
                return Result.Failure<OpenWebhookMessage>($"The Postmark open webhook property 'Metadata[\"{metadata.Key}\"]' cannot be null.");
        }

        if (message.Recipient is null)
            return MissingRequiredProperty(nameof(OpenWebhookMessage.Recipient));

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(OpenWebhookMessage.MessageStream));

        return Result.Success(message);
    }

    private static Result<OpenWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<OpenWebhookMessage>($"The Postmark open webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<OpenWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<OpenWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
