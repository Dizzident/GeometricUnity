# Phase461 - Dimensional-Transmutation Reading Menu (Team A Wave-1, A2)

Team A rank-3 of the 2026-07-10 three-team elimination program
(`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md`, Wave-1 item 3;
unresolved objection O6 is the reconstruction gate). ELIMINATION
computation: exact rational arithmetic on every band decision, sub-second.

## Question

The reference snapshot's coefficient-free "cosmological constant as a VEV"
slot statement (routed here by phase460's pinned-relation criterion) admits
only a finite menu of power readings

    M(a,b) = c * (gravityAnchor^a * ccScale^b)^(1/(a+b)).

Does ANY reading, against a pre-registered consistency-window list, yield an
import-clean, primary-band, trials-surviving anchor candidate?

## Pre-registrations (all declared before any value is computed)

- Readings: (a,b) in {(1,0),(0,1),(1,1),(2,1),(1,2),(3,1),(1,3),(4,1),(1,4)}
  x gravity-anchor conventions {full, reduced}; the (0,b) reading is
  anchor-variant independent and registered once. 17 readings.
- Windows (COMPLETE, per-window imports-observed flags + roles ex ante):
  7 live test-only observed-comparison windows (massW, massZ, massH,
  massTop, qcdScaleNf5, massElectron, neutrinoScale) + the BANNED 246-GeV
  electroweak VEV window (circular AND gauge-variant per the recorded
  task-force ruling), role=referee-reconstruction-only, excluded from all
  live verdicts. 119 live rows.
- Look-elsewhere control: primary coefficient band c in [1/(4*pi), 4*pi]
  (fixed rational endpoints); wide band [1/(16*pi^2), 16*pi^2] demoted to a
  labeled secondary sweep; p1 = 2*ln(missFactor)/ln(anchorRange) under the
  log-uniform null, Bonferroni-corrected over 119 rows; survival threshold
  0.05.
- RECONSTRUCTION GATE (O6, fail-closed): unless >= 1 menu row against the
  referee-reconstruction window reproduces the task-force referee's ~460x
  kill within the pre-registered band [300, 700] (conventions: linear and
  cc-plane-squared), the phase fail-closes as menu-incomplete with NO
  verdict.
- Verdict taxonomy: candidate-anchor-forwarded-to-A3 /
  declared-comparison-consistency-only / part12d-anchor-readings-all-miss /
  menu-incomplete.

Band membership and kill-factor decisions are EXACT (BigInteger rational
cross-multiplication of k-th powers); doubles appear only in reporting and
in the pre-registered p-value convention.

## Committed result (smoke-verified)

- Reconstruction gate GREEN with exactly one candidate: M(1,1)-full against
  the banned VEV window under the cc-plane-squared convention, kill factor
  451.105 in [300, 700] - the referee's "~460x, referee-confirmed" kill
  reconstructed and committed as the referee reading.
- Live result: 4 primary-band hits - M(1,2)-full/reduced vs qcdScaleNf5
  (miss 5.33 / 9.12) and M(1,3)-full/reduced vs massElectron (miss 4.72 /
  7.07) - ALL import-laden, NONE trials-surviving (minimum corrected
  p = 1.0); 11 further rows land only in the labeled secondary band.
- Verdict: `declared-comparison-consistency-only` - the part-12d anchor
  readings supply NO import-clean, trials-surviving dimensionful anchor;
  the cc/dark-energy anchor stays killed for anchor purposes; A6 consumes
  this as an A2 rule-out of anchor-grade content.

Terminal:
`dimensional-transmutation-reading-menu-declared-comparison-consistency-only`.

## Run

```
dotnet run -c Release --project \
  studies/phase461_dimensional_transmutation_reading_menu_001/Phase461DimensionalTransmutationReadingMenu.csproj
```

Outputs: `output/dimensional_transmutation_reading_menu.json` (+ summary).

## Boundaries

Declared-comparison phase (`targetBlindConstruction = false`, phase429/451
style separation): every GeV number is a labeled test-only import, never a
prediction. `physicistReviewPending = true`. No contract filled; nothing
promoted; `promotedPhysicalMassClaimCount = 0` unchanged.
