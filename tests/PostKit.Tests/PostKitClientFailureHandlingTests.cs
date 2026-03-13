using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientFailureHandlingTests
{
    [Fact]
    public async Task SendEmailAsync_WithNullEmail_ThrowsArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendEmailAsync(null!, CancellationToken.None));
        Assert.False(postmark.PostWasCalled);
    }

    private sealed class RecordingPostmarkClient : IPostmarkClient
    {
        public bool PostWasCalled { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            PostWasCalled = true;
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("GetAsync should not be called in this test.");
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
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
