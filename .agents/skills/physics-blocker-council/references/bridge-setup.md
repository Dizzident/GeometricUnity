# Bridge setup and trust boundary

This skill is usable without a bridge only for preflight exclusions. A live
cross-model council requires both locally configured stdio servers.

## Codex to Claude

- Bridge package: `@leo000001/claude-code-mcp@2.8.3`.
- Audited upstream commit: `47aa47b4f7f02f5dd5c6bc9174d30a42f54484f2`.
- Persistent prefix:
  `~/.local/share/physics-blocker-council/claude-code-mcp`.
- Expected installed `dist/index.js` SHA-256:
  `115620306d0beab12cfab22fb741dd319d4e255472c068aa90c58bbb9bdc9c81`.
- Expected MCP tools: `claude_code`, `claude_code_check`,
  `claude_code_reply`, and `claude_code_session`.

Launch the bridge through `/usr/bin/env -i` with only the minimum `HOME`,
`USER`, `PATH`, locale, and explicit Claude executable path. Never forward the
conductor's full environment. Disk resume remains disabled. Every council call
must use `strictAllowedTools=true`; participants receive read-only tools only,
or an empty built-in tool set when the frozen packet contains all evidence.

## Claude to Codex

Use the official local `codex mcp-server`, also launched through
`/usr/bin/env -i`. Pin the server configuration to `gpt-5.6-sol` and high
reasoning. The reciprocal skill must verify the effective model reported by the
session and stop on substitution or an unavailable model. The expected official
tool surface is `codex` plus `codex-reply`; it is synchronous and has no separate
poll or cancel operation.

## Installation and upgrades

External installation and global MCP registration require explicit user
approval. Install outside the repository with lifecycle scripts disabled. Before
registration, require all of:

1. `npm audit --omit=dev` reports zero known runtime vulnerabilities;
2. bridge build, typecheck, and unit tests pass in a disposable audited checkout;
3. the installed version and build hash match this file;
4. no live model call has occurred during those checks.

Treat any version, hash, tool-schema, permission, or model-name drift as a closed
bridge. Audit the new source and dependencies, update this record deliberately,
rerun the skill tests and repository validation, and obtain approval before
re-enabling live calls. Never use `npm audit fix --force` on the persistent
installation.

## Smoke-test sequence

After registration, test transport/tool discovery first without starting a model
session. Then, with the user present, make one bounded Claude call and one bounded
Codex call against a synthetic non-repository prompt. Disable built-in tools,
limit turns and budget, verify the effective model and effort, and cancel on any
permission request or schema drift. A successful smoke test permits only the
council workflow described by this skill; it is not blanket authorization for
arbitrary cross-model calls.
