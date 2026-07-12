# Phase466: WS3 VEV Completion Contract (STEP 0 skeleton)

Team C, plan item 5 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`; C-CONTRACT,
time-critical). STEP 0 **skeleton**: emits the pre-registered interim terminal
`awaiting-schema` and the standing claim boundary. The schema and mapper are not
yet implemented. This file pre-registers the contract.

## Interim terminal

`awaiting-schema` — skeleton only; zero physics compute; nothing completed or
promoted; `physicistReviewPending = true`.

## Pre-registered contract (to be implemented)

- **Hash pins.** Hard-pin the phase434 `targetBlindConstructionHash`
  **`f5eafcc74583ecdf…`** and the phase256 templateId.
- **Field partition (6/7/4/9).** Recompute the partition into **6** finite-VEV
  rows, **7** conditional rows, **4** amplitude-blocked rows, and **9**
  lineage-blocked rows; any drift is a blocking terminal.
- **Completion map.** The z-field enters only on the labeled `g′²/g² = 3/5`
  import branch; `branchNormalizationSourceId` appears only as a labeled
  convention adoption.
- **Impossibility cap.** `wsThreeCannotComplete = true` is permanent on **all 9
  lineage fields** (O8 becomes a schema theorem).
- **O5 guard.** ≥ 3σ from exact free, or no completion.
- **Consumption rule.** `{schemaId, schemaHash}` is hash-asserted in the
  integrity script; consumers pin to that pair.

## Five golden fixtures (committed BEFORE the mapper)

1. **T-finite-vev** — a finite-VEV completion.
2. **T-runaway** — a runaway direction.
3. **T-singlet-wins** — the singlet channel dominates.
4. **T-inconclusive** — an inconclusive completion.
5. **adversarial 1.5σ-from-free** — an adversarial near-free case.

Asserts: template mutation count 0; zero GeV tokens; zero unqualified
scalar-label tokens. On any fixture failure the schema is REJECTED and the hold
stays in force — never lifted by weakening an assert.

## Hold interaction

On green, C's half of the WS3 hold lifts (published schema-commit date); the
hold fully lifts only jointly with the O4 M-probe ruling. Phase457's null-hash
firewall pins to this phase's triple in a later no-cascade checkpoint.

## Framing

Zero physics computation at STEP 0. Lattice-unit quantities stay in lattice
units; no GeV/pole/VEV promotion; `promotedPhysicalMassClaimCount = 0`.
