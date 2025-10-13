namespace PostKit;

/// <summary>
/// Represents an email attachment that can be sent with a <see cref="Email"/>.
/// </summary>
public sealed class Attachment
{
    private Attachment(string name, string contentType, string content, string? contentId)
    {
        Name = name;
        ContentType = contentType;
        Content = content;
        ContentId = contentId;
    }

    /// <summary>
    /// Gets the file name that will be presented to the email recipient.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the MIME content type of the attachment.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the Base64 encoded contents of the attachment.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the optional content identifier of the attachment when it should be embedded in the message body.
    /// </summary>
    public string? ContentId { get; }

    /// <summary>
    /// Creates a new <see cref="Attachment"/> from the provided raw content.
    /// </summary>
    /// <param name="name">The file name to associate with the attachment.</param>
    /// <param name="contentType">The MIME content type describing the attachment.</param>
    /// <param name="content">The raw attachment content that will be encoded as Base64.</param>
    /// <param name="contentId">The optional content identifier used for inline attachments.</param>
    /// <returns>The created <see cref="Attachment"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when any parameter is invalid.</exception>
    public static Attachment Create(string name, string contentType, ReadOnlySpan<byte> content, string? contentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attachment name must be specified.", nameof(name));

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Attachment name contains invalid characters.", nameof(name));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type must be specified.", nameof(contentType));

        if (!MimeKit.ContentType.TryParse(contentType, out var parsedContentType))
            throw new ArgumentException("Content type is not a valid MIME type.", nameof(contentType));

        if (content.IsEmpty)
            throw new ArgumentException("Attachment content cannot be empty.", nameof(content));

        var encodedContent = Convert.ToBase64String(content);

        string? normalizedContentId = null;
        if (contentId is null)
            return new Attachment(name, parsedContentType.MimeType, encodedContent, normalizedContentId);
        
        var trimmedContentId = contentId.Trim();

        if (trimmedContentId.Length == 0)
            throw new ArgumentException("Content ID cannot be empty or whitespace.", nameof(contentId));

        if (trimmedContentId.StartsWith("cid:", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Content ID should not include the 'cid:' prefix.", nameof(contentId));

        if (trimmedContentId.Contains('<', StringComparison.Ordinal) || trimmedContentId.Contains('>', StringComparison.Ordinal))
            throw new ArgumentException("Content ID should not contain angle brackets.", nameof(contentId));

        if (!trimmedContentId.Contains('@', StringComparison.Ordinal))
            throw new ArgumentException("Content ID must contain an '@' character.", nameof(contentId));

        var candidate = $"cid:{trimmedContentId}";
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            throw new ArgumentException("Content ID is not a valid CID URI.", nameof(contentId));

        normalizedContentId = candidate;

        return new Attachment(name, parsedContentType.MimeType, encodedContent, normalizedContentId);
    }
}
