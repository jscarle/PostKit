using JetBrains.Annotations;

namespace PostKit.Emails;

/// <summary>Represents the response returned after Postmark accepts an email for delivery.</summary>
public sealed record EmailSubmission
{
    /// <summary>Gets the identifier assigned to the accepted message.</summary>
    public Guid MessageId { [UsedImplicitly] get; }

    /// <summary>Gets the RFC-style internet message identifier for the accepted message.</summary>
    public string InternetMessageId { [UsedImplicitly] get; }

    /// <summary>Gets the recipient string returned by Postmark, when included.</summary>
    public string? To { [UsedImplicitly] get; }

    /// <summary>Gets the time Postmark accepted the email.</summary>
    public DateTimeOffset SubmittedAt { [UsedImplicitly] get; }

    internal EmailSubmission(Guid messageId, string? to, DateTimeOffset submittedAt, string? internetMessageId = null)
    {
        MessageId = messageId;
        InternetMessageId = string.IsNullOrWhiteSpace(internetMessageId)
            ? FormatInternetMessageId(messageId)
            : internetMessageId.Trim();
        To = to;
        SubmittedAt = submittedAt;
    }

    internal static string ResolveInternetMessageId(Guid messageId, IReadOnlyDictionary<string, string>? headers, bool requireKeepId)
    {
        var messageIdHeader = GetHeaderValue(headers, "Message-ID");
        if (string.IsNullOrWhiteSpace(messageIdHeader))
            return FormatInternetMessageId(messageId);

        if (requireKeepId)
        {
            var keepIdHeader = GetHeaderValue(headers, "X-PM-KeepID");
            if (!bool.TryParse(keepIdHeader?.Trim(), out var keepId) || !keepId)
                return FormatInternetMessageId(messageId);
        }

        return messageIdHeader.Trim();
    }

    private static string FormatInternetMessageId(Guid messageId)
    {
        return $"<{messageId:D}@mtasv.net>";
    }

    private static string? GetHeaderValue(IReadOnlyDictionary<string, string>? headers, string headerName)
    {
        if (headers is null)
            return null;

        if (headers.TryGetValue(headerName, out var value))
            return value;

        foreach (var header in headers)
            if (string.Equals(header.Key, headerName, StringComparison.OrdinalIgnoreCase))
                return header.Value;

        return null;
    }
}
