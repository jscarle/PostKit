using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PostKit.IntegrationTests;

/// <summary>Helper class for creating test clients.</summary>
internal static class TestHelper
{
    /// <summary>Creates a PostKit client configured with the test API token.</summary>
    public static IPostKitClient CreateClient(string? apiToken = TestConfiguration.ApiToken)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PostKit:ServerApiToken"] = apiToken })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPostKit();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IPostKitClient>();
    }
}
