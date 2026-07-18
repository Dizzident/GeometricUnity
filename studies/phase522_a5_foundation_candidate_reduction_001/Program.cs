using System.Security.Cryptography;
using System.Text.Json;

const string PhaseDir = "studies/phase522_a5_foundation_candidate_reduction_001";
const string ContractPath = PhaseDir + "/preregistration/phase522_a5_foundation_candidate_reduction_contract_v1.json";
const string OutputDir = PhaseDir + "/output";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement precursors = contract.GetProperty("precursorBindings");
string[] bindingNames =
[
    "phase517Summary", "phase517Contract", "phase517Program",
    "phase518Summary", "phase518Contract", "phase518Program",
    "phase519Summary", "phase519Contract", "phase519Program",
    "phase520Summary", "phase520Contract", "phase520Program",
    "phase521Summary", "phase521Contract", "phase521Program",
];
BindingSpec[] specs = bindingNames.Select(name => Spec(ToKebab(name), precursors.GetProperty(name))).ToArray();

// Construction firewall: refuse before reading a precursor until every exact
// binding has been frozen into this audit-authored contract.
if (specs.Any(x => IsPlaceholder(x.ExpectedSha256)))
    throw new InvalidOperationException("Phase522 is a non-runnable template: exact Phase520/521 bindings remain placeholders.");

Binding[] bindings = specs.Select(Bind).ToArray();
string[] precedence = Strings(contract, "precedence");
string[] frozenPrecedence =
[
    "invalid-or-drifted-input",
    "action-member-or-omega-parity-unresolved",
    "no-total-reflection-compatible-candidate",
    "reflection-pullback-or-boundary-incomplete",
    "o1-o2-dependency-or-positive-time-algebra-unresolved",
    "normalized-measure-or-domain-incomplete",
    "candidate-target-equality-or-hermiticity-unproved",
    "finite-only",
    "necessity-gluing-or-all-scope-unproved",
    "candidate-package-ready-for-independent-mathematical-review",
];

bool contractValid = I(contract, "schemaVersion") == 1
    && S(contract, "contractId") == "phase522-a18-a5-foundation-candidate-reduction-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A18"
    && S(contract, "contractStatus") == "final-frozen-before-first-runnable-invocation"
    && B(contract, "frozenBeforePrecursorConsumption")
    && precedence.SequenceEqual(frozenPrecedence, StringComparer.Ordinal)
    && S(contract, "expectedCurrentVerdict") == "action-member-or-omega-parity-unresolved"
    && S(contract, "strongestPermittedTerminal") == "candidate-package-ready-for-independent-mathematical-review"
    && S(contract, "strongestPermittedClaim") == "independent-mathematical-review-readiness-only"
    && B(contract, "mayReduceSurvivorMenu")
    && !B(contract, "candidateRegistrationAllowed")
    && !B(contract, "phase515Or516CreationOrUnlockAllowed")
    && !B(contract, "theoremOrCounterexampleClaimAllowed")
    && !B(contract, "reflectionPositivityClaimAllowed")
    && !B(contract, "samplingReprocessingOrProductionAllowed")
    && !B(contract, "phase481OrPhase458WorkAllowed")
    && B(contract, "externalReviewPending")
    && !B(contract, "physicalClaimAllowed")
    && I(contract, "promotedPhysicalMassClaimCount") == 0;

