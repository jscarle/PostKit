using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Bounces;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientCancellationTests
{
    [Fact]
    public async Task SendEmailAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());
        var email = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Canceled")
            .WithTextBody("Canceled")
            .Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendEmailAsync(email, cts.Token));
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());
        var emails = new[]
        {
            Email.CreateBuilder()
                .From("sender@postkit.com")
                .To("recipient@postkit.com")
                .WithSubject("Canceled")
                .WithTextBody("Canceled")
                .Build(),
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendEmailBatchAsync(emails, cts.Token));
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());
        var bulkEmail = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .WithSubject("Canceled")
            .WithTextBody("Canceled")
            .UsingMessageStream(MessageStream.Broadcast)
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendBulkEmailAsync(bulkEmail, cts.Token));
    }

    [Fact]
    public async Task GetBouncesAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.GetBouncesAsync(new BounceQuery(10, 0), cts.Token));
    }

    [Fact]
    public async Task ActivateBounceAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.ActivateBounceAsync(1, cts.Token));
    }

    [Fact]
    public async Task GetBulkEmailStatusAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.GetBulkEmailStatusAsync(Guid.NewGuid(), cts.Token));
    }

    private sealed class CancelingPostmarkClient : IPostmarkClient
    {
        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            throw new OperationCanceledException(cancellationToken);
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
