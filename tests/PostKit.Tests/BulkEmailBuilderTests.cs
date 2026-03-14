using System.Text.Json;
using System.Text.Json.Serialization;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class BulkEmailBuilderTests
{
    [Fact]
    public void Build_WithReplyToInterfaceChaining_Succeeds()
    {
        var builder = BulkEmail.Compose()
            .ReplyTo("reply@postkit.com");

        var bulkEmail = builder
            .ReplyTo("other-reply@postkit.com")
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

        var message = builder
            .To("second@postkit.com")
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
    public void Build_WithoutAnyMessageRecipients_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmailMessage.Compose()
            .Build());

        Assert.Equal("At least one recipient is required.", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithTransactionalStream_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream(MessageStream.Transactional));

        Assert.Contains("only supports broadcast message streams", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UsingMessageStream_WithOutboundStreamId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream("outbound"));

        Assert.Contains("only supports broadcast message streams", exception.Message, StringComparison.Ordinal);
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
    public void UsingMessageStream_WithReservedPrefix_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .UseMessageStream("pm-broadcast"));

        Assert.Equal("The message stream ID is invalid. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void WithTag_WithNullTag_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => BulkEmail.Compose()
            .WithTag(null!));

        Assert.Equal("tag", exception.ParamName);
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

        Assert.Equal("The template model could not be serialized. (Parameter 'templateModel')", exception.Message);
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
    public void WithTemplateModel_WithPerCallSerializerOptions_PreservesExplicitPropertyNames()
    {
        var builder = BulkEmailMessage.FromTemplate()
            .To("recipient@postkit.com");

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

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

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void WithSubject_WhenEmojiExceedsUtf16Limit_Throws()
    {
        var subject = string.Concat(Enumerable.Repeat("😀", 1001));

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.Compose()
            .Subject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void Build_WithLargePerMessageHeadersButContentWithinLimit_Succeeds()
    {
        var textBody = new string('a', 4 * 1024 * 1024);
        var largeHeaderValue = new string('h', 1024 * 1024);
        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .Subject("Hello")
            .TextBody(textBody);

        for (var index = 0; index < 47; index++)
        {
            builder.AddMessage(BulkEmailMessage.Compose()
                .To($"recipient{index}@postkit.com")
                .AddHeader("X-Large-Header", largeHeaderValue)
                .Build());
        }

        var bulkEmail = builder.Build();

        Assert.Equal(47, bulkEmail.Messages.Count);
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
    public void Build_WithLargePerMessageTemplateModelsPushingPastEstimatedBulkLimit_Throws()
    {
        var builder = BulkEmail.FromTemplate(42)
            .From("sender@postkit.com");

        for (var index = 0; index < 11; index++)
        {
            builder.AddMessage(BulkEmailMessage.FromTemplate()
                .To($"recipient{index}@postkit.com")
                .WithModel(new { Data = new string('x', 5 * 1024 * 1024) })
                .Build());
        }

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated bulk request size exceeds Postmark's 50 MB limit.", exception.Message);
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

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated message content exceeds Postmark's 10 MB limit.", exception.Message);
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

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated message content exceeds Postmark's 10 MB limit.", exception.Message);
    }

    private sealed class CyclicTemplateModel
    {
        public CyclicTemplateModel? Self { get; private set; }

        public static CyclicTemplateModel Create()
        {
            var model = new CyclicTemplateModel();
            model.Self = model;
            return model;
        }
    }
}
