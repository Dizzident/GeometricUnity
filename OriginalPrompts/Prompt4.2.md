## Falsification and Validation Protocol

This section defines the protocol by which any Geometric Unity output is converted into an observable claim and compared against actual observation. Its purpose is procedural, not rhetorical. The completion document does not assume that GU is correct. It assumes only that, if the framework is to become assessable, then every outward-facing claim must pass through an auditable chain from formal construction to observational comparison. That requirement is already built into the completion scaffold: observed physics must come only after the observation machinery, principal-bundle setup, and dynamical equations are fixed, and the final empirical layer must include a comparison-to-observation framework, explicit falsification criteria, and reproducibility requirements.  

The protocol also follows the document-wide status discipline. A phenomenological statement is never to be cited as if it were a theorem, and no prediction is to be presented without a declared status and dependency chain. This matters especially here because the GU draft contains both strong structural ambitions and incomplete phenomenological claims, including explicit statements that some “algebraic predictions” exist at the level of internal quantum numbers while energy-scale predictions would require further work. The validation protocol therefore begins by classifying outputs before it compares them to data.  

### 1. Purpose and governing rule

The governing rule is:

**No GU claim may enter empirical comparison unless it has first been expressed as a typed observational output with an explicit derivation path, an observable map, a comparison rule, and a falsifier.**

This rule prevents three failure modes that the completion document is designed to avoid. First, it prevents purely interpretive statements from being treated as predictions. Second, it prevents formal objects from being compared directly to experiment without an intermediate observable model. Third, it prevents failed numerical instantiations from being confused with failure of the entire completion program. The completion document explicitly separates formal completion, computational readiness, and falsifiability readiness, and this protocol sits exactly at that interface. 

### 2. Output classes admitted into the protocol

Every candidate output must be placed into one of the three empirical-output classes already anticipated by the completion outline:

1. **Exact structural output**
2. **Semi-quantitative output**
3. **Quantitative output** 

These are interpreted as follows.

An **exact structural output** is a discrete claim such as a representation content, branching pattern, charge assignment, chirality pattern, multiplicity statement, or operator-theoretic implication. Such outputs are not compared to floating-point measurements directly; they are compared to established structural facts of observed physics. A failure here is a structural falsifier.

A **semi-quantitative output** is a claim that predicts hierarchy, ordering, sign, allowed-versus-forbidden channel, qualitative running behavior, or scale relation up to bounded uncertainty or unresolved model dependence. Such outputs are compared using admissible tolerance bands and ranking criteria rather than exact numerical equality.

A **quantitative output** is a numerical prediction with units, normalization, uncertainty, and a reproducible derivation path. Only these outputs may be compared to measured central values and error bars in the standard sense.

A claim that is only an interpretive identification, programmatic goal, or phenomenological analogy is not yet an admitted output of this protocol. It must remain outside the comparison stage until it is upgraded. This is required by the status hierarchy. 

### 3. The seven-stage validation pipeline

Every admitted output must pass through the following seven stages.

#### Stage 1 — Formal source fixation

The claim must be attached to a unique formal source in the completed GU framework. This means identifying:

* the underlying bundles, operators, and fields involved,
* the relevant completion choices and inserted assumptions,
* the theorem, proposition, or conjectural mechanism from which the claim arises,
* and the exact branch of completion used if multiple branches remain open.

No observational comparison is allowed before the claim’s formal origin is fixed. This follows directly from the dependency map, which places prediction claims only after the observation machinery, dynamics, deformation theory, and observed-field decompositions are completed. 

#### Stage 2 — Observable extraction map

The formal claim must then be translated into an **observable extraction map**
[
\mathcal O:\ \text{GU output space} \longrightarrow \text{observable space}.
]

This map is the heart of the protocol. It is the explicit rule that tells us how a formal GU object becomes something that can be checked. Examples include:

* representation content (\mapsto) observed charge multiplets,
* operator spectrum (\mapsto) particle mass candidates,
* effective interaction terms (\mapsto) couplings or mixing matrices,
* low-energy bosonic reduction (\mapsto) Einstein/Yang–Mills/Higgs sector observables,
* deformation stability (\mapsto) whether a predicted sector is robust or fine-tuned.

The draft already provides intended “locations within GU” for many standard physical ingredients, but that appendix is only a placement guide, not yet an observable map. The completion document must therefore require that each such intended location be upgraded into a mathematically explicit extraction rule before any validation claim is made. 

#### Stage 3 — Auxiliary model declaration

Most phenomenological comparisons will require auxiliary modeling beyond the raw GU formalism. This must be declared explicitly. Such auxiliary ingredients may include:

* low-energy reduction ansätze,
* symmetry-breaking or vacuum-selection choices,
* effective-field truncations,
* renormalization prescriptions,
* background choices,
* numerical discretization schemes,
* statistical likelihood models.

These are not to be hidden. They are part of the comparison pipeline and may materially affect predictions. The ambiguity register in the completion scaffold already requires identifying which choices are cosmetic and which materially affect predictions. The validation protocol inherits that requirement and makes it operational. 

