# Phase459: Phase452 Record-Reconciliation Attestation

Team A rank-1 **A0 (GATING)** — Wave-0 item 0.2 of the committed three-team
elimination program (`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md`),
made machine-checked and standing. Phase number 459 = Team A's first registry
number (`docs/Phases/PHASE_NUMBER_REGISTRY.md`). Audit-class: **zero physics
compute, seconds**, runs in the generator forever.

## The question

The phase452 record reconciliation CONTENT landed in the Wave-0 ops
checkpoint (e797defe): the committed default-budget 16000/10000-trajectory
output is the SOLE canonical spectroscopy record, and the earlier
journal/restart-prompt gap values (2.4352 / 2.4547) came from a
never-committed reduced-budget env-override run and are superseded. Until
this phase, that reconciliation was prose. Phase459 turns it into a
fail-closed attestation that every generator pass re-executes, so the
canonical record can never drift, be silently replaced, or be re-polluted by
an env-override run without the whole pass going red. Every spectroscopy
citation on every team gates on this (cross-team dependency spine:
0.1 → 0.2 (A0) → everything).

## What is attested (pre-registered, pinned as source constants)

1. **Canonical configuration.** The phase452 summary records
   `trajProduction = 16000`, `trajControl = 10000`, `rngSeed = 20260705`
   (and every production/control column carries the matching trajectory
   counts), AND the committed phase452 `Program.cs` still carries exactly
   those default literals (parsed from source, phase202-style const+parse) —
   so neither the output nor the source can move independently.
2. **Canonical gap record** (tight tolerance `1e-9` on the stored doubles):
   - identity `a*m` = `2.7132465417703235 ± 0.18463902186135914` (O1; O2
     pinned separately), with `plateauChi2Dof = null` and window `{0}` —
     attested AS inconclusive-by-construction as a measurement (T = 3 has
     exactly one informative cosh point);
   - sd2 `mO1 = 2.555338250004908 ± 0.07246835972150631`,
     `mO2 = 2.4986465424511284 ± 0.07007825117627481`; the combined gap is
     RE-DERIVED here via the phase452 pre-registered rho=1 combination rule
     and must equal `2.526042101792096 ± 0.07123324126755279`
     (= "2.5260 ± 0.0712", quoted in the stored verdict reason);
   - the interpolator cross-check `0.5623628195265302` sigma (stored value
     pinned AND recomputed from the stored gaps, compatible, below the
     3-sigma gate);
   - the exact analytic free gaps `2.550880306794233` (identity) /
     `2.532028376864343` (sd2 O1) / `2.3569501678347393` (sd2 O2).
3. **Cross-action ratio** sd2/identity, COMPUTED from the two stored
   combined gaps: `0.9310035276570129 ± 0.06857994094256506` (= 0.931 ±
   0.069; CROSS-ACTION deliverable class, never folded into the
   spectrum-ratio table), and it must sit within **1.5 sigma** of the exact
   free ratio `0.9926096375907257` (measured: ~0.90 sigma) — the binding
   FREE-FIELD-COMPATIBLE label condition, sigma recomputed from the stored
   errors.
4. **Tamper protection.** The superseded literals `2.4352` and `2.4547`
   appear NOWHERE in either phase452 output JSON (they never did). The scan
   runs on volatile-field-scrubbed text (`generatedAt`, `runtimeSeconds`,
   `msPerTrajectory` removed) so a regenerated timing digit string can never
   flip this check either way.
5. **Standing claim boundary.** Target-blind flags, all contract-fill and
   promotion booleans at their fail-closed values,
   `physicistReviewPending = true` (Wave-0 item 0.3 OPEN, root and
   recordedBoundary), and the lattice-units-only language including the
   binding `NEVER m_H` label caveat in `pureGaugeRatioNote`.

31 checks total; every check records `requirement` and `observed`.

## Verdict taxonomy (pre-registered, two terminals, fail-closed)

- **record-reconciled-canonical** — ALL checks green: the committed phase452
  output IS the reconciled canonical record; program-wide spectroscopy
  citations may rely on it.
- **config-mismatch-quarantine** — ANY check fails; every failing check is
  named in `failingChecks`. Per the committed rule-out branch, phase452 is
  demoted to unverified-output, ALL spectroscopy rows are quarantined
  program-wide, and a mandatory `--full` re-run precedes any pole citation.

The phase always writes its verdict and **exits 0** (the phase202/448
pattern); `scripts/verify_boson_claim_integrity.sh` asserts
`verdictKind === "record-reconciled-canonical"` and fail-closes the whole
pass otherwise.

## Volatility rule

The generator REGENERATES the phase452 outputs each pass; the fixed
`rngSeed` makes the measurement values deterministic. Asserted content is
therefore restricted to configuration, seeded measurement values, structure,
boundary flags, and prose. Timestamps and timing fields are never read.

## Env knobs

NONE. This phase reads no environment variables (env-clean by construction —
the phase452 reconciliation lesson applied to the attestor itself), and the
output records `envKnobsRead = false`.

## Run

```
dotnet run --no-build -c Release --project studies/phase459_spectroscopy_record_attestation_001/Phase459SpectroscopyRecordAttestation.csproj
```

Runtime ~0.03 s. Committed smoke result (2026-07-10):
`spectroscopy-record-attestation-passed-record-reconciled-canonical`, 31/31
checks green. Fail-closed branches exercised in development: a tampered
`trajProduction`, an injected superseded literal, and a tampered source
default literal each flipped the verdict to `config-mismatch-quarantine`
with the failing check named (and exit code still 0).

## Outputs

`output/spectroscopy_record_attestation.json` and `…_summary.json`
(identical).

## Mandatory framing

This phase performs zero physics computation and promotes nothing. The
attested numbers are workbench-relative structure data of the reduced spin-4
slice (su(2) toy algebra, lattice-canonical n=3 torus, lattice units,
beta = 1 convention recorded) — NEVER physical masses; the FREE-FIELD-
COMPATIBLE label remains binding on every attested row; NO GeV/pole/VEV
promotion; no Phase201/Phase256 contract field is filled;
`physicistReviewPending = true` is carried explicitly.
