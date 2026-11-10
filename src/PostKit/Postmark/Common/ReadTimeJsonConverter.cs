using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostKit.Postmark.Common;

internal sealed class ReadTimeJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var seconds) || seconds < 0)
            throw new JsonException("ReadSeconds must be a nonnegative integer.");

        return TimeSpan.FromSeconds(seconds);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        if (value < TimeSpan.Zero || value.Ticks % TimeSpan.TicksPerSecond != 0 || value.TotalSeconds > int.MaxValue)
            throw new JsonException("ReadTime must contain a nonnegative whole number of seconds no greater than Int32.MaxValue.");

        writer.WriteNumberValue((int)value.TotalSeconds);
    }
}
