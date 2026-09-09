# Phase596 implementation: fixed-operator Helmholtz audit

Prospective A51 deterministic exact audit of fixed zero-order curvature-only
Euler covectors. Owned study:
`studies/phase596_fixed_operator_helmholtz_audit_001`.

The frozen design predicts principal rank 134 on 234 equations, reduced
invariance rank 8 on 40 equations, and combined rank 142 on 1026 equations,
leaving two Chern-Simons-type u(1,1) bilinears. It includes a 12-by-78
curvature map, independent matrix/Fourier controls, a variable-coefficient
closedness decoy, and a conditional fourteen-dimensional spatial sign audit.
The full-gauge/Spin theorem is separately proved under explicit hypotheses;
frame covariance and missing source intent are not silently identified with
those hypotheses.

## Reviewed freeze and first approved result

The coordinator and independent reviewer read the complete code, proof,
project, compiled helpers and contract before explicit first-run approval.
Release build passed with zero warnings and zero errors. Exactly eleven
unique bindings and the complete live 726-file core manifest matched.

Frozen contract SHA256:
`9a82952d1967e0eeb400bd46ade16dbf0400b5770eca678eb61a728637310639`.

The first scientific Release run passed, exit 0, with terminal
`fixed-operator-helmholtz-controls-pass-conditional-classification-only`.
Shell timing including dotnet startup was 0.694 seconds elapsed,
0.668 user CPU seconds and 0.060 system CPU seconds. A preliminary attempt
to invoke the unavailable `/usr/bin/time` wrapper exited before launching
the study; it was not a scientific execution. The approved run used the
shell timing keyword. Peak resident memory was not measured.

Observed ranks agree with the independent proofs and finite-field lower
bounds: principal 234-by-144 rank 134/nullity 10; reduced 40-by-10 rank
8/nullity 2; combined 1026-by-144 rank 142/nullity 2; curvature map 12-by-78
rank 9/nullity 69. All rational kernel/RREF consistency and independent
modular checks passed. The unreduced cubic menu contained all 792 rows.

All 16 brackets, 64 Jacobi triples, 16 Gram entries and four real-form
controls passed. Both nonabelian and central positive controls passed.
The repeated-spatial decoy had principal defect 2; the noninvariant
symmetric bilinear passed the principal check but had reduced and cubic
defects 4. The variable-coefficient decoy retained a symmetric actual
Hessian while its desired covector failed reciprocity, as frozen.

All 15288 spatial characters were checked, with zero degree-three
survivors, and all 168 positive subgroup characters passed. This verifies
the bounded sign control; the full-u128 statement remains a separately
written conditional proof, not a numerical full-gauge census.

Full and summary JSON are byte-identical. Their common SHA256 is
`dbb522e4cd869a7de1905b8064ab1d96dfceb5e029d4884059fff0ee6963245b`.
Known-answer and aggregate controls passed; all frozen bytes were checked
unchanged after execution. No failed scientific run or repair occurred.

All fourteen authority firewalls remain false, O4 and external source review
remain pending, and promoted physical mass claims remain zero. The result
does not select author intent, an action, a measure, physical fields or an
unrestricted source-wide no-go theorem.
