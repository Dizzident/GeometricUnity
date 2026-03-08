## Claim-Status Ledger

This ledger separates the extracted *Geometric Unity* material into five status classes: axioms, definitions, propositions, conjectures, and phenomenological claims. It follows the completion document’s rule that primitive commitments, fixed meanings, derived results, conjectural statements, and formal-to-physical mappings must not be allowed to blur together. It also follows the rule that, when classification is uncertain, the weaker and more explicit label should be preferred.  

The ledger below is intentionally conservative. It classifies only what can be extracted from the draft fragments and the completion scaffold now in hand. Where the draft appears to treat a statement as established but the visible material does not contain a proof, the item is either recorded as a proposition with deferred proof status or demoted to conjecture. Where a statement connects formal structures to Einstein, Dirac, Yang–Mills, Higgs, family, or observed-sector content, it is recorded as a phenomenological claim rather than as a formal theorem.   

### A. Axioms

These are the minimal primitive commitments that the completion document may safely elevate to axioms because they function as foundational starting data rather than derived consequences. In the current extracted material, only a small number qualify cleanly. Everything else should remain a definition, inserted assumption, or conjecture until the source text is reconstructed more fully.  

**Axiom A.7.1 (Admissible base structure).**
Let (X^4) be a smooth, oriented, spin 4-manifold. The draft treats this as the minimal starting substrate and explicitly emphasizes that no metric or richer geometric structure is assumed at the outset.
**Status:** formal
**Source basis:** draft-native
**Used later in:** observerse, chimeric bundles, spinors, recovery program
**Proof status:** none required 

**Axiom A.7.2 (Minimal-data posture).**
The completion program may take as primitive that the draft intends familiar physics to be recovered from very limited initial data rather than postulated in standard space-time form. This is structurally central, but its exact formal boundary is not stabilized in the visible draft, so it should be treated as a weak governing axiom or else downgraded to an inserted methodological assumption if stricter formalism is preferred.
**Status:** formal but weak
**Source basis:** draft-implicit
**Used later in:** recovery layer, interpretation of native versus observed fields
**Proof status:** none required
**Warning:** boundary with inserted assumptions remains unclear 

### B. Definitions

The draft is richest at the level of named objects and structural constructions. Most of the cleanly extractable material belongs here. The completion outline also identifies definitions as the main content of the normalized core vocabulary chapters, which is consistent with what is actually recoverable from the source.  

**Definition D.8.1 (Observerse).**
An observerse is a triple ((X^n,Y^d,\iota)) with local maps into (Y) inducing pullback metric and normal-bundle structure; trivial, Einsteinian, and ambient cases are distinguished in the draft.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** A.7.1 

**Definition D.8.2 (Native and invasive fields).**
A native field lives on its own ambient space (X) or (Y); an invasive field on (X) is obtained by pullback from (Y).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.8.1 

**Definition D.9.1 (Einsteinian observerse).**
The draft treats (Y=\mathrm{Met}(X)) as a principal strong-form setting, with sections corresponding to metric fields, although admissible signature sectors and global normalization remain incomplete.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.8.1
**Warning:** partially defined globally 

**Definition D.9.2 (Observation map / pullback).**
The pullback (\iota^*) is the basic operation by which metric and other observed structures are recovered from (Y) onto (X).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.8.1
**Warning:** global recovery role not fully unified in the visible text 

**Definition D.10.1 (Horizontal and vertical structures).**
The draft explicitly defines the horizontal bundle (H^*=\pi^*(T^*X)\hookrightarrow T^*Y), its dual (H), and the vertical bundle (V\subset TY).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.9.1 

**Definition D.10.2 (Frobenius metric on (V)).**
The vertical directions carry a natural Frobenius inner product, with a signature choice singled out for the physically relevant ((1,3)) sector.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.10.1 

**Definition D.10.3 (Chimeric bundles).**
The chimeric bundles are (C(Y)=V\oplus H^*) and (C^*(Y)=V^*\oplus H).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.10.1, D.10.2 

**Definition D.11.1 (Topological or chimeric spinors).**
The draft introduces spinors on the chimeric bundle in order to obtain spinorial structure prior to an ordinary metric on (X).
**Status:** formal
**Source basis:** draft-implicit to draft-native
**Dependencies:** D.10.3
**Warning:** bundle-theoretic admissibility remains incomplete 

