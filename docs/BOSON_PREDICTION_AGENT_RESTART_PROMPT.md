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

### Current Scientific Status

No successful physical W/Z/H prediction has been achieved. The current package
still blocks physical comparison because the source-lineage and observed-field
contracts are empty.

Current gate status after the Phase412 work (and the 2026-06-12
platform fix - GPU parity defect root-caused and discharged):

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=205`,
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
  IA-5 throughout)
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

### Most Recent Implemented Work

The latest work added Phase412 (user directive), the quartic
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

Phase412 (like Phase388-411) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `quartic-sm-doublet-intersection-analysis-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/LOCAL-COMPLETION-V29-FERMIONIC-YUKAWA.md`
(Phase399/Phase400/Phase401 coupled-stationarity closure section).
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase412_quartic_sm_doublet_intersection_analysis_001/Phase412QuarticSmDoubletIntersectionAnalysis.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The targeted Phase412 run passes all exactness checks; Phase202 now
reports `checklistPassedCount=205`, `checklistFailedCount=3`; claim
integrity verified with `promotedPhysicalMassClaimCount=0`. (Platform
state: Gu.Interop.Tests 158/158 with the real-mesh parity and
buffer-handle recycling tests; both Phase405 platform notes discharged
2026-06-12.) All seven broad scanners still report zero intake-ready
evidence.

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
- `TOE-GU-ICEBERG-20250423.md` (+ full transcript under
  `docs/Reference/ExperimentReferences/transcripts/`): the GU-native
  structural ansatz for the scalar/Yukawa gap rows - qualitative only;
  cite GU-DRAFT-2021 as primary

### Best Next Work

Do not try to promote another numerical near-pass. The next useful work must
either find or derive a theorem-level artifact that can fill the missing
contract fields, or remove a physical blocker on the VO-7 branch.

USER DIRECTIVE (2026-06-11): COMPLETED. The three brute-force
computations are done and committed: Phase404 (ratio menu tan^2 = 3/5,
family pattern derived, adjoint Higgs-doublet excluded), Phase405
(vacuum manifold permits but does not select doublet VEVs; native GPU
curvature kernel real-mesh defect found and characterized - platform
follow-up), Phase406 (falsification map: survivors are exactly
non-adjoint x larger-algebra; binding gaps choice-independent). The loop
now resumes the standing research program below.

Phase405 design (ready to implement): su(3)-valued connections (the
minimal doublet-bearing algebra per Phase403) on a LARGER structured
fiber-bundle mesh (rows=cols=6+ for a carrier in the thousands);
question: does the Upsilon = 0 vacuum manifold's local structure permit/
select doublet-block VEV directions? (1) GN Hessian spectrum at the
trivial vacuum, kernel/soft directions classified by the su(2)xu(1)
block decomposition (triplet/doublet/singlet); (2) brute-force orbit
scan: S_B evaluated exactly over a dense sample of doublet-block VEV
directions x magnitudes, landscape mapped in gauge-invariant orbit
coordinates; (3) GPU: Gu.Interop.GpuSolverBackend implements
ISolverBackend with algebra-generic EvaluateDerived (curvature/torsion/
Shiab/residual) - run the thousands of independent evaluations on the
GPU (CudaNativeBackend; native lib verified) with CPU-parity gates on a
subsample and recorded speedup; check whether BuildJacobian/
ComputeGradient are GPU-implemented (header comment may be stale - Phase
I GAP-4/8 added CUDA Stage 2/3 Jacobian/Krylov) and fall back to CPU for
those pieces if not, honestly recorded.

The most useful next branches are:

1. The physical VO-6/VO-7 derivation against the Phase398 8-item gap
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
   covered. THE INTERNAL STRUCTURAL PROGRAM IS BACK AT ITS HONEST
   BOUNDARY. Standing work: literature monitoring at checkpoint
   cadence; the epsilon/Shiab route if a quantitative specification
   appears. Optional internal follow-ups, in priority order, IF the
   loop has idle capacity: (i) a noncompact real-form spot-check of
   the Phase408/409/411/412 no-gos (the single most plausible loophole
   per DEEP-RESEARCH-20260612), (ii) the unobserved-phase sector IF
   the draft's Chapter 14 machinery can be pinned to a computable
   structure (re-read the draft text first).
   Deep-research follow-ups (catalogue when revisited): GU IV (v2)
   "The Rig for Lambda" (DOI 10.5281/zenodo.17402261); the hinted
   "Geometric Unity V"; the Hebrew University dark-energy talk
   artifact; the unverified "Kleis" machine-audit lead.
2. (CLOSED by Phase399 + Phase400 + Phase401: the quadratic-model coupled
   critical point is solved modulo flat directions, every flat ray is
   quartically lifted, and the attempted construction of the relaxed
   critical point machine-characterized the kernel relaxation as
   NON-PERTURBATIVE - positive-relaxed near-null valleys of the quartic
   form (anisotropy 1.4e8) plus adiabatic source growth carry the
   relaxation out of every trust region. No internal question remains for
   this component; the "4D observed vacuum" gap-ledger row now carries the
   Phase401 boundary evidence.)
3. Periodic external literature monitoring at checkpoint cadence (the
   2026-06-10 survey and the 2026-06-11 post-Phase400 sweep found no
   GU-native scalar-sector source; the 2026-06-11 sweep catalogued
   arXiv:2503.14578, G2-RICCI-FLOW-TORSION-EWSB, which imports the 246 GeV
   scale and weak angle - the negative boundary stands; the internal
   toy-branch construction otherwise reached its honest limit).

If a source or new derivation appears to satisfy any of these, create a new
fail-closed phase rather than editing Phase201/Phase256 directly. The phase
should prove target independence, check every required contract field, and then
let the existing gates decide whether promotion is allowed.

### Start-Up Checklist

Run these first:

```bash
git status --short
git log -3 --oneline
tail -120 docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md
rg -n "Phase412|quarticSmDoubletIntersectionAnalysis|quarticWeldedScalarSmDoubletAbsentAllChannels" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  studies/phase412_quartic_sm_doublet_intersection_analysis_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase412 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase412 quartic sm doublet intersection analysis
```
