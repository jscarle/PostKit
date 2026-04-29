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
                    Content = new StringContent("""{"ErrorCode":999999,"Message":"Brand new Postmark error."}""", Encoding.UTF8, MediaTypeNames.Application.Json),
                }
            )
        );
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>("/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
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
                    Content = new StringContent("""{"ErrorCode":10,"Message":"The Postmark Test API Token may only be used on the /email endpoint."}""", Encoding.UTF8, MediaTypeNames.Application.Json),
                }
            )
        );
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>("/email/bulk", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
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
                    Content = new StringContent("""{"ErrorCode":11,"Message":"Multiple errors occurred.","Errors":{"From":["From is required."],"To":["To is required."]}}""", Encoding.UTF8, MediaTypeNames.Application.Json),
                }
            )
        );
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>("/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal(PostmarkErrorCode.MultipleErrorsOccurred, postmarkError.ErrorCode);

        var errors = Assert.IsType<JsonObject>(postmarkError.Errors);
        Assert.Equal("From is required.", errors["From"]?[0]
            ?.GetValue<string>()
        );
        Assert.Equal("To is required.", errors["To"]?[0]
            ?.GetValue<string>()
        );
    }

    [Fact]
    public async Task PutAsync_SendsEmptyJsonPayloadWithApplicationJsonContentType()
    {
        using var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        using var httpClient = new HttpClient(handler);
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PutAsync<PostmarkResponse>("/bounces/1/activate", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.True(handler.HadContent);
        Assert.Equal(MediaTypeNames.Application.Json, handler.LastContentType);
        Assert.Equal("{}", handler.LastContentBody);
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage responseMessage) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
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
