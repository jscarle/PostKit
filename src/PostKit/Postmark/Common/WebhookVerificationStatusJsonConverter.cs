using System.Text.Json;
using System.Text.Json.Serialization;
using LightResults;
using PostKit.Webhooks;

namespace PostKit.Postmark.Common;

internal sealed class WebhookVerificationStatusJsonConverter : JsonConverter<WebhookVerificationStatus>
{
    internal static Result<WebhookVerificationStatus> FromPostmark(string value)
    {
        var mapped = value switch
        {
            "verified" => WebhookVerificationStatus.Verified,
            "unverified" => WebhookVerificationStatus.Unverified,
            _ => (WebhookVerificationStatus?)null
        };

        if (mapped is null)
            return Result.Failure<WebhookVerificationStatus>($"WebhookVerificationStatus value '{value}' returned by Postmark is not supported. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).");

        return Result.Success(mapped.Value);
    }

    internal static string ToPostmark(WebhookVerificationStatus value)
    {
        return value switch
        {
            WebhookVerificationStatus.Verified => "verified",
            WebhookVerificationStatus.Unverified => "unverified",
            _ => throw new NotImplementedException($"WebhookVerificationStatus enum value '{value}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    public override WebhookVerificationStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
            throw new JsonException($"WebhookVerificationStatus must be a string. Received {reader.TokenType}.");

        var result = FromPostmark(reader.GetString()!);
        if (result.IsFailure(out var error, out var value))
            throw new JsonException(error.Message);

        return value;
    }

    public override void Write(Utf8JsonWriter writer, WebhookVerificationStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToPostmark(value));
    }

    public override WebhookVerificationStatus ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, WebhookVerificationStatus value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(ToPostmark(value));
    }
}
