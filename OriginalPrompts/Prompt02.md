## Style and Status Convention Sheet

### For the *Geometric Unity Completion Document*

This sheet defines the writing, labeling, and status conventions for the completion document so every statement is tagged by role, certainty, and dependency. Its purpose is to prevent the document from mixing formal mathematics, interpretive claims, and phenomenological ambitions into one undifferentiated narrative.

---

# 1. Governing principle

The completion document must clearly separate:

1. **primitive commitments**
2. **formal definitions**
3. **derived mathematical results**
4. **working assumptions inserted for completion**
5. **speculative claims**
6. **claims that map formal structures to observed physics**

No statement should appear without an identifiable status.
If a statement is important enough to rely on later, it must be labeled.

---

# 2. Document-wide style rules

## 2.1 Writing style

Use a formal mathematical research style with the following preferences:

* state claims in the strongest justified form, and no stronger
* distinguish explicitly between “is defined as,” “it follows that,” “we assume,” “we conjecture,” and “we interpret”
* avoid rhetorical phrasing like “clearly,” “obviously,” “remarkably,” or “one sees”
* avoid presenting interpretive physics statements as proved mathematics
* when a proof is absent, say so directly
* when a step is heuristic, mark it as heuristic
* when a construction depends on an inserted completion choice, flag that dependency locally

## 2.2 Paragraph discipline

Each subsection should move in this order when possible:

1. setup
2. labeled statements
3. derivation or discussion
4. dependency note
5. unresolved issues

## 2.3 Symbol discipline

Every symbol introduced in the main text must satisfy all of the following:

* has a first point of definition
* has a unique meaning unless explicitly overloaded
* appears in the notation registry
* carries its ambient domain when ambiguity is possible
* is not reused for a different object later

## 2.4 Dependency discipline

Any result used later must include one or more of:

* a proof
* a citation to a prior internal result
* an explicit dependency on an inserted assumption
* an explicit status as conjectural or phenomenological

---

# 3. Status hierarchy

The document uses a strict status hierarchy. Higher-status items may support lower-status interpretation, but lower-status items may not be used as if they were higher-status results.

## Tier A — Formal foundation

These are admissible as core mathematical support.

* **Axiom**
* **Definition**
* **Lemma**
* **Proposition**
* **Theorem**
* **Corollary**

## Tier B — Completion scaffolding

These are allowed, but must be visibly marked as introduced to complete the document rather than extracted unambiguously from the original draft.

* **Inserted Assumption**
* **Inserted Convention**
* **Inserted Choice**
* **Completion Rule**

## Tier C — Incomplete but structured claims

These are legitimate research objects but not established results.

* **Conjecture**
* **Heuristic Principle**
* **Open Problem**
* **Programmatic Goal**

## Tier D — Physics interpretation layer

These connect formal objects to physical interpretation.

* **Phenomenological Mapping**
* **Interpretive Identification**
* **Candidate Correspondence**
* **Prediction Target**
* **Falsification Criterion**

A Tier D item must never be cited as though it were a Tier A theorem.

---

# 4. Label set and required meaning

## 4.1 Axiom

**Purpose:** Declare a foundational starting commitment of the completion document.

Use an **Axiom** only for a primitive assumption that the document chooses as foundational and does not derive internally.

Format:
**Axiom A.n**

Example use:

* topological assumptions on the base space
* admissibility of a class of observerse constructions
* primitive regularity assumptions

Rules:

* axioms must be minimal
* axioms should not encode downstream conclusions
* if an axiom is not clearly present in the original draft, it should usually be an Inserted Assumption instead

---

## 4.2 Definition

**Purpose:** Fix meaning.

Use **Definition** for any object, map, class, operator, status term, or equivalence relation that will be used later.

Format:
**Definition D.n**

Rules:

* definitions must not contain hidden existence claims unless stated
* if existence is nontrivial, separate it into a proposition or theorem
* if the definition depends on an inserted choice, append a dependency note

Example:
**Definition D.14 (Observed field sector).** …

---

## 4.3 Lemma / Proposition / Theorem / Corollary

These are derived mathematical statements.

### Lemma

Use for a local technical result serving a nearby proof.

Format:
**Lemma L.n**

### Proposition

Use for a mathematically meaningful result that is not positioned as a central structural milestone.

Format:
**Proposition P.n**

### Theorem

Use only for major structural results central to the completion program.

Format:
**Theorem T.n**

### Corollary

Use for immediate consequences of a previous result.

