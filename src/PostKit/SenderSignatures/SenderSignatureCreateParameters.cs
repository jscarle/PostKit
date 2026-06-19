using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents parameters for creating a Postmark sender signature.</summary>
public sealed record SenderSignatureCreateParameters
{
    /// <summary>Gets the sender email address.</summary>
    public required string FromEmail { [UsedImplicitly] get; init; }

    /// <summary>Gets the sender display name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional reply-to email address.</summary>
    public string? ReplyToEmailAddress { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional return-path domain.</summary>
    public string? ReturnPathDomain { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional personal note sent with the confirmation email.</summary>
    public string? ConfirmationPersonalNote { [UsedImplicitly] get; init; }
}