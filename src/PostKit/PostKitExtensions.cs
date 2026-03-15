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
    /// <summary>
    /// Registers the default (non-keyed) PostKit services using configuration from the <c>PostKit</c> section.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance so calls can be chained.</returns>
    public static IServiceCollection AddPostKit(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient("Postmark");

        services.AddOptions<PostKitOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection("PostKit").Bind(options));

        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

        services.TryAddTransient<IPostmarkClient>(sp =>
            sp.GetRequiredService<IPostmarkClientFactory>().Create());

        services.TryAddTransient<IPostKitClient, PostKitClient>();

        return services;
    }

    /// <summary>
    /// Registers keyed PostKit services using configuration from <c>PostKit:{configurationKey}</c>.
    /// If <paramref name="configurationKey"/> is omitted, the value is determined from <paramref name="serviceKey"/>
    /// by first using it directly when it is a <see cref="string"/>, otherwise by calling <c>ToString()</c>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="serviceKey">The key used to resolve the keyed services.</param>
    /// <param name="configurationKey">
    /// The configuration key to bind options from. When omitted, the value is inferred from <paramref name="serviceKey"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="serviceKey"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a configuration key cannot be determined.
    /// </exception>
    /// <returns>The same <paramref name="services"/> instance so calls can be chained.</returns>
    public static IServiceCollection AddKeyedPostKit(
        this IServiceCollection services,
        object serviceKey,
        string? configurationKey = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceKey);

        services.AddHttpClient("Postmark");

        var namedOptionsKey = configurationKey;
        if (string.IsNullOrWhiteSpace(namedOptionsKey))
        {
            namedOptionsKey = serviceKey is string keyString
                ? keyString
                : serviceKey.ToString();
        }

        if (string.IsNullOrWhiteSpace(namedOptionsKey))
        {
            throw new InvalidOperationException(
                "Cannot determine the configuration key using the specified service key. Please specify a configuration key.");
        }

        services.AddOptions<PostKitOptions>(namedOptionsKey)
            .Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection("PostKit")
                    .GetSection(namedOptionsKey)
                    .Bind(options));

        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

        services.RemoveAllKeyed<IPostmarkClient>(serviceKey);
        services.RemoveAllKeyed<IPostKitClient>(serviceKey);

        services.AddKeyedTransient<IPostmarkClient>(serviceKey, (sp, _) =>
            sp.GetRequiredService<IPostmarkClientFactory>().Create(namedOptionsKey));

        services.AddKeyedTransient<IPostKitClient>(serviceKey, (sp, _) =>
            new PostKitClient(
                sp.GetRequiredKeyedService<IPostmarkClient>(serviceKey),
                sp.GetRequiredService<ILogger<PostKitClient>>()));

        return services;
    }
}