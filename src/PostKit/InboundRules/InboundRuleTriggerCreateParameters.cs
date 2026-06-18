using JetBrains.Annotations;

namespace PostKit.InboundRules;

/// <summary>Represents parameters for creating an inbound rule trigger.</summary>
public sealed record InboundRuleTriggerCreateParameters
{
    /// <summary>Gets the blocked email address or domain.</summary>
    public required string Rule { [UsedImplicitly] get; init; }
}