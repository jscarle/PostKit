namespace PostKit;

partial class EmailBuilder
{
    private string? _subject;

    public EmailBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }
}
