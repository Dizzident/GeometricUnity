## Mathematical-to-Computational Lowering

This section specifies how the **Minimal GU v1** formal objects are converted into discrete numerical objects suitable for simulation. Its role is to define the lowering map from the continuous mathematical model to an implementable computational model without adding new physical claims. This matches the completion document’s separation between mathematical completion, computational implementation, and empirical testing, and it belongs exactly in the executable-mathematics phase anticipated by the completion outline.  

The lowering procedure must respect the structure already fixed in the minimal specification:

* a base manifold (X^4),
* an observerse branch (Y \xrightarrow{\pi} X) with chosen observation section (\sigma),
* a principal bundle (P \to Y),
* a connection (\omega) as the primary dynamical field,
* curvature (F_\omega),
* a torsion-type term (T_\omega),
* a Shiab-type term (S_\omega),
* the bosonic residual
  [
  \Upsilon_\omega = S_\omega - T_\omega,
  ]
* and the second-order residual equation
  [
  D_\omega^* \Upsilon_\omega = 0.
  ]

This is the smallest branch that preserves the draft’s connection-centered dynamics, the first-/second-order pairing, and the distinction between native (Y)-fields and observed (X)-fields. The draft itself repeatedly centers curvature, connection, pullback observation, and the first-/second-order “Dirac pair” logic, while the completion outline explicitly reserves a dedicated chapter for computable geometric objects, discretization strategy, simulation architecture, and validation.   

### 1. Lowering principle

The computational model is not a separate theory. It is a **typed discretization** of the mathematical model. Every continuous object in Minimal GU v1 must therefore be assigned:

1. a discrete carrier,
2. a finite representation,
3. an assembly rule,
4. a numerical update rule or solver role,
5. and a reconstruction rule back to continuous-looking diagnostics.

No continuous symbol is allowed to enter code without an explicit discrete counterpart. This follows directly from the completion document’s requirement that bundles, sections, connections, curvature, operator representations, and variational functionals all become computable objects before simulation claims are made. 

### 2. Two-level architecture

The lowering should be carried out in two stages.

#### 2.1 Symbolic level

The symbolic level preserves the mathematical types:

* manifold,
* chart,
* bundle,
* section,
* connection,
* curvature,
* operator,
* residual,
* action,
* observation map.

This corresponds to the “symbolic data model” and “algebraic objects to encode” called for in the executable-mathematics outline. 

#### 2.2 Numerical level

The numerical level replaces those objects by finite data:

* mesh or lattice for (Y),
* sampled or indexed representation of (X),
* finite-dimensional gauge variables,
* sparse operator matrices,
* quadrature rules,
* nonlinear residual vectors,
* linearized Jacobian operators,
* and observed diagnostic fields on sampled (X)-locations.

The symbolic level defines correctness; the numerical level enables simulation.

### 3. Discrete domains

#### 3.1 Discrete base (X)

The base manifold (X) is lowered to a finite sample domain
[
X_h = {x_i}_{i=1}^{N_X}
]
or to a mesh (\mathcal M_X) if observation diagnostics require interpolation or field comparison on (X).

Because v1 does not solve native dynamics on (X), (X_h) is primarily an **observation domain**. It stores:

* sampled observation points,
* connectivity if visualization is needed,
* and pullback targets for observed fields.

#### 3.2 Discrete observerse (Y)

The true simulation domain is the discretization of (Y),
[
Y_h = (\mathcal V,\mathcal E,\mathcal F,\ldots),
]
where (Y_h) may be realized by one of three branch choices:

* finite element mesh,
* structured grid,
* lattice or cell complex.

The completion outline explicitly allows finite-element, spectral, or lattice strategies, but also requires that the chosen discretization be gauge-compatible and accompanied by stability and error-analysis targets. 

For Minimal GU v1, the preferred lowering is:

* **finite elements** for smooth variational solves on curved (Y),
* or **lattice gauge discretization** when gauge covariance is the dominant concern.

The document should not force a single universal method, but the data schema must support both.

### 4. Discrete observerse maps

The continuous maps
[
\pi:Y\to X,\qquad \sigma:X\to Y
]
become discrete incidence/interpolation operators.

#### 4.1 Projection map

The projection (\pi) becomes a lookup or interpolation map
[
\pi_h : Y_h \to X_h.
]
Computationally, (\pi_h) is stored as either:

* an index map from each (Y)-cell or quadrature point to a containing (X)-cell,
* or a barycentric interpolation rule into (X_h).

#### 4.2 Observation section

The chosen observation branch (\sigma) becomes a sampled embedding
[
\sigma_h : X_h \to Y_h.
]
This is stored as:

