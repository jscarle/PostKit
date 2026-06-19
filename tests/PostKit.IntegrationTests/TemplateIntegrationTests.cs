using PostKit.Common;
using PostKit.Emails;

namespace PostKit.IntegrationTests;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
/// <summary>Integration tests for template-based emails through Postmark API.</summary>
public class TemplateIntegrationTests
{
    private readonly IPostKitClient _client = TestHelper.CreateClient();

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateId_Succeeds()
    {
        // Arrange
        var templateModel = new { product_name = "Test Product", product_url = "https://example.com/product", name = "Test User" };

        var email = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAlias_Succeeds()
    {
        // Arrange
        var templateModel = new { company_name = "Test Company", company_address = "123 Test St" };

        var email = Email.FromTemplate("message-en")
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndInlineCss_Succeeds()
    {
        // Arrange
        var templateModel = new { title = "Test Title", content = "Test Content" };

        var email = Email.FromTemplate(41813873, true)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndMetadata_Succeeds()
    {
        // Arrange
        var templateModel = new { user_name = "John Doe", action = "verify_email" };

        var email = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .AddMetadata("template_type", "verification")
            .AddMetadata("user_id", "98765")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndTracking_Succeeds()
    {
        // Arrange
        var templateModel = new { notification_type = "order_confirmation", order_number = "12345" };

        var email = Email.FromTemplate("message-en")
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .EnableOpenTracking()
            .UseLinkTracking()
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndTag_Succeeds()
    {
        // Arrange
        var templateModel = new { content = "Newsletter content" };

        var email = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .WithTag("newsletter")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndHeaders_Succeeds()
    {
        // Arrange
        var templateModel = new { message = "Custom header test" };

        var email = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .AddHeader("X-Template-Version", "1.0")
            .AddHeader("X-Campaign-Id", "campaign-123")
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndAttachment_Succeeds()
    {
        // Arrange
        var templateModel = new { invoice_number = "INV-2024-001" };

        var attachment = Attachment.Create("invoice.txt", "text/plain", "Invoice details"u8.ToArray());

        var email = Email.FromTemplate("message-en")
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .AddAttachment(attachment)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailBatchAsync_WithTemplates_Succeeds()
    {
        // Arrange
        var email1 = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(new { name = "User 1" })
            .Build();

        var email2 = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To("another@postkit.com")
            .WithModel(new { name = "User 2" })
            .Build();

        var emails = new[] { email1, email2 };

        // Act
        var result = await _client.SendEmailBatchAsync(emails, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var batchResponse), result.ToString());
        Assert.True(batchResponse.IsSuccessful);
        Assert.Equal(2, batchResponse.Results.Count);
        Assert.All(batchResponse.Results, r =>
        {
            Assert.True(r.IsSuccess(out var response), r.ToString());
            Assert.NotEqual(Guid.Empty, response.MessageId);
        });
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithComplexTemplateModel_Succeeds()
    {
        // Arrange
        var templateModel = new
        {
            user = new { first_name = "John", last_name = "Doe", email = "john.doe@postkit.com" },
            order = new
            {
                id = "ORDER-123", date = "2024-01-15", total = 99.99, items = new[] { new { name = "Product 1", quantity = 2, price = 29.99 }, new { name = "Product 2", quantity = 1, price = 40.01 } }
            },
            settings = new { currency = "USD", tax_rate = 0.08 }
        };

        var email = Email.FromTemplate(41813873)
            .From(TestConfiguration.TestFromEmail)
            .To(TestConfiguration.TestToEmail)
            .WithModel(templateModel)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }

    [Fact(Skip = "Cannot be tested with test API token.")]
    public async Task SendEmailAsync_WithTemplateAndAllFeatures_Succeeds()
    {
        // Arrange
        var templateModel = new { recipient_name = "Test Recipient", notification_type = "comprehensive", data = new { key = "value" } };

        var attachment = Attachment.Create("data.txt", "text/plain", "Additional data"u8.ToArray());

        var email = Email.FromTemplate(41813873, true)
            .From(TestConfiguration.TestFromEmail, "Template Sender")
            .To(TestConfiguration.TestToEmail, "Template Recipient")
            .Cc(TestConfiguration.TestCcEmail)
            .ReplyTo(TestConfiguration.TestReplyToEmail)
            .WithModel(templateModel)
            .WithTag("template-comprehensive")
            .AddHeader("X-Template-Test", "comprehensive")
            .AddMetadata("test_type", "template-full")
            .EnableOpenTracking()
            .UseLinkTracking()
            .AddAttachment(attachment)
            .UseMessageStream(MessageStream.Broadcast)
            .Build();

        // Act
        var result = await _client.SendEmailAsync(email, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.NotEqual(Guid.Empty, response.MessageId);
    }
}