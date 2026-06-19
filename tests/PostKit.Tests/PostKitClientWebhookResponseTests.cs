using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostKit.Webhooks;
using PostmarkWebhookRequest = PostKit.Postmark.Webhooks.WebhookRequest;

namespace PostKit.Tests;

public class PostKitClientWebhookResponseTests
{
    private const string WebhookJson = """
                                       {
                                         "ID": 1234567,
                                         "Url": "https://example.com/webhook",
                                         "MessageStream": "outbound",
                                         "HttpAuth": {
                                           "Username": "user",
                                           "Password": "pass"
                                         },
                                         "HttpHeaders": [
                                           {
                                             "Name": "X-Webhook",
                                             "Value": "value"
                                           }
                                         ],
                                         "Triggers": {
                                           "Open": {
                                             "Enabled": true,
                                             "PostFirstOpenOnly": false
                                           },
                                           "Click": {
                                             "Enabled": true
                                           },
                                           "Delivery": {
                                             "Enabled": true
                                           },
                                           "Bounce": {
                                             "Enabled": true,
                                             "IncludeContent": true
                                           },
                                           "SpamComplaint": {
                                             "Enabled": false,
                                             "IncludeContent": false
                                           },
                                           "SubscriptionChange": {
                                             "Enabled": false
                                           }
                                         }
                                       }
                                       """;

    [Fact]
    public async Task ListWebhooksAsync_WithMessageStream_MapsWebhooks()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/webhooks?MessageStream=outbound"] = $$"""{"Webhooks":[{{WebhookJson}}]}"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListWebhooksAsync(MessageStream.Transactional, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var list), result.ToString());
        var webhook = Assert.Single(list.Webhooks);
        Assert.Equal(1234567, webhook.Id);
        Assert.Equal("https://example.com/webhook", webhook.Url);
        Assert.Equal("outbound", webhook.MessageStream);
        Assert.Equal("user", webhook.HttpAuth!.Username);
        var header = Assert.Single(webhook.HttpHeaders);
        Assert.Equal("X-Webhook", header.Name);
        Assert.True(webhook.Triggers.Open!.Enabled);
        Assert.False(webhook.Triggers.Open.PostFirstOpenOnly);
        Assert.True(webhook.Triggers.Bounce!.IncludeContent);
    }

    [Fact]
    public async Task GetWebhookAsync_MapsWebhook()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/webhooks/1234567"] = WebhookJson
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetWebhookAsync(1234567, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var webhook), result.ToString());
        Assert.Equal(1234567, webhook.Id);
        Assert.Equal("outbound", webhook.MessageStream);
        Assert.Equal("/webhooks/1234567", postmark.LastGetEndpoint);
    }

    [Fact]
    public async Task CreateWebhookAsync_SendsCreateRequest()
    {
        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/webhooks"] = WebhookJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateWebhookAsync(new WebhookCreateParameters
        {
            Url = "https://example.com/webhook",
            MessageStreamId = "broadcast",
            HttpAuth = new WebhookHttpAuth { Username = "user", Password = "pass" },
            HttpHeaders = [new WebhookHeader { Name = "X-Webhook", Value = "value" }],
            Triggers = new WebhookTriggers
            {
                Open = new WebhookOpenTrigger { Enabled = true, PostFirstOpenOnly = false },
                Click = new WebhookBasicTrigger { Enabled = true },
                Bounce = new WebhookContentTrigger { Enabled = true, IncludeContent = true }
            }
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Equal("/webhooks", postmark.LastPostEndpoint);
        var body = Assert.IsType<PostmarkWebhookRequest>(postmark.LastPostBody);
        Assert.Equal("https://example.com/webhook", body.Url);
        Assert.Equal("broadcast", body.MessageStream);
        Assert.Equal("user", body.HttpAuth!.Username);
        Assert.Equal("X-Webhook", Assert.Single(body.HttpHeaders!)
            .Name);
        Assert.True(body.Triggers!.Open!.Enabled);
        Assert.True(body.Triggers.Bounce!.IncludeContent);
    }

    [Fact]
    public async Task EditWebhookAsync_SendsPutRequest()
    {
        var postmark = new RecordingPostmarkClient(putResponses: new Dictionary<string, string> { ["/webhooks/1234567"] = WebhookJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.EditWebhookAsync(1234567, new WebhookEditParameters
        {
            Url = "https://example.com/updated",
            Triggers = new WebhookTriggers { Delivery = new WebhookBasicTrigger { Enabled = true } }
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Equal("/webhooks/1234567", postmark.LastPutEndpoint);
        var body = Assert.IsType<PostmarkWebhookRequest>(postmark.LastPutBody);
        Assert.Equal("https://example.com/updated", body.Url);
        Assert.True(body.Triggers!.Delivery!.Enabled);
        Assert.Null(body.MessageStream);
    }

    [Fact]
    public async Task DeleteWebhookAsync_MapsDeletion()
    {
        var postmark = new RecordingPostmarkClient(deleteResponses: new Dictionary<string, string> { ["/webhooks/1234567"] = """{"ErrorCode":0,"Message":"Webhook removed."}""" });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.DeleteWebhookAsync(1234567, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var deletion), result.ToString());
        Assert.Equal("Webhook removed.", deletion.Message);
        Assert.Equal("/webhooks/1234567", postmark.LastDeleteEndpoint);
    }

    [Fact]
    public async Task CreateWebhookAsync_WithRelativeUrl_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateWebhookAsync(new WebhookCreateParameters
        {
            Url = "/relative",
            Triggers = new WebhookTriggers { Delivery = new WebhookBasicTrigger { Enabled = true } }
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The webhook create parameters URL must be an absolute HTTP or HTTPS URL.", error.Message);
        Assert.Null(postmark.LastPostEndpoint);
    }

    private sealed class RecordingPostmarkClient(
        Dictionary<string, string>? getResponses = null,
        Dictionary<string, string>? postResponses = null,
        Dictionary<string, string>? putResponses = null,
        Dictionary<string, string>? deleteResponses = null) : IPostmarkClient
    {
        public string? LastGetEndpoint { get; private set; }

        public string? LastPostEndpoint { get; private set; }

        public object? LastPostBody { get; private set; }

        public string? LastPutEndpoint { get; private set; }

        public object? LastPutBody { get; private set; }

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

        public Task<Result<TResponse>> PutAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastPutEndpoint = endpoint;
            LastPutBody = body;
            return GetResponse<TResponse>(putResponses, endpoint);
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