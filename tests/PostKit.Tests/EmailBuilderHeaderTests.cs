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
    public void AttachmentCreate_WithPrefixedContentId_NormalizesAndSucceeds()
    {
        var attachment = Attachment.Create("image.png", "image/png", new byte[] { 1 }, "cid:part1.01030607.06070005@gmail.com");

        Assert.Equal("cid:part1.01030607.06070005@gmail.com", attachment.ContentId);
    }

    [Fact]
    public void AttachmentCreate_WithAsciiContentIdWithoutAt_Succeeds()
    {
        var attachment = Attachment.Create("image.png", "image/png", new byte[] { 1 }, "c7d-2q41-zfw");

        Assert.Equal("cid:c7d-2q41-zfw", attachment.ContentId);
    }

    [Fact]
    public void AttachmentCreate_WithWhitespaceInContentId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", new byte[] { 1 }, "part 1@example.com"));

        Assert.Equal("Content ID must contain only visible ASCII characters and no spaces. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void AttachmentCreate_WithNonAsciiContentId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Attachment.Create("image.png", "image/png", new byte[] { 1 }, "parté@example.com"));

        Assert.Equal("Content ID must contain only visible ASCII characters and no spaces. (Parameter 'contentId')", exception.Message);
    }

    [Fact]
    public void WithHeader_DuplicateDictionaryKeys_ThrowsArgumentExceptionWithHeaderMessage()
    {
        var builder = Email.Compose();
        var headers = new Dictionary<string, string> { ["X-Test"] = "A", ["x-test"] = "B" };

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader(headers));

        Assert.Equal("There are duplicate header entries. (Parameter 'headers')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareCarriageReturnFold_ThrowsArgumentException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("X-Test", "bad\r value"));

        Assert.Equal("The header value is invalid. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithHeader_BareLineFeedFold_ThrowsArgumentException()
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentException>(() => builder.AddHeader("X-Test", "bad\n value"));

        Assert.Equal("The header value is invalid. (Parameter 'value')", exception.Message);
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
}
