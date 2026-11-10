# Changes

This file contains PostKit release notes and upgrade guidance. General installation, configuration, and usage documentation remains in the [README](README.md).

## Version 11.0.1 API Accuracy And Usability

Added `GetOutboundReadTimeStatsAsync` for `/stats/outbound/opens/readTimes`, with owned read-only totals and daily counts keyed by provider bucket names. Custom `IPostKitClient` implementations and hand-written test doubles must add this member.
Statistics accept live-observed empty series and omitted zero categories without deriving aggregate values from daily values. Overview statistics expose `WithReadTimeRecorded`.

Webhook edits validate before reading, merge changes with the current configuration, preserve omitted settings, and clear headers only for an explicit empty collection. This adds a GET and is not atomic with the update. Returned statuses are excluded from mutations.
Layout-template edits without a subject read the existing type and apply the appropriate validation. Failed or cancelled reads prevent updates. Mutations retain their no-automatic-retry behavior.

Added `DisableOpenTracking()` to composed/templated ordinary and bulk builders. Open webhook and REST event results expose optional `TimeSpan? ReadTime`. Missing outbound-message and bounce tags are preserved as null.
Bulk submission/status responses reject an empty returned request ID or non-finite/out-of-range completion percentage. Existing `Failed = 3`, `Cancelled = 4`, validation errors, required job subject, optional counters, and extension data remain supported.
Fresh composed/templated sandbox observations did not establish a nullable job subject or required counters across all applicable response shapes; those proposed changes remain pending live evidence.

### Property migration

Recompile callers and replace string comparisons or assignments with the corresponding enum values:

| Property | Current type |
| --- | --- |
| `Webhook.Status`, each webhook trigger's `Status` | `WebhookVerificationStatus?` (`Verified`, `Unverified`) |
| `WebhookVerificationTriggerResult.TriggerType` | `WebhookTriggerType` |
| `WebhookStatistics.Statuses` | `IReadOnlyDictionary<WebhookTriggerType, WebhookVerificationStatus>` |
| `WebhookStatistics.MetricsByTrigger` | `IReadOnlyDictionary<WebhookTriggerType, WebhookStatisticsMetrics>` |
| `MessageTrackingEvent.Platform`, open/click webhook `Platform`, `MessageTrackingQuery.Platform` | `MessagePlatform?` (`WebMail`, `Desktop`, `Mobile`, `Unknown`) |
| `OutboundMessageEventDetails.ClickLocation` | `LinkClickLocation?` (`Html`, `Text`) |
| `MessageClick.ClickLocation`, `ClickWebhookMessage.ClickLocation` | `LinkClickLocation` (required; missing/null values are rejected) |
| `OutboundMessageEventDetails.BounceId` | `long?` |
| `OutboundMessageEventDetails.SuppressSending` | `bool?` |
| `OutboundMessageEventDetails.Origin` | `SuppressionOrigin?` |
| Outbound-message and bounce `Tag` | `string?` |

The new enums preserve current Postmark wire strings internally. Unrecognized values produce actionable failures with the original value and the repository issue-reporting URL; `MessagePlatform.Unknown` remains a valid provider category.
`OutboundMessageEventDetails.Values` retains provider details for diagnostics. No duplicate string properties were added. See the README's contract-evidence notes for live versus fixture/source-derived coverage.

## Version 11.0.0 .NET 11 Support

Added a `net11.0` target alongside `net8.0`, `net9.0`, and `net10.0`. Building the repository requires the stable .NET 11 SDK selected by `global.json`.

Updated direct package dependencies and test infrastructure to their latest supported stable releases. The .NET 8 and 9 targets use Microsoft 10.0 packages because Microsoft 11.0 packages support .NET 10 and later.
LightResults is now version 11.0.0 across all supported targets.

Replaced the full MimeKit package with `MimeKit.Core` 5.0, removing the unused cryptography, BouncyCastle, and explicit `System.Security.Cryptography.Pkcs` dependencies.
Its core types, including `MailboxAddress` exposed by PostKit, moved from `MimeKit.dll` to `MimeKit.Core.dll`; recompile applications when upgrading. Applications using MimeKit's cryptography APIs must now reference `MimeKit.Cryptography` explicitly.
Address parsing also preserves punycode domains and trims ASCII whitespace from display names. See the [MimeKit 5 porting guide](https://github.com/jstedfast/MimeKit/blob/master/PortingGuide.md) for dependency migration details.

## Version 10.4.0 Webhook And Bulk API Additions

Added webhook statistics, explicit webhook verification, and cursor-based bulk request listing. Webhook create/edit parameters accept an optional `Verify` choice; verification failures expose typed per-trigger details through
`PostmarkError.Verification`. Statistics preserve Postmark's window and metric values, including missing response-time averages as null. Webhook status and trigger names remain open strings where the new endpoints return them.

`IPostKitClient` has three additional members: `GetWebhookStatisticsAsync`, `VerifyWebhookAsync`, and `ListBulkEmailRequestsAsync`. Custom implementations and hand-written test doubles must implement these methods. Existing methods, overloads,
configuration, and DI registration remain available.

Bulk status supports `Cancelled = 4` while retaining `Failed = 3` and existing failed-submission handling. Optional `ReleasedCount` and `FailedCount` are also retained unchanged in `AdditionalProperties`, including uninterpretable values.
List entries use `BulkEmailRequestSummary` with a nullable subject; the existing `BulkEmailJob.Subject` and send/status validation contracts are unchanged. See the README for verification outcomes, explicit verification choices, and pagination examples.

Updated package dependencies and test infrastructure, including the security update for `System.Security.Cryptography.Pkcs`.

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
