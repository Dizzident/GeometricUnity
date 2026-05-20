# Implementation P310: Completion Variational Branch to W/Z Normalization Audit

Phase310 tests a narrow post-Phase309 loophole: whether the latest local
completion revision's branch-local bosonic variational and linearization
workbench can justify the specific Phase302 W/Z normalization lead.

The audit reads
`TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md`
and records the lines where the revision supplies a residual, action,
linearization, adjoint, and numerical-lowering framework. It then cross-checks
that framework against the specific non-promotable lead from Phase302,
Phase308, and Phase309:

- common scale `source-mode-vector-length=156`;
- W-only particle multiplier `adjoint-casimir-over-fundamental-casimir=8/3`;
- charged-ladder transfer;
- physical W/Z source-row derivation and branch-stability sidecars.

Result: the completion draft provides a useful variational workbench, but it
does not provide the missing W/Z normalization theorem. Phase310 therefore
keeps `canFillPhase201WzContract=false` and does not promote W/Z masses.

Artifacts:

- `studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit.json`
- `studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit_summary.json`
