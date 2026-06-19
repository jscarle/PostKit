using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents client details for a tracked message event.</summary>
public sealed record MessageTrackingClient
{
    /// <summary>Gets the client name.</summary>
    public required string? Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the client company.</summary>
    public required string? Company { [UsedImplicitly] get; init; }

    /// <summary>Gets the client family.</summary>
    public required string? Family { [UsedImplicitly] get; init; }
}