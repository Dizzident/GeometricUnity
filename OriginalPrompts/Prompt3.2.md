## Chimeric Bundle and Horizontal/Vertical Decomposition

This section formalizes the horizontal/vertical splitting and the chimeric bundle construction in the Einsteinian observerse. The source draft makes these constructions central: once (Y=\mathrm{Met}(X)) is chosen, it introduces tangent/cotangent relations on (Y), a vertical bundle (V), a horizontal cotangent bundle (H^*=\pi^*(T^*X)\subset T^*Y), a Frobenius metric on vertical directions, and then the chimeric bundles
[
C(Y)=V\oplus H^*,\qquad C^*(Y)=V^*\oplus H.
]
It also claims that these are “canonically” or “semi-canonically” related to (TY) and (T^*Y), and that this is the geometric bridge needed for the later spinor program.   

The completion document keeps that architecture but makes three changes. First, it separates what is actually defined in the draft from what is only sketched. Second, it fixes the ambient bundle-theoretic language so that later spinor and connection sections can refer back to this section without silently changing conventions. Third, it records all missing existence, uniqueness, and compatibility facts as explicit proof obligations rather than treating them as already settled. That is consistent with the completion outline, which places proto-Riemannian geometry before the chimeric bundle, and then treats the semi-canonical relation to (TY) and (T^*Y) as only partially defined and in need of proof.  

### 9.1 Standing setup

We work in the Einsteinian observerse from the previous section. Thus (X) is a smooth oriented spin 4-manifold, (Y=\mathrm{Met}(X)) is the chosen metric bundle over (X), and
[
\pi:Y\to X
]
is the bundle projection. The completion document treats (Y) as the open subbundle of (\mathrm{Sym}^2(T^*X)) consisting of nondegenerate symmetric bilinear forms in a fixed admissible signature sector. This bundle-level model is not fully normalized in the draft, but it is the natural formal completion of the draft’s repeated use of (Y) as the bundle of pointwise metrics over (X).  

### 9.2 Tangent and cotangent structure on (Y)

The draft’s proto-Riemannian discussion starts from the claim that, unlike the usual metric or symplectic setting where tangent and cotangent bundles are canonically related by a chosen geometric structure, the Einsteinian observerse gives natural but non-isomorphic maps
[
TY \to T^*Y,\qquad T^*Y \to TY,
]
with nontrivial kernels, and that these fit into a long exact pattern from which the chimeric bundle emerges.  

For the completion document, the rigorous starting point is simpler:

**Definition 9.2.1 (Vertical bundle).**
The vertical bundle of the fibration (\pi:Y\to X) is
[
V:=\ker(d\pi)\subset TY.
]

This is standard for any smooth fiber bundle, so existence and uniqueness of (V) as a smooth vector subbundle follow once (Y\to X) is fixed as a smooth bundle. In the present setting this is the cleanest part of the draft’s geometry: the draft explicitly defines (V\subset TY) as the vectors pointing along the fibers of (Y) over (X).  

**Definition 9.2.2 (Horizontal cotangent bundle).**
The horizontal cotangent bundle is
[
H^*:=\pi^*(T^*X).
]

The draft explicitly defines (H^*=\pi^*(T^*X)) and embeds it into (T^*Y). The completion document takes that inclusion to be the canonical pullback inclusion induced by the projection map (\pi). 

**Definition 9.2.3 (Horizontal bundle).**
The horizontal bundle is the dual bundle
[
H:=(H^*)^*.
]

This matches the draft’s statement that the dual of (H^*) will simply be called (H). 

### 9.3 Exact sequences and what they do and do not provide

Once (V) is defined, there is a standard short exact sequence of vector bundles
[
0\longrightarrow V \longrightarrow TY \xrightarrow{,d\pi,} \pi^*(TX)\longrightarrow 0.
]
Dually, there is a short exact sequence
[
0\longrightarrow \pi^*(T^*X)\xrightarrow{,\pi^*,} T^*Y \longrightarrow V^* \longrightarrow 0.
]
Since (\pi^*(T^*X)=H^*), this becomes
[
0\longrightarrow H^* \longrightarrow T^*Y \longrightarrow V^* \longrightarrow 0.
]

