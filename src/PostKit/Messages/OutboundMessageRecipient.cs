using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents an email address returned in an outbound message recipient list.</summary>
public sealed record OutboundMessageRecipient
{
    /// <summary>Gets the recipient email address.</summary>
    public string Email { [UsedImplicitly] get; }

    /// <summary>Gets the recipient display name when Postmark includes one.</summary>
    public string? Name { [UsedImplicitly] get; }

    internal OutboundMessageRecipient(string email, string? name)
    {
        Email = email;
        Name = name;
    }
}
