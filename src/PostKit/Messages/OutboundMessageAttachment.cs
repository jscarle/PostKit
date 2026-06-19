using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents an attachment returned with an outbound message summary.</summary>
public sealed record OutboundMessageAttachment
{
    internal OutboundMessageAttachment(string name, string? contentType, string? content, string? contentId)
    {
        Name = name;
        ContentType = contentType;
        Content = content;
        ContentId = contentId;
    }

    /// <summary>Gets the attachment file name.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets the MIME content type when Postmark includes it.</summary>
    public string? ContentType { [UsedImplicitly] get; }

    /// <summary>Gets the Base64 attachment content when Postmark includes it.</summary>
    public string? Content { [UsedImplicitly] get; }

    /// <summary>Gets the content identifier when Postmark includes it.</summary>
    public string? ContentId { [UsedImplicitly] get; }
}