## Minimal GU v1 Specification

This section defines the **smallest internally coherent subset** of the Geometric Unity program that can be written down as an executable mathematical model, implemented numerically, and subjected to structural tests. The purpose of this v1 specification is not to preserve the full ambition of the draft, but to isolate a branch of the formalism that retains the draft’s distinctive core architecture while removing every component whose present form is too ambiguous to support implementation. The draft itself organizes the theory around minimal starting data on (X^4), an observerse with a second space (Y), native field content on different spaces, a distinguished connection, modified torsion, a Shiab-operator family, and paired first- and second-order bosonic equations (\Upsilon_\omega=0) and (D_\omega^*\Upsilon_\omega=0).  

The completion program’s own dependency structure makes clear that a simulation-capable model cannot begin from the full theory draft all at once. The strongest dependency chain runs from normalized definitions through observerse, bundle structure, connection/torsion/operators, and only then to Lagrangians, equations, recovery, and prediction. It also explicitly warns against writing simulation architecture before the operator and equation set are fixed.  The minimal v1 subset is therefore intentionally **branch-selecting** and **completion-inserted**: it freezes enough of the open choices to obtain a definite PDE/variational system, while postponing the unresolved representation-theoretic and phenomenological layers.

### 1. Purpose of the minimal v1 subset

The minimal GU v1 subset is the smallest model that preserves all of the following draft-native ideas:

1. the theory starts from a smooth oriented spin (4)-manifold (X) with very little primitive structure, 
2. the primary dynamical arena is not (X) alone but an observerse involving a second space (Y), with the draft explicitly stating that space-time is to be recovered rather than treated as fundamental, and that metric and other fields are native to different spaces, 
3. the bosonic sector is organized around a first-order quantity (\Upsilon_\omega) and a related second-order equation (D_\omega^*\Upsilon_\omega=0),  
4. observation is represented by a pullback/recovery step from (Y) to (X), not by identifying (Y) and (X) from the outset. 

Everything else is postponed unless it is strictly necessary to make those four ideas executable.

### 2. What v1 includes

The v1 model includes only the following layers:

* a fixed base manifold (X^4),
* a fixed observerse branch ((X,Y,\pi,\sigma)),
* a fixed principal bundle (P\to Y) with chosen compact structure group,
* a connection field (\omega) on (P) as the sole dynamical unknown,
* a fixed choice of the displaced-torsion construction,
* a fixed choice of one Shiab-type operator,
* the first-order bosonic residual (\Upsilon_\omega),
* the second-order bosonic residual equation (D_\omega^*\Upsilon_\omega=0),
* a restricted observation map from (Y)-fields to (X)-fields for diagnostics only.

This aligns with the completion outline’s separation between foundations, symmetry structure, dynamics, observation, and only later representation decomposition and phenomenology. 

### 3. What v1 excludes

The following are **outside** the v1 specification:

* fermionic fields and the Dirac-type block operator of the draft, 
* topological-spinor recovery as a dynamical component rather than a future extension,
* family structure, chirality recovery, imposter-generation claims, and quantum-number assignments, 
* Standard Model identifications, bosonic decomposition claims, and all phenomenological mappings,
* anomaly questions, quantization, renormalization, and quantum consistency,
* any claim that the v1 system already reproduces Einstein, Yang–Mills, Higgs, or observed matter content beyond structural analogy.

These exclusions are required by the document’s own status discipline, which forbids treating interpretive or conjectural layers as if they were already formal results. 

### 4. Minimal formal data

We now fix the primitive data of the v1 model.

**Axiom A.v1.1 (Base manifold).**
Let (X) be a smooth, oriented, spin (4)-manifold. No metric, symplectic, complex, or other geometric structure on (X) is taken as primitive. This matches the draft’s declared starting point. 

**Inserted Assumption IA.v1.1 (Admissible observerse branch).**
There exists a smooth manifold (Y), a smooth surjective submersion (\pi:Y\to X), and a chosen smooth section (\sigma:X\to Y) on the domain of interest. The section is the observation branch used by v1.