JsonElement phase517 = ReadRoot(SpecById("phase517-summary").Path);
JsonElement phase518 = ReadRoot(SpecById("phase518-summary").Path);
JsonElement phase519 = ReadRoot(SpecById("phase519-summary").Path);
JsonElement phase520 = ReadRoot(SpecById("phase520-summary").Path);
JsonElement phase521 = ReadRoot(SpecById("phase521-summary").Path);
JsonElement phase520Contract = ReadRoot(SpecById("phase520-contract").Path);
JsonElement phase521Contract = ReadRoot(SpecById("phase521-contract").Path);
JsonElement phase520SerializationAudit = phase520.GetProperty("serializationAudit");
JsonElement phase520ActionMemberAudit = phase520.GetProperty("actionMemberAudit");
JsonElement phase520OmegaParityAudit = phase520.GetProperty("omegaParityAudit");
JsonElement phase521FrozenMenu = phase521.GetProperty("frozenMenu");
JsonElement[] phase521NegativeRows = Elements(phase521, "negativeControls");
JsonElement[] phase521CandidateRows = Elements(phase521, "auditAuthoredCandidates");
JsonElement[] phase521MeshCoordinateRows = Elements(phase521, "meshCoordinateControls");
JsonElement[] phase521ConstructionRows = Elements(phase521, "constructionControls");
JsonElement[] phase521ReflectionCellRows = Elements(phase521, "reflectionCellControls");
int[] frozenExtents = Ints(phase521FrozenMenu, "extentMenu");
string[] expectedNegativeKeys = ExpectedRowKeys("required-negative-control", "lattice-canonical-kuhn-negative-control", frozenExtents);
string[] expectedCandidateKeys = ExpectedRowKeys("audit-authored-candidate", "periodic-cubical-cell-poset-barycentric", frozenExtents);
string[] rowCertifiedPairs = phase521CandidateRows
    .GroupBy(x => JsonString(x, "geometryId") + "::" + JsonString(x, "reflectionId"), StringComparer.Ordinal)
    .Where(group => group.Select(x => x.GetProperty("extent").GetInt32()).Distinct().Order().SequenceEqual(frozenExtents)
        && group.All(CandidateRowValid))
    .Select(group => group.Key).Order(StringComparer.Ordinal).ToArray();

