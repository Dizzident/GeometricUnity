# Phase410: Curvature-Coupled VEV-Selection Probe

## Question

The primary-speaker source TOE-GU-40YEARS-20250602 states (qualitatively)
that the VEV is "coaxed out of the vacuum" by SCALAR CURVATURE in GU's
improved Einstein equation and plays the role of a fundamental mass
scale. Phase405 proved the bare control-branch objective gives an exactly
flat vacuum manifold with no selection. Does the SIMPLEST faithful
bosonic realization of the curvature claim - a uniform curvature-coupled
quadratic term S_aug = S_B + (kappaR/2) R_eff ||omega||^2 - produce VEV
magnitude/direction selection where the bare objective produced none?

## Construction

Phase405 infrastructure exactly (su(3), 6x6 structured fiber-bundle mesh,
closed dx/dy profiles, CPU reference backend per IA-5, plain-norm
objective). R_eff and kappaR are arbitrary-sign EXTERNAL parameters (the
toy branch has no dynamical metric); VEV formation needs
kappaR R_eff < 0. Batteries: (C1) exact flat-ray coefficients on all 8
rank-1 directions; (C2) direction-blindness of the quadratic invariant
c2; (C3) exact quartic coefficients K at maximal mixing over all 28
pairs, with the proportionality K = const x ||[u,v]||^2 verified; (C4)
the augmented landscape's analytic minima t*^2 = |kappaR R_eff| c2 / (4K)
and depth proportional to 1/K - the deepest stratum classified by su(2) x
u(1) block content (triplet 0-2 / doublet 3-6 / singlet 7).

## Results

- **C1 RUNAWAY**: every rank-1 ray is exactly flat in S_B (max bare
  objective 0.0), so the augmented landscape is UNBOUNDED BELOW along
  every such ray for kappaR R_eff < 0 - no finite VEV forms where the
  vacuum manifold actually lives. Uniform curvature coupling produces
  runaway, not a VEV.
- **C2 DIRECTION-BLIND**: c2 is identical across all 8 rank-1 directions
  (relative deviation 0.0) - no triplet/doublet/singlet ordering can
  arise at quadratic level.
- **C3 EXACT SHAPE**: flatness = commutativity (3 commuting / 25
  non-commuting pairs); S_B = K t^4 exactly on the lifted sector;
  K / ||[u,v]||^2 constant across all non-commuting pairs
  (cv 1.2e-15).
- **C4 NO DOUBLET SELECTION**: the lifted-sector depth ordering is the
  INVERSE bracket-norm ordering; the deepest stratum
  (||[u,v]||^2 = 1/4, 16 pairs) is BLOCK-DEGENERATE across
  doublet-doublet AND doublet-triplet mixed pairs -
  `doubletVevSelectedByCurvatureCoupling = False`.
- **Verdict**: `uniform-curvature-coupling-produces-runaway-not-doublet-
  selection`. Scalar-sector sub-gap (b) remains open with sharpened
  evidence: the curvature-coaxing mechanism requires DIRECTION-DEPENDENT
  curvature coupling or the fermionic/Dirac-sector realization ("a VEV
  in a Dirac-like operator" - the Phase411 candidate) - exactly the
  internal structure the missing scalar-sector specification would have
  to provide.

## Status

Fail-closed. Control-branch objective only; R_eff/kappaR arbitrary
external parameters never fit to data; qualitative-source-motivated
(cite GU-DRAFT-2021 as primary); the fermionic realization and
direction-dependent couplings are NOT probed (named non-claims); no
scales; nothing promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase410_curvature_coupled_vev_selection_probe_001/Phase410CurvatureCoupledVevSelectionProbe.csproj
```
