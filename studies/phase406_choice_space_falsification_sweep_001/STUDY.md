# Phase406: Choice-Space Falsification Sweep (Brute Force #3)

## Question

USER DIRECTIVE (2026-06-11), brute force #3: sweep GU's open discrete
choices (signature, embedding path, algebra class, scalar location)
against the machine-verified structural filters and produce the
surviving/falsified map.

## Construction

Two new exact computations close the open axes (CPU; the GPU portion of
the directive was delivered in Phase405 along with the native-kernel
defect finding): (A) the SU(5)-type route's embedding-derived ratio
computed directly on su(5) with Y = diag(-1/3,-1/3,-1/3,1/2,1/2);
(B) explicit Cl(6,4) and Cl(7,3) gamma constructions with signature
metrics, Clifford relations and chiral-half dimensions machine-verified.
The sweep composes these with the machine-recorded filter outcomes of
Phases 396/397/403/404/405 (read from their summary JSONs) over 16 choice
combinations and 5 filters (doublet presence, custodial capability,
family pattern, derived ratio, VEV selection).

## Results

- **Path independence**: the su(5) route gives tan^2 = 3/5 exactly, equal
  to the Pati-Salam value - the coupling-ratio lineage is a property of
  the chain's complexification, not the route.
- **Signature independence**: both Cl(6,4) and Cl(7,3) verify exactly and
  give 16-dimensional chiral families - the draft's open signature choice
  does not affect the family/ratio structure.
- **The falsification map**: 4 of 16 combinations survive, and they are
  EXACTLY {larger algebra} x {non-adjoint scalar location} x {either
  embedding path} x {either signature}. The su(2)-only toy branch is
  falsified for the doublet route everywhere (explaining Phase397 at the
  choice level); the gauge-adjoint scalar location is falsified on the
  chain everywhere (Phase404).
- **The binding gaps are CHOICE-INDEPENDENT**: no combination anywhere
  provides a VEV selection mechanism (Phase405), and none supplies the
  quantitative chain - these cannot be fixed by picking different
  discrete options; they require the physical derivation.

## Status

Fail-closed. A structural map, not a derivation; no scales; nothing
promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase406_choice_space_falsification_sweep_001/Phase406ChoiceSpaceFalsificationSweep.csproj
```
