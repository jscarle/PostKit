using System.Text.Json.Serialization;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Identifies a Postmark webhook event type.</summary>
[JsonConverter(typeof(WebhookTriggerTypeJsonConverter))]
public enum WebhookTriggerType
{
    /// <summary>The Open value reported by Postmark.</summary>
    Open = 0,

    /// <summary>The Click value reported by Postmark.</summary>
    Click = 1,

    /// <summary>The Delivery value reported by Postmark.</summary>
    Delivery = 2,

    /// <summary>The Bounce value reported by Postmark.</summary>
    Bounce = 3,

    /// <summary>The SpamComplaint value reported by Postmark.</summary>
    SpamComplaint = 4,

    /// <summary>The SubscriptionChange value reported by Postmark.</summary>
    SubscriptionChange = 5
}
