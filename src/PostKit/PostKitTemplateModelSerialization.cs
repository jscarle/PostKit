using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostKit;

/// <summary>Provides process-wide defaults for serializing template models.</summary>
public static class PostKitTemplateModelSerialization
{
    private static JsonSerializerOptions _defaultSerializerOptions = CreateDefaultSerializerOptions();

    /// <summary>Gets or sets the default serializer options used when a template model is provided without explicit per-call serializer options. A defensive copy is returned and stored to avoid accidental external mutation of the global defaults.</summary>
    public static JsonSerializerOptions DefaultSerializerOptions
    {
        get => new(_defaultSerializerOptions);
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Default serializer options cannot be null.");

            _defaultSerializerOptions = new JsonSerializerOptions(value);
        }
    }

    internal static JsonSerializerOptions Resolve(JsonSerializerOptions? serializerOptions)
    {
        return serializerOptions is null ? new JsonSerializerOptions(_defaultSerializerOptions) : new JsonSerializerOptions(serializerOptions);
    }

    private static JsonSerializerOptions CreateDefaultSerializerOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    }
}
