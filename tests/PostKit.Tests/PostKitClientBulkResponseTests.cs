using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.BulkEmails;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientBulkResponseTests
{
    [Fact]
    public async Task SendBulkEmailAsync_UsesBulkEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Id": "c42d4a19-b645-4cdd-9859-08d8f24b649a",
                                      "SubmittedAt": "2026-03-11T00:31:10.9843566Z",
                                      "TotalMessages": 7,
                                      "PercentageCompleted": 37.5,
                                      "Status": "Accepted",
                                      "Subject": "Server-calculated subject"
                                    }
                                    """;

        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Bulk hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .AddMessage(BulkEmailMessage.Compose()
                .Cc("cc@postkit.com")
                .Build())
            .Build();

        var postmark = new RecordingPostmarkClient(responseJson);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendBulkEmailAsync(bulkEmail, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/email/bulk", postmark.LastEndpoint);
        Assert.Equal(Guid.Parse("c42d4a19-b645-4cdd-9859-08d8f24b649a"), response.Id);
        Assert.Equal(BulkEmailStatus.Accepted, response.Status);
        Assert.Equal(7, response.TotalMessages);
        Assert.Equal(37.5, response.PercentageCompleted);
        Assert.Equal("Server-calculated subject", response.Subject);
        Assert.Contains("\"Cc\":\"cc@postkit.com\"", postmark.LastRequestJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WhenSubmitStatusIsFailed_ReturnsFailure()
    {
        const string responseJson = """
                                    {
                                      "Id": "c42d4a19-b645-4cdd-9859-08d8f24b649a",
                                      "SubmittedAt": "2026-03-11T00:31:10.9843566Z",
                                      "TotalMessages": 1,
                                      "PercentageCompleted": 100,
                                      "Status": "Failed",
                                      "Subject": "Bulk hello"
                                    }
                                    """;

        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Bulk hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        var postmark = new RecordingPostmarkClient(responseJson);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendBulkEmailAsync(bulkEmail, CancellationToken.None);

        Assert.True(result.IsFailure(), result.ToString());
        Assert.Equal("/email/bulk", postmark.LastEndpoint);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WhenSubmitStatusIsCompleted_MapsCompletionPercentage()
    {
        const string responseJson = """
                                    {
                                      "Id": "c42d4a19-b645-4cdd-9859-08d8f24b649a",
                                      "SubmittedAt": "2026-03-11T00:31:10.9843566Z",
                                      "TotalMessages": 1,
                                      "PercentageCompleted": 100,
                                      "Status": "Completed",
                                      "Subject": "Bulk hello"
                                    }
                                    """;

        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Bulk hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        var postmark = new RecordingPostmarkClient(responseJson);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendBulkEmailAsync(bulkEmail, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(BulkEmailStatus.Completed, response.Status);
        Assert.Equal(100, response.PercentageCompleted);
    }

    [Fact]
    public async Task SendBulkEmailAsync_WhenSubmitResponseOmitsTotalMessages_ReturnsFailure()
    {
        const string responseJson = """
                                    {
                                      "Id": "c42d4a19-b645-4cdd-9859-08d8f24b649a",
                                      "SubmittedAt": "2026-03-11T00:31:10.9843566Z",
                                      "PercentageCompleted": 0,
                                      "Status": "Accepted",
                                      "Subject": "Bulk hello"
                                    }
                                    """;

        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Bulk hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        var postmark = new RecordingPostmarkClient(responseJson);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendBulkEmailAsync(bulkEmail, CancellationToken.None);

        Assert.True(result.IsFailure(), result.ToString());
        Assert.Contains("TotalMessages was not returned", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBulkEmailStatusAsync_UsesBulkStatusEndpoint()
    {
        const string responseJson = """
                                    {
                                      "Id": "c42d4a19-b645-4cdd-9859-08d8f24b649a",
                                      "SubmittedAt": "2026-03-11T00:31:10.9843566Z",
                                      "TotalMessages": 1,
                                      "PercentageCompleted": 100,
                                      "Status": "Completed",
                                      "Subject": "Bulk hello"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(responseJson);
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);
        var bulkRequestId = Guid.Parse("c42d4a19-b645-4cdd-9859-08d8f24b649a");

        var result = await client.GetBulkEmailStatusAsync(bulkRequestId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal($"/email/bulk/{bulkRequestId:D}", postmark.LastEndpoint);
        Assert.Equal(BulkEmailStatus.Completed, response.Status);
        Assert.Equal(100, response.PercentageCompleted);
    }

    private sealed class RecordingPostmarkClient(string responseJson) : IPostmarkClient
    {
        public string? LastEndpoint { get; private set; }

        public string? LastRequestJson { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            LastRequestJson = JsonSerializer.Serialize(body);
            return Task.FromResult(Result.Success(Deserialize<TResponse>()));
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            return Task.FromResult(Result.Success(Deserialize<TResponse>()));
        }

        private TResponse Deserialize<TResponse>()
        {
            var value = JsonSerializer.Deserialize<TResponse>(responseJson);
            return value ?? throw new InvalidOperationException("Response JSON could not be deserialized.");
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
