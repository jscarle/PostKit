using Microsoft.Extensions.DependencyInjection;
using PostKit.Configuration;
using PostKit.Postmark;

namespace PostKit;

public static class PostKitExtensions
{
    public static void AddPostKit(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.ConfigureOptions<PostKitOptionsSetup>();

        services.AddTransient<IPostmarkClient, PostmarkClient>();

        services.AddTransient<IPostKitClient, PostKitClient>();
    }
}
