using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Servers;

internal sealed class ServerResponse
{
    [JsonPropertyName("ID")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("ApiTokens")]
    public List<string?>? ApiTokens { get; [UsedImplicitly] init; }

    [JsonPropertyName("Color")]
    public string? Color { get; [UsedImplicitly] init; }

    [JsonPropertyName("SmtpApiActivated")]
    public bool? SmtpApiActivated { get; [UsedImplicitly] init; }

    [JsonPropertyName("RawEmailEnabled")]
    public bool? RawEmailEnabled { get; [UsedImplicitly] init; }

    [JsonPropertyName("DeliveryType")]
    public string? DeliveryType { get; [UsedImplicitly] init; }

    [JsonPropertyName("ServerLink")]
    public string? ServerLink { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundAddress")]
    public string? InboundAddress { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundHookUrl")]
    public string? InboundHookUrl { get; [UsedImplicitly] init; }

    [JsonPropertyName("BounceHookUrl")]
    public string? BounceHookUrl { get; [UsedImplicitly] init; }

    [JsonPropertyName("OpenHookUrl")]
    public string? OpenHookUrl { get; [UsedImplicitly] init; }

    [JsonPropertyName("DeliveryHookUrl")]
    public string? DeliveryHookUrl { get; [UsedImplicitly] init; }

    [JsonPropertyName("PostFirstOpenOnly")]
    public bool? PostFirstOpenOnly { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundDomain")]
    public string? InboundDomain { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundHash")]
    public string? InboundHash { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundSpamThreshold")]
    public int? InboundSpamThreshold { get; [UsedImplicitly] init; }

    [JsonPropertyName("TrackOpens")]
    public bool? TrackOpens { get; [UsedImplicitly] init; }

    [JsonPropertyName("TrackLinks")]
    public string? TrackLinks { get; [UsedImplicitly] init; }

    [JsonPropertyName("IncludeBounceContentInHook")]
    public bool? IncludeBounceContentInHook { get; [UsedImplicitly] init; }

    [JsonPropertyName("ClickHookUrl")]
    public string? ClickHookUrl { get; [UsedImplicitly] init; }

    [JsonPropertyName("EnableSmtpApiErrorHooks")]
    public bool? EnableSmtpApiErrorHooks { get; [UsedImplicitly] init; }
}
