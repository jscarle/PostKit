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
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Cancellation Test")
            .TextBody("This request should be cancelled.")
            .Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _client.SendEmailAsync(email, cts.Token));
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Batch Cancellation Test")
            .TextBody("This batch request should be cancelled.")
            .Build();

        var emails = new[] { email };
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _client.SendEmailBatchAsync(emails, cts.Token));
    }

    [Fact]
    public async Task SendEmailAsync_WithSpecialCharactersInSubject_Succeeds()
    {
        // Arrange
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Special chars: émojis 🎉 symbols ★♥ quotes \"'")
            .TextBody("Testing special characters in subject.")
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
        var email = Email.Compose()
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .Subject("Unicode Content Test")
            .TextBody("Testing Unicode: 你好世界 🌍 Здравствуй мир こんにちは世界")
            .HtmlBody("<html><body><p>Unicode: 你好世界 🌍 Здравствуй мир こんにちは世界</p></body></html>")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }
}
