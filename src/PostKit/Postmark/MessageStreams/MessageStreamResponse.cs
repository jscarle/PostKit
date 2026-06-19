using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.MessageStreams;

internal sealed class MessageStreamResponse
{
    [JsonPropertyName("ID")]
    public string? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("ServerID")]
    public long? ServerId { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Description")]
    public string? Description { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageStreamType")]
    public string? MessageStreamType { get; [UsedImplicitly] init; }

    [JsonPropertyName("CreatedAt")]
    public DateTimeOffset? CreatedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("UpdatedAt")]
    public DateTimeOffset? UpdatedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("ArchivedAt")]
    public DateTimeOffset? ArchivedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("ExpectedPurgeDate")]
    public DateTimeOffset? ExpectedPurgeDate { get; [UsedImplicitly] init; }

    [JsonPropertyName("SubscriptionManagementConfiguration")]
    public MessageStreamSubscriptionManagementResponse? SubscriptionManagementConfiguration { get; [UsedImplicitly] init; }
}