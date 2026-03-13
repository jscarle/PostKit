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
                Email.CreateBuilder()
                    .To("test@postkit.com")
                    .WithSubject("No From Address")
                    .WithTextBody("This should fail without From address.")
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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .WithSubject("No Recipients")
                    .WithTextBody("This should fail without recipients.")
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
                var builder = Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .WithSubject("Too Many Recipients")
                    .WithTextBody("This should fail with too many recipients.");

                // Add 51 recipients (limit is 50)
                var toBuilder = builder.To($"recipient@postkit.com");
                for (var i = 1; i < 51; i++)
                    toBuilder.AlsoTo($"recipient{i}@postkit.com");

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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithTextBody("This should fail without subject or template.")
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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithSubject("No Body")
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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithSubject("Body and Template Conflict")
                    .UsingTemplate(41813873)
                    .Build();
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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithTextBody("This should fail when combining body with template.")
                    .UsingTemplate(41813873)
                    .Build();
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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithHtmlBody("This should fail when combining body with template.")
                    .UsingTemplate(41813873)
                    .Build();
            }
        );

        Assert.Contains("already been set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailBuilder_Build_WithTemplateAndSubjectInInvalidState_ThrowsException()
    {
        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .UsingTemplate(41813873)
            .WithTemplateModel(new { Name = "Alice" });

        var subjectField = typeof(EmailBuilder).GetField("_subject", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(subjectField);
        subjectField.SetValue(builder, "Unexpected subject");

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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithSubject("Large Text Body")
                    .WithTextBody(largeText)
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
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithSubject("Large HTML Body")
                    .WithHtmlBody(largeHtml)
                    .Build();
            }
        );

        Assert.Contains("5 MB", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithTotalSizeExceedingLimit_ThrowsException()
    {
        // Arrange - Create bodies and attachments that together exceed 10 MB
        var text = new string('A', 3 * 1024 * 1024); // 3 MB
        var html = new string('B', 3 * 1024 * 1024); // 3 MB
        var attachmentData = new byte[5 * 1024 * 1024]; // 5 MB
        Array.Fill(attachmentData, (byte)'C');
        var attachment = Attachment.Create("large.dat", "application/octet-stream", attachmentData);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithSubject("Total Size Too Large")
                    .WithTextBody(text)
                    .WithHtmlBody(html)
                    .WithAttachment(attachment)
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
        var email = Email.CreateBuilder()
            .From("invalid-email")
            .To("also-invalid")
            .WithSubject("Invalid Email Format")
            .WithTextBody("Testing invalid email addresses.")
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
        var email = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Empty Tag Test")
            .WithTextBody("Testing empty tag.")
            .WithTag(string.Empty)
            .Build();

        // Assert
        Assert.NotNull(email);
        Assert.Equal(string.Empty, email.Tag);
    }

    [Fact]
    public void EmailBuilder_WithNullTag_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.CreateBuilder()
            .WithTag(null!));

        Assert.Equal("tag", exception.ParamName);
    }

    [Fact]
    public void EmailBuilder_WithNullTemplateModel_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            {
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .UsingTemplate(41813873)
                    .WithTemplateModel(null!);
            }
        );
    }

    [Fact]
    public void EmailBuilder_WithTemplateModelWithoutTemplate_ThrowsException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                Email.CreateBuilder()
                    .From("sender@postkit.com")
                    .To("recipient@postkit.com")
                    .WithSubject("Missing template")
                    .WithTextBody("Body")
                    .WithTemplateModel(new { name = "Alice" })
                    .Build();
            }
        );

        Assert.Equal("A template ID or alias is required when using a template model.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithEmojiSubjectExceedingUtf16Limit_ThrowsException()
    {
        var subject = string.Concat(Enumerable.Repeat("😀", 1001));

        var exception = Assert.Throws<ArgumentException>(() => Email.CreateBuilder()
            .WithSubject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }
}
