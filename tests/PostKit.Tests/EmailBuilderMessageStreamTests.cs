using PostKit.Emails;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class EmailBuilderMessageStreamTests
{
    [Fact]
    public void UsingMessageStream_WithString_SetsMessageStreamOnEmailRequest()
    {
        const string messageStreamId = "custom-stream-id";

        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Test Email")
            .HtmlBody("<p>Hello</p>")
            .UseMessageStream(messageStreamId)
            .Build();

        var request = email.ToEmailRequest();

        Assert.Equal(messageStreamId, request.MessageStream);
    }

    [Fact]
    public void UsingMessageStream_WithUnderscore_SetsMessageStreamOnEmailRequest()
    {
        const string messageStreamId = "custom_stream-id";

        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Test Email")
            .HtmlBody("<p>Hello</p>")
            .UseMessageStream(messageStreamId)
            .Build();

        var request = email.ToEmailRequest();

        Assert.Equal(messageStreamId, request.MessageStream);
    }

    [Fact]
    public void UsingMessageStream_WithLeadingUnderscore_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .UseMessageStream("_marketing")
        );

        Assert.Equal("The message stream ID is invalid. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithReservedPrefix_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .UseMessageStream("pm-marketing")
        );

        Assert.Equal("The message stream ID is invalid. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithReservedAllId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .UseMessageStream("all")
        );

        Assert.Equal("The message stream ID is invalid. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithNullString_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .UseMessageStream(null!)
        );

        Assert.Equal("messageStreamId", exception.ParamName);
    }
}
