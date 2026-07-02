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
- **Boundary-manifold invariant (asserted) — VOLUMES only (codim-1).** Every
  interior volume (3-subsimplex) is shared by exactly 2 pentachora; every boundary
  volume by exactly 1. (Compute a volume→cell incidence multiplicity histogram;
  assert values ∈ {1,2} and the boundary count matches the 4·(surface)
  expectation.) **The `==2` check is codim-1 (VOLUMES) ONLY** — do NOT assert it
  for faces: face (codim-2) multiplicity is dimensionally forced into `{4,6}` on
  the Freudenthal 4-lattice, not 2 (confirmed with dev-mesh; the earlier
  "==2-for-faces" phrasing was an assignment error). The manifold-closedness
  reading is the volume-level check.
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
truth for `Γ̂(e)`; the `ê·Γ` refinement lands in the production assembler; the
standalone `SpinorDiracOperator` is KEPT as a `SpinorField`-native reference that
DELEGATES to the shared contraction.** This is the CERTIFIED M2 state (committed
`7e6b7c64`) and the ratified design.

> **MILESTONE-FREEZE NOTE (team-lead, standing rule).** A later option-(i) rework
> (delete the standalone) was attempted post-certification; the lead REVERTED it
> because M2 was already a banked/committed milestone. Once a milestone is
> announced COMMITTED, its files are FROZEN — changes go through a leader-approved
> change request with re-certification, never a direct edit. Option-(ii) stands.
> (My earlier note approving the option-(i) deletion was a post-certification
> change that bypassed this gate — corrected here.)

