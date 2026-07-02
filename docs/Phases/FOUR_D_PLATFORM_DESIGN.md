# Four-Dimensional Platform — Detailed Technical Design

Author: Architect (4D platform build team lead)
Date: 2026-07-02
Status: APPROVED FOR IMPLEMENTATION — physics decisions folded in (2026-07-02)
Companion to: `docs/Phases/FOUR_D_PLATFORM_BUILD_PLAN.md`
Physics decisions: `docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md` (physicist-4d)

This is the implementation contract for M1/M2/M3. It is precise enough that a
developer implements without guessing. Physics-underdetermined choices are
explicitly parameterized and marked **[FAMILY]**; developers implement the
*family*, not a hardcoded member. All six physicist-4d decisions are now folded
in; the previously open **[PHYSICIST-GATED]** items are resolved and cited to
the physics-decisions memo, and the one delegated interface decision (Shiab
output degree) is settled in §3.2.

---

## 0. Ground rules (apply to every milestone)

- **Additive only.** No existing public member changes signature or behavior.
  3169 existing tests stay green; 0 warnings.
- **Backward-compat rule (HARD CONSTRAINT, verified against the codebase).**
  `SimplicialMesh` is constructed by object-initializer in three places:
  `MeshTopologyBuilder.Build`, `tests/Gu.Phase2.Viz.Tests/Phase2VizTests.cs`,
  and `tests/Gu.VulkanViewer.Tests/TestDataHelper.cs`. Both test sites set
  *exactly* the current `required` properties. Therefore **every new
  `SimplicialMesh` property MUST be non-`required`** and default to an empty
  array (`Array.Empty<int[]>()`), never `required`, never null-by-contract.
  This guarantees the two hand-written test meshes and all existing `Build`
  callers compile and behave identically with zero edits.
- **Build/test loop:** `dotnet build && dotnet test --no-build`; stale cache →
  `dotnet clean && dotnet build`. A linter rewrites csproj `TargetFramework` to
  `net10.0` — do not fight it.
- **Types are `sealed class`, not `record`.** No `with`. `System.Math.Sqrt`
  where `Gu.Math` is imported.
- **Work on `main`.** One small additive commit per milestone slice, message
  style matching `git log` ("Add …"). Suggested:
  `Add 4D simplicial volume topology (platform M1)`,
  `Add Cl(4) spinor layer (platform M2)`,
  `Add Einsteinian Shiab family skeleton (platform M3)`.

### Milestone dependency graph

```
        M1 (Gu.Geometry)  ── 4D mesh + 3-subsimplex topology + discrete d
        /            \
   (soft dep)      (hard dep)
      /                  \
 M2 (Gu.Phase4.Spin)   M3 (Gu.ReferenceCpu + Gu.Branching)
 Clifford/spinor       Einsteinian Shiab family
```

- **M1 → M2 (soft).** M2 code compiles and unit-tests against *any*
  `SimplicialMesh` (including existing 2D/3D). M2's *final acceptance* runs on
  `CreateUniform4D`. dev-spinor may start immediately, testing the Dirac hop on
  a 2D/3D mesh, and swap in 4D when M1 lands.
- **M1 → M3 (hard).** The Einsteinian Shiab's family-members-differ property is
  *only realizable on dimX ≥ 4* (on dimX=2, Λ²(T*X) is 1-dimensional — see
  `ShiabFamilyScopeChecker`, reasons 1–4). dev-shiab must wait for
  `CreateUniform4D` + `CellFaces`/volume data before the distinguishing tests
  can pass.
- **M2 ⟂ M3.** No shared files, no ordering between them. The spinor layer is
  independent of the bosonic Shiab.

### File-ownership map (NO shared file edited by two parallel devs)

| Dev | Project | Files (M = modify, N = new) |
|---|---|---|
| dev-mesh (M1) | `Gu.Geometry` | `SimplicialMesh.cs` (M), `MeshTopologyBuilder.cs` (M), `SimplicialMeshGenerator.cs` (M), `ToyGeometryFactory.cs` (M), `DiscreteExteriorDerivative.cs` (N), `ThreeFormField.cs` (N) |
| dev-spinor (M2) | `Gu.Phase4.Spin`, `Gu.Phase4.Dirac` | `CliffordAlgebraFactory.cs` (N), `SpinorField.cs` (N, only if no existing type), `IInvariantBasisProjector.cs` (N), `ReducedCliffordSliceProjector.cs` (N) in `Gu.Phase4.Spin`; **additive** `GammaEdgeScheme` refinement to `CpuDiracOperatorAssembler.cs` (M, opt-in, default unchanged) in `Gu.Phase4.Dirac` |
| dev-shiab (M3) | `Gu.ReferenceCpu`, `Gu.Branching` | `EinsteinianShiabOperator.cs` (N), `EinsteinianShiabFamilySpec.cs` (N); registration only in a study/test, no edit to `BranchOperatorRegistry.cs` (it is data-driven) |
| qa-4d | `tests/*` | new `*4DTests.cs` in the matching test project per milestone |

`ToyGeometryFactory.cs` and `SimplicialMeshGenerator.cs` are both owned solely
by dev-mesh; no other dev edits `Gu.Geometry`. If M3 ever needs a new geometry
helper, it requests it from dev-mesh (sequenced through the architect), never
edits `Gu.Geometry` directly.

---

## 1. M1 — 4D mesh + 3-subsimplex (volume) topology + discrete forms

**Owner:** dev-mesh. **Project:** `Gu.Geometry`. **Risk:** LOW.

### 1.1 New `SimplicialMesh` properties (all NON-required, empty-array default)

Add the 3-subsimplex ("volume") layer, mirroring the existing edge/face layer.
"Volume" = 3-subsimplex = tetrahedron. Index conventions and orientation match
the existing standard simplicial boundary `∂[v0..vk] = Σ_i (-1)^i [omit v_i]`.

```csharp
/// <summary>
/// Volumes (3-subsimplices). Each volume is a quadruple of vertex indices,
/// canonical order v0 < v1 < v2 < v3. Populated only when SimplicialDimension >= 3
/// (empty for 2D meshes). ad-valued 3-forms live on volumes.
/// </summary>
public int[][] Volumes { get; init; } = Array.Empty<int[]>();

/// <summary>
/// For each volume, its 4 boundary faces (2-subsimplices), stored in the
/// omit-vertex order (omit v0, omit v1, omit v2, omit v3) i.e. faces
/// {v1v2v3, v0v2v3, v0v1v3, v0v1v2}, each in canonical sorted form.
/// </summary>
public int[][] VolumeBoundaryFaces { get; init; } = Array.Empty<int[]>();

/// <summary>
/// Orientation signs for each volume's boundary faces:
/// VolumeBoundaryOrientations[vol][i] = (-1)^i  →  {+1, -1, +1, -1}.
/// Discrete d: (d omega3form)[vol] uses these; d(2-form)->3-form is the
/// transpose/coboundary and uses the SAME signs (see 1.5).
/// </summary>
public int[][] VolumeBoundaryOrientations { get; init; } = Array.Empty<int[]>();

/// <summary>
/// For each top cell (4-simplex / pentachoron), the indices of its volumes.
/// CellVolumes[cellIdx] = the C(5,4)=5 volume indices (omit each vertex).
/// Empty for meshes with SimplicialDimension < 3.
/// </summary>
public int[][] CellVolumes { get; init; } = Array.Empty<int[]>();
```

Add derived count (non-breaking, computed):

```csharp
/// <summary>Number of volumes (3-subsimplices). Zero for 2D meshes.</summary>
public int VolumeCount => Volumes.Length;
```

**OPTIONAL / DEFERRED — `CellVolumeOrientations`.** The top exterior derivative
`d: 3-form → 4-form` (values on cells) needs per-cell signed volume incidence.
This is NOT required by M2/M3 (the Shiab is a 2-form→2-form operator; torsion is
2-form-valued). Defer it: add only if a later 4-form study needs it. Rationale:
keeps M1 minimal and the acceptance surface small. Documented here so it is a
known, named gap rather than an omission.

