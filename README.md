# PostKit

A MimeKit infused implementation of the Postmark API.

## Quickstart

### Prerequisites

- .NET 9.0 or later
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

### Basic Usage

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

> **Note:** Postmark enforces a combined attachment size limit of 10 MB. PostKit automatically enforces this limit when you call `WithAttachment` or `WithAttachments`.

### Builder Pattern Features

PostKit uses a fluent builder pattern with the following capabilities:

- **Email Addresses**: Support for simple strings, name/address pairs, or MimeKit `MailboxAddress` objects
- **Multiple Recipients**: Chain `AlsoTo()`, `AlsoCc()`, or `AlsoBcc()` to add additional recipients
- **Validation**: Automatic validation of email addresses, character limits, and required fields
- **Flexible API**: Mix and match any combination of features

### Error Handling

PostKit uses structured logging and handles errors gracefully:

```csharp
try
{
    await _postKitClient.SendEmailAsync(email);
    // Email sent successfully - check logs for details
}
catch (Exception ex)
{
    // Handle any unexpected errors
    // PostKit will log detailed error information automatically
}
```

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
