using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents a successful sender signature action.</summary>
public sealed record SenderSignatureAction
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}