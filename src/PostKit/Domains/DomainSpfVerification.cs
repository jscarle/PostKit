using JetBrains.Annotations;

namespace PostKit.Domains;

/// <summary>Represents an SPF verification result for a sender domain.</summary>
public sealed record DomainSpfVerification
{
    /// <summary>Gets the SPF host value.</summary>
    public required string? SpfHost { [UsedImplicitly] get; init; }

    /// <summary>Gets whether SPF is verified.</summary>
    public required bool SpfVerified { [UsedImplicitly] get; init; }

    /// <summary>Gets the SPF TXT value.</summary>
    public required string? SpfTextValue { [UsedImplicitly] get; init; }
}
