using System.Text.Json;
using System.Text.Json.Serialization;
using LightResults;
using PostKit.Webhooks;

namespace PostKit.Postmark.Common;

internal sealed class WebhookTriggerTypeJsonConverter : JsonConverter<WebhookTriggerType>
{
    internal static Result<WebhookTriggerType> FromPostmark(string value)
    {
        var mapped = value switch
        {
            "Open" => WebhookTriggerType.Open,
            "Click" => WebhookTriggerType.Click,
            "Delivery" => WebhookTriggerType.Delivery,
            "Bounce" => WebhookTriggerType.Bounce,
            "SpamComplaint" => WebhookTriggerType.SpamComplaint,
            "SubscriptionChange" => WebhookTriggerType.SubscriptionChange,
            _ => (WebhookTriggerType?)null
        };

        if (mapped is null)
            return Result.Failure<WebhookTriggerType>($"WebhookTriggerType value '{value}' returned by Postmark is not supported. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).");

        return Result.Success(mapped.Value);
    }

    internal static string ToPostmark(WebhookTriggerType value)
    {
        return value switch
        {
            WebhookTriggerType.Open => "Open",
            WebhookTriggerType.Click => "Click",
            WebhookTriggerType.Delivery => "Delivery",
            WebhookTriggerType.Bounce => "Bounce",
            WebhookTriggerType.SpamComplaint => "SpamComplaint",
            WebhookTriggerType.SubscriptionChange => "SubscriptionChange",
            _ => throw new NotImplementedException($"WebhookTriggerType enum value '{value}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    public override WebhookTriggerType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
            throw new JsonException($"WebhookTriggerType must be a string. Received {reader.TokenType}.");

        var result = FromPostmark(reader.GetString()!);
        if (result.IsFailure(out var error, out var value))
            throw new JsonException(error.Message);

        return value;
    }

    public override void Write(Utf8JsonWriter writer, WebhookTriggerType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToPostmark(value));
    }

    public override WebhookTriggerType ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, WebhookTriggerType value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(ToPostmark(value));
    }
}
