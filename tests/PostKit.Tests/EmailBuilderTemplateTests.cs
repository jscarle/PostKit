using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Postmark;
using PostKit.Postmark.Email;
using Xunit;

namespace PostKit.Tests;

public class EmailBuilderTemplateTests
{
    [Fact]
    public void WithTemplateId_Build_SetsTemplateProperties()
    {
        var templateModel = new { Name = "Alice" };

        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithTemplate(42, templateModel, inlineCss: true)
            .Build();

        Assert.Equal(42, email.TemplateId);
        Assert.Null(email.TemplateAlias);
        Assert.Same(templateModel, email.TemplateModel);
        Assert.True(email.InlineCss);
    }

    [Fact]
    public void WithTemplateAlias_Build_SetsTemplateProperties()
    {
        var templateModel = new { Name = "Bob" };

        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithTemplate("welcome-email", templateModel, inlineCss: false)
            .Build();

        Assert.Null(email.TemplateId);
        Assert.Equal("welcome-email", email.TemplateAlias);
        Assert.Same(templateModel, email.TemplateModel);
        Assert.False(email.InlineCss);
    }

    [Fact]
    public async Task SendEmailAsync_WithTemplate_RoutesThroughTemplateEndpoint()
    {
        var templateModel = new { Name = "Charlie" };

        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithTemplate(7, templateModel)
            .Build();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsSuccess());
        Assert.Equal("/email/withTemplate", postmark.LastEndpoint);
        Assert.IsType<EmailRequest>(postmark.LastRequest);
    }

    private sealed class RecordingPostmarkClient : IPostmarkClient
    {
        public string? LastEndpoint { get; private set; }

        public object? LastRequest { get; private set; }

        public Task<Result<TResponse>> SendAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            LastRequest = body;

            if (typeof(TResponse) != typeof(EmailResponse))
                throw new InvalidOperationException("Unexpected response type.");

            var response = new EmailResponse
            {
                MessageId = Guid.NewGuid()
                    .ToString(),
                ErrorCode = 0,
                Message = "",
            };

            return Task.FromResult(Result.Success((TResponse)(object)response));
        }
    }

    private sealed class TestLogger : ILogger<PostKitClient>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

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
