using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the verification result for one webhook trigger.</summary>
public sealed record WebhookVerificationTriggerResult
{
    internal WebhookVerificationTriggerResult(string triggerType, bool success, int statusCode, string message)
    {
        TriggerType = triggerType;
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }

    /// <summary>Gets the provider's event type.</summary>
    public string TriggerType { [UsedImplicitly] get; }

    /// <summary>Gets whether this trigger passed verification.</summary>
    public bool Success { [UsedImplicitly] get; }

    /// <summary>Gets the callback HTTP status code, or zero when no response was received.</summary>
    public int StatusCode { [UsedImplicitly] get; }

    /// <summary>Gets the provider's result message.</summary>
    public string Message { [UsedImplicitly] get; }
}
