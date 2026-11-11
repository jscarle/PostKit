using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a webhook trigger that only has an enabled flag.</summary>
public sealed record WebhookBasicTrigger
{
    /// <summary>Gets whether the trigger is enabled.</summary>
    public required bool Enabled { [UsedImplicitly] get; init; }

    /// <summary>Gets the verification state returned by Postmark. This read-only provider field is excluded from mutations.</summary>
    public WebhookVerificationStatus? Status { [UsedImplicitly] get; init; }
}