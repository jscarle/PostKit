using JetBrains.Annotations;

namespace PostKit.InboundRules;

/// <summary>Represents a successful inbound rule trigger deletion.</summary>
public sealed record InboundRuleTriggerDeletion
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}