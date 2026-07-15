using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phase482A5TheoremScout;

const string OutputDir = "studies/phase482_a5_theorem_scout_001/output";
const string OutputSlug = "a5_theorem_scout";
const string A5Path = "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json";
const string Phase478ContractPath = "studies/phase478_phase458_gate_specification_closure_001/preregistration/phase458_gate_contract_v1.json";
const string Phase481Path = "studies/phase481_phase456_prospective_repair_preregistration_001/output/phase456_prospective_repair_preregistration_summary.json";
const string ExpectedA5Sha256 = "b31ab07942ea08c83abbd0dc913d8213aa173bedf3b2c71833929ce604f58974";
const string ExpectedPhase478ContractSha256 = "d2bd1aa491ec7a9fc680c8f367fbed353b4abde040d7511671fe417dfba894f0";
const string DecisionContract = "phase482-a9-theorem-scout-v1|precedence=invalid-proof-package>counterexample-refutes-proposed-no-go>no-go-theorem-closed>obstructions-survive-no-theorem|families=composite-correlation-transport,reflection-positivity-committed-simplicial-action,face-local-interaction-hypothesis|closure=all-three-proof-gates+unchanged-action+unchanged-triangulation+unchanged-observable+volume-uniform+all-coupling-regimes+no-unproved-premise|generic-lemma-counterexample-is-obstruction-not-target-refutation|zero-authority";

var inputErrors = new List<string>();
var a5Binding = BindA5();
var phase478Binding = BindPhase478Contract();
var phase481Boundary = BindPhase481Boundary();
if (!a5Binding.Valid) inputErrors.Add("a5-obstruction-record-invalid-or-drifted");
if (!phase478Binding.Valid) inputErrors.Add("phase478-contract-invalid-or-drifted");
if (!phase481Boundary.Valid) inputErrors.Add("phase481-boundary-invalid-or-drifted");

var composite = CompositeTransportAnalysis.Run();
var reflectionLocality = ReflectionLocalityAnalysis.Run();
bool reflectionBatteryValid =
    reflectionLocality.InputsValid &&
    reflectionLocality.ReflectionIsExactInvolution &&
    reflectionLocality.ReflectionIsVertexBijection &&
    reflectionLocality.KuhnSimplexCountPerCube == 24 &&
    reflectionLocality.TriangularFaceCountPerCube == 110 &&
    reflectionLocality.PeriodicExtent == 4 &&
    reflectionLocality.PeriodicSimplexCount == 6144 &&
    reflectionLocality.SiteReflectionPeriodicInvolution &&
    reflectionLocality.LinkReflectionPeriodicInvolution &&
    reflectionLocality.SiteReflectedPeriodicSimplexClosureCount == 0 &&
    reflectionLocality.LinkReflectedPeriodicSimplexClosureCount == 0 &&
    !reflectionLocality.SiteReflectionPeriodicAutomorphism &&
    !reflectionLocality.LinkReflectionPeriodicAutomorphism &&
    reflectionLocality.WitnessSimplexWasCommitted &&
    !reflectionLocality.ReflectedWitnessSimplexIsCommitted &&
    reflectionLocality.WitnessFaceWasCommitted &&
    !reflectionLocality.ReflectedWitnessFaceIsCommitted &&
    reflectionLocality.PrimitiveCurvatureSupportIsFaceLocal &&
    reflectionLocality.SingleSiteInteractionHypothesisRefuted &&
    reflectionLocality.IndependentFaceVariableFactorizationRefuted &&
    reflectionLocality.ProofGateCountImplemented == 10 &&
    reflectionLocality.ProofGateCountPassed == 10;
if (!composite.InputsValid) inputErrors.Add("composite-transport-battery-invalid");
if (!reflectionBatteryValid) inputErrors.Add("reflection-locality-battery-invalid");

