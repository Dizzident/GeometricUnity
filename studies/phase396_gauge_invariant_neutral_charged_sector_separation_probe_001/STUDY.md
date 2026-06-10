# Phase396: Gauge-Invariant Neutral/Charged Sector Separation Probe

## Question

Phase395 proved any defensible observed-field extraction must be built from
gauge invariants relative to the background invariant axis n_omega. What is
the simplest such extraction, and does the recomputed bosonic spectrum carry
the residual-U(1) sector structure it predicts?

## Construction

- Extraction: for a carrier vector b, the neutral (invariant-axis) fraction
  is `f(b) = sum_e (b_e . n_omega)^2 / sum_e |b_e|^2`; charged fraction
  `1 - f`. Exactly gauge-invariant under simultaneous rotations (verified at
  7.8e-16).
- Cluster invariant: within a degenerate su(2) triplet the eigenbasis is
  arbitrary, so the invariant is the TRACE of the invariant-axis projector
  over the cluster span: `neutralContent = sum_cluster f(mode)`.
- Hypothesis: the residual-U(1) skeleton predicts each exact triplet splits
  as one neutral direction plus one charged pair (neutralContent ~ 1.0).
- Inputs: persisted omega and the Phase394-recomputed full bosonic spectrum
  (run Phase394 first; this probe reads its working directory).

## Result: the splitting is EXACT across the whole spectrum

- All 68 triplet clusters (both backgrounds) have neutral content 1.0 with
  max deviation 1.9e-7: every triplet is exactly {1 neutral, 2 charged}.
- The 18-dimensional bosonic kernel has neutral content exactly 6.0
  (6 neutral + 12 charged directions).
- Extraction invariance residual 7.8e-16.

This is the discrete residual-U(1) skeleton of {Z-like, W-pair-like} sector
classification, materialized from gauge invariants as Phase395 requires.

## Honest Phase256 audit (fail-closed)

The probe records per-field reasons why ZERO of the 20 Phase256 contract
fields can be filled: the su(2)-only control branch has no hypercharge
U(1)_Y (hence no electroweak embedding, no photon eigenstate, no weak
angle), no scalar sector, a toy 2D vacuum, and no scale/pole/GeV lineage;
and the construction is study-defined, not a theorem with source lineage.
Sector labels are residual-U(1) labels, NOT observed particle names.
`canFillPhase256ObservedFieldExtractionContract=False`.

## Reproduce

```bash
dotnet run --project studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj
dotnet run --project studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/Phase396GaugeInvariantNeutralChargedSectorSeparationProbe.csproj
```
