using PostKit.Emails;

namespace PostKit.Common;

/// <summary>Represents an email attachment that can be sent with a <see cref="Email"/>.</summary>
public sealed class Attachment
{
    /// <summary>Gets the file name that will be presented to the email recipient.</summary>
    public string Name { get; }

    /// <summary>Gets the MIME content type of the attachment.</summary>
    public string ContentType { get; }

    /// <summary>Gets the Base64 encoded contents of the attachment.</summary>
    public string Content { get; }

    /// <summary>Gets the optional content identifier of the attachment when it should be embedded in the message body.</summary>
    public string? ContentId { get; }

    private static readonly HashSet<string> ForbiddenFileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vbs",
        "exe",
        "bin",
        "bat",
        "chm",
        "com",
        "cpl",
        "crt",
        "hlp",
        "hta",
        "inf",
        "ins",
        "isp",
        "jse",
        "lnk",
        "mdb",
        "pcd",
        "pif",
        "reg",
        "scr",
        "sct",
        "shs",
        "vbe",
        "vba",
        "wsf",
        "wsh",
        "wsl",
        "msc",
        "msi",
        "msp",
        "mst",
    };

    private Attachment(string name, string contentType, string content, string? contentId)
    {
        Name = name;
        ContentType = contentType;
        Content = content;
        ContentId = contentId;
    }

    /// <summary>Creates a new <see cref="Attachment"/> from the provided raw content.</summary>
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

        if (ContainsInvalidNameCharacter(name))
            throw new ArgumentException("Attachment name contains invalid characters.", nameof(name));

        var trimmedName = name.TrimEnd();
        ValidateFileType(trimmedName);

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type must be specified.", nameof(contentType));

        if (!MimeKit.ContentType.TryParse(contentType, out var parsedContentType))
            throw new ArgumentException("Content type is not a valid MIME type.", nameof(contentType));

        if (content.IsEmpty)
            throw new ArgumentException("Attachment content cannot be empty.", nameof(content));

        var encodedContent = Convert.ToBase64String(content);

        if (contentId is null)
            return new Attachment(name, parsedContentType.MimeType, encodedContent, contentId);

        var trimmedContentId = contentId.Trim();
        if (trimmedContentId.Length == 0)
            throw new ArgumentException("Content ID cannot be empty or whitespace.", nameof(contentId));

        var contentIdValue = trimmedContentId.StartsWith("cid:", StringComparison.OrdinalIgnoreCase) ? trimmedContentId["cid:".Length..] : trimmedContentId;

        if (contentIdValue.Length == 0)
            throw new ArgumentException("Content ID cannot be empty or whitespace.", nameof(contentId));

        if (contentIdValue.Contains('<', StringComparison.Ordinal) || contentIdValue.Contains('>', StringComparison.Ordinal))
            throw new ArgumentException("Content ID should not contain angle brackets.", nameof(contentId));

        foreach (var ch in contentIdValue)
        {
            if (ch is < (char)0x21 or > (char)0x7E)
                throw new ArgumentException("Content ID must contain only visible ASCII characters and no spaces.", nameof(contentId));
        }

        var normalizedContentId = $"cid:{contentIdValue}";

        return new Attachment(name, parsedContentType.MimeType, encodedContent, normalizedContentId);
    }

    private static bool ContainsInvalidNameCharacter(string name)
    {
        foreach (var ch in name)
        {
            if (char.IsControl(ch) || ch is '/' or '\\')
                return true;
        }

        return false;
    }

    private static void ValidateFileType(string name)
    {
        var lastDotIndex = name.LastIndexOf('.');
        if (lastDotIndex < 0 || lastDotIndex == name.Length - 1)
            return;

        var fileType = name[(lastDotIndex + 1)..];
        if (ForbiddenFileTypes.Contains(fileType))
            throw new ArgumentException("Attachment file type is not accepted by Postmark.", nameof(name));
    }
}
