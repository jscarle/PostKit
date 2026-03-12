using PostKit.BulkEmails;
using PostKit.Common;

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
    public void Build_WithTemplateAndNoBodies_Succeeds()
    {
        var bulkEmail = BulkEmail.CreateBuilder()
            .From("sender@postkit.com")
            .WithTemplate("welcome-email")
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
    public void WithSubject_WhenLongerThan2000Characters_Throws()
    {
        var subject = new string('S', 2001);

        var exception = Assert.Throws<ArgumentException>(() => BulkEmail.CreateBuilder()
            .WithSubject(subject));

        Assert.Equal("The subject cannot be longer than 2000 characters. (Parameter 'subject')", exception.Message);
    }
}
