using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PostKit.Configuration;

namespace PostKit.Tests;

public class PostKitExtensionsTests
{
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
        Assert.DoesNotContain("The configuration section 'PostKit' must define 'ServerApiToken'.", exception.Failures);
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
    public void AddKeyedPostKit_DefaultRegistration_ThrowsValidationExceptionWhenRequestedSectionIsMissingServerApiToken()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = "root-token", ["PostKit:Secondary:AccountApiToken"] = "account-only" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(serviceProvider.GetRequiredService<IPostKitClient>);

        Assert.Contains("The configuration section 'PostKit:Secondary' must define 'ServerApiToken'.", exception.Failures);
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
        Assert.DoesNotContain("The configuration section 'PostKit:Marketing' must define 'ServerApiToken'.", exception.Failures);
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

        var exception = Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredKeyedService<IPostKitClient>("shared"));

        Assert.Contains("The configuration section 'PostKit:Secondary' must define 'ServerApiToken'.", exception.Failures);
    }
}
