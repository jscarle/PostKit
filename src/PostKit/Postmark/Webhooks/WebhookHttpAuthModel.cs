using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookHttpAuthModel
{
    [JsonPropertyName("Username")]
    public string? Username { get; [UsedImplicitly] init; }

    [JsonPropertyName("Password")]
    public string? Password { get; [UsedImplicitly] init; }
}