These two short exact sequences are the rigorous bundle-theoretic content behind the draft’s diagrammatic presentation. The draft states that the vertical sequences are short exact and that the central horizontal sequence is long exact, with remaining arrows being metric isomorphisms.  

For the completion document, however, one must be careful:

**Remark 9.3.1.**
The short exact sequences above are rigorous and standard. The draft’s larger “repeating long exact sequence” is not yet rigorous as stated in the visible material. In particular, the exact definition of the horizontal arrows, the sense in which they repeat, and the hypotheses under which the unspecified maps are “metric isomorphisms” are not fully supplied in the draft text visible here. 

**Proof obligation 9.3.2.**
A later technical appendix must reconstruct the full commutative diagram in a standard categorical form, specify every arrow, and prove exactness at each node.

So the completion document adopts the short exact sequences as foundational, and treats the larger long-exact-style diagram as a heuristic source claim pending reconstruction.

### 9.4 Fiberwise identification of the vertical bundle

The draft identifies vertical tangent directions with symmetric bilinear forms varying along the metric fiber: at a point (y\in Y), vertical tangent vectors are represented by derivatives of smooth paths of nondegenerate metrics through (y), hence by symmetric two-tensors. It then defines the Frobenius inner product by double contraction against the metric represented by the point (y). 

The completion document formalizes this as follows.

**Proposition 9.4.1 (Fiberwise model for (V)).**
Let (y\in Y_x\subset \mathrm{Sym}^2(T_x^*X)) be a nondegenerate symmetric bilinear form on (T_xX). Then there is a canonical vector-space identification
[
V_y \cong \mathrm{Sym}^2(T_x^*X).
]

**Explanation.**
Because (Y_x) is an open subset of the vector space (\mathrm{Sym}^2(T_x^*X)), its tangent space at (y) is canonically identified with (\mathrm{Sym}^2(T_x^*X)). Since the fiber (Y_x) is the fiber of (\pi), this tangent space is exactly the vertical tangent space (V_y).

This proposition is one of the few places where the draft’s geometric intention can be made fully rigorous with little extra invention. It also explains why the draft may treat vertical vectors as symmetric tensors (A,B). 

**Inserted Assumption 9.4.2 (Smooth bundle model for (Y)).**
The above identification is used globally because (Y) has been fixed as an open subbundle of (\mathrm{Sym}^2(T^*X)).

This assumption is stronger than the draft’s informal language but is already implicit in its use of (Y) as the metric bundle.

### 9.5 Metrics on (H^*), (H), and (V)

The draft states that at a point (g\in Y), the bundle (H^*=\pi^*(T^*X)) carries an induced metric via the pulled-back metric (g), and that the vertical space carries a natural Frobenius metric defined by double contraction against (g).  

The completion document fixes this as follows.

**Definition 9.5.1 (Horizontal metric on (H^*)).**
For (y\in Y_x), write (g_y) for the nondegenerate bilinear form represented by (y). Then (g_y) induces a nondegenerate bilinear form on (T_x^*X), hence on the fiber
[
H_y^* \cong T_x^*X.
]
This defines a smooth fiber metric on (H^*).

**Definition 9.5.2 (Horizontal metric on (H)).**
The dual metric on (H=(H^*)^*) is the dual metric induced from the metric on (H^*).

**Definition 9.5.3 (Frobenius metric on (V)).**
For (y\in Y_x), identify (V_y\cong \mathrm{Sym}^2(T_x^*X)). The Frobenius bilinear form at (y) is
[
\langle A,B\rangle_{V,y}
:= \mathrm{Tr}_{g_y}(A^\sharp \circ B^\sharp),
]
equivalently the double contraction of (A) and (B) using (g_y).

This is the coordinate-free version of the draft’s matrix formula (\mathrm{Tr}(A^TB)=\mathrm{Tr}(AB)) in an orthonormal basis. 

**Remark 9.5.4.**
The draft further decomposes the vertical fiber into trace and traceless parts and records a signature count, with a free sign choice on the trace direction and a specific choice made in the ((1,3)) sector, giving signature ((4,6)) in dimension four.   For the completion document, that signature statement should be retained only after a clean derivation is supplied.

**Proof obligation 9.5.5.**
Supply a full derivation of the signature of the Frobenius metric on (\mathrm{Sym}^2(T_x^*X)) for arbitrary base signature ((i,n-i)), and verify the stated ((4,6)) result in the (n=4), (i=1) sector.

