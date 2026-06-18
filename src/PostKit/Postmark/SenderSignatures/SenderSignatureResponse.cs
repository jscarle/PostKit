using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal sealed class SenderSignatureResponse : SenderSignatureSummaryResponse
{
    [JsonPropertyName("SPFVerified")]
    public bool? SpfVerified { get; [UsedImplicitly] init; }

    [JsonPropertyName("SPFHost")]
    public string? SpfHost { get; [UsedImplicitly] init; }

    [JsonPropertyName("SPFTextValue")]
    public string? SpfTextValue { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMVerified")]
    public bool? DkimVerified { get; [UsedImplicitly] init; }

    [JsonPropertyName("WeakDKIM")]
    public bool? WeakDkim { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMHost")]
    public string? DkimHost { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMTextValue")]
    public string? DkimTextValue { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMPendingHost")]
    public string? DkimPendingHost { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMPendingTextValue")]
    public string? DkimPendingTextValue { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMRevokedHost")]
    public string? DkimRevokedHost { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMRevokedTextValue")]
    public string? DkimRevokedTextValue { get; [UsedImplicitly] init; }

    [JsonPropertyName("SafeToRemoveRevokedKeyFromDNS")]
    public bool? SafeToRemoveRevokedKeyFromDns { get; [UsedImplicitly] init; }

    [JsonPropertyName("DKIMUpdateStatus")]
    public string? DkimUpdateStatus { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReturnPathDomain")]
    public string? ReturnPathDomain { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReturnPathDomainVerified")]
    public bool? ReturnPathDomainVerified { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReturnPathDomainCNAMEValue")]
    public string? ReturnPathDomainCNameValue { get; [UsedImplicitly] init; }

    [JsonPropertyName("ConfirmationPersonalNote")]
    public string? ConfirmationPersonalNote { get; [UsedImplicitly] init; }
}
