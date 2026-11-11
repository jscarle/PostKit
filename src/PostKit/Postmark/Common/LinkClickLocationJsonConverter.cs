using System.Text.Json;
using System.Text.Json.Serialization;
using LightResults;
using PostKit.Common;

namespace PostKit.Postmark.Common;

internal sealed class LinkClickLocationJsonConverter : JsonConverter<LinkClickLocation>
{
    internal static Result<LinkClickLocation> FromPostmark(string value)
    {
        var mapped = value switch
        {
            "HTML" => LinkClickLocation.Html,
            "Text" => LinkClickLocation.Text,
            _ => (LinkClickLocation?)null
        };

        if (mapped is null)
            return Result.Failure<LinkClickLocation>($"LinkClickLocation value '{value}' returned by Postmark is not supported. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).");

        return Result.Success(mapped.Value);
    }

    internal static string ToPostmark(LinkClickLocation value)
    {
        return value switch
        {
            LinkClickLocation.Html => "HTML",
            LinkClickLocation.Text => "Text",
            _ => throw new NotImplementedException($"LinkClickLocation enum value '{value}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    public override LinkClickLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
            throw new JsonException($"LinkClickLocation must be a string. Received {reader.TokenType}.");

        var result = FromPostmark(reader.GetString()!);
        if (result.IsFailure(out var error, out var value))
            throw new JsonException(error.Message);

        return value;
    }

    public override void Write(Utf8JsonWriter writer, LinkClickLocation value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToPostmark(value));
    }

    public override LinkClickLocation ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, LinkClickLocation value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(ToPostmark(value));
    }
}