**Definition D.11.2 (Metric spinors).**
Metric spinors are the observed or induced spinorial structures that arise once metric data are in play.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.11.1
**Warning:** transition from topological spinors is only partially formalized 

**Definition D.11.3 (Spinor functor exponential property).**
The draft explicitly uses the identity (/!S(W_a\oplus W_b)\cong /!S(W_a)\otimes /!S(W_b)) as a structural ingredient of the chimeric spinor program.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** none beyond spinor conventions 

**Definition D.12.1 (Induced connection on (Y) / Zorro construction).**
The draft presents a metric-to-connection transmission chain from Levi-Civita data on (X) to induced connection data on (Y).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** A.7.1, D.9.1
**Warning:** construction details remain incomplete 

**Definition D.17.1 (Distinguished connection (A_0)).**
(A_0) is the preferred connection used as the origin of the affine connection picture and as the anchor for the tilted gauge structure.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.12.1
**Warning:** intrinsic construction not fully pinned down in the extracted text 

**Definition D.17.2 (Bi-connection map and associated connections).**
The draft explicitly defines (\mu_{A_0}:G\to A\times A) and the induced (A_\omega)- and (B_\omega)-connections.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.17.1, D.16.1
**Proof status:** definition-level only 

**Definition D.18.1 (Augmented or displaced torsion (T_\omega)).**
(T_\omega) is introduced as the modified torsion object needed in the bosonic field equations.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.17.2
**Warning:** full formula not visible in the present extracted text 

**Definition D.18.2 (Swerved curvature (S_\omega)).**
The draft labels the curvature-derived term in the first-order bosonic equation as swerved curvature or swervature.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** curvature sector, D.17.2
**Warning:** only equation-level presentation is visible  

**Definition D.18.3 (Combined bosonic field (\Upsilon_\omega)).**
The central bosonic quantity is (\Upsilon_\omega=S_\omega-T_\omega).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.18.1, D.18.2  

**Definition D.19.1 (Shiab operator family).**
The draft architecture clearly requires a family of Shiab operators, but the visible text does not contain a stable full definition.
**Status:** formal but incomplete
**Source basis:** draft-native
**Dependencies:** operator chapter
**Warning:** this is one of the strongest ambiguity points in the source  

### C. Propositions

The source contains several statements that behave like propositions: they assert structural relations, equation consequences, or variational consequences. But most of them either lack visible proofs in the extracted text or are embedded in explanatory prose. They should therefore be recorded with explicit proof-status warnings rather than promoted to theorem level. 

**Proposition P.11.1 (Topological-to-metric spinor relation).**
Given the appropriate Levi-Civita-induced data, the draft asserts an identification linking topological spinors on the chimeric bundle to metric spinors.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.11.1, D.11.2, D.12.1
**Proof status:** deferred
**Warning:** hypotheses and uniqueness are not yet stabilized in the visible material 

**Proposition P.18.1 (First-order bosonic equation).**
The bosonic first-order theory is governed by the equation (\Upsilon_\omega=S_\omega-T_\omega=0).
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.18.1–D.18.3
**Proof status:** none as proposition; equation asserted as defining field equation 

**Proposition P.20.1 (Second-order bosonic Euler–Lagrange consequence).**
The second-order bosonic Lagrangian has Euler–Lagrange form (D_\omega^*\Upsilon_\omega=0), equivalently phrased in the draft as a Yang–Mills–Maxwell-like equation with bosonic source.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** D.18.3, covariant differential operators
**Proof status:** sketched in source, not fully rederived here 

**Proposition P.21.1 (First-order implies second-order satisfaction).**
The draft summary claims that once the reduced first-order theory is satisfied, the Euler–Lagrange equations of the second related Lagrangian are automatically satisfied.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** P.18.1, P.20.1
**Proof status:** deferred
**Warning:** should not be upgraded beyond proposition until the variational derivation is reconstructed in full  

**Proposition P.2.1 (Failure of Einstein projection covariance under ordinary gauge transformation).**
The draft explicitly records the curvature covariance law (F_{A\cdot h}=h^{-1}F_Ah) and the failure of the Einstein projection to transform compatibly, which is one of the motivating technical diagnoses behind the altered formalism.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** curvature, Einstein projection
**Proof status:** source-stated; derivation not reconstructed here 