#### Stage 4 — Prediction instantiation

Once the observable map and auxiliary model are fixed, the claim is instantiated into one of three deliverables:

* a structural prediction record,
* a semi-quantitative prediction record,
* or a quantitative prediction record.

Each record must include:

* predicted value or structure,
* units and normalization if applicable,
* uncertainty decomposition,
* assumptions used,
* numerical method if any,
* and the exact external data set to be used for comparison.

At this stage the output becomes a registry item eligible for validation.

#### Stage 5 — Comparison against external observations

The prediction is then compared to actual observations using a comparison rule appropriate to its type.

For **exact structural outputs**, comparison is Boolean or logical-constraint based: match, mismatch, or underdetermined. For example, if GU predicts a representation branching inconsistent with observed charge assignments, that is immediate falsification at the structural level.

For **semi-quantitative outputs**, comparison is based on interval consistency, ordering consistency, sign consistency, hierarchy consistency, or likelihood ranking relative to alternatives.

For **quantitative outputs**, comparison uses standard statistical machinery: residuals, normalized residuals, chi-square-type metrics, likelihood scores, posterior predictive checks, or other explicitly declared goodness-of-fit measures. The completion outline already calls for a statistical comparison methodology, tolerance bands, and reproducibility requirements; those become mandatory here. 

#### Stage 6 — Falsification decision

Each comparison must terminate in one of four outcomes:

* **Validated within declared scope**
* **Provisionally consistent but underdetermined**
* **Tension requiring revision**
* **Falsified for this branch/instantiation**

The phrase “validated within declared scope” is intentionally narrow. It does not mean the full theory is proved true. It means only that a specific claim, under specified assumptions and observable mapping, is consistent with the selected observations.

A falsification decision must always specify **what** was falsified:

* the raw structural claim,
* the chosen representation branch,
* the auxiliary low-energy model,
* the numerical implementation,
* or the broader GU mechanism itself.

This is essential because the draft itself distinguishes general ideas from particular instantiations, and the completion document must not allow that distinction to become an excuse for evading clear failure. The protocol therefore records both local falsification and branch-level survival status. 

#### Stage 7 — Reproducibility package

No comparison result counts unless it is reproducible. Each validation result must therefore ship with a reproducibility package containing:

* the exact formal source references,
* all assumptions and inserted choices,
* the observable extraction code or symbolic derivation,
* numerical solver settings,
* external data identifiers,
* statistical scripts,
* and versioned output artifacts.

This requirement is already implied by the executable-mathematics and simulation architecture sections of the completion scaffold, including symbolic reference implementations, discretization choices, output products, and validation against reference implementations. 

### 4. Observable types and comparison modes

The protocol distinguishes five observable families.

#### 4.1 Structural observables

These include group reductions, representation content, charge assignments, chirality patterns, multiplicities, family counts, and allowed operator couplings.

These are tested against known structural facts. They require no floating-point fit, only a rigorous extraction of observed content from the formalism. Structural disagreement is the cleanest kind of falsification and should be prioritized first. This matches the completion roadmap’s emphasis on decomposition and branching results before deeper phenomenology. 

#### 4.2 Effective-field observables

These include whether GU reduces to Einstein-like, Yang–Mills-like, Dirac-like, and Higgs/Klein–Gordon-like sectors in the claimed limits. The draft explicitly presents first- and second-order equations as the source of these recoveries. Validation here means proving and then testing the reduction map, not merely asserting resemblance.  

#### 4.3 Spectral observables

These include masses, effective masses, eigenvalue gaps, mode multiplicities, and stability spectra derived from operators or linearized deformation theory. These require a completed operator theory plus declared numerical methods.

#### 4.4 Coupling observables

These include effective gauge couplings, mixing matrices, Yukawa-like quantities, and vacuum-expectation-value-derived effective parameters. Since the draft explicitly places Yukawa couplings and the cosmological constant in VEV-type structures, any comparison here must specify the vacuum-selection rule and effective truncation used. 

#### 4.5 New-sector observables

These include predicted new particles, dark or looking-glass sectors, exotic spinorial content, or deviations from Standard-Model expectations. The draft explicitly claims algebraic predictions for internal quantum numbers of new particles, but also acknowledges that turning those into energy-scale statements requires additional work. The protocol therefore requires that such claims remain structural or semi-quantitative until a mass-and-production model is truly derived. 

### 5. External data requirements

No claim may be compared to “experiment in general.” Each comparison must declare its external data source category. The minimum categories are:

* accepted structural facts of established theory/experiment,
* curated experimental measurements,
* astronomical or cosmological observations where relevant,
* collider bounds and exclusion regions where relevant,
* lattice or numerical benchmark data where a direct experiment is unavailable,
* and internal symbolic/numerical cross-checks for formal reductions.

For each external source, the protocol must record:

* source name,
* version or release,
* date accessed,
* preprocessing applied,
* and reason that this source is appropriate.

The completion scaffold already says the framework should produce numerically traceable outputs and compare them to external data with reproducibility requirements. This section makes those source declarations mandatory. 

