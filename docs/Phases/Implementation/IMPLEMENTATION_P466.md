# Implementation P466 - WS3 VEV Completion Contract (Team C, Wave-2) (Team C)

Phase466 is the **C-CONTRACT v3** implementation of
`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, item 5 (time-critical). Registry
number 466 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does

It commits the **schema** a WS3 verdict must satisfy to mechanically complete
rows of the phase256 observed-field-extraction intake template. It is a schema,
not a measurement: no VEV is asserted to exist and no row is filled.

- **Hard pins (drift => blocking).** phase434
  `targetBlindConstructionHash`
  `f5eafcc74583ecdf83872fdf914c1a5483847499f97359d9cbe8ae44c396023b` and phase256
  `templateId` `observed-field-extraction-wzh-intake-template-v1` are read from
  the committed summaries and asserted (`schema-rejected-pin-drift` on drift).
- **Partition recomputed (drift => blocking).** The 6-row / 7-conditional /
  4-amplitude-blocked / 9-lineage-blocked partition is recomputed from the
  committed phase434 `conditionalCompletionLedger` and the phase256
  `requirementRows` and matched field-by-field (`7 + 4 + 9 = 20` template
  fields; `extractionRowCount = 6`). Drift emits
  `schema-rejected-partition-drift`.
- **VEV intake record schema** `{direction, magnitudeInLatticeUnits, member,
  channel, errorModel}` with the O8 M-probe scope note (the M field is a labeled
  probe convention, never sourced; a gap proves a singlet, never the named
  electroweak scalar).
- **Completion map.** A conforming, `>= 3 sigma` (O5), weak-doublet VEV completes
  the 7 conditional rows and the amplitude rows; the **z-amplitude row completes
  ONLY on the labeled `g'^2/g^2 = 3/5` import branch**
  (`branchNormalizationSourceId` as a labeled convention adoption). The 9 lineage
  rows are **permanently blocked** - `wsThreeCannotComplete = true` is
  machine-asserted on all 9 (O8 as a schema theorem).
- **O5 guard.** Completion requires `>= 3 sigma` departure from the exact free
  control, else no completion.

## Five golden fixtures (committed BEFORE the mapper)

`studies/phase466_ws3_vev_completion_contract_001/fixtures/` holds the five
pre-registered fixtures, each with its expected outcome. The mapper independently
applies the completion function and asserts a match:

1. `T-finite-vev` -> completes (7 conditional + 4 amplitude incl. z; 0 lineage).
2. `T-runaway` -> no completion (no finite magnitude).
3. `T-singlet-wins` -> no completion; singlet flag (O8: a singlet gap is not the
   weak-doublet completion).
4. `T-inconclusive` -> no completion (direction/member unresolved).
5. `adversarial-1.5sigma-from-free` -> no completion (below the O5 guard).

Only the first completes. Any fixture mismatch emits
`schema-rejected-fixture-failure` and the WS3 hold stays in force.

## Actual result

- **Terminal: `schema-committed`.** Pins hold; partition matches; all five
  fixtures reproduce their pre-registered outcomes; the O8 cap is asserted on all
  9 lineage fields; the GeV / capital-scalar-label token self-scan is clean
  (`0 / 0`); `templateMutationCount = 0`.
- `schemaId = ws3-vev-completion-contract-schema-v1`;
  `schemaHash = 7159ea49a45e3044c4393542b24a5db596f5d1423150020b072849ec8cb322b9`.
- **Consumption rule** recorded in the output: consumers (e.g. the phase457
  null-hash firewall) pin to `{schemaId, schemaHash}`; a WS3 verdict is admissible
  only if it supplies a conforming intake record and clears the O5 guard.

On `schema-committed`, C's half of the WS3 hold may lift on the published
schema-commit date; the hold fully lifts **only jointly** with the O4 M-probe
ruling.

## Framing

No VEV is asserted; no row is filled; the 9 lineage rows are permanently
un-completable by WS3. Lattice-unit quantities stay in lattice units;
`promotedPhysicalMassClaimCount` remains 0; `physicistReviewPending` is carried
explicitly.
