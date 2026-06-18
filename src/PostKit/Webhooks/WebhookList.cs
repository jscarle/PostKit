using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the webhooks returned for a server.</summary>
public sealed record WebhookList
{
    /// <summary>Gets the webhooks returned by Postmark.</summary>
    public required IReadOnlyList<Webhook> Webhooks { [UsedImplicitly] get; init; }
}
