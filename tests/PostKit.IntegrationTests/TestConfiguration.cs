using Microsoft.Extensions.Configuration;

namespace PostKit.IntegrationTests;

/// <summary>Test configuration for integration tests.</summary>
internal static class TestConfiguration
{
    private const string DefaultApiToken = "POSTMARK_API_TEST";
    private const string DefaultTestFromEmail = "sender@postkit.com";
    private const string DefaultTestToEmail = "receiver@postkit.com";
    private const string DefaultTestCcEmail = "cc@postkit.com";
    private const string DefaultTestBccEmail = "bcc@postkit.com";
    private const string DefaultTestReplyToEmail = "replyto@postkit.com";

    private static readonly Lazy<IConfigurationRoot> Configuration = new(CreateConfiguration);
    private static readonly Lazy<IConfigurationRoot> DevelopmentConfiguration = new(CreateDevelopmentConfiguration);

    /// <summary>Postmark test API token - This is a special test token provided by Postmark.</summary>
    public static string ApiToken => GetValue("ApiToken", DefaultApiToken);

    /// <summary>Test email addresses recognized by Postmark test API.</summary>
    public static string TestFromEmail => GetValue("FromEmail", DefaultTestFromEmail);

    public static string TestToEmail => GetValue("ToEmail", DefaultTestToEmail);
    public static string TestCcEmail => GetValue("CcEmail", DefaultTestCcEmail);
    public static string TestBccEmail => GetValue("BccEmail", DefaultTestBccEmail);
    public static string TestReplyToEmail => GetValue("ReplyToEmail", DefaultTestReplyToEmail);

    /// <summary>Values loaded directly from appsettings.Development.json for live-server-only integration tests.</summary>
    public static string? DevelopmentApiToken => GetDevelopmentValue("ApiToken");

    public static string? DevelopmentFromEmail => GetDevelopmentValue("FromEmail");
    public static string? DevelopmentToEmail => GetDevelopmentValue("ToEmail");
    public static string? DevelopmentCcEmail => GetDevelopmentValue("CcEmail");
    public static string? DevelopmentBccEmail => GetDevelopmentValue("BccEmail");
    public static string? DevelopmentReplyToEmail => GetDevelopmentValue("ReplyToEmail");

    private static IConfigurationRoot CreateConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        return new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{environment}.json", true)
            .Build();
    }

    private static IConfigurationRoot CreateDevelopmentConfiguration()
    {
        return new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Development.json", true)
            .Build();
    }

    private static string GetValue(string key, string fallback)
    {
        var value = Configuration.Value[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string? GetDevelopmentValue(string key)
    {
        var value = DevelopmentConfiguration.Value[key];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