### 9.6 The chimeric bundles

The draft explicitly defines two metric bundles
[
C(Y)=V\oplus H^*,\qquad C^*(Y)=V^*\oplus H,
]
and says they are canonically isomorphic to each other and semi-canonically related to tangent and cotangent bundles. 

The completion document adopts the definitional part but weakens the isomorphism claims to the precise level supported by the visible source.

**Definition 9.6.1 (Chimeric bundles).**
The chimeric bundle and dual chimeric bundle are
[
C:=V\oplus H^*,\qquad C^*:=V^*\oplus H.
]

This matches the draft exactly at the level of bundle definition. 

**Definition 9.6.2 (Chimeric metrics).**
Choose a sign (\varepsilon\in{+1,-1}). The chimeric metric on (C) is
[
g_C^{(\varepsilon)}:=\langle\cdot,\cdot\rangle_V \oplus \varepsilon,\langle\cdot,\cdot\rangle_{H^*},
]
with (V) and (H^*) declared orthogonal. Dually, (C^*) carries the dual metric.

This formalizes the draft’s statement that the vertical and horizontal pieces each carry natural metrics and that, by taking them orthogonal, their direct sum inherits two sign-dependent metrics. 

**Inserted Convention 9.6.3.**
The sign choice (\varepsilon) is treated as a genuine free convention unless and until later spinorial, representation-theoretic, or phenomenological constraints force one choice.

This reflects the draft’s own note that the sign choice is one of the few non-forced choices in the strong form. 

### 9.7 Relation to (TY) and (T^*Y)

This is where rigor becomes essential. The draft says (C) and (C^*) are “semi-canonically” isomorphic to the tangent and cotangent bundles and presents a diagram linking them to the exact sequences.  But from the standpoint of ordinary bundle theory, (V\oplus H^*) is not canonically the same object as (TY), and (V^*\oplus H) is not canonically the same object as (T^*Y), unless additional splitting or identification data are supplied.

The completion document therefore separates three distinct levels.

**Level 1: canonical exact-sequence data.**
What is canonical is:
[
0\to V\to TY\to \pi^*TX\to 0,\qquad
0\to H^*\to T^*Y\to V^*\to 0.
]
These are forced by the bundle (\pi:Y\to X).

**Level 2: noncanonical splitting data.**
To obtain an isomorphism
[
TY \cong V\oplus \pi^*TX
]
one needs a choice of splitting of the tangent exact sequence, equivalently an Ehresmann connection on (\pi:Y\to X). Likewise, to identify
[
T^*Y \cong H^*\oplus V^*
]
one needs the dual splitting.

**Level 3: metric-dependent identifications.**
To compare (\pi^*TX) with (H^*) or (V^*) with (V), one needs fiber metrics. Those metrics are available here because each point (y\in Y) is itself a metric on (X), and because the Frobenius form gives a metric on (V). But these are metric identifications, not purely topological ones.

This yields the correct statement:

**Proposition 9.7.1 (Conditional semi-canonical identification).**
Given:

1. a splitting (s:\pi^*TX\to TY) of the tangent exact sequence, and
2. the fiber metrics on (H^*) and (V),

there are induced bundle isomorphisms
[
TY \cong V\oplus \pi^*TX \cong V\oplus H^*=C,
]
and dually
[
T^*Y \cong H^*\oplus V^* \cong V^*\oplus H=C^*.
]

These isomorphisms are not canonical in the strict sense. They are canonical only relative to the chosen splitting and the already available fiber metrics.

This is the rigorous version of what the draft appears to mean by “semi-canonical.” It also matches the completion scaffold’s assessment that the structural relation between ((C,C^*)) and ((TY,T^*Y)) is only partially defined and that the precise global morphisms and uniqueness conditions are not stabilized. 

### 9.8 Horizontal/vertical decomposition: what is actually canonical

Because the word “horizontal” is often used for a chosen complement of (V) inside (TY), the draft’s notation requires care. In the visible text, (H^*=\pi^*(T^*X)\subset T^*Y) is canonical, while (H=(H^*)^*) is also canonical as an abstract dual bundle. But no canonical subbundle
[
\widetilde H \subset TY
]
complementary to (V) has yet been constructed in the visible draft. Such a subbundle would require a splitting of the tangent exact sequence, equivalently an Ehresmann connection on (\pi). The later Zorro construction is exactly where the draft intends to produce connection data on (Y), so that splitting belongs downstream, not here. 

