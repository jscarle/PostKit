using JetBrains.Annotations;

namespace PostKit.Emails;

/// <summary>Represents the response returned after Postmark accepts an email for delivery.</summary>
public sealed record SendEmailResponse
{
    /// <summary>Gets the identifier assigned to the accepted message.</summary>
    public Guid MessageId { [UsedImplicitly] get; }

    /// <summary>Gets the RFC-style internet message identifier for the accepted message.</summary>
    public string InternetMessageId { [UsedImplicitly] get; }

    /// <summary>Gets the recipient string returned by Postmark, when included.</summary>
    public string? To { [UsedImplicitly] get; }

    /// <summary>Gets the time Postmark accepted the email.</summary>
    public DateTimeOffset SubmittedAt { [UsedImplicitly] get; }

    internal SendEmailResponse(Guid messageId, string? to, DateTimeOffset submittedAt)
    {
        MessageId = messageId;
        InternetMessageId = FormatInternetMessageId(messageId);
        To = to;
        SubmittedAt = submittedAt;
    }

    internal static string FormatInternetMessageId(Guid messageId)
    {
        return $"<{messageId:D}@mtasv.net>";
    }
}
