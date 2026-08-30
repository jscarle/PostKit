using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;

namespace PostKit.Postmark;

[UsedImplicitly]
internal sealed class PostmarkClientFactory(IHttpClientFactory httpClientFactory, IOptions<PostKitOptions> defaultOptions, IOptionsMonitor<PostKitOptions> namedOptions, ILogger<PostmarkClient> logger, PostmarkRateLimiter rateLimiter)
    : IPostmarkClientFactory
{
    public PostmarkClient Create(string? key = null)
    {
        var httpClient = httpClientFactory.CreateClient("Postmark");
        IOptions<PostKitOptions> postKitOptions = key is null ? defaultOptions : new RebasedOptions<PostKitOptions>(namedOptions, key);
        return new PostmarkClient(httpClient, postKitOptions, logger, rateLimiter);
    }
}
