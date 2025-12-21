using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PostKit.Configuration;
using PostKit.Postmark;

namespace PostKit;

/// <summary>Provides dependency injection helpers for registering PostKit services.</summary>
public static class PostKitExtensions
{
    /// <summary>Registers the services required to send email through PostKit.</summary>
    /// <param name="services">The service collection to configure.</param>
    public static IServiceCollection AddPostKit(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddOptions<PostKitOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("PostKit")
                        .Bind(options);
                }
            );

        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

        services.TryAddTransient(sp => sp.GetRequiredService<IPostmarkClientFactory>()
            .Create()
        );

        services.TryAddTransient<IPostKitClient, PostKitClient>();

        return services;
    }

    /// <summary>Registers a keyed PostmarkClient for a specific key. The named options bind from PostKit:{key}. Missing values can be inherited from the root PostKit section.</summary>
    public static IServiceCollection AddKeyedPostKit(this IServiceCollection services, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must be non-empty.", nameof(key));

        services.AddHttpClient();

        services.AddOptions<PostKitOptions>(key)
            .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("PostKit")
                        .GetSection(key)
                        .Bind(options);
                }
            );

        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

        services.AddKeyedTransient<IPostmarkClient, PostmarkClient>(key, (sp, _) => sp.GetRequiredService<IPostmarkClientFactory>()
            .Create(key)
        );

        services.AddKeyedTransient<IPostKitClient, PostKitClient>(key, (sp, _) =>
            {
                var postmarkClient = sp.GetRequiredKeyedService<IPostmarkClient>(key);
                var logger = sp.GetRequiredService<ILogger<PostKitClient>>();
                return new PostKitClient(postmarkClient, logger);
            }
        );

        return services;
    }
}
