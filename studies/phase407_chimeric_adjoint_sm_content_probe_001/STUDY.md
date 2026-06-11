# Phase407: Chimeric-Adjoint SM-Content Probe

## Question

The Phase406 falsification map left one surviving scalar route: the
NON-ADJOINT components of the GU connection. Phase404 excluded the
SM-Higgs doublet from the internal gauge adjoint (45). Does the FULL
chimeric algebra's adjoint contain the SM-Higgs quantum numbers, and
where?

## Construction

so(14) compact arithmetic for the chimeric spin(7,7) (the Phase404
real-form caveat applies): adjoint 91 = so(4)_spacetime 6 +
so(10)_internal 45 + frame-cross-internal (4 x 10) block 40. The internal
factor carries the Phase404 SM chain (color = traceless centralizer of J;
Pati-Salam su(2)_L/R with auto-resolved signs; Y = R3 + J/3). The 91 is
branched into simultaneous eigenspaces of {spacetime Casimir, color
Casimir, su(2)_L Casimir, Y^2}, every block tagged.

## Results

- **SM-Higgs-pattern doublets EXIST in the chimeric adjoint**:
  color-singlet, j = 1/2, |Y| = 1/2 EXACTLY (lepton-normalized) - 16
  states, and they live precisely in the frame-cross-internal (4 x 10)
  block, the sector the falsification map isolated. Mechanism
  (machine-verified): the Pati-Salam vector 4 = (2_L, 2_R) carries
  B-L = 0, so the doublet's hypercharge is the pure su(2)_R Cartan
  +-1/2.
- **The spacetime-scalar sector (pure internal 45 + pure spacetime 6)
  contains NO such doublet** - Phase404's exclusion re-confirmed inside
  the bigger frame; the dimension accounting closes exactly (91 states).
- **Honest tag**: the found doublets carry a spacetime-VECTOR index. The
  spin-0 extraction - the Iceberg's symmetric-2-tensor/vertical-form
  route through the Y14 -> X4 pullback (GU-DRAFT-2021 section 9) - is
  the NAMED next structural step before this block can be called a Higgs
  candidate; the binding gaps (VEV selection, quantitative chain) are
  unchanged.

## Status

Fail-closed. Representation bookkeeping only; compact-form caveat
recorded; no dynamics, scales, or promotion; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase407_chimeric_adjoint_sm_content_probe_001/Phase407ChimericAdjointSmContentProbe.csproj
```
