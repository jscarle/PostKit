using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class PostmarkClientErrorHandlingTests
{
    [Fact]
    public async Task PostAsync_WithUnknownPostmarkErrorCode_ReturnsPostmarkError()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent("""{"ErrorCode":999999,"Message":"Brand new Postmark error."}""", Encoding.UTF8, MediaTypeNames.Application.Json)
        }));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>(PostmarkTokenScope.Server, "/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal((PostmarkErrorCode)999999, postmarkError.ErrorCode);
        Assert.Equal("Brand new Postmark error.", postmarkError.Message);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, postmarkError.StatusCode);
    }

    [Fact]
    public async Task PostAsync_WithStructuredForbiddenError_ReturnsPostmarkError()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"ErrorCode":10,"Message":"The Postmark Test API Token may only be used on the /email endpoint."}""", Encoding.UTF8, MediaTypeNames.Application.Json)
        }));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>(PostmarkTokenScope.Server, "/email/bulk", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal((PostmarkErrorCode)10, postmarkError.ErrorCode);
        Assert.Equal("The Postmark Test API Token may only be used on the /email endpoint.", postmarkError.Message);
        Assert.Equal(HttpStatusCode.Forbidden, postmarkError.StatusCode);
    }

    [Fact]
    public async Task PostAsync_WithMultipleErrors_PreservesErrorDetails()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent("""{"ErrorCode":11,"Message":"Multiple errors occurred.","Errors":{"From":["From is required."],"To":["To is required."]}}""", Encoding.UTF8, MediaTypeNames.Application.Json)
        }));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>(PostmarkTokenScope.Server, "/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal(PostmarkErrorCode.MultipleErrorsOccurred, postmarkError.ErrorCode);

        var errors = Assert.IsType<JsonObject>(postmarkError.Errors);
        Assert.Equal("From is required.", errors["From"]?[0]
            ?.GetValue<string>());
        Assert.Equal("To is required.", errors["To"]?[0]
            ?.GetValue<string>());
    }

    [Fact]
    public async Task PostAsync_WithMalformedSuccessfulJson_ReturnsFailure()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"MessageID":""", Encoding.UTF8, MediaTypeNames.Application.Json)
        }));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>(PostmarkTokenScope.Server, "/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The response from the '/email' endpoint of the Postmark API could not be deserialized because the response JSON was invalid.", error.Message);
    }

    [Fact]
    public async Task PostAsync_WithMalformedStrictErrorJson_ReturnsHelpfulFailure()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent("""{"ErrorCode":""", Encoding.UTF8, MediaTypeNames.Application.Json)
        }));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>(PostmarkTokenScope.Server, "/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The error response from the '/email' endpoint of the Postmark API could not be deserialized because the response JSON was invalid.", error.Message);
    }

    [Fact]
    public async Task PutAsync_SendsEmptyJsonPayloadWithApplicationJsonContentType()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PutAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/bounces/1/activate", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.True(handler.HadContent);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal("{}", handler.LastContentBody);
    }

    [Fact]
    public async Task PutAsync_WithBody_SendsJsonPayloadWithApplicationJsonContentType()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token" }), new TestLogger<PostmarkClient>());

        var result = await client.PutAsync<object, PostmarkResponse>(PostmarkTokenScope.Server, "/templates/1", new { Name = "Updated" }, CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal("""{"name":"Updated"}""", handler.LastContentBody);
        Assert.Equal("server-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Server-Token")));
    }

    [Fact]
    public async Task PostAsync_WithoutBody_SendsEmptyPayloadWithServerToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/message-streams/broadcast/archive", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.True(handler.HadContent);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal(string.Empty, handler.LastContentBody);
        Assert.Equal("server-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Server-Token")));
    }

    [Fact]
    public async Task PostAsync_WithAccountScopeWithoutBody_SendsEmptyPayloadWithAccountToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token", AccountApiToken = "account-token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<PostmarkResponse>(PostmarkTokenScope.Account, "/domains/12/rotatedkim", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.True(handler.HadContent);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal(string.Empty, handler.LastContentBody);
        Assert.Equal("account-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Account-Token")));
        Assert.False(handler.LastRequest.Headers.Contains("X-Postmark-Server-Token"));
    }

    [Fact]
    public async Task PutAsync_WithAccountScopeWithoutBody_SendsEmptyPayloadWithAccountToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token", AccountApiToken = "account-token" }), new TestLogger<PostmarkClient>());

        var result = await client.PutAsync<PostmarkResponse>(PostmarkTokenScope.Account, "/domains/12/verifyDkim", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.True(handler.HadContent);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal(string.Empty, handler.LastContentBody);
        Assert.Equal("account-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Account-Token")));
        Assert.False(handler.LastRequest.Headers.Contains("X-Postmark-Server-Token"));
    }

    [Fact]
    public async Task PatchAsync_WithBody_SendsJsonPayloadWithServerToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token" }), new TestLogger<PostmarkClient>());

        var result = await client.PatchAsync<object, PostmarkResponse>(PostmarkTokenScope.Server, "/message-streams/broadcast", new { Name = "Broadcast" }, CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal("""{"name":"Broadcast"}""", handler.LastContentBody);
        Assert.Equal("server-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Server-Token")));
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteWithServerToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token" }), new TestLogger<PostmarkClient>());

        var result = await client.DeleteAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/webhooks/12", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.False(handler.HadContent);
        Assert.Equal("server-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Server-Token")));
    }

    [Fact]
    public async Task DeleteAsync_WithAccountScope_SendsDeleteWithAccountToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token", AccountApiToken = "account-token" }), new TestLogger<PostmarkClient>());

        var result = await client.DeleteAsync<PostmarkResponse>(PostmarkTokenScope.Account, "/domains/12", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.False(handler.HadContent);
        Assert.Equal("account-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Account-Token")));
        Assert.False(handler.LastRequest.Headers.Contains("X-Postmark-Server-Token"));
    }

    [Fact]
    public async Task GetAsync_WithAccountScope_SendsAccountTokenWithoutServerToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token", AccountApiToken = "account-token" }), new TestLogger<PostmarkClient>());

        var result = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Account, "/data-removals/1", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("account-token", Assert.Single(handler.LastRequest.Headers.GetValues("X-Postmark-Account-Token")));
        Assert.False(handler.LastRequest.Headers.Contains("X-Postmark-Server-Token"));
    }

    [Fact]
    public async Task GetAsync_WithAccountScopeWithoutAccountToken_ThrowsPostKitConfigurationError()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token" }), new TestLogger<PostmarkClient>());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Account, "/data-removals/1", CancellationToken.None));

        Assert.Equal("The account API token has not been set.", exception.Message);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task GetAsync_WithAccountScopeUnauthorizedResponse_NamesAccountToken()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(string.Empty) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "server-token", AccountApiToken = "account-token" }), new TestLogger<PostmarkClient>());

        var result = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Account, "/data-removals/1", CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var httpError = Assert.IsType<HttpError>(error);
        Assert.Equal(HttpStatusCode.Unauthorized, httpError.StatusCode);
        Assert.Equal("The account API token is invalid.", httpError.Message);
    }

    [Fact]
    public async Task GetAsync_WithRateLimitHeaders_DelaysSuccessiveCallsInLearnedBucket()
    {
        var delays = new List<TimeSpan>();
        using var handler = new SequenceHttpMessageHandler(CreateRateLimitedResponse(HttpStatusCode.OK, 10), CreateRateLimitedResponse(HttpStatusCode.OK, 10));
        using var httpClient = new HttpClient(handler);
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>(), rateLimiter);

        var firstResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/messages/outbound?count=1", CancellationToken.None);
        var secondResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details", CancellationToken.None);

        Assert.True(firstResult.IsSuccess(out _), firstResult.ToString());
        Assert.True(secondResult.IsSuccess(out _), secondResult.ToString());
        var delay = Assert.Single(delays);
        Assert.True(delay >= TimeSpan.FromMilliseconds(1), $"Expected at least a 1 ms delay, received {delay.TotalMilliseconds} ms.");
    }

    [Fact]
    public async Task GetAsync_WithKnownRateLimitedEndpoint_DelaysColdSuccessiveCallsBeforeHeaders()
    {
        var delays = new List<TimeSpan>();
        using var handler = new SequenceHttpMessageHandler(CreateOkResponse(), CreateOkResponse());
        using var httpClient = new HttpClient(handler);
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>(), rateLimiter);

        var firstResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/messages/outbound?count=1", CancellationToken.None);
        var secondResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/messages/outbound?count=1&offset=1", CancellationToken.None);

        Assert.True(firstResult.IsSuccess(out _), firstResult.ToString());
        Assert.True(secondResult.IsSuccess(out _), secondResult.ToString());
        var delay = Assert.Single(delays);
        Assert.True(delay >= TimeSpan.FromMilliseconds(1), $"Expected at least a 1 ms delay, received {delay.TotalMilliseconds} ms.");
    }

    [Fact]
    public async Task GetAsync_WithTooManyRequests_RetriesWithExponentialBackoff()
    {
        var delays = new List<TimeSpan>();
        using var handler = new SequenceHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests), new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>(), rateLimiter, NoJitter);

        var result = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/bounces?count=1&offset=0", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Equal(3, handler.RequestCount);
        Assert.Equal([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)], delays);
    }

    [Fact]
    public async Task GetAsync_WithTooManyRequests_AppliesConfiguredJitter()
    {
        var delays = new List<TimeSpan>();
        using var handler = new SequenceHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>(), rateLimiter, static () => 1.2);

        var result = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/bounces?count=1&offset=0", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        var delay = Assert.Single(delays);
        Assert.Equal(TimeSpan.FromMilliseconds(1200), delay);
    }

    [Fact]
    public async Task GetAsync_WithTooManyRequests_BlocksEndpointGroupOnly()
    {
        var delays = new List<TimeSpan>();
        using var handler = new SequenceHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests), CreateOkResponse(), CreateOkResponse(), CreateOkResponse());
        using var httpClient = new HttpClient(handler);
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>(), rateLimiter, NoJitter);

        var detailsResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details", CancellationToken.None);

        Assert.True(detailsResult.IsSuccess(out _), detailsResult.ToString());
        var retryDelay = Assert.Single(delays);
        Assert.Equal(TimeSpan.FromSeconds(1), retryDelay);

        var suppressionsResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/message-streams/outbound/suppressions/dump", CancellationToken.None);

        Assert.True(suppressionsResult.IsSuccess(out _), suppressionsResult.ToString());
        Assert.Single(delays);

        var searchResult = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/messages/outbound?count=1", CancellationToken.None);

        Assert.True(searchResult.IsSuccess(out _), searchResult.ToString());
        Assert.Equal(2, delays.Count);
        Assert.True(delays[1] >= TimeSpan.FromMilliseconds(1), $"Expected the outbound message group to remain delayed, received {delays[1].TotalMilliseconds} ms.");
    }

    [Fact]
    public async Task GetAsync_WithRepeatedTooManyRequests_ReturnsFailureAfterRetryAtFinalDelay()
    {
        var delays = new List<TimeSpan>();
        using var handler = new SequenceHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests), new HttpResponseMessage(HttpStatusCode.TooManyRequests), new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new HttpResponseMessage(HttpStatusCode.TooManyRequests), new HttpResponseMessage(HttpStatusCode.TooManyRequests), new HttpResponseMessage(HttpStatusCode.TooManyRequests), new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        using var httpClient = new HttpClient(handler);
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>(), rateLimiter, NoJitter);

        var result = await client.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/bounces?count=1&offset=0", CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var httpError = Assert.IsType<HttpError>(error);
        Assert.Equal(HttpStatusCode.TooManyRequests, httpError.StatusCode);
        Assert.Equal(7, handler.RequestCount);
        Assert.Equal([
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8),
            TimeSpan.FromSeconds(16),
            TimeSpan.FromSeconds(30)
        ], delays);
    }

    private static HttpResponseMessage CreateRateLimitedResponse(HttpStatusCode statusCode, int rateLimit)
    {
        var response = CreateResponse(statusCode);
        response.Headers.Add("RateLimit-Limit", rateLimit.ToString(CultureInfo.InvariantCulture));
        response.Headers.Add("RateLimit-Remaining", (rateLimit - 1).ToString(CultureInfo.InvariantCulture));
        response.Headers.Add("RateLimit-Reset", "1");
        response.Headers.Add("X-RateLimit-Limit-Second", rateLimit.ToString(CultureInfo.InvariantCulture));
        response.Headers.Add("X-RateLimit-Remaining-Second", (rateLimit - 1).ToString(CultureInfo.InvariantCulture));
        return response;
    }

    private static HttpResponseMessage CreateOkResponse()
    {
        return CreateResponse(HttpStatusCode.OK);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) };
    }

    private static double NoJitter()
    {
        return 1;
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage responseMessage) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(responseMessage);
        }
    }

    private sealed class SequenceHttpMessageHandler(params HttpResponseMessage[] responseMessages) : HttpMessageHandler, IDisposable
    {
        private readonly Queue<HttpResponseMessage> _responseMessages = new(responseMessages);

        public int RequestCount { get; private set; }

        public new void Dispose()
        {
            while (_responseMessages.TryDequeue(out var responseMessage))
                responseMessage.Dispose();

            base.Dispose();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            if (!_responseMessages.TryDequeue(out var responseMessage))
                throw new InvalidOperationException("No response message was queued for this request.");

            return Task.FromResult(responseMessage);
        }
    }

    private sealed class RecordingHttpMessageHandler(HttpResponseMessage responseMessage) : HttpMessageHandler, IDisposable
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastContentType { get; private set; }
        public string? LastContentBody { get; private set; }
        public bool HadContent { get; private set; }

        public new void Dispose()
        {
            LastRequest?.Dispose();
            responseMessage.Dispose();
            base.Dispose();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            HadContent = request.Content is not null;
            LastContentType = request.Content?.Headers.ContentType?.MediaType;
            LastContentBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseMessage;
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
