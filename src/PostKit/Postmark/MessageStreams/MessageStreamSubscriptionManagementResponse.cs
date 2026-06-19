using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.MessageStreams;

internal sealed class MessageStreamSubscriptionManagementResponse
{
    [JsonPropertyName("UnsubscribeHandlingType")]
    public string? UnsubscribeHandlingType { get; [UsedImplicitly] init; }
}