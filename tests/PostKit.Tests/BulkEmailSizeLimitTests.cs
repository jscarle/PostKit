using System.Text.Json;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class BulkEmailSizeLimitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Build_WithBodyOneCharacterOverLimit_ThrowsHelpfulException(bool html)
    {
        var body = new string('a', 5 * 1024 * 1024 + 1);
        var builder = BulkEmail.Compose().From("sender@postkit.com").Subject("Body boundary")
            .AddMessage(BulkEmailMessage.Compose().To("recipient@postkit.com").Build());
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
        var builder = BulkEmail.Compose().From("sender@postkit.com").Subject("Unicode boundary")
            .AddMessage(BulkEmailMessage.Compose().To("recipient@postkit.com").Build());
        if (html)
            builder.HtmlBody(body);
        else
            builder.TextBody(body);

        var email = builder.Build();

        Assert.Equal(body, html ? email.HtmlBody : email.TextBody);
    }

    [Fact]
    public void Build_WithCombinedRawAttachmentsOneByteBelowTenMiBAndMaximumBodies_Succeeds()
    {
        var first = Attachment.Create("first.dat", "application/octet-stream", new byte[5 * 1024 * 1024 - 1]);
        var second = Attachment.Create("second.dat", "application/octet-stream", new byte[5 * 1024 * 1024]);
        var body = new string('a', 5 * 1024 * 1024);

        var email = BulkEmail.Compose().From("sender@postkit.com").Subject("Bulk attachment boundary")
            .AddAttachment(first).AddAttachment([second]).TextBody(body).HtmlBody(body)
            .AddMessage(BulkEmailMessage.Compose().To("recipient@postkit.com").Build()).Build();

        Assert.Equal(2, email.Attachments!.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddAttachment_WithExactlyTenMiBRawContent_ThrowsWithoutChangingBuilder(bool collection)
    {
        var accepted = Attachment.Create("accepted.dat", "application/octet-stream", new byte[10 * 1024 * 1024 - 1]);
        var extra = Attachment.Create("extra.dat", "application/octet-stream", new byte[1]);
        var builder = BulkEmail.Compose().From("sender@postkit.com").Subject("Bulk attachment overflow").TextBody("Hello")
            .AddAttachment(accepted).AddMessage(BulkEmailMessage.Compose().To("recipient@postkit.com").Build());

        var exception = collection
            ? Assert.Throws<InvalidOperationException>(() => builder.AddAttachment([extra]))
            : Assert.Throws<InvalidOperationException>(() => builder.AddAttachment(extra));

        Assert.Contains("Actual size: 10,485,760 bytes. Limit: 10,485,759 bytes.", exception.Message, StringComparison.Ordinal);
        Assert.Same(accepted, Assert.Single(builder.Build().Attachments!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Build_WithJsonPayloadAtOrAboveLimit_UsesExactSerializedSize(int excessBytes)
    {
        var baseline = BulkEmail.Compose().From("sender@postkit.com").Subject("JSON boundary").TextBody("Hello")
            .AddMessage(BulkEmailMessage.Compose().To("recipient@postkit.com").AddHeader("X-Padding", "p").Build()).Build();
        var baselineSize = JsonSerializer.SerializeToUtf8Bytes(baseline.ToBulkEmailRequest(), PostmarkConfiguration.JsonSerializerOptions).Length;
        var paddingLength = 50 * 1024 * 1024 - baselineSize + 1 + excessBytes;
        var builder = BulkEmail.Compose().From("sender@postkit.com").Subject("JSON boundary").TextBody("Hello")
            .AddMessage(BulkEmailMessage.Compose().To("recipient@postkit.com").AddHeader("X-Padding", new string('p', paddingLength)).Build());

        if (excessBytes == 0)
        {
            var email = builder.Build();
            Assert.Equal(52_428_800, JsonSerializer.SerializeToUtf8Bytes(email.ToBulkEmailRequest(), PostmarkConfiguration.JsonSerializerOptions).Length);
        }
        else
        {
            var exception = Assert.Throws<InvalidOperationException>(builder.Build);
            Assert.Contains("Actual size: 52,428,801 bytes. Limit: 52,428,800 bytes.", exception.Message, StringComparison.Ordinal);
        }
    }
}
