# Phase584: signed spatial curvature consistency

Amendment A46 registers a prospective actual-assembler control of the paired
sign dictionary motivated by Phase583. See the [study and proof](../../../studies/phase584_signed_spatial_curvature_consistency_001/STUDY.md)
and [frozen contract](../../../studies/phase584_signed_spatial_curvature_consistency_001/preregistration/contract_v1.json).

The study evaluates four smooth profiles on seven shrinking triangle sizes
and all six vertex permutations through the unmodified core assembler on
single-triangle two-dimensional meshes. It
compares both omega=A/output=F and omega=-A/output=-F against independently
expanded signed-area curvature polynomials. The identity
Qreg+Qloop=[x,x+y+z] predicts leading paired-sign spatial consistency and
retains a quadratic fixed-mesh amplitude defect. The original Phase565 mesh,
wave and weak-field control are reproduced separately; its negative terminal
is required and remains unchanged.

First frozen Release execution (2026-09-08) passed:
`paired-sign-smooth-spatial-consistency-fixed-mesh-obstruction-preserved`.
All 20 input bindings match. The 19,683 exact identity cases pass, and all
168 spatial rows / 336 assembly calls have zero leading-coefficient,
polynomial, Stokes and assembler-identity residual. The canonical affine
witness error decreases from 0.1767766952966369 to 0.0027621358640099515;
every nonzero spatial sequence reduces by at least a factor 64. The
constant noncommuting paired result is exact, while the same-sign error is 2.

On the original four-dimensional fixed mesh, the same and paired weak-field
defects have slopes 2.000016126988648 and 2.000012839777975. The original
array-order remainder has slope 3.000002799864725. Signed quadratic
coefficient relative errors are 0.00018426766555846463 and
0.00013510272440782806. Thus the favorable smooth spatial limit coexists
with the preserved Phase565 fixed-mesh obstruction.

The own-project Release build passed with zero warnings/errors. First-run
process wall time was approximately 0.27 seconds, within the prospective
budget estimate. No scientific repairs or frozen-input changes were made.
The full [output](../../../studies/phase584_signed_spatial_curvature_consistency_001/output/signed_spatial_curvature_consistency.json)
and summary have SHA256
`80ce1c203d1e6207119da1423e11336ce5df6ea18d8057473db27eca8aff64b7`.
The contract SHA256 is
`eaeb353c92d35ee9a63627048fc70870742ee703dda676d0df78c467b2eab0fa`.

This establishes at most a conditional smooth classical consistency result.
It does not select author intent, derive the registered source action or its
rough-field measure, establish quantum convergence or identify a particle.
All authority flags remain false, Phase561 remains closed, external review
is pending and promotedPhysicalMassClaimCount=0.
