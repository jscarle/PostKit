using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the outcome of testing a webhook.</summary>
public sealed record WebhookVerification
{
    internal WebhookVerification(long? id, string url, bool success, IReadOnlyList<WebhookVerificationTriggerResult> results, string message)
    {
        Id = id;
        Url = url;
        Success = success;
        Results = results;
        Message = message;
    }

    /// <summary>Gets the webhook identifier, when assigned.</summary>
    public long? Id { [UsedImplicitly] get; }

    /// <summary>Gets the tested webhook URL.</summary>
    public string Url { [UsedImplicitly] get; }

    /// <summary>Gets whether every enabled trigger passed verification.</summary>
    public bool Success { [UsedImplicitly] get; }

    /// <summary>Gets the individual trigger results.</summary>
    public IReadOnlyList<WebhookVerificationTriggerResult> Results { [UsedImplicitly] get; }

    /// <summary>Gets the provider's verification summary.</summary>
    public string Message { [UsedImplicitly] get; }
}
