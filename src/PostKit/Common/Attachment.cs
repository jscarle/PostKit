using PostKit.Emails;

namespace PostKit.Common;

/// <summary>Represents an email attachment that can be sent with a <see cref="Email" />.</summary>
public sealed class Attachment
{
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
        "mst"
    };

    private Attachment(string name, string contentType, string content, string? contentId)
    {
        Name = name;
        ContentType = contentType;
        Content = content;
        ContentId = contentId;
    }

    /// <summary>Gets the file name that will be presented to the email recipient.</summary>
    public string Name { get; }

    /// <summary>Gets the MIME content type of the attachment.</summary>
    public string ContentType { get; }

    /// <summary>Gets the Base64 encoded contents of the attachment.</summary>
    public string Content { get; }

    /// <summary>Gets the optional content identifier of the attachment when it should be embedded in the message body.</summary>
    public string? ContentId { get; }

    /// <summary>Creates a new <see cref="Attachment" /> from the provided raw content.</summary>
    /// <param name="name">The file name to associate with the attachment.</param>
    /// <param name="contentType">The MIME content type describing the attachment.</param>
    /// <param name="content">The raw attachment content that will be encoded as Base64.</param>
    /// <param name="contentId">The optional content identifier used for inline attachments.</param>
    /// <returns>The created <see cref="Attachment" />.</returns>
    /// <exception cref="ArgumentException">Thrown when any parameter is invalid.</exception>
    public static Attachment Create(string name, string contentType, ReadOnlySpan<byte> content, string? contentId = null)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name), "Attachment name cannot be null.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attachment name is required.", nameof(name));

        if (TryGetInvalidNameCharacter(name, out var invalidCharacter, out var invalidCharacterIndex))
            throw new ArgumentException($"Attachment name cannot contain control characters, '/' or '\\'. Invalid character {ValidationExtensions.FormatCharacter(invalidCharacter)} at index {invalidCharacterIndex}.", nameof(name));

        var trimmedName = name.TrimEnd();
        ValidateFileType(trimmedName);

        if (contentType is null)
            throw new ArgumentNullException(nameof(contentType), "Attachment content type cannot be null.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Attachment content type is required.", nameof(contentType));

        if (!MimeKit.ContentType.TryParse(contentType, out var parsedContentType))
            throw new ArgumentException("Attachment content type must be a valid MIME type, for example 'application/pdf' or 'image/png'.", nameof(contentType));

        if (content.IsEmpty)
            throw new ArgumentException("Attachment content cannot be empty.", nameof(content));

        var encodedContent = Convert.ToBase64String(content);

        if (contentId is null)
            return new Attachment(name, parsedContentType.MimeType, encodedContent, contentId);

        var contentIdStartIndex = 0;
        var contentIdEndIndex = contentId.Length;

        while (contentIdStartIndex < contentIdEndIndex && char.IsWhiteSpace(contentId[contentIdStartIndex]))
            contentIdStartIndex++;

        while (contentIdEndIndex > contentIdStartIndex && char.IsWhiteSpace(contentId[contentIdEndIndex - 1]))
            contentIdEndIndex--;

        if (contentIdStartIndex == contentIdEndIndex)
            throw new ArgumentException("Content ID cannot be empty or whitespace.", nameof(contentId));

        const string contentIdPrefix = "cid:";
        var trimmedContentId = contentId.AsSpan(contentIdStartIndex, contentIdEndIndex - contentIdStartIndex);
        if (trimmedContentId.StartsWith(contentIdPrefix, StringComparison.OrdinalIgnoreCase))
            contentIdStartIndex += contentIdPrefix.Length;

        if (contentIdStartIndex == contentIdEndIndex)
            throw new ArgumentException("Content ID cannot be empty or whitespace.", nameof(contentId));

        for (var index = contentIdStartIndex; index < contentIdEndIndex; index++)
        {
            var ch = contentId[index];
            if (ch is '<' or '>')
                throw new ArgumentException($"Content ID cannot contain angle brackets. Invalid character {ValidationExtensions.FormatCharacter(ch)} at index {index}.", nameof(contentId));

            if (ch is < (char)0x21 or > (char)0x7E)
                throw new ArgumentException($"Content ID must contain only visible ASCII characters; spaces and control characters are not allowed. Invalid character {ValidationExtensions.FormatCharacter(ch)} at index {index}.",
                    nameof(contentId));
        }

        var contentIdValue = contentId[contentIdStartIndex..contentIdEndIndex];
        var normalizedContentId = $"cid:{contentIdValue}";

        return new Attachment(name, parsedContentType.MimeType, encodedContent, normalizedContentId);
    }

    private static bool TryGetInvalidNameCharacter(string name, out char invalidCharacter, out int invalidCharacterIndex)
    {
        for (var index = 0; index < name.Length; index++)
        {
            var ch = name[index];
            if (char.IsControl(ch) || ch is '/' or '\\')
            {
                invalidCharacter = ch;
                invalidCharacterIndex = index;
                return true;
            }
        }

        invalidCharacter = default;
        invalidCharacterIndex = -1;
        return false;
    }

    private static void ValidateFileType(string name)
    {
        var lastDotIndex = name.LastIndexOf('.');
        if (lastDotIndex < 0 || lastDotIndex == name.Length - 1)
            return;

        var fileType = name[(lastDotIndex + 1)..];
        if (ForbiddenFileTypes.Contains(fileType))
            throw new ArgumentException($"Attachment file type '.{fileType}' is not accepted by Postmark.", nameof(name));
    }
}