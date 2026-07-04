# Phase444 Unlock Projects — Engineering Scoping Memo (2026-07-03)

Prepared post-Phase446 to support the user's investment decision on the two
measurement-scoped unlock projects named by Phase444. Read-only scoping; no
code was changed. Anchors verified against the current tree.

## Bottom line

| Project | Size | Risk | Additive? | Unlocks |
|---|---|---|---|---|
| (i) Lattice-canonical geometry conventions | M–L | Medium-High | Yes (builder-gated), but the *math* is unproven | Exact torus block-diagonalization → exact mode-volume eigenvalue run |
| (ii) Adjoint / joint-gradient path | S–M | Low-Medium | Yes (new methods only) | Feasible SLQ tr-log + cheap gradients; reusable for any future two-loop work |

These are not substitutes. (ii) is the cheap, well-understood, low-risk buy —
no new math, makes the *stochastic* estimate feasible, and its VJP/adjoint
primitive is exactly what any future two-loop machinery needs. (i) is the
expensive, higher-risk bet that could fail outright, but it is the only path
to *exact* block spectra.

## Project (i): lattice-canonical orientation conventions

- The convention lives in ONE file: `src/Gu.Geometry/MeshTopologyBuilder.cs`
  (`Build`, line 19): edge canonical order `(v0<v1)` line 52; vertex-edge sign
  lines 202–204; face order `SortThree` line 82; face boundary orientation
  hard-wired `{+1,-1,+1}` line 123; volume order `SortFour` + `{+1,-1,+1,-1}`
  lines 145/184. Reached via `SimplicialMeshGenerator.CreateUniform4DPeriodic`
  (line 193) → `BuildKuhn4D(n, periodic:true)` (205) → `Build` (273). The
  generator comment (line 176) claims the `(-1)^i` convention "is preserved"
  under wrapping — true for the chain complex (∂∘∂=0), which is a DIFFERENT
  property from translation covariance; the local orientation frame is tied to
  the global index sort, which the row-major wrap reorders (Phase444 F1 =
  56.8%).
- Consumers are convention-agnostic: all ~20+ downstream sites (CurvatureAssembler.cs:38,
  IdentityShiabCpu.cs:79, LocalAlgebraicTorsionOperator.cs:74/153,
  MetricScaledShiabOperator.cs:106, AugmentedTorsionCpu.cs:142,
  FirstOrderShiabOperator.cs:99, EinsteinianShiabOperator.cs:578,
  DiscreteExteriorDerivative.cs:31; vertex-edge-sign: CoulombGaugePenalty,
  CoulombSliceOperator, SpinorDiracOperator) read the precomputed arrays and
  never re-derive the convention; CUDA mirrors once via
  `Gu.Interop/MeshTopologyData.cs`. THE ADDITIVE LEVER: a period-gated builder
  variant emitting covariant arrays fixes everything downstream with zero call
  site edits. A non-additive rewrite breaks hard-coded sign asserts
  (SimplicialMeshTests.cs:124–126, Mesh4DTests.cs:218) and GPU goldens — must
  be additive, open-mesh byte-identical (the Phase444 `VertexFaceRule`/
  `latticePeriod` pattern).
- Scope reducer (confirmed): the mass matrix is NOT implicated
  (`CpuMassMatrix.cs:37,134` uses uniform 1.0 face weights + identity Lie
  metric — already translation-invariant). The pure-curvature non-covariance
  (2.5e-4) comes entirely from CurvatureAssembler's global-index face vertex
  order in the quadratic bracket term (lines 59–85). The fix localizes to the
  builder's orientation arrays + face vertex ordering.
- The hard part is NOT plumbing: it is whether a purely local, index-free
  orientation rule exists that is simultaneously (a) a valid oriented chain
  complex on (Z_n)^4, (b) byte-identical to today's convention on open
  meshes, (c) translation-covariant. The Phase444 prototypes failed because
  they only touched the Shiab's face attachment/bivectors, never the mesh's
  own orientation arrays.
