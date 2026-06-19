namespace PostKit.Messages;

/// <summary>Represents a single metadata field filter for outbound message searches.</summary>
public sealed record OutboundMessageMetadataFilter
{
    /// <summary>Gets the metadata field name. Postmark sends this as the suffix in a <c>metadata_</c> query parameter.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the metadata value to match.</summary>
    public required string Value { get; init; }
}