using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal class SenderSignatureSummaryResponse
{
    [JsonPropertyName("ID")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Domain")]
    public string? Domain { get; [UsedImplicitly] init; }

    [JsonPropertyName("EmailAddress")]
    public string? EmailAddress { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReplyToEmailAddress")]
    public string? ReplyToEmailAddress { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Confirmed")]
    public bool? Confirmed { get; [UsedImplicitly] init; }
}
