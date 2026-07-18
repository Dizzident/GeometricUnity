using System.Security.Cryptography;
using System.Text.Json;

const string ContractPath = "studies/phase514_a5_registered_reflection_foundation_audit_001/preregistration/phase514_registered_reflection_foundation_contract_v1.json";
const string OutputDir = "studies/phase514_a5_registered_reflection_foundation_audit_001/output";
const string Phase482SummaryPath = "studies/phase482_a5_theorem_scout_001/output/a5_theorem_scout_summary.json";
const string Phase482ProgramPath = "studies/phase482_a5_theorem_scout_001/Program.cs";
const string Phase482ReflectionPath = "studies/phase482_a5_theorem_scout_001/ReflectionLocalityAnalysis.cs";
const string Phase482CompositePath = "studies/phase482_a5_theorem_scout_001/CompositeTransportAnalysis.cs";
const string Phase482AdjudicationPath = "studies/phase482_a5_theorem_scout_001/TheoremScoutAdjudication.cs";
const string Phase456A5Path = "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json";
const string Phase456A4Path = "studies/phase456_consolidated_n4_launch_001/preregistration/a4_symmetry_irrep_projectors.json";
const string Phase456GeneratorPath = "studies/phase456_consolidated_n4_launch_001/preregistration/a4a5_pack_generator/Program.cs";
const string Phase478ContractPath = "studies/phase478_phase458_gate_specification_closure_001/preregistration/phase458_gate_contract_v1.json";
const string Phase452ProgramPath = "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs";
const string ShiabOperatorPath = "src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs";
const string CurvatureAssemblerPath = "src/Gu.ReferenceCpu/CurvatureAssembler.cs";
const string CpuMassMatrixPath = "src/Gu.ReferenceCpu/CpuMassMatrix.cs";
const string MeshGeneratorPath = "src/Gu.Geometry/SimplicialMeshGenerator.cs";
const string ShiabFamilySpecPath = "src/Gu.ReferenceCpu/EinsteinianShiabFamilySpec.cs";
const string Phase456PackManifestPath = "studies/phase456_consolidated_n4_launch_001/preregistration/pack_manifest.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");
var bindings = new[]
{
    Bind("phase482-summary", Phase482SummaryPath, expected.GetProperty("phase482SummarySha256").GetString()!),
    Bind("phase482-program", Phase482ProgramPath, expected.GetProperty("phase482ProgramSha256").GetString()!),
    Bind("phase482-reflection-analysis", Phase482ReflectionPath, expected.GetProperty("phase482ReflectionSha256").GetString()!),
    Bind("phase482-composite-analysis", Phase482CompositePath, expected.GetProperty("phase482CompositeSha256").GetString()!),
    Bind("phase482-adjudication", Phase482AdjudicationPath, expected.GetProperty("phase482AdjudicationSha256").GetString()!),
    Bind("phase456-a5-record", Phase456A5Path, expected.GetProperty("phase456A5Sha256").GetString()!),
    Bind("phase456-a4-record", Phase456A4Path, expected.GetProperty("phase456A4Sha256").GetString()!),
    Bind("phase456-a4a5-generator", Phase456GeneratorPath, expected.GetProperty("phase456GeneratorSha256").GetString()!),
    Bind("phase478-g1-contract", Phase478ContractPath, expected.GetProperty("phase478ContractSha256").GetString()!),
    Bind("phase452-action-observable-source", Phase452ProgramPath, expected.GetProperty("phase452ProgramSha256").GetString()!),
    Bind("shiab-operator-source", ShiabOperatorPath, expected.GetProperty("shiabOperatorSha256").GetString()!),
    Bind("curvature-assembler-source", CurvatureAssemblerPath, expected.GetProperty("curvatureAssemblerSha256").GetString()!),
    Bind("cpu-mass-matrix-source", CpuMassMatrixPath, expected.GetProperty("cpuMassMatrixSha256").GetString()!),
    Bind("mesh-generator-source", MeshGeneratorPath, expected.GetProperty("meshGeneratorSha256").GetString()!),
    Bind("shiab-family-spec-source", ShiabFamilySpecPath, expected.GetProperty("shiabFamilySpecSha256").GetString()!),
    Bind("phase456-pack-manifest", Phase456PackManifestPath, expected.GetProperty("phase456PackManifestSha256").GetString()!),
};

