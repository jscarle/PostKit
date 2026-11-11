using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a webhook trigger that can include message content.</summary>
public sealed record WebhookContentTrigger
{
    /// <summary>Gets whether the trigger is enabled.</summary>
    public required bool Enabled { [UsedImplicitly] get; init; }

    /// <summary>Gets whether Postmark should include message content in the webhook POST.</summary>
    public required bool IncludeContent { [UsedImplicitly] get; init; }

    /// <summary>Gets the verification state returned by Postmark. This read-only provider field is excluded from mutations.</summary>
    public WebhookVerificationStatus? Status { [UsedImplicitly] get; init; }
}