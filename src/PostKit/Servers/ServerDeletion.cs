using JetBrains.Annotations;

namespace PostKit.Servers;

/// <summary>Represents a successful server deletion.</summary>
public sealed record ServerDeletion
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}
