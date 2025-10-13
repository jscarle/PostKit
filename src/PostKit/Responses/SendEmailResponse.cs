using JetBrains.Annotations;

namespace PostKit.Responses;

/// <summary>Represents the response returned after Postmark accepts an email for delivery.</summary>
public sealed record SendEmailResponse
{
    /// <summary>Gets the identifier assigned to the accepted message.</summary>
    public string MessageId { [UsedImplicitly] get; }

    /// <summary>Initializes a new instance of the <see cref="SendEmailResponse"/> class.</summary>
    /// <param name="messageId">The identifier returned by Postmark for the message.</param>
    internal SendEmailResponse(string messageId)
    {
        MessageId = $"<{messageId}@mtasv.net>";
    }
}