var familyInputs = new[]
{
    new TheoremScoutFamilyResult
    {
        FamilyId = TheoremScoutAdjudication.CompositeCorrelationTransport,
        InputsValid = composite.InputsValid,
        AnalysisImplemented = true,
        ProofGateImplemented = true,
        CounterexampleGateImplemented = true,
        RequiredClaimProved = composite.ActionSpecificCompositeTheoremProved,
        RefutingCounterexampleEstablished = false,
        CounterexampleWithinProposedTheoremDomain = false,
        CommittedActionUsed = false,
        CommittedTriangulationUsed = false,
        WorkbenchObservableUnchanged = true,
        UniformBeyondAuditedFiniteVolume = true,
        AllRequiredCouplingRegimesCovered = false,
        NoUnprovedPremiseImported = true,
    },
    new TheoremScoutFamilyResult
    {
        FamilyId = TheoremScoutAdjudication.ReflectionPositivity,
        InputsValid = reflectionBatteryValid,
        AnalysisImplemented = true,
        ProofGateImplemented = true,
        CounterexampleGateImplemented = true,
        RequiredClaimProved = reflectionLocality.ReflectionPositivityEstablished,
        RefutingCounterexampleEstablished = reflectionLocality.ReflectionPositivityRefuted,
        CounterexampleWithinProposedTheoremDomain = false,
        CommittedActionUsed = true,
        CommittedTriangulationUsed = true,
        WorkbenchObservableUnchanged = true,
        UniformBeyondAuditedFiniteVolume = true,
        AllRequiredCouplingRegimesCovered = true,
        NoUnprovedPremiseImported = true,
    },
    new TheoremScoutFamilyResult
    {
        FamilyId = TheoremScoutAdjudication.FaceLocalInteraction,
        InputsValid = reflectionBatteryValid,
        AnalysisImplemented = true,
        ProofGateImplemented = true,
        CounterexampleGateImplemented = true,
        RequiredClaimProved = reflectionLocality.StrictFaceFactorizationEstablished,
        RefutingCounterexampleEstablished = false,
        CounterexampleWithinProposedTheoremDomain = false,
        CommittedActionUsed = true,
        CommittedTriangulationUsed = true,
        WorkbenchObservableUnchanged = true,
        UniformBeyondAuditedFiniteVolume = true,
        AllRequiredCouplingRegimesCovered = true,
        NoUnprovedPremiseImported = true,
    },
};

bool precedenceBatteryPassed = TheoremScoutAdjudication.RunPrecedenceBattery();
if (!precedenceBatteryPassed) inputErrors.Add("adjudication-precedence-battery-failed");
var adjudication = TheoremScoutAdjudication.Evaluate(new TheoremScoutInputs
{
    A5ObstructionRecordExact = a5Binding.Valid,
    Phase478ContractExact = phase478Binding.Valid,
    DecisionTaxonomyFrozenBeforeInspection = true,
    CommittedActionIdentifiedExactly = a5Binding.Valid && reflectionBatteryValid,
    CommittedTriangulationIdentifiedExactly = reflectionBatteryValid,
    Families = familyInputs,
});

bool inputsValid = inputErrors.Count == 0 && adjudication.InputsValid;
string verdictKind = inputsValid ? adjudication.VerdictKind : TheoremScoutAdjudication.InvalidProofPackage;
bool theoremClaimed = inputsValid && adjudication.TheoremClaimed;
bool closesLimbL8 = inputsValid && adjudication.ClosesLimbL8;
bool a9BoundaryHeld =
    !phase481Boundary.PackCreated && !theoremClaimed && !closesLimbL8 &&
    !adjudication.Phase458EvaluationAuthorized && !adjudication.SamplingAuthorized &&
    !adjudication.BinderLaunchAuthorized && !adjudication.ProductionAuthorized &&
    adjudication.PromotedPhysicalMassClaimCount == 0;

