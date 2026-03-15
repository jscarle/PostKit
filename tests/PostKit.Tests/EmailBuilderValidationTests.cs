using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostKit.Common;
using PostKit.Emails;

namespace PostKit.Tests;

/// <summary>Unit tests for email builder validation rules.</summary>
public class EmailBuilderValidationTests
{
    [Fact]
    public void SendEmailAsync_WithInvalidApiToken_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddPostKit();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            serviceProvider.GetRequiredService<IPostKitClient>();
        });

        // Assert
        Assert.Contains("API token has not been set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithoutFrom_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .To("test@postkit.com")
                    .Subject("No From Address")
                    .TextBody("This should fail without From address.")
                    .Build();
            }
        );

        Assert.Equal("From address is required.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithoutRecipients_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .Subject("No Recipients")
                    .TextBody("This should fail without recipients.")
                    .Build();
            }
        );

        Assert.Equal("At least one recipient is required.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithTooManyRecipients_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var builder = Email.Compose()
                    .From("sender@postkit.com")
                    .Subject("Too Many Recipients")
                    .TextBody("This should fail with too many recipients.");

                // Add 51 recipients (limit is 50)
                var toBuilder = builder.To($"recipient@postkit.com");
                for (var i = 1; i < 51; i++)
                    toBuilder.To($"recipient{i}@postkit.com");

                builder.Build();
            }
        );

        Assert.Contains("too many recipients", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithoutSubjectOrTemplate_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .TextBody("This should fail without subject or template.")
                    .Build();
            }
        );

        Assert.Contains("subject", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithoutBodyOrTemplate_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .Subject("No Body")
                    .Build();
            }
        );

        Assert.Contains("body", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithSubjectAndTemplate_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .Subject("First subject")
                    .Subject("Second subject");
            }
        );

        Assert.Contains("already been set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithTextBodyAndTemplate_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .TextBody("First body")
                    .TextBody("Second body");
            }
        );

        Assert.Contains("already been set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithHtmlBodyAndTemplate_ThrowsException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .HtmlBody("First body")
                    .HtmlBody("Second body");
            }
        );

        Assert.Contains("already been set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_Build_WithTemplateAndSubjectInInvalidState_ThrowsException()
    {
        var builder = Email.FromTemplate(41813873)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(new { Name = "Alice" });

        var draftField = typeof(TemplatedEmailBuilder).GetField("_draft", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(draftField);

        var draft = draftField.GetValue(builder);
        Assert.NotNull(draft);

        var subjectField = draft.GetType()
            .GetField("_subject", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(subjectField);
        subjectField.SetValue(draft, "Unexpected subject");

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Neither a text or HTML body, nor a subject may be specified when using a template.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithVeryLargeTextBody_ThrowsException()
    {
        // Arrange - Create a text body larger than 5 MB
        var largeText = new string('A', 6 * 1024 * 1024); // 6 MB

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .Subject("Large Text Body")
                    .TextBody(largeText)
                    .Build();
            }
        );

        Assert.Contains("5 MB", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithVeryLargeHtmlBody_ThrowsException()
    {
        // Arrange - Create an HTML body larger than 5 MB
        var largeText = new string('A', 6 * 1024 * 1024); // 6 MB
        var largeHtml = $"<html><body>{largeText}</body></html>"; // > 5 MB

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .Subject("Large HTML Body")
                    .HtmlBody(largeHtml)
                    .Build();
            }
        );

        Assert.Contains("5 MB", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithLargeBodyAndAttachmentCombination_ThrowsException()
    {
        // Arrange - Build still enforces the limit when attachments are added before the bodies.
        var text = new string('A', 5 * 1024 * 1024); // 5 MB
        var attachmentData = new byte[6 * 1024 * 1024]; // 6 MB
        Array.Fill(attachmentData, (byte)'C');
        var attachment = Attachment.Create("large.dat", "application/octet-stream", attachmentData);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .Subject("Large Body And Attachment")
                    .AddAttachment(attachment)
                    .TextBody(text)
                    .Build();
            }
        );

        Assert.Contains("10 MB", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithInvalidEmailAddress_IsAcceptedByBuilder()
    {
        // Note: The builder accepts the email format as-is and lets Postmark validate it.
        // This test verifies the builder doesn't reject malformed emails at build time.

        // Arrange & Act
        var email = Email.Compose()
            .From("invalid-email")
            .To("also-invalid")
            .Subject("Invalid Email Format")
            .TextBody("Testing invalid email addresses.")
            .Build();

        // Assert - Build succeeds, but send would fail
        Assert.NotNull(email);
        Assert.NotNull(email.From);
        Assert.Equal("invalid-email", email.From.Address);
    }

    [Fact]
    public void EmailBuilder_WithEmptyTag_Succeeds()
    {
        // Arrange & Act
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Empty Tag Test")
            .TextBody("Testing empty tag.")
            .WithTag(string.Empty)
            .Build();

        // Assert
        Assert.NotNull(email);
        Assert.Equal(string.Empty, email.Tag);
    }

    [Fact]
    public void EmailBuilder_WithEmptyMetadataValue_Succeeds()
    {
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Empty Metadata Value")
            .TextBody("Testing empty metadata values.")
            .AddMetadata("empty_value", string.Empty)
            .Build();

        Assert.NotNull(email.Metadata);
        Assert.Equal(string.Empty, email.Metadata["empty_value"]);
    }

    [Fact]
    public void EmailBuilder_WithNullTag_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .WithTag(null!));

        Assert.Equal("tag", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullTemplateModel_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            {
                Email.FromTemplate(41813873)
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithModel(null!);
            }
        );
    }

    [Fact]
    public void EmailBuilder_WithTemplateModelWithoutTemplate_ThrowsException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.FromTemplate(41813873)
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .Build();
            }
        );

        Assert.Equal("A template model is required when using a template.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithFromCalledTwice_ThrowsException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.Compose()
                    .From("sender@postkit.com")
                    .From("sender2@postkit.com")
                    .Build();
            }
        );

        Assert.Contains("already been set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_WithEmojiSubjectExceedingUtf16Limit_ThrowsException()
    {
        var subject = string.Concat(Enumerable.Repeat("😀", 1001));

        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .Subject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }
}