* one target point in (Y_h) per observation point in (X_h),
* or interpolation weights into nearby (Y)-degrees of freedom.

This is the computational form of the draft’s observation-by-pullback mechanism, where fields native to (Y) are observed on (X) through the chosen observation map rather than assumed native to (X) from the start. 

### 5. Bundles and fibers

The continuous principal bundle (P \to Y) is lowered by local trivialization over mesh patches.

#### 5.1 Local bundle charts

Choose an atlas or discrete patching of (Y_h). Over each patch, represent (P) as a local trivial bundle with fiber (H). The discrete bundle data is then:

* patch identifiers,
* transition functions on overlaps,
* a chosen representation (\rho:H\to \mathrm{GL}(V)),
* and the Lie algebra basis for (\mathfrak h).

This is enough for v1 because the minimal model only requires a principal bundle, an adjoint bundle, and a connection field; the full chimeric and spinorial bundle stack is postponed. 

#### 5.2 Adjoint bundle discretization

The adjoint bundle (\operatorname{ad}P) is lowered to a field of Lie-algebra coefficient vectors over the cells, nodes, edges, or faces of (Y_h). Numerically this means:

* one (\dim \mathfrak h)-vector per chosen geometric carrier,
* with basis matrices fixed globally or patchwise,
* plus overlap transformations if using multiple charts.

### 6. Discrete connection variables

The mathematical connection (\omega) becomes finite degrees of freedom attached to the chosen discrete carriers.

There are two standard lowerings.

#### 6.1 Finite-element lowering

Represent (\omega) as a discrete Lie-algebra-valued 1-form
[
\omega_h = \sum_a \omega^a \phi_a,
]
where the (\phi_a) are basis 1-forms on (Y_h). The coefficients (\omega^a) are the primary unknowns.

This gives:

* direct compatibility with variational assembly,
* sparse Jacobians,
* and straightforward linearization.

#### 6.2 Lattice-gauge lowering

Represent the connection by link variables
[
U_{ij} \in H
]
assigned to oriented edges of (Y_h). These are the discrete parallel transports. Their Lie-algebra logarithms provide local connection coordinates when needed.

This lowering is especially natural when gauge covariance must be preserved as strongly as possible at the discrete level, which is important because the draft is highly sensitive to failures of gauge covariance and repeatedly contrasts well-behaved curvature objects with ill-behaved direct potential-based constructions. 

### 7. Discrete curvature

The curvature
[
F_\omega \in \Omega^2(Y,\operatorname{ad}P)
]
must be lowered to a discrete 2-form or plaquette object.

#### 7.1 Finite-element curvature

If (\omega_h) is a discrete 1-form, compute
[
F_h = d_h\omega_h + \tfrac12[\omega_h,\omega_h]_h
]
using the discrete exterior derivative and the chosen discrete Lie bracket/product assembly rule.

#### 7.2 Lattice curvature

If link variables are used, curvature is represented by holonomy around minimal faces:
[
U_f = \prod_{e\subset \partial f} U_e,
]
with the discrete curvature extracted from (U_f) by logarithm or other representation-dependent local map.

In either case, the curvature carrier is a face-, cell-, or quadrature-based Lie-algebra vector.

### 8. Lowering of background geometric data

Minimal GU v1 treats the metric on (Y) as fixed background data for adjoints, norms, and integration. Therefore the lowering must include:

* metric tensor samples (g_Y) at nodes, cells, or quadrature points,
* inverse metric samples,
* volume weights,
* Hodge-star surrogates or mass matrices,
* and any background tensors used in defining (T_\omega) or (S_\omega).

Without these, the norm (|\Upsilon_\omega|^2), the formal adjoint (D_\omega^*), and the variational residual cannot be assembled. This directly matches the completion requirement that operator representations and variational functionals be made computable, not merely named. 

### 9. Lowering of torsion and Shiab branches

The most important branch-dependent part of the lowering is the conversion of the chosen continuous maps
[
\omega \mapsto T_\omega,\qquad \omega \mapsto S_\omega
]
into discrete operators.

Because the completion program already recognizes that multiple completion branches are possible and that some free choices materially affect predictions, the computational form must preserve branch traceability rather than hide it. 

#### 9.1 Discrete torsion branch

The chosen torsion branch lowers to a discrete map
[
\mathcal T_h : \omega_h \mapsto T_h.
]

Its implementation type is one of:

* algebraic local map at quadrature points,
* sparse stencil,
* nonlinear sparse operator,
* or face/cell assembly rule.

#### 9.2 Discrete Shiab branch

