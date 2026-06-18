using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents parameters for editing a Postmark sender signature.</summary>
public sealed record SenderSignatureEditParameters
{
    /// <summary>Gets the sender display name.</summary>
    public string? Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional reply-to email address.</summary>
    public string? ReplyToEmailAddress { [UsedImplicitly] get; init; }
}