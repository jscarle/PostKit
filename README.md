[![Banner](https://raw.githubusercontent.com/jscarle/PostKit/refs/heads/develop/Banner.png)](https://github.com/jscarle/PostKit)

# PostKit

A MimeKit infused implementation of the Postmark API.

[![develop](https://img.shields.io/github/actions/workflow/status/jscarle/PostKit/develop.yml?branch=develop&logo=github)](https://github.com/jscarle/PostKit/actions/workflows/develop.yml)
[![nuget](https://img.shields.io/nuget/v/PostKit)](https://www.nuget.org/packages/PostKit)
[![downloads](https://img.shields.io/nuget/dt/PostKit)](https://www.nuget.org/packages/PostKit)

Release notes and upgrade guidance are maintained in [CHANGES.md](CHANGES.md).

## Quickstart

### Prerequisites

- .NET 8.0 or later
- A [Postmark](https://postmarkapp.com) account with a Server API Token
- Verified sender email addresses in your Postmark account

### Installation

Install PostKit via NuGet:

```bash
dotnet add package PostKit
```

Current package targets `net8.0`, `net9.0`, and `net10.0`.

### Configuration

Add PostKit to your services and configure your Postmark API token:

```csharp
// Program.cs (Minimal API / Web App)
using PostKit;

var builder = WebApplication.CreateBuilder(args);

// Add PostKit to services
builder.Services.AddPostKit(builder.Configuration);

var app = builder.Build();
```

In ASP.NET Core apps, `builder.Services.AddPostKit()` also works because `IConfiguration` is already registered in the service provider. The explicit `IConfiguration` overload shown above validates that the `PostKit` section exists
immediately and does not require registering `IConfiguration` yourself.

Configure your Postmark Server API Token in `appsettings.json`:

```json
{
  "PostKit": {
    "ServerApiToken": "your-postmark-server-token-here",
    "AccountApiToken": "your-postmark-account-token-here"
  }
}
```

`AccountApiToken` is optional unless you call account-level endpoints such as template push or data removal.

Or set it via environment variables:

```bash
PostKit__ServerApiToken=your-postmark-server-token-here
```

If you need multiple Postmark configurations (for example, separate servers or tenants), register keyed services with `AddKeyedPostKit`. Each keyed registration binds to `PostKit:{configurationKey}`; if you omit `configurationKey`, the
`serviceKey.ToString()` value is used.

When `IConfiguration` is available from dependency injection, you can use the shorter overloads:

```csharp
builder.Services.AddKeyedPostKit("Marketing"); // binds PostKit:Marketing
builder.Services.AddKeyedPostKit("Production", "Default"); // resolves with key "Production", binds PostKit:Default
```

When you are configuring a standalone `ServiceCollection` or want missing sections to fail immediately, pass the configuration explicitly:

```csharp
builder.Services.AddKeyedPostKit("Marketing", builder.Configuration); // binds PostKit:Marketing
```

```csharp
public enum PostmarkServer
{
    Development,
    Production
}

builder.Services.AddKeyedPostKit(PostmarkServer.Development, builder.Configuration); // binds PostKit:Development
builder.Services.AddKeyedPostKit(PostmarkServer.Production, builder.Configuration, "Default"); // binds PostKit:Default
```

```json
{
  "PostKit": {
    "Marketing": { "ServerApiToken": "token-1" },
    "Development": { "ServerApiToken": "token-2" },
    "Default": { "ServerApiToken": "token-3" }
  }
}
```

### HTTP Resilience And Retry Safety

Postmark does not currently support idempotency keys. Repeating an unsafe request such as `POST /email` can therefore send the same email more than once. Custom `Message-ID` headers and metadata are useful for correlation, but Postmark does not document
them as deduplication keys.

PostKit proactively paces all requests against known or observed Postmark rate limits. When Postmark returns `429 Too Many Requests`, PostKit honors `Retry-After` when present and otherwise uses bounded exponential backoff with jitter, but it retries only
safe methods such as `GET`. Unsafe methods (`POST`, `PUT`, `PATCH`, and `DELETE`) return the first response without an automatic retry.

An `HttpClient` resilience handler runs inside PostKit's request and can retry before PostKit observes the result. Because the .NET standard resilience handler retries unsafe methods by default, PostKit's named HTTP client removes resilience handlers
registered earlier and installs a PostKit-owned standard pipeline. The default pipeline:

- retains standard rate limiting, total and attempt timeouts, safe-method retries, and circuit breaking;
- disables retries for unsafe methods; and
- excludes `429 Too Many Requests` from transport retry and circuit breaking so PostKit's shared rate-limit buckets remain the sole owner of `429` backoff.

Register application-wide HTTP client defaults before calling `AddPostKit`. No additional configuration is required for the safe default. To customize the named client, call `ConfigurePostKitHttpClient` after every `AddPostKit` and `AddKeyedPostKit`
registration:

```csharp
using PostKit;

builder.Services.AddPostKit(builder.Configuration);

builder.Services.ConfigurePostKitHttpClient(httpClient =>
{
    httpClient.SetHandlerLifetime(TimeSpan.FromMinutes(10));
});
```

Configurations added through this hook extend the existing named client. Adding another resilience handler without first removing PostKit's handler creates nested pipelines. A caller that replaces the default pipeline or re-enables unsafe retries assumes
responsibility for duplicate effects and for ensuring that PostKit remains the sole owner of `429` retries.

The application remains responsible for business-level idempotency. For security emails, billing notifications, and other sensitive operations, use a stable application operation ID and durable or process-local delivery state appropriate to the workflow.
Do not blindly retry an unsafe request that returns `PostmarkUnknownOutcomeError` or a successful unsafe request that returns `PostmarkInvalidResponseError`.

PostKit validates that the selected configuration section defines `ServerApiToken`, `AccountApiToken`, or both. When you use the explicit configuration overloads above, missing sections fail immediately and missing tokens fail during
startup or first resolution. Server-level endpoints still require `ServerApiToken`; account-level endpoints still require `AccountApiToken`.

Resolve keyed clients with the standard keyed DI APIs:

```csharp
var marketingClient = app.Services.GetRequiredKeyedService<IPostKitClient>("Marketing");
```

### Basic Usage

Most application code will use these namespaces:

```csharp
using PostKit;
using PostKit.Common;
using PostKit.Emails;
```

Add the endpoint namespace you need when working outside basic email sending: `PostKit.BulkEmails`, `PostKit.Bounces`, `PostKit.DataRemovals`, `PostKit.Domains`, `PostKit.InboundRules`, `PostKit.MessageStreams`, `PostKit.Messages`,
`PostKit.SenderSignatures`, `PostKit.Servers`, `PostKit.Stats`, `PostKit.Suppressions`, `PostKit.Templates`, or `PostKit.Webhooks`.

PostKit uses a fluent builder pattern with the following capabilities:

- **Email Addresses**: Support for simple strings, address/display-name pairs, or MimeKit `MailboxAddress` objects
- **Display Names**: Address/display-name overloads are address-first, for example `.From("sender@example.com", "Sender Name")`
- **Multiple Recipients**: Repeat `To()`, `Cc()`, `Bcc()`, or `ReplyTo()` to add additional recipients
- **Templates**: Send templated emails by template ID or alias, including in batches
- **Bulk Email API**: Submit broadcast bulk email jobs and poll their processing status
- **Suppressions API**: Query, create, and delete message stream suppressions
- **Validation**: Builder validation covers required fields, recipient counts, headers, metadata, message streams, template/body exclusivity, and size limits. Postmark still performs final sender and recipient validation

#### Simple Email

```csharp
using PostKit;
using PostKit.Emails;

// Inject IPostKitClient via dependency injection
public class EmailService
{
    private readonly IPostKitClient _postKitClient;

    public EmailService(IPostKitClient postKitClient)
    {
        _postKitClient = postKitClient;
    }

    public async Task SendWelcomeEmailAsync()
    {
        var email = Email.Compose()
            .From("noreply@yourapp.com")
            .To("user@example.com")
            .Subject("Welcome to Our Service!")
            .TextBody("Thank you for signing up!")
            .Build();

        await _postKitClient.SendEmailAsync(email);
    }
}
```

#### Rich HTML Email

```csharp
var email = Email.Compose()
    .From("sarah@company.com", "Sarah Johnson")
    .To("customer@example.com")
    .Subject("Your Order Confirmation")
    .HtmlBody(@"
        <h1>Order Confirmed!</h1>
        <p>Thank you for your purchase. Your order #12345 has been confirmed.</p>
        <a href='https://yourapp.com/orders/12345'>View Order Details</a>
    ")
    .TextBody("Order Confirmed! Thank you for your purchase. Your order #12345 has been confirmed. View details at: https://yourapp.com/orders/12345")
    .Build();

await _postKitClient.SendEmailAsync(email);
```

#### Multiple Recipients

```csharp
var email = Email.Compose()
    .From("notifications@company.com")
    .To(new[] { "user1@example.com", "user2@example.com" })
    .Cc("manager@company.com")
    .Bcc("admin@company.com")
    .Subject("Team Update")
    .TextBody("Important team announcement...")
    .Build();

await _postKitClient.SendEmailAsync(email);
```

#### Batch Sending

```csharp
var welcomeEmail = Email.Compose()
    .From("noreply@yourapp.com")
    .To("user1@example.com")
    .Subject("Welcome!")
    .TextBody("Thanks for signing up")
    .Build();

var reminderEmail = Email.Compose()
    .From("noreply@yourapp.com")
    .To("user2@example.com")
    .Subject("Complete Your Profile")
    .TextBody("Finish setting up your account")
    .Build();

var batchResult = await _postKitClient.SendEmailBatchAsync(new[] { welcomeEmail, reminderEmail });

if (batchResult.IsSuccess(out var batchResponse))
{
    if (!batchResponse.IsSuccessful)
    {
        foreach (var result in batchResponse.Results)
        {
            if (result.IsFailure(out var error, out _))
                Console.WriteLine($"Batch item failed: {error.Message}");
        }
    }
}
```

#### Templates

```csharp
var email = Email.FromTemplate("welcome-email", inlineCss: true)
    .From("noreply@yourapp.com")
    .To("user@example.com")
    .WithModel(new
    {
        name = "Alice",
        product = "PostKit"
    })
    .Build();

await _postKitClient.SendEmailAsync(email);
```

When batching template emails, every email in the batch must use a template. Mixing templated and non-templated emails in the same batch is rejected by the client.

Template models use `System.Text.Json` web defaults by default, so CLR properties such as `FirstName` serialize as `firstName`. If you want different naming,
set [PostKitTemplateModelSerialization.DefaultSerializerOptions](#template-model-serialization) globally or pass explicit serializer options to `WithModel(...)` for that call.

#### Advanced Features

```csharp
var email = Email.Compose()
    .From("newsletter@company.com")
    .To("subscriber@example.com")
    .ReplyTo("support@company.com")
    .Subject("Monthly Newsletter")
    .HtmlBody("<h1>Newsletter</h1><p>Check out our latest updates!</p>")
    .WithTag("newsletter")
    .AddMetadata("campaign", "monthly-2024")
    .AddMetadata("segment", "premium-users")
    .EnableOpenTracking()
    .UseLinkTracking(LinkTracking.HtmlAndText)
    .UseMessageStream(MessageStream.Broadcast)
    .AddHeader("X-Campaign-ID", "CAMP-001")
    .Build();

await _postKitClient.SendEmailAsync(email);
```

#### Attachments and Inline Images

```csharp
var invoice = Attachment.Create(
    name: "invoice.pdf",
    contentType: "application/pdf",
    content: await File.ReadAllBytesAsync("invoice.pdf"));

var logo = Attachment.Create(
    name: "logo.png",
    contentType: "image/png",
    content: await File.ReadAllBytesAsync("logo.png"),
    contentId: "logo@yourapp.com");

var email = Email.Compose()
    .From("billing@company.com")
    .To("customer@example.com")
    .Subject("Your Monthly Invoice")
    .HtmlBody($"<p>Please find your invoice attached.</p><img src=\"{logo.ContentId}\" alt=\"Company Logo\" />")
    .AddAttachment(invoice)
    .AddAttachment(logo)
    .Build();

await _postKitClient.SendEmailAsync(email);
```

#### Bulk Emails

```csharp
var bulkEmail = BulkEmail.FromTemplate("subscriber-welcome")
    .From("newsletter@company.com")
    .UseMessageStream(MessageStream.Broadcast)
    .AddMessage(BulkEmailMessage.FromTemplate()
        .To("alice@example.com")
        .WithModel(new { firstName = "Alice" })
        .Build())
    .AddMessage(BulkEmailMessage.FromTemplate()
        .To("bob@example.com")
        .WithModel(new { firstName = "Bob" })
        .Build())
    .Build();

var submitResult = await _postKitClient.SendBulkEmailAsync(bulkEmail);

if (submitResult.IsSuccess(out var submitted))
{
    var statusResult = await _postKitClient.GetBulkEmailStatusAsync(submitted.Id);
}
```

PostKit enforces the Bulk Email API's broadcast-stream requirement. `MessageStream.Transactional` and the `outbound` stream ID are rejected by the bulk builder.

Use `BulkEmail.Compose()` and `BulkEmailMessage.Compose()` for non-template bulk jobs with a shared subject and body:

```csharp
var bulkEmail = BulkEmail.Compose()
    .From("announcements@company.com")
    .Subject("Service update")
    .TextBody("A service update is available.")
    .UseMessageStream(MessageStream.Broadcast)
    .AddMessage(BulkEmailMessage.Compose()
        .To("alice@example.com")
        .Build())
    .AddMessage(BulkEmailMessage.Compose()
        .To("bob@example.com")
        .Build())
    .Build();
```

If Postmark accepts the bulk request but reports unprocessable messages, `SendBulkEmailAsync` returns a failed result with `BulkEmailValidationError`. The error includes both the accepted bulk job and the server-provided
unprocessable-content payload.

```csharp
using PostKit.Errors;

var result = await _postKitClient.SendBulkEmailAsync(bulkEmail);

if (result.IsFailure(out var error, out _) && error is BulkEmailValidationError validationError)
{
    Console.WriteLine($"Bulk request accepted as {validationError.AcceptedJob.Id}");
    Console.WriteLine(validationError.UnprocessableContent);
}
```

### Bounces

```csharp
using MimeKit;
using PostKit.Bounces;
using PostKit.Common;

var pageResult = await _postKitClient.GetBouncesAsync(
    MessageStream.Transactional,
    count: 100,
    query: new BounceQuery
    {
        Type = BounceType.HardBounce,
        Inactive = true,
        EmailFilter = new MailboxAddress(null, "user@example.com"),
    });

if (pageResult.IsSuccess(out var page))
{
    foreach (var bounce in page.Bounces)
        Console.WriteLine($"{bounce.Id}: {bounce.Type} for {bounce.Email}");
}
```

The bounce client also supports `GetBounceAsync`, `GetBounceDumpAsync`, `GetDeliveryStatsAsync`, and `ActivateBounceAsync`. Bounce date filters accept `DateTimeOffset` values and are converted to Postmark's US Eastern time before the
request is made.

### Messages

```csharp
using MimeKit;
using PostKit.Common;
using PostKit.Messages;

var messagesResult = await _postKitClient.SearchOutboundMessagesAsync(
    MessageStream.Transactional,
    count: 100,
    query: new OutboundMessageQuery
    {
        Recipient = new MailboxAddress(null, "user@example.com"),
        Status = OutboundMessageStatus.Sent,
        Metadata = new OutboundMessageMetadataFilter { Name = "campaign", Value = "welcome" },
    });

if (messagesResult.IsSuccess(out var page))
{
    foreach (var message in page.Messages)
        Console.WriteLine($"{message.MessageId}: {message.Status} {message.Subject}");

    var firstMessage = page.Messages.FirstOrDefault();
    if (firstMessage is not null)
    {
        var detailsResult = await _postKitClient.GetOutboundMessageDetailsAsync(firstMessage.MessageId);
        var dumpResult = await _postKitClient.GetOutboundMessageDumpAsync(firstMessage.MessageId);
    }
}
```

Outbound and inbound message date filters accept `DateTimeOffset` values and are converted to Postmark's US Eastern time before the request is made. Postmark currently supports one metadata filter per outbound message search.
Outbound message details expose nullable `TextBody`, `HtmlBody`, and `Body` properties because Postmark can omit content variants or the raw source on the details response. Use `GetOutboundMessageDumpAsync` when you specifically need the
raw source. The Messages API also includes `SearchInboundMessagesAsync`, `GetInboundMessageDetailsAsync`, `BypassInboundMessageRulesAsync`, `RetryInboundMessageAsync`, `SearchMessageOpensAsync`, `GetMessageOpensAsync`,
`SearchMessageClicksAsync`, and `GetMessageClicksAsync`.

### Templates, Webhooks, Stats, And Account APIs

```csharp
using PostKit.DataRemovals;
using PostKit.Domains;
using PostKit.InboundRules;
using PostKit.MessageStreams;
using PostKit.SenderSignatures;
using PostKit.Servers;
using PostKit.Stats;
using PostKit.Templates;
using PostKit.Webhooks;

var template = await _postKitClient.CreateTemplateAsync(new TemplateCreateParameters
{
    Name = "Welcome",
    Alias = "welcome-v1",
    Subject = "Welcome",
    HtmlBody = "<h1>Hello {{name}}</h1>",
});

var webhook = await _postKitClient.CreateWebhookAsync(new WebhookCreateParameters
{
    Url = "https://example.com/postmark/webhook",
    MessageStreamId = "outbound",
    Triggers = new WebhookTriggers { Bounce = new WebhookContentTrigger { Enabled = true, IncludeContent = false } },
});

var stats = await _postKitClient.GetOutboundStatsOverviewAsync(new OutboundStatsQuery
{
    FromDate = new DateOnly(2026, 1, 1),
    ToDate = new DateOnly(2026, 1, 31),
    MessageStreamId = "outbound",
});

var inboundRule = await _postKitClient.CreateInboundRuleTriggerAsync("blocked-sender@example.com");

var dataRemoval = await _postKitClient.CreateDataRemovalAsync(new DataRemovalCreateParameters
{
    RequestedBy = "privacy@example.com",
    RequestedFor = "recipient@example.com",
    NotifyWhenCompleted = true,
});

var servers = await _postKitClient.ListServersAsync(query: new ServerQuery { Name = "Marketing" });
var streams = await _postKitClient.ListMessageStreamsAsync(new MessageStreamQuery { IncludeArchivedStreams = true });
var domains = await _postKitClient.ListDomainsAsync();
var signatures = await _postKitClient.ListSenderSignaturesAsync();
```

Template CRUD, webhooks, inbound rules, stats, and message streams use server API tokens. `PushTemplatesAsync`, server management by ID, domains, sender signatures, and data-removal calls use Postmark account-level endpoints and require
`PostKit:AccountApiToken`.

### Suppressions

```csharp
using PostKit.Common;
using PostKit.Suppressions;

var suppressionsResult = await _postKitClient.GetSuppressionsAsync(
    MessageStream.Broadcast,
    new SuppressionQuery
    {
        Reason = SuppressionReason.ManualSuppression,
        FromDate = new DateOnly(2026, 3, 1),
        ToDate = new DateOnly(2026, 3, 31),
    });

if (suppressionsResult.IsSuccess(out var dump))
{
    foreach (var suppression in dump.Suppressions)
        Console.WriteLine($"{suppression.EmailAddress}: {suppression.Reason}");
}

var created = await _postKitClient.CreateSuppressionsAsync(MessageStream.Broadcast, "user@example.com");
var deleted = await _postKitClient.DeleteSuppressionsAsync(MessageStream.Broadcast, "user@example.com");
```

`GetSuppressionsAsync` maps the stream-scoped Postmark suppression dump, including `HardBounce`, `SpamComplaint`, and `ManualSuppression` reasons. Create and delete calls return per-address statuses from Postmark; deleting a
`HardBounce` suppression reactivates that address, while Postmark does not allow deleting `SpamComplaint` suppressions.

### Template Model Serialization

By default, template models use `System.Text.Json` web defaults with null values omitted. That means CLR property names are camel-cased.

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using PostKit;
using PostKit.Emails;

PostKitTemplateModelSerialization.DefaultSerializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = null,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
};

var serializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = null,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
};

var email = Email.FromTemplate("welcome-email")
    .From("noreply@yourapp.com")
    .To("user@example.com")
    .WithModel(new
    {
        FirstName = "Alice"
    }, serializerOptions)
    .Build();
```

### Size Limits

Postmark limits `TextBody` and `HtmlBody` to 5 MB each, email message size to 10 MB, bulk email payloads to 50 MB, and email batches to 500 items / 50 MB. PostKit uses conservative lower-bound estimates based on already-materialized values
to
prevent grossly oversized requests without doing expensive serialization. The local pre-checks count template-model bytes, body bytes, Base64 attachment bytes, and custom-header bytes, and the batch/bulk payload checks also include metadata
bytes; Postmark remains the authoritative source of truth for the final server-side size checks.

### Error Handling

PostKit uses [LightResults](https://github.com/jscarle/LightResults) for error handling. Every `IPostKitClient` operation returns a `Result<T>` value. Add `using PostKit.Errors;` when you want to inspect concrete error types.

`SendEmailAsync` returns a `Result<EmailSubmission>` that contains either the response or error information:

```csharp
var result = await _postKitClient.SendEmailAsync(email);

if (result.IsSuccess(out var response, out var error))
{
    // Email sent successfully
    Console.WriteLine($"Email sent with MessageId: {response.MessageId}");
    Console.WriteLine($"Internet Message-Id: {response.InternetMessageId}");
}
else
{
    // Handle the error
    Console.WriteLine($"Failed to send email: {error.Message}");
}
```

#### Error Types

PostKit may return different types of errors depending on the failure scenario:

**PostmarkUnknownOutcomeError** - Returned when PostKit does not receive a conclusive HTTP response because of a transport failure or timeout. `IsRetrySafe` identifies safe methods such as `GET`. For unsafe methods, Postmark may have processed the operation,
so the application must not retry unless it can prevent duplicate effects:

```csharp
if (error is PostmarkUnknownOutcomeError unknownOutcomeError)
{
    Console.WriteLine($"Unknown outcome: {unknownOutcomeError.Method} {unknownOutcomeError.Endpoint}");

    if (!unknownOutcomeError.IsRetrySafe)
    {
        // Reconcile the operation through application delivery state before retrying.
    }
}
```

**PostmarkInvalidResponseError** - Returned when Postmark supplies a response that cannot be deserialized or does not match the documented response contract. Examples include malformed JSON, missing acceptance fields, invalid message identifiers, and mismatched
batch result counts. Repeated occurrences most likely indicate a change in the Postmark API, and the error message directs the developer to [open an issue in the PostKit repository](https://github.com/jscarle/PostKit/issues):

```csharp
if (error is PostmarkInvalidResponseError invalidResponseError)
{
    Console.WriteLine($"Invalid response: {invalidResponseError.Method} {invalidResponseError.Endpoint}");
    Console.WriteLine($"HTTP status: {invalidResponseError.StatusCode}");
}
```

When an invalid response follows a successful unsafe request, the operation may already have been processed and must not be blindly retried.

**PostmarkError** - Returned when Postmark supplies a valid, structured Postmark error payload. This includes Postmark API validation errors and per-email failures inside a successfully processed batch request:

```csharp
if (error is PostmarkError postmarkError)
{
    Console.WriteLine($"Postmark error: {postmarkError.ErrorCode} - {postmarkError.Message}");

    // Handle specific error codes
    switch (postmarkError.ErrorCode)
    {
        case PostmarkErrorCode.SenderSignatureNotFound:
            // The From address doesn't have a verified sender signature
            break;
        case PostmarkErrorCode.InvalidEmailRequest:
            // The email request validation failed
            break;
        case PostmarkErrorCode.NotAllowedToSend:
            // Account has run out of credits
            break;
    }
}
```

**HttpError** - Returned when Postmark or an intermediary supplies a non-success HTTP response without a recognized structured Postmark error. `RetryAfter` contains the requested delay when the response provides a valid `Retry-After` header:

```csharp
if (error is HttpError httpError)
{
    Console.WriteLine($"HTTP error: {httpError.StatusCode} - {httpError.Message}");
    Console.WriteLine($"Retry after: {httpError.RetryAfter}");
}
```

Caller-requested cancellation continues to throw `OperationCanceledException`. If cancellation happens after an unsafe request was dispatched, its outcome can also be unknown, so cancellation should not trigger an unconditional retry.

**BulkEmailValidationError** - Returned when the Bulk Email API accepts a request but reports unprocessable messages:

```csharp
if (error is BulkEmailValidationError bulkError)
{
    Console.WriteLine($"Accepted bulk job: {bulkError.AcceptedJob.Id}");
    Console.WriteLine(bulkError.UnprocessableContent);
}
```

See `PostmarkErrorCode` enum for the complete list of possible Postmark error codes.

### Message Streams

Postmark supports different message streams for different types of emails. PostKit maps `MessageStream.Transactional` to `outbound` and `MessageStream.Broadcast` to `broadcast`. The Bulk Email API only supports broadcast streams.

```csharp
// For transactional emails (default)
.UseMessageStream(MessageStream.Transactional)

// For broadcast/marketing emails
.UseMessageStream(MessageStream.Broadcast)

// Or use a custom stream ID
.UseMessageStream("custom-stream-id")
```

### Complete Console Application Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PostKit;
using PostKit.Emails;

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["PostKit:ServerApiToken"] = "your-postmark-server-token-here",
    })
    .Build();

var services = new ServiceCollection();
services.AddLogging();
services.AddPostKit(configuration);

using var serviceProvider = services.BuildServiceProvider();

// Get the PostKit client
var postKitClient = serviceProvider.GetRequiredService<IPostKitClient>();

// Create and send an email
var email = Email.Compose()
    .From("test@yourapp.com")
    .To("recipient@example.com")
    .Subject("Test Email from PostKit")
    .TextBody("Hello from PostKit! This email was sent using the PostKit library.")
    .HtmlBody("<h1>Hello from PostKit!</h1><p>This email was sent using the <strong>PostKit</strong> library.</p>")
    .Build();

await postKitClient.SendEmailAsync(email);

Console.WriteLine("Email sent successfully!");
```

## Development

The following tables track development progress and map the different Postmark API endpoints to their respective methods in PostKit.

## Email API

|   | Endpoint                                                                                   | Implementation                       |
|---|--------------------------------------------------------------------------------------------|--------------------------------------|
| ✅ | [Send a single email](https://postmarkapp.com/developer/api/email-api#send-a-single-email) | `IPostKitClient.SendEmailAsync`      |
| ✅ | [Send batch emails](https://postmarkapp.com/developer/api/email-api#send-batch-emails)     | `IPostKitClient.SendEmailBatchAsync` |

---

## Bulk Email API

|   | Endpoint                                                                                                                                           | Implementation                           |
|---|----------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------|
| ✅ | [Send bulk emails BETA](https://postmarkapp.com/developer/api/bulk-email#send-bulk-emails)                                                         | `IPostKitClient.SendBulkEmailAsync`      |
| ✅ | [Get the status/details of a bulk API request BETA](https://postmarkapp.com/developer/api/bulk-email#get-the-status-details-of-a-bulk-api-request) | `IPostKitClient.GetBulkEmailStatusAsync` |

---

## Bounce API

|   | Endpoint                                                                                    | Implementation                         |
|---|---------------------------------------------------------------------------------------------|----------------------------------------|
| ✅ | [Get delivery stats](https://postmarkapp.com/developer/api/bounce-api#get-delivery-stats)   | `IPostKitClient.GetDeliveryStatsAsync` |
| ✅ | [Get bounces](https://postmarkapp.com/developer/api/bounce-api#get-bounces)                 | `IPostKitClient.GetBouncesAsync`       |
| ✅ | [Get a single bounce](https://postmarkapp.com/developer/api/bounce-api#get-a-single-bounce) | `IPostKitClient.GetBounceAsync`        |
| ✅ | [Get bounce dump](https://postmarkapp.com/developer/api/bounce-api#get-bounce-dump)         | `IPostKitClient.GetBounceDumpAsync`    |
| ✅ | [Activate a bounce](https://postmarkapp.com/developer/api/bounce-api#activate-a-bounce)     | `IPostKitClient.ActivateBounceAsync`   |
| ✅ | [Bounce types](https://postmarkapp.com/developer/api/bounce-api#bounce-types)               |                                        |

---

## Templates API

|   | Endpoint                                                                                                                 | Implementation                         |
|---|--------------------------------------------------------------------------------------------------------------------------|----------------------------------------|
| ✅ | [Send email with template](https://postmarkapp.com/developer/api/templates-api#send-email-with-template)                 | `IPostKitClient.SendEmailAsync`        |
| ✅ | [Send batch with templates](https://postmarkapp.com/developer/api/templates-api#send-batch-with-templates)               | `IPostKitClient.SendEmailBatchAsync`   |
| ✅ | [Push templates to another server](https://postmarkapp.com/developer/api/templates-api#push-templates-to-another-server) | `IPostKitClient.PushTemplatesAsync`    |
| ✅ | [Get a template](https://postmarkapp.com/developer/api/templates-api#get-a-template)                                     | `IPostKitClient.GetTemplateAsync`      |
| ✅ | [Create a template](https://postmarkapp.com/developer/api/templates-api#create-a-template)                               | `IPostKitClient.CreateTemplateAsync`   |
| ✅ | [Edit a template](https://postmarkapp.com/developer/api/templates-api#edit-a-template)                                   | `IPostKitClient.EditTemplateAsync`     |
| ✅ | [List templates](https://postmarkapp.com/developer/api/templates-api#list-templates)                                     | `IPostKitClient.ListTemplatesAsync`    |
| ✅ | [Delete a template](https://postmarkapp.com/developer/api/templates-api#delete-a-template)                               | `IPostKitClient.DeleteTemplateAsync`   |
| ✅ | [Validate a template](https://postmarkapp.com/developer/api/templates-api#validate-a-template)                           | `IPostKitClient.ValidateTemplateAsync` |

---

## Server API

|   | Endpoint                                                                            | Implementation                   |
|---|-------------------------------------------------------------------------------------|----------------------------------|
| ✅ | [Get the server](https://postmarkapp.com/developer/api/server-api#get-the-server)   | `IPostKitClient.GetServerAsync`  |
| ✅ | [Edit the server](https://postmarkapp.com/developer/api/server-api#edit-the-server) | `IPostKitClient.EditServerAsync` |

---

## Servers API

|   | Endpoint                                                                             | Implementation                     |
|---|--------------------------------------------------------------------------------------|------------------------------------|
| ✅ | [Get a server](https://postmarkapp.com/developer/api/servers-api#get-a-server)       | `IPostKitClient.GetServerAsync`    |
| ✅ | [Create a server](https://postmarkapp.com/developer/api/servers-api#create-a-server) | `IPostKitClient.CreateServerAsync` |
| ✅ | [Edit a server](https://postmarkapp.com/developer/api/servers-api#edit-a-server)     | `IPostKitClient.EditServerAsync`   |
| ✅ | [List servers](https://postmarkapp.com/developer/api/servers-api#list-servers)       | `IPostKitClient.ListServersAsync`  |
| ✅ | [Delete a server](https://postmarkapp.com/developer/api/servers-api#delete-a-server) | `IPostKitClient.DeleteServerAsync` |

---

## Message Streams API

|   | Endpoint                                                                                                           | Implementation                               |
|---|--------------------------------------------------------------------------------------------------------------------|----------------------------------------------|
| ✅ | [List message streams](https://postmarkapp.com/developer/api/message-streams-api#list-message-streams)             | `IPostKitClient.ListMessageStreamsAsync`     |
| ✅ | [Get a message stream](https://postmarkapp.com/developer/api/message-streams-api#get-a-message-stream)             | `IPostKitClient.GetMessageStreamAsync`       |
| ✅ | [Edit a message stream](https://postmarkapp.com/developer/api/message-streams-api#edit-a-message-stream)           | `IPostKitClient.EditMessageStreamAsync`      |
| ✅ | [Create a message stream](https://postmarkapp.com/developer/api/message-streams-api#create-a-message-stream)       | `IPostKitClient.CreateMessageStreamAsync`    |
| ✅ | [Archive a message stream](https://postmarkapp.com/developer/api/message-streams-api#archive-a-message-stream)     | `IPostKitClient.ArchiveMessageStreamAsync`   |
| ✅ | [Unarchive a message stream](https://postmarkapp.com/developer/api/message-streams-api#unarchive-a-message-stream) | `IPostKitClient.UnarchiveMessageStreamAsync` |

---

## Messages API

|   | Endpoint                                                                                                                                          | Implementation                                  |
|---|---------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------|
| ✅ | [Outbound message search](https://postmarkapp.com/developer/api/messages-api#outbound-message-search)                                             | `IPostKitClient.SearchOutboundMessagesAsync`    |
| ✅ | [Outbound message details](https://postmarkapp.com/developer/api/messages-api#outbound-message-details)                                           | `IPostKitClient.GetOutboundMessageDetailsAsync` |
| ✅ | [Outbound message dump](https://postmarkapp.com/developer/api/messages-api#outbound-message-dump)                                                 | `IPostKitClient.GetOutboundMessageDumpAsync`    |
| ✅ | [Inbound message search](https://postmarkapp.com/developer/api/messages-api#inbound-message-search)                                               | `IPostKitClient.SearchInboundMessagesAsync`     |
| ✅ | [Inbound message details](https://postmarkapp.com/developer/api/messages-api#inbound-message-details)                                             | `IPostKitClient.GetInboundMessageDetailsAsync`  |
| ✅ | [Bypass rules for a blocked inbound message](https://postmarkapp.com/developer/api/messages-api#bypass-rules-for-a-blocked-inbound-message)       | `IPostKitClient.BypassInboundMessageRulesAsync` |
| ✅ | [Retry a failed inbound message for processing](https://postmarkapp.com/developer/api/messages-api#retry-a-failed-inbound-message-for-processing) | `IPostKitClient.RetryInboundMessageAsync`       |
| ✅ | [Message opens](https://postmarkapp.com/developer/api/messages-api#message-opens)                                                                 | `IPostKitClient.SearchMessageOpensAsync`        |
| ✅ | [Opens for a single message](https://postmarkapp.com/developer/api/messages-api#opens-for-a-single-message)                                       | `IPostKitClient.GetMessageOpensAsync`           |
| ✅ | [Message clicks](https://postmarkapp.com/developer/api/messages-api#message-clicks)                                                               | `IPostKitClient.SearchMessageClicksAsync`       |
| ✅ | [Clicks for a single message](https://postmarkapp.com/developer/api/messages-api#clicks-for-a-single-message)                                     | `IPostKitClient.GetMessageClicksAsync`          |

---

## Domains API

|   | Endpoint                                                                                       | Implementation                               |
|---|------------------------------------------------------------------------------------------------|----------------------------------------------|
| ✅ | [List domains](https://postmarkapp.com/developer/api/domains-api#list-domains)                 | `IPostKitClient.ListDomainsAsync`            |
| ✅ | [Get domain details](https://postmarkapp.com/developer/api/domains-api#get-domain-details)     | `IPostKitClient.GetDomainAsync`              |
| ✅ | [Create domain](https://postmarkapp.com/developer/api/domains-api#create-domain)               | `IPostKitClient.CreateDomainAsync`           |
| ✅ | [Edit domain](https://postmarkapp.com/developer/api/domains-api#edit-domain)                   | `IPostKitClient.EditDomainAsync`             |
| ✅ | [Delete domain](https://postmarkapp.com/developer/api/domains-api#delete-domain)               | `IPostKitClient.DeleteDomainAsync`           |
| ✅ | [Verify DKIM](https://postmarkapp.com/developer/api/domains-api#verify-dkim)                   | `IPostKitClient.VerifyDomainDkimAsync`       |
| ✅ | [Verify Return-Path](https://postmarkapp.com/developer/api/domains-api#verify-return-path)     | `IPostKitClient.VerifyDomainReturnPathAsync` |
| ✅ | [Verify an SPF record](https://postmarkapp.com/developer/api/domains-api#verify-an-spf-record) | `IPostKitClient.VerifyDomainSpfAsync`        |
| ✅ | [Rotate DKIM keys](https://postmarkapp.com/developer/api/domains-api#rotate-dkim-keys)         | `IPostKitClient.RotateDomainDkimAsync`       |

---

## Sender signatures API

|   | Endpoint                                                                                              | Implementation                                          |
|---|-------------------------------------------------------------------------------------------------------|---------------------------------------------------------|
| ✅ | [List sender signatures](https://postmarkapp.com/developer/api/signatures-api#list-sender-signatures) | `IPostKitClient.ListSenderSignaturesAsync`              |
| ✅ | [Get sender signature](https://postmarkapp.com/developer/api/signatures-api#get-sender-signature)     | `IPostKitClient.GetSenderSignatureAsync`                |
| ✅ | [Create a signature](https://postmarkapp.com/developer/api/signatures-api#create-a-signature)         | `IPostKitClient.CreateSenderSignatureAsync`             |
| ✅ | [Edit a signature](https://postmarkapp.com/developer/api/signatures-api#edit-a-signature)             | `IPostKitClient.EditSenderSignatureAsync`               |
| ✅ | [Delete a signature](https://postmarkapp.com/developer/api/signatures-api#delete-a-signature)         | `IPostKitClient.DeleteSenderSignatureAsync`             |
| ✅ | [Resend a confirmation](https://postmarkapp.com/developer/api/signatures-api#resend-a-confirmation)   | `IPostKitClient.ResendSenderSignatureConfirmationAsync` |
| ✅ | [Verify an SPF record](https://postmarkapp.com/developer/api/signatures-api#verify-an-spf-record)     | `IPostKitClient.VerifySenderSignatureSpfAsync`          |
| ✅ | [Request a new DKIM](https://postmarkapp.com/developer/api/signatures-api#request-a-new-dkim)         | `IPostKitClient.RequestNewDkimForSenderSignatureAsync`  |

---

## Stats API

|   | Endpoint                                                                                                 | Implementation                                      |
|---|----------------------------------------------------------------------------------------------------------|-----------------------------------------------------|
| ✅ | [Get outbound overview](https://postmarkapp.com/developer/api/stats-api#get-outbound-overview)           | `IPostKitClient.GetOutboundStatsOverviewAsync`      |
| ✅ | [Get sent counts](https://postmarkapp.com/developer/api/stats-api#get-sent-counts)                       | `IPostKitClient.GetOutboundSentStatsAsync`          |
| ✅ | [Get bounce counts](https://postmarkapp.com/developer/api/stats-api#get-bounce-counts)                   | `IPostKitClient.GetOutboundBounceStatsAsync`        |
| ✅ | [Get spam complaints](https://postmarkapp.com/developer/api/stats-api#get-spam-complaints)               | `IPostKitClient.GetOutboundSpamComplaintStatsAsync` |
| ✅ | [Get tracked email counts](https://postmarkapp.com/developer/api/stats-api#get-tracked-email-counts)     | `IPostKitClient.GetOutboundTrackedEmailStatsAsync`  |
| ✅ | [Get email open counts](https://postmarkapp.com/developer/api/stats-api#get-email-open-counts)           | `IPostKitClient.GetOutboundOpenStatsAsync`          |
| ✅ | [Get email platform usage](https://postmarkapp.com/developer/api/stats-api#get-email-platform-usage)     | `IPostKitClient.GetOutboundEmailPlatformStatsAsync` |
| ✅ | [Get email client usage](https://postmarkapp.com/developer/api/stats-api#get-email-client-usage)         | `IPostKitClient.GetOutboundEmailClientStatsAsync`   |
| ✅ | [Get click counts](https://postmarkapp.com/developer/api/stats-api#get-click-counts)                     | `IPostKitClient.GetOutboundClickStatsAsync`         |
| ✅ | [Get browser usage](https://postmarkapp.com/developer/api/stats-api#get-browser-usage)                   | `IPostKitClient.GetOutboundClickBrowserStatsAsync`  |
| ✅ | [Get browser platform usage](https://postmarkapp.com/developer/api/stats-api#get-browser-platform-usage) | `IPostKitClient.GetOutboundClickPlatformStatsAsync` |
| ✅ | [Get click location](https://postmarkapp.com/developer/api/stats-api#get-click-location)                 | `IPostKitClient.GetOutboundClickLocationStatsAsync` |

---

## Triggers: Inbound rules

|   | Endpoint                                                                                                                          | Implementation                                 |
|---|-----------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------|
| ✅ | [List inbound rule triggers](https://postmarkapp.com/developer/api/inbound-rules-triggers-api#list-inbound-rule-triggers)         | `IPostKitClient.ListInboundRuleTriggersAsync`  |
| ✅ | [Create an inbound rule trigger](https://postmarkapp.com/developer/api/inbound-rules-triggers-api#create-an-inbound-rule-trigger) | `IPostKitClient.CreateInboundRuleTriggerAsync` |
| ✅ | [Delete a single trigger](https://postmarkapp.com/developer/api/inbound-rules-triggers-api#delete-a-single-trigger)               | `IPostKitClient.DeleteInboundRuleTriggerAsync` |

---

## Webhooks API

|   | Endpoint                                                                                | Implementation                      |
|---|-----------------------------------------------------------------------------------------|-------------------------------------|
| ✅ | [List webhooks](https://postmarkapp.com/developer/api/webhooks-api#list-webhooks)       | `IPostKitClient.ListWebhooksAsync`  |
| ✅ | [Get a webhook](https://postmarkapp.com/developer/api/webhooks-api#get-a-webhook)       | `IPostKitClient.GetWebhookAsync`    |
| ✅ | [Create a webhook](https://postmarkapp.com/developer/api/webhooks-api#create-a-webhook) | `IPostKitClient.CreateWebhookAsync` |
| ✅ | [Edit a webhook](https://postmarkapp.com/developer/api/webhooks-api#edit-a-webhook)     | `IPostKitClient.EditWebhookAsync`   |
| ✅ | [Delete a webhook](https://postmarkapp.com/developer/api/webhooks-api#delete-a-webhook) | `IPostKitClient.DeleteWebhookAsync` |

---

## Suppressions API

|   | Endpoint                                                                                            | Implementation                           |
|---|-----------------------------------------------------------------------------------------------------|------------------------------------------|
| ✅ | [Suppression dump](https://postmarkapp.com/developer/api/suppressions-api#suppression-dump)         | `IPostKitClient.GetSuppressionsAsync`    |
| ✅ | [Create a Suppression](https://postmarkapp.com/developer/api/suppressions-api#create-a-suppression) | `IPostKitClient.CreateSuppressionsAsync` |
| ✅ | [Delete a Suppression](https://postmarkapp.com/developer/api/suppressions-api#delete-a-suppression) | `IPostKitClient.DeleteSuppressionsAsync` |

---

## Data Removal API

|   | Endpoint                                                                                                                           | Implementation                          |
|---|------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------|
| ✅ | [Create a Data Removal request](https://postmarkapp.com/developer/api/data-removals-api#create-a-data-removal-request)             | `IPostKitClient.CreateDataRemovalAsync` |
| ✅ | [Check a Data Removal request status](https://postmarkapp.com/developer/api/data-removals-api#check-a-data-removal-request-status) | `IPostKitClient.GetDataRemovalAsync`    |