**Orientation sign convention — canonical statement (consistent with existing
edge→face convention).** For a k-simplex `[v0<v1<...<vk]`, the boundary is
`∂ = Σ_{i=0}^{k} (-1)^i [v0,…,v̂_i,…,vk]`; the sub-simplex omitting the i-th
listed vertex carries sign `(-1)^i`, and it is stored in canonical sorted order
(already sorted because we delete one entry from a sorted list). This is exactly
the rule `MeshTopologyBuilder` already applies to faces (face `{v0,v1,v2}` →
edges with signs `(-1)^2, (-1)^1, (-1)^0` on `{e01, e02, e12}` = `+,-,+`, i.e.
sign = `(-1)^{index of the omitted vertex}`). M1 applies the identical rule one
degree up.

### 1.2 `MeshTopologyBuilder` — extract 3-subsimplices

Extend `Build` (do not add a new public method). Mirror the existing face
extraction block, one degree higher, guarded by `simplicialDimension >= 3`.

Algorithm (insert after the face block, before vertex-edge incidence):

1. **Extract unique volumes.** Iterate cells; for every 4-subset
   `(i<j<k<l)` of a cell's vertices, canonicalize via a `SortFour(a,b,c,d)`
   helper (mirror `SortThree`), dedup through
   `Dictionary<(int,int,int,int), int> volumeMap`, append to `volumeList`.
   Accumulate `cellVolumesList[cellIdx]`.
   Guard: only run when `simplicialDimension >= 3`; otherwise every
   `cellVolumesList` entry is `Array.Empty<int>()` and `volumeList` is empty.
2. **Volume boundary faces + orientations.** For each volume
   `{v0<v1<v2<v3}`, look up the 4 boundary faces in `faceMap`
   (they are guaranteed present because faces are extracted from the same
   cells):
   - omit v0 → `SortThree(v1,v2,v3)`, sign `+1`
   - omit v1 → `SortThree(v0,v2,v3)`, sign `-1`
   - omit v2 → `SortThree(v0,v1,v3)`, sign `+1`
   - omit v3 → `SortThree(v0,v1,v2)`, sign `-1`
   Store as `VolumeBoundaryFaces[vol] = {f_omit0, f_omit1, f_omit2, f_omit3}`,
   `VolumeBoundaryOrientations[vol] = {+1,-1,+1,-1}`.
3. Populate the four new properties in the returned `SimplicialMesh`. When
   `simplicialDimension < 3` leave them as their empty-array defaults (do not
   set them). This preserves the 2D construction path byte-for-byte.

**Dedup / canonical ordering** is identical in spirit to faces: canonical =
strictly ascending vertex tuple; the map key IS the canonical tuple; index =
insertion order.

**Behavioral note (additive, safe):** after M1, existing 3D meshes
(`CreateUniform3D`, `CreateToy3D`) begin to carry populated `Volumes` etc. This
is purely additive — no existing property changes, no existing test reads these
new arrays. It is a free correctness bonus (3D volume/boundary data becomes
available) and must be covered by a regression test asserting the 3D volume
data is self-consistent via exact-integer `∂∂=0` nilpotency (`B1·B2=0`,
`B2·B3=0`) + sharing-multiplicity. Do NOT assert Euler = 1 for the 3D mesh — the
pre-existing 5-tet decomposition is non-conforming (`V−E+F−Vol = 3`); see §1.7
point 5.

### 1.3 `SimplicialMeshGenerator.CreateUniform4D(int n)` — Freudenthal/Kuhn

**Decision: use the Kuhn–Freudenthal (S₄ permutation) triangulation, 24
pentachora per tesseract.** Rationale, and the explicit check the plan
requested:

> The existing `CreateUniform3D` uses a **5-tetrahedra** per-cube decomposition
> (not the 6-tet Kuhn). We deliberately do **not** mirror the 5-simplex style in
> 4D. The permutation (Kuhn/Freudenthal) construction is chosen because it is
> **conforming across shared cell boundaries by construction** (adjacent cells
> agree on the diagonal of every shared sub-face, so `MeshTopologyBuilder`'s
> vertex-tuple dedup produces a valid manifold), it generalizes cleanly to any
> dimension, and it has clean, verifiable subsimplex counts (below). The
> per-cube count is `d! = 24` for d=4.

Construction:

```
vertexCount = (n+1)^4
Idx(x,y,z,w) = ((x*(n+1) + y)*(n+1) + z)*(n+1) + w   // row-major, w fastest
coords[v*4 + {0,1,2,3}] = {x,y,z,w}
For each hypercube (x,y,z,w) in [0,n)^4:
  lower corner c = (x,y,z,w)
  For each permutation π of (0,1,2,3):   // 24 permutations
     p = c                               // start vertex (0,0,0,0 offset)
     cellVerts[0] = Idx(p)
     for step k = 0..3:
        p[π[k]] += 1                      // add one unit along axis π[k]
        cellVerts[k+1] = Idx(p)
     append cellVerts (5 vertices) to cells
Build via MeshTopologyBuilder.Build(embeddingDimension:4, simplicialDimension:4, ...)
```

All 24 pentachora of a tesseract share the main diagonal
`(0,0,0,0)→(1,1,1,1)`; the k-simplices of the triangulation are exactly the
chains of subsets of `{0,1,2,3}` under inclusion (this is what makes the counts
below exact).

Validation: `n < 1` → `ArgumentOutOfRangeException` (match `CreateUniform3D`).

### 1.4 Acceptance counts — shown arithmetic

The k-subsimplices of the Kuhn triangulation of a **single** tesseract are in
bijection with chains of `k+1` distinct subsets of `{1,2,3,4}` ordered by
inclusion. Number of chains of `k` subsets with size-set `{s_1<…<s_k}` is the
multinomial `4! / (s_1! (s_2−s_1)! … (4−s_k)!)`. Summing over all size-sets:

| subsimplex | k (vertices) | count per tesseract |
|---|---|---|
| vertices | 1 | `Σ C(4,s) = 16` |
| edges | 2 | `65` |
| faces | 3 | `110` |
| volumes | 4 | `84` |
| cells (pentachora) | 5 | `4! = 24` |

Worked totals: edges `4+6+4+1+12+12+4+12+6+4 = 65`; faces
`12+12+4+12+6+4+24+12+12+12 = 110`; volumes `24+12+12+12+24 = 84`.
**Euler check (single tesseract, contractible 4-ball, χ = 1):**
`16 − 65 + 110 − 84 + 24 = 1` ✓.

**`CreateUniform4D(1)` — PRIMARY hand-verified golden acceptance:**

```
VertexCount = 16, EdgeCount = 65, FaceCount = 110, VolumeCount = 84, CellCount = 24
Euler: V - E + F - Vol + C == 1
```

qa-4d asserts all five counts exactly plus the Euler identity. These are
independently derived above and are the definitive M1 gate.

**`CreateUniform4D(2)` — exact where derivable, invariant-gated otherwise:**

- Exact and asserted: `VertexCount = 3^4 = 81`, `CellCount = 16 * 24 = 384`.
- **Euler invariant (asserted):** `[0,2]^4` is a contractible solid block, so
  `V − E + F − Vol + C = 1`, i.e. `81 − E + F − Vol + 384 = 1` ⟹
  `E − F + Vol = 464`. qa-4d asserts this identity against whatever the builder
  produces.
- **Boundary-manifold invariant (asserted):** every interior volume
  (3-subsimplex) is shared by exactly 2 pentachora; every boundary volume by
  exactly 1. (Compute a volume→cell incidence multiplicity histogram; assert
  values ∈ {1,2} and the boundary count matches the 4·(surface) expectation.)
