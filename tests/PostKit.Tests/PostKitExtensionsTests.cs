using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PostKit.Configuration;

namespace PostKit.Tests;

public class PostKitExtensionsTests
{
    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_UsesRequestedConfigurationSectionOnly()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IPostKitClient>());

        Assert.Equal("The server API token has not been set.", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_ExposesRequestedSectionAsDefaultOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>().Value;

        Assert.Null(options.ServerApiToken);
        Assert.Equal("account-only", options.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_ExposesRequestedSectionThroughOptionsMonitor()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PostKitOptions>>();

        Assert.Null(optionsMonitor.CurrentValue.ServerApiToken);
        Assert.Equal("account-only", optionsMonitor.CurrentValue.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_ExposesRequestedSectionThroughOptionsSnapshot()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:ServerApiToken"] = "root-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var optionsSnapshot = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PostKitOptions>>();

        Assert.Null(optionsSnapshot.Value.ServerApiToken);
        Assert.Equal("account-only", optionsSnapshot.Value.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_LastCallWins()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:Primary:ServerApiToken"] = "primary-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit(configurationKey: "Primary");
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IPostKitClient>());

        Assert.Equal("The server API token has not been set.", exception.Message);
    }

    [Fact]
    public void AddKeyedPostKit_DefaultRegistration_LastCallWinsForDefaultOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:Primary:ServerApiToken"] = "primary-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit(configurationKey: "Primary");
        services.AddKeyedPostKit(configurationKey: "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PostKitOptions>>().Value;

        Assert.Null(options.ServerApiToken);
        Assert.Equal("account-only", options.AccountApiToken);
    }

    [Fact]
    public void AddKeyedPostKit_KeyedRegistration_LastCallWinsForSameServiceKey()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostKit:Primary:ServerApiToken"] = "primary-token",
                ["PostKit:Secondary:AccountApiToken"] = "account-only",
            }
        ).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddKeyedPostKit("shared", "Primary");
        services.AddKeyedPostKit("shared", "Secondary");

        using var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredKeyedService<IPostKitClient>("shared"));

        Assert.Equal("The server API token has not been set.", exception.Message);
    }
}
