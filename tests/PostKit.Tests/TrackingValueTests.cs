using System.Text.Json;
using System.Text.Json.Nodes;
using PostKit.Common;
using PostKit.Messages;
using PostKit.Postmark.Common;
using PostKit.Webhooks;

namespace PostKit.Tests;

public class TrackingValueTests
{
    private const string OpenJson = """
        {"RecordType":"Open","FirstOpen":true,"UserAgent":"Synthetic client","MessageID":"883953f4-6105-42a2-a16a-77a8eac79483",
         "Metadata":{},"ReceivedAt":"2026-11-10T20:00:00Z","Recipient":"recipient@example.com","MessageStream":"outbound"}
        """;

    [Theory]
    [InlineData("WebMail", MessagePlatform.WebMail)]
    [InlineData("Desktop", MessagePlatform.Desktop)]
    [InlineData("Mobile", MessagePlatform.Mobile)]
    [InlineData("Unknown", MessagePlatform.Unknown)]
    public void OpenWebhook_WithCurrentPlatform_MapsEnumAndPreservesWireValue(string wireValue, MessagePlatform expected)
    {
        var body = JsonNode.Parse(OpenJson)!;
        body["Platform"] = wireValue;

        var result = OpenWebhookDeserializer.Deserialize(body);

        Assert.True(result.IsSuccess(out var message), result.ToString());
        Assert.Equal(expected, message.Platform);
        var serialized = JsonSerializer.SerializeToNode(message, PostmarkConfiguration.JsonSerializerOptions)!;
        Assert.Equal(wireValue, serialized["Platform"]!.GetValue<string>());
    }

    [Fact]
    public void OpenWebhook_WithUnrecognizedPlatform_ReportsOriginalValueAndIssueLink()
    {
        var body = JsonNode.Parse(OpenJson)!;
        body["Platform"] = "UnrecognizedDevice";

        var result = OpenWebhookDeserializer.Deserialize(body);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("UnrecognizedDevice", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://github.com/jscarle/PostKit/issues", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public void OpenWebhook_WithReadingTime_ExposesDurationAndRoundTripsSeconds(int seconds)
    {
        var body = JsonNode.Parse(OpenJson)!;
        body["ReadSeconds"] = seconds;

        var result = OpenWebhookDeserializer.Deserialize(body);

        Assert.True(result.IsSuccess(out var message), result.ToString());
        Assert.Equal(TimeSpan.FromSeconds(seconds), message.ReadTime);
        var serialized = JsonSerializer.SerializeToNode(message, PostmarkConfiguration.JsonSerializerOptions)!;
        Assert.Equal(seconds, serialized["ReadSeconds"]!.GetValue<int>());
        Assert.False(serialized.AsObject().ContainsKey("ReadTime"));
    }

    [Fact]
    public void OpenWebhook_WithoutReadingTime_RepresentsUnavailableMeasurement()
    {
        var result = OpenWebhookDeserializer.Deserialize(OpenJson);

        Assert.True(result.IsSuccess(out var message), result.ToString());
        Assert.Null(message.ReadTime);
    }

    [Fact]
    public void OpenWebhook_WithNegativeReadingTime_ReturnsHelpfulFailure()
    {
        var body = JsonNode.Parse(OpenJson)!;
        body["ReadSeconds"] = -1;

        var result = OpenWebhookDeserializer.Deserialize(body);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("ReadSeconds", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(5)]
    public async Task GetMessageOpensAsync_WithReadingTime_MapsNullableDuration(int? seconds)
    {
        var open = JsonNode.Parse(OpenJson)!;
        if (seconds.HasValue)
            open["ReadSeconds"] = seconds.Value;
        var body = new JsonObject { ["TotalCount"] = 1, ["Opens"] = new JsonArray(open) };
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetMessageOpensAsync(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : null, Assert.Single(page.Opens).ReadTime);
    }

    [Theory]
    [InlineData("HTML", LinkClickLocation.Html)]
    [InlineData("Text", LinkClickLocation.Text)]
    public void ClickWebhook_WithCurrentLocation_MapsEnumAndPreservesWireValue(string wireValue, LinkClickLocation expected)
    {
        var body = JsonNode.Parse(OpenJson)!.AsObject();
        body["RecordType"] = "Click";
        body["ClickLocation"] = wireValue;
        body["OriginalLink"] = "https://example.com";

        var result = ClickWebhookDeserializer.Deserialize(body);

        Assert.True(result.IsSuccess(out var message), result.ToString());
        Assert.Equal(expected, message.ClickLocation);
        Assert.Equal(wireValue, JsonSerializer.SerializeToNode(message)!["ClickLocation"]!.GetValue<string>());
    }

    [Fact]
    public void ClickWebhook_WithUnrecognizedLocation_ReportsOriginalValueAndIssueLink()
    {
        var body = JsonNode.Parse(OpenJson)!.AsObject();
        body["RecordType"] = "Click";
        body["ClickLocation"] = "AnotherBody";
        body["OriginalLink"] = "https://example.com";

        var result = ClickWebhookDeserializer.Deserialize(body);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("AnotherBody", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://github.com/jscarle/PostKit/issues", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("HTML", LinkClickLocation.Html)]
    [InlineData("Text", LinkClickLocation.Text)]
    public async Task GetMessageClicksAsync_WithCurrentLocation_ReturnsRequiredTypedValue(string wireValue, LinkClickLocation expected)
    {
        var click = JsonNode.Parse(OpenJson)!;
        click["RecordType"] = "Click";
        click["ClickLocation"] = wireValue;
        var body = new JsonObject { ["TotalCount"] = 1, ["Clicks"] = new JsonArray(click) };
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetMessageClicksAsync(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        LinkClickLocation location = Assert.Single(page.Clicks).ClickLocation;
        Assert.Equal(expected, location);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task MessageClickEndpoints_WithMissingOrNullLocation_ReturnHelpfulFailure(bool search, bool explicitNull)
    {
        var click = JsonNode.Parse(OpenJson)!;
        click["RecordType"] = "Click";
        if (explicitNull)
            click["ClickLocation"] = null;
        var body = new JsonObject { ["TotalCount"] = 1, ["Clicks"] = new JsonArray(click) };
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = search
            ? await session.Client.SearchMessageClicksAsync("outbound", cancellationToken: TestContext.Current.CancellationToken)
            : await session.Client.GetMessageClicksAsync(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("ClickLocation", error.Message, StringComparison.Ordinal);
        Assert.Contains("Click item 0", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetMessageClicksAsync_WithUnrecognizedLocation_ReportsOriginalValueAndIssueLink()
    {
        var click = JsonNode.Parse(OpenJson)!;
        click["RecordType"] = "Click";
        click["ClickLocation"] = "AnotherBody";
        var body = new JsonObject { ["TotalCount"] = 1, ["Clicks"] = new JsonArray(click) };
        using var session = new PostmarkEndpointTestSession(body.ToJsonString());

        var result = await session.Client.GetMessageClicksAsync(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("AnotherBody", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://github.com/jscarle/PostKit/issues", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchMessageOpensAsync_WithInvalidPlatform_FailsBeforeNetworkRequest()
    {
        using var session = new PostmarkEndpointTestSession("{}");

        var result = await session.Client.SearchMessageOpensAsync("outbound", query: new MessageTrackingQuery { Platform = (MessagePlatform)999 }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _));
        Assert.Contains("Platform", error.Message, StringComparison.Ordinal);
        Assert.Contains("999", error.Message, StringComparison.Ordinal);
        Assert.Contains("https://github.com/jscarle/PostKit/issues", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, session.RequestCount);
    }
}
