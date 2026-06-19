using PostKit.Common;
using PostKit.Emails;

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

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.StartsWith("Text body exceeds Postmark's 5 MB limit. Actual size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 5,242,880 bytes.", exception.Message, StringComparison.Ordinal);
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

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.StartsWith("HTML body exceeds Postmark's 5 MB limit. Actual size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 5,242,880 bytes.", exception.Message, StringComparison.Ordinal);
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

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.StartsWith("Text body exceeds Postmark's 5 MB limit. Actual size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 5,242,880 bytes.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddAttachment_WithExistingBodyPushingPastMessageLimit_ThrowsInvalidOperationException()
    {
        var textBody = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var attachment = Attachment.Create("large.dat", "application/octet-stream", new byte[6 * 1024 * 1024]);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized message")
            .TextBody(textBody);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddAttachment(attachment));

        AssertEstimatedMessageSizeMessage(exception);
    }

    [Fact]
    public void Build_WithAttachmentPayloadExceedingLimit_ThrowsInvalidOperationException()
    {
        var rawAttachmentBytes = new byte[(int)PostmarkSizeEstimator.MessageSizeLimitInBytes + 1];
        var attachment = Attachment.Create("large.dat", "application/octet-stream", rawAttachmentBytes);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized attachment")
            .TextBody("Hello world");

        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddAttachment(attachment));

        AssertEstimatedMessageSizeMessage(exception);
    }

    [Fact]
    public void Build_WithBase64AttachmentAddedBeforeBodyStillExceedingLimit_ThrowsInvalidOperationException()
    {
        var textBody = new string('a', 4_500_000);
        var attachment = Attachment.Create("large.dat", "application/octet-stream", new byte[5 * 1024 * 1024]);

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Oversized by encoded attachment")
            .AddAttachment(attachment)
            .TextBody(textBody);

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedMessageSizeMessage(exception);
    }

    [Fact]
    public void Build_WithLargeHeadersPushingPastMessageLimit_ThrowsInvalidOperationException()
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

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedMessageSizeMessage(exception);
    }

    [Fact]
    public void Build_WithLargeTemplateModelPushingPastMessageLimit_ThrowsInvalidOperationException()
    {
        var templateModel = new { Data = new string('x', 11 * 1024 * 1024) };

        var builder = Email.FromTemplate(42)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(templateModel);

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedMessageSizeMessage(exception);
    }

    private static void AssertEstimatedMessageSizeMessage(InvalidOperationException exception)
    {
        Assert.StartsWith("Estimated message content exceeds Postmark's 10 MB limit. Estimated size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 10,485,760 bytes.", exception.Message, StringComparison.Ordinal);
    }
}