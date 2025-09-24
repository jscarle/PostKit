using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private string? _htmlBody;
    private string? _textBody;

    public IEmailBuilder WithHtmlBody(string html)
    {
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));

        _htmlBody = html;

        return this;
    }

    public IEmailBuilder WithTextBody(string text)
    {
        _textBody.EnsureNotSet(nameof(Email.TextBody));

        _textBody = text;

        return this;
    }
}