Accordingly:

**Inserted Convention 9.8.1.**
In this section, (H) denotes ((H^*)^*), not a chosen horizontal subbundle of (TY).

**Remark 9.8.2.**
Any later notation using (H\subset TY) must explicitly indicate the extra splitting or connection that realizes such an inclusion.

This is one of the most important compatibility clarifications, because otherwise the chimeric bundle can be mistaken for an actual decomposition of (TY) before the needed connection data have been supplied.

### 9.9 Existence, uniqueness, and compatibility ledger

This section is intended to be rigorous, so the missing details must be stated plainly.

#### Existence facts already secured

The following objects exist once the Einsteinian observerse (Y=\mathrm{Met}(X)\to X) is fixed:

1. the vertical bundle (V=\ker(d\pi)\subset TY);
2. the pullback cotangent bundle (H^*=\pi^*(T^*X));
3. the dual bundle (H=(H^*)^*);
4. the short exact sequences relating (V,TY,\pi^*TX) and (H^*,T^*Y,V^*);
5. the bundle direct sums (C=V\oplus H^*) and (C^*=V^*\oplus H);
6. the fiberwise metrics on (H^*), (H), and (V), hence the sign-dependent chimeric metrics on (C) and (C^*).   

#### Missing existence results

The following are not yet established in the visible draft and must be treated as open proof obligations:

1. a globally defined splitting (TY\cong V\oplus \widetilde H) with (\widetilde H\subset TY);
2. the global long-exact-style diagram asserted in the draft with all arrows explicitly defined;
3. the existence of a preferred or natural splitting compatible with later metric transfer and spinor constructions;
4. global spin or (\mathrm{Spin}(p,q)) admissibility of the chimeric bundle for the chosen signature sector.  

#### Missing uniqueness results

The visible draft does not prove uniqueness of:

1. the sign choice in the chimeric metric;
2. any splitting of the tangent exact sequence;
3. any induced semi-canonical isomorphism (C\cong TY) or (C^*\cong T^*Y);
4. any later topological-to-metric spinor identification built on these data.  

#### Missing compatibility results

The visible draft also leaves unproved the following compatibility statements:

1. smoothness compatibility of the Frobenius metric with changes of local trivialization of (Y);
2. compatibility of the chimeric metric with any later chosen splitting (TY\cong V\oplus \widetilde H);
3. compatibility of the sign choice with the later (\mathrm{Spin}(7,7))-type structure invoked downstream;
4. compatibility of observation pullbacks (\iota^*(TY)=TU\oplus N_\iota) with the chimeric splitting data;
5. compatibility between the semi-canonical identifications here and the Zorro-induced connection data later.  

### 9.10 Formal status of the section

The completion document therefore classifies the material in this section as follows.

The definitions of (V), (H^*), (H), (C), and (C^*) are formal and stable. The Frobenius metric and the induced chimeric metrics are formal up to the explicit sign convention and the need for a cleaned-up signature derivation. The relation to (TY) and (T^*Y) is only conditional and should be labeled **semi-canonical relative to chosen splitting data**, not canonical simpliciter. The larger commutative/long exact diagram is presently heuristic unless reconstructed fully. The existence of a preferred horizontal subbundle inside (TY) belongs to the later connection chapter, not to the present one.  

### 9.11 Section summary

The rigorous core of the draft’s proto-Riemannian/chimeric geometry is this: in the Einsteinian observerse, the metric bundle (Y\to X) has a canonical vertical bundle (V), a canonical pullback cotangent bundle (H^*=\pi^*(T^*X)), and therefore chimeric bundles
[
C=V\oplus H^*,\qquad C^*=V^*\oplus H
]
carrying natural sign-dependent fiber metrics. What is *not* yet rigorous in the draft is any unconditional identification of these bundles with (TY) and (T^*Y), because that requires additional splitting data and compatibility proofs not yet supplied. Those missing pieces are not minor technicalities; they are exactly the bridge needed before the later topological spinor, Zorro, and principal-bundle sections can be considered mathematically complete.  

I can write the next section as **Spinors Without a Prior Metric** and build it directly on this corrected chimeric-bundle formalism.
