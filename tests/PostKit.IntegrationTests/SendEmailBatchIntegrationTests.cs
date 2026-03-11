using LightResults;
using PostKit.Common;
using PostKit.Emails;

namespace PostKit.IntegrationTests;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
/// <summary>Integration tests for sending batch emails through Postmark API.</summary>
public class SendEmailBatchIntegrationTests
{
    private readonly IPostKitClient _client = TestHelper.CreateClient();

    [Fact]
    public async Task SendEmailBatchAsync_WithTwoEmails_Succeeds()
    {
        // Arrange
        var email1 = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Batch Email 1")
            .WithTextBody("This is the first email in the batch.")
            .Build();

        var email2 = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("another@example.com")
            .WithSubject("Batch Email 2")
            .WithTextBody("This is the second email in the batch.")
            .Build();

        var emails = new[] { email1, email2 };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(2, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithMultipleEmails_Succeeds()
    {
        // Arrange
        var emails = new List<Email>();
        for (var i = 1; i <= 5; i++)
        {
            var email = Email.CreateBuilder()
                .From(TestConfiguration.TestFromEmail)
                .To($"recipient{i}@example.com")
                .WithSubject($"Batch Email {i}")
                .WithTextBody($"This is email number {i} in the batch.")
                .WithTag($"batch-{i}")
                .Build();
            emails.Add(email);
        }

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(5, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithDifferentEmailTypes_Succeeds()
    {
        // Arrange
        var textEmail = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Text Email in Batch")
            .WithTextBody("This is a text email.")
            .Build();

        var htmlEmail = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("recipient2@example.com")
            .WithSubject("HTML Email in Batch")
            .WithHtmlBody("<html><body><h1>HTML Email</h1></body></html>")
            .Build();

        var multipartEmail = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("recipient3@example.com")
            .WithSubject("Multipart Email in Batch")
            .WithTextBody("Text version")
            .WithHtmlBody("<html><body><p>HTML version</p></body></html>")
            .Build();

        var emails = new[] { textEmail, htmlEmail, multipartEmail };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(3, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithAttachments_Succeeds()
    {
        // Arrange
        var attachment1 = Attachment.Create("doc1.txt", "text/plain", "Document 1 content"u8.ToArray());
        var attachment2 = Attachment.Create("doc2.txt", "text/plain", "Document 2 content"u8.ToArray());

        var email1 = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Batch Email with Attachment 1")
            .WithTextBody("This email has attachment 1.")
            .WithAttachment(attachment1)
            .Build();

        var email2 = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("another@example.com")
            .WithSubject("Batch Email with Attachment 2")
            .WithTextBody("This email has attachment 2.")
            .WithAttachment(attachment2)
            .Build();

        var emails = new[] { email1, email2 };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(2, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithMetadataAndTracking_Succeeds()
    {
        // Arrange
        var email1 = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Batch Email with Metadata 1")
            .WithHtmlBody("<html><body><p>Email with tracking.</p></body></html>")
            .WithMetadata("batch_id", "1")
            .WithOpenTracking()
            .WithLinkTracking(LinkTracking.HtmlOnly)
            .Build();

        var email2 = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("another@example.com")
            .WithSubject("Batch Email with Metadata 2")
            .WithHtmlBody("<html><body><p>Another email with tracking.</p></body></html>")
            .WithMetadata("batch_id", "2")
            .WithOpenTracking()
            .Build();

        var emails = new[] { email1, email2 };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(2, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithDifferentMessageStreams_Succeeds()
    {
        // Arrange
        var transactionalEmail = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Transactional Email")
            .WithTextBody("This is a transactional email.")
            .UsingMessageStream(MessageStream.Transactional)
            .Build();

        var broadcastEmail = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To("another@example.com")
            .WithSubject("Broadcast Email")
            .WithTextBody("This is a broadcast email.")
            .UsingMessageStream(MessageStream.Broadcast)
            .Build();

        var emails = new[] { transactionalEmail, broadcastEmail };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(2, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithMaximumBatchSize_Succeeds()
    {
        // Arrange - Postmark allows up to 500 emails per batch
        var emails = new List<Email>();
        for (var i = 1; i <= 500; i++)
        {
            var email = Email.CreateBuilder()
                .From(TestConfiguration.TestFromEmail)
                .To($"recipient{i}@example.com")
                .WithSubject($"Batch Email {i}")
                .WithTextBody($"Email {i} of 500.")
                .Build();
            emails.Add(email);
        }

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(500, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithComplexEmails_Succeeds()
    {
        // Arrange
        var attachment = Attachment.Create("report.txt", "text/plain", "Report data"u8.ToArray());

        var email1 = Email.CreateBuilder()
            .From("Batch Sender", TestConfiguration.TestFromEmail)
            .To("Recipient 1", TestConfiguration.TestToEmail)
            .Cc(TestConfiguration.TestCcEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .WithSubject("Complex Batch Email 1")
            .WithTextBody("Text version of complex email.")
            .WithHtmlBody("<html><body><h1>Complex Email</h1></body></html>")
            .WithTag("complex-batch")
            .WithHeader("X-Batch-Id", "batch-1")
            .WithMetadata("email_type", "complex")
            .WithOpenTracking()
            .WithAttachment(attachment)
            .Build();

        var email2 = Email.CreateBuilder()
            .From("Batch Sender", TestConfiguration.TestFromEmail)
            .To("Recipient 2", "recipient2@example.com")
            .Bcc(TestConfiguration.TestBccEmail)
            .WithSubject("Complex Batch Email 2")
            .WithHtmlBody("<html><body><p>Another complex email with <a href='https://example.com'>link</a>.</p></body></html>")
            .WithTag("complex-batch")
            .WithMetadata("email_type", "complex")
            .WithLinkTracking()
            .UsingMessageStream(MessageStream.Broadcast)
            .Build();

        var emails = new[] { email1, email2 };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(2, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, AssertSuccessfulBatchResult);
    }

    private static void AssertSuccessfulBatchResult(Result<SendEmailResponse> result)
    {
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
        Assert.Equal($"<{response.MessageId:D}@mtasv.net>", response.InternetMessageId);
    }
}