- Cheaper spike FIRST: fold the exact orientation-transfer cocycle into the
  momentum change-of-basis (a Bloch basis carrying the twist), geometry
  untouched. Known caveat: since the pure-curvature scalar itself fails
  covariance, the twist is NOT a pure ±1 sign cocycle — the face bivector
  genuinely rotates within Λ² (an SO-valued cocycle), making this nearly as
  hard; still worth the short spike before the mesh-convention rewrite.
- Size M–L (1–2 core files + small Shiab touches at
  EinsteinianShiabOperator.cs:529/547 and
  EinsteinianShiabBatteries.ConstantTwoForm; cost is design + validation, not
  line count). Risk Medium-High (the mathematical core is unproven — a real
  chance no local convention exists).

## Project (ii): adjoint / joint-gradient path

- Forward chain: ConnectionField → CurvatureAssembler.Assemble
  (CurvatureAssembler.cs:24) → EinsteinianShiabOperator.EvaluateWithTheta
  (EinsteinianShiabOperator.cs:435) → ApplyContraction (275) →
  CpuMassMatrix.InnerProduct (CpuMassMatrix.cs:95); S_B = ½⟨Υ, MΥ⟩.
  Analytic FORWARD linearizations already exist and are FD-verified:
  CovariantExteriorDerivative (573) = dF/dω; LinearizeAnalytic (203);
  LinearizeTheta (462) via Lambda2Algebra.MatrixExpFrechet (484).
- The gap: the existing "transpose" (`CpuLocalJacobian.ApplyTranspose`,
  CpuLocalJacobian.cs:85) assembles Jᵀ COLUMN-BY-COLUMN — O(nIn × nOut),
  which IS the measured ~60 s/Hv blocker. Not a true reverse-mode VJP.
- What's missing — one true transpose per stage, each a ~30–50-line mirror of
  an existing FD-verified forward method:
  1. Contraction adjoint (mirror of ApplyContraction 275–332): swap
     gather/scatter, transpose per-cell faceMap, move _faceInvCount averaging
     to the input side.
  2. Curvature adjoint (mirror of CovariantExteriorDerivative 573–621):
     face-covector → edge-covector; coboundary scatter + frozen-ω bracket.
  3. Theta adjoint (transpose of LinearizeTheta 462): adjoint of the
     exp-Fréchet derivative (= Fréchet derivative of exp(Aᵀ), standard;
     routine exists) + transpose of FaceTheta scatter.
  Then ∇S_B = Jᵀ(MΥ) is ONE composite O(mesh) VJP; an Hv is one central FD of
  that analytic gradient (2 evals, still O(mesh)) — ~60 s → ms-scale, making
  SLQ feasible.
- Fully additive: new transpose methods beside the forward ones + a
  matrix-free joint-gradient assembler (analogue of
  CpuLocalJacobian.ComputeGradient, line 128) + an Hv wrapper. Gating tests:
  the existing FD-vs-analytic pattern
  (EinsteinianShiabBatteries.LinearizeThetaFdResidual, line 434) + the adjoint
  dot-product identity ⟨Jv, w⟩ = ⟨v, Jᵀw⟩.
- Size S–M; risk Low-Medium (only subtlety is the exp-Fréchet adjoint, caught
  cheaply by the dot-product test). Does NOT give exact eigenvalue counts —
  SLQ is variance-bounded stochastic — which is the sole scientific reason to
  also fund (i).

## Recommendation

Fund (ii) FIRST: smaller, lower-risk, no new math, directly clears the
measured SLQ blocker, and the adjoint primitive is reusable by any future
two-loop machinery. Treat (i) as a separate larger bet justified only if
exact block spectra are required; de-risk with a short spike on the SO-valued
translation-cocycle (Bloch-twist) alternative before committing to the
mesh-convention rewrite.
