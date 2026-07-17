---
name: physics-blocker-council
description: Form a fail-closed cross-model scientific council using native Claude Fable 5 agents and bridged GPT-5.6 Sol sessions. Use when the user explicitly requests a physics council, cross-model challenge, or blocker brainstorm; or when a scientific blocker survives two substantive attempts or two validation cycles and no decisive safe next experiment is known. Do not trigger for an ordinary first failure, a running calculation, missing permission or credentials, or a question reserved for an external human ruling.
---

# Physics Blocker Council

Use Claude as the sole conductor. Run independent Fable and Sol teams, exchange
anonymized memos, preserve disagreement, and return the cheapest decisive safe
experiment. Model personas do not replace credentialed physicists.

## Preflight

1. Read `CLAUDE.md` and the authoritative restart/state documents.
2. Read the canonical protocol and boundaries completely:
   - `.agents/skills/physics-blocker-council/references/council-protocol.md`
   - `.agents/skills/physics-blocker-council/references/safety-boundaries.md`
   When installing, repairing, or diagnosing either bridge, also read
   `.agents/skills/physics-blocker-council/references/bridge-setup.md`.
3. Apply exclusions before triggers. A human-only ruling, missing authority or
   credentials, or a running calculation must be classified without checking a
   bridge, creating artifacts, or convening participants. Explicit requests do
   not override this exclusion.
4. Confirm the stuck threshold or an explicit request. Do not repeat a council
   against an unchanged evidence state.
5. Confirm a configured official Codex MCP server exposes `codex` and
   `codex-reply`. If unavailable, stop before any cross-model call and request
   installation approval.
6. Announce why the council is being invoked. Never bypass tool approval.
7. Initialize `/tmp` artifacts with:

   ```bash
   node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs init \
     --blocker BLOCKER_ID --conductor claude
   ```

## Run and adjudicate

Follow the canonical council protocol. Use native Fable 5 agents at high effort
for the Fable roles and separate GPT-5.6 Sol/high MCP sessions for the Sol roles.
Give both sides byte-identical frozen evidence. In every delegated prompt forbid
repository writes, commands, human-ruling inference, recursive councils, and calls
to the opposite model family.

Start each external Sol role with `codex`, setting `model=gpt-5.6-sol`,
`sandbox=read-only`, `approval-policy=never`, and high model reasoning in
`config`. Preserve the returned `threadId` and use `codex-reply` only for the
bounded cross-review/rebuttal rounds. The official interface is synchronous and
does not expose poll or cancel; keep prompts bounded and stop rather than opening
a new thread when continuation fails.

Validate and anonymize memos with the canonical helper before cross-review:

```bash
node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs validate-memo MEMO.json
node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs anonymize \
  MEMO.json --output ANONYMIZED.json
```

Reject malformed or gate-unsafe memos and preserve them as negative artifacts.
Treat gate compliance as pass/fail. Do not let council members write to the
repository. Resume implementation only when the original user request already
authorizes the recommended action; otherwise present the recommendation and ask.

Report the strongest proposal and objection, decisive test, cost, gate impact,
remaining human dependency, and whether the blocker actually changed.
For a preflight refusal, use `not-applicable` for proposal, objection, and test;
report zero council compute and state that the blocker is unchanged.