- **E, F, Vol golden values for n=2:** hand-derivation across the 16-tesseract
  dedup is error-prone and is **not** pre-committed here. Procedure: on the
  first green implementation run, capture `EdgeCount/FaceCount/VolumeCount`,
  confirm they satisfy `E − F + Vol = 464` and the boundary-sharing invariant,
  then pin them as golden regression constants in the test. This is the honest,
  robust gate; the Euler + boundary invariants are strong enough to catch any
  topology bug (a missing or double-counted subsimplex breaks χ).

### 1.5 Discrete forms: `ThreeFormField` + `d: 2-form → 3-form`

**Decision: INCLUDE (cheap given 1.1/1.2).** Keep it dependency-free of
`Gu.Math`/`LieAlgebra` so it lives entirely in `Gu.Geometry` (dev-mesh owns it,
no cross-project edit). Operate on flat `double[]` coefficient arrays with an
explicit `componentsPerCarrier` (this is exactly how the ad-valued layout
`[carrierIdx * dimG + a]` is used elsewhere; `dimG` is passed as
`componentsPerCarrier`).

```csharp
// ThreeFormField.cs — ad-valued (or scalar) 3-form sampled on volumes.
public sealed class ThreeFormField
{
    public SimplicialMesh Mesh { get; }
    public int ComponentsPerVolume { get; }        // e.g. dim(g); 1 for scalar
    public double[] Coefficients { get; }           // length Mesh.VolumeCount * ComponentsPerVolume
    // ctor validates length == VolumeCount * ComponentsPerVolume
}

// DiscreteExteriorDerivative.cs — pure topology-driven coboundary.
public static class DiscreteExteriorDerivative
{
    /// (d omega)[face] = Σ_i FaceBoundaryOrientations[face][i] * omega[edge_i].
    /// Mirrors CurvatureAssembler's linear (d) part; provided for completeness.
    public static double[] EdgeToFace(SimplicialMesh mesh, double[] edgeCoeffs, int componentsPerCarrier);

    /// (d alpha)[volume] = Σ_i VolumeBoundaryOrientations[volume][i] * alpha[face_i].
    /// The 2-form → 3-form exterior derivative on the 4D mesh.
    public static ThreeFormField FaceToVolume(SimplicialMesh mesh, double[] faceCoeffs, int componentsPerCarrier);
}
```

Acceptance (per physicist-4d decision 2 — `d∘d = 0` is COMBINATORIAL, not
numerical): the primary gate is **exact-integer** nilpotency of the incidence
(boundary) matrices. Let `B1` = vertex×edge, `B2` = edge×face, `B3` = face×volume
signed incidence matrices (entries in {−1,0,+1} from the orientation
conventions). Assert `B1·B2 == 0` and `B2·B3 == 0` as **integer** matrix
products (no tolerance — every entry exactly 0). This is `∂∘∂ = 0` on the
oriented complex and is the strongest, cheapest proof the volume boundary signs
are correct. Then additionally assert the ad-valued `double` form
`FaceToVolume(EdgeToFace(ω)) == 0` to machine precision on `CreateUniform4D(1)`
as a smoke test of the `double[]` code path.

### 1.6 `ToyGeometryFactory` additions

Add two factories; both delegate to `MeshTopologyBuilder.Build` with
`simplicialDimension:4` so the new volume layer is populated automatically.

- **`CreateToy4D()`** — one tesseract via the Kuhn triangulation (equivalent to
  `CreateUniform4D(1)` wrapped as a trivial-fiber `FiberBundleMesh`,
  `BaseMesh == AmbientMesh`, identity maps). Human-debuggable smallest case for
  M3 unit tests. Section coefficients: unit vector length = pick 1 (trivial),
  matching the `CreateStructured2D` pattern.
- **`CreateStructuredFiberBundle4D(int n, int fiberSize = 1)`** — analog of
  `CreateStructuredFiberBundle2D`. The base X_h is `CreateUniform4D(n)`
  (dimX = 4).
  - `fiberSize == 1` (**default**): trivial fiber, `BaseMesh == AmbientMesh`,
    identity `π`/`σ` maps, exactly like `CreateStructured2D`. This is what M3's
    Einsteinian Shiab studies consume (the Shiab is intrinsic to the 4D base).
  - `fiberSize > 1`: attach `fiberSize` fiber points per base vertex in an
    embedding dimension `dimY = 4 + (fiberSize additional coords)`, mirroring
    `CreateStructuredFiberBundle2D`'s 5D construction (base coords + small fiber
    offsets), with `σ` selecting fiber point 0. **RESOLVED (decision 2):** first
    4D studies use the trivial fiber (`fiberSize=1`); the higher-`dimY` path
    toward 14D is a parameterized future extension, not the M3 default.

Fiber-attachment spec for `fiberSize > 1` (explicit, mirrors the 2D factory):
`yVertCount = xVertCount * fiberSize`; `yCoords[yv*dimY + 0..3] = base coords`;
`yCoords[yv*dimY + (4+j)] = f * smallOffset(j)`; `xVertToYVert[xv] = xv*fiberSize`;
Y cells built from σ-selected fiber points per base cell (copy the 2D pattern's
`XCellToYCell` bookkeeping). Validate with `ValidateSection()`/`ValidateFibers()`.

### 1.7 M1 acceptance gate (qa-4d enforces)

1. All 3169 existing tests green; 0 warnings.
2. `CreateUniform4D(1)`: exact counts 16/65/110/84/24 + Euler = 1.
3. `CreateUniform4D(2)`: V=81, C=384, `E−F+Vol=464`, boundary-sharing ∈ {1,2},
   E/F/Vol pinned as golden.
4. `∂∘∂ = 0` as **exact-integer** `B1·B2 == 0` and `B2·B3 == 0` on
   `CreateUniform4D(1)`, plus the `double`-path `d∘d` smoke test.
5. Existing 3D mesh now carries self-consistent `Volumes`, certified by
   exact-integer boundary-of-boundary nilpotency (`B1·B2=0`, `B2·B3=0`) +
   face/volume sharing-multiplicity on `CreateUniform3D(2)`. **NOTE (design
   correction, dev-mesh + arch-4d, approved):** do NOT assert Euler = 1 for the
   3D mesh. The pre-existing 5-tet cube decomposition is **non-conforming across
   shared cube faces** (adjacent cubes use mismatched face diagonals — the very
   reason M1 chose the conforming Freudenthal construction for 4D, §1.3), so its
   alternating sum `V−E+F−Vol = 3`, not 1. `∂∂=0` nilpotency is the correct
   dimension- and conformity-agnostic self-consistency certificate here (it is a
   purely local per-simplex identity, valid for any chain complex). The 4D Euler
   assertions (items 2–3) remain valid because the Freudenthal 4D mesh IS
   conforming.
6. Backward-compat: the two hand-written `new SimplicialMesh{…}` test sites
   compile unchanged; a 2D mesh has `VolumeCount == 0` and all new arrays empty.

---

## 2. M2 — Clifford / spinor layer

**Owner:** dev-spinor. **Project:** `Gu.Phase4.Spin`. **Risk:** MEDIUM.

### 2.1 GammaMatrixBuilder relocation — **CORRECTION: already production**

The build plan says "relocate `GammaMatrixBuilder` from study code". **Verified:
it already lives in `src/Gu.Phase4.Spin/GammaMatrixBuilder.cs`** as a production
class, together with `CliffordSignature`, `GammaOperatorBundle`,
`SpinorRepresentationSpec`, `GammaConventionSpec`, `ChiralityConventionSpec`,
`ConjugationConventionSpec`, `CliffordAlgebraValidator`,
`CliffordValidationResult`. **No relocation is needed.** M2 is purely additive
on top of these. dev-spinor must NOT modify `GammaMatrixBuilder` (it is tested
and consumed elsewhere).

Key facts to build on: `GammaMatrixBuilder.Build(signature, convention,
provenance)` returns a `GammaOperatorBundle` with `GammaMatrices` =
`Complex[][,]` (`[direction][row,col]`), `SpinorDimension = 2^floor(n/2)`,
negative-signature directions multiplied by `i`, chirality matrix for even n.

### 2.2 `CliffordAlgebraFactory.CreateClifford4D(...)` — parameterized signature

