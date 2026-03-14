using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Emails;
using PostKit.Postmark;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class PostKitClientBatchSizeLimitTests
{
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

        Assert.False(result.IsSuccess());
        Assert.Equal(0, postmark.CallCount);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithLargeHeadersButPayloadWithinLimit_Succeeds()
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

        var postmark = new RecordingPostmarkClient(new List<PostKit.Postmark.Email.EmailResponse>(Enumerable.Range(0, emails.Count)
            .Select(index => new PostKit.Postmark.Email.EmailResponse
            {
                ErrorCode = 0,
                Message = "OK",
                MessageId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTimeOffset.UtcNow,
                To = $"recipient{index}@postkit.com",
            })));
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(emails, CancellationToken.None);

        Assert.True(result.IsSuccess(), result.ToString());
        Assert.Equal(1, postmark.CallCount);
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
    }

    private sealed class RecordingPostmarkClient(List<PostKit.Postmark.Email.EmailResponse>? responses = null) : IPostmarkClient
    {
        public int CallCount { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            CallCount++;

            if (typeof(TResponse) == typeof(List<PostKit.Postmark.Email.EmailResponse>) && responses is not null)
                return Task.FromResult(Result.Success((TResponse)(object)responses));

            throw new InvalidOperationException("Postmark should not be called for oversized batches.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
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
