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

Current gate status after the Phase408 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=201`,
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
  sharpened), `gpuParityDefectDetected=True` (native curvature kernel
  linear-part defect on real mesh topology - platform follow-up; science
  on CPU per IA-5)
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

The latest work added Phase408, the vertical spin-zero extraction
obstruction probe. The chimeric weld (internal 10 = metrics fiber
Sym^2(R^4)) was formalized exactly (homomorphism residual 2.2e-16) and
three consequences machine-verified: (V1) the weld ENTANGLES spin and
isospin - pi(so(4)) commutes with no SM generator; (V2) the centralizer
of pi(so(4)) in so(10) is TRIVIAL (Schur on the multiplicity-free
9 + 1); (V3) the spin-0 slot of the vertical 10 is exactly the 1-dim
trace direction, so the naive vertical-trace extraction CANNOT carry the
4-real-dim SM doublet for ANY weld alignment. Scalar-sector sub-gap (a)
reaches its final internal form: the Phase407 frame-cross doublet cannot
descend through the trace slot; the draft's epsilon-conjugation/Shiab
machinery or unobserved-phase fields (not specified quantitatively in
the primary text) is the precisely named requirement. THE INTERNAL
STRUCTURAL PROGRAM IS AT ITS HONEST BOUNDARY. Study:
`studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001`
(IMPLEMENTATION_P408.md). Before that, Phase407 found the SM-Higgs
quantum numbers in the chimeric frame-cross-internal block.

### Integration Points Already Updated

Phase408 (like Phase388-407) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `vertical-spin-zero-extraction-obstruction-probe-materialized`)
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
dotnet run --project studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/Phase408VerticalSpinZeroExtractionObstructionProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase408 line, the Phase202 incomplete
status (`checklistPassedCount=201`, `checklistFailedCount=3`), and the same
claim-integrity status (`promotedPhysicalMassClaimCount=0`). All seven broad
scanners still report zero intake-ready evidence.

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
   primary. THE INTERNAL STRUCTURAL PROGRAM IS AT ITS HONEST BOUNDARY:
   remaining internal candidates require new primary-source
   specifications or theorem-level sources; standing work is literature
   monitoring at checkpoint cadence, the epsilon/Shiab route if a
   specification appears, and the platform follow-up (native CUDA
   curvature kernel real-mesh fix + real-mesh parity tests).
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
rg -n "Phase408|verticalSpinZeroExtractionObstructionProbe|spinZeroSlotCannotCarryFullDoublet" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase408 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase408 vertical spin zero extraction obstruction probe
```
