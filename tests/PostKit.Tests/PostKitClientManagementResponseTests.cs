using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Domains;
using PostKit.Errors;
using PostKit.MessageStreams;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostKit.SenderSignatures;
using PostKit.Servers;
using DomainCreateRequestModel = PostKit.Postmark.Domains.DomainCreateRequest;
using MessageStreamRequestModel = PostKit.Postmark.MessageStreams.MessageStreamRequest;
using SenderSignatureCreateRequestModel = PostKit.Postmark.SenderSignatures.SenderSignatureCreateRequest;
using SenderSignatureEditRequestModel = PostKit.Postmark.SenderSignatures.SenderSignatureEditRequest;
using ServerRequestModel = PostKit.Postmark.Servers.ServerRequest;

namespace PostKit.Tests;

public class PostKitClientManagementResponseTests
{
    private const string ServerJson = """
                                      {
                                        "ID": 42,
                                        "Name": "PostKit Testing",
                                        "ApiTokens": ["server-token-a"],
                                        "Color": "blue",
                                        "SmtpApiActivated": true,
                                        "RawEmailEnabled": false,
                                        "DeliveryType": "Sandbox",
                                        "ServerLink": "https://account.postmarkapp.com/servers/42",
                                        "InboundAddress": "hash@inbound.postmarkapp.com",
                                        "InboundHookUrl": "",
                                        "BounceHookUrl": "https://example.com/bounce",
                                        "OpenHookUrl": null,
                                        "DeliveryHookUrl": "https://example.com/delivery",
                                        "PostFirstOpenOnly": true,
                                        "InboundDomain": "inbound.example.com",
                                        "InboundHash": "hash",
                                        "InboundSpamThreshold": 5,
                                        "TrackOpens": true,
                                        "TrackLinks": "HtmlAndText",
                                        "IncludeBounceContentInHook": false,
                                        "ClickHookUrl": "https://example.com/click",
                                        "EnableSmtpApiErrorHooks": true
                                      }
                                      """;

    private const string ServerListJson = """
                                          {
                                            "TotalCount": 1,
                                            "Servers": [
                                              {
                                                "ID": 42,
                                                "Name": "PostKit Testing",
                                                "ApiTokens": ["server-token-a"],
                                                "Color": "blue",
                                                "SmtpApiActivated": true,
                                                "RawEmailEnabled": false,
                                                "DeliveryType": "Sandbox",
                                                "ServerLink": "https://account.postmarkapp.com/servers/42",
                                                "InboundAddress": "hash@inbound.postmarkapp.com",
                                                "InboundHookUrl": "",
                                                "BounceHookUrl": "https://example.com/bounce",
                                                "OpenHookUrl": null,
                                                "DeliveryHookUrl": "https://example.com/delivery",
                                                "PostFirstOpenOnly": true,
                                                "InboundDomain": "inbound.example.com",
                                                "InboundHash": "hash",
                                                "InboundSpamThreshold": 5,
                                                "TrackOpens": true,
                                                "TrackLinks": "HtmlAndText",
                                                "IncludeBounceContentInHook": false,
                                                "ClickHookUrl": "https://example.com/click",
                                                "EnableSmtpApiErrorHooks": true
                                              }
                                            ]
                                          }
                                          """;

    private const string MessageStreamJson = """
                                             {
                                               "ID": "broadcast",
                                               "ServerID": 42,
                                               "Name": "Broadcast",
                                               "Description": "News",
                                               "MessageStreamType": "Broadcasts",
                                               "CreatedAt": "2026-01-01T00:00:00Z",
                                               "UpdatedAt": "2026-01-02T00:00:00Z",
                                               "ArchivedAt": null,
                                               "ExpectedPurgeDate": null,
                                               "SubscriptionManagementConfiguration": {
                                                 "UnsubscribeHandlingType": "Postmark"
                                               }
                                             }
                                             """;

    private const string MessageStreamListJson = """
                                                 {
                                                   "MessageStreams": [
                                                     {
                                                       "ID": "broadcast",
                                                       "ServerID": 42,
                                                       "Name": "Broadcast",
                                                       "Description": "News",
                                                       "MessageStreamType": "Broadcasts",
                                                       "CreatedAt": "2026-01-01T00:00:00Z",
                                                       "UpdatedAt": "2026-01-02T00:00:00Z",
                                                       "ArchivedAt": null,
                                                       "ExpectedPurgeDate": null,
                                                       "SubscriptionManagementConfiguration": {
                                                         "UnsubscribeHandlingType": "Postmark"
                                                       }
                                                     }
                                                   ]
                                                 }
                                                 """;

