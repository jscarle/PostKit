using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.BulkEmails;
using PostKit.Emails;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientFailureHandlingTests
{
    [Fact]
    public async Task SendEmailAsync_WithNullEmail_ThrowsArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendEmailAsync(null!, CancellationToken.None));

        Assert.Equal("email", exception.ParamName);
        Assert.Equal("The email cannot be null. (Parameter 'email')", exception.Message);
        Assert.False(postmark.PostWasCalled);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithNullEmails_ThrowsArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendEmailBatchAsync(null!, CancellationToken.None));

        Assert.Equal("emails", exception.ParamName);
        Assert.Equal("The email batch cannot be null. (Parameter 'emails')", exception.Message);
        Assert.False(postmark.PostWasCalled);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithNullEmail_ThrowsArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.SendBulkEmailAsync(null!, CancellationToken.None));

        Assert.Equal("email", exception.ParamName);
        Assert.Equal("The bulk email cannot be null. (Parameter 'email')", exception.Message);
        Assert.False(postmark.PostWasCalled);
    }

    [Fact]
    public async Task GetBouncesAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetBouncesAsync(null!, CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
        Assert.Equal("The bounce query cannot be null. (Parameter 'query')", exception.Message);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithNullMessageStream_ThrowsArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetSuppressionsAsync(null!, cancellationToken: CancellationToken.None));

        Assert.Equal("messageStream", exception.ParamName);
        Assert.Equal("The message stream ID cannot be null. (Parameter 'messageStream')", exception.Message);
    }

    [Fact]
    public async Task SuppressionBatchOperations_WithNullArguments_ThrowArgumentNullException()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var nullMessageStream = await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateSuppressionsAsync(null!, ["user@example.com"], CancellationToken.None));
        var nullEmails = await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeleteSuppressionsAsync("broadcast", (IEnumerable<string>)null!, CancellationToken.None));

        Assert.Equal("messageStream", nullMessageStream.ParamName);
        Assert.Equal("The message stream ID cannot be null. (Parameter 'messageStream')", nullMessageStream.Message);
        Assert.Equal("emailAddresses", nullEmails.ParamName);
        Assert.Equal("The suppression email address collection cannot be null. (Parameter 'emailAddresses')", nullEmails.Message);
    }

    [Fact]
    public async Task SendEmailAsync_WhenEmailCannotBePrepared_ReturnsHelpfulFailure()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var result = await client.SendEmailAsync(new Email(), CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The email could not be prepared for sending: From is unexpectedly null.", error.Message);
        Assert.False(postmark.PostWasCalled);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WhenBulkEmailCannotBePrepared_ReturnsHelpfulFailure()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger<PostKitClient>());

        var result = await client.SendBulkEmailAsync(new BulkEmail(), CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bulk email could not be prepared for sending: From is unexpectedly null.", error.Message);
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
