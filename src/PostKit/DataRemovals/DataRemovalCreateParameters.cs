using JetBrains.Annotations;

namespace PostKit.DataRemovals;

/// <summary>Represents parameters for creating a Postmark data removal request.</summary>
public sealed record DataRemovalCreateParameters
{
    /// <summary>Gets the email address of the user making the request.</summary>
    public required string RequestedBy { [UsedImplicitly] get; init; }

    /// <summary>Gets the recipient email address whose data should be removed.</summary>
    public required string RequestedFor { [UsedImplicitly] get; init; }

    /// <summary>Gets whether Postmark should notify <see cref="RequestedBy" /> when the request is complete.</summary>
    public required bool NotifyWhenCompleted { [UsedImplicitly] get; init; }
}
