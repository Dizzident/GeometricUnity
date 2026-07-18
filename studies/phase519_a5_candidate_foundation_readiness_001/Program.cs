using System.Security.Cryptography;
using System.Text.Json;

const string PhaseDir = "studies/phase519_a5_candidate_foundation_readiness_001";
const string ContractPath = PhaseDir + "/preregistration/phase519_a5_candidate_foundation_readiness_contract_v1.json";
const string OutputDir = PhaseDir + "/output";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement precursorBindings = contract.GetProperty("precursorBindings");
BindingSpec[] specs =
[
    Spec("phase517-summary", precursorBindings.GetProperty("phase517Summary")),
    Spec("phase517-contract", precursorBindings.GetProperty("phase517Contract")),
    Spec("phase517-program", precursorBindings.GetProperty("phase517Program")),
    Spec("phase517-site-candidate", precursorBindings.GetProperty("phase517SiteCandidate")),
    Spec("phase517-link-candidate", precursorBindings.GetProperty("phase517LinkCandidate")),
    Spec("phase518-summary", precursorBindings.GetProperty("phase518Summary")),
    Spec("phase518-contract", precursorBindings.GetProperty("phase518Contract")),
    Spec("phase518-program", precursorBindings.GetProperty("phase518Program")),
];

// Mandatory construction firewall: only this phase's own contract has been read.
// Refuse before touching either precursor while any exact hash remains unfrozen.
if (specs.Any(x => IsPlaceholder(x.ExpectedSha256)))
    throw new InvalidOperationException("Phase519 is a non-runnable template: exact Phase517 and Phase518 summary/contract SHA-256 bindings are still placeholders.");

Binding[] bindings = specs.Select(Bind).ToArray();
BindingSpec phase517Summary = specs.Single(x => x.Id == "phase517-summary");
BindingSpec phase517Contract = specs.Single(x => x.Id == "phase517-contract");
BindingSpec phase517Program = specs.Single(x => x.Id == "phase517-program");
BindingSpec siteCandidate = specs.Single(x => x.Id == "phase517-site-candidate");
BindingSpec linkCandidate = specs.Single(x => x.Id == "phase517-link-candidate");
BindingSpec phase518Summary = specs.Single(x => x.Id == "phase518-summary");
BindingSpec phase518Contract = specs.Single(x => x.Id == "phase518-contract");
BindingSpec phase518Program = specs.Single(x => x.Id == "phase518-program");
string[] precedence = Strings(contract, "precedence");

bool contractValid = I(contract, "schemaVersion") == 1
    && S(contract, "contractId") == "phase519-a17-a5-candidate-foundation-readiness-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A17"
    && S(contract, "contractStatus") == "final-frozen-before-first-runnable-invocation"
    && B(contract, "frozenBeforePrecursorConsumption")
    && precedence.SequenceEqual(new[]
    {
        "invalid-or-drifted-input",
        "action-or-observable-subject-ambiguous",
        "configuration-or-normalized-measure-incomplete",
        "reflection-or-pullback-incomplete",
        "candidate-target-equality-unproved",
        "bilinear-finiteness-or-hermiticity-unproved",
        "finite-only",
        "necessity-unproved",
        "embedding-or-gluing-unproved",
        "all-scope-unproved",
        "candidate-package-review-ready",
    }, StringComparer.Ordinal)
    && S(contract, "expectedCurrentVerdict") == "action-or-observable-subject-ambiguous"
    && S(contract, "strongestPermittedTerminal") == "candidate-package-review-ready"
    && S(contract, "strongestPermittedClaim") == "independent-math-review-readiness-only"
    && !B(contract, "phase515Or516Unlocked") && B(contract, "externalReviewPending")
    && !B(contract, "theoremClaimAllowed") && !B(contract, "samplingOrProductionAllowed")
    && !B(contract, "physicalClaimAllowed") && I(contract, "promotedPhysicalMassClaimCount") == 0;

