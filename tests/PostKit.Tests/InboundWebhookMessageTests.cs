using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Webhooks;

namespace PostKit.Tests;

public class InboundWebhookMessageTests
{
    private const string DeserializationFailureMessage =
        "The Postmark inbound webhook JSON could not be deserialized. Ensure the input contains one valid Postmark inbound webhook JSON object.";

    private const string SampleJson = """
                                              {
                                                "FromName": "Sender Name",
                                                "MessageStream": "inbound",
                                                "From": "\"Sender Name\" <sender@example.com>",
                                                "FromFull": {
                                                  "Email": "sender@example.com",
                                                  "Name": "Sender Name",
                                                  "MailboxHash": "from-hash"
                                                },
                                                "To": "\"Recipient\" <inbound+ticket-123@example.com>",
                                                "ToFull": [
                                                  {
                                                    "Email": "inbound+ticket-123@example.com",
                                                    "Name": "Recipient",
                                                    "MailboxHash": "ticket-123"
                                                  }
                                                ],
                                                "Cc": "cc@example.com",
                                                "CcFull": [
                                                  {
                                                    "Email": "cc@example.com",
                                                    "Name": "",
                                                    "MailboxHash": ""
                                                  }
                                                ],
                                                "Bcc": "bcc@example.com",
                                                "BccFull": [
                                                  {
                                                    "Email": "bcc@example.com",
                                                    "Name": "",
                                                    "MailboxHash": ""
                                                  }
                                                ],
                                                "OriginalRecipient": "inbound+ticket-123@example.com",
                                                "Subject": "Test subject",
                                                "MessageID": "73e6d360-66eb-11e1-8e72-a8904824019b",
                                                "ReplyTo": "reply@example.com",
                                                "MailboxHash": "ticket-123",
                                                "Date": "Fri, 1 Aug 2014 16:45:32 -04:00",
                                                "TextBody": "Plain text body",
                                                "HtmlBody": "<p>HTML body</p>",
                                                "StrippedTextReply": "Reply text",
                                                "Tag": "support",
                                                "Headers": [
                                                  {
                                                    "Name": "X-Spam-Score",
                                                    "Value": "-0.1"
                                                  }
                                                ],
                                                "Attachments": [
                                                  {
                                                    "Name": "test.txt",
                                                    "Content": "SGVsbG8=",
                                                    "ContentType": "text/plain",
                                                    "ContentLength": 5,
                                                    "ContentID": "test.txt@example.com"
                                                  }
                                                ],
                                                "RawEmail": "From: sender@example.com\r\n\r\nPlain text body",
                                                "FutureProperty": {
                                                  "Version": 2
                                                }
                                              }
                                              """;

    [Fact]
    public void Deserialize_WithCompleteInboundWebhook_MapsObservedProductionShape()
    {
        var result = InboundWebhookDeserializer.Deserialize(SampleJson);

        var message = AssertSuccessful(result);
        AssertMessageShape(message);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithPublicContracts_MapsMessageAndNestedTypes()
    {
        var message = JsonSerializer.Deserialize<InboundWebhookMessage>(SampleJson);

        Assert.NotNull(message);
        AssertMessageShape(message);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithMissingRequiredMember_ThrowsJsonException()
    {
        var node = ParseSampleObject();
        Assert.True(node.Remove("Subject"));

        var json = node.ToJsonString();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<InboundWebhookMessage>(json));
    }

    [Fact]
    public void JsonSerializerDeserialize_WithMissingNestedRequiredMember_ThrowsJsonException()
    {
        var node = ParseSampleObject();
        var attachment = GetOnlyAttachment(node);
        Assert.True(attachment.Remove("ContentType"));

        var json = node.ToJsonString();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<InboundWebhookMessage>(json));
    }

    [Fact]
    public void JsonSerializerDeserialize_WithoutOptionalMembers_Succeeds()
    {
        var node = ParseSampleObject();
        Assert.True(node.Remove("RawEmail"));
        var attachment = GetOnlyAttachment(node);
        Assert.True(attachment.Remove("ContentID"));

        var message = JsonSerializer.Deserialize<InboundWebhookMessage>(node);

        Assert.NotNull(message);
        Assert.Null(message.RawEmail);
        var deserializedAttachment = Assert.IsType<InboundWebhookAttachment>(Assert.Single(message.Attachments));
        Assert.Null(deserializedAttachment.ContentId);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithUnknownRootProperty_IgnoresPropertyWithoutExposingIt()
    {
        var message = JsonSerializer.Deserialize<InboundWebhookMessage>(SampleJson);

        Assert.NotNull(message);
        Assert.Null(typeof(InboundWebhookMessage).GetProperty("AdditionalProperties"));
        Assert.DoesNotContain(typeof(InboundWebhookMessage).GetProperties(), static property => property.Name == "FutureProperty");
    }

    [Fact]
    public void Deserialize_FromEverySynchronousSystemTextJsonSource_Succeeds()
    {
        var utf8Json = Encoding.UTF8.GetBytes(SampleJson);

        AssertSuccessful(InboundWebhookDeserializer.Deserialize(SampleJson));
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(SampleJson.AsSpan()));
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(utf8Json.AsSpan()));

