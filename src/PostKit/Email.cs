using MimeKit;

namespace PostKit;

/// <summary>Represents an email message that can be composed and sent through PostKit.</summary>
public sealed class Email
{
    /// <summary>Gets the sender of the email.</summary>
    public MailboxAddress? From { get; internal init; }

    /// <summary>Gets the reply-to recipients for the email.</summary>
    public IReadOnlyCollection<MailboxAddress>? ReplyTo { get; internal init; }

    /// <summary>Gets the primary recipients of the email.</summary>
    public IReadOnlyCollection<MailboxAddress>? To { get; internal init; }

    /// <summary>Gets the carbon-copy recipients of the email.</summary>
    public IReadOnlyCollection<MailboxAddress>? Cc { get; internal init; }

    /// <summary>Gets the blind-carbon-copy recipients of the email.</summary>
    public IReadOnlyCollection<MailboxAddress>? Bcc { get; internal init; }

    /// <summary>Gets the subject line of the email.</summary>
    public string? Subject { get; internal init; }

    /// <summary>Gets the HTML body of the email.</summary>
    public string? HtmlBody { get; internal init; }

    /// <summary>Gets the plain text body of the email.</summary>
    public string? TextBody { get; internal init; }

    /// <summary>Gets the optional tag used to categorize the email in Postmark.</summary>
    public string? Tag { get; internal init; }

    /// <summary>Gets the custom headers applied to the email.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; internal init; }

    /// <summary>Gets the metadata that accompanies the email.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; internal init; }

    /// <summary>Gets a value indicating whether open tracking is enabled for the email.</summary>
    public bool? OpenTracking { get; internal init; }

    /// <summary>Gets the link tracking mode applied to the email.</summary>
    public LinkTracking? LinkTracking { get; internal init; }

    /// <summary>Gets the message stream the email will be sent through.</summary>
    public string? MessageStream { get; internal init; }

    /// <summary>Gets the attachments that will be sent with the email.</summary>
    public IReadOnlyCollection<Attachment>? Attachments { get; internal init; }

    /// <summary>Gets the Postmark template identifier to use when sending the email.</summary>
    public int? TemplateId { get; internal init; }

    /// <summary>Gets the Postmark template alias to use when sending the email.</summary>
    public string? TemplateAlias { get; internal init; }

    /// <summary>Gets the model that will be merged into the email template.</summary>
    public object? TemplateModel { get; internal init; }

    /// <summary>Gets a value indicating whether CSS should be inlined when sending the email.</summary>
    public bool? InlineCss { get; internal init; }

    internal Email()
    {
    }

    /// <summary>Creates a new <see cref="EmailBuilder"/> for composing an <see cref="Email"/>.</summary>
    /// <returns>A builder that can be used to configure an email.</returns>
    public static EmailBuilder CreateBuilder()
    {
        return new EmailBuilder();
    }
}