string reflectionSource = File.ReadAllText(Phase482ReflectionPath);
string generatorSource = File.ReadAllText(Phase456GeneratorPath);
string phase452Source = File.ReadAllText(Phase452ProgramPath);
string familySource = File.ReadAllText(ShiabFamilySpecPath);
string[] requiredFields = contract.GetProperty("requiredDefinitionFields").EnumerateArray()
    .Select(x => x.GetString() ?? string.Empty).ToArray();
string[] certificateArtifacts = contract.GetProperty("expectedDedicatedCertificateArtifacts").EnumerateArray()
    .Select(x => x.GetString() ?? string.Empty).ToArray();
string[] precedence = contract.GetProperty("precedence").EnumerateArray()
    .Select(x => x.GetString() ?? string.Empty).ToArray();
string[] expectedFields =
{
    "unique-action-member-and-functional", "configuration-space-or-gauge-quotient",
    "normalized-integrable-measure", "coupling-domain", "allowed-volume-domain",
    "unique-site-or-link-reflection", "reflection-plane-and-vertex-map",
    "oriented-edge-pullback-and-reversal-sign", "boundary-edge-treatment",
    "complex-conjugation-rule", "positive-time-observable-algebra",
    "unchanged-o1-o2-membership", "connected-composite-subtraction-rule",
    "exact-os-bilinear-or-crossing-kernel", "bilinear-finiteness-domain",
    "hermiticity-identity", "candidate-equals-registered-target-proof",
    "necessary-versus-sufficient-classification", "allowed-volume-witness-embedding",
    "all-required-coupling-and-volume-scope",
};
string[] expectedCertificatePaths =
{
    "studies/phase482_a5_theorem_scout_001/preregistration/registered_reflection_kernel_v1.json",
    "studies/phase482_a5_theorem_scout_001/preregistration/positive_time_observable_algebra_v1.json",
    "studies/phase482_a5_theorem_scout_001/preregistration/os_bilinear_domain_v1.json",
    "studies/phase482_a5_theorem_scout_001/preregistration/necessity_and_embedding_certificate_v1.json",
};

bool precursorSemanticsValid =
    JsonString(Phase482SummaryPath, "verdictKind") == "obstructions-survive-no-theorem"
    && !JsonBool(Phase482SummaryPath, "theoremClaimed")
    && !JsonBool(Phase482SummaryPath, "closesLimbL8")
    && JsonString(Phase456A5Path, "verdict") == "a5-stage-a-gaussian-domination-not-provable-obstruction-recorded"
    && !JsonBool(Phase456A5Path, "provable")
    && JsonNestedBool(Phase456A4Path, "parity", "timeReflectionRealized")
    && JsonString(Phase478ContractPath, "contractId") == "phase458-gate-contract-v1"
    && reflectionSource.Contains("linkCentered: false", StringComparison.Ordinal)
    && reflectionSource.Contains("linkCentered: true", StringComparison.Ordinal)
    && generatorSource.Contains("O1 = Tr(F^2), O2 = Tr(Upsilon^2)", StringComparison.Ordinal)
    && phase452Source.Contains("sd2-id0/c0.5", StringComparison.Ordinal)
    && phase452Source.Contains("<O(t)O(0)> - <O>^2", StringComparison.Ordinal)
    && familySource.Contains("no canonical member", StringComparison.Ordinal);

var certificateArtifactAudit = certificateArtifacts.Select(path => new
{
    path,
    present = File.Exists(path),
    status = File.Exists(path) ? "present-not-bound-by-this-snapshot" : "not-present-at-snapshot",
}).ToArray();

