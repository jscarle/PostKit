using System.Text.Json.Serialization;
using PostKit.Postmark.Common;

namespace PostKit.Webhooks;

/// <summary>Represents the verification state reported by Postmark.</summary>
[JsonConverter(typeof(WebhookVerificationStatusJsonConverter))]
public enum WebhookVerificationStatus
{
    /// <summary>The Verified value reported by Postmark.</summary>
    Verified = 0,

    /// <summary>The Unverified value reported by Postmark.</summary>
    Unverified = 1
}
