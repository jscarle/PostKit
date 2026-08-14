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
- Current broad endpoint implementation order: MailboxAddress snapshot TODO, Templates API, Webhooks API, remaining Messages API, Triggers: Inbound rules, Stats API, Data Removal API.
- Live Postmark development/testing for this work is limited to the named `PostKit Testing` and `PostKit Alternate Testing` servers. Do not write their server API tokens into repository files, logs, docs, commits, PR bodies, or summaries.
- Never perform live Postmark reads or writes against servers named `Development`, `Infrastructure`, `Production`, `Test`, or any similarly production-sensitive server unless the user explicitly changes this constraint in a later message.
- Account-token live testing must be read-only unless the operation is pushing templates only between the named `PostKit Testing` and `PostKit Alternate Testing` servers.
- Current broad endpoint implementation order after the first API expansion: Server API, Servers API, Message Streams API, Domains API, Sender signatures API.
- For query objects, keep likely filters first where requested: `FromDate` before `ToDate` in bounce and outbound message queries.
- Prefer `DateTimeOffset` for Postmark date/time query values and convert through the existing timezone helper/extension patterns.
- For rate-limit behavior, use a progressive delay after successive calls within the last-second window when endpoints expose rate-limit headers, and use exponential retry for `429` responses. Keep retries bounded and observable.
- Do not repeat full test server API keys in documentation, logs, commit messages, or PR bodies.
- Release work usually happens from `feature/work` with version updates to both package and assembly versions, then a PR/release flow matching previous repository releases. Verify the current branch and remote state first because `feature/work` may be reused.

## Release Intent, Versioning, and Workflow

Treat explicit release requests—such as “Mint a new release,” “Release this,” “Publish this as a new release,” “Create a new release,” or any clear equivalent—as authorization to carry out PostKit's full release workflow, including committing, pushing, tagging, and publishing as needed.

If the user has not clearly requested a release, or if the appropriate change category cannot be determined confidently, ask for clarification **before** committing, pushing, creating a tag, or publishing a release.

Only change PostKit's package, assembly, and file versions when the user has clearly requested that a release be published. Do not bump versions during ordinary implementation, refactoring, bug-fixing, dependency updates, testing, commits, pushes, or pull-request preparation unless those actions are part of an explicitly requested release.

Choose the next version using these repository-specific rules:

- Minor, backward-compatible changes increment the patch component. Example: `10.3.1` → `10.3.2`.
- Moderate or breaking changes increment the middle component and reset the patch. Example: `10.3.1` → `10.4.0`.
- Increment the major component and reset the other components only when the newest supported .NET version changes. Example: `10.3.1` → `11.0.0`.
- Breaking changes alone do not justify a major-version increment in this repository. If multiple categories apply, use the highest applicable category.

Use three components for tags and release names, such as `v10.3.2`, and four components for `Version`, `AssemblyVersion`, and `FileVersion` in `src/PostKit/PostKit.csproj`, such as `10.3.2.0`. Create release tags as lightweight tags; historical tag types are mixed, but new release tags must be lightweight.

A clear release request authorizes this complete workflow:

1. Inspect the worktree, current branch, `origin`, `develop`, recent commits, pull requests, and releases before editing release metadata.
2. Determine the next version, update `Version`, `AssemblyVersion`, and `FileVersion` in `src/PostKit/PostKit.csproj`, and verify that the tag and release do not already exist.
3. Validate the release-scoped changes using PostKit's normal restore, warning-free Release build, unit tests for net8.0, net9.0, and net10.0, package creation, and Rider inspection requirements. Do not perform prohibited live Postmark operations.
4. Commit all release-scoped changes on the reusable `feature/work` branch using a concise past-tense sentence ending in a period, then push `feature/work`.
5. Open a ready pull request from `feature/work` to `develop` using recent `Summary`, `Why`, and `Validation` body conventions, enable squash auto-merge, and monitor Build, CodeQL, review state, and other required checks.
6. Fix and push any issue that prevents the pull request from merging, then wait until it is merged.
7. Fast-forward local `develop` from `origin/develop` and reset local `feature/work` to that latest `develop` commit. The remote feature branch may be deleted automatically after merge.
8. Create the lightweight `vX.Y.Z` tag on the latest `develop` commit and push that tag to `origin`.
9. Publish a non-draft GitHub release named `vX.Y.Z - Description`, using concise release-note bullets consistent with recent PostKit releases.
10. Monitor the release-triggered `Publish` workflow until the PostKit NuGet package is published successfully, fixing in-scope failures when possible.

Do not require separate confirmation for each Git or GitHub step after an unambiguous release command. Report the pull request, merge commit, tag, GitHub release URL, and NuGet publishing result when complete.
