using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Postmark;
using PostKit.Suppressions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PostKit.Tests;

public class PostKitClientSuppressionResponseTests
{
    [Fact]
    public async Task GetSuppressionsAsync_UsesSuppressionDumpEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "manual@example.com",
                                          "SuppressionReason": "ManualSuppression",
                                          "Origin": "Recipient",
                                          "CreatedAt": "2026-03-12T09:30:00-05:00"
                                        },
                                        {
                                          "EmailAddress": "bounce@example.com",
                                          "SuppressionReason": "HardBounce",
                                          "Origin": "Customer",
                                          "CreatedAt": "2026-03-12T10:00:00-05:00"
                                        },
                                        {
                                          "EmailAddress": "spam@example.com",
                                          "SuppressionReason": "SpamComplaint",
                                          "Origin": "Admin",
                                          "CreatedAt": "2026-03-12T11:00:00-05:00"
                                        }
                                      ]
                                    }
                                    """;

        const string endpoint = "/message-streams/broadcast/suppressions/dump?SuppressionReason=ManualSuppression&Origin=Recipient&todate=2026-03-13&fromdate=2026-03-12&EmailAddress=user%2Bsuppressed%40example.com";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new SuppressionQuery
        {
            Reason = SuppressionReason.ManualSuppression,
            Origin = SuppressionOrigin.Recipient,
            ToDate = new DateOnly(2026, 3, 13),
            FromDate = new DateOnly(2026, 3, 12),
            EmailAddress = "user+suppressed@example.com"
        };

        var result = await client.GetSuppressionsAsync("broadcast", query, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Collection(response.Suppressions, first =>
        {
            Assert.Equal("manual@example.com", first.EmailAddress);
            Assert.Equal(SuppressionReason.ManualSuppression, first.Reason);
            Assert.Equal(SuppressionOrigin.Recipient, first.Origin);
            Assert.Equal(DateTimeOffset.Parse("2026-03-12T09:30:00-05:00"), first.CreatedAt);
        }, second =>
        {
            Assert.Equal("bounce@example.com", second.EmailAddress);
            Assert.Equal(SuppressionReason.HardBounce, second.Reason);
            Assert.Equal(SuppressionOrigin.Customer, second.Origin);
        }, third =>
        {
            Assert.Equal("spam@example.com", third.EmailAddress);
            Assert.Equal(SuppressionReason.SpamComplaint, third.Reason);
            Assert.Equal(SuppressionOrigin.Admin, third.Origin);
        });
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithDateFilters_UsesDateOnlyQueryValues()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": []
                                    }
                                    """;

        const string endpoint = "/message-streams/broadcast/suppressions/dump?todate=2026-01-16&fromdate=2026-01-15";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new SuppressionQuery
        {
            FromDate = new DateOnly(2026, 1, 15),
            ToDate = new DateOnly(2026, 1, 16)
        };

        var result = await client.GetSuppressionsAsync("broadcast", query, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Suppressions);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithNullQuery_UsesUnfilteredSuppressionDumpEndpoint()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": []
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/message-streams/broadcast/suppressions/dump"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync("broadcast", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/broadcast/suppressions/dump", postmark.LastEndpoint);
        Assert.Empty(response.Suppressions);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithMessageStreamEnum_UsesMappedMessageStream()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": []
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/message-streams/outbound/suppressions/dump"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync(MessageStream.Transactional, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/outbound/suppressions/dump", postmark.LastEndpoint);
        Assert.Empty(response.Suppressions);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithFromDateAfterToDate_FailsWithActualValues()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync("broadcast", new SuppressionQuery { FromDate = new DateOnly(2026, 3, 13), ToDate = new DateOnly(2026, 3, 12) }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The suppression query from-date must not be later than the to-date. FromDate: 2026-03-13; ToDate: 2026-03-12.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task CreateSuppressionsAsync_UsesSuppressionEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "good@example.com",
                                          "Status": "Suppressed",
                                          "Message": null
                                        },
                                        {
                                          "EmailAddress": "bad@example.com",
                                          "Status": "Failed",
                                          "Message": "An invalid email address was provided."
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams/broadcast/suppressions"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateSuppressionsAsync("broadcast", ["good@example.com", "bad@example.com"], CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/broadcast/suppressions", postmark.LastEndpoint);
        Assert.Contains("\"EmailAddress\":\"good@example.com\"", postmark.LastRequestJson, StringComparison.Ordinal);
        Assert.Collection(response.Suppressions, first =>
        {
            Assert.Equal("good@example.com", first.EmailAddress);
            Assert.Equal(SuppressionStatus.Suppressed, first.Status);
            Assert.Null(first.Message);
        }, second =>
        {
            Assert.Equal("bad@example.com", second.EmailAddress);
            Assert.Equal(SuppressionStatus.Failed, second.Status);
            Assert.Equal("An invalid email address was provided.", second.Message);
        });
    }

    [Fact]
    public async Task CreateSuppressionsAsync_WithMessageStreamEnumAndSingleAddress_UsesMappedMessageStreamAndSingleAddressBody()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "single@example.com",
                                          "Status": "Suppressed",
                                          "Message": null
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams/outbound/suppressions"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateSuppressionsAsync(MessageStream.Transactional, "single@example.com", CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/outbound/suppressions", postmark.LastEndpoint);
        Assert.Contains("\"EmailAddress\":\"single@example.com\"", postmark.LastRequestJson, StringComparison.Ordinal);
        Assert.DoesNotContain("good@example.com", postmark.LastRequestJson, StringComparison.Ordinal);
        Assert.Equal(SuppressionStatus.Suppressed, Assert.Single(response.Suppressions)
            .Status);
    }

    [Fact]
    public async Task DeleteSuppressionsAsync_UsesSuppressionDeleteEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "reactivated@example.com",
                                          "Status": "Deleted",
                                          "Message": null
                                        },
                                        {
                                          "EmailAddress": "spam@example.com",
                                          "Status": "Failed",
                                          "Message": "You do not have the required authority to change this suppression."
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams/broadcast/suppressions/delete"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.DeleteSuppressionsAsync("broadcast", ["reactivated@example.com", "spam@example.com"], CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/broadcast/suppressions/delete", postmark.LastEndpoint);
        Assert.Contains("\"EmailAddress\":\"reactivated@example.com\"", postmark.LastRequestJson, StringComparison.Ordinal);
        Assert.Collection(response.Suppressions, first => Assert.Equal(SuppressionStatus.Deleted, first.Status), second => Assert.Equal(SuppressionStatus.Failed, second.Status));
    }

    [Fact]
    public async Task DeleteSuppressionsAsync_WithSingleAddress_UsesSingleAddressBody()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "single@example.com",
                                          "Status": "Deleted",
                                          "Message": null
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams/broadcast/suppressions/delete"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.DeleteSuppressionsAsync("broadcast", "single@example.com", CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/broadcast/suppressions/delete", postmark.LastEndpoint);
        Assert.Contains("\"EmailAddress\":\"single@example.com\"", postmark.LastRequestJson, StringComparison.Ordinal);
        Assert.Equal(SuppressionStatus.Deleted, Assert.Single(response.Suppressions)
            .Status);
    }

    [Fact]
    public async Task DeleteSuppressionsAsync_WithMessageStreamEnum_UsesMappedMessageStream()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "single@example.com",
                                          "Status": "Deleted",
                                          "Message": null
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams/outbound/suppressions/delete"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.DeleteSuppressionsAsync(MessageStream.Transactional, ["single@example.com"], CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/message-streams/outbound/suppressions/delete", postmark.LastEndpoint);
        Assert.Equal(SuppressionStatus.Deleted, Assert.Single(response.Suppressions)
            .Status);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithUnknownReason_ReturnsFailureWithRawValue()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "future@example.com",
                                          "SuppressionReason": "FutureSuppression",
                                          "Origin": "Recipient",
                                          "CreatedAt": "2026-03-12T09:30:00-05:00"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/message-streams/broadcast/suppressions/dump"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync("broadcast", TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Suppression item 0 could not be mapped: SuppressionReason value 'FutureSuppression' returned from the Postmark Suppressions API is not supported.", error.Message);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithUnknownOrigin_ReturnsFailureWithRawValue()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "future@example.com",
                                          "SuppressionReason": "ManualSuppression",
                                          "Origin": "FutureOrigin",
                                          "CreatedAt": "2026-03-12T09:30:00-05:00"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/message-streams/broadcast/suppressions/dump"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync("broadcast", TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Suppression item 0 could not be mapped: Origin value 'FutureOrigin' returned from the Postmark Suppressions API is not supported.", error.Message);
    }

    [Fact]
    public async Task CreateSuppressionsAsync_WithUnknownStatus_ReturnsFailureWithRawValue()
    {
        const string responseJson = """
                                    {
                                      "Suppressions": [
                                        {
                                          "EmailAddress": "future@example.com",
                                          "Status": "Queued",
                                          "Message": null
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams/broadcast/suppressions"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateSuppressionsAsync("broadcast", ["future@example.com"], CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Suppression result item 0 could not be mapped: Status value 'Queued' returned from the Postmark Suppressions API is not supported.", error.Message);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithWhitespaceEmailAddressFilter_FailsWithOmitGuidance()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync("broadcast", new SuppressionQuery { EmailAddress = "\t " }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The suppression query email address filter cannot be empty or whitespace. Set EmailAddress to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithInvalidMessageStream_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync("_broadcast", TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(
            "The suppression query message stream must be 1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'. First character must be a lowercase letter. Received '_' at index 0.",
            error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetSuppressionsAsync_WithInvalidMessageStreamEnum_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetSuppressionsAsync((MessageStream)999, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The suppression query message stream must be MessageStream.Transactional or MessageStream.Broadcast. Received 999.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task DeleteSuppressionsAsync_WithTooManyAddresses_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());
        var addresses = Enumerable.Range(0, 51)
            .Select(index => $"user{index}@example.com");

        var result = await client.DeleteSuppressionsAsync("broadcast", addresses, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The suppression delete request email address collection must contain between 1 and 50 addresses. Actual count: 51.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task CreateSuppressionsAsync_WithWhitespaceAddress_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateSuppressionsAsync("broadcast", ["valid@example.com", "  "], CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The suppression create request email address at index 1 cannot be empty or whitespace. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? getResponses = null, Dictionary<string, string>? postResponses = null) : IPostmarkClient
    {
        private readonly Dictionary<string, string> _getResponses = getResponses ?? [];
        private readonly Dictionary<string, string> _postResponses = postResponses ?? [];
        public string? LastEndpoint { get; private set; }
        public string? LastRequestJson { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            LastRequestJson = JsonSerializer.Serialize(body);
            return Task.FromResult(Result.Success(Deserialize<TResponse>(_postResponses, endpoint)));
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            return Task.FromResult(Result.Success(Deserialize<TResponse>(_getResponses, endpoint)));
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