### 6. Statistical and logical comparison rules

The comparison rule depends on the output class.

For **structural outputs**, use logical consistency tests:
[
\text{Pass},\ \text{Fail},\ \text{Underdetermined}.
]
No p-values are needed for a charge mismatch or incorrect branching rule.

For **semi-quantitative outputs**, use ordered or interval comparisons:
[
\text{Predicted interval} \cap \text{Observed interval} \neq \varnothing,
]
plus rank-order checks, sign checks, and monotonicity tests.

For **quantitative outputs**, use declared metrics such as:
[
r_i = \frac{O_i - P_i}{\sigma_i},
\qquad
\chi^2 = \sum_i r_i^2,
]
or other appropriately justified likelihood-based criteria. The key rule is that the metric must be fixed before looking at the result.

In all cases, tolerance bands must be declared in advance and split into:

* observational uncertainty,
* numerical uncertainty,
* model-truncation uncertainty,
* and completion-choice uncertainty.

This decomposition prevents an apparent agreement from being driven entirely by loosely controlled assumptions.

### 7. Falsifier taxonomy

The completion scaffold already anticipates structural, representation-theoretic, dynamical, phenomenological, and simulation falsifiers. The protocol turns these into an operational taxonomy. 

#### 7.1 Structural falsifiers

A structural falsifier occurs when the framework cannot produce the required observed decomposition, representation content, charge pattern, family structure, or operator compatibility. These are the strongest and cleanest falsifiers.

#### 7.2 Representation-theoretic falsifiers

A representation-theoretic falsifier occurs when the branching from GU structures to observed matter/gauge sectors is inconsistent, non-unique in a way that destroys predictive force, or incompatible with known quantum numbers.

#### 7.3 Dynamical falsifiers

A dynamical falsifier occurs when the completed equations are ill-posed, inconsistent, fail to reduce to the claimed effective equations, or yield no physically acceptable solutions in the regime where the draft claims recovery.

#### 7.4 Phenomenological falsifiers

A phenomenological falsifier occurs when an extracted observable disagrees with measured or established observational data beyond declared tolerances.

#### 7.5 Simulation falsifiers

A simulation falsifier occurs when independent implementations of the same formally specified output do not agree, or when numerical convergence fails in a way that invalidates the claimed observable extraction.

### 8. Validation order

The protocol imposes a strict order of validation.

First come **internal mathematical consistency checks**: notation, bundles, signatures, representations, gauge covariance, variational consistency, dimensional analysis, and classical limits. These are already listed in the mathematical validation program and must be passed before any phenomenology is trusted. 

Second come **recovery checks**: can the claimed Einstein-like, Yang–Mills-like, Dirac-like, and Higgs-like sectors actually be extracted in the stated limits? The draft explicitly treats these as central recovery claims, so they are the natural intermediate benchmark.  

Third come **structural phenomenology checks**: charge assignments, family patterns, representation content, and chirality structure.

Only after those pass should the protocol advance to **numerical phenomenology** such as masses, couplings, and new-particle search regions.

This order is not optional. It follows the dependency map’s rule that observed physics must come after the observation mechanism and dynamics are locked down. 

### 9. Validation record template

Every tested claim should be entered in a standard validation record with these fields:

**Validation Record V.n**

* **Claim ID**
* **Prediction class**: exact structural / semi-quantitative / quantitative
* **Formal source**
* **Dependencies**
* **Inserted assumptions and choices**
* **Observable extraction map**
* **Auxiliary model**
* **Numerical method**, if any
* **External data source**
* **Comparison rule**
* **Tolerance model**
* **Outcome**
* **Falsifier status**
* **Reproducibility package location**
* **Notes on branch sensitivity**

This template keeps the empirical layer auditable and prevents later prose from overstating what has actually been tested.

### 10. Minimal standard for saying “compared to observation”

The completion document may say that a GU claim has been compared to observation only if all of the following are true:

1. the claim has a declared status,
2. the formal source is fixed,
3. the observable extraction map is explicit,
4. all auxiliary assumptions are listed,
5. the external data source is named,
6. the comparison rule is declared in advance,
7. the result is reproducible,
8. and a falsifier is stated.

Without those eight items, the document may say only that a comparison program is proposed, not that a real validation has occurred.

### 11. Scope note

This protocol is intentionally stronger than the draft’s rhetorical discussion of the scientific method. The completion document is not the place to defend broad theoretical ambition by loosening empirical standards. Its job is to make GU assessable. A completion program succeeds here not by proving the theory true, but by ensuring that every meaningful claim can, in principle and eventually in practice, survive or fail under explicit comparison to actual observations. That is exactly the standard set by the completion scaffold’s goals of computational readiness and falsifiability readiness. 

The result is a disciplined bridge:

[
\text{formal GU construction}
;\to;
\text{typed prediction}
;\to;
\text{observable extraction}
;\to;
\text{external data comparison}
;\to;
\text{validation or falsification decision}.
]

That bridge is the minimum machinery required for the later empirical claims of the completion document to count as more than interpretation.
