# Phase V Validation Report: phase5-su2-branch-refinement-env-validation

**Study ID:** phase5-su2-branch-refinement-env-validation
**Report date:** 2026-03-15

## Branch Independence
- Invariant quantities: 3
- Fragile quantities: 2
- Total analyzed: 5

## Convergence
- Convergent quantities: 5
- Non-convergent: 0
- Evidence source: bridge-derived from 4 admitted background record(s).
- Refinement seed family: multi-variant admitted atlas.
- Direct solver-backed refinement family: no.

## Quantitative
- Passed matches: 7
- Failed matches: 1
- Benchmark class `control`: 6 match(es)
- Benchmark class `internal-benchmark`: 2 match(es)
- Failed `internal-benchmark` matches: 1

## Falsification
- Total falsifiers: 4
- Active fatal: 1
- Active high: 3
- Demotions: 4

## Dossiers
- phase5-su2-branch-refinement-env-validation-provenance-dossier

---

**IMPORTANT:** This study mixes control-study targets and stronger benchmark targets.
None of them is a real-world experimental measurement or a physical prediction.

**Reproduction command:**
```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec <campaign.json> --out-dir <dir>
```
