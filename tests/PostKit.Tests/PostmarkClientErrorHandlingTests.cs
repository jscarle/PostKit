using System.Net;
using System.Net.Mime;
using System.Text;
using LightResults;
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
        ));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>("/email", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out EmailResponse? _), result.ToString());
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
        ));
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), new TestLogger<PostmarkClient>());

        var result = await client.PostAsync<object, EmailResponse>("/email/bulk", new { Name = "Alice" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out EmailResponse? _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal((PostmarkErrorCode)10, postmarkError.ErrorCode);
        Assert.Equal("The Postmark Test API Token may only be used on the /email endpoint.", postmarkError.Message);
        Assert.Equal(HttpStatusCode.Forbidden, postmarkError.StatusCode);
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage responseMessage) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(responseMessage);
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
