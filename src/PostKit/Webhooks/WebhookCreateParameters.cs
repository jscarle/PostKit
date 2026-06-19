using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents parameters for creating a webhook.</summary>
public sealed record WebhookCreateParameters
{
    /// <summary>Gets the webhook URL.</summary>
    public required string Url { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional message stream ID. When omitted, Postmark uses the default outbound stream.</summary>
    public string? MessageStreamId { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional HTTP basic authentication settings.</summary>
    public WebhookHttpAuth? HttpAuth { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional custom HTTP headers.</summary>
    public IReadOnlyList<WebhookHeader>? HttpHeaders { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional trigger settings.</summary>
    public WebhookTriggers? Triggers { [UsedImplicitly] get; init; }
}