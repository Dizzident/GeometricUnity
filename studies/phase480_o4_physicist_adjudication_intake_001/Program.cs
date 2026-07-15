using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

// Phase480: fail-closed external human physicist memo intake. This phase
// authenticates supplied bytes; it never authors, chooses, or infers a ruling.
var stopwatch = Stopwatch.StartNew();
const string Slug = "o4_physicist_adjudication_intake";
const string OutputDir = "studies/phase480_o4_physicist_adjudication_intake_001/output";
const string ContractPath = "studies/phase480_o4_physicist_adjudication_intake_001/preregistration/o4_intake_contract_v1.json";
const string ReviewerRegistryPath = "studies/phase480_o4_physicist_adjudication_intake_001/preregistration/o4_reviewer_registry_v1.json";
const string VerifierPath = "studies/phase480_o4_physicist_adjudication_intake_001/verify_intake.js";
const string VerifierTestPath = "studies/phase480_o4_physicist_adjudication_intake_001/verify_intake_test.js";
const string MemoSchemaPath = "scripts/o4_register/o4_human_memo_schema_v1.json";

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

var start = new ProcessStartInfo
{
    FileName = "node",
    WorkingDirectory = Directory.GetCurrentDirectory(),
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
};
start.ArgumentList.Add("studies/phase480_o4_physicist_adjudication_intake_001/verify_intake.js");
using var process = Process.Start(start) ?? throw new InvalidOperationException("Failed to start Phase480 intake verifier.");
string stdout = process.StandardOutput.ReadToEnd();
string stderr = process.StandardError.ReadToEnd();
process.WaitForExit();
if (process.ExitCode != 0)
    throw new InvalidOperationException($"Phase480 verifier failed closed (exit {process.ExitCode}): {stderr}{stdout}");

using var verificationDocument = JsonDocument.Parse(stdout);
JsonElement verification = verificationDocument.RootElement.Clone();
bool valid = verification.GetProperty("valid").GetBoolean();
bool inputPresent = verification.GetProperty("inputPresent").GetBoolean();
string terminal = verification.GetProperty("terminal").GetString()
    ?? "awaiting-external-physicist-ruling";
bool cryptographicVerificationPerformed = valid
    && verification.GetProperty("cryptographicVerificationPerformed").GetBoolean();
bool repositoryBindingVerified = valid
    && verification.GetProperty("repositoryBindingVerified").GetBoolean();
bool reviewedArtifactSetsVerified = valid
    && verification.GetProperty("reviewedArtifactSetsVerified").GetBoolean();
if (valid && (!cryptographicVerificationPerformed || !repositoryBindingVerified || !reviewedArtifactSetsVerified))
    throw new InvalidOperationException("Phase480 positive terminal lacks its full authentication conjunction.");
JsonElement normalizedRulings = valid
    ? verification.GetProperty("normalizedRulings").Clone()
    : JsonSerializer.SerializeToElement(Array.Empty<object>());
string signatureVerificationStatus = valid
    ? verification.GetProperty("signatureVerificationStatus").GetString()!
    : "not-performed-input-absent-or-invalid";

var result = new
{
    phaseId = "phase480-o4-physicist-adjudication-intake",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = $"o4-physicist-adjudication-intake-{terminal}",
    interimTerminal = terminal,
    verdictKind = terminal,
    applicationSubjectKind = "o4-physicist-adjudication-intake",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A4",
    waveOrder = 4,
    skeletonBuilt = false,
    zeroPhysicsCompute = true,
    intakeImplemented = true,
    awaitingImplementation = false,
    awaitingExternalPhysicistRuling = !valid,
    executionPriorityDependencies = new[] { "phase477", "phase478", "phase479" },
    intakeContractPath = ContractPath,
    intakeContractSha256 = Sha256(ContractPath),
    reviewerRegistryPath = ReviewerRegistryPath,
    reviewerRegistrySha256 = Sha256(ReviewerRegistryPath),
    verifierPath = VerifierPath,
    verifierSha256 = Sha256(VerifierPath),
    verifierTestPath = VerifierTestPath,
    verifierTestSha256 = Sha256(VerifierTestPath),
    memoSchemaPath = MemoSchemaPath,
    memoSchemaSha256 = Sha256(MemoSchemaPath),
    memoInputPath = "studies/phase480_o4_physicist_adjudication_intake_001/input/o4_human_physicist_memo_v1.json",
    inputPresent,
    genuineSignedMemoPresent = valid,
    rulingConsumed = valid,
    rulingAuthoredOrInferred = false,
    cryptographicVerificationPerformed,
    repositoryBindingVerified,
    reviewedArtifactSetsVerified,
    externalMemoValidated = valid,
    humanAuthorshipValidated = valid && verification.GetProperty("humanAuthorshipValidated").GetBoolean(),
    repositoryBindingValidated = valid && verification.GetProperty("repositoryBindingValidated").GetBoolean(),
    signedPayloadHashValidated = valid && verification.GetProperty("signedPayloadHashValidated").GetBoolean(),
    signatureProvenanceValidated = valid && verification.GetProperty("signatureProvenanceValidated").GetBoolean(),
    signatureVerificationStatus,
    syntheticOrTemplateInput = false,
    rulingContentMachineAuthoredOrInferred = false,
    normalizedRulings,
    acceptedSignatureMode = "ed25519-detached",
    unsupportedSignatureModesFailClosed = true,
    riskAcceptanceCountsAsRuling = false,
    shapeValidationCountsAsSignatureVerification = false,
    verification,
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
    holdLifted = false,
    decision = valid
        ? "A genuine externally authored memo passed strict JSON, exact semantic-shape, repository-binding, reviewed-artifact-set, RFC8785-JCS payload-hash, reviewer-registry, and Ed25519 signature verification. Phase480 records authentication only; it authors no ruling, lifts no hold, fills no source contract, and promotes nothing."
        : "No cryptographically authenticated external human physicist memo is available. Missing, malformed, stale, synthetic, template, unbound, or unverifiable input remains at awaiting-external-physicist-ruling; no ruling is consumed or inferred.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, $"{Slug}.json"), json);
File.WriteAllText(Path.Combine(OutputDir, $"{Slug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"intakeImplemented=True inputPresent={inputPresent} authenticated={valid} promotedPhysicalMassClaimCount=0");
