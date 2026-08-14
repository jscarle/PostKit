using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LightResults;
using PostKit.Webhooks;

namespace PostKit.Tests;

public class OutboundWebhookMessageTests
{
    private const string DeliveryJson = """
                                                {
                                                  "MessageID": "883953f4-6105-42a2-a16a-77a8eac79483",
                                                  "Recipient": "john@example.com",
                                                  "DeliveredAt": "2019-11-05T16:33:54.9070259Z",
                                                  "Details": "Test delivery webhook details",
                                                  "Tag": "welcome-email",
                                                  "ServerID": 23,
                                                  "Metadata": {
                                                    "a_key": "a_value",
                                                    "b_key": "b_value"
                                                  },
                                                  "RecordType": "Delivery",
                                                  "MessageStream": "outbound",
                                                  "FutureProperty": { "Version": 2 }
                                                }
                                                """;

    private const string BounceJson = """
                                              {
                                                "RecordType": "Bounce",
                                                "MessageStream": "outbound",
                                                "ID": 4323372036854775807,
                                                "Type": "HardBounce",
                                                "TypeCode": 1,
                                                "Name": "Hard bounce",
                                                "Tag": "Test",
                                                "MessageID": "883953f4-6105-42a2-a16a-77a8eac79483",
                                                "Metadata": { "a_key": "a_value" },
                                                "ServerID": 23,
                                                "Description": "The server was unable to deliver your message.",
                                                "Details": "Test bounce details",
                                                "Email": "john@example.com",
                                                "From": "sender@example.com",
                                                "BouncedAt": "2019-11-05T16:33:54.9070259Z",
                                                "DumpAvailable": true,
                                                "Inactive": true,
                                                "CanActivate": true,
                                                "Subject": "Test subject",
                                                "Content": "<Full dump of bounce>"
                                              }
                                              """;

    private const string OpenJson = """
                                            {
                                              "RecordType": "Open",
                                              "MessageStream": "outbound",
                                              "FirstOpen": true,
                                              "Client": {
                                                "Name": "Chrome 35.0.1916.153",
                                                "Company": "Google",
                                                "Family": "Chrome"
                                              },
                                              "OS": {
                                                "Name": "OS X 10.7 Lion",
                                                "Company": "Apple Computer, Inc.",
                                                "Family": "OS X 10"
                                              },
                                              "Platform": "WebMail",
                                              "UserAgent": "Mozilla/5.0",
                                              "Geo": {
                                                "CountryISOCode": "RS",
                                                "Country": "Serbia",
                                                "RegionISOCode": "VO",
                                                "Region": "Autonomna Pokrajina Vojvodina",
                                                "City": "Novi Sad",
                                                "Zip": "21000",
                                                "Coords": "45.2517,19.8369",
                                                "IP": "188.2.95.4"
                                              },
                                              "MessageID": "883953f4-6105-42a2-a16a-77a8eac79483",
                                              "Metadata": { "a_key": "a_value" },
                                              "ReceivedAt": "2019-11-05T16:33:54.9070259Z",
                                              "Tag": "welcome-email",
                                              "Recipient": "john@example.com"
                                            }
                                            """;

    private const string ClickJson = """
                                             {
                                               "RecordType": "Click",
                                               "MessageStream": "outbound",
                                               "ClickLocation": "HTML",
                                               "Client": {
                                                 "Name": "Chrome 35.0.1916.153",
                                                 "Company": "Google",
                                                 "Family": "Chrome"
                                               },
                                               "OS": {
                                                 "Name": "OS X 10.7 Lion",
                                                 "Company": "Apple Computer, Inc.",
                                                 "Family": "OS X 10"
                                               },
                                               "Platform": "Desktop",
                                               "UserAgent": "Mozilla/5.0",
                                               "OriginalLink": "https://example.com",
                                               "Geo": {
                                                 "CountryISOCode": "RS",
                                                 "Country": "Serbia",
                                                 "RegionISOCode": "VO",
                                                 "Region": "Autonomna Pokrajina Vojvodina",
                                                 "City": "Novi Sad",
                                                 "Zip": "21000",
                                                 "Coords": "45.2517,19.8369",
                                                 "IP": "8.8.8.8"
                                               },
                                               "MessageID": "00000000-0000-0000-0000-000000000000",
                                               "Metadata": { "a_key": "a_value" },
                                               "ReceivedAt": "2017-10-25T15:21:11.9065619Z",
                                               "Tag": "welcome-email",
                                               "Recipient": "john@example.com"
                                             }
                                             """;

