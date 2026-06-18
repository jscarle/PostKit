using JetBrains.Annotations;

namespace PostKit.Domains;

/// <summary>Represents parameters for editing a Postmark sender domain.</summary>
public sealed record DomainEditParameters
{
    /// <summary>Gets the return-path domain.</summary>
    public required string ReturnPathDomain { [UsedImplicitly] get; init; }
}