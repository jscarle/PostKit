using PostKit.Common;
using PostKit.Emails;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class EmailBuilderSizeLimitTests
{
    [Fact]
    public void Build_WithOversizedTextBody_ThrowsInvalidOperationException()
    {
        var oversizedText = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes + 1);

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Oversized text body")
            .WithTextBody(oversizedText);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithOversizedHtmlBody_ThrowsInvalidOperationException()
    {
        var oversizedHtml = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes + 1);

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Oversized HTML body")
            .WithHtmlBody(oversizedHtml);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithUtf8BodyExceedingLimit_ThrowsInvalidOperationException()
    {
        var oversizedText = new string('é', (int)(PostmarkSizeEstimator.BodySizeLimitInBytes / 2) + 1);

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Oversized UTF-8 text body")
            .WithTextBody(oversizedText);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithAttachmentsPushingPastMessageLimit_ThrowsInvalidOperationException()
    {
        var textBody = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var htmlBody = new string('b', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var attachment = Attachment.Create("tiny.txt", "text/plain", new byte[] { 1 });

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Oversized message")
            .WithTextBody(textBody)
            .WithHtmlBody(htmlBody)
            .WithAttachment(attachment);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithAttachmentWhoseBase64EncodingExceedsLimit_ThrowsInvalidOperationException()
    {
        var rawAttachmentBytes = new byte[(int)(PostmarkSizeEstimator.MessageSizeLimitInBytes / 4 * 3) + 1];
        var attachment = Attachment.Create("large.bin", "application/octet-stream", rawAttachmentBytes);

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Oversized attachment")
            .WithTextBody("Hello world");

        Assert.Throws<InvalidOperationException>(() => builder.WithAttachment(attachment));
    }
}
