namespace PostKit;

partial class EmailBuilder
{
    private string? _htmlBody;
    private string? _textBody;

    public EmailBuilder WithHtmlBody(string html)
    {
        _htmlBody = html;
        return this;
    }

    public EmailBuilder WithTextBody(string text)
    {
        _textBody = text;
        return this;
    }
}