Format:
**Corollary C.n**

Rules for all four:

* each must state hypotheses explicitly
* each must indicate proof status
* if proof is omitted, say **Proof status: omitted**
* if proof is incomplete, do not label as proposition/theorem unless paired with a visible status tag such as Draft Proof or Proof Outstanding

Recommended proof-status footer:

* **Proof:** complete
* **Proof:** sketched
* **Proof status:** deferred
* **Proof status:** outstanding

If proof is outstanding and the result is still relied upon, that dependency must be explicitly flagged.

---

## 4.4 Conjecture

**Purpose:** Record a structured but unproved claim believed or explored by the completion program.

Format:
**Conjecture Q.n**

Use when:

* a statement is plausible but unproved
* a structural equivalence is suspected
* a phenomenological consequence depends on unverified mathematics

Rules:

* a conjecture may guide work
* a conjecture may not be used later as though established
* if downstream sections depend on it, that dependence must be local and explicit

Recommended footer fields:

* **Evidence**
* **Dependencies**
* **Impact if false**

---

## 4.5 Phenomenological Mapping

**Purpose:** Relate a formal structure in the document to an observed or hypothesized physical entity, sector, parameter, or behavior.

Format:
**Phenomenological Mapping PM.n**

This is one of the most important labels in the entire document.

Use for statements like:

* a representation component corresponds to a candidate observed boson sector
* a spinorial structure is interpreted as a family pattern
* a geometric quantity is mapped to a measurable coupling or mass relation
* an effective observed sector is identified from pullback or decomposition

Rules:

* this label does **not** mean the mapping is proved physically correct
* each mapping must state whether it is:

  * structural
  * interpretive
  * tentative
  * quantitative
* each mapping must state what kind of support exists:

  * exact derivation
  * representation-theoretic match
  * heuristic analogy
  * numerical fit target
* each mapping must state falsifiability conditions when possible

Required subfields:

* **Formal source**
* **Physical target**
* **Status**
* **Support**
* **Failure modes**
* **Dependencies**

Recommended statuses within PM items:

* **PM-Structural**
* **PM-Candidate**
* **PM-Tentative**
* **PM-Quantitative**

Example:
**Phenomenological Mapping PM.7 (Candidate identification of an observed electroweak sector).**

---

## 4.6 Inserted Assumption

**Purpose:** Mark an assumption added by the completion program that is not cleanly extractable as an explicit commitment of the original draft but is needed to make the document mathematically or computationally executable.

Format:
**Inserted Assumption IA.n**

This label is critical. It prevents silent supplementation of the source material.

Use when:

* regularity must be specified
* a bundle existence condition is added
* a representation choice is fixed
* a sign, normalization, or admissibility condition is imposed
* a PDE or variational framework requires hypotheses absent from the draft

Rules:

* every Inserted Assumption must state why it is inserted
* every Inserted Assumption must state whether it is:

  * conservative
  * strengthening
  * simplifying
  * branch-selecting
* if removable later, note that
* if it materially affects predictions, say so explicitly

Required subfields:

* **Reason for insertion**
* **Minimality assessment**
* **Affected sections**
* **Whether removable**
* **Prediction sensitivity**

Example:
**Inserted Assumption IA.3 (Sobolev regularity for admissible connection fields).**

---

# 5. Related inserted labels

To keep the document clean, use these additional inserted-status labels where appropriate.

## 5.1 Inserted Convention

Format:
**Inserted Convention IC.n**

Use for editorial or technical normalization choices that do not substantially alter substance, such as:

* index placement conventions
* sign conventions
* normalization choices
* naming conventions

An Inserted Convention should not carry physical content unless explicitly stated.

## 5.2 Inserted Choice

Format:
**Inserted Choice IX.n**

Use where several valid branches exist and one branch is selected for continuity of exposition or implementation.

Examples:

* selecting one admissible operator family
* fixing one subgroup embedding
* choosing one discretization scheme
* choosing one representative in an equivalence class

## 5.3 Completion Rule

Format:
**Completion Rule CR.n**

Use for procedural rules that govern how the completion document fills gaps.

Examples:

* always separate topological from metric dependence
* do not infer observational identifications before decomposition is fixed
* simulation sections may only consume normalized operator definitions

These are meta-document rules rather than mathematical claims.

---

# 6. Required status block format

Every major labeled item should use a compact status block when helpful.

Recommended template:

