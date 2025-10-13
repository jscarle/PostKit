using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private string? _subject;

    /// <inheritdoc />
    public IEmailBuilder WithSubject(string subject)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));

        var length = subject.AsSpan()
            .GetUtf16Length();
        if (length > 2000)
            throw new ArgumentException("The subject cannot be longer than 2000 characters.", nameof(subject));

        _subject = subject;

        return this;
    }
}
