using Xunit;

namespace PostKit.Tests;

public class EmailBuilderSizeLimitTests
{
    [Fact]
    public void Build_WithOversizedTextBody_ThrowsInvalidOperationException()
    {
        const int messageSizeLimit = 10 * 1024 * 1024;
        var oversizedText = new string('a', messageSizeLimit + 1);

        var builder = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithTextBody(oversizedText);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
}
