using Microsoft.Extensions.DependencyInjection;
using PostKit.Configuration;
using PostKit.Postmark;

namespace PostKit;

/// <summary>
/// Provides dependency injection helpers for registering PostKit services.
/// </summary>
public static class PostKitExtensions
{
    /// <summary>
    /// Registers the services required to send email through PostKit.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static void AddPostKit(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.ConfigureOptions<PostKitOptionsSetup>();

        services.AddTransient<IPostmarkClient, PostmarkClient>();

        services.AddTransient<IPostKitClient, PostKitClient>();
    }
}
