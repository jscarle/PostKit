using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.DataRemovals;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostmarkDataRemovalRequest = PostKit.Postmark.DataRemovals.DataRemovalCreateRequest;

namespace PostKit.Tests;

public class PostKitClientDataRemovalResponseTests
{
    [Fact]
    public async Task CreateDataRemovalAsync_UsesAccountPostAndMapsResponse()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/data-removals"] = """{ "ID": 1234, "Status": "Pending" }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateDataRemovalAsync(new DataRemovalCreateParameters
        {
            RequestedBy = "requester@example.com",
            RequestedFor = "recipient@example.com",
            NotifyWhenCompleted = true
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var dataRemoval), result.ToString());
        Assert.Equal(1234, dataRemoval.Id);
        Assert.Equal(DataRemovalStatus.Pending, dataRemoval.Status);
        Assert.Equal("/data-removals", postmark.LastAccountPostEndpoint);
        var body = Assert.IsType<PostmarkDataRemovalRequest>(postmark.LastAccountPostBody);
        Assert.Equal("requester@example.com", body.RequestedBy);
        Assert.Equal("recipient@example.com", body.RequestedFor);
        Assert.True(body.NotifyWhenCompleted);
    }

    [Fact]
    public async Task GetDataRemovalAsync_UsesAccountGetAndMapsResponse()
    {
        var postmark = new RecordingPostmarkClient(accountGetResponses: new Dictionary<string, string>
        {
            ["/data-removals/1234"] = """{ "ID": 1234, "Status": "Done" }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetDataRemovalAsync(1234, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var dataRemoval), result.ToString());
        Assert.Equal(1234, dataRemoval.Id);
        Assert.Equal(DataRemovalStatus.Done, dataRemoval.Status);
        Assert.Equal("/data-removals/1234", postmark.LastAccountGetEndpoint);
    }

    [Fact]
    public async Task CreateDataRemovalAsync_WithInvalidRequestedFor_FailsBeforeApiCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateDataRemovalAsync(new DataRemovalCreateParameters
        {
            RequestedBy = "requester@example.com",
            RequestedFor = "not-an-email",
            NotifyWhenCompleted = false
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The data removal create parameters requested-for email address must be a valid email address. Set RequestedFor to an address like 'recipient@example.com'.", error.Message);
        Assert.Null(postmark.LastAccountPostEndpoint);
    }

    [Fact]
    public async Task GetDataRemovalAsync_WithUnknownStatus_ReturnsFailureWithRawValue()
    {
        var postmark = new RecordingPostmarkClient(accountGetResponses: new Dictionary<string, string>
        {
            ["/data-removals/1234"] = """{ "ID": 1234, "Status": "Purged" }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetDataRemovalAsync(1234, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Status value 'Purged' returned from the Postmark Data Removal API is not supported.", error.Message);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? accountPostResponses = null, Dictionary<string, string>? accountGetResponses = null) : IPostmarkClient
    {
        public string? LastAccountPostEndpoint { get; private set; }

        public object? LastAccountPostBody { get; private set; }

        public string? LastAccountGetEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            if (tokenScope != PostmarkTokenScope.Account)
                throw new InvalidOperationException("Server-level PostAsync should not be called in this test.");

            LastAccountPostEndpoint = endpoint;
            LastAccountPostBody = body;
            return GetResponse<TResponse>(accountPostResponses, endpoint);
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            if (tokenScope != PostmarkTokenScope.Account)
                throw new InvalidOperationException("Server-level GetAsync should not be called in this test.");

            LastAccountGetEndpoint = endpoint;
            return GetResponse<TResponse>(accountGetResponses, endpoint);
        }

        private static Task<Result<TResponse>> GetResponse<TResponse>(Dictionary<string, string>? responses, string endpoint)
        {
            if (responses is null || !responses.TryGetValue(endpoint, out var responseJson))
                throw new InvalidOperationException($"No response was configured for endpoint '{endpoint}'.");

            var response = JsonSerializer.Deserialize<TResponse>(responseJson, PostmarkConfiguration.JsonSerializerOptions);
            if (response is null)
                throw new InvalidOperationException($"Configured response for endpoint '{endpoint}' deserialized to null.");

            return Task.FromResult(Result.Success(response));
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
