using PostKit.Emails;

namespace PostKit.IntegrationTests;

/// <summary>Integration tests for error scenarios and edge cases with Postmark API.</summary>
public class ErrorScenarioIntegrationTests
{
    private readonly IPostKitClient _client = TestHelper.CreateClient();

    [Fact]
    public async Task SendEmailBatchAsync_WithEmptyCollection_Fails()
    {
        // Arrange
        var emails = Array.Empty<Email>();

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure());
    }

    [Fact]
    public async Task SendEmailAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Cancellation Test")
            .WithTextBody("This request should be cancelled.")
            .Build();

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var result = await _client.SendEmailAsync(email, cts.Token);
        Assert.True(result.IsFailure());
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Batch Cancellation Test")
            .WithTextBody("This batch request should be cancelled.")
            .Build();

        var emails = new[] { email };
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var result = await _client.SendEmailBatchAsync(emails, cts.Token);
        Assert.True(result.IsFailure());
    }

    [Fact]
    public async Task SendEmailAsync_WithSpecialCharactersInSubject_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Special chars: émojis 🎉 symbols ★♥ quotes \"'")
            .WithTextBody("Testing special characters in subject.")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithUnicodeContent_Succeeds()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithSubject("Unicode Content Test")
            .WithTextBody("Testing Unicode: 你好世界 🌍 Здравствуй мир こんにちは世界")
            .WithHtmlBody("<html><body><p>Unicode: 你好世界 🌍 Здравствуй мир こんにちは世界</p></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }
}
