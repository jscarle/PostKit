using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Deserializes and validates Postmark spam complaint webhook messages.</summary>
public static class SpamComplaintWebhookDeserializer
{
    private const string DeserializationFailureMessage =
        "The Postmark spam complaint webhook JSON could not be deserialized. Ensure the input contains one valid Postmark spam complaint webhook JSON object.";

    /// <summary>Deserializes a Postmark spam complaint webhook from a JSON string.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json), "The Postmark spam complaint webhook JSON cannot be null.");

        SpamComplaintWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SpamComplaintWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark spam complaint webhook from JSON characters.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(ReadOnlySpan<char> json)
    {
        SpamComplaintWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SpamComplaintWebhookMessage>(json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark spam complaint webhook from UTF-8 JSON bytes.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        SpamComplaintWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SpamComplaintWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark spam complaint webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(Stream utf8Json)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark spam complaint webhook JSON stream cannot be null.");

        SpamComplaintWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SpamComplaintWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Asynchronously deserializes a Postmark spam complaint webhook from a UTF-8 JSON stream without closing the stream.</summary>
    public static async Task<Result<SpamComplaintWebhookMessage>> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark spam complaint webhook JSON stream cannot be null.");

        SpamComplaintWebhookMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<SpamComplaintWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Asynchronously deserializes a Postmark spam complaint webhook from a UTF-8 JSON pipe without completing the pipe reader.</summary>
    public static async Task<Result<SpamComplaintWebhookMessage>> DeserializeAsync(PipeReader utf8Json, CancellationToken cancellationToken = default)
    {
        if (utf8Json is null)
            throw new ArgumentNullException(nameof(utf8Json), "The Postmark spam complaint webhook JSON pipe reader cannot be null.");

        SpamComplaintWebhookMessage? message;
        try
        {
#if NET10_0_OR_GREATER
            message = await JsonSerializer.DeserializeAsync<SpamComplaintWebhookMessage>(utf8Json, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
#else
            await using var stream = utf8Json.AsStream(leaveOpen: true);
            message = await JsonSerializer.DeserializeAsync<SpamComplaintWebhookMessage>(stream, PostmarkConfiguration.JsonSerializerOptions, cancellationToken);
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

    /// <summary>Deserializes one Postmark spam complaint webhook value from a UTF-8 JSON reader.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(ref Utf8JsonReader reader)
    {
        SpamComplaintWebhookMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<SpamComplaintWebhookMessage>(ref reader, PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark spam complaint webhook from the root element of a JSON document.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(JsonDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document), "The Postmark spam complaint webhook JSON document cannot be null.");

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

    /// <summary>Deserializes a Postmark spam complaint webhook from a JSON element.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(JsonElement element)
    {
        SpamComplaintWebhookMessage? message;
        try
        {
            message = element.Deserialize<SpamComplaintWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    /// <summary>Deserializes a Postmark spam complaint webhook from a JSON node.</summary>
    public static Result<SpamComplaintWebhookMessage> Deserialize(JsonNode node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "The Postmark spam complaint webhook JSON node cannot be null.");

        SpamComplaintWebhookMessage? message;
        try
        {
            message = node.Deserialize<SpamComplaintWebhookMessage>(PostmarkConfiguration.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return CreateDeserializationFailure(ex);
        }

        return Validate(message);
    }

    // System.Text.Json permits explicit nulls for required non-nullable reference properties, so these checks are intentional.
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private static Result<SpamComplaintWebhookMessage> Validate(SpamComplaintWebhookMessage? message)
    {
        if (message is null)
            return Result.Failure<SpamComplaintWebhookMessage>("The Postmark spam complaint webhook JSON was null. Ensure the input contains the JSON object posted by Postmark.");

        if (message.RecordType is null)
            return MissingRequiredProperty(nameof(OutboundWebhookMessage.RecordType));

        if (!string.Equals(message.RecordType, "SpamComplaint", StringComparison.Ordinal))
            return Result.Failure<SpamComplaintWebhookMessage>($"The Postmark spam complaint webhook property 'RecordType' must be 'SpamComplaint'. Received '{message.RecordType}'.");

        if (message.Id < 0)
            return Result.Failure<SpamComplaintWebhookMessage>($"The Postmark spam complaint webhook property 'Id' cannot be negative. Received {message.Id}.");

        if (message.Type is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Type));

        if (message.TypeCode < 0)
            return Result.Failure<SpamComplaintWebhookMessage>($"The Postmark spam complaint webhook property 'TypeCode' cannot be negative. Received {message.TypeCode}.");

        if (message.Name is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Name));

        if (message.Metadata is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Metadata));

        foreach (var metadata in message.Metadata)
        {
            if (metadata.Value is null)
                return Result.Failure<SpamComplaintWebhookMessage>($"The Postmark spam complaint webhook property 'Metadata[\"{metadata.Key}\"]' cannot be null.");
        }

        if (message.ServerId < 0)
            return Result.Failure<SpamComplaintWebhookMessage>($"The Postmark spam complaint webhook property 'ServerId' cannot be negative. Received {message.ServerId}.");

        if (message.Description is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Description));

        if (message.Details is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Details));

        if (message.Email is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Email));

        if (message.Subject is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.Subject));

        if (message.MessageStream is null)
            return MissingRequiredProperty(nameof(SpamComplaintWebhookMessage.MessageStream));

        return Result.Success(message);
    }

    private static Result<SpamComplaintWebhookMessage> MissingRequiredProperty(string propertyName)
    {
        return Result.Failure<SpamComplaintWebhookMessage>($"The Postmark spam complaint webhook property '{propertyName}' is required and cannot be null.");
    }

    private static Result<SpamComplaintWebhookMessage> CreateDeserializationFailure(Exception exception)
    {
        return Result.Failure<SpamComplaintWebhookMessage>(DeserializationFailureMessage, exception);
    }
    // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
}
