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
        var email1 = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Batch Email 1")
            .TextBody("This is the first email in the batch.")
            .Build();

        var email2 = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To("another@postkit.com")
            .Subject("Batch Email 2")
            .TextBody("This is the second email in the batch.")
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
            var email = Email.Compose()
                .From(TestConfiguration.TestFromEmail)
                .To($"recipient{i}@postkit.com")
                .Subject($"Batch Email {i}")
                .TextBody($"This is email number {i} in the batch.")
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
        var textEmail = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Text Email in Batch")
            .TextBody("This is a text email.")
            .Build();

        var htmlEmail = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To("recipient2@postkit.com")
            .Subject("HTML Email in Batch")
            .HtmlBody("<html><body><h1>HTML Email</h1></body></html>")
            .Build();

        var multipartEmail = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To("recipient3@postkit.com")
            .Subject("Multipart Email in Batch")
            .TextBody("Text version")
            .HtmlBody("<html><body><p>HTML version</p></body></html>")
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

        var email1 = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Batch Email with Attachment 1")
            .TextBody("This email has attachment 1.")
            .AddAttachment(attachment1)
            .Build();

        var email2 = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To("another@postkit.com")
            .Subject("Batch Email with Attachment 2")
            .TextBody("This email has attachment 2.")
            .AddAttachment(attachment2)
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
        var email1 = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Batch Email with Metadata 1")
            .HtmlBody("<html><body><p>Email with tracking.</p></body></html>")
            .AddMetadata("batch_id", "1")
            .EnableOpenTracking()
            .UseLinkTracking(LinkTracking.HtmlOnly)
            .Build();

        var email2 = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To("another@postkit.com")
            .Subject("Batch Email with Metadata 2")
            .HtmlBody("<html><body><p>Another email with tracking.</p></body></html>")
            .AddMetadata("batch_id", "2")
            .EnableOpenTracking()
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
        var transactionalEmail = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Transactional Email")
            .TextBody("This is a transactional email.")
            .UseMessageStream(MessageStream.Transactional)
            .Build();

        var broadcastEmail = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To("another@postkit.com")
            .Subject("Broadcast Email")
            .TextBody("This is a broadcast email.")
            .UseMessageStream(MessageStream.Broadcast)
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
            var email = Email.Compose()
                .From(TestConfiguration.TestFromEmail)
                .To($"recipient{i}@postkit.com")
                .Subject($"Batch Email {i}")
                .TextBody($"Email {i} of 500.")
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

        var email1 = Email.Compose()
            .From("Batch Sender", TestConfiguration.TestFromEmail)
            .To("Recipient 1", TestConfiguration.TestToEmail)
            .Cc(TestConfiguration.TestCcEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .Subject("Complex Batch Email 1")
            .TextBody("Text version of complex email.")
            .HtmlBody("<html><body><h1>Complex Email</h1></body></html>")
            .WithTag("complex-batch")
            .AddHeader("X-Batch-Id", "batch-1")
            .AddMetadata("email_type", "complex")
            .EnableOpenTracking()
            .AddAttachment(attachment)
            .Build();

        var email2 = Email.Compose()
            .From("Batch Sender", TestConfiguration.TestFromEmail)
            .To("Recipient 2", "recipient2@postkit.com")
            .Bcc(TestConfiguration.TestBccEmail)
            .Subject("Complex Batch Email 2")
            .HtmlBody("<html><body><p>Another complex email with <a href='https://example.com'>link</a>.</p></body></html>")
            .WithTag("complex-batch")
            .AddMetadata("email_type", "complex")
            .UseLinkTracking()
            .UseMessageStream(MessageStream.Broadcast)
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

    private static void AssertSuccessfulBatchResult(Result<EmailSubmission> result)
    {
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
        Assert.Equal($"<{response.MessageId:D}@mtasv.net>", response.InternetMessageId);
    }
}
