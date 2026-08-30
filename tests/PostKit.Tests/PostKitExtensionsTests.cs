using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class PostKitExtensionsTests
{
    [Fact]
    public void AddPostKit_DefaultRegistration_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.CurrentValue.ServerApiToken);

        configuration["PostKit:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.CurrentValue.ServerApiToken);
    }

    [Fact]
    public void AddPostKit_WithConfiguration_BindsPostKitSectionWithoutIConfigurationRegistration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token", ["PostKit:AccountApiToken"] = "account-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Equal("root-token", options.ServerApiToken);
        Assert.Equal("account-token", options.AccountApiToken);
        Assert.NotNull(serviceProvider.GetRequiredService<IPostKitClient>());
    }

    [Fact]
    public void AddPostKit_WithConfiguration_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.CurrentValue.ServerApiToken);

        configuration["PostKit:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.CurrentValue.ServerApiToken);
    }

    [Fact]
    public void AddPostKit_WithConfiguration_AllowsAccountApiTokenWithoutServerApiToken()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:AccountApiToken"] = "account-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Null(options.ServerApiToken);
        Assert.Equal("account-token", options.AccountApiToken);
        Assert.NotNull(serviceProvider.GetRequiredService<IPostKitClient>());
    }

    [Fact]
    public void ConfigurePostKitHttpClient_AfterRegistration_ExposesSupportedNamedClientBuilder()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token" })
            .Build();
        var configuredClientName = string.Empty;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);
        var returnedServices = services.ConfigurePostKitHttpClient(httpClient => configuredClientName = httpClient.Name);

        Assert.Same(services, returnedServices);
        Assert.Equal("Postmark", configuredClientName);
    }

    [Fact]
    public void ConfigurePostKitHttpClient_WithNullServices_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => PostKitExtensions.ConfigurePostKitHttpClient(null!, _ => { }));

        Assert.Equal("services", exception.ParamName);
        Assert.Equal("The service collection cannot be null. (Parameter 'services')", exception.Message);
    }

    [Fact]
    public void ConfigurePostKitHttpClient_WithNullConfiguration_ThrowsHelpfulException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() => services.ConfigurePostKitHttpClient(null!));

        Assert.Equal("configure", exception.ParamName);
        Assert.Equal("The HTTP client configuration cannot be null. (Parameter 'configure')", exception.Message);
    }

    [Fact]
    public void AddPostKit_WithConfigurationSection_BindsProvidedSectionWithoutIConfigurationRegistration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Tenants:Marketing:ServerApiToken"] = "marketing-token", ["Tenants:Marketing:AccountApiToken"] = "marketing-account" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration.GetSection("Tenants:Marketing"));

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Equal("marketing-token", options.ServerApiToken);
        Assert.Equal("marketing-account", options.AccountApiToken);
        Assert.NotNull(serviceProvider.GetRequiredService<IPostKitClient>());
    }

    [Fact]
    public void AddPostKit_WithConfigurationSection_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Tenants:Marketing:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration.GetSection("Tenants:Marketing"));

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.CurrentValue.ServerApiToken);

        configuration["Tenants:Marketing:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.CurrentValue.ServerApiToken);
    }

    [Fact]
    public void AddPostKit_WithConfiguration_ThrowsWhenPostKitSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddPostKit(configuration));

        Assert.Equal("The configuration section 'PostKit' could not be found.", exception.Message);
    }

    [Fact]
    public void AddPostKit_WithNullServices_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => PostKitExtensions.AddPostKit(null!));

        Assert.Equal("services", exception.ParamName);
        Assert.Equal("The service collection cannot be null. (Parameter 'services')", exception.Message);
    }

    [Fact]
    public void AddPostKit_WithNullConfiguration_ThrowsHelpfulException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() => services.AddPostKit((IConfiguration)null!));

        Assert.Equal("configuration", exception.ParamName);
        Assert.Equal("The configuration root cannot be null. (Parameter 'configuration')", exception.Message);
    }

    [Fact]
    public void AddPostKit_WithNullConfigurationSection_ThrowsHelpfulException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() => services.AddPostKit(null!));

        Assert.Equal("configurationSection", exception.ParamName);
        Assert.Equal("The configuration section cannot be null. (Parameter 'configurationSection')", exception.Message);
    }

    [Fact]
    public void AddPostKit_DefaultRegistration_ThrowsValidationExceptionWhenPostKitSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(serviceProvider.GetRequiredService<IPostKitClient>);

        Assert.Contains("The configuration section 'PostKit' could not be found.", exception.Failures);
        Assert.DoesNotContain("The configuration section 'PostKit' must define 'ServerApiToken' or 'AccountApiToken'.", exception.Failures);
    }

    [Fact]
    public void AddPostKit_WithConfigurationSection_ThrowsWhenSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddPostKit(configuration.GetSection("Tenants:Missing")));

        Assert.Equal("The configuration section 'Tenants:Missing' could not be found.", exception.Message);
    }

    [Fact]
    public void AddPostKit_WithConfigurationSection_ThrowsValidationExceptionWhenSectionHasNoApiToken()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = " " })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration.GetSection("PostKit"));

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(serviceProvider.GetRequiredService<IPostKitClient>);

        Assert.Contains("The configuration section 'PostKit' must define 'ServerApiToken' or 'AccountApiToken'.", exception.Failures);
    }

    [Fact]
    public void AddKeyedPostKit_KeyedRegistrationWithInferredConfigurationKey_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Marketing:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit("Marketing");

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.Get("PostKit:Marketing").ServerApiToken);

        configuration["PostKit:Marketing:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.Get("PostKit:Marketing").ServerApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_KeyedRegistrationWithExplicitConfigurationKey_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Default:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit("Production", "Default");

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.Get("PostKit:Default").ServerApiToken);

        configuration["PostKit:Default:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.Get("PostKit:Default").ServerApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_WithConfiguration_BindsInferredSectionWithoutIConfigurationRegistration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Marketing:ServerApiToken"] = "marketing-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKeyedPostKit("Marketing", configuration);

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredKeyedService<IPostKitClient>("Marketing"));
    }

    [Fact]
    public void AddKeyedPostKit_WithConfiguration_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Default:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKeyedPostKit("Production", configuration, "Default");

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.Get("PostKit:Default").ServerApiToken);

        configuration["PostKit:Default:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.Get("PostKit:Default").ServerApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_WithConfiguration_ThrowsWhenPostKitSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddKeyedPostKit("Marketing", configuration));

        Assert.Equal("The configuration section 'PostKit' could not be found.", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_WithConfiguration_ThrowsWhenKeyedSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Other:ServerApiToken"] = "other-token" })
            .Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddKeyedPostKit("Marketing", configuration));

        Assert.Equal("The configuration section 'PostKit:Marketing' could not be found.", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_WithNullServiceKey_ThrowsHelpfulException()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() => services.AddKeyedPostKit(null!, configuration));

        Assert.Equal("serviceKey", exception.ParamName);
        Assert.Equal("The service key cannot be null. (Parameter 'serviceKey')", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_WithNullConfiguration_ThrowsHelpfulException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() => services.AddKeyedPostKit("Marketing", (IConfiguration)null!));

        Assert.Equal("configuration", exception.ParamName);
        Assert.Equal("The configuration root cannot be null. (Parameter 'configuration')", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_WithNullConfigurationSection_ThrowsHelpfulException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() => services.AddKeyedPostKit("Marketing", (IConfigurationSection)null!));

        Assert.Equal("configurationSection", exception.ParamName);
        Assert.Equal("The configuration section cannot be null. (Parameter 'configurationSection')", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_WithConfigurationSection_BindsProvidedSectionWithoutIConfigurationRegistration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Tenants:Marketing:ServerApiToken"] = "marketing-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKeyedPostKit("Marketing", configuration.GetSection("Tenants:Marketing"));

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredKeyedService<IPostKitClient>("Marketing"));
    }

    [Fact]
    public void AddKeyedPostKit_WithConfigurationSection_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Tenants:Marketing:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKeyedPostKit("Marketing", configuration.GetSection("Tenants:Marketing"));

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.Get("Tenants:Marketing").ServerApiToken);

        configuration["Tenants:Marketing:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.Get("Tenants:Marketing").ServerApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_TrimsConfigurationKey()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Marketing:ServerApiToken"] = "marketing-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit(configurationKey: " Marketing ");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Equal("marketing-token", options.ServerApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistrationWithConfigurationKey_ReloadsOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Secondary:ServerApiToken"] = "initial-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("initial-token", optionsMonitor.CurrentValue.ServerApiToken);

        configuration["PostKit:Secondary:ServerApiToken"] = "refreshed-token";
        configuration.Reload();

        Assert.Equal("refreshed-token", optionsMonitor.CurrentValue.ServerApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_UsesRequestedConfigurationSectionOnly()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token", ["PostKit:Secondary:ServerApiToken"] = "secondary-token", ["PostKit:Secondary:AccountApiToken"] = "account-only"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Equal("secondary-token", options.ServerApiToken);
        Assert.Equal("account-only", options.AccountApiToken);
        Assert.NotNull(serviceProvider.GetRequiredService<IPostKitClient>());
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_ExposesRequestedSectionThroughOptionsMonitor()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token", ["PostKit:Secondary:ServerApiToken"] = "secondary-token", ["PostKit:Secondary:AccountApiToken"] = "account-only"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Equal("secondary-token", optionsMonitor.CurrentValue.ServerApiToken);
        Assert.Equal("account-only", optionsMonitor.CurrentValue.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_ExposesRequestedSectionThroughOptionsSnapshot()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token", ["PostKit:Secondary:ServerApiToken"] = "secondary-token", ["PostKit:Secondary:AccountApiToken"] = "account-only"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var optionsSnapshot = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PostKitOptions>>();

        Assert.Equal("secondary-token", optionsSnapshot.Value.ServerApiToken);
        Assert.Equal("account-only", optionsSnapshot.Value.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_AllowsRequestedSectionWithOnlyAccountApiToken()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token", ["PostKit:Secondary:AccountApiToken"] = "account-only" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Null(options.ServerApiToken);
        Assert.Equal("account-only", options.AccountApiToken);
        Assert.NotNull(serviceProvider.GetRequiredService<IPostKitClient>());
    }

    [Fact]
    public void AddKeyedPostKit_KeyedRegistration_ThrowsValidationExceptionWhenRequestedSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Other:ServerApiToken"] = "other-token" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit("Marketing");

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredKeyedService<IPostKitClient>("Marketing"));

        Assert.Contains("The configuration section 'PostKit:Marketing' could not be found.", exception.Failures);
        Assert.DoesNotContain("The configuration section 'PostKit:Marketing' must define 'ServerApiToken' or 'AccountApiToken'.", exception.Failures);
    }

    [Fact]
    public async Task DefaultAndKeyedClients_WhenConfigurationReloads_UseRefreshedTokens()
    {
        const string infrastructurePostKitKey = "Infrastructure";
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "default-initial-token", ["PostKit:Infrastructure:ServerApiToken"] = "infrastructure-initial-token"
            })
            .Build();
        var handler = new RecordingTokenHttpMessageHandler();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(infrastructurePostKitKey);
        services.ConfigurePostKitHttpClient(httpClient => httpClient.ConfigurePrimaryHttpMessageHandler(() => handler));

        await using var serviceProvider = services.BuildServiceProvider();
        var defaultClient = serviceProvider.GetRequiredService<IPostmarkClient>();
        var infrastructureClient = serviceProvider.GetRequiredKeyedService<IPostmarkClient>(infrastructurePostKitKey);

        var initialDefaultResult = await defaultClient.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/server", CancellationToken.None);
        var initialInfrastructureResult = await infrastructureClient.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/server", CancellationToken.None);

        configuration["PostKit:ServerApiToken"] = "default-refreshed-token";
        configuration["PostKit:Infrastructure:ServerApiToken"] = "infrastructure-refreshed-token";
        configuration.Reload();

        var refreshedDefaultResult = await defaultClient.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/server", CancellationToken.None);
        var refreshedInfrastructureResult = await infrastructureClient.GetAsync<PostmarkResponse>(PostmarkTokenScope.Server, "/server", CancellationToken.None);

        Assert.True(initialDefaultResult.IsSuccess(out _), initialDefaultResult.ToString());
        Assert.True(initialInfrastructureResult.IsSuccess(out _), initialInfrastructureResult.ToString());
        Assert.True(refreshedDefaultResult.IsSuccess(out _), refreshedDefaultResult.ToString());
        Assert.True(refreshedInfrastructureResult.IsSuccess(out _), refreshedInfrastructureResult.ToString());
        Assert.Equal(["default-initial-token", "infrastructure-initial-token", "default-refreshed-token", "infrastructure-refreshed-token"], handler.ServerApiTokens);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_LastCallWinsForDefaultOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:Primary:ServerApiToken"] = "primary-token", ["PostKit:Secondary:ServerApiToken"] = "secondary-token", ["PostKit:Secondary:AccountApiToken"] = "account-only"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit(configurationKey: "Primary");
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>()
            .Value;

        Assert.Equal("secondary-token", options.ServerApiToken);
        Assert.Equal("account-only", options.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_KeyedRegistration_LastCallWinsForSameServiceKey()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:Primary:ServerApiToken"] = "primary-token", ["PostKit:Secondary:AccountApiToken"] = "account-only" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit("shared", "Primary");
        services.AddKeyedPostKit("shared", "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredKeyedService<IPostKitClient>("shared"));
    }

    private sealed class RecordingTokenHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<string> _serverApiTokens = [];

        public IReadOnlyList<string> ServerApiTokens => _serverApiTokens;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var serverApiToken = request.Headers.GetValues("X-Postmark-Server-Token")
                .Single();
            _serverApiTokens.Add(serverApiToken);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ErrorCode":0,"Message":"OK"}""", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            return Task.FromResult(response);
        }
    }
}
