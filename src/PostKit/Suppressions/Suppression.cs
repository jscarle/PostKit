using JetBrains.Annotations;

namespace PostKit.Suppressions;

/// <summary>Represents a single suppressed recipient returned from Postmark.</summary>
public sealed record Suppression
{
    internal Suppression(string emailAddress, SuppressionReason reason, SuppressionOrigin origin, DateTimeOffset createdAt)
    {
        EmailAddress = emailAddress;
        Reason = reason;
        Origin = origin;
        CreatedAt = createdAt;
    }

    /// <summary>Gets the suppressed email address.</summary>
    public string EmailAddress { [UsedImplicitly] get; }

    /// <summary>Gets the reason the address is suppressed.</summary>
    public SuppressionReason Reason { [UsedImplicitly] get; }

    /// <summary>Gets the origin that added the address to the suppression list.</summary>
    public SuppressionOrigin Origin { [UsedImplicitly] get; }

    /// <summary>Gets the timestamp when Postmark created the suppression.</summary>
    public DateTimeOffset CreatedAt { [UsedImplicitly] get; }
}