**Status:** formal / inserted / conjectural / phenomenological
**Source basis:** draft / completion / mixed
**Dependencies:** D.4, IA.2, P.7
**Used later in:** §12, §18, §27
**Proof status:** complete / sketched / deferred / none
**Prediction sensitivity:** none / low / medium / high

For Phenomenological Mappings, replace proof status with:
**Support level:** exact / structural / heuristic / numerical-target-only

---

# 7. Numbering convention

Use stable chapter-scoped numbering.

Recommended scheme:

* Axiom A.2.1
* Definition D.5.3
* Lemma L.11.4
* Proposition P.17.2
* Theorem T.21.1
* Corollary C.21.2
* Inserted Assumption IA.15.1
* Inserted Convention IC.0.3
* Inserted Choice IX.19.2
* Completion Rule CR.1.4
* Conjecture Q.28.1
* Phenomenological Mapping PM.26.3

Where the first number is the chapter and the second is the item number within that chapter.

This makes dependencies easier to trace and avoids renumbering chaos.

---

# 8. Margin tags / inline shorthand

For drafting and review, each labeled item may also carry a short inline status tag:

* **[AX]** Axiom
* **[DEF]** Definition
* **[LEM]** Lemma
* **[PROP]** Proposition
* **[THM]** Theorem
* **[COR]** Corollary
* **[IA]** Inserted Assumption
* **[IC]** Inserted Convention
* **[IX]** Inserted Choice
* **[CR]** Completion Rule
* **[CONJ]** Conjecture
* **[PM]** Phenomenological Mapping
* **[OP]** Open Problem
* **[HP]** Heuristic Principle

These are optional in polished prose but useful in draft mode and review tables.

---

# 9. Source-basis tagging

Every nontrivial claim should also be classifiable by origin.

Use one of:

* **Draft-native**
  Clearly stated or directly recoverable from the original draft.

* **Draft-implicit**
  Strongly suggested by the draft, but not fully formalized there.

* **Completion-inserted**
  Added by the completion document to close a gap.

* **Completion-derived**
  Not stated in the draft, but derived from normalized assumptions and definitions.

This source-basis tag is especially important for:

* Inserted Assumptions
* major Propositions
* Phenomenological Mappings

---

# 10. Rules for using each label downstream

## 10.1 What may support what

* Axioms and Definitions may support everything.
* Lemmas, Propositions, Theorems, and Corollaries may support everything below them.
* Inserted Assumptions may support formal derivations, but every downstream use must preserve awareness that a completion choice has entered.
* Conjectures may support only:

  * programmatic discussion
  * branch planning
  * explicitly conditional sections
* Phenomenological Mappings may support only:

  * interpretive discussion
  * prediction planning
  * falsification planning

## 10.2 What may not happen

* a conjecture cannot be cited as a theorem
* a phenomenological mapping cannot be cited as a proposition
* an inserted assumption cannot be smuggled in as if it were draft-native
* a heuristic argument cannot silently stand in for a proof

---

# 11. Mandatory phrasing conventions

Use the following verbs precisely:

* **Define** for definitions only
* **Assume** for axioms or inserted assumptions
* **Prove / derive** for established mathematical results
* **Conjecture** for unproved structured claims
* **Interpret / map** for physical identification
* **Fix** for conventions or branch choices
* **Observe** only for immediate mathematical facts or empirical statements, not rhetorical emphasis

Preferred phrases:

* “We define…”
* “We assume…”
* “Under IA.4, it follows that…”
* “This suggests, but does not establish, that…”
* “We record the following conjecture…”
* “We introduce the following phenomenological mapping…”
* “This identification is tentative and depends on…”

Avoid:

* “This shows” when no proof is given
* “This is” when it is really “this is interpreted as”
* “Naturally” when an inserted choice is being made
* “Equivalent” unless an equivalence is proved

---

# 12. Formatting templates

## 12.1 Axiom template

**Axiom A.7.1 (Admissible base structure).**
Let (X) be a smooth, oriented four-manifold satisfying …

**Status:** formal
**Source basis:** draft-implicit
**Used later in:** §8, §10, §13

---

## 12.2 Definition template

**Definition D.10.2 (Chimeric dual bundle).**
We define (C^*) to be …

**Status:** formal
**Source basis:** draft-native
**Dependencies:** A.7.1

---

## 12.3 Proposition template

**Proposition P.18.3 (Covariance of augmented torsion under the tilted action).**
Assume A.7.1, D.17.2, and IA.15.1. Then …

