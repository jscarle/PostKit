using JetBrains.Annotations;

namespace PostKit.Domains;

/// <summary>Represents a successful sender domain deletion.</summary>
public sealed record DomainDeletion
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}