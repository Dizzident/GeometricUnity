# Implementation P309: Source-Mode Vector-Length Measure Normalization Audit

Phase309 tests whether the Phase302 `source-mode-vector-length=156` scale can
be justified as a hidden amplitude-measure normalization rather than as a
coordinate-count diagnostic.

The audit reads Phase120, Phase300, Phase302, Phase308, and the Phase12 source
mode vectors used by the identity-split replay. It checks three facts:

- Phase120 already validates the analytic/finite-difference amplitude measure
  with `commonScaleMean=1`.
- The relevant Phase12 mode vectors are `unit-M-norm` coordinate vectors with
  `vectorLength=156` and L2 norm one.
- The norm conversion associated with a unit vector over 156 coordinates is
  `sqrt(156)`, not `156`.

## Result

Terminal status:

`source-mode-vector-length-measure-normalization-audit-vector-length-scale-not-measure-derived`

The audit preserves the Phase302 numerical lead but rejects the hidden-measure
interpretation:

- `phase120CommonScaleMean=1.0000000000001665`;
- `commonVectorLength=156`;
- `sqrtCommonVectorLength=12.489995996796797`;
- `maxModeL2NormDeviationFromUnity=2.220446049250313E-16`;
- `vectorLengthScaleIsNotL2MeasureConversion=true`;
- `hiddenMeasureConversionPresent=false`;
- `sourceModeVectorLengthScalePromotable=false`;
- `canFillPhase201WzContract=false`.

Phase309 therefore narrows the W/Z blocker: the `156` factor is not justified
by a missing mode-vector measure conversion. A future promotion would need a new
source-side theorem deriving vector-length scaling before target comparison.

## Outputs

- `studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit.json`
- `studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit_summary.json`
