# Four-Dimensional Platform Build Plan (Decision Package)

Produced 2026-07-02 from a read-only codebase exploration, in response to
the Phase441 terminal frontier: further internal physics progress
requires realizing the draft's canonical Shiab operator, which needs a
dimension-four base mesh and a spinor-realized invariant basis. This is
a PLATFORM ENGINEERING investment decision - no phase work depends on it
until approved.

## Executive Summary

Feasible and low-risk structurally: the mesh topology types, residual
assembler, mass matrix, and Jacobian machinery are already
dimension-generic; the Phase408/417 gamma-matrix builder (arbitrary
Cl(p,q), tested) needs relocation, not rewriting; all changes are
additive (no destructive refactoring). Total ~3,600 new LOC, 10 new
classes, ~8 modified files, ~75 new tests, 5.5-8 weeks single-developer
(4-5 weeks with two). The dominant risk is physics under-determination
(draft Definition 8.1 / eq. 9.3), not engineering.

## Milestones

- M1 (~1000 LOC, 1.5-2 wks, LOW risk): 4D mesh + discrete forms.
  Extend MeshTopologyBuilder to extract 3-subsimplices (volumes) and
  their boundary orientations; add SimplicialMesh volume properties;
  CreateUniform4D(n) and CreateToy4D() factories; 3-form field type +
  exterior derivative. Acceptance: 2x2x2x2 grid with correct
  vertex/edge/face/volume counts; all existing 2D/3D tests green.
- M2 (~1200 LOC, 2-3 wks, MEDIUM risk): Clifford/spinor layer.
  Relocate GammaMatrixBuilder from study code into Gu.Phase4.Spin;
  SpinorField (vertex-valued Dirac spinors), SpinorDiracOperator,
  minimal invariant-basis projector; instantiate Cl(3,1) (spinor dim 4).
  Risk: Definition 8.1's [Lambda^i x u(64,64)]^{Spin(7,7)} basis needs a
  scoping decision (full vs reduced slice).
- M3 (~1400 LOC, 2-3 wks, HIGH risk): eq. 9.3 Einsteinian Shiab.
  Ricci-like + Weyl-like two-term operator on the 6-dimensional
  Lambda^2(T*X) of the 4D base, implementing IShiabBranchOperator;
  linearization + Hessian symmetry batteries; family-expansion
  comparison phase (the 2D blocker in ShiabFamilyScopeChecker lifts).
  Risk: the draft's operator taxonomy is under-determined - recommend a
  physics decision on Phi_1/Phi_2 before M3 starts; Phase441's
  candidate-family methodology then applies (universality sweep over
  the admissible Phi choices).

## Why This Matters for the Boson Program

Three independent no-go results (Phases 435/436/440) pin the missing
dynamical scale on bosonic structure beyond the 2D toy family, and
Phase441 proved no 2D-realizable completion can differ. M3 delivers the
first genuinely richer Shiab family; the existing phase methodology
(exact batteries, candidate-family universality, fail-closed gates)
transfers directly to 4D studies.

## Open Questions Before Starting

1. Definition 8.1 scope: full u(64,64) invariant basis or reduced Cl(4)
   slice for M2? (Recommend reduced slice + recorded boundary.)
2. Ambient/fiber dimension policy for the 4D base (stay small vs 14D).
3. Whether Shiab variants that change form degree are in scope
   (interface change - defer to M4).
4. Production goal: permanent platform capability vs proof-of-concept
   toward dimX -> 10/14.

Full details (file/line pointers, per-milestone definitions of done,
risk table, test strategy) are in the session journal record of
2026-07-02 and recoverable from the exploration transcript.
