using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

/// <summary>Represents a Postmark bulk email request.</summary>
public sealed class BulkEmail
{
    internal BulkEmail()
    {
    }

    /// <summary>Gets the sender of the bulk email request.</summary>
    public MailboxAddress From { get; internal init; } = null!;

    /// <summary>Gets the reply-to recipients for the bulk email request.</summary>
    public IReadOnlyCollection<MailboxAddress>? ReplyTo { get; internal init; }

    /// <summary>Gets the subject shared by the bulk email request.</summary>
    public string? Subject { get; internal init; }

    /// <summary>Gets the HTML body shared by the bulk email request.</summary>
    public string? HtmlBody { get; internal init; }

    /// <summary>Gets the plain-text body shared by the bulk email request.</summary>
    public string? TextBody { get; internal init; }

    /// <summary>Gets the optional tag used to categorize the bulk email request in Postmark.</summary>
    public string? Tag { get; internal init; }

    /// <summary>Gets the custom headers shared by the bulk email request.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; internal init; }

    /// <summary>Gets the metadata shared by the bulk email request.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; internal init; }

    /// <summary>Gets a value indicating whether open tracking is enabled.</summary>
    public bool? OpenTracking { get; internal init; }

    /// <summary>Gets the link tracking mode applied to the bulk email request.</summary>
    public LinkTracking? LinkTracking { get; internal init; }

    /// <summary>Gets the message stream the bulk email request will use.</summary>
    public string? MessageStream { get; internal init; }

    /// <summary>Gets the attachments shared by the bulk email request.</summary>
    public IReadOnlyCollection<Attachment>? Attachments { get; internal init; }

    /// <summary>Gets the Postmark template identifier used by the bulk email request.</summary>
    public int? TemplateId { get; internal init; }

    /// <summary>Gets the Postmark template alias used by the bulk email request.</summary>
    public string? TemplateAlias { get; internal init; }

    /// <summary>Gets a value indicating whether CSS should be inlined when rendering the Postmark template.</summary>
    public bool? InlineCss { get; internal init; }

    /// <summary>Gets the recipient-specific messages included in the bulk request.</summary>
    public IReadOnlyList<BulkEmailMessage> Messages { get; internal init; } = [];

    /// <summary>Creates a new <see cref="ComposedBulkEmailBuilder" /> for composing a <see cref="BulkEmail" />.</summary>
    public static ComposedBulkEmailBuilder Compose()
    {
        return new ComposedBulkEmailBuilder();
    }

    /// <summary>Creates a new <see cref="TemplatedBulkEmailBuilder" /> for composing a <see cref="BulkEmail" /> from a Postmark template.</summary>
    /// <param name="templateId">The Postmark template identifier.</param>
    /// <param name="inlineCss">Whether CSS should be inlined when rendering the template.</param>
    public static TemplatedBulkEmailBuilder FromTemplate(int templateId, bool? inlineCss = null)
    {
        return new TemplatedBulkEmailBuilder(templateId, inlineCss);
    }

    /// <summary>Creates a new <see cref="TemplatedBulkEmailBuilder" /> for composing a <see cref="BulkEmail" /> from a Postmark template.</summary>
    /// <param name="templateAlias">The Postmark template alias.</param>
    /// <param name="inlineCss">Whether CSS should be inlined when rendering the template.</param>
    public static TemplatedBulkEmailBuilder FromTemplate(string templateAlias, bool? inlineCss = null)
    {
        return new TemplatedBulkEmailBuilder(templateAlias, inlineCss);
    }
}