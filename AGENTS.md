# Repository instructions

## Git workflow

- Work directly on `main` unless the user requests another branch; do not
  create a branch or ask the user to reconfirm this preference for routine work.
- For authorized Git operations, use the explicit repository prefix:
  `git -C /home/josh/Documents/GitHub/GeometricUnity <command>`.
- Automatically reuse matching active saved approvals for repository-scoped
  commands such as `diff`, `add`, `commit`, `merge`, and `push`. Proceed with
  already-authorized operations without redundant conversational confirmation.
  Do not assume approvals exist in every session; bare `git` invocations do
  not match rules that require the explicit repository prefix.
- Request execution escalation when required by the sandbox; the runtime
  decides whether a saved rule satisfies approval or a user prompt is needed.
  For a necessary new approval, offer an operation-specific prefix such as
  `["git", "-C", "/home/josh/Documents/GitHub/GeometricUnity", "push"]`
  through the runtime approval mechanism so the user can save it for later
  authorized operations, rather than asking a separate chat question.
- Treat the active runtime approval rules as authoritative; this file records
  the workflow but does not grant permissions. Do not grant yourself access,
  request blanket Git access, or change permission settings to avoid prompts.
- Prefer separate, simple commands so the approval system can match each
  command reliably. Do not broaden permissions or bypass a required prompt.
- Saved approvals do not authorize unrelated work, destructive operations,
  or force-pushing. Follow the user's task scope and preserve unrelated edits.
