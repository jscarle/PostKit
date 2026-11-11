using PostKit.Common;
using PostKit.Emails;

namespace PostKit.IntegrationTests;

public class EmailSizeLimitIntegrationTests
{
    // This explicit token always validates without delivery, regardless of local live-test configuration.
    private readonly IPostKitClient _client = TestHelper.CreateClient("POSTMARK_API_TEST");

    [Fact]
    public async Task SendEmailAsync_WithTenMiBRawAttachmentAndBothMaximumBodies_Succeeds()
    {
        var body = new string('a', 5 * 1024 * 1024);
        var attachment = Attachment.Create("boundary.dat", "application/octet-stream", new byte[10 * 1024 * 1024]);
        var email = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("PostKit size boundary")
            .AddAttachment(attachment).TextBody(body).HtmlBody(body).Build();

        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out _), result.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SendEmailAsync_WithNonAsciiBodyAtCharacterLimit_Succeeds(bool html)
    {
        var body = new string('é', 5 * 1024 * 1024);
        var builder = Email.Compose().From("sender@postkit.com").To("recipient@postkit.com").Subject("PostKit Unicode size boundary");
        if (html)
            builder.HtmlBody(body);
        else
            builder.TextBody(body);
        var email = builder.Build();

        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out _), result.ToString());
    }
}