bool precursorSemanticsValid =
    JsonBool(phase517, "inputsValid") && JsonString(phase517, "verdictKind") == RequiredVerdict(precursors, "phase517Summary")
    && JsonBool(phase518, "inputsValid") && JsonString(phase518, "verdictKind") == RequiredVerdict(precursors, "phase518Summary")
    && JsonBool(phase519, "inputsValid") && JsonString(phase519, "verdictKind") == RequiredVerdict(precursors, "phase519Summary")
    && JsonBool(phase520, "inputsValid") && JsonBool(phase520, "contractValid")
    && JsonString(phase520, "verdictKind") == JsonString(precursors.GetProperty("phase520Summary"), "requiredVerdictKind")
    && JsonString(phase520, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A18"
    && JsonString(phase520Contract, "contractId") == "phase520-a18-action-subject-lineage-parity-v1"
    && JsonString(phase520Contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A18"
    && JsonBool(phase520SerializationAudit, "generatorContainsQualifier")
    && JsonBool(phase520SerializationAudit, "a5ResultFieldInventoryExact")
    && !JsonBool(phase520SerializationAudit, "dedicatedQualifierOrMemberFieldPresent")
    && I(phase520SerializationAudit, "committedSerializedStringValueCount") == 11
    && Elements(phase520SerializationAudit, "committedSerializedToyControlQualifierHits").Length == 0
    && !JsonBool(phase520SerializationAudit, "committedSerializedValuesPreserveToyControlQualifier")
    && JsonBool(phase520SerializationAudit, "generatorSerializesA5ResultDirectly")
    && JsonBool(phase520SerializationAudit, "frozenQualifierFieldAndValueAuditValid")
    && !JsonBool(phase520SerializationAudit, "genericSchemaNonRepresentabilityClaimed")
    && !JsonBool(phase520SerializationAudit, "serializationCauseBeyondSchemaRepresentabilityInferred")
    && !JsonBool(phase520SerializationAudit, "semanticApplicabilityResolved")
    && !JsonBool(phase520SerializationAudit, "serializationRepaired")
    && JsonBool(phase520ActionMemberAudit, "actionMemberInventoryComplete")
    && JsonString(phase520ActionMemberAudit, "auditMode") == "frozen-negative-current-evidence-census"
    && JsonBool(phase520ActionMemberAudit, "futureSelectedMemberArtifactRequiresNewContractAndImplementation")
    && !JsonBool(phase520ActionMemberAudit, "uniqueExecutableActionMemberSelected")
    && JsonBool(phase520OmegaParityAudit, "frozenInventoryComplete")
    && I(phase520OmegaParityAudit, "registeredEvidenceCount") == 2
    && !JsonBool(phase520OmegaParityAudit, "exactIdentityCancelsEveryOmegaOddContribution")
    && !JsonBool(phase520OmegaParityAudit, "exhaustiveMathematicalAbsenceClaimed")
    && JsonBool(phase521, "inputsValid") && JsonBool(phase521, "contractValid")
    && JsonString(phase521, "verdictKind") == JsonString(precursors.GetProperty("phase521Summary"), "requiredVerdictKind")
    && JsonString(phase521, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A18"
    && JsonString(phase521Contract, "contractId") == "phase521-a18-frozen-reflection-compatible-triangulation-census-v1"
    && JsonString(phase521Contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A18"
    && JsonBool(phase521, "finiteCandidateSurvives") && JsonBool(phase521, "candidatesRemainUnregistered")
    && JsonBool(phase521, "exactNegativeControlReproduced") && JsonBool(phase521, "negativeControlsReject")
    && JsonBool(phase521, "candidateIncidenceValid") && JsonBool(phase521, "exactIntegerCombinatorialControlsOnly")
    && JsonBool(phase521, "committedMeshCoordinatesValid")
    && JsonBool(phase521, "independentBarycentricFormulasValid")
    && JsonBool(phase521, "reflectionRepresentationControlsValid")
    && !JsonBool(phase521, "floatingPointGeometryOrToleranceUsedForClosure")
    && !JsonBool(phase521, "missingImagesSilentlyZeroFilled")
    && !JsonBool(phase521, "restrictedClosurePromotedToTotalClosure")
    && JsonBool(phase521FrozenMenu, "menuFrozenBeforeScoring")
    && !JsonBool(phase521FrozenMenu, "physicalTargetsConsultedForConstruction")
    && !JsonBool(phase521FrozenMenu, "resultBasedTuningPerformed")
    && Strings(phase521FrozenMenu, "geometryIds").SequenceEqual(new[]
        { "lattice-canonical-kuhn-negative-control", "periodic-cubical-cell-poset-barycentric" }, StringComparer.Ordinal)
    && frozenExtents.SequenceEqual(new[] { 3, 4 })
    && Strings(phase521FrozenMenu, "reflectionIds").SequenceEqual(new[]
        { "site-centered-axis0", "link-centered-axis0" }, StringComparer.Ordinal)
    && phase521NegativeRows.Length == 4 && phase521NegativeRows.All(NegativeControlRowValid)
    && phase521NegativeRows.Select(RowKey).Order(StringComparer.Ordinal).SequenceEqual(expectedNegativeKeys, StringComparer.Ordinal)
    && phase521CandidateRows.Length == 4 && phase521CandidateRows.All(CandidateRowValid)
    && phase521CandidateRows.All(x => CandidateConstructionCrossLinked(x, phase521ConstructionRows))
    && phase521CandidateRows.Select(RowKey).Order(StringComparer.Ordinal).SequenceEqual(expectedCandidateKeys, StringComparer.Ordinal)
    && phase521MeshCoordinateRows.Length == 2
    && phase521MeshCoordinateRows.Select(x => x.GetProperty("extent").GetInt32()).Order().SequenceEqual(frozenExtents)
    && phase521MeshCoordinateRows.All(x => JsonBool(x, "finiteIntegralInRangeUniqueCoordinates"))
    && phase521ConstructionRows.Length == 2 && phase521ConstructionRows.All(ConstructionRowValid)
    && phase521ConstructionRows.Select(x => x.GetProperty("extent").GetInt32()).Order().SequenceEqual(frozenExtents)
    && phase521ReflectionCellRows.Length == 4
    && phase521ReflectionCellRows.All(x => ReflectionCellRowValid(x, phase521ConstructionRows))
    && phase521ReflectionCellRows.Select(ReflectionCellKey).Order(StringComparer.Ordinal).SequenceEqual(
        expectedCandidateKeys.Select(x => x[(x.IndexOf("::", StringComparison.Ordinal) + 2)..]).Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && Strings(phase521, "survivingGeometryIds").Order(StringComparer.Ordinal).SequenceEqual(
        rowCertifiedPairs.Select(PairGeometry).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && Strings(phase521, "survivingReflectionIds").Order(StringComparer.Ordinal).SequenceEqual(
        rowCertifiedPairs.Select(PairReflection).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && !JsonBool(phase521, "finiteSurvivalSupportsAllVolumeInference")
    && !JsonBool(phase521, "allVolumeEmbeddingTheoremEstablished")
    && !JsonBool(phase521, "orientationTheoremEstablished")
    && !JsonBool(phase521, "currentCommittedMeshMutated")
    && !JsonBool(phase521, "candidateSelectedForProduction")
    && !JsonBool(phase521, "candidateRegistrationPerformed")
    && JsonBool(phase520, "externalReviewPending") && JsonBool(phase521, "externalReviewPending")
    && I(phase520, "promotedPhysicalMassClaimCount") == 0 && I(phase521, "promotedPhysicalMassClaimCount") == 0;
bool invalidOrDrifted = !contractValid || bindings.Any(x => !x.HashMatches) || !precursorSemanticsValid;

// These accessors are deliberately tied to the committed Phase520/521 output
// schema. A missing or renamed field is input drift, never evidence of closure.
bool actionMemberResolved = JsonBool(phase520ActionMemberAudit, "uniqueExecutableActionMemberSelected");
bool omegaParityResolved = JsonBool(phase520OmegaParityAudit, "exactIdentityCancelsEveryOmegaOddContribution");
bool actionMemberOrOmegaParityUnresolved = !actionMemberResolved || !omegaParityResolved;

int frozenGeometryReflectionPairCount = Strings(phase521FrozenMenu, "geometryIds").Length
    * Strings(phase521FrozenMenu, "reflectionIds").Length;
string[] totalCompatibleCandidates = rowCertifiedPairs;
bool noTotalReflectionCompatibleCandidate = totalCompatibleCandidates.Length == 0;
bool candidateSpecificReflectionPullbackBoundaryCertificatesBound = false;
bool reflectionPullbackOrBoundaryIncomplete = totalCompatibleCandidates.Length > 0
    && (!JsonBool(phase521, "boundaryOrPullbackRulesRegistered")
        || !candidateSpecificReflectionPullbackBoundaryCertificatesBound);
bool candidateSpecificHermiticityCertificateBound = false;
bool candidateSpecificNecessityAndGluingCertificatesBound = false;
bool o1O2DependencyOrPositiveTimeAlgebraUnresolved = !JsonBool(phase521, "actionOrObservableEvaluated");
bool normalizedMeasureOrDomainIncomplete = !JsonBool(phase521, "normalizedMeasureEvaluated");
bool candidateTargetEqualityOrHermiticityUnproved = !JsonBool(phase521, "candidateToTargetEqualityEstablished")
    || !candidateSpecificHermiticityCertificateBound;
bool finiteOnly = !JsonBool(phase521, "finiteSurvivalSupportsAllVolumeInference");
bool necessityGluingOrAllScopeUnproved = !candidateSpecificNecessityAndGluingCertificatesBound
    || !JsonBool(phase521, "allVolumeEmbeddingTheoremEstablished")
    || !JsonBool(phase521, "orientationTheoremEstablished");

bool[] blockers =
[
    invalidOrDrifted,
    actionMemberOrOmegaParityUnresolved,
    noTotalReflectionCompatibleCandidate,
    reflectionPullbackOrBoundaryIncomplete,
    o1O2DependencyOrPositiveTimeAlgebraUnresolved,
    normalizedMeasureOrDomainIncomplete,
    candidateTargetEqualityOrHermiticityUnproved,
    finiteOnly,
    necessityGluingOrAllScopeUnproved,
];
string verdict = Decide(blockers, precedence);
var precedenceBattery = Enumerable.Range(0, precedence.Length).Select(index =>
{
    bool[] synthetic = new bool[precedence.Length - 1];
    if (index < synthetic.Length) synthetic[index] = true;
    string actual = Decide(synthetic, precedence);
    return new { index, expected = precedence[index], actual, passed = actual == precedence[index] };
}).ToArray();

var result = new
{
    schemaVersion = 1,
    phase = 522,
    phaseId = "phase522-a5-foundation-candidate-reduction",
    terminalStatus = "a5-foundation-candidate-reduction-" + verdict,
    verdictKind = verdict,
    inputsValid = !invalidOrDrifted,
    contractValid,
    applicationSubjectKind = "a5-foundation-candidate-reduction-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A18",
    deterministicZeroSampling = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    contract = new { path = ContractPath, sha256 = Sha(ContractPath), frozenBeforePrecursorConsumption = true },
    exactInputBindings = bindings,
    precursorSemanticsValid,
    precedence,
    precedenceBattery,
    blockers = new
    {
        invalidOrDrifted,
        actionMemberOrOmegaParityUnresolved,
        noTotalReflectionCompatibleCandidate,
        reflectionPullbackOrBoundaryIncomplete,
        o1O2DependencyOrPositiveTimeAlgebraUnresolved,
        normalizedMeasureOrDomainIncomplete,
        candidateTargetEqualityOrHermiticityUnproved,
        finiteOnly,
        necessityGluingOrAllScopeUnproved,
    },
    candidateReduction = new
    {
        sourceMenuFrozenByPhase521 = true,
        totalCompatibleCandidates,
        survivorCount = totalCompatibleCandidates.Length,
        frozenGeometryReflectionPairCount,
        fullFrozenGeometryReflectionMenuReduced = frozenGeometryReflectionPairCount > totalCompatibleCandidates.Length,
        auditAuthoredCandidatePairCount = expectedCandidateKeys.Select(x => x[..x.LastIndexOf("::", StringComparison.Ordinal)])
            .Distinct(StringComparer.Ordinal).Count(),
        auditAuthoredCandidateMenuReduced = expectedCandidateKeys.Select(x => x[..x.LastIndexOf("::", StringComparison.Ordinal)])
            .Distinct(StringComparer.Ordinal).Count() > totalCompatibleCandidates.Length,
        candidateSelected = false,
        candidateRanked = false,
        candidateRegistered = false,
        candidateSpecificReflectionPullbackBoundaryCertificatesBound,
        candidateSpecificHermiticityCertificateBound,
        candidateSpecificNecessityAndGluingCertificatesBound,
        nextExactMissingCertificates = new[]
        {
            "unique executable A5 action-member lineage certificate",
            "exact identity eliminating every omega-sign-odd contribution",
            "complete reflection pullback and boundary rule for each survivor",
            "exact O1/O2 dependency and positive-time-algebra membership certificate",
            "normalized measure/domain, target-equality, and Hermiticity certificates",
            "necessity, shared-edge gluing, and all-coupling/all-volume certificates",
        },
    },
    strongestPermittedTerminal = "candidate-package-ready-for-independent-mathematical-review",
    strongestPermittedClaim = "independent-mathematical-review-readiness-only",
    candidatePackageIndependentMathematicalReviewReady = verdict == "candidate-package-ready-for-independent-mathematical-review",
    reviewReady = verdict == "candidate-package-ready-for-independent-mathematical-review",
    externalReviewPending = true,
    phase515Locked = true,
    phase516Locked = true,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    candidateDefinitionRegistered = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    phase458G1Satisfied = false,
    closesLimbL8 = false,
    phase458EvaluationPerformed = false,
    phase481PackWorkPerformed = false,
    precursorArtifactsMutated = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    launchAuthorized = false,
    binderLaunchAuthorized = false,
    accelerationAuthorized = false,
    o4Discharged = false,
    humanRulingPerformed = false,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    physicalUnitOrGevClaimMade = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a18BoundaryHeld = true,
    decision = verdict switch
    {
        "invalid-or-drifted-input" => "At least one exact precursor binding or required semantic field is invalid or drifted. No candidate is registered and nothing is authorized.",
        "candidate-package-ready-for-independent-mathematical-review" => "The frozen foundation blockers are cleared only to independent mathematical review readiness. This is not a registration, theorem, counterexample, or execution authority, and external review remains pending.",
        _ => "The package remains fail-closed at the earliest unresolved A18 foundation requirement. The survivor menu is diagnostic only; no candidate is registered and nothing is authorized.",
    },
};

Require(precedenceBattery.All(x => x.passed), "Phase522 precedence battery drifted.");
Require(result.contractValid, "Phase522 frozen contract validation drifted.");
Require(verdict == S(contract, "expectedCurrentVerdict"), "Phase522 expected current terminal drifted.");
Require(result.targetBlindConstruction && !result.physicalTargetsConsultedForConstruction
    && !result.reviewReady && result.externalReviewPending && result.phase515Locked && result.phase516Locked
    && !result.phase515MayBeCreated && !result.phase516MayBeCreated && !result.candidateDefinitionRegistered,
    "Phase522 target-blind, review, candidate-registration, or Phase515/516 lock firewall drifted.");
Require(!result.theoremClaimed && !result.targetCounterexampleClaimed
    && !result.reflectionPositivityEstablished && !result.reflectionPositivityRefuted
    && !result.phase458G1Satisfied && !result.closesLimbL8 && !result.phase458EvaluationPerformed
    && !result.phase481PackWorkPerformed && !result.samplingOrReprocessingRun && !result.hmcRun
    && !result.benchmarkRun && !result.productionAuthorized && !result.launchAuthorized
    && !result.binderLaunchAuthorized && !result.accelerationAuthorized && !result.o4Discharged
    && !result.humanRulingPerformed && !result.sourceContractApplicationAllowed
    && !result.routePromotesWzMasses && !result.routePromotesHiggsMass
    && !result.routeCompletesBosonPredictions && !result.physicalUnitOrGevClaimMade
    && result.noGevPromotion && result.promotedPhysicalMassClaimCount == 0,
    "Phase522 theorem, execution, route, review, or physical-claim firewall drifted.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options) + Environment.NewLine;
File.WriteAllText(Path.Combine(OutputDir, "a5_foundation_candidate_reduction.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_foundation_candidate_reduction_summary.json"), json);
Console.WriteLine(result.terminalStatus);

BindingSpec SpecById(string id) => specs.Single(x => x.Id == id);
static BindingSpec Spec(string id, JsonElement element) => new(id, JsonString(element, "path"), JsonString(element, "sha256"));
static Binding Bind(BindingSpec spec)
{
    string actual = File.Exists(spec.Path) ? Sha(spec.Path) : "missing";
    return new Binding(spec.Id, spec.Path, spec.ExpectedSha256, actual, actual == spec.ExpectedSha256);
}
static string Decide(bool[] blockers, string[] precedence)
{
    for (int i = 0; i < blockers.Length; i++)
        if (blockers[i]) return precedence[i];
    return precedence[^1];
}
static JsonElement ReadRoot(string path) => JsonDocument.Parse(File.ReadAllBytes(path)).RootElement.Clone();
static JsonElement[] Elements(JsonElement element, string name) =>
    element.GetProperty(name).EnumerateArray().Select(x => x.Clone()).ToArray();
static int[] Ints(JsonElement element, string name) =>
    element.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();
static bool CandidateRowValid(JsonElement row) =>
    JsonString(row, "role") == "audit-authored-candidate"
    && JsonBool(row, "vertexBijection") && JsonBool(row, "vertexInvolution")
    && JsonBool(row, "incidenceValid") && JsonBool(row, "reflectedIncidencePreserved")
    && JsonBool(row, "totalSimplexClosure")
    && row.GetProperty("firstMissingWitness").ValueKind == JsonValueKind.Null
    && row.GetProperty("vertexCount").GetInt32() == Ints(row, "simplexCounts")[0]
    && row.GetProperty("incidenceCheckCount").GetInt64() == Ints(row, "simplexCounts")
        .Select((count, dimension) => dimension == 0 ? 0L : (long)count * (dimension + 1)).Sum()
    && Ints(row, "mappedSimplexCounts").SequenceEqual(Ints(row, "simplexCounts"))
    && Ints(row, "restrictedTwiceIdentityCounts").SequenceEqual(Ints(row, "simplexCounts"));
static bool NegativeControlRowValid(JsonElement row) =>
    JsonString(row, "role") == "required-negative-control"
    && JsonBool(row, "vertexBijection") && JsonBool(row, "vertexInvolution")
    && JsonBool(row, "incidenceValid") && !JsonBool(row, "reflectedIncidencePreserved")
    && !JsonBool(row, "totalSimplexClosure")
    && row.GetProperty("firstMissingWitness").ValueKind == JsonValueKind.Object
    && Ints(row, "simplexCounts").SequenceEqual(ExpectedNegativeSimplexCounts(row.GetProperty("extent").GetInt32()))
    && Ints(row, "mappedSimplexCounts").SequenceEqual(ExpectedNegativeMappedCounts(row.GetProperty("extent").GetInt32()))
    && Ints(row, "restrictedTwiceIdentityCounts").SequenceEqual(Ints(row, "mappedSimplexCounts"));
static bool ConstructionRowValid(JsonElement row) =>
    JsonString(row, "cellRepresentation") == "canonical-base-coordinate-in-Zn4-plus-active-axis-mask-0-through-15"
    && JsonBool(row, "canonicalRepresentationValid") && JsonBool(row, "allCellsCoveredByMaximalChains")
    && JsonBool(row, "maximalChainCoverIncidenceValid")
    && Ints(row, "cellRankCounts").SequenceEqual(ExpectedCellRankCounts(row.GetProperty("extent").GetInt32()))
    && Ints(row, "expectedCellRankCounts").SequenceEqual(ExpectedCellRankCounts(row.GetProperty("extent").GetInt32()))
    && Ints(row, "barycentricSimplexCounts").SequenceEqual(ExpectedBarycentricCounts(row.GetProperty("extent").GetInt32()))
    && Ints(row, "expectedBarycentricSimplexCounts").SequenceEqual(ExpectedBarycentricCounts(row.GetProperty("extent").GetInt32()))
    && row.GetProperty("maximalChainGenerationCount").GetInt64() == ExpectedMaximalChains(row.GetProperty("extent").GetInt32())
    && row.GetProperty("maximalChainCount").GetInt64() == ExpectedMaximalChains(row.GetProperty("extent").GetInt32())
    && row.GetProperty("expectedMaximalChainCount").GetInt64() == ExpectedMaximalChains(row.GetProperty("extent").GetInt32())
    && row.GetProperty("maximalChainCoverIncidenceCheckCount").GetInt64() == 4 * ExpectedMaximalChains(row.GetProperty("extent").GetInt32());
static bool CandidateConstructionCrossLinked(JsonElement row, JsonElement[] constructionRows)
{
    int extent = row.GetProperty("extent").GetInt32();
    JsonElement construction = constructionRows.Single(x => x.GetProperty("extent").GetInt32() == extent);
    return Ints(row, "simplexCounts").SequenceEqual(Ints(construction, "barycentricSimplexCounts"));
}
static bool ReflectionCellRowValid(JsonElement row, JsonElement[] constructionRows)
{
    int extent = row.GetProperty("extent").GetInt32();
    string reflection = JsonString(row, "reflectionId");
    JsonElement construction = constructionRows.Single(x => x.GetProperty("extent").GetInt32() == extent);
    bool mapStringsValid = reflection switch
    {
        "site-centered-axis0" => row.GetProperty("offset").GetInt32() == 0
            && JsonString(row, "inactiveAxis0BaseMap") == "b0 -> -b0 mod n"
            && JsonString(row, "activeAxis0BaseMap") == "b0 -> -b0-1 mod n",
        "link-centered-axis0" => row.GetProperty("offset").GetInt32() == 1
            && JsonString(row, "inactiveAxis0BaseMap") == "b0 -> 1-b0 mod n"
            && JsonString(row, "activeAxis0BaseMap") == "b0 -> -b0 mod n",
        _ => false,
    };
    return row.GetProperty("axis").GetInt32() == 0 && mapStringsValid
    && row.GetProperty("cellCount").GetInt32() == 16 * Pow4(extent)
    && JsonBool(row, "allMappedCellsCanonical") && JsonBool(row, "activeMaskPreserved")
    && JsonBool(row, "cellMapBijection") && JsonBool(row, "cellMapInvolution")
    && JsonBool(row, "cellRankCountsPreserved")
    && Ints(row, "sourceCellRankCounts").SequenceEqual(Ints(construction, "cellRankCounts"))
    && Ints(row, "mappedCellRankCounts").SequenceEqual(Ints(construction, "cellRankCounts"));
}
static string ReflectionCellKey(JsonElement row) => "periodic-cubical-cell-poset-barycentric::"
    + JsonString(row, "reflectionId") + "::" + row.GetProperty("extent").GetInt32();
static int[] ExpectedNegativeSimplexCounts(int extent) => extent switch
{
    3 => [81, 1215, 4050, 4860, 1944],
    4 => [256, 3840, 12800, 15360, 6144],
    _ => [],
};
static int[] ExpectedNegativeMappedCounts(int extent) => extent switch
{
    3 => [81, 648, 972, 486, 0],
    4 => [256, 2048, 3072, 1536, 0],
    _ => [],
};
static int Pow4(int extent) => checked(extent * extent * extent * extent);
static int[] ExpectedCellRankCounts(int extent) => [1 * Pow4(extent), 4 * Pow4(extent), 6 * Pow4(extent), 4 * Pow4(extent), 1 * Pow4(extent)];
static int[] ExpectedBarycentricCounts(int extent) => [16 * Pow4(extent), 240 * Pow4(extent), 800 * Pow4(extent), 960 * Pow4(extent), 384 * Pow4(extent)];
static long ExpectedMaximalChains(int extent) => 384L * Pow4(extent);
static string[] ExpectedRowKeys(string role, string geometry, int[] extents) =>
    extents.SelectMany(extent => new[] { "site-centered-axis0", "link-centered-axis0" }
        .Select(reflection => role + "::" + geometry + "::" + reflection + "::" + extent))
        .Order(StringComparer.Ordinal).ToArray();
static string RowKey(JsonElement row) => JsonString(row, "role") + "::" + JsonString(row, "geometryId")
    + "::" + JsonString(row, "reflectionId") + "::" + row.GetProperty("extent").GetInt32();
static string RequiredVerdict(JsonElement bindings, string name) =>
    JsonString(bindings.GetProperty(name), "requiredVerdictKind");
static string PairGeometry(string pair) => pair[..pair.IndexOf("::", StringComparison.Ordinal)];
static string PairReflection(string pair) => pair[(pair.IndexOf("::", StringComparison.Ordinal) + 2)..];
static bool IsPlaceholder(string value) => string.IsNullOrWhiteSpace(value) || value.StartsWith("__", StringComparison.Ordinal);
static string ToKebab(string value) => string.Concat(value.Select((c, i) => char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
static string[] Strings(JsonElement element, string name) => element.GetProperty(name).EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
static string JsonString(JsonElement element, string name) => element.GetProperty(name).GetString() ?? "";
static bool JsonBool(JsonElement element, string name) => element.GetProperty(name).GetBoolean();
static string S(JsonElement element, string name) => JsonString(element, name);
static bool B(JsonElement element, string name) => JsonBool(element, name);
static int I(JsonElement element, string name) => element.GetProperty(name).GetInt32();
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

internal sealed class BindingSpec
{
    public BindingSpec(string id, string path, string expectedSha256)
    {
        Id = id;
        Path = path;
        ExpectedSha256 = expectedSha256;
    }

    public string Id { get; }
    public string Path { get; }
    public string ExpectedSha256 { get; }
}

internal sealed class Binding
{
    public Binding(string id, string path, string expectedSha256, string actualSha256, bool hashMatches)
    {
        Id = id;
        Path = path;
        ExpectedSha256 = expectedSha256;
        ActualSha256 = actualSha256;
        HashMatches = hashMatches;
    }

    public string Id { get; }
    public string Path { get; }
    public string ExpectedSha256 { get; }
    public string ActualSha256 { get; }
    public bool HashMatches { get; }
}
