# Repository instructions

## Git workflow

- Work directly on `main` unless the user requests another branch.
- For authorized Git operations, use the explicit repository prefix:
  `git -C /home/josh/Documents/GitHub/GeometricUnity <command>`.
  The saved approval rule covers `diff`, `add`, `commit`, `merge`, and
  `push` with this prefix; bare `git` invocations do not match that rule.
- Reuse these saved approvals for in-scope Git operations without asking
  for redundant conversational confirmation. Request execution escalation
  when required by the sandbox; the runtime decides whether a saved rule
  satisfies approval or a user prompt is necessary.
- Treat the active runtime approval rules as authoritative; this file records
  the workflow but does not grant permissions. If an authorized operation
  needs a new approval, request a narrowly scoped, repository-specific command
  prefix through the approval mechanism so the user can save it for reuse.
- Prefer separate, simple commands so the approval system can match each
  command reliably. Do not broaden permissions or bypass a required prompt.
- Saved approvals do not authorize unrelated work, destructive operations,
  or force-pushing. Follow the user's task scope and preserve unrelated edits.
