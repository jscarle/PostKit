using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a successful webhook deletion.</summary>
public sealed record WebhookDeletion
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}