var partialFieldIds = new HashSet<string>(StringComparer.Ordinal)
{
    "unique-action-member-and-functional",
    "configuration-space-or-gauge-quotient",
    "coupling-domain",
    "allowed-volume-domain",
    "unique-site-or-link-reflection",
    "reflection-plane-and-vertex-map",
    "unchanged-o1-o2-membership",
    "connected-composite-subtraction-rule",
    "all-required-coupling-and-volume-scope",
};
var fieldAudit = requiredFields.Select(id => new
{
    id,
    status = partialFieldIds.Contains(id) ? "source-defined-or-partial-candidate" : "registered-certificate-absent",
    theoremRegistered = false,
    reason = id switch
    {
        "unique-action-member-and-functional" => "The sources define action-family members and describe a quartic control branch, but no exact-bound artifact registers one unique theorem subject.",
        "configuration-space-or-gauge-quotient" => "Executable field arrays are source-defined, but no theorem configuration space, gauge quotient, or measure domain is registered.",
        "coupling-domain" => "The A5 prose refers to beta but freezes no exact all-required-coupling domain.",
        "allowed-volume-domain" => "Phase482 enumerates local and n=4 complexes but freezes no theorem volume domain.",
        "unique-site-or-link-reflection" => "Phase482 tests both site- and link-centered candidates; neither is registered as the theorem reflection.",
        "reflection-plane-and-vertex-map" => "Exact candidate vertex maps are implemented for both reflection choices, but no unique theorem map is registered.",
        "unchanged-o1-o2-membership" => "O1 and O2 are described, but no positive-time algebra or exact membership certificate is registered.",
        "connected-composite-subtraction-rule" => "The executable source defines connected subtraction, but no registered positive-time algebra ties it to an OS form.",
        "all-required-coupling-and-volume-scope" => "The adjudication requires uniform volume and coupling coverage, but the exact allowed domain is not enumerated.",
        "exact-os-bilinear-or-crossing-kernel" => "No exact OS form or full shared-edge crossing kernel artifact exists.",
        "hermiticity-identity" => "Hermiticity cannot be tested before a bilinear, domain, conjugation, and reflection are defined.",
        "necessary-versus-sufficient-classification" => "No certificate proves which candidate lemmas are necessary for the registered target inequality.",
        "allowed-volume-witness-embedding" => "No rule embeds a local witness into an allowed committed volume and the registered target.",
        _ => "No exact-bound repository artifact registers this required theorem field.",
    },
}).ToArray();

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase514-registered-reflection-foundation-contract-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A16"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && requiredFields.SequenceEqual(expectedFields, StringComparer.Ordinal)
    && certificateArtifacts.SequenceEqual(expectedCertificatePaths, StringComparer.Ordinal)
    && precedence.SequenceEqual(new[] { "invalid-or-drifted-input", "registered-foundation-definition-missing" }, StringComparer.Ordinal)
    && !contract.GetProperty("finiteSurvivalCanProveTheorem").GetBoolean()
    && contract.GetProperty("candidateKernelStatus").GetString() == "not-evaluated-no-registered-definition"
    && contract.GetProperty("snapshotOnlyCensus").GetBoolean()
    && !contract.GetProperty("phase515CreationAllowedByThisSnapshot").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool inputsValid = contractValid
    && bindings.Length == 16
    && bindings.All(x => x.HashMatches)
    && precursorSemanticsValid
    && fieldAudit.Length == 20
    && fieldAudit.Count(x => x.status == "source-defined-or-partial-candidate") == 9
    && fieldAudit.Count(x => x.status == "registered-certificate-absent") == 11
    && certificateArtifactAudit.Length == 4
    && certificateArtifactAudit.All(x => !x.present);
string verdict = inputsValid ? precedence[1] : precedence[0];
bool phase515MayBeCreated = false;

