# Boson Prediction Agent Restart Prompt

Use this prompt to restart the GeometricUnity boson-prediction investigation
from the current checkpoint.

## Prompt For The Next Agent

You are working in `/home/josh/Documents/GitHub/GeometricUnity` on the
Geometric Unity boson-prediction/source-law investigation. Continue in the same
style: read the repo first, keep edits scoped, preserve scientific caution, keep
the journal and reference ledger current, validate before committing, and do not
promote W/Z/H masses unless the source-lineage contracts are genuinely filled.

### Operating Rules

- Use `rg`/`rg --files` first for searches.
- Use `apply_patch` for manual edits.
- Do not revert user or unrelated work. The worktree may be dirty.
- Avoid destructive git commands.
- If broad research or diagnosis is needed and multi-agent tooling is
  available, launch a read-only explorer/subagent and incorporate its findings.
- Keep a running diagnosis journal in
  `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.
- Track references through `ExperimentReferences.md` plus detailed notes under
  `docs/Reference/ExperimentReferences/`.
- Before treating any source as promotion evidence, require exact source
  lineage, observed-field extraction, target independence, pole extraction, and
  GeV/unit normalization.
- If external research is needed, use current primary/official sources where
  possible and add/update reference detail notes before using the source.
- USER DIRECTIVE (2026-07-01): run every validation phase with Release builds
  (`dotnet run -c Release`). The generator script encodes this; use the same
  flag for targeted runs. The Debug JIT costs 3-8x on numeric phases and
  validation wall-time bounds the innovation pace. Measured: the full
  generator pass takes 22.1 min in Release (2026-07-01), vs roughly twice
  that for the earlier uninstrumented Debug passes the same day.
- SCANNER-REGISTRATION HAZARD (learned 2026-07-01): the broad text scanners
  (phase281's GU-RVG local search, phase207's blocker list, and kin) audit
  repo text OUTSIDE their known-file lists. Any NEW doc that names scanner
  terms (95.4 GeV, metric engineering, GU-RVG, source rows, ...) - including
  IMPLEMENTATION_PNNN.md files - must be registered in those exclusion lists
  in the same checkpoint, or the next generator pass fails with a 48-phase
  fail-closed cascade rooted at the scanner. This is the system working as
  designed; register docs deliberately, never weaken the scanners.

### Current Scientific Status

No successful physical W/Z/H prediction has been achieved. The current package
still blocks physical comparison because the source-lineage and observed-field
contracts are empty.

CURRENT CHECKPOINT (2026-07-15, Phase456 production): the explicitly authorized,
environment-clean consolidated n=4 HMC completed in 21846.6 s under the
committed defaults. Pack/default/authorization/storage/exact-control shape is
valid, but the pre-registered analysis is not: the free-field sampler gate is
red, SD2 beta=1 has generic N_eff 7.15 and k_min-row N_eff 16.62, and the A2 and
dispersion mass rows are non-finite. The any-invalid-row firewall withholds the
family-wise threshold and emits `production-analysis-invalid`; no G3
motivation, n=5 escalation, L6/L8 closure, spectrum characterization, or claim
follows. The measured 0.1137 CPU-week cost supplies Phase458/G2 within budget.
Phase478 has now frozen the exact Phase458 contract. Phase458 remains
`blocked-inputs-incomplete`: G1 is available-false (the exact A5 obstruction,
not a theorem), G2 and G6 are available-true, G3/G4/G5 are missing, and no
input is invalid or drifted. G6 projects 1.5044386598 CPU-weeks under its
committed n=3..6 n^4-volume model and therefore selects the CPU route; that is
scheduling eligibility only and authorizes no launch. Phase471 remains open
L5/L6/L8 with L7 withheld. The O4 engagement remains the human critical path.
Phase101 stays blocked, Phase202 is 295/3 after the A13 checkpoint, the
missing-field counts remain 15/14, and `promotedPhysicalMassClaimCount=0`.

O4 INFRASTRUCTURE (2026-07-15, Phase477): the exact coverage register is
current at 31 recursively review-pending outputs. The former phase-name/prose
heuristic is retired; a 31-entry typed contract distinguishes direct physics
rulings, transitive dependents, separate pending packets, and administrative
zero-compute rows. The inert memo schema/template and dependency map share
exactly 13 ruling IDs. The synthetic-overturn battery exercises all 94
dependency edges with zero unrelated-branch changes, canonical mutations, or
Phase457 candidate-path writes. The integrity verifier runs register currency,
exact-coverage, and overturn checks on every pass. This is tooling only: no O4
memo or ruling exists, no pending flag changed, and Phase477 rests at
`infrastructure-ready-pending-human-ruling`.

EXPLORATORY SELF-AUDIT LANE (2026-07-15, Amendment A5; Phases484-486):
external review is put aside as an operational blocker, not waived as an
eventual promotion requirement. Phase484 opens only a reduced, target-blind
exploration lane behind a prospective-only evidence firewall. Phase485 maps
all 13 O4 items to explicit falsifiers without authoring rulings. Phase486
replays committed evidence to expose the soft-floor and zero-mode fragilities,
record the ladder repair and scoped compact-form check, and rank theta-measure,
workbench-model, and checkpoint/restart controls next. None of these phases
satisfies O4, G4/G5, Phase458, or a source contract; no production launch is
authorized and `promotedPhysicalMassClaimCount=0`. Binding plan:
`docs/Phases/EXPLORATORY_SELF_AUDIT_PLAN_2026-07-15.md`. Registry 484-486 is
allocated; its former 487+ frontier is superseded by Amendment A6 below.

CONVENTION-ROBUSTNESS TRANCHE (2026-07-15, Amendment A6; Phases487-489):
all three Phase486 priorities are implemented. Phase487's independent SO(3)
quadrature and analytic-moment batteries pass. Phase488's four separately
constructed proposal families pass the frozen zero-action stationarity,
inversion, composition, and binned detailed-balance controls. Phase489 proves
bit-exact JSON checkpoint/resume for three reduced synthetic chains and records
separate cross-proposal statistical agreement. These are exploration-lane
controls only: the Phase488 test is coarse and zero-action, while Phase489 is a
synthetic bounded target, so neither validates the Phase450/452/456 production
operator. They do not author or discharge O4, modify committed upstream
artifacts, satisfy Phase458, authorize production, fill a source contract, or
support a physical-unit claim. A surviving result may motivate a new
prospective confirmation checkpoint but cannot be retrospectively relabeled. Binding plan:
`docs/Phases/CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15.md`. Registry
487-489 is allocated; its former 490+ frontier is superseded by Amendment A7
below.

PHASE455 CONVENTION-CLOSURE EXPLORATION (2026-07-15, Amendment A7;
Phases490-492): Phase490 independently reconstructs the committed k=0 spectra
but finds no compatible generator-image map or projector that classifies the
determinant kernels as gauge volume. It therefore records
`quotient-underdetermined`, not a forced quotient. Phase491 admits only two
reconstructible bosonic restrictions and evaluates 24 valid target-blind
branches: 7 are null and 17 contain a candidate well. Another 36 advertised
soft-floor rows remain explicitly invalid because no committed map applies
the Phase447 Hessian-relative floor to the determinant factors. Its terminal
is `model-convention-fragile`, with outcome dependence localized to bosonic
model, zero-mode treatment, fermionic normalization, and axis orbit. Phase492
binds both exact artifacts and records `localized-assumption-dependence`.
These are valid negative exploratory terminals: they do not rewrite Phase455,
satisfy Phase458 G3/G5, discharge O4, authorize sampling/production, fill a
source contract, or support a physical-unit claim. Gate-bearing reuse requires
a new prospective confirmation checkpoint and the still-binding external
interpretation. Binding plan:
`docs/Phases/PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15.md`. Registry 490-492
is allocated; its former 493+ frontier is superseded by Amendment A8 below. Mandatory incremental validation passed
with 87 steps run and 273 skipped; Phase202 is 280/3 and integrity preserves
the 15/14 deficits and zero promoted claims.

PHASE456 FORENSIC RECOVERY (2026-07-15, Amendment A8; Phases493-495):
Phase493 binds the immutable Phase456 production artifact, analyzer, launch
summary, authorization, and preregistration pack. It independently reproduces
both a frozen-estimator domain failure and effective-sample-size limitations,
while the retained primitive sampler and exact-Gaussian controls do not support
a sampler implementation defect. Its terminal is `mixed-failure-supported`.
Phase494's six reduced deterministic/exact-Gaussian oracle batteries pass as
tests, but the result is `mixed-estimator-outcome`: the original estimator is
exact on a positive single-pole domain, a fixed positive two-pole alternative
is feasible on its grid-resolved oracle, and T=4 plus the frozen covariance do
not identify general spectral content. Sign-indefinite channels retain an
unresolved positivity premise. Phase495 exact-binds both outputs and records
`estimator-structurally-unidentifiable`; insufficient statistics remains a
supported lower-precedence condition. No underlying-observable invalidity is
declared. A8 does not reinterpret Phase456, construct or authorize Phase481,
authorize fresh sampling or production, satisfy Phase458 G3/G5, discharge O4,
fill a source contract, or support a physical-unit claim. Binding plan:
`docs/Phases/PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15.md`. Registry 493-495
is allocated; its former 496+ frontier is superseded by Amendment A10 below. Mandatory incremental validation passed;
Phase202 is 283/3 and integrity preserves the 15/14 source deficits and zero
promoted claims.

PHASE456 PROSPECTIVE MEASUREMENT-REPAIR DESIGN (2026-07-15, Amendment A10;
Phases496-498): Phase496 exact-binds the retained Phase456 and forensic-chain
bytes. Four stored time indices reduce to at most three independent periodic
values, and no raw configuration-level samples survive; the admitted
two-component periodic model has four free parameters. The retained bytes are
therefore insufficient for identifiable repair, although the 64 spatial
momentum aggregates and 50 aligned delete-block summaries remain inventoried.
Phase497 freezes a target-blind synthetic one/two-component periodic-GLS oracle
and acquisition grid. Only temporal extent 16 with effective-sample-size floor
2048 passes its complete recovery, model-selection, covariance, sign, and
invalid-row menu within that model. Phase498 exact-binds both results and
records `new-acquisition-specification-ready`. This means only that the
synthetic specification may inform a later independently frozen Phase481 pack;
it is not a production power guarantee, creates no pack, and authorizes no
sampling. New written sampling authorization is still required. Phase456 is
not reinterpreted, Phase458 G3/G5 and O4 remain closed, external review remains
required, and no source-contract or physical-unit authority follows. Registry
496-498 is allocated; its former 499+ frontier is superseded by Amendment A11 below. Phase202 is 286/3 and
`promotedPhysicalMassClaimCount=0`.

PHASE456 ACQUISITION ROBUSTNESS (2026-07-15, Amendment A11;
Phases499-501): Phase499 exact-binds the retained Phase456 and Phase496 bytes
and audits five columns, 340 stored correlator families, and 17,000 finite
aligned delete-block rows. Aggregate `T=4` variance, covariance, and stored
autocorrelation diagnostics are descriptively recoverable, but no `T=16`
temporal covariance kernel, autocorrelation penalty, channel/momentum scaling
law, or delete-block design is retained. Phase500 freezes a broader synthetic
stress family before consuming Phase499. The Phase497 `T=16` / ESS-floor
`2048` point is conditional only; 17/20 audited points are conditional and only
`T=32` / ESS-floor `8192` passes the complete synthetic menu. Phase501 exact-
binds both outputs and Phase456's measured cost, then selects
`insufficient-retained-calibration`: favorable synthetic robustness cannot
override missing empirical calibration. The selected synthetic point is not a
production power guarantee. No Phase481 pack was created, no sampling was
authorized, and Phase458 G3/G5 and O4 remain closed. Registry 499-501 is
allocated; its former 502+ frontier is superseded by Amendment A12 below. Phase202 is 289/3 and
`promotedPhysicalMassClaimCount=0`.

PHASE456 ADAPTIVE CALIBRATION DESIGN (2026-07-15, Amendment A12;
Phases502-504): the user supplied written permission for sampling and
reprocessing necessary to validate the repair program, but the three A12
phases themselves run no sampler and grant no launch authority. Phase502
exact-binds Phase501 and Phase456's measured cost and freezes a four-chain
`T=16` protocol with conditional `T=32` escalation, configuration-level
retention, explicit covariance/autocorrelation/ESS/split-R-hat rules, two
stable checkpoints, a 2 CPU-week ceiling, and hash-refuse-to-run requirements.
Phase503's frozen 36-row synthetic menu preserves a negative result: coverage
and model-selection fail across the complete adversarial family, although the
nominal single/separated-pair subset and all invalid, convergence, escalation,
and cost controls pass. Its terminal is `assumption-conditional-protocol`.
Phase504 exact-binds the chain and selects that conditional terminal by frozen
precedence; the permission element cannot override it, no Phase481 pack is
created, and no sampling or reprocessing is launched. Phase458 G3/G5 and O4
remain closed, external review remains required, and no source-contract or
physical-unit authority follows. Registry 502-504 is allocated; its former
505+ frontier is superseded by Amendment A13 below. Phase202 is 292/3 and
`promotedPhysicalMassClaimCount=0`.

PHASE456 SELECTIVE-INFERENCE REPAIR (2026-07-15, Amendment A13;
Phases505-507): Phase505 exact-binds the preserved Phase503 negative and
accounts for all eleven failed rows. Two are coverage-only misses, two are
false decisive single-to-pair calls, and seven are pair ambiguities; finite-
replicate labels never excuse the original failures. Phase506 prospectively
freezes a set-valued rule: identifiable controls require decisive calls while
near-degenerate and weak-secondary cases may be explicitly unresolved. Its
complete 36-row synthetic menu passes with 576 decisive correct calls, 576
honestly unresolved calls, and zero wrong decisive calls. Phase507 passes its
nine-case precedence battery and records
`selective-inference-protocol-ready-for-independent-phase481-pack-construction`.
This is planning evidence only. The user's existing permission element is
present, but no Phase481 pack is created and no sampling or reprocessing is
launched. Phase480 remains awaiting a genuine signed external memo, O4 and
Phase458 G3/G5 remain closed, and still-binding upstream authorization prevents
Phase481 implementation or launch. Registry 505-507 is allocated; its former
508+ frontier is superseded by Amendment A14 below. Phase202 is 295/3 and
`promotedPhysicalMassClaimCount=0`.

PHASE481 PACK-CONSTRUCTION PLANNING (2026-07-16): at the user's direction,
Phase507's planning permission has been exercised without constructing the
pack. The machine-checked plan is
`studies/phase481_phase456_prospective_repair_preregistration_001/planning/phase481_pack_construction_plan_v1.json`.
It exact-binds Phase456 and Phases502/506/507/480, forwards the four-chain
T16/conditional-T32 storage, convergence, cost, and selective-inference rules,
and fixes the post-gate construction sequence. Planning exposed four blockers:
the Phase452/456 implementation is isotropic `n^4` while the forwarded design
names temporal extents without selecting `4^3 x T` versus `T^4`; a portable
RNG/restart contract is not frozen; a matching CPU reference implementation
does not exist; and runtime/storage refusal bounds depend on those choices.
Phase480 also remains unsatisfied. The Phase481 skeleton terminal is unchanged,
no pre-registration pack bytes exist, no sampling or reprocessing is launched,
and `promotedPhysicalMassClaimCount=0`.

PHASE481 PRECONSTRUCTION CLOSURE (2026-07-16, Amendment A14;
Phases508-510): Phase508 resolves the geometry question from exact-bound
repository semantics, selecting spatial-four temporal lattices `[4,4,4,16]`
and `[4,4,4,32]`. Phase509 adds per-axis periodic extents to the canonical
mesh and reference-CPU operator while preserving scalar/isotropic behavior;
all nine reduced deterministic topology/operator controls pass. Phase510
freezes the portable xoshiro256** chain/RNG design, exact resource arithmetic,
cost-refusal rule, and prospective fingerprint-mutation surface. Adversarial
review rejected its initial premature ready branch. The corrected authoritative
terminal is `cost-envelope-failure`: no admissible production-throughput
evidence exists, so the two-CPU-week envelope refuses readiness. The reduced
RNG restart control is not a production HMC checkpoint codec, and prospective
fingerprint distinguishability is not a production pack-loader refusal proof.
Thus geometry and anisotropic CPU feasibility are closed, but pack construction
still requires admissible noncontending throughput evidence plus the later P4
production loader/refusal and checkpoint integration. Phase480 remains
unsatisfied. No Phase481 pack was created, no production-sized allocation,
sampling, HMC, or benchmark ran, and no launch or physical-claim authority
follows. Registry 508-510 is allocated; its former 511+ frontier is superseded
by Amendment A15 below. Phase202 is 298/3 and
`promotedPhysicalMassClaimCount=0`.

PHASE481 WORKLOAD-DEFINITION AUDIT (2026-07-17, Amendment A15; Phase511):
a fail-closed GPT-5.6 Sol/high and Claude Fable 5/high council ranked a static
eligibility audit ahead of throughput timing. Phase511 exact-binds six current
planning, contract, result, and historical-source artifacts and audits 22
benchmark-defining fields. Four prospective geometry/RNG/cadence/cost fields
are frozen; 18 executable-default, operation-ledger, allocation, and aggregate-
CPU-accounting fields are missing. Phase481 P1 has not emitted its planned
default configuration or implementation fingerprint, and the historical
Phase452 path is not silently adopted. The authoritative terminal is
`production-default-or-implementation-missing`; workload completeness and
benchmark eligibility are false. No benchmark, full T16/T32 allocation,
sampling, HMC, pack, loader, checkpoint integration, or launch ran. Phase510's
cost envelope remains unresolved, Phase480/O4 and Phase458 G3/G4/G5 remain
closed, and no source-contract or physical-unit authority follows. Registry
511-513 is allocated for A15; Phase512 is hard-gated on a future Phase511
eligible terminal and Phase513 is reserved for dependent cost re-adjudication.
The later A16-A18 sections supersede the former 514+ allocation frontier.
`promotedPhysicalMassClaimCount=0`.
Phase202 is 299/3 after the Phase511 checklist addition.

PHASE482 DETERMINISTIC THEOREM SCOUT (2026-07-15, Amendment A9): Amendment A9
narrowly permits Phase482's proof computation while Phase481 stays at its
non-authorizing skeleton; no Phase481 sampling prerequisite is relaxed. The
scout exact-binds the Phase456 A5 obstruction record and Phase478 gate
contract. Exact finite ensembles show that identical two-point data can have
positive, zero, or negative connected-square correlations, so two-point data
alone cannot transport the composite sign or value. Exact local and periodic
n=4 enumeration shows that the committed Kuhn complex is not closed under the
registered site- or link-centered time reflections, and its shared edge
variables prevent single-site or independent-face-variable factorization.
These results obstruct the standard proposed proof route; they do not refute
the target inequality for the committed action and neither prove nor refute
reflection positivity by another construction. The terminal is
`a5-theorem-scout-obstructions-survive-no-theorem`: L8 and Phase458 G1 remain
open. Phase482 authorizes no Phase458 evaluation, Binder work, sampling,
acceleration, production, O4 discharge, source-contract fill, or physical-unit
claim. Its former registry frontier is superseded by Amendments A10-A11.

PHASE482 THEOREM-FOUNDATION SNAPSHOT (2026-07-17, Amendment A16; Phase514):
the council's second-ranked machine route was narrowed to a zero-physics-
compute registered-definition census before any symbolic falsifier. Phase514
exact-binds sixteen current action-family, geometry, A5, G1-contract, and
Phase482 artifacts and audits twenty required OS-foundation fields. Nine have
source-defined or partial candidates; eleven registered certificates are
absent; zero are registered for theorem use. No exact-bound artifact registers
one OS reflection, positive-time algebra, normalized measure/domain, exact
bilinear, Hermiticity proof, necessity classification, or allowed-volume
witness embedding. The terminal is
`a5-registered-reflection-foundation-registered-foundation-definition-missing`.
This is an epistemic repository result, not a mathematical nonexistence claim,
reflection-positivity counterexample, or theorem. Phase515 and Phase516 remain
reserved and hard-gated. Registry 514-516 is allocated; the later A17 section
supersedes the former 517+ allocation state. Phase458 G1 and L8 remain open,
and no Phase458/Phase481/sampling/O4 or
physical-unit authority follows, and `promotedPhysicalMassClaimCount=0`.
Phase202 is 300/3 after the Phase514 checklist addition.

A5 DUAL-REFLECTION FOUNDATION AND READINESS (2026-07-18, Amendment A17;
Phases517-519): registry entries 517-519 and the A17 plan are allocated and
implemented. Phase517 is a valid audit with terminal
`action-member-or-omega-parity-ambiguous`. The exact-bound A5 generator calls
its subject a toy-control branch, but the serialized artifact loses that
qualifier. The executable curvature has the form `F=L(omega)+Q(omega)`, while
no exact-bound identity excludes the resulting cubic sign-odd cross term or
establishes omega parity. Phase517 therefore freezes the site-centered and
link-centered axis-0 rows coequally as formal diagnostic candidates; neither
is selected, combined, A5-validated, or registered as a theorem reflection.

Phase518 exact-binds both candidate artifacts and derives their maps after
cross-checking the Phase517 summary rows, paths, hashes, statuses, slices, and
roles. On the exact lattice-canonical periodic n=4 complex, both vertex maps
are bijective involutions. For each candidate, 2,048 of 3,840 edges and 3,072
of 12,800 faces map back into the complex, while 0 of 6,144 four-simplices
does. Failure already on the underlying simplex sets is sufficient to rule out
a total oriented-complex automorphism; no face or four-simplex orientation-
parity result is inferred. The terminal is
`dual-candidate-oriented-complex-nonclosure`.

Phase519 is a valid downstream readiness adjudicator with terminal
`action-or-observable-subject-ambiguous`. The package is not ready for
independent mathematical review: the action member/parity remains ambiguous,
and exact observable dependency, positive-time-algebra membership, measure
normalization/domain, and OS-bilinear Hermiticity remain unproved. Phase515 and
Phase516 stay locked and may not be created. No reflection-positivity theorem
or counterexample follows; Phase458 G1 and limb L8 are unchanged, external
review remains pending, and `promotedPhysicalMassClaimCount=0`. Adversarial
review found fail-open candidate-schema, lineage, map-derivation, coordinate,
scope, and documentation gaps; all were repaired before the final Phase517 ->
Phase518 -> Phase519 downstream run. Phase101 remains physically blocked.
Phase202 is 303 passed / 3 standing failures after the three A17 checklist
rows. The mandatory incremental checkpoint ran 85 steps, skipped 298, and
ended at `boson-claim-integrity-verified` with zero promoted physical-mass
claims.

A5 FOUNDATION REFINEMENT (2026-07-18, Amendment A18; Phases520-522): registry
entries 520-522 and the A18 plan are allocated and implemented. Phase520
exact-binds the generator, serialized A5 record, executable action-family
sources, and Phase517/519 findings. Its complete frozen `A5Result` field
inventory has no dedicated qualifier, control, or member field, and none of
the eleven committed serialized string values preserves the generator's
toy/control qualifier. It does not claim generic schema non-representability
or infer a serialization cause or semantic applicability. The
executable subject contains two action members, the family declares no
canonical member, and the A5 routine binds neither. Two registered parity
assertions remain prose/comment evidence rather than exact derivations; no
exhaustive mathematical-absence claim is made. The terminal is
`action-member-unresolved`.

Phase521 freezes the committed Kuhn decomposition as a negative control and a
periodic cubical-cell-poset barycentric subdivision as an audit-authored
candidate before scoring. Independent cell-rank, barycentric f-vector,
maximal-chain, incidence, coordinate-integrality, and reflection-
representation controls pass at extents 3 and 4. The n=4 negative control
reproduces 2,048 mapped edges, 3,072 mapped faces, and zero mapped
four-simplices for both reflections. On the barycentric candidate, both the
site- and link-centered reflections preserve every tested simplex. The
terminal `finite-dual-reflection-compatible-candidate-survives` is finite-only:
neither geometry nor reflection is selected, registered, installed as the
committed mesh, or promoted to an orientation/all-volume theorem.

Phase522 exact-binds the complete Phase517-521 chain and reduces the frozen
menu without taking a Cartesian product of summary labels. The two actual
barycentric site/link pairs survive as unregistered diagnostics. Its earliest
blocker remains `action-member-or-omega-parity-unresolved`; reflection
pullback/boundary, O1/O2 positive-time membership, normalized measure/domain,
target equality, Hermiticity, finite-scope, necessity, gluing, and all-scope
certificates remain open. The package is not independent-review-ready,
Phases515-516 remain locked, Phase458 G1 and L8 remain open, external review
remains pending, and `promotedPhysicalMassClaimCount=0`. Registry 523+ is
unassigned. Phase202 is now 306 passed / 3 standing failures after the three
A18 checklist rows. The mandatory incremental checkpoint ran 83 steps, skipped
303, and ended at `boson-claim-integrity-verified` with zero promoted physical-
mass claims.

PHASE458 CONTRACT (2026-07-15, Phase478): the G1-G6 four-state schema, exact
upstream terminal allowlists, source bindings, outcome precedence, and G6
projection are committed and machine-checked. The projection uses one
Phase456-default-equivalent five-column workload at n=3,4,5,6, n^4 volume
scaling, and a 1.5 safety factor, yielding 1.5044386598 CPU-weeks against the
inclusive 2.0 CPU-week CPU boundary. Current counts are 2 available-true, 1
available-false, 3 missing, and 0 invalid/drifted. Phase478 did not evaluate
Phase458, infer an O4 ruling, or authorize Binder/CUDA/production work; its
terminal is `gate-specification-closed-phase458-inputs-incomplete`.

PHASE457 READINESS (2026-07-15, Phase479): the unsafe legacy parser that
accepted a two-field boolean/signer JSON at three candidate paths is retired.
Phase457 now consumes only a verified normalized Phase480 C3 derivative with
the exact probe-only option and five scope assertions. The memo schema's stale
nonexistent coverage-manifest path was repaired to the actual exact coverage
contract; the current schema/template hashes are `989a49b6...d82a` and
`50106013...2a4b`. The full Phase466 pin is green, but no human C3 derivative,
eligible real Arm-Q ensemble, RNG/FRESH provenance, or prospective motivation
pack exists. The hold remains closed, Arm Q and Stage B remain unrun, and L7
remains withheld. Phase479's terminal is
`post-ruling-readiness-contract-closed-hold-remains`.

Current gate status after the Phase448 work (plus the 2026-06-12 platform
fix - GPU parity defect root-caused and discharged - and the 2026-07-01
|Y|=1/2 calibration defect fix in the Phase411/417 informational SM
censuses):

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=245`,
  `checklistFailedCount=3`
