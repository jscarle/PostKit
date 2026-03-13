using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Bounces;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientBounceResponseTests
{
    [Fact]
    public async Task GetBouncesAsync_UsesBounceSearchEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "HardBounce",
                                          "TypeCode": 1,
                                          "Name": "Hard bounce",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "The server was unable to deliver your message.",
                                          "Details": "smtp;550 mailbox unavailable",
                                          "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                          "From": "from@publi-7.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": true,
                                          "CanActivate": true,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=25&offset=10&type=HardBounce&inactive=true&emailFilter=HardBounce%40bounce-testing.postmarkapp.com&messageID=69ce4784-c202-41c6-a1a9-91757022b25e&tag=ops%2Balerts&todate=2026-03-11T13%3A59%3A59&fromdate=2026-03-11T13%3A00%3A00&messagestream=outbound"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);
        var query = new BounceQuery(25, 10)
        {
            Type = BounceType.HardBounce,
            Inactive = true,
            EmailFilter = "HardBounce@bounce-testing.postmarkapp.com",
            MessageId = Guid.Parse("69ce4784-c202-41c6-a1a9-91757022b25e"),
            Tag = "ops+alerts",
            ToDate = new DateTime(2026, 3, 11, 13, 59, 59),
            FromDate = new DateTime(2026, 3, 11, 13, 0, 0),
            MessageStream = "outbound",
        };

        var result = await client.GetBouncesAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/bounces?count=25&offset=10&type=HardBounce&inactive=true&emailFilter=HardBounce%40bounce-testing.postmarkapp.com&messageID=69ce4784-c202-41c6-a1a9-91757022b25e&tag=ops%2Balerts&todate=2026-03-11T13%3A59%3A59&fromdate=2026-03-11T13%3A00%3A00&messagestream=outbound", postmark.LastEndpoint);

        Assert.Equal(1, response.TotalCount);
        var bounce = Assert.Single(response.Bounces);
        Assert.Equal("Bounce", bounce.RecordType);
        Assert.Equal(1599950051, bounce.Id);
        Assert.Equal(BounceType.HardBounce, bounce.Type);
        Assert.Equal(1, bounce.TypeCode);
        Assert.Equal(Guid.Parse("69ce4784-c202-41c6-a1a9-91757022b25e"), bounce.MessageId);
        Assert.Equal(18451835, bounce.ServerId);
        Assert.Equal("outbound", bounce.MessageStream);
        Assert.Equal("from@publi-7.com", bounce.From);
        Assert.True(bounce.DumpAvailable);
        Assert.True(bounce.Inactive);
        Assert.True(bounce.CanActivate);
    }

    [Fact]
    public async Task GetBounceAsync_UsesBounceEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "ID": 1599950051,
                                      "Type": "HardBounce",
                                      "TypeCode": 1,
                                      "Name": "Hard bounce",
                                      "Tag": "",
                                      "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                      "ServerID": 18451835,
                                      "MessageStream": "outbound",
                                      "Description": "The server was unable to deliver your message.",
                                      "Details": "smtp;550 mailbox unavailable",
                                      "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                      "From": "from@publi-7.com",
                                      "BouncedAt": "2026-03-11T17:33:38Z",
                                      "DumpAvailable": true,
                                      "Inactive": true,
                                      "CanActivate": true,
                                      "Subject": "PostKit Bounces API probe",
                                      "Content": "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/bounces/1599950051", postmark.LastEndpoint);
        Assert.Equal("Bounce", response.RecordType);
        Assert.Equal(BounceType.HardBounce, response.Type);
        Assert.Equal(Guid.Parse("69ce4784-c202-41c6-a1a9-91757022b25e"), response.MessageId);
        Assert.Contains("69ce4784-c202-41c6-a1a9-91757022b25e", response.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBouncesAsync_WhenFromIsMissing_MapsNullFrom()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "HardBounce",
                                          "TypeCode": 1,
                                          "Name": "Hard bounce",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "The server was unable to deliver your message.",
                                          "Details": "smtp;550 mailbox unavailable",
                                          "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": true,
                                          "CanActivate": true,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=10&offset=0"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery(10, 0), CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        var bounce = Assert.Single(response.Bounces);
        Assert.Null(bounce.From);
    }

    [Fact]
    public async Task GetBouncesAsync_WithChallengeVerification_MapsSupportedBounceType()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "ChallengeVerification",
                                          "TypeCode": 16384,
                                          "Name": "Challenge verification",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "Challenge verification response.",
                                          "Details": "smtp; challenge verification",
                                          "Email": "ChallengeVerification@bounce-testing.postmarkapp.com",
                                          "From": "from@publi-7.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": false,
                                          "CanActivate": false,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=10&offset=0"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery(10, 0), CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(BounceType.ChallengeVerification, Assert.Single(response.Bounces).Type);
    }

    [Fact]
    public async Task GetBouncesAsync_WithChallengeVerificationFilter_UsesSupportedTypeValue()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Bounces": []
                                    }
                                    """;

        var endpoint = "/bounces?count=10&offset=0&type=ChallengeVerification";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery(10, 0) { Type = BounceType.ChallengeVerification }, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Bounces);
    }

    [Fact]
    public async Task GetBouncesAsync_WithLegacyMailFrontierMatadorFilter_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

#pragma warning disable CS0618
        var result = await client.GetBouncesAsync(new BounceQuery(10, 0) { Type = BounceType.MailFrontierMatador }, CancellationToken.None);
#pragma warning restore CS0618

        Assert.True(result.IsFailure());
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBounceAsync_WithWhitespaceOnlySubject_PreservesSubject()
    {
        const string responseJson = """
                                    {
                                      "RecordType": "Bounce",
                                      "ID": 1599950051,
                                      "Type": "HardBounce",
                                      "TypeCode": 1,
                                      "Name": "Hard bounce",
                                      "Tag": "",
                                      "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                      "ServerID": 18451835,
                                      "MessageStream": "outbound",
                                      "Description": "The server was unable to deliver your message.",
                                      "Details": "smtp;550 mailbox unavailable",
                                      "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                      "From": "from@publi-7.com",
                                      "BouncedAt": "2026-03-11T17:33:38Z",
                                      "DumpAvailable": true,
                                      "Inactive": true,
                                      "CanActivate": true,
                                      "Subject": "   ",
                                      "Content": "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("   ", response.Subject);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_UsesDeliveryStatsEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": 1,
                                      "Bounces": [
                                        {
                                          "Name": "All",
                                          "Count": 4
                                        },
                                        {
                                          "Type": "SoftBounce",
                                          "Name": "Soft bounce",
                                          "Count": 3
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/deliverystats", postmark.LastEndpoint);
        Assert.Equal(1, response.InactiveMails);
        Assert.Equal(2, response.Bounces.Count);
        Assert.Null(response.Bounces[0].Type);
        Assert.Equal("All", response.Bounces[0].Name);
        Assert.Equal(4, response.Bounces[0].Count);
        Assert.Equal(BounceType.SoftBounce, response.Bounces[1].Type);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_WithChallengeVerification_MapsSupportedBounceType()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": 0,
                                      "Bounces": [
                                        {
                                          "Type": "ChallengeVerification",
                                          "Name": "Challenge verification",
                                          "Count": 1
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(BounceType.ChallengeVerification, Assert.Single(response.Bounces).Type);
    }

    [Fact]
    public async Task GetBounceDumpAsync_UsesBounceDumpEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Body": "X-PM-Message-Id: 5f760f92-8c8a-43a9-a991-afaebb372eff"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1600045793/dump"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceDumpAsync(1600045793, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/bounces/1600045793/dump", postmark.LastEndpoint);
        Assert.Contains("5f760f92-8c8a-43a9-a991-afaebb372eff", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ActivateBounceAsync_UsesActivateEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Message": "OK",
                                      "Bounce": {
                                        "ID": 1599950051,
                                        "Type": "HardBounce",
                                        "TypeCode": 1,
                                        "Name": "Hard bounce",
                                        "Tag": "",
                                        "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                        "ServerID": 18451835,
                                        "MessageStream": "outbound",
                                        "Description": "The server was unable to deliver your message.",
                                        "Details": "smtp;550 mailbox unavailable",
                                        "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                        "From": "from@publi-7.com",
                                        "BouncedAt": "2026-03-11T17:33:38Z",
                                        "DumpAvailable": true,
                                        "Inactive": true,
                                        "CanActivate": true,
                                        "Subject": "PostKit Bounces API probe"
                                      }
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(putResponses: new Dictionary<string, string> { ["/bounces/1599950051/activate"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.ActivateBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/bounces/1599950051/activate", postmark.LastEndpoint);
        Assert.Equal("OK", response.Message);
        Assert.Equal("Bounce", response.Bounce.RecordType);
        Assert.Equal(1599950051, response.Bounce.Id);
        Assert.Equal(BounceType.HardBounce, response.Bounce.Type);
    }

    [Fact]
    public async Task GetBouncesAsync_WithInvalidCount_Fails()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery(0, 0), CancellationToken.None);

        Assert.True(result.IsFailure());
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WhenCountAndOffsetExceedSearchWindow_Fails()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery(500, 9800), CancellationToken.None);

        Assert.True(result.IsFailure());
        Assert.Null(postmark.LastEndpoint);
    }

    private sealed class RecordingPostmarkClient(
        Dictionary<string, string>? getResponses = null,
        Dictionary<string, string>? putResponses = null
    ) : IPostmarkClient
    {
        private readonly Dictionary<string, string> _getResponses = getResponses ?? [];
        private readonly Dictionary<string, string> _putResponses = putResponses ?? [];

        public string? LastEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            return Task.FromResult(Result.Success(Deserialize<TResponse>(_getResponses, endpoint)));
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            return Task.FromResult(Result.Success(Deserialize<TResponse>(_putResponses, endpoint)));
        }

        private static TResponse Deserialize<TResponse>(IReadOnlyDictionary<string, string> responses, string endpoint)
        {
            if (!responses.TryGetValue(endpoint, out var responseJson))
                throw new InvalidOperationException($"No response JSON was configured for endpoint '{endpoint}'.");

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
