using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostKit.Postmark.Common;

internal static class PostmarkConfiguration
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
}