using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientBatchSizeLimitTests
{
    [Fact]
    public async Task SendEmailBatchAsync_WithOversizedBatch_ReturnsFailure()
    {
        var bodySize = (int)PostmarkSizeEstimator.BodySizeLimitInBytes;
        var textBody = new string('a', bodySize);
        var htmlBody = new string('b', bodySize);
        var emailCount = (int)(PostmarkSizeEstimator.BatchPayloadSizeLimitInBytes / PostmarkSizeEstimator.MessageSizeLimitInBytes) + 1;

        var emails = Enumerable.Range(0, emailCount)
            .Select(index => Email.CreateBuilder()
                .From("sender@example.com")
                .To($"recipient{index}@example.com")
                .WithSubject("Batch size check")
                .WithTextBody(textBody)
                .WithHtmlBody(htmlBody)
                .Build())
            .ToList();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailBatchAsync(emails, CancellationToken.None);

        Assert.False(result.IsSuccess());
        Assert.Equal(0, postmark.CallCount);
    }

    private sealed class RecordingPostmarkClient : IPostmarkClient
    {
        public int CallCount { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
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