**Reason for insertion:** the draft and completion plan require observation/pullback machinery, but existence, regularity, and admissibility details are explicitly marked as open technical questions. 
**Minimality assessment:** strengthening.
**Prediction sensitivity:** medium.

**Inserted Assumption IA.v1.2 (Fixed metric and density on (Y)).**
The manifold (Y) is equipped with a fixed smooth pseudo-Riemannian metric (g_Y) and associated density (\mu_Y). These are treated as background structures in v1 and are not solved for dynamically.

**Reason for insertion:** the second-order operator (D_\omega^*) and the norm (|\Upsilon_\omega|^2) require an adjoint convention and integration measure, both of which the completion notes identify as missing unless operator and density conventions are fixed. 
**Minimality assessment:** conservative for implementation, but branch-selecting relative to the full theory.
**Prediction sensitivity:** high.

**Inserted Assumption IA.v1.3 (Fixed principal bundle).**
Let (H) be a compact Lie group and (P\to Y) a smooth principal (H)-bundle with adjoint bundle (\operatorname{ad}P).

**Reason for insertion:** the completion notes explicitly state that a full PDE statement requires an actual principal bundle, structure group, and bundle targets for curvature, torsion, and (\Upsilon_\omega). 
**Minimality assessment:** necessary.
**Prediction sensitivity:** high.

For v1, (H) should be chosen to make the numerics stable and the bundle theory conventional. A compact matrix group such as (U(1)), (SU(2)), or (SU(2)\times U(1)) is sufficient. The choice is not a phenomenological commitment; it is an implementation branch.

### 5. Dynamical variable

**Definition D.v1.1 (Connection field).**
The configuration space of the v1 model is the affine space (\mathcal A(P)) of smooth connections (\omega) on (P\to Y).

This reflects the draft’s shift of emphasis from ordinary space-time fields to connection-space structures and the completion outline’s requirement that the “space of connections” be fixed before the dynamics are formalized.  

**Definition D.v1.2 (Curvature).**
For (\omega\in\mathcal A(P)), define its curvature by
[
F_\omega \in \Omega^2(Y,\operatorname{ad}P).
]

### 6. Fixed geometric auxiliaries

The draft’s bosonic sector depends on displaced torsion (T_\omega) and a swerved-curvature/Shiab construction (S_\omega), combined as (\Upsilon_\omega=S_\omega-T_\omega).  However, the completion notes also identify the Shiab family as one of the most ambiguous parts of the draft and state that, without a canonical operator, the first-order bosonic equation is not one PDE system but a family of PDE systems indexed by operator choice. 

The minimal v1 branch resolves this by fixing both constructions as part of the model definition rather than trying to derive them uniquely.

**Inserted Choice IX.v1.1 (Torsion branch).**
Fix a smooth affine map
[
\mathcal T:\mathcal A(P)\to \Omega^2(Y,\operatorname{ad}P),\qquad \omega\mapsto T_\omega,
]
to be interpreted as the displaced-torsion term of the v1 branch.

**Inserted Choice IX.v1.2 (Shiab branch).**
Fix a smooth differential operator assignment
[
\mathcal S:\mathcal A(P)\to \Omega^2(Y,\operatorname{ad}P),\qquad \omega\mapsto S_\omega,
]
with the same target as (T_\omega).

**Reason for both choices:** the draft supplies the role of these objects in the equations, but not a unique completion. v1 therefore converts operator ambiguity into explicit branch data rather than hiding it.  

At the implementation level, the simplest admissible branch is:

* (T_\omega) built algebraically from (\omega) and a fixed background tensor on (Y),
* (S_\omega) built from (F_\omega) through a fixed first-order contraction/projection rule.

This is the smallest choice that preserves the draft’s intended “swerved curvature minus displaced torsion” structure while remaining computationally tractable.

### 7. Bosonic residual and actions

**Definition D.v1.3 (Bosonic residual).**
Define
[
\Upsilon_\omega := S_\omega - T_\omega
\in \Omega^2(Y,\operatorname{ad}P).
]

This is the v1 analogue of the draft’s first-order bosonic quantity. 

