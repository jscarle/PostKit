using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Common;
using PostKit.Messages;
using PostKit.Postmark;

namespace PostKit.Tests;

public class PostKitClientMessageResponseTests
{
    private const string SuccessfulResponseJson = """
                                                  {
                                                    "TotalCount": 1,
                                                    "Messages": [
                                                      {
                                                        "Tag": "Invitation",
                                                        "MessageID": "0ac29aee-e1cd-480d-b08d-4f48548ff48d",
                                                        "MessageStream": "outbound",
                                                        "To": [
                                                          {
                                                            "Email": "john.doe@yahoo.com",
                                                            "Name": null
                                                          }
                                                        ],
                                                        "Cc": [
                                                          {
                                                            "Email": "copy@example.com",
                                                            "Name": "Carbon Copy"
                                                          }
                                                        ],
                                                        "Bcc": [],
                                                        "Recipients": [
                                                          "john.doe@yahoo.com",
                                                          "copy@example.com"
                                                        ],
                                                        "ReceivedAt": "2014-02-20T07:25:02.8782715-05:00",
                                                        "From": "\"Joe\" <joe@domain.com>",
                                                        "Subject": "staging",
                                                        "Attachments": [
                                                          {
                                                            "Name": "invoice.pdf",
                                                            "ContentType": "application/pdf",
                                                            "ContentID": "cid:invoice",
                                                            "Content": "YmFzZTY0"
                                                          }
                                                        ],
                                                        "Status": "Sent",
                                                        "TrackOpens": true,
                                                        "TrackLinks": "HtmlAndText",
                                                        "Metadata": {
                                                          "color": "blue",
                                                          "client-id": "12345"
                                                        },
                                                        "Sandboxed": false
                                                      }
                                                    ]
                                                  }
                                                  """;

    private const string SuccessfulDetailsResponseJson = """
                                                         {
                                                           "TextBody": "Thank you for your order...",
                                                           "HtmlBody": "<p>Thank you for your order...</p>",
                                                           "Body": "SMTP dump data",
                                                           "Tag": "product-orders",
                                                           "MessageID": "07311c54-0687-4ab9-b034-b54b5bad88ba",
                                                           "MessageStream": "outbound",
                                                           "To": [
                                                             {
                                                               "Email": "john.doe@yahoo.com",
                                                               "Name": null
                                                             }
                                                           ],
                                                           "Cc": [],
                                                           "Bcc": [],
                                                           "Recipients": [
                                                             "john.doe@yahoo.com"
                                                           ],
                                                           "ReceivedAt": "2014-02-14T11:12:54.8054242-05:00",
                                                           "From": "\"Joe\" <joe@domain.com>",
                                                           "Subject": "Parts Order #5454",
                                                           "Attachments": [
                                                             "myimage.png",
                                                             "mypaper.doc"
                                                           ],
                                                           "Status": "Sent",
                                                           "TrackOpens": true,
                                                           "TrackLinks": "HtmlOnly",
                                                           "Metadata": {
                                                             "color": "blue",
                                                             "client-id": "12345"
                                                           },
                                                           "Sandboxed": false,
                                                           "MessageEvents": [
                                                             {
                                                               "Recipient": "john.doe@yahoo.com",
                                                               "Type": "Delivered",
                                                               "ReceivedAt": "2014-02-14T11:13:10.8054242-05:00",
                                                               "Details": {
                                                                 "DeliveryMessage": "smtp;250 2.0.0 OK",
                                                                 "DestinationServer": "yahoo-smtp-in.l.yahoo.com",
                                                                 "DestinationIP": "173.194.74.256",
                                                                 "ProviderLatencyMs": 42
                                                               }
                                                             },
                                                             {
                                                               "Recipient": "click-tracked@example.com",
                                                               "Type": "LinkClicked",
                                                               "ReceivedAt": "2016-10-05T16:03:56.0000000-04:00",
                                                               "Details": {
                                                                 "Summary": "Tracked link was clicked from the HTMLBody.",
                                                                 "Link": "https://example.com/a/path/to/the/future?queryValue=1&queryValue=2",
                                                                 "ClickLocation": "HTML"
                                                               }
                                                             }
                                                           ]
                                                         }
                                                         """;

