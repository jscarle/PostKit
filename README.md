[![Banner](https://raw.githubusercontent.com/jscarle/PostKit/refs/heads/develop/Banner.png)](https://github.com/jscarle/PostKit)

# PostKit

A MimeKit infused implementation of the Postmark API.

[![develop](https://img.shields.io/github/actions/workflow/status/jscarle/PostKit/develop.yml?branch=develop&logo=github)](https://github.com/jscarle/PostKit/actions/workflows/develop.yml)
[![nuget](https://img.shields.io/nuget/v/PostKit)](https://www.nuget.org/packages/PostKit)
[![downloads](https://img.shields.io/nuget/dt/PostKit)](https://www.nuget.org/packages/PostKit)

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

### Configuration

Add PostKit to your services and configure your Postmark API token:

```csharp
// Program.cs (Minimal API / Web App)
using PostKit;

var builder = WebApplication.CreateBuilder(args);

// Add PostKit to services
builder.Services.AddPostKit();

var app = builder.Build();
```

Configure your Postmark Server API Token in `appsettings.json`:

```json
{
  "PostKit": {
    "ServerApiToken": "your-postmark-server-token-here"
  }
}
```

Or set it via environment variables:

```bash
PostKit__ServerApiToken=your-postmark-server-token-here
```

If you need multiple Postmark configurations (for example, separate servers or tenants), register keyed services with `AddKeyedPostKit`. Each keyed registration binds to `PostKit:{configurationKey}`; if you omit `configurationKey`, the
`serviceKey.ToString()` value is used.

```csharp
builder.Services.AddKeyedPostKit("Marketing"); // binds PostKit:Marketing
```

```csharp
public enum PostmarkServer
{
    Development,
    Production
}

builder.Services.AddKeyedPostKit(PostmarkServer.Development); // binds PostKit:Development
builder.Services.AddKeyedPostKit(PostmarkServer.Production, "Default"); // binds PostKit:Default
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

### Basic Usage

PostKit uses a fluent builder pattern with the following capabilities:

- **Email Addresses**: Support for simple strings, name/address pairs, or MimeKit `MailboxAddress` objects
- **Multiple Recipients**: Chain `AlsoTo()`, `AlsoCc()`, or `AlsoBcc()` to add additional recipients
- **Validation**: Automatic validation of email addresses, character limits, and required fields

#### Simple Email

```csharp
using PostKit;

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
        var email = Email.CreateBuilder()
            .From("noreply@yourapp.com")
            .To("user@example.com")
            .WithSubject("Welcome to Our Service!")
            .WithTextBody("Thank you for signing up!")
            .Build();

        await _postKitClient.SendEmailAsync(email);
    }
}
```

#### Rich HTML Email

```csharp
var email = Email.CreateBuilder()
    .From("Sarah Johnson", "sarah@company.com")
    .To("customer@example.com")
    .WithSubject("Your Order Confirmation")
    .WithHtmlBody(@"
        <h1>Order Confirmed!</h1>
        <p>Thank you for your purchase. Your order #12345 has been confirmed.</p>
        <a href='https://yourapp.com/orders/12345'>View Order Details</a>
    ")
    .WithTextBody("Order Confirmed! Thank you for your purchase. Your order #12345 has been confirmed. View details at: https://yourapp.com/orders/12345")
    .Build();

await _postKitClient.SendEmailAsync(email);
```

#### Multiple Recipients

```csharp
var email = Email.CreateBuilder()
    .From("notifications@company.com")
    .To(new[] { "user1@example.com", "user2@example.com" })
    .Cc("manager@company.com")
    .Bcc("admin@company.com")
    .WithSubject("Team Update")
    .WithTextBody("Important team announcement...")
    .Build();

await _postKitClient.SendEmailAsync(email);
```

#### Batch Sending

```csharp
var welcomeEmail = Email.CreateBuilder()
    .From("noreply@yourapp.com")
    .To("user1@example.com")
    .WithSubject("Welcome!")
    .WithTextBody("Thanks for signing up")
    .Build();

var reminderEmail = Email.CreateBuilder()
    .From("noreply@yourapp.com")
    .To("user2@example.com")
    .WithSubject("Complete Your Profile")
    .WithTextBody("Finish setting up your account")
    .Build();

var batchResult = await _postKitClient.SendEmailBatchAsync(new[] { welcomeEmail, reminderEmail });

if (batchResult.IsSuccess(out var batchResponse))
{
    if (!batchResponse.IsSuccessful)
    {
        foreach (var result in batchResponse.Results.Where(static result => !result.IsSuccess()))
        {
            if (result.IsFailure(out var error, out _) && error is PostmarkError postmarkError)
                Console.WriteLine($"Batch item failed: {postmarkError.ErrorCode} - {postmarkError.Message}");
        }
    }
}
```

#### Advanced Features

```csharp
var email = Email.CreateBuilder()
    .From("newsletter@company.com")
    .To("subscriber@example.com")
    .ReplyTo("support@company.com")
    .WithSubject("Monthly Newsletter")
    .WithHtmlBody("<h1>Newsletter</h1><p>Check out our latest updates!</p>")
    .WithTag("newsletter")
    .WithMetadata("campaign", "monthly-2024")
    .WithMetadata("segment", "premium-users")
    .WithOpenTracking(true)
    .WithLinkTracking(LinkTracking.HtmlAndText)
    .UsingMessageStream(MessageStream.Broadcast)
    .WithHeader("X-Campaign-ID", "CAMP-001")
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

var email = Email.CreateBuilder()
    .From("billing@company.com")
    .To("customer@example.com")
    .WithSubject("Your Monthly Invoice")
    .WithHtmlBody($"<p>Please find your invoice attached.</p><img src=\"{logo.ContentId}\" alt=\"Company Logo\" />")
    .WithAttachment(invoice)
    .WithAttachment(logo)
    .Build();

await _postKitClient.SendEmailAsync(email);
```

### Size Limits

Postmark limits `TextBody` and `HtmlBody` to 5 MB each, and total message size (including attachments) to 10 MB. When batching, Postmark accepts up to 500 emails per batch and batch payloads are limited to 50 MB. PostKit uses a conservative
estimate to prevent grossly oversized requests. Actual size limits will be enforced by the Postmark API.

### Error Handling

PostKit uses [LightResults](https://github.com/jscarle/LightResults) for error handling. `SendEmailAsync` returns a `Result<SendEmailResponse>` that contains either the response or error information:

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

**HttpError** - Returned for HTTP-level failures (network issues, timeouts, non-422 status codes):

```csharp
if (error is HttpError httpError)
{
    Console.WriteLine($"HTTP error: {httpError.StatusCode} - {httpError.Message}");
}
```

**PostmarkError** - Returned for Postmark API validation errors (422 status code), and for per-email failures inside a successful batch request:

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

See `PostmarkErrorCode` enum for the complete list of possible Postmark error codes.

### Message Streams

Postmark supports different message streams for different types of emails:

```csharp
// For transactional emails (default)
.UsingMessageStream(MessageStream.Transactional)

// For broadcast/marketing emails
.UsingMessageStream(MessageStream.Broadcast)

// Or use a custom stream ID
.UsingMessageStream("custom-stream-id")
```

### Complete Console Application Example

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PostKit;

var builder = Host.CreateApplicationBuilder(args);

// Add PostKit
builder.Services.AddPostKit();

var host = builder.Build();

// Get the PostKit client
var postKitClient = host.Services.GetRequiredService<IPostKitClient>();

// Create and send an email
var email = Email.CreateBuilder()
    .From("test@yourapp.com")
    .To("recipient@example.com")
    .WithSubject("Test Email from PostKit")
    .WithTextBody("Hello from PostKit! This email was sent using the PostKit library.")
    .WithHtmlBody("<h1>Hello from PostKit!</h1><p>This email was sent using the <strong>PostKit</strong> library.</p>")
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

## Bulk Email

|    | Endpoint                                                                                                                                           | Implementation |
|----|----------------------------------------------------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [Send bulk emails BETA](https://postmarkapp.com/developer/api/bulk-email#send-bulk-emails)                                                         |                |
| ✏️ | [Get the status/details of a bulk API request BETA](https://postmarkapp.com/developer/api/bulk-email#get-the-status-details-of-a-bulk-api-request) |                |

---

## Bounce API

|    | Endpoint                                                                                    | Implementation |
|----|---------------------------------------------------------------------------------------------|----------------|
| ✏️ | [Get delivery stats](https://postmarkapp.com/developer/api/bounce-api#get-delivery-stats)   |                |
| ✏️ | [Get bounces](https://postmarkapp.com/developer/api/bounce-api#get-bounces)                 |                |
| ✏️ | [Get a single bounce](https://postmarkapp.com/developer/api/bounce-api#get-a-single-bounce) |                |
| ✏️ | [Get bounce dump](https://postmarkapp.com/developer/api/bounce-api#get-bounce-dump)         |                |
| ✏️ | [Activate a bounce](https://postmarkapp.com/developer/api/bounce-api#activate-a-bounce)     |                |
| ✏️ | [Bounce types](https://postmarkapp.com/developer/api/bounce-api#bounce-types)               |                |
| ✏️ | [Rebound](https://postmarkapp.com/developer/api/bounce-api#rebound)                         |                |

---

## Templates API

|    | Endpoint                                                                                                                 | Implementation                       |
|----|--------------------------------------------------------------------------------------------------------------------------|--------------------------------------|
| ✅  | [Send email with template](https://postmarkapp.com/developer/api/templates-api#send-email-with-template)                 | `IPostKitClient.SendEmailAsync`      |
| ✅  | [Send batch with templates](https://postmarkapp.com/developer/api/templates-api#send-batch-with-templates)               | `IPostKitClient.SendEmailBatchAsync` |
| ✏️ | [Push templates to another server](https://postmarkapp.com/developer/api/templates-api#push-templates-to-another-server) |                                      |
| ✏️ | [Get a template](https://postmarkapp.com/developer/api/templates-api#get-a-template)                                     |                                      |
| ✏️ | [Create a template](https://postmarkapp.com/developer/api/templates-api#create-a-template)                               |                                      |
| ✏️ | [Edit a template](https://postmarkapp.com/developer/api/templates-api#edit-a-template)                                   |                                      |
| ✏️ | [List templates](https://postmarkapp.com/developer/api/templates-api#list-templates)                                     |                                      |
| ✏️ | [Delete a template](https://postmarkapp.com/developer/api/templates-api#delete-a-template)                               |                                      |
| ✏️ | [Validate a template](https://postmarkapp.com/developer/api/templates-api#validate-a-template)                           |                                      |

---

## Server API

|    | Endpoint                                                                            | Implementation |
|----|-------------------------------------------------------------------------------------|----------------|
| ✏️ | [Get the server](https://postmarkapp.com/developer/api/server-api#get-the-server)   |                |
| ✏️ | [Edit the server](https://postmarkapp.com/developer/api/server-api#edit-the-server) |                |

---

## Servers API

|    | Endpoint                                                                             | Implementation |
|----|--------------------------------------------------------------------------------------|----------------|
| ✏️ | [Get a server](https://postmarkapp.com/developer/api/servers-api#get-a-server)       |                |
| ✏️ | [Create a server](https://postmarkapp.com/developer/api/servers-api#create-a-server) |                |
| ✏️ | [Edit a server](https://postmarkapp.com/developer/api/servers-api#edit-a-server)     |                |
| ✏️ | [List servers](https://postmarkapp.com/developer/api/servers-api#list-servers)       |                |
| ✏️ | [Delete a server](https://postmarkapp.com/developer/api/servers-api#delete-a-server) |                |

---

## Message Streams API

|    | Endpoint                                                                                                           | Implementation |
|----|--------------------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [List message streams](https://postmarkapp.com/developer/api/message-streams-api#list-message-streams)             |                |
| ✏️ | [Get a message stream](https://postmarkapp.com/developer/api/message-streams-api#get-a-message-stream)             |                |
| ✏️ | [Edit a message stream](https://postmarkapp.com/developer/api/message-streams-api#edit-a-message-stream)           |                |
| ✏️ | [Create a message stream](https://postmarkapp.com/developer/api/message-streams-api#create-a-message-stream)       |                |
| ✏️ | [Archive a message stream](https://postmarkapp.com/developer/api/message-streams-api#archive-a-message-stream)     |                |
| ✏️ | [Unarchive a message stream](https://postmarkapp.com/developer/api/message-streams-api#unarchive-a-message-stream) |                |

---

## Messages API

|    | Endpoint                                                                                                                                          | Implementation |
|----|---------------------------------------------------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [Outbound message search](https://postmarkapp.com/developer/api/messages-api#outbound-message-search)                                             |                |
| ✏️ | [Outbound message details](https://postmarkapp.com/developer/api/messages-api#outbound-message-details)                                           |                |
| ✏️ | [Outbound message dump](https://postmarkapp.com/developer/api/messages-api#outbound-message-dump)                                                 |                |
| ✏️ | [Inbound message search](https://postmarkapp.com/developer/api/messages-api#inbound-message-search)                                               |                |
| ✏️ | [Inbound message details](https://postmarkapp.com/developer/api/messages-api#inbound-message-details)                                             |                |
| ✏️ | [Bypass rules for a blocked inbound message](https://postmarkapp.com/developer/api/messages-api#bypass-rules-for-a-blocked-inbound-message)       |                |
| ✏️ | [Retry a failed inbound message for processing](https://postmarkapp.com/developer/api/messages-api#retry-a-failed-inbound-message-for-processing) |                |
| ✏️ | [Message opens](https://postmarkapp.com/developer/api/messages-api#message-opens)                                                                 |                |
| ✏️ | [Opens for a single message](https://postmarkapp.com/developer/api/messages-api#opens-for-a-single-message)                                       |                |
| ✏️ | [Message clicks](https://postmarkapp.com/developer/api/messages-api#message-clicks)                                                               |                |
| ✏️ | [Clicks for a single message](https://postmarkapp.com/developer/api/messages-api#clicks-for-a-single-message)                                     |                |

---

## Domains API

|    | Endpoint                                                                                       | Implementation |
|----|------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [List domains](https://postmarkapp.com/developer/api/domains-api#list-domains)                 |                |
| ✏️ | [Get domain details](https://postmarkapp.com/developer/api/domains-api#get-domain-details)     |                |
| ✏️ | [Create domain](https://postmarkapp.com/developer/api/domains-api#create-domain)               |                |
| ✏️ | [Edit domain](https://postmarkapp.com/developer/api/domains-api#edit-domain)                   |                |
| ✏️ | [Delete domain](https://postmarkapp.com/developer/api/domains-api#delete-domain)               |                |
| ✏️ | [Verify DKIM](https://postmarkapp.com/developer/api/domains-api#verify-dkim)                   |                |
| ✏️ | [Verify Return-Path](https://postmarkapp.com/developer/api/domains-api#verify-return-path)     |                |
| ✏️ | [Verify an SPF record](https://postmarkapp.com/developer/api/domains-api#verify-an-spf-record) |                |
| ✏️ | [Rotate DKIM keys](https://postmarkapp.com/developer/api/domains-api#rotate-dkim-keys)         |                |

---

## Sender signatures API

|    | Endpoint                                                                                              | Implementation |
|----|-------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [List sender signatures](https://postmarkapp.com/developer/api/signatures-api#list-sender-signatures) |                |
| ✏️ | [Get sender signature](https://postmarkapp.com/developer/api/signatures-api#get-sender-signature)     |                |
| ✏️ | [Create a signature](https://postmarkapp.com/developer/api/signatures-api#create-a-signature)         |                |
| ✏️ | [Edit a signature](https://postmarkapp.com/developer/api/signatures-api#edit-a-signature)             |                |
| ✏️ | [Delete a signature](https://postmarkapp.com/developer/api/signatures-api#delete-a-signature)         |                |
| ✏️ | [Resend a confirmation](https://postmarkapp.com/developer/api/signatures-api#resend-a-confirmation)   |                |
| ✏️ | [Verify an SPF record](https://postmarkapp.com/developer/api/signatures-api#verify-an-spf-record)     |                |
| ✏️ | [Request a new DKIM](https://postmarkapp.com/developer/api/signatures-api#request-a-new-dkim)         |                |

---

## Stats API

|    | Endpoint                                                                                                 | Implementation |
|----|----------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [Get outbound overview](https://postmarkapp.com/developer/api/stats-api#get-outbound-overview)           |                |
| ✏️ | [Get sent counts](https://postmarkapp.com/developer/api/stats-api#get-sent-counts)                       |                |
| ✏️ | [Get bounce counts](https://postmarkapp.com/developer/api/stats-api#get-bounce-counts)                   |                |
| ✏️ | [Get spam complaints](https://postmarkapp.com/developer/api/stats-api#get-spam-complaints)               |                |
| ✏️ | [Get tracked email counts](https://postmarkapp.com/developer/api/stats-api#get-tracked-email-counts)     |                |
| ✏️ | [Get email open counts](https://postmarkapp.com/developer/api/stats-api#get-email-open-counts)           |                |
| ✏️ | [Get email platform usage](https://postmarkapp.com/developer/api/stats-api#get-email-platform-usage)     |                |
| ✏️ | [Get email client usage](https://postmarkapp.com/developer/api/stats-api#get-email-client-usage)         |                |
| ✏️ | [Get click counts](https://postmarkapp.com/developer/api/stats-api#get-click-counts)                     |                |
| ✏️ | [Get browser usage](https://postmarkapp.com/developer/api/stats-api#get-browser-usage)                   |                |
| ✏️ | [Get browser platform usage](https://postmarkapp.com/developer/api/stats-api#get-browser-platform-usage) |                |
| ✏️ | [Get click location](https://postmarkapp.com/developer/api/stats-api#get-click-location)                 |                |

---

## Triggers: Inbound rules

|    | Endpoint                                                                                                                          | Implementation |
|----|-----------------------------------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [List inbound rule triggers](https://postmarkapp.com/developer/api/inbound-rules-triggers-api#list-inbound-rule-triggers)         |                |
| ✏️ | [Create an inbound rule trigger](https://postmarkapp.com/developer/api/inbound-rules-triggers-api#create-an-inbound-rule-trigger) |                |
| ✏️ | [Delete a single trigger](https://postmarkapp.com/developer/api/inbound-rules-triggers-api#delete-a-single-trigger)               |                |

---

## Webhooks API

|    | Endpoint                                                                                | Implementation |
|----|-----------------------------------------------------------------------------------------|----------------|
| ✏️ | [List webhooks](https://postmarkapp.com/developer/api/webhooks-api#list-webhooks)       |                |
| ✏️ | [Get a webhook](https://postmarkapp.com/developer/api/webhooks-api#get-a-webhook)       |                |
| ✏️ | [Create a webhook](https://postmarkapp.com/developer/api/webhooks-api#create-a-webhook) |                |
| ✏️ | [Edit a webhook](https://postmarkapp.com/developer/api/webhooks-api#edit-a-webhook)     |                |
| ✏️ | [Delete a webhook](https://postmarkapp.com/developer/api/webhooks-api#delete-a-webhook) |                |

---

## Suppressions API

|    | Endpoint                                                                                            | Implementation |
|----|-----------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [Suppression dump](https://postmarkapp.com/developer/api/suppressions-api#suppression-dump)         |                |
| ✏️ | [Create a Suppression](https://postmarkapp.com/developer/api/suppressions-api#create-a-suppression) |                |
| ✏️ | [Delete a Suppression](https://postmarkapp.com/developer/api/suppressions-api#delete-a-suppression) |                |

---

## Data Removal API

|    | Endpoint                                                                                                                           | Implementation |
|----|------------------------------------------------------------------------------------------------------------------------------------|----------------|
| ✏️ | [Create a Data Removal request](https://postmarkapp.com/developer/api/data-removals-api#create-a-data-removal-request)             |                |
| ✏️ | [Check a Data Removal request status](https://postmarkapp.com/developer/api/data-removals-api#check-a-data-removal-request-status) |                |