**Definition D.v1.4 (First-order action).**
Define the first-order bosonic functional
[
I_1(\omega):=\frac12\int_Y \langle \Upsilon_\omega,\Upsilon_\omega\rangle_{g_Y,\operatorname{ad}P}, d\mu_Y.
]

This is the minimal implementation-compatible realization of the draft’s first-order bosonic theory, whose Euler–Lagrange condition is represented there by (\Upsilon_\omega=0). 

**Definition D.v1.5 (Linearization and adjoint).**
Let
[
D_\omega := d_\omega \Upsilon : T_\omega\mathcal A(P)\to \Omega^2(Y,\operatorname{ad}P)
]
denote the Fréchet linearization of (\omega\mapsto \Upsilon_\omega). Let (D_\omega^*) be its formal adjoint with respect to the (L^2)-pairing induced by (g_Y), the chosen inner product on (\operatorname{ad}P), and (\mu_Y).

This supplies the operator normalization that the completion notes say must be fixed globally before the dynamics count as mathematically complete. 

**Definition D.v1.6 (Second-order action).**
Define
[
I_2(\omega):=\frac12\int_Y \langle \Upsilon_\omega,\Upsilon_\omega\rangle, d\mu_Y.
]

In v1, (I_1) and (I_2) coincide at the level of implementation: the first-order theory is the zero-residual condition, while the second-order theory is the stationarity/minimization problem for the residual norm. This preserves the draft’s Dirac-pair logic in minimal form without requiring the full original Lagrangian hierarchy. The draft explicitly presents a second-order bosonic functional as a norm-square of (\Upsilon_\omega) and states that (D_\omega^*\Upsilon_\omega=0) is the associated second-order condition.  

### 8. Equations of the v1 model

The v1 model uses two nested equations.

**Definition D.v1.7 (First-order equation).**
The first-order v1 bosonic equation is
[
\Upsilon_\omega = 0.
]

**Definition D.v1.8 (Second-order equation).**
The second-order v1 bosonic equation is
[
D_\omega^*\Upsilon_\omega = 0.
]

The first-order equation is the exact target of the model. The second-order equation is the practical solver equation and the variational stationarity condition for the residual norm. This mirrors the draft’s claim that the second-order equation is naturally related to the first-order theory and is automatically satisfied when the first-order equation holds. 

### 9. Gauge handling

The completion notes are explicit that a connection-valued PDE is not meaningfully classifiable or numerically well posed until gauge-fixed and that the draft does not yet supply a stabilized gauge-free versus gauge-fixed form.  The v1 model therefore adopts gauge fixing as part of the specification.

**Inserted Assumption IA.v1.4 (Gauge fixing).**
A gauge condition
[
\mathcal G(\omega)=0
]
is fixed as part of the branch definition, and the implemented second-order solve is
[
D_\omega^*\Upsilon_\omega + \lambda,\mathcal G^\dagger \mathcal G(\omega)=0
]
for a chosen penalty parameter (\lambda>0), or equivalently a constrained solve with (\mathcal G(\omega)=0).

**Reason for insertion:** without gauge fixing, local well-posedness and numerical classification are unavailable in the present draft state. 
**Minimality assessment:** necessary for implementation.
**Prediction sensitivity:** medium.

For v1, a Coulomb-type background gauge relative to a reference connection is sufficient.

### 10. Observation and recovered quantities

The draft’s conceptual distinctiveness lies in placing metric and other field content on different spaces and recovering observed physics through observation/pullback.  The minimal v1 model keeps this architecture, but only in a structural, non-phenomenological form.

**Definition D.v1.9 (Observation map).**
For any (Y)-tensorial quantity (Q(\omega)) admissible for pullback, define its observed counterpart on (X) by
[
Q_{\mathrm{obs}}(\omega) := \sigma^*Q(\omega).
]

Only quantities well defined under the chosen observation branch may be pulled back.

**Completion Rule CR.v1.1.**
No observed field obtained by pullback in v1 is to be identified with an actual gravitational, gauge, Higgs, or matter field unless that identification is separately justified in a later phenomenological chapter.

