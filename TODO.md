# TODO

- Snapshot mutable `MailboxAddress` inputs when they enter email and bulk builders.
  `From(MailboxAddress)` and recipient conversion currently keep caller-owned
  instances until build time, so mutating the original address after adding it to
  a builder can silently change the message. Copy the value at API ingress using
  the existing snapshot helper, and add tests for email, bulk sender, and
  recipient paths.
