using System.Text.Json;

namespace PostKit.Emails;

/// <summary>Provides template model overloads for <see cref="IEmailBuilder"/>.</summary>
public static class EmailBuilderTemplateModelExtensions
{
    /// <summary>Sets the model that will be merged into the selected template using explicit serializer options for this call.</summary>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="templateModel">The template model data.</param>
    /// <param name="serializerOptions">The serializer options to use for this template model.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder implementation does not support per-call serializer options.</exception>
    public static IEmailBuilder WithTemplateModel(this IEmailBuilder builder, object templateModel, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder switch
        {
            EmailBuilder emailBuilder => emailBuilder.WithTemplateModel(templateModel, serializerOptions),
            _ => throw new NotSupportedException("Per-call template model serializer options are only supported by PostKit's built-in EmailBuilder."),
        };
    }
}
