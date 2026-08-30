using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents parameters for editing a webhook.</summary>
public sealed record WebhookEditParameters
{
    /// <summary>Gets the new webhook URL.</summary>
    public string? Url { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional HTTP basic authentication settings.</summary>
    public WebhookHttpAuth? HttpAuth { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional custom HTTP headers.</summary>
    public IReadOnlyList<WebhookHeader>? HttpHeaders { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional trigger settings.</summary>
    public WebhookTriggers? Triggers { [UsedImplicitly] get; init; }
}
