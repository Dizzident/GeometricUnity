# Implementation P221: SU(2) Casimir W/Z Normalization Probe

P221 tests a concrete numerical lead for the W/Z absolute-mass miss.

Current result:

`su2-casimir-wz-normalization-probe-numerically-successful-not-promotable`

The probe replaces the Phase63 single-generator trace-half scale `sqrt(1/2)` with an SU(2) spin-1 isotropic RMS/Casimir scale `sqrt(C2(adj)/dim su2) = sqrt(2/3)`. Equivalently, it multiplies the Phase65 weak coupling by `2/sqrt(3)`.

This gives:

- weak coupling `0.6531972647421809`;
- W mass `80.41500121225275 GeV`;
- Z mass `91.41277561445557 GeV`;
- old Phase74 target-comparison residuals below `1 sigma` for both W and Z.

The phase is intentionally not promotional. The current Phase64 matrix element is a single bosonic perturbation matrix element, while the probe assumes an isotropic SU(2) triplet RMS amplitude. No upstream GU artifact currently derives that replacement, Phase76 still requires a replayed raw matrix-element or scalar-sector revision rather than a post-hoc normalization swap, and Phase77 blocks the Phase65 raw `0.8` amplitude as `scalar-study-input` with unknown normalization rather than replayed analytic matrix-element evidence.

Use this as a strong W/Z research lead, not as a published prediction. Promotion requires a target-independent derivation that Phase64/65 should use the spin-1 triplet RMS normalization, followed by Phase201/P209/P210/P213 source-lineage gates.
