using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostKit.Stats;

namespace PostKit.Tests;

public class PostKitClientStatsResponseTests
{
    [Fact]
    public async Task GetOutboundStatsOverviewAsync_UsesOverviewEndpointAndMapsResponse()
    {
        const string endpoint = "/stats/outbound?tag=welcome&fromdate=2014-01-01&todate=2014-02-01&messagestream=outbound";
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            [endpoint] = """
                         {
                           "Sent": 615,
                           "Bounced": 64,
                           "SMTPApiErrors": 25,
                           "BounceRate": 10.406,
                           "SpamComplaints": 10,
                           "SpamComplaintsRate": 1.626,
                           "Opens": 166,
                           "UniqueOpens": 26,
                           "Tracked": 111,
                           "WithLinkTracking": 90,
                           "WithOpenTracking": 51,
                           "TotalTrackedLinksSent": 60,
                           "UniqueLinksClicked": 30,
                           "TotalClicks": 72,
                           "WithClientRecorded": 14,
                           "WithPlatformRecorded": 10
                         }
                         """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundStatsOverviewAsync(new OutboundStatsQuery
        {
            Tag = "welcome",
            FromDate = new DateOnly(2014, 1, 1),
            ToDate = new DateOnly(2014, 2, 1),
            MessageStreamId = "outbound"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var stats), result.ToString());
        Assert.Equal(endpoint, postmark.LastGetEndpoint);
        Assert.Equal(615, stats.Sent);
        Assert.Equal(64, stats.Bounced);
        Assert.Equal(25, stats.SmtpApiErrors);
        Assert.Equal(10.406, stats.BounceRate);
        Assert.Equal(10, stats.SpamComplaints);
        Assert.Equal(1.626, stats.SpamComplaintsRate);
        Assert.Equal(166, stats.Opens);
        Assert.Equal(26, stats.UniqueOpens);
        Assert.Equal(72, stats.TotalClicks);
        Assert.Equal(30, stats.UniqueLinksClicked);
        Assert.Equal(60, stats.TotalTrackedLinksSent);
        Assert.Equal(111, stats.Tracked);
        Assert.Equal(90, stats.WithLinkTracking);
        Assert.Equal(51, stats.WithOpenTracking);
        Assert.Equal(14, stats.WithClientRecorded);
        Assert.Equal(10, stats.WithPlatformRecorded);
    }