**Proof status:** deferred
**Status:** formal
**Source basis:** completion-derived
**Dependencies:** D.17.2, IA.15.1
**Warning:** depends materially on IA.15.1

---

## 12.4 Conjecture template

**Conjecture Q.28.1 (Effective family splitting mechanism).**
Under the normalized decomposition scheme, …

**Status:** conjectural
**Source basis:** draft-implicit
**Dependencies:** §26, §27
**Evidence:** representation pattern match only
**Impact if false:** family interpretation must be replaced

---

## 12.5 Phenomenological Mapping template

**Phenomenological Mapping PM.26.3 (Candidate observed gauge-sector correspondence).**
The decomposition component … is interpreted as a candidate observed … sector.

**Formal source:** decomposition of …
**Physical target:** observed …
**Status:** PM-Candidate
**Support:** structural
**Dependencies:** P.26.1, IX.13.2
**Failure modes:** branching mismatch; wrong chirality; nonunique embedding
**Prediction sensitivity:** high

---

## 12.6 Inserted Assumption template

**Inserted Assumption IA.15.1 (Sobolev regularity of admissible fields).**
For the variational analysis, we assume all admissible fields lie in …

**Reason for insertion:** variational derivatives are otherwise undefined in the required sense
**Minimality assessment:** likely conservative
**Affected sections:** §15, §20, §21, §24, §35
**Whether removable:** possibly, after a distributional reformulation
**Prediction sensitivity:** low

---

# 13. Open-problem and heuristic conventions

## Open Problem

Format:
**Open Problem OP.n**

Use when a concrete unresolved task is known.

Example:

* proving existence of a global observerse satisfying specified compatibility conditions
* classifying admissible Shiab operators under a chosen covariance rule

## Heuristic Principle

Format:
**Heuristic Principle HP.n**

Use when a guiding idea is informative but not formally established.

Example:

* a geometric intuition motivating an observational pullback
* a representation-counting argument not yet upgraded to a theorem

Heuristic principles should never be used as sole support for formal claims.

---

# 14. Review flags

Each chapter should maintain visible flags for unresolved items:

* **[UNPROVED]**
* **[INSERTED]**
* **[AMBIGUOUS]**
* **[NONUNIQUE]**
* **[PHENO]**
* **[SIM-DEPENDENT]**
* **[DATA-DEPENDENT]**

These can appear in editorial review tables or chapter-end checklists.

---

# 15. Minimum compliance rules

A section is not considered complete unless:

1. every reusable object is formally defined
2. every major claim has a status label
3. every inserted assumption is explicitly marked
4. every phenomenological identification is labeled as a mapping, not as a theorem
5. every unresolved proof obligation is visible
6. dependencies on conjectures are explicit
7. no section silently shifts from mathematics to physics interpretation

---

# 16. Recommended one-page condensed convention table

| Label                    | Meaning                           | May be used as formal support? |     Requires proof? | Must state dependencies? |
| ------------------------ | --------------------------------- | -----------------------------: | ------------------: | -----------------------: |
| Axiom                    | primitive foundation              |                            Yes |                  No |                      Yes |
| Definition               | fixed meaning                     |                            Yes |                  No |       Yes, if nontrivial |
| Lemma                    | local derived result              |                            Yes | Yes or proof status |                      Yes |
| Proposition              | standard derived result           |                            Yes | Yes or proof status |                      Yes |
| Theorem                  | major derived result              |                            Yes | Yes or proof status |                      Yes |
| Corollary                | immediate consequence             |                            Yes |       Usually brief |                      Yes |
| Inserted Assumption      | completion-added assumption       |                  Conditionally |                  No |                      Yes |
| Inserted Convention      | editorial/normalization choice    |                        Limited |                  No |                      Yes |
| Inserted Choice          | branch selection                  |                  Conditionally |                  No |                      Yes |
| Completion Rule          | meta-rule of completion process   |                Procedural only |                  No |                      Yes |
| Conjecture               | unproved structured claim         |       No, except conditionally |                  No |                      Yes |
| Phenomenological Mapping | formal-to-physical identification |         No, not as mathematics |                  No |                      Yes |

---

# 17. Final governing sentence

When there is any doubt about how to label a statement, use the **weaker and more explicit** label:

* prefer **Inserted Assumption** over silently upgrading a gap,
* prefer **Conjecture** over overstated proposition,
* prefer **Phenomenological Mapping** over implied physical proof.

This preserves the integrity of the completion document.

I can also turn this into a polished appendix-ready section with formal numbering and house-style language for direct insertion into the document.
