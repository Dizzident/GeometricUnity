# Phase LVIII - Electroweak Absolute Scale Calibration Builder

Phase LVII gates absolute W/Z prediction on a validated electroweak bridge.
Phase LVIII adds the deterministic builder that consumes such a bridge.

`ElectroweakAbsoluteScaleCalibrationBuilder` combines:

- the Phase LIV external electroweak scale in GeV;
- a validated `ElectroweakBridgeRecord`;
- W and Z absolute mass mappings.

It returns a shared GeV-per-internal-mass-unit scale and matching W/Z
`PhysicalCalibrationRecord` entries. Invalid bridges or wrong mappings return a
blocked result with no calibrations.

The project still needs a real bridge derivation before absolute W/Z masses can
be emitted.