- Claim integrity:
  `boson-claim-integrity-verified`,
  `sourceLineageMissing=true`, `wzMissingFieldCount=15`,
  `higgsMissingFieldCount=14`, `promotedPhysicalMassClaimCount=0`
- Phase389:
  `vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed=True`,
  `discreteGaugeCompatibilityIdentityExact=True`,
  `canFillPhase201WzContract=False`
- Phase390:
  `convergedControlBranchFermionModeRebuildPassed=True`,
  `persistedPhase12ModeBranchUnconverged=True`,
  `wardZeroCurrentSharplyTested=True`
- Phase391:
  `denseReplayVerdict=confirmed` (Gram invariants solver-independent)
- Phase392:
  `actionDerivedResponseStructureVerdict=diverges-from-gram-structure`
  (suppressed axis is metric-dependent)
- Phase393:
  `shellAggregatedSourceCancels=True`, `perModeSourceLiesInGramImage=True`
- Phase394:
  `firstOrderAsymmetricBackreactionConstructed=True`,
  `tripletClusteringObserved=True`
- Phase395:
  `suppressedAxisIsGaugeCovariantNotCanonical=True`,
  `blockGramEffectivelyRankOne=True`
- Phase396:
  `tripletNeutralChargedSplitObserved=True` (all 68 triplets, exact)
- Phase397:
  `neutralMixingElementVanishesInFermionBilinearChannel=True`,
  `photonZSeparationUnderdetermined=True`
- Phase398:
  `vo6ControlBranchComponentsComplete=True` (5/5),
  `vo7ControlBranchComponentsComplete=True` (4/4),
  `physicalCompletionStillMissing=True` (8-item gap ledger)
- Phase399:
  `quadraticModelCoupledCriticalPointSolvePassed=True`,
  `quadraticModelCoupledFixedPointConverged=True` (8/8 runs, gradient <= 9.5e-11),
  `flatDirectionObstructionPresent=True` (0.047 per unit kappa),
  `kappaScalingConsistentWithFirstOrder=True`,
  `vo7CoupledStationarityControlComponentDischarged=True`
- Phase400:
  `fullBosonicActionFlatDirectionLiftProbePassed=True`,
  `liftVerdict=all-lifted-with-saddle-directions` (36/36 kernel directions
  lifted, 0 exactly flat, quartic norms 1.2e-3..5.8e-2, saddle depth
  <= 4.5e-19 residual-scale),
  `kernelObstructionFullyRelaxableAtHigherOrder=True`,
  `minObstructionLiftedFraction=1`
- Phase401:
  `fullQuarticActionCoupledCriticalPointConstructionPassed=True`,
  `noPerturbativeCoupledCriticalPointFound=True` (12/12 coupled runs exit
  their trust regions without stationarity; kappa=0 baselines converge to
  2.7e-17),
  `valleyAnisotropyRatio=1.4e8` (positive-relaxed near-null valleys of the
  quartic form), `maxAdiabaticSourceNormGrowth=7.4`
- Phase402:
  `guDraftScalarRouteDictionaryAuditPassed=True`,
  `higgsPotentialIsUpsilonPairingInPrimary=True` (draft eq. 12.28 places
  the Higgs potential at <U,U> and Higgs KG at D*U=0 - the repo's Mode-B
  objective and stationarity, verified at the backgrounds),
  `routeRequiresDoubletEquivalentSubstructure=True` (adjoint-triplet VEV
  cannot produce the SM neutral sector; doublet can),
  `zeroDimensionfulAnchorsInPrimary=True` (no GeV/246 anywhere in the
  primary draft)
- Phase403:
  `adjointDoubletSubstructureBranchingProbePassed=True`,
  `su2AdjointHasNoDoubletBlock=True` (toy branch algebra too small -
  explains the Phase397 underdetermination),
  `su3AdjointContainsConjugateDoubletPair=True` (|Y0| = sqrt(3)/2),
  `custodialPatternProducedByAdjointDoubletBlock=True` (identity residual
  0.0), `embeddingTanSquared=3` (derived, recorded blind),
  `couplingRatioLineageMechanismIdentified=True`