        using var stream = new MemoryStream(utf8Json);
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(stream));
        Assert.True(stream.CanRead);

        var reader = new Utf8JsonReader(utf8Json);
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(ref reader));

        using var document = JsonDocument.Parse(SampleJson);
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(document));
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(document.RootElement));

        var node = JsonNode.Parse(SampleJson);
        Assert.NotNull(node);
        AssertSuccessful(InboundWebhookDeserializer.Deserialize(node));
    }

    [Fact]
    public async Task DeserializeAsync_FromStreamAndPipeReader_SucceedsWithoutClosingOrCompletingSources()
    {
        var utf8Json = Encoding.UTF8.GetBytes(SampleJson);
        await using var stream = new MemoryStream(utf8Json);

        var streamResult = await InboundWebhookDeserializer.DeserializeAsync(stream, CancellationToken.None);

        AssertSuccessful(streamResult);
        Assert.True(stream.CanRead);

        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(utf8Json, TestContext.Current.CancellationToken);
        await pipe.Writer.CompleteAsync();
        var trackingReader = new TrackingPipeReader(pipe.Reader);

        var pipeResult = await InboundWebhookDeserializer.DeserializeAsync(trackingReader, CancellationToken.None);

        AssertSuccessful(pipeResult);
        Assert.False(trackingReader.CompleteCalled);
        await trackingReader.CompleteAsync();
    }

    [Fact]
    public void Deserialize_WithDisposedJsonDocument_ReturnsFailureWithAttachedException()
    {
        var document = JsonDocument.Parse(SampleJson);
        document.Dispose();

        var result = InboundWebhookDeserializer.Deserialize(document);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(DeserializationFailureMessage, error.Message);
        Assert.IsType<ObjectDisposedException>(error.Exception);
    }

    [Fact]
    public void Deserialize_WithoutOptionalRawEmailOrContentId_Succeeds()
    {
        var node = ParseSampleObject();
        Assert.True(node.Remove("RawEmail"));
        var attachment = GetOnlyAttachment(node);
        Assert.True(attachment.Remove("ContentID"));

        var result = InboundWebhookDeserializer.Deserialize(node);

        var message = AssertSuccessful(result);
        Assert.Null(message.RawEmail);
        var deserializedAttachment = Assert.IsType<InboundWebhookAttachment>(Assert.Single(message.Attachments));
        Assert.Null(deserializedAttachment.ContentId);
    }

    [Fact]
    public void Deserialize_WithMalformedJson_ReturnsFailureWithJsonException()
    {
        var result = InboundWebhookDeserializer.Deserialize("{\"FromName\":");

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(DeserializationFailureMessage, error.Message);
        Assert.IsType<JsonException>(error.Exception);
    }

    [Fact]
    public void Deserialize_WithExplicitNullRootMember_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        node["Cc"] = null;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'Cc' is required and cannot be null.");
    }

    [Fact]
    public void Deserialize_WithExplicitNullNestedMember_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        var fromFull = node["FromFull"]?.AsObject();
        Assert.NotNull(fromFull);
        fromFull["Email"] = null;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'FromFull.Email' is required and cannot be null.");
    }

    [Fact]
    public void Deserialize_WithNullAddressCollectionEntry_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        var recipients = node["ToFull"]?.AsArray();
        Assert.NotNull(recipients);
        recipients[0] = null;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'ToFull[0]' cannot be null.");
    }

    [Fact]
    public void Deserialize_WithNullHeaderCollectionEntry_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        var headers = node["Headers"]?.AsArray();
        Assert.NotNull(headers);
        headers[0] = null;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'Headers[0]' cannot be null.");
    }

    [Fact]
    public void Deserialize_WithInvalidNestedAttachmentValue_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        var attachment = GetOnlyAttachment(node);
        attachment["Content"] = null;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'Attachments[0].Content' is required and cannot be null.");
    }

    [Fact]
    public void Deserialize_WithNullAttachmentCollectionEntry_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        var attachments = node["Attachments"]?.AsArray();
        Assert.NotNull(attachments);
        attachments[0] = null;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'Attachments[0]' cannot be null.");
    }

    [Fact]
    public void Deserialize_WithNegativeAttachmentLength_ReturnsPropertySpecificFailure()
    {
        var node = ParseSampleObject();
        var attachment = GetOnlyAttachment(node);
        attachment["ContentLength"] = -1;

        var result = InboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark inbound webhook property 'Attachments[0].ContentLength' cannot be negative. Received -1.");
    }

    [Fact]
    public void Deserialize_WithInvalidMessageId_ReturnsFailureWithJsonException()
    {
        var json = SampleJson.Replace("73e6d360-66eb-11e1-8e72-a8904824019b", "not-a-guid", StringComparison.Ordinal);

        var result = InboundWebhookDeserializer.Deserialize(json);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(DeserializationFailureMessage, error.Message);
        Assert.IsType<JsonException>(error.Exception);
    }

    [Fact]
    public async Task DeserializeAsync_WhenCancellationIsRequested_PropagatesCancellationWithoutCompletingSources()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleJson));
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            InboundWebhookDeserializer.DeserializeAsync(stream, cancellationTokenSource.Token));
        Assert.True(stream.CanRead);

        var pipe = new Pipe();
        var trackingReader = new TrackingPipeReader(pipe.Reader);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            InboundWebhookDeserializer.DeserializeAsync(trackingReader, cancellationTokenSource.Token));
        Assert.False(trackingReader.CompleteCalled);

        await pipe.Writer.CompleteAsync();
        await trackingReader.CompleteAsync();
    }

    private static JsonObject ParseSampleObject()
    {
        var node = JsonNode.Parse(SampleJson)?.AsObject();
        Assert.NotNull(node);

        return node;
    }

    private static JsonObject GetOnlyAttachment(JsonObject message)
    {
        var attachments = message["Attachments"]?.AsArray();
        Assert.NotNull(attachments);
        var attachmentNode = Assert.Single(attachments);
        Assert.NotNull(attachmentNode);

        return attachmentNode.AsObject();
    }

    private static void AssertMessageShape(InboundWebhookMessage message)
    {
        Assert.Equal("Sender Name", message.FromName);
        Assert.Equal("inbound", message.MessageStream);
        Assert.Equal("\"Sender Name\" <sender@example.com>", message.From);
        Assert.Equal("sender@example.com", message.FromFull.Email);
        Assert.Equal("Sender Name", message.FromFull.Name);
        Assert.Equal("from-hash", message.FromFull.MailboxHash);
        Assert.Equal("\"Recipient\" <inbound+ticket-123@example.com>", message.To);
        var to = Assert.IsType<InboundWebhookAddress>(Assert.Single(message.ToFull));
        Assert.Equal("inbound+ticket-123@example.com", to.Email);
        Assert.Equal("Recipient", to.Name);
        Assert.Equal("ticket-123", to.MailboxHash);
        Assert.Equal("cc@example.com", message.Cc);
        Assert.Equal("cc@example.com", Assert.IsType<InboundWebhookAddress>(Assert.Single(message.CcFull)).Email);
        Assert.Equal("bcc@example.com", message.Bcc);
        Assert.Equal("bcc@example.com", Assert.IsType<InboundWebhookAddress>(Assert.Single(message.BccFull)).Email);
        Assert.Equal("inbound+ticket-123@example.com", message.OriginalRecipient);
        Assert.Equal("Test subject", message.Subject);
        Assert.Equal(Guid.Parse("73e6d360-66eb-11e1-8e72-a8904824019b"), message.MessageId);
        Assert.Equal("reply@example.com", message.ReplyTo);
        Assert.Equal("ticket-123", message.MailboxHash);
        Assert.Equal("Fri, 1 Aug 2014 16:45:32 -04:00", message.Date);
        Assert.Equal("Plain text body", message.TextBody);
        Assert.Equal("<p>HTML body</p>", message.HtmlBody);
        Assert.Equal("Reply text", message.StrippedTextReply);
        Assert.Equal("support", message.Tag);
        var header = Assert.IsType<InboundWebhookHeader>(Assert.Single(message.Headers));
        Assert.Equal("X-Spam-Score", header.Name);
        Assert.Equal("-0.1", header.Value);
        var attachment = Assert.IsType<InboundWebhookAttachment>(Assert.Single(message.Attachments));
        Assert.Equal("test.txt", attachment.Name);
        Assert.Equal("SGVsbG8=", attachment.Content);
        Assert.Equal("text/plain", attachment.ContentType);
        Assert.Equal(5, attachment.ContentLength);
        Assert.Equal("test.txt@example.com", attachment.ContentId);
        Assert.Equal("From: sender@example.com\r\n\r\nPlain text body", message.RawEmail);
    }

    private static InboundWebhookMessage AssertSuccessful(Result<InboundWebhookMessage> result)
    {
        Assert.True(result.IsSuccess(out var message), result.ToString());
        return message;
    }

    private static void AssertFailure(Result<InboundWebhookMessage> result, string expectedMessage)
    {
        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(expectedMessage, error.Message);
    }

    private sealed class TrackingPipeReader(PipeReader reader) : PipeReader
    {
        public bool CompleteCalled { get; private set; }

        public override void AdvanceTo(SequencePosition consumed)
        {
            reader.AdvanceTo(consumed);
        }

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
            reader.AdvanceTo(consumed, examined);
        }

        public override void CancelPendingRead()
        {
            reader.CancelPendingRead();
        }

        public override void Complete(Exception? exception = null)
        {
            CompleteCalled = true;
            reader.Complete(exception);
        }

        public override ValueTask CompleteAsync(Exception? exception = null)
        {
            CompleteCalled = true;
            return reader.CompleteAsync(exception);
        }

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            return reader.ReadAsync(cancellationToken);
        }

        public override bool TryRead(out ReadResult result)
        {
            return reader.TryRead(out result);
        }
    }
}
