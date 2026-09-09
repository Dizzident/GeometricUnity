# Phase 579 implementation: collective-coordinate Jacobian self-check

## Status

Phase579 executed the Phase485 `O4-F1-COLLECTIVE-COORDINATE` falsifier and
returned `phase450-lineage-convention-defective-preserved-negative`.
The run was deterministic and zero-sampling. The registered
`Random(20260703)` stream was used only to reconstruct the exact Phase450 ray.

## Exact-bound reconstruction

The frozen v2 contract binds seven artifacts: the Phase450 study contract,
program, implementation record, full and summary binding-condition outputs,
and the Phase485 full and summary falsifier censuses. On the lattice-canonical
`n=3` torus, the independent reconstruction obtained 81 vertices, 1,215
edges, and 3,645 omega coefficients. Its 48 registered coefficient slots and
15 active edge types reproduced the committed raw ray norm
`15.579849825913325` and post-projection norm
`1.0000000000000007` exactly at binary64 output precision.

## Result

For `Phi=<u_inv,omega>`, the analytic row Jacobian is `u_inv`, the coarea
Jacobian is `||u_inv||=1`, and the constrained-measure factor is therefore 1.
Centered finite differences independently evaluated every one of the 3,645
components at three frozen steps; the worst absolute discrepancy was
`2.1525757305101978e-11` against `2e-8`.

The exactly solvable isotropic free-Gaussian control used an independently
assembled four-dimensional collective-plus-transverse basis and deterministic
three-dimensional tensor Simpson integration. Its constrained relative
potential matched `alpha*Phi^2/2` at all five frozen coordinate values, with
worst absolute error `8.260059303211165e-14` against `5e-11`; the coarea
factor was included explicitly.

All 81 registered lattice translations preserved the ray exactly and the
coordinate to `5.637851296924623e-17`. The registered projected global-orbit
tangent overlap was `6.676313690258395e-18`, and simultaneous SO(3) adjoint
actions on the ray and field preserved their scalar pairing to
`1.3877787807814457e-17`. Those are, respectively, an infinitesimal statement
at the ray and passive covariance of the pairing; neither proves invariance of
Phase450's fixed coordinate.

The decisive v2 gate therefore applies each finite global adjoint rotation
actively to `omega` while holding the registered `u_inv` fixed. It fails by a
wide margin: the coordinate changes by as much as
`0.018092493622111343` on the frozen generic state and
`0.9157447490058648` on the registered ray, against `5e-14`. Thus the
Jacobian and solvable-limit treatment is internally consistent, but the
claimed gauge invariance of the Phase450-lineage fixed coordinate is not.

This is a preserved first-class implementation negative, not a reinterpretation
or repair of Phase450. The O4 internal assessment now proposes defer pending a
genuinely gauge-invariant replacement. It remains machine-authored and
non-authoritative: Phase450 is unchanged, no O4 ruling is authored,
`mayAuthorRuling:false`, external review remains pending, and
`promotedPhysicalMassClaimCount=0`.

## Hashes

- Program.cs:
  `d72ae8946e289611c550a69e82f7eecf86fbf808f514f8408f91d80a10542480`
- csproj:
  `5afbb08715e55ac1845cb48b8618d1d2ccb3a83d37cc8224f10a00e7bfe54bc2`
- STUDY.md:
  `7c41d40095b0288d421ec3d85bec840444bc90f34afcb915cbe0054f89e026aa`
- frozen contract v2:
  `5144e66f9dd1e703d3883a6463aac5b5650bb6e5e4a7f3f40ba38688a89e5b49`
- full and summary outputs:
  `aa1294627db8b802c657260a6251f763ca525f57f4f8ed766c3a4494179c8cce`
