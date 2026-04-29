using MimeKit;
using PostKit.BulkEmails;
using PostKit.Emails;

namespace PostKit.Tests;

public class BuilderNullValidationTests
{
    [Fact]
    public void EmailBuilder_WithNullSubject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .Subject(null!)
        );

        Assert.Equal("subject", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullHtmlBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .HtmlBody(null!)
        );

        Assert.Equal("html", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullTextBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .TextBody(null!)
        );

        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullSubject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .Subject(null!)
        );

        Assert.Equal("subject", exception.ParamName);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullHtmlBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .HtmlBody(null!)
        );

        Assert.Equal("htmlBody", exception.ParamName);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullTextBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .TextBody(null!)
        );

        Assert.Equal("textBody", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullHeadersDictionary_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddHeader(null!)
        );

        Assert.Equal("headers", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullMetadataSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddMetadata((IEnumerable<KeyValuePair<string, string>>)null!)
        );

        Assert.Equal("metadata", exception.ParamName);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullReplyToSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .ReplyTo((IEnumerable<MailboxAddress>)null!)
        );

        Assert.Equal("mailboxAddresses", exception.ParamName);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullFromMailboxAddress_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .From((MailboxAddress)null!)
        );

        Assert.Equal("mailboxAddress", exception.ParamName);
    }

    [Fact]
    public void BulkEmailBuilder_AlsoReplyTo_WithNullMailboxItem_ThrowsArgumentNullException()
    {
        var replyTo = new MailboxAddress[] { null! };

        var builder = BulkEmail.Compose()
            .ReplyTo("reply@postkit.com");

        var exception = Assert.Throws<ArgumentNullException>(() => builder.ReplyTo((IEnumerable<MailboxAddress>)replyTo));

        Assert.Equal("mailboxAddress", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullFromMailboxAddress_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .From((MailboxAddress)null!)
        );

        Assert.Equal("mailboxAddress", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_To_WithNullMailboxSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .To((IEnumerable<MailboxAddress>)null!)
        );

        Assert.Equal("mailboxAddresses", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_AlsoTo_WithNullMailboxItem_ThrowsArgumentNullException()
    {
        var recipients = new MailboxAddress[] { null! };

        var builder = Email.Compose()
            .To("recipient@postkit.com");

        var exception = Assert.Throws<ArgumentNullException>(() => builder.To((IEnumerable<MailboxAddress>)recipients));

        Assert.Equal("mailboxAddress", exception.ParamName);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithNullHeadersDictionary_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .AddHeader(null!)
        );

        Assert.Equal("headers", exception.ParamName);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithNullMetadataDictionary_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .AddMetadata(null!)
        );

        Assert.Equal("metadata", exception.ParamName);
    }

    [Fact]
    public void BulkEmailMessageBuilder_To_WithNullMailboxSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .To((IEnumerable<MailboxAddress>)null!)
        );

        Assert.Equal("mailboxAddresses", exception.ParamName);
    }
}
