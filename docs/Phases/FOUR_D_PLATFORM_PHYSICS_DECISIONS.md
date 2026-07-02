# Four-Dimensional Platform Build — Physics Decisions

Author: physicist-4d (physics owner of the 4D platform build team).
Status: initial complete draft, 2026-07-02. This is the physics contract the
team implements against. Every decision below is marked **PINNED** (fixed,
implement exactly), **CANDIDATE** (a scanned/enumerable choice — build it
parameterized), or **RECORDED BOUNDARY** (an honest scope limit the studies
must carry in their fail-closed language).

Primary source throughout is the 2021 draft at
`docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt` (cited as
`draft:LINE`), cross-checked against the completion revision
`TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v26.md`
(cited as `v26:LINE`) for the canonical active branch. Line ranges were
verified by hand for this memo.

---

## 0. Orientation: what this build is for, and the one physics trap

The internal boson program is at a theorem-grade terminal frontier. Three
independent no-go results — the two-condensate scale-gap probe
(`studies/phase435_two_condensate_scale_gap_probe_001`), the exact-Hessian
saturation no-go (`studies/phase436_exact_hessian_saturation_no_go_probe_001`),
and the coupled fixed-point negative (Phase440) — pin the missing dynamical
electroweak scale on bosonic structure *beyond* the 2D toy branch family. The
toy-branch universality sweep
(`studies/phase441_toy_branch_family_universality_sweep_001`) then proved the
no-saturation theorem holds across the entire 36-member toy family and stated
the frontier: `scaleGapRequiresDimFourSpinorShiabOrSourceAnchor = True`. The
draft's canonical active Shiab (`v26:1597`, Inserted Choice IX.32.2.1) cannot be
realized on `dimX = 2`; it requires `dimX >= 4` and a spinor-realized invariant
basis (Phase441 STUDY.md, "Canonical-Shiab impossibility"). This build supplies
exactly that prerequisite.

**The one physics trap the whole team must understand up front.** The
Phase436/441 no-saturation theorem does not rest on the *dimension* of the base.
It rests on this chain (Phase436 §A, Phase441 "Key structural fact"):

1. On the control branch, `Upsilon = S(F) - T` with `S = F` (identity Shiab)
   and `F = d omega + (1/2)[omega wedge omega]` is **exactly degree-2 in
   omega** — because `S = F` is *linear in F* and `F` is degree-2 in `omega`.
2. Therefore the objective `S_B = (1/2)||Upsilon||^2` is degree-4 in `omega`,
   and the exact Hessian at a background `t*u` is a degree-2 polynomial in `t`:
   `H(t) = A0 + t*A1 + t^2*A2`, nothing beyond `t^2`.
3. Masses^2 grow exactly as `t^2` ⇒ the one-loop potential grows exactly
   logarithmically ⇒ **no log-saturation ⇒ no dynamical scale**.

Merely moving to a 4-dimensional base **does not by itself break this**. The
Einsteinian contraction of eq. 9.3 (`draft:2126`) is *also linear in its
argument* `xi = F` (both terms are linear in `xi`). A naive linear
implementation of eq. 9.3 with a trivial gauge dressing (`epsilon = 1`) will
reproduce a degree-2-in-`t` Hessian and the *exact same no-go*, just on a bigger
mesh. The team would spend M1–M3 and land back on the frontier.

**What actually breaks quarticity** is the `omega`-dependence that the *control
branch sets to trivial* and the richer branch turns on:

- the **`epsilon`-conjugation** `epsilon^{-1} Phi epsilon` (`draft:2027-2034`,
  `draft:2083-2084`): `epsilon in H` lives inside the inhomogeneous gauge group
  `G = H ⋉ N` (`draft:1617`) as, in the draft's own words, "a non-linear
  sigma-field of sorts" (`draft:2084`). When `epsilon` carries part of the
  connection degrees of freedom, `S_h(F)` is no longer linear in `omega`; and
- the **differential** structure of the canonical active branch `Sigma_mc`
  (`v26:1601-1609`): `Sigma_mc(Xi) = Pi_T(K_{A0}(d_{B_omega} Xi)) +
  L_{A0}(T^aug_omega, Xi)`, whose covariant derivative `d_{B_omega}` uses
  `B_omega = ∇_0 + epsilon^{-1} d_0 epsilon` (`draft:2210-2211`) — an
  `omega`-dependent connection — and whose zero-order term `L_{A0}` couples the
  augmented torsion to the curvature (a genuinely new nonlinearity).

So the physics headline for M3, stated once and carried into every study:

> The 4D base is necessary but not sufficient. The scale-breaking test is
> whether a Shiab that is genuinely **non-scalar on the 6-dimensional
> `Lambda^2(T*X)`** *and* carries `omega`-dependence through `epsilon` and/or
> `B_omega` produces a Hessian of degree **> 2** in the background amplitude.
> The pure algebraic eq. 9.3 with `epsilon = 1` is the control that must
> reproduce the toy no-go.

Everything below is built to make that test clean, honest, and fail-closed.

---

## 1. Clifford signature for M2

**Decision: PRIMARY `Cl(4,0)` (Euclidean, spinor dim 4 complex). Signature
`Cl(p,q)` PARAMETERIZED; `Cl(3,1)` / `Cl(1,3)` a supported branch variant.**

### What the draft pins and what it leaves free

- The physical base spacetime `X` is Lorentzian: the bosonic action is
  `I1B : G × MET(X^{1,3}) -> R` (`draft:2117`), and the `(1,3)` signature is
  treated as **anthropic data**, explicitly "not meaningfully distinguished"
  from `(3,1)` (`draft:1070-1072`). So on physics grounds the draft's base is
  `Cl(3,1) ≅ Cl(1,3)`, spinor dim `2^floor(4/2) = 4` complex.
- The ambient `Y` is split signature `(7,7)`, with `Cl(7,7) ≅ R(128)`
  (`draft:1088-1096`) and `Spin(7,7) -> SO(64,64) -> U(64,64)` (`draft:1100`).
  That is the home of Definition 8.1 — out of scope for M2 (see §3).

### Why Euclidean `Cl(4,0)` is the primary realization

This is a genuine physics recommendation, not a convenience:

