using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using PostKit.Common;
using PostKit.Emails;

namespace PostKit.Tests;

/// <summary>Unit tests for email builder validation rules.</summary>
public class EmailBuilderValidationTests
{
    [Fact]
    public void SendEmailAsync_WithMissingPostKitSection_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddPostKit();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var exception = Assert.Throws<OptionsValidationException>(() => { serviceProvider.GetRequiredService<IPostKitClient>(); });

        // Assert
        Assert.Contains("The configuration section 'PostKit' could not be found.", exception.Failures);
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
        });

        Assert.Equal("From address is required before building the email. Call From(...).", exception.Message);
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
        });

        Assert.Equal("At least one recipient is required before building the email. Call To(...), Cc(...), or Bcc(...).", exception.Message);
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
            var toBuilder = builder.To("recipient@postkit.com");
            for (var i = 1; i < 51; i++)
                toBuilder.To($"recipient{i}@postkit.com");

            builder.Build();
        });

        Assert.Contains("too many recipients", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Actual recipient count: 51.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmailBuilder_WithTooLongFromAddress_ThrowsHelpfulExceptionWithActualLength()
    {
        var displayName = new string('a', 260);
        var mailboxAddress = new MailboxAddress(displayName, "sender@postkit.com");
        var expectedLength = mailboxAddress.ToString(true)
            .Length;

        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .From("sender@postkit.com", displayName));

        Assert.Equal("address", exception.ParamName);
        Assert.Equal($"The From address cannot exceed 255 characters. Actual length: {expectedLength}. (Parameter 'address')", exception.Message);
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
        });

        Assert.Equal("Subject is required before building the email. Call Subject(...).", exception.Message);
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
        });

        Assert.Equal("Message content is required before building the email. Call TextBody(...) or HtmlBody(...).", exception.Message);
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
        });

        Assert.Equal("Cannot set Subject because it has already been set.", exception.Message);
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
        });

        Assert.Equal("Cannot set TextBody because it has already been set.", exception.Message);
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
        });

        Assert.Equal("Cannot set HtmlBody because it has already been set.", exception.Message);
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

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Equal("Template emails cannot also set Subject, TextBody, or HtmlBody. Use either template fields (TemplateId or TemplateAlias with TemplateModel) or content fields (Subject with TextBody or HtmlBody).", exception.Message);
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
        });

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
        });

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
        });

        Assert.Contains("10 MB", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithLocalOnlyStringEmailAddress_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .From("invalid-email"));

        Assert.Equal("address", exception.ParamName);
        Assert.Equal("The email address is invalid. Use a valid mailbox address such as 'recipient@example.com'. (Parameter 'address')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithLocalOnlyMailboxAddress_IsAcceptedByBuilder()
    {
        var email = Email.Compose()
            .From(new MailboxAddress(null, "invalid-email"))
            .To(new MailboxAddress(null, "also-invalid"))
            .Subject("Advanced mailbox address")
            .TextBody("Testing advanced mailbox address input.")
            .Build();

        Assert.NotNull(email);
        Assert.NotNull(email.From);
        Assert.Equal("invalid-email", email.From.Address);
    }

    [Fact]
    public void EmailBuilder_WithMalformedEmailAddress_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .To("\r\n"));

        Assert.Equal("address", exception.ParamName);
        Assert.Equal("The email address is invalid. Use a valid mailbox address such as 'recipient@example.com'. (Parameter 'address')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithMalformedEmailAddressInSequence_ThrowsHelpfulExceptionWithIndex()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .To(["valid@example.com", "\r\n"]));

        Assert.Equal("addresses", exception.ParamName);
        Assert.Equal("The email address at index 1 is invalid. Use a valid mailbox address such as 'recipient@example.com'. (Parameter 'addresses')", exception.Message);
    }

    [Theory]
    [InlineData("From")]
    [InlineData("ReplyTo")]
    [InlineData("To")]
    [InlineData("Cc")]
    [InlineData("Bcc")]
    public void EmailBuilder_DisplayNameOverloadWithOldOrder_ThrowsHelpfulException(string methodName)
    {
        var builder = Email.Compose();

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (methodName)
            {
                case "From":
                    builder.From("Recipient Name", "recipient@example.com");
                    break;
                case "ReplyTo":
                    builder.ReplyTo("Recipient Name", "recipient@example.com");
                    break;
                case "To":
                    builder.To("Recipient Name", "recipient@example.com");
                    break;
                case "Cc":
                    builder.Cc("Recipient Name", "recipient@example.com");
                    break;
                case "Bcc":
                    builder.Bcc("Recipient Name", "recipient@example.com");
                    break;
            }
        });

        Assert.Equal("address", exception.ParamName);
        Assert.Equal($"Display name overloads must specify the email address first: use .{methodName}(\"recipient@example.com\", \"Recipient Name\"). (Parameter 'address')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_DisplayNameOverloadWithInvalidAddressAndName_ThrowsAddressValidationException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .To("invalid-email", "Recipient @ Company"));

        Assert.Equal("address", exception.ParamName);
        Assert.Equal("The email address is invalid. Use a valid mailbox address such as 'recipient@example.com'. (Parameter 'address')", exception.Message);
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
    public void EmailBuilder_WithNullMetadataValue_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .AddMetadata("metadata_key", null!));

        Assert.Equal("value", exception.ParamName);
        Assert.Equal("The metadata value cannot be null. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithInvalidMetadataName_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .AddMetadata(new string('a', 21), "value"));

        Assert.Equal("The metadata name must not exceed 20 characters. Actual length: 21. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithMetadataNameStartingWithWhitespace_ThrowsHelpfulExceptionWithIndex()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .AddMetadata(" campaign", "value"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("The metadata name is required, must not exceed 20 characters, and cannot start or end with whitespace. Invalid leading whitespace space at index 0. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithMetadataNameEndingWithWhitespaceInSequence_ThrowsHelpfulExceptionWithIndex()
    {
        IEnumerable<KeyValuePair<string, string>> metadata =
        [
            new("campaign", "spring"),
            new("segment\t", "beta")
        ];

        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .AddMetadata(metadata));

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("The metadata name at index 1 is invalid. The metadata name is required, must not exceed 20 characters, and cannot start or end with whitespace. Invalid trailing whitespace tab at index 7. (Parameter 'metadata')",
            exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithTooLongMetadataValue_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .AddMetadata("metadata_key", new string('a', 81)));

        Assert.Equal("The metadata value must not exceed 80 characters. Actual length: 81. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithTooLongMetadataValueInSequence_ThrowsHelpfulExceptionWithActualLengthAndIndex()
    {
        var builder = Email.Compose();
        IEnumerable<KeyValuePair<string, string>> metadata =
        [
            new("first", "value"),
            new("second", new string('a', 81))
        ];

        var exception = Assert.Throws<ArgumentException>(() => builder.AddMetadata(metadata));

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("The metadata value at index 1 must not exceed 80 characters. Actual length: 81. (Parameter 'metadata')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithDuplicateMetadataName_ThrowsHelpfulExceptionWithName()
    {
        var builder = Email.Compose()
            .AddMetadata("campaign", "spring");

        var exception = Assert.Throws<ArgumentException>(() => builder.AddMetadata("CAMPAIGN", "summer"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'CAMPAIGN' duplicates existing metadata name 'campaign'. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_AddingEleventhMetadataField_ThrowsHelpfulExceptionWithCounts()
    {
        var builder = Email.Compose();
        for (var index = 0; index < 10; index++)
            builder.AddMetadata($"key{index}", "value");

        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddMetadata("extra", "value"));

        Assert.Equal("Cannot set more than 10 metadata fields for a message. Adding 1 metadata field to the existing 10 would produce 11.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_AddingDuplicateMetadataWhenAtLimit_ThrowsDuplicateNameException()
    {
        var builder = Email.Compose();
        for (var index = 0; index < 10; index++)
            builder.AddMetadata($"key{index}", "value");

        var exception = Assert.Throws<ArgumentException>(() => builder.AddMetadata("KEY0", "duplicate"));

        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'KEY0' duplicates existing metadata name 'key0'. (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullMetadataValueInSequence_ThrowsHelpfulExceptionWithIndexAndDoesNotMutate()
    {
        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Metadata validation")
            .TextBody("Metadata validation");
        IEnumerable<KeyValuePair<string, string>> metadata =
        [
            new("first", "value"),
            new("second", null!)
        ];

        var exception = Assert.Throws<ArgumentNullException>(() => builder.AddMetadata(metadata));

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("The metadata value at index 1 cannot be null. (Parameter 'metadata')", exception.Message);
        Assert.Null(builder.Build()
            .Metadata);
    }

    [Fact]
    public void EmailBuilder_WithMetadataSequenceExceedingLimit_ThrowsHelpfulExceptionAndDoesNotPartiallyMutate()
    {
        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Metadata validation")
            .TextBody("Metadata validation")
            .AddMetadata("existing", "value");
        var metadata = Enumerable.Range(0, 10)
            .Select(static index => new KeyValuePair<string, string>($"key{index}", "value"));

        var exception = Assert.Throws<ArgumentException>(() => builder.AddMetadata(metadata));

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("Cannot set more than 10 metadata fields for a message. Adding 10 metadata fields to the existing 1 would produce 11. (Parameter 'metadata')", exception.Message);

        var email = builder.Build();
        Assert.NotNull(email.Metadata);
        Assert.Single(email.Metadata);
        Assert.True(email.Metadata.ContainsKey("existing"));
        Assert.False(email.Metadata.ContainsKey("key0"));
    }

    [Fact]
    public void EmailBuilder_WithNullTag_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .WithTag(null!));

        Assert.Equal("tag", exception.ParamName);
        Assert.Equal("The tag cannot be null. (Parameter 'tag')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithTooLongTag_ThrowsHelpfulExceptionWithActualLength()
    {
        var tag = new string('a', 1001);

        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .WithTag(tag));

        Assert.Equal("The tag cannot be longer than 1000 characters. Actual length: 1001. (Parameter 'tag')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_FromTemplateWithNullTemplateAlias_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.FromTemplate(null!));

        Assert.Equal("templateAlias", exception.ParamName);
        Assert.Equal("The template alias cannot be null. (Parameter 'templateAlias')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_FromTemplateWithTooLongTemplateAlias_ThrowsHelpfulExceptionWithActualLength()
    {
        var templateAlias = new string('a', 65);

        var exception = Assert.Throws<ArgumentException>(() => Email.FromTemplate(templateAlias));

        Assert.Equal("templateAlias", exception.ParamName);
        Assert.Equal("The template alias must not exceed 64 characters. Actual length: 65. (Parameter 'templateAlias')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_FromTemplateWithTemplateAliasContainingInvalidCharacter_ThrowsHelpfulExceptionWithIndex()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.FromTemplate("welcome email"));

        Assert.Equal("templateAlias", exception.ParamName);
        Assert.Equal("The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters. Invalid character space at index 7. (Parameter 'templateAlias')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullAttachmentInSequence_ThrowsHelpfulExceptionWithIndex()
    {
        var attachment = Attachment.Create("test.txt", "text/plain", "content"u8.ToArray());
        IEnumerable<Attachment> attachments = [attachment, null!];

        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .AddAttachment(attachments));

        Assert.Equal("attachments", exception.ParamName);
        Assert.Equal("The attachment at index 1 cannot be null. (Parameter 'attachments')", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithNullTemplateModel_ThrowsException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            Email.FromTemplate(41813873)
                .From("sender@postkit.com")
                .To("recipient@postkit.com")
                .WithModel(null!);
        });

        Assert.Equal("templateModel", exception.ParamName);
        Assert.Equal("The template model cannot be null. (Parameter 'templateModel')", exception.Message);
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
        });

        Assert.Equal("TemplateModel is required when TemplateId or TemplateAlias is set. Call WithModel(...).", exception.Message);
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
        });

        Assert.Equal("Cannot set From because it has already been set.", exception.Message);
    }

    [Fact]
    public void EmailBuilder_WithEmojiSubjectExceedingUtf16Limit_ThrowsException()
    {
        var subject = string.Concat(Enumerable.Repeat("😀", 1001));

        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .Subject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. Actual length: 2002. (Parameter 'subject')", exception.Message);
    }
}