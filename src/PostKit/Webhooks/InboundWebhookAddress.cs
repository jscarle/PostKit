using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an address in a Postmark inbound webhook.</summary>
public sealed record InboundWebhookAddress
{
    /// <summary>Gets the email address.</summary>
    [JsonPropertyName("Email")]
    public required string Email { [UsedImplicitly] get; init; }

    /// <summary>Gets the display name. Postmark uses an empty string when no display name is available.</summary>
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the plus-addressing mailbox hash. Postmark uses an empty string when no mailbox hash is available.</summary>
    [JsonPropertyName("MailboxHash")]
    public required string MailboxHash { [UsedImplicitly] get; init; }
}
