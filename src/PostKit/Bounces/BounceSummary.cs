using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents a bounce summary entry returned by Postmark delivery stats.</summary>
public sealed record BounceSummary
{
    internal BounceSummary(BounceType? type, string name, int count)
    {
        Type = type;
        Name = name;
        Count = count;
    }

    /// <summary>Gets the bounce type for the entry when Postmark provides one.</summary>
    public BounceType? Type { [UsedImplicitly] get; }

    /// <summary>Gets the display name for the entry.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets the count associated with the entry.</summary>
    public int Count { [UsedImplicitly] get; }
}
