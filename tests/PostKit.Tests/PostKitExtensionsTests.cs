using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
}