var result = new
{
    phaseId = "phase514-a5-registered-reflection-foundation-audit",
    terminalStatus = $"a5-registered-reflection-foundation-{verdict}",
    verdictKind = verdict,
    inputsValid,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A16",
    contract = new { path = ContractPath, sha256 = Sha(File.ReadAllBytes(ContractPath)), frozenBeforePrecursorConsumption = true },
    exactInputBindings = bindings,
    sourceFacts = new
    {
        phase482ObstructionsSurvive = true,
        siteReflectionCandidatePresent = true,
        linkReflectionCandidatePresent = true,
        neitherCandidateRegisteredAsUniqueTheoremReflection = true,
        a4RealizedTimeReversalIsNotTreatedAsTheRegisteredOsReflection = true,
        a5ActionAndObservableDescriptionPresent = true,
        executableSourceContainsMultipleActionMembers = true,
        standardReflectionRouteObstructedButReflectionPositivityNotRefuted = true,
        sourceCensusExhaustive = false,
    },
    definitionAudit = new
    {
        requiredFieldCount = fieldAudit.Length,
        theoremRegisteredFieldCount = fieldAudit.Count(x => x.theoremRegistered),
        sourceDefinedOrPartialCandidateCount = fieldAudit.Count(x => x.status == "source-defined-or-partial-candidate"),
        registeredCertificateAbsentCount = fieldAudit.Count(x => x.status == "registered-certificate-absent"),
        fields = fieldAudit,
        expectedDedicatedCertificateArtifactCount = certificateArtifactAudit.Length,
        presentDedicatedCertificateArtifactCount = certificateArtifactAudit.Count(x => x.present),
        dedicatedCertificateArtifacts = certificateArtifactAudit,
    },
    registeredFoundationComplete = false,
    registeredExactOsBilinearFound = false,
    hermiticityEvaluableFromBoundRegisteredArtifacts = false,
    nonHermitianWitnessEstablished = false,
    registeredNecessityAndEmbeddingCertificateFound = false,
    candidateKernelStatus = "not-evaluated-no-registered-definition",
    candidateKernelFailureRefutesTargetStatus = "not-proved-or-adjudicated",
    finiteSurvivalWouldProveTheorem = false,
    decisionPrecedence = precedence,
    phase515MayBeCreated,
    phase516MayBeCreated = false,
    theoremClaimed = false,
    targetCounterexampleEstablished = false,
    closesLimbL8 = false,
    phase458G1Satisfied = false,
    phase458Evaluated = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    phase480Satisfied = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase482ArtifactMutated = false,
    zeroPhysicsCompute = true,
    symbolicFalsifierRun = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    launchAuthorized = false,
    binderLaunchAuthorized = false,
    accelerationAuthorized = false,
    o4Discharged = false,
    humanRulingAuthoredOrInferred = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    externalReviewPending = true,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a16BoundaryHeld = true,
    decision = "No exact-bound repository artifact registers a unique OS reflection, positive-time algebra, measure/domain, bilinear, Hermiticity proof, necessity classification, or witness embedding. Their mathematical existence or nonexistence was not adjudicated. Phase515 remains hard-gated.",
};

Require(inputsValid, "Phase514 inputs or frozen snapshot contract drifted.");
Require(verdict == "registered-foundation-definition-missing", "Phase514 snapshot terminal drifted.");
Require(result.definitionAudit.theoremRegisteredFieldCount == 0 && result.definitionAudit.sourceDefinedOrPartialCandidateCount == 9 && result.definitionAudit.registeredCertificateAbsentCount == 11, "Phase514 definition census drifted.");
Require(!phase515MayBeCreated && !result.theoremClaimed && !result.targetCounterexampleEstablished && !result.closesLimbL8 && !result.phase458G1Satisfied, "Phase514 theorem firewall drifted.");
Require(result.a16BoundaryHeld && result.zeroPhysicsCompute && !result.symbolicFalsifierRun && !result.samplingOrReprocessingRun && !result.productionAuthorized && result.promotedPhysicalMassClaimCount == 0, "Phase514 execution or claim firewall drifted.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "a5_registered_reflection_foundation_audit.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_registered_reflection_foundation_audit_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"inputs={inputsValid} registered={result.definitionAudit.theoremRegisteredFieldCount}/{result.definitionAudit.requiredFieldCount} partial={result.definitionAudit.sourceDefinedOrPartialCandidateCount} phase515={phase515MayBeCreated}");

static Binding Bind(string id, string path, string expected)
{
    string actual = File.Exists(path) ? Sha(File.ReadAllBytes(path)) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}

static string JsonString(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
    return doc.RootElement.GetProperty(property).GetString() ?? string.Empty;
}

static bool JsonBool(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
    return doc.RootElement.GetProperty(property).GetBoolean();
}

static bool JsonNestedBool(string path, string parent, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
    return doc.RootElement.GetProperty(parent).GetProperty(property).GetBoolean();
}

static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class Binding(string id, string path, string expectedSha256, string actualSha256, bool hashMatches)
{
    public string Id { get; } = id;
    public string Path { get; } = path;
    public string ExpectedSha256 { get; } = expectedSha256;
    public string ActualSha256 { get; } = actualSha256;
    public bool HashMatches { get; } = hashMatches;
}
