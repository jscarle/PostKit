using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents a successful sender signature deletion.</summary>
public sealed record SenderSignatureDeletion
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}