This follows the completion document’s insistence on separating formal constructions from physical mappings. 

In v1, the observation map is used only to produce diagnostics such as:

* pulled-back curvature scalars,
* pulled-back residual norms,
* pulled-back invariant densities,
* effective (X)-space tensor profiles for visualization.

### 11. Function-space and regularity minimum

**Inserted Assumption IA.v1.5 (Regularity).**
Admissible connections lie in a Sobolev class (H^k) with (k) large enough for all products, pullbacks, and adjoint operations in the v1 equations to be well defined. The exact (k) depends on (\dim Y).

This mirrors the completion document’s use of inserted regularity assumptions to make the variational framework executable. 

### 12. What “implemented and tested” means in v1

A model counts as a valid implementation of Minimal GU v1 only if it supports all of the following concrete tasks:

1. **Field representation.** Encode (Y), (P), (\omega), (F_\omega), (T_\omega), (S_\omega), and (\Upsilon_\omega).
2. **Residual evaluation.** Compute (|\Upsilon_\omega|_{L^2(Y)}).
3. **Variational solve.** Numerically solve the gauge-fixed second-order equation or minimize (I_2).
4. **First-order verification.** Check whether the resulting solution also satisfies (\Upsilon_\omega\approx 0).
5. **Observation.** Pull back selected (Y)-quantities to (X) via (\sigma^*).
6. **Branch traceability.** Record the exact operator, torsion, gauge, and bundle choices used.

These requirements match the completion program’s emphasis on executable mathematics, computable geometric objects, discretization, simulation architecture, and traceability from assumptions to outputs.  

### 13. Minimal validation targets

The v1 model is not tested first by comparison with particle data. It is tested first by **internal mathematical and numerical criteria**, because those are the only criteria appropriate to this stage.

The minimum v1 test suite is:

* **well-definedness test:** all operators and pullbacks are type-consistent;
* **gauge-consistency test:** the gauge-fixed solver is stable under small gauge-equivalent perturbations of initial data;
* **residual test:** numerical solutions decrease (I_2) and ideally approach (\Upsilon_\omega=0);
* **branch-sensitivity test:** changing the Shiab or torsion branch changes outputs in a controlled and logged way;
* **observation test:** pulled-back observables on (X) are smooth and numerically reproducible;
* **discretization test:** refinement reduces residual and stabilizes observed diagnostics.

These tests correspond to the completion program’s consistency-check layer and its later simulation/validation pipeline.  

### 14. Why this is the smallest coherent subset

This subset is minimal because removing any one of the following destroys the draft’s distinctive structure:

* remove (Y), and the observerse/recovery architecture disappears;
* remove (\omega), and the connection-centered dynamics disappear;
* remove (T_\omega) or (S_\omega), and the defining bosonic residual (\Upsilon_\omega) disappears;
* remove (D_\omega^*\Upsilon_\omega=0), and there is no practical variational equation to solve;
* remove (\sigma^*), and there is no observation step from native (Y)-fields back to (X).

At the same time, everything excluded from v1 is excluded because the completion notes identify it as still dependent on unresolved existence questions, structure-group ambiguities, operator nonuniqueness, or phenomenological branching.  

### 15. Status of the v1 model

The Minimal GU v1 Specification should be treated as:

**Status:** inserted
**Source basis:** mixed; draft-native architecture with completion-inserted branch choices
**Dependencies:** normalized observerse, fixed bundle data, fixed operator conventions, fixed regularity class
**Proof status:** none as a full theory; sufficient as an executable model specification
**Prediction sensitivity:** high with respect to operator, bundle, metric, and gauge choices

The correct interpretation is therefore not “this is Geometric Unity completed,” but rather:

**this is the first executable bosonic branch of the Geometric Unity completion program.**

It is the smallest subset that can be discretized, solved, stress-tested, and used to determine whether the formal core has enough coherence to justify extending the program to fermions, decomposition, and observational comparison.

If you want, I can write the next companion section as either **“Minimal GU v1 Data Model and State Representation”** or **“Minimal GU v1 Numerical Workflow and Solver Loop.”**
