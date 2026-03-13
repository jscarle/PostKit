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
        IBulkEmailReplyToBuilder builder = BulkEmail.CreateBuilder()
            .ReplyTo("reply@postkit.com");

        var bulkEmail = builder
            .AlsoReplyTo("other-reply@postkit.com")
            .From("sender@postkit.com")
            .WithSubject("Hello")
            .WithTextBody("Hello world")
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        Assert.NotNull(bulkEmail.ReplyTo);
        Assert.Equal(2, bulkEmail.ReplyTo.Count);
    }

    [Fact]
    public void Build_WithMessageInterfaceChaining_Succeeds()
    {
        IBulkEmailMessageToBuilder builder = BulkEmailMessage.CreateBuilder()
            .To("first@postkit.com");

        var message = builder
            .AlsoTo("second@postkit.com")
            .WithMetadata("FirstName", "Alice")
            .Build();

        Assert.NotNull(message.To);
        Assert.Equal(2, message.To.Count);
        Assert.NotNull(message.Metadata);
        Assert.Equal("Alice", message.Metadata["FirstName"]);
    }

    [Fact]
    public void Build_WithCcOnlyMessage_Succeeds()
    {
        var bulkEmail = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .WithSubject("Hello")
            .WithTextBody("Hello world")
            .AddMessage(BulkEmailMessage.CreateBuilder()
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
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmailMessage.CreateBuilder()
            .Build());

        Assert.Equal("At least one recipient is required.", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithTransactionalStream_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.CreateBuilder()
            .UsingMessageStream(MessageStream.Transactional));

        Assert.Contains("only supports broadcast message streams", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UsingMessageStream_WithOutboundStreamId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.CreateBuilder()
            .UsingMessageStream("outbound"));

        Assert.Contains("only supports broadcast message streams", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UsingMessageStream_WithUnderscoreStreamId_Succeeds()
    {
        var bulkEmail = BulkEmail.CreateBuilder()
            .UsingMessageStream("broadcast_stream")
            .From("sender@postkit.com")
            .WithSubject("Hello")
            .WithTextBody("Hello world")
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To("recipient@postkit.com")
                .Build())
            .Build();

        Assert.Equal("broadcast_stream", bulkEmail.MessageStream);
    }

    [Fact]
    public void UsingMessageStream_WithReservedPrefix_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.CreateBuilder()
            .UsingMessageStream("pm-broadcast"));

        Assert.Equal("The message stream ID is invalid. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void Build_WithTemplateAndNoBodies_Succeeds()
    {
        var bulkEmail = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .UsingTemplate("welcome-email")
            .AddMessage(BulkEmailMessage.CreateBuilder()
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
        var exception = Assert.Throws<ArgumentException>(() => BulkEmailMessage.CreateBuilder()
            .WithTemplateModel(CyclicTemplateModel.Create()));

        Assert.Equal("The template model could not be serialized. (Parameter 'templateModel')", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void WithTemplateModel_WithScalarModel_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => BulkEmailMessage.CreateBuilder()
            .WithTemplateModel("Alice"));

        Assert.Equal("The template model must serialize to a JSON object. (Parameter 'templateModel')", exception.Message);
    }

    [Fact]
    public void WithTemplateModel_WithPerCallSerializerOptions_PreservesExplicitPropertyNames()
    {
        IBulkEmailMessageBuilder builder = BulkEmailMessage.CreateBuilder()
            .To("recipient@postkit.com");

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        var bulkEmail = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .UsingTemplate(42)
            .AddMessage(builder.WithTemplateModel(new { FirstName = "Alice" }, serializerOptions)
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

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.CreateBuilder()
            .WithSubject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void WithSubject_WhenEmojiExceedsUtf16Limit_Throws()
    {
        var subject = string.Concat(Enumerable.Repeat("😀", 1001));

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.CreateBuilder()
            .WithSubject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }

    [Fact]
    public void Build_WithLargePerMessageHeadersPushingPastEstimatedBulkLimit_Throws()
    {
        var textBody = new string('a', 4 * 1024 * 1024);
        var largeHeaderValue = new string('h', 1024 * 1024);
        var builder = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .WithSubject("Hello")
            .WithTextBody(textBody);

        for (var index = 0; index < 47; index++)
        {
            builder.AddMessage(BulkEmailMessage.CreateBuilder()
                .To($"recipient{index}@postkit.com")
                .WithHeader("X-Large-Header", largeHeaderValue)
                .Build());
        }

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated bulk request size exceeds Postmark's 50 MB limit.", exception.Message);
    }

    [Fact]
    public void Build_WithPerMessageTemplateModelAndNoTemplate_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .WithSubject("Hello")
            .WithTextBody("Hello world")
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To("recipient@postkit.com")
                .WithTemplateModel(new { Name = "Alice" })
                .Build())
            .Build());

        Assert.Equal("A template ID or alias is required when using per-message template models.", exception.Message);
    }

    [Fact]
    public void Build_WithLargePerMessageTemplateModelsPushingPastEstimatedBulkLimit_Throws()
    {
        var builder = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .UsingTemplate(42);

        for (var index = 0; index < 11; index++)
        {
            builder.AddMessage(BulkEmailMessage.CreateBuilder()
                .To($"recipient{index}@postkit.com")
                .WithTemplateModel(new { Data = new string('x', 5 * 1024 * 1024) })
                .Build());
        }

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Equal("Estimated bulk request size exceeds Postmark's 50 MB limit.", exception.Message);
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