- **Standalone kept** (`SpinorDiracOperator`, dev-spinor's tests stay green),
  refactored to DELEGATE to the single `ê·Γ` contraction — no second implementation.
  Documented as the `SpinorField`-native reference, NOT the production operator.
- **Production path (REQUIRED, load-bearing):** the `ê·Γ` refinement lands in
  `CpuDiracOperatorAssembler` via the opt-in `GammaEdgeScheme`, the unit
  `Γ̂(e) = Σ_μ (Δx_μ/|e|)·γ^μ` (physicist §6c). This is the operator the fermion
  solver actually consumes. The `GammaEdgeScheme` enum lives in `Gu.Phase4.Dirac`
  (next to the assembler; `Dirac → Spin`).
- **`SpinorField` stays** (verified: `Gu.Phase4.Dirac` has NO typed vertex-spinor
  class). dev-spinor's `ToFlat`/`FromFlat` bridge to the assembler's
  `2*(v*dofsPerCell+g*spinorDim+s)` layout is invited as a **proper post-build
  delta via a leader-approved change request** (team-lead), not an in-milestone edit.

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

**Normalization note.** `EdgeVectorContraction` must use the **unit** `ê·Γ` with
the `/|e|` stencil (physicist §6c), so on axis-aligned edges it reduces EXACTLY to
the existing `DominantAxis` single-gamma scheme (that exact reduction is what keeps
the `DominantAxis` default byte-identical and is a §2.4 acceptance check). Do NOT
use raw `Σ Δx_μ γ^μ` (it is `|e|²` off on diagonal edges). Both the assembler's
`EdgeVectorContraction` and the delegating standalone use the same unit `ê·Γ`.

**Hermiticity carrier — RESOLVED (physicist-4d): (a) PERIODIC 4-torus for fermion
studies; open block for the bosonic probe.** dev-spinor correctly identified that
the difference-form `D` (with the on-site `−Γ ψ_v` term) is exactly anti-Hermitian
only when the incident-edge on-site terms cancel — true at interior vertices,
FALSE at boundary vertices of an open lattice; the pure-hopping `H`
(`BuildHoppingMatrix`) is anti-Hermitian on ANY mesh. **Decision:** the fermion
spectral studies consume a **periodic 4-torus** `CreateUniform4D` variant — every
vertex is interior ⇒ `Σ_q ê_{pq}=0` everywhere ⇒ the FULL difference-form `D`
(on-site term included) is exactly anti-Hermitian, `iD` Hermitian to machine
precision, so the existing `DiracOperatorValidator` (`‖D−D†‖/‖D‖≤1e-10`) stays
honest with NO special-cased operator and no 2D/4D code divergence. Rationale
(physicist): it is the standard naive-lattice-Dirac setup (translation-invariant,
clean momentum-space spectrum `~Σ_μ γ_μ sin k_μ`); `χ=0`; periodicity preserves
the `∂∘∂=0` combinatorial structure (the 4-torus is a valid oriented complex, so
QA's exact-integer `B1·B2=0`, `B2·B3=0` still hold); consistent with the
Euclidean-slice (§1) and the boson program's fermion-loop studies. The
pure-hopping `H` is the fallback only if periodic meshing is too costly for the
timeline (it imposes an implicit Neumann-like BC — a different operator). Record
`baseSignature=Cl(4,0)-euclidean-slice`, `fermionCarrier=periodic-4-torus`,
`naiveDiracDoublersPresent=true` (standard `2⁴=16` doublers on the torus — a
recorded property, out of scope to remove; would need Wilson/staggered). Optional
(noted, not required for the first pass): anti-periodic BC in one direction lifts
the trivial `k=0` zero mode.
- **M1b MESH ITEM (dev-mesh, in flight) — periodic `CreateUniform4D` variant.**
  Identify opposite faces of `[0,n]⁴` (edges/faces/volumes wrap). Additive
  follow-on (M1 otherwise complete): the **fermion path consumes the periodic
  mesh; the bosonic Hessian-degree probe consumes the open block** — the two
  coexist. Not a blocker for M2's Hermiticity acceptance (uses dev-spinor's
  `i·H_hop`-on-open-`CreateUniform4D(1)` now); full-`D`-on-torus is the
  fermion-carrier check once the variant lands. **dev-mesh corrections ENDORSED
  and pinned here:**
  - **Minimum `n=3` (PINNED).** `n=2` degenerates (simplex-translate collisions;
    `χ=200`, not `0`) — `CreateUniform4DPeriodic` throws
    `ArgumentOutOfRangeException` below `n=3`. Recorded as a real constraint.
  - **Golden orbit counts (per lattice point): `1 / 15 / 50 / 60 / 24`**
    (vertices/edges/faces/volumes/pentachora per site), giving `χ = 0` (`T⁴`, as
    expected for the 4-torus). QA asserts these + `χ=0`; the `∂∘∂=0` integer
    checks (`B1·B2=0`, `B2·B3=0`) still hold (valid oriented complex).
  - **Minimal-image convention for wrapped edges (PINNED).** A wrapped edge's
    coordinate difference is `n−1` in the naive reading; the frame-contracted
    Dirac `Γ̂(e)=ê·Γ` MUST use the **minimal-image** edge vector (period `n`) so
    the momentum-space spectrum is the clean `Σ_μ γ_μ sin k_μ`. Documented in the
    variant's XML remarks.
  - **Backlog (change request, later):** dev-mesh deliberately did NOT add a
    `Period` field to `SimplicialMesh` (additive-minimal). If future fermion work
    needs the period exposed on the mesh, that is a leader-approved change request,
    not an in-milestone edit. Noted as backlog.

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
   (§2.5). Studies record `shiabOutputDegree = 2`,
   `draftOperatorIsDegreeRaising = true`,
   `reducedRealizationCapturesRicciWeylAlgebraNotFormDegree = true`,
   `draftDegreeReductionRecorded = true`.
5. **Reversible/extensible.** If physics later demands the honest Ω³ output, M1
   already ships `Volumes` + `FaceToVolume` (§1.5); a 3-form carrier + 3-form
   torsion can be added additively with zero rework of the 2-form path. That
   escape hatch is why M1 built the volume layer even though M3 does not consume
   it yet.

**CONFIRMED by physicist-4d (memo §6e), with the rigor:** the draft operator IS
genuinely degree-raising — eq 9.2 (`draft:2121`) maps Ω²(Y)→Ω^{d-1}(Y), and the
degree bookkeeping forces `Φ₁` to be a **1-form** (`Φ₁ ∧ *ξ`, with `*ξ` a
(d−2)-form, must land in Ω^{d-1}). BUT the degree-raising Hodge/wedge legs are
**linear in `ξ=F` and carry ZERO ω-dependence**, so they are irrelevant to the
Hessian-degree question. The eps-conjugation (degree-preserving similarity on the
ad-value) plus the `Λ²→Λ²` projectors carry ALL the ω-dependence and ALL the
richness. So Ω²→Ω² is not a compromise — it is the correct minimal realization
that isolates exactly the physics the probe measures, and makes the control arm
literally the existing identity-Shiab pipeline. The honest Ω³ + 3-form-torsion
realization is a legitimate LATER milestone (the M1 escape hatch), deferred.

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
EvaluateWithTheta(F_faces, ω, θ, manifest, geom):     // θ explicit param (VertexCount*dimG); θ=0 ⇒ ε=1
  Ad_v = exp(ad_{θ_v})   per VERTEX    // ε an H-valued gauge 0-form on vertices. §3.5
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
    /// THE SCALE-BREAKING LEVER (CO-SIGNED §3.5). ε carries θ, an INDEPENDENT
    /// H-valued DOF (θ per VERTEX). Physicist-4d co-signed 5-mode taxonomy (memo §6e):
    ///   "trivial"                 θ=0 (ε=1) — CONTROL, degree-2 (reproduces the no-go)
    ///   "frozen-background"       θ fixed ≠ 0, not varied — diagnostic, degree-2
    ///   "slaved-wilson-smoketest" (mode 1) θ=κ·Σω — POSTULATED f(ω); optional NON-GATING smoke-test
    ///   "epsilon-dynamical"       (mode 2) θ VARIED, joint (ω,θ) Hessian, NO stationarity solve
    ///                             — THE phase442 HEADLINE / study instrument; degree > 2 possible
    ///   "variational"             (mode 3) ε*(ω) via ∂S/∂ε=0, integrated out — DEFERRED upgrade
    ///                             (where the eventual SCALE extraction lives; costly implicit-fn deriv)
    public string EpsilonMode { get; init; } = "trivial";
    /// κ — ONLY used by the slaved-wilson-smoketest mode (non-gating). Ignored otherwise.
    public double SlavedWilsonKappa { get; init; } = 0.0;
    /// Derived, e.g. "einsteinian-shiab/sd2-id0/c0.5/comm/epsilon-dynamical".
    public string BranchId => $"einsteinian-shiab/{Phi1.InvariantElement}-{Phi2.InvariantElement}" +
                              $"/c{EinsteinCoefficient}/{Abbrev(BracketType)}/{EpsilonMode}";
}
```

- **Control member** `{Phi1=id0, Phi2=id0, EpsilonMode="trivial"}` reduces to a
  scalar multiple of `F` and MUST reproduce the toy no-go (§3.6 battery 1 expects
  the control to *fail* the richness certificate — the correct outcome).
- **THE ONE THING DEVS MUST INTERNALIZE (memo §0):** the 4D base is necessary but
  NOT sufficient. eq 9.3 is *linear in F*; with `EpsilonMode ∈ {trivial,
  frozen-background}` the whole map is linear, so the Hessian stays degree-2 in
  `t` — **reproducing the Phase436/441 no-go on a bigger mesh.** Degree > 2 is
  reachable **only** with `EpsilonMode="epsilon-dynamical"` (mode 2 — the varied θ
  DOF over the joint `(ω, θ)` Hessian, no stationarity solve). Build and test toward
  `epsilon-dynamical`; keep `trivial` as the validation control.
  (`slaved-wilson-smoketest` is a non-gating diagnostic only — never the study
  instrument; `variational` (mode 3) is the deferred scale-extraction upgrade.)
- The M3 sweep pairs this menu with the M1 torsion and A0 menus exactly as
  Phase441; the registry `ValidateCarrierMatch`es every member.

### 3.5 The `ε` dressing + `Linearize` contract — CO-SIGNED & RATIFIED (independent ε)

> **STATUS: CO-SIGNED & RATIFIED — freeze CLOSED (2026-07-02).** physicist-4d
> (physics) + arch-4d (architecture). physicist-4d ratified the independent-θ §3.5
> **as written** to team-lead ("This is my settled, final position — closed"),
> postdating every earlier slaved fragment; committed at `dc7ddd36`. The binding
> decision is **(B) INDEPENDENT ε**.
>
> **PINNED:** ε carries a `θ` field that is an **independent `H`-valued DOF** (NOT
> a function of ω); the Hessian is over the **joint `(ω, θ)`**; carrier stays
> **Ω²→Ω²**; the slaved Wilson form is demoted to an optional labelled NON-GATING
> smoke-test (`EpsilonMode="slaved-wilson-smoketest"`). Record keys:
> `epsilonRealization=independent-theta-dof`, `hessianOverJointOmegaTheta=true`,
> `slavedWilsonKeptAsSmokeTestOnly=true`, `shiabOutputDegree=2`.
>
#### PHYSICS HALF (authored verbatim by physicist-4d; source of truth: `FOUR_D_PLATFORM_PHYSICS_DECISIONS.md` §6e)

**Why ε is an independent field, and why the joint (ω,θ) Hessian is the object.**
In GU the bosonic connection on Y is the inhomogeneous-gauge-group element
ω_Y = (ε, ϖ), where the group is the semidirect product G = H ⋉ N (draft
Definition 5.1, line 1617): H is the gauge group and N = Ω¹(ad) the ad-valued
1-forms. In a semidirect product the two factors are independent coordinates —
ε ∈ H and ϖ ∈ N are separate dynamical fields, not one a function of the other.
The draft uses this independence directly: the augmented torsion is
T_ω = ϖ − ε⁻¹d₀ε (draft line 2200), and the Shiab dresses its invariant elements
by conjugation ε⁻¹Φε, with ε described as "a non-linear sigma-field of sorts"
living inside G (draft lines 2083–2084).

The physics question this study asks — can the Einsteinian Shiab generate a
dynamical scale? — is whether the one-loop effective potential log-saturates. That
potential is the trace-log of the second variation of the action with respect to
ALL physical fields. Since ε and ϖ are both physical, the relevant object is the
JOINT Hessian over (ω, θ), where θ is the Lie-algebra coordinate of
ε = exp(θ^a T_a). This is the mode pinned here (mode "epsilon-dynamical"): θ is an
independent fluctuation direction and the Hessian is assembled over (ω, θ). No
stationarity solve is performed — that further step (integrating ε out via
∂S/∂ε = 0 to obtain a composite ε*(ω)) is the "variational" mode, deferred to a
follow-up, and is where the eventual scale EXTRACTION will live.

**Why the slaved Wilson ansatz is the wrong object (kept only as a smoke-test).**
The slaved ansatz sets θ = κ·Σ_{e∈cell} ω_e — i.e. it postulates ε as an arbitrary
function of ω and eliminates it as an independent field. Its Hessian is
d²S_B/dω² evaluated along that chosen curve in (ω, θ) space: a constrained
projection of the joint Hessian dictated by the arbitrary functional κ·Σω, not the
physical joint Hessian the effective potential requires. Its degree-lift is real
in the sense that it does inherit the genuine exp(ad_θ) non-polynomiality of the
ε-conjugation, but it routes that non-polynomiality through an inserted functional
whose form (and the coupling κ) is not fixed by the theory — so the lift is not
cleanly attributable to GU's Shiab and is dismissible as an artifact of the
postulate. It is therefore retained ONLY as an optional, explicitly-labelled
smoke-test (EpsilonMode="slaved-wilson-smoketest"), never as the study instrument.

**The isolation battery (what makes an eventual degree-lift defensible).**
The decisive check that the joint-Hessian mode enables — and the slaved form
cannot — is the isolation battery. With the identity Shiab (R = I, i.e. S = F),
the curvature F depends only on ω and is completely independent of ε; therefore θ
does not appear in Upsilon = S − T at all, and the θ-block of the joint Hessian is
identically degenerate (zero). Consequently, any nonzero θ-block — and any lift of
the Hessian degree above 2 — that appears once R is the genuine Einsteinian
contraction is attributable to the Shiab's ε-dependence, and to nothing else. This
is what makes a degree>2 verdict robust against the obvious criticism "you merely
added a new nonlinear degree of freedom": the control with R = I proves the added
degree of freedom is inert by itself.

**Relation to the Phase436/441 no-go and the honest boundary.**
On the control branch (ε = 1, identity Shiab, trivial torsion) the joint-Hessian
mode reduces exactly to the Phase436/441 setting: Upsilon is degree-2 in ω, the
exact Hessian is degree-2 in the background amplitude, and no scale can arise. The
first study's structural question is precisely whether reinstating the physical ε
sector, dressed by the Einsteinian contraction, lifts that degree. A degree-2
result would extend the no-go theorem to the faithful 4D operator (sharpening the
frontier onto the deferred variational/differential structure); a degree>2 result
establishes the NECESSARY condition for a dynamical scale on a draft-canonical
operator for the first time — a candidate mechanism, NOT a scale, NOT a promotion.
Extracting an actual scale requires the deferred variational mode plus the
subsequent effective-potential analysis. Both verdicts are legitimate and
fail-closed; nothing is promoted, and no Phase201/Phase256 contract field is
touched.
>
> **ARCHITECTURE HALF (arch-4d):** the `EvaluateWithTheta`/`LinearizeTheta` joint-Hessian
> plumbing below (§3.6) + the batteries (§3.7). dev-shiab is building it as **M3b**;
> the ratified text (this section) is the M3b QA-gate prerequisite — now satisfied.
>
> **SCOPE FLAG (architect, for team-lead):** the joint-`(ω,θ)` Hessian is a
> **material design addition** beyond original M3 — recommend relabelling **M3b**
> (estimate at the end of this subsection). This is an architecture/sizing note,
> not a physics objection; I do NOT dispute (B) — it is feasible (the plumbing is
> §3.6, which the lead confirmed is the target).

The **architecture half** below (modes, `Linearize` mechanics, §3.6 plumbing,
batteries) is arch-4d's and is written to the binding (B) content; it stays
stable under the physicist's physics text.

**Discrete form (architecture; carrier confirmed VERTEX by physicist-4d).**
`ε_v = exp(θ^a_v T_a) ∈ H` per **VERTEX** `v` (ε is an H-valued gauge 0-form →
lives on vertices, lattice-gauge convention: 0-forms→vertices, 1-forms→edges,
2-forms→faces); `θ` is `VertexCount × dimG`, `θ≡0 ⇔ ε=1`;
`g_v = Ad_{ε_v} = exp(ad_{θ_v})`. The per-face Ad uses ε at the face's
**lowest-index incident vertex** (pinned default); **incident-vertex average** is a
mandated phase442 robustness variant (both rules run, degree verdict reported under
each; a material difference is a finding to flag). `S_h(F)_face = R(Ad_{ε_v}(F_advalue))`.
θ enters the operator as an **explicit stateless parameter** —
`EvaluateWithTheta(F, ω, θ)` — not stored operator state (the interface `Evaluate`
is θ=0); the study harness sweeps `θ`.

**Modes.** `θ` is the independent DOF; the modes say how it is treated:

| `EpsilonMode` | `θ` treatment | Hessian DOF | `Linearize` |
|---|---|---|---|
| `trivial` | `θ=0` (ε=1) | `ω` only | analytic `dS/dω(δ)=R(dF/dω(δ))`; reuse `IdentityShiabCpu.Linearize`. Degree-2, Phase436 exact. |
| `frozen-background` | `θ` FIXED ≠ 0, not varied | `ω` only | analytic, constant `Ad_ε`. Degree-2. |
| `epsilon-dynamical` (mode 2, HEADLINE) | `θ` VARIED, no stationarity solve | `(ω, θ)` jointly | ω-block analytic (as frozen); θ-block `LinearizeTheta` via the d-exp below. Degree > 2. |
| `slaved-wilson-smoketest` (mode 1) | `ε=exp(κΣ_{e∈cell}ω_e)` | `ω` only | non-gating smoke-test; NOT the study instrument. |
| `variational` (mode 3, DEFERRED) | `ε*(ω)` via `∂S/∂ε=0`, integrated out | `ω` (ε eliminated) | needs implicit-fn deriv through stationarity; the deferred scale-extraction upgrade. |

**Why degree > 2 (mechanics, consistent with (B); physicist text authoritative).**
`Ad_{ε(θ)} = exp(ad_θ)` is all-orders nonlinear in `θ` while `R(F)` is degree-2 in
`ϖ`; over the joint `(ω, θ)` DOF the objective is degree > 2, so the Hessian at a
joint background `t·(u_ω, u_θ)` exceeds degree-2 in `t`. At `θ=0`, `Ad=I`,
`S_h=R(F)` degree-2 → Phase436 reproduced exactly.

**θ-direction Linearize — EXACT (mechanics).** For `θ → θ+δθ`:
`δ(Ad_ε X) = [ ∫₀¹ exp(u·ad_θ)·ad_{δθ}·exp((1−u)·ad_θ) du ] X` (the d-exp of the
matrix exponential); at `θ=0` it reduces to `δ(Ad_ε X) = [δθ, X]`. Implement via
eigendecomposition of `ad_θ` or series. The ω-block `Linearize` holds `Ad_ε` fixed
and is analytic; `B_ω` does NOT enter (that is the deferred differential `Σ_mc`
branch).

**Ad convention:** fix `ε⁻¹(·)ε` vs `ε(·)ε⁻¹` uniformly, consistent with the
gauge-covariance battery (§3.7 #5).

**Consistency (physicist, non-blocking):** the A0-based augmented torsion already
realizes `T_ω = ϖ − ε⁻¹d₀ε` with `A0` in the pure-gauge/reference role; keep `ε`
and the augmented-torsion reference consistent when both are active.

**Sign-off (physics RATIFIED, dc7ddd36; these are the M3b QA-gate conditions).**
The implementation must meet: (a) the `θ=0` control (trivial torsion)
reproduces Phase436 degree-2 exactly; (b) the analytic `LinearizeTheta` matches
its finite-difference check; (c) the **θ-block isolation battery** passes — under
the identity Shiab (`R=id`) the θ-block of the joint Hessian is degenerate (the
check the slaved form structurally cannot run; lead arbitration). These REPLACE
the earlier κ-scan pair (which moves to the non-gating smoke-test). The degree
*verdict* itself does NOT gate — a degree-2 result over the joint DOF is a
legitimate frontier-sharpening outcome (memo §5).

**SCOPE NOTE — recommend M3b (architect).** Independent-θ adds, beyond original M3:
(i) the `θ` field state + `Ad_ε=exp(ad_θ)` conjugation in `Evaluate`; (ii)
`LinearizeTheta` (d-exp); (iii) a **joint `(ω, θ)` Hessian assembled at the study
level** — the phase442 probe's DOF vector becomes `(ω_edges·dimG ⊕ θ_cells·dimG)`
and it must finite-difference over BOTH fields (Hessian dimension grows
accordingly). Estimate: ~300–500 new LOC (operator ~150–250, study joint-DOF
harness ~150, tests ~200), ≈0.5–1 dev-week. This is a genuine addition, not
original-M3 scope — flagging it as **M3b** rather than absorbing it silently.

### 3.6 Class contract

```csharp
public sealed class EinsteinianShiabOperator : IShiabBranchOperator
{
    public EinsteinianShiabOperator(SimplicialMesh mesh, LieAlgebra algebra,
                                    EinsteinianShiabFamilyMember member);
    // BranchId => member.BranchId; OutputCarrierType => "curvature-2form";
    // OutputSignature => identical to IdentityShiabCpu.OutputSignature (carrier match REQUIRED).

