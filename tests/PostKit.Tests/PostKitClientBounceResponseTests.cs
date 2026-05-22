using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Bounces;
using PostKit.Postmark;
using PostKit.Postmark.Bounces;

namespace PostKit.Tests;

[Collection(BounceResponseTestsCollection.Name)]
public class PostKitClientBounceResponseTests
{
    [Fact]
    public async Task GetBouncesAsync_UsesBounceSearchEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "HardBounce",
                                          "TypeCode": 1,
                                          "Name": "Hard bounce",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "The server was unable to deliver your message.",
                                          "Details": "smtp;550 mailbox unavailable",
                                          "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                          "From": "from@publi-7.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": true,
                                          "CanActivate": true,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
            {
                ["/bounces?count=25&offset=10&type=HardBounce&inactive=true&emailFilter=HardBounce%40bounce-testing.postmarkapp.com&messageID=69ce4784-c202-41c6-a1a9-91757022b25e&tag=ops%2Balerts&todate=2026-03-11T13%3A59%3A59&fromdate=2026-03-11T13%3A00%3A00&messagestream=outbound"] =
                    responseJson,
            }
        );
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);
        var query = new BounceQuery
        {
            Count = 25,
            Offset = 10,
            Type = BounceType.HardBounce,
            Inactive = true,
            EmailFilter = new MailboxAddress("Bounce Target", "HardBounce@bounce-testing.postmarkapp.com"),
            MessageId = Guid.Parse("69ce4784-c202-41c6-a1a9-91757022b25e"),
            Tag = "ops+alerts",
            ToDate = new DateTime(2026, 3, 11, 13, 59, 59),
            FromDate = new DateTime(2026, 3, 11, 13, 0, 0),
            MessageStream = "outbound",
        };

        var result = await client.GetBouncesAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(
            "/bounces?count=25&offset=10&type=HardBounce&inactive=true&emailFilter=HardBounce%40bounce-testing.postmarkapp.com&messageID=69ce4784-c202-41c6-a1a9-91757022b25e&tag=ops%2Balerts&todate=2026-03-11T13%3A59%3A59&fromdate=2026-03-11T13%3A00%3A00&messagestream=outbound",
            postmark.LastEndpoint
        );

        Assert.Equal(1, response.TotalCount);
        var bounce = Assert.Single(response.Bounces);
        Assert.Equal("Bounce", bounce.RecordType);
        Assert.Equal(1599950051, bounce.Id);
        Assert.Equal(BounceType.HardBounce, bounce.Type);
        Assert.Equal(Guid.Parse("69ce4784-c202-41c6-a1a9-91757022b25e"), bounce.MessageId);
        Assert.Equal(18451835, bounce.ServerId);
        Assert.Equal("outbound", bounce.MessageStream);
        Assert.Equal("from@publi-7.com", bounce.From);
        Assert.True(bounce.DumpAvailable);
        Assert.True(bounce.Inactive);
        Assert.True(bounce.CanActivate);
    }

    [Fact]
    public async Task GetBouncesAsync_WithUtcDates_ConvertsQueryWindowToEasternTime()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Bounces": []
                                    }
                                    """;

        const string endpoint = "/bounces?count=10&offset=0&todate=2026-01-15T13%3A59%3A59&fromdate=2026-01-15T13%3A00%3A00";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);
        var query = new BounceQuery { Count = 10, FromDate = new DateTime(2026, 1, 15, 18, 0, 0, DateTimeKind.Utc), ToDate = new DateTime(2026, 1, 15, 18, 59, 59, DateTimeKind.Utc) };

        var result = await client.GetBouncesAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Bounces);
    }

    [Fact]
    public async Task GetBouncesAsync_WithUtcDateOnlyFilters_ConvertsCalendarDateToEasternTime()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Bounces": []
                                    }
                                    """;

        const string endpoint = "/bounces?count=10&offset=0&todate=2026-01-14&fromdate=2026-01-14";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);
        var query = new BounceQuery { Count = 10, FromDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), ToDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) };

        var result = await client.GetBouncesAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Bounces);
    }

    [Fact]
    public async Task GetBouncesAsync_WithInvalidLocalDstTime_ReturnsFailureWithoutCallingApi()
    {
        var invalidLocalTime = DateTime.SpecifyKind(new DateTime(2026, 3, 8, 2, 30, 0), DateTimeKind.Local);
        if (!TimeZoneInfo.Local.IsInvalidTime(invalidLocalTime))
            Assert.Skip($"Local time zone '{TimeZoneInfo.Local.Id}' does not treat 2026-03-08 02:30:00 as invalid.");

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10, FromDate = invalidLocalTime }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query from-date is an invalid local time because it falls within a daylight-saving time transition. Use UTC or choose an unambiguous local time.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WithFromDateAfterToDate_FailsWithActualValues()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);
        var query = new BounceQuery { Count = 10, FromDate = new DateTime(2026, 3, 12), ToDate = new DateTime(2026, 3, 11) };

        var result = await client.GetBouncesAsync(query, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query from-date must not be later than the to-date. FromDate: 2026-03-12T00:00:00.0000000; ToDate: 2026-03-11T00:00:00.0000000.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBounceAsync_UsesBounceEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "ID": 1599950051,
                                      "Type": "HardBounce",
                                      "TypeCode": 1,
                                      "Name": "Hard bounce",
                                      "Tag": "",
                                      "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                      "ServerID": 18451835,
                                      "MessageStream": "outbound",
                                      "Description": "The server was unable to deliver your message.",
                                      "Details": "smtp;550 mailbox unavailable",
                                      "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                      "From": "from@publi-7.com",
                                      "BouncedAt": "2026-03-11T17:33:38Z",
                                      "DumpAvailable": true,
                                      "Inactive": true,
                                      "CanActivate": true,
                                      "Subject": "PostKit Bounces API probe",
                                      "Content": "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.IsAssignableFrom<Bounce>(response);
        Assert.Equal("/bounces/1599950051", postmark.LastEndpoint);
        Assert.Equal("Bounce", response.RecordType);
        Assert.Equal(BounceType.HardBounce, response.Type);
        Assert.Equal(Guid.Parse("69ce4784-c202-41c6-a1a9-91757022b25e"), response.MessageId);
        Assert.Contains("69ce4784-c202-41c6-a1a9-91757022b25e", response.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBouncesAsync_WhenFromIsMissing_MapsNullFrom()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "HardBounce",
                                          "TypeCode": 1,
                                          "Name": "Hard bounce",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "The server was unable to deliver your message.",
                                          "Details": "smtp;550 mailbox unavailable",
                                          "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": true,
                                          "CanActivate": true,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=10&offset=0"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10 }, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        var bounce = Assert.Single(response.Bounces);
        Assert.Null(bounce.From);
    }

    [Fact]
    public async Task GetBouncesAsync_WithChallengeVerification_MapsSupportedBounceType()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "ChallengeVerification",
                                          "TypeCode": 16384,
                                          "Name": "Challenge verification",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "Challenge verification response.",
                                          "Details": "smtp; challenge verification",
                                          "Email": "ChallengeVerification@bounce-testing.postmarkapp.com",
                                          "From": "from@publi-7.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": false,
                                          "CanActivate": false,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=10&offset=0"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10 }, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(BounceType.ChallengeVerification, Assert.Single(response.Bounces)
            .Type
        );
    }

    [Fact]
    public async Task GetBouncesAsync_WithUnknownBounceType_ReturnsFailure()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 1,
                                      "Bounces": [
                                        {
                                          "RecordType": "Bounce",
                                          "ID": 1599950051,
                                          "Type": "BrandNewBounce",
                                          "Name": "Brand new bounce",
                                          "Tag": "",
                                          "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                          "ServerID": 18451835,
                                          "MessageStream": "outbound",
                                          "Description": "The server was unable to deliver your message.",
                                          "Details": "smtp;550 mailbox unavailable",
                                          "Email": "BrandNewBounce@bounce-testing.postmarkapp.com",
                                          "BouncedAt": "2026-03-11T17:33:38Z",
                                          "DumpAvailable": true,
                                          "Inactive": true,
                                          "CanActivate": true,
                                          "Subject": "PostKit Bounces API probe"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=10&offset=0"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10 }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("Bounce item 0 could not be mapped: Bounce type value 'BrandNewBounce' returned from the Postmark Bounces API is not supported.", error.Message);
    }

    [Fact]
    public async Task GetBouncesAsync_WithChallengeVerificationFilter_UsesSupportedTypeValue()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": 0,
                                      "Bounces": []
                                    }
                                    """;

        var endpoint = "/bounces?count=10&offset=0&type=ChallengeVerification";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { [endpoint] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10, Type = BounceType.ChallengeVerification }, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(endpoint, postmark.LastEndpoint);
        Assert.Empty(response.Bounces);
    }

    [Fact]
    public async Task GetBounceAsync_WithWhitespaceOnlySubject_PreservesSubject()
    {
        const string responseJson = """
                                    {
                                      "RecordType": "Bounce",
                                      "ID": 1599950051,
                                      "Type": "HardBounce",
                                      "TypeCode": 1,
                                      "Name": "Hard bounce",
                                      "Tag": "",
                                      "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                      "ServerID": 18451835,
                                      "MessageStream": "outbound",
                                      "Description": "The server was unable to deliver your message.",
                                      "Details": "smtp;550 mailbox unavailable",
                                      "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                      "From": "from@publi-7.com",
                                      "BouncedAt": "2026-03-11T17:33:38Z",
                                      "DumpAvailable": true,
                                      "Inactive": true,
                                      "CanActivate": true,
                                      "Subject": "   ",
                                      "Content": "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("   ", response.Subject);
    }

    [Fact]
    public async Task GetBouncesAsync_WhenTotalCountIsNegative_ReturnsHelpfulFailureWithValue()
    {
        const string responseJson = """
                                    {
                                      "TotalCount": -1,
                                      "Bounces": []
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces?count=10&offset=0"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10 }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("TotalCount returned from the Postmark Bounces API was invalid. Received -1.", error.Message);
    }

    [Fact]
    public async Task GetBounceAsync_WhenIdIsNotPositive_ReturnsHelpfulFailureWithValue()
    {
        const string responseJson = """
                                    {
                                      "ID": 0
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("ID returned from the Postmark Bounces API was invalid. Received 0.", error.Message);
    }

    [Fact]
    public async Task GetBounceAsync_WhenServerIdIsNotPositive_ReturnsHelpfulFailureWithValue()
    {
        const string responseJson = """
                                    {
                                      "ID": 1599950051,
                                      "Type": "HardBounce",
                                      "Name": "Hard bounce",
                                      "Tag": "",
                                      "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                      "ServerID": 0
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("ServerID returned from the Postmark Bounces API was invalid. Received 0.", error.Message);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_UsesDeliveryStatsEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": 1,
                                      "Bounces": [
                                        {
                                          "Name": "All",
                                          "Count": 4
                                        },
                                        {
                                          "Type": "SoftBounce",
                                          "Name": "Soft bounce",
                                          "Count": 3
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/deliverystats", postmark.LastEndpoint);
        Assert.Equal(1, response.InactiveMails);
        Assert.Equal(2, response.Bounces.Count);
        var bounces = response.Bounces.ToArray();
        Assert.Null(bounces[0].Type);
        Assert.Equal("All", bounces[0].Name);
        Assert.Equal(4, bounces[0].Count);
        Assert.Equal(BounceType.SoftBounce, bounces[1].Type);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_WithChallengeVerification_MapsSupportedBounceType()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": 0,
                                      "Bounces": [
                                        {
                                          "Type": "ChallengeVerification",
                                          "Name": "Challenge verification",
                                          "Count": 1
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(BounceType.ChallengeVerification, Assert.Single(response.Bounces)
            .Type
        );
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_WhenInactiveMailsIsNegative_ReturnsHelpfulFailureWithValue()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": -2,
                                      "Bounces": []
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("InactiveMails returned from the Postmark Bounces API was invalid. Received -2.", error.Message);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_WhenBounceCountIsNegative_ReturnsHelpfulFailureWithValueAndIndex()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": 0,
                                      "Bounces": [
                                        {
                                          "Name": "All",
                                          "Count": -3
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("Delivery stats bounce item 0 could not be mapped: Count returned from the Postmark Bounces API was invalid. Received -3.", error.Message);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_WithUnknownBounceType_ReturnsFailure()
    {
        const string responseJson = """
                                    {
                                      "InactiveMails": 1,
                                      "Bounces": [
                                        {
                                          "Name": "Brand new bounce",
                                          "Count": 1,
                                          "Type": "BrandNewBounce"
                                        }
                                      ]
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/deliverystats"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetDeliveryStatsAsync(CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("Delivery stats bounce item 0 could not be mapped: Bounce type value 'BrandNewBounce' returned from the Postmark Bounces API is not supported.", error.Message);
    }

    [Fact]
    public async Task GetBounceDumpAsync_UsesBounceDumpEndpointAndMapsResponse()
    {
        const string responseJson = """
                                    {
                                      "Body": "X-PM-Message-Id: 5f760f92-8c8a-43a9-a991-afaebb372eff"
                                    }
                                    """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1600045793/dump"] = responseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBounceDumpAsync(1600045793, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("/bounces/1600045793/dump", postmark.LastEndpoint);
        Assert.Contains("5f760f92-8c8a-43a9-a991-afaebb372eff", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ActivateBounceAsync_UsesActivateEndpointAndMapsResponse()
    {
        const string activateResponseJson = """
                                            {
                                              "Message": "OK",
                                              "Bounce": {
                                                "ID": 1599950051,
                                                "Type": "HardBounce",
                                                "TypeCode": 1,
                                                "Name": "Hard bounce",
                                                "Tag": "",
                                                "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                                "ServerID": 18451835,
                                                "MessageStream": "outbound",
                                                "Description": "The server was unable to deliver your message.",
                                                "Details": "smtp;550 mailbox unavailable",
                                                "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                                "From": "from@publi-7.com",
                                                "BouncedAt": "2026-03-11T17:33:38Z",
                                                "DumpAvailable": true,
                                                "Inactive": true,
                                                "CanActivate": true,
                                                "Subject": "PostKit Bounces API probe"
                                              }
                                            }
                                            """;
        const string getResponseJson = """
                                       {
                                         "ID": 1599950051,
                                         "Type": "HardBounce",
                                         "TypeCode": 1,
                                         "Name": "Hard bounce",
                                         "Tag": "",
                                         "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                         "ServerID": 18451835,
                                         "MessageStream": "outbound",
                                         "Description": "The server was unable to deliver your message.",
                                         "Details": "smtp;550 mailbox unavailable",
                                         "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                         "From": "from@publi-7.com",
                                         "BouncedAt": "2026-03-11T17:33:38Z",
                                         "DumpAvailable": true,
                                         "Inactive": false,
                                         "CanActivate": true,
                                         "Subject": "PostKit Bounces API probe",
                                         "Content": "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e"
                                       }
                                       """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = getResponseJson }, new Dictionary<string, string> { ["/bounces/1599950051/activate"] = activateResponseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.ActivateBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(["/bounces/1599950051/activate", "/bounces/1599950051"], postmark.CalledEndpoints);
        Assert.Equal("OK", response.Message);
        Assert.Equal("Bounce", response.Bounce.RecordType);
        Assert.Equal(1599950051, response.Bounce.Id);
        Assert.Equal(BounceType.HardBounce, response.Bounce.Type);
        Assert.False(response.Bounce.Inactive);
    }

    [Fact]
    public async Task ActivateBounceAsync_UsesConfirmedBounceStateFromFollowUpGet()
    {
        const string activateResponseJson = """
                                            {
                                              "Message": "OK",
                                              "Bounce": {
                                                "ID": 1599950051,
                                                "Type": "HardBounce",
                                                "TypeCode": 1,
                                                "Name": "Hard bounce",
                                                "Tag": "",
                                                "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                                "ServerID": 18451835,
                                                "MessageStream": "outbound",
                                                "Description": "The server was unable to deliver your message.",
                                                "Details": "smtp;550 mailbox unavailable",
                                                "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                                "From": "from@publi-7.com",
                                                "BouncedAt": "2026-03-11T17:33:38Z",
                                                "DumpAvailable": true,
                                                "Inactive": true,
                                                "CanActivate": true,
                                                "Subject": "PostKit Bounces API probe"
                                              }
                                            }
                                            """;
        const string getResponseJson = """
                                       {
                                         "ID": 1599950051,
                                         "Type": "HardBounce",
                                         "TypeCode": 1,
                                         "Name": "Hard bounce",
                                         "Tag": "",
                                         "MessageID": "69ce4784-c202-41c6-a1a9-91757022b25e",
                                         "ServerID": 18451835,
                                         "MessageStream": "outbound",
                                         "Description": "The server was unable to deliver your message.",
                                         "Details": "smtp;550 mailbox unavailable",
                                         "Email": "HardBounce@bounce-testing.postmarkapp.com",
                                         "From": "from@publi-7.com",
                                         "BouncedAt": "2026-03-11T17:33:38Z",
                                         "DumpAvailable": true,
                                         "Inactive": false,
                                         "CanActivate": true,
                                         "Subject": "PostKit Bounces API probe",
                                         "Content": "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e"
                                       }
                                       """;

        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/bounces/1599950051"] = getResponseJson }, new Dictionary<string, string> { ["/bounces/1599950051/activate"] = activateResponseJson });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.ActivateBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("OK", response.Message);
        Assert.False(response.Bounce.Inactive);
        Assert.Equal(["/bounces/1599950051/activate", "/bounces/1599950051"], postmark.CalledEndpoints);
    }

    [Fact]
    public async Task ActivateBounceAsync_WhenConfirmationPollingHitsTransientException_RetriesAndReturnsConfirmedBounce()
    {
        var postmark = new TransientConfirmationFailurePostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.ActivateBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("OK", response.Message);
        Assert.False(response.Bounce.Inactive);
        Assert.Equal(["/bounces/1599950051/activate", "/bounces/1599950051", "/bounces/1599950051"], postmark.CalledEndpoints);
    }

    [Fact]
    public async Task ActivateBounceAsync_WhenConfirmationNeverObservesActiveBounce_ReturnsFailure()
    {
        var postmark = new NeverConfirmedBouncePostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.ActivateBounceAsync(1599950051, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Contains("could not be confirmed as active", error.Message, StringComparison.Ordinal);
        Assert.Equal(31, postmark.CalledEndpoints.Count);
    }

    [Fact]
    public async Task GetBouncesAsync_WithInvalidCount_Fails()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 0 }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query count must be between 1 and 500. Received 0.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WithInvalidOffset_FailsWithActualValue()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10, Offset = -1 }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query offset must be zero or greater. Received -1.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WhenCountAndOffsetExceedSearchWindow_Fails()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 500, Offset = 9800 }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query count and offset cannot exceed 10000 when combined. Count: 500; offset: 9800; combined: 10300.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WithWhitespaceTagFilter_FailsWithOmitGuidance()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10, Tag = "\t " }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query tag filter cannot be empty or whitespace. Set Tag to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WithWhitespaceMessageStreamFilter_FailsWithOmitGuidance()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10, MessageStream = "  " }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query message stream filter cannot be empty or whitespace. Set MessageStream to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task BounceOperations_WithInvalidId_FailWithActualValue()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var bounce = await client.GetBounceAsync(0, CancellationToken.None);
        var dump = await client.GetBounceDumpAsync(-1, CancellationToken.None);
        var activation = await client.ActivateBounceAsync(0, CancellationToken.None);

        Assert.True(bounce.IsFailure(out var bounceError, out var _), bounce.ToString());
        Assert.Equal("The bounce ID must be greater than zero. Received 0.", bounceError.Message);
        Assert.True(dump.IsFailure(out var dumpError, out var _), dump.ToString());
        Assert.Equal("The bounce ID must be greater than zero. Received -1.", dumpError.Message);
        Assert.True(activation.IsFailure(out var activationError, out var _), activation.ToString());
        Assert.Equal("The bounce ID must be greater than zero. Received 0.", activationError.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    [Fact]
    public async Task GetBouncesAsync_WithInvalidMessageStreamFilter_FailsBeforeCallingApi()
    {
        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.GetBouncesAsync(new BounceQuery { Count = 10, MessageStream = "_invalid" }, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());
        Assert.Equal("The bounce query message stream filter must be 1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'. First character must be a lowercase letter. Received '_' at index 0.", error.Message);
        Assert.Null(postmark.LastEndpoint);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? getResponses = null, Dictionary<string, string>? putResponses = null) : IPostmarkClient
    {
        public List<string> CalledEndpoints { get; } = [];

        public string? LastEndpoint { get; private set; }
        private readonly Dictionary<string, string> _getResponses = getResponses ?? [];
        private readonly Dictionary<string, string> _putResponses = putResponses ?? [];

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            CalledEndpoints.Add(endpoint);
            return Task.FromResult(Result.Success(Deserialize<TResponse>(_getResponses, endpoint)));
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            CalledEndpoints.Add(endpoint);
            return Task.FromResult(Result.Success(Deserialize<TResponse>(_putResponses, endpoint)));
        }

        private static TResponse Deserialize<TResponse>(IReadOnlyDictionary<string, string> responses, string endpoint)
        {
            if (!responses.TryGetValue(endpoint, out var responseJson))
                throw new InvalidOperationException($"No response JSON was configured for endpoint '{endpoint}'.");

            var value = JsonSerializer.Deserialize<TResponse>(responseJson);
            return value ?? throw new InvalidOperationException("Response JSON could not be deserialized.");
        }
    }

    private sealed class TransientConfirmationFailurePostmarkClient : IPostmarkClient
    {
        public List<string> CalledEndpoints { get; } = [];
        private int _getAttempts;

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            _getAttempts++;
            if (_getAttempts == 1)
                throw new InvalidOperationException("Transient confirmation failure.");

            var bounce = new BounceResponse
            {
                Id = 1599950051,
                Type = "HardBounce",
                Name = "Hard bounce",
                Tag = "",
                MessageId = "69ce4784-c202-41c6-a1a9-91757022b25e",
                ServerId = 18451835,
                MessageStream = "outbound",
                Description = "The server was unable to deliver your message.",
                Details = "smtp;550 mailbox unavailable",
                Email = "HardBounce@bounce-testing.postmarkapp.com",
                From = "from@publi-7.com",
                BouncedAt = DateTimeOffset.Parse("2026-03-11T17:33:38Z"),
                DumpAvailable = true,
                Inactive = false,
                CanActivate = true,
                Subject = "PostKit Bounces API probe",
                Content = "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e",
            };

            return Task.FromResult(Result.Success((TResponse)(object)bounce));
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var activation = new ActivateBounceResponse
            {
                Message = "OK",
                Bounce = new BounceResponse
                {
                    Id = 1599950051,
                    Type = "HardBounce",
                    Name = "Hard bounce",
                    Tag = "",
                    MessageId = "69ce4784-c202-41c6-a1a9-91757022b25e",
                    ServerId = 18451835,
                    MessageStream = "outbound",
                    Description = "The server was unable to deliver your message.",
                    Details = "smtp;550 mailbox unavailable",
                    Email = "HardBounce@bounce-testing.postmarkapp.com",
                    From = "from@publi-7.com",
                    BouncedAt = DateTimeOffset.Parse("2026-03-11T17:33:38Z"),
                    DumpAvailable = true,
                    Inactive = true,
                    CanActivate = true,
                    Subject = "PostKit Bounces API probe",
                },
            };

            return Task.FromResult(Result.Success((TResponse)(object)activation));
        }
    }

    private sealed class NeverConfirmedBouncePostmarkClient : IPostmarkClient
    {
        public List<string> CalledEndpoints { get; } = [];

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var bounce = new BounceResponse
            {
                Id = 1599950051,
                Type = "HardBounce",
                Name = "Hard bounce",
                Tag = "",
                MessageId = "69ce4784-c202-41c6-a1a9-91757022b25e",
                ServerId = 18451835,
                MessageStream = "outbound",
                Description = "The server was unable to deliver your message.",
                Details = "smtp;550 mailbox unavailable",
                Email = "HardBounce@bounce-testing.postmarkapp.com",
                From = "from@publi-7.com",
                BouncedAt = DateTimeOffset.Parse("2026-03-11T17:33:38Z"),
                DumpAvailable = true,
                Inactive = true,
                CanActivate = true,
                Subject = "PostKit Bounces API probe",
                Content = "X-PM-Message-Id: 69ce4784-c202-41c6-a1a9-91757022b25e",
            };

            return Task.FromResult(Result.Success((TResponse)(object)bounce));
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            CalledEndpoints.Add(endpoint);

            var activation = new ActivateBounceResponse
            {
                Message = "OK",
                Bounce = new BounceResponse
                {
                    Id = 1599950051,
                    Type = "HardBounce",
                    Name = "Hard bounce",
                    Tag = "",
                    MessageId = "69ce4784-c202-41c6-a1a9-91757022b25e",
                    ServerId = 18451835,
                    MessageStream = "outbound",
                    Description = "The server was unable to deliver your message.",
                    Details = "smtp;550 mailbox unavailable",
                    Email = "HardBounce@bounce-testing.postmarkapp.com",
                    From = "from@publi-7.com",
                    BouncedAt = DateTimeOffset.Parse("2026-03-11T17:33:38Z"),
                    DumpAvailable = true,
                    Inactive = true,
                    CanActivate = true,
                    Subject = "PostKit Bounces API probe",
                },
            };

            return Task.FromResult(Result.Success((TResponse)(object)activation));
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

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class BounceResponseTestsCollection
{
    public const string Name = nameof(BounceResponseTestsCollection);
}
