---
name: physics-blocker-council
description: Form a fail-closed cross-model scientific council using native GPT-5.6 Sol subagents and bridged local Claude Fable 5 sessions. Use when the user explicitly requests a physics council, cross-model challenge, or blocker brainstorm; or when a scientific blocker survives two substantive attempts or two validation cycles and no decisive safe next experiment is known. Do not trigger for an ordinary first failure, a running calculation, missing permission or credentials, or a question reserved for an external human ruling.
---

# Physics Blocker Council

Use Codex as the sole conductor. Run independent Sol and Fable teams, exchange
anonymized memos, preserve disagreement, and return the cheapest decisive safe
experiment. Treat every participant as a model persona, never as a credentialed
physicist or external adjudicator.

## Preflight

1. Read the repository instructions and authoritative restart/state documents
   before selecting blockers. In GeometricUnity, begin with `CLAUDE.md` and
   `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md`.
2. Read [safety-boundaries.md](references/safety-boundaries.md) and
   [council-protocol.md](references/council-protocol.md) completely. When
   installing, repairing, or diagnosing either bridge, also read
   [bridge-setup.md](references/bridge-setup.md).
3. Apply exclusions before triggers. If the blocker requires an external human
   ruling, missing authority/credentials, or completion of an already-running
   calculation, classify it without checking the bridge, creating artifacts, or
   convening participants. An explicit council request never overrides this rule.
4. Confirm that the task is genuinely stuck or explicitly requests the council.
   Count an unchanged blocker only once; do not invoke another council until its
   evidence state changes.
5. Confirm callable `claude_code`, `claude_code_check`, and
   `claude_code_reply` MCP tools. If unavailable, stop before any model call and
   report that the local Claude bridge is not installed or enabled.
6. Announce why the council is being invoked. Live bridge calls require the
   normal tool approval policy; never bypass an approval.
7. Initialize an ephemeral run under `/tmp`:

   ```bash
   node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs init \
     --blocker BLOCKER_ID --conductor codex
   ```

## Run the council

Follow [council-protocol.md](references/council-protocol.md). Use at most five
blockers and three roles per team. Spawn the Sol roles as native subagents using
`gpt-5.6-sol` with high reasoning when that configuration is available. Start
separate local Claude sessions with `model=claude-fable-5`, `effort=high`, a
strict read-only tool allowlist, and explicit turn/budget ceilings.

Include this instruction in every delegated prompt:

> Do not invoke a council, call another model, edit files, run commands, infer a
> human ruling, or coordinate with another participant. Analyze only the frozen
> evidence packet and return one memo conforming to the supplied schema.

Validate every memo before exchange:

```bash
node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs validate-memo MEMO.json
```

Anonymize accepted memos before cross-review:

```bash
node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs anonymize \
  MEMO.json --output ANONYMIZED.json
```

Initialize the scoring matrix from the anonymized proposals:

```bash
node .agents/skills/physics-blocker-council/scripts/council_artifacts.mjs init-matrix \
  ANONYMIZED-1.json ANONYMIZED-2.json --output MATRIX.json
```

Reject rather than repair a memo that lacks exact evidence bindings, a
falsifier, gate analysis, or the zero-claim/human-ruling firewalls. Preserve
rejected and withdrawn memos as negative artifacts in the temporary run.

## Adjudicate and resume

Score exact support, consistency, falsifiability, information gain, cost, and
convention robustness. Treat gate compliance as pass/fail, never as an averaged
score. Classify each result as one of:

- `decisive-experiment-ready`
- `exploratory-test-ready`
- `requires-new-phase`
- `requires-new-authorization`
- `human-ruling-required`
- `no-credible-local-route`

Do not let the council write to the repository. The root conductor may resume
the original task only when the recommended action is already authorized. For
new GeometricUnity phases, allocate from the registry and wire every validation
surface in one checkpoint. Keep council transcripts outside scanned roots; do
not commit them automatically.

Report the strongest proposal, the strongest objection, the decisive test,
estimated cost, gate impact, remaining human dependency, and whether the
original blocker changed.

For a preflight refusal, report the classification and reason, set proposal,
objection, and decisive test to `not-applicable`, report zero council compute,
name the external dependency, and state that the blocker is unchanged.