The chosen Shiab branch lowers to
[
\mathcal S_h : \omega_h \mapsto S_h.
]

Its implementation type is one of:

* first-order operator on curvature data,
* projected curvature transform,
* or mixed local/nonlocal operator.

For the minimal branch, both (T_h) and (S_h) must have the **same discrete carrier type**, so that the residual subtraction is type-correct.

### 10. Discrete bosonic residual

The continuous residual
[
\Upsilon_\omega = S_\omega - T_\omega
]
lowers to a residual vector or residual field
[
\Upsilon_h = S_h - T_h.
]

This object is central. It is the discrete form of the first-order equation, the integrand driver of the second-order functional, and the main diagnostic of convergence. In implementation terms, (\Upsilon_h) is stored as:

* one Lie-algebra coefficient block per discrete 2-form carrier,
* or one block per quadrature sample if assembled weakly.

### 11. Lowering of the action functional

The continuous norm-square functional
[
I_2(\omega) = \frac12 \int_Y \langle \Upsilon_\omega,\Upsilon_\omega\rangle, d\mu_Y
]
becomes the discrete objective
[
I_{2,h}(\omega_h)=\frac12,\Upsilon_h^\top M_\Upsilon,\Upsilon_h,
]
where (M_\Upsilon) is the mass matrix or quadrature-weight matrix for the residual carrier space.

This is the main variational object in the computational model. It provides:

* a scalar objective for minimization,
* a convergence metric,
* and a common interface across discretization strategies.

### 12. Linearization and Jacobian lowering

The continuous Fréchet derivative
[
D_\omega = d_\omega \Upsilon
]
lowers to a discrete Jacobian or tangent operator
[
J_h = \frac{\partial \Upsilon_h}{\partial \omega_h}.
]

Depending on the chosen representation, (J_h) is assembled as:

* a sparse matrix,
* a block sparse matrix,
* a matrix-free operator,
* or a bundle-aware tangent operator acting on perturbations (\delta\omega_h).

The adjoint equation
[
D_\omega^*\Upsilon_\omega = 0
]
lowers to
[
J_h^\top M_\Upsilon \Upsilon_h = 0,
]
possibly with gauge and regularization terms added.

This is the principal computational equation of Minimal GU v1.

### 13. Gauge fixing in discrete form

The completion program already flags gauge handling as necessary before one can claim PDE completeness or simulation readiness. The computational model must therefore lower the chosen gauge condition explicitly rather than assume it is “understood.” 

Let the continuous gauge condition be
[
\mathcal G(\omega)=0.
]
Its discrete form is
[
\mathcal G_h(\omega_h)=0.
]

This is implemented in one of three ways:

1. **penalty form**
   [
   J_h^\top M_\Upsilon \Upsilon_h + \lambda G_h^\top M_G G_h(\omega_h)=0,
   ]
2. **constraint form**
   solve with explicit gauge constraints,
3. **reduced-coordinate form**
   eliminate gauge-redundant degrees of freedom before solve.

For v1, penalty or reduced-coordinate form is sufficient.

### 14. Observation lowering

The observation map is where the native (Y)-simulation becomes an (X)-diagnostic output.

If (Q_h) is a discrete (Y)-field or invariant, the observed field is
[
Q_{\mathrm{obs},h} = \sigma_h^* Q_h.
]

Computationally this means:

* sample (Q_h) at the discrete image of (\sigma_h(X_h)),
* interpolate from the (Y_h) carrier space to those locations,
* and store the result on (X_h).

This preserves the draft’s formal distinction between native fields upstairs and observed fields downstairs, while making the pullback operational for simulation and visualization. 

Typical observed outputs in v1 are:

* pulled-back curvature norms,
* pulled-back residual density,
* pulled-back gauge-invariant scalars,
* and selected effective tensor fields for visualization.

### 15. Discrete state model

For simulation, all lowered objects should be assembled into a single typed state.

**Definition (Computational state for Minimal GU v1).**
A simulation state is the tuple
[
\mathsf{State}_h =
(X_h,Y_h,\pi_h,\sigma_h,P_h,\rho,\omega_h,F_h,T_h,S_h,\Upsilon_h,M,J_h,\mathcal G_h,\mathcal O_h),
]
where:

* (X_h) is the observation discretization,
* (Y_h) is the simulation discretization,
* (P_h) denotes the local bundle-trivialization data,
* (\rho) is the chosen group representation,
* (\omega_h) is the primary unknown,
* (F_h,T_h,S_h,\Upsilon_h) are derived fields,
* (M) collects mass, Hodge, and quadrature operators,
* (J_h) is the linearized residual operator,
* (\mathcal G_h) is the gauge mechanism,
* and (\mathcal O_h) is the observation/extraction layer.

