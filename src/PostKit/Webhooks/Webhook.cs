using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a webhook returned from Postmark.</summary>
public sealed record Webhook
{
    internal Webhook(long id, string url, string messageStream, WebhookHttpAuth? httpAuth, IReadOnlyList<WebhookHeader> httpHeaders, WebhookTriggers triggers)
    {
        Id = id;
        Url = url;
        MessageStream = messageStream;
        HttpAuth = httpAuth;
        HttpHeaders = httpHeaders;
        Triggers = triggers;
    }

    /// <summary>Gets the webhook ID.</summary>
    public long Id { [UsedImplicitly] get; }

    /// <summary>Gets the webhook URL.</summary>
    public string Url { [UsedImplicitly] get; }

    /// <summary>Gets the associated message stream ID.</summary>
    public string MessageStream { [UsedImplicitly] get; }

    /// <summary>Gets the HTTP basic authentication settings, when configured.</summary>
    public WebhookHttpAuth? HttpAuth { [UsedImplicitly] get; }

    /// <summary>Gets custom HTTP headers.</summary>
    public IReadOnlyList<WebhookHeader> HttpHeaders { [UsedImplicitly] get; }

    /// <summary>Gets the webhook trigger settings.</summary>
    public WebhookTriggers Triggers { [UsedImplicitly] get; }
}