- Phase404 (brute force #1 of the user directive):
  `guEmbeddingChainCouplingRatioEnumerationPassed=True`,
  `standardTanSquaredEmb=3/5` (sin^2 = 3/8; derived blind, internal
  lepton-doublet normalization; scan menu over 224 directions),
  `familyPatternDerived=True` (16 -> 1/6, 1/2, 2/3, 1/3, 1, 0),
  `adjointColorSingletChargedDoubletAbsentEverywhere=True` (the SM-Higgs
  doublet CANNOT come from the gauge-adjoint part of the connection on
  this chain - non-adjoint components required)
- Phase405 (brute force #2 of the user directive):
  `vacuumManifoldDoubletVevOrbitScanPassed=True`,
  `vacuumManifoldPermitsConstantDoubletVev=True` (all rank-1 directions
  exactly flat), `flatnessEqualsCommutativity=True` (11 = 11 over 2592
  samples, exact quartic landscape),
  `noSelectionMechanismAtConstantRank1Level=True` (sub-gap (b) open,
  sharpened), `gpuParityDefectDetected=False` (the originally detected
  defect was root-caused 2026-06-12 to a GpuSolverBackend.Initialize
  lifecycle bug - NOT the native kernel - fixed and regression-tested;
  post-fix parity 27/27 at maxAbsDev 3.9e-34; science ran on CPU per
  IA-5 throughout). As of 2026-06-16 the default validation path is
  CPU-only (`gpuParitySkippedByDefault=True`, `gpuParityGateSatisfied=True`)
  so the generator does not initialize both CPU and CUDA paths; set
  `PHASE405_ENABLE_GPU=1` only when explicitly re-running the CUDA parity
  characterization.
- Phase406 (brute force #3 of the user directive):
  `choiceSpaceFalsificationSweepPassed=True`,
  `ratioPathIndependent=True` (su(5) route tan^2 = 3/5 = Pati-Salam),
  `signatureAxisVerified=True` (Cl(6,4)/Cl(7,3) both 16-dim chiral),
  `survivorsAreExactlyNonAdjointLargerAlgebra=True` (4 of 16 combinations),
  `noCombinationProvidesVevSelection=True` (binding gaps are
  CHOICE-INDEPENDENT)
- Phase407:
  `chimericAdjointSmContentProbePassed=True`,
  `higgsPatternDoubletExistsInChimericAdjoint=True` (16 states with
  color-singlet j=1/2 |Y|=1/2 - the SM-Higgs quantum numbers - in the
  frame-cross-internal (4 x 10) block of the chimeric adjoint 91),
  `higgsPatternCarriesSpacetimeVectorIndex=True` (spin-0 extraction via
  the Y14 vertical-form structure is the named next step),
  `internalSectorStillExcluded=True` (Phase404 re-confirmed in the
  bigger frame)
- Phase408:
  `verticalSpinZeroExtractionObstructionProbePassed=True`,
  `weldEntanglesSpinAndIsospin=True` (pi(so(4)) commutes with NO SM
  generator), `centralizerIsTrivial=True` (kernel dim 0),
  `spinZeroSlotCannotCarryFullDoublet=True` (trace slot is 1-dim vs the
  4-real-dim doublet - the naive vertical-trace extraction is OBSTRUCTED
  for any weld alignment; the draft's epsilon/Shiab machinery is the
  named open route)
- Phase409:
  `invariantPairingMenuSpinZeroExtractionProbePassed=True`,
  `linearSpinZeroContentIsZero=True` (V = 4x10 has no singlet - every
  linear extraction closed, any parity),
  `bilinearSpinZeroDoubletAbsent=True` (the complete 7-dim bilinear
  spin-0 sector, 6 parity-even + 1 epsilon-built parity-odd, contains
  no SM-doublet state; SM-stable subspace 1-dim),
  `oddOrderSpinZeroForbidden=True` (all welded irreps half-integer
  pairs; trilinear count exactly 0),
  `obstructionMenuCompleteThroughBilinearOrder=True` (the epsilon route
  on frame-cross content is CLOSED through bilinear order; remaining:
  quartic+ even composites, epsilon/Shiab on content beyond the
  frame-cross block, or a different welded carrier)
- Phase410:
  `curvatureCoupledVevSelectionProbePassed=True`,
  `curvatureCouplingProducesRunawayAlongFlatRays=True` (every rank-1
  vacuum ray exactly flat -> unbounded descent under the uniform
  negative-mass-squared curvature term),
  `maxDepthStratumBlockDegenerate=True` (the lifted sector's deepest
  stratum, ||[u,v]||^2 = 1/4 with 16 pairs, spans doublet-doublet AND
  doublet-triplet pairs), `doubletVevSelectedByCurvatureCoupling=False`
  (the uniform bosonic curvature-coaxing realization is CLOSED;
  selection requires direction-dependent coupling or the Dirac-sector
  realization)
- Phase411:
  `quarticDiracSquaredSpinorCompositeProbePassed=True`,
  `leftRightBilinearChannelHasNoWeldedScalar=True` (the Dirac mass/
  Yukawa channel S_L x S_R contains NO welded scalar at all - its
  (half,int) x (int,half) content cannot pair to a singlet),
  `majoranaSpinZeroSmDoubletCount=0` (the Majorana channels' 16 welded
  scalars carry no SM-doublet state),
  `spinorBilinearSpinZeroDoubletAbsent=True` (the Dirac-squared
  composite reading is CLOSED at bilinear-channel level; named
  remaining: quartic SM-stable analysis, unobserved-phase fields,
  noncompact real-form evasion)
- Phase412 (user directive 2026-06-12):
  `quarticSmDoubletIntersectionAnalysisPassed=True`,
  `quarticWeldedScalarSmDoubletAbsentAllChannels=True` (the ambient
  16^4 intersection of channel-allowed welded isotypics with the
  SM-doublet isotypic is ZERO in every channel; top Gram eigenvalues
  <= 0.604 vs the required 1.0 - decisive margins; covers all
  statistics projections), `doubletIsotypicRealDimension=480`,
  new data: odd-mixed channels LLLR/LRRR carry zero welded singlets;
  THE COMPOSITE PROGRAM IS CLOSED THROUGH QUARTIC ORDER on every
  probed carrier
- Phase413:
  `noncompactRealFormTransferProbePassed=True`,
  `complexifiedWeldsCoincide=True` (the Lorentzian Sym^2(R^{1,3}) weld
  complexifies exactly to the compact weld under T4 = diag(i,1,1,1),
  residual 2.2e-16 - every complex-linear no-go count transfers),
  `inducedFormSignature=(7,3)` (matching the Phase406 Cl(7,3) axis),
  `linearCountTransfers=True` and `bilinearCountTransfers=True` (direct
  noncompact recomputation: 0 and 7, equal to compact),
  `realFormTransferEstablished=True` (THE NO-GOS ARE REAL-FORM
  INDEPENDENT; the noncompact evasion is closed on finite-dimensional
  carriers; named residuals: real-structure bookkeeping, unitary
  category)
- Phase414:
  `generalShiabEpsilonOperatorAnsatzProbePassed=True`,
  `requestedOperatorAlphabetCovered=True` (wedge, Hodge star, contraction,
  commutator, `i`-weighted anticommutator, Clifford volume, epsilon
  conjugation),
  `operatorMenuCandidateCount=13` (11 closed low-order families, 2 open
  residual families),
  `lowOrderProbedCarrierFamiliesClosed=True`,
  `anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet=False`,
  `onlyOpenFamiliesRequireNewCarrierOrSourceSpecification=True`
  (remaining: computable unobserved-phase carrier, or explicit first-order
  cohomology/square-root `delta_omega` specification)
- Phase415:
  `fermionicCohomologySquareRootAnsatzProbePassed=True`,
  `requiredSpecificationFieldCount=8`,
  `suppliedRequiredSpecificationFieldCount=0`,
  `localCandidateComplexCount=6` (3 locally specified and closed, 3 open
  because they require new source specification),
  `specifiedLocalCandidatesClosed=True`,
  `candidateComplexesWithWeldedScalarSmDoubletCount=0`,
  `candidateComplexesWithObservedProjectionDataCount=0`,
  `deltaOmegaStillRequiresSpecification=True`,
  `unobservedPhaseStillRequiresCarrier=True`
- Phase416:
  `unobservedPhaseCarrierCensusPassed=True`,
  `sourcePinnedUnobservedCarrierCount=3`,
  `computableUnobservedCarrierCount=3`,
  `draftZetaDecompositionDimensionCheck=True` (`64 + 192 + 576 = 832`),
  `sourcePinnedUnobservedLinearBosonicSpinZeroCandidateCount=0`,
  `sourcePinnedUnobservedWeldedScalarSmDoubletCandidateCount=0`,
  `unobservedPhaseStillRequiresBosonicMap=True`,
  `vectorSpinor144RemainsConcreteNextCarrier=True`
- Phase417 (re-run 2026-07-01 with the exact |Y|=1/2 calibration fix):
  `vectorSpinor144DecompositionProbePassed=True`,
  `gammaTraceRankComplex=16`,
  `vectorSpinor144KernelComplexDimension=144`,
  `vectorSpinor144DimensionCheck=True`,
  `vectorSpinor144InvariantUnderProbedGenerators=True`,
  `yHalfCalibrationExact=True`,
  `internalSmHiggsPatternComplexDimension=6` (CORRECTED from the pre-fix 0:
  the old ">0.05" heuristic tested |Y|=1/3; the 144 does contain a
  6-complex-dim color-singlet j_L=1/2 |Y|=1/2 FERMIONIC block - it is not
  a welded scalar, so the linear no-go stands),
  `linearWeldedScalarCountTotal=0`,
  `vectorSpinor144StillRequiresBosonicProjectionMap=True`
- Phase418:
  `directionDependentCurvatureVevCouplingScanPassed=True`,
  `blockProjectorsCommuteWithResidualGaugeAction=True`,
  `pureQuadraticDirectionDependentCouplingsStillRunAway=True`,
  `stabilizedCandidateSelectsDoubletCount=5`,
  `nonTautologicalDoubletSelectorCount=4`,
  `sourceDefinedDoubletSelectorCount=0`,
  `directionDependentCouplingSourceLawStillMissing=True`,
  `finiteVevScaleStillExternal=True`
- Phase419:
  `observedFieldSymbolicExtractionTemplatePassed=True`,
  `allPhase256FieldsMappedBySymbolicTemplate=True`,
  `projectionRowCount=4`,
  `sourceDefinedPhase256FieldCount=0`,
  `phase295IntakeReadyCandidateCount=0`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase420:
  `naiveCurvatureMassScaleSanityCheckPassed=True`,
  `literalScalarCurvatureMassReadingDimensionallyConsistent=False`,
  `squaredMassCurvatureReadingDimensionallyConsistent=True`,
  `squaredMassReadingProvidesOnlySymbolicScaleShell=True`,
  `providedRequiredScaleSpecificationFieldCount=1`,
  `missingScaleSpecificationFieldCount=9`,
  `sourceProvidesGeVUnitNormalization=False`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase421:
  `coxGuIvV2LcdmRigBosonContractAuditPassed=True`,
  `lcdmRigScopeConfirmed=True`, `rigHookCount=7`,
  `electroweakProjectionHookCount=0`,
  `absentContractKeywordCount=14`,
  `sourceProvidesBosonContractEvidence=False`,
  `sourceProvidesPhotonWzHProjectionRows=False`,
  `sourceProvidesGeVUnitNormalization=False`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase422:
  `vectorSpinor144BilinearScalarCapacityAuditPassed=True`,
  `leftCarrierRealDimension=576`,
  `rightCarrierRealDimension=576`,
  `sameChiralityScalarCapacity=528`,
  `mixedChiralityScalarCapacity=0`,
  `mixedChiralityDiracLikeScalarChannelClosed=True`,
  `sameChiralityMajoranaLikeScalarSectorPresent=True`,
  `directSmStableAnalysisDeferredDueToLargeSector=True`,
  `vectorSpinor144BilinearStillRequiresSourceProjectionMap=True`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase423:
  `zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed=True`,
  `currentJune2026SourceDeltaConfirmed=True`,
  `sourceProvidesSpinorialDarkSectorContext=True`,
  `sourceMentions95GevDilaton=True`,
  `sourceMentionsKoideDilatonShift=True`,
  `sourceUsesExternalElectroweakVev246Gev=True`,
  `sourceProvidesVectorSpinor144ProjectionMap=False`,
  `sourceProvidesWzSourceRows=False`,
  `sourceProvidesHiggsScalarSourceRow=False`,
  `sourceProvidesObservedElectroweakNamespaceMap=False`,
  `sourceProvidesGeVUnitNormalization=False`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase424:
  `vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed=True`,
  `llIntersectionRealDimension=0`, `rrIntersectionRealDimension=0`
  (candidate weight sectors 3584 each, doublet isotypics 1344 real dims
  each, top Gram eigenvalues 0.059259 vs the required 1.0),
  `sameChiralityWeldedScalarSmDoubletAbsent=True`,
  `majorana16AmbientRecheckSmDoubletAbsent=True` (corrected Phase411
  verdict confirmed by the strictly stronger ambient method),
  `vectorSpinor144BilinearCompositeRouteClosed=True`,
  `characterCapacitiesMatchPhase422=True` (264/264/0),
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase425:
  `crossCarrierBilinearSmDoubletCompletionAuditPassed=True`,
  `gamma4FrameExact=True`, `sixWeldedContentExact=True` (Q_{3/2} built
  exactly: 6 = gamma-traceless remainder in 4 x 2 = 2 + 6, content
  (1/2,1)/(1,1/2)), `qLinearCarrierHasNoWeldedScalar=True`,
  `allCapacitiesMatchExpectations=True` (complex capacities: Z x S = 13,
  Q x Q = 14, Q x S = 6, Q x Z = 29 per chirality pair; all seven
  mixed-parity channels exactly 0),
  `crossCarrierWeldedScalarSmDoubletCount=0`,
  `allNonzeroChannelsSmDoubletAbsent=True` (top Gram eigenvalues
  0.017-0.444 vs required 1.0),
  `mirrorChannelsTransferFromDecidedChannels=True`,
  `bilinearCompositeLayerClosedOnAllSourcePinnedCarriers=True`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase426:
  `coxGuSeriesBosonContractAuditPassed=True`, `seriesRecordCount=5`
  (Cox GU I-V, zenodo 20550275/20517363/20517502/20518853/20531776),
  `guIiDerivesTanSquaredThreeFifthsAtUnification=True`,
  `guIiKernelRelationCorroboratesPhase404=True` (independent tree-level
  corroboration of the blind tan^2 = 3/5 embedding result),
  `guIiNamesPatiSalamBiDoubletScalarChannel=True` (candidate (1,2,2) seed
  only; not proven realized),
  `guIiProvidesScalarPotential=False`, `guIiProvidesVevOrScale=False`,
  `guIiProvidesMassSpectrumOrPole=False`,
  `sourceProvidesWzSourceRows=False`,
  `sourceProvidesGeVUnitNormalization=False`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase427:
  `hofsethGuRvgSuperluminalSourceAuditPassed=True`,
  `originalRecordTombstoned=True` (zenodo 21056575 deleted 2026-07-01 as
  "duplicate"), `liveSuccessorRecordLocated=True` (zenodo 21117379),
  `weinsteinCoAuthorshipExternallyVerified=False`,
  `sourceUsesExternalElectroweakVev246Gev=True` (imported input for the
  27.2 TeV dilaton decay constant),
  `sourceMarksCondensateAmplitudeAsObservationalInput=True`,
  `sourceProvidesWzSourceRows=False`,
  `sourceProvidesGeVUnitNormalization=False`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase428 (first beyond-the-literature experiment):
  `fermionLoopBlockSelectionNoGoProbePassed=True`,
  `fermionLoopClassFunctionOnRankOneRays=True` (closed-form ray spectrum
  verified 1.6e-13; conjugacy witness exact),
  `tripletDoubletFermionLoopExactlyDegenerate=True` (1.1e-13, both
  fundamental-3 and adjoint-8),
  `fermionLoopProvidesPositiveQuarticStabilizer=False` (large-t slopes
  -N log t match coupled-mode counts exactly),
  `doubletSelectedByFermionLoop=False`,
  `fermionLoopBlockSelectionMechanismClosed=True`
  (su(3)-breaking fermionic structure required and not source-defined),
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase429 (second beyond-the-literature experiment):
  `targetBlindDimensionlessRatioLedgerPassed=True`,
  `exactArithmeticVerified=True` (tan^2 = 3/5 -> sin^2 = 3/8,
  GUT-normalized ratio 1 fixed; tree m_W/m_Z = sqrt(5/8) conditional on
  the missing breaking sector), `fixedRowCount=3`, `conditionalRowCount=1`,
  `comparisonLineageFieldCount=6`,
  `sourceDefinedComparisonLineageFieldCount=0`,
  `measuredElectroweakValuesConsulted=False`,
  `derivationComparisonSeparationMaintained=True`,
  `canFillPhase201WzContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- Phase430 (team wave): `netOneLoopDirectionSelectionProbePassed=True`,
  `netOneLoopDirectionSelective=True`,
  `derivedContentSelectsHyperchargeAxis=True`,
  `su3ToSu2U1BreakingDirectionDynamicallyPreferred=True`,
  `noFiniteMinimumOnRays=True` (no scale law),
  `bosonicOneLoopIsRecordedWorkbenchModel=True`
- Phase431 (team wave): `lambda8BackgroundDoubletReopeningProbePassed=True`,
  `tdDegeneracyBrokenByBackground=True`,
  `backgroundInducesBlockDependentMassLaw=True`,
  `backgroundParameterT8DynamicallyDerived=False` (candidate-only),
  `scaleLawProduced=False`
- Phase432 (team wave): `weldedFermionLoopBlockSelectionProbePassed=True`,
  `weldBreaksInternalLoopDegeneracies=True`,
  `weldStructureSourcePinned=True`,
  `weldCouplingNormalizationSourceDefined=False`
- Phase433 (team wave): `blindBetaCoefficientRunningLedgerPassed=True`,
  `exactRationalArithmeticVerified=True`, `betaLedgerRowCount=4`,
  `b1MinusB2FamilyIndependent=True` (blind slope -55/32 / -109/64),
  `measuredElectroweakValuesConsulted=False`
- Phase434 (team wave): `conditionalObservedFieldExtractionRowLedgerPassed=True`,
  `extractionRowsExact=True` (6 rows; template partition 7/4/9 of 20),
  `candidateVevExistenceEstablished=False`,
  `isObservedFieldExtractionTheorem=False`
- Phase435: `twoCondensateScaleGapProbePassed=True`,
  `derivedAxisRunawayUndercutsInterior=True` (interior LOCAL two-condensate
  minimum exists for the derived content, gradient 5e-4, but the lambda_8
  runaway undercuts it), `fundamentalShowsNoCondensationOnset=True`,
  `finiteSelfConsistentScaleExists=False`,
  `scaleRequiresLogSaturationBeyondWorkbench=True`,
  `logSaturationStructureSourceDefined=False` (THE quantified scale gap)
- Phase436: `exactHessianSaturationNoGoProbePassed=True`,
  `exactHessianMassesGrowExactlyAsTSquared=True` (H(t) = A0 + t A1 + t^2 A2
  exactly; third t-difference 0),
  `logSaturationImpossibleFromExactControlBranchHessianAtOneLoop=True`,
  `scaleGapPinnedBeyondControlBranch=True` (theorem-grade: the gap is the
  control branch itself, not workbench modeling),
  `phase430SlopeCountsConfirmedByExactHessian=True` (64 = 4x16, 96 = 6x16),
  `workbenchMassValuesDifferFromExactHessian=True` (recorded honestly)
- Phase437: `fourDimensionalTransmutationScalingProbePassed=True`,
  runaway STRUCTURAL (4D per-volume slopes identical to 2D; continuum CW
  t^4 log t regime unreachable on bounded-dispersion lattices; no
  transmutation minimum at any L; cwFitIsIllConditioned honesty note)
- Phase438: `selfConsistentCondensateGapEquationProbePassed=True`,
  `gapEquationHasNontrivialSolutions=True`,
  `dynamicalScaleGenerationObserved=True` (FIRST internal dynamical
  scale: g2_crit falls with volume, R^2=0.998; exponential scale-law
  signature R^2 0.83-0.90), `criticalCouplingFallsWithVolume=True`,
  `hyperchargeChannelCompetitiveWithSinglet=False` (the singlet wins:
  scale and breaking direction live in DIFFERENT mechanisms);
  mean-field + convention + candidate-only boundaries recorded
- Phase439: `gapEquationLambda8BackgroundChannelSteeringProbePassed=True`,
  `backgroundInducesChannelSteering=True` (strict margins, monotone in
  t8), `dynamicalMassPatternAlignsWithSu2U1=True` (Sigma_1=Sigma_2 !=
  Sigma_3 - automatic alignment from lambda_8's (x,x,-2x) eigenvalues),
  `transmutationSignatureSurvivesBackground=True` - THE CANDIDATE-LEVEL
  LOOP IS CLOSED: direction selection (430) -> background steering (439)
  -> aligned dynamical masses with a transmutation scale (438); the one
  remaining link is self-consistency of t8 itself
- Phase440: `coupledBackgroundCondensateFixedPointProbePassed=True`,
  HONEST NEGATIVE: `jointFixedPointExists=False` (29 trivial / 19
  runaway / 0 interior over 48 configs);
  `condensateSaturatesFermionicRunaway=True` (delays but cannot stop -
  at large t8 the background gaps out the condensing IR modes and the
  joint-optimal Sigma falls to zero); boundary decomposition pins the
  failure on the workbench bosonic sector - the THIRD independent
  convergence (with 435/436) onto the physical VO-6/VO-7 structure or a
  source anchor
- Phase441: `toyBranchFamilyUniversalitySweepPassed=True` - ALL SEVEN
  universality verdicts hold across the 36-member family (Shiab x
  torsion x A0): degree-2 Upsilon (1.5e-15), quadratic Hessian
  decomposition (2.8e-14), counts 64/96 invariant, runaway -640
  universal; the canonical physical Shiab is NOT realizable on the toy
  (dimX>=4 + Cl(7,7)/128 spinor basis required); TERMINAL FRONTIER:
  `scaleGapRequiresDimFourSpinorShiabOrSourceAnchor=True`
- Phase442 (ON THE USER-APPROVED DIMENSION-FOUR PLATFORM, commits
  13725f97..f33bc791): `jointOmegaThetaHessianDegreeProbePassed=True`,
  `controlArmReproducesPhase436DegreeTwo=True` (2.6e-15),
  `isolationThetaBlockExactlyDegenerate=True` (bit-exact 0 vs 6.6e-2),
  `einsteinianJointHessianDegreeExceedsTwo=True` (all nine members,
  BOTH vertex->face rules), `honestySweepPassed=True`,
  `noScaleProduced=True`, `degreeLiftIsNecessaryNotSufficientForScale=True`
  - THE DRAFT-CANONICAL SHIAB BREAKS THE EXACT-QUADRATICITY: the
  structural wall against a dynamical bosonic scale is ABSENT on the 4D
  platform (necessary-not-sufficient; nothing promoted)
- Phase443: `jointEffectivePotentialSaturationProbePassed=True`,
  `variationalThetaStationaritySolved=True` (3.4e-9 relative at every
  composite point), `identityControlShowsNoSaturation=True`,
  `einsteinianLogSaturationObserved=False` - the honest verdict: the
  degree-lift does NOT saturate V_eff at one loop on the minimal
  16-vertex 4D mesh (necessary satisfied, sufficiency not yet);
  named levers: CreateUniform4D(2)+ with smarter Hessian assembly,
  two-loop/RG improvement, richer Phi menus
- Phase444: `phase444Passed=True`,
  `modeVolumeChangesVerdict="undetermined-tooling-blocked"` - the
  mode-volume lever is BLOCKED at three measured levels (lowest-index
  rule 57% non-covariant; raw bivectors violate the minimal-image
  contract, 75% seam faces; DEFINITIVE: Gu.Geometry global-index
  orientation conventions break translation covariance - even ||F||^2
  fails at 2.5e-4; SLQ ~60s/Hv infeasible). Additive conventions kept
  (VertexFaceRule.IncidentAverage; latticePeriod minimal-image);
  TWO NAMED UNLOCK PROJECTS for user decision: lattice-canonical
  geometry conventions OR adjoint/joint-gradient for SLQ
- Phase445: `rgImprovedJointPotentialProbePassed=True`,
  `verdictSchemeStable=False`, `einsteinianRgSaturationObserved=False`
  - THE FIRST INTERIOR FINITE MINIMA IN THE PROGRAM'S HISTORY appear
  under RG improvement (t* ~ 1.5-2.25, measured running strongly
  positive vs identity negative, controls pristine) but the verdict
  flips at the widest fit window: a SCHEME-DEPENDENT CANDIDATE,
  suggestive not established; phase446 must resolve the scheme
  dependence (no platform work needed)
- Phase446: `rgSchemeDependenceResolutionProbePassed=True`,
  `resolutionKind=fit-normalization-artifact`,
  `phase445MinimaResolvedAsFitNormalizationArtifact=True`,
  `einsteinianRgSaturationObserved=False`,
  `candidateSurvivesSchemeControl=False` - RESOLVED: the Phase445
  minima are normalization leakage of the deep-negative one-loop
  constant into the {t^4} fit basis. Replication exact on the strided
  48-point grid; constant subtraction kills every minimum (the raw
  V_eff argmin is provably shift-invariant); the 4-basis fit menu is
  irreducibly scheme-dependent (no member survives; one enriched
  scheme even hands the IDENTITY control a manufactured minimum); a
  provably-monotone SYNTHETIC control gets a manufactured minimum from
  the verbatim scheme (and constant subtraction kills that too); the
  constant-immune direct measurement (L = t d/dt, jump-aware, 30-40
  usable intervals, offset-immunity exact) supports NO minimum (sd2
  cL=-0.79; asd2 cL=+4.45 with t* far out of range). THE RG-IMPROVED
  POTENTIAL-FIT ROUTE ON THE MINIMAL MESH IS CLOSED; frontier returns
  to the Phase444 unlock projects, beyond-one-loop structure, or a
  source anchor
- Phase447: `twoLoopSaturationProbePassed=True`,
  `resolutionKind=non-perturbative-or-convention-bound`,
  `twoLoopVerdictAdmissible=False`, `perturbativeRegime=False`
  (max |V_2loop|/|V_1loop| = 1.6e3, median 155),
  `floorSweepStable=False`, `twoLoopCandidate=False` - the GENUINE
  two-loop vacuum terms (figure-eight + soft-mode sunset on the
  positive-subspace propagator; all numerical batteries green incl.
  the exact-quartic identity anchor at 1.3e-7) are IR-dominated,
  convention-bound, and violate perturbativity at the minimal-mesh
  backgrounds: THE TWO-LOOP LEVER IS CLOSED AS NON-PERTURBATIVE AT
  THIS SCOPE. Structure result: S_B is EXACTLY quartic in omega and
  transcendental in theta (resolves the Phase442 degree finding).
  The internal no-platform program at the minimal mesh is EXHAUSTED
  THROUGH TWO LOOPS (443 one-loop no; 446 RG-fit artifact; 447
  two-loop non-perturbative); saddle/propagator convention
  physicistReviewPending
- Phase448: `torusModeVolumeSaturationProbePassed=True`,
  `modeVolumeVerdict=no-saturation-persists-across-mode-volumes`,
  `anySaturationAnyVolume=False`, `volumeTrendSeedStable=True` - THE
  PHASE444 QUESTION IS ANSWERED using both completed unlock projects:
  on lattice-canonical tori n=3/4 (joint DOF 3888/12288) with
  translation-INVARIANT rays, the joint Hessian is BLOCK-CIRCULANT
  and its EXACT spectrum comes from 48 orbit-representative
  Hessian-vector products per point (covariance 9.5e-15/1.4e-14;
  block-reconstruction 2.9e-11; 12.7 min total vs the pre-unlock
  ~2h/point). Every Einsteinian member stays trivial-origin at 5x and
  16x the minimal mode count: THE ONE-LOOP NO-SATURATION VERDICT IS
  NOT A SMALL-MESH ARTIFACT - phase443's mode-volume lever is decided
  negative. Invariant-ray convention recorded; physicistReviewPending
- Phase449: `variationalGaussianEffectivePotentialProbePassed=True`,
  `verdictKind=gap-equation-breakdown`,
  `einsteinianGaussianSaturationObserved=False`,
  `hartreeSelfConsistentSolutionExistsEverywhere=False` - the review
  board's #1 experiment (diagonal CJT-Hartree rigorous bound) decided
  on its pre-registered third outcome: the diagonal-Gaussian family
  FAILS TO EXIST on 32/64 Einsteinian points (identity control clean
  everywhere; genuine ansatz-basis dependence recorded by the anchor
  rotation check; all numerical batteries green). The cheapest
  non-perturbative rung cannot hold the negative-mode structure; the
  scale question passes intact to the phase450 constraint-EP HMC
  (ansatz-free, four binding conditions)
- Phase450: `constraintEffectivePotentialHmcProbePassed=True`,
  `verdictKind=inconclusive-gates-failed`,
  `allEinsteinianSingleWell=True`, `anyEinsteinianFlatBottom=False`,
  `controlClean=True`, `onlyAntisymmetryGatesFailed=True` - the
  ansatz-free umbrella/WHAM CEP under all four binding conditions:
  BOTH production runs show single-well-at-zero everywhere (large-beta
  Pearson 0.9996) but the parity-antisymmetry gate trips at 5.06 sigma
  while the independent tadpole is zero (-0.04+-0.10) - an
  unattributed WHAM stitching-error-model item; phase450 itself did
  NOT claim the null. RESOLVED BY PHASE453 (2026-07-10): the 5.06
  sigma was a WHAM stitching artifact (shared f_i covariance omitted
  from the per-bin error model; the same class also arises at
  mixed-stiffness ladder junctions) - see the Phase453 bullet
- Phase453: `whamParityErrorModelRepairPassed=True`,
  `verdictKind=symmetric-phase-null` (T1) - the corrected error
  models (Arm A: 400-replicate moving-block bootstrap with a full
  WHAM re-solve per replicate, propagating the f_i stitching
  covariance; Arm B: within-window antisymmetrized U_odd; thresholds
  CALIBRATED on 2000 synthetic even-CEP ensembles, baked sigma99=3.16)
  are BOTH GREEN on BOTH members (identity 2.49/0.89, sd2 2.30/2.46),
  single-well-at-zero everywhere, fresh tadpoles zero (0.217+-0.195 /
  0.026+-0.088), identity control clean, all batteries green. THE
  SYMMETRIC-PHASE NULL IS CLAIMED; the frontier statement is now
  "NO NON-PERTURBATIVE SCALE ALONG TRANSLATION-INVARIANT RAYS AT n=3"
  (tree 443 / one-loop 446 / two-loop 447 / Hartree 449 / HMC CEP
  450+453), scoped to the audited member family and workbench
  conventions (physicistReviewPending). The two budgeted T3
  iterations are committed diagnostics: iteration 2 PROVED the
  artifact class by localizing a statistics-growing odd residual
  (7.14/6.50 sigma) at the |Phi|=0.25 mixed-stiffness sub-ladder
  junction with the direct tadpole at zero and the uniform-ladder
  identity clean - LADDER LESSON: umbrella programs use
  uniform-stiffness ladders or junction-aware error models. Beyond-ray
  directions and volume scaling are the live B2 candidates (team B
  ranks 2-6)
- Phase451: `twoLoopUnificationLedgerPassed=True`,
  `verdictKind=tension-persists-quantified` - sin^2(m_Z) predicted
  0.2076 (1L, referee witness) / 0.2106 (2L) from the derived 3/8
  boundary + one IR coupling vs observed 0.23122: gap ~115x the honest
  threshold band. THE DERIVED SM-CONTENT RUNNING IS QUANTITATIVELY
  FALSIFIED AS-IS; the ledger is the standing falsification filter for
  any proposed observerse intermediate content (rerun it the moment a
  content row is source-defined). Even closure would be GUT-generic
- Phase452: `scalarChannelSpectroscopyProbePassed=True`,
  `scalarChannelVerdict=scalar-channel-gapped-measured` - THE
  PROGRAM'S FIRST MEASURED POLE. CANONICAL COMMITTED NUMBERS
  (record reconciliation 2026-07-10; the committed default-budget
  16000/10000-trajectory output at f7249d8e is the sole canonical
  record; earlier journal/restart values 2.4352/2.4547 came from a
  never-committed reduced-budget env-override run and are
  superseded): identity a*m_0++ = 2.7132+-0.1846 (plateauChi2Dof
  null, window {0} - inconclusive-by-construction as a measurement);
  sd2 combined 2.5260+-0.0712 (mO1 2.5553+-0.0725, mO2
  2.4986+-0.0701, interpolator cross-check 0.56 sigma; zero modes
  252 = dim ker d exactly), gated by the exact block-spectrum
  free-field control (exact free gaps 2.5509/2.5320/2.3570).
  Cross-action ratio sd2/identity = 0.931+-0.069 (CROSS-ACTION
  deliverable class, never folded into the spectrum-ratio table;
  0.9 sigma from the free ratio 0.9926 - FREE-FIELD-COMPATIBLE label
  binding). The review board's convex/gapped picture is CONFIRMED BY
  MEASUREMENT. Lattice units only, never m_H (label caveat binding);
  T=3 single-cosh-point limitation recorded; formal attestation
  phase queued as A-team's first registry number

Interpretation: the control-branch program has traced every
electroweak-shaped gap to its physical root. The sector skeleton is exact
(Phase396: all 68 triplets split 1 neutral + 1 charged pair). Phase397's
u(1) extension then found: (1) the Z-like channel is SOURCELESS (su(2)-
neutral source fraction <= 0.0023; the coupled residual is purely
charged-sector); (2) the fermion-bilinear neutral mixing element VANISHES
identically (trace selection rule) - photon/Z mixing requires a
symmetry-breaking scalar/VEV sector, welding the Phase256 photon-projection
gap to the Phase201 Higgs scalar-source gap, exactly as in the SM where
W3-B mixing enters through the Higgs; (3) the photon/Z separation is a
one-parameter family blocked by the named gap {hypercharge lineage,
coupling-ratio lineage, scalar sector}. Internal toy-branch construction
has reached its honest limit: the remaining requirements (scalar sector,
hypercharge/coupling lineage, 4D observed vacuum, scale/pole/GeV) all live
in the unsolved GU derivation chain (VO-6/VO-7 physically completed) or new
theorem-level sources.

### 2026-07-04 Review Board Outcome (post-Phase448) — READ THIS FIRST

A three-specialist review board (QFT, lattice, mathematician) plus a
hostile adversarial referee (who reproduced every checkable claim
against the committed data and the live platform) adjudicated the
accumulated blockers. Full record: the 2026-07-04 review-board entry in
docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md. Outcomes that BIND future
work:

1. THE POSITIVE-SUBSPACE SADDLE V_eff IS RETIRED (unconditional): not
   stationary in omega; discontinuous at negative-mode crossings;
   loop hierarchy inverted (median |V_1loop|/S_B = 127-205 from the
   committed phase447 data); unbounded below at t->0 (K log t, K>0,
   and K=0 rays are impossible). Do not build further phases on it.
2. CORRECTED LEDGER: what survives of 443/448 is (a) TREE-LEVEL
   NO-SCALE (robust: S_B = a2 t^2 + a4 t^4, a2,a4 > 0 on invariant
   rays) and (b) "NO PERTURBATIVE COLEMAN-WEINBERG SCALE". The
   stronger phrasing "internal route provably empty" is REJECTED.
   New exact fact: a3 = 0 exactly on invariant rays by the
   omega -> -omega reflection symmetry (a genuine Z2; a3 is REAL
   ~1e-4 off the invariant sector).
3. THE NON-PERTURBATIVE QUESTION IS OPEN (first internal route to
   REOPEN): the true effective potential is CONVEX - "interior
   minimum" was the wrong signature; SSB = flat Maxwell bottom /
   degenerate +-v minima, diagnosed by finite-volume constraint-EP +
   Binder crossing. The 30-235 transverse negative modes the retired
   convention discarded are exactly where a dynamical scale could
   hide. S_B >= 0 real => e^{-S_B} has NO SIGN PROBLEM.
4. NAMED NEXT EXPERIMENTS (adversarially filtered, ranked):
   PHASE449 = variational Gaussian (CJT-Hartree) EP - a rigorous
   Feynman-Jensen upper bound, closed-form Wick contractions from the
   omega-quartic structure, ~free using the block spectra; a null is
   strong evidence against a scale, a positive is mean-field-only.
   PHASE450 = constraint-EP HMC with FOUR BINDING CONDITIONS:
   (i) gauge-invariant invariant-ray collective coordinate (random
   rays leak pure-gauge motion through ker d), (ii) THETA-HAAR measure
   (theta enters only via exp(ad_theta) in SO(3) per vertex; the R^n
   theta-integral diverges - verified), (iii) demonstrated
   ergodicity/critical-slowing control, (iv) the theta=0 slice is a
   sampler demo, never a physics verdict. Feasibility is PROVEN:
   working HMC harness on the real action, 8.5-8.8 ms/trajectory,
   acceptance ~0.996+, exact gates green (referee-reproduced).
   THEN Binder/correlation-length across >= 4 torus sizes (n=3,4
   insufficient). FRG-on-invariant-sector is REFUTED - do not build.
5. Convention review queue DISCHARGED with outcomes (445/446 moot;
   447 object retired - its non-perturbativity verdict stands as the
   correct symptom; 448 invariant rays sound). New pending
   conventions to pin in the phase449/450 designs: theta-Haar,
   collective coordinate, Faddeev-Popov normalization caveat for
   cross-member spectral comparisons.

### 2026-07-05 Theory-Completion Task Force — THE PARALLEL WORK PLAN (BINDING)

Three specialist teams (Breaking / Scale / Observables) with cross-team
bulletins, adjudicated by a hostile adversarial referee who reran every
checkable number (full record: the 2026-07-05 journal entry). Ruling:
PARTIALLY ACTIONABLE — four certified steps, two theorem/source-level
blockers no computation can remove. IMPLEMENT THE CERTIFIED STEPS AS
PARALLEL WORKSTREAMS (they are independent):

  WS1 [hours; no dependencies]: TWO-LOOP UNIFICATION LEDGER phase —
  predict sin^2(m_Z) from the derived 3/8 boundary at the a1=a2
  crossing + exactly ONE low-energy coupling (NEVER import sin^2(m_Z)
  — the referee ruled the one-loop "gap closure" circular; the honest
  one-loop prediction is 0.208, the textbook GUT miss, making the
  two-loop test genuinely falsifiable). Fail-closed; report the
  tension; success is GUT-generic until observerse intermediate
  content is defined. Corrected figure: exact hierarchy condition
  b*alpha_GUT = 2pi/ln(M_Pl/m_W) = 0.159.
  WS2 [~0.4-4 h runs; no dependencies]: 0++ MASS-GAP SPECTROSCOPY
  phase — tr(F^2) + Mode-B <Tr Upsilon†Upsilon> interpolators,
  zero-momentum time-slice correlators on lattice-canonical tori
  (n=3,4), COSH-CORRECTED effective masses ONLY, exact block-spectrum
  free-field gate (flat to 1e-5), theta-Haar measure + phase450
  binding conditions. Measurement-tests the review board's
  convex/gapped picture.
  WS3 [1-2 dev-days + short runs; no dependencies]: UPSILON-PORTAL
  CONDENSATION PROBE (referee-rescoped from the bi-doublet triple) —
  the bare Mexican-hat variant is a TEMPLATE CONTROL ONLY (the
  postulated-doublet conditional is tautological: rho=1 and
  m_W/m_Z=cos theta follow by construction and carry zero GU content;
  the "vs sqrt(5/8)" comparison also imports the coupling ratio =
  the phase397 gap). The GU-bearing question, SANCTIONED: does
  xi Tr(M† Upsilon M) dynamically drive the DOUBLET channel to a
  FINITE VEV? PRE-REGISTERED fail-closed gates: must beat the
  phase410 runaway mode (finite VEV, not unbounded descent) AND the
  phase438 singlet-wins mode (doublet before/instead of singlet).
  The corpus predicts failure - that is why it is worth measuring.
  su(2)-HONEST SCOPE BINDING: no hypercharge => rho/Weinberg not
  measurable; gates are O_H mass gap + CUSTODIAL TRIPLET DEGENERACY
  (necessary-not-sufficient); an O_H gap proves a singlet state,
  never "the Higgs".
  WS4 [weeks, embarrassingly parallel]: BINDER PHASE MAP, SPLIT BY
  ORDER PARAMETER (referee-binding): (i) the ADJOINT/Upsilon-condensate
  column has a GENUINE transition - Binder-valid SSB diagnostic
  (pure gauge alone has NO bulk transition; its deliverable is the
  WS2 mass gap, not Binder); (ii) any fundamental-M scan is an
  Osterwalder-Seiler CROSSOVER - Binder locates the region only, the
  deliverable is the spectrum (degeneracy/mass ratios), never a
  transition point. >= 4 torus volumes.
  WS5 [weeks; heavy, higher risk, run behind WS1-4]: CL(7,7)/128-SPINOR
  SHIAB BUILD — the only structural attack on the scale wall; risk of
  reproducing necessary-not-sufficient at higher cost.

  BLOCKED (do not attempt without new source input): m_W/m_Z vs
  sqrt(5/8), photon/Z separation, rho measurement — all require the
  phase397 {hypercharge, coupling-ratio} lineage; the 144-block fermion
  condensate requires an undefined GU fermionic action. The single
  mandatory dimensionful anchor (M_Planck recommended over v=246,
  which is circular AND gauge-variant per Elitzur) remains a
  source-level decision. ELITZUR REFRAME (recorded): the program's
  VEV-selection no-gos were true classical/mean-field facts irrelevant
  to the physical question; physical content = gauge-invariant
  composite poles; the 408-425 operator-content no-gos stand.

  Phase449 (variational Gaussian bound) and phase450 (CEP-HMC, four
  binding conditions) remain in flight and are complementary to
  WS1-WS4 (WS2/WS3 share the HMC infrastructure with phase450).

### PROGRAM HALT NOTE (2026-07-05) - SUPERSEDED by the 2026-07-10 three-team program

The 2026-07-05 halt was lifted at user direction on 2026-07-09/10. The
BINDING work plan is now docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md
(three adversarially-approved team elimination programs; 22-item
kill-ordered queue in five waves; cross-team dependency spine; nine
recorded unresolved objections). Wave 0 (ops checkpoint: env-knob
fingerprint fix proven by injection test; phase-number registry
docs/Phases/PHASE_NUMBER_REGISTRY.md with B=453-458, A=459-464,
C=465-470; phase452 record reconciliation - see the corrected Phase452
gate bullet; WS3 verdict-emission HOLD until team C's completion
contract schema commits) is DONE. Wave-0 item 0.3 remains OPEN: the
physicist sign-off memo on the 445-452 conventions (invariant rays,
positive-mode IR rule, theta-Haar, saddle backgrounds) - phases may
pre-register against pending status citing physicistReviewPending.
WAVE-1 COMPLETE (2026-07-12, checkpoint e42fec44; see the journal's
blocker assessment): PHASE453 T1 null claimed + frontier upgraded;
PHASE454 origin vacuum certified clean in all 48 momentum sectors
through cubic order (2016 transverse negatives at non-stationary ray
points, sector-resolved for the closure theorem); PHASE459 phase452
record machine-attested 31/31; PHASE460 no derivable anchor under the
prescribed reading (unique breaking relation = the dimensionally-
invalid literal; squared reading blocked by a 31-item prose-only
extraction set - THE concrete B1 obstacle; CC re-gradings break
pinned corpus relations); PHASE461 zero import-clean transmutation
anchors (referee ~460x kill reconstructed at 451.105; nothing
forwarded to A3). B2 is narrowed to four named candidates: n=4
volume, exact fermionic -log det, WS3 portal (held on C-CONTRACT
schema), WS4 Binder. B3 untouched - team C's ladder is unbuilt.
WAVE-2 FIRST RESULTS (2026-07-12, see the journal entry of the same
date): PHASE464 ADJUDICATED B1 as blocked-upstream-ambiguous (the
pre-committed no-claim sentence is the citable record; machine
route proven dead by phase462 pinning-insufficient 30/31; transport
killed at the audited menu by phase463 - the strongest C3);
PHASE465 closed the anomaly route (positive-dimensional cone, SM not
an isolated selection); PHASE466 committed the completion-contract
schema (C's half of the WS3 hold LIFTED; O8 now a schema theorem);
PHASE455 landed T3 CONVENTION-FRAGILE (the below-origin-well verdict
flips on the zero-mode convention; flip axis + the S_B workbench-model
choice routed to O4 as o4QueueItems; ledger limb L5 stays open).
THE O4 PHYSICIST ENGAGEMENT IS THE PROGRAM-WIDE CRITICAL PATH: it
decides the 455 flip, the WS3 M-probe (hold's second half), the
445-455 convention families, and optionally the 462 quote-pinning
reopening. Dossier: docs/Phases/Adjudication/O4_CONVENTIONS_REGISTER.md.
ALSO DONE 2026-07-12 (f02863be): phase456 pack committed behind the
proven hash-refuse-to-run gate (A4: PARITY BANNED on the Kuhn torus -
no realized improper element; exact-rational projectors for the
identity 2x2 GEVP + A2 channel; per-site correlator storage flag
MANDATORY; A5 Gaussian-domination NOT provable at Stage A, three
obstructions recorded, beta ladders live -> feeds 458 G1) and
phase457 Stage-A (all seven portal cells certified exactly, closed
form g_crit = sqrt(2*lambda); verdict firewall machine-checked CLOSED
pending the verified Phase480 O4 M-probe derivative; Arm Q gated off). The
new-directory C queue and C-LIFT are complete. Phase456 production completed
under explicit written risk renewal. Phase458 must not execute until all G1-G6
inputs exist; production supplies G2 but explicitly does not supply G3.

WAVE-2 LABELED AMENDMENT A4 allocated phases 477-483 as a fail-closed
blocker-resolution extension in strict execution-priority order: 477 O4
adjudication infrastructure; 478 Phase458 gate-specification closure; 479
Phase457 post-ruling readiness; 480 genuine external physicist-adjudication
intake; 481 prospective Phase456 repair pre-registration; 482 A5 theorem
scout; 483 source-defined reopening intake. Their initial skeletons performed
zero physics computation and could not satisfy a scientific gate. Phase480 is now
an implemented, exact-path, Ed25519-verified intake but stays human-input gated
at `awaiting-external-physicist-ruling` because its memo path is absent and its
pre-pinned reviewer registry is empty; Phase481 cannot sample or mutate/reinterpret Phase456;
Phase482 has now executed under Amendment A9 and cannot claim a theorem from
its obstruction-only result; Phase483 cannot invent
source fields. Phase477 is now substantively complete at exact coverage 31/31,
13 memo ruling IDs, and 94/94 synthetic dependency edges, while remaining
human-ruling pending. Phase478 is substantively complete but Phase458 remains
blocked on G3/G4/G5. Phase479 is substantively complete: the exact Phase480
derivative, Arm-Q provenance, and Team-C deadline interfaces are frozen, while
the Phase457 hold remains closed and Phase471's firewall remains unchanged.
Phase480 is substantively implemented: strict JSON, exact 13-ruling semantics,
ancestor-commit/current-byte bindings, exact reviewed-artifact sets, JCS payload
hashing, a pre-pinned reviewer registry, and real Ed25519 verification are
required as one conjunction. No genuine external memo or reviewer key is
present, so no ruling is consumed, normalized rulings are empty, and the hold
remains closed.
At that checkpoint registry 477-507 was allocated; the later A14-A18 sections
supersede its former 508+ frontier, and the current registry frontier is 523+.

WAVE-2 C-LINEAGE MACHINE RESULTS (2026-07-14, incremental validation green:
82 ran / 260 skipped, integrity verified): PHASE467 reconstructs the exact
four-dimensional `u(1)+su(2)_R` commutant in both the compact arithmetic
and a direct `so(6,4)` field-of-definition arm. The 13-versus-25
stabilizer discriminator passes, but 176 committed-menu rays have the
generic dimension-13 stabilizer, including non-Y rays, so the terminal is
`all-symmetric-non-y-route-closed`; C-PERMANENCE P2 is a scoped negative
and C-LIFT must carry the surviving generic rays. PHASE468 hash-pins four
Phase404/407-induced content rows, reproduces the Phase451 two-loop
witness, and finds every row outside the honest threshold band; terminal
`full-menu-out-of-band-scoped-exhaustion`, supplying C-PERMANENCE P4 only
at the declared conditional scalar-proxy/full-interval scope. Neither
phase supplies a coupling normalization, scale, pole, or prediction;
`promotedPhysicalMassClaimCount=0`. Phase458 remains executable only when its
G1-G6 inputs exist. The A4 phases 477-483 are ordered infrastructure, intake,
pre-registration, and scout work; their skeletons do not bypass that gate.
Phase456 production completed under explicit written risk renewal and supplies
G2 only; its invalid-analysis terminal cannot supply G3. C-LIFT 469 and
C-PERMANENCE 470 are complete.

WAVE-2 PLAN ADOPTED (2026-07-12, binding):
docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md is the current execution
queue - STEP 0 batched wiring checkpoint (skeletons 455-458/462-466/
471, one ~6 h cascade, three injection tests at the commit gate),
then the 15-item kill-ordered queue with immediate parallel launches
(462 Stage-P + 463 T4; 455 exact fermionic backreaction; 466+465 C
batch; O4 register from day 1 - the program-wide critical path).
Registry extended B:471-476. Phase456 requirements surviving session
turnover: the hash-refuse-to-run pre-registration gate and the
per-site (un-slice-summed) correlator storage flag are MANDATORY pack
items. The WS3 hold lifts only on the conjunction (466 schema green)
AND (O4 M-probe ruling). LEGACY QUEUE (superseded by the plan above,
kept for lineage): (1) PHASE453 - DONE
(see above); (2) Wave-1
parallel cheap kills: A1 units-equivariance kernel, A2 part-12d
reading menu, C-KERNEL v3, C-STABILIZER v3, C-CONTRACT v3 (schema
BEFORE any WS3 verdict), B beyond-ray Arms A/B, B exact fermionic
-log det D(t), C-CLOSURE items; (3) Wave-2 gating analytics (A5
Gaussian-domination pre-theorem CAN CANCEL the beta ladders; A3
transport structure theorems; A4 automorphism/irrep enumeration EX
ANTE of any n=4 launch; C-LIFT Stages -1..2); (4) Wave-3 consolidated
sampling (THE one n=4 launch hosting all teams' pre-registered
columns; WS3 portal probe under the emission hold rules); (5) Wave-4
weeks-scale closure (WS4 Binder columns - the only CUDA trigger; WS5
Cl(7,7) build); then the terminal adjudications (A6, C-PERMANENCE, B
closure theorem-of-record). Checkpoint-cadence literature monitoring
continues (last sweep 2026-07-04, clean). Phase numbers: registry
only - next B number 453, next A number 459, next C number 465.

### PERFORMANCE EXPLORATION OUTCOME (2026-07-09, user-directed, design-only)

At user direction two parallel read-only investigations assessed the
two named performance levers. Both memos are recorded here as the
binding design record; NOTHING was implemented (program halt stands).

RULING: INCREMENTAL VALIDATION FIRST, CUDA NOT NOW.

STATUS UPDATE (2026-07-10): item (1) is IMPLEMENTED AND ACCEPTED.
Tooling: scripts/incremental/ (Node stdlib, 47 unit tests) + wrapper
scripts/run_boson_phases_incremental.sh (--incremental / --full; the
original generator script is untouched and remains the default
semantics). Acceptance: no-change pass 159.8 s wall (77 ran / 248
skipped) ending boson-claim-integrity-verified with only-volatile
output diffs; staleness injection on leaf phase181 flips run/skip
correctly; downstream invalidation proven organically (six scanner
edits -> exactly their consumer cascade re-ran). ~160x on the typical
checkpoint. TWO BINDING LESSONS: (a) Directory.Build.props is a real
undeclared input of every phase (dotnet run --no-build re-evaluates
MSBuild) - it is a global fingerprint component; (b) ANY new repo file
whose PATH or text contains scanner topic keywords will flip the
whole-repo scanners to review-required and cascade blocked verdicts
through the precursor gates (the first acceptance attempt failed
exactly this way: the driver's own skip reports + manifest embed phase
project paths; the phase278/phase279 audit topic terms matched inside
those path strings; 7.25 h cascade, integrity red; a FULL pass would
have failed the same way). scripts/incremental/ and the manifest are now excluded in the
six scripts-root scanners (278/279/281/289/295/296); apply the same
exclusion-list discipline to any future non-phase tooling that writes
inside the repo. Promotion-relevant claims still require --full.

(1) FAIL-CLOSED CONTENT-HASH INCREMENTAL VALIDATION - the ~100x lever.
Measured facts (prototype DAG extractor + recorded runtimeSeconds):
the generator re-runs 317 phases; the dependency graph is
NEAR-TOTALLY-ORDERED (279 of 317 phases have ~272 downstream
dependents; mega-hubs phase213 fan-out 163, phase201 85, phase256 74),
so subtree pruning for mid-graph edits buys nothing - the win is
content-hash SKIPPING of unchanged phases on the common
append-one-phase checkpoint. Cost is extreme-tail concentrated: 8
numeric probes (450: 2.53 h, 452: 1.29 h, 443, 447, 449, 445, 446,
448) hold 6.28 h of the full pass, which NOW MEASURES ~6.5-7 h (the
3-5.5 h figure elsewhere in this document predates the HMC-era
phases). A typical append-one-audit checkpoint under the scheme: ~2-4
min (~99% saved); an append-one-HMC-probe checkpoint: its own runtime
+ ~3 min (~55-60% saved); any src/ engine edit correctly invalidates
the 39 src-linked phases -> effectively full rerun (fail-closed).
Design (binding): committed manifest scripts/boson_incremental_manifest.json
mapping phase -> {schemaVersion, inputFingerprint, outputs canonical
hashes}; skip IFF exact fingerprint + output-hash match, ANY doubt ->
run. Output hashing canonicalizes JSON minus a declared volatile-key
list (generatedAt, runtimeSeconds, timing fields) - files on disk are
never rewritten. The 7 whole-repo text scanners (204/205/253/279/281/
295/296) and the tail gates (101/202/212/216/217/158/219 + integrity
script + 151/156 which read the generator script itself) ALWAYS run.
CRITICAL SOUNDNESS FINDINGS: (a) the claim-integrity verifier asserts
VALUES and external checksums but does not recompute output hashes -
it CANNOT detect a silently-stale-but-valid output, so the manifest is
the load-bearing freshness guarantee; (b) static const-path parsing is
UNSOUND for fingerprinting - 103 phases read undeclared inputs (legacy
dir-root JSONs, phase12 background_family tree reads, phase142 csproj
enumeration, 151/156 reading the generator script) - so per-phase real
read-sets must be captured ONCE via an instrumented run (strace -f -e
openat, filtered to repo paths) and stored in the manifest,
re-instrumented when the phase's code changes. Migration: land
extractor + DAG-generated phase registry (no behavior change); seed
the manifest from one full pass; gate skipping behind --incremental
with --full retained for promotion-relevant claims and periodic CI
re-validation. Prototype extractor + machine report were produced in
session scratch (disposable; the design above is self-contained).

(2) CUDA - NOT NOW (two capping facts). (a) The physics is entirely
fp64 and the resident RTX 4080 SUPER (Ada, CC 8.9) runs fp64 at 1/64
fp32 rate (~0.5 TFLOP/s) - the 16-core CPU already delivers ~1 TFLOP/s
aggregate fp64, so honest ceilings are only ~8-20x on HMC phases (via
batching the 16 umbrella windows into one kernel), ~3-8x on the 48M-eval
stencil scans, and NEGATIVE on eigensolves (12288-dim syevd slower than
16-core LAPACK). (b) The shipped native library is NOT a CUDA build:
ldd shows native/build/libgu_cuda_core.so is not linked against
libcudart (CPU-fallback #else path) - the CPU-vs-CUDA parity tests
pass trivially because both sides are the same CPU code - and the
device kernels that exist implement the TOY CONTROL branch (identity
Shiab), not the Einsteinian operator. A real port = writing the
production operator + per-face 3x3 matrix-exp + MatrixExpFrechet as
new fp64 device code (6-12 dev-days), with the risk concentrated
exactly where silent numerical bias could contaminate physics (the
program was already bitten by the 2026-06-12 silent GPU fallback).
REVISIT CUDA IFF: WS4 Binder commits to weeks of continuous HMC (then
fund only Inc 0 real-CUDA-build + Inc 1 batched forward-action kernel,
~3-5 dev-days, for the stencil-scan 3-8x), or hardware moves to a
data-center card with full-rate fp64 (then the full port is a genuine
20-40x and eigensolves move too).

### Most Recent Implemented Work

The latest work (2026-07-04, after both phase444 unlock projects were
completed at user direction - adjoint/joint-gradient path commit
7a7e397d, gradient 2.2 ms / Hv 4.7 ms vs the old ~60 s/Hv;
lattice-canonical conventions commit 82d43559, translation covariance
1.18e-4 -> 7.8e-15, opt-in, open meshes SHA256-byte-identical) is
PHASE448, the torus mode-volume saturation probe - ANSWERING THE
QUESTION PHASE444 LEFT UNDETERMINED. On translation-invariant rays
(the constant-field CW analogue; per-type coefficients shared across
volumes) exact covariance makes the joint (omega,theta) Hessian
BLOCK-CIRCULANT over (Z_n)^4, so its ENTIRE spectrum comes from 48
orbit-representative Hessian-vector products per point, DFT-assembled
into Hermitian 48x48 momentum blocks ({k,-k} pairs via the real
symmetric embedding). The 3888-dim (n=3) and 12288-dim (n=4) joint
Hessians were eigensolved EXACTLY - covariance 9.5e-15/1.4e-14,
block-reconstruction 2.9e-11, theta equivariance battery, seed-stable
- in 12.7 minutes total. VERDICT:
modeVolumeVerdict=no-saturation-persists-across-mode-volumes; every
Einsteinian member remains trivial-origin (V_eff monotone rising) at
5x and 16x the minimal mode count. PHASE443'S MODE-VOLUME LEVER IS
DECIDED NEGATIVE: the one-loop no-saturation verdict is not a
small-mesh artifact. BUILD LESSON (fail-closed smoke): the stored
index-ordered edge direction does not commute with translation - the
covariance battery caught the missing orientation signs at 1.6e-2;
the lattice-gauge fix (all orbit machinery in the base->tip gauge)
collapsed it to 9.5e-15. Recorded conventions (invariant rays,
positive-mode IR rule, saddle backgrounds) carry
physicistReviewPending. Remaining frontier: beyond-one-loop at volume
(cheap with the block machinery IF a physicist-sanctioned convention
exists), the accumulated physicist review queue (445/446/447/448
conventions), or a source anchor from checkpoint-cadence monitoring.
Study: phase448_torus_mode_volume_saturation_probe_001
(IMPLEMENTATION_P448.md). Runtime ~13 min.

Before that, the work (2026-07-04, after the post-Phase446 scouting) is
PHASE447, the two-loop saturation probe - the last no-platform lever,
decided honestly: THE TWO-LOOP LEVER IS NON-PERTURBATIVE AT THE
MINIMAL-MESH SCOPE. The genuine two-loop vacuum terms (figure-eight
over all positive mode pairs + soft-mode-truncated sunset with K-sweep,
full-deterministic anchors, and a recorded propagator soft-floor
convention with floor sweep; directional FD stencils at h=0.1 on the
joint Hessian eigenvectors, exact on the omega sector because S_B is
EXACTLY QUARTIC in omega - the structure result that also resolves
Phase442's "degree > 2" as theta-transcendentality) pass every
numerical battery (theta 1.4e-9; identity exact-quartic anchor 1.3e-7;
Einsteinian Richardson 7.6e-4; offset immunity 2.9e-7) and then fail
every PHYSICS admissibility axis: max |V_2loop|/|V_1loop| = 1.6e3
(median 155 - the loop expansion is out of control), the
classification flips across the soft-floor sweep, the seeds, and the
propagator-convention arm, and even the identity control acquires a
spurious "minimum" (not a theorem violation at two loops - evidence of
the same breakdown). Pre-registered taxonomy verdict:
resolutionKind=non-perturbative-or-convention-bound,
twoLoopVerdictAdmissible=false, twoLoopCandidate=false. THE INTERNAL
NO-PLATFORM PROGRAM AT THE MINIMAL MESH IS EXHAUSTED THROUGH TWO
LOOPS (443/446/447). BUILD LESSONS (fail-closed smoke runs earned
their keep): the scouted stencil step h=5e-3 was PROVEN
noise-dominated by the offset-immunity battery (impossible to fail
with clean stencils; failed at 135%) and fixed by exploiting the
omega-quartic exactness at large h; the IR-domination forced the
recorded soft-floor convention + sweep and the perturbativity gate.
Deciding the scale question beyond this scope requires changing the
IR spectrum (the two Phase444 unlock projects; the committed scoping
memo recommends the adjoint/joint-gradient path first) or a source
anchor. Saddle/propagator convention physicistReviewPending. Runtime
42 min. Study: phase447_two_loop_saturation_probe_001
(IMPLEMENTATION_P447.md).

Before that, the work (2026-07-03, after Phase445) was PHASE446, the RG
scheme-dependence resolution probe - closing the arc's most tantalizing
lead with a decisive, pre-registered diagnosis: THE PHASE445 INTERIOR
MINIMA ARE A FIT-NORMALIZATION ARTIFACT. The mechanism (found by
prototype replay of the committed Phase443 rays BEFORE new compute, and
independently confirmed by a reviewer pass over the committed arrays):
the {t^4} one-loop fit basis leaks the deep-negative t-independent
one-loop constant (~-160; pure vacuum normalization that provably
cannot move the true V_eff minimum) into the fitted coefficient,
forcing V_RG -> 0 at t -> 0 and manufacturing an interior dip for any
deeply negative, slowly rising curve; the positive-one-loop identity
control is structurally blind to this artifact class. Phase446
recomputed the rays (verbatim hardened machinery, same rays/RngSeed,
48-point grid whose strided subset EXACTLY matches the Phase445 grid)
and decided it with four arms + a synthetic control: replication exact;
constant subtraction kills every minimum at every window on both grids
(offset arm; the raw argmin is provably shift-invariant, asserted);
the 4-basis x windows x grids x normalizations menu is irreducibly
scheme-dependent with NO surviving member (one enriched scheme even
hands the identity control a manufactured minimum); a provably-monotone
synthetic control (positive tree + [-150 + 40 ln t] one-loop) receives
a manufactured minimum from the verbatim scheme at every window (and
the direct arm must stay clean on it - a fail-closed blocker); the
constant-immune direct measurement (L = t d/dt annihilates constants
exactly; jump-aware midpoint log-derivatives, 30-40 usable intervals,
L-image basis {1, 2t^2, 3t^3, 4t^4, 4t^4 ln t + t^4}, offset-immunity
exact, centered-R^2 gated) supports NO resummation minimum (sd2
cL=-0.79 nonpositive; asd2 cL=+4.45 with implied t* far outside the
grid). Theta gate: all points pass relative <= 1e-8 or the 1e-10
absolute-gradient floor (2 smallest-t points via the floor - the
Phase443 dimensional-gate lesson recurring at tiny t). Verdict:
resolutionKind=fit-normalization-artifact,
einsteinianRgSaturationObserved=false,
candidateSurvivesSchemeControl=false - THE RG-IMPROVED POTENTIAL-FIT
ROUTE ON THE MINIMAL MESH IS CLOSED. Method lessons encoded: gate on
CENTERED R^2 (through-origin uncentered R^2 is vacuous); a control must
be able to EXHIBIT the artifact class it guards; windowed-fit
classifiers must be constant-shift invariant before their minima are
believed. Frontier (sharpened): the two Phase444 unlock projects,
genuine beyond-one-loop structure, or a source anchor. Runtime ~15 min.
Study: phase446_rg_scheme_dependence_resolution_probe_001
(IMPLEMENTATION_P446.md).

POST-PHASE446 SCOUTING (2026-07-03, same day): three parallel
investigations decided the next moves. (1) Literature sweep: NOTHING
new merits an audit (geometricunity.org unchanged; no new
Zenodo/arXiv records; the tracked GU-RVG record 21117379 churned to a
THIRD checksum in two weeks - drift-logged in its reference note,
posture unchanged, Phase427 verdict pinned to the audited checksum).
(2) The two Phase444 unlock projects were engineering-scoped
(studies/phase444_.../UNLOCK_SCOPING.md): fund the adjoint/joint-
gradient path FIRST (S-M, low-medium risk, fully additive, clears the
measured ~60s/Hv SLQ blocker via three ~30-50-line transposes of
existing FD-verified forward linearizations); the geometry-conventions
project is M-L with an UNPROVEN mathematical core - spike the
Bloch-twist (SO-valued cocycle) alternative before committing.
(3) NAMED NEXT: PHASE447 = genuine TWO-LOOP saturation probe, scouted
FEASIBLE with existing machinery (design note:
studies/phase446_.../PHASE447_TWO_LOOP_DESIGN.md). Key structure
result: S_B is an EXACT QUARTIC in omega but TRANSCENDENTAL in theta
(Ad = exp(ad_theta)) - resolving Phase442's "degree > 2" as
theta-transcendentality and handing the probe an EXACT-arithmetic
identity control. Route: positive-subspace propagator (recorded
convention, physicistReviewPending - the negative-mode/saddle question
is the flag), exact figure-eight (~5 s/point) + soft-mode-truncated
sunset (~20 s/point, convergence-in-K battery + stochastic cross-check
+ full deterministic anchor points); ~1.5-3 h total at phase443 scope.

Before that, the work (2026-07-03, after Phase444) was PHASE445, the
RG-improved joint potential probe - the no-platform alternative,
delivering the arc's most tantalizing result: THE FIRST INTERIOR FINITE
MINIMA EVER (Einsteinian members, t* ~ 1.5-2.25, windows 3/5; measured
workbench beta strongly positive [8,2500] vs identity negative; identity
develops NO spurious minimum under the same prescription; theta gate
1.4e-9) - recorded honestly as a SCHEME-DEPENDENT CANDIDATE because the
verdict flips at window 7 (poor fits there, R^2 0.31):
verdictSchemeStable=false, einsteinianRgSaturationObserved=false.
NAMED NEXT (phase446, no platform work): resolve the scheme dependence
- regime-aware/adaptive windows, DIRECT beta measurement from the
running Hessian rather than potential fits, denser t-grids around the
candidate minima; if the minima survive scheme control the program has
its first workbench-relative dynamical-scale candidate. RG prescription
carries physicistReviewPending. Study:
phase445_rg_improved_joint_potential_probe_001 (IMPLEMENTATION_P445.md).

Before that, the work (2026-07-03, after Phase443) was PHASE444, the
mode-volume-scaled saturation probe - honestly TOOLING-BLOCKED: exact
momentum block-diagonalization on the M1b torus is defeated by
Gu.Geometry's global-index orientation conventions (three measured
levels, definitive signed-S_B covariance test; even pure curvature
fails at 2.5e-4), and SLQ is infeasible without a platform adjoint
(~60s per Hessian-vector product). Additive periodic-mesh conventions
were authorized and kept (VertexFaceRule.IncidentAverage +
latticePeriod minimal-image bivectors; open-mesh byte-identical,
hard-gated; 8 new tests incl. a documented-limitation test).
NAMED UNLOCK PROJECTS - BOTH COMPLETED 2026-07-04 (user-directed;
scoping memo at
studies/phase444_mode_volume_scaled_saturation_probe_001/UNLOCK_SCOPING.md;
parallel worktree builds, serial integration, full-suite validation
between merges): (i) lattice-canonical conventions COMMIT 82d43559 -
the spike PROVED the math core (chain order of minimal-image lattice
displacements; Kuhn subsimplices are chains under the componentwise
partial order => translation-covariant by construction); OPT-IN via
CreateUniform4DPeriodic(n, latticeCanonical: true) (default false so
this study's committed non-covariance assertions keep reproducing -
verified by rerun, Passed=True, verdict byte-stable); covariance
1.18e-4 -> 7.8e-15, open meshes SHA256-byte-identical, 14 gating
tests. (ii) adjoint/joint-gradient path COMMIT 7a7e397d - three
reverse-mode transposes + ComputeJointGradient + JointHessianVector-
Product; gradient 2.2 ms / Hv 4.7 ms vs the measured ~60 s/Hv; adjoint
dot-product identities 1e-10. THE MODE-VOLUME LEVER IS UNBLOCKED:
exact momentum block-diagonalization AND feasible SLQ are both
available; a phase448 mode-volume-scaled saturation probe on the
latticeCanonical torus is the NAMED NEXT physics study (torus studies
MUST opt in with latticeCanonical: true). Original project statements:
(i) lattice-canonical orientation conventions through
Gu.Geometry => exact torus block-diagonalization; (ii) adjoint/joint-
gradient platform path => feasible SLQ. No-platform-work alternatives:
two-loop/RG on the minimal mesh; richer Phi menus.
physicistReviewPending=true on the minimal-image contraction semantics.
Study: phase444_mode_volume_scaled_saturation_probe_001
(IMPLEMENTATION_P444.md).

Before that, the work (2026-07-03, after Phase442) was PHASE443, the joint
effective-potential saturation probe: variational theta*(omega) solved
by scale-aware Newton with continuation (relative stationarity 3.4e-9
everywhere; the phase fail-closed itself through THREE runs until the
solver was provably converged - four hardening rounds: analytic-Jacobian
Newton, warm-start continuation + backward first-point re-solve,
dimensionally-correct relative gates, scale-aware LM damping +
multi-start rescue). VERDICT: no log-saturation at one loop on the
minimal 4D workbench - identity control anchors the theorems, every
Einsteinian member trivial-origin. NECESSARY (Phase442) satisfied;
SUFFICIENCY fails on the 16-vertex mesh at one loop; the Phase437
mode-volume lesson names the probable reason. NAMED NEXT (phase444):
the same probe on CreateUniform4D(2) - requires an ENGINEERING item
first (joint-Hessian assembly smarter than dense FD: the 81-vertex mesh
has joint DOF ~1500, so dense FD Hessians are ~2.3M evaluations per
point - use LinearizeTheta/Linearize Jacobian assembly + Gauss-Newton
structure instead). Alternatives: two-loop/RG improvement; richer Phi
menus. Study: phase443_joint_effective_potential_saturation_probe_001
(IMPLEMENTATION_P443.md).

Before that, the work (2026-07-03) was PHASE442 - the first physics study on
the user-approved dimension-four platform (built by a six-agent team:
M1 4D mesh, M1b periodic torus, M2 Clifford/spinor + Dirac scheme, M3a
Einsteinian Shiab family, M3b independent-theta joint-Hessian arm; all
QA-certified, physicist-signed, generator pass proven undisturbed; see
docs/Phases/FOUR_D_PLATFORM_DESIGN.md + FOUR_D_PLATFORM_PHYSICS_DECISIONS.md).
RESULT: the draft-canonical Einsteinian Shiab with independent-theta
epsilon-conjugation BREAKS the Phase436/441 exact-quadraticity of the
joint (omega,theta) Hessian - control anchored at 2.6e-15, isolation
bit-exact, all nine members lift, both vertex->face rules agree.
NECESSARY-NOT-SUFFICIENT: no scale/pole/VEV/GeV produced; nothing
promoted. THE NAMED NEXT STUDY (phase443): the joint (omega,theta)
EFFECTIVE POTENTIAL - variational eps*(omega) mode + the
Coleman-Weinberg/gap-equation saturation analysis (the Phase435/438
machinery on the lifted Hessian) - for the first time a scale question
with NO known structural obstruction. Study:
phase442_joint_omega_theta_hessian_degree_probe_001
(IMPLEMENTATION_P442.md). HAZARD NOTE: phase442 is registered in ALL
EIGHT scanners including phase253 (dimension-4 regex) and both
IMPLEMENTATION_P442.md registries.

Before that, the work (2026-07-02, thirteenth checkpoint) added Phase441, the
toy-branch family universality sweep - THE TERMINAL INTERNAL RESULT: all
seven universality verdicts hold across the full 36-member family, the
no-saturation theorem extends family-wide (verified insight: [u,u]=0
makes every member's Upsilon affine along single-direction rays and A0
cancels from the growing-mode operator), the runaway is universal, and
the draft's canonical physical Shiab is NOT realizable on the toy.
TERMINAL FRONTIER STATEMENT (family-complete, theorem-grade):
scaleGapRequiresDimFourSpinorShiabOrSourceAnchor=True. THE INTERNAL
EXPERIMENT PROGRAM IS COMPLETE. Remaining: steady-state monitoring;
incremental-validation design note; and the ONE named platform
prerequisite for further internal progress - a dimX>=4 mesh + Clifford
structure to realize the canonical Shiab (a PLATFORM ENGINEERING
investment decision for the user, not a phase). Study:
phase441_toy_branch_family_universality_sweep_001
(IMPLEMENTATION_P441.md).

Before that, the work (2026-07-02, twelfth checkpoint) added Phase440, the
coupled background-condensate fixed-point probe - the final link of the
candidate chain, decided as an HONEST NEGATIVE: the joint (t8, Sigma)
system has no interior fixed point (29 trivial / 19 runaway / 0 interior
over 48 configs). The condensate saturates the fermionic runaway but
cannot stop it (at large t8 the background gaps out the condensing IR
modes; joint-optimal Sigma -> 0; slope resumes). The boundary dW/dt8
decomposition (bosonic +, fermionic -, net -) pins the failure on the
workbench bosonic sector - the third independent convergence onto the
physical VO-6/VO-7 structure or a source anchor. THE INTERNAL DYNAMICAL
PROGRAM IS COMPLETE WITH A FULLY MAPPED FRONTIER: what remains internal
is ONLY the physical-branch candidate-family universality sweep (needs a
design exploration of the nontrivial Shiab/torsion machinery); otherwise
incremental-validation design and checkpoint-cadence monitoring. Study:
phase440_coupled_background_condensate_fixed_point_probe_001
(IMPLEMENTATION_P440.md).

Before that, the work (2026-07-02, eleventh checkpoint) added Phase439, the
gap-equation-in-lambda_8-background channel-steering probe, closing the
internal story AT CANDIDATE LEVEL: the background steers the condensate
channel (strict free-energy margins, monotone in t8), the free-per-color
gap equation decouples exactly and yields the su(2)xu(1)-ALIGNED pattern
Sigma_1=Sigma_2 != Sigma_3 (automatic from lambda_8's (x,x,-2x)
eigenvalues), and the transmutation signature survives. Chain: direction
selection (430) -> background steering (439) -> aligned dynamical masses
+ scale (438). THE ONE REMAINING LINK: t8 is still a recorded candidate
parameter. NAMED NEXT: phase440 = coupled background-condensate
fixed-point probe (joint (t8, Sigma_vec) gap system - does a nontrivial
self-consistent t8* exist?); then the physical-branch universality
sweep; incremental-validation design; monitoring. Study:
phase439_gap_equation_lambda8_background_channel_steering_probe_001
(IMPLEMENTATION_P439.md).

Before that, the work (2026-07-02, tenth checkpoint: the SCALE-GAP ATTACK WAVE,
phases 437-438 built by parallel agents) decided two of the three
computable surfaces of the pinned scale gap. Phase437: the one-loop
runaway is STRUCTURAL, not a small-2D-lattice artifact - 4D per-volume
slopes are identical to 2D (exact Phase430 anchor to 2e-5), the landscape
is log-dominated at every L, and the continuum CW regime is unreachable
on bounded-dispersion lattices. Phase438: the never-tested backreaction
mechanism class IS VIABLE - the mean-field gap equation generates the
first internal dynamical scale (critical coupling falls with volume,
1/ln L fit R^2=0.998; exponential ln Sigma* ~ -c/g2 signature) - but the
scalar singlet outcompetes the hypercharge channel at every coupling:
the gap equation supplies a SCALE, not the BREAKING DIRECTION. NAMED NEXT
EXPERIMENTS: phase439 = gap-equation-in-background probe (solve the
Phase438 gap equation inside the Phase431 lambda_8 background where T/D
degeneracy is already broken - does the background steer the condensate
channel?); phase440 = physical-branch candidate-family universality sweep
(needs a design exploration of the nontrivial Shiab/torsion machinery).
Studies: phase437_four_dimensional_transmutation_scaling_probe_001,
phase438_self_consistent_condensate_gap_equation_probe_001
(IMPLEMENTATION_P437/P438.md).

Before that, the work (2026-07-02, ninth checkpoint) added Phase436, the
exact-Hessian saturation no-go probe, completing the Coleman-Weinberg
program: the true control-branch Hessian at backgrounds t*u decomposes
EXACTLY as H(t) = A0 + t A1 + t^2 A2 (third t-difference zero; the odd
term vanishes for Cartan and is ~0.15 relative for root directions,
recorded honestly), so masses grow exactly as t^2 and NO log-saturation
can arise at one loop - THE SCALE GAP IS PINNED BEYOND THE CONTROL BRANCH
AT THEOREM GRADE, coinciding exactly with the program's oldest standing
gap (physical VO-6/VO-7 completion or a source anchor). Growing-mode
counts confirm the Phase430 slope arithmetic under the exact Hessian.
Study: studies/phase436_exact_hessian_saturation_no_go_probe_001
(IMPLEMENTATION_P436.md). THE INTERNAL EXPERIMENT PROGRAM HAS CONVERGED
onto the standing requirements; named engineering work is the
performance program; monitoring continues at checkpoint cadence.

Before that, the work (2026-07-02, eighth checkpoint) added Phase435, the
two-condensate scale-gap probe, deciding the self-consistent background
question: with an IR-continuous relative potential (zero-dispersion
doubler sector excluded from both determinants, a recorded convention;
block functional dense-verified at 1.7e-12), the fundamental content
shows no condensation onset, and the derived content has a genuine
interior LOCAL two-condensate minimum (gradient 5e-4, positive-definite
Hessian; candidate-only) that the pure-lambda_8 log-runaway undercuts -
NO finite global minimum exists. The scale gap is now one named missing
structure: log-saturation (higher-order/compactness/UV) that no reviewed
source defines. Study: studies/phase435_two_condensate_scale_gap_probe_001
(IMPLEMENTATION_P435.md). Named next: the EXACT-HESSIAN TIE-IN (replace
the workbench bosonic model with the true control-branch Hessian via the
Phase400/405 machinery - the last internal experiment with scale-law
potential), then the performance program and monitoring.

Before that, the work (2026-07-02, seventh checkpoint: the five-phase TEAM WAVE
430-434, built by parallel agents in isolated study dirs and integrated
centrally) executed the full Coleman-Weinberg program named on 2026-07-01:
Phase430 - the full one-loop landscape is direction-selecting and THE
DERIVED MATTER CONTENT DYNAMICALLY SELECTS THE HYPERCHARGE AXIS (the
su(3)->su(2)xu(1) breaking direction), integer-exact slopes, no scale law;
Phase431 - a candidate lambda_8 background REOPENS the T/D distinction and
INDUCES the block-dependent mass law Phase418 had to import (opposite
signs: doublet destabilized), background candidate-only; Phase432 - the
SOURCE-PINNED chimeric weld breaks internal loop degeneracies (the
su-breaking fermionic structure Phase428 proved necessary exists in the
draft's own carriers); Phase433 - exact-rational blind beta ledger (the
n_f=3+Higgs row reproduces the standard SM set as a witness; blind running
slope family-independent); Phase434 - conditional photon/W/Z extraction
rows partitioning the Phase419 template 7/4/9. THE TWO HONEST REMAINING
GAPS: a derived (not candidate) background with a finite scale, and tying
the workbench bosonic determinant / weld normalization to the actual
control-branch Hessian and draft normalization. Studies:
phase430_net_one_loop_direction_selection_probe_001,
phase431_lambda8_background_doublet_reopening_probe_001,
phase432_welded_fermion_loop_block_selection_probe_001,
phase433_blind_beta_coefficient_running_ledger_001,
phase434_conditional_observed_field_extraction_row_ledger_001
(IMPLEMENTATION_P430..434.md). Named next: self-consistent background
iteration (feed stage-one into stage-two and iterate), exact-Hessian
tie-in, performance program, monitoring.

Before that, the work (2026-07-01, sixth checkpoint of the session) added
Phase429, the target-blind dimensionless-ratio ledger - the second
beyond-the-literature experiment. The complete scale-free surface of the
embedding chain is an explicit fail-closed ledger (exact arithmetic from
the blind tan^2 = 3/5 only): sin^2 = 3/8 and the GUT-normalized ratio 1
are fixed at the unification point; the tree-level m_W/m_Z = sqrt(5/8) is
conditional on the missing symmetry-breaking sector; each row lists its
six missing comparison-lineage fields, zero source-defined; derivation is
strictly separated from comparison (no measured value appears in the
phase). Study:
`studies/phase429_target_blind_dimensionless_ratio_ledger_001`
(IMPLEMENTATION_P429.md). Both named experiments of the 2026-07-01
directive are now done.

Before that, the work (2026-07-01, fifth checkpoint of the session) added
Phase428, the fermion-loop block-selection no-go probe - the FIRST
experiment of the beyond-the-literature directive, closing the last named
internal mechanism class for doublet VEV selection. On constant rank-1
rays the one-loop fermion determinant is an adjoint-orbit class function
(exact closed-form spectrum, dense-solve verified 1.6e-13); the color-swap
conjugator places lambda_1..lambda_7 on one orbit, so the potential is
EXACTLY triplet/doublet-degenerate (1.1e-13) in both probed
representations; every ray falls like -N log t (slopes match coupled-mode
counts exactly) so no quartic stabilizer is generated. Fermionic
backreaction therefore cannot select the doublet without su(3)-breaking
fermionic structure that no source defines. All named internal doublet-
selection mechanism classes are now closed (405 bosonic, 410 curvature,
418 quadratics, action-quartic post-processing, 428 fermion loop);
residuals are non-constant configurations, multi-loop effects, and a
source-defined su(3)-breaking specification. Study:
`studies/phase428_fermion_loop_block_selection_no_go_probe_001`
(IMPLEMENTATION_P428.md).

Before that, the work (2026-07-01, fourth checkpoint of the session) added
Phase427, the Hofseth GU-RVG superluminal source audit, discharging the
second and final NEW-LEAD from the same-day literature sweep. The
originally-catalogued record 21056575 was DELETED from Zenodo on
2026-07-01 (tombstone reason "duplicate", hours after the sweep found
it); the audit located and audited the live successor 21117379 (same
title; PDF md5 90be901bc227bc90e493c295aa276046; 6465 extracted lines).
The paper imports v = 246 GeV as an explicit input (its own
derived/input/open table marks it "I") to set a 27.2 TeV dilaton decay
constant, marks its condensate amplitude as "fixed by observation rather
than computation", and carries the externally-unverified
Weinstein-Harvard co-authorship matching the arXiv:2606.02184
fabricated-attribution pattern. No W/Z/H rows, projection, weak-angle,
pole, or GeV-from-geometry lineage; verdict
`hofseth-gu-rvg-superluminal-audited-external-inputs-no-contract-fill`.
Reference note:
`docs/Reference/ExperimentReferences/ZENODO-21117379-GURVG-SUPERLUMINAL.md`.
Study: `studies/phase427_hofseth_gu_rvg_superluminal_source_audit_001`
(IMPLEMENTATION_P427.md). BOTH sweep leads are now discharged; the
standing program returns to checkpoint-cadence monitoring and the
source-defined projection/action/VEV requirement.

Before that, the work (2026-07-01, third checkpoint of the session) added
Phase426, the Cox GU series (I-V) boson contract audit, discharging the
first NEW-LEAD from the same-day literature sweep. All five June 2026
Zenodo records are retrieved, md5-verified, extracted, and audited:
GU I/III/IV/V carry no electroweak contract content, while GU II ("The
Matter Ledger") derives the tree-level hypercharge kernel with
`g_Y^2 = (3/5) g^2` at a Pati-Salam unification point - INDEPENDENTLY
CORROBORATING the repository's blind Phase404 result - and names the
minimal Pati-Salam bi-doublet scalar channel `(1,2,2)` (the Phase403/409
doublet-carrier shape). GU II's own scope boundaries deny a scalar
potential, VEV, mass spectrum, unification scale, or measured fit, and it
does not prove any internal fluctuation contains the channel: verdict
`cox-gu-series-audited-gu-ii-corroborates-embedding-no-contract-fill`,
no contract fill, nothing promoted. Reference note:
`docs/Reference/ExperimentReferences/COX-GU-SERIES-I-V-202606.md`. Study:
`studies/phase426_cox_gu_series_boson_contract_audit_001`
(IMPLEMENTATION_P426.md). The remaining NEW-LEAD (Hofseth GU-RVG
successor, zenodo 21056575) is the named next audit.

Before that, the work (2026-07-01, second checkpoint of the session) added
Phase425, the cross-carrier bilinear SM-doublet completion audit. It
constructs the `Q_{3/2}` carrier exactly (the spacetime `6` = the
gamma-traceless remainder in `4 x 2 = 2 + 6`, the 4D analog of Phase417's
`10 x 16 = 16 + 144` split; `A4 A4^dag = 4I` exact; welded content exactly
`(1/2,1)`/`(1,1/2)`; no linear welded scalar) and decides every unprobed
two-carrier bilinear channel among `S = 2 x 16`, `Z = 2 x 144`,
`Q = 6 x 16`, and the mirror sector: the seven mixed-parity channels have
exactly zero welded-scalar capacity by character arithmetic, and the eight
same-parity channels (capacities 13/14/6/29 complex per chirality pair)
all have ZERO SM-doublet content by the ambient intersection (top Gram
eigenvalues 0.017-0.444 vs the required 1.0). Mirror channels transfer by
representation identity. Combined with Phases 409/411/412/422/424, NO
BILINEAR COMPOSITE OF ANY SOURCE-PINNED CARRIER PAIR CARRIES A
WELDED-SCALAR SM-DOUBLET - the bilinear composite layer is CLOSED on the
complete carrier menu of the primary draft. A NumPy prototype validated
every number first; the study runs Release (~1 min). Study:
`studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001`
(IMPLEMENTATION_P425.md).

The same session's literature sweep resolved two named monitor targets and
found two NEW-LEAD records (both pre-screening non-promotional, named for
short fail-closed audits at the next checkpoint): the Cox "Geometric
Unity V" series now exists (GU V "The Lambda Rig"
`10.5281/zenodo.20531776`, GU IV "The Observable Interface"
`10.5281/zenodo.20518853`, GU III `10.5281/zenodo.20517502`, GU I reissue
`10.5281/zenodo.20550275`, published 2026-06-02..05; cosmology-only audit
protocol), and the Hofseth GU-RVG successor record
`10.5281/zenodo.21056575` (2026-06-18; superluminal metric engineering;
condensate amplitude "fixed by observation rather than computation"; the
claimed Weinstein co-authorship matches the known fabricated-attribution
pattern, arXiv:2606.02184). The "Kleis" lead is RESOLVED (kleis.io, a
Z3/SMT verification platform, no GU content) and the Hebrew University
dark-energy talk has NO artifact (closest proxy: the April 2025 UCSD
"From Dark to Geometric Energy" talk on theportal.group, qualitative
dark-energy content only). geometricunity.org is unchanged.

Before that, the work (2026-07-01) added Phase424, the vector-spinor `144`
bilinear SM-doublet intersection analysis, deciding the question Phase422
explicitly deferred. Using the Phase412 ambient-intersection method
(strictly stronger than a stable-subspace census; covers all statistics
projections) on an exact SM-diagonal complex kernel basis (gamma-trace
frame `A A^dag = 10 I` exact, Cartan residual `1.2e-10`, projector
idempotency `<= 1e-6`), it shows the 528-dimensional same-chirality
Majorana-like welded-scalar sector contains NO SM-doublet state:
`Z_L x Z_L` and `Z_R x Z_R` each have candidate weight sector `3584`,
doublet isotypic `1344` real dims, and intersection ZERO with top Gram
eigenvalues `0.059259` vs the required `1.0`. With Phase417 (linear) and
Phase422 (mixed-chirality) this CLOSES the vector-spinor `144` bilinear
composite route. The same run confirms the corrected Phase411 Majorana
verdict on the observed `2 x 16` channels (intersection zero in both) and
cross-checks Phase422's capacities by character arithmetic (264/264/0
exact). An independent NumPy prototype reproduced every count and
eigenvalue first. The phase runs Release in the generator (~7 min vs
~25 min under the Debug JIT). Study:
`studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001`
(IMPLEMENTATION_P424.md).

The same session found and fixed the |Y|=1/2 CALIBRATION DEFECT in the
Phase411/417 informational SM censuses: both calibrated |Y|=1/2 as the
"smallest Y^2 eigenvalue above 0.05" on the 16, which selects the |Y|=1/3
family value 1/9, not the lepton-doublet value 1/4 (the 16's Y weights
include +/-1/2 exactly; Phase412's weight-census method was never
affected). After the fix (exact Y^2=1/4, asserted to 1e-9; Phase411 also
moved to a joint-eigenbasis census), Phase417 reports
`internalSmHiggsPatternComplexDimension=6` (a fermionic block, not a
welded scalar - the linear no-go is unchanged) and Phase411 keeps
`majoranaSpinZeroSmDoubletCount=0`. No promotion decision depended on
either count. Phase202 and the claim-integrity script assert the corrected
values with dated notes; see the 2026-07-01 journal entry.

The session also CLOSED exploration candidate C (deriving Phase418's
"external" quartic stabilizer from the action itself) using committed
Phase410 pair-table data alone: within-block quartics are TT=2.25 uniform
vs DD min 0.5625 / avg 1.125, block sums exactly symmetric (6.75), rank-1
rays exactly flat - the action's own quartic cannot select the doublet, so
Phase418's external-stabilizer requirement is genuine.

Before that, the work added Phase423, the June 2026 Zenodo GU-RVG spinorial
dark-sector boson contract audit for DOI `10.5281/zenodo.20618066`. It records
the PDF (`1647698` bytes,
`md5:da23008825cf90eb89138b5c560ef47f`), supplemental ZIP (`1556360` bytes,
`md5:53b2461515af445e3a6a459ace3619bb`), `pdftotext` extraction line count
`4015`, and supplemental search scope (`58` archive entries, `32` text/code
files). Exact result:
`zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed=True`,
`currentJune2026SourceDeltaConfirmed=True`,
`priorPhase312SourceSetDidNotIncludeThisRecord=True`,
`sourceProvidesSpinorialDarkSectorContext=True`,
`sourceMentions95GevDilaton=True`,
`sourceMentionsKoideDilatonShift=True`,
`sourceUsesExternalElectroweakVev246Gev=True`,
`sourceProvidesVectorSpinor144ProjectionMap=False`,
`sourceProvidesBosonContractEvidence=False`,
`sourceProvidesObservedElectroweakNamespaceMap=False`,
`sourceProvidesPhotonWzHProjectionRows=False`,
`sourceProvidesWzSourceRows=False`,
`sourceProvidesHiggsScalarSourceRow=False`,
`sourceProvidesWeakAngleOrCouplingLineage=False`,
`sourceProvidesPoleExtraction=False`,
`sourceProvidesGeVUnitNormalization=False`,
`canFillPhase201WzContract=False`,
`canFillPhase201HiggsContract=False`, and
`canFillPhase256ObservedFieldExtractionContract=False`. Interpretation: the
new source is useful current GU-RVG spinorial dark-sector / 95.4 GeV
dilaton/Koide context, but it supplies no electroweak source-lineage rows and
does not answer Phase422's vector-spinor projection-map requirement. Study:
`studies/phase423_zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_001`
(IMPLEMENTATION_P423.md).

Before that, Phase422 added the vector-spinor `144` bilinear scalar-capacity
audit. It extends Phase417's source-pinned `Z_{1/2}` vector-spinor branch beyond
the linear `2 x 144` no-go by testing exact welded character scalar capacity for
`Z_L x Z_L`, `Z_R x Z_R`, and `Z_L x Z_R`. Exact result:
`vectorSpinor144BilinearScalarCapacityAuditPassed=True`,
`sameChiralityScalarCapacity=528`, `mixedChiralityScalarCapacity=0`,
`mixedChiralityDiracLikeScalarChannelClosed=True`,
`sameChiralityMajoranaLikeScalarSectorPresent=True`,
`directSmStableAnalysisDeferredDueToLargeSector=True`,
`vectorSpinor144BilinearStillRequiresSourceProjectionMap=True`, and no
Phase201/Phase256 field can be filled. Interpretation: the mixed-chirality
Dirac-like bilinear route is closed, while same-chirality Majorana-like
bilinears have a large candidate-only welded-scalar capacity requiring a
source-defined projection map, action, VEV rule, observed-field rows, weak-angle
lineage, pole extraction, and GeV normalization. Study:
`studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001`
(IMPLEMENTATION_P422.md).

Before that, Phase421 added the Cox GU IV v2 LCDM rig boson contract audit.
It materializes the restart prompt's source-level theorem-search follow-up for
Zenodo DOI `10.5281/zenodo.17402261` by verifying the actual `GUT.4.1.pdf`
artifact (`702258` bytes, `md5:1d51f99a44cf51c8023dbc500e58ed3c`,
`pdftotext` line count `3305`). Exact result:
`coxGuIvV2LcdmRigBosonContractAuditPassed=True`,
`lcdmRigScopeConfirmed=True`, `projectorVariationTheoremPresent=True`,
`etheringtonGuardrailPresent=True`, `sevenCosmologyHooksPresent=True`,
`rigHookCount=7`, `electroweakProjectionHookCount=0`,
`absentContractKeywordCount=14`,
`sourceProvidesBosonContractEvidence=False`,
`sourceProvidesObservedElectroweakNamespaceMap=False`,
`sourceProvidesPhotonWzHProjectionRows=False`,
`sourceProvidesWzSourceRows=False`,
`sourceProvidesHiggsScalarSourceRow=False`,
`sourceProvidesElectroweakVevMap=False`,
`sourceProvidesWeakAngleOrCouplingLineage=False`,
`sourceProvidesCurvatureToElectroweakScaleLaw=False`,
`sourceProvidesPoleExtraction=False`,
`sourceProvidesGeVUnitNormalization=False`,
`canFillPhase201WzContract=False`,
`canFillPhase201HiggsContract=False`,
`canFillPhase256ObservedFieldExtractionContract=False`,
`routePromotesWzMasses=False`, and `routePromotesHiggsMass=False`.
Interpretation: GU IV v2 is a LambdaCDM/cosmology test rig with BRST/BV
projector-variation guardrails, Etherington reciprocity, seven data hooks, and
reproducibility/corridor checks. It is not an electroweak source-law paper and
contains no fillable Phase201/Phase256 contract evidence. Study:
`studies/phase421_cox_gu_iv_v2_lcdm_rig_boson_contract_audit_001`
(IMPLEMENTATION_P421.md).

Before that, Phase420 added the naive curvature mass-scale sanity check.
It materializes the restart prompt's post-Phase419 scale branch by testing the
Superphysics/GU-draft `part-12c` stylized `m = R(y)/4` clue without consulting
physical W/Z/H targets. Exact result:
`naiveCurvatureMassScaleSanityCheckPassed=True`,
`literalScalarCurvatureMassReadingDimensionallyConsistent=False`,
`squaredMassCurvatureReadingDimensionallyConsistent=True`,
`squaredMassReadingProvidesOnlySymbolicScaleShell=True`,
`singleCurvatureScalarCanOnlySetCommonScale=True`,
`naiveCurvatureLawCanDistinguishWzHRows=False`,
`providedRequiredScaleSpecificationFieldCount=1`,
`missingScaleSpecificationFieldCount=9`,
`sourceProvidesGeVUnitNormalization=False`,
`canFillPhase201WzContract=False`,
`canFillPhase201HiggsContract=False`,
`canFillPhase256ObservedFieldExtractionContract=False`,
`routePromotesWzMasses=False`, and `routePromotesHiggsMass=False`.
Interpretation: the literal scalar-curvature mass reading is dimensionally
closed; the Lichnerowicz-style `m^2 = R/4` repair is only a symbolic one-scale
shell. A viable curvature route now needs a source-defined curvature value or
equation, sign, coefficient normalization, electroweak VEV map,
weak-angle/coupling lineage, photon/W/Z/H rows, pole extraction, and GeV/unit
normalization. Study:
`studies/phase420_naive_curvature_mass_scale_sanity_check_001`
(IMPLEMENTATION_P420.md).

Before that, Phase419 added the observed-field symbolic extraction
template. It materializes the restart prompt's FMS/dressing-field-style branch:
assume a scalar doublet exists, then write the target-blind algebraic rows
needed for photon, W, Z, and Higgs observed-field projection before Phase256
could ever be filled. Exact result:
`observedFieldSymbolicExtractionTemplatePassed=True`,
`applicationSubjectKind=fms-dressing-field-observed-extraction-template`,
`fmsDressingExternalTemplateSupport=True`,
`fmsDressingExternalTemplatesPromotional=False`,
`symbolicMapRowCount=8`, `projectionRowCount=4`,
`allPhase256FieldsMappedBySymbolicTemplate=True`,
`sourceDefinedPhase256FieldCount=0`,
`requiredGuInputProvidedCount=0`,
`phase295IntakeReadyCandidateCount=0`, and
`canFillPhase256ObservedFieldExtractionContract=False`. Interpretation: the
minimal observed-field map is now explicit as a symbolic checklist, but it
does not supply a GU-native doublet carrier, observed vacuum, dressing field,
electroweak embedding, weak-angle/coupling lineage, mass operator, pole
extraction, stability sidecars, or GeV normalization. No Phase201 or Phase256
field is filled. Study:
`studies/phase419_observed_field_symbolic_extraction_template_001`
(IMPLEMENTATION_P419.md).

Before that, Phase418 added the direction-dependent curvature/VEV coupling
scan. It extends Phase410's uniform-curvature no-go by enumerating the minimal
residual-`su(2)+u(1)` block-isotypic menu on the same `su(3)` control branch:
triplet `T={0,1,2}`, doublet `D={3,4,5,6}`, and singlet `S={7}`. Exact
result: `directionDependentCurvatureVevCouplingScanPassed=True`,
`blockProjectorsCommuteWithResidualGaugeAction=True`,
`blockProjectorsCommuteWithFullSu3=False`,
`pureQuadraticDirectionDependentCouplingsStillRunAway=True`,
`stabilizedLandauMenuCandidateCount=9`,
`stabilizedCandidateSelectsDoubletCount=5`,
`nonTautologicalDoubletSelectorCount=4`,
`sourceDefinedDoubletSelectorCount=0`,
`directionDependentCouplingSourceLawStillMissing=True`,
`finiteVevScaleStillExternal=True`, and
`quarticStabilizerStillExternal=True`. Interpretation: the direction-dependent
curvature branch is not mathematically empty - stabilized block laws can select
the doublet - but every successful selector imports an extra block mass law and
quartic stabilizer that current sources do not define. Pure quadratic
direction-dependent curvature terms still run away on the Phase405/410 flat
rays. No Phase201 or Phase256 field is filled. Study:
`studies/phase418_direction_dependent_curvature_vev_coupling_scan_001`
(IMPLEMENTATION_P418.md).

Before that, Phase417 added the vector-spinor `144` decomposition probe.
It takes the Phase416 `Z_{1/2}` carrier target and constructs the
`10 x 16 -> 16 + 144` split directly as the gamma-trace kernel
`10 x 16+ -> 16-`, using the same Cl(10), SM-chain, and Sym^2 welded-spin
conventions as Phases407/411. Exact result:
`vectorSpinor144DecompositionProbePassed=True`,
`gammaTraceRankComplex=16`,
`vectorSpinor144KernelComplexDimension=144`,
`vectorSpinor144DimensionCheck=True`,
`vectorSpinor144InvariantUnderProbedGenerators=True`,
`internalSmHiggsPatternComplexDimension=6` (the value originally recorded
here as 0 was a |Y|=1/2 calibration defect, fixed and re-run 2026-07-01 -
the block is fermionic representation data, not a welded scalar),
`linearWeldedScalarCountTotal=0`,
`vectorSpinor144LinearCarrierHasNoWeldedScalar=True`, and
`sourceDefinedEvenCompositeOrBosonicProjectionMapCount=0`. The vector-spinor
branch is now closed at the linear gamma-trace/decomposition level: no linear
scalar/VEV carrier emerges, and current sources still supply no bosonic
projection/action/VEV map, observed photon/W/Z/H rows, weak-angle lineage,
pole extraction, or GeV normalization. No Phase201 or Phase256 field is
filled. Study:
`studies/phase417_vector_spinor_144_decomposition_probe_001`
(IMPLEMENTATION_P417.md).

Before that, Phase416, the unobserved-phase carrier census, grounded the
previously vague "unobserved phase" residual in GU-DRAFT-2021 sections 11-12:
the source pins three unobserved/dark fermionic carrier families, `Q_{3/2}`
(192 states per displayed `zeta` side), `Z_{1/2}` (576 states per side from
the vector-spinor `144` remainder in `10 x 16 = 16 + 144`), and a dark
decoupled Weyl-half / Looking Glass mirror sector (64 states). The source
dimension check is exact: `64 + 192 + 576 = 832`. No Phase201 or Phase256
field is filled. Study:
`studies/phase416_unobserved_phase_carrier_census_001`
(IMPLEMENTATION_P416.md).

Before that, Phase415, the fermionic cohomology/square-root
`delta_omega` ansatz probe. It treats Superphysics `part-09b` section 9.3 as a
research clue, not a theorem, and turns the clue into an admissibility ledger:
the reviewed source names `chi`, `omega` subfields, `Upsilon_omega`,
cohomology-obstruction language, and a possible first-order square root, but
supplies none of the eight required specification fields (differential, carrier
complex, cohomology target, square law, adjoint/inner product, scalar-doublet
projection, observed boson rows, normalization lineage). The locally
specifiable candidates are closed by prior evidence: Phase411 closes the
chiral Dirac/Yukawa bilinear route, Phase389 materializes only a discrete
control-branch identity, Phase397 shows neutral mixing vanishes in the
fermion-bilinear channel, Phase401 closes the perturbative coupled-critical
point route on the toy action, and Phase414 closes the low-order
Shiab/epsilon envelope. Exact result:
`fermionicCohomologySquareRootAnsatzProbePassed=True`,
`localCandidateComplexCount=6`, `locallySpecifiedCandidateCount=3`,
`closedLocalCandidateCount=3`, `openSpecificationCandidateCount=3`,
`candidateComplexesWithWeldedScalarSmDoubletCount=0`, and
`candidateComplexesWithObservedProjectionDataCount=0`. Remaining work now
requires a source-defined `delta_omega` complex or a computable
unobserved-phase carrier. Study:
`studies/phase415_fermionic_cohomology_square_root_ansatz_probe_001`
(IMPLEMENTATION_P415.md).

Before that, Phase414, the general Shiab/epsilon operator ansatz
closure certificate requested by the 2026-06-16 restart guidance. It enumerates
the low-order invariant operation alphabet (wedge, Hodge star, contraction,
commutator, `i`-weighted anticommutator, Clifford volume, epsilon conjugation)
and binds every currently computable family to exact upstream no-go evidence:
Phase408 closes the naive vertical trace route; Phase409 closes linear,
bilinear, and epsilon-built frame-cross extraction; Phase411/412 close
spinor-bilinear and quartic composite variants; Phase413 closes finite-
dimensional noncompact real-form evasion. Exact result:
`generalShiabEpsilonOperatorAnsatzProbePassed=True`,
`operatorMenuCandidateCount=13`, `closedLowOrderFamilyCount=11`,
`openResidualFamilyCount=2`, and
`anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet=False`. The remaining
work is now sharply two-pronged: a computable unobserved-phase carrier, or an
explicit first-order cohomology/square-root `delta_omega` specification. Study:
`studies/phase414_general_shiab_epsilon_operator_ansatz_probe_001`
(IMPLEMENTATION_P414.md).

Before that, Phase413, the noncompact real-form transfer
probe, closing the most plausible loophole named by
DEEP-RESEARCH-20260612. Exact results: the Lorentzian chimeric weld
pi_eta: so(1,3) -> gl(10) on Sym^2(R^{1,3}) is an exact homomorphism
preserving the induced trace form of machine signature (7,3) (the
Phase406 Cl(7,3) axis); the KEYSTONE - the complexified Lorentzian and
compact welds coincide exactly under T4 = diag(i,1,1,1) (2.2e-16) - so
every complex-linear kernel count in Phases 408-412 transfers verbatim;
direct noncompact recomputation confirms (linear singlet 0, bilinear
spin-0 = 7). THE NO-GOS ARE REAL-FORM INDEPENDENT. The remaining named
routes reduce to TWO: the draft's unobserved-phase fields, or a new
primary-source specification. Study:
`studies/phase413_noncompact_real_form_transfer_probe_001`
(IMPLEMENTATION_P413.md).

Before that, Phase412 (user directive), the quartic
SM-doublet intersection analysis, deciding the order Phase411 deferred
- via an AMBIENT-INTERSECTION formulation strictly stronger than the
stable-subspace one. Exact results: the SM-doublet candidates of
16^(x4) live in a 896-state weight sector; the doublet isotypic is
exactly 480 real dims (residual 8.0e-15); its intersection with the
channel-allowed welded isotypics is ZERO in every channel (top Gram
eigenvalues 0.146/0.000/0.479/0.000/0.113, union 0.604, vs the
required 1.0); the odd-mixed channels LLLR/LRRR carry zero welded
singlets at all; the even channels reproduce Phase411's counts. THE
COMPOSITE PROGRAM IS CLOSED THROUGH QUARTIC ORDER on every probed
carrier (frame-cross tensor, bosonic vacuum, spinor bilinear, spinor
quartic), covering all statistics projections. Remaining named routes:
the draft's unobserved-phase fields, a noncompact real-form evasion
(Nguyen-Polya caveat), or a new primary-source specification. Study:
`studies/phase412_quartic_sm_doublet_intersection_analysis_001`
(IMPLEMENTATION_P412.md). Runtime ~8 min (parallelized; first attempt
timed out single-threaded - engineering notes in the journal).

Before that, Phase411, the quartic/Dirac-squared spinor-sector
composite probe - the convergence point of Phase409, Phase410, and the
"quartic Higgs from Dirac squaring" heuristic. On the chimeric chiral
carriers S_L/R = 2_L/R (x) 16 (Cl(4) Weyl halves, machine-verified
homomorphism; Phase404 Cl(10) 16; Phase408 weld), exactly: the internal
16 branches as (1/2,3/2) + (3/2,1/2); the DIRAC MASS CHANNEL S_L x S_R
(the Yukawa channel, where the SM Higgs couples) contains ZERO welded
scalars - its (half,int) x (int,half) content cannot pair to a singlet
for any alignment; the Majorana channels carry 16 welded scalars each
(direct kernel = character count) with ZERO SM-doublet states in their
SM-stable subspace. The composite-extraction question is now closed
through bilinear order on EVERY probed carrier (frame-cross tensor,
bosonic vacuum, spinor bilinear). Named remaining: the quartic
SM-stable analysis, the draft's unobserved-phase fields, a noncompact
real-form evasion (complex/compact arithmetic; Nguyen-Polya caveat
carried). Study:
`studies/phase411_quartic_dirac_squared_spinor_composite_probe_001`
(IMPLEMENTATION_P411.md).

Before that, Phase410, the curvature-coupled VEV-selection
probe: the TOE-GU-40YEARS curvature-coaxing claim's simplest faithful
bosonic realization (S_aug = S_B + (kappaR/2) R_eff ||omega||^2 on the
Phase405 su(3) 6x6 landscape) FAILS two ways at once - (C1) RUNAWAY:
every rank-1 vacuum ray is exactly flat, so the augmented landscape is
unbounded below along every such ray (no finite VEV forms where the
vacuum manifold lives); (C2) the quadratic invariant is direction-blind
(no block ordering at quadratic level); (C3) on the lifted sector the
exact shape K proportional to ||[u,v]||^2 (cv 1.2e-15) makes the depth
ordering the INVERSE bracket-norm ordering; (C4) the deepest stratum is
BLOCK-DEGENERATE (doublet-doublet AND doublet-triplet pairs) - no
doublet selection. Sub-gap (b) stays open, sharpened: the mechanism
requires direction-dependent coupling or the fermionic/Dirac-sector
realization (the Phase411 candidate). Study:
`studies/phase410_curvature_coupled_vev_selection_probe_001`
(IMPLEMENTATION_P410.md).

Before that, Phase409, the invariant-pairing-menu spin-zero
extraction probe (source-independent of the unverified relayed "Shiab
uniqueness" summary - the menu is machine-enumerated on the chain from
scratch). Exact results: the fiber pairing menu is 4x4 = 1 (even),
10x10 = 2 (even), 4x10 = 0 (NO parity-odd fiber pairing exists); the
welded frame-cross block V = 4x10 = (1/2,1/2)^2 + (1/2,3/2) + (3/2,1/2)
+ (3/2,3/2) has ZERO singlet content (every linear extraction closed,
any parity); the complete bilinear spin-0 sector is exactly 7-dim
(6 parity-even + 1 epsilon-built parity-odd) and its largest SM-stable
subspace (1-dim) contains NO SM-doublet state in either parity sector;
ALL ODD ORDERS are closed exactly (every welded irrep is half-integer x
half-integer - trilinear count 0). The epsilon route ON FRAME-CROSS
CONTENT is closed through bilinear order; the named remaining routes are
quartic+ even composites, epsilon/Shiab machinery on content BEYOND the
frame-cross block (spinorial / unobserved-phase fields), or a different
welded carrier. Study:
`studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001`
(IMPLEMENTATION_P409.md).

Immediately before that, the same day discharged the named PLATFORM
FOLLOW-UP:
the Phase405 GPU parity defect was root-caused, fixed, and
regression-tested. Fail-closed bisection (a minimal C harness on a
single triangle and on the full real 420-edge/216-face mesh, then a C#
probe splitting the production path) EXONERATED the native CUDA
curvature kernel - it is exact on real mesh topology. The actual defect:
`GpuSolverBackend.Initialize` re-initialized the already-prepared native
backend; `gu_initialize` does a full shutdown first, silently discarding
the uploaded topology/algebra/A0, so every native physics kernel
silently degraded to its identity-stub fallback (F = omega, T = 0 -
exactly the recorded DIAG signature). Fix:
`src/Gu.Interop/GpuSolverBackend.cs` Initialize now adopts a prepared
backend instead of wiping it. Guard: real-mesh parity tests in
`tests/Gu.Interop.Tests/RealMeshPhysicsParityTests.cs` (managed
reference + real CUDA, in the serialized GPU collection). Phase405
re-run: parity 27/27 (maxAbsDev 3.9e-34), all science verdicts
unchanged (science ran on CPU per IA-5 throughout); its summary JSON,
STUDY.md, and IMPLEMENTATION_P405.md carry the dated resolution. See
the 2026-06-12 journal entry.

Before that, Phase408 (the vertical spin-zero extraction obstruction
probe) formalized the chimeric weld exactly (homomorphism residual
2.2e-16) and machine-verified: (V1) the weld ENTANGLES spin and isospin;
(V2) the centralizer of pi(so(4)) in so(10) is TRIVIAL; (V3) the spin-0
slot of the vertical 10 is the 1-dim trace direction, so the naive
vertical-trace extraction CANNOT carry the 4-real-dim SM doublet for ANY
weld alignment. Scalar-sector sub-gap (a) is at its final internal form:
the draft's epsilon-conjugation/Shiab machinery or unobserved-phase
fields (not specified quantitatively in the primary text) is the
precisely named requirement. THE INTERNAL STRUCTURAL PROGRAM IS AT ITS
HONEST BOUNDARY. Study:
`studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001`
(IMPLEMENTATION_P408.md).

### Integration Points Already Updated

Phases 442-445 (like Phase388-441) are wired into:

- `scripts/generate_validated_boson_predictions.sh` (single broad pass; the
  older duplicated final sweep was removed on 2026-06-16; the Phase424 line
  uses `dotnet run -c Release` because its dense complex spectral
  projectors take ~25 min under the Debug JIT vs ~7 min optimized)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist items
  `observed-field-symbolic-extraction-template-materialized` and
  `naive-curvature-mass-scale-sanity-check-materialized` and
  `cox-gu-iv-v2-lcdm-rig-boson-contract-audit-materialized` and
  `vector-spinor-144-bilinear-scalar-capacity-audit-materialized` and
  `zenodo-gu-rvg-spinorial-dark-sector-boson-contract-audit-materialized` and
  `vector-spinor-144-bilinear-sm-doublet-intersection-analysis-materialized` and
  `cross-carrier-bilinear-sm-doublet-completion-audit-materialized` and
  `cox-gu-series-boson-contract-audit-materialized` and
  `hofseth-gu-rvg-superluminal-source-audit-materialized` and
  `fermion-loop-block-selection-no-go-probe-materialized` and
  `target-blind-dimensionless-ratio-ledger-materialized` and
  `net-one-loop-direction-selection-probe-materialized` and
  `lambda8-background-doublet-reopening-probe-materialized` and
  `welded-fermion-loop-block-selection-probe-materialized` and
  `blind-beta-coefficient-running-ledger-materialized` and
  `conditional-observed-field-extraction-row-ledger-materialized` and
  `two-condensate-scale-gap-probe-materialized` and
  `exact-hessian-saturation-no-go-probe-materialized` and
  `four-dimensional-transmutation-scaling-probe-materialized` and
  `self-consistent-condensate-gap-equation-probe-materialized` and
  `gap-equation-lambda8-background-channel-steering-probe-materialized` and
  `coupled-background-condensate-fixed-point-probe-materialized` and
  `toy-branch-family-universality-sweep-materialized` and
  `joint-omega-theta-hessian-degree-probe-materialized` and
  `joint-effective-potential-saturation-probe-materialized` and
  `mode-volume-scaled-saturation-probe-materialized` and
  `rg-improved-joint-potential-probe-materialized`;
  the Phase417 checklist row now asserts the corrected
  `yHalfCalibrationExact=True` and `internalSmHiggsPatternComplexDimension=6`)
- `scripts/verify_boson_claim_integrity.sh` (Phase424 asserts plus the
  corrected Phase417 census assert)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md`,
`docs/Reference/ExperimentReferences/ZENODO-20618066-GURVG-SPINORIAL-DARK-SECTOR.md`
(Phase423 current GU-RVG spinorial dark-sector source audit),
`docs/Reference/ExperimentReferences/COX-GU-IV-V2-17402261.md`
(Phase421 full-text source audit),
`docs/Reference/ExperimentReferences/FMS-GAUGE-INVARIANT-SPECTRUM.md` and
`docs/Reference/ExperimentReferences/DRESSING-FIELD-ELECTROWEAK-OBSERVED-VARIABLES.md`
(Phase419 external-template use notes),
`docs/Reference/ExperimentReferences/LOCAL-COMPLETION-V29-FERMIONIC-YUKAWA.md`
(Phase399/Phase400/Phase401 coupled-stationarity closure section), and
`docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md`
(2026-06-16 expanded Superphysics mirror check plus Phase415 use of part-09b
as a non-promotional `delta_omega` clue and Phase420 use of part-12c as the
naive curvature-scale clue), and `GU-DRAFT-2021.md` / the TOE Iceberg note
(Phase416 carrier census plus Phase417 vector-spinor `144` decomposition plus
Phase418 direction-dependent curvature boundary, and Phase422's
vector-spinor bilinear scalar-capacity boundary). The
diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase413_noncompact_real_form_transfer_probe_001/Phase413NoncompactRealFormTransferProbe.csproj
dotnet run --project studies/phase414_general_shiab_epsilon_operator_ansatz_probe_001/Phase414GeneralShiabEpsilonOperatorAnsatzProbe.csproj
dotnet run --project studies/phase415_fermionic_cohomology_square_root_ansatz_probe_001/Phase415FermionicCohomologySquareRootAnsatzProbe.csproj
dotnet run --project studies/phase416_unobserved_phase_carrier_census_001/Phase416UnobservedPhaseCarrierCensus.csproj
dotnet run --project studies/phase417_vector_spinor_144_decomposition_probe_001/Phase417VectorSpinor144DecompositionProbe.csproj
dotnet run --project studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/Phase418DirectionDependentCurvatureVevCouplingScan.csproj
dotnet run --project studies/phase419_observed_field_symbolic_extraction_template_001/Phase419ObservedFieldSymbolicExtractionTemplate.csproj
dotnet run --project studies/phase420_naive_curvature_mass_scale_sanity_check_001/Phase420NaiveCurvatureMassScaleSanityCheck.csproj
dotnet run --project studies/phase421_cox_gu_iv_v2_lcdm_rig_boson_contract_audit_001/Phase421CoxGuIvV2LcdmRigBosonContractAudit.csproj
dotnet run --project studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/Phase422VectorSpinor144BilinearScalarCapacityAudit.csproj
dotnet run --project studies/phase423_zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_001/Phase423ZenodoGuRvgSpinorialDarkSectorBosonContractAudit.csproj
dotnet run -c Release --project studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001/Phase424VectorSpinor144BilinearSmDoubletIntersection.csproj
dotnet run -c Release --project studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001/Phase425CrossCarrierBilinearSmDoubletCompletionAudit.csproj
dotnet run --project studies/phase426_cox_gu_series_boson_contract_audit_001/Phase426CoxGuSeriesBosonContractAudit.csproj
dotnet run --project studies/phase427_hofseth_gu_rvg_superluminal_source_audit_001/Phase427HofsethGuRvgSuperluminalSourceAudit.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
# Optional Phase405 CUDA parity characterization only when needed:
PHASE405_ENABLE_GPU=1 LD_LIBRARY_PATH=native/build dotnet run --project studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/Phase405VacuumManifoldDoubletVevOrbitScan.csproj
```

The targeted Phase424 (Release, ~7 min) and Phase425 (Release, ~1 min) runs
pass and preserve the fail-closed boundary; the fixed Phase411/Phase417
re-runs pass with their corrected censuses; Phase202 now reports
`checklistPassedCount=245`, `checklistFailedCount=3`; claim integrity
verifies Phase424 through Phase452 (and the corrected Phase417 values)
with `promotedPhysicalMassClaimCount=0`. The full direct
`./scripts/generate_validated_boson_predictions.sh` pass completed with
Phase424 through Phase436 included and ended at
`boson-claim-integrity-verified`. (Platform
state: Gu.Interop.Tests 158/158 with the real-mesh parity and
buffer-handle recycling tests; both Phase405 platform notes discharged
2026-06-12.) All seven broad scanners still report zero intake-ready
evidence.

Validation-context warning from 2026-06-16: run the generator directly as shown
above. In this Codex shell, redirecting `dotnet run`/`dotnet build` output to a
plain file produced a local shell/fnm false failure (`Build FAILED` with zero
compiler errors) around Phase280, while the same project and the direct full
generator invocation passed. Do not treat a redirected/piped tail script as the
validation signal; use the real script exit code from the direct command.

### Current Reference Structure

`ExperimentReferences.md` is the top-level source ledger. Each row should have
a linked detail note under `docs/Reference/ExperimentReferences/`.

When adding or revisiting a source:

1. Add/update the row in `ExperimentReferences.md`.
2. Add/update the linked detail note.
3. Record how the source was used.
4. State exactly what source-lineage fields it can and cannot supply.
5. Update `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md` when it changes the
   diagnosis or confirms a negative boundary.

Important current local detail notes:

- `LOCAL-COMPLETION-V29-FERMIONIC-YUKAWA.md`
- `LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION.md`
- `DIRAC-SHELL-RESPONSE-BOUNDARY.md`
- `COX-FIRST-PRINCIPLES-I-19800512.md`
- `COX-GU-IV-V2-17402261.md`: full-text audit of GU IV v2's LambdaCDM
  testing rig; positive for guardrails/hooks, negative for electroweak
  observed-field rows, W/Z/H source lineage, VEV/scale, pole, and GeV
  normalization
- `TOE-GU-ICEBERG-20250423.md` (+ full transcript under
  `docs/Reference/ExperimentReferences/transcripts/`): the GU-native
  structural ansatz for the scalar/Yukawa gap rows - qualitative only;
  cite GU-DRAFT-2021 as primary
- `SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md`: readable mirror/search aid for
  the public GU draft. The 2026-06-16 all-page pass found no fillable
  epsilon/Shiab doublet map, curvature-to-electroweak-VEV law,
  observed-field projection rows, weak-angle running, pole extraction, or GeV
  normalization. Phase420 then tested the `part-12c` `m = R(y)/4` clue and
  found the literal scalar-curvature mass reading dimensionally invalid and the
  squared-mass repair only a symbolic one-scale shell.

### Performance Program Status (2026-07-02)

The generator now builds all 301 phase projects ONCE via
`scripts/BosonPhasesTraversal.proj` (parallel MSBuild) and runs each phase
with `dotnet run --no-build -c Release`; run order and behavior are
unchanged (full pass verified, jitter-only diffs). Measured honestly:
20.2 min vs the 22.1-min baseline (~9%) - the pass is COMPUTE-BOUND in
the heavy numeric phases (8563 s CPU at 710% utilization), not
build-bound. Side benefit: single-phase iteration is much faster after
one traversal build. NOTE: new phases must be added BOTH to the generator
script AND to scripts/BosonPhasesTraversal.proj (the traversal item
list), or --no-build will fail on the unbuilt project. THE NEXT REAL
LEVER is content-hash incremental validation - DESIGN NOW COMPLETE AND
BINDING, see "PERFORMANCE EXPLORATION OUTCOME (2026-07-09)" above
(~100x on the typical append-one-phase checkpoint; committed
fingerprint manifest; instrumented read-set capture because static
parsing is unsound; whole-repo scanners and tail gates always run;
--full retained for promotion-relevant claims). Not yet implemented.

### Four-Dimensional Platform Build (decision pending)

The costed engineering plan for the dimX-four platform build - the
terminal frontier's named prerequisite - is at
docs/Phases/FOUR_D_PLATFORM_BUILD_PLAN.md: ~3,600 LOC, 10 new classes,
three additive milestones (4D mesh, Cl(3,1) spinor layer, eq. 9.3
Einsteinian Shiab), 5.5-8 weeks, main risk = physics
under-determination of the draft's Shiab (Definition 8.1 / eq. 9.3),
which the Phase441 candidate-family methodology is designed to handle.
AWAITING USER APPROVAL before any build work starts.

### Best Next Work

Do not try to promote another numerical near-pass. The next useful work must
either find or derive a theorem-level artifact that can fill the missing
contract fields, or remove a physical blocker on the VO-7 branch.

USER DIRECTIVE (2026-07-01, standing): GO BEYOND THE LITERATURE. Do not
only audit sources - attempt to solve the outstanding problems directly:
formulate candidate solutions as target-blind hypotheses, implement them as
fail-closed experiment phases, and let the computation decide. Named
candidate experiments (each a legitimate internal computation, no new
sources required):

- FERMION-LOOP EFFECTIVE POTENTIAL ON THE DOUBLET BLOCK: DONE by
  Phase428 with a decisive NEGATIVE: the one-loop determinant on constant
  rank-1 rays is an adjoint-orbit class function, exactly
  triplet/doublet-degenerate, with no quartic stabilizer - the mechanism
  class is closed; only su(3)-breaking fermionic structure (not
  source-defined) could evade it.
- FULL ONE-LOOP COLEMAN-WEINBERG PROGRAM (named 2026-07-01 after the
  Phase430 prototype; THE current best physics opportunity, four staged
  experiments):
  (430a) NET ONE-LOOP RAY SLOPE: the fermion-only mixed-configuration
  prototype showed minima always escape along the rank-1 rays (the
  bosonic tree quartic vanishes there exactly, so the fermionic -N_F log t
  descent is unopposed). But the BOSONIC fluctuation determinant enters
  with the opposite sign: transverse masses grow like t^2 ||[u,v]||^2, so
  the ray-direction one-loop slope is (N_B - N_F) log t with both counts
  exact orbit/mode-count data. Toy counts: ~+192 bosonic vs -128
  fundamental-fermion modes -> net CONFINING (+64); adjoint fermions
  (-384) -> runaway. THE SIGN IS A COMPUTABLE FUNCTION OF THE MATTER
  CONTENT, and the matter content is the blind Phase404 derivation -
  compute slope(content) exactly, record whether the DERIVED content
  confines.
  (430b) If confined: full one-loop (bosonic + fermionic) landscape on
  mixed two-direction configurations (pair types TT/TD/DD/TS/DS have
  genuinely different joint orbits - the Phase428 class-function no-go
  does NOT apply) -> do interior minima form, which pair types are
  deepest, does the deepest carry doublet content, and what dimensionless
  scale ratio does the log/quartic balance generate (dimensional
  transmutation)?
  (431) WELDED-FERMION LOOPS: the chimeric weld (Phase408,
  source-pinned) is fermionic structure BEYOND an su(3)-invariant sector
  - test whether the weld itself supplies the su(3)-breaking that
  Phase428 proved is required, without inventing anything.
  (432) BLIND BETA-COEFFICIENT LEDGER: one-loop beta coefficients from
  the Phase404-derived family content are pure group theory (no scales);
  extend the Phase429 ledger to the one-parameter running family
  sin^2(mu) - the strongest legal step toward a future comparison phase.
  (433) CONDITIONAL EXTRACTION ROWS: derive the photon/W/Z projection
  rows as FUNCTIONS of a candidate VEV direction (candidate-only),
  converting the Phase419 template to conditionally-computed so that a
  dynamical VEV from 430b would mechanically complete it.
- TARGET-BLIND DIMENSIONLESS-RATIO LEDGER: DONE by Phase429: three rows
  fixed blind (tan^2 = 3/5, sin^2 = 3/8, GUT ratio 1), the tree mass
  ratio conditional on the missing breaking sector, six comparison-
  lineage fields per row with zero source-defined. A future comparison
  phase must import that lineage explicitly and face the existing gates.
- PERFORMANCE PROGRAM (validation wall-time bounds innovation pace):
  Release builds everywhere (DONE 2026-07-01); single shared build pass
  (DONE 2026-07-02, BosonPhasesTraversal.proj, ~9%); content-hash
  incremental validation DESIGNED 2026-07-09 (see PERFORMANCE
  EXPLORATION OUTCOME above; ~100x on typical checkpoints; NOT yet
  implemented); dependency-DAG parallel generator DEPRIORITIZED (the
  graph is near-totally-ordered - measured 2026-07-09 - so there is
  almost no exploitable width); CUDA ruled NOT NOW 2026-07-09 (fp64
  1/64-rate on the resident Ada card; native lib not actually built
  against CUDA; revisit conditions recorded above). Any adopted change
  must preserve the sequential semantics of dependent phases and be
  validated by a full pass before adoption.

USER DIRECTIVE (2026-06-11): COMPLETED. The three brute-force
computations are done and committed: Phase404 (ratio menu tan^2 = 3/5,
family pattern derived, adjoint Higgs-doublet excluded), Phase405
(vacuum manifold permits but does not select doublet VEVs; native GPU
curvature kernel real-mesh defect found and characterized - platform
follow-up), Phase406 (falsification map: survivors are exactly
non-adjoint x larger-algebra; binding gaps choice-independent). The loop
now resumes the standing research program below.

Historical note: the old Phase405 design is complete. Its outcome is the
current vacuum-manifold boundary: rank-1 doublet VEV directions are permitted
and exactly flat, but the bare Upsilon = 0 landscape does not select them; the
GPU parity issue discovered during that work was a platform lifecycle defect
and has been fixed/regression-tested.

The most useful next branches are:

1. Mathematical hypothesis-test branches against the remaining gaps. These are
   NOT promotion routes by themselves; treat them as target-blind experiments
   that can find a concrete candidate structure or rule out broad ansatz
   families. Each should be a new fail-closed phase, update the journal, and
   leave Phase201/Phase256 untouched unless every contract field is actually
   filled. Recommended order:
   - Scale sanity checks: DONE at the naive curvature-mass level by Phase420.
     The Superphysics/draft stylized `m = R/4` clue cannot be used directly:
     the literal scalar-curvature mass reading is dimensionally invalid, and
     the `m^2 = R/4` repair is only a symbolic one-scale shell with nine
     missing specification fields. Further progress requires a source-defined
     curvature value/equation, sign, coefficient normalization, VEV map,
     weak-angle/coupling lineage, photon/W/Z/H rows, pole extraction, and
     GeV/unit normalization.
   - Observed-field extraction template: DONE at symbolic-template level by
     Phase419. The FMS/dressing-style map covers all 20 Phase256 fields and
     states photon/W/Z/H projection rows, weak-angle lineage, and pole
     extraction requirements, but it supplies zero source-defined fields. Further
     progress requires a GU-native carrier, observed vacuum/dressing theorem,
     embedding, couplings, mass operator, scale, and unit lineage.
   - Direction-dependent curvature/VEV coupling scan: DONE at the minimal
   block-isotypic workbench level by Phase418. Phase410 closed only the
   uniform bosonic curvature coupling; Phase418 shows pure block-weighted
   quadratics still run away, while stabilized block laws can select the
   doublet only by adding a non-source-defined block mass law and quartic
   stabilizer. Further progress requires a source-defined curvature kernel,
   stabilizer, sign/scale, and observed-field extraction.
   - Vector-spinor `144` decomposition / composite probe: CLOSED through
     bilinear order by Phases417, 422, and 424. Phase417 shows the
     source-pinned `Z_{1/2}` carrier's `144` has exact kernel dimension, a
     6-complex-dim fermionic SM-Higgs-pattern block (corrected 2026-07-01),
     and no linear welded scalar. Phase422 closes the mixed-chirality
     Dirac-like bilinear scalar channel (`Z_L x Z_R = 0`). Phase424 then
     decides the deferred same-chirality question: the 528-dim
     Majorana-like welded-scalar sector contains NO SM-doublet (ambient
     intersection zero in both channels, top Gram eigenvalue 0.059259).
     Further progress from this branch requires a genuinely new
     source-defined bosonic projection map, action, or VEV selection rule -
     no internal decomposition question remains at bilinear order.
   - Cross-carrier bilinear completion: DONE by Phase425 on the FULL
     source-pinned carrier menu. The Q_{3/2} carrier is constructed exactly
     (6 = gamma-traceless remainder in 4 x 2 = 2 + 6, welded content
     (1/2,1)/(1,1/2), no linear welded scalar); all mixed-parity channels
     are exactly zero-capacity; all same-parity channels (Z x S, Q x Q,
     Q x S, Q x Z) carry no welded-scalar SM-doublet by ambient
     intersection; mirror channels transfer by representation identity.
     THE BILINEAR COMPOSITE LAYER IS CLOSED ON EVERY SOURCE-PINNED CARRIER
     PAIR. Remaining composite territory is quartic+ on the new carriers -
     pursue only if a source names a specific composite.
   - Unobserved-phase carrier census: DONE at source-pinned linear-carrier
     level by Phase416. The current sources define dark fermionic carriers but
     no linear bosonic spin-zero SM-doublet carrier or projection map.
   - Fermionic cohomology square-root ansatz: DONE by Phase415 on the
     currently specifiable carriers. `part-09b` section 9.3 is useful as a
     research clue, but no reviewed source supplies the `delta_omega`
     differential, carrier complex, cohomology target, square law, projection
     rows, or normalization lineage. Further progress requires a new
     source-level `delta_omega` specification rather than another local
     near-pass.
   - General Shiab/epsilon operator ansatz: DONE by Phase414 on the currently
     computable carriers. The low-order alphabet is covered, 11 families are
     closed by Phases408-413, and the 2 remaining residuals require a new
     carrier/source specification rather than another internal near-pass.
2. The physical VO-6/VO-7 derivation against the Phase398 8-item gap
   ledger, headed by the symmetry-breaking scalar/VEV sector (welded to
   photon/Z mixing by Phase397) and the hypercharge/coupling lineage. Each
   component solved should arrive through a new fail-closed phase that
   names the Phase398 ledger row it discharges, proves target independence,
   and lets the existing gates decide promotion. The scalar-route audit
   against the primary draft is DONE (Phase402): the draft's eq. 12.28
   dictionary pins the Higgs potential to <U,U> (= the repo's Mode-B
   objective) and Higgs KG to D*U=0; symmetry breaking must come from the
   Upsilon = 0 locus geometry; the scalar-sector row is now three named
   sub-gaps: (a) a doublet-equivalent substructure inside the pulled-back
   connection component (the adjoint-triplet pattern cannot produce the
   SM neutral sector - machine-verified discriminator), (b) the
   vacuum-manifold breaking geometry on a physical branch, (c) the
   quantitative chain (VEV scale/pole/GeV - zero anchors exist in the
   primary). The doublet-substructure probe is DONE (Phase403): doublet
   blocks exist generically in larger adjoints with the custodial pattern
   exact and the coupling ratio EMBEDDING-DERIVED (tan^2 = 3 for the
   su(3) toy, recorded blind) - the ratio-lineage mechanism is
   identified. The GU-specific chain enumeration is DONE (Phase404:
   tan^2 = 3/5, family pattern derived, adjoint Higgs-doublet excluded),
   and Phase407 found the SM-Higgs quantum numbers in the chimeric
   frame-cross-internal block (spacetime-vector-valued). The spin-0
   extraction is now CHARACTERIZED (Phase408): the naive vertical-trace
   route is OBSTRUCTED (trace slot 1-dim vs doublet 4-dim; weld
   entangles spin/isospin with trivial centralizer) - the draft's
   epsilon-conjugation/Shiab machinery or unobserved-phase fields is the
   precisely named open requirement, unspecified quantitatively in the
   primary. (The platform follow-up - the Phase405 GPU parity defect -
   was root-caused and FIXED 2026-06-12: a GpuSolverBackend.Initialize
   lifecycle bug, not the native kernel; real-mesh parity tests now
   guard it.) The invariant-pairing-menu probe is DONE (Phase409): the
   epsilon route on frame-cross content is CLOSED through bilinear
   order, all odd orders are closed exactly, and the menu is
   machine-enumerated (no parity-odd fiber pairing exists). The
   curvature-coupling probe is DONE (Phase410): the uniform bosonic
   realization of the curvature-coaxing claim produces RUNAWAY along the
   exactly-flat vacuum rays and a block-degenerate deepest stratum on
   the lifted sector - no doublet selection; the mechanism requires
   direction-dependent coupling or the Dirac-sector realization. The
   Dirac-sector composite probe is DONE (Phase411): the Dirac mass
   channel carries NO welded scalar at all; the Majorana channels'
   welded scalars carry no SM doublet - the composite-extraction
   question is CLOSED through bilinear order on every probed carrier
   (frame-cross tensor, bosonic vacuum, spinor bilinear). The quartic
   order is DONE (Phase412, user directive): the ambient intersection
   is ZERO in every channel with decisive margins - THE COMPOSITE
   PROGRAM IS CLOSED THROUGH QUARTIC ORDER, all statistics projections
   covered. The noncompact loophole is DONE (Phase413): the
   complexified welds coincide exactly, so the no-gos are REAL-FORM
   INDEPENDENT - the noncompact evasion is closed on finite-dimensional
   carriers (induced signature (7,3), matching the Phase406 Cl(7,3)
   axis). Phase414 and Phase415 then closed the low-order Shiab/epsilon
   and `delta_omega` square-root branches on currently specifiable
   carriers. Phase416 pinned the draft's unobserved/dark carrier census:
   `Q_{3/2}`, `Z_{1/2}` with vector-spinor `144`, and the dark mirror
   Weyl half are source-defined fermionic carriers, but none is a linear
   bosonic spin-zero SM-doublet carrier and no bosonic map/action is
   supplied. Phase417 then decomposed the `Z` vector-spinor `144` itself:
   the gamma-trace split is exact, the internal SM-Higgs-pattern census
   holds a 6-complex-dim fermionic block (corrected 2026-07-01 after the
   |Y|=1/2 calibration defect fix; originally misrecorded as zero), and the
   chiral `2 x 144` carrier has no linear welded scalar.
   Phase422 then characterized the bilinear boundary: the mixed-chirality
   Dirac-like `Z_L x Z_R` scalar channel is closed, while same-chirality
   Majorana-like channels have 528 welded-scalar capacity. Phase424 then
   DECIDED that deferred sector: the ambient intersection of the SM-doublet
   isotypic with the welded-scalar isotypic is ZERO in both same-chirality
   channels (top Gram eigenvalue 0.059259 vs required 1.0) - the
   vector-spinor `144` bilinear composite route is CLOSED, and the corrected
   Phase411 Majorana verdict is confirmed by the same stronger method.
   Phase418 then showed that direction-dependent block laws can select the
   doublet only after adding a non-source-defined block mass law and quartic
   stabilizer; pure block-weighted quadratics still run away (and the
   2026-07-01 Phase410 pair-table post-processing shows the action's own
   quartic cannot supply that stabilizer: TT=2.25 uniform vs DD avg 1.125,
   block sums exactly symmetric - the external requirement is genuine).
   Phase419 then
   materialized the FMS/dressing-style observed-field projection template:
   photon, W, Z, and Higgs rows are now explicit symbolic requirements, but no
   source-defined Phase256 field is supplied. Phase420 then closed the naive
   curvature-scale shortcut: literal `m=R/4` is dimensionally invalid for
   scalar curvature, and the repaired squared-mass reading supplies no unit,
   VEV, particle-row, pole, or GeV lineage. THE INTERNAL
   STRUCTURAL PROGRAM IS AT ITS HONEST BOUNDARY, NOW WITH NAMED ROUTES:
   a source-defined vector-spinor/composite bosonic projection map (the
   same-chirality scalar-sector decomposition itself is DONE and negative,
   Phase424), a
   source-defined direction-dependent curvature kernel with stabilizer and
   scale, a source-defined observed-field extraction theorem filling the
   Phase419 template, a source-defined curvature-to-electroweak scale law
   satisfying Phase420's missing specification fields, or a new primary-source
   specification. The census-level internal candidates named after Phase424
   (the Q_{3/2} carrier probe and the cross-carrier bilinear consolidation)
   are BOTH DONE by Phase425 - the bilinear layer is closed on every
   source-pinned carrier pair. Standing work:
   literature monitoring at checkpoint
   cadence; the epsilon/Shiab route if a quantitative specification
   appears.
   Deep-research follow-ups: GU IV (v2) "The Rig for Lambda" is DONE by
   Phase421 (non-promotional); the GU-RVG June 2026 spinorial dark-sector
   delta is DONE by Phase423 (non-promotional); the 2026-07-01 sweep
   RESOLVED the remaining named targets - "Kleis" = kleis.io (a Z3/SMT
   verification platform, no GU content; lead closed), the Hebrew
   University talk has no artifact (UCSD April 2025 proxy is qualitative
   only), and TWO NEW-LEAD records are named for short fail-closed source
   audits at the next checkpoint: the Cox "Geometric Unity V" series is
   DONE by Phase426 (all five records audited; GU II corroborates the
   Phase404 tan^2 = 3/5 lineage and the (1,2,2) doublet-channel shape but
   supplies no potential/VEV/spectrum/scale/fit); the Hofseth GU-RVG
   successor (10.5281/zenodo.21056575; observationally-fixed condensate
   amplitude; likely fabricated co-authorship per arXiv:2606.02184)
   is DONE by Phase427: the original record was deleted 2026-07-01
   (tombstone "duplicate"); the live successor 21117379 imports
   v = 246 GeV and fixes its condensate amplitude by observation -
   non-promotional. Both sweep leads are discharged; monitoring returns
   to checkpoint cadence.
3. (CLOSED by Phase399 + Phase400 + Phase401: the quadratic-model coupled
   critical point is solved modulo flat directions, every flat ray is
   quartically lifted, and the attempted construction of the relaxed
   critical point machine-characterized the kernel relaxation as
   NON-PERTURBATIVE - positive-relaxed near-null valleys of the quartic
   form (anisotropy 1.4e8) plus adiabatic source growth carry the
   relaxation out of every trust region. No internal question remains for
   this component; the "4D observed vacuum" gap-ledger row now carries the
   Phase401 boundary evidence.)
4. Periodic external literature monitoring at checkpoint cadence (the
   2026-06-10 survey and the 2026-06-11 post-Phase400 sweep found no
   GU-native scalar-sector source; the 2026-06-11 sweep catalogued
   arXiv:2503.14578, G2-RICCI-FLOW-TORSION-EWSB, which imports the 246 GeV
   scale and weak angle; the 2026-06-16 expanded Superphysics GU draft mirror
   pass reviewed all twenty-five RSS-listed pages and found no fillable
   epsilon/Shiab doublet map, curvature-to-electroweak-VEV law, observed
   photon/W/Z/H projection rows, weak-angle running, pole extraction, or GeV
   normalization; Phase420 then closed the naive `m=R/4` scale shortcut at the
   dimensional/bookkeeping level; Phase421 then audited GU IV v2's LambdaCDM
   rig full text and found guardrails/hooks but no electroweak source rows,
   observed-field theorem, scale law, pole extraction, or GeV normalization;
   Phase423 then audited the June 2026 Zenodo GU-RVG spinorial dark-sector
   article and supplement, finding current spinorial/95 GeV/Koide context but
   no W/Z/H source rows, no observed projection, no vector-spinor projection
   map, and no unit/pole lineage - the negative boundary stands; the internal
   toy-branch construction otherwise reached its honest limit).

If a source or new derivation appears to satisfy any of these, create a new
fail-closed phase rather than editing Phase201/Phase256 directly. The phase
should prove target independence, check every required contract field, and then
let the existing gates decide whether promotion is allowed.

### VALIDATION STATE AT HANDOFF (2026-07-02, read first)

The Phase441 checkpoint was committed WITHOUT a completed full generator
pass: the first full pass failed on a newly-discovered EIGHTH scanner
(phase253, global observed-sector vacuum scan - it regexes `dimX >= 4`
combined with vacuum/VEV/Hessian terms and classifies studies|reports
.md/.json as production artifacts; the Phase441 frontier-statement text
tripped it, cascading 253->254->255 into ~114 failed checklist items).
The fix (phase441 study dir registered in phase253's
excludedPathFragments with a dated justification) was applied and
verified: phase253/254/255 rerun clean individually, and the direct gate
chain (phase101 -> phase202 -> integrity) reports 234 passed / 3 failed
with promotedPhysicalMassClaimCount=0. The RERUN full pass was then
CANCELLED at user request mid-flight. FIRST ACTION ON RESUME: run
`./scripts/generate_validated_boson_predictions.sh` from scratch and
confirm it ends at `boson-claim-integrity-verified` before any further
science work. Note the killed pass left some untracked phase outputs
partially regenerated (harmless - the fresh pass regenerates all).

### Start-Up Checklist

Run these first:

```bash
git status --short
git log -3 --oneline
tail -160 docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md
rg -n "Phase427|hofsethGuRvgSuperluminalSourceAudit|Phase426|coxGuSeriesBosonContractAudit|Phase425|zenodo.20531776|zenodo.21117379" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  docs/Reference/ExperimentReferences/GU-DRAFT-2021.md \
  docs/Reference/ExperimentReferences/COX-GU-SERIES-I-V-202606.md \
  studies/phase426_cox_gu_series_boson_contract_audit_001 \
  studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001 \
  studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase443 output JSON files, and commit a checkpoint
after validation. REMEMBER: new phases must be registered in BOTH the
generator script AND scripts/BosonPhasesTraversal.proj, and in ALL EIGHT
scanners (including phase253's dimension-4 exclusions).

Suggested checkpoint message:

```text
Add phase445 RG-improved potential probe (scheme-dependent candidate)
```
