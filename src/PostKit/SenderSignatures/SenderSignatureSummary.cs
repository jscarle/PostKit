using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents a Postmark sender signature summary.</summary>
public record SenderSignatureSummary
{
    internal SenderSignatureSummary(long id, string domain, string emailAddress, string? replyToEmailAddress, string name, bool confirmed)
    {
        Id = id;
        Domain = domain;
        EmailAddress = emailAddress;
        ReplyToEmailAddress = replyToEmailAddress;
        Name = name;
        Confirmed = confirmed;
    }

    /// <summary>Gets the sender signature ID.</summary>
    public long Id { [UsedImplicitly] get; }

    /// <summary>Gets the sender domain.</summary>
    public string Domain { [UsedImplicitly] get; }

    /// <summary>Gets the sender email address.</summary>
    public string EmailAddress { [UsedImplicitly] get; }

    /// <summary>Gets the optional reply-to email address.</summary>
    public string? ReplyToEmailAddress { [UsedImplicitly] get; }

    /// <summary>Gets the sender display name.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets whether the sender signature is confirmed.</summary>
    public bool Confirmed { [UsedImplicitly] get; }
}
