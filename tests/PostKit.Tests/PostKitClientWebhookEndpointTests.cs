using System.Net;
using System.Text.Json.Nodes;
using PostKit.Errors;
using PostKit.Webhooks;

namespace PostKit.Tests;

public class PostKitClientWebhookEndpointTests
{
    private const string WebhookJson = """{"ID":7,"Url":"https://example.com/hook","MessageStream":"outbound","Triggers":{}}""";
    private const string VerificationJson = """
        {"Id":7,"Url":"https://example.com/hook","Success":false,"Message":"1/2 triggers verified successfully","Results":[
          {"TriggerType":"Delivery","Success":true,"StatusCode":200,"Message":"OK"},
          {"TriggerType":"FutureEvent","Success":false,"StatusCode":0,"Message":"No response"}]}
        """;
    private const string MetricsJson = """
        {"TotalRequests":3000000000,"SuccessCount":4,"FailureCount":1,"RetryCount":2,"SuccessRate":80,
         "SlowCount":1,"VerySlowCount":0,"AverageTerminalResponseTimeMs":25,"AverageRetryResponseTimeMs":null}
        """;
    private const string StatisticsJson = $$$"""
        {"WebhookId":7,"ServerId":8,"MessageStreamId":"outbound","Url":"https://example.com/hook",
         "Statuses":{"FutureEvent":"future-status"},"TimeRange":{"StartTime":"2026-08-11T18:00:00Z","EndTime":"2026-08-12T18:00:00Z","Hours":24},
         "Metrics":{{{MetricsJson}}},"MetricsByTrigger":{"FutureEvent":{{{MetricsJson}}}}}
        """;