**RESOLVED (physicist-4d decision 1): PRIMARY = Cl(4,0) Euclidean** (spinor dim 4);
signature stays parameterized as Cl(p,q) with Cl(3,1) a supported branch variant.
Rationale that drives M3: in Euclidean 4D the Hodge star on 2-forms satisfies
`*² = +1`, giving the **real** self-dual/anti-self-dual split `Λ² = Λ²₊ ⊕ Λ²₋`
(3+3) that the Einsteinian Ricci/Weyl Shiab (§3) needs. Lorentzian Cl(3,1) gives
`*² = −1` (a complex structure on Λ²) — messier for a first realization — and the
entire existing workbench is Euclidean. Design for BOTH; **default to Cl(4,0)**.

```csharp
public static class CliffordAlgebraFactory
{
    /// Build the 4D gamma bundle. signature defaults are supplied by the caller;
    /// physicist selects Cl(3,1) (Lorentzian) or Cl(4,0) (Riemannian) later.
    public static GammaOperatorBundle CreateClifford4D(
        CliffordSignature signature,               // {Positive=4,Negative=0} primary; {Positive=3,Negative=1} variant
        GammaConventionSpec? convention = null,    // default: "dirac-tensor-product-v1"
        ProvenanceMeta? provenance = null);

    // Convenience; Riemannian is the PRIMARY per decision 1, Lorentzian a variant:
    public static GammaOperatorBundle CreateClifford4DRiemannian();   // Cl(4,0)  <-- primary/default
    public static GammaOperatorBundle CreateClifford4DLorentzian();   // Cl(3,1)  <-- branch variant
}
```

Contract: `signature.Dimension == 4`; result `SpinorDimension == 4`; chirality
matrix present (even dimension). Validate via the existing
`CliffordAlgebraValidator` (anticommutator `{γ^μ,γ^ν} = 2 η^{μν} I`) and store
the `CliffordValidationResult` on the bundle. Both signatures MUST pass the
validator (with η = diag(+,+,+,+) or diag(+,+,+,−) respectively). The core
method still takes an explicit `signature` (no buried constant); `Riemannian`
is the default entry point studies use. **[FAMILY]** M2 reuses/extends the
already-production `GammaMatrixBuilder` (arbitrary Cl(p,q)); it does not
relocate anything — the plan's "relocate from study code" is stale (decision 1).

### 2.3 `SpinorField` — vertex-valued Dirac spinors

**First check `Gu.Phase4.Dirac` for an existing vertex-spinor representation**
(the project already has `FermionSpectralConfig`, `MassPsiWeightsBuilder`, and
the Dirac assembler that consumes spinor vectors). If a suitable vertex-spinor
type exists, dev-spinor extends/reuses it rather than duplicating. If not, add
`SpinorField` in `Gu.Phase4.Spin` as below.

```csharp
public sealed class SpinorField
{
    public SimplicialMesh Mesh { get; }
    public int SpinorDimension { get; }              // 2^floor(dim/2); 4 in 4D
    public int GaugeComponents { get; }               // default 1 (pure Clifford spinor)
    /// Row-major, complex: Values[(vertex * GaugeComponents + g) * SpinorDimension + s].
    /// Length = VertexCount * GaugeComponents * SpinorDimension.
    public Complex[] Values { get; }
    // ctor validates length; GetVertexSpinor(int v) -> ReadOnlySpan<Complex> of one vertex block.
}
```

- Storage: `System.Numerics.Complex[]` (consistent with the spin layer's use of
  `Complex`), vertex-major then gauge then spinor index — matching the
  `[carrier * dim + component]` layout used across the codebase.
- **Gauge index (S(Y) ⊗ V_ρ):** `SpinorRepresentationSpec` states the gauge
  index is tracked separately. Keep `GaugeComponents` with **default 1** (pure
  Clifford spinor) so M2 stays minimal; the multiplier reserves layout space for
  the `⊗ V_ρ` extension without committing to it. **[DEFERRED]** V_ρ gauge
  coupling is out of M2 scope (consistent with the reduced slice, §2.5/decision 3).

### 2.4 Dirac operator — production assembler + shared edge-gamma (RECONCILED)

**RECONCILIATION (dev-spinor's M2 crossed the correction mid-flight; arch-4d
ruling).** dev-spinor shipped a clean standalone `SpinorDiracOperator` in
`Gu.Phase4.Spin` (per my *pre-correction* §2.4). Meanwhile the production discrete
Dirac operator is `src/Gu.Phase4.Dirac/CpuDiracOperatorAssembler.cs` — consumed by
`FermionSpectralSolver`, `DiracVariationComputer`, ~7 phase studies, and many
tests; `SpinorDiracOperator`'s only consumer is its own test file. Dependency
direction: `Gu.Phase4.Dirac → Gu.Phase4.Spin` (and `Gu.Geometry → Gu.Core` only,
so `Spin → Geometry` is clean, no cycle). **Ruling = option (ii): ONE source of
truth for `Γ̂(e)`, the production refinement lands in the assembler, the standalone
survives as a `SpinorField`-native reference that delegates to the shared helper.**

- **Single source of truth (REQUIRED):** extract the edge-gamma contraction into
  one shared helper in `Gu.Phase4.Spin` (co-located with `GammaOperatorBundle`),
  computing the **unit** `Γ̂(e) = Σ_μ (Δx_μ/|e|)·γ^μ = ê·Γ` (physicist §6c). Both
  the assembler and the standalone call it — no second implementation of `Γ̂(e)`.
- **Production path (REQUIRED, load-bearing):** add the opt-in `GammaEdgeScheme`
  to `CpuDiracOperatorAssembler` using the shared helper (below). This is the
  operator the fermion solver actually consumes, so the 4D correction MUST live
  here, not only in the standalone.
