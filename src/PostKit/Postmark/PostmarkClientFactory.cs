using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;

namespace PostKit.Postmark;

[UsedImplicitly]
internal sealed class PostmarkClientFactory(IHttpClientFactory httpClientFactory, IOptions<PostKitOptions> defaultOptions, IOptionsMonitor<PostKitOptions> namedOptions, ILogger<PostmarkClient> logger) : IPostmarkClientFactory
{
    public PostmarkClient Create(string? key = null)
    {
        var httpClient = httpClientFactory.CreateClient("Postmark");
        var postKitOptions = key is null ? defaultOptions.Value : namedOptions.Get(key);
        return new PostmarkClient(httpClient, Options.Create(postKitOptions), logger);
    }
}
