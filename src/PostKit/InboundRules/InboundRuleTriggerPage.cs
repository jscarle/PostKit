using JetBrains.Annotations;

namespace PostKit.InboundRules;

/// <summary>Represents a page of inbound rule triggers.</summary>
public sealed record InboundRuleTriggerPage
{
    /// <summary>Gets the total number of matching triggers.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the inbound rule triggers returned for this page.</summary>
    public required IReadOnlyList<InboundRuleTrigger> InboundRules { [UsedImplicitly] get; init; }
}