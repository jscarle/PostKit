using System.Text.Json.Nodes;
using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Represents a recipient-specific message within a Postmark bulk email request.</summary>
public sealed class BulkEmailMessage
{
    internal BulkEmailMessage()
    {
    }

    /// <summary>Gets the primary recipients for the message.</summary>
    public IReadOnlyCollection<MailboxAddress>? To { get; internal init; }

    /// <summary>Gets the carbon-copy recipients for the message.</summary>
    public IReadOnlyCollection<MailboxAddress>? Cc { get; internal init; }

    /// <summary>Gets the blind-carbon-copy recipients for the message.</summary>
    public IReadOnlyCollection<MailboxAddress>? Bcc { get; internal init; }

    /// <summary>Gets a snapshot of the template model for recipient-specific rendering.</summary>
    public object? TemplateModel { get; internal init; }

    /// <summary>Gets the metadata that applies only to this message.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; internal init; }

    /// <summary>Gets the headers that apply only to this message.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; internal init; }

    internal JsonNode? TemplateModelNode { get; init; }

    internal int TemplateModelSizeInBytes { get; init; }

    /// <summary>Creates a new <see cref="ComposedBulkEmailMessageBuilder" /> for composing a <see cref="BulkEmailMessage" />.</summary>
    public static ComposedBulkEmailMessageBuilder Compose()
    {
        return new ComposedBulkEmailMessageBuilder();
    }

    /// <summary>Creates a new <see cref="TemplatedBulkEmailMessageBuilder" /> for composing a <see cref="BulkEmailMessage" /> with a template model.</summary>
    public static TemplatedBulkEmailMessageBuilder FromTemplate()
    {
        return new TemplatedBulkEmailMessageBuilder();
    }
}