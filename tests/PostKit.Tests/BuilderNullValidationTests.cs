using MimeKit;
using PostKit.BulkEmails;
using PostKit.Common;
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
        Assert.Equal("The subject cannot be null. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullHtmlBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .HtmlBody(null!)
        );

        Assert.Equal("html", exception.ParamName);
        Assert.Equal("The HTML body cannot be null. (Parameter 'html')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullTextBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .TextBody(null!)
        );

        Assert.Equal("text", exception.ParamName);
        Assert.Equal("The text body cannot be null. (Parameter 'text')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullSubject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .Subject(null!)
        );

        Assert.Equal("subject", exception.ParamName);
        Assert.Equal("The subject cannot be null. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullHtmlBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .HtmlBody(null!)
        );

        Assert.Equal("htmlBody", exception.ParamName);
        Assert.Equal("The HTML body cannot be null. (Parameter 'htmlBody')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullTextBody_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .TextBody(null!)
        );

        Assert.Equal("textBody", exception.ParamName);
        Assert.Equal("The text body cannot be null. (Parameter 'textBody')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullHeadersDictionary_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddHeader(null!)
        );

        Assert.Equal("headers", exception.ParamName);
        Assert.Equal("The header collection cannot be null. (Parameter 'headers')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullMetadataSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddMetadata((IEnumerable<KeyValuePair<string, string>>)null!)
        );

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("The metadata collection cannot be null. (Parameter 'metadata')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullReplyToSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .ReplyTo((IEnumerable<MailboxAddress>)null!)
        );

        Assert.Equal("mailboxAddresses", exception.ParamName);
        Assert.Equal("The email address collection cannot be null. (Parameter 'mailboxAddresses')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullFromMailboxAddress_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .From((MailboxAddress)null!)
        );

        Assert.Equal("mailboxAddress", exception.ParamName);
        Assert.Equal("The from address cannot be null. (Parameter 'mailboxAddress')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_AlsoReplyTo_WithNullMailboxItem_ThrowsArgumentNullException()
    {
        var replyTo = new MailboxAddress[] { null! };

        var builder = BulkEmail.Compose()
            .ReplyTo("reply@postkit.com");

        var exception = Assert.Throws<ArgumentNullException>(() => builder.ReplyTo((IEnumerable<MailboxAddress>)replyTo));

        Assert.Equal("mailboxAddresses", exception.ParamName);
        Assert.Equal("The email address at index 0 cannot be null. (Parameter 'mailboxAddresses')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullFromMailboxAddress_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .From((MailboxAddress)null!)
        );

        Assert.Equal("mailboxAddress", exception.ParamName);
        Assert.Equal("The from address cannot be null. (Parameter 'mailboxAddress')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullAttachment_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddAttachment((Attachment)null!)
        );

        Assert.Equal("attachment", exception.ParamName);
        Assert.Equal("The attachment cannot be null. (Parameter 'attachment')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullAttachment_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .AddAttachment((Attachment)null!)
        );

        Assert.Equal("attachment", exception.ParamName);
        Assert.Equal("The attachment cannot be null. (Parameter 'attachment')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullMessage_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .AddMessage((BulkEmailMessage)null!)
        );

        Assert.Equal("message", exception.ParamName);
        Assert.Equal("The bulk email message cannot be null. (Parameter 'message')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullAttachmentCollection_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddAttachment((IEnumerable<Attachment>)null!)
        );

        Assert.Equal("attachments", exception.ParamName);
        Assert.Equal("The attachments collection cannot be null. (Parameter 'attachments')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullAttachmentCollection_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .AddAttachment((IEnumerable<Attachment>)null!)
        );

        Assert.Equal("attachments", exception.ParamName);
        Assert.Equal("The attachments collection cannot be null. (Parameter 'attachments')", exception.Message);
    }

    [Fact]
    public void BulkEmailBuilder_WithNullMessageCollection_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .AddMessage((IEnumerable<BulkEmailMessage>)null!)
        );

        Assert.Equal("messages", exception.ParamName);
        Assert.Equal("The bulk email message collection cannot be null. (Parameter 'messages')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_To_WithNullMailboxSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .To((IEnumerable<MailboxAddress>)null!)
        );

        Assert.Equal("mailboxAddresses", exception.ParamName);
        Assert.Equal("The email address collection cannot be null. (Parameter 'mailboxAddresses')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_AlsoTo_WithNullMailboxItem_ThrowsArgumentNullException()
    {
        var recipients = new MailboxAddress[] { null! };

        var builder = Email.Compose()
            .To("recipient@postkit.com");

        var exception = Assert.Throws<ArgumentNullException>(() => builder.To((IEnumerable<MailboxAddress>)recipients));

        Assert.Equal("mailboxAddresses", exception.ParamName);
        Assert.Equal("The email address at index 0 cannot be null. (Parameter 'mailboxAddresses')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithNullHeadersDictionary_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .AddHeader(null!)
        );

        Assert.Equal("headers", exception.ParamName);
        Assert.Equal("The header collection cannot be null. (Parameter 'headers')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithNullMetadataDictionary_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .AddMetadata(null!)
        );

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("The metadata collection cannot be null. (Parameter 'metadata')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_To_WithNullMailboxSequence_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .To((IEnumerable<MailboxAddress>)null!)
        );

        Assert.Equal("mailboxAddresses", exception.ParamName);
        Assert.Equal("The email address collection cannot be null. (Parameter 'mailboxAddresses')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullSerializerOptions_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.FromTemplate(42)
            .WithModel(new { Name = "Alice" }, null!)
        );

        Assert.Equal("serializerOptions", exception.ParamName);
        Assert.Equal("The serializer options cannot be null. (Parameter 'serializerOptions')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuilder_WithNullSerializerOptions_ThrowsHelpfulArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.FromTemplate()
            .WithModel(new { Name = "Alice" }, null!)
        );

        Assert.Equal("serializerOptions", exception.ParamName);
        Assert.Equal("The serializer options cannot be null. (Parameter 'serializerOptions')", exception.Message);
    }
}
