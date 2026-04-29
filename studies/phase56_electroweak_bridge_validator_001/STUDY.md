# Phase LVI - Electroweak Bridge Validator

Phase LV defined the weak-coupling normalization rules. Phase LVI implements
those rules in code.

The new `ElectroweakBridgeRecord` and `ElectroweakBridgeValidator` create a
hard gate before absolute W/Z mass projection. A bridge must be validated,
finite, target-independent, normalized, and connected to a declared
mass-generation relation. The validator rejects finite-difference coupling
profile magnitudes and W/Z target-fit scales.

This phase still does not produce absolute W/Z masses. It provides the code path
that the next bridge derivation must satisfy before projection can proceed.
