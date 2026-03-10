using Microsoft.Extensions.Configuration;

namespace PostKit.IntegrationTests;

/// <summary>Test configuration for integration tests.</summary>
internal static class TestConfiguration
{
    private const string DefaultApiToken = "POSTMARK_API_TEST";
    private const string DefaultTestFromEmail = "sender@example.com";
    private const string DefaultTestToEmail = "receiver@example.com";
    private const string DefaultTestCcEmail = "cc@example.com";
    private const string DefaultTestBccEmail = "bcc@example.com";
    private const string DefaultTestReplyToEmail = "replyto@example.com";

    private static readonly Lazy<IConfigurationRoot> Configuration = new(CreateConfiguration);

    /// <summary>Postmark test API token - This is a special test token provided by Postmark.</summary>
    public static string ApiToken => GetValue("ApiToken", DefaultApiToken);

    /// <summary>Test email addresses recognized by Postmark test API.</summary>
    public static string TestFromEmail => GetValue("FromEmail", DefaultTestFromEmail);

    public static string TestToEmail => GetValue("ToEmail", DefaultTestToEmail);
    public static string TestCcEmail => GetValue("CcEmail", DefaultTestCcEmail);
    public static string TestBccEmail => GetValue("BccEmail", DefaultTestBccEmail);
    public static string TestReplyToEmail => GetValue("ReplyToEmail", DefaultTestReplyToEmail);

    private static IConfigurationRoot CreateConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();
    }

    private static string GetValue(string key, string fallback)
    {
        var value = Configuration.Value[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
