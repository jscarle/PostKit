namespace PostKit.IntegrationTests;

/// <summary>Test configuration for integration tests.</summary>
internal static class TestConfiguration
{
    /// <summary>Postmark test API token - This is a special test token provided by Postmark.</summary>
    public const string ApiToken = "POSTMARK_API_TEST";

    /// <summary>Test email addresses recognized by Postmark test API.</summary>
    public const string TestFromEmail = "sender@example.com";

    public const string TestToEmail = "receiver@example.com";
    public const string TestCcEmail = "cc@example.com";
    public const string TestBccEmail = "bcc@example.com";
    public const string TestReplyToEmail = "replyto@example.com";
}
