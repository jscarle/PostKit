using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostKit.Emails;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class PostKitHttpClientResilienceTests
{
    [Fact]
    public async Task SendEmailAsync_WithInheritedUnsafeRetryHandler_UsesPostKitSafeDefault()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token" })
            .Build();
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ErrorCode":0,"Message":"OK","MessageID":"7ce76f50-17f1-4c88-ae1f-9e4f72fcdcc3","SubmittedAt":"2026-08-08T12:00:00Z","To":"recipient@postkit.com"}""", Encoding.UTF8,
                    MediaTypeNames.Application.Json)
            });
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureHttpClientDefaults(httpClient => httpClient.AddStandardResilienceHandler());
        services.AddPostKit(configuration);
        services.ConfigurePostKitHttpClient(httpClient => httpClient.ConfigurePrimaryHttpMessageHandler(() => handler));

        await using var serviceProvider = services.BuildServiceProvider();
        var postKitClient = serviceProvider.GetRequiredService<IPostKitClient>();
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Retry safety")
            .TextBody("This request must not be repeated.")
            .Build();

        var result = await postKitClient.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var httpError = Assert.IsType<HttpError>(error);
        Assert.Equal(HttpStatusCode.InternalServerError, httpError.StatusCode);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetAsync_WithTooManyRequests_LeavesRetryToPostKitRateLimiter()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token" })
            .Build();
        var delays = new List<TimeSpan>();
        var rateLimiter = new PostmarkRateLimiter((delay, _) =>
        {
            delays.Add(delay);
            return Task.CompletedTask;
        });
        var handler = new SequenceHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json) });
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(rateLimiter);
        services.AddPostKit(configuration);
        services.ConfigurePostKitHttpClient(httpClient => httpClient.ConfigurePrimaryHttpMessageHandler(() => handler));

        await using var serviceProvider = services.BuildServiceProvider();
        var postmarkClient = serviceProvider.GetRequiredService<IPostmarkClient>();

        var result = await postmarkClient.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/server", CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Equal(2, handler.RequestCount);
        var delay = Assert.Single(delays);
        Assert.InRange(delay, TimeSpan.FromMilliseconds(800), TimeSpan.FromMilliseconds(1200));
    }

    [Fact]
    public async Task SendEmailAsync_WithCallerReplacementPipeline_UsesCallerConfiguration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token" })
            .Build();
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ErrorCode":0,"Message":"OK","MessageID":"7ce76f50-17f1-4c88-ae1f-9e4f72fcdcc3","SubmittedAt":"2026-08-08T12:00:00Z","To":"recipient@postkit.com"}""", Encoding.UTF8,
                    MediaTypeNames.Application.Json)
            });
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);
        services.ConfigurePostKitHttpClient(httpClient =>
        {
#pragma warning disable EXTEXP0001 // The caller deliberately replaces PostKit's default resilience pipeline.
            httpClient.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001
            httpClient.AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 1;
                options.Retry.Delay = TimeSpan.Zero;
                options.Retry.UseJitter = false;
                options.Retry.ShouldHandle = arguments => ValueTask.FromResult(arguments.Outcome.Result?.StatusCode == HttpStatusCode.InternalServerError);
            });
            httpClient.ConfigurePrimaryHttpMessageHandler(() => handler);
        });

        await using var serviceProvider = services.BuildServiceProvider();
        var postKitClient = serviceProvider.GetRequiredService<IPostKitClient>();
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Caller retry behavior")
            .TextBody("The caller deliberately enabled this retry.")
            .Build();

        var result = await postKitClient.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Equal(2, handler.RequestCount);
    }

    private sealed class SequenceHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            if (!_responses.TryDequeue(out var response))
                throw new InvalidOperationException("No response was queued for the Postmark request.");

            return Task.FromResult(response);
        }
    }
}
