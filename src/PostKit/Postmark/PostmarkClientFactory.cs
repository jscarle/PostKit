using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace PostKit.Postmark;

[UsedImplicitly]
internal sealed class PostmarkClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<PostKitOptions> defaultOptions,
    IOptionsMonitor<PostKitOptions> namedOptions
#if DEBUG
    ,
    ILogger<PostmarkClient> logger
#endif
) : IPostmarkClientFactory
{
    public PostmarkClient Create(string? key = null)
    {
        var httpClient = httpClientFactory.CreateClient();
        var postKitOptions = key is null ? defaultOptions.Value : namedOptions.Get(key);
        return new PostmarkClient(httpClient, Options.Create(postKitOptions)
#if DEBUG
            , logger
#endif
        );
    }
}
