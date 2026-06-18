using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents a Postmark sender signature.</summary>
public sealed record SenderSignature : SenderSignatureSummary
{
    internal SenderSignature(long id, string domain, string emailAddress, string? replyToEmailAddress, string name, bool confirmed, bool spfVerified, string? spfHost, string? spfTextValue, bool dkimVerified, bool weakDkim, string? dkimHost,
        string? dkimTextValue, string? dkimPendingHost, string? dkimPendingTextValue, string? dkimRevokedHost, string? dkimRevokedTextValue, bool? safeToRemoveRevokedKeyFromDns, string? dkimUpdateStatus, string? returnPathDomain,
        bool returnPathDomainVerified, string? returnPathDomainCNameValue) : base(id, domain, emailAddress, replyToEmailAddress, name, confirmed)
    {
        SpfVerified = spfVerified;
        SpfHost = spfHost;
        SpfTextValue = spfTextValue;
        DkimVerified = dkimVerified;
        WeakDkim = weakDkim;
        DkimHost = dkimHost;
        DkimTextValue = dkimTextValue;
        DkimPendingHost = dkimPendingHost;
        DkimPendingTextValue = dkimPendingTextValue;
        DkimRevokedHost = dkimRevokedHost;
        DkimRevokedTextValue = dkimRevokedTextValue;
        SafeToRemoveRevokedKeyFromDns = safeToRemoveRevokedKeyFromDns;
        DkimUpdateStatus = dkimUpdateStatus;
        ReturnPathDomain = returnPathDomain;
        ReturnPathDomainVerified = returnPathDomainVerified;
        ReturnPathDomainCNameValue = returnPathDomainCNameValue;
    }

    /// <summary>Gets whether SPF is verified.</summary>
    public bool SpfVerified { [UsedImplicitly] get; }

    /// <summary>Gets the SPF host value.</summary>
    public string? SpfHost { [UsedImplicitly] get; }

    /// <summary>Gets the SPF TXT value.</summary>
    public string? SpfTextValue { [UsedImplicitly] get; }

    /// <summary>Gets whether DKIM is verified.</summary>
    public bool DkimVerified { [UsedImplicitly] get; }

    /// <summary>Gets whether the signature uses weak DKIM.</summary>
    public bool WeakDkim { [UsedImplicitly] get; }

    /// <summary>Gets the DKIM host value.</summary>
    public string? DkimHost { [UsedImplicitly] get; }

    /// <summary>Gets the DKIM TXT value.</summary>
    public string? DkimTextValue { [UsedImplicitly] get; }

    /// <summary>Gets the pending DKIM host value.</summary>
    public string? DkimPendingHost { [UsedImplicitly] get; }

    /// <summary>Gets the pending DKIM TXT value.</summary>
    public string? DkimPendingTextValue { [UsedImplicitly] get; }

    /// <summary>Gets the revoked DKIM host value.</summary>
    public string? DkimRevokedHost { [UsedImplicitly] get; }

    /// <summary>Gets the revoked DKIM TXT value.</summary>
    public string? DkimRevokedTextValue { [UsedImplicitly] get; }

    /// <summary>Gets whether the revoked DKIM key is safe to remove from DNS.</summary>
    public bool? SafeToRemoveRevokedKeyFromDns { [UsedImplicitly] get; }

    /// <summary>Gets the raw DKIM update status returned by Postmark.</summary>
    public string? DkimUpdateStatus { [UsedImplicitly] get; }

    /// <summary>Gets the return-path domain.</summary>
    public string? ReturnPathDomain { [UsedImplicitly] get; }

    /// <summary>Gets whether the return-path domain is verified.</summary>
    public bool ReturnPathDomainVerified { [UsedImplicitly] get; }

    /// <summary>Gets the return-path CNAME value.</summary>
    public string? ReturnPathDomainCNameValue { [UsedImplicitly] get; }
}