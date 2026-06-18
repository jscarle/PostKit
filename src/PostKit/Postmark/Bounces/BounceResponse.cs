using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class BounceResponse
{
    [JsonPropertyName("RecordType")]
    public string? RecordType { get; [UsedImplicitly] init; }

    [JsonPropertyName("ID")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Type")]
    public string? Type { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Tag")]
    public string? Tag { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageID")]
    public string? MessageId { get; [UsedImplicitly] init; }

    [JsonPropertyName("ServerID")]
    public long? ServerId { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { get; [UsedImplicitly] init; }

    [JsonPropertyName("Description")]
    public string? Description { get; [UsedImplicitly] init; }

    [JsonPropertyName("Details")]
    public string? Details { get; [UsedImplicitly] init; }

    [JsonPropertyName("Email")]
    public string? Email { get; [UsedImplicitly] init; }

    [JsonPropertyName("From")]
    public string? From { get; [UsedImplicitly] init; }

    [JsonPropertyName("BouncedAt")]
    public DateTimeOffset? BouncedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("DumpAvailable")]
    public bool? DumpAvailable { get; [UsedImplicitly] init; }

    [JsonPropertyName("Inactive")]
    public bool? Inactive { get; [UsedImplicitly] init; }

    [JsonPropertyName("CanActivate")]
    public bool? CanActivate { get; [UsedImplicitly] init; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; [UsedImplicitly] init; }

    [JsonPropertyName("Content")]
    public string? Content { get; [UsedImplicitly] init; }
}