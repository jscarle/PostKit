using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class PostmarkClientLoggingTests
{
    [Fact]
    public async Task PostAsync_TraceLogging_LogsEndpointAndSizeWithoutPayloadContent()
    {
        const string sensitiveSubject = "Quarterly payroll export";
        const string sensitiveBody = "Account number 123456789";
        const string sensitiveRecipient = "recipient@secret.example";
        const string responseJson = """
                                    {
                                      "ErrorCode": 0,
                                      "Message": "OK",
                                      "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                      "SubmittedAt": "2026-03-14T12:00:00Z",
                                      "To": "recipient@secret.example"
                                    }
                                    """;

        using var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json) }));
        var logger = new CollectingLogger<PostmarkClient>();
        var client = new PostmarkClient(httpClient, Options.Create(new PostKitOptions { ServerApiToken = "token" }), logger);
        var request = new EmailRequest
        {
            TemplateId = null,
            TemplateAlias = null,
            TemplateModel = null,
            InlineCss = null,
            From = "sender@postkit.com",
            ReplyTo = null,
            To = sensitiveRecipient,
            Cc = null,
            Bcc = null,
            Subject = sensitiveSubject,
            HtmlBody = null,
            TextBody = sensitiveBody,
            Attachments = null,
            Tag = null,
            Headers = null,
            Metadata = null,
            TrackOpens = null,
            TrackLinks = null,
            MessageStream = null
        };

        var result = await client.PostAsync<EmailRequest, EmailResponse>(PostmarkTokenScope.Server, "/email", request, CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Contains(logger.Messages, static message => message.Contains("Postmark API request to /email with", StringComparison.Ordinal));
        Assert.Contains(logger.Messages, static message => message.Contains("Postmark API response from /email with", StringComparison.Ordinal));
        Assert.DoesNotContain(logger.Messages, message => message.Contains(sensitiveSubject, StringComparison.Ordinal));
        Assert.DoesNotContain(logger.Messages, message => message.Contains(sensitiveBody, StringComparison.Ordinal));
        Assert.DoesNotContain(logger.Messages, message => message.Contains(sensitiveRecipient, StringComparison.Ordinal));
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage responseMessage) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(responseMessage);
        }
    }

    private sealed class CollectingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

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
            Messages.Add(formatter(state, exception));
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
