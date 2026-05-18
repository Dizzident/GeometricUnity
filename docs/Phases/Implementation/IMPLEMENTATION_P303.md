# Implementation P303: Identity-Split Branch Source Normalization Audit

## Purpose

Phase303 audits the remaining branch-stability gap exposed by Phase302. Phase302 found a target-independent raw/common numerical lead for the identity-split W/Z replay rows, but the lead still failed stability. Phase303 checks the per-row sidecars and simple branch-local source-mode descriptors to see whether the near-pass can be repaired without a new theorem.

## Inputs

- Phase24 initial W/Z identity-rule readiness, retained only as historical blocker context.
- Phase27 W/Z identity-rule readiness after charge sectors.
- Phase27 electroweak mixing-convention readiness.
- Phase251 upstream W/Z identity-rule source-chain audit.
- Phase213 source-lineage blocker matrix.
- Phase299 identity-split production W/Z replay.
- Phase302 identity-split particle normalization audit.

## Behavior

The audit applies the Phase302 best source-invariant raw/common lead to all four Phase299 W/Z rows, then evaluates:

- row-level raw gate status;
- particle-level branch relative spread;
- common W/Z mean-scale consistency;
- simple source-mode descriptor normalizers based on L1, L-infinity, SU(2)-triple norms, dominant-axis norms, dominant-axis energy, solver residual norm, and Phase27 charged-plane/neutral-axis sector projections.

Descriptor normalizers are balanced within each particle and use no target observables for construction.

## Result

Phase303 is expected to remain non-promotional. It certifies that the Phase302 near-pass still fails row-level raw/stability sidecars and that simple target-independent source-mode descriptor normalizers do not produce a stable all-row W/Z source normalizer.

The identity/mixing sidecar distinction is explicit: Phase27 identity and mixing labels are current and ready, but Phase251 classifies them as internal identity/ratio evidence rather than an absolute W/Z source law. Phase303 therefore blocks promotion on the missing source-law transfer theorem and row-level stability, not on stale Phase24/Phase26 readiness artifacts.

Phase27 projection descriptors are included to test the strongest convention-aware local repair: W rows may use the charged SU(2) plane and Z rows may use the neutral axis. Those descriptors still do not produce a stable all-row source normalizer.

## Output

- `studies/phase303_identity_split_branch_source_normalization_audit_001/output/identity_split_branch_source_normalization_audit.json`
- `studies/phase303_identity_split_branch_source_normalization_audit_001/output/identity_split_branch_source_normalization_audit_summary.json`
