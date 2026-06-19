using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.MessageStreams;

internal sealed class MessageStreamRequest
{
    [JsonPropertyName("ID")]
    public string? Id { [UsedImplicitly] get; init; }

    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("Description")]
    public string? Description { [UsedImplicitly] get; init; }

    [JsonPropertyName("MessageStreamType")]
    public string? MessageStreamType { [UsedImplicitly] get; init; }

    [JsonPropertyName("SubscriptionManagementConfiguration")]
    public MessageStreamSubscriptionManagementRequest? SubscriptionManagementConfiguration { [UsedImplicitly] get; init; }
}