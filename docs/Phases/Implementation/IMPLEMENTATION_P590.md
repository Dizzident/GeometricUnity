# Phase590: mixed-signature Clifford and source-tensor controls

Amendment A48 pack, frozen before first scientific Release execution.
See the [exact study and proof](../../../studies/phase590_source_clifford_tensor_controls_001/STUDY.md).

The actual14D GammaMatrixBuilder is compared against independently derived
signed Gaussian-unit permutation matrices and exact Clifford blades. All
16384 grades/basis blades are covered. The Hermitian-form and chirality
controls retain the core's mixed-signature convention as an explicit decoy.

All91 Spin generators test simultaneous dual-form and adjoint invariance of
canonical one-/two-form tensors and their volume-dual companions. Exact
trace-pairing controls distinguish their indefinite norms. Disjoint grades
establish at least two invariant directions at each form degree, without
asserting completeness or selecting coefficients.

First frozen Release execution passed (2026-09-08):
`mixed-signature-clifford-tensor-controls-pass-source-choice-open`.

All12 input bindings and the726-file core source tree match. All229376 dense
gamma entries equal the independent exact bit construction. All16384 blades
and229376 blade/right-generator products pass with zero tolerance. H is a
Hermitian involution with trace zero, hence signature(64,64); every gamma is
H-anti-Hermitian. The grade rule yields8128 real and8256 imaginary
H-anti-Hermitian basis directions. The raw builder chirality squares to -I,
while its diagnostic i-rephasing equals the volume element and squares to I.
The core convention remains unchanged.

All364 tensor/generator rows and19110 matrix-commutator comparisons pass
exactly. Per tensor, all91 wrong-dual-sign controls and the49 explicitly
mixed-signature wrong-metric controls are rejected. The declared pairings are:

| Form degree | Canonical norm | Volume-dual companion norm | Cross pairing |
|---|---:|---:|---:|
| 1 | -14 | +14 | 0 |
| 2 | +91 | -91 | 0 |

Disjoint grades establish at least two independent invariant directions at
each form degree. Their formal real coefficient planes have norms
`14*(-a^2+b^2)` and `91*(c^2-d^2)`; even a norm condition would not generally
pick a unique direction. This is a lower bound, not an exhaustive taxonomy.

The final own-project Release build passed with zero warnings/errors. The
first scientific process completed in approximately0.53seconds, and no
scientific repair or frozen-input edit was made. The full
[output](../../../studies/phase590_source_clifford_tensor_controls_001/output/source_clifford_tensor_controls.json)
and summary both have SHA256
`21d3e4b1bbcb54a030f4c2de56eef909503405a9f414eac713ed9134a7ae365d`.
The frozen contract SHA256 is
`028664b9aba7241c2c63da46fe13799d444b893bef08634cc4cd15cb48a4a4ac`.

No core convention, source operator or physical interpretation is selected.
All authority flags remain false, external review remains pending and
promotedPhysicalMassClaimCount=0.
