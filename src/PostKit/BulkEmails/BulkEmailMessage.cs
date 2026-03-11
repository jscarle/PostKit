using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Represents a recipient-specific message within a Postmark bulk email request.</summary>
public sealed class BulkEmailMessage
{
    /// <summary>Gets the primary recipients for the message.</summary>
    public IReadOnlyCollection<MailboxAddress>? To { get; internal init; }

    /// <summary>Gets the carbon-copy recipients for the message.</summary>
    public IReadOnlyCollection<MailboxAddress>? Cc { get; internal init; }

    /// <summary>Gets the blind-carbon-copy recipients for the message.</summary>
    public IReadOnlyCollection<MailboxAddress>? Bcc { get; internal init; }

    /// <summary>Gets the template model for recipient-specific rendering.</summary>
    public object? TemplateModel { get; internal init; }

    /// <summary>Gets the metadata that applies only to this message.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; internal init; }

    /// <summary>Gets the headers that apply only to this message.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; internal init; }

    internal BulkEmailMessage()
    {
    }

    /// <summary>Creates a new <see cref="BulkEmailMessageBuilder"/> for composing a <see cref="BulkEmailMessage"/>.</summary>
    public static BulkEmailMessageBuilder CreateBuilder()
    {
        return new BulkEmailMessageBuilder();
    }
}