    private const string SuccessfulDumpResponseJson = """
                                                      {
                                                        "Body": "From: \"John Doe\" <john.doe@yahoo.com>\r\nTo: \"john.doe@yahoo.com\" <john.doe@yahoo.com>\r\n\r\nThank you for your order=2E=2E=2E\r\n"
                                                      }
                                                      """;

    [Fact]
    public async Task SearchOutboundMessagesAsync_UsesOutboundMessageSearchEndpointAndMapsResponse()
    {
        const string endpoint =
            "/messages/outbound?count=50&offset=0&recipient=john.doe%40yahoo.com&fromemail=joe%40domain.com&tag=welcome&status=sent&todate=2015-01-12&fromdate=2015-01-01&subject=staging%20%2B%20prod&messagestream=outbound&metadata_color=blue";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = SuccessfulResponseJson });
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new OutboundMessageQuery
        {
            Recipient = new MailboxAddress("John Doe", "john.doe@yahoo.com"),
            FromEmail = new MailboxAddress("Joe", "joe@domain.com"),
            Tag = "welcome",
            Status = OutboundMessageStatus.Sent,
            ToDate = new DateTimeOffset(2015, 1, 12, 0, 0, 0, TimeSpan.FromHours(-5)),
            FromDate = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.FromHours(-5)),
            Subject = "staging + prod",
            Metadata = new OutboundMessageMetadataFilter { Name = "color", Value = "blue" }
        };

        var result = await client.SearchOutboundMessagesAsync("outbound", 50, query: query, cancellationToken: CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Equal(1, response.TotalCount);

        var message = Assert.Single(response.Messages);
        Assert.Equal("Invitation", message.Tag);
        Assert.Equal(Guid.Parse("0ac29aee-e1cd-480d-b08d-4f48548ff48d"), message.MessageId);
        Assert.Equal("outbound", message.MessageStream);
        Assert.Equal(DateTimeOffset.Parse("2014-02-20T07:25:02.8782715-05:00"), message.ReceivedAt);
        Assert.Equal("\"Joe\" <joe@domain.com>", message.From);
        Assert.Equal("staging", message.Subject);
        Assert.Equal(OutboundMessageStatus.Sent, message.Status);
        Assert.True(message.TrackOpens);
        Assert.Equal(LinkTracking.HtmlAndText, message.TrackLinks);
        Assert.False(message.Sandboxed);
        Assert.Equal("blue", message.Metadata["color"]);

        var to = Assert.Single(message.To);
        Assert.Equal("john.doe@yahoo.com", to.Email);
        Assert.Null(to.Name);
        var cc = Assert.Single(message.Cc);
        Assert.Equal("copy@example.com", cc.Email);
        Assert.Equal("Carbon Copy", cc.Name);
        Assert.Empty(message.Bcc);
        Assert.Equal(["john.doe@yahoo.com", "copy@example.com"], message.Recipients);

        var attachment = Assert.Single(message.Attachments);
        Assert.Equal("invoice.pdf", attachment.Name);
        Assert.Equal("application/pdf", attachment.ContentType);
        Assert.Equal("cid:invoice", attachment.ContentId);
        Assert.Equal("YmFzZTY0", attachment.Content);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_UsesOutboundMessageDetailsEndpointAndMapsResponse()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = SuccessfulDetailsResponseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDetailsAsync(messageId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Equal("Thank you for your order...", response.TextBody);
        Assert.Equal("<p>Thank you for your order...</p>", response.HtmlBody);
        Assert.Equal("SMTP dump data", response.Body);
        Assert.Equal("product-orders", response.Tag);
        Assert.Equal(messageId, response.MessageId);
        Assert.Equal("outbound", response.MessageStream);
        Assert.Equal(DateTimeOffset.Parse("2014-02-14T11:12:54.8054242-05:00"), response.ReceivedAt);
        Assert.Equal("\"Joe\" <joe@domain.com>", response.From);
        Assert.Equal("Parts Order #5454", response.Subject);
        Assert.Equal(OutboundMessageStatus.Sent, response.Status);
        Assert.True(response.TrackOpens);
        Assert.Equal(LinkTracking.HtmlOnly, response.TrackLinks);
        Assert.False(response.Sandboxed);
        Assert.Equal("blue", response.Metadata["color"]);

        var to = Assert.Single(response.To);
        Assert.Equal("john.doe@yahoo.com", to.Email);
        Assert.Null(to.Name);
        Assert.Empty(response.Cc);
        Assert.Empty(response.Bcc);
        Assert.Equal(["john.doe@yahoo.com"], response.Recipients);
        Assert.Equal(["myimage.png", "mypaper.doc"], response.Attachments.Select(static attachment => attachment.Name));

        var delivered = response.MessageEvents.ElementAt(0);
        Assert.Equal("john.doe@yahoo.com", delivered.Recipient);
        Assert.Equal(OutboundMessageEventType.Delivered, delivered.Type);
        Assert.Equal(DateTimeOffset.Parse("2014-02-14T11:13:10.8054242-05:00"), delivered.ReceivedAt);
        Assert.Equal("smtp;250 2.0.0 OK", delivered.Details.DeliveryMessage);
        Assert.Equal("yahoo-smtp-in.l.yahoo.com", delivered.Details.DestinationServer);
        Assert.Equal("173.194.74.256", delivered.Details.DestinationIp);
        Assert.Equal("42", delivered.Details.Values["ProviderLatencyMs"]);

        var clicked = response.MessageEvents.ElementAt(1);
        Assert.Equal("click-tracked@example.com", clicked.Recipient);
        Assert.Equal(OutboundMessageEventType.LinkClicked, clicked.Type);
        Assert.Equal("Tracked link was clicked from the HTMLBody.", clicked.Details.Summary);
        Assert.Equal("https://example.com/a/path/to/the/future?queryValue=1&queryValue=2", clicked.Details.Link);
        Assert.Equal("HTML", clicked.Details.ClickLocation);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_WithEmptyMessageId_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDetailsAsync(Guid.Empty, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message ID must not be empty.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_WithUnknownEventType_ReturnsFailureWithRawValue()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details";
        var responseJson = SuccessfulDetailsResponseJson.Replace("\"Type\": \"Delivered\"", "\"Type\": \"FutureEvent\"", StringComparison.Ordinal);
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDetailsAsync(messageId, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("MessageEvents item 0 could not be mapped: Type value 'FutureEvent' returned from the Postmark Messages API is not supported.", error.Message);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_WhenTextBodyIsNull_MapsNullableTextBody()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details";
        var responseJson = SuccessfulDetailsResponseJson.Replace("\"TextBody\": \"Thank you for your order...\"", "\"TextBody\": null", StringComparison.Ordinal);
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDetailsAsync(messageId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Null(response.TextBody);
        Assert.Equal("<p>Thank you for your order...</p>", response.HtmlBody);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_WhenHtmlBodyIsNull_MapsNullableHtmlBody()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details";
        var responseJson = SuccessfulDetailsResponseJson.Replace("\"HtmlBody\": \"<p>Thank you for your order...</p>\"", "\"HtmlBody\": null", StringComparison.Ordinal);
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDetailsAsync(messageId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("Thank you for your order...", response.TextBody);
        Assert.Null(response.HtmlBody);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_WhenBodyIsMissing_MapsNullableBody()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/details";
        var responseJson = SuccessfulDetailsResponseJson.Replace("\"Body\": \"SMTP dump data\",", "", StringComparison.Ordinal);
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDetailsAsync(messageId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Null(response.Body);
        Assert.Equal("Thank you for your order...", response.TextBody);
        Assert.Equal("<p>Thank you for your order...</p>", response.HtmlBody);
    }

    [Fact]
    public async Task GetOutboundMessageDumpAsync_UsesOutboundMessageDumpEndpointAndMapsResponse()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/dump";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = SuccessfulDumpResponseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDumpAsync(messageId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Equal("From: \"John Doe\" <john.doe@yahoo.com>\r\nTo: \"john.doe@yahoo.com\" <john.doe@yahoo.com>\r\n\r\nThank you for your order=2E=2E=2E\r\n", response.Body);
    }

    [Fact]
    public async Task GetOutboundMessageDumpAsync_WithEmptyMessageId_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDumpAsync(Guid.Empty, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message ID must not be empty.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetOutboundMessageDumpAsync_WhenBodyIsEmpty_ReturnsSuccessfulDump()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/dump";
        const string responseJson = """
                                    {
                                      "Body": ""
                                    }
                                    """;
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDumpAsync(messageId, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(string.Empty, response.Body);
    }

    [Fact]
    public async Task GetOutboundMessageDumpAsync_WhenBodyIsMissing_ReturnsHelpfulFailure()
    {
        var messageId = Guid.Parse("07311c54-0687-4ab9-b034-b54b5bad88ba");
        const string endpoint = "/messages/outbound/07311c54-0687-4ab9-b034-b54b5bad88ba/dump";
        const string responseJson = """
                                    {
                                      "Body": null
                                    }
                                    """;
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundMessageDumpAsync(messageId, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Body was not returned from the Postmark Messages API.", error.Message);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithUtcDates_ConvertsQueryWindowToEasternTime()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Messages": []
                                    }
                                    """;

        const string endpoint = "/messages/outbound?count=10&offset=0&todate=2026-01-15T13%3A59%3A59&fromdate=2026-01-15T13%3A00%3A00&messagestream=outbound";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new OutboundMessageQuery { FromDate = new DateTimeOffset(2026, 1, 15, 18, 0, 0, TimeSpan.Zero), ToDate = new DateTimeOffset(2026, 1, 15, 18, 59, 59, TimeSpan.Zero) };

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, query: query, cancellationToken: CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Messages);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithProcessedStatus_UsesProcessedQueryValue()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Messages": []
                                    }
                                    """;

        const string endpoint = "/messages/outbound?count=10&offset=0&status=processed&messagestream=outbound";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, query: new OutboundMessageQuery { Status = OutboundMessageStatus.Processed }, cancellationToken: CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Messages);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithMessageStreamEnum_UsesMappedStreamAndDefaultWindow()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Messages": []
                                    }
                                    """;

        const string endpoint = "/messages/outbound?count=500&offset=0&messagestream=broadcast";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync(MessageStream.Broadcast, cancellationToken: CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Messages);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithStringAttachmentName_MapsNameOnlyAttachment()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Messages": [
                                        {
                                          "Tag": "",
                                          "MessageID": "0ac29aee-e1cd-480d-b08d-4f48548ff48d",
                                          "MessageStream": "outbound",
                                          "To": [],
                                          "Cc": [],
                                          "Bcc": [],
                                          "Recipients": ["john.doe@yahoo.com"],
                                          "ReceivedAt": "2014-02-20T07:25:02.8782715-05:00",
                                          "From": "\"Joe\" <joe@domain.com>",
                                          "Subject": "staging",
                                          "Attachments": ["invoice.pdf"],
                                          "Status": "Queued",
                                          "TrackOpens": false,
                                          "TrackLinks": "None",
                                          "Sandboxed": false
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/messages/outbound?count=10&offset=0&messagestream=outbound"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, cancellationToken: CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        var message = Assert.Single(response.Messages);
        Assert.Equal(OutboundMessageStatus.Queued, message.Status);
        Assert.Equal(LinkTracking.None, message.TrackLinks);
        Assert.Empty(message.Metadata);

        var attachment = Assert.Single(message.Attachments);
        Assert.Equal("invoice.pdf", attachment.Name);
        Assert.Null(attachment.ContentType);
        Assert.Null(attachment.ContentId);
        Assert.Null(attachment.Content);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithUnknownStatus_ReturnsFailureWithRawValue()
    {
        var responseJson = SuccessfulResponseJson.Replace("\"Status\": \"Sent\"", "\"Status\": \"Deferred\"", StringComparison.Ordinal);
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/messages/outbound?count=10&offset=0&messagestream=outbound"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Outbound message item 0 could not be mapped: Status value 'Deferred' returned from the Postmark Messages API is not supported.", error.Message);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithUnknownTrackLinks_ReturnsFailureWithRawValue()
    {
        var responseJson = SuccessfulResponseJson.Replace("\"TrackLinks\": \"HtmlAndText\"", "\"TrackLinks\": \"FutureMode\"", StringComparison.Ordinal);
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/messages/outbound?count=10&offset=0&messagestream=outbound"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("Outbound message item 0 could not be mapped: TrackLinks value 'FutureMode' returned from the Postmark Messages API is not supported.", error.Message);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WhenTotalCountIsNegative_ReturnsHelpfulFailureWithValue()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": -1,
                                      "Messages": []
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/messages/outbound?count=10&offset=0&messagestream=outbound"] = responseJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("TotalCount returned from the Postmark Messages API was invalid. Received -1.", error.Message);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithInvalidCount_Fails()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 0, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query count must be between 1 and 500. Received 0.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WhenCountAndOffsetExceedSearchWindow_Fails()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 500, 9800, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query count and offset cannot exceed 10000 when combined. Count: 500; offset: 9800; combined: 10300.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithWhitespaceSubjectFilter_FailsWithOmitGuidance()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, query: new OutboundMessageQuery { Subject = "\t " }, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query subject filter cannot be empty or whitespace. Set Subject to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithFromDateAfterToDate_FailsWithActualValues()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10,
            query: new OutboundMessageQuery { FromDate = new DateTimeOffset(2026, 3, 12, 0, 0, 0, TimeSpan.Zero), ToDate = new DateTimeOffset(2026, 3, 11, 0, 0, 0, TimeSpan.Zero) }, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query from-date must not be later than the to-date. FromDate: 2026-03-12T00:00:00.0000000+00:00; ToDate: 2026-03-11T00:00:00.0000000+00:00.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithInvalidMessageStream_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("_invalid", 10, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(
            "The outbound message query message stream must be 1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'. First character must be a lowercase letter. Received '_' at index 0.",
            error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithInvalidStatusEnum_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, query: new OutboundMessageQuery { Status = (OutboundMessageStatus)999 }, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query status filter must be OutboundMessageStatus.Queued, OutboundMessageStatus.Sent, or OutboundMessageStatus.Processed. Received 999.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithInvalidMetadataName_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new OutboundMessageQuery { Metadata = new OutboundMessageMetadataFilter { Name = " color", Value = "blue" } };

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, query: query, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query metadata filter name is invalid. The metadata name is required, must not exceed 20 characters, and cannot start or end with whitespace. Invalid leading whitespace space at index 0.",
            error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithWhitespaceMetadataValue_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());
        var query = new OutboundMessageQuery { Metadata = new OutboundMessageMetadataFilter { Name = "color", Value = "\t " } };

        var result = await client.SearchOutboundMessagesAsync("outbound", 10, query: query, cancellationToken: CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound message query metadata filter value cannot be empty or whitespace. Set Metadata to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? getResponses = null) : IPostmarkClient
    {
        private readonly Dictionary<string, string> _getResponses = getResponses ?? [];
        public string? LastEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
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
