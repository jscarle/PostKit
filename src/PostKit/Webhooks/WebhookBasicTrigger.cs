using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a webhook trigger that only has an enabled flag.</summary>
public sealed record WebhookBasicTrigger
{
    /// <summary>Gets whether the trigger is enabled.</summary>
    public required bool Enabled { [UsedImplicitly] get; init; }
}