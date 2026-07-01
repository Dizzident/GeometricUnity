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

Current gate status after the Phase425 work (plus the 2026-06-12 platform
fix - GPU parity defect root-caused and discharged - and the 2026-07-01
|Y|=1/2 calibration defect fix in the Phase411/417 informational SM
censuses):

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=218`,
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

The latest work (2026-07-01, second checkpoint of the session) added
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

Phase425 (like Phase388-424) is wired into:

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
  `cross-carrier-bilinear-sm-doublet-completion-audit-materialized`;
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
`checklistPassedCount=218`, `checklistFailedCount=3`; claim integrity
verifies Phase424/Phase425 (and the corrected Phase417 values) with
`promotedPhysicalMassClaimCount=0`. The full direct
`./scripts/generate_validated_boson_predictions.sh` pass completed with
Phase424 and Phase425 included and ended at `boson-claim-integrity-verified`. (Platform
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
   audits at the next checkpoint: the Cox "Geometric Unity V" series
   (10.5281/zenodo.20531776 with GU IV 20518853, GU III 20517502, GU I
   20550275; cosmology-only pre-screen) and the Hofseth GU-RVG successor
   (10.5281/zenodo.21056575; observationally-fixed condensate amplitude;
   likely fabricated co-authorship per arXiv:2606.02184). Both are
   expected non-promotional.
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

### Start-Up Checklist

Run these first:

```bash
git status --short
git log -3 --oneline
tail -160 docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md
rg -n "Phase425|crossCarrierBilinearSmDoubletCompletionAudit|Phase424|vectorSpinor144BilinearSmDoubletIntersection|yHalfCalibrationExact|majoranaYHalfCalibrationExact|zenodo.20531776|zenodo.21056575" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  docs/Reference/ExperimentReferences/GU-DRAFT-2021.md \
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
force-add the ignored Phase425 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase425 cross-carrier bilinear completion audit
```
