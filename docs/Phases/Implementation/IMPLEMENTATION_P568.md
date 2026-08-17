# Phase 568 implementation — path-ordered-curvature downstream counterfactual audit

Phase568 is a deterministic, zero-sampling comparison between the frozen
registered complete-lattice action and the exact study-local candidate emitted
by Phase567.  It neither modifies `CurvatureAssembler` nor introduces the
candidate into production code.

The phase compile-links and exact-binds Phase567's study-local candidate
curvature, forward differential, and Euclidean transpose. It retains only a
separate registered-order reconstruction for the frozen-core control. Before
any full-lattice comparison, it requires:

- reconstruction of the registered curvature, action, and gradient against
  the core implementation;
- candidate differential, transpose-dot-product, directional-gradient, and
  quartic-polynomiality controls;
- equality of registered and candidate Hessian actions at the origin; and
- preservation of the 252-dimensional exact closed-form origin null basis.

The outcome arm reads the six exact-bound Phase548 checkpoint positions in the
same order used by Phase550.  It reports old/new actions, gradient norms and
directions, homogeneous degree-two/three/four components, and a complete dense
candidate Hessian spectrum.  Registered spectra are reused from Phase550.
Negative-inertia counts use one shared per-position roundoff floor derived from
both spectra, preventing branch-specific thresholds from biasing the result.

Only one 3645-by-3645 candidate Hessian and one factorization copy exist at a
time.  A deterministic resource estimate is checked before allocation.  No
RNG, HMC, trajectory replay, configuration retention, protected seed, gauge
quotient, or normalization occurs.

The terminal taxonomy separates invalid inputs, resource refusal,
implementation failures, origin-equivalence failure, spectrum-validation
failure, complete removal of audited negative inertia, uniform reduction,
mixed response, and no reduction.  Every valid scientific terminal remains a
workbench counterfactual.  It does not source-select the candidate, reinterpret
Phase548 or Phase550, open Phase561, satisfy O4 or Phase458, create a Phase481
pack, authorize production, or support a physical-unit claim.

The v1 run was stopped after its first checkpoint row and emitted no terminal
artifact. V2 retains the exact v1 contract and records the partial attempt as a
non-terminal incident. Before any dense allocation, v2 machine-checks both the
committed and independently derived runtime resource estimates, all resource-
governance booleans, every authority firewall, the interpretation rules,
external-review status, and the zero-claim boundary.

V3 additionally exact-binds the finalized program, both predecessor contracts,
and the v1 stop artifact. It asserts the predecessor metadata, the absence of a
v1 terminal according to that exact-bound artifact, every required
implementation-control flag, all spectral comparison firewalls, and equality
between the committed checkpoint count, the actual frozen list, and the
resource arithmetic. This is a governance-only strengthening of v2.
