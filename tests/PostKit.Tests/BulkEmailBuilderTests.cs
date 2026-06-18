using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MimeKit;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class BulkEmailBuilderTests
{
    private const string MessageStreamRuleMessage =
        "The message stream ID must be 1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'.";

    [Fact]
    public void Build_WithReplyToInterfaceChaining_Succeeds()
    {
        var builder = BulkEmail.Compose()
            .ReplyTo("reply@postkit.com");

        var bulkEmail = builder.ReplyTo("other-reply@postkit.com")
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        Assert.NotNull(bulkEmail.ReplyTo);
        Assert.Equal(2, bulkEmail.ReplyTo.Count);
    }

    [Fact]
    public void Build_WithMessageInterfaceChaining_Succeeds()
    {
        var builder = BulkEmailMessage.Compose()
            .To("first@postkit.com");

        var message = builder.To("second@postkit.com")
            .AddMetadata("FirstName", "Alice")
            .Build();

        Assert.NotNull(message.To);
        Assert.Equal(2, message.To.Count);
        Assert.NotNull(message.Metadata);
        Assert.Equal("Alice", message.Metadata["FirstName"]);
    }

    [Fact]
    public void Build_WithCcOnlyMessage_Succeeds()
    {
        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .Cc("cc@postkit.com")
                .Build())
            .Build();

        Assert.Equal("sender@postkit.com", bulkEmail.From.Address);
        Assert.Equal("Hello", bulkEmail.Subject);
        Assert.Single(bulkEmail.Messages);
        Assert.Null(bulkEmail.Messages[0].To);
        Assert.NotNull(bulkEmail.Messages[0].Cc);
    }

    [Fact]
    public void From_WithTooLongAddress_ThrowsHelpfulExceptionWithActualLength()
    {
        var displayName = new string('a', 260);
        var mailboxAddress = new MailboxAddress(displayName, "sender@postkit.com");
        var expectedLength = mailboxAddress.ToString(true)
            .Length;

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .From("sender@postkit.com", displayName));

        Assert.Equal("address", exception.ParamName);
        Assert.Equal($"The From address cannot exceed 255 characters. Actual length: {expectedLength}. (Parameter 'address')", exception.Message);
    }

    [Fact]
    public void Build_WithEmptyMetadataValues_Succeeds()
    {
        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .AddMetadata("campaign", string.Empty)
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .AddMetadata("recipient", string.Empty)
                .Build())
            .Build();

        Assert.NotNull(bulkEmail.Metadata);
        Assert.Equal(string.Empty, bulkEmail.Metadata["campaign"]);
        var messageMetadata = bulkEmail.Messages[0].Metadata;
        Assert.NotNull(messageMetadata);
        Assert.Equal(string.Empty, messageMetadata["recipient"]);
    }

    [Fact]
    public void Build_WithNullMetadataValues_ThrowsHelpfulException()
    {
        var bulkException = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .AddMetadata("campaign", null!));
        var messageException = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.Compose()
            .AddMetadata("recipient", null!));

        Assert.Equal("value", bulkException.ParamName);
        Assert.Equal("The metadata value cannot be null. (Parameter 'value')", bulkException.Message);
        Assert.Equal("value", messageException.ParamName);
        Assert.Equal("The metadata value cannot be null. (Parameter 'value')", messageException.Message);
    }

    [Fact]
    public void Build_WithDuplicateSingleMetadataValues_ThrowsHelpfulExceptionWithName()
    {
        var bulkBuilder = BulkEmail.Compose()
            .AddMetadata("campaign", "spring");
        var messageBuilder = BulkEmailMessage.Compose()
            .AddMetadata("recipient", "alpha");

        var bulkException = Assert.Throws<ArgumentException>(() => bulkBuilder.AddMetadata("CAMPAIGN", "summer"));
        var messageException = Assert.Throws<ArgumentException>(() => messageBuilder.AddMetadata("RECIPIENT", "beta"));

        Assert.Equal("name", bulkException.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'CAMPAIGN' duplicates existing metadata name 'campaign'. (Parameter 'name')", bulkException.Message);
        Assert.Equal("name", messageException.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'RECIPIENT' duplicates existing metadata name 'recipient'. (Parameter 'name')", messageException.Message);
    }

    [Fact]
    public void Build_AddingEleventhSingleMetadataField_ThrowsHelpfulExceptionWithCounts()
    {
        var bulkBuilder = BulkEmail.Compose();
        var messageBuilder = BulkEmailMessage.Compose();
        for (var index = 0; index < 10; index++)
        {
            bulkBuilder.AddMetadata($"bulk{index}", "value");
            messageBuilder.AddMetadata($"message{index}", "value");
        }

        var bulkException = Assert.Throws<InvalidOperationException>(() => bulkBuilder.AddMetadata("extra", "value"));
        var messageException = Assert.Throws<InvalidOperationException>(() => messageBuilder.AddMetadata("extra", "value"));

        Assert.Equal("Cannot set more than 10 metadata fields for a message. Adding 1 metadata field to the existing 10 would produce 11.", bulkException.Message);
        Assert.Equal("Cannot set more than 10 metadata fields for a message. Adding 1 metadata field to the existing 10 would produce 11.", messageException.Message);
    }

    [Fact]
    public void Build_AddingDuplicateSingleMetadataFieldWhenAtLimit_ThrowsDuplicateNameException()
    {
        var bulkBuilder = BulkEmail.Compose();
        var messageBuilder = BulkEmailMessage.Compose();
        for (var index = 0; index < 10; index++)
        {
            bulkBuilder.AddMetadata($"bulk{index}", "value");
            messageBuilder.AddMetadata($"message{index}", "value");
        }

        var bulkException = Assert.Throws<ArgumentException>(() => bulkBuilder.AddMetadata("BULK0", "duplicate"));
        var messageException = Assert.Throws<ArgumentException>(() => messageBuilder.AddMetadata("MESSAGE0", "duplicate"));

        Assert.Equal("name", bulkException.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'BULK0' duplicates existing metadata name 'bulk0'. (Parameter 'name')", bulkException.Message);
        Assert.Equal("name", messageException.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'MESSAGE0' duplicates existing metadata name 'message0'. (Parameter 'name')", messageException.Message);
    }

    [Theory]
    [InlineData("From")]
    [InlineData("ReplyTo")]
    public void BulkEmailBuilder_DisplayNameOverloadWithOldOrder_ThrowsHelpfulException(string methodName)
    {
        var builder = BulkEmail.Compose();

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (methodName)
            {
                case "From":
                    builder.From("Sender Name", "sender@postkit.com");
                    break;
                case "ReplyTo":
                    builder.ReplyTo("Reply Target", "reply@postkit.com");
                    break;
            }
        });

        Assert.Equal("address", exception.ParamName);
        Assert.Equal($"Display name overloads must specify the email address first: use .{methodName}(\"recipient@example.com\", \"Recipient Name\"). (Parameter 'address')", exception.Message);
    }

    [Theory]
    [InlineData("To")]
    [InlineData("Cc")]
    [InlineData("Bcc")]
    public void BulkEmailMessageBuilder_DisplayNameOverloadWithOldOrder_ThrowsHelpfulException(string methodName)
    {
        var builder = BulkEmailMessage.Compose();

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (methodName)
            {
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
    public void Build_WithoutAnyMessageRecipients_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmailMessage.Compose()
            .Build());

        Assert.Equal("At least one recipient is required before building the bulk email message. Call To(...), Cc(...), or Bcc(...).", exception.Message);
    }

    [Fact]
    public void Build_WithoutFrom_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmail.Compose()
            .Subject("Hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build());

        Assert.Equal("From address is required before building the bulk email. Call From(...).", exception.Message);
    }

    [Fact]
    public void Build_WithoutMessages_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .Build());

        Assert.Equal("At least one message is required before building the bulk email. Call AddMessage(...).", exception.Message);
    }

    [Fact]
    public void Build_WithoutSubject_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmail.Compose()
            .From("sender@postkit.com")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build());

        Assert.Equal("Subject is required before building the bulk email. Call Subject(...).", exception.Message);
    }

    [Fact]
    public void Build_WithoutBody_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build());

        Assert.Equal("Message content is required before building the bulk email. Call TextBody(...) or HtmlBody(...).", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithTransactionalStream_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream(MessageStream.Transactional));

        Assert.Equal("Bulk sends require a broadcast message stream. Received MessageStream.Transactional; use MessageStream.Broadcast or a broadcast stream ID. (Parameter 'messageStream')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithOutboundStreamId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream("outbound"));

        Assert.Equal("Bulk sends require a broadcast message stream. Received 'outbound'; use MessageStream.Broadcast or a broadcast stream ID. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithUnderscoreStreamId_Succeeds()
    {
        var bulkEmail = BulkEmail.Compose()
            .UseMessageStream("broadcast_stream")
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        Assert.Equal("broadcast_stream", bulkEmail.MessageStream);
    }

    [Fact]
    public void UsingMessageStream_WithLeadingUnderscore_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream("_broadcast"));

        Assert.Equal($"{MessageStreamRuleMessage} First character must be a lowercase letter. Received '_' at index 0. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithReservedPrefix_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream("pm-broadcast"));

        Assert.Equal($"{MessageStreamRuleMessage} The prefix 'pm-' is reserved. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithReservedAllId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream("all"));

        Assert.Equal($"{MessageStreamRuleMessage} 'all' is reserved. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithNullString_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .UseMessageStream(null!));

        Assert.Equal("messageStreamId", exception.ParamName);
        Assert.Equal("The message stream ID cannot be null. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void WithTag_WithNullTag_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .WithTag(null!));

        Assert.Equal("tag", exception.ParamName);
        Assert.Equal("The tag cannot be null. (Parameter 'tag')", exception.Message);
    }

    [Fact]
    public void WithTag_WithTooLongTag_ThrowsHelpfulExceptionWithActualLength()
    {
        var tag = new string('a', 1001);

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .WithTag(tag));

        Assert.Equal("The tag cannot be longer than 1000 characters. Actual length: 1001. (Parameter 'tag')", exception.Message);
    }

    [Fact]
    public void FromTemplate_WithNullTemplateAlias_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.FromTemplate(null!));

        Assert.Equal("templateAlias", exception.ParamName);
        Assert.Equal("The template alias cannot be null. (Parameter 'templateAlias')", exception.Message);
    }

    [Fact]
    public void FromTemplate_WithInvalidTemplateId_ThrowsHelpfulExceptionWithActualValue()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.FromTemplate(0));

        Assert.Equal("templateId", exception.ParamName);
        Assert.Equal("The template ID must be greater than zero. Received 0. (Parameter 'templateId')", exception.Message);
    }

    [Fact]
    public void FromTemplate_WithTooLongTemplateAlias_ThrowsHelpfulExceptionWithActualLength()
    {
        var templateAlias = new string('a', 65);

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.FromTemplate(templateAlias));

        Assert.Equal("templateAlias", exception.ParamName);
        Assert.Equal("The template alias must not exceed 64 characters. Actual length: 65. (Parameter 'templateAlias')", exception.Message);
    }

    [Fact]
    public void FromTemplate_WithTemplateAliasStartingWithDigit_ThrowsHelpfulExceptionWithInvalidCharacter()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.FromTemplate("1welcome"));

        Assert.Equal("templateAlias", exception.ParamName);
        Assert.Equal("The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters. First character must be a letter. Received '1' at index 0. (Parameter 'templateAlias')",
            exception.Message);
    }

    [Fact]
    public void AddAttachment_WithNullAttachmentInSequence_ThrowsHelpfulExceptionWithIndex()
    {
        var attachment = Attachment.Create("test.txt", "text/plain", "content"u8.ToArray());
        IEnumerable<Attachment> attachments = [attachment, null!];

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .AddAttachment(attachments));

        Assert.Equal("attachments", exception.ParamName);
        Assert.Equal("The attachment at index 1 cannot be null. (Parameter 'attachments')", exception.Message);
    }

    [Fact]
    public void AddMessage_WithNullMessageInSequence_ThrowsHelpfulExceptionWithIndex()
    {
        var message = BulkEmailMessage.Compose()
            .To("recipient@postkit.com")
            .Build();
        IEnumerable<BulkEmailMessage> messages = [message, null!];

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .AddMessage(messages));

        Assert.Equal("messages", exception.ParamName);
        Assert.Equal("The bulk email message at index 1 cannot be null. (Parameter 'messages')", exception.Message);
    }

    [Fact]
    public void AddMetadata_WithSequenceDuplicatingExistingMetadata_ThrowsHelpfulExceptionAndDoesNotPartiallyMutate()
    {
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Metadata validation")
            .TextBody("Metadata validation")
            .AddMetadata("campaign", "spring")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build());
        IEnumerable<KeyValuePair<string, string>> metadata =
        [
            new("segment", "beta"),
            new("CAMPAIGN", "duplicate")
        ];

        var exception = Assert.Throws<ArgumentException>(() => builder.AddMetadata(metadata));

        Assert.Equal("metadata", exception.ParamName);
        Assert.Equal("Metadata names must be unique and are compared case-insensitively. Metadata name 'CAMPAIGN' at index 1 duplicates existing metadata name 'campaign'. (Parameter 'metadata')", exception.Message);

        var bulkEmail = builder.Build();
        Assert.NotNull(bulkEmail.Metadata);
        Assert.Single(bulkEmail.Metadata);
        Assert.True(bulkEmail.Metadata.ContainsKey("campaign"));
        Assert.False(bulkEmail.Metadata.ContainsKey("segment"));
    }

    [Fact]
    public void Build_WithTemplateAndNoBodies_Succeeds()
    {
        var bulkEmail = BulkEmail.FromTemplate("welcome-email")
            .From("sender@postkit.com")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        Assert.Equal("welcome-email", bulkEmail.TemplateAlias);
        Assert.Null(bulkEmail.Subject);
        Assert.Null(bulkEmail.HtmlBody);
        Assert.Null(bulkEmail.TextBody);
    }

    [Fact]
    public void WithTemplateModel_WithUnserializableModel_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmailMessage.FromTemplate()
            .WithModel(CyclicTemplateModel.Create()));

        Assert.Equal("The template model could not be serialized to a JSON object. Ensure it does not contain cycles or members unsupported by System.Text.Json. (Parameter 'templateModel')", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void WithTemplateModel_WithScalarModel_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmailMessage.FromTemplate()
            .WithModel("Alice"));

        Assert.Equal("The template model must serialize to a JSON object. (Parameter 'templateModel')", exception.Message);
    }

    [Fact]
    public void WithTemplateModel_WithNullSerializerOptions_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmailMessage.FromTemplate()
            .WithModel(new { Name = "Alice" }, null!));

        Assert.Equal("serializerOptions", exception.ParamName);
        Assert.Equal("The serializer options cannot be null. (Parameter 'serializerOptions')", exception.Message);
    }

    [Fact]
    public void WithTemplateModel_WithPerCallSerializerOptions_PreservesExplicitPropertyNames()
    {
        var builder = BulkEmailMessage.FromTemplate()
            .To("recipient@postkit.com");

        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        var bulkEmail = BulkEmail.FromTemplate(42)
            .From("sender@postkit.com")
            .AddMessage(builder.WithModel(new { FirstName = "Alice" }, serializerOptions)
                .Build())
            .Build();

        var request = bulkEmail.ToBulkEmailRequest();
        var message = Assert.Single(request.Messages);

        Assert.NotNull(message.TemplateModel);
        Assert.Equal("Alice", message.TemplateModel["FirstName"]!.GetValue<string>());
        Assert.Null(message.TemplateModel["firstName"]);
    }

    [Fact]
    public void WithSubject_WhenLongerThan2000Characters_Throws()
    {
        var subject = new string('S', 2001);

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .Subject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. Actual length: 2001. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void WithSubject_WhenEmojiExceedsUtf16Limit_Throws()
    {
        var subject = string.Concat(Enumerable.Repeat("😀", 1001));

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .Subject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. Actual length: 2002. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void BulkEmailMessageBuild_WithTooManyRecipients_ThrowsHelpfulExceptionWithActualCount()
    {
        var builder = BulkEmailMessage.Compose();
        for (var index = 0; index < 51; index++)
            builder.To($"recipient{index}@postkit.com");

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Contains("too many recipients", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Actual recipient count: 51.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WithLargePerMessageHeadersPushingPastEstimatedBulkLimit_Throws()
    {
        var textBody = new string('a', 4 * 1024 * 1024);
        var largeHeaderValue = new string('h', 1024 * 1024);
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody(textBody);

        for (var index = 0; index < 47; index++)
            builder.AddMessage(BulkEmailMessage.Compose()
                .To($"recipient{index}@postkit.com")
                .AddHeader("X-Large-Header", largeHeaderValue)
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedBulkSizeMessage(exception);
    }

    [Fact]
    public void Build_WithPerMessageTemplateModelAndNoTemplate_Succeeds()
    {
        var bulkEmail = BulkEmail.FromTemplate(42)
            .From("sender@postkit.com")
            .AddMessage(BulkEmailMessage.FromTemplate()
                .To("recipient@postkit.com")
                .WithModel(new { Name = "Alice" })
                .Build())
            .Build();

        var request = bulkEmail.ToBulkEmailRequest();
        var message = Assert.Single(request.Messages);

        Assert.NotNull(message.TemplateModel);
        Assert.Equal("Alice", message.TemplateModel["Name"]!.GetValue<string>());
    }

    [Fact]
    public void Build_WithRequestAndMessageMetadataExceedingTenCombinedValues_Throws()
    {
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world");

        for (var index = 0; index < 10; index++)
            builder.AddMetadata($"root{index}", "value");

        builder.AddMessage(BulkEmailMessage.Compose()
            .To("recipient@postkit.com")
            .AddMetadata("message-extra", "value")
            .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Equal("Cannot set more than 10 metadata fields for bulk message at index 0 after combining request-level and message-level metadata. Request-level count: 10; message-level count: 1; combined count: 11.", exception.Message);
    }

    [Fact]
    public void Build_WithMessageMetadataDuplicatingRequestMetadataCaseInsensitive_Throws()
    {
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world");

        for (var index = 0; index < 10; index++)
            builder.AddMetadata($"root{index}", "value");

        builder.AddMessage(BulkEmailMessage.Compose()
            .To("recipient@postkit.com")
            .AddMetadata("ROOT0", "override")
            .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Equal(
            "Cannot use duplicate metadata names for bulk message at index 0 after combining request-level and message-level metadata. Metadata name 'ROOT0' duplicates request-level metadata name 'root0'. Metadata names are compared case-insensitively.",
            exception.Message);
    }

    [Fact]
    public void Build_WithMessageHeaderDuplicatingRequestHeaderCaseInsensitive_Throws()
    {
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .AddHeader("List-Unsubscribe-Post", "List-Unsubscribe=One-Click")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .AddHeader("list-unsubscribe-post", "List-Unsubscribe=One-Click")
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Equal(
            "Cannot use duplicate header names for bulk message at index 0 after combining request-level and message-level headers. Header name 'list-unsubscribe-post' duplicates request-level header name 'List-Unsubscribe-Post'. Header names are compared case-insensitively.",
            exception.Message);
    }

    [Fact]
    public void Build_WithDifferentRequestAndMessageHeaders_Succeeds()
    {
        var bulkEmail = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody("Hello world")
            .AddHeader("X-Campaign", "launch")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .AddHeader("List-Unsubscribe-Post", "List-Unsubscribe=One-Click")
                .Build())
            .Build();

        Assert.NotNull(bulkEmail.Headers);
        Assert.Equal("launch", bulkEmail.Headers["X-Campaign"]);
        var messageHeaders = bulkEmail.Messages[0].Headers;
        Assert.NotNull(messageHeaders);
        Assert.Equal("List-Unsubscribe=One-Click", messageHeaders["List-Unsubscribe-Post"]);
    }

    [Fact]
    public void Build_WithMissingSubjectAndMergedMetadataDuplicate_ReportsMissingSubjectFirst()
    {
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .TextBody("Hello world")
            .AddMetadata("root", "value")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .AddMetadata("ROOT", "override")
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Equal("Subject is required before building the bulk email. Call Subject(...).", exception.Message);
    }

    [Fact]
    public void Build_WithMissingBodyAndMergedMetadataDuplicate_ReportsMissingBodyFirst()
    {
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .AddMetadata("root", "value")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .AddMetadata("ROOT", "override")
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.Equal("Message content is required before building the bulk email. Call TextBody(...) or HtmlBody(...).", exception.Message);
    }

    [Fact]
    public void Build_WithLargePerMessageTemplateModelsPushingPastEstimatedBulkLimit_Throws()
    {
        var builder = BulkEmail.FromTemplate(42)
            .From("sender@postkit.com");

        for (var index = 0; index < 11; index++)
            builder.AddMessage(BulkEmailMessage.FromTemplate()
                .To($"recipient{index}@postkit.com")
                .WithModel(new { Data = new string('x', 5 * 1024 * 1024) })
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedBulkSizeMessage(exception);
    }

    [Fact]
    public void Build_WithLargeBodyAndAttachmentCombination_Throws()
    {
        var textBody = new string('a', (int)PostmarkSizeEstimator.BodySizeLimitInBytes);
        var attachment = Attachment.Create("large.dat", "application/octet-stream", new byte[6 * 1024 * 1024]);

        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Oversized bulk message")
            .AddAttachment(attachment)
            .TextBody(textBody)
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedMessageSizeMessage(exception);
    }

    [Fact]
    public void Build_WithBase64AttachmentAddedBeforeBodyStillExceedingLimit_Throws()
    {
        var textBody = new string('a', 4_500_000);
        var attachment = Attachment.Create("large.dat", "application/octet-stream", new byte[5 * 1024 * 1024]);

        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Oversized bulk message")
            .AddAttachment(attachment)
            .TextBody(textBody)
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        AssertEstimatedMessageSizeMessage(exception);
    }

    [Fact]
    public void Build_WithPerMessageTemplateModelPushingPastMessageLimit_Throws()
    {
        var builder = BulkEmail.FromTemplate(42)
            .From("sender@postkit.com")
            .AddMessage(BulkEmailMessage.FromTemplate()
                .To("recipient@postkit.com")
                .WithModel(new { Data = new string('x', 11 * 1024 * 1024) })
                .Build());

        var exception = Assert.Throws<InvalidOperationException>(builder.Build);

        Assert.StartsWith("Estimated message content for bulk message at index 0 exceeds Postmark's 10 MB limit. Estimated size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 10,485,760 bytes.", exception.Message, StringComparison.Ordinal);
    }

    private static void AssertEstimatedMessageSizeMessage(InvalidOperationException exception)
    {
        Assert.StartsWith("Estimated message content exceeds Postmark's 10 MB limit. Estimated size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 10,485,760 bytes.", exception.Message, StringComparison.Ordinal);
    }

    private static void AssertEstimatedBulkSizeMessage(InvalidOperationException exception)
    {
        Assert.StartsWith("Estimated bulk request size exceeds Postmark's 50 MB limit. Estimated size: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Limit: 52,428,800 bytes.", exception.Message, StringComparison.Ordinal);
    }

    private sealed class CyclicTemplateModel
    {
        public CyclicTemplateModel? Self { [UsedImplicitly] get; private set; }

        public static CyclicTemplateModel Create()
        {
            var model = new CyclicTemplateModel();
            model.Self = model;
            return model;
        }
    }
}