This matches the completion outline’s stated need for a symbolic front end, numerical back end, state representation, constraint handling, and output products. 

### 16. Lowering of solver modes

Minimal GU v1 admits two numerical modes.

#### 16.1 Variational mode

Minimize
[
I_{2,h}(\omega_h)=\tfrac12,\Upsilon_h^\top M_\Upsilon \Upsilon_h
]
subject to gauge handling.

This is the cleanest mode for a first implementation.

#### 16.2 Residual equation mode

Solve
[
J_h^\top M_\Upsilon \Upsilon_h + \text{gauge terms} = 0.
]

This is useful when directly implementing Newton, Gauss-Newton, or matrix-free Krylov solvers.

The completion outline explicitly leaves room for either time evolution or variational solve mode; v1 should choose variational solve mode by default, because the minimal bosonic branch is specified as a residual-minimization problem rather than a physical time-evolution theory. 

### 17. Numerical consistency requirements

The lowering is acceptable only if the following properties hold.

#### 17.1 Type consistency

Each discrete operator must map between the correct carrier spaces. In particular:

* (T_h) and (S_h) must land in the same residual space,
* (J_h) must differentiate residual-space quantities with respect to connection-space variables,
* (\sigma_h^*) must only act on fields whose discrete pullback is defined.

#### 17.2 Gauge consistency

The discrete formulation should preserve exact gauge covariance where possible and should at minimum avoid introducing avoidable gauge artifacts. This is especially important because the draft’s motivation repeatedly turns on the difference between curvature-level covariance and potential-level pathologies.  

#### 17.3 Branch traceability

Every simulation result must record:

* the chosen (H),
* the chosen discrete geometry of (Y_h),
* the torsion branch,
* the Shiab branch,
* the gauge strategy,
* the quadrature and basis order,
* and the observation branch (\sigma_h).

This is required because multiple completion branches are expected, and a numerical result is uninterpretable without branch provenance. 

### 18. Recommended implementation layering

The completion outline already anticipates a symbolic front end, numerical back end, CUDA-targeted kernels, and a C# orchestration layer with a native interop boundary.  A consistent lowering pipeline is therefore:

1. **symbolic schema layer**
   defines manifolds, bundles, operators, and branch metadata;

2. **discretization layer**
   builds (X_h), (Y_h), bundle patches, basis functions, and carrier spaces;

3. **field layer**
   stores (\omega_h) and derived objects (F_h,T_h,S_h,\Upsilon_h);

4. **assembly layer**
   builds mass matrices, Jacobians, and gauge operators;

5. **solver layer**
   minimizes (I_{2,h}) or solves the discrete adjoint residual equation;

6. **observation layer**
   computes (\sigma_h^*Q_h) and invariant diagnostics;

7. **validation layer**
   checks convergence, branch sensitivity, and discretization stability.

### 19. Minimal lowering contract

A numerical implementation may claim to realize Minimal GU v1 only if it provides the following explicit lowering maps:

[
\begin{aligned}
X &\mapsto X_h,\
Y &\mapsto Y_h,\
\pi &\mapsto \pi_h,\
\sigma &\mapsto \sigma_h,\
P &\mapsto P_h,\
\omega &\mapsto \omega_h,\
F_\omega &\mapsto F_h,\
T_\omega &\mapsto T_h,\
S_\omega &\mapsto S_h,\
\Upsilon_\omega &\mapsto \Upsilon_h,\
D_\omega &\mapsto J_h,\
D_\omega^*\Upsilon_\omega &\mapsto J_h^\top M_\Upsilon \Upsilon_h,\
\sigma^*Q &\mapsto \sigma_h^*Q_h.
\end{aligned}
]

If any one of these maps is missing, the computational model is not yet a lowered realization of the mathematical theory; it is only a partial prototype.

### 20. Status of this section

This section should be treated as a **computational completion layer** rather than a new physical postulate. It does not settle the open mathematical ambiguities of the full draft. Instead, it defines the exact standard by which a minimal GU branch becomes simulation-ready: every formal object must become a typed finite object, every continuous equation must become an assembled residual system, and every observational claim must pass through a discrete observation map before it is compared or visualized. That is exactly the boundary the completion document sets between mathematical formalization, executable realization, and later empirical confrontation.  

I can write the next section as **Computable Geometric Objects**, **Discretization Strategy**, or **Minimal GU v1 State Representation** so the computational chapter flows cleanly.
