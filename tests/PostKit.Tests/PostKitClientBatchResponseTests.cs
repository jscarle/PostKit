using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Email;
using PostKit.Responses;

namespace PostKit.Tests;

public class PostKitClientBatchResponseTests
{
    [Fact]
    public async Task SendEmailBatchAsync_MapsSuccessfulItemsToSuccessfulResults()
    {
        var messageId = Guid.Parse("53ee8d49-dd20-4f1a-b65e-8ef299b7a504");
        var submittedAt = new DateTimeOffset(2026, 3, 10, 23, 45, 0, TimeSpan.Zero);
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithSubject("Batch success")
            .WithTextBody("success")
            .Build();

        var postmark = new RecordingPostmarkClient(new List<EmailResponse>
        {
            new()
            {
                MessageId = messageId.ToString("D"),
                SubmittedAt = submittedAt,
                To = "recipient@example.com",
                ErrorCode = 0,
                Message = "OK",
            },
        });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(new[] { email }, CancellationToken.None);

        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);

        var itemResult = Assert.Single(batchResponse.Results);
        Assert.True(itemResult.IsSuccess(out var response), itemResult.ToString());
        Assert.Equal(messageId, response.MessageId);
        Assert.Equal("<53ee8d49-dd20-4f1a-b65e-8ef299b7a504@mtasv.net>", response.InternetMessageId);
        Assert.Equal("recipient@example.com", response.To);
        Assert.Equal(submittedAt, response.SubmittedAt);
    }

    [Fact]
    public async Task SendEmailBatchAsync_MapsRejectedItemsToFailedResults()
    {
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithSubject("Batch failure")
            .WithTextBody("failure")
            .Build();

        var postmark = new RecordingPostmarkClient(new List<EmailResponse>
        {
            new()
            {
                ErrorCode = 300,
                Message = "Invalid email request.",
            },
        });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(new[] { email }, CancellationToken.None);

        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.False(batchResponse.IsSuccessful);

        var itemResult = Assert.Single(batchResponse.Results);
        Assert.True(itemResult.IsFailure(out var error, out SendEmailResponse? _), itemResult.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal(PostmarkErrorCode.InvalidEmailRequest, postmarkError.ErrorCode);
        Assert.Equal("Invalid email request.", postmarkError.Message);
    }

    private sealed class RecordingPostmarkClient(List<EmailResponse> response) : IPostmarkClient
    {
        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            if (typeof(TResponse) != typeof(List<EmailResponse>))
                throw new InvalidOperationException("Unexpected response type.");

            return Task.FromResult(Result.Success((TResponse)(object)response));
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
