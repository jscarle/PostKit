using JetBrains.Annotations;

namespace PostKit.Servers;

/// <summary>Represents filters for listing Postmark servers.</summary>
public sealed record ServerQuery
{
    /// <summary>Gets the optional server name filter.</summary>
    public string? Name { [UsedImplicitly] get; init; }
}