1. **The entire existing workbench is Euclidean/Riemannian.** The discrete
   meshes are Euclidean lattices; the naive Dirac operator is built Hermitian
   for Riemannian `Y` (memory `phase4-physics`, and
   `src/Gu.Phase4.Spin/CliffordSignature.cs:9` — "Default for Phase IV:
   `Cl(dimY,0)` — Riemannian"); Mode-B positive-definiteness requires a
   positive-definite pairing. Introducing a Lorentzian Hodge star and a
   non-Hermitian lattice Dirac in the *same* milestone that introduces the 4D
   base would confound two hard changes at once.

2. **The Einsteinian Shiab decomposition is *cleaner* in Euclidean 4D — this is
   the decisive argument.** The two-term operator of eq. 9.3 lives on
   `Lambda^2(T*X)`, and its whole point is the Ricci/Weyl split (`draft:2133`:
   "the Weyl curvature tensor is annihilated by the contraction operator"). In
   **Euclidean** signature the Hodge star on 2-forms satisfies `*^2 = +1`, so
   `Lambda^2 = Lambda^2_+ ⊕ Lambda^2_-` splits into real self-dual and
   anti-self-dual 3-dimensional eigenspaces — exactly the structure that makes
   the curvature decompose as scalar ⊕ traceless-Ricci ⊕ Weyl_+ ⊕ Weyl_-. In
   **Lorentzian** signature `*^2 = -1` on 2-forms, the ±-split is complex, and
   the clean real projectors the Shiab needs become a complex structure. For a
   first faithful realization of the Einsteinian contraction, Euclidean is the
   honest and simpler home.

3. **The scale-breaking question is signature-agnostic and is naturally a
   Euclidean (energy-functional) question.** The objective `S_B =
   (1/2)||Upsilon||^2` and its Hessian degree — the actual Phase436 observable —
   is defined on a positive-definite carrier. The draft itself notes the
   Wick-style move from "Lorentzian Signature with hyperbolic equations to
   Euclidean signature" (`draft:296`) as legitimate. Testing the Hessian degree
   in Euclidean signature is the physically meaningful computation; a Lorentzian
   redo is a later robustness branch, not the primary result.

### Recorded boundary

Euclidean `Cl(4,0)` is a **Euclidean slice**, not the draft's Lorentzian base.
It is faithful to the *operator structure* being tested (form degrees, Hodge
placement, Ricci/Weyl split, Hessian degree) but is not the physical Lorentzian
spacetime. Studies must carry: `baseSignature = Cl(4,0)-euclidean-slice`,
`lorentzianBaseNotYetRealized = true`, and record that any mass-like reading is
a Euclidean-functional eigenvalue, not an on-shell pole.

### Engineering note (feasibility confirmed, no rewrite needed)

`src/Gu.Phase4.Spin/GammaMatrixBuilder.cs` already builds arbitrary `Cl(p,q)`
via the recursive Pauli tensor-product construction, multiplies
negative-signature gammas by `i` for `q > 0` (`GammaMatrixBuilder.cs:41-52`),
and builds the chirality matrix in even dimension. Its own header states "only
`Cl(p,0)` fully validated in this first pass." So `Cl(4,0)` is directly
available and tested-in-kind; `Cl(3,1)` is present but must be validated (M2
QA). The build plan's "relocate GammaMatrixBuilder from study code" is stale —
it already lives in `src/Gu.Phase4.Spin`; M2 should *reuse and extend* it, not
relocate it. Chirality `gamma_chi = i^{n/2} gamma_1...gamma_n` exists for even
`n`; for `Cl(4,0)` this gives the `Spin(4) = SU(2)×SU(2)` L/R decomposition M3
needs for the self-dual/anti-self-dual projectors.

---

## 2. Triangulation and discrete-forms validity (M1)

**Decision: the Coxeter–Freudenthal–Kuhn (CFK) decomposition of the 4-cube into
`4! = 24` oriented simplices PRESERVES the discrete-exterior-calculus structure
the platform relies on. `d∘d = 0` through 3-forms is guaranteed combinatorially,
not numerically, provided boundary orientations are built from canonical vertex
ordering. PINNED.**

### Rationale

The platform's discrete exterior derivative is the simplicial coboundary: the
transpose of the boundary incidence maps that `MeshTopologyBuilder` already
constructs from canonical vertex ordering (`v0 < v1 < ...`,
`MeshTopologyBuilder.cs:40-50`; the current 2D generator splits each square into
2 triangles with fixed local vertex order,
`SimplicialMeshGenerator.cs:43-57`). For **any** simplicial complex, `∂∘∂ = 0`
is a combinatorial identity (each codimension-2 face is reached by exactly two
oriented paths that cancel), and `d∘d = 0` follows by transposition. The CFK
triangulation of the `n`-cube is a standard, orientation-consistent simplicial
subdivision: order the `2^n` cube vertices by coordinates, and for each
permutation `pi` of the `n` axes take the monotone lattice path
`0 -> e_{pi(1)} -> e_{pi(1)}+e_{pi(2)} -> ... -> (1,...,1)`; the `n!` resulting
simplices tile the cube, any two meet in a common face, and all inherit a
consistent orientation from the ambient cube. For `n = 4` this is `24` simplices
per tesseract. This preserves `d∘d = 0` because it produces a bona fide oriented
simplicial complex — nothing about dimension 4 is special to the algebra.

The physics requirement is only that the **orientation convention is applied
consistently** across the new 3-subsimplices (volumes) and their 2-subsimplex
(face) boundaries, so the sign pattern that gives `∂∘∂ = 0` is not corrupted.
That is an implementation-discipline requirement, and it is exactly what QA must
certify.

### Orientation convention (PINNED) and the QA test QA must run

- **Convention:** every `k`-subsimplex is stored with vertices in canonical
  ascending order; the boundary of a `k`-simplex `[v0,...,vk]` is
  `sum_i (-1)^i [v0,...,v̂_i,...,vk]`, and each face's incidence sign into a
  parent simplex is the sign of the permutation carrying the parent's induced
  face-vertex order into canonical order. This matches the existing edge/face
  convention and extends it to volumes.
- **QA acceptance test (boundary-of-boundary vanishing on volumes):** assemble
  the integer incidence matrices `B1` (vertices←edges), `B2` (edges←faces),
  `B3` (faces←volumes). Assert in **exact integer arithmetic**:
  `B1 · B2 = 0` and `B2 · B3 = 0` (every entry exactly zero). Equivalently, for
  every 3-simplex the signed face boundary, expanded to its edges, cancels. This
  is stronger than a floating tolerance and must pass exactly.
- **Count-consistency test:** the `2×2×2×2` grid (`CreateUniform4D(2)`), built
  as `16` tesseracts × `24` simplices, must report the Euler-consistent
  vertex/edge/face/volume/4-cell counts. QA computes the alternating sum
  `V - E + F + Vol - C4` and checks it equals the expected Euler characteristic
  of the meshed 4-torus/4-block (record the closed vs open boundary convention
  chosen; a periodic 4-torus gives `chi = 0`).
- **Regression:** all existing 2D/3D `d∘d` and Bianchi checks stay green
  (the Bianchi structural falsifier `d_omega(F) = 0`, physics-guidance memory
  §Falsifier, and `dd=0 + Jacobi` identity, `FalsifierRegistry.cs:23`).

### Recorded boundary

The CFK mesh is a **Euclidean uniform lattice**, not a physical spacetime
triangulation; it inherits the toy-geometry caveats already standard in the
program. No metric refinement/curvature-of-`X` is claimed at M1 — the base
metric is flat unless a later milestone varies it.

---

## 3. Definition 8.1 scoping for M2

**Decision: the full `[Lambda^i(R^{7,7}) ⊗ u(64,64)]^{Spin(7,7)}` invariant
basis (`draft:2073-2077`, eq. 8.7) is OUT OF SCOPE. M2 realizes a REDUCED
`Spin(4)` SLICE: the `Spin(4)`-invariant elements of
`Lambda^i(T*X^4) ⊗ End(S)` for the chosen spinor bundle `S` of `Cl(4,0)`.
RECORDED BOUNDARY.**

### The smallest faithful invariant-basis statement on a 4D base

Fix `S` = the Dirac spinor bundle of `Cl(4,0)`, `dim_C S = 4`, so
`End(S) ≅ gl(4,C)` (real dimension 32; the traceless part `sl(4,C)` or the
compact real form `u(4)`/`su(4)` are the natural invariant carriers). The
structure group acting geometrically is `Spin(4) = SU(2)_L × SU(2)_R`. The
reduced invariant basis is

```
{ Phi_i } = a basis of  [ Lambda^i(T*X^4) ⊗ End(S) ]^{Spin(4)} ,  i = 0..4.
```

Concretely the invariants the M3 Shiab actually needs are small and enumerable:

- `i = 0`: the identity/trace element `1_{End(S)}` (a scalar) — the "pure trace"
  element the draft names in §8.1 (`draft:2041-2043`).
- `i = 2`: the self-dual and anti-self-dual projectors `P_+`, `P_-` on
  `Lambda^2` (each 3-dimensional), realized on spinors via the chirality-graded
  gamma bilinears `gamma_{[mu} gamma_{nu]}` — these are the invariant elements
  that carry the Ricci/Weyl split.
- `i = 4`: the volume form `omega_vol` / the chirality element `gamma_chi`
  (`draft:2049`'s `theta` map analog on the slice).

### What it CAN claim

It faithfully realizes the **form-degree structure, the Hodge-star placement,
the `epsilon`-conjugation dressing, and the two-term Einsteinian contraction of
eq. 9.3 on the 6-dimensional `Lambda^2(T*X^4)`** — i.e., the operator taxonomy
that the 2D toy provably *cannot* carry (`ShiabFamilyScopeChecker.cs:22-24`,
blocked-reason (1): "Ricci/Weyl decomposition requires `dimX >= 4` where
`Lambda^2` has dimension 6"). This is the genuine enrichment the frontier
demands.

### What it CANNOT claim (RECORDED BOUNDARY, mandatory study language)

- It is **not** the draft's `[Lambda^i(R^{7,7}) ⊗ u(64,64)]^{Spin(7,7)}` basis.
  It drops the ambient `(7,7)` structure, uses `Spin(4)` not `Spin(7,7)`, and
  replaces `u(64,64)` with `End(S)` for a `dim 4` spinor.
- It therefore **cannot** represent the chimeric weld, the internal
  `su(3)×su(2)×u(1)` content, the vector-spinor `144`, or any of the
  SM-doublet-carrier structure the composite program (Phases 407–425) was built
  on. It is silent on the electroweak namespace.
- Consequently **no** result from M2/M3 may touch the Phase201 W/Z contract, the
  Phase201 Higgs contract, or the Phase256 observed-field extraction contract.

Mandatory recorded-boundary keys for every study on this slice:
`definition81Scope = reduced-spin4-slice`, `ambientSevenSevenRealized = false`,
`internalGaugeContentRealized = false`, `weldRealized = false`,
`canFillPhase201WzContract = false`, `canFillPhase256Contract = false`.

---

## 4. Eq. 9.3 candidate family for M3

**Decision: `EinsteinianShiabOperator` exposes a parameterized two-term family
built exactly on the eq. 9.3 shape. The two-term shape, the double-Hodge
placement, and the `epsilon`-conjugation are PINNED; the `Phi` basis elements,
the relative coefficient, the bracket type, and the `epsilon` dynamics are
CANDIDATE (enumerable for a Phase441-style universality sweep).**

### The pinned skeleton (verified against three draft locations)

Eq. 9.3 (`draft:2126-2129`), restated identically at draft item 3
(`draft:2204-2207`):

```
S_h(xi) = [ (eps^{-1} Phi_1 eps) ∧ (*xi) ]                          <- "Ricci Like"
        - c * [ (eps^{-1} Phi_1 eps) ∧ * [ (eps^{-1} Phi_2 eps) ∧ (*xi) ] ]
                                                                    <- "Ricci Scalar Like"
        for a gauge-covariant ad-valued 2-form  xi in Omega^2(Y, ad).
```

PINNED elements:

- **Two terms, subtractive**, in exact parallel to the Einstein tensor
  `G = Ric - (1/2) R g` (`draft:2123-2124` "makes the parallel to Einstein's
  contraction of the full Riemannian curvature explicit"; annotations "Ricci
  Like" / "g_{mu nu}-like" / "Ricci Scalar Like", `draft:2128-2129`).
- **Hodge-star placement**: `*xi` inside the first bracket; the nested
  `* [ ... ∧ (*xi) ]` double-Hodge in the second. PINNED exactly.
- **`epsilon`-conjugation** `eps^{-1} Phi eps` on every `Phi` (the "Ship in a
  Bottle" gauge-covariantization, `draft:2021-2027`). PINNED. Note `Phi` is a
  "normed Lie-Algebra-valued r-form valued in an invariant subspace of the
  structure group" (`draft:2031-2032`).
- **Output carrier**: `S_h` must have `OutputSignature` *strictly identical* to
  the torsion operator's output (`IShiabBranchOperator.cs:20-25`) so
  `Upsilon = S_h - T` is well-defined. Eq. 9.3 maps `Omega^2 -> Omega^{d-1}`
  (`draft:2121`); on the 4D base `d - 1 = 3`, so the honest carrier is a
  3-form. **Architecture note for arch-4d:** the current interface hard-codes
  `OutputCarrierType = "curvature-2form"`; M3 needs either (a) a `d=4` torsion
  target that is also a 3-form (augmented torsion `T^aug in Omega^1`… see the
  degree bookkeeping below) or (b) a documented reduction. This is the single
  interface decision M3 must settle before coding — see §4.4.

### 4.1 The Einstein coefficient `c`

`draft:2126-2129` renders a factor associated with the "g_{mu nu}-like" middle
brace. Following the Einstein analogy the natural value is `c = 1/2`. PINNED
default `c = 1/2`; CANDIDATE for the sweep (`c in {0, 1/2, 1}` at minimum, where
`c = 0` degenerates to the one-term "Ricci-like" contraction and is a useful
control). The draft does not rigorously fix `c`; record it as
`einsteinCoefficient` with default `0.5` and provenance "Einstein-analogy,
draft-unpinned".

### 4.2 Admissible `Phi` choices on the reduced slice

`Phi_1`, `Phi_2` range over the `Spin(4)`-invariant elements of §3. The
admissible, enumerable menu:

| symbol | form degree | invariant element | role |
|---|---|---|---|
| `id0` | 0 | `1_{End(S)}` (trace) | scalar dressing → recovers scalar-multiple-of-F when both Φ are `id0` |
| `sd2` | 2 | self-dual projector `P_+` on `Lambda^2` | Ricci-like contraction, self-dual channel |
| `asd2`| 2 | anti-self-dual projector `P_-` | Ricci-like, anti-self-dual channel |
| `vol4`| 4 | volume form / `gamma_chi` | scalar-scaled dual, parity-odd dressing |

Genuinely-rich members require at least one of `Phi_1, Phi_2 in {sd2, asd2,
vol4}`. The **control member** is `Phi_1 = Phi_2 = id0`, `eps = 1` — this MUST
reduce to a scalar multiple of `F` and reproduce the toy no-go (see §5).

### 4.3 Bracket type and `epsilon` dynamics

- **Bracket**: eq. 8.1 (`draft:2028-2034`) allows commutator, or
  `i`-weighted anticommutator (`i^{(1±1)/2}` prefactor, the `±` in eq. 8.1).
  CANDIDATE `bracketType in {commutator, i-anticommutator}`; PINNED default
  `commutator` (matches the platform's existing `[omega,omega]` convention and
  the `d_{A0}` bracket with no `1/2` factor — memory `MEMORY.md`, augmented
  torsion note).
- **`epsilon` dynamics**: this is the physics lever (see §0). CANDIDATE
  `epsilonMode in {trivial (eps=1), frozen-background, omega-coupled}`. PINNED
  default for the *scale-breaking* run is `omega-coupled` (the sigma-field
  reading, `draft:2084`); PINNED default for the *control* run is `trivial`.
  The `omega-coupled` mode is what can lift the Hessian degree above 2.

### 4.4 FAMILY-MEMBER SPEC format (for the universality sweep)

Mirror the Phase441 `(shiab, torsion, A0)` product. A member is a record:

```
EinsteinianShiabFamilyMember {
  phi1:          { formDegree: int, invariantElement: "id0"|"sd2"|"asd2"|"vol4" },
  phi2:          { formDegree: int, invariantElement: "id0"|"sd2"|"asd2"|"vol4" },
  einsteinCoefficient: double,          // default 0.5; sweep {0, 0.5, 1}
  bracketType:   "commutator" | "i-anticommutator",
  epsilonMode:   "trivial" | "frozen-background" | "omega-coupled",
  branchId:      derived string, e.g. "einsteinian-shiab/sd2-id0/c0.5/comm/omega-coupled"
}
```

The M3 sweep is then the Cartesian product (paired with the M1 torsion and A0
menus, exactly as Phase441). The registry (`BranchOperatorRegistry`) must
`ValidateCarrierMatch` for every member (Phase441 battery, STUDY.md line 103).

### 4.5 Batteries that distinguish a correct implementation

A correct `EinsteinianShiabOperator` must pass all of:

1. **Richness certificate (the headline battery).** For any member with a
   nontrivial `Phi` (e.g. `sd2`), assert `S_h(F) ≠ c·F` for every scalar `c`:
   the operator's matrix on `Lambda^2 ⊗ ad` is **not** proportional to identity
   (measure eigenvalue spread / off-identity Frobenius norm above a floor). This
   is precisely the capability the 2D toy provably lacks
   (`ShiabFamilyScopeChecker.cs`, `BuildExpansionBlockedReason` (1)) — it is
   what makes the 4D family "genuinely richer." The control member
   (`id0,id0,eps=1`) must FAIL this (it *is* a scalar multiple of `F`), which is
   the correct, expected outcome for the control.
2. **Weyl annihilation** (`draft:2133-2135`: "the Weyl curvature tensor is
   annihilated by the contraction operator"). Feed a pure Weyl-like 2-form (the
   part of `Lambda^2` orthogonal to the scalar/traceless-Ricci directions
   reachable by the chosen `Phi`) and assert `S_h` sends it to zero (or projects
   it out) to numerical tolerance. If the implementation does not annihilate the
   Weyl part, it is not the Einsteinian contraction.
3. **Carrier-signature identity**: `S_h.OutputSignature == T.OutputSignature`
   (strict, all fields — `IShiabBranchOperator.cs:20-25`).
4. **Linearization / Hessian symmetry**: the analytic `Linearize` must match a
   finite-difference of `Evaluate` (Phase436 machinery), and the composite
   `B = Sigma^* Sigma` must be symmetric/symmetrizable on the chosen energy
   space — the self-adjointness demanded by Inserted Choice IX.32.2.1
   (`v26:1615`, `v26:1641-1645`). QA asserts `||H - H^T|| / ||H||` below floor.
5. **Gauge covariance** under `eps`-dressing: `S_h(Xi·h) = S_h(Xi)·h` to first
   order (`v26:1661-1667`, Prop 32.4.2; physics-guidance falsifier 5).

---

## 5. What success looks like — the first 4D physics study

**Study name: `4D control-vs-Einsteinian Hessian-degree probe`** (the named
first post-build study; memory `four-d-platform-build`).

### The question

Does a genuinely richer Shiab break the **exact quarticity** (degree-2-in-`t`
Hessian) that Phase436/441 proved forbids a dynamical bosonic scale?

### Expected decision structure (target-blind, both outcomes are legitimate)

Reuse the Phase436 exact-Hessian finite-difference machinery on the `Cl(4,0)`
4D mesh. For each of a small set of members, compute the exact Hessian `H(t)` of
`S_B = (1/2)||Upsilon||^2` at constant rank-1 backgrounds `t*u`, and measure the
**third and fourth finite `t`-differences** of `H(t)` (Phase436 measured the
third difference `= 0` to machine precision on the control branch).

- **Control arm** — member `(id0, id0, eps=1, trivial torsion)`. **Prediction:
  third `t`-difference `= 0`** (degree-2 Hessian), reproducing Phase436/441
  exactly on the 4D mesh. This arm is the *validation gate*: if the control does
  NOT reproduce the toy no-go, the 4D machinery has a bug and no other verdict
  is trustworthy. (Physics reason: `id0,id0,eps=1` ⇒ `S_h = c·F`, linear in
  `F`, `Upsilon` degree-2 in `omega`, `[u,u]=0` on a single-direction ray ⇒
  affine — the entire Phase441 "Key structural fact" carries over.)
- **Einsteinian arm** — member with `sd2`/`asd2` `Phi` and
  `epsilonMode = omega-coupled`. **The open question.** Two possible verdicts,
  both recorded honestly:
  - `hessianRemainsDegreeTwo = true` (third `t`-difference `= 0`): the richer
    algebraic contraction, even non-scalar on `Lambda^2`, does *not* by itself
    lift the degree — the no-saturation theorem extends to the 4D algebraic
    Einsteinian family. This would sharpen the frontier onto the *differential*
    `Sigma_mc` term (`d_{B_omega}`) and/or a source anchor.
  - `hessianDegreeExceedsTwo = true` (nonzero third/fourth `t`-difference): the
    `omega`-dependence through `eps`/`B_omega` lifts the Hessian degree — the
    **necessary** condition for log-saturation and a dynamical scale is met on a
    draft-canonical operator for the first time. This is a candidate mechanism,
    NOT a scale and NOT a promotion (see boundaries).

### Fail-closed boundaries (mandatory)

- Toy 4D Euclidean geometry; candidate-only `Phi`; no measured value consulted.
- A degree `> 2` result establishes *only* the necessary condition; it does not
  produce a finite scale, a pole, a VEV, a coupling, or a GeV normalization.
  Extracting an actual scale requires the subsequent
  one-loop-potential/gap-equation analysis (the Phase435/438 machinery re-run on
  the lifted Hessian), which is a *later* study.
- `canFillPhase201WzContract = false`, `canFillPhase201HiggsContract = false`,
  `canFillPhase256Contract = false` regardless of outcome. Nothing is promoted.
- The probe's `...Passed` gate keys on internal consistency (control arm
  reproduces Phase436; batteries §4.5 green) and passes *regardless of how the
  degree verdict falls* — exactly the Phase441 fail-closed discipline
  (STUDY.md, "Fail-closed").
- Scanner registration: the new study doc and `IMPLEMENTATION_P4xx.md` must be
  registered in the broad text-scanner exclusion lists in the same checkpoint
  (restart-prompt SCANNER-REGISTRATION HAZARD, 2026-07-01).

### Why this is the right first study

It is the minimal, decisive experiment that (a) validates the entire M1–M3 build
against the known Phase436 result via the control arm, and (b) directly attacks
the terminal frontier's single open question with a target-blind, fail-closed
computation whose either outcome is scientifically informative. It reuses
existing machinery (Phase436 finite differences, Phase441 sweep harness) and
transfers the program's established methodology to 4D.

---

## 6. Summary of decisions (for arch-4d to fold into the design)

| # | Decision | Status |
|---|---|---|
| 1 | Primary base signature `Cl(4,0)` Euclidean, spinor dim 4; `Cl(p,q)` parameterized, `Cl(3,1)` a branch variant | PINNED primary + parameterized |
| 1 | Euclidean chosen because `*^2=+1` gives the real self-dual/anti-self-dual `Lambda^2` split the Einsteinian Shiab needs, and the whole workbench is Euclidean | rationale |
| 2 | CFK (`4! = 24`-simplex) tesseract decomposition; `d∘d = 0` guaranteed combinatorially; QA runs exact-integer `B1·B2=0`, `B2·B3=0` + count/Euler check | PINNED |
| 3 | Definition 8.1 reduced to the `Spin(4)`-invariant slice `[Lambda^i(T*X^4) ⊗ End(S)]^{Spin(4)}`; full `(7,7)`/`u(64,64)` basis out of scope; internal SM content NOT realized | RECORDED BOUNDARY |
| 4 | Eq. 9.3 two-term family: shape/double-Hodge/`eps`-conjugation PINNED; `Phi` menu {id0,sd2,asd2,vol4}, coefficient `c`, bracket, `epsilonMode` CANDIDATE; FAMILY-MEMBER SPEC per §4.4 | PINNED shape + CANDIDATE params |
| 4 | Richness battery: `S_h(F) ≠ c·F` for nontrivial `Phi` (the 4D-only capability); plus Weyl-annihilation, carrier-identity, Hessian-symmetry, gauge-covariance | PINNED batteries |
| 5 | First study = 4D control-vs-Einsteinian Hessian-degree probe; control arm must reproduce Phase436 degree-2; Einsteinian arm tests degree `> 2`; fully fail-closed | PINNED study spec |

---

## 6b. Fiber/ambient dimension policy (architect Q2)

**Decision: for M1–M3 and the first study, PIN `dimY = dimX = 4` — a TRIVIAL
geometric fiber. The gauge structure stays in the `ad`-bundle over the 4D base
(as in the current toy). The spinor bundle `S` of `Cl(4,0)` is used ONLY as the
Shiab's algebraic invariant carrier (`End(S)`, §3), NOT as a Kaluza–Klein fiber.
Design `dimY` as a parameter so a later milestone can grow toward the draft's
`dimY = 14` observerse. RECORDED BOUNDARY.**

Rationale: the first study (the Hessian-degree probe, §5) is a *bosonic* question
about `Upsilon = S_h(F) - T` on the `ad`-valued forms over `X`. The gauge algebra
(`su(3)`, matching Phase436/441's `2×2` `su(3)` fiber-bundle mesh, carrier 416)
enters through the `ad`-bundle exactly as it does now; nothing in that question
needs a geometric fiber. The spinor layer is needed for the Shiab's `Φ` elements
(self-dual/anti-self-dual projectors realized as gamma bilinears), which is an
`End(S)` construction, not a fiber direction. Keeping `dimY = 4` makes the
control arm *directly* comparable to Phase436 (same `ad`-bundle, same `Upsilon`
structure — only `dimX: 2 → 4` and a richer Shiab change), which is the whole
point of the control arm as a validation gate.

Recorded boundary (mandatory study language): `ambientDimY = 4`,
`geometricFiberTrivial = true`, `spinorUsedAsShiabCarrierNotFiber = true`,
`draft14dObserverseNotRealized = true`, `noKaluzaKleinTowerModeled = true`. The
internal SM content lives in the fiber/`(7,7)`-spinor directions that are absent
here (consistent with §3) — so this cannot and does not touch the electroweak
contracts. Gauge algebra for the first study: use `su(3)` to match Phase436/441
directly; `su(2)` (`CreateSu2WithTracePairing`, positive-definite) is the lighter
smoke-test.

## 6c. Dirac operator conventions for M2 `SpinorDiracOperator` (architect Q4)

**Anchor: reuse the platform's existing Euclidean naive-Dirac scheme in
`src/Gu.Phase4.Dirac/CpuDiracOperatorAssembler.cs` unchanged in structure, with
ONE required refinement for the Freudenthal mesh's diagonal edges.**

The existing scheme (`CpuDiracOperatorAssembler.cs:14-17`, `:191-234`) is
vertex-centered central difference:

```
(D_h psi)_v = sum_{e=(v,w)} Gamma_hat(e) * (psi_w - psi_v) / |e|
```

with antisymmetric edge contribution (`+Gamma·diff` at `v`, `-Gamma·diff` at `w`,
`:215-233`) and gauge coupling added per edge (`:236-247`).

- **Edge weighting / diagonal edges (the one refinement — PINNED).** The current
  code sets `Gamma_hat(e)` = the gamma of the *dominant* direction
  `mu = argmax|e_mu|` (`:197`, `DominantDirection`). On the Freudenthal/CFK mesh
  most edges are diagonal (differ in ≥2 coordinates), and on a diagonal the
  `argmax` is a **tie** with an arbitrary tie-break — this is already a latent
  imprecision in the 2D toy (its square-splitting diagonal `(−1,+1)` ties too),
  and it becomes dominant in 4D. Replace the single dominant gamma with the
  **Clifford contraction of the unit edge vector**:
  ```
  Gamma_hat(e) = sum_mu ( (x_w - x_v)_mu / |e| ) * Gamma_mu      (= ê · Gamma)
  ```
  This is the geometrically correct directional Dirac derivative along the edge,
  it is a Hermitian matrix (real combination of Hermitian Riemannian gammas), and
  it reduces *exactly* to the existing single-gamma scheme on axis-aligned edges,
  so no existing 2D/3D result changes. This removes the tie-break, restores
  rotational/orientation consistency on the isotropic Freudenthal mesh, and is
  the continuum-consistent choice (`D = Gamma^mu partial_mu`).
- **Hermiticity / `i` convention (PINNED — keep existing).** Keep
  `CpuDiracOperatorAssembler`'s Hermiticity bookkeeping and its validator
  (`IsHermitian` when `||D - D†|| / ||D|| <= 1e-10`, `:63`, `:25`). For
  `Cl(4,0)` with Hermitian gammas the edge-contracted operator preserves the
  same (anti-)Hermitian structure the 2D operator has; QA asserts the same
  residual bound. The mass-like scale is read as in the fermionic memory
  (`phase4-physics`): `ObservedMassLikeScale = sqrt(Re(lambda)^2 + Im(lambda)^2)`
  — the Dirac eigenvalue *is* the mass scale directly (first-order operator), NOT
  `sqrt(|lambda|)`. Do not introduce a new `i` convention; if a mass term is
  added it multiplies the identity-on-spinor block.
- **Mass-term placement (PINNED).** Default `M_branch = 0` for the first study
  (the assembler already sets `MassBranchTermIncluded = false`, `:79`). If a
  Dirac mass is later switched on it is a diagonal (vertex-local) `m·I_{spinor}`
  term added to `D_h`, block-diagonal in the gauge index — matching the
  `M_psi`-pairing convention (`phase4-physics` memory). It is NOT an edge/hopping
  term.
- **Spin connection (PINNED — keep existing).** Flat Levi-Civita on the uniform
  4D lattice (legitimate initial simplification, `phase4-physics` P4-IA-003);
  gauge coupling `omega_mu^a rho(T_a)` enters per edge via the existing
  `AddGaugeCouplingContribution` path, default representation adjoint.
- **Fermion carrier / boundary conditions (PINNED — periodic 4-torus).** The
  difference-form `D` (with the on-site `−Gamma psi_v` term) is exactly
  anti-Hermitian only where incident-edge on-site terms cancel — i.e. at interior
  vertices where the neighbor set is `±`-symmetric so `sum_q ê_{pq} = 0`. On the
  OPEN block `[0,n]^4` this fails at boundary vertices (`sum_q ê_{pq} != 0` ⇒ a
  Hermitian diagonal defect), so `iD` is not exactly Hermitian there. **Fermion
  spectral studies therefore run on a PERIODIC 4-torus** (identify opposite faces
  of `[0,n]^4`): every vertex is interior, `D` is exactly anti-Hermitian, `iD`
  Hermitian to machine precision, the existing `DiracOperatorValidator` bound
  holds unchanged, and translation invariance gives a clean, analytically
  understood spectrum (`~ sum_mu gamma_mu sin k_mu`) free of boundary artifacts —
  the standard naive-lattice-Dirac setup. The OPEN block is retained for the
  bosonic Hessian-degree probe (which does not consume `D`). Record
  `fermionCarrier = periodic-4-torus`. Do NOT drop the on-site term to force
  anti-Hermiticity on the open block (the pure-hopping operator is a *different*
  operator with an implicit Neumann-like BC — acceptable only as a fallback if
  periodic meshing is infeasible). Optional later refinement: anti-periodic BC in
  one direction to lift the trivial `k=0` zero mode.

Continuum-limit consistency: with the edge-contracted `Gamma_hat`, the naive
lattice `D_h` converges to `Gamma^mu(partial_mu + omega_mu)` — the same operator
the boson-program studies validated in 2D — so the 4D spinor layer's continuum
limit is consistent with the lattice conventions already in use. (Note: naive
central-difference Dirac carries the standard fermion-doubler modes; this is a
recorded, pre-existing property of the platform's discretization, not new to 4D,
and is out of scope to fix here — flag it as `naiveDiracDoublersPresent = true`.)

## 6d. Sign-off item I retain (omega-coupled epsilon realization)

I am the physics sign-off on the discrete realization of the `omega-coupled`
`epsilonMode` in M3. When arch-4d/dev-shiab produce the revised M3 section, I
will confirm the discrete `epsilon^{-1} Phi epsilon` dressing is faithful to
`draft:2083-2084` (the sigma-field reading — `epsilon in H` carrying connection
DOF) and, if the differential canonical branch is realized, that
`B_omega = ∇_0 + epsilon^{-1} d_0 epsilon` matches `draft:2210-2211`
(`v26:1601-1609`). The specific check: the discrete `epsilon` must be an
`H`-valued field on the mesh whose variation feeds `dS_h/domega` a term that is
NOT present when `epsilon = 1` — that nonzero extra term is the mechanism that
can lift the Hessian degree above 2. If the discrete `epsilon` is frozen or
enters only multiplicatively-constant, the realization is degenerate and I will
send it back.

---

## 7. Summary addendum (Q2/Q4)

| # | Decision | Status |
|---|---|---|
| Q2 | `dimY = dimX = 4`, trivial geometric fiber; spinor `S` is the Shiab carrier, not a KK fiber; `dimY` parameterized for later growth to 14 | PINNED + RECORDED BOUNDARY |
| Q4 | Reuse existing naive central-difference vertex Dirac; refine `Gamma_hat(e) = ê·Gamma` (Clifford-contract the unit edge vector) for diagonal Freudenthal edges; keep existing Hermiticity/`i`/mass conventions; flat LC + per-edge gauge coupling | PINNED |

## 6e. Interface confirmation (Ω²→Ω²) and the discrete ε conjugator

### Interface: CONFIRMED degree-preserving Ω²→Ω² for the first realization

arch-4d's `Ω²→Ω²` (curvature-2form in, curvature-2form out) realization is
**confirmed** for M3 and the first study, and the degree-raising outer leg is
**out of scope** for now. Physics justification (this is a real physics call,
not a convenience):

- The draft operator is genuinely degree-*raising*: eq. 9.2 (`draft:2121`) maps
  `Omega^2(Y) -> Omega^{d-1}(Y)`, and the degree bookkeeping forces `Phi_1` to be
  a **1-form** (`Phi_1 ∧ *xi` with `*xi` a `(d-2)`-form must land in
  `Omega^{d-1}`). So the literal eq. 9.3 raises form degree via wedging with a
  1-form `Phi` and the double-Hodge.
- **But the degree-raising Hodge/wedge legs are LINEAR in `xi = F` and carry no
  `omega`-dependence.** They are therefore *irrelevant to the Hessian-degree
  question*, which is entirely about whether `S_h(F)` carries `omega`-dependence
  beyond that already in `F`. The `epsilon`-conjugation (degree-preserving, a
  similarity transform on the ad-value) and the `Lambda^2 -> Lambda^2` projectors
  (`id0/sd2/asd2/vol4`) carry **all** the relevant `omega`-dependence and all the
  richness. Dropping the linear degree-raising leg loses nothing that the probe
  measures, and it makes the control arm *literally* the existing
  identity-Shiab/trivial-torsion pipeline ⇒ exact Phase436 reproduction.

RECORDED BOUNDARY (mandatory study keys): `shiabOutputDegree = 2`,
`draftOperatorIsDegreeRaising = true` (draft is `Omega^2 -> Omega^{d-1}` with
`Phi_1` a 1-form), `reducedRealizationCapturesRicciWeylAlgebraNotFormDegree =
true`, `draftDegreeReductionRecorded = true`. Honest `Omega^3` output with a
3-form torsion target is a legitimate later milestone (M1's Volumes/FaceToVolume
is the escape hatch) but it forfeits literal Phase436 control-arm reproduction
and needs new 4D torsion physics — deferred.

Two batteries must be re-expressed in the projector language (they survive):
(1) **Richness** — assert `S_h(F) ≠ c·F` using a genuinely non-scalar projector
(e.g. `sd2 = P_+`, or `R = P_+ − (1/2)·trace-part`), which is not proportional
to identity on the 6-dim `Lambda^2`. (2) **Weyl annihilation** — `S_h` must kill
the Weyl-like part of the `Lambda^2 = scalar ⊕ traceless-Ricci ⊕ Weyl_±` split
(`draft:2133-2135`), now realized as a projector composition rather than a Hodge
double-contraction.

### The discrete ε conjugator for `EpsilonMode = "omega-coupled"` (architect Q)

**`epsilon` is an INDEPENDENT `H`-valued 0-form (vertex field), part of the
inhomogeneous-gauge-group connection PAIR `(epsilon, varpi)` — NOT a function of
the existing `omega = varpi`.** This is faithful to `draft:1617` (`G = H ⋉ N`,
so `epsilon in H` and `varpi in N` are independent) and to `draft:2200`
(`T_omega = varpi − epsilon^{-1} d_0 epsilon`). So "omega-coupled" mode = add
`epsilon` as a dynamical `H`-valued vertex field to the bosonic configuration and
vary it in the Hessian; "trivial"/control mode = `epsilon ≡ 1`, not varied.

Discrete definition (reduced slice, `H = SU(N)` structure group of the
ad-bundle):

- `epsilon_v = exp( theta^a_v T_a ) in H` at each vertex `v`, with Lie-algebra
  coefficients `theta_v` the new dynamical DOF (`theta ≡ 0` ⇔ `epsilon = 1`).
- Conjugator acting on the ad-value: `g = Ad_{epsilon_v} = exp( ad_{theta_v} )`,
  a `dimG × dimG` matrix, `(ad_theta)^c_b = f^c_{ab} theta^a`.
- `S_h(F)_face = R( Ad_{epsilon}( F_advalue ) )` where `R` is the chosen
  `Lambda^2` form-projector and `Ad_epsilon` acts on the Lie-algebra index;
  `epsilon` is evaluated at the face's representative vertex (document the
  vertex→face rule — lowest-index incident vertex, or the incident-vertex
  average; either is admissible, just record it).
- **Linearize (exact analytic form the contract needs):** for `theta ->
  theta + delta_theta`,
  ```
  delta( Ad_epsilon X ) = [ ∫_0^1 exp(u·ad_theta) · ad_{delta_theta} · exp((1-u)·ad_theta) du ] X
  ```
  (the standard d-exp of the matrix exponential). At the control point
  `theta = 0` this reduces to `delta(Ad_epsilon X) = [delta_theta, X]`. Implement
  via the closed-form d-exp (eigendecomposition of `ad_theta`) or the series; the
  §4.5 finite-difference-vs-analytic battery validates it.
- **Why this lifts the degree:** `Ad_{epsilon(theta)} = exp(ad_theta)` is
  all-orders nonlinear in `theta`, while `R(F)` is degree-2 in `varpi`. Composing
  gives `S_h` of degree `> 2` in the enlarged configuration `(theta, varpi)`, so
  `Upsilon` and the Hessian acquire degree `> 2` in the background amplitude `t`.
  With `epsilon = 1` (`theta = 0`, control), `Ad = I`, `S_h = R(F)` is degree-2,
  and the Hessian is exactly degree-2 — Phase436 reproduced.
- **`B_omega` does NOT enter this realization.** The Maurer–Cartan
  `epsilon^{-1} d_0 epsilon` (discretely `log(epsilon_v^{-1} epsilon_w)/|e|` on
  edge `e=(v,w)`) and `B_omega = ∇_0 + epsilon^{-1} d_0 epsilon` appear ONLY if
  the differential canonical branch `Sigma_mc` (`v26:1601-1609`) is later
  implemented. For the confirmed algebraic `Omega²→Omega²` realization the
  Linearize contract needs `d(Ad_epsilon)` only — not `dB_omega`. This simplifies
  arch-4d's chain rule.

Consistency note (non-blocking): the platform's `A0`-based augmented torsion
already realizes the `T_omega = varpi − epsilon^{-1} d_0 epsilon` structure with
`A0` in the pure-gauge/reference role; keep `epsilon` and the augmented-torsion
reference mutually consistent when both are active.

### FINAL on the `epsilon` realization — INDEPENDENT `H`-valued field (supersedes the slaved detour)

Honest record of the decision path: I first specified `epsilon` as an
independent field (above); on reading arch-4d's first design I endorsed a
*slaved* `eps = eps(omega) = exp(kappa·sum_{e in cell} omega_e)` as a simpler
first pass; arch-4d then correctly pushed back to the independent field. **The
independent `H`-valued field is the right realization and it is now final.** I am
retracting the slaved-form endorsement. Why the independent field is correct, not
merely more faithful:

- **It is the draft's actual field content.** The bosonic connection is the
  inhomogeneous-gauge-group pair `omega_Y = (epsilon, varpi)` with `epsilon in H`
  and `varpi in N` **independent** (`draft:1617`, `G = H ⋉ N`; `draft:2200`,
  `T_omega = varpi − epsilon^{-1} d_0 epsilon`, treats `epsilon` as a field). The
  one-loop effective potential — the object whose log-saturation would give a
  dynamical scale — is a trace-log over the second variation w.r.t. **all**
  physical fields, i.e. the **joint `(varpi, theta)` Hessian**. The slaved form
  computes the wrong Hessian (it collapses `theta` onto a chosen function of
  `varpi`).
- **The slaved form's degree-lift is dismissible as an artifact.** `theta =
  kappa·sum omega_e` is an *arbitrary* functional (why that accumulation? why
  `kappa`?); a critic could attribute its non-affineness to the choice, not to
  physics. The independent field's degree-lift comes from a genuine physical
  fluctuation direction — defensible.

Realization (matches arch-4d's revised §3.5/§3.6):

- `epsilon_v = exp(theta^a_v T_a) in H` per vertex; `theta` is an **independent
  dynamical DOF**, threaded as operator state `EpsilonTheta` (length
  `VertexCount · dimG`) with a separate `LinearizeTheta` for the `theta`-block —
  additive, no `IShiabBranchOperator` signature change.
- `g = Ad_{epsilon_v} = exp(ad_{theta_v})`; `S_h(F)_face = R(Ad_epsilon(F))`.
- Modes: `trivial` (`theta = 0`, not varied — control), `frozen-background`
  (`theta` fixed, not varied — linear), `omega-coupled` = **`epsilon-dynamical`**
  (`theta` varied — the only degree-lifting mode). NB the name "omega-coupled" is
  a misnomer; "epsilon-dynamical" is clearer since `theta` is independent of
  `omega`.
- **Exact `theta`-Linearize:** `delta(Ad_epsilon X) = [∫_0^1 exp(u·ad_theta)
  ad_{delta_theta} exp((1-u)·ad_theta) du] X`, → `[delta_theta, X]` at
  `theta = 0`. FD fallback (Phase436-style) is the pinned first-pass reference.
- `B_omega` does NOT enter (only `Ad_epsilon`); differential `Sigma_mc` deferred.

Probe consequences (design §3.9): the `epsilon-dynamical` arm's Hessian is over
the **joint `(omega, theta)` DOF**, and the rank-1 background ray must carry a
`theta`-component `(u_omega, u_theta)`. Then `Ad_{epsilon(t)} = exp(t·ad_{u_theta})`
is all-orders nonlinear in `t` while `F(t) = t·d(u_omega)` is linear ⇒ `S_h(t)`
non-affine ⇒ joint Hessian degree `> 2`. The `theta = 0` control (pure-`omega`
ray) recovers Phase436 degree-2 exactly — the validation gate is preserved.

Batteries (update §3.7):
- **Isolation battery (answers the skeptic — REQUIRED).** With **identity Shiab**
  (`R = I` / `S = F`), `theta` must NOT appear in `Upsilon` at all (`F` is
  `epsilon`-independent), so the `theta`-Hessian block is degenerate/zero. Assert
  this. It proves the degree-lift is attributable to the **Einsteinian Shiab's
  `epsilon`-dependence**, not to `theta` as a free field.
- **Honesty sweep.** Third `t`-difference of `H(t)` → 0 as `|u_theta| → 0` and
  grows with `|u_theta|` — separates a genuine degree-lift from numerical noise.
- **`Ad` convention:** fix `eps^{-1}(·)eps` vs `eps(·)eps^{-1}` consistently with
  the gauge-covariance battery (§4.5 battery 5).

Record keys (aligned with co-signed design §3.5):
`epsilonRealization = independent-theta-dof`, `hessianOverJointOmegaTheta = true`,
`slavedWilsonKeptAsSmokeTestOnly = true`, `shiabOutputDegree = 2`.

#### `EpsilonMode` taxonomy (co-signed with arch-4d) — three ways to treat ε, do not conflate

The lead's insight (correct): an independent ε solved by its own stationarity
`∂S/∂ε = 0` yields an `omega`-dependent composite `eps*(omega)` — so "slaved" and
"independent" are not opposites, they differ in *how the eps→omega relation
arises*. But there are **three** distinct realizations, and the first study hinges
on not merging the middle one into the third:

1. **`slaved-ansatz`** — `theta = kappa·sum omega_e` **POSTULATED** (Wilson
   ansatz). `omega`-only Hessian. Its degree-lift is *muddied-but-real*: it does
   exhibit the genuine `exp(ad_theta)` non-polynomiality of the eps-conjugation,
   but routes it through an arbitrary `omega`-functional, so it **cannot run the
   isolation battery** and cannot cleanly separate physics from the postulate.
   → OPTIONAL smoke-test, non-gating, NOT the headline.
2. **`epsilon-dynamical` (joint Hessian)** — `theta` an **INDEPENDENT fluctuation
   direction**; compute the **joint `(omega, theta)` Hessian**; *no stationarity
   solve*. This is what memo §6e specifies and what the `EpsilonTheta` /
   `LinearizeTheta` plumbing delivers. Cost is modest (a larger Hessian vector +
   one isolation run — the same Phase436 FD machinery, no new algorithm). The
   isolation battery runs here and certifies the degree-lift is Shiab-caused.
   → **FIRST-PASS HEADLINE.**
3. **`variational`** — `eps*(omega)` obtained by solving `∂S/∂ε = 0` and
   integrating ε out (the true one-loop effective potential; `Linearize` needs the
   implicit-function derivative through the stationarity condition). This is the
   genuinely costly one and is where the eventual dynamical-*scale* extraction
   lives. → DEFERRED faithful upgrade (named follow-up study).

Plus `trivial` (`theta = 0`, control) and `frozen-background` (`theta` fixed,
linear — diagnostic).

**Is the costly `variational` (3) REQUIRED for the first study to be meaningful?
NO.** The first study's question (§5) is the *structural* one — does the
Einsteinian Shiab break the exact-quadraticity Phase436/441 established? — and
that is answered cleanly by the joint Hessian (2) with the isolation battery,
which needs no stationarity solve. `variational` (3) is required for the eventual
*scale* extraction, not for the degree-verdict. So we pin **(2) as first-pass
headline**, **(1) as optional smoke-test**, **(3) as deferred**. (`slaved-ansatz`
is retained as a labelled cross-check, not retracted outright — it is a legitimate
smoke-test, just not the headline.)

**SIGN-OFF (both criteria, unchanged in spirit):** I confirm the Ω²→Ω² carrier
(§3.2) and the independent-`epsilon` conjugator (§3.5/§3.6). Final sign-off on
the built operator clears once (a) `LinearizeTheta` matches its finite-difference
check, and (b) the `theta = 0` control arm reproduces Phase436 degree-2 exactly.
The degree verdict itself does not gate sign-off — a degree-2 Einsteinian result
is a legitimate frontier-sharpening outcome (§5).

### The single most important message to the developers

Building the 4D mesh and the linear eq. 9.3 operator is necessary but will
reproduce the toy no-go if `epsilon = 1`. The scale-breaking physics lives in
the `omega`-dependence of the gauge dressing `epsilon^{-1} Phi epsilon` (the
sigma-field) and, ultimately, the differential canonical branch `Sigma_mc`'s
`d_{B_omega}` term. M3's `omega-coupled` `epsilonMode` is the first place the
Hessian can acquire degree `> 2`. Design and test toward that, and keep the
`epsilon = 1` control arm as the validation gate.
