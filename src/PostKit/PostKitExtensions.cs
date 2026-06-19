using JetBrains.Annotations;
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
    private const string ConfigurationSectionName = "PostKit";

    private static readonly string RequiredTokenMessage = $"must define '{nameof(PostKitOptions.ServerApiToken)}' or '{nameof(PostKitOptions.AccountApiToken)}'.";

    /// <summary>Registers the default (non-keyed) PostKit services using configuration from the <c>PostKit</c> section.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services" /> instance so calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddPostKit(this IServiceCollection services)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "The service collection cannot be null.");

        return AddDefaultPostKitRegistration(services, ConfigurationSectionName, true, optionsBuilder => optionsBuilder.Configure<IConfiguration>((options, configuration) => configuration.GetSection(ConfigurationSectionName)
            .Bind(options)));
    }

    /// <summary>Registers the default (non-keyed) PostKit services using configuration from the <c>PostKit</c> section of the provided configuration root.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration root that contains the <c>PostKit</c> section.</param>
    /// <returns>The same <paramref name="services" /> instance so calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddPostKit(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "The service collection cannot be null.");

        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration), "The configuration root cannot be null.");

        var configurationSection = configuration.GetSection(ConfigurationSectionName);
        EnsureConfigurationSectionExists(configurationSection);

        return services.AddPostKit(configurationSection);
    }

    /// <summary>Registers the default (non-keyed) PostKit services using the provided configuration section.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configurationSection">The configuration section to bind options from.</param>
    /// <exception cref="InvalidOperationException">Thrown when the provided configuration section cannot be found.</exception>
    /// <returns>The same <paramref name="services" /> instance so calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddPostKit(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "The service collection cannot be null.");

        if (configurationSection is null)
            throw new ArgumentNullException(nameof(configurationSection), "The configuration section cannot be null.");

        EnsureConfigurationSectionExists(configurationSection);

        return AddDefaultPostKitRegistration(services, GetOptionsName(configurationSection), false, optionsBuilder => optionsBuilder.Configure(configurationSection.Bind));
    }

    /// <summary>Registers PostKit services either as default (non-keyed) services or as keyed services.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="serviceKey">The key used to resolve keyed services. When <see langword="null" />, the default service registrations are rebound.</param>
    /// <param name="configurationKey">The configuration key to bind options from. When <paramref name="serviceKey" /> is provided and this value is omitted, the key is inferred from <paramref name="serviceKey" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a keyed registration cannot determine the configuration key.</exception>
    /// <returns>The same <paramref name="services" /> instance so calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddKeyedPostKit(this IServiceCollection services, object? serviceKey = null, string? configurationKey = null)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "The service collection cannot be null.");

        if (serviceKey is null)
        {
            if (string.IsNullOrWhiteSpace(configurationKey))
                return services.AddPostKit();

            var sectionPath = GetSectionPath(NormalizeConfigurationKey(configurationKey));
            return AddDefaultPostKitRegistration(services, sectionPath, true, optionsBuilder => optionsBuilder.Configure<IConfiguration>((options, configuration) => configuration.GetSection(sectionPath)
                .Bind(options)));
        }

        var resolvedConfigurationKey = ResolveConfigurationKey(serviceKey, configurationKey);
        var resolvedSectionPath = GetSectionPath(resolvedConfigurationKey);
        return AddKeyedPostKitRegistration(services, serviceKey, resolvedSectionPath, true, optionsBuilder => optionsBuilder.Configure<IConfiguration>((options, configuration) => configuration.GetSection(resolvedSectionPath)
            .Bind(options)));
    }

    /// <summary>Registers keyed PostKit services using configuration from the <c>PostKit</c> section of the provided configuration root.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="serviceKey">The key used to resolve the keyed services.</param>
    /// <param name="configuration">The configuration root that contains the <c>PostKit</c> section.</param>
    /// <param name="configurationKey">The configuration key to bind options from. When omitted, the value is inferred from <paramref name="serviceKey" />.</param>
    /// <returns>The same <paramref name="services" /> instance so calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddKeyedPostKit(this IServiceCollection services, object serviceKey, IConfiguration configuration, string? configurationKey = null)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "The service collection cannot be null.");

        if (serviceKey is null)
            throw new ArgumentNullException(nameof(serviceKey), "The service key cannot be null.");

        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration), "The configuration root cannot be null.");

        var resolvedConfigurationKey = ResolveConfigurationKey(serviceKey, configurationKey);
        var rootSection = configuration.GetSection(ConfigurationSectionName);
        EnsureConfigurationSectionExists(rootSection);

        var configurationSection = rootSection.GetSection(resolvedConfigurationKey);
        EnsureConfigurationSectionExists(configurationSection);

        return services.AddKeyedPostKit(serviceKey, configurationSection);
    }

    /// <summary>Registers keyed PostKit services using the provided configuration section.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="serviceKey">The key used to resolve the keyed services.</param>
    /// <param name="configurationSection">The configuration section to bind options from.</param>
    /// <exception cref="InvalidOperationException">Thrown when the provided configuration section cannot be found.</exception>
    /// <returns>The same <paramref name="services" /> instance so calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddKeyedPostKit(this IServiceCollection services, object serviceKey, IConfigurationSection configurationSection)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "The service collection cannot be null.");

        if (serviceKey is null)
            throw new ArgumentNullException(nameof(serviceKey), "The service key cannot be null.");

        if (configurationSection is null)
            throw new ArgumentNullException(nameof(configurationSection), "The configuration section cannot be null.");

        EnsureConfigurationSectionExists(configurationSection);

        return AddKeyedPostKitRegistration(services, serviceKey, GetOptionsName(configurationSection), false, optionsBuilder => optionsBuilder.Configure(configurationSection.Bind));
    }

    private static IServiceCollection AddDefaultPostKitRegistration(IServiceCollection services, string optionsName, bool validateConfigurationSectionFromServices, Action<OptionsBuilder<PostKitOptions>> configureOptions)
    {
        RegisterCommonServices(services);

        var optionsBuilder = AddValidatedOptions(services, optionsName, validateConfigurationSectionFromServices);
        configureOptions(optionsBuilder);

        services.Replace(ServiceDescriptor.Singleton<IOptionsMonitor<PostKitOptions>>(sp => new RebasedOptionsMonitor<PostKitOptions>(sp.GetRequiredService<IOptionsFactory<PostKitOptions>>(),
            sp.GetServices<IOptionsChangeTokenSource<PostKitOptions>>(), sp.GetRequiredService<IOptionsMonitorCache<PostKitOptions>>(), optionsName)));

        services.Replace(ServiceDescriptor.Singleton<IOptions<PostKitOptions>>(sp => new RebasedOptions<PostKitOptions>(sp.GetRequiredService<IOptionsMonitor<PostKitOptions>>())));

        services.Replace(ServiceDescriptor.Scoped<IOptionsSnapshot<PostKitOptions>>(sp => new RebasedOptionsSnapshot<PostKitOptions>(sp.GetRequiredService<IOptionsFactory<PostKitOptions>>(), optionsName)));

        services.Replace(ServiceDescriptor.Transient<IPostmarkClient>(sp => sp.GetRequiredService<IPostmarkClientFactory>()
            .Create()));

        services.Replace(ServiceDescriptor.Transient<IPostKitClient>(sp => new PostKitClient(sp.GetRequiredService<IPostmarkClient>(), sp.GetRequiredService<ILogger<PostKitClient>>())));

        return services;
    }

    private static IServiceCollection AddKeyedPostKitRegistration(IServiceCollection services, object serviceKey, string optionsName, bool validateConfigurationSectionFromServices, Action<OptionsBuilder<PostKitOptions>> configureOptions)
    {
        RegisterCommonServices(services);

        var optionsBuilder = AddValidatedOptions(services, optionsName, validateConfigurationSectionFromServices);
        configureOptions(optionsBuilder);

        services.RemoveAllKeyed<IPostmarkClient>(serviceKey);
        services.RemoveAllKeyed<IPostKitClient>(serviceKey);

        services.AddKeyedTransient<IPostmarkClient>(serviceKey, (sp, _) => sp.GetRequiredService<IPostmarkClientFactory>()
            .Create(optionsName));

        services.AddKeyedTransient<IPostKitClient>(serviceKey, (sp, _) => new PostKitClient(sp.GetRequiredKeyedService<IPostmarkClient>(serviceKey), sp.GetRequiredService<ILogger<PostKitClient>>()));

        return services;
    }

    private static void RegisterCommonServices(IServiceCollection services)
    {
        services.AddHttpClient("Postmark");
        services.TryAddSingleton<IPostmarkClientFactory, PostmarkClientFactory>();
        services.TryAddSingleton<PostmarkRateLimiter>();
    }

    private static OptionsBuilder<PostKitOptions> AddValidatedOptions(IServiceCollection services, string optionsName, bool validateConfigurationSectionFromServices)
    {
        var optionsBuilder = services.AddOptions<PostKitOptions>(optionsName);

        if (validateConfigurationSectionFromServices)
        {
            optionsBuilder.Validate<IConfiguration>((_, configuration) => configuration.GetSection(optionsName)
                    .Exists(), $"The configuration section '{optionsName}' could not be found.");
            optionsBuilder.Validate<IConfiguration>((options, configuration) =>
                {
                    var section = configuration.GetSection(optionsName);
                    return !section.Exists() || HasAnyApiToken(options);
                }, $"The configuration section '{optionsName}' {RequiredTokenMessage}");
        }
        else
        {
            optionsBuilder.Validate(HasAnyApiToken, $"The configuration section '{optionsName}' {RequiredTokenMessage}");
        }

        optionsBuilder.ValidateOnStart();

        return optionsBuilder;
    }

    private static bool HasAnyApiToken(PostKitOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.ServerApiToken) || !string.IsNullOrWhiteSpace(options.AccountApiToken);
    }

    private static string ResolveConfigurationKey(object serviceKey, string? configurationKey)
    {
        var resolvedConfigurationKey = configurationKey;
        if (string.IsNullOrWhiteSpace(resolvedConfigurationKey))
            resolvedConfigurationKey = serviceKey as string ?? serviceKey.ToString();

        if (string.IsNullOrWhiteSpace(resolvedConfigurationKey))
            throw new InvalidOperationException("Cannot determine the configuration key using the specified service key. Please specify a configuration key.");

        return NormalizeConfigurationKey(resolvedConfigurationKey);
    }

    private static string NormalizeConfigurationKey(string configurationKey)
    {
        return configurationKey.Trim();
    }

    private static string GetSectionPath(string configurationKey)
    {
        return $"{ConfigurationSectionName}:{configurationKey}";
    }

    private static string GetOptionsName(IConfigurationSection configurationSection)
    {
        return string.IsNullOrWhiteSpace(configurationSection.Path) ? configurationSection.Key : configurationSection.Path;
    }

    private static void EnsureConfigurationSectionExists(IConfigurationSection configurationSection)
    {
        if (configurationSection.Exists())
            return;

        var sectionName = string.IsNullOrWhiteSpace(configurationSection.Path) ? configurationSection.Key : configurationSection.Path;

        throw new InvalidOperationException($"The configuration section '{sectionName}' could not be found.");
    }
}