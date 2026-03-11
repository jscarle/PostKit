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

        _subject = subject;

        return this;
    }
}
