using PostKit.Emails;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class EmailBuilderMessageStreamTests
{
    [Fact]
    public void UsingMessageStream_WithString_SetsMessageStreamOnEmailRequest()
    {
        const string messageStreamId = "custom-stream-id";

        var email = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Test Email")
            .WithHtmlBody("<p>Hello</p>")
            .UsingMessageStream(messageStreamId)
            .Build();

        var request = email.ToEmailRequest();

        Assert.Equal(messageStreamId, request.MessageStream);
    }

    [Fact]
    public void UsingMessageStream_WithUnderscore_SetsMessageStreamOnEmailRequest()
    {
        const string messageStreamId = "custom_stream-id";

        var email = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Test Email")
            .WithHtmlBody("<p>Hello</p>")
            .UsingMessageStream(messageStreamId)
            .Build();

        var request = email.ToEmailRequest();

        Assert.Equal(messageStreamId, request.MessageStream);
    }

    [Fact]
    public void UsingMessageStream_WithReservedPrefix_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.CreateBuilder()
            .UsingMessageStream("pm-marketing"));

        Assert.Equal("The message stream ID is invalid. (Parameter 'messageStreamId')", exception.Message);
    }
}
