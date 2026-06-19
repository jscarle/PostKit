using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents HTTP basic authentication settings for a webhook.</summary>
public sealed record WebhookHttpAuth
{
    /// <summary>Gets the HTTP basic authentication username.</summary>
    public required string Username { [UsedImplicitly] get; init; }

    /// <summary>Gets the HTTP basic authentication password.</summary>
    public required string Password { [UsedImplicitly] get; init; }
}