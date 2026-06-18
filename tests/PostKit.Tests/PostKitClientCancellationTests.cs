using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Postmark;
using ActivateBounceModel = PostKit.Postmark.Bounces.ActivateBounceResponse;
using BounceModel = PostKit.Postmark.Bounces.BounceResponse;

namespace PostKit.Tests;

public class PostKitClientCancellationTests
{
    [Fact]
    public async Task SendEmailAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Canceled")
            .TextBody("Canceled")
            .Build();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendEmailAsync(email, cts.Token));
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());
        var emails = new[]
        {
            Email.Compose()
                .From("sender@postkit.com")
                .To("recipient@postkit.com")
                .Subject("Canceled")
                .TextBody("Canceled")
                .Build()
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendEmailBatchAsync(emails, cts.Token));
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());
        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Canceled")
            .TextBody("Canceled")
            .UseMessageStream(MessageStream.Broadcast)
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendBulkEmailAsync(bulkEmail, cts.Token));
    }

    [Fact]
    public async Task GetBouncesAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.GetBouncesAsync("outbound", 10, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.GetSuppressionsAsync("broadcast", cts.Token));
    }

    [Fact]
    public async Task CreateSuppressionsAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.CreateSuppressionsAsync("broadcast", ["user@example.com"], cts.Token));
    }

    [Fact]
    public async Task DeleteSuppressionsAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.DeleteSuppressionsAsync("broadcast", ["user@example.com"], cts.Token));
    }

    [Fact]
    public async Task ActivateBounceAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.ActivateBounceAsync(1, cts.Token));
    }

    [Fact]
    public async Task ActivateBounceAsync_WhenCanceledDuringConfirmation_PropagatesCancellation()
    {
        var postmark = new BounceConfirmationCancelingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.ActivateBounceAsync(1, cts.Token));
        Assert.Equal(["/bounces/1/activate", "/bounces/1"], postmark.CalledEndpoints);
    }

    [Fact]
    public async Task GetBulkEmailStatusAsync_WithCanceledToken_PropagatesCancellation()
    {
        var client = new PostKitClient(new CancelingPostmarkClient(), new TestLogger());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => client.GetBulkEmailStatusAsync(Guid.NewGuid(), cts.Token));
    }

    private sealed class CancelingPostmarkClient : IPostmarkClient
    {
        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private sealed class BounceConfirmationCancelingPostmarkClient : IPostmarkClient
    {
        public List<string> CalledEndpoints { get; } = [];

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var bounce = CreateBounceModel(true);
            return Task.FromResult(Result.Success((TResponse)(object)bounce));
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var activation = new ActivateBounceModel { Message = "OK", Bounce = CreateBounceModel(true) };

            return Task.FromResult(Result.Success((TResponse)(object)activation));
        }

        private static BounceModel CreateBounceModel(bool inactive)
        {
            return new BounceModel
            {
                Id = 1,
                Type = "HardBounce",
                Name = "Hard bounce",
                Tag = "",
                MessageId = "69ce4784-c202-41c6-a1a9-91757022b25e",
                ServerId = 18451835,
                MessageStream = "outbound",
                Description = "The server was unable to deliver your message.",
                Details = "smtp;550 mailbox unavailable",
                Email = "HardBounce@bounce-testing.postmarkapp.com",
                From = "from@postkit.com",
                BouncedAt = DateTimeOffset.Parse("2026-03-11T17:33:38Z"),
                DumpAvailable = true,
                Inactive = inactive,
                CanActivate = true,
                Subject = "PostKit Bounces API probe"
            };
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
