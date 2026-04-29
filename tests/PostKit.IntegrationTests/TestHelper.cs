using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PostKit.IntegrationTests;

/// <summary>Helper class for creating test clients.</summary>
internal static class TestHelper
{
    /// <summary>Creates a PostKit client configured with the test API token.</summary>
    public static IPostKitClient CreateClient(string? apiToken = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = apiToken ?? TestConfiguration.ApiToken })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPostKit(configuration);

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IPostKitClient>();
    }
}
