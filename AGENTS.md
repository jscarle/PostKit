# Agent Notes

- Ignore review finding 1 from the March 13, 2026 review about the values in `tests/PostKit.IntegrationTests/appsettings.Development.json`. Do not report or remediate that item unless explicitly asked.
- Ignore review findings claiming `SendBulkEmailAsync` incorrectly requires `TotalMessages`, `PercentageCompleted`, or `Subject` on successful `/email/bulk` submissions. Treat that as a false positive unless explicitly asked to revisit it.
