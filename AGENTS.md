# Repository instructions

## Git workflow

- Work directly on `main` unless the user requests another branch.
- For authorized Git operations, use the explicit repository prefix:
  `git -C /home/josh/Documents/GitHub/GeometricUnity <command>`.
  The saved approval rule covers `diff`, `add`, `commit`, `merge`, and
  `push` with this prefix; bare `git` invocations do not match that rule.
- Prefer separate, simple commands so the approval system can match each
  command reliably. Do not broaden permissions or bypass a required prompt.
- Saved approvals do not authorize unrelated work, destructive operations,
  or force-pushing. Follow the user's task scope and preserve unrelated edits.
