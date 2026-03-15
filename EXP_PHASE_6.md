# EXP_PHASE_6.md

Use the prompt below when you want Codex/Claude Code to execute the phase that
comes after Phase VI. It assumes the agent is starting cold in this repository.

```text
Execute the full post-Phase-VI evidence campaign phase from scratch. Treat this as an end-to-end implementation, execution, evaluation, and planning task.

Repository root:
- /home/josh/Documents/GitHub/GeometricUnity

Starting point and context:
- Phase V gap handoff: /home/josh/Documents/GitHub/GeometricUnity/GAPS_P5_01.md
- Phase V open issues: /home/josh/Documents/GitHub/GeometricUnity/PHASE_5_OPEN_ISSUES.md
- Phase V architecture: /home/josh/Documents/GitHub/GeometricUnity/ARCH_P5.md
- Phase V implementation plan: /home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P5.md
- Phase VI architecture: /home/josh/Documents/GitHub/GeometricUnity/ARCH_P6.md
- Phase VI implementation handoff: /home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P6.md
- Phase VI open issues: /home/josh/Documents/GitHub/GeometricUnity/PHASE_6_OPEN_ISSUES.md
- Assumptions ledger: /home/josh/Documents/GitHub/GeometricUnity/ASSUMPTIONS.md

Latest verified Phase V evidence set:
- Batch summary: /home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/summary.md
- Artifacts root: /home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/campaign_artifacts
- Logs root: /home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/logs
- Reference campaign spec: /home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json
- Batch runner: /home/josh/Documents/GitHub/GeometricUnity/scripts/run_phase5_gap_closure_batch.sh

What Phase VI was supposed to achieve:
1. Make the checked-in reference campaign satisfy the full evidence contract.
2. Make falsifier and observation coverage explicit in artifacts and dossiers.
3. Move the main evidence path from hand-authored Phase V value tables to bridged upstream artifacts where possible.
4. Upgrade the standard campaign beyond toy-only environment and toy-placeholder target evidence.
5. Leave the repository with a standard evidence campaign that is interpretable without reading source code.

What the current successful Phase V run actually proved:
1. The batch runner and campaign runner work.
2. The required artifact tree is emitted and verified.
3. The checked-in toy reference campaign is numerically clean.
4. The standard checked-in campaign still undershoots the intended evidence contract.

Primary objective:
Complete the phase after Phase VI: use the Phase VI outputs and upgraded evidence pipeline to produce an evidence-bearing campaign that can tell a non-physicist what survived, what failed, and what should be done next.

Binding execution rules:
1. Read ARCH_P6.md, IMPLEMENTATION_P6.md, and PHASE_6_OPEN_ISSUES.md first.
2. Treat IMPLEMENTATION_P6.md as the implementation spec for finishing Phase VI prerequisites if any are still open.
3. Do not assume the checked-in reference campaign is evidence-complete just because the old Phase V batch passed.
4. Use generated artifacts as primary evidence for evaluation, not planning prose.
5. Do not weaken validators or checks just to make the campaign pass.
6. Keep all new work aligned with the repo’s existing C# and JSON serialization patterns.
7. Add or update tests whenever a contract is closed.
8. Continue until the end-to-end evidence campaign succeeds or you hit a real blocker that cannot be resolved from repository context.

Mandatory workflow:
1. Read:
   - /home/josh/Documents/GitHub/GeometricUnity/ARCH_P6.md
   - /home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P6.md
   - /home/josh/Documents/GitHub/GeometricUnity/PHASE_6_OPEN_ISSUES.md
   - /home/josh/Documents/GitHub/GeometricUnity/ASSUMPTIONS.md
   - /home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/summary.md
2. Inspect the current checked-in reference campaign config and code paths.
3. Implement any still-missing Phase VI prerequisites, including at minimum:
   - campaign contract validation,
   - explicit sidecar evidence paths in the standard campaign,
   - coverage accounting in falsifier/dossier outputs,
   - background-bridge-driven evidence export where feasible,
   - non-toy environment and target realism improvements called for by Phase VI.
4. Run the relevant tests after each substantive fix.
5. Run the upgraded standard campaign end-to-end.
6. Verify the final artifacts, dossiers, logs, and manifests.
7. Evaluate the completed results strictly.
8. Then create:
   - /home/josh/Documents/GitHub/GeometricUnity/ARCH_P7.md
   - /home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P7.md
   - /home/josh/Documents/GitHub/GeometricUnity/PHASE_7_OPEN_ISSUES.md
   - /home/josh/Documents/GitHub/GeometricUnity/PHASE7_SUMMARY.md

Requirements for the evaluation:
1. Be strict.
2. Distinguish:
   - contracts implemented and actually exercised,
   - contracts implemented but not exercised by the standard campaign,
   - scientifically meaningful evidence,
   - toy/control-study-only evidence,
   - open problems that must move to Phase VII.
3. If a required sidecar or coverage field is missing, treat that as an open gap.
4. If a relevant test suite was not run or failed, treat the affected area as not closed.
5. If the campaign is still toy-only in a critical dimension, say so plainly.

Requirements for PHASE7_SUMMARY.md:
1. Write it for a smart non-physicist.
2. Explain:
   - what the campaign tested,
   - what passed,
   - what still does not count as real-world evidence,
   - what can now be done next,
   - what decisions a project lead can make from the results.
3. Avoid jargon where possible.
4. When jargon is unavoidable, translate it immediately into plain language.

Final response requirements:
1. Summarize what changed.
2. List the tests that were run.
3. Report the exact successful output directory.
4. Report the exact summary and artifact root paths.
5. State what Phase VI became truly complete, what remained incomplete, and what was promoted into Phase VII.
6. Confirm the four Phase VII files were created.
7. End with a short plain-English explanation of what the results mean.
```
