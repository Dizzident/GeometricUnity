# Phase583 - Bi-connection convention reconciliation

Date: 2026-09-08. Amendment A45. Deterministic, zero sampling.

Outcome: `printed-sign-conflict-proved-two-compatible-families-registered-map-unresolved`.
The first frozen v1 Release execution passed without scientific revisions.

## Result

Freeze the original semidirect product, ordinary right connection action and
B=A0 dot epsilon. Enumerate c,s,r in {-1,+1}, where A=A0+c p, the tilted
subgroup shifts p by s*C_h, and the quotient uses derivative sign r.
The exact residual coefficients are c*s-1 for first-connection covariance
and s+r for quotient invariance. The original right-action stabilizer also
requires s=-1. Exactly two triples preserve covariance and quotient:
(-1,-1,+1) and (+1,+1,-1). Only the first also uses the original stabilizer.

The printed triple (+1,-1,+1) preserves that stabilizer and quotient, but not
the claimed common transformation of A and B. A nonconstant commuting gauge
transformation sends initial A=B=0 to A'=-C, B'=C, hence T'=-2C. For frozen
C=(1,2,-1), the squared Lie-vector norm is 24 rather than zero. This is a
contradiction between the compared formulas, not proof the entire theory fails.

Both consistent branches give c*pi_r=epsilon*T*epsilon^-1 with
T=c*p-epsilon^-1*d0(epsilon). Simultaneously negating p,c,s,r leaves A,B,T
and c*pi unchanged. This establishes the relative-field/quotient dictionary
within the menu, not author intent or registered omega identification.

## Exact controls and scope

The known-answer battery validates Lie-generator commutators and the 24 proper
integer rotation matrices, including closure and inverses. All 13,824 sign/
rotation/derivative cases pass in checked Int64 arithmetic. A nonzero A0 is
included. Ordinary derivatives are composed independently by the product rule.
Reference recovery, cocycle, residual-formula and sign-relabeling discrepancies
are all exactly zero. Both compatible dictionary residuals are zero.

The analytic identities hold for arbitrary smooth first jets of matrix groups;
the finite rotations are executable controls, not the universality proof.
This audit is exhaustive only within the frozen three-sign menu, and does not
audit every other equation in the draft. The coefficient sign flip has unit
absolute finite-dimensional Jacobian; no full measure equivalence is claimed.
The primary text remains unchanged and exact-bound. See the
[proof](../../../studies/phase583_biconnection_convention_reconciliation_001/STUDY.md)
and [reference note](../../Reference/ExperimentReferences/BICONNECTION-CONVENTIONS-20260908.md).

## Reproduction and integration

    dotnet run -c Release --project studies/phase583_biconnection_convention_reconciliation_001
    ./scripts/run_boson_phases_incremental.sh --incremental

Program SHA256:
`db446bb701403ff193c2039d76c7642f87c675f31f7db04e4bf0c07b122dc456`.
Contract SHA256:
`73e7b9b1e7fe75508b56991d9582c2ce3e2b68c716cfdf721ebd2fd510681b6a`.
Full and summary output SHA256:
`01d45ea499dfe46d9125bf83c6ad8e52bd8bc4db32995e32ef1453a524abac70`.
Five exact bindings include program, project, source and two upstream outputs.

Generator, traversal, all nine applicable scanners (both Phase207 helpers),
Phase101 mirror, Phase202 checklist, verifier, registry, amendment, restart,
journal and reference ledger are wired. The verifier requires 363 checklist
passes and the three standing physical-completion failures. The timestamped
incremental skip report records integrated validation; no promotion-relevant
full pass or commit is implied by this exploratory result.

## Next task

Derive the two-connection action in (B,T), beginning with
F(A)=F(B)+d_B T+T wedge T. Compare the source curvature-plus-T expression
with the registered curvature-only trivial-torsion objective. Removing a T
term is not imposing T=0. Determine which field the registered omega would
have to represent, which reference/epsilon terms and variations are missing,
and how the measure transforms. Do not replace an action, import Haar, or
relabel previous correlators without that bridge. O4 is pending, Phase561
remains closed, all fourteen authority flags are false, and
promotedPhysicalMassClaimCount=0. Registry 584+ is unassigned.
