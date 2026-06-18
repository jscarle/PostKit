using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the open-tracking webhook trigger.</summary>
public sealed record WebhookOpenTrigger
{
    /// <summary>Gets whether the trigger is enabled.</summary>
    public required bool Enabled { [UsedImplicitly] get; init; }

    /// <summary>Gets whether Postmark should POST only the first open.</summary>
    public required bool PostFirstOpenOnly { [UsedImplicitly] get; init; }
}