    [Fact]
    public async Task OutboundStatsMethods_UseDocumentedEndpoints()
    {
        var responses = new Dictionary<string, string>
        {
            ["/stats/outbound/sends"] = """{ "Days": [], "Sent": 0 }""",
            ["/stats/outbound/bounces"] = """{ "Days": [], "HardBounce": 0, "SMTPApiError": 0, "SoftBounce": 0, "Transient": 0 }""",
            ["/stats/outbound/spam"] = """{ "Days": [], "SpamComplaint": 0 }""",
            ["/stats/outbound/tracked"] = """{ "Days": [], "Tracked": 0 }""",
            ["/stats/outbound/opens"] = """{ "Days": [], "Opens": 0, "Unique": 0 }""",
            ["/stats/outbound/opens/platforms"] = """{ "Days": [], "Desktop": 0, "Mobile": 0, "Unknown": 0, "WebMail": 0 }""",
            ["/stats/outbound/opens/emailclients"] = """{ "Days": [], "Apple Mail": 0 }""",
            ["/stats/outbound/clicks"] = """{ "Days": [], "Clicks": 0, "Unique": 0 }""",
            ["/stats/outbound/clicks/browserfamilies"] = """{ "Days": [], "Google Chrome": 0 }""",
            ["/stats/outbound/clicks/platforms"] = """{ "Days": [], "Desktop": 0, "Mobile": 0, "Unknown": 0 }""",
            ["/stats/outbound/clicks/location"] = """{ "Days": [], "HTML": 0, "Text": 0 }"""
        };
        var postmark = new RecordingPostmarkClient(responses);
        var client = new PostKitClient(postmark, new TestLogger());

        Assert.True((await client.GetOutboundSentStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/sends", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundBounceStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/bounces", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundSpamComplaintStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/spam", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundTrackedEmailStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/tracked", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundOpenStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/opens", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundEmailPlatformStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/opens/platforms", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundEmailClientStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/opens/emailclients", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundClickStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/clicks", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundClickBrowserStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/clicks/browserfamilies", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundClickPlatformStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/clicks/platforms", postmark.LastGetEndpoint);

        Assert.True((await client.GetOutboundClickLocationStatsAsync(cancellationToken: TestContext.Current.CancellationToken)).IsSuccess(out _));
        Assert.Equal("/stats/outbound/clicks/location", postmark.LastGetEndpoint);
    }

    [Fact]
    public async Task GetOutboundBounceStatsAsync_MapsMissingDailyFieldsAsZero()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/stats/outbound/bounces"] = """
                                          {
                                            "Days": [
                                              { "Date": "2014-01-01", "HardBounce": 12, "SoftBounce": 36 },
                                              { "Date": "2014-01-03", "Transient": 7 }
                                            ],
                                            "HardBounce": 12,
                                            "SMTPApiError": 0,
                                            "SoftBounce": 36,
                                            "Transient": 7
                                          }
                                          """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundBounceStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var stats), result.ToString());
        Assert.Equal(12, stats.HardBounce);
        Assert.Equal(0, stats.SmtpApiError);
        Assert.Equal(36, stats.SoftBounce);
        Assert.Equal(7, stats.Transient);
        Assert.Equal(new DateOnly(2014, 1, 1), stats.Days[0].Date);
        Assert.Equal(12, stats.Days[0].HardBounce);
        Assert.Equal(0, stats.Days[0].SmtpApiError);
        Assert.Equal(36, stats.Days[0].SoftBounce);
        Assert.Equal(0, stats.Days[0].Transient);
        Assert.Equal(7, stats.Days[1].Transient);
    }

    [Fact]
    public async Task GetOutboundEmailClientStatsAsync_MapsDynamicUsageNames()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/stats/outbound/opens/emailclients"] = """
                                                     {
                                                       "Days": [
                                                         { "Date": "2014-01-01", "Apple Mail": 1, "Outlook 2010": 1 }
                                                       ],
                                                       "Apple Mail": 6,
                                                       "Outlook 2010": 8
                                                     }
                                                     """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundEmailClientStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var stats), result.ToString());
        Assert.Equal(6, stats.Totals["Apple Mail"]);
        Assert.Equal(8, stats.Totals["Outlook 2010"]);
        var day = Assert.Single(stats.Days);
        Assert.Equal(new DateOnly(2014, 1, 1), day.Date);
        Assert.Equal(1, day.Counts["Apple Mail"]);
        Assert.Equal(1, day.Counts["Outlook 2010"]);
    }

    [Fact]
    public async Task GetOutboundClickLocationStatsAsync_MapsHtmlAndTextCounts()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/stats/outbound/clicks/location"] = """
                                                  {
                                                    "Days": [
                                                      { "Date": "2014-01-01", "HTML": 1 },
                                                      { "Date": "2014-01-02", "Text": 2 }
                                                    ],
                                                    "HTML": 4,
                                                    "Text": 4
                                                  }
                                                  """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundClickLocationStatsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var stats), result.ToString());
        Assert.Equal(4, stats.Html);
        Assert.Equal(4, stats.Text);
        Assert.Equal(1, stats.Days[0].Html);
        Assert.Equal(0, stats.Days[0].Text);
        Assert.Equal(0, stats.Days[1].Html);
        Assert.Equal(2, stats.Days[1].Text);
    }

    [Fact]
    public async Task GetOutboundSentStatsAsync_WithWhitespaceTag_FailsBeforeApiCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetOutboundSentStatsAsync(new OutboundStatsQuery { Tag = "\t " }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The outbound stats query tag filter cannot be empty or whitespace. Set Tag to null to omit this filter. Actual length: 2.", error.Message);
        Assert.Null(postmark.LastGetEndpoint);
    }

    private sealed class RecordingPostmarkClient(Dictionary<string, string>? getResponses = null) : IPostmarkClient
    {
        public string? LastGetEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("PostAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastGetEndpoint = endpoint;
            if (getResponses is null || !getResponses.TryGetValue(endpoint, out var responseJson))
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
