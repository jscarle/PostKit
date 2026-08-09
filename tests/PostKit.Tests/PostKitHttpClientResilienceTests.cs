using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using PostKit.Emails;
using PostKit.Errors;

namespace PostKit.Tests;

public class PostKitHttpClientResilienceTests
{
    [Fact]
    public async Task SendEmailAsync_WithSafePostKitResilienceConfiguration_DoesNotRetryServerError()
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
        services.ConfigurePostKitHttpClient(httpClient =>
        {
#pragma warning disable EXTEXP0001 // The test verifies replacement of inherited resilience handlers for Postmark requests.
            httpClient.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001
            httpClient.AddStandardResilienceHandler(options =>
            {
                options.Retry.DisableForUnsafeHttpMethods();
                var shouldHandle = options.Retry.ShouldHandle;
                options.Retry.ShouldHandle = arguments => arguments.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests
                    ? ValueTask.FromResult(false)
                    : shouldHandle(arguments);
            });
            httpClient.ConfigurePrimaryHttpMessageHandler(() => handler);
        });

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
