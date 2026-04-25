# IMPLEMENTATION_P17.md

## Purpose

Phase XVII continues the physical-prediction unblock started in Phase XVI.
The goal is not to force a particle-physics claim through the system. The goal
is to make the repository capable of producing a physical prediction only when
the scientific bridge is explicit, tested, and traceable.

This phase covers the work needed between benchmark validation and the first
honest physical-boson comparison campaign:

- unit and scale calibration contracts;
- authoritative physical target evidence;
- physical prediction projection records;
- fail-closed activation rules for any future physical comparison campaign.

## Baseline

At the start of this phase, the active Phase V campaign can compare benchmark
observables, but its current observables remain benchmark quantities:

- `bosonic-eigenvalue-ratio-1`;
- `bosonic-eigenvalue-ratio-2`;
- `bosonic-eigenvalue-ratio-3`;
- `fermionic-eigenvalue-ratio-1`;
- `fermionic-eigenvalue-ratio-2`.

The physical claim gate is intentionally blocked because there is no validated
physical mapping, no validated physical calibration, no active physical target
evidence table, and active fatal/high falsifiers remain.

## Completed Work

### P17-M1 Physical Observable Mapping Contract

Status: complete.

Implemented:

- `PhysicalObservableMapping` and `PhysicalObservableMappingTable`;
- `physical_observable_mapping.schema.json`;
- campaign validation for physical targets that lack mappings;
- blocked placeholder photon masslessness mapping;
- `targetPhysicalObservableId` for validated mappings and prediction records.

Result:

- benchmark targets can still run without physical mappings;
- physical targets fail validation unless a matching validated mapping exists;
- blocked and provisional mappings remain explicit claim blockers.

### P17-M2 Observable Classification Contract

Status: complete.

Implemented:

- `ObservableClassification` and `ObservableClassificationTable`;
- `observable_classification.schema.json`;
- checked-in classifications for active campaign observables;
- report output that shows benchmark-only versus physical-observable status.

Result:

- current eigenvalue-ratio observables are explicitly benchmark quantities;
- no current active observable is silently treated as a W, Z, Higgs, or photon
  property.

### P17-M3 Physical Claim Gate

Status: complete.

Implemented:

- `PhysicalClaimGate`;
- report and CLI rendering;
- tests for blocked and allowed gate states.

Result:

- reports may state benchmark comparison status;
- reports cannot state that a physical boson prediction passed unless mapping,
  classification, calibration, physical target evidence, and falsifier checks
  all pass.

### P17-M4 Physical Calibration Contract

Status: complete.

Implemented:

- `PhysicalCalibrationRecord` and `PhysicalCalibrationTable`;
- `physical_calibration.schema.json`;
- campaign spec support for `physicalCalibrationPath`;
- validation that physical targets with validated mappings still require a
  validated calibration;
- blocked calibration placeholder for photon masslessness normalization.

Result:

- dimensionful and dimensionless physical comparisons now require explicit
  scale or normalization records;
- blocked calibrations surface closure requirements instead of allowing a
  comparison.

### P17-M5 Experimental Target Evidence

Status: complete.

Implemented:

- physical metadata fields on `ExternalTarget`;
- schema support for citation, source URL, retrieval date, unit, particle id,
  observable type, unit family, and confidence level;
- inactive PDG-backed physical target table under
  `studies/phase18_experimental_targets_001/physical_targets.json`;
- tests requiring citation metadata for physical targets.

Targets added:

- W boson mass: `80.3692 +/- 0.0133 GeV`;
- Z boson mass: `91.1880 +/- 0.0020 GeV`;
- Higgs mass: `125.20 +/- 0.11 GeV`;
- W/Z mass ratio: `0.88136 +/- 0.00015`.

Result:

- authoritative target values are available but inactive;
- the active benchmark campaign cannot accidentally claim physical agreement.

### P17-M6 Physical Prediction Projection Records

Status: complete.

Implemented:

- `PhysicalPredictionProjector`;
- `PhysicalPredictionRecord`;
- report and CLI sections for physical prediction records;
- tests proving validated inputs emit a prediction and benchmark-only
  classifications block prediction.

Result:

- future campaigns can emit a physical prediction record with propagated
  uncertainty;
- the active campaign emits only a blocked diagnostic record for the photon
  placeholder.

## Validation

Commands run successfully:

- `dotnet build GeometricUnity.slnx`;
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`;
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase18_physical_prediction_projector_check --validate-first`;
- `dotnet test GeometricUnity.slnx --no-build`.

The generated reference report still blocks physical predictions, as intended:

- no validated physical mapping is active;
- no computed observable is classified as a physical observable;
- no validated calibration is active;
- no physical target evidence table is active in the reference campaign;
- fatal/high falsifiers remain active.

## Remaining Work

### P17-M7 First Dimensionless Physical Observable

Status: scaffold complete; validated extraction pending.

Implement a real computed physical observable, preferably a dimensionless one
such as `physical-w-z-mass-ratio`, before attempting GeV scale setting.

Progress:

- added inactive `studies/phase19_dimensionless_wz_candidate_001`;
- added a W/Z vector-mode ratio candidate observable artifact;
- added provisional mapping to `physical-w-z-mass-ratio`;
- added provisional dimensionless identity normalization;
- added isolated PDG W/Z mass-ratio target evidence;
- added tests proving the scaffold remains blocked and inactive.
- added reusable positive-mode ratio extraction support with uncertainty
  propagation in `SpectrumObservableExtractor`.
- added a typed overload that creates the ratio from two quantitative mode
  records only when environment, branch, refinement level, and total
  uncertainty are consistent.
- added `IdentifiedPhysicalModeRecord` and a fail-closed ratio path requiring
  validated physical mode inputs with matching units.
- added provisional Phase XIX W and Z mode records documenting the required
  future input shape.

Definition of done:

- a computed observable exists with physical provenance and uncertainty;
- the extraction method is distinct from benchmark eigenvalue-ratio controls;
- the observable is not activated unless the mapping and calibration are
  validated.

Remaining:

- replace the placeholder candidate value with an independently computed W/Z
  mode-ratio observable;
- derive and test the W/Z vector-mode identification rule;
- estimate branch, refinement, and environment uncertainty components instead
  of using `-1` unestimated sentinels.

### P17-M8 First Physical Comparison Campaign

Status: pending.

Create a separate campaign that references a physical target table only after
the computed observable exists.

Definition of done:

- campaign validation passes with physical target evidence active;
- the campaign either emits a predicted record, a failed comparison, or a clear
  not-computable/blocker state;
- the reference benchmark campaign remains separate.

### P17-M9 Falsifier Resolution For Physical Claims

Status: pending.

Physical claims remain blocked while fatal/high falsifiers are active.

Definition of done:

- each fatal/high falsifier is repaired or carried as an explicit blocker;
- no physical prediction is marked passed in the presence of unresolved severe
  falsifiers.

## Plain-English Success Criteria

This phase succeeds when the codebase can produce physical prediction artifacts
without confusing them with benchmark results. A successful physical prediction
still requires new physics-facing observable extraction, not just report wiring.
