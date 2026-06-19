using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents operating system details for a tracked message event.</summary>
public sealed record MessageTrackingOperatingSystem
{
    /// <summary>Gets the operating system name.</summary>
    public required string? Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the operating system company.</summary>
    public required string? Company { [UsedImplicitly] get; init; }

    /// <summary>Gets the operating system family.</summary>
    public required string? Family { [UsedImplicitly] get; init; }
}