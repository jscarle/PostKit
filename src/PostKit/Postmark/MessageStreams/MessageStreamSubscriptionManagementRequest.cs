using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.MessageStreams;

internal sealed class MessageStreamSubscriptionManagementRequest
{
    [JsonPropertyName("UnsubscribeHandlingType")]
    public string? UnsubscribeHandlingType { [UsedImplicitly] get; init; }
}
