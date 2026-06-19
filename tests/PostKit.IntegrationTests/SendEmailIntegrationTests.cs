using PostKit.Common;
using PostKit.Emails;

namespace PostKit.IntegrationTests;

/// <summary>Integration tests for sending emails through Postmark API.</summary>
public class SendEmailIntegrationTests
{
    private readonly IPostKitClient _client = TestHelper.CreateClient();

    [Fact]
    public async Task SendEmailAsync_WithSimpleTextEmail_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Simple Text Email Test")
            .TextBody("This is a simple text email for integration testing.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
        Assert.Equal($"<{response.MessageId:D}@mtasv.net>", response.InternetMessageId);
        Assert.Equal(TestConfiguration.TestToEmail, response.To);
        Assert.NotEqual(default, response.SubmittedAt);
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlEmail_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("HTML Email Test")
            .HtmlBody("<html><body><h1>HTML Email</h1><p>This is a <strong>HTML</strong> email.</p></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
        Assert.Equal(TestConfiguration.TestToEmail, response.To);
    }

    [Fact]
    public async Task SendEmailAsync_WithTextAndHtmlEmail_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Multipart Email Test")
            .TextBody("This is the plain text version.")
            .HtmlBody("<html><body><h1>HTML Version</h1><p>This is the <strong>HTML</strong> version.</p></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleRecipients_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .To("another@postkit.com")
            .Subject("Multiple Recipients Test")
            .TextBody("This email is sent to multiple recipients.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithCcRecipients_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Cc(TestConfiguration.TestCcEmail)
            .Subject("CC Recipients Test")
            .TextBody("This email has CC recipients.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithBccRecipients_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Bcc(TestConfiguration.TestBccEmail)
            .Subject("BCC Recipients Test")
            .TextBody("This email has BCC recipients.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithReplyTo_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .Subject("Reply-To Test")
            .TextBody("This email has a Reply-To address.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithTag_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Tag Test")
            .TextBody("This email has a tag.")
            .WithTag("integration-test")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithCustomHeaders_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Custom Headers Test")
            .TextBody("This email has custom headers.")
            .AddHeader("X-Custom-Header", "CustomValue")
            .AddHeader("X-Another-Header", "AnotherValue")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMetadata_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Metadata Test")
            .TextBody("This email has metadata.")
            .AddMetadata("user_id", "12345")
            .AddMetadata("campaign", "integration-test")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithOpenTracking_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Open Tracking Test")
            .HtmlBody("<html><body><p>This email has open tracking enabled.</p></body></html>")
            .EnableOpenTracking()
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithLinkTracking_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Link Tracking Test")
            .HtmlBody("<html><body><p>Click <a href='https://example.com'>here</a> for link tracking.</p></body></html>")
            .UseLinkTracking()
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithAttachment_Succeeds()
    {
        // Arrange
        var attachmentContent = "This is a test attachment content."u8.ToArray();
        var attachment = Attachment.Create("test.txt", "text/plain", attachmentContent);

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Attachment Test")
            .TextBody("This email has an attachment.")
            .AddAttachment(attachment)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleAttachments_Succeeds()
    {
        // Arrange
        var attachment1 = Attachment.Create("file1.txt", "text/plain", "Content 1"u8.ToArray());
        var attachment2 = Attachment.Create("file2.txt", "text/plain", "Content 2"u8.ToArray());

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Multiple Attachments Test")
            .TextBody("This email has multiple attachments.")
            .AddAttachment(attachment1)
            .AddAttachment(attachment2)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMessageStream_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Message Stream Test")
            .TextBody("This email uses a specific message stream.")
            .UseMessageStream(MessageStream.Broadcast)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithFromName_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail, "Test Sender")
            .To(TestConfiguration.TestToEmail)
            .Subject("From Name Test")
            .TextBody("This email has a sender name.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithToName_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail, "Test Recipient")
            .Subject("To Name Test")
            .TextBody("This email has a recipient name.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithAllFeatures_Succeeds()
    {
        // Arrange
        var attachment = Attachment.Create("document.txt", "text/plain", "Document content"u8.ToArray());

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail, "Integration Test Sender")
            .To(TestConfiguration.TestToEmail, "Primary Recipient")
            .To("second@postkit.com")
            .Cc(TestConfiguration.TestCcEmail)
            .Bcc(TestConfiguration.TestBccEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .Subject("Comprehensive Feature Test")
            .TextBody("This is the plain text version with all features.")
            .HtmlBody("<html><body><h1>Comprehensive Test</h1><p>Testing <strong>all features</strong>.</p></body></html>")
            .WithTag("comprehensive-test")
            .AddHeader("X-Test-Header", "TestValue")
            .AddMetadata("test_type", "comprehensive")
            .AddMetadata("feature_count", "all")
            .EnableOpenTracking()
            .UseLinkTracking()
            .AddAttachment(attachment)
            .UseMessageStream(MessageStream.Broadcast)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }
}