using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Emails;

namespace PostKit.Tests;

public class EmailBuilderHeaderTests
{
    private const string HeaderValueRuleMessage = "The header value must contain only visible ASCII characters or tab, and folded lines must use CRLF followed by a space or tab.";

    [Fact]
    public void AttachmentCreate_WithForbiddenFileType_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("payload.bin", "application/octet-stream", [1]));

        Assert.Equal("Attachment file type '.bin' is not accepted by Postmark. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithNullName_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Attachment.Create(null!, "text/plain", "content"u8.ToArray()));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Attachment name cannot be null. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithNullContentType_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Attachment.Create("report.txt", null!, "content"u8.ToArray()));

        Assert.Equal("contentType", exception.ParamName);
        Assert.Equal("Attachment content type cannot be null. (Parameter 'contentType')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithPrefixedContentId_NormalizesAndSucceeds()
    {
        var attachment = Attachment.Create("image.png", "image/png", [1], "cid:part1.01030607.06070005@gmail.com");

        Assert.Equal("cid:part1.01030607.06070005@gmail.com", attachment.ContentId);
    }

    [Fact]
    public void AttachmentCreate_WithAsciiContentIdWithoutAt_Succeeds()
    {
        var attachment = Attachment.Create("image.png", "image/png", [1], "c7d-2q41-zfw");

        Assert.Equal("cid:c7d-2q41-zfw", attachment.ContentId);
    }

    [Fact]
    public void AttachmentCreate_WithPlatformSpecificPunctuationInName_Succeeds()
    {
        var attachment = Attachment.Create("report:2026?.txt", "text/plain", "content"u8.ToArray());

        Assert.Equal("report:2026?.txt", attachment.Name);
    }

    [Fact]
    public void AttachmentCreate_WithPathSeparatorInName_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("reports/2026.txt", "text/plain", "content"u8.ToArray()));

        Assert.Equal("Attachment name cannot contain control characters, '/' or '\\'. Invalid character '/' at index 7. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithInvalidContentType_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("report.txt", "not-a-mime-type", "content"u8.ToArray()));

        Assert.Equal("Attachment content type must be a valid MIME type, for example 'application/pdf' or 'image/png'. (Parameter 'contentType')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithWhitespaceInContentId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", [1], "part 1@example.com"));

        Assert.Equal("Content ID must contain only visible ASCII characters; spaces and control characters are not allowed. Invalid character space at index 4. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithPrefixedContentId_ReportsOriginalArgumentIndex()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", [1], "cid:part 1@example.com"));

        Assert.Equal("Content ID must contain only visible ASCII characters; spaces and control characters are not allowed. Invalid character space at index 8. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithTrimmedPrefixedContentId_ReportsOriginalArgumentIndex()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", [1], " cid:part<1@example.com "));

        Assert.Equal("Content ID cannot contain angle brackets. Invalid character '<' at index 9. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithNonAsciiContentId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", [1], "parté@example.com"));

        Assert.Equal("Content ID must contain only visible ASCII characters; spaces and control characters are not allowed. Invalid character U+00E9 at index 4. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithAngleBracketInContentId_ThrowsArgumentExceptionWithIndex()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", [1], "part<1@example.com"));

        Assert.Equal("Content ID cannot contain angle brackets. Invalid character '<' at index 4. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void WithHeader_DuplicateDictionaryKeys_ThrowsArgumentExceptionWithHeaderMessage()
    {
        var builder = Email.Compose();
        var headers = new Dictionary<string, string> { ["X-Test"] = "A", ["x-test"] = "B" };

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader(headers));

        Assert.Equal("Header names must be unique and are compared case-insensitively. Header name 'x-test' at index 1 duplicates index 0. (Parameter 'headers')", exception.Message);
    }

    [Fact]
    public void WithHeader_DuplicateSingleHeader_ThrowsHelpfulExceptionWithName()
    {
        var builder = Email.Compose()
            .AddHeader("X-Test", "A");

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("x-test", "B"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Header names must be unique and are compared case-insensitively. Header name 'x-test' duplicates existing header name 'X-Test'. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void WithHeader_InvalidValueInSequence_ThrowsHelpfulExceptionWithIndexAndDoesNotMutate()
    {
        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Header validation")
            .TextBody("Header validation");
        IEnumerable<KeyValuePair<string, string>> headers =
        [
            new("X-First", "value"),
            new("X-Second", "bad\n value"),
        ];

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader(headers));

        Assert.Equal("headers", exception.ParamName);
        Assert.Equal($"The header value at index 1 is invalid. {HeaderValueRuleMessage} Invalid character U+000A at index 3; use CRLF followed by a space or tab for folded lines. (Parameter 'headers')", exception.Message);
        Assert.Null(builder.Build()
            .Headers
        );
    }

    [Fact]
    public void WithHeader_DuplicateExistingHeaderInSequence_ThrowsHelpfulExceptionAndDoesNotPartiallyMutate()
    {
        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Header validation")
            .TextBody("Header validation")
            .AddHeader("X-Existing", "value");
        IEnumerable<KeyValuePair<string, string>> headers =
        [
            new("X-New", "value"),
            new("x-existing", "duplicate"),
        ];

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader(headers));

        Assert.Equal("headers", exception.ParamName);
        Assert.Equal("Header names must be unique and are compared case-insensitively. Header name 'x-existing' at index 1 duplicates existing header name 'X-Existing'. (Parameter 'headers')", exception.Message);

        var email = builder.Build();
        Assert.NotNull(email.Headers);
        Assert.Single(email.Headers);
        Assert.False(email.Headers.ContainsKey("X-New"));
    }

    [Fact]
    public void WithHeader_NullName_ThrowsHelpfulException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentNullException>(() => builder.AddHeader(null!, "value"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("The header name cannot be null. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void WithHeader_NullValue_ThrowsHelpfulException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentNullException>(() => builder.AddHeader("X-Test", null!));

        Assert.Equal("value", exception.ParamName);
        Assert.Equal("The header value cannot be null. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_NameContainingColon_ThrowsHelpfulException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("X-Test:", "value"));

        Assert.Equal("The header name is required and must contain only visible ASCII characters except ':'. Invalid character ':' at index 6. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareCarriageReturnFold_ThrowsArgumentException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("X-Test", "bad\r value"));

        Assert.Equal($"{HeaderValueRuleMessage} Invalid line folding at index 3; CRLF must be followed by a space or tab. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareLineFeedFold_ThrowsArgumentException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("X-Test", "bad\n value"));

        Assert.Equal($"{HeaderValueRuleMessage} Invalid character U+000A at index 3; use CRLF followed by a space or tab for folded lines. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_FoldedHeaderUsingTab_IsAccepted()
    {
        var builder = Email.Compose();

        var exception = Record.Exception(() => builder.AddHeader("X-Test", "good\r\n\tvalue"));

        Assert.Null(exception);
    }

    [Fact]
    public void WithHeader_NameUsingVisibleAsciiTokenCharacters_IsAccepted()
    {
        var builder = Email.Compose();

        var exception = Record.Exception(() => builder.AddHeader("X_Custom+Trace", "value"));

        Assert.Null(exception);
    }

    [Fact]
    public void WithHeader_ValueContainingPlainTab_IsAccepted()
    {
        var builder = Email.Compose();

        var exception = Record.Exception(() => builder.AddHeader("X-Test", "good\tvalue"));

        Assert.Null(exception);
    }

    [Fact]
    public void BulkEmailBuilder_WithHeader_NameUsingVisibleAsciiTokenCharacters_IsAccepted()
    {
        var builder = BulkEmail.Compose();

        var exception = Record.Exception(() => builder.AddHeader("X_Custom+Trace", "value"));

        Assert.Null(exception);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithHeader_ValueContainingPlainTab_IsAccepted()
    {
        var builder = BulkEmailMessage.Compose();

        var exception = Record.Exception(() => builder.AddHeader("X-Test", "good\tvalue"));

        Assert.Null(exception);
    }

    [Fact]
    public void BulkEmailBuilder_WithHeader_NullName_ThrowsHelpfulException()
    {
        var builder = BulkEmail.Compose();

        var exception = Assert.Throws<ArgumentNullException>(() => builder.AddHeader(null!, "value"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("The header name cannot be null. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithDuplicateSingleHeader_ThrowsHelpfulExceptionWithName()
    {
        var builder = BulkEmail.Compose()
            .AddHeader("X-Test", "A");

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("x-test", "B"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Header names must be unique and are compared case-insensitively. Header name 'x-test' duplicates existing header name 'X-Test'. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithHeader_NullValue_ThrowsHelpfulException()
    {
        var builder = BulkEmailMessage.Compose();

        var exception = Assert.Throws<ArgumentNullException>(() => builder.AddHeader("X-Test", null!));

        Assert.Equal("value", exception.ParamName);
        Assert.Equal("The header value cannot be null. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithDuplicateSingleHeader_ThrowsHelpfulExceptionWithName()
    {
        var builder = BulkEmailMessage.Compose()
            .AddHeader("X-Test", "A");

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("x-test", "B"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Header names must be unique and are compared case-insensitively. Header name 'x-test' duplicates existing header name 'X-Test'. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithHeaderSequenceInvalidValue_ThrowsHelpfulExceptionWithIndexAndDoesNotMutate()
    {
        var builder = BulkEmailMessage.Compose()
            .To("recipient@postkit.com");
        IEnumerable<KeyValuePair<string, string>> headers =
        [
            new("X-First", "value"),
            new("X-Second", "bad\n value"),
        ];

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader(headers));

        Assert.Equal("headers", exception.ParamName);
        Assert.Equal($"The header value at index 1 is invalid. {HeaderValueRuleMessage} Invalid character U+000A at index 3; use CRLF followed by a space or tab for folded lines. (Parameter 'headers')", exception.Message);
        Assert.Null(builder.Build()
            .Headers
        );
    }
}
