# Changes

This file contains PostKit release notes and upgrade guidance. General installation, configuration, and usage documentation remains in the [README](README.md).

## Version 10.3.0 Retry Safety Changes

PostKit now limits automatic `429 Too Many Requests` retries to safe HTTP methods. Unsafe requests such as email submissions are returned to the caller after the first response so the application can decide whether its business operation can be
retried without producing duplicate effects. Requests that do not receive a conclusive HTTP response return `PostmarkUnknownOutcomeError`. Responses that do not match Postmark's documented contract return `PostmarkInvalidResponseError`.

PostKit removes inherited resilience handlers from its named HTTP client and installs a safe default pipeline. Applications can still customize or replace that pipeline after completing all PostKit registrations. See
[HTTP Resilience And Retry Safety](README.md#http-resilience-and-retry-safety) for details.

Direct package dependencies and test infrastructure packages were updated to their latest stable releases available for this version.

## Version 10.1.0 Breaking Changes Since v10.0.3

Upgrading from `10.0.3` to `10.1.0` requires source changes for the email builders, response models, and some namespaces.

Typical upgrade imports:

```csharp
using PostKit;
using PostKit.Common;
using PostKit.Emails;
```

### Namespace Changes

- `Email`, `ComposedEmailBuilder`, `TemplatedEmailBuilder`, `EmailSubmission`, and `EmailBatchSubmission` live in `PostKit.Emails`.
- `Attachment`, `LinkTracking`, and `MessageStream` live in `PostKit.Common`.
- The old `PostKit.Responses` namespace was removed. Use `PostKit.Emails.EmailSubmission` and `PostKit.Emails.EmailBatchSubmission`.

### Builder API Changes

`Email.CreateBuilder()` and the old `EmailBuilder` / `IEmail*Builder` interfaces were removed. Use `Email.Compose()` for body-based messages and `Email.FromTemplate(...)` for template messages.

```csharp
// 10.0.3
var email = Email.CreateBuilder()
    .From("Sender", "sender@example.com")
    .To("Recipient", "recipient@example.com")
    .WithSubject("Hello")
    .WithTextBody("Hello")
    .Build();

// 10.1.0
var email = Email.Compose()
    .From("sender@example.com", "Sender")
    .To("recipient@example.com", "Recipient")
    .Subject("Hello")
    .TextBody("Hello")
    .Build();
```

Method replacements:

| 10.0.3                                                           | 10.1.0                                                       |
|------------------------------------------------------------------|--------------------------------------------------------------|
| `Email.CreateBuilder()`                                          | `Email.Compose()` or `Email.FromTemplate(...)`               |
| `WithSubject(...)`                                               | `Subject(...)`                                               |
| `WithHtmlBody(...)`                                              | `HtmlBody(...)`                                              |
| `WithTextBody(...)`                                              | `TextBody(...)`                                              |
| `WithAttachment(...)`, `WithAttachments(...)`                    | `AddAttachment(...)`                                         |
| `WithHeader(...)`, `WithHeaders(...)`                            | `AddHeader(...)`                                             |
| `WithMetadata(...)`                                              | `AddMetadata(...)`                                           |
| `WithLinkTracking(...)`                                          | `UseLinkTracking(...)`                                       |
| `UsingMessageStream(...)`                                        | `UseMessageStream(...)`                                      |
| `WithOpenTracking()` / `WithOpenTracking(true)`                  | `EnableOpenTracking()`                                       |
| `WithOpenTracking(false)`                                        | Omit the call; there is no explicit false setter in `10.1.0` |
| `WithTemplate(idOrAlias, model, inlineCss)`                       | `Email.FromTemplate(idOrAlias, inlineCss).WithModel(model)`  |
| `AlsoTo(...)`, `AlsoCc(...)`, `AlsoBcc(...)`, `AlsoReplyTo(...)` | Repeat `To(...)`, `Cc(...)`, `Bcc(...)`, or `ReplyTo(...)`   |

Display name overloads are address-first in `10.1.0`. Use `.From("sender@example.com", "Sender Name")`, `.To("recipient@example.com", "Recipient Name")`, and the same order for `ReplyTo`, `Cc`, and `Bcc`.

Template emails now require a model before `Build()`. The model is serialized and snapshotted when `WithModel(...)` is called, must serialize to a JSON object, and uses `PostKitTemplateModelSerialization.DefaultSerializerOptions` unless
you pass per-call serializer options. If you inspect `Email.TemplateModel`, expect the JSON snapshot rather than the original CLR object.

Built emails also snapshot addresses, headers, metadata, attachments, and template models. Mutating the source collections or model object after `Build()` no longer changes the email that will be sent.

### Client Response Changes

- `IPostKitClient.SendEmailAsync` returns `Result<EmailSubmission>` instead of `Result<SendEmailResponse>`.
- `IPostKitClient.SendEmailBatchAsync` returns `Result<EmailBatchSubmission>` instead of `Result<SendEmailBatchResponse>`. Its parameter is now `IEnumerable<Email>` instead of `IReadOnlyCollection<Email>`.
- `SendEmailResponse`, `SendEmailBatchResponse`, and `SendEmailBatchResult` were removed.
- `EmailSubmission.MessageId` is now a `Guid` containing Postmark's message identifier. Use `EmailSubmission.InternetMessageId` when you need the RFC-style `<...@mtasv.net>` value.
- `InternetMessageId` preserves your outbound `Message-ID` header only when `X-PM-KeepID: true` is also set; otherwise it falls back to the Postmark-based `<...@mtasv.net>` value.
- `EmailBatchSubmission.Results` exposes `IReadOnlyList<Result<EmailSubmission>>`. Per-email failures are failed item results, typically containing `PostmarkError`, instead of `SendEmailBatchResult` entries with `ErrorCode`, `Message`,
  and `Response` properties.

### Interface And DI Changes

`IPostKitClient` now includes Postmark API methods beyond sending email, including messages, templates, webhooks, inbound rules, stats, suppressions, and data removals. Code that implements `IPostKitClient` directly, including hand-written
test doubles, must implement these methods.

PostKit service registration now validates that the selected configuration section defines at least one Postmark API token. `AddPostKit()` and `AddKeyedPostKit()` still exist, but missing tokens can fail during startup or first
resolution, and the explicit configuration overloads throw immediately when a required configuration section is missing. Default and keyed registrations also replace existing PostKit client/options registrations for the same service/key,
so register custom replacements after calling PostKit's registration helpers.

### Validation Changes

Several validations now happen before a request is sent:

- Custom message stream IDs reject reserved IDs such as `all` and IDs starting with `pm-`.
- Template models are serialized at build time and must be non-null JSON objects.
- Message and batch size estimates now include UTF-8 body bytes, Base64 attachment payloads, headers, metadata, and template model size.
- Header folding must use `CRLF` followed by whitespace.
