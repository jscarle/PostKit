using System.Text.Json;
using System.Text.Json.Serialization;
using LightResults;
using PostKit.Common;

namespace PostKit.Postmark.Common;

internal sealed class MessagePlatformJsonConverter : JsonConverter<MessagePlatform>
{
    internal static Result<MessagePlatform> FromPostmark(string value)
    {
        var mapped = value switch
        {
            "WebMail" => MessagePlatform.WebMail,
            "Desktop" => MessagePlatform.Desktop,
            "Mobile" => MessagePlatform.Mobile,
            "Unknown" => MessagePlatform.Unknown,
            _ => (MessagePlatform?)null
        };

        if (mapped is null)
            return Result.Failure<MessagePlatform>($"MessagePlatform value '{value}' returned by Postmark is not supported. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).");

        return Result.Success(mapped.Value);
    }

    internal static string ToPostmark(MessagePlatform value)
    {
        return value switch
        {
            MessagePlatform.WebMail => "WebMail",
            MessagePlatform.Desktop => "Desktop",
            MessagePlatform.Mobile => "Mobile",
            MessagePlatform.Unknown => "Unknown",
            _ => throw new NotImplementedException($"MessagePlatform enum value '{value}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    public override MessagePlatform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
            throw new JsonException($"MessagePlatform must be a string. Received {reader.TokenType}.");

        var result = FromPostmark(reader.GetString()!);
        if (result.IsFailure(out var error, out var value))
            throw new JsonException(error.Message);

        return value;
    }

    public override void Write(Utf8JsonWriter writer, MessagePlatform value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToPostmark(value));
    }

    public override MessagePlatform ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, MessagePlatform value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(ToPostmark(value));
    }
}
