using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Bounces;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Emails;
using ActivateBounceModel = PostKit.Postmark.Bounces.ActivateBounceResponse;
using BounceModel = PostKit.Postmark.Bounces.BounceResponse;
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

    private sealed class BounceConfirmationCancelingPostmarkClient : IPostmarkClient
    {
        public List<string> CalledEndpoints { get; } = [];

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var bounce = CreateBounceModel(inactive: true);
            return Task.FromResult(Result.Success((TResponse)(object)bounce));
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var activation = new ActivateBounceModel
            {
                Message = "OK",
                Bounce = CreateBounceModel(inactive: true),
            };

            return Task.FromResult(Result.Success((TResponse)(object)activation));
        }

        private static BounceModel CreateBounceModel(bool inactive)
        {
            return new BounceModel
            {
                Id = 1,
                Type = "HardBounce",
                TypeCode = 1,
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
                Subject = "PostKit Bounces API probe",
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
