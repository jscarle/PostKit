using JetBrains.Annotations;

namespace PostKit.InboundRules;

/// <summary>Represents an inbound rule trigger returned by Postmark.</summary>
public sealed record InboundRuleTrigger
{
    /// <summary>Gets the trigger ID.</summary>
    public required long Id { [UsedImplicitly] get; init; }

    /// <summary>Gets the blocked email address or domain.</summary>
    public required string Rule { [UsedImplicitly] get; init; }
}