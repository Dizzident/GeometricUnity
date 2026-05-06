# Implementation P121

Phase121 repairs the Phase120 analytic-variation consistency blocker.

Changes:

- Updated `DiracVariationComputer` to match `CpuDiracOperatorAssembler`:
  - dominant-edge gamma convention instead of continuous edge-direction gamma contraction;
  - gauge-major local DOF ordering instead of spinor-major ordering.
- Added a regression test requiring analytic variation to match finite-difference variation from the assembler.
- Reran Phase120. It now derives a promotable target-independent amplitude measure:
  - finite-difference-to-analytic scale: `1.0000000000001665`;
  - W/Z relative spread: `2.511324481701686E-13`;
  - max scaled residual: `1.345204985743145E-12`.

Result:

- Terminal status: `analytic-variation-amplitude-measure-derived`
- Phase101 now hands off to rerun Phase115 and the W/Z absolute projection with the repaired analytic variation operator.
