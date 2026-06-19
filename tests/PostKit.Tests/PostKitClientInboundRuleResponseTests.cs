using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.InboundRules;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostmarkInboundRuleRequest = PostKit.Postmark.InboundRules.InboundRuleTriggerCreateRequest;

namespace PostKit.Tests;

public class PostKitClientInboundRuleResponseTests
{
    [Fact]
    public async Task ListInboundRuleTriggersAsync_UsesListEndpointAndMapsResponse()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/triggers/inboundrules?count=50&offset=10"] = """
                                                            {
                                                              "TotalCount": 2,
                                                              "InboundRules": [
                                                                { "ID": 3, "Rule": "someone@example.com" },
                                                                { "ID": 7, "Rule": "baddomain.com" }
                                                              ]
                                                            }
                                                            """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListInboundRuleTriggersAsync(50, 10, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal("/triggers/inboundrules?count=50&offset=10", postmark.LastGetEndpoint);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(3, page.InboundRules[0].Id);
        Assert.Equal("someone@example.com", page.InboundRules[0].Rule);
        Assert.Equal(7, page.InboundRules[1].Id);
        Assert.Equal("baddomain.com", page.InboundRules[1].Rule);
    }

    [Fact]
    public async Task CreateInboundRuleTriggerAsync_SendsCreateRequest()
    {
        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string>
        {
            ["/triggers/inboundrules"] = """{ "ID": 15, "Rule": "someone@example.com" }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateInboundRuleTriggerAsync(new InboundRuleTriggerCreateParameters { Rule = "someone@example.com" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var rule), result.ToString());
        Assert.Equal(15, rule.Id);
        Assert.Equal("someone@example.com", rule.Rule);
        Assert.Equal("/triggers/inboundrules", postmark.LastPostEndpoint);
        var body = Assert.IsType<PostmarkInboundRuleRequest>(postmark.LastPostBody);
        Assert.Equal("someone@example.com", body.Rule);
    }

    [Fact]
    public async Task DeleteInboundRuleTriggerAsync_MapsDeletion()
    {
        var postmark = new RecordingPostmarkClient(deleteResponses: new Dictionary<string, string>
        {
            ["/triggers/inboundrules/15"] = """{ "ErrorCode": 0, "Message": "Rule someone@example.com removed." }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.DeleteInboundRuleTriggerAsync(15, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var deletion), result.ToString());
        Assert.Equal("/triggers/inboundrules/15", postmark.LastDeleteEndpoint);
        Assert.Equal("Rule someone@example.com removed.", deletion.Message);
    }

    [Fact]
    public async Task CreateInboundRuleTriggerAsync_WithWhitespaceRule_FailsBeforeApiCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateInboundRuleTriggerAsync(new InboundRuleTriggerCreateParameters { Rule = "\t " }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The inbound rule trigger create parameters rule cannot be empty or whitespace. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastPostEndpoint);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? getResponses = null, Dictionary<string, string>? postResponses = null, Dictionary<string, string>? deleteResponses = null) : IPostmarkClient
    {
        public string? LastGetEndpoint { get; private set; }

        public string? LastPostEndpoint { get; private set; }

        public object? LastPostBody { get; private set; }

        public string? LastDeleteEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastPostEndpoint = endpoint;
            LastPostBody = body;
            return GetResponse<TResponse>(postResponses, endpoint);
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastGetEndpoint = endpoint;
            return GetResponse<TResponse>(getResponses, endpoint);
        }

        public Task<Result<TResponse>> DeleteAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastDeleteEndpoint = endpoint;
            return GetResponse<TResponse>(deleteResponses, endpoint);
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