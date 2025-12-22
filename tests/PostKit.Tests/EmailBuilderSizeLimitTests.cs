using PostKit.Common;
using Xunit;

namespace PostKit.Tests;

public class EmailBuilderSizeLimitTests
{
    [Fact]
    public void Build_WithOversizedTextBody_ThrowsInvalidOperationException()
    {
        var oversizedText = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes + 1);

        var builder = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithSubject("Oversized text body")
            .WithTextBody(oversizedText);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithOversizedHtmlBody_ThrowsInvalidOperationException()
    {
        var oversizedHtml = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes + 1);

        var builder = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithSubject("Oversized HTML body")
            .WithHtmlBody(oversizedHtml);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithAttachmentsPushingPastMessageLimit_ThrowsInvalidOperationException()
    {
        var textBody = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var htmlBody = new string('b', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var attachment = Attachment.Create("tiny.txt", "text/plain", new byte[] { 1 });

        var builder = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithSubject("Oversized message")
            .WithTextBody(textBody)
            .WithHtmlBody(htmlBody)
            .WithAttachment(attachment);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
}
