using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a custom HTTP header sent with webhook requests.</summary>
public sealed record WebhookHeader
{
    /// <summary>Gets the header name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the header value.</summary>
    public required string Value { [UsedImplicitly] get; init; }
}
