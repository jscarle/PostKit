using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Common;
using PostKit.Messages;
using PostKit.Postmark;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class PostKitClientRemainingMessageResponseTests
{
    [Fact]
    public async Task SearchInboundMessagesAsync_UsesInboundEndpointAndMapsResponse()
    {
        const string endpoint = "/messages/inbound?count=50&offset=0&recipient=inbound%40example.com&fromemail=sender%40example.com&tag=welcome&subject=Hello%20there&mailboxhash=abc123&status=blocked&todate=2015-03-01&fromdate=2015-02-01";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            [endpoint] = """
                         {
                           "TotalCount": 1,
                           "InboundMessages": [
                             {
                               "From": "sender@example.com",
                               "FromName": "Sender",
                               "FromFull": { "Email": "sender@example.com", "Name": "Sender" },
                               "To": "inbound@example.com",
                               "ToFull": [{ "Email": "inbound@example.com", "Name": "" }],
                               "CcFull": [],
                               "Cc": "",
                               "ReplyTo": "",
                               "OriginalRecipient": "inbound@example.com",
                               "Subject": "Hello there",
                               "Date": "Fri, 14 Feb 2014 11:12:56 -0500",
                               "MailboxHash": "abc123",
                               "Tag": "welcome",
                               "Attachments": [
                                 { "Name": "invoice.pdf", "ContentID": "", "ContentType": "application/pdf", "ContentLength": 1024 }
                               ],
                               "MessageID": "cc5727a0-ea30-4e79-baea-aa43c9628ac4",
                               "Status": "Blocked"
                             }
                           ]
                         }
                         """
        });
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new InboundMessageQuery
        {
            Recipient = new MailboxAddress("Inbound", "inbound@example.com"),
            FromEmail = new MailboxAddress("Sender", "sender@example.com"),
            Tag = "welcome",
            Subject = "Hello there",
            MailboxHash = "abc123",
            Status = InboundMessageStatus.Blocked,
            FromDate = new DateTimeOffset(2015, 2, 1, 0, 0, 0, TimeSpan.FromHours(-5)),
            ToDate = new DateTimeOffset(2015, 3, 1, 0, 0, 0, TimeSpan.FromHours(-5))
        };

        var result = await client.SearchInboundMessagesAsync(50, 0, query, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(endpoint, postmark.LastGetEndpoint);
        Assert.Equal(1, page.TotalCount);
        var message = Assert.Single(page.Messages);
        Assert.Equal("sender@example.com", message.From);
        Assert.Equal("Sender", message.FromName);
        Assert.Equal("inbound@example.com", message.To);
        Assert.Equal("inbound@example.com", Assert.Single(message.ToFull)
            .Email);
        Assert.Null(message.Cc);
        Assert.Null(message.ReplyTo);
        Assert.Equal("abc123", message.MailboxHash);
        Assert.Equal("welcome", message.Tag);
        Assert.Equal(Guid.Parse("cc5727a0-ea30-4e79-baea-aa43c9628ac4"), message.MessageId);
        Assert.Equal(InboundMessageStatus.Blocked, message.Status);
        var attachment = Assert.Single(message.Attachments);
        Assert.Equal("invoice.pdf", attachment.Name);
        Assert.Null(attachment.ContentId);
        Assert.Equal("application/pdf", attachment.ContentType);
        Assert.Equal(1024, attachment.ContentLength);
    }

    [Fact]
    public async Task GetInboundMessageDetailsAsync_MapsDetails()
    {
        var messageId = Guid.Parse("cc5727a0-ea30-4e79-baea-aa43c9628ac4");
        const string endpoint = "/messages/inbound/cc5727a0-ea30-4e79-baea-aa43c9628ac4/details";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            [endpoint] = """
                         {
                           "From": "sender@example.com",
                           "FromName": "Sender",
                           "FromFull": { "Email": "sender@example.com", "Name": "Sender" },
                           "To": "inbound@example.com",
                           "ToFull": [{ "Email": "inbound@example.com", "Name": "" }],
                           "CcFull": [],
                           "Cc": "",
                           "ReplyTo": "",
                           "OriginalRecipient": "inbound@example.com",
                           "Subject": "Hello there",
                           "Date": "Fri, 14 Feb 2014 11:12:56 -0500",
                           "MailboxHash": "",
                           "TextBody": "Text body",
                           "HtmlBody": "<p>Text body</p>",
                           "Tag": "",
                           "Headers": [{ "Name": "X-Spam-Status", "Value": "No" }],
                           "Attachments": [],
                           "MessageID": "cc5727a0-ea30-4e79-baea-aa43c9628ac4",
                           "BlockedReason": "Blocked by rule.",
                           "Status": "Processed"
                         }
                         """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetInboundMessageDetailsAsync(messageId, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var details), result.ToString());
        Assert.Equal(endpoint, postmark.LastGetEndpoint);
        Assert.Equal("Text body", details.TextBody);
        Assert.Equal("<p>Text body</p>", details.HtmlBody);
        Assert.Equal("Blocked by rule.", details.BlockedReason);
        var header = Assert.Single(details.Headers);
        Assert.Equal("X-Spam-Status", header.Name);
        Assert.Equal("No", header.Value);
        Assert.Equal(InboundMessageStatus.Processed, details.Status);
        Assert.Null(details.MailboxHash);
        Assert.Null(details.Tag);
    }

    [Fact]
    public async Task BypassInboundMessageRulesAsync_UsesPutEndpointAndMapsResponse()
    {
        var messageId = Guid.Parse("cc5727a0-ea30-4e79-baea-aa43c9628ac4");
        const string endpoint = "/messages/inbound/cc5727a0-ea30-4e79-baea-aa43c9628ac4/bypass";
        var postmark = new RecordingPostmarkClient(putResponses: new Dictionary<string, string>
        {
            [endpoint] = """{ "Message": "Successfully bypassed blocked message." }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.BypassInboundMessageRulesAsync(messageId, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var action), result.ToString());
        Assert.Equal(endpoint, postmark.LastPutEndpoint);
        Assert.Equal("Successfully bypassed blocked message.", action.Message);
    }

    [Fact]
    public async Task RetryInboundMessageAsync_UsesPutEndpointAndMapsResponse()
    {
        var messageId = Guid.Parse("cc5727a0-ea30-4e79-baea-aa43c9628ac4");
        const string endpoint = "/messages/inbound/cc5727a0-ea30-4e79-baea-aa43c9628ac4/retry";
        var postmark = new RecordingPostmarkClient(putResponses: new Dictionary<string, string>
        {
            [endpoint] = """{ "Message": "Successfully rescheduled failed message." }"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.RetryInboundMessageAsync(messageId, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var action), result.ToString());
        Assert.Equal(endpoint, postmark.LastPutEndpoint);
        Assert.Equal("Successfully rescheduled failed message.", action.Message);
    }

    [Fact]
    public async Task SearchMessageOpensAsync_UsesOpenSearchEndpointAndMapsResponse()
    {
        const string endpoint =
            "/messages/outbound/opens?count=50&offset=0&recipient=john.doe%40example.com&tag=welcome&messagestream=outbound&client_name=Chrome&client_company=Google&client_family=Chrome&os_name=OS%20X&os_family=OS%20X&os_company=Apple&platform=WebMail&country=Serbia&region=Vojvodina&city=Novi%20Sad";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            [endpoint] = """
                         {
                           "TotalCount": 1,
                           "Opens": [
                             {
                               "RecordType": "Open",
                               "Client": { "Name": "Chrome", "Company": "Google", "Family": "Chrome" },
                               "OS": { "Name": "OS X", "Company": "Apple", "Family": "OS X" },
                               "Platform": "WebMail",
                               "UserAgent": "Mozilla/5.0",
                               "Geo": { "CountryISOCode": "RS", "Country": "Serbia", "RegionISOCode": "VO", "Region": "Vojvodina", "City": "Novi Sad", "Zip": "21000", "Coords": "45.2517,19.8369", "IP": "188.2.95.4" },
                               "MessageID": "927e56d4-dc66-4070-bbf0-1db76c2ae14b",
                               "MessageStream": "outbound",
                               "ReceivedAt": "2014-04-30T05:04:23.8768746-04:00",
                               "Tag": "welcome",
                               "Recipient": "john.doe@example.com"
                             }
                           ]
                         }
                         """
        });
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new MessageTrackingQuery
        {
            Recipient = new MailboxAddress("John Doe", "john.doe@example.com"),
            Tag = "welcome",
            ClientName = "Chrome",
            ClientCompany = "Google",
            ClientFamily = "Chrome",
            OsName = "OS X",
            OsFamily = "OS X",
            OsCompany = "Apple",
            Platform = "WebMail",
            Country = "Serbia",
            Region = "Vojvodina",
            City = "Novi Sad"
        };

        var result = await client.SearchMessageOpensAsync(MessageStream.Transactional, 50, query: query, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(endpoint, postmark.LastGetEndpoint);
        var open = Assert.Single(page.Opens);
        Assert.Equal("Open", open.RecordType);
        Assert.Equal("Chrome", open.Client!.Name);
        Assert.Equal("OS X", open.Os!.Family);
        Assert.Equal("Novi Sad", open.Geo!.City);
        Assert.Equal(Guid.Parse("927e56d4-dc66-4070-bbf0-1db76c2ae14b"), open.MessageId);
        Assert.Equal("outbound", open.MessageStream);
    }

    [Fact]
    public async Task GetMessageClicksAsync_UsesSingleClickEndpointAndMapsResponse()
    {
        var messageId = Guid.Parse("927e56d4-dc66-4070-bbf0-1db76c2ae14b");
        const string endpoint = "/messages/outbound/clicks/927e56d4-dc66-4070-bbf0-1db76c2ae14b?count=10&offset=5";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            [endpoint] = """
                         {
                           "TotalCount": 1,
                           "Clicks": [
                             {
                               "ClickLocation": "HTML",
                               "Client": { "Name": "Chrome", "Company": "Google", "Family": "Chrome" },
                               "OS": { "Name": "OS X", "Company": "Apple", "Family": "OS X" },
                               "OriginalLink": "https://example.com",
                               "Platform": "WebMail",
                               "UserAgent": "Mozilla/5.0",
                               "Geo": null,
                               "MessageID": "927e56d4-dc66-4070-bbf0-1db76c2ae14b",
                               "MessageStream": "outbound",
                               "ReceivedAt": "2014-04-30T05:04:23.8768746-04:00",
                               "Tag": "welcome",
                               "Recipient": "john.doe@example.com"
                             }
                           ]
                         }
                         """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetMessageClicksAsync(messageId, 10, 5, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(endpoint, postmark.LastGetEndpoint);
        var click = Assert.Single(page.Clicks);
        Assert.Null(click.RecordType);
        Assert.Equal("HTML", click.ClickLocation);
        Assert.Equal("https://example.com", click.OriginalLink);
        Assert.Equal("welcome", click.Tag);
        Assert.Equal("john.doe@example.com", click.Recipient);
    }

    [Fact]
    public async Task SearchInboundMessagesAsync_WithWhitespaceMailboxHash_FailsBeforeApiCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchInboundMessagesAsync(query: new InboundMessageQuery { MailboxHash = "\t " }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The inbound message query mailbox hash filter cannot be empty or whitespace. Set MailboxHash to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastGetEndpoint);
    }

    [Fact]
    public async Task SearchMessageClicksAsync_WithWhitespaceClientName_FailsBeforeApiCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchMessageClicksAsync("outbound", query: new MessageTrackingQuery { ClientName = " " }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The message clicks query client name filter cannot be empty or whitespace. Set ClientName to null to omit this filter. Actual length: 1.", error.Message);
        Assert.Null(postmark.LastGetEndpoint);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? getResponses = null, Dictionary<string, string>? putResponses = null) : IPostmarkClient
    {
        public string? LastGetEndpoint { get; private set; }

        public string? LastPutEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastGetEndpoint = endpoint;
            return GetResponse<TResponse>(getResponses, endpoint);
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastPutEndpoint = endpoint;
            return GetResponse<TResponse>(putResponses, endpoint);
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
