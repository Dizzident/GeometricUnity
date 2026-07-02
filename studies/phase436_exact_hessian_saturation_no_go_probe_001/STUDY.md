# Phase436: Exact-Hessian Saturation No-Go Probe

The tie-in named by Phase435. Phases 430/431/435 modelled the bosonic one-loop
with a workbench mass model (masses^2 = eigenvalues of `-(ad_u)^2`). This phase
computes the TRUE control-branch bosonic Hessian of the exact objective
`S_B(omega) = (1/2)||Upsilon||^2` at constant rank-1 backgrounds `t*u`, using
the repo's own CPU reference solver (su(3), trivial torsion, identity Shiab,
`A0 = 0`) on the 2x2 fiber-bundle mesh (52 edges, 24 faces; carrier 416,
Upsilon 192).

## (A) Exact quadratic decomposition theorem

On this branch `Upsilon = d omega + (1/2)[omega wedge omega]` is exactly
degree-2 in `omega` (verified: third field-difference `~3.5e-16`; the mixed
second difference is step-size invariant). Its linear part `L = d` is the free
Yang-Mills kinetic operator (`||L|| = 24`, nonzero) that annihilates closed
forms - which is exactly why the exact-1-form rank-1 backgrounds stay flat
(`S_B(t*u) = 0`). Consequently the exact Hessian at `t*u` is EXACTLY a degree-2
matrix polynomial in `t`:

```
H(t) = A0 + t*A1 + t^2*A2      (nothing beyond t^2)
```

verified by the vanishing third `t`-difference (`0` to machine precision on 200
sampled matrix elements per direction). `A0 = H(0)` is the background-independent
free-kinetic Hessian (full rank 192); `A2` is the quadratic-in-background mass
term. The odd-in-`t` term `A1` is recorded honestly: it VANISHES for the
`lambda_8`-type Cartan/hypercharge background (`|A1|/|A0| = 0`) and is NONZERO
for the `lambda_4`-type root background (`|A1|/|A0| = 0.15`) - a genuine
covariant cross term, not a defect.

**Headline:** asymptotically `H(t) -> t^2*A2`, so the growing Hessian masses^2
scale EXACTLY as `t^2`, the bosonic one-loop grows exactly logarithmically, and
NO log-saturation can arise from the exact control-branch Hessian at one loop.
The Phase435 scale gap is pinned to structure BEYOND the control branch (a
physical VO-6/VO-7 completion or a source anchor), not to workbench modeling.

## (B) Workbench-model fidelity

Exact growing-mode counts (positive eigenvalues of `A2 = J2^T J2`, with
`J2 = J(u) - J(0)` the linearized-bracket operator):

| direction     | exact count | `-(ad_u)^2` nonzero | factor |
|---------------|-------------|---------------------|--------|
| lambda_8-type | 64          | 4                   | 16     |
| lambda_4-type | 96          | 6                   | 16     |

The counts factor EXACTLY as `rank(-(ad_u)^2) * 16` with a SINGLE shared
geometric multiplicity, so the per-su(3)-direction counts match the workbench
counts: Phase430's log-slope direction-selection arithmetic is CONFIRMED under
the exact control-branch Hessian (`phase430SlopeCountsConfirmedByExactHessian =
True`). Only the mass VALUES differ (exact `A2` eigenvalues `O(0.2)`; workbench
`O(0.25..1)`) - the workbench model is a recorded convention and only counts
entered Phase430's verdicts.

## Verdict

`exactHessianSaturationNoGoProbePassed = True`,
`exactHessianQuadraticDecompositionVerified = True`,
`exactHessianMassesGrowExactlyAsTSquared = True`,
`logSaturationImpossibleFromExactControlBranchHessianAtOneLoop = True`,
`scaleGapPinnedBeyondControlBranch = True`.

Everything is control-branch structure recorded blind. No scale, pole, or GeV
lineage exists; no Phase201 or Phase256 field is filled; nothing is promoted.

## Run

```bash
dotnet run -c Release --project studies/phase436_exact_hessian_saturation_no_go_probe_001/Phase436ExactHessianSaturationNoGoProbe.csproj
```
