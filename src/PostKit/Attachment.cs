using System;
using System.IO;
using MimeKit;

namespace PostKit;

public sealed class Attachment
{
    private Attachment(string name, string contentType, string content, int contentLength, string? contentId)
    {
        Name = name;
        ContentType = contentType;
        Content = content;
        ContentLength = contentLength;
        ContentId = contentId;
    }

    public string Name { get; }

    public string ContentType { get; }

    public string Content { get; }

    public string? ContentId { get; }

    internal int ContentLength { get; }

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
        var byteLength = content.Length;

        string? normalizedContentId = null;
        if (contentId is not null)
        {
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
        }

        return new Attachment(name, parsedContentType.MimeType, encodedContent, byteLength, normalizedContentId);
    }
}
