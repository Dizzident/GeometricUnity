# EXP_PHASE_V.md

```text
Execute the full Phase V gap-closure batch, self-correct until it succeeds, and then immediately evaluate the completed results to generate the Phase VI planning documents.

Primary objectives:
1. Run `/home/josh/Documents/GitHub/GeometricUnity/scripts/run_phase5_gap_closure_batch.sh`
2. If it fails, diagnose the failure, implement the missing fixes, and rerun until it completes successfully
3. After the batch succeeds, review the generated code, tests, logs, and artifacts
4. Then create:
   - `/home/josh/Documents/GitHub/GeometricUnity/ARCH_P6.md`
   - `/home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P6.md`
   - `/home/josh/Documents/GitHub/GeometricUnity/PHASE_6_OPEN_ISSUES.md`

Binding source of truth for implementation:
- `/home/josh/Documents/GitHub/GeometricUnity/GAPS_P5_01.md`

Supporting docs:
- `/home/josh/Documents/GitHub/GeometricUnity/PHASE_5_OPEN_ISSUES.md`
- `/home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P5.md`
- `/home/josh/Documents/GitHub/GeometricUnity/ARCH_P5.md`
- `/home/josh/Documents/GitHub/GeometricUnity/ASSUMPTIONS.md`
- `/home/josh/Documents/GitHub/GeometricUnity/PHASE_6_EVALUATION_PROMPT.md`

Execution rules:
1. Treat `GAPS_P5_01.md` as the implementation spec. Do not invent alternate contracts unless the repo makes them impossible.
2. Actually run the batch script. Do not stop at analysis.
3. If the script fails:
   - identify the exact failing command,
   - inspect the logs it generated,
   - patch the root cause in code, config, artifacts, or script,
   - rerun the relevant tests,
   - rerun the batch script,
   - repeat until success.
4. Do not stop after a partial fix. Continue until the batch exits successfully or you hit a real blocker that cannot be resolved from repository context.
5. If the script expects missing inputs such as `campaign.json`, sidecar JSON files, CLI commands, artifact loaders, or schemas, implement them according to `GAPS_P5_01.md`.
6. If a documented contract in `GAPS_P5_01.md` is impossible in the current codebase, make the minimum necessary correction and explain it clearly in the final summary.
7. Do not weaken the batch script by removing checks just to make it pass unless the corresponding contract has been intentionally changed and justified.
8. Keep all changes aligned with the repo’s existing C# and JSON serialization patterns.
9. Add or update tests whenever a gap is closed.
10. Use the generated artifacts as primary evidence for the Phase VI planning pass.

Mandatory workflow:
1. Read `/home/josh/Documents/GitHub/GeometricUnity/GAPS_P5_01.md`
2. Run:
   `./scripts/run_phase5_gap_closure_batch.sh`
3. On failure:
   - inspect the failing logs,
   - inspect the code path,
   - patch the repo,
   - rerun the relevant tests,
   - rerun the batch
4. Repeat until the batch succeeds
5. After success, read:
   - the generated batch summary,
   - the campaign artifacts,
   - the campaign logs
6. Then use the evaluation criteria from `/home/josh/Documents/GitHub/GeometricUnity/PHASE_6_EVALUATION_PROMPT.md`
7. Generate:
   - `/home/josh/Documents/GitHub/GeometricUnity/ARCH_P6.md`
   - `/home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P6.md`
   - `/home/josh/Documents/GitHub/GeometricUnity/PHASE_6_OPEN_ISSUES.md`

Requirements for the Phase VI evaluation:
1. Be strict. Do not mark a Phase V gap closed unless code, tests, logs, and artifacts support it.
2. Distinguish:
   - completed and verified closures,
   - partial closures,
   - failed or missing work,
   - work that should be deferred to Phase VI.
3. Use the generated campaign artifacts as primary evidence, not just the planning documents.
4. If a required artifact is missing or malformed, treat that as an open gap.
5. If a relevant test suite failed or was not run, treat that gap as not fully closed.
6. Base Phase VI on the actual post-batch state, not the intended one.

Success criteria before writing Phase VI documents:
- `scripts/run_phase5_gap_closure_batch.sh` exits successfully
- all required artifacts are verified by the script
- relevant Phase V and supporting tests pass
- the implementation is materially aligned with `GAPS_P5_01.md`

Final response requirements:
1. Summarize what was changed to get the batch to pass
2. List the tests that were run
3. Report the exact successful batch output directory
4. Report the exact summary file path
5. Report the exact campaign artifacts root
6. State what Phase V work is truly complete and what remains open
7. Confirm that the three Phase VI files were created
8. List any residual risks or follow-up items that did not block completion
```
