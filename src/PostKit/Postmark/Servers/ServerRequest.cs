using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Servers;

internal sealed class ServerRequest
{
    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("Color")]
    public string? Color { [UsedImplicitly] get; init; }

    [JsonPropertyName("SmtpApiActivated")]
    public bool? SmtpApiActivated { [UsedImplicitly] get; init; }

    [JsonPropertyName("RawEmailEnabled")]
    public bool? RawEmailEnabled { [UsedImplicitly] get; init; }

    [JsonPropertyName("DeliveryType")]
    public string? DeliveryType { [UsedImplicitly] get; init; }

    [JsonPropertyName("InboundHookUrl")]
    public string? InboundHookUrl { [UsedImplicitly] get; init; }

    [JsonPropertyName("BounceHookUrl")]
    public string? BounceHookUrl { [UsedImplicitly] get; init; }

    [JsonPropertyName("OpenHookUrl")]
    public string? OpenHookUrl { [UsedImplicitly] get; init; }

    [JsonPropertyName("DeliveryHookUrl")]
    public string? DeliveryHookUrl { [UsedImplicitly] get; init; }

    [JsonPropertyName("PostFirstOpenOnly")]
    public bool? PostFirstOpenOnly { [UsedImplicitly] get; init; }

    [JsonPropertyName("InboundDomain")]
    public string? InboundDomain { [UsedImplicitly] get; init; }

    [JsonPropertyName("InboundSpamThreshold")]
    public int? InboundSpamThreshold { [UsedImplicitly] get; init; }

    [JsonPropertyName("TrackOpens")]
    public bool? TrackOpens { [UsedImplicitly] get; init; }

    [JsonPropertyName("TrackLinks")]
    public string? TrackLinks { [UsedImplicitly] get; init; }

    [JsonPropertyName("IncludeBounceContentInHook")]
    public bool? IncludeBounceContentInHook { [UsedImplicitly] get; init; }

    [JsonPropertyName("ClickHookUrl")]
    public string? ClickHookUrl { [UsedImplicitly] get; init; }

    [JsonPropertyName("EnableSmtpApiErrorHooks")]
    public bool? EnableSmtpApiErrorHooks { [UsedImplicitly] get; init; }
}
