using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Postmark;

namespace PostKit;

/// <summary>Provides dependency injection helpers for registering PostKit services.</summary>
public static class PostKitExtensions
{
    /// <summary>Registers the default (non-keyed) PostKit services.</summary>
    /// <remarks>
    ///     <para>
    ///     This method:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Ensures <see cref="IHttpClientFactory"/> is available by calling <c>AddHttpClient()</c>.</description>
    ///         </item>
    ///         <item>
    ///             <description>Binds <see cref="PostKitOptions"/> from the root <c>PostKit</c> configuration section.</description>
    ///         </item>
    ///         <item>
    ///             <description>Registers <see cref="IPostKitClient"/> and related Postmark services.</description>
    ///         </item>
    ///     </list>
    ///     </para>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance so calls can be chained.</returns>
    public static IServiceCollection AddPostKit(this IServiceCollection services)
    {
        services.AddHttpClient("Postmark");

        services.AddOptions<PostKitOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("PostKit")
                        .Bind(options);
                }
            );

        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

        services.TryAddTransient<IPostmarkClient>(sp => sp.GetRequiredService<IPostmarkClientFactory>()
            .Create()
        );

        services.TryAddTransient<IPostKitClient, PostKitClient>();

        return services;
    }

    /// <summary>Registers PostKit services as either default (non-keyed) services or keyed services for the specified <paramref name="serviceKey"/>.</summary>
    /// <remarks>
    ///     <para>This method always calls <c>AddHttpClient()</c> to ensure <see cref="IHttpClientFactory"/> is available.</para>
    ///     <para>This method binds options from exactly one configuration section (values are not merged with the root <c>PostKit</c> section).</para>
    ///     <para>
    ///     This method behaves as follows:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>If <paramref name="serviceKey"/> is <see langword="null"/> and <paramref name="configurationKey"/> is <see langword="null"/> or whitespace, this method calls <see cref="AddPostKit(IServiceCollection)"/>.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             If <paramref name="serviceKey"/> is <see langword="null"/> and <paramref name="configurationKey"/> is provided, this method registers default (non-keyed) services and binds the default (unnamed)
    ///             <see cref="PostKitOptions"/> from <c>PostKit:{configurationKey}</c>.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             If <paramref name="serviceKey"/> is not <see langword="null"/>, this method registers keyed services and binds named options from <c>PostKit:{namedOptionsKey}</c>, where <c>namedOptionsKey</c> is
    ///             <paramref name="configurationKey"/> when provided; otherwise <c>serviceKey.ToString()</c>.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     </para>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> used to resolve keyed services. If <see langword="null"/>, this method registers non-keyed services.</param>
    /// <param name="configurationKey">The configuration key to bind options from (i.e. <c>PostKit:{configurationKey}</c>). For keyed registrations, if omitted, <c>serviceKey.ToString()</c> is used.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="serviceKey"/> is not <see langword="null"/>, <paramref name="configurationKey"/> is <see langword="null"/>, and <c>serviceKey.ToString()</c> is <see langword="null"/>
    /// or empty (so a configuration key cannot be determined).
    /// </exception>
    /// <returns>The same <paramref name="services"/> instance so calls can be chained.</returns>
    public static IServiceCollection AddKeyedPostKit(this IServiceCollection services, object? serviceKey = null, string? configurationKey = null)
    {
        if (serviceKey is null)
        {
            if (string.IsNullOrWhiteSpace(configurationKey))
                return services.AddPostKit();

            services.AddHttpClient("Postmark");

            services.AddOptions<PostKitOptions>(configurationKey)
                .Configure<IConfiguration>((options, configuration) =>
                    {
                        configuration.GetSection("PostKit")
                            .GetSection(configurationKey)
                            .Bind(options);
                    }
                );

            services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

            services.Replace(ServiceDescriptor.Singleton<IOptions<PostKitOptions>>(sp => Options.Create(
                sp.GetRequiredService<IOptionsMonitor<PostKitOptions>>()
                    .Get(configurationKey)
            )));

            services.Replace(ServiceDescriptor.Transient<IPostmarkClient>(sp => sp.GetRequiredService<IPostmarkClientFactory>()
                .Create()
            ));

            services.Replace(ServiceDescriptor.Transient<IPostKitClient>(sp =>
                {
                    var postmarkClient = sp.GetRequiredService<IPostmarkClient>();
                    var logger = sp.GetRequiredService<ILogger<PostKitClient>>();
                    return new PostKitClient(postmarkClient, logger);
                }
            ));

            return services;
        }

        services.AddHttpClient("Postmark");

        string namedOptionsKey;
        if (configurationKey is not null)
        {
            namedOptionsKey = configurationKey;
        }
        else
        {
            var keyString = serviceKey.ToString();
            if (string.IsNullOrEmpty(keyString))
                throw new InvalidOperationException("Cannot determine the configuration key using the specified service key, please specify a configuration key to bind configuration values from.");
            namedOptionsKey = keyString;
        }

        services.AddOptions<PostKitOptions>(namedOptionsKey)
            .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("PostKit")
                        .GetSection(namedOptionsKey)
                        .Bind(options);
                }
            );

        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();

        services.TryAddKeyedTransient<IPostmarkClient>(serviceKey, (sp, _) => sp.GetRequiredService<IPostmarkClientFactory>()
            .Create(namedOptionsKey)
        );

        services.TryAddKeyedTransient<IPostKitClient>(serviceKey, (sp, _) =>
            {
                var postmarkClient = sp.GetRequiredKeyedService<IPostmarkClient>(serviceKey);
                var logger = sp.GetRequiredService<ILogger<PostKitClient>>();
                return new PostKitClient(postmarkClient, logger);
            }
        );

        return services;
    }
}
