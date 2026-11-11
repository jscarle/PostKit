using System.Text.Json;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class EmailBuilderSizeLimitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Build_WithBodyOneCharacterOverLimit_ThrowsHelpfulException(bool html)
    {
        var body = new string('a', 5 * 1024 * 1024 + 1);
        var builder = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Body boundary");
        if (html)
            builder.HtmlBody(body);
        else
            builder.TextBody(body);

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        var propertyName = html ? "HtmlBody" : "TextBody";
        Assert.Equal($"{propertyName} exceeds Postmark's limit of 5,242,880 characters. Actual length: 5,242,881. Shorten {propertyName}(...).", exception.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Build_WithNonAsciiBodyAtCharacterLimit_Succeeds(bool html)
    {
        var body = new string('é', 5 * 1024 * 1024);
        var builder = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Unicode body boundary");
        if (html)
            builder.HtmlBody(body);
        else
            builder.TextBody(body);

        var email = builder.Build();

        Assert.Equal(body, html ? email.HtmlBody : email.TextBody);
    }

    [Fact]
    public void Build_WithEmojiBodyOverUtf16CharacterLimit_Throws()
    {
        var body = string.Concat(Enumerable.Repeat("😀", 5 * 1024 * 1024 / 2 + 1));
        var builder = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Emoji boundary").TextBody(body);

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("Actual length: 5,242,882.", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(10_485_758)]
    [InlineData(10_485_759)]
    [InlineData(10_485_760)]
    [InlineData(10_350_182)]
    public void Build_WithRawAttachmentsWithinLimitAndBothMaximumBodies_Succeeds(int rawBytes)
    {
        var attachment = Attachment.Create("boundary.dat", "application/octet-stream", new byte[rawBytes]);
        var body = new string('a', 5 * 1024 * 1024);

        var email = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Attachment boundary")
            .TextBody(body).HtmlBody(body).AddAttachment(attachment).Build();

        Assert.Same(attachment, Assert.Single(email.Attachments!));
        Assert.Equal(rawBytes, attachment.ContentSizeInBytes);
    }

    [Fact]
    public void Build_WithMaximumAttachmentAddedBeforeBodies_Succeeds()
    {
        var attachment = Attachment.Create("boundary.dat", "application/octet-stream", new byte[10 * 1024 * 1024]);
        var body = new string('a', 5 * 1024 * 1024);

        var email = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Attachment first")
            .AddAttachment(attachment).TextBody(body).HtmlBody(body).Build();

        Assert.Same(attachment, Assert.Single(email.Attachments!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddAttachment_WithOneRawByteOverLimit_ThrowsWithoutChangingBuilder(bool collection)
    {
        var accepted = Attachment.Create("accepted.dat", "application/octet-stream", new byte[10 * 1024 * 1024]);
        var extra = Attachment.Create("extra.dat", "application/octet-stream", new byte[1]);
        var builder = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Attachment overflow")
            .TextBody("Hello").AddAttachment(accepted);

        var exception = collection
            ? Assert.Throws<InvalidOperationException>(() => builder.AddAttachment([extra]))
            : Assert.Throws<InvalidOperationException>(() => builder.AddAttachment(extra));

        Assert.Contains("Actual size: 10,485,761 bytes. Limit: 10,485,760 bytes.", exception.Message, StringComparison.Ordinal);
        Assert.Same(accepted, Assert.Single(builder.Build().Attachments!));
    }

    [Fact]
    public void AddAttachment_WithOversizedSingleAttachment_Throws()
    {
        var attachment = Attachment.Create("large.dat", "application/octet-stream", new byte[10 * 1024 * 1024 + 1]);
        var builder = Email.Compose();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddAttachment(attachment));

        Assert.Contains("Actual size: 10,485,761 bytes. Limit: 10,485,760 bytes.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WithMultiplePaddedAttachmentsAtCombinedRawLimit_Succeeds()
    {
        var first = Attachment.Create("first.dat", "application/octet-stream", new byte[5 * 1024 * 1024]);
        var second = Attachment.Create("second.dat", "application/octet-stream", new byte[5 * 1024 * 1024]);

        var email = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Combined attachments")
            .TextBody("Hello").AddAttachment(first).AddAttachment([second]).Build();

        Assert.Equal(2, email.Attachments!.Count);
    }

    [Fact]
    public void Build_WithLargeHeadersAndBodiesAboveFormerCombinedLimit_Succeeds()
    {
        var body = new string('a', 4 * 1024 * 1024);
        var headerValue = new string('h', 3 * 1024 * 1024);

        var email = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("Independent fields")
            .TextBody(body).HtmlBody(body).AddHeader("X-Large-Header", headerValue).Build();

        Assert.Equal(headerValue, email.Headers!["X-Large-Header"]);
    }

    [Fact]
    public void Build_WithTemplateModelAboveFormerMessageLimit_Succeeds()
    {
        var model = new { Data = new string('x', 11 * 1024 * 1024) };

        var email = Email.FromTemplate(42).From("sender@postkit.com").To("recipient@postkit.com").WithModel(model).Build();

        Assert.NotNull(email.TemplateModel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Build_WithJsonPayloadAtOrAboveLimit_UsesExactSerializedSize(int excessBytes)
    {
        var baseline = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("JSON boundary")
            .TextBody("Hello").AddHeader("X-Padding", "p").Build();
        var baselineSize = JsonSerializer.SerializeToUtf8Bytes(baseline.ToEmailRequest(), PostmarkConfiguration.JsonSerializerOptions).Length;
        var paddingLength = 50 * 1024 * 1024 - baselineSize + 1 + excessBytes;
        var builder = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("JSON boundary")
            .TextBody("Hello").AddHeader("X-Padding", new string('p', paddingLength));

        if (excessBytes == 0)
        {
            var email = builder.Build();
            Assert.Equal(52_428_800, JsonSerializer.SerializeToUtf8Bytes(email.ToEmailRequest(), PostmarkConfiguration.JsonSerializerOptions).Length);
        }
        else
        {
            var exception = Assert.Throws<InvalidOperationException>(builder.Build);
            Assert.Contains("Actual size: 52,428,801 bytes. Limit: 52,428,800 bytes.", exception.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Build_WithJsonEscapingPushingPayloadOverLimit_Throws()
    {
        var model = new { Data = new string('é', 9 * 1024 * 1024) };
        var builder = Email.FromTemplate(42).From("sender@postkit.com").To("recipient@postkit.com").WithModel(model);

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.StartsWith("Email JSON payload exceeds Postmark's 50 MiB limit.", exception.Message, StringComparison.Ordinal);
    }
}
