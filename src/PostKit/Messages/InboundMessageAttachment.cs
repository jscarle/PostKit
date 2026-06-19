using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents an inbound message attachment.</summary>
public sealed record InboundMessageAttachment
{
    /// <summary>Gets the attachment name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the content ID, when available.</summary>
    public required string? ContentId { [UsedImplicitly] get; init; }

    /// <summary>Gets the content type.</summary>
    public required string ContentType { [UsedImplicitly] get; init; }

    /// <summary>Gets the content length in bytes.</summary>
    public required long ContentLength { [UsedImplicitly] get; init; }
}