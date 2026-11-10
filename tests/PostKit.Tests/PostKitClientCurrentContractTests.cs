using System.Net;
using System.Text.Json.Nodes;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Errors;
using PostKit.Stats;
using PostKit.Suppressions;
using PostKit.Templates;
using PostKit.Webhooks;

namespace PostKit.Tests;

public class PostKitClientCurrentContractTests
{
    private const string MessageId = "883953f4-6105-42a2-a16a-77a8eac79483";
    // Shape observed on the authorized sandbox: untagged search/details omit Tag.
    private const string UntaggedMessageJson = """
        {"MessageID":"883953f4-6105-42a2-a16a-77a8eac79483","MessageStream":"outbound",
         "To":[{"Email":"recipient@example.com","Name":""}],"Cc":[],"Bcc":[],"Recipients":["recipient@example.com"],
         "ReceivedAt":"2026-11-10T20:00:00Z","From":"sender@example.com","Subject":"Synthetic test",
         "Attachments":[],"Status":"Sent","TrackOpens":false,"TrackLinks":"None","Metadata":{},"Sandboxed":true,"MessageEvents":[]}
        """;
    private const string ConfiguredWebhookJson = """
        {"ID":7,"Url":"https://example.com/hook","MessageStream":"outbound","Status":"verified",
         "HttpAuth":{"Username":"synthetic-user","Password":"synthetic-password"},"HttpHeaders":[{"Name":"X-Synthetic","Value":"original"}],
         "Triggers":{"Open":{"Enabled":true,"PostFirstOpenOnly":true,"Status":"verified"},
                     "Delivery":{"Enabled":true,"Status":"verified"},"Bounce":{"Enabled":true,"IncludeContent":true,"Status":"unverified"}}}
        """;
    private const string LayoutJson = """
        {"TemplateId":7,"Name":"Layout","Active":true,"Alias":"layout","TemplateType":"Layout",
         "AssociatedServerId":8,"HtmlBody":"<main>{{{ @content }}}</main>"}
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EditWebhookAsync_WithPartialChanges_PreservesConfigurationAndExcludesStatuses(bool clearHeaders)
    {
        using var session = new PostmarkEndpointTestSession(ConfiguredWebhookJson);
        var parameters = new WebhookEditParameters
        {
            HttpHeaders = clearHeaders ? [] : null,
            Triggers = new WebhookTriggers { Delivery = new WebhookBasicTrigger { Enabled = false, Status = WebhookVerificationStatus.Unverified } }
        };

        var result = await session.Client.EditWebhookAsync(7, parameters, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var webhook), result.ToString());
        Assert.Equal(WebhookVerificationStatus.Verified, webhook.Status);
        Assert.Equal(WebhookVerificationStatus.Unverified, webhook.Triggers.Bounce!.Status);
        Assert.Equal(2, session.RequestCount);
        Assert.Equal(HttpMethod.Put, session.Method);
        var body = JsonNode.Parse(session.Body!)!;
        Assert.Equal("https://example.com/hook", body["Url"]!.GetValue<string>());
        Assert.Equal("synthetic-password", body["HttpAuth"]!["Password"]!.GetValue<string>());
        Assert.Equal(clearHeaders ? 0 : 1, body["HttpHeaders"]!.AsArray().Count);
        Assert.False(body["Triggers"]!["Delivery"]!["Enabled"]!.GetValue<bool>());
        Assert.True(body["Triggers"]!["Open"]!["PostFirstOpenOnly"]!.GetValue<bool>());
        Assert.True(body["Triggers"]!["Bounce"]!["IncludeContent"]!.GetValue<bool>());
        Assert.DoesNotContain("Status", session.Body!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EditWebhookAsync_WithInvalidUrl_FailsBeforeReadingConfiguration()
    {
        using var session = new PostmarkEndpointTestSession(ConfiguredWebhookJson);

        var result = await session.Client.EditWebhookAsync(7, new WebhookEditParameters { Url = "/relative" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure());
        Assert.Equal(0, session.RequestCount);
    }

    [Fact]
    public async Task EditWebhookAsync_WithCancelledLookup_DoesNotSendMutation()
    {
        using var session = new PostmarkEndpointTestSession(ConfiguredWebhookJson);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Client.EditWebhookAsync(7, new WebhookEditParameters { Verify = false }, cancellation.Token));

        Assert.Equal(0, session.RequestCount);
    }

    [Theory]
    [InlineData("Status")]
    [InlineData("trigger status")]
    public async Task GetWebhookAsync_WithUnrecognizedStatus_ReturnsValueAndReportingGuidance(string location)
    {
        var body = JsonNode.Parse(ConfiguredWebhookJson)!;
        if (location == "Status")
            body["Status"] = "waiting-for-provider";
        else
            body["Triggers"]!["Delivery"]!["Status"] = "waiting-for-provider";
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("waiting-for-provider", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://github.com/jscarle/PostKit/issues", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Open", WebhookTriggerType.Open)]
    [InlineData("Click", WebhookTriggerType.Click)]
    [InlineData("Delivery", WebhookTriggerType.Delivery)]
    [InlineData("Bounce", WebhookTriggerType.Bounce)]
    [InlineData("SpamComplaint", WebhookTriggerType.SpamComplaint)]
    [InlineData("SubscriptionChange", WebhookTriggerType.SubscriptionChange)]
    public async Task VerifyWebhookAsync_WithCurrentTriggerName_MapsTypedResult(string name, WebhookTriggerType expected)
    {
        var body = $$"""{"Id":7,"Url":"https://example.com/hook","Success":false,"Message":"Failed check","Results":[{"TriggerType":"{{name}}","Success":false,"StatusCode":0,"Message":"No response"}]}""";
        using var session = new PostmarkEndpointTestSession(body);

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var verification), result.ToString());
        Assert.False(verification.Success);
        Assert.Equal(expected, Assert.Single(verification.Results).TriggerType);
    }

    [Fact]
    public async Task VerifyWebhookAsync_WithUnrecognizedTrigger_ReportsOriginalValue()
    {
        using var session = new PostmarkEndpointTestSession("""{"Id":7,"Url":"https://example.com/hook","Success":true,"Message":"OK","Results":[{"TriggerType":"NewEvent","Success":true,"StatusCode":200,"Message":"OK"}]}""");

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkInvalidResponseError>(error);
        Assert.Contains("NewEvent", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://github.com/jscarle/PostKit/issues", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetOutboundReadTimeStatsAsync_WithCurrentEmptyResponse_ReturnsEmptyOwnedCollections()
    {
        // This response was observed live on the authorized sandbox on 2026-11-10.
        using var session = new PostmarkEndpointTestSession("""{"Days":[]}""");

        var result = await session.Client.GetOutboundReadTimeStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var statistics), result.ToString());
        Assert.Empty(statistics.Totals);
        Assert.Empty(statistics.Days);
        Assert.Equal("/stats/outbound/opens/readTimes", session.Endpoint);
        Assert.False(session.HasAccountToken);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, int>)statistics.Totals).Add("bucket", 1));
        Assert.Throws<NotSupportedException>(() => ((IList<OutboundUsageStatsDay>)statistics.Days).Clear());
    }

    [Fact]
    public async Task GetOutboundReadTimeStatsAsync_WithNamedBuckets_PreservesCountsAndFilters()
    {
        // SDK-derived dynamic bucket contract; this populated payload is not a live capture.
        using var session = new PostmarkEndpointTestSession("""{"Days":[{"Date":"2026-11-10","read_2s":2}],"read_2s":7,"another-bucket":1}""");
        var query = new OutboundStatsQuery { Tag = "receipt & order", FromDate = new DateOnly(2026, 11, 1), ToDate = new DateOnly(2026, 11, 10), MessageStreamId = "outbound" };

        var result = await session.Client.GetOutboundReadTimeStatsAsync(query, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var statistics), result.ToString());
        Assert.Equal(7, statistics.Totals["read_2s"]);
        Assert.Equal(1, statistics.Totals["another-bucket"]);
        Assert.Equal(2, Assert.Single(statistics.Days).Counts["read_2s"]);
        Assert.Equal(new DateOnly(2026, 11, 10), statistics.Days[0].Date);
        Assert.Equal("/stats/outbound/opens/readTimes?tag=receipt%20%26%20order&fromdate=2026-11-01&todate=2026-11-10&messagestream=outbound", session.Endpoint);
    }

    [Theory]
    [InlineData("{}", "Days")]
    [InlineData("{\"Days\":[],\"bucket\":-1}", "bucket")]
    [InlineData("{\"Days\":[{\"bucket\":1}]}", "Date")]
    [InlineData("{\"Days\":[],\"bucket\":\"one\"}", "bucket")]
    public async Task GetOutboundReadTimeStatsAsync_WithMalformedResponse_ReturnsHelpfulFailure(string body, string affectedProperty)
    {
        using var session = new PostmarkEndpointTestSession(body);

        var result = await session.Client.GetOutboundReadTimeStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains(affectedProperty, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetOutboundReadTimeStatsAsync_WithEmptyDailyCounts_AcceptsTheDateWithoutInventingBuckets()
    {
        using var session = new PostmarkEndpointTestSession("""{"Days":[{"Date":"2026-11-10"}]}""");

        var result = await session.Client.GetOutboundReadTimeStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var statistics), result.ToString());
        Assert.Empty(statistics.Totals);
        Assert.Empty(Assert.Single(statistics.Days).Counts);
    }

    [Fact]
    public async Task GetOutboundSentStatsAsync_WithMalformedSuppliedCategory_DoesNotTreatItAsOmitted()
    {
        using var session = new PostmarkEndpointTestSession("""{"Days":[],"Sent":null}""");

        var result = await session.Client.GetOutboundSentStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("Sent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetOutboundReadTimeStatsAsync_WithBlankFilter_FailsBeforeSending()
    {
        using var session = new PostmarkEndpointTestSession("{}");

        var result = await session.Client.GetOutboundReadTimeStatsAsync(new OutboundStatsQuery { Tag = " " }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("null", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, session.RequestCount);
    }

    [Fact]
    public async Task GetOutboundReadTimeStatsAsync_WithCancelledToken_PropagatesCancellation()
    {
        using var session = new PostmarkEndpointTestSession("{}");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Client.GetOutboundReadTimeStatsAsync(cancellationToken: cancellation.Token));

        Assert.Equal(0, session.RequestCount);
    }

    [Fact]
    public async Task SearchOutboundMessagesAsync_WithUntaggedResponse_ReturnsNullTag()
    {
        using var session = new PostmarkEndpointTestSession($$"""{"TotalCount":1,"Messages":[{{UntaggedMessageJson}}]}""");

        var result = await session.Client.SearchOutboundMessagesAsync("outbound", cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Null(Assert.Single(page.Messages).Tag);
    }

    [Fact]
    public async Task GetBounceAsync_WithOmittedTag_PreservesNull()
    {
        using var session = new PostmarkEndpointTestSession("""
            {"ID":7,"Type":"HardBounce","Name":"Hard bounce","MessageID":"883953f4-6105-42a2-a16a-77a8eac79483",
             "ServerID":8,"MessageStream":"outbound","Description":"Synthetic","Details":"Synthetic",
             "Email":"recipient@example.com","From":"sender@example.com","BouncedAt":"2026-11-10T20:00:00Z",
             "DumpAvailable":false,"Inactive":true,"CanActivate":true,"Subject":"Synthetic","Content":""}
            """);

        var result = await session.Client.GetBounceAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var bounce), result.ToString());
        Assert.Null(bounce.Tag);
    }

    [Fact]
    public async Task GetOutboundMessageDetailsAsync_WithTypedEventFields_ConvertsOnceAndKeepsDiagnostics()
    {
        var body = JsonNode.Parse(UntaggedMessageJson)!;
        body["MessageEvents"] = JsonNode.Parse("""
            [{"Recipient":"recipient@example.com","Type":"Bounced","ReceivedAt":"2026-11-10T20:00:00Z","Details":{"BounceID":"374814878","Summary":"Synthetic"}},
             {"Recipient":"recipient@example.com","Type":"SubscriptionChanged","ReceivedAt":"2026-11-10T20:00:01Z","Details":{"Origin":"Recipient","SuppressSending":"True","ProviderDetail":42}},
             {"Recipient":"recipient@example.com","Type":"LinkClicked","ReceivedAt":"2026-11-10T20:00:02Z","Details":{"ClickLocation":"HTML","Link":"https://example.com"}}]
            """);
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetOutboundMessageDetailsAsync(Guid.Parse(MessageId), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var message), result.ToString());
        Assert.Null(message.Tag);
        var events = message.MessageEvents.ToArray();
        Assert.Equal(374814878L, events[0].Details.BounceId);
        Assert.Equal(SuppressionOrigin.Recipient, events[1].Details.Origin);
        Assert.True(events[1].Details.SuppressSending);
        Assert.Equal("42", events[1].Details.Values["ProviderDetail"]);
        Assert.Equal(LinkClickLocation.Html, events[2].Details.ClickLocation);
    }

    [Theory]
    [InlineData("BounceID", "not-an-id")]
    [InlineData("SuppressSending", "not-a-boolean")]
    [InlineData("Origin", "AnotherOrigin")]
    [InlineData("ClickLocation", "AnotherBody")]
    public async Task GetOutboundMessageDetailsAsync_WithInvalidKnownDetail_ReportsFieldValueAndIndex(string name, string value)
    {
        var body = JsonNode.Parse(UntaggedMessageJson)!;
        body["MessageEvents"] = new JsonArray(new JsonObject
        {
            ["Recipient"] = "recipient@example.com", ["Type"] = "Bounced", ["ReceivedAt"] = "2026-11-10T20:00:00Z",
            ["Details"] = new JsonObject { [name] = value }
        });
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetOutboundMessageDetailsAsync(Guid.Parse(MessageId), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains(value, error.Message, StringComparison.Ordinal);
        Assert.Contains("item 0", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EditTemplateAsync_WithMissingSubject_ValidatesTheCurrentTemplateType(bool layout)
    {
        var body = JsonNode.Parse(LayoutJson)!;
        body["TemplateType"] = layout ? "Layout" : "Standard";
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.EditTemplateAsync(7, new TemplateEditParameters { Name = "Updated", HtmlBody = "<main>{{{ @content }}}</main>" }, TestContext.Current.CancellationToken);

        if (layout)
        {
            Assert.True(result.IsSuccess(out _), result.ToString());
            Assert.Equal(2, session.RequestCount);
            Assert.Equal(HttpMethod.Put, session.Method);
            Assert.False(JsonNode.Parse(session.Body!)!.AsObject().ContainsKey("Subject"));
        }
        else
        {
            Assert.True(result.IsFailure(out var error, out _), result.ToString());
            Assert.Contains("subject is required for standard templates", error.Message, StringComparison.Ordinal);
            Assert.Equal(1, session.RequestCount);
            Assert.Equal(HttpMethod.Get, session.Method);
        }
    }

    [Fact]
    public async Task EditTemplateAsync_WithFailedLookup_DoesNotSendMutation()
    {
        using var session = new PostmarkEndpointTestSession("", HttpStatusCode.NotFound);

        var result = await session.Client.EditTemplateAsync(7, new TemplateEditParameters { Name = "Layout", TextBody = "{{{ @content }}}" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure());
        Assert.Equal(1, session.RequestCount);
        Assert.Equal(HttpMethod.Get, session.Method);
    }

    [Fact]
    public async Task EditTemplateAsync_WithInvalidName_DoesNotReadTemplate()
    {
        using var session = new PostmarkEndpointTestSession(LayoutJson);

        var result = await session.Client.EditTemplateAsync(7, new TemplateEditParameters { Name = " ", TextBody = "{{{ @content }}}" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure());
        Assert.Equal(0, session.RequestCount);
    }

    [Fact]
    public async Task EditTemplateAsync_WithCancelledLookup_DoesNotSendMutation()
    {
        using var session = new PostmarkEndpointTestSession(LayoutJson);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Client.EditTemplateAsync(7,
            new TemplateEditParameters { Name = "Layout", TextBody = "{{{ @content }}}" }, cancellation.Token));

        Assert.Equal(0, session.RequestCount);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("1e400")]
    public async Task GetBulkEmailStatusAsync_WithInvalidProgress_FailsWithTheAffectedProperty(string progress)
    {
        using var session = new PostmarkEndpointTestSession($$"""{"Id":"883953f4-6105-42a2-a16a-77a8eac79483","SubmittedAt":"2026-11-10T20:00:00Z","Status":"Completed","TotalMessages":1,"PercentageCompleted":{{progress}},"Subject":"Test"}""");

        var result = await session.Client.GetBulkEmailStatusAsync(Guid.Parse(MessageId), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("PercentageCompleted", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBulkEmailStatusAsync_WithEmptyReturnedId_ReturnsHelpfulFailure()
    {
        using var session = new PostmarkEndpointTestSession("""{"Id":"00000000-0000-0000-0000-000000000000","SubmittedAt":"2026-11-10T20:00:00Z","Status":"Accepted"}""");

        var result = await session.Client.GetBulkEmailStatusAsync(Guid.Parse(MessageId), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("nonempty GUID", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Id", "\"00000000-0000-0000-0000-000000000000\"", "Id")]
    [InlineData("PercentageCompleted", "101", "PercentageCompleted")]
    public async Task SendBulkEmailAsync_WithMalformedSuccessfulResponse_RejectsInvalidField(string property, string value, string affectedProperty)
    {
        var body = JsonNode.Parse("""{"Id":"883953f4-6105-42a2-a16a-77a8eac79483","SubmittedAt":"2026-11-10T20:00:00Z","Status":"Accepted","TotalMessages":1,"PercentageCompleted":0,"Subject":"Synthetic"}""")!;
        body[property] = JsonNode.Parse(value);
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());
        var bulk = BulkEmail.Compose().From("sender@example.com").Subject("Synthetic").TextBody("Synthetic")
            .AddMessage(BulkEmailMessage.Compose().To("recipient@example.com").Build()).Build();

        var result = await session.Client.SendBulkEmailAsync(bulk, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains(affectedProperty, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EmailBuilders_WithTrackingDisabled_SerializeFalseAndPreserveUnsetDefault()
    {
        Email[] emails =
        [
            Email.Compose().From("sender@example.com").To("recipient@example.com").Subject("Synthetic").TextBody("Synthetic").DisableOpenTracking().Build(),
            Email.FromTemplate(7).From("sender@example.com").To("recipient@example.com").WithModel(new { Name = "Synthetic" }).DisableOpenTracking().Build()
        ];
        foreach (var email in emails)
        {
            using var session = new PostmarkEndpointTestSession("""{"ErrorCode":0,"Message":"OK","MessageID":"883953f4-6105-42a2-a16a-77a8eac79483","SubmittedAt":"2026-11-10T20:00:00Z","To":"recipient@example.com"}""");
            var result = await session.Client.SendEmailAsync(email, TestContext.Current.CancellationToken);
            Assert.True(result.IsSuccess(), result.ToString());
            Assert.False(JsonNode.Parse(session.Body!)!["TrackOpens"]!.GetValue<bool>());
        }
        var unset = Email.Compose().From("sender@example.com").To("recipient@example.com").Subject("Synthetic").TextBody("Synthetic").Build();
        Assert.Null(unset.OpenTracking);
        Assert.Throws<InvalidOperationException>(() => Email.Compose().DisableOpenTracking().EnableOpenTracking());
        Assert.Throws<InvalidOperationException>(() => Email.FromTemplate(7).EnableOpenTracking().DisableOpenTracking());
    }

    [Fact]
    public async Task BulkBuilders_WithTrackingDisabled_SerializeFalseAndPreserveSetOnceValidation()
    {
        var composed = BulkEmail.Compose().From("sender@example.com").Subject("Synthetic").TextBody("Synthetic").DisableOpenTracking()
            .AddMessage(BulkEmailMessage.Compose().To("recipient@example.com").Build()).Build();
        var templated = BulkEmail.FromTemplate(7).From("sender@example.com").DisableOpenTracking()
            .AddMessage(BulkEmailMessage.FromTemplate().To("recipient@example.com").WithModel(new { Name = "Synthetic" }).Build()).Build();

        Assert.False(composed.OpenTracking);
        Assert.False(templated.OpenTracking);
        foreach (var bulk in new[] { composed, templated })
        {
            using var session = new PostmarkEndpointTestSession("""{"Id":"883953f4-6105-42a2-a16a-77a8eac79483","SubmittedAt":"2026-11-10T20:00:00Z","Status":"Accepted","TotalMessages":1,"PercentageCompleted":0,"Subject":"Synthetic"}""");

            var result = await session.Client.SendBulkEmailAsync(bulk, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess(), result.ToString());
            Assert.False(JsonNode.Parse(session.Body!)!["TrackOpens"]!.GetValue<bool>());
        }
        Assert.Throws<InvalidOperationException>(() => BulkEmail.Compose().EnableOpenTracking().DisableOpenTracking());
        Assert.Throws<InvalidOperationException>(() => BulkEmail.FromTemplate(7).DisableOpenTracking().EnableOpenTracking());
    }
}
