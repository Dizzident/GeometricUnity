# Phase 377 Selected Source-Mode Shell-Response Gram Audit

## Purpose

Phase377 composes the Phase376 shell-restricted perturbation blocks into a
target-blind Hilbert-Schmidt pullback metric on the selected source-mode
subspace:

```text
G_a = Psi_shell^dagger deltaK[b_a] Psi_shell
Q_ab = Re Tr(G_a^dagger G_b)
```

The persisted Phase12 campaign supplies `12` orthonormal selected
connection-`1`-form source modes per background. Phase377 audits both persisted
and analytic Phase376 blocks, verifies `Q` parity, positive semidefiniteness,
nonzero response, and invariance under a deterministic unitary change of shell
basis.

## Run

```bash
dotnet run --project studies/phase377_selected_source_mode_shell_response_gram_audit_001
```

## Outputs

- Summary: `output/selected_source_mode_shell_response_gram_audit_summary.json`
- Full audit: `output/selected_source_mode_shell_response_gram_audit.json`
- Per-background evidence: `output/backgrounds/*.json`

## Boundary

`Q` is a study-defined discrete response metric on the selected source-mode
subspace. It is not the Hessian of a GU action, a regularized fermion
determinant, a full connection-carrier response operator, a gauge-reduced
operator, a W/Z mass matrix, or a Higgs scalar-source row.
