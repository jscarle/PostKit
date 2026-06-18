using JetBrains.Annotations;

namespace PostKit.Domains;

/// <summary>Represents a DKIM rotation result for a sender domain.</summary>
public sealed record DomainDkimRotation
{
    /// <summary>Gets the domain ID.</summary>
    public required long Id { [UsedImplicitly] get; init; }

    /// <summary>Gets the domain name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets whether DKIM is verified.</summary>
    public required bool DkimVerified { [UsedImplicitly] get; init; }

    /// <summary>Gets whether the domain uses weak DKIM.</summary>
    public required bool WeakDkim { [UsedImplicitly] get; init; }

    /// <summary>Gets the DKIM host value.</summary>
    public required string? DkimHost { [UsedImplicitly] get; init; }

    /// <summary>Gets the DKIM TXT value.</summary>
    public required string? DkimTextValue { [UsedImplicitly] get; init; }

    /// <summary>Gets the pending DKIM host value.</summary>
    public required string? DkimPendingHost { [UsedImplicitly] get; init; }

    /// <summary>Gets the pending DKIM TXT value.</summary>
    public required string? DkimPendingTextValue { [UsedImplicitly] get; init; }

    /// <summary>Gets the revoked DKIM host value.</summary>
    public required string? DkimRevokedHost { [UsedImplicitly] get; init; }

    /// <summary>Gets the revoked DKIM TXT value.</summary>
    public required string? DkimRevokedTextValue { [UsedImplicitly] get; init; }

    /// <summary>Gets whether the revoked DKIM key is safe to remove from DNS.</summary>
    public required bool? SafeToRemoveRevokedKeyFromDns { [UsedImplicitly] get; init; }

    /// <summary>Gets the raw DKIM update status returned by Postmark.</summary>
    public required string? DkimUpdateStatus { [UsedImplicitly] get; init; }
}