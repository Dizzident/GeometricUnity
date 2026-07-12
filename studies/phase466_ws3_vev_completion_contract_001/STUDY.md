# Phase466: WS3 VEV Completion Contract (C-CONTRACT v3)

Team C, plan item 5 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`; C-CONTRACT,
time-critical). Implemented. See
`docs/Phases/Implementation/IMPLEMENTATION_P466.md` for the full record.

## The contract

Commits the schema a WS3 verdict must satisfy to mechanically complete rows of
the phase256 intake template. A schema, not a measurement.

- **Hash pins.** phase434 `targetBlindConstructionHash` `f5eafcc74583ecdf...`
  (full) and phase256 `templateId`
  `observed-field-extraction-wzh-intake-template-v1`; drift => blocking.
- **Field partition (6/7/4/9).** Recomputed from the committed phase434 ledger
  and phase256 template (`7 + 4 + 9 = 20`; `extractionRowCount = 6`); drift =>
  blocking.
- **VEV intake record** `{direction, magnitudeInLatticeUnits, member, channel,
  errorModel}` with the O8 M-probe scope note.
- **Completion map.** A conforming `>= 3 sigma` weak-doublet VEV completes the 7
  conditional rows and the amplitude rows; the z-amplitude completes ONLY on the
  labeled `g'^2/g^2 = 3/5` import branch (`branchNormalizationSourceId` as a
  labeled convention adoption).
- **Impossibility cap.** `wsThreeCannotComplete = true` machine-asserted on all 9
  lineage fields (O8 schema theorem).
- **O5 guard.** `>= 3 sigma` from exact free, or no completion.
- **Consumption rule.** `{schemaId, schemaHash}` recorded; consumers pin to it.

## Five golden fixtures (committed BEFORE the mapper)

`fixtures/`: `T-finite-vev` (completes), `T-runaway`, `T-singlet-wins` (O8
singlet flag), `T-inconclusive`, `adversarial-1.5sigma-from-free` (below the O5
guard). Only the first completes. Any mismatch => `schema-rejected-fixture-failure`
and the hold stays. Self-scan asserts zero GeV tokens and zero capital
scalar-label tokens; `templateMutationCount = 0`.

## Actual result

Terminal **`schema-committed`**. `schemaId = ws3-vev-completion-contract-schema-v1`;
`schemaHash = 7159ea49a45e3044c4393542b24a5db596f5d1423150020b072849ec8cb322b9`.
On green, C's half of the WS3 hold may lift on the published schema-commit date;
the hold fully lifts only jointly with the O4 M-probe ruling.

## Framing

No VEV asserted; no row filled; lattice-unit quantities stay in lattice units;
`promotedPhysicalMassClaimCount = 0`.
