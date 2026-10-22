using System.Net;
using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Errors;
using PostKit.Webhooks;

namespace PostKit.IntegrationTests;

/// <summary>Explicitly opted-in live tests for webhook verification, statistics, and bulk request listing.</summary>
public sealed class NewApiIntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    private HttpClient? _serverHttpClient;
    private ServiceProvider? _provider;
    private IPostKitClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("POSTKIT_RUN_LIVE_API_TESTS"), "1", StringComparison.Ordinal))
            Assert.Skip("Set POSTKIT_RUN_LIVE_API_TESTS=1 and the process-local live test credentials to run these tests.");

        var accountToken = RequireLiveSetting("POSTKIT_LIVE_ACCOUNT_TOKEN");
        var serverToken = RequireLiveSetting("POSTKIT_LIVE_SERVER_TOKEN");
        var allowedNames = new HashSet<string>(["PostKit Testing", "PostKit Alternate Testing"], StringComparer.Ordinal);
        // An operator may explicitly authorize a different server for a single process; never infer this from a token alone.
        var authorizedName = Environment.GetEnvironmentVariable("POSTKIT_LIVE_AUTHORIZED_SERVER_NAME");
        if (!string.IsNullOrWhiteSpace(authorizedName))
            allowedNames.Add(authorizedName);

        using var accountClient = new HttpClient();
        accountClient.BaseAddress = new Uri("https://api.postmarkapp.com/");
        accountClient.Timeout = TimeSpan.FromSeconds(30);
        accountClient.DefaultRequestHeaders.Add("Accept", "application/json");
        accountClient.DefaultRequestHeaders.Add("X-Postmark-Account-Token", accountToken);

        long? matchedId = null;
        string? matchedName = null;
        foreach (var name in allowedNames)
        {
            using var response = await accountClient.GetAsync($"servers?count=100&offset=0&name={Uri.EscapeDataString(name)}", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            await using var body = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
            using var document = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
            foreach (var server in document.RootElement.GetProperty("Servers").EnumerateArray())
            {
                if (!string.Equals(server.GetProperty("Name").GetString(), name, StringComparison.Ordinal))
                    continue;
                if (!server.GetProperty("ApiTokens").EnumerateArray().Any(token => string.Equals(token.GetString(), serverToken, StringComparison.Ordinal)))
                    continue;

                matchedId = server.GetProperty("ID").GetInt64();
                matchedName = name;
            }
        }

        Assert.True(matchedId.HasValue, "The supplied server token did not match an explicitly allowed server in the account inventory. No server-level requests were sent.");
        _serverHttpClient = new HttpClient { BaseAddress = new Uri("https://api.postmarkapp.com/"), Timeout = TimeSpan.FromSeconds(30) };
        _serverHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _serverHttpClient.DefaultRequestHeaders.Add("X-Postmark-Server-Token", serverToken);
        using var identityResponse = await _serverHttpClient.GetAsync("server", TestContext.Current.CancellationToken);
        identityResponse.EnsureSuccessStatusCode();
        using var identity = JsonDocument.Parse(await identityResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(matchedId, identity.RootElement.GetProperty("ID").GetInt64());
        Assert.Equal(matchedName, identity.RootElement.GetProperty("Name").GetString());
        output.WriteLine($"Verified allowed server: {matchedName} (ID {matchedId}).");

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = serverToken }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);
        _provider = services.BuildServiceProvider();
        _client = _provider.GetRequiredService<IPostKitClient>();
    }

    public async ValueTask DisposeAsync()
    {
        _serverHttpClient?.Dispose();
        if (_provider is not null)
            await _provider.DisposeAsync();
    }

    [Fact]
    public async Task WebhookEndpoints_WithDisposableWebhooks_VerifyMapAndCleanUp()
    {
        var callbackUrl = RequireLiveSetting("POSTKIT_LIVE_WEBHOOK_URL");
        var callback = new UriBuilder(callbackUrl);
        var marker = "postkit-test=" + Guid.NewGuid().ToString("N");
        callback.Query = string.IsNullOrEmpty(callback.Query) ? marker : callback.Query.TrimStart('?') + "&" + marker;
        var testUrl = callback.Uri.AbsoluteUri;

        var inventoryResult = await _client.ListWebhooksAsync(TestContext.Current.CancellationToken);
        Assert.True(inventoryResult.IsSuccess(out var inventory), inventoryResult.ToString());
        var existingIds = inventory.Webhooks.Select(static webhook => webhook.Id).ToHashSet();
        var createdIds = new List<long>();
        output.WriteLine($"Initial webhook inventory: {existingIds.Count} webhook(s).");

        try
        {
            var createResult = await _client.CreateWebhookAsync(new WebhookCreateParameters
            {
                Url = testUrl,
                Verify = false,
                Triggers = new WebhookTriggers { Delivery = new WebhookBasicTrigger { Enabled = true }, Open = new WebhookOpenTrigger { Enabled = true, PostFirstOpenOnly = false } }
            }, TestContext.Current.CancellationToken);
            Assert.True(createResult.IsSuccess(out var webhook), createResult.ToString());
            createdIds.Add(webhook.Id);
            output.WriteLine($"Created disposable webhook {webhook.Id}; status: {webhook.Status ?? "omitted"}.");

            var unverifiedEdit = await _client.EditWebhookAsync(webhook.Id, new WebhookEditParameters { Verify = false }, TestContext.Current.CancellationToken);
            Assert.True(unverifiedEdit.IsSuccess(out var editedWebhook), unverifiedEdit.ToString());
            Assert.True(editedWebhook.Triggers.Delivery?.Enabled);
            Assert.True(editedWebhook.Triggers.Open?.Enabled);
            output.WriteLine("Verify-only false edit preserved both enabled triggers.");

            var verifyResult = await _client.VerifyWebhookAsync(webhook.Id, TestContext.Current.CancellationToken);
            Assert.True(verifyResult.IsSuccess(out var verification), verifyResult.ToString());
            Assert.True(verification.Success, verification.Message);
            Assert.NotEmpty(verification.Results);
            Assert.All(verification.Results, static trigger => Assert.True(trigger.Success, trigger.Message));
            output.WriteLine($"Verified webhook {webhook.Id}: {verification.Results.Count} passing trigger(s).");

            var editResult = await _client.EditWebhookAsync(webhook.Id, new WebhookEditParameters { Verify = true }, TestContext.Current.CancellationToken);
            Assert.True(editResult.IsSuccess(out _), editResult.ToString());

            var verifiedCreateResult = await _client.CreateWebhookAsync(new WebhookCreateParameters
            {
                Url = testUrl,
                Verify = true,
                Triggers = new WebhookTriggers { Click = new WebhookBasicTrigger { Enabled = true } }
            }, TestContext.Current.CancellationToken);
            Assert.True(verifiedCreateResult.IsSuccess(out var secondWebhook), verifiedCreateResult.ToString());
            createdIds.Add(secondWebhook.Id);
            output.WriteLine($"Created disposable verified webhook {secondWebhook.Id}.");

            var statisticsResult = await _client.GetWebhookStatisticsAsync(webhook.Id, TestContext.Current.CancellationToken);
            Assert.True(statisticsResult.IsSuccess(out var statistics), statisticsResult.ToString());
            Assert.Equal(webhook.Id, statistics.WebhookId);
            Assert.True(statistics.TimeRange.StartTime <= statistics.TimeRange.EndTime);
            Assert.InRange(statistics.Metrics.TotalRequests, 0, long.MaxValue);
            output.WriteLine($"Statistics mapped for webhook {statistics.WebhookId}: {statistics.Metrics.TotalRequests} requests; {statistics.TimeRange.Hours}-hour window.");
        }
        finally
        {
            foreach (var id in createdIds)
            {
                var deletion = await _client.DeleteWebhookAsync(id, CancellationToken.None);
                output.WriteLine(deletion.IsSuccess() ? $"Deleted disposable webhook {id}." : $"Initial cleanup attempt failed for webhook {id}: {deletion}");
            }

            // Recover IDs through the raw inventory as well: a successful create followed by a mapping failure still needs cleanup.
            using var response = await _serverHttpClient!.GetAsync("webhooks", CancellationToken.None);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken.None));
            foreach (var webhook in document.RootElement.GetProperty("Webhooks").EnumerateArray())
            {
                var id = webhook.GetProperty("ID").GetInt64();
                if (existingIds.Contains(id) || !string.Equals(webhook.GetProperty("Url").GetString(), testUrl, StringComparison.Ordinal))
                    continue;
                var deletion = await _client.DeleteWebhookAsync(id, CancellationToken.None);
                output.WriteLine(deletion.IsSuccess() ? $"Deleted recovered disposable webhook {id}." : $"Cleanup failed for webhook {id}: {deletion}");
            }

            var afterResult = await _client.ListWebhooksAsync(CancellationToken.None);
            Assert.True(afterResult.IsSuccess(out var after), afterResult.ToString());
            Assert.DoesNotContain(after.Webhooks, webhook => string.Equals(webhook.Url, testUrl, StringComparison.Ordinal));
            Assert.All(existingIds, id => Assert.Contains(after.Webhooks, webhook => webhook.Id == id));
            output.WriteLine("Cleanup verified; original webhooks remain present.");
        }
    }

    [Fact]
    public async Task BulkRequests_WithApprovedAccess_ListPagesAndMapStatus()
    {
        var firstResult = await _client.ListBulkEmailRequestsAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        var first = RequireBulkAccess(firstResult);
        output.WriteLine($"Bulk listing succeeded: {first.Requests.Count} request(s) on the first page.");

        if (first.PaginationKey is null)
        {
            var from = Environment.GetEnvironmentVariable("POSTKIT_LIVE_FROM_EMAIL");
            var to = Environment.GetEnvironmentVariable("POSTKIT_LIVE_TO_EMAIL");
            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                for (var index = first.Requests.Count; index < 2; index++)
                {
                    var email = BulkEmail.Compose().From(from).Subject("PostKit API compatibility test").TextBody("Synthetic PostKit test message.")
                        .UseMessageStream(MessageStream.Broadcast).AddMessage(BulkEmailMessage.Compose().To(to).Build()).Build();
                    var submission = await _client.SendBulkEmailAsync(email, TestContext.Current.CancellationToken);
                    Assert.True(submission.IsSuccess(out var job), submission.ToString());
                    output.WriteLine($"Submitted one-recipient test bulk job {job.Id}.");
                }
                first = RequireBulkAccess(await _client.ListBulkEmailRequestsAsync(1, cancellationToken: TestContext.Current.CancellationToken));
            }
        }

        if (first.PaginationKey is not null)
        {
            var second = RequireBulkAccess(await _client.ListBulkEmailRequestsAsync(1, first.PaginationKey, TestContext.Current.CancellationToken));
            Assert.DoesNotContain(second.Requests, request => first.Requests.Any(previous => previous.Id == request.Id));
            output.WriteLine($"Cursor pagination succeeded: {second.Requests.Count} request(s) on the second page.");
        }
        else
        {
            output.WriteLine("Live cursor pagination was not exercised: insufficient existing jobs and no configured test sender/recipient.");
        }

        foreach (var request in first.Requests)
        {
            // The list contract permits an absent subject; the existing status method intentionally retains its older contract.
            if (request.Subject is null)
                continue;
            var status = await _client.GetBulkEmailStatusAsync(request.Id, TestContext.Current.CancellationToken);
            Assert.True(status.IsSuccess(out var job), status.ToString());
            Assert.Equal(request.Id, job.Id);
            output.WriteLine($"Mapped bulk status {job.Status}; released={job.ReleasedCount?.ToString() ?? "omitted"}, failed={job.FailedCount?.ToString() ?? "omitted"}.");
        }
    }

    private static string RequireLiveSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
            return value;
        Assert.Skip($"Live API testing requires the process-local {name} setting.");
        return null!;
    }

    private static BulkEmailRequestPage RequireBulkAccess(Result<BulkEmailRequestPage> result)
    {
        if (result.IsSuccess(out var page))
            return page;
        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        if (error is PostmarkError { ErrorCode: PostmarkErrorCode.EndpointApprovalRequired } or HttpError { StatusCode: HttpStatusCode.NotFound })
            Assert.Skip("The approved test server does not have the documented bulk-list endpoint enabled.");
        Assert.Fail(result.ToString());
        return null!;
    }
}
