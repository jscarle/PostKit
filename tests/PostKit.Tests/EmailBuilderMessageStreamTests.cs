using PostKit.Emails;
using PostKit.Postmark.Common;

namespace PostKit.Tests;

public class EmailBuilderMessageStreamTests
{
    private const string MessageStreamRuleMessage =
        "The message stream ID must be 1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'.";

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
            .UseMessageStream("_marketing"));

        Assert.Equal($"{MessageStreamRuleMessage} First character must be a lowercase letter. Received '_' at index 0. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithReservedPrefix_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .UseMessageStream("pm-marketing"));

        Assert.Equal($"{MessageStreamRuleMessage} The prefix 'pm-' is reserved. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithReservedAllId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .UseMessageStream("all"));

        Assert.Equal($"{MessageStreamRuleMessage} 'all' is reserved. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithTooLongId_ThrowsArgumentExceptionWithActualLength()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.Compose()
            .UseMessageStream(new string('a', 31)));

        Assert.Equal($"{MessageStreamRuleMessage} Actual length: 31. (Parameter 'messageStreamId')", exception.Message);
    }

    [Fact]
    public void UsingMessageStream_WithNullString_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => Email.Compose()
            .UseMessageStream(null!));

        Assert.Equal("messageStreamId", exception.ParamName);
    }
}