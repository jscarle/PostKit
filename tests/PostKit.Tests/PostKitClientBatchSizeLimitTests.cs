using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Emails;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class PostKitClientBatchSizeLimitTests
{
    [Fact]
    public async Task SendEmailBatchAsync_WithTooManyEmails_ReturnsFailureWithActualCount()
    {
        var emails = Enumerable.Range(0, 501)
            .Select(index => Email.Compose()
                .From("sender@postkit.com")
                .To($"recipient{index}@postkit.com")
                .Subject("Batch count check")
                .TextBody("Count check")
                .Build())
            .ToList();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(emails, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Postmark only accepts 500 emails per batch request. Received 501.", error.Message);
        Assert.Equal(0, postmark.CallCount);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithOversizedBatch_ReturnsFailure()
    {
        var bodySize = 4 * 1024 * 1024;
        var textBody = new string('a', bodySize);
        var htmlBody = new string('b', bodySize);
        const int emailCount = 7;

        var emails = Enumerable.Range(0, emailCount)
            .Select(index => Email.Compose()
                .From("sender@postkit.com")
                .To($"recipient{index}@postkit.com")
                .Subject("Batch size check")
                .TextBody(textBody)
                .HtmlBody(htmlBody)
                .Build())
            .ToList();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(emails, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.StartsWith("Estimated batch payload size exceeds Postmark's 50 MB limit. Estimated size: ", error.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 52,428,800 bytes.", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, postmark.CallCount);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithLargeHeadersPushingPastBatchLimit_ReturnsFailure()
    {
        var textBody = new string('a', 4 * 1024 * 1024);
        var largeHeaderValue = new string('h', 1024 * 1024);
        var emails = Enumerable.Range(0, 11)
            .Select(index => Email.Compose()
                .From("sender@postkit.com")
                .To($"recipient{index}@postkit.com")
                .Subject("Batch size check")
                .TextBody(textBody)
                .AddHeader("X-Large-Header", largeHeaderValue)
                .Build())
            .ToList();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(emails, CancellationToken.None);

        Assert.False(result.IsSuccess());
        Assert.Equal(0, postmark.CallCount);
        Assert.Contains("Estimated batch payload size exceeds", result.ToString(), StringComparison.Ordinal);
        Assert.Contains("Estimated size:", result.ToString(), StringComparison.Ordinal);
        Assert.Contains("Limit: 52,428,800 bytes.", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithLargeTemplateModelsPushingPastEstimatedBatchLimit_ReturnsFailure()
    {
        var emails = Enumerable.Range(0, 11)
            .Select(index => Email.FromTemplate(42)
                .From("sender@postkit.com")
                .To($"recipient{index}@postkit.com")
                .WithModel(new { Data = new string('x', 5 * 1024 * 1024) })
                .Build())
            .ToList();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(emails, CancellationToken.None);

        Assert.False(result.IsSuccess());
        Assert.Equal(0, postmark.CallCount);
        Assert.Contains("Estimated batch payload size exceeds", result.ToString(), StringComparison.Ordinal);
        Assert.Contains("Estimated size:", result.ToString(), StringComparison.Ordinal);
        Assert.Contains("Limit: 52,428,800 bytes.", result.ToString(), StringComparison.Ordinal);
    }

    private sealed class RecordingPostmarkClient(List<EmailResponse>? responses = null) : IPostmarkClient
    {
        public int CallCount { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            CallCount++;

            if (typeof(TResponse) == typeof(List<EmailResponse>) && responses is not null)
                return Task.FromResult(Result.Success((TResponse)(object)responses));

            throw new InvalidOperationException("Postmark should not be called for oversized batches.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Postmark should not be called for oversized batches.");
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
