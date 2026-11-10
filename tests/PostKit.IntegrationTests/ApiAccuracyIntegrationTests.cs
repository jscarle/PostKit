using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostKit.Stats;
using PostKit.Templates;
using PostKit.Webhooks;

namespace PostKit.IntegrationTests;

/// <summary>Opt-in, server-token-only contract checks against an explicitly identified sandbox.</summary>
public sealed class ApiAccuracyIntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    private HttpClient? _httpClient;
    private ServiceProvider? _provider;
    private IPostKitClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("POSTKIT_RUN_LIVE_ACCURACY_TESTS"), "1", StringComparison.Ordinal))
            Assert.Skip("Set POSTKIT_RUN_LIVE_ACCURACY_TESTS=1 to run these explicit sandbox checks.");

        var token = RequireSetting("POSTKIT_LIVE_SERVER_TOKEN");
        var expectedName = RequireSetting("POSTKIT_LIVE_AUTHORIZED_SERVER_NAME");
        var expectedId = long.Parse(RequireSetting("POSTKIT_LIVE_AUTHORIZED_SERVER_ID"), CultureInfo.InvariantCulture);
        _httpClient = new HttpClient { BaseAddress = new Uri("https://api.postmarkapp.com/"), Timeout = TimeSpan.FromSeconds(30) };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("X-Postmark-Server-Token", token);

        using var response = await _httpClient.GetAsync("server", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var identity = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(expectedId, identity.RootElement.GetProperty("ID").GetInt64());
        Assert.Equal(expectedName, identity.RootElement.GetProperty("Name").GetString());
        Assert.Equal("Sandbox", identity.RootElement.GetProperty("DeliveryType").GetString());

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = token }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);
        _provider = services.BuildServiceProvider();
        _client = _provider.GetRequiredService<IPostKitClient>();
        output.WriteLine("Authorized sandbox identity and delivery type verified.");
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        if (_provider is not null)
            await _provider.DisposeAsync();
    }

    [Fact]
    public async Task CurrentContracts_WithDisposableResources_MapAndCleanUp()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var marker = "postkit-accuracy-" + Guid.NewGuid().ToString("N");
        var templateAlias = marker + "-layout";
        var callback = new UriBuilder(RequireSetting("POSTKIT_LIVE_WEBHOOK_URL"));
        callback.Query = string.IsNullOrEmpty(callback.Query) ? $"postkit-test={marker}" : callback.Query.TrimStart('?') + $"&postkit-test={marker}";
        var callbackUrls = new HashSet<string>(StringComparer.Ordinal) { callback.Uri.AbsoluteUri };
        var createdWebhookIds = new HashSet<long>();
        var createdTemplateIds = new HashSet<long>();
        var beforeHooksResult = await _client.ListWebhooksAsync(cancellationToken);
        Assert.True(beforeHooksResult.IsSuccess(out var beforeHooks), beforeHooksResult.ToString());
        var beforeTemplatesResult = await _client.ListTemplatesAsync(cancellationToken: cancellationToken);
        Assert.True(beforeTemplatesResult.IsSuccess(out var beforeTemplates), beforeTemplatesResult.ToString());
        var originalHooks = beforeHooks.Webhooks.Select(static webhook => webhook.Id).ToHashSet();
        var originalTemplates = beforeTemplates.Templates.Select(static template => template.TemplateId).ToHashSet();

        try
        {
            var query = new OutboundStatsQuery { FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), ToDate = DateOnly.FromDateTime(DateTime.UtcNow) };
            var overviewResult = await _client.GetOutboundStatsOverviewAsync(query, cancellationToken);
            Assert.True(overviewResult.IsSuccess(out var overview), overviewResult.ToString());
            Assert.InRange(overview.WithReadTimeRecorded, 0, int.MaxValue);
            var sent = await _client.GetOutboundSentStatsAsync(query, cancellationToken);
            Assert.True(sent.IsSuccess(), sent.ToString());
            var bounces = await _client.GetOutboundBounceStatsAsync(query, cancellationToken);
            Assert.True(bounces.IsSuccess(), bounces.ToString());
            var spam = await _client.GetOutboundSpamComplaintStatsAsync(query, cancellationToken);
            Assert.True(spam.IsSuccess(), spam.ToString());
            var tracked = await _client.GetOutboundTrackedEmailStatsAsync(query, cancellationToken);
            Assert.True(tracked.IsSuccess(), tracked.ToString());
            var opens = await _client.GetOutboundOpenStatsAsync(query, cancellationToken);
            Assert.True(opens.IsSuccess(), opens.ToString());
            var platforms = await _client.GetOutboundEmailPlatformStatsAsync(query, cancellationToken);
            Assert.True(platforms.IsSuccess(), platforms.ToString());
            var clients = await _client.GetOutboundEmailClientStatsAsync(query, cancellationToken);
            Assert.True(clients.IsSuccess(), clients.ToString());
            var clicks = await _client.GetOutboundClickStatsAsync(query, cancellationToken);
            Assert.True(clicks.IsSuccess(), clicks.ToString());
            var browsers = await _client.GetOutboundClickBrowserStatsAsync(query, cancellationToken);
            Assert.True(browsers.IsSuccess(), browsers.ToString());
            var clickPlatforms = await _client.GetOutboundClickPlatformStatsAsync(query, cancellationToken);
            Assert.True(clickPlatforms.IsSuccess(), clickPlatforms.ToString());
            var locations = await _client.GetOutboundClickLocationStatsAsync(query, cancellationToken);
            Assert.True(locations.IsSuccess(), locations.ToString());
            var readingTimes = await _client.GetOutboundReadTimeStatsAsync(query, cancellationToken);
            Assert.True(readingTimes.IsSuccess(), readingTimes.ToString());
            output.WriteLine("All 13 statistics operations mapped current responses.");

            var messagesResult = await _client.SearchOutboundMessagesAsync("outbound", count: 10, cancellationToken: cancellationToken);
            Assert.True(messagesResult.IsSuccess(out var messages), messagesResult.ToString());
            var message = messages.Messages.FirstOrDefault();
            if (message is not null)
            {
                var details = await _client.GetOutboundMessageDetailsAsync(message.MessageId, cancellationToken);
                Assert.True(details.IsSuccess(), details.ToString());
                output.WriteLine("Current outbound search and details mapped successfully.");
            }

            var bulkPageResult = await _client.ListBulkEmailRequestsAsync(1, cancellationToken: cancellationToken);
            Assert.True(bulkPageResult.IsSuccess(out var bulkPage), bulkPageResult.ToString());
            foreach (var request in bulkPage.Requests)
            {
                var statusResult = await _client.GetBulkEmailStatusAsync(request.Id, cancellationToken);
                Assert.True(statusResult.IsSuccess(out var status), statusResult.ToString());
                Assert.Equal(request.Id, status.Id);
                Assert.Equal(request.ReleasedCount, status.ReleasedCount);
                Assert.Equal(request.FailedCount, status.FailedCount);
            }
            if (bulkPage.PaginationKey is not null)
            {
                var nextResult = await _client.ListBulkEmailRequestsAsync(1, bulkPage.PaginationKey, cancellationToken);
                Assert.True(nextResult.IsSuccess(out var next), nextResult.ToString());
                Assert.DoesNotContain(next.Requests, item => bulkPage.Requests.Any(first => first.Id == item.Id));
                output.WriteLine("Existing bulk jobs mapped, including opaque cursor pagination.");
            }

            var createResult = await _client.CreateWebhookAsync(new WebhookCreateParameters
            {
                Url = callback.Uri.AbsoluteUri,
                Verify = false,
                Triggers = new WebhookTriggers { Delivery = new WebhookBasicTrigger { Enabled = true }, Open = new WebhookOpenTrigger { Enabled = true, PostFirstOpenOnly = true } }
            }, cancellationToken);
            Assert.True(createResult.IsSuccess(out var webhook), createResult.ToString());
            createdWebhookIds.Add(webhook.Id);

            var auth = new WebhookHttpAuth { Username = marker, Password = "synthetic-" + marker };
            var setupResult = await _client.EditWebhookAsync(webhook.Id, new WebhookEditParameters
            {
                Verify = false, HttpAuth = auth, HttpHeaders = [new WebhookHeader { Name = "X-PostKit-Contract", Value = marker }]
            }, cancellationToken);
            Assert.True(setupResult.IsSuccess(), setupResult.ToString());
            var verifyResult = await _client.VerifyWebhookAsync(webhook.Id, cancellationToken);
            Assert.True(verifyResult.IsSuccess(out var verification), verifyResult.ToString());
            Assert.True(verification.Success, verification.Message);
            Assert.Contains(verification.Results, static result => result.TriggerType == WebhookTriggerType.Delivery);
            Assert.All(verification.Results, static result => Assert.True(result.Success, result.Message));

            var editResult = await _client.EditWebhookAsync(webhook.Id, new WebhookEditParameters { HttpHeaders = [] }, cancellationToken);
            Assert.True(editResult.IsSuccess(out var edited), editResult.ToString());
            Assert.Equal(auth.Username, edited.HttpAuth!.Username);
            Assert.Empty(edited.HttpHeaders);
            Assert.True(edited.Triggers.Delivery!.Enabled);
            Assert.True(edited.Triggers.Open!.PostFirstOpenOnly);
            var statistics = await _client.GetWebhookStatisticsAsync(webhook.Id, cancellationToken);
            Assert.True(statistics.IsSuccess(), statistics.ToString());
            output.WriteLine("Webhook creation, authenticated partial editing, verification, and typed statistics mapped.");

            var layoutResult = await _client.CreateTemplateAsync(new TemplateCreateParameters
            {
                Name = marker, Alias = templateAlias, TemplateType = TemplateType.Layout, HtmlBody = "<main>{{{ @content }}}</main>"
            }, cancellationToken);
            Assert.True(layoutResult.IsSuccess(out var layout), layoutResult.ToString());
            createdTemplateIds.Add(layout.TemplateId);
            var layoutEdit = await _client.EditTemplateAsync(layout.TemplateId, new TemplateEditParameters
            {
                Name = marker, HtmlBody = "<section>{{{ @content }}}</section>"
            }, cancellationToken);
            Assert.True(layoutEdit.IsSuccess(), layoutEdit.ToString());
            output.WriteLine("Layout template edited successfully without a subject.");
        }
        finally
        {
            // Recover successful creates whose response mapping failed before an ID could be tracked.
            using var hooksResponse = await _httpClient!.GetAsync("webhooks", CancellationToken.None);
            hooksResponse.EnsureSuccessStatusCode();
            using var hooks = JsonDocument.Parse(await hooksResponse.Content.ReadAsStringAsync(CancellationToken.None));
            foreach (var hook in hooks.RootElement.GetProperty("Webhooks").EnumerateArray())
                if (callbackUrls.Contains(hook.GetProperty("Url").GetString()!))
                    createdWebhookIds.Add(hook.GetProperty("ID").GetInt64());
            using var templatesResponse = await _httpClient.GetAsync("templates?count=100&offset=0", CancellationToken.None);
            templatesResponse.EnsureSuccessStatusCode();
            using var templates = JsonDocument.Parse(await templatesResponse.Content.ReadAsStringAsync(CancellationToken.None));
            foreach (var template in templates.RootElement.GetProperty("Templates").EnumerateArray())
                if (string.Equals(template.GetProperty("Alias").GetString(), templateAlias, StringComparison.Ordinal))
                    createdTemplateIds.Add(template.GetProperty("TemplateId").GetInt64());

            foreach (var id in createdWebhookIds)
            {
                Assert.DoesNotContain(id, originalHooks);
                var deletion = await _client.DeleteWebhookAsync(id, CancellationToken.None);
                output.WriteLine(deletion.IsSuccess() ? $"Deleted disposable webhook {id}." : $"Webhook cleanup failed for {id}: {deletion}");
            }
            foreach (var id in createdTemplateIds)
            {
                Assert.DoesNotContain(id, originalTemplates);
                var deletion = await _client.DeleteTemplateAsync(id, CancellationToken.None);
                output.WriteLine(deletion.IsSuccess() ? $"Deleted disposable template {id}." : $"Template cleanup failed for {id}: {deletion}");
            }
            var hooksAfter = await _client.ListWebhooksAsync(CancellationToken.None);
            Assert.True(hooksAfter.IsSuccess(out var remainingHooks), hooksAfter.ToString());
            Assert.DoesNotContain(remainingHooks.Webhooks, item => createdWebhookIds.Contains(item.Id));
            Assert.All(originalHooks, id => Assert.Contains(remainingHooks.Webhooks, item => item.Id == id));
            var templatesAfter = await _client.ListTemplatesAsync(cancellationToken: CancellationToken.None);
            Assert.True(templatesAfter.IsSuccess(out var remainingTemplates), templatesAfter.ToString());
            Assert.DoesNotContain(remainingTemplates.Templates, item => createdTemplateIds.Contains(item.TemplateId));
            Assert.All(originalTemplates, id => Assert.Contains(remainingTemplates.Templates, item => item.TemplateId == id));
            output.WriteLine("Cleanup verified; pre-existing resources preserved.");
        }
    }

    private static string RequireSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
            return value;
        Assert.Skip($"The explicit sandbox fixture requires process-local {name}.");
        return null!;
    }
}
