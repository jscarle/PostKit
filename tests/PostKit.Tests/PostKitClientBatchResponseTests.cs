using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Emails;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class PostKitClientBatchResponseTests
{
    [Fact]
    public async Task SendEmailBatchAsync_WithNullEmailInSequence_ThrowsHelpfulExceptionWithIndex()
    {
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Batch validation")
            .TextBody("validation")
            .Build();
        IEnumerable<Email> emails = [email, null!];
        var client = new PostKitClient(new RecordingPostmarkClient([]), new TestLogger());

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await client.SendEmailBatchAsync(emails, CancellationToken.None));

        Assert.Equal("emails", exception.ParamName);
        Assert.Equal("The email at index 1 cannot be null. (Parameter 'emails')", exception.Message);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithMixedTemplateAndNonTemplateEmails_ReturnsHelpfulFailure()
    {
        var templatedEmail = Email.FromTemplate(42)
            .From("sender@postkit.com")
            .To("templated@postkit.com")
            .WithModel(new { Name = "Alice" })
            .Build();
        var composedEmail = Email.Compose()
            .From("sender@postkit.com")
            .To("composed@postkit.com")
            .Subject("Composed")
            .TextBody("Composed")
            .Build();
        var client = new PostKitClient(new RecordingPostmarkClient([]), new TestLogger());

        var result = await client.SendEmailBatchAsync([templatedEmail, composedEmail], CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Each email in a batch must either use a template or none may use a template. Found 1 templated and 1 non-templated emails; first templated item index: 0, first non-templated item index: 1.", error.Message);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithSerializationFailure_ReturnsHelpfulFailureWithIndex()
    {
        var client = new PostKitClient(new RecordingPostmarkClient([]), new TestLogger());

        var result = await client.SendEmailBatchAsync([new Email()], CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Batch item 0 could not be prepared for sending: From is unexpectedly null.", error.Message);
    }

    [Fact]
    public async Task SendEmailBatchAsync_MapsSuccessfulItemsToSuccessfulResults()
    {
        var messageId = Guid.Parse("53ee8d49-dd20-4f1a-b65e-8ef299b7a504");
        var submittedAt = new DateTimeOffset(2026, 3, 10, 23, 45, 0, TimeSpan.Zero);
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Batch success")
            .TextBody("success")
            .Build();

        var postmark = new RecordingPostmarkClient([
            new EmailResponse
            {
                MessageId = messageId.ToString("D"),
                SubmittedAt = submittedAt,
                To = "recipient@postkit.com",
                ErrorCode = 0,
                Message = "OK"
            }
        ]);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync([email], CancellationToken.None);

        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);

        var itemResult = Assert.Single(batchResponse.Results);
        Assert.True(itemResult.IsSuccess(out var response), itemResult.ToString());
        Assert.Equal(messageId, response.MessageId);
        Assert.Equal("<53ee8d49-dd20-4f1a-b65e-8ef299b7a504@mtasv.net>", response.InternetMessageId);
        Assert.Equal("recipient@postkit.com", response.To);
        Assert.Equal(submittedAt, response.SubmittedAt);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithKeepIdAndMessageIdHeader_UsesHeaderForInternetMessageId()
    {
        var messageId = Guid.Parse("53ee8d49-dd20-4f1a-b65e-8ef299b7a504");
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Batch keep id")
            .TextBody("success")
            .AddHeader("Message-ID", "<batch-custom@example.com>")
            .AddHeader("X-PM-KeepID", "true")
            .Build();

        var postmark = new RecordingPostmarkClient([
            new EmailResponse
            {
                MessageId = messageId.ToString("D"),
                SubmittedAt = DateTimeOffset.UtcNow,
                To = "recipient@postkit.com",
                ErrorCode = 0,
                Message = "OK"
            }
        ]);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync([email], CancellationToken.None);

        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        var itemResult = Assert.Single(batchResponse.Results);
        Assert.True(itemResult.IsSuccess(out var response), itemResult.ToString());
        Assert.Equal("<batch-custom@example.com>", response.InternetMessageId);
    }

    [Fact]
    public async Task SendEmailBatchAsync_MapsRejectedItemsToFailedResults()
    {
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Batch failure")
            .TextBody("failure")
            .Build();

        var postmark = new RecordingPostmarkClient([
            new EmailResponse { ErrorCode = 300, Message = "Invalid email request." }
        ]);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync([email], CancellationToken.None);

        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.False(batchResponse.IsSuccessful);

        var itemResult = Assert.Single(batchResponse.Results);
        Assert.True(itemResult.IsFailure(out var error, out _), itemResult.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal(PostmarkErrorCode.InvalidEmailRequest, postmarkError.ErrorCode);
        Assert.Equal("Invalid email request.", postmarkError.Message);
    }

    [Fact]
    public async Task SendEmailBatchAsync_MapsUnknownRejectedItemsToFailedResults()
    {
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Batch unknown failure")
            .TextBody("failure")
            .Build();

        var postmark = new RecordingPostmarkClient([
            new EmailResponse { ErrorCode = 999999, Message = "Brand new Postmark error." }
        ]);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync([email], CancellationToken.None);

        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.False(batchResponse.IsSuccessful);

        var itemResult = Assert.Single(batchResponse.Results);
        Assert.True(itemResult.IsFailure(out var error, out _), itemResult.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal((PostmarkErrorCode)999999, postmarkError.ErrorCode);
        Assert.Equal("Brand new Postmark error.", postmarkError.Message);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithUnexpectedResponseCount_ReturnsHelpfulFailure()
    {
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Batch count")
            .TextBody("count")
            .Build();
        var client = new PostKitClient(new RecordingPostmarkClient([]), new TestLogger());

        var result = await client.SendEmailBatchAsync([email], CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Postmark returned an unexpected number of results for the batch request. Expected 1, received 0.", error.Message);
    }

    private sealed class RecordingPostmarkClient(List<EmailResponse> response) : IPostmarkClient
    {
        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            if (typeof(TResponse) != typeof(List<EmailResponse>))
                throw new InvalidOperationException("Unexpected response type.");

            return Task.FromResult(Result.Success((TResponse)(object)response));
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("GetAsync should not be called in this test.");
        }
    }

    private sealed class TestLogger : ILogger<PostKitClient>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}