    public FieldTensor Evaluate(FieldTensor curvatureF, FieldTensor omega,
                                BranchManifest manifest, GeometryContext geometry);  // θ=0 path
    public FieldTensor Linearize(FieldTensor curvatureF, FieldTensor omega,
                                 FieldTensor deltaOmega, BranchManifest manifest,
                                 GeometryContext geometry);   // ω-block, Ad_ε held fixed

    /// θ as an EXPLICIT STATELESS parameter (θ = VertexCount*dimG; all-zero ⇔ ε=1).
    /// Stateless (no stored EpsilonTheta): the study harness passes θ per call as it
    /// sweeps the (ω, θ) DOF. Evaluate(...) == EvaluateWithTheta(..., θ=0).
    public FieldTensor EvaluateWithTheta(FieldTensor curvatureF, FieldTensor omega,
                                         double[] theta, BranchManifest manifest,
                                         GeometryContext geometry);

    /// θ-block linearization (the §3.5 d-exp). Separate because IShiabBranchOperator's
    /// Linearize carries only δω, not δθ. Validated vs FD (sign-off criterion b).
    public FieldTensor LinearizeTheta(FieldTensor curvatureF, FieldTensor omega,
                                      double[] theta, double[] deltaTheta,
                                      BranchManifest manifest, GeometryContext geometry);
}
```

**How the independent `ε` DOF threads through the fixed interface (the M3b design
work).** `θ` cannot ride `IShiabBranchOperator.Evaluate(…, omega, …)` /
`Linearize(…, δω, …)` — those signatures carry only `ω`. Resolution (blessed
stateless, cleaner than stored state):
- `θ` is an **explicit stateless parameter** on `EvaluateWithTheta`
  (`VertexCount*dimG`); no mutable operator state. The interface `Evaluate` is the
  `θ=0` path.
- `Linearize` (interface) supplies the **ω-block** of the Jacobian (analytic,
  `Ad_ε` fixed at the passed θ); `LinearizeTheta` supplies the **θ-block** via the
  d-exp (exact analytic, delivered by dev-shiab and verified vs FD at 2.3e-10).
- The **joint `(ω, θ)` Hessian is assembled at the STUDY level** (§3.9): the
  phase442 probe forms the DOF vector `(ω_edges·dimG ⊕ θ_verts·dimG)` and
  finite-differences the objective over BOTH fields (passing θ explicitly),
  cross-checking against `LinearizeTheta`. `IShiabBranchOperator` is unchanged
  (additive: `EvaluateWithTheta`/`LinearizeTheta` are new members, no signature
  change) — the study-side joint-DOF harness is the M3b addition.

**`OutputSignature`** must be byte-identical to the torsion operator it pairs
with (all fields; `Degree="2"`, `ComponentOrderId="face-major"`,
`CarrierType="curvature-2form"`, …). Copy `IdentityShiabCpu.OutputSignature`
exactly; QA runs `BranchOperatorRegistry.ValidateCarrierMatch`.

**`Linearize` per §3.5:** analytic degree-2 for `trivial`/`frozen-background`; for
`epsilon-dynamical` the ω-block is analytic (`Ad_ε` fixed) and the θ-block is
`LinearizeTheta` (the exact d-exp), validated against FD over θ. §3.5 is authoritative.

### 3.7 M3 acceptance batteries (qa-4d enforces): memo §4.5 five + ε study batteries

1. **Richness certificate (THE HEADLINE) — projector language (physicist-4d §6e).**
   The member must use a **genuinely non-scalar** `Λ²` projector — `sd2 = P₊`, or
   `R = P₊ − ½·(trace part)` — i.e. `R` must NOT be `∝ identity` on the 6-dim
   `Λ²`. Assert `S_h(F) ≠ c·F` for every scalar `c` (the operator's matrix on
   `Λ²⊗ad` has eigenvalue spread / off-identity Frobenius norm above a floor) on
   `CreateUniform4D(1)`. This is exactly the capability the 2D toy provably lacks
   (`ShiabFamilyScopeChecker` blocked-reason 1). **The control `(id0,id0,trivial)`
   MUST FAIL this** (`R ∝ identity` ⇒ scalar multiple of `F`) — the correct,
   expected control outcome.
2. **Weyl annihilation** (`draft:2133`) — projector composition. Using the
   `Λ² = scalar ⊕ traceless-Ricci ⊕ Weyl₊ ⊕ Weyl₋` decomposition, feed a 2-form
   in the Weyl-like sector and assert the projector composition `R` sends it to
   zero to tolerance. If `R` does not annihilate the Weyl part, it is not the
   Einsteinian contraction.
3. **Carrier-signature identity:** `S_h.OutputSignature == T.OutputSignature`
   (strict, all fields).
4. **`θ=0` control reproduces Phase436 (sign-off condition a).** With
   `EpsilonMode="trivial"` (θ=0, ε=1) + trivial torsion, the joint-DOF Hessian
   restricted to ω reproduces Phase436 degree-2 exactly (third `t`-difference
   vanishes). If the control does not reproduce it, the 4D backend is wrong.
5. **`LinearizeTheta` matches FD + Hessian symmetry (sign-off condition b).** The
   analytic θ-block `LinearizeTheta` (d-exp, §3.5) matches a finite-difference of
   `Evaluate` over `θ` to ~1e-7 relative; the ω-block `Linearize` matches FD over ω.
   Assemble the joint `H` (via `CpuLocalJacobian`/`CpuMassMatrix` with
   `LieAlgebraFactory.CreateSu2WithTracePairing()`, positive-definite — project
   memory) and assert `‖H − Hᵀ‖ / ‖H‖` below floor (self-adjointness, IX.32.2.1).
6. **θ-block isolation battery (sign-off condition c; lead arbitration).** Under
   the identity Shiab (`R = id`), assert the **θ-block of the joint Hessian is
   degenerate** — ε-conjugation of an identity projector cannot lift the degree,
   so a genuine degree-lift must come from the non-scalar `R`, not from the θ
   parametrization alone. This is the check the slaved Wilson form structurally
   **cannot** run (it has no independent θ-block), and it is why the independent-θ
   realization is the faithful instrument.
7. **Gauge covariance** under `ε`-dressing: `S_h(Ξ·h) = S_h(Ξ)·h` to first order
   (`v26:1661-1667`; physics-guidance falsifier 5). Fix the `ε⁻¹(·)ε` vs
   `ε(·)ε⁻¹` convention uniformly to make this pass.
8. **θ-amplitude HONESTY sweep (physicist-4d, REQUIRED for the phase442 degree
   verdict; study/analysis battery, not an operator sign-off gate).** Sweep the
   θ-component amplitude `|u_θ|` of the joint background and assert the third
   `t`-difference of `H(t)` **→ 0 as `|u_θ| → 0` and grows with `|u_θ|`**. This
   REPLACES the retired κ-scan (which was slaved-form-specific) and, together with
   the isolation battery (#6), makes a degree>2 verdict robust: the lift vanishes
   when the ε sector is switched off and scales with it.

(Batteries 4–6 are the operator SIGN-OFF gates; 6 + 8 are the study/analysis
batteries that make the degree verdict defensible. All REPLACE the earlier κ-scan
pair, which applied to the now-demoted slaved form. The degree *verdict* does NOT
gate sign-off — a degree-2 result over the joint `(ω, θ)` DOF is a legitimate
frontier-sharpening outcome, memo §5.)

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

### 3.9 First physics study (task #12 handoff) — 4D joint-(ω,θ) Hessian-degree/saturation probe

physicist-4d decision 5 defines the first study: **"4D control-vs-Einsteinian
joint-(ω,θ) Hessian-degree/saturation probe"** (updated per the co-signed §3.5:
θ is an independent DOF, so the probe's Hessian is over the joint `(ω, θ)`). It is the natural terminus of M1–M3 and the acceptance
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
- **Einsteinian arm** `{Phi1∈{sd2,asd2}, EpsilonMode="epsilon-dynamical"}` (mode 2)
  measures whether the JOINT Hessian acquires degree > 2. **DOF NOTE (CO-SIGNED §3.5):**
  the probe is a **joint `(ω, θ)` Hessian-degree/saturation probe** — θ (the
  VERTEX-based independent H-valued DOF, `VertexCount·dimG`) is a first-class variable
  alongside ω. The rank-1 background is `t·(u_ω, u_θ)` with a nonzero θ-component; θ is
  passed as the explicit `EvaluateWithTheta` parameter as the harness sweeps `t`.
  `Ad_{ε(θ)}=exp(ad_θ)` all-orders nonlinear in θ is the degree-lift mechanism.
  This is the **M3b joint-DOF harness addition** flagged in §3.5. Per memo §5 both
  verdicts are legitimate and publishable: `hessianRemainsDegreeTwo` (theorem
  extends; sharpens the frontier onto the differential `Σ_mc` term) or
  `hessianDegreeExceedsTwo` (necessary condition for a scale met on a draft-canonical
  operator for the first time — a candidate mechanism, NOT a scale, NOT a promotion).
  **Nothing is promoted either way.** (The slaved-Wilson form may be run as a
  labelled NON-GATING smoke-test only — never the verdict instrument.)
- **Fail-closed wall (mandatory, copy `phase436`):** target-blind construction,
  no scales/poles/GeV lineage, `draftAlignmentStatus = surrogate`,
  `shiabOutputDegree = 2`, `draftOperatorIsDegreeRaising = true`,
  `reducedRealizationCapturesRicciWeylAlgebraNotFormDegree = true`,
  `draftDegreeReductionRecorded = true`,
  `epsilonRealization = independent-theta-dof`, `hessianOverJointOmegaTheta = true`,
  `slavedWilsonKeptAsSmokeTestOnly = true`, `isolationBatteryPassed` (the θ-block
  degeneracy under identity Shiab — the artifact-separation instrument that proves
  any degree-lift is caused by the Shiab's ε-dependence, not the inserted θ DOF;
  GATING, sign-off condition c), plus the full recorded-boundary key set the
  physicist enumerated (memo §3/§6b/§6c/§6e):
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