    private const string SpamComplaintJson = """
                                                     {
                                                       "RecordType": "SpamComplaint",
                                                       "MessageStream": "outbound",
                                                       "ID": 42,
                                                       "Type": "SpamComplaint",
                                                       "TypeCode": 512,
                                                       "Name": "Spam complaint",
                                                       "Tag": "Test",
                                                       "MessageID": "00000000-0000-0000-0000-000000000000",
                                                       "Metadata": { "a_key": "a_value" },
                                                       "ServerID": 1234,
                                                       "Description": "",
                                                       "Details": "Test spam complaint details",
                                                       "Email": "john@example.com",
                                                       "From": "sender@example.com",
                                                       "BouncedAt": "2019-11-05T16:33:54.9070259Z",
                                                       "DumpAvailable": true,
                                                       "Inactive": true,
                                                       "CanActivate": false,
                                                       "Subject": "Test subject",
                                                       "Content": "<Abuse report dump>"
                                                     }
                                                     """;

    private const string SubscriptionChangeJson = """
                                                        {
                                                          "RecordType": "SubscriptionChange",
                                                          "MessageID": "883953f4-6105-42a2-a16a-77a8eac79483",
                                                          "ServerID": 123456,
                                                          "MessageStream": "outbound",
                                                          "ChangedAt": "2020-02-01T10:53:34.416071Z",
                                                          "Recipient": "bounced-address@example.com",
                                                          "Origin": "Recipient",
                                                          "SuppressSending": true,
                                                          "SuppressionReason": "HardBounce",
                                                          "Tag": "my-tag",
                                                          "Metadata": { "example": "value" }
                                                        }
                                                        """;

