using System.Text.Json;

namespace PostKit.BulkEmails;

/// <summary>Provides template model overloads for <see cref="IBulkEmailMessageBuilder"/>.</summary>
public static class BulkEmailMessageBuilderTemplateModelExtensions
{
    /// <summary>Sets the recipient-specific template model using explicit serializer options for this call.</summary>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="templateModel">The template model data.</param>
    /// <param name="serializerOptions">The serializer options to use for this template model.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="NotSupportedException">Thrown when the builder implementation does not support per-call serializer options.</exception>
    public static IBulkEmailMessageBuilder WithTemplateModel(this IBulkEmailMessageBuilder builder, object templateModel, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder switch
        {
            BulkEmailMessageBuilder messageBuilder => messageBuilder.WithTemplateModel(templateModel, serializerOptions),
            _ => throw new NotSupportedException("Per-call template model serializer options are only supported by PostKit's built-in BulkEmailMessageBuilder."),
        };
    }
}
