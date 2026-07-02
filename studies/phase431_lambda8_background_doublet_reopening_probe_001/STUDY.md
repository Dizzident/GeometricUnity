# Phase431: lambda_8-Background Doublet-Reopening Probe

Stage two of the Coleman-Weinberg program begun in Phase428. Phase428 proved
the one-loop fermion determinant on constant rank-1 rays `omega = t*u` is an
adjoint-orbit class function, so the triplet axes `T={lambda_1,2,3}` and the
doublet axes `D={lambda_4..7}` are EXACTLY degenerate for any su(3)-invariant
fermion sector - doublet selection requires su(3)-breaking fermionic
structure. This phase supplies one candidate breaking: a constant `lambda_8`
background `omega_bg = t8*(lambda_8/2)`.

Mechanism: `[lambda_8, T-axes] = 0` (background leaves the triplet unbroken)
while `[lambda_8, D-axes] != 0` (it breaks the doublet), so the class-function
argument no longer forces T/D degeneracy.

## Result (machine-verified)

- BLOCK SPECTRUM: the background-plus-probe Dirac operator block-diagonalizes
  over lattice momentum into `4*dimG`-dim blocks with the exact closed form
  `lambda^2 = (s1 + m_c)^2 + (s2 + m_c)^2`, where `m_c` are the eigenvalues of
  the single Hermitian gauge matrix `M(t8,t,u) = t8*(lambda_8/2) + t*(lambda_u/2)`
  in the fermion rep (the same `M` couples on both lattice directions, so
  `s1 I + M` and `s2 I + M` commute and the block factorizes). Verified
  against a full 192-dim dense Hermitian Dirac solve at a noncommuting point
  (`t8=1, t=0.7, u=lambda_4`), residual 4.4e-13.
- REOPENING: the one-loop potentials are exactly degenerate at `t8=0`
  (residual 3.6e-14) and split by O(45-135) once `t8>0`.
- BLOCK-DEPENDENT MASS LAW: the small-t quadratic coefficient of the fermion
  one-loop potential (fundamental) is POSITIVE on the triplet probe and
  NEGATIVE on the doublet probe (opposite signs, both finite and h-stable):

  | t8  | A_T (lambda_1) | A_D (lambda_4) |
  |-----|----------------|----------------|
  | 0.0 | 2.32e8 (degenerate, non-analytic origin) | 2.32e8 |
  | 0.5 | +190.26        | -97.70         |
  | 1.0 | +41.51         | -28.68         |
  | 2.0 | +4.18          | -16.04         |

  In the gauge/boson sector the doublet direction is GAPPED (analytic finite
  quadratic: A_D(B) = 89.7/34.1/12.4 for t8=0.5/1.0/2.0) while the triplet
  retains probe-Higgsed massless-to-massive modes (non-analytic log runaway,
  2 modes lifted). This is exactly the direction-dependent quadratic structure
  Phase418 had to import by hand.

## Fail-closed

`t8` is a RECORDED CANDIDATE BACKGROUND PARAMETER swept over a grid, NOT a
dynamically derived scale (deriving it is the Phase430 chain's job). The 4x4
lattice, 4-dim spinors, naive central-difference Dirac, fundamental/adjoint
fermion contents, and the bosonic structural model are recorded conventions.
No scale law is produced, no target value is consulted, and no Phase201 or
Phase256 field is filled. The triplet-vs-doublet ordering is `t8`- and
`t`-dependent (recorded, not used to claim dynamical selection).

## Run (Release, sub-second)

```bash
dotnet run -c Release --project studies/phase431_lambda8_background_doublet_reopening_probe_001/Phase431Lambda8BackgroundDoubletReopeningProbe.csproj
```
