# Phase LVII - Absolute W/Z Bridge Gate Integration

Phase LVI added a validator for electroweak bridge records. Phase LVII wires
that validator into `PhysicalPredictionProjector` for absolute W/Z mass
mappings.

Absolute W/Z mass projection now requires a passing `ElectroweakBridgeTable`.
Without it, `physical-w-boson-mass-gev` and `physical-z-boson-mass-gev` remain
blocked even if a mapping and calibration are otherwise validated. Rejected
bridge input kinds such as coupling-profile magnitudes also block projection.

This is an enforcement phase. The next phase must derive a real bridge record
from normalized internal weak-coupling or mass-generation evidence.