**Proposition P.2.2 (Ordinary gauge-potential norm action is non-invariant).**
The draft explicitly uses the naive action (I(A)=|A|^2) and its non-invariance to motivate avoiding ordinary direct use of gauge potentials in the action.
**Status:** formal
**Source basis:** draft-native
**Dependencies:** connection transformation law
**Proof status:** source-stated; derivation not reconstructed here 

### D. Conjectures

Several core ambitions of the draft are better classified as conjectures in the completion document, because the visible material frames them as beliefs, proposals, or structural aspirations rather than completed mathematical results. The completion scaffold explicitly anticipates such relabeling whenever the draft blurs definition, derivation, and aspiration.  

**Conjecture Q.12.1 (Space-time is not fundamental).**
Space-time is to be recovered as an approximation from observerse rather than taken as the fundamental substrate.
**Status:** conjectural
**Source basis:** draft-native
**Dependencies:** D.8.1, D.9.2
**Evidence:** structural role of observerse only
**Impact if false:** the recovery architecture collapses back toward ordinary space-time fundamentality 

**Conjecture Q.12.2 (Metric and observed field content are native to different spaces).**
The draft proposes that the metric on (X) and observed bosonic and fermionic fields should not be treated as fundamental on the same space.
**Status:** conjectural
**Source basis:** draft-native
**Dependencies:** D.8.1, D.8.2, D.9.2
**Evidence:** conceptual argument in the draft, not a completed theorem
**Impact if false:** the distinct native-versus-observed ontology loses much of its motivation 

**Conjecture Q.9.1 (Restricted metrics on (Y) are effectively connection-determined).**
The draft claims that the relevant metric information on (Y) is highly restricted and effectively equivalent to a space of splitting connections, rather than to unrestricted metrics.
**Status:** conjectural
**Source basis:** draft-native
**Dependencies:** D.9.1, D.12.1
**Evidence:** explanatory argument only in the visible material
**Impact if false:** the separation between “true” metric data and quantized connection data is weakened 

**Conjecture Q.12.3 (Einstein and Dirac are unified only indirectly through deeper first-order structure).**
The draft states that Einstein and Dirac should be replaced by reduced Euler–Lagrange equations of a first-order theory rather than unified directly in standard form.
**Status:** conjectural as program statement, formal as target architecture
**Source basis:** draft-native
**Dependencies:** P.18.1, P.20.1
**Evidence:** stated as belief and programmatic claim
**Impact if false:** the draft’s central replacement strategy would fail  

**Conjecture Q.19.1 (Existence of an admissible or canonical Shiab choice).**
The draft architecture presumes a useful operator family and seems to require a good or canonical member, but the visible material does not establish one.
**Status:** conjectural
**Source basis:** draft-implicit
**Dependencies:** D.19.1, fermionic and bosonic operator sectors
**Evidence:** chapter prominence only
**Impact if false:** operator sector may remain non-unique or non-predictive  

### E. Phenomenological claims

Anything that maps the formal structure to Einstein gravity, Yang–Mills–Maxwell behavior, Dirac matter, Higgs/Klein–Gordon structure, family structure, or candidate observed sectors belongs here. The completion scaffold is explicit that such items must be treated as mappings or targets, not as already established consequences.  

**Phenomenological Mapping PM.1.1 (Recovery target for familiar physics).**
The stylized opening recovery map is a target statement saying the framework aims to recover Einstein, Yang–Mills–Maxwell, Dirac, Higgs, Yukawa, Lorentz, internal symmetry, and family data.
**Formal source:** high-level recovery architecture
**Physical target:** standard observed field and interaction content
**Status:** PM-Structural
**Support:** programmatic target only
**Failure modes:** decomposition mismatch; incorrect sector recovery; missing dynamics
**Dependencies:** observerse, decomposition, operator, and Lagrangian sectors 

