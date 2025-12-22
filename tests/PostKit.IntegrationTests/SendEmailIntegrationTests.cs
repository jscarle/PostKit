namespace PostKit.IntegrationTests;

/// <summary>Integration tests for sending emails through Postmark API.</summary>
public class SendEmailIntegrationTests
{
    private readonly IPostKitClient _client = TestHelper.CreateClient();

    [Fact]
    public async Task SendEmailAsync_WithSimpleTextEmail_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Simple Text Email Test")
            .WithTextBody("This is a simple text email for integration testing.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlEmail_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("HTML Email Test")
            .WithHtmlBody("<html><body><h1>HTML Email</h1><p>This is a <strong>HTML</strong> email.</p></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithTextAndHtmlEmail_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Multipart Email Test")
            .WithTextBody("This is the plain text version.")
            .WithHtmlBody("<html><body><h1>HTML Version</h1><p>This is the <strong>HTML</strong> version.</p></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleRecipients_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .AlsoTo("another@example.com")
            .WithSubject("Multiple Recipients Test")
            .WithTextBody("This email is sent to multiple recipients.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithCcRecipients_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Cc(TestConfiguration.TestCcEmail)
            .WithSubject("CC Recipients Test")
            .WithTextBody("This email has CC recipients.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithBccRecipients_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Bcc(TestConfiguration.TestBccEmail)
            .WithSubject("BCC Recipients Test")
            .WithTextBody("This email has BCC recipients.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithReplyTo_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .WithSubject("Reply-To Test")
            .WithTextBody("This email has a Reply-To address.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithTag_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Tag Test")
            .WithTextBody("This email has a tag.")
            .WithTag("integration-test")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithCustomHeaders_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Custom Headers Test")
            .WithTextBody("This email has custom headers.")
            .WithHeader("X-Custom-Header", "CustomValue")
            .WithHeader("X-Another-Header", "AnotherValue")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMetadata_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Metadata Test")
            .WithTextBody("This email has metadata.")
            .WithMetadata("user_id", "12345")
            .WithMetadata("campaign", "integration-test")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithOpenTracking_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Open Tracking Test")
            .WithHtmlBody("<html><body><p>This email has open tracking enabled.</p></body></html>")
            .WithOpenTracking()
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithLinkTracking_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Link Tracking Test")
            .WithHtmlBody("<html><body><p>Click <a href='https://example.com'>here</a> for link tracking.</p></body></html>")
            .WithLinkTracking()
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithAttachment_Succeeds()
    {
        // Arrange
        var attachmentContent = "This is a test attachment content."u8.ToArray();
        var attachment = Attachment.Create("test.txt", "text/plain", attachmentContent);

        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Attachment Test")
            .WithTextBody("This email has an attachment.")
            .WithAttachment(attachment)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleAttachments_Succeeds()
    {
        // Arrange
        var attachment1 = Attachment.Create("file1.txt", "text/plain", "Content 1"u8.ToArray());
        var attachment2 = Attachment.Create("file2.txt", "text/plain", "Content 2"u8.ToArray());

        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Multiple Attachments Test")
            .WithTextBody("This email has multiple attachments.")
            .WithAttachment(attachment1)
            .WithAttachment(attachment2)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMessageStream_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Message Stream Test")
            .WithTextBody("This email uses a specific message stream.")
            .UsingMessageStream(MessageStream.Broadcast)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithFromName_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From("Test Sender", TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("From Name Test")
            .WithTextBody("This email has a sender name.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithToName_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("Test Recipient", TestConfiguration.TestToEmail)
            .WithSubject("To Name Test")
            .WithTextBody("This email has a recipient name.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithAllFeatures_Succeeds()
    {
        // Arrange
        var attachment = Attachment.Create("document.txt", "text/plain", "Document content"u8.ToArray());

        var email = Email.CreateBuilder()
            .From("Integration Test Sender", TestConfiguration.TestFromEmail)
            .To("Primary Recipient", TestConfiguration.TestToEmail)
            .AlsoTo("second@example.com")
            .Cc(TestConfiguration.TestCcEmail)
            .Bcc(TestConfiguration.TestBccEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .WithSubject("Comprehensive Feature Test")
            .WithTextBody("This is the plain text version with all features.")
            .WithHtmlBody("<html><body><h1>Comprehensive Test</h1><p>Testing <strong>all features</strong>.</p></body></html>")
            .WithTag("comprehensive-test")
            .WithHeader("X-Test-Header", "TestValue")
            .WithMetadata("test_type", "comprehensive")
            .WithMetadata("feature_count", "all")
            .WithOpenTracking()
            .WithLinkTracking()
            .WithAttachment(attachment)
            .UsingMessageStream(MessageStream.Broadcast)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEmpty(response.MessageId);
    }
}
