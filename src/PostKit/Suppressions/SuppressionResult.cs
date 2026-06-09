using JetBrains.Annotations;

namespace PostKit.Suppressions;

/// <summary>Represents a per-address suppression create or delete result returned from Postmark.</summary>
public sealed record SuppressionResult
{
    /// <summary>Gets the email address that the suppression change was requested for.</summary>
    public string EmailAddress { [UsedImplicitly] get; }

    /// <summary>Gets the per-address suppression change status.</summary>
    public SuppressionStatus Status { [UsedImplicitly] get; }

    /// <summary>Gets the server-provided message for failed changes, if Postmark returned one.</summary>
    public string? Message { [UsedImplicitly] get; }

    internal SuppressionResult(string emailAddress, SuppressionStatus status, string? message)
    {
        EmailAddress = emailAddress;
        Status = status;
        Message = message;
    }
}
