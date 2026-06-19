using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents webhook trigger settings.</summary>
public sealed record WebhookTriggers
{
    /// <summary>Gets the open trigger settings.</summary>
    public WebhookOpenTrigger? Open { [UsedImplicitly] get; init; }

    /// <summary>Gets the click trigger settings.</summary>
    public WebhookBasicTrigger? Click { [UsedImplicitly] get; init; }

    /// <summary>Gets the delivery trigger settings.</summary>
    public WebhookBasicTrigger? Delivery { [UsedImplicitly] get; init; }

    /// <summary>Gets the bounce trigger settings.</summary>
    public WebhookContentTrigger? Bounce { [UsedImplicitly] get; init; }

    /// <summary>Gets the spam complaint trigger settings.</summary>
    public WebhookContentTrigger? SpamComplaint { [UsedImplicitly] get; init; }

    /// <summary>Gets the subscription change trigger settings.</summary>
    public WebhookBasicTrigger? SubscriptionChange { [UsedImplicitly] get; init; }
}