    [Fact]
    public async Task GetWebhookStatisticsAsync_WithCompleteResponse_PreservesMetricsAndFutureTriggers()
    {
        using var session = new PostmarkEndpointTestSession(StatisticsJson);

        var result = await session.Client.GetWebhookStatisticsAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var statistics), result.ToString());
        Assert.Equal(3000000000L, statistics.Metrics.TotalRequests);
        Assert.Equal("future-status", statistics.Statuses["FutureEvent"]);
        Assert.Null(statistics.MetricsByTrigger["FutureEvent"].AverageRetryResponseTimeMs);
        Assert.Equal(TimeSpan.Zero, statistics.TimeRange.StartTime.Offset);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string>)statistics.Statuses).Add("Other", "verified"));
        Assert.Equal(HttpMethod.Get, session.Method);
        Assert.Equal("/webhooks/7/statistics", session.Endpoint);
        Assert.Equal("test-server-token", session.ServerToken);
        Assert.False(session.HasAccountToken);
    }

    [Fact]
    public async Task GetWebhookStatisticsAsync_WithNoActivity_AcceptsZerosAndEmptyDictionaries()
    {
        var body = JsonNode.Parse(StatisticsJson)!;
        body["Statuses"] = new JsonObject();
        body["MetricsByTrigger"] = new JsonObject();
        foreach (var name in body["Metrics"]!.AsObject().Select(static property => property.Key).ToArray())
            body["Metrics"]![name] = name == "AverageRetryResponseTimeMs" ? null : JsonValue.Create(0);
        body["Metrics"]!.AsObject().Remove("AverageTerminalResponseTimeMs");
        body["Metrics"]!.AsObject().Remove("AverageRetryResponseTimeMs");
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetWebhookStatisticsAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var statistics), result.ToString());
        Assert.Empty(statistics.Statuses);
        Assert.Empty(statistics.MetricsByTrigger);
        Assert.Equal(0, statistics.Metrics.TotalRequests);
        Assert.Null(statistics.Metrics.AverageTerminalResponseTimeMs);
        Assert.Null(statistics.Metrics.AverageRetryResponseTimeMs);
    }

    [Theory]
    [InlineData("TotalRequests", -1)]
    [InlineData("SuccessCount", -1)]
    [InlineData("FailureCount", -1)]
    [InlineData("RetryCount", -1)]
    [InlineData("SlowCount", -1)]
    [InlineData("VerySlowCount", -1)]
    [InlineData("AverageTerminalResponseTimeMs", -1)]
    [InlineData("AverageRetryResponseTimeMs", -1)]
    [InlineData("SuccessRate", 101)]
    public async Task GetWebhookStatisticsAsync_WithInvalidMetric_IdentifiesTheAffectedProperty(string name, int value)
    {
        var body = JsonNode.Parse(StatisticsJson)!;
        body["Metrics"]![name] = value;
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetWebhookStatisticsAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkInvalidResponseError>(error);
        Assert.Contains(name, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("WebhookId")]
    [InlineData("ServerId")]
    [InlineData("MessageStreamId")]
    [InlineData("Url")]
    [InlineData("Statuses")]
    [InlineData("TimeRange")]
    [InlineData("Metrics")]
    [InlineData("MetricsByTrigger")]
    public async Task GetWebhookStatisticsAsync_WithMissingRequiredField_ReturnsInvalidResponse(string name)
    {
        var body = JsonNode.Parse(StatisticsJson)!.AsObject();
        body.Remove(name);
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetWebhookStatisticsAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkInvalidResponseError>(error);
        Assert.Contains(name, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetWebhookStatisticsAsync_WithReversedWindow_ReturnsInvalidResponse()
    {
        var body = JsonNode.Parse(StatisticsJson)!;
        body["TimeRange"]!["StartTime"] = "2026-08-13T00:00:00Z";
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetWebhookStatisticsAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("StartTime", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WebhookEndpoints_WithInvalidId_DoNotSendRequests(long id)
    {
        using var session = new PostmarkEndpointTestSession("{}");

        var statistics = await session.Client.GetWebhookStatisticsAsync(id, TestContext.Current.CancellationToken);
        var verification = await session.Client.VerifyWebhookAsync(id, TestContext.Current.CancellationToken);

        Assert.True(statistics.IsFailure());
        Assert.True(verification.IsFailure());
        Assert.Equal(0, session.RequestCount);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public async Task VerifyWebhookAsync_WithCompletedCheck_ReturnsTheProviderOutcome(int passingTriggers, bool success)
    {
        var body = JsonNode.Parse(VerificationJson)!;
        body["Success"] = success;
        for (var index = 0; index < 2; index++)
        {
            body["Results"]![index]!["Success"] = index < passingTriggers;
            body["Results"]![index]!["StatusCode"] = index < passingTriggers ? 200 : 0;
        }
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var verification), result.ToString());
        Assert.Equal(success, verification.Success);
        Assert.Equal(2, verification.Results.Count);
        Assert.Equal("FutureEvent", verification.Results[1].TriggerType);
        Assert.Equal(passingTriggers == 2 ? 200 : 0, verification.Results[1].StatusCode);
        Assert.Throws<NotSupportedException>(() => ((IList<WebhookVerificationTriggerResult>)verification.Results).Clear());
        Assert.Equal(HttpMethod.Post, session.Method);
        Assert.Equal("/webhooks/7/verify", session.Endpoint);
        Assert.True(string.IsNullOrEmpty(session.Body));
        Assert.Equal("test-server-token", session.ServerToken);
        Assert.False(session.HasAccountToken);
    }

    [Theory]
    [InlineData("Id")]
    [InlineData("Url")]
    [InlineData("Success")]
    [InlineData("Results")]
    [InlineData("Message")]
    public async Task VerifyWebhookAsync_WithMissingRequiredField_ReturnsInvalidResponse(string name)
    {
        var body = JsonNode.Parse(VerificationJson)!.AsObject();
        body.Remove(name);
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkInvalidResponseError>(error);
        Assert.Contains(name, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateWebhookAsync_WithOptionalVerify_PreservesSerializationAndStatus(bool? verify)
    {
        var body = JsonNode.Parse(WebhookJson)!;
        body["Status"] = "future-status";
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.CreateWebhookAsync(new WebhookCreateParameters { Url = "https://example.com/hook", Verify = verify }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var webhook), result.ToString());
        Assert.Equal("future-status", webhook.Status);
        var request = JsonNode.Parse(session.Body!)!.AsObject();
        Assert.Equal(verify.HasValue, request.ContainsKey("Verify"));
        if (verify.HasValue)
            Assert.Equal(verify.Value, request["Verify"]!.GetValue<bool>());
        Assert.Equal(verify.HasValue ? $"/webhooks?verify={(verify.Value ? "true" : "false")}" : "/webhooks", session.Endpoint);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EditWebhookAsync_WithOnlyVerify_SendsTheExplicitValue(bool verify)
    {
        var existing = JsonNode.Parse(WebhookJson)!;
        existing["Triggers"] = JsonNode.Parse("""{"Delivery":{"Enabled":true},"Open":{"Enabled":true,"PostFirstOpenOnly":false}}""");
        using var session = new PostmarkEndpointTestSession(existing.ToJsonString());

        var result = await session.Client.EditWebhookAsync(7, new WebhookEditParameters { Verify = verify }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var webhook), result.ToString());
        Assert.Null(webhook.Status);
        Assert.Equal(HttpMethod.Put, session.Method);
        Assert.Equal(2, session.RequestCount);
        Assert.Equal($"/webhooks/7?verify={(verify ? "true" : "false")}", session.Endpoint);
        Assert.Equal("https://example.com/hook", JsonNode.Parse(session.Body!)!["Url"]!.GetValue<string>());
        Assert.Equal(verify, JsonNode.Parse(session.Body!)!["Verify"]!.GetValue<bool>());
        if (verify)
        {
            Assert.True(JsonNode.Parse(session.Body!)!["Triggers"]!["Delivery"]!["Enabled"]!.GetValue<bool>());
            Assert.False(JsonNode.Parse(session.Body!)!["Triggers"]!["Open"]!["PostFirstOpenOnly"]!.GetValue<bool>());
        }
    }

    [Fact]
    public async Task EditWebhookAsync_WhenVerificationLookupFails_DoesNotSendTheMutation()
    {
        using var session = new PostmarkEndpointTestSession("", HttpStatusCode.NotFound);

        var result = await session.Client.EditWebhookAsync(7, new WebhookEditParameters { Verify = true }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<HttpError>(error);
        Assert.Equal(1, session.RequestCount);
        Assert.Equal(HttpMethod.Get, session.Method);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateWebhookAsync_WithNestedVerificationFailure_PreservesBothMessagesAndUnsavedId(bool bodyCode)
    {
        var verification = JsonNode.Parse(VerificationJson)!;
        verification["Id"] = 0;
        var body = new JsonObject
        {
            ["Message"] = "Nothing was saved.",
            ["Errors"] = new JsonObject { ["Hint"] = "Verify the endpoint." },
            ["Verification"] = verification
        };
        if (bodyCode)
            body["ErrorCode"] = 1364;
        using var session = new PostmarkEndpointTestSession(body.ToJsonString(), HttpStatusCode.UnprocessableEntity, "1364");

        var result = await session.Client.CreateWebhookAsync(new WebhookCreateParameters { Url = "https://example.com/hook", Verify = true }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal("Nothing was saved.", postmarkError.Message);
        Assert.Equal("Verify the endpoint.", postmarkError.Errors!["Hint"]!.GetValue<string>());
        var details = Assert.IsType<WebhookVerification>(postmarkError.Verification);
        Assert.Null(details.Id);
        Assert.Equal("1/2 triggers verified successfully", details.Message);
        Assert.Equal("https://example.com/hook", details.Url);
        Assert.Equal(2, details.Results.Count);
    }

    [Fact]
    public async Task UnrelatedEndpoint_WithHeaderOnlyVerificationError_KeepsStrictGenericParsing()
    {
        using var session = new PostmarkEndpointTestSession(VerificationJson, HttpStatusCode.UnprocessableEntity, "1364");

        var result = await session.Client.ListBulkEmailRequestsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkInvalidResponseError>(error);
    }

    [Fact]
    public async Task EditWebhookAsync_WithEmptyParameters_FailsBeforeSending()
    {
        using var session = new PostmarkEndpointTestSession(WebhookJson);

        var result = await session.Client.EditWebhookAsync(7, new WebhookEditParameters(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("Verify", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, session.RequestCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task WebhookMutation_WithVerificationFailure_PreservesTypedErrorDetails(bool edit, bool bodyCode)
    {
        var body = JsonNode.Parse(VerificationJson)!.AsObject();
        body.Remove("Id");
        if (bodyCode)
            body["ErrorCode"] = 1364;
        using var session = new PostmarkEndpointTestSession(body.ToJsonString(), HttpStatusCode.UnprocessableEntity, bodyCode ? null : "1364");

        var result = edit
            ? await session.Client.EditWebhookAsync(7, new WebhookEditParameters
            {
                Url = "https://example.com/hook", Verify = true, Triggers = new WebhookTriggers { Delivery = new WebhookBasicTrigger { Enabled = true } }
            }, TestContext.Current.CancellationToken)
            : await session.Client.CreateWebhookAsync(new WebhookCreateParameters { Url = "https://example.com/hook" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal(PostmarkErrorCode.WebhookVerificationFailed, postmarkError.ErrorCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, postmarkError.StatusCode);
        var verification = Assert.IsType<WebhookVerification>(postmarkError.Verification);
        Assert.Null(verification.Id);
        Assert.False(verification.Success);
        Assert.Equal("No response", verification.Results[1].Message);
    }

    [Fact]
    public async Task VerifyWebhookAsync_WithStandardError_PreservesPostmarkError()
    {
        using var session = new PostmarkEndpointTestSession("""{"ErrorCode":1364,"Message":"Verification rejected."}""", HttpStatusCode.UnprocessableEntity);

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Null(Assert.IsType<PostmarkError>(error).Verification);
    }

    [Fact]
    public async Task VerifyWebhookAsync_WithUnrelatedErrorFields_DoesNotInterpretVerificationData()
    {
        using var session = new PostmarkEndpointTestSession("""{"ErrorCode":99999,"Message":"Future error.","Success":"future shape","Results":42}""", HttpStatusCode.UnprocessableEntity, "1364");

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal((PostmarkErrorCode)99999, postmarkError.ErrorCode);
        Assert.Null(postmarkError.Verification);
    }

    [Fact]
    public async Task VerifyWebhookAsync_WithMalformedVerificationError_ReturnsInvalidResponse()
    {
        using var session = new PostmarkEndpointTestSession("""{"Success":false,"Message":"Failed","Url":"https://example.com/hook","Results":[null]}""", HttpStatusCode.UnprocessableEntity, "1364");

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("Results[0]", Assert.IsType<PostmarkInvalidResponseError>(error).Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task VerifyWebhookAsync_WithHttpFailure_DoesNotRetryThePost(HttpStatusCode status)
    {
        using var session = new PostmarkEndpointTestSession("", status);

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(status, Assert.IsType<HttpError>(error).StatusCode);
        Assert.Equal(1, session.RequestCount);
    }

    [Fact]
    public async Task WebhookEndpoints_WithCancellation_PropagateCancellation()
    {
        using var session = new PostmarkEndpointTestSession(StatisticsJson);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Client.GetWebhookStatisticsAsync(7, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Client.VerifyWebhookAsync(7, cancellation.Token));
    }

    [Fact]
    public async Task VerifyWebhookAsync_WithNetworkFailure_ReturnsUnknownOutcomeWithoutRetry()
    {
        using var session = new PostmarkEndpointTestSession(VerificationJson);
        session.Failure = new HttpRequestException("Connection dropped.");

        var result = await session.Client.VerifyWebhookAsync(7, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkUnknownOutcomeError>(error);
        Assert.Equal(1, session.RequestCount);
    }
}
