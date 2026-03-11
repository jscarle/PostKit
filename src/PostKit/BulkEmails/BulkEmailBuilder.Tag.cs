using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private string? _tag;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithTag(string tag)
    {
        _tag.EnsureNotSet(nameof(BulkEmail.Tag));

        if (tag.Length > 1000)
            throw new ArgumentException("The tag cannot be longer than 1000 characters.", nameof(tag));

        _tag = tag;

        return this;
    }
}