    private const string DomainJson = """
                                      {
                                        "ID": 12,
                                        "Name": "example.com",
                                        "SPFVerified": true,
                                        "SPFHost": "@",
                                        "SPFTextValue": "v=spf1 include:spf.mtasv.net ~all",
                                        "DKIMVerified": true,
                                        "WeakDKIM": false,
                                        "DKIMHost": "pm._domainkey",
                                        "DKIMTextValue": "k=rsa; p=abc",
                                        "DKIMPendingHost": "",
                                        "DKIMPendingTextValue": "",
                                        "DKIMRevokedHost": null,
                                        "DKIMRevokedTextValue": null,
                                        "SafeToRemoveRevokedKeyFromDNS": false,
                                        "DKIMUpdateStatus": "Verified",
                                        "ReturnPathDomain": "pm-bounces.example.com",
                                        "ReturnPathDomainVerified": true,
                                        "ReturnPathDomainCNAMEValue": "pm.mtasv.net"
                                      }
                                      """;

    private const string DomainListJson = """
                                          {
                                            "TotalCount": 1,
                                            "Domains": [
                                              {
                                                "ID": 12,
                                                "Name": "example.com",
                                                "SPFVerified": true,
                                                "SPFHost": "@",
                                                "SPFTextValue": "v=spf1 include:spf.mtasv.net ~all",
                                                "DKIMVerified": true,
                                                "WeakDKIM": false,
                                                "DKIMHost": "pm._domainkey",
                                                "DKIMTextValue": "k=rsa; p=abc",
                                                "DKIMPendingHost": "",
                                                "DKIMPendingTextValue": "",
                                                "DKIMRevokedHost": null,
                                                "DKIMRevokedTextValue": null,
                                                "SafeToRemoveRevokedKeyFromDNS": false,
                                                "DKIMUpdateStatus": "Verified",
                                                "ReturnPathDomain": "pm-bounces.example.com",
                                                "ReturnPathDomainVerified": true,
                                                "ReturnPathDomainCNAMEValue": "pm.mtasv.net"
                                              }
                                            ]
                                          }
                                          """;

    private const string SenderSignatureJson = """
                                               {
                                                 "ID": 77,
                                                 "Domain": "example.com",
                                                 "EmailAddress": "sender@example.com",
                                                 "ReplyToEmailAddress": "reply@example.com",
                                                 "Name": "Sender",
                                                 "Confirmed": true,
                                                 "SPFVerified": true,
                                                 "SPFHost": "@",
                                                 "SPFTextValue": "v=spf1 include:spf.mtasv.net ~all",
                                                 "DKIMVerified": true,
                                                 "WeakDKIM": false,
                                                 "DKIMHost": "pm._domainkey",
                                                 "DKIMTextValue": "k=rsa; p=abc",
                                                 "DKIMPendingHost": "",
                                                 "DKIMPendingTextValue": "",
                                                 "DKIMRevokedHost": null,
                                                 "DKIMRevokedTextValue": null,
                                                 "SafeToRemoveRevokedKeyFromDNS": false,
                                                 "DKIMUpdateStatus": "Verified",
                                                 "ReturnPathDomain": "pm-bounces.example.com",
                                                 "ReturnPathDomainVerified": true,
                                                 "ReturnPathDomainCNAMEValue": "pm.mtasv.net",
                                                 "ConfirmationPersonalNote": "Please confirm this sender."
                                               }
                                               """;

    private const string SenderSignatureListJson = """
                                                   {
                                                     "TotalCount": 1,
                                                     "SenderSignatures": [
                                                       {
                                                         "ID": 77,
                                                         "Domain": "example.com",
                                                         "EmailAddress": "sender@example.com",
                                                         "ReplyToEmailAddress": "reply@example.com",
                                                         "Name": "Sender",
                                                         "Confirmed": true
                                                       }
                                                     ]
                                                   }
                                                   """;