    [Fact]
    public void JsonSerializerDeserialize_WithDeliveryContract_MapsDocumentedShape()
    {
        var message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(DeliveryJson);

        Assert.NotNull(message);
        Assert.Equal("Delivery", message.RecordType);
        Assert.Equal(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), message.MessageId);
        Assert.Equal("john@example.com", message.Recipient);
        Assert.Equal(DateTimeOffset.Parse("2019-11-05T16:33:54.9070259Z"), message.DeliveredAt);
        Assert.Equal("Test delivery webhook details", message.Details);
        Assert.Equal("welcome-email", message.Tag);
        Assert.Equal(23, message.ServerId);
        Assert.Equal("a_value", message.Metadata["a_key"]);
        Assert.Equal("outbound", message.MessageStream);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithBounceContract_MapsDocumentedShape()
    {
        var message = JsonSerializer.Deserialize<BounceWebhookMessage>(BounceJson);

        Assert.NotNull(message);
        Assert.Equal("Bounce", message.RecordType);
        Assert.Equal(4323372036854775807, message.Id);
        Assert.Equal("HardBounce", message.Type);
        Assert.Equal(1, message.TypeCode);
        Assert.Equal(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), message.MessageId);
        Assert.Equal(DateTimeOffset.Parse("2019-11-05T16:33:54.9070259Z"), message.BouncedAt);
        Assert.True(message.DumpAvailable);
        Assert.True(message.Inactive);
        Assert.True(message.CanActivate);
        Assert.Equal("<Full dump of bounce>", message.Content);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithOpenContract_MapsTrackingTypes()
    {
        var message = JsonSerializer.Deserialize<OpenWebhookMessage>(OpenJson);

        Assert.NotNull(message);
        Assert.Equal("Open", message.RecordType);
        Assert.True(message.FirstOpen);
        Assert.Equal("Chrome 35.0.1916.153", message.Client?.Name);
        Assert.Equal("OS X 10.7 Lion", message.Os?.Name);
        Assert.Equal("WebMail", message.Platform);
        Assert.Equal("RS", message.Geo?.CountryIsoCode);
        Assert.Equal("188.2.95.4", message.Geo?.Ip);
        Assert.Equal(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), message.MessageId);
        Assert.Equal(DateTimeOffset.Parse("2019-11-05T16:33:54.9070259Z"), message.ReceivedAt);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithClickContract_MapsTrackingTypes()
    {
        var message = JsonSerializer.Deserialize<ClickWebhookMessage>(ClickJson);

        Assert.NotNull(message);
        Assert.Equal("Click", message.RecordType);
        Assert.Equal("HTML", message.ClickLocation);
        Assert.Equal("https://example.com", message.OriginalLink);
        Assert.Equal("Chrome", message.Client?.Family);
        Assert.Equal("OS X 10", message.Os?.Family);
        Assert.Equal("8.8.8.8", message.Geo?.Ip);
        Assert.Equal(Guid.Empty, message.MessageId);
        Assert.Equal(DateTimeOffset.Parse("2017-10-25T15:21:11.9065619Z"), message.ReceivedAt);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithSpamComplaintContract_MapsDocumentedShape()
    {
        var message = JsonSerializer.Deserialize<SpamComplaintWebhookMessage>(SpamComplaintJson);

        Assert.NotNull(message);
        Assert.Equal("SpamComplaint", message.RecordType);
        Assert.Equal(42, message.Id);
        Assert.Equal("SpamComplaint", message.Type);
        Assert.Equal(512, message.TypeCode);
        Assert.Equal(Guid.Empty, message.MessageId);
        Assert.Equal("", message.Description);
        Assert.False(message.CanActivate);
        Assert.Equal("<Abuse report dump>", message.Content);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithSubscriptionChangeContract_MapsDocumentedShape()
    {
        var message = JsonSerializer.Deserialize<SubscriptionChangeWebhookMessage>(SubscriptionChangeJson);

        Assert.NotNull(message);
        Assert.Equal("SubscriptionChange", message.RecordType);
        Assert.Equal(Guid.Parse("883953f4-6105-42a2-a16a-77a8eac79483"), message.MessageId);
        Assert.Equal(123456, message.ServerId);
        Assert.Equal(DateTimeOffset.Parse("2020-02-01T10:53:34.416071Z"), message.ChangedAt);
        Assert.True(message.SuppressSending);
        Assert.Equal("HardBounce", message.SuppressionReason);
        Assert.Equal("value", message.Metadata["example"]);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithMissingRequiredMember_ThrowsJsonException()
    {
        var node = ParseObject(DeliveryJson);
        Assert.True(node.Remove("Details"));

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DeliveryWebhookMessage>(node));
    }

    [Fact]
    public void JsonSerializerDeserialize_WithoutOptionalMembers_Succeeds()
    {
        var bounceNode = ParseObject(BounceJson);
        Assert.True(bounceNode.Remove("Tag"));
        Assert.True(bounceNode.Remove("From"));
        Assert.True(bounceNode.Remove("Content"));

        var bounce = JsonSerializer.Deserialize<BounceWebhookMessage>(bounceNode);

        Assert.NotNull(bounce);
        Assert.Null(bounce.Tag);
        Assert.Null(bounce.From);
        Assert.Null(bounce.Content);

        var openNode = ParseObject(OpenJson);
        Assert.True(openNode.Remove("Client"));
        Assert.True(openNode.Remove("OS"));
        Assert.True(openNode.Remove("Platform"));
        Assert.True(openNode.Remove("Geo"));
        Assert.True(openNode.Remove("Tag"));

        var open = JsonSerializer.Deserialize<OpenWebhookMessage>(openNode);

        Assert.NotNull(open);
        Assert.Null(open.Client);
        Assert.Null(open.Os);
        Assert.Null(open.Platform);
        Assert.Null(open.Geo);
        Assert.Null(open.Tag);
    }

    [Fact]
    public void JsonSerializerDeserialize_WithUnknownRootProperty_IgnoresPropertyWithoutExposingIt()
    {
        var message = JsonSerializer.Deserialize<DeliveryWebhookMessage>(DeliveryJson);

        Assert.NotNull(message);
        Assert.Null(typeof(DeliveryWebhookMessage).GetProperty("AdditionalProperties"));
        Assert.DoesNotContain(typeof(DeliveryWebhookMessage).GetProperties(), static property => property.Name == "FutureProperty");
    }

    [Fact]
    public void DedicatedDeserializers_WithDocumentedPayloads_ReturnConcreteMessages()
    {
        AssertSuccessful(BounceWebhookDeserializer.Deserialize(BounceJson));
        AssertSuccessful(ClickWebhookDeserializer.Deserialize(ClickJson));
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(DeliveryJson));
        AssertSuccessful(OpenWebhookDeserializer.Deserialize(OpenJson));
        AssertSuccessful(SpamComplaintWebhookDeserializer.Deserialize(SpamComplaintJson));
        AssertSuccessful(SubscriptionChangeWebhookDeserializer.Deserialize(SubscriptionChangeJson));
    }

    [Fact]
    public void DeliveryDeserialize_FromEverySynchronousSystemTextJsonSource_Succeeds()
    {
        var utf8Json = Encoding.UTF8.GetBytes(DeliveryJson);

        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(DeliveryJson));
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(DeliveryJson.AsSpan()));
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(utf8Json.AsSpan()));

        using var stream = new MemoryStream(utf8Json);
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(stream));
        Assert.True(stream.CanRead);

        var reader = new Utf8JsonReader(utf8Json);
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(ref reader));

        using var document = JsonDocument.Parse(DeliveryJson);
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(document));
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(document.RootElement));

        var node = JsonNode.Parse(DeliveryJson);
        Assert.NotNull(node);
        AssertSuccessful(DeliveryWebhookDeserializer.Deserialize(node));
    }

    [Fact]
    public async Task DeliveryDeserializeAsync_FromStreamAndPipeReader_SucceedsWithoutClosingOrCompletingSources()
    {
        var utf8Json = Encoding.UTF8.GetBytes(DeliveryJson);
        await using var stream = new MemoryStream(utf8Json);

        var streamResult = await DeliveryWebhookDeserializer.DeserializeAsync(stream, CancellationToken.None);

        AssertSuccessful(streamResult);
        Assert.True(stream.CanRead);

        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(utf8Json, TestContext.Current.CancellationToken);
        await pipe.Writer.CompleteAsync();
        var trackingReader = new TrackingPipeReader(pipe.Reader);

        var pipeResult = await DeliveryWebhookDeserializer.DeserializeAsync(trackingReader, CancellationToken.None);

        AssertSuccessful(pipeResult);
        Assert.False(trackingReader.CompleteCalled);
        await trackingReader.CompleteAsync();
    }

    [Fact]
    public void OutboundDeserialize_WithEveryRecordType_DispatchesToConcreteMessage()
    {
        Assert.IsType<BounceWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(BounceJson)));
        Assert.IsType<ClickWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(ClickJson)));
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(DeliveryJson)));
        Assert.IsType<OpenWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(OpenJson)));
        Assert.IsType<SpamComplaintWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(SpamComplaintJson)));
        Assert.IsType<SubscriptionChangeWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(SubscriptionChangeJson)));
    }

    [Fact]
    public void OutboundDeserialize_FromEverySynchronousSystemTextJsonSource_Succeeds()
    {
        var utf8Json = Encoding.UTF8.GetBytes(DeliveryJson);

        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(DeliveryJson)));
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(DeliveryJson.AsSpan())));
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(utf8Json.AsSpan())));

        using var stream = new MemoryStream(utf8Json);
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(stream)));
        Assert.True(stream.CanRead);

        var reader = new Utf8JsonReader(utf8Json);
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(ref reader)));

        using var document = JsonDocument.Parse(DeliveryJson);
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(document)));
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(document.RootElement)));

        var node = JsonNode.Parse(DeliveryJson);
        Assert.NotNull(node);
        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(OutboundWebhookDeserializer.Deserialize(node)));
    }

    [Fact]
    public async Task OutboundDeserializeAsync_FromStreamAndPipeReader_SucceedsWithoutClosingOrCompletingSources()
    {
        var utf8Json = Encoding.UTF8.GetBytes(DeliveryJson);
        await using var stream = new MemoryStream(utf8Json);

        var streamResult = await OutboundWebhookDeserializer.DeserializeAsync(stream, CancellationToken.None);

        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(streamResult));
        Assert.True(stream.CanRead);

        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(utf8Json, TestContext.Current.CancellationToken);
        await pipe.Writer.CompleteAsync();
        var trackingReader = new TrackingPipeReader(pipe.Reader);

        var pipeResult = await OutboundWebhookDeserializer.DeserializeAsync(trackingReader, CancellationToken.None);

        Assert.IsType<DeliveryWebhookMessage>(AssertSuccessful(pipeResult));
        Assert.False(trackingReader.CompleteCalled);
        await trackingReader.CompleteAsync();
    }

    [Fact]
    public void Deserialize_WithMalformedJson_ReturnsFailureWithJsonException()
    {
        var result = DeliveryWebhookDeserializer.Deserialize("{\"RecordType\":");

        AssertFailureWithException<JsonException, DeliveryWebhookMessage>(result,
            "The Postmark delivery webhook JSON could not be deserialized. Ensure the input contains one valid Postmark delivery webhook JSON object.");
    }

    [Fact]
    public void Deserialize_WithExplicitNullRequiredMember_ReturnsPropertySpecificFailure()
    {
        var node = ParseObject(DeliveryJson);
        node["Recipient"] = null;

        var result = DeliveryWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark delivery webhook property 'Recipient' is required and cannot be null.");
    }

    [Fact]
    public void Deserialize_WithNullMetadataValue_ReturnsPropertySpecificFailure()
    {
        var node = ParseObject(OpenJson);
        var metadata = node["Metadata"]?.AsObject();
        Assert.NotNull(metadata);
        metadata["a_key"] = null;

        var result = OpenWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark open webhook property 'Metadata[\"a_key\"]' cannot be null.");
    }

    [Fact]
    public void Deserialize_WithWrongRecordType_ReturnsPropertySpecificFailure()
    {
        var node = ParseObject(ClickJson);
        node["RecordType"] = "Open";

        var result = ClickWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark click webhook property 'RecordType' must be 'Click'. Received 'Open'.");
    }

    [Fact]
    public void Deserialize_WithNegativeNumericValue_ReturnsPropertySpecificFailure()
    {
        var node = ParseObject(BounceJson);
        node["ID"] = -1;

        var result = BounceWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark bounce webhook property 'Id' cannot be negative. Received -1.");
    }

    [Fact]
    public void SubscriptionChangeDeserialize_WithReactivation_AllowsDocumentedNullAndEmptyValues()
    {
        var node = ParseObject(SubscriptionChangeJson);
        node["MessageID"] = null;
        node["SuppressSending"] = false;
        node["SuppressionReason"] = null;
        node["Tag"] = null;
        node["Metadata"] = new JsonObject();

        var message = AssertSuccessful(SubscriptionChangeWebhookDeserializer.Deserialize(node));

        Assert.Null(message.MessageId);
        Assert.False(message.SuppressSending);
        Assert.Null(message.SuppressionReason);
        Assert.Null(message.Tag);
        Assert.Empty(message.Metadata);
    }

    [Fact]
    public void SubscriptionChangeDeserialize_WithSuppressionWithoutReason_ReturnsPropertySpecificFailure()
    {
        var node = ParseObject(SubscriptionChangeJson);
        node["SuppressionReason"] = null;

        var result = SubscriptionChangeWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark subscription change webhook property 'SuppressionReason' cannot be null when 'SuppressSending' is true.");
    }

    [Fact]
    public void OutboundDeserialize_WithoutRecordType_ReturnsPropertySpecificFailure()
    {
        var node = ParseObject(DeliveryJson);
        Assert.True(node.Remove("RecordType"));

        var result = OutboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark outbound webhook property 'RecordType' is required.");
    }

    [Fact]
    public void OutboundDeserialize_WithUnknownRecordType_ReturnsValueSpecificFailure()
    {
        var node = ParseObject(DeliveryJson);
        node["RecordType"] = "FutureEvent";

        var result = OutboundWebhookDeserializer.Deserialize(node);

        AssertFailure(result, "The Postmark outbound webhook RecordType value 'FutureEvent' is not supported.");
    }

    [Fact]
    public void Deserialize_WithDisposedJsonDocument_ReturnsFailureWithAttachedException()
    {
        var deliveryDocument = JsonDocument.Parse(DeliveryJson);
        deliveryDocument.Dispose();

        var deliveryResult = DeliveryWebhookDeserializer.Deserialize(deliveryDocument);

        AssertFailureWithException<ObjectDisposedException, DeliveryWebhookMessage>(deliveryResult,
            "The Postmark delivery webhook JSON could not be deserialized. Ensure the input contains one valid Postmark delivery webhook JSON object.");

        var outboundDocument = JsonDocument.Parse(DeliveryJson);
        outboundDocument.Dispose();

        var outboundResult = OutboundWebhookDeserializer.Deserialize(outboundDocument);

        AssertFailureWithException<ObjectDisposedException, OutboundWebhookMessage>(outboundResult,
            "The Postmark outbound webhook JSON could not be deserialized. Ensure the input contains one valid Postmark outbound webhook JSON object.");
    }

    [Fact]
    public async Task DeserializeAsync_WhenCancellationIsRequested_PropagatesCancellationWithoutCompletingSources()
    {
        await using var deliveryStream = new MemoryStream(Encoding.UTF8.GetBytes(DeliveryJson));
        await using var outboundStream = new MemoryStream(Encoding.UTF8.GetBytes(DeliveryJson));
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            DeliveryWebhookDeserializer.DeserializeAsync(deliveryStream, cancellationTokenSource.Token));
        Assert.True(deliveryStream.CanRead);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OutboundWebhookDeserializer.DeserializeAsync(outboundStream, cancellationTokenSource.Token));
        Assert.True(outboundStream.CanRead);

        var pipe = new Pipe();
        var trackingReader = new TrackingPipeReader(pipe.Reader);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OutboundWebhookDeserializer.DeserializeAsync(trackingReader, cancellationTokenSource.Token));
        Assert.False(trackingReader.CompleteCalled);

        await pipe.Writer.CompleteAsync();
        await trackingReader.CompleteAsync();
    }

    private static JsonObject ParseObject(string json)
    {
        var node = JsonNode.Parse(json)?.AsObject();
        Assert.NotNull(node);

        return node;
    }

    private static TMessage AssertSuccessful<TMessage>(Result<TMessage> result)
    {
        Assert.True(result.IsSuccess(out var message), result.ToString());
        return message;
    }

    private static void AssertFailure<TMessage>(Result<TMessage> result, string expectedMessage)
    {
        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(expectedMessage, error.Message);
    }

    private static void AssertFailureWithException<TException, TMessage>(Result<TMessage> result, string expectedMessage) where TException : Exception
    {
        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(expectedMessage, error.Message);
        Assert.IsType<TException>(error.Exception);
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