**Phenomenological Mapping PM.12.1 (First-order bosonic equation as gravity-like equation).**
The draft explicitly annotates the first-order bosonic equation so that (S_\omega) plays a role analogous to (R_{\mu\nu}-\frac{s}{2}g_{\mu\nu}) and (T_\omega) plays the role of cosmological and stress-energy contributions.
**Formal source:** (\Upsilon_\omega=S_\omega-T_\omega=0)
**Physical target:** Einstein-field-equation-like structure
**Status:** PM-Candidate
**Support:** heuristic annotated comparison
**Failure modes:** mismatch of tensor structure; missing covariance proof; wrong source interpretation
**Dependencies:** D.18.1–D.18.3 

**Phenomenological Mapping PM.20.1 (Second-order bosonic equation as Yang–Mills–Maxwell analog).**
The draft explicitly labels the second-order equation (D_\omega^*F_{A_\omega}=J_\omega^B) as Yang–Mills–Maxwell-like with bosonic source.
**Formal source:** second-order bosonic variational equation
**Physical target:** Yang–Mills–Maxwell dynamics
**Status:** PM-Candidate
**Support:** structural analogy from displayed equation
**Failure modes:** nonstandard current structure; covariance or representation mismatch
**Dependencies:** P.20.1 

**Phenomenological Mapping PM.12.2 (Recovered space-time from observerse).**
The draft’s recovery program treats the observed space-time world as emerging from a broader two-space structure ((X,Y,\iota)).
**Formal source:** observerse and pullback machinery
**Physical target:** ordinary observed space-time
**Status:** PM-Structural
**Support:** conceptual and architectural only
**Failure modes:** no admissible recovery theorem; nonunique observation maps; wrong induced metric behavior
**Dependencies:** D.8.1, D.9.2 

**Phenomenological Mapping PM.12.3 (Different native spaces for metric and observed fields).**
The draft proposes that harmonization may come from placing metric and observed bosonic/fermionic sectors as native to different spaces rather than the same one.
**Formal source:** native/invasive field ontology
**Physical target:** reinterpretation of metric-versus-matter ontology
**Status:** PM-Tentative
**Support:** heuristic conceptual argument
**Failure modes:** inability to produce correct observed couplings or quantum structure
**Dependencies:** D.8.2, D.9.2 

**Phenomenological Mapping PM.12.4 (Modified Yang–Mills analog has a Dirac square root in an Einstein–Chern–Simons-like equation).**
The draft explicitly frames one of its major ambitions as the claim that the modified Yang–Mills equation analog admits a Dirac-like square-root relation through a mutant Einstein–Chern–Simons-like structure.
**Formal source:** bosonic and fermionic operator/Lagrangian architecture
**Physical target:** unified relation among gravity-like, gauge-like, and fermionic dynamics
**Status:** PM-Candidate
**Support:** heuristic analogy plus displayed comparative formulas
**Failure modes:** no operator factorization; no consistent square-root interpretation; wrong classical limit
**Dependencies:** D.19.1, fermionic operator sector, P.20.1 

**Phenomenological Mapping PM.28.1 (Family and observed-sector recovery).**
The completion scaffold and the draft summary make clear that observed bosonic sectors, fermionic quantum numbers, true versus effective generations, and three-family structure are intended outputs of the framework.
**Formal source:** representation decomposition and observation pipeline
**Physical target:** Standard-Model-like bosons, fermions, and family structure
**Status:** PM-Structural for now
**Support:** target-setting only in the presently visible material
**Failure modes:** branching mismatch; wrong chirality; wrong family count; no unique embedding
**Dependencies:** decomposition chapters not yet reconstructed in detail  

### Ledger summary by class

At the present extraction level, the draft yields only a very small axiomatic core, a large definitional core, a moderate set of proposition-like statements, a substantial conjectural layer, and an even larger phenomenological layer. That distribution is exactly what the completion document should expect from an ambitious but incomplete research program: many named objects, several asserted equations, relatively few completed proofs, and many formal-to-physical aspirations that must not yet be treated as established mathematics.  

A practical consequence is that the next chapters should proceed in this order: lock the axiom set, formalize the definitions, identify which proposition-level statements can actually be proved, demote the rest to conjectures where needed, and only then rebuild the phenomenological mappings on top of a stable decomposition and recovery formalism. That sequencing matches the completion outline’s dependency graph and its explicit warning not to write detailed phenomenology before the representation and decomposition machinery is normalized. 

I can turn this next into a compact appendix-style table with columns for **ID, statement, class, source basis, dependencies, proof/support status, and prediction sensitivity**.