    [Fact]
    public async Task GetServerAsync_UsesServerEndpointAndMapsServer()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string> { ["/server"] = ServerJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetServerAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var server), result.ToString());
        Assert.Equal(42, server.Id);
        Assert.Equal("PostKit Testing", server.Name);
        Assert.Equal(ServerColor.Blue, server.Color);
        Assert.Equal(ServerDeliveryType.Sandbox, server.DeliveryType);
        Assert.Equal(LinkTracking.HtmlAndText, server.TrackLinks);
        Assert.Equal("server-token-a", Assert.Single(server.ApiTokens));
        Assert.Equal("/server", postmark.LastGetEndpoint);
    }

    [Fact]
    public async Task ListServersAsync_WithNameFilter_UsesAccountEndpointAndMapsPage()
    {
        var postmark = new RecordingPostmarkClient(accountGetResponses: new Dictionary<string, string>
        {
            ["/servers?count=25&offset=50&name=PostKit%20Testing"] = ServerListJson
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListServersAsync(25, 50, new ServerQuery { Name = "PostKit Testing" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(1, page.TotalCount);
        var server = Assert.Single(page.Servers);
        Assert.Equal(42, server.Id);
        Assert.Equal("/servers?count=25&offset=50&name=PostKit%20Testing", postmark.LastAccountGetEndpoint);
    }

    [Fact]
    public async Task CreateServerAsync_UsesAccountPost()
    {
        var postmark = new RecordingPostmarkClient(accountPostResponses: new Dictionary<string, string> { ["/servers"] = ServerJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateServerAsync(new ServerCreateParameters
        {
            Name = "PostKit Testing",
            Color = ServerColor.Blue,
            DeliveryType = ServerDeliveryType.Sandbox,
            SmtpApiActivated = true,
            RawEmailEnabled = false,
            BounceHookUrl = "https://example.com/bounce",
            TrackLinks = LinkTracking.HtmlAndText
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var server), result.ToString());
        Assert.Equal(42, server.Id);
        Assert.Equal("/servers", postmark.LastAccountPostEndpoint);
        var body = Assert.IsType<ServerRequestModel>(postmark.LastAccountPostBody);
        Assert.Equal("PostKit Testing", body.Name);
        Assert.Equal("Blue", body.Color);
        Assert.Equal("Sandbox", body.DeliveryType);
        Assert.Equal("HtmlAndText", body.TrackLinks);
    }

    [Fact]
    public async Task EditServerAsync_WithId_UsesAccountPut()
    {
        var postmark = new RecordingPostmarkClient(accountPutResponses: new Dictionary<string, string> { ["/servers/42"] = ServerJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.EditServerAsync(42, new ServerEditParameters { Name = "PostKit Testing", TrackOpens = true }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var server), result.ToString());
        Assert.Equal("PostKit Testing", server.Name);
        Assert.Equal("/servers/42", postmark.LastAccountPutEndpoint);
        var body = Assert.IsType<ServerRequestModel>(postmark.LastAccountPutBody);
        Assert.Equal("PostKit Testing", body.Name);
        Assert.True(body.TrackOpens);
    }

    [Fact]
    public async Task ListMessageStreamsAsync_WithQuery_MapsStreams()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/message-streams?MessageStreamType=all&IncludeArchivedStreams=true"] = MessageStreamListJson
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListMessageStreamsAsync(new MessageStreamQuery { MessageStreamType = MessageStreamListType.All, IncludeArchivedStreams = true }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var list), result.ToString());
        var stream = Assert.Single(list.MessageStreams);
        Assert.Equal("broadcast", stream.Id);
        Assert.Equal(MessageStreamType.Broadcasts, stream.MessageStreamType);
        Assert.Equal(UnsubscribeHandlingType.Postmark, stream.SubscriptionManagementConfiguration?.UnsubscribeHandlingType);
    }

    [Fact]
    public async Task CreateMessageStreamAsync_SendsRequest()
    {
        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string> { ["/message-streams"] = MessageStreamJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateMessageStreamAsync(new MessageStreamCreateParameters
        {
            Id = "broadcast",
            Name = "Broadcast",
            Description = "News",
            MessageStreamType = MessageStreamType.Broadcasts,
            SubscriptionManagementConfiguration = new MessageStreamSubscriptionManagementConfiguration { UnsubscribeHandlingType = UnsubscribeHandlingType.Postmark }
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var stream), result.ToString());
        Assert.Equal("broadcast", stream.Id);
        Assert.Equal("/message-streams", postmark.LastPostEndpoint);
        var body = Assert.IsType<MessageStreamRequestModel>(postmark.LastPostBody);
        Assert.Equal("broadcast", body.Id);
        Assert.Equal("Broadcasts", body.MessageStreamType);
        Assert.Equal("Postmark", body.SubscriptionManagementConfiguration?.UnsubscribeHandlingType);
    }

    [Fact]
    public async Task EditMessageStreamAsync_UsesPatch()
    {
        var postmark = new RecordingPostmarkClient(patchResponses: new Dictionary<string, string> { ["/message-streams/broadcast"] = MessageStreamJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.EditMessageStreamAsync("broadcast", new MessageStreamEditParameters { Name = "Broadcast Updated", Description = "" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out _), result.ToString());
        Assert.Equal("/message-streams/broadcast", postmark.LastPatchEndpoint);
        var body = Assert.IsType<MessageStreamRequestModel>(postmark.LastPatchBody);
        Assert.Equal("Broadcast Updated", body.Name);
        Assert.Equal(string.Empty, body.Description);
    }

    [Fact]
    public async Task ArchiveMessageStreamAsync_UsesEmptyPost()
    {
        var postmark = new RecordingPostmarkClient(emptyPostResponses: new Dictionary<string, string>
        {
            ["/message-streams/broadcast/archive"] = """{"ID":"broadcast","ServerID":42,"ExpectedPurgeDate":"2026-07-01T00:00:00Z"}"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ArchiveMessageStreamAsync("broadcast", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var archive), result.ToString());
        Assert.Equal("broadcast", archive.Id);
        Assert.Equal("/message-streams/broadcast/archive", postmark.LastEmptyPostEndpoint);
    }

    [Fact]
    public async Task ListDomainsAsync_MapsPage()
    {
        var postmark = new RecordingPostmarkClient(accountGetResponses: new Dictionary<string, string> { ["/domains?count=10&offset=5"] = DomainListJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListDomainsAsync(10, 5, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(1, page.TotalCount);
        var domain = Assert.Single(page.Domains);
        Assert.Equal(12, domain.Id);
        Assert.True(domain.DkimVerified);
        Assert.Equal("/domains?count=10&offset=5", postmark.LastAccountGetEndpoint);
    }

    [Fact]
    public async Task CreateDomainAsync_SendsAccountPost()
    {
        var postmark = new RecordingPostmarkClient(accountPostResponses: new Dictionary<string, string> { ["/domains"] = DomainJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateDomainAsync(new DomainCreateParameters { Name = "example.com", ReturnPathDomain = "pm-bounces.example.com" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var domain), result.ToString());
        Assert.Equal("example.com", domain.Name);
        Assert.Equal("/domains", postmark.LastAccountPostEndpoint);
        var body = Assert.IsType<DomainCreateRequestModel>(postmark.LastAccountPostBody);
        Assert.Equal("example.com", body.Name);
        Assert.Equal("pm-bounces.example.com", body.ReturnPathDomain);
    }

    [Fact]
    public async Task CreateDomainAsync_WithSurroundingWhitespaceName_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateDomainAsync(new DomainCreateParameters { Name = " example.com " }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The domain create parameters name cannot start or end with whitespace. Pass a domain name like 'example.com' without surrounding whitespace. Actual length: 13.", error.Message);
        Assert.Null(postmark.LastAccountPostEndpoint);
    }

    [Fact]
    public async Task VerifyDomainDkimAsync_UsesAccountPutWithoutBody()
    {
        var postmark = new RecordingPostmarkClient(accountEmptyPutResponses: new Dictionary<string, string> { ["/domains/12/verifyDkim"] = DomainJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.VerifyDomainDkimAsync(12, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var domain), result.ToString());
        Assert.True(domain.DkimVerified);
        Assert.Equal("/domains/12/verifyDkim", postmark.LastAccountEmptyPutEndpoint);
    }

    [Fact]
    public async Task VerifyDomainReturnPathAsync_UsesAccountPutWithoutBody()
    {
        var postmark = new RecordingPostmarkClient(accountEmptyPutResponses: new Dictionary<string, string> { ["/domains/12/verifyReturnPath"] = DomainJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.VerifyDomainReturnPathAsync(12, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var domain), result.ToString());
        Assert.True(domain.ReturnPathDomainVerified);
        Assert.Equal("/domains/12/verifyReturnPath", postmark.LastAccountEmptyPutEndpoint);
    }

    [Fact]
    public async Task RotateDomainDkimAsync_UsesAccountPostWithoutBody()
    {
        var postmark = new RecordingPostmarkClient(accountEmptyPostResponses: new Dictionary<string, string> { ["/domains/12/rotatedkim"] = DomainJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.RotateDomainDkimAsync(12, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var rotation), result.ToString());
        Assert.Equal(12, rotation.Id);
        Assert.Equal("Verified", rotation.DkimUpdateStatus);
        Assert.Equal("/domains/12/rotatedkim", postmark.LastAccountEmptyPostEndpoint);
    }

    [Fact]
    public async Task ListSenderSignaturesAsync_MapsPage()
    {
        var postmark = new RecordingPostmarkClient(accountGetResponses: new Dictionary<string, string> { ["/senders?count=10&offset=5"] = SenderSignatureListJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListSenderSignaturesAsync(10, 5, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(1, page.TotalCount);
        var signature = Assert.Single(page.SenderSignatures);
        Assert.Equal(77, signature.Id);
        Assert.Equal("sender@example.com", signature.EmailAddress);
    }

    [Fact]
    public async Task CreateSenderSignatureAsync_SendsAccountPost()
    {
        var postmark = new RecordingPostmarkClient(accountPostResponses: new Dictionary<string, string> { ["/senders"] = SenderSignatureJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateSenderSignatureAsync(new SenderSignatureCreateParameters
        {
            FromEmail = "sender@example.com",
            Name = "Sender",
            ReplyToEmailAddress = "reply@example.com",
            ReturnPathDomain = "pm-bounces.example.com",
            ConfirmationPersonalNote = "Please confirm this sender."
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var signature), result.ToString());
        Assert.Equal(77, signature.Id);
        Assert.Equal("Please confirm this sender.", signature.ConfirmationPersonalNote);
        Assert.Equal("/senders", postmark.LastAccountPostEndpoint);
        var body = Assert.IsType<SenderSignatureCreateRequestModel>(postmark.LastAccountPostBody);
        Assert.Equal("sender@example.com", body.FromEmail);
        Assert.Equal("reply@example.com", body.ReplyToEmailAddress);
        Assert.Equal("pm-bounces.example.com", body.ReturnPathDomain);
        Assert.Equal("Please confirm this sender.", body.ConfirmationPersonalNote);
    }

    [Fact]
    public async Task EditSenderSignatureAsync_SendsAccountPut()
    {
        var postmark = new RecordingPostmarkClient(accountPutResponses: new Dictionary<string, string> { ["/senders/77"] = SenderSignatureJson });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.EditSenderSignatureAsync(77, new SenderSignatureEditParameters
        {
            Name = "Updated Sender",
            ReplyToEmailAddress = "updated-reply@example.com",
            ReturnPathDomain = "pm-bounces.example.com",
            ConfirmationPersonalNote = "Please confirm this updated sender."
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var signature), result.ToString());
        Assert.Equal(77, signature.Id);
        Assert.Equal("/senders/77", postmark.LastAccountPutEndpoint);
        var body = Assert.IsType<SenderSignatureEditRequestModel>(postmark.LastAccountPutBody);
        Assert.Equal("Updated Sender", body.Name);
        Assert.Equal("updated-reply@example.com", body.ReplyToEmailAddress);
        Assert.Equal("pm-bounces.example.com", body.ReturnPathDomain);
        Assert.Equal("Please confirm this updated sender.", body.ConfirmationPersonalNote);
    }

    [Fact]
    public void SenderSignatureCreateRequest_SerializesPostmarkRequestFieldNames()
    {
        var request = new SenderSignatureCreateRequestModel
        {
            FromEmail = "sender@example.com",
            Name = "Sender",
            ReplyToEmailAddress = "reply@example.com",
            ReturnPathDomain = "pm-bounces.example.com",
            ConfirmationPersonalNote = "Please confirm this sender."
        };

        var json = JsonSerializer.Serialize(request, PostmarkConfiguration.JsonSerializerOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("sender@example.com", root.GetProperty("FromEmail").GetString());
        Assert.Equal("Sender", root.GetProperty("Name").GetString());
        Assert.Equal("reply@example.com", root.GetProperty("ReplyToEmail").GetString());
        Assert.Equal("pm-bounces.example.com", root.GetProperty("ReturnPathDomain").GetString());
        Assert.Equal("Please confirm this sender.", root.GetProperty("ConfirmationPersonalNote").GetString());
        Assert.False(root.TryGetProperty("ReplyToEmailAddress", out _));
    }

    [Fact]
    public void SenderSignatureEditRequest_SerializesPostmarkRequestFieldNames()
    {
        var request = new SenderSignatureEditRequestModel
        {
            Name = "Updated Sender",
            ReplyToEmailAddress = "reply@example.com",
            ReturnPathDomain = "pm-bounces.example.com",
            ConfirmationPersonalNote = "Please confirm this sender."
        };

        var json = JsonSerializer.Serialize(request, PostmarkConfiguration.JsonSerializerOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Updated Sender", root.GetProperty("Name").GetString());
        Assert.Equal("reply@example.com", root.GetProperty("ReplyToEmail").GetString());
        Assert.Equal("pm-bounces.example.com", root.GetProperty("ReturnPathDomain").GetString());
        Assert.Equal("Please confirm this sender.", root.GetProperty("ConfirmationPersonalNote").GetString());
        Assert.False(root.TryGetProperty("ReplyToEmailAddress", out _));
    }

    [Fact]
    public async Task CreateSenderSignatureAsync_WithTooLongConfirmationPersonalNote_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());
        var note = new string('a', 401);

        var result = await client.CreateSenderSignatureAsync(new SenderSignatureCreateParameters
        {
            FromEmail = "sender@example.com",
            Name = "Sender",
            ConfirmationPersonalNote = note
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The sender signature create parameters confirmation personal note must not exceed 400 characters. Actual length: 401.", error.Message);
        Assert.Null(postmark.LastAccountPostEndpoint);
    }

    [Fact]
    public async Task CreateSenderSignatureAsync_WithSurroundingWhitespaceFromEmail_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateSenderSignatureAsync(new SenderSignatureCreateParameters
        {
            FromEmail = "sender@example.com ",
            Name = "Sender"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The sender signature create parameters from email address cannot start or end with whitespace. Set FromEmail to an address like 'sender@example.com' without surrounding whitespace. Actual length: 19.", error.Message);
        Assert.Null(postmark.LastAccountPostEndpoint);
    }

    [Fact]
    public async Task EditSenderSignatureAsync_WithoutName_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.EditSenderSignatureAsync(77, new SenderSignatureEditParameters
        {
            Name = null!,
            ReplyToEmailAddress = "reply@example.com"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The sender signature edit parameters name cannot be null.", error.Message);
        Assert.Null(postmark.LastAccountPutEndpoint);
    }

    [Fact]
    public async Task ResendSenderSignatureConfirmationAsync_UsesAccountPostWithoutBody()
    {
        var postmark = new RecordingPostmarkClient(accountEmptyPostResponses: new Dictionary<string, string> { ["/senders/77/resend"] = """{"ErrorCode":0,"Message":"Confirmation email resent."}""" });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ResendSenderSignatureConfirmationAsync(77, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var action), result.ToString());
        Assert.Equal("Confirmation email resent.", action.Message);
        Assert.Equal("/senders/77/resend", postmark.LastAccountEmptyPostEndpoint);
    }

    [Fact]
    public async Task RequestNewDkimForSenderSignatureAsync_WithPostmarkErrorCode_ReturnsFailure()
    {
        var postmark = new RecordingPostmarkClient(accountEmptyPostResponses: new Dictionary<string, string> { ["/senders/77/requestnewdkim"] = """{"ErrorCode":505,"Message":"This DKIM is already being renewed."}""" });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.RequestNewDkimForSenderSignatureAsync(77, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        var postmarkError = Assert.IsType<PostmarkError>(error);
        Assert.Equal((PostmarkErrorCode)505, postmarkError.ErrorCode);
        Assert.Equal("This DKIM is already being renewed.", postmarkError.Message);
    }

    private sealed class RecordingPostmarkClient(
        Dictionary<string, string>? getResponses = null,
        Dictionary<string, string>? accountGetResponses = null,
        Dictionary<string, string>? postResponses = null,
        Dictionary<string, string>? emptyPostResponses = null,
        Dictionary<string, string>? accountPostResponses = null,
        Dictionary<string, string>? accountEmptyPostResponses = null,
        Dictionary<string, string>? putResponses = null,
        Dictionary<string, string>? accountPutResponses = null,
        Dictionary<string, string>? accountEmptyPutResponses = null,
        Dictionary<string, string>? patchResponses = null,
        Dictionary<string, string>? deleteResponses = null,
        Dictionary<string, string>? accountDeleteResponses = null) : IPostmarkClient
    {
        public string? LastGetEndpoint { get; private set; }

        public string? LastAccountGetEndpoint { get; private set; }

        public string? LastPostEndpoint { get; private set; }

        public object? LastPostBody { get; private set; }

        public string? LastEmptyPostEndpoint { get; private set; }

        public string? LastAccountPostEndpoint { get; private set; }

        public object? LastAccountPostBody { get; private set; }

        public string? LastAccountEmptyPostEndpoint { get; private set; }

        public string? LastPutEndpoint { get; private set; }

        public object? LastPutBody { get; private set; }

        public string? LastAccountPutEndpoint { get; private set; }

        public object? LastAccountPutBody { get; private set; }

        public string? LastAccountEmptyPutEndpoint { get; private set; }

        public string? LastPatchEndpoint { get; private set; }

        public object? LastPatchBody { get; private set; }

        public string? LastDeleteEndpoint { get; private set; }

        public string? LastAccountDeleteEndpoint { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountPostEndpoint = endpoint;
                LastAccountPostBody = body;
                return GetResponse<TResponse>(accountPostResponses, endpoint);
            }

            LastPostEndpoint = endpoint;
            LastPostBody = body;
            return GetResponse<TResponse>(postResponses, endpoint);
        }

        public Task<Result<TResponse>> PostAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountEmptyPostEndpoint = endpoint;
                return GetResponse<TResponse>(accountEmptyPostResponses, endpoint);
            }

            LastEmptyPostEndpoint = endpoint;
            return GetResponse<TResponse>(emptyPostResponses, endpoint);
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountGetEndpoint = endpoint;
                return GetResponse<TResponse>(accountGetResponses, endpoint);
            }

            LastGetEndpoint = endpoint;
            return GetResponse<TResponse>(getResponses, endpoint);
        }

        public Task<Result<TResponse>> PutAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountPutEndpoint = endpoint;
                LastAccountPutBody = body;
                return GetResponse<TResponse>(accountPutResponses, endpoint);
            }

            LastPutEndpoint = endpoint;
            LastPutBody = body;
            return GetResponse<TResponse>(putResponses, endpoint);
        }

        public Task<Result<TResponse>> PutAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountEmptyPutEndpoint = endpoint;
                return GetResponse<TResponse>(accountEmptyPutResponses, endpoint);
            }

            throw new InvalidOperationException("Server-level empty PutAsync should not be called in this test.");
        }

        public Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastPatchEndpoint = endpoint;
            LastPatchBody = body;
            return GetResponse<TResponse>(patchResponses, endpoint);
        }

        public Task<Result<TResponse>> DeleteAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountDeleteEndpoint = endpoint;
                return GetResponse<TResponse>(accountDeleteResponses, endpoint);
            }

            LastDeleteEndpoint = endpoint;
            return GetResponse<TResponse>(deleteResponses, endpoint);
        }

        private static Task<Result<TResponse>> GetResponse<TResponse>(Dictionary<string, string>? responses, string endpoint)
        {
            if (responses is null || !responses.TryGetValue(endpoint, out var responseJson))
                throw new InvalidOperationException($"No response was configured for endpoint '{endpoint}'.");

            var response = JsonSerializer.Deserialize<TResponse>(responseJson, PostmarkConfiguration.JsonSerializerOptions);
            if (response is null)
                throw new InvalidOperationException($"Configured response for endpoint '{endpoint}' deserialized to null.");

            return Task.FromResult(Result.Success(response));
        }
    }

    private sealed class TestLogger : ILogger<PostKitClient>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