using var phase517Doc = JsonDocument.Parse(File.ReadAllBytes(phase517Summary.Path));
using var phase518Doc = JsonDocument.Parse(File.ReadAllBytes(phase518Summary.Path));
using var phase517ContractDoc = JsonDocument.Parse(File.ReadAllBytes(phase517Contract.Path));
using var phase518ContractDoc = JsonDocument.Parse(File.ReadAllBytes(phase518Contract.Path));
using var siteCandidateDoc = JsonDocument.Parse(File.ReadAllBytes(siteCandidate.Path));
using var linkCandidateDoc = JsonDocument.Parse(File.ReadAllBytes(linkCandidate.Path));
JsonElement phase517 = phase517Doc.RootElement;
JsonElement phase518 = phase518Doc.RootElement;
JsonElement phase517FrozenContract = phase517ContractDoc.RootElement;
JsonElement phase518FrozenContract = phase518ContractDoc.RootElement;
JsonElement site = siteCandidateDoc.RootElement;
JsonElement link = linkCandidateDoc.RootElement;
string phase517Source = File.ReadAllText(phase517Program.Path);
string phase518Source = File.ReadAllText(phase518Program.Path);

bool precursorSemanticsValid = JsonBool(phase517, "inputsValid")
    && JsonString(phase517, "verdictKind") == RequiredVerdict(precursorBindings.GetProperty("phase517Summary"))
    && JsonBool(phase518, "inputsValid")
    && JsonString(phase518, "verdictKind") == RequiredVerdict(precursorBindings.GetProperty("phase518Summary"))
    && JsonString(phase517FrozenContract, "contractId") == "phase517-a17-dual-reflection-candidate-foundation-v1"
    && JsonString(phase517FrozenContract, "expectedCurrentVerdict") == "action-member-or-omega-parity-ambiguous"
    && JsonString(phase518FrozenContract, "contractId") == "phase518-a5-dual-reflection-exact-consistency-contract-v1"
    && JsonString(phase518FrozenContract, "expectedCurrentVerdict") == "dual-candidate-oriented-complex-nonclosure"
    && CandidateValid(site, "site-centered-axis0", "site-centered")
    && CandidateValid(link, "link-centered-axis0", "link-centered")
    && CandidateArtifactsMatch(phase517, siteCandidate, linkCandidate)
    && phase517Source.Contains("ActionAndParityAreUnambiguous() => false", StringComparison.Ordinal)
    && phase517Source.Contains("candidateSelectionPerformed = false", StringComparison.Ordinal)
    && phase518Source.Contains("dualCandidateOrientedComplexNonclosure = dualNonclosure", StringComparison.Ordinal)
    && phase518Source.Contains("finiteControlSupportsAllVolumeInference = false", StringComparison.Ordinal);
bool invalidOrDrifted = !contractValid || bindings.Any(x => !x.HashMatches) || !precursorSemanticsValid;

bool actionOrObservableAmbiguous =
    JsonString(phase517.GetProperty("actionSubjectAudit"), "status") == "action-member-or-omega-parity-ambiguous"
    || JsonString(phase517.GetProperty("unresolved"), "observableMembership") != "proved";
bool configurationOrNormalizedMeasureIncomplete =
    JsonString(phase517.GetProperty("unresolved"), "measureNormalization") != "proved";
bool reflectionOrPullbackIncomplete =
    JsonBool(phase518, "dualCandidateOrientedComplexNonclosure")
    || !JsonBool(phase518, "fullCandidateActionCovarianceEvaluated");
bool candidateTargetEqualityUnproved =
    JsonString(phase517.GetProperty("unresolved"), "targetEquality") != "proved";
bool bilinearFinitenessOrHermiticityUnproved =
    JsonString(phase517.GetProperty("unresolved"), "hermiticity") != "proved"
    || !JsonBool(phase518, "osBilinearHermiticityEvaluated");
bool finiteOnly = !JsonBool(phase518, "finiteControlSupportsAllVolumeInference");
string necessityAndEmbedding = JsonString(phase517.GetProperty("unresolved"), "necessityAndEmbedding");
bool necessityUnproved = necessityAndEmbedding != "proved";
bool embeddingOrGluingUnproved = necessityAndEmbedding != "proved";
bool allScopeUnproved = !JsonBool(phase518, "registeredTheoremReflectionEstablished");

