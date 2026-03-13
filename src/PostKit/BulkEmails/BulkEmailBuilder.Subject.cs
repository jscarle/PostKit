using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private string? _subject;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithSubject(string subject)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));

        var length = subject.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 2000)
            throw new ArgumentException("The subject cannot be longer than 2000 characters.", nameof(subject));

        _subject = subject;

        return this;
    }
}