var result = new
{
    phaseId = "phase482-a5-theorem-scout",
    terminalStatus = $"a5-theorem-scout-{verdictKind}",
    verdictKind,
    applicationSubjectKind = "a5-theorem-scout",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A9",
    waveOrder = 6,
    skeletonBuilt = false,
    zeroPhysicsCompute = false,
    deterministicProofComputeOnly = true,
    awaitingImplementation = false,
    executionPriorityOrderingNarrowlyAmended = true,
    phase481SamplingBoundaryUnchanged = true,
    decisionContractSha256 = Sha256(Encoding.UTF8.GetBytes(DecisionContract)),
    decisionContract = DecisionContract,
    taxonomy = new[]
    {
        TheoremScoutAdjudication.InvalidProofPackage,
        TheoremScoutAdjudication.NoGoTheoremClosed,
        TheoremScoutAdjudication.CounterexampleRefutesProposedNoGo,
        TheoremScoutAdjudication.ObstructionsSurviveNoTheorem,
    },
    inputsValid,
    inputErrors,
    frozenInputs = new { a5Binding, phase478Binding, phase481Boundary },
    compositeTransport = composite,
    reflectionLocality,
    reflectionBatteryValid,
    familyInputs,
    precedenceBatteryPassed,
    adjudication,
    proofGateCountImplemented = 3,
    allThreeObstructionFamilyProofGatesGreen = adjudication.AllThreeObstructionFamilyProofGatesGreen,
    validCommittedActionCounterexampleEstablished = adjudication.ValidRefutingCounterexampleEstablished,
    theoremClaimed,
    closesLimbL8,
    phase458G1Satisfied = theoremClaimed,
    phase458Evaluated = false,
    phase458EvaluationAuthorized = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    phase481PackConstructed = false,
    phase481PackAuthorized = false,
    samplingAuthorized = false,
    binderLaunchAuthorized = false,
    accelerationAuthorized = false,
    productionAuthorized = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase456ArtifactReadOrReinterpreted = false,
    phase456TerminalChanged = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    physicistReviewPending = true,
    scaleIsWorkbenchRelativeCandidateOnly = true,
    physicalCouplingProvided = false,
    routeProvidesPhysicalEffectiveActionHessian = false,
    routeProvidesVevOrSourceScaleLineage = false,
    routeProvidesPoleExtractionAndGeVNormalization = false,
    sourceContractApplicationAllowed = false,
    phase201TemplateMutated = false,
    fieldsAppliedToPhase201TemplateCount = 0,
    acceptedContractFieldCount = 0,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a9BoundaryHeld,
    decision = verdictKind switch
    {
        "no-go-theorem-closed" => "All three committed-scope proof gates are green. Phase482 closes L8 and marks only Phase458 G1; it does not evaluate Phase458 or authorize sampling.",
        "counterexample-refutes-proposed-no-go" => "An exact committed-action counterexample refutes the proposed no-go inequality. L8 remains open and no sampling is authorized.",
        "obstructions-survive-no-theorem" => "Exact batteries block transport from two-point data alone, the committed Kuhn complex is not reflection closed, and single-site or independent-face factorization does not type-check. These are proof-route obstructions, not a target counterexample or theorem; L8 remains open.",
        _ => "A frozen input or proof package is invalid or incomplete. Phase482 fails closed and emits no theorem or authority.",
    },
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, $"{OutputSlug}.json"), json);
File.WriteAllText(Path.Combine(OutputDir, $"{OutputSlug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"inputsValid={inputsValid} theoremClaimed={theoremClaimed} closesLimbL8={closesLimbL8} a9BoundaryHeld={a9BoundaryHeld}");

static InputBinding BindA5()
{
    if (!File.Exists(A5Path)) return new InputBinding(A5Path, null, false, "missing", false);
    byte[] bytes = File.ReadAllBytes(A5Path);
    string hash = Sha256(bytes);
    try
    {
        using var doc = JsonDocument.Parse(bytes);
        var root = doc.RootElement;
        bool schema = root.GetProperty("packItem").GetString() == "A5_stage_a_gaussian_domination_attempt" &&
            root.GetProperty("provable").GetBoolean() == false &&
            root.GetProperty("verdict").GetString() == "a5-stage-a-gaussian-domination-not-provable-obstruction-recorded" &&
            root.GetProperty("obstructions").GetArrayLength() == 3;
        return new InputBinding(A5Path, hash, hash == ExpectedA5Sha256 && schema, schema ? "exact-hash-and-schema" : "schema-invalid", false);
    }
    catch (Exception ex) { return new InputBinding(A5Path, hash, false, $"json-invalid:{ex.GetType().Name}", false); }
}

static InputBinding BindPhase478Contract()
{
    if (!File.Exists(Phase478ContractPath)) return new InputBinding(Phase478ContractPath, null, false, "missing", false);
    byte[] bytes = File.ReadAllBytes(Phase478ContractPath);
    string hash = Sha256(bytes);
    try
    {
        using var doc = JsonDocument.Parse(bytes);
        var root = doc.RootElement;
        var g1 = root.GetProperty("gates").GetProperty("G1");
        bool schema = root.GetProperty("contractId").GetString() == "phase458-gate-contract-v1" &&
            g1.GetProperty("phase482AllowedVerdictKinds").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
            {
                "no-go-theorem-closed", "obstructions-survive-no-theorem", "counterexample-refutes-proposed-no-go",
            });
        return new InputBinding(Phase478ContractPath, hash, hash == ExpectedPhase478ContractSha256 && schema, schema ? "exact-hash-and-schema" : "schema-invalid", false);
    }
    catch (Exception ex) { return new InputBinding(Phase478ContractPath, hash, false, $"json-invalid:{ex.GetType().Name}", false); }
}

static InputBinding BindPhase481Boundary()
{
    if (!File.Exists(Phase481Path)) return new InputBinding(Phase481Path, null, false, "missing", false);
    byte[] bytes = File.ReadAllBytes(Phase481Path);
    string hash = Sha256(bytes);
    try
    {
        using var doc = JsonDocument.Parse(bytes);
        var root = doc.RootElement;
        bool packCreated = root.GetProperty("freshRepairPackCreated").GetBoolean();
        bool schema = root.GetProperty("verdictKind").GetString() == "preregistration-skeleton-awaiting-implementation" &&
            root.GetProperty("samplingRunOrAuthorized").GetBoolean() == false && !packCreated;
        return new InputBinding(Phase481Path, hash, schema, schema ? "non-authorizing-boundary-verified" : "boundary-invalid", packCreated);
    }
    catch (Exception ex) { return new InputBinding(Phase481Path, hash, false, $"json-invalid:{ex.GetType().Name}", false); }
}

static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

sealed record InputBinding(string Path, string? ByteSha256, bool Valid, string Status, bool PackCreated);