- **Standalone (`SpinorDiracOperator`):** KEEP (dev-spinor's 48 tests stay), but
  refactor `FrameGamma` to delegate to the shared helper and adopt the unit
  `ê·Γ` + `/|e|` normalization (it currently uses raw `Δx` with no `/|e|` — see
  the rework note below). Document it explicitly as the `SpinorField`-native
  reference/probe, NOT the production fermion operator (that is the assembler).

The assembler already provides: vertex-centered central difference
`(D_h ψ)_v = Σ_{e=(v,w)} Γ̂(e)·(ψ_w − ψ_v)/|e|`, antisymmetric edge contribution,
per-edge gauge coupling, `DiracOperatorValidator`, mass branch
(`MassBranchTermIncluded`), flat Levi-Civita. M2 applies ONE additive refinement.

**The one refinement (memo §6c, PINNED) — diagonal edges.** The current code sets
`Γ̂(e)` = the gamma of the *dominant* axis `μ = argmax|e_μ|`
(`CpuDiracOperatorAssembler.cs:197,370` `DominantDirection`). On the Freudenthal
mesh most edges are diagonal (differ in ≥2 coords), where `argmax` is an
arbitrary tie-break. Replace it with the **Clifford contraction of the unit edge
vector** — which independently matches the `Γ(e)` I proposed earlier (good
convergence with the physicist):

```
Γ̂(e) = Σ_μ ( (x_w − x_v)_μ / |e| ) · Γ_μ      ( = ê · Γ )
```

This is Hermitian (real combination of Hermitian Riemannian gammas) and
**reduces exactly to the existing single-gamma scheme on axis-aligned edges**.

**BACKWARD-COMPAT (HARD CONSTRAINT):** on the 2D toy the square-splitting
diagonal `(−1,+1)` ties too, so switching `DominantDirection → ê·Γ`
unconditionally WOULD change 2D/3D diagonal-edge outputs and could break existing
Dirac tests. Therefore the refinement is **opt-in**: add a `GammaEdgeScheme`
enum (`DominantAxis` = existing default, `EdgeVectorContraction` = new) threaded
through the assembler; existing callers keep `DominantAxis` and are byte-identical;
the 4D path selects `EdgeVectorContraction`. This preserves all 3169 tests.

- **Conventions RESOLVED (memo §6c):** keep the existing Hermiticity/`i`
  bookkeeping and `DiracOperatorValidator` (`‖D−D†‖/‖D‖ ≤ 1e-10`). Mass-like
  scale reads **directly** as the Dirac eigenvalue
  `sqrt(Re(λ)² + Im(λ)²)` — NOT `sqrt(|λ|)` (first-order operator; project memory
  `phase4-physics`). Mass term (default `m=0`, `MassBranchTermIncluded=false`) is
  a diagonal vertex-local `m·I_spinor`, block-diagonal in gauge index — NOT a
  hopping term. Spin connection: flat Levi-Civita on the uniform lattice; gauge
  coupling per edge via the existing `AddGaugeCouplingContribution`.
- **Recorded boundary:** naive central-difference Dirac carries fermion doublers
  — a pre-existing property of the discretization, not new to 4D; studies flag
  `naiveDiracDoublersPresent = true`.
- Acceptance (structural): (a) `Γ̂(e)` for a unit axis-`μ` edge equals `γ^μ`
  (proves `EdgeVectorContraction` reduces to `DominantAxis` on axis edges);
  (b) `{Γ̂(e),Γ̂(e)} = 2 I` for a unit edge vector; (c) the Hermiticity battery
  is asserted against `i·H_hop` (the massless pure-hopping matrix), which is
  Hermitian to 1e-14 on ANY mesh including `CreateUniform4D(1)` — do NOT assert
  (anti-)Hermiticity of the difference-form `D` on an open mesh (see below);
  (d) all existing `Gu.Phase4.Dirac.Tests` green (default scheme unchanged).

**Rework note for dev-spinor (normalization).** The standalone currently uses
raw `Γ(e) = Σ Δx_μ γ^μ` with no `/|e|`, so on diagonal edges it is `|e|²` times
the physicist's `ê·Γ`-with-`/|e|` convention (identical only on unit axis edges).
Adopt the shared unit `ê·Γ` helper and the `/|e|` stencil so the standalone and
the assembler compute the same `Γ̂(e)`. This changes only diagonal-edge test
constants (the axis-edge and Hermiticity tests are unaffected).

**Hermiticity carrier — dev-spinor's `BuildHoppingMatrix` resolution ENDORSED,
one physics confirmation pending.** dev-spinor correctly identified that the
difference-form `D` (with the on-site `−Γ ψ_v` term) is exactly anti-Hermitian
only when the incident-edge on-site terms cancel — true at interior vertices,
FALSE at boundary vertices of an open lattice — whereas the pure-hopping matrix
`H` (`BuildHoppingMatrix`) is exactly anti-Hermitian on ANY mesh, so `iH` is
Hermitian. `CreateUniform4D(n)` as specified is an OPEN block `[0,n]⁴` with
boundary, so the difference-form `D` is NOT exactly anti-Hermitian there.
Endorsed resolution + **[PHYSICIST-4D CONFIRM]** which carrier the 4D *fermion*
studies consume: recommend either (a) a **periodic 4-torus** `CreateUniform4D`
variant (interior-only ⇒ `D` anti-Hermitian; the memo §2 already floats the
4-torus, `χ=0`), or (b) consume the pure-hopping `H`. NOT a blocker for the first
study, which is **bosonic** (`Upsilon = S_h(F) − T` on ad-forms) and does not
consume the Dirac operator at all.

**Cl(3,1) local re-phase — ruling: KEEP LOCAL.** dev-spinor re-phased the Cl(3,1)
chirality matrix inside `CliffordAlgebraFactory` because the shared
`GammaMatrixBuilder` emits the Riemannian phase. Since Cl(3,1) is a *branch
variant* (Cl(4,0) is primary) and `GammaMatrixBuilder` is shared code other
phases depend on, the local re-phase is the correct **additive** choice — do NOT
modify the shared builder now. Document the local re-phase; revisit fixing the
shared builder only if Cl(3,1) later becomes load-bearing.

**Dependency ruling — `Spin → Geometry` is ACCEPTABLE long-term.** `Gu.Geometry`
references only `Gu.Core`; `Gu.Phase4.Spin → Gu.Geometry` adds no cycle and
matches how `Gu.ReferenceCpu`/`Gu.Phase4.Dirac` already depend on `Gu.Geometry`.
Keep it.

### 2.5 Invariant-basis projector — interface + reduced slice

**RESOLVED (physicist-4d decision 3).** Definition 8.1's full
`[Λ^i(R^{7,7}) ⊗ u(64,64)]^{Spin(7,7)}` invariant basis is **OUT OF SCOPE** for
M2 (it is the 14D `Y_h` program). The **reduced slice** is precisely the
**Spin(4)-invariant elements of `Λ^i(T*X⁴) ⊗ End(S)`** (S = the dim-4 spinor
space). This slice CAN realize the eq 9.3 operator structure on the 6-dim `Λ²`;
it CANNOT touch internal SM content, the weld, the 144, or the electroweak
namespace. **HARD CONSTRAINT (fail-closed):** every 4D study built on this slice
carries `canFillPhase201WzContract = canFillPhase201HiggsContract =
canFillPhase256ObservedFieldExtractionContract = false` **unconditionally** — the
reduced slice is structurally incapable of filling those contracts, and studies
must assert this (mirror the fail-closed boolean wall in `phase436`). Deliver the
*interface* plus the reduced-slice implementation so downstream code has a stable
seam.

```csharp
public interface IInvariantBasisProjector
{
    string ProjectorId { get; }
    int InvariantDimension { get; }                       // dim of the invariant subspace
    /// Project a raw (Λ^i ⊗ spinor-endomorphism) element onto the invariant subspace.
    Complex[] Project(Complex[] rawTensor);
}

// ReducedCliffordSliceProjector : Spin(4)-invariant elements of Λ^i(T*X) ⊗ End(S).
// Scope (decision 3): the Spin(4)-invariant subspace of the spinor endomorphism
// algebra tensored with the exterior powers of the 4D cotangent space — enough
// to realize eq 9.3's Λ² operator structure, NOT the full u(64,64)/Spin(7,7).
// InvariantDimension recorded honestly; XML docs state the recorded boundary and
// that it cannot reach SM/weld/144/electroweak content.
```

- M2 delivers `IInvariantBasisProjector` + `ReducedCliffordSliceProjector` with
  the Spin(4)-invariant `Λ^i(T*X⁴) ⊗ End(S)` scope and a recorded boundary. The
  full Definition 8.1 basis is a named, deferred gap.

### 2.6 M2 acceptance gate

1. All existing tests green; 0 warnings.
2. `CreateClifford4D` for BOTH signatures: `SpinorDimension == 4`, validator
   passes, chirality matrix present.
3. Dirac refinement structural tests 2.4(a–d): `EdgeVectorContraction` reduces to
   the existing `DominantAxis` on axis-aligned edges, satisfies the Clifford
   relation, passes `DiracOperatorValidator` Hermiticity on `CreateUniform4D(1)`,
   and — critically — **all existing `Gu.Phase4.Dirac.Tests` stay green with the
   default `DominantAxis` scheme** (opt-in refinement, zero behavior change).
4. `SpinorField` layout round-trips (`GetVertexSpinor` ⟷ flat index), or the
   reused existing type is documented.
5. `ReducedCliffordSliceProjector` idempotent (`Project∘Project == Project`) and
   `InvariantDimension` matches its documented Spin(4)-invariant scope.
6. XML docs on the projector and Dirac refinement state the reduced-slice
   boundary and every convention default (mass, `i`, doublers).

---

## 3. M3 — Einsteinian Shiab operator family

**Owner:** dev-shiab. **Projects:** `Gu.ReferenceCpu` (operator),
`Gu.Branching` (no code change — registration is data-driven via the study).
**Risk:** HIGH (physics under-determination). **Hard dep:** M1.

### 3.1 What the operator is, discretely

`IShiabBranchOperator` maps a curvature-2-form to a 2-form with
`OutputCarrierType = "curvature-2form"`, `Degree = "2"`, `ComponentOrderId =
"face-major"`, ad-valued, **face-valued** (coefficients length
`FaceCount * dimG`). This carrier is fixed and MUST match the torsion output
(enforced by `BranchOperatorRegistry.ValidateCarrierMatch`). M3 does **not**
change the carrier — it changes the *content* of the face→face map.

On dimX = 2, any carrier-preserving linear Shiab is a scalar multiple of
identity (Λ² is 1-dim per point) — that is the documented 2D blocker in
`ShiabFamilyScopeChecker`. On dimX = 4, Λ²(T*X) is **6-dimensional** per point,
and a pentachoron has C(5,3) = 10 triangular faces spanning that 6-dim space.
The Einsteinian Shiab is a **per-cell linear contraction on Λ²** that mixes a
cell's face-values through the metric — the first genuinely richer Shiab family,
which is exactly why it can only be exercised once M1 exists.

### 3.2 SETTLED: output degree = 2-form (curvature-2form carrier preserved)

This is the interface decision physicist-4d delegated to the architect
(memo §4.4: "eq 9.3 maps Ω² → Ω^{d-1}; on the 4D base that's a 3-form, but
`IShiabBranchOperator` hard-codes `OutputCarrierType="curvature-2form"`… pin
this before dev-shiab codes").

**DECISION: keep the 2-form carrier. The 4D Einsteinian Shiab is realized as a
degree-preserving Ω²→Ω² map; NO new carrier, NO new torsion target, NO degree
reduction wrapper. Output `Degree="2"`, `CarrierType="curvature-2form"`,
`ComponentOrderId="face-major"`, face-valued — byte-identical to
`IdentityShiabCpu.OutputSignature`.**

Why this is the correct realization (not merely the convenient one), reconciling
the memo's own physics:

1. **The two pinned ingredients are both degree-preserving on Ω².** The Phi menu
   `{id0, sd2(P_+), asd2(P_-), vol4}` are projectors of the curvature operator
   `R: Λ² → Λ²` onto its irreducible pieces (scalar/Ricci-trace, self-dual Weyl,
   anti-self-dual Weyl, volume/trace) — all **Ω²→Ω² endomorphisms**. The
   eps-conjugation `F ↦ g(ω)·F·g(ω)⁻¹` (with `g` built from
   `B_ω = ∇₀ + eps⁻¹ d₀ eps`) is a similarity transform — also **degree-preserving**.
2. **Quarticity does NOT require a degree change.** physicist-4d's headline
   warning: a linear eq 9.3 with `eps=1` stays linear in `F`, so `Upsilon` stays
   degree-2 in `ω` and the Hessian stays degree-2 in `t` (it reproduces the toy
   no-go on a bigger mesh). Degree > 2 appears **only** through the
   ω-dependence of the eps-conjugation (draft:2084) and/or the `d_{B_ω}` term of
   the differential canonical branch (v26:1601–1609). That mechanism is a
   degree-preserving *nonlinearity in ω*, not a jump to Ω³. So Ω³ output is
   **not needed** to reach the physics the program cares about.
3. **The control arm is then LITERALLY the existing pipeline.** `(id0, eps=1)` +
   `TrivialTorsionCpu` reproduces `IdentityShiabCpu`/Phase436 exactly — a clean
   validation gate — because the carrier, torsion, mass matrix, and residual
   assembler are all unchanged.
4. **Recorded boundary.** Landing in Ω² is the exact Ω²→Ω² curvature-operator
   core of eq 9.3's Ω^{d-1} content, consistent with M2's reduced-slice scope
   (§2.5). The projection away from the outer degree-raising `D_A`/Hodge leg is
   documented, not hidden — studies record `shiabOutputDegree = 2` and
   `draftDegreeReductionRecorded = true`.
5. **Reversible/extensible.** If physics later demands the honest Ω³ output, M1
   already ships `Volumes` + `FaceToVolume` (§1.5); a 3-form carrier + 3-form
   torsion can be added additively with zero rework of the 2-form path. That
   escape hatch is why M1 built the volume layer even though M3 does not consume
   it yet.

*(Confirmation requested from physicist-4d: that the eps-conjugation / Weyl-
selection realization is intended as the Ω²→Ω² core, i.e. the outer degree-raising
leg is out of scope for the first realization. Design proceeds on this basis.)*

### 3.3 The pinned eq 9.3 shape and its Ω²→Ω² (Λ²) realization

physicist-4d PINS the exact two-term operator (memo §4, `draft:2126-2129`),
acting on the ad-valued 2-form `ξ = F`:

```
S_h(ξ) =  [ (eps⁻¹ Φ₁ eps) ∧ (∗ξ) ]                              <- "Ricci-like"
       − c · [ (eps⁻¹ Φ₁ eps) ∧ ∗[ (eps⁻¹ Φ₂ eps) ∧ (∗ξ) ] ]    <- "Ricci-scalar-like"
```

PINNED exactly: the two subtractive terms, the `∗ξ` inside the first bracket and
the **nested double-Hodge** `∗[ … ∧ (∗ξ) ]` in the second, and the
`eps⁻¹ Φ eps` conjugation on every `Φ`. `Φ₁` appears in both terms; `Φ₂` only in
the second. `Φ₁,Φ₂` are single invariant elements from the reduced-slice menu
(§2.5), NOT a free weighted sum.

**Realization = the curvature-operator core on Λ² (this is the settled §3.2
carrier decision, and it is exactly what the physicist's batteries test).** In
Euclidean 4D the Hodge star `∗: Λ² → Λ²` is a 6×6 involution (`∗²=+1`) with real
±1 eigenspaces `Λ²₊ ⊕ Λ²₋` (3+3). Each `Φ` becomes a Λ²-endomorphism
`A(Φ)` per ad-component (`id0`→ scalar·identity; `sd2`→ P₊; `asd2`→ P₋; `vol4`→
the `∗`-scaling / parity element). The operator is then a composition of Λ²→Λ²
maps, so `S_h(ξ)` stays a 2-form — and batteries 1 (richness: "matrix on
`Λ²⊗ad` not proportional to identity") and 2 (Weyl-annihilation: "feed a pure
Weyl 2-form, assert `S_h` → 0") are literally statements about this 6×6-per-ad
matrix, confirming the Ω²→Ω² reading is the intended one.

```
Evaluate(F_faces, ω, manifest, geom):
  eps = BuildEps(spec.EpsilonMode, ω, background)     // §3.5; identity/frozen/omega-coupled
  for each cell (pentachoron):
    faces = mesh.CellFaces[cell]                        // 10 faces span the 6-dim Λ²
    // 1. face 2-vector B_face = edge_a ∧ edge_b in R^4  → assemble W (6 x 10); build ∗ (6x6).
    // 2. lift F_faces (ad-valued) into the cell's Λ² rep via W.
    // 3. build A1 = eps⁻¹ A(Φ₁) eps,  A2 = eps⁻¹ A(Φ₂) eps   (ad-conjugation per §3.5)
    //    term1 = A1( ∗ ξ )
    //    term2 = A1( ∗ ( A2( ∗ ξ ) ) )      // exact double-Hodge nesting
    //    S = term1 − c · term2               // c = einsteinCoefficient; bracketType per spec
    // 4. project back to faces (W⁺ least-squares), scatter, average faces shared across cells.
  return face-valued ad-2-form, OutputSignature identical to IdentityShiabCpu.
```

The metric enters through `B_face` and `∗`; decision 1 fixes Cl(4,0) precisely
so the SD/ASD split (hence `sd2`/`asd2`) is real.

### 3.4 FAMILY-MEMBER SPEC — verbatim from memo §4.4 **[FAMILY]**

No canonical member (physicist-4d Q3): this is a Phase441-style universality
sweep over the Cartesian product. Fields match memo §4.4 exactly:

```csharp
public sealed class InvariantElementSpec           // phi1 / phi2
{
    public int FormDegree { get; init; }                       // 0, 2, or 4
    public string InvariantElement { get; init; } = "id0";     // "id0"|"sd2"|"asd2"|"vol4"
}

public sealed class EinsteinianShiabFamilyMember
{
    public InvariantElementSpec Phi1 { get; init; } = new();   // both terms
    public InvariantElementSpec Phi2 { get; init; } = new();   // second term only
    /// Einstein coefficient c. PINNED default 0.5; sweep {0, 0.5, 1} (c=0 = one-term control).
    public double EinsteinCoefficient { get; init; } = 0.5;
    /// PINNED default "commutator" (matches existing [ω,ω]); sweep adds "i-anticommutator".
    public string BracketType { get; init; } = "commutator";
    /// THE SCALE-BREAKING LEVER (memo §0/§4.3). Three modes; see §3.5.
    ///   "trivial"          eps=1 — CONTROL, linear in ω, degree-2 Hessian (reproduces the no-go)
    ///   "frozen-background" eps from a FIXED background field — still LINEAR in ω
    ///   "omega-coupled"    eps carries connection DOF — NONLINEAR in ω; only this can lift degree>2
    public string EpsilonMode { get; init; } = "trivial";
    /// Derived, e.g. "einsteinian-shiab/sd2-id0/c0.5/comm/omega-coupled".
    public string BranchId => $"einsteinian-shiab/{Phi1.InvariantElement}-{Phi2.InvariantElement}" +
                              $"/c{EinsteinCoefficient}/{Abbrev(BracketType)}/{EpsilonMode}";
}
```

- **Control member** `{Phi1=id0, Phi2=id0, EpsilonMode="trivial"}` reduces to a
  scalar multiple of `F` and MUST reproduce the toy no-go (§3.6 battery 1 expects
  the control to *fail* the richness certificate — the correct outcome).
- **THE ONE THING DEVS MUST INTERNALIZE (memo §0, carried verbatim):** the 4D
  base is necessary but NOT sufficient. eq 9.3 is *linear in F*; with
  `EpsilonMode ∈ {trivial, frozen-background}` the whole map is linear in ω, so
  `Upsilon` stays degree-2 in ω and the Hessian stays degree-2 in `t` — this
  **reproduces the Phase436/441 no-go on a bigger mesh.** Degree > 2 is reachable
  **only** through the ω-dependence turned on by `EpsilonMode="omega-coupled"`.
  Build and test toward `omega-coupled`; keep `trivial` as the validation control.
- The M3 sweep pairs this menu with the M1 torsion and A0 menus exactly as
  Phase441; the registry `ValidateCarrierMatch`es every member.

### 3.5 The `eps` dressing per mode + `Linearize` contract

`eps` is an `H`-valued (reduced-slice: `End(S)`/`ad`-valued) field on the mesh;
the conjugation `eps⁻¹ Φ eps` is a similarity transform — **degree-preserving**,
which is why it can supply ω-nonlinearity without changing the 2-form carrier.

| `EpsilonMode` | `eps` source | linear in ω? | `Linearize` |
|---|---|---|---|
| `trivial` | `eps = 1` | yes | analytic: `dS/dω(δ) = S̃(dF/dω(δ))`, `S̃` = the (const) Λ² composition; `dF/dω(δ)=d(δ)+[ω,δ]` reused from `IdentityShiabCpu.Linearize`. Exact Jacobian, degree-2. |
| `frozen-background` | `eps = eps(bg)`, a FIXED field passed to the ctor, independent of the live ω | yes | analytic, same as trivial but with the constant `eps`-conjugated `S̃`. Still degree-2. |
| `omega-coupled` | `eps = eps(ω)` carries connection DOF (sigma-field, `draft:2083-2084`) | **no** | see below |

**`omega-coupled` Linearize (the nonlinear mode).** `S = S̃_{eps(ω)}(F(ω))` with
BOTH `F` and `eps` depending on ω:
`dS/dω(δ) = S̃_{eps}(dF/dω(δ)) + (∂S̃/∂eps · dEps/dω(δ))(F)`.
The second term is the extra piece **absent when eps=1** — it is the mechanism
that lifts the Hessian degree (physicist-4d §6d makes its non-vanishing the
sign-off criterion).
- **Fallback (PINNED for the first pass): finite-difference `Linearize`,
  Phase436-style.** Until the discrete `dEps/dω` is pinned (below), implement
  `Linearize` for `omega-coupled` as the central difference of `Evaluate`
  (exactly `phase436`'s `JacobianColumn`: `(S(ω+hδ) − S(ω−hδ))/2h`). This is
  exact-enough for the Hessian-degree probe (which itself finite-differences) and
  removes the analytic-derivative risk from the critical path.
- **Proposed concrete discrete `eps(ω)` for physicist-4d §6d sign-off
  (CANDIDATE, not yet pinned):** an `H`-valued vertex/cell field
  `eps_cell = exp( κ · Σ_{e∈cell} ω_e )` (a Wilson-line-like accumulation of the
  connection over the cell's edges), coupling `κ`; `κ=0` recovers `eps=1`. This
  makes `eps` genuinely carry connection DOF and gives `dEps/dω(δ) = κ · eps ·
  (Σ_e δ_e)` for the analytic form. **dev-shiab must NOT trust the omega-coupled
  arm until physicist-4d confirms this discrete `eps` map (memo §6d): the check
  is that `dEps/dω ≠ 0` feeds `dS/dω` a term that vanishes at `eps=1`.**

### 3.6 Class contract

```csharp
public sealed class EinsteinianShiabOperator : IShiabBranchOperator
{
    public EinsteinianShiabOperator(SimplicialMesh mesh, LieAlgebra algebra,
                                    EinsteinianShiabFamilyMember member,
                                    /* frozen-background eps (null unless EpsilonMode="frozen-background") */
                                    double[]? backgroundEps = null);
    // BranchId => member.BranchId (e.g. "einsteinian-shiab/sd2-id0/c0.5/comm/omega-coupled");
    // OutputCarrierType => "curvature-2form";
    // OutputSignature => identical to IdentityShiabCpu.OutputSignature (carrier match REQUIRED).

    public FieldTensor Evaluate(FieldTensor curvatureF, FieldTensor omega,
                                BranchManifest manifest, GeometryContext geometry);
    public FieldTensor Linearize(FieldTensor curvatureF, FieldTensor omega,
                                 FieldTensor deltaOmega, BranchManifest manifest,
                                 GeometryContext geometry);
}
```

**`OutputSignature`** must be byte-identical to the torsion operator it pairs
with (all fields; `Degree="2"`, `ComponentOrderId="face-major"`,
`CarrierType="curvature-2form"`, …). Copy `IdentityShiabCpu.OutputSignature`
exactly; QA runs `BranchOperatorRegistry.ValidateCarrierMatch`.

**`Linearize` is dispatched by `EpsilonMode` per the §3.5 table:** analytic exact
Jacobian for `trivial`/`frozen-background` (both linear in ω, degree-2 residual);
Phase436-style finite-difference fallback for `omega-coupled` (nonlinear in ω,
degree can exceed 2). Do not duplicate the contract here — §3.5 is authoritative.

### 3.7 M3 acceptance batteries — memo §4.5, verbatim five (qa-4d enforces)

1. **Richness certificate (THE HEADLINE).** For any member with a nontrivial `Φ`
   (e.g. `sd2`), assert `S_h(F) ≠ c·F` for every scalar `c`: the operator's
   matrix on `Λ²⊗ad` is **not** proportional to identity (eigenvalue spread /
   off-identity Frobenius norm above a floor) on `CreateUniform4D(1)`. This is
   exactly the capability the 2D toy provably lacks (`ShiabFamilyScopeChecker`
   blocked-reason 1). **The control member `(id0,id0,trivial)` MUST FAIL this**
   (it *is* a scalar multiple of `F`) — that failure is the correct, expected
   control outcome, not a bug.
2. **Weyl annihilation** (`draft:2133`). Feed a pure Weyl-like 2-form (the
   `Λ²` part orthogonal to the scalar/traceless-Ricci directions the chosen `Φ`
   reaches) and assert `S_h` sends it to zero / projects it out to tolerance. If
   the operator does not annihilate the Weyl part, it is not the Einsteinian
   contraction.
3. **Carrier-signature identity:** `S_h.OutputSignature == T.OutputSignature`
   (strict, all fields).
4. **Linearization / Hessian symmetry:** analytic `Linearize` matches the
   finite-difference of `Evaluate` (Phase436 machinery) for `trivial` and
   `frozen-background`; for `omega-coupled` the FD `Linearize` IS the reference.
   Assemble `H` (via `CpuLocalJacobian`/`CpuMassMatrix` with
   `LieAlgebraFactory.CreateSu2WithTracePairing()`, positive-definite — project
   memory) and assert `‖H − Hᵀ‖ / ‖H‖` below floor (self-adjointness, IX.32.2.1).
5. **Gauge covariance** under `eps`-dressing: `S_h(Ξ·h) = S_h(Ξ)·h` to first
   order (`v26:1661-1667`; physics-guidance falsifier 5).

Plus regression: all existing tests green, 0 warnings.

### 3.8 Consumer-contract preservation (must not regress the study surface)

The physics studies consume the backend exactly as `phase436` does:
`new CpuSolverBackend(mesh, algebra, torsion, shiab)` then
`backend.EvaluateDerived(ω, a0, manifest, geometry)` →
`derived.ResidualUpsilon`. `EinsteinianShiabOperator` satisfies
`IShiabBranchOperator` with the identical carrier, so it drops into
`CpuResidualAssembler` / `CpuSolverBackend` with **no change to the consumer
contract**. A study can then set `mesh = CreateUniform4D(n)` (or
`CreateStructuredFiberBundle4D(n).AmbientMesh`) and
`shiab = new EinsteinianShiabOperator(mesh, algebra, member)` and run the existing
degree-2/decomposition/flatness batteries in 4D. Preserving this seam is a hard
requirement and is covered by §3.7 battery 3 (carrier identity) + battery 4
(Jacobian plugs into the existing assembler).

### 3.9 First physics study (task #12 handoff) — 4D Hessian-degree probe

physicist-4d decision 5 defines the first study: **"4D control-vs-Einsteinian
Hessian-degree probe."** It is the natural terminus of M1–M3 and the acceptance
demonstration that the platform delivers a genuinely richer Shiab than the 2D
toy. Design contract:

- **Reuse `phase436` machinery verbatim:** the exact-Hessian finite-difference
  harness (`JacobianColumn`, `UpsilonHessianElement`, the `t`-difference
  decomposition) applied on `CreateUniform4D(n)` (or
  `CreateStructuredFiberBundle4D(n).AmbientMesh`), `su(3)` (or `su(2)`) algebra.
- **Control arm** `{Phi1=id0, Phi2=id0, EpsilonMode="trivial"}` + `TrivialTorsionCpu`
  **MUST reproduce Phase436's degree-2 result** (third/fourth `t`-difference of
  `H(t)` vanishes; `[u,u]=0` on a single-direction ray). This is the validation
  gate — if the control arm is not degree-2, the 4D backend is wrong, not the
  physics. (It must also FAIL the §3.7 richness certificate — the expected
  control outcome.)
- **Einsteinian arm** `{Phi1∈{sd2,asd2}, EpsilonMode="omega-coupled", …}` measures
  whether `H(t)` acquires degree > 2 (nonzero third/fourth `t`-difference). Per
  memo §5 both verdicts are legitimate and publishable: `hessianRemainsDegreeTwo`
  (theorem extends; sharpens frontier onto the differential `Σ_mc` term) or
  `hessianDegreeExceedsTwo` (necessary condition for a scale met on a
  draft-canonical operator for the first time — a candidate mechanism, NOT a
  scale, NOT a promotion). **Nothing is promoted either way.**
- **Fail-closed wall (mandatory, copy `phase436`):** target-blind construction,
  no scales/poles/GeV lineage, `draftAlignmentStatus = surrogate`,
  `shiabOutputDegree = 2`, `draftDegreeReductionRecorded = true`, plus the full
  recorded-boundary key set the physicist enumerated (memo §3/§6b/§6c):
  `canFillPhase201WzContract = canFillPhase201HiggsContract =
  canFillPhase256Contract = false` (unconditionally),
  `definition81Scope = reduced-spin4-slice`, `ambientSevenSevenRealized = false`,
  `internalGaugeContentRealized = false`, `weldRealized = false`,
  `baseSignature = Cl(4,0)-euclidean-slice`, `lorentzianBaseNotYetRealized = true`,
  `geometricFiberTrivial = true`, `spinorUsedAsShiabCarrierNotFiber = true`,
  `draft14dObserverseNotRealized = true`, `noKaluzaKleinTowerModeled = true`,
  `naiveDiracDoublersPresent = true`. The probe's `...Passed` gate keys on
  internal consistency only (control arm reproduces Phase436; §3.7 batteries
  green) and passes regardless of how the degree verdict falls.
- **Scanner registration (memo §5):** register the new study doc and any
  `IMPLEMENTATION_P4xx.md` in the broad text-scanner exclusion lists in the same
  checkpoint (restart-prompt SCANNER-REGISTRATION HAZARD).
- Lives in `studies/phaseNNN_4d_control_vs_einsteinian_hessian_degree_probe_001/`
  (next phase number at handoff time), following the existing study `Program.cs`
  layout. Owned by the physics program, not the platform devs; qa-4d validates
  the control-arm reproduction gate.

---

## 4. Physics decisions — RESOLVED (folded in from physicist-4d memo)

All five previously-open items are resolved; the memo
(`FOUR_D_PLATFORM_PHYSICS_DECISIONS.md`) is authoritative for physics content.

1. **Signature** → PRIMARY **Cl(4,0)** (Euclidean, `*²=+1` real SD/ASD split),
   Cl(3,1) a parameterized branch variant. §2.2.
2. **Fiber/ambient policy** → first 4D studies use the **trivial fiber**
   (dimX = dimY = 4, `fiberSize=1`); the `fiberSize`/higher-`dimY` path toward
   14D is a parameterized future extension. §1.6.
3. **Definition 8.1 scope** → reduced slice = **Spin(4)-invariant
   `Λ^i(T*X⁴) ⊗ End(S)`**; full `u(64,64)/Spin(7,7)` out of scope;
   `canFill*` = false unconditionally. §2.5.
4. **Eq 9.3 family** → two-term shape / double-Hodge / eps-conjugation pinned;
   Phi menu `{id0, sd2, asd2, vol4}`, `EinsteinCoefficient` c (default 0.5),
   `BracketType`, `EpsilonMode` as the quarticity switch. §3.4.
5. **Dirac convention** → edge weight `w_e`, mass, and `i`/Hermiticity are
   constructor parameters with documented defaults (`w_e=1`, `m=0`); the
   frame-contraction `Γ(e)=Σ Δx_μ γ^μ` handles diagonal Freudenthal edges. §2.4.

**One interface decision the architect settled (§3.2):** the 4D Einsteinian
Shiab output stays a **2-form** (curvature-2form carrier preserved) — a
degree-preserving Ω²→Ω² realization, no new carrier/torsion. Confirmation
requested from physicist-4d that the outer degree-raising leg of eq 9.3 is out
of scope for the first realization; design proceeds on this basis.

---

## 5. Test-project layout & conventions

- New tests go in the existing per-project test projects:
  `tests/Gu.Geometry.Tests` (M1), `tests/Gu.Phase4.Spin.Tests` (M2),
  `tests/Gu.ReferenceCpu.Tests` (M3), with new files suffixed `4DTests.cs`.
- No new test projects. No new solution entries.
- Each milestone lands as its own small additive commit on `main` once its
  acceptance gate is green; qa-4d signs off per gate before the next dependent
  milestone starts.
