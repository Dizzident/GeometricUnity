# PHASE_6_EVALUATION_PROMPT.md

Use this prompt after the Phase V gap-closure batch has completed.

Replace the bracketed placeholders before sending it to Claude Code.

```text
Review the completed Phase V gap-closure results and produce the Phase VI planning documents.

Inputs to review:
- Batch summary: [BATCH_SUMMARY_PATH]
- Campaign artifacts root: [ARTIFACTS_ROOT]
- Logs root: [LOGS_ROOT]
- Phase V gap handoff: [GAPS_P5_01_PATH]
- Phase V open issues: [PHASE_5_OPEN_ISSUES_PATH]
- Phase V implementation plan: /home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P5.md
- Phase V architecture: /home/josh/Documents/GitHub/GeometricUnity/ARCH_P5.md
- Assumptions ledger: /home/josh/Documents/GitHub/GeometricUnity/ASSUMPTIONS.md

Tasks:
1. Verify whether the work described in GAPS_P5_01.md is actually complete from code, tests, logs, and artifacts.
2. Identify any remaining Phase V gaps, regressions, placeholder evidence, or incomplete contracts.
3. Distinguish:
   - completed and verified closures,
   - partial closures,
   - failed or missing work,
   - work that should be deferred to Phase VI.
4. Evaluate the scientific quality of the resulting Phase V evidence:
   - branch robustness,
   - convergence quality,
   - environment realism,
   - observation-chain validity,
   - falsifier coverage,
   - quantitative target quality,
   - dossier/report completeness,
   - reproducibility quality.
5. Based on the actual outputs, create the next-phase planning docs for Phase VI.

Deliverables to create:
- /home/josh/Documents/GitHub/GeometricUnity/ARCH_P6.md
- /home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P6.md
- /home/josh/Documents/GitHub/GeometricUnity/PHASE_6_OPEN_ISSUES.md

Requirements for the review:
- Be strict. Do not mark a gap closed unless code, tests, and artifacts all support it.
- Use the generated campaign artifacts as primary evidence, not just the planning docs.
- Call out any mismatches between the intended contracts in GAPS_P5_01.md and what was actually implemented.
- If a required artifact is missing or malformed, treat that as an open gap.
- If a test suite relevant to a gap did not run or failed, treat that gap as not fully closed.

Requirements for ARCH_P6.md:
- Define the purpose of Phase VI based on the real post-Phase-V state, not an idealized one.
- State the minimum scientific and engineering prerequisites inherited from the finished Phase V work.
- Define the major workstreams and dependency graph for Phase VI.
- Focus on the next evidence-bearing step, not generic future ambitions.

Requirements for IMPLEMENTATION_P6.md:
- Make it a Claude-ready implementation handoff.
- Include exact milestones, artifact contracts, CLI additions, schemas, and tests.
- Resolve major design choices instead of leaving them open where possible.
- Build on actual outputs and gaps found in the completed Phase V review.

Requirements for PHASE_6_OPEN_ISSUES.md:
- Include only issues that genuinely remain after the reviewed Phase V results.
- Separate:
  - scientific limitations,
  - infrastructure limitations,
  - evidence/provenance limitations,
  - intentionally deferred work.

Response format:
1. First provide a concise review of the Phase V results with findings ordered by severity.
2. Then summarize what is truly complete and what is still open.
3. Then create or update the three Phase VI files.
4. At the end, provide a short explanation of why Phase VI is the correct next step.
```
