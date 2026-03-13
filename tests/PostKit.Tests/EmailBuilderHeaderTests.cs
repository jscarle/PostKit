using PostKit.Common;
using PostKit.BulkEmails;
using PostKit.Emails;

namespace PostKit.Tests;

public class EmailBuilderHeaderTests
{
    [Fact]
    public void AttachmentCreate_WithForbiddenFileType_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("payload.bin", "application/octet-stream", new byte[] { 1 }));

        Assert.Equal("Attachment file type is not accepted by Postmark. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void WithHeader_DuplicateDictionaryKeys_ThrowsArgumentExceptionWithHeaderMessage()
    {
        var builder = Email.CreateBuilder();
        var headers = new Dictionary<string, string> { ["X-Test"] = "A", ["x-test"] = "B" };

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeaders(headers));

        Assert.Equal("There are duplicate header entries. (Parameter 'headers')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareCarriageReturnFold_ThrowsArgumentException()
    {
        var builder = Email.CreateBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeader("X-Test", "bad\r value"));

        Assert.Equal("The header value is invalid. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareLineFeedFold_ThrowsArgumentException()
    {
        var builder = Email.CreateBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithHeader("X-Test", "bad\n value"));

        Assert.Equal("The header value is invalid. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_FoldedHeaderUsingTab_IsAccepted()
    {
        var builder = Email.CreateBuilder();

        var exception = Record.Exception(() => builder.WithHeader("X-Test", "good\r\n\tvalue"));

        Assert.Null(exception);
    }

    [Fact]
    public void WithHeader_NameUsingVisibleAsciiTokenCharacters_IsAccepted()
    {
        var builder = Email.CreateBuilder();

        var exception = Record.Exception(() => builder.WithHeader("X_Custom+Trace", "value"));

        Assert.Null(exception);
    }

    [Fact]
    public void WithHeader_ValueContainingPlainTab_IsAccepted()
    {
        var builder = Email.CreateBuilder();

        var exception = Record.Exception(() => builder.WithHeader("X-Test", "good\tvalue"));

        Assert.Null(exception);
    }

    [Fact]
    public void BulkEmailBuilder_WithHeader_NameUsingVisibleAsciiTokenCharacters_IsAccepted()
    {
        var builder = BulkEmail.CreateBuilder();

        var exception = Record.Exception(() => builder.WithHeader("X_Custom+Trace", "value"));

        Assert.Null(exception);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithHeader_ValueContainingPlainTab_IsAccepted()
    {
        var builder = BulkEmailMessage.CreateBuilder();

        var exception = Record.Exception(() => builder.WithHeader("X-Test", "good\tvalue"));

        Assert.Null(exception);
    }
}
