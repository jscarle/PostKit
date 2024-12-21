using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace PostKit.Configuration;

[UsedImplicitly]
internal sealed class PostKitOptionsSetup(IConfiguration configuration) : IConfigureOptions<PostKitOptions>
{
    public void Configure(PostKitOptions options)
    {
        configuration.GetSection("PostKit")
            .Bind(options);
    }
}
