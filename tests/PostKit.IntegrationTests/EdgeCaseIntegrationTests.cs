using PostKit.Common;
using PostKit.Emails;

namespace PostKit.IntegrationTests;

/// <summary>Integration tests for edge cases and boundary conditions with Postmark API.</summary>
public class EdgeCaseIntegrationTests
{
    private readonly IPostKitClient _client = TestHelper.CreateClient();

    [Fact]
    public async Task SendEmailAsync_WithExactly50Recipients_Succeeds()
    {
        // Arrange - Test the maximum allowed recipients (50)
        var builder = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .Subject("50 Recipients Test")
            .TextBody("Testing exact maximum recipient limit.");

        var toBuilder = builder.To("recipient@postkit.com");
        // Add exactly 50 recipients
        for (var i = 1; i < 50; i++)
            toBuilder.To($"recipient{i}@postkit.com");

        var email = builder.Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMixedRecipientsAtLimit_Succeeds()
    {
        // Arrange - 20 To + 15 Cc + 15 Bcc = 50 (at limit)
        var builder = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .Subject("Mixed Recipients at Limit")
            .TextBody("Testing mixed recipient types at maximum limit.");

        var toBuilder = builder.To("to@postkit.com");
        for (var i = 1; i < 20; i++)
            toBuilder.To($"to{i}@postkit.com");

        var ccBuilder = builder.Cc("cc@postkit.com");
        for (var i = 1; i < 15; i++)
            ccBuilder.Cc($"cc{i}@postkit.com");

        var bccBuilder = builder.Bcc("bcc@postkit.com");
        for (var i = 1; i < 15; i++)
            bccBuilder.Bcc($"bcc{i}@postkit.com");

        var email = builder.Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMinimalContent_Succeeds()
    {
        // Arrange - Absolute minimum required fields
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("M")
            .TextBody("X")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithVeryLongSubject_Succeeds()
    {
        // Arrange
        var longSubject = new string('S', 500);
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject(longSubject)
            .TextBody("Testing very long subject line.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithLargeButValidTextBody_Succeeds()
    {
        // Arrange - Just under the 5 MB limit
        var largeText = new string('A', 4 * 1024 * 1024); // 4 MB
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Large Text Body")
            .TextBody(largeText)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleReplyToAddresses_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .ReplyTo("another-reply@postkit.com")
            .ReplyTo("third-reply@postkit.com")
            .Subject("Multiple Reply-To Test")
            .TextBody("Testing multiple Reply-To addresses.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMaximumHeaderCount_Succeeds()
    {
        // Arrange - Add many custom headers
        var builder = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Many Headers Test")
            .TextBody("Testing many custom headers.");

        for (var i = 1; i <= 10; i++)
            builder.AddHeader($"X-Custom-Header-{i}", $"Value-{i}");

        var email = builder.Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMaximumMetadataCount_Succeeds()
    {
        // Arrange - Add many metadata key-value pairs
        var builder = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Many Metadata Test")
            .TextBody("Testing many metadata entries.");

        for (var i = 1; i <= 10; i++)
            builder.AddMetadata($"key_{i}", $"value_{i}");

        var email = builder.Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithVeryLongMetadataValues_Succeeds()
    {
        // Arrange
        var longValue = new string('V', 80);
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Long Metadata Values Test")
            .TextBody("Testing long metadata values.")
            .AddMetadata("long_key", longValue)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyStringMetadata_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Empty Metadata Test")
            .TextBody("Testing empty metadata values.")
            .AddMetadata("metadata_key", string.Empty)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithWhitespaceOnlyContent_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("   ")
            .TextBody("   \t\n   ")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithSameAddressInToAndCc_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Cc(TestConfiguration.TestToEmail) // Same address
            .Subject("Duplicate Address Test")
            .TextBody("Testing same address in To and Cc.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithVeryLongTag_Succeeds()
    {
        // Arrange
        var longTag = new string('T', 200);
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Long Tag Test")
            .TextBody("Testing very long tag.")
            .WithTag(longTag)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithBinaryAttachment_Succeeds()
    {
        // Arrange - Create binary data with all byte values
        var binaryData = new byte[256];
        for (var i = 0; i < 256; i++)
            binaryData[i] = (byte)i;

        var attachment = Attachment.Create("binary.zip", "application/octet-stream", binaryData);

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Binary Attachment Test")
            .TextBody("Testing binary attachment.")
            .AddAttachment(attachment)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithLargeAttachment_Succeeds()
    {
        // Arrange - Create a large but valid attachment (under message size limit)
        var largeData = new byte[2 * 1024 * 1024]; // 2 MB
        Array.Fill(largeData, (byte)'D');
        var attachment = Attachment.Create("large.dat", "application/octet-stream", largeData);

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Large Attachment Test")
            .TextBody("Testing large attachment.")
            .AddAttachment(attachment)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleAttachmentsSameFilename_Succeeds()
    {
        // Arrange
        var attachment1 = Attachment.Create("document.txt", "text/plain", "Content 1"u8.ToArray());
        var attachment2 = Attachment.Create("document.txt", "text/plain", "Content 2"u8.ToArray());

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Duplicate Filename Test")
            .TextBody("Testing attachments with same filename.")
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
    public async Task SendEmailAsync_WithSpecialCharactersInFilename_Succeeds()
    {
        // Arrange
        var attachment = Attachment.Create("file-name_with.special~chars (1).txt", "text/plain", "Content"u8.ToArray());

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Special Filename Test")
            .TextBody("Testing attachment with special characters in filename.")
            .AddAttachment(attachment)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithSingleEmail_Succeeds()
    {
        // Arrange - Edge case: batch with only one email
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Single Email Batch")
            .TextBody("Testing batch with single email.")
            .Build();

        var emails = new[] { email };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Single(batchResponse.Results);
        var result0 = batchResponse.Results[0];
        Assert.True(result0.IsSuccess(out var response0), result0.ToString());
        Assert.NotEqual(Guid.Empty, response0.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlOnlyNoText_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("HTML Only Test")
            .HtmlBody("<html><body><h1>Only HTML, no text version</h1></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithComplexHtmlStructure_Succeeds()
    {
        // Arrange
        var complexHtml = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body { font-family: Arial, sans-serif; }
        .container { max-width: 600px; margin: 0 auto; }
        .header { background-color: #007bff; color: white; padding: 20px; }
        .content { padding: 20px; }
        table { width: 100%; border-collapse: collapse; }
        td { border: 1px solid #ddd; padding: 8px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Complex HTML Email</h1>
        </div>
        <div class='content'>
            <p>This is a <strong>complex</strong> HTML email with:</p>
            <ul>
                <li>Lists</li>
                <li>Links: <a href='https://example.com'>Example</a></li>
                <li>Images: <img src='https://via.placeholder.com/150' alt='Placeholder'></li>
            </ul>
            <table>
                <tr><td>Row 1</td><td>Data 1</td></tr>
                <tr><td>Row 2</td><td>Data 2</td></tr>
            </table>
        </div>
    </div>
</body>
</html>";

        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Complex HTML Test")
            .HtmlBody(complexHtml)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }
}