bool[] blockers =
[
    invalidOrDrifted, actionOrObservableAmbiguous, configurationOrNormalizedMeasureIncomplete,
    reflectionOrPullbackIncomplete, candidateTargetEqualityUnproved,
    bilinearFinitenessOrHermiticityUnproved, finiteOnly, necessityUnproved,
    embeddingOrGluingUnproved, allScopeUnproved,
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
    phase = 519,
    phaseId = "phase519-a5-candidate-foundation-readiness",
    terminalStatus = "a5-candidate-foundation-readiness-" + verdict,
    verdictKind = verdict,
    inputsValid = !invalidOrDrifted,
    contractValid,
    applicationSubjectKind = "a5-dual-reflection-candidate-foundation-readiness-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A17",
    zeroPhysicsCompute = true,
    prospectiveAdjudicator = true,
    confirmationEvidence = false,
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
        actionOrObservableAmbiguous,
        configurationOrNormalizedMeasureIncomplete,
        reflectionOrPullbackIncomplete,
        candidateTargetEqualityUnproved,
        bilinearFinitenessOrHermiticityUnproved,
        finiteOnly,
        necessityUnproved,
        embeddingOrGluingUnproved,
        allScopeUnproved,
    },
    expectedCurrentTerminalPreservesPhase517ActionAmbiguity = verdict == "action-or-observable-subject-ambiguous",
    strongestPermittedTerminal = "candidate-package-review-ready",
    strongestPermittedClaim = "independent-math-review-readiness-only",
    candidatePackageIndependentMathReviewReady = verdict == "candidate-package-review-ready",
    reviewReady = verdict == "candidate-package-review-ready",
    externalReviewPending = true,
    phase515Locked = true,
    phase516Locked = true,
    candidateDefinitionRegistered = false,
    registeredFoundationEstablished = false,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    phase458G1Satisfied = false,
    closesLimbL8 = false,
    phase458EvaluationPerformed = false,
    phase515Unlocked = false,
    phase516Unlocked = false,
    phase480OrPhase481WorkPerformed = false,
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
    a17BoundaryHeld = true,
    firewalls = new
    {
        theoremClaimed = false,
        targetCounterexampleClaimed = false,
        reflectionPositivityEstablished = false,
        reflectionPositivityRefuted = false,
        phase458G1Satisfied = false,
        closesLimbL8 = false,
        phase458EvaluationPerformed = false,
        phase515Unlocked = false,
        phase516Unlocked = false,
        phase480OrPhase481WorkPerformed = false,
        samplingOrReprocessingRun = false,
        hmcRun = false,
        benchmarkRun = false,
        physicsComputeRun = false,
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
        promotedPhysicalMassClaimCount = 0,
    },
    decision = verdict switch
    {
        "invalid-or-drifted-input" => "At least one exact precursor binding or required semantic field is missing, invalid, or drifted. The package is not review-ready and nothing is authorized.",
        "candidate-package-review-ready" => "All frozen foundation blockers are cleared. The candidate package is ready only for independent mathematical review; this is not theorem evidence or execution authority, and external review remains pending.",
        _ => "The candidate package remains fail-closed at its earliest substantive unresolved foundation requirement. It is not review-ready and nothing is authorized.",
    },
};

Require(precedenceBattery.All(x => x.passed), "Phase519 precedence battery drifted.");
Require(result.contractValid, "Phase519 frozen contract validation drifted.");
Require(verdict == S(contract, "expectedCurrentVerdict"), "Phase519 expected current terminal drifted.");
Require(result.targetBlindConstruction && !result.physicalTargetsConsultedForConstruction
    && !result.candidatePackageIndependentMathReviewReady && !result.reviewReady && result.externalReviewPending
    && result.phase515Locked && result.phase516Locked && !result.phase515Unlocked && !result.phase516Unlocked
    && !result.candidateDefinitionRegistered && !result.registeredFoundationEstablished
    && !result.phase515MayBeCreated && !result.phase516MayBeCreated,
    "Phase519 target-blind review, definition, foundation, or lock firewall drifted.");
Require(!result.theoremClaimed && !result.targetCounterexampleClaimed
    && !result.reflectionPositivityEstablished && !result.reflectionPositivityRefuted
    && !result.phase458G1Satisfied && !result.closesLimbL8 && !result.phase458EvaluationPerformed
    && !result.sourceContractApplicationAllowed && !result.routePromotesWzMasses
    && !result.routePromotesHiggsMass && !result.routeCompletesBosonPredictions
    && !result.physicalUnitOrGevClaimMade && result.noGevPromotion && result.promotedPhysicalMassClaimCount == 0,
    "Phase519 direct theorem, positivity, counterexample, route, source, or GeV firewall drifted.");
Require(!result.firewalls.theoremClaimed && !result.firewalls.targetCounterexampleClaimed
    && !result.firewalls.reflectionPositivityEstablished && !result.firewalls.reflectionPositivityRefuted
    && !result.firewalls.phase458G1Satisfied && !result.firewalls.closesLimbL8
    && !result.firewalls.sourceContractApplicationAllowed && !result.firewalls.routePromotesWzMasses
    && !result.firewalls.routePromotesHiggsMass && !result.firewalls.routeCompletesBosonPredictions
    && !result.firewalls.physicalUnitOrGevClaimMade && result.firewalls.promotedPhysicalMassClaimCount == 0
    && !result.firewalls.productionAuthorized && !result.firewalls.launchAuthorized,
    "Phase519 nested claim or execution firewall drifted.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options) + Environment.NewLine;
File.WriteAllText(Path.Combine(OutputDir, "a5_candidate_foundation_readiness.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_candidate_foundation_readiness_summary.json"), json);
Console.WriteLine(result.terminalStatus);

static BindingSpec Spec(string id, JsonElement element) => new(
    id,
    element.GetProperty("path").GetString() ?? "",
    element.GetProperty("sha256").GetString() ?? "");

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

static bool IsPlaceholder(string value) => string.IsNullOrWhiteSpace(value) || value.StartsWith("__", StringComparison.Ordinal);
static bool CandidateValid(JsonElement candidate, string id, string centering) =>
    JsonString(candidate, "candidateId") == id
    && JsonString(candidate, "centering") == centering
    && JsonString(candidate, "status") == "audit-authored-candidate-not-registered-theorem-reflection"
    && JsonString(candidate, "actionMember") == "ambiguous-not-frozen-by-a5"
    && JsonString(candidate, "candidateRole") == "formal-diagnostic-row-only"
    && !JsonBool(candidate, "registered") && !JsonBool(candidate, "a5Validated")
    && JsonString(candidate, "measureDomain").Contains("normalization not proved", StringComparison.Ordinal)
    && JsonString(candidate, "orientedEdgePullback").Contains("conditional", StringComparison.Ordinal);
static bool CandidateArtifactsMatch(JsonElement phase517, BindingSpec site, BindingSpec link)
{
    JsonElement[] artifacts = phase517.GetProperty("formalCandidateFoundation").GetProperty("candidateArtifacts").EnumerateArray().ToArray();
    return artifacts.Length == 2
        && artifacts.Any(x => JsonString(x, "candidateId") == "site-centered-axis0" && JsonString(x, "path") == site.Path && JsonString(x, "sha256") == site.ExpectedSha256)
        && artifacts.Any(x => JsonString(x, "candidateId") == "link-centered-axis0" && JsonString(x, "path") == link.Path && JsonString(x, "sha256") == link.ExpectedSha256);
}
static string RequiredVerdict(JsonElement element) => element.GetProperty("requiredVerdictKind").GetString() ?? "";
static string[] Strings(JsonElement element, string name) => element.GetProperty(name).EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
static string JsonString(JsonElement element, string name) => element.GetProperty(name).GetString() ?? "";
static bool JsonBool(JsonElement element, string name) => element.GetProperty(name).GetBoolean();
static string S(JsonElement element, string name) => JsonString(element, name);
static bool B(JsonElement element, string name) => JsonBool(element, name);
static int I(JsonElement element, string name) => element.GetProperty(name).GetInt32();
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

internal sealed record BindingSpec(string Id, string Path, string ExpectedSha256);
internal sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
