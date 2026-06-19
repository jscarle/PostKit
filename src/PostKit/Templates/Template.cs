using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents a full template returned from Postmark.</summary>
public sealed record Template : TemplateSummary
{
    internal Template(long templateId, string name, string? subject, string? htmlBody, string? textBody, long associatedServerId, bool active, string? alias, TemplateType templateType, string? layoutTemplate) : base(templateId, name,
        active, alias, templateType, layoutTemplate)
    {
        Subject = subject;
        HtmlBody = htmlBody;
        TextBody = textBody;
        AssociatedServerId = associatedServerId;
    }

    /// <summary>Gets the template subject content.</summary>
    public string? Subject { [UsedImplicitly] get; }

    /// <summary>Gets the template HTML body content.</summary>
    public string? HtmlBody { [UsedImplicitly] get; }

    /// <summary>Gets the template text body content.</summary>
    public string? TextBody { [UsedImplicitly] get; }

    /// <summary>Gets the associated Postmark server ID.</summary>
    public long AssociatedServerId { [UsedImplicitly] get; }
}