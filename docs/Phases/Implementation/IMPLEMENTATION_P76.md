# Phase LXXVI - Weak-Coupling Amplitude Normalization Audit

## Goal

Audit the Phase LXV weak-coupling amplitude path after the Phase LXXV coherent W/Z absolute mass miss diagnostic.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WeakCouplingAmplitudeNormalizationAudit.cs`
- `tests/Gu.Phase5.Reporting.Tests/WeakCouplingAmplitudeNormalizationAuditTests.cs`
- `studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json`
- `studies/phase76_weak_coupling_amplitude_normalization_audit_001/STUDY.md`

The audit consumes the Phase LXV weak-coupling extraction and the Phase LXXV W/Z mass miss diagnostic. It separates the current coupling into:

- raw matrix-element magnitude: `0.8`;
- canonical SU(2) trace-half generator scale: `0.7071067811865476`;
- current weak coupling: `0.5656854249492381`.

## Finding

The Phase LXXV target-implied weak coupling is `0.6522081710229882`. Holding the canonical generator normalization fixed, that would require raw matrix-element magnitude `0.9223616409512609`, or a raw matrix-element scale increase of `1.152952051189076`.

If instead the raw matrix element were held fixed, the generator scale would need to become `0.8152602137787353`, which is incompatible with the canonical SU(2) trace-half normalization currently encoded by Phase LXIII.

This means the coherent W/Z absolute mass miss is not explained by the SU(2) generator normalization. The next blocker is the Phase LXV scalar raw matrix-element input or the scalar-sector relation that converts it into the weak-coupling amplitude.

## Next Step

Phase LXXVII should replace the scalar `rawMatrixElementMagnitude: 0.8` study input with a replayed analytic matrix-element evaluator, or produce an explicit scalar-sector correction relation with provenance that is independent of W/Z physical mass targets.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 269 passed
- `git diff --check`
