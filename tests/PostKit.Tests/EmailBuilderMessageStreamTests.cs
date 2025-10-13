using PostKit;
using PostKit.Common;
using Xunit;

namespace PostKit.Tests;

public class EmailBuilderMessageStreamTests
{
    [Fact]
    public void UsingMessageStream_WithString_SetsMessageStreamOnEmailRequest()
    {
        const string messageStreamId = "custom-stream-id";

        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .WithHtmlBody("<p>Hello</p>")
            .UsingMessageStream(messageStreamId)
            .Build();

        var request = email.ToEmailRequest();

        Assert.Equal(messageStreamId, request.MessageStream);
    }
}
