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

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized text body")
            .TextBody(oversizedText);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithOversizedHtmlBody_ThrowsInvalidOperationException()
    {
        var oversizedHtml = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes + 1);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized HTML body")
            .HtmlBody(oversizedHtml);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithUtf8BodyExceedingLimit_ThrowsInvalidOperationException()
    {
        var oversizedText = new string('é', (int)(PostmarkSizeEstimator.BodySizeLimitInBytes / 2) + 1);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized UTF-8 text body")
            .TextBody(oversizedText);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithAttachmentsPushingPastMessageLimit_ThrowsInvalidOperationException()
    {
        var textBody = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var htmlBody = new string('b', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var attachment = Attachment.Create("tiny.txt", "text/plain", new byte[] { 1 });

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized message")
            .TextBody(textBody)
            .HtmlBody(htmlBody)
            .AddAttachment(attachment);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithAttachmentWhoseBase64EncodingExceedsLimit_ThrowsInvalidOperationException()
    {
        var rawAttachmentBytes = new byte[(int)(PostmarkSizeEstimator.MessageSizeLimitInBytes / 4 * 3) + 1];
        var attachment = Attachment.Create("large.dat", "application/octet-stream", rawAttachmentBytes);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized attachment")
            .TextBody("Hello world");

        Assert.Throws<InvalidOperationException>(() => builder.AddAttachment(attachment));
    }

    [Fact]
    public void Build_WithLargeHeadersPushingPastEstimatedMessageLimit_ThrowsInvalidOperationException()
    {
        var textBody = new string('a', 4 * 1024 * 1024);
        var htmlBody = new string('b', 4 * 1024 * 1024);
        var largeHeaderValue = new string('h', 3 * 1024 * 1024);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized by headers")
            .TextBody(textBody)
            .HtmlBody(htmlBody)
            .AddHeader("X-Large-Header", largeHeaderValue);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated message size exceeds Postmark's 10 MB limit.", exception.Message);
    }

    [Fact]
    public void Build_WithLargeTemplateModelPushingPastEstimatedMessageLimit_ThrowsInvalidOperationException()
    {
        var templateModel = new { Data = new string('x', 11 * 1024 * 1024) };

        var builder = Email.FromTemplate(42)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(templateModel);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated message size exceeds Postmark's 10 MB limit.", exception.Message);
    }
}
