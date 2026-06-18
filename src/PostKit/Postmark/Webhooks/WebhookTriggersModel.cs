using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookTriggersModel
{
    [JsonPropertyName("Open")]
    public WebhookOpenTriggerModel? Open { get; [UsedImplicitly] init; }

    [JsonPropertyName("Click")]
    public WebhookBasicTriggerModel? Click { get; [UsedImplicitly] init; }

    [JsonPropertyName("Delivery")]
    public WebhookBasicTriggerModel? Delivery { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bounce")]
    public WebhookContentTriggerModel? Bounce { get; [UsedImplicitly] init; }

    [JsonPropertyName("SpamComplaint")]
    public WebhookContentTriggerModel? SpamComplaint { get; [UsedImplicitly] init; }

    [JsonPropertyName("SubscriptionChange")]
    public WebhookBasicTriggerModel? SubscriptionChange { get; [UsedImplicitly] init; }
}
