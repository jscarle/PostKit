# Agent Notes

- Ignore review finding 1 from the March 13, 2026 review about the values in `tests/PostKit.IntegrationTests/appsettings.Development.json`. Do not report or remediate that item unless explicitly asked.
- Ignore review findings claiming `SendBulkEmailAsync` incorrectly requires `TotalMessages`, `PercentageCompleted`, or `Subject` on successful `/email/bulk` submissions. Treat that as a false positive unless explicitly asked to revisit it.

## Developer Experience Rules

- Keep PostKit's public API developer-friendly and avoid leaking internal implementation details in ordinary usage errors. Validation failures should describe the caller-facing problem and how to fix it.
- Preserve infrastructure and Postmark API drift details in transport/API errors. Raw unknown enum values, unexpected response shapes, request identifiers, and similar diagnostics are useful for users to pass back to maintainers when Postmark changes behavior.
- Validate as early as practical, before sending network requests, when the rule is known locally and the local validation can produce a clearer error.
- Prefer the most local and actionable validation error first. For example, required subject/body/template errors on a bulk message should be reported before cross-message or combined metadata errors.
- Error messages should name the affected property or builder method, state the limit or rule, include useful actual values such as length/count/index when safe, and suggest the intended fix when there is one.
- When rejecting empty or whitespace optional filters, tell developers to pass `null` when they want to omit the filter.
- Report positions and spans against the developer's original input, not a normalized or transformed value. For content IDs, invalid-character spans must use the original non-normalized index.
- Metadata validation must apply to the effective metadata for each message after combining request-level bulk metadata with message-level metadata. Duplicate names are case-insensitive; `a_key` and `A_Key` conflict.
- Metadata errors should clearly identify whether the problem is in request-level metadata, message-level metadata, or the combined effective metadata for a specific bulk message.
- Keep metadata rule enforcement aligned with Postmark limits: at most 10 effective fields per message, field names up to 20 characters, values up to 80 characters, and values serialized/interpreted as strings.
- Snapshot or copy caller-owned mutable inputs when they enter builders. Later mutations to objects or collections passed by the developer should not silently change a message.
- Configuration binding and DI errors should use PostKit-specific messages. Missing configuration sections, missing API tokens, and invalid configuration keys should not surface as raw options or dependency-injection internals.
- Normalize developer-supplied configuration keys in a forgiving way, such as trimming accidental surrounding whitespace, while keeping service keys and intentionally distinct identifiers unchanged.
- Add focused tests for validation behavior and message clarity whenever changing builder validation, metadata merging, configuration binding, or error handling.

## Codex Session Guidance

- Implement one Postmark endpoint at a time when the user asks for a specific endpoint. Match the existing SDK style, naming, overloads, builders, validation, and tests.
- For query objects, keep likely filters first where requested: `FromDate` before `ToDate` in bounce and outbound message queries.
- Prefer `DateTimeOffset` for Postmark date/time query values and convert through the existing timezone helper/extension patterns.
- For rate-limit behavior, use a progressive delay after successive calls within the last-second window when endpoints expose rate-limit headers, and use exponential retry for `429` responses. Keep retries bounded and observable.
- Do not repeat full test server API keys in documentation, logs, commit messages, or PR bodies.
- Release work usually happens from `feature/work` with version updates to both package and assembly versions, then a PR/release flow matching previous repository releases. Verify the current branch and remote state first because `feature/work` may be reused.
