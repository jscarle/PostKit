using JetBrains.Annotations;

namespace PostKit.Domains;

/// <summary>Represents parameters for creating a Postmark sender domain.</summary>
public sealed record DomainCreateParameters
{
    /// <summary>Gets the domain name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional return-path domain.</summary>
    public string? ReturnPathDomain { [UsedImplicitly] get; init; }
}