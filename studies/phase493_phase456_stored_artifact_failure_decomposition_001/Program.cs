using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string ArtifactPath = "studies/phase456_consolidated_n4_launch_001/production/phase452_n4/scalar_channel_spectroscopy_probe.json";
const string ArtifactSummaryPath = "studies/phase456_consolidated_n4_launch_001/production/phase452_n4/scalar_channel_spectroscopy_probe_summary.json";
const string Phase456OutputPath = "studies/phase456_consolidated_n4_launch_001/output/consolidated_n4_launch_summary.json";
const string Phase456ProgramPath = "studies/phase456_consolidated_n4_launch_001/Program.cs";
const string AnalyzerPath = "studies/phase456_consolidated_n4_launch_001/ProductionAnalysis.cs";
const string AuthorizationPath = "studies/phase456_consolidated_n4_launch_001/production_authorization.json";
const string PackDir = "studies/phase456_consolidated_n4_launch_001/preregistration";
const string OutputDir = "studies/phase493_phase456_stored_artifact_failure_decomposition_001/output";
const string OutputSlug = "phase456_stored_artifact_failure_decomposition";
const string PinnedArtifactSha256 = "9b7e965a0b8ac906bc1352f908b28b6eb22511579c02ee91562f63beb67ed9cb";
const string PinnedPackSha256 = "40fd3c3488f94d18f50961e85d0bb3a3eabd1a31a071b61149875b8cf3d437aa";
const string PinnedAuthorizationSha256 = "b097b0504c6eafdf79523ef7913c69ab37fe086411334fea7e91c9fc9be70642";
const string DecisionContractCanonical =
    "phase493-a8-v1|precedence=invalid-committed-artifact>mixed-failure-supported>sampler-defect-supported>estimator-domain-failure-supported>statistics-limited-supported>failure-source-unresolved|" +
    "sampler-defect=primitive-sampler-or-exact-control-defect-observed|estimator-domain=retained-full-or-jackknife-correlator-outside-frozen-cosh-domain|" +
    "statistics-limited=verdict-row-neff-below-100|mixed=at-least-two-supported-components|read-only-no-rehabilitation-no-authority";

string[] packFiles = { "a4_symmetry_irrep_projectors.json", "a5_gaussian_domination_stage_a.json", "pack_manifest.json" };
string[] taxonomy =
{
    "sampler-defect-supported", "estimator-domain-failure-supported", "statistics-limited-supported",
    "mixed-failure-supported", "failure-source-unresolved", "invalid-committed-artifact",
};
var errors = new List<string>();

FrozenJson artifact = ReadJson(ArtifactPath, errors);
FrozenJson artifactSummary = ReadJson(ArtifactSummaryPath, errors);
FrozenJson phase456 = ReadJson(Phase456OutputPath, errors);
FrozenFile programSource = ReadFile(Phase456ProgramPath, errors);
FrozenFile analyzerSource = ReadFile(AnalyzerPath, errors);
FrozenFile authorization = ReadFile(AuthorizationPath, errors);
FrozenFile[] packParts = packFiles.Select(name => ReadFile(Path.Combine(PackDir, name), errors)).ToArray();

bool artifactTwinsExact = artifact.Bytes is not null && artifactSummary.Bytes is not null && artifact.Bytes.SequenceEqual(artifactSummary.Bytes);
bool artifactHashPinned = artifact.Sha256 == PinnedArtifactSha256 && artifactSummary.Sha256 == PinnedArtifactSha256;
bool authorizationHashPinned = authorization.Sha256 == PinnedAuthorizationSha256;
string? computedPackSha256 = packParts.All(x => x.Bytes is not null)
    ? Sha256Bytes(packParts.SelectMany(x => x.Bytes!).ToArray()) : null;
bool packHashPinned = computedPackSha256 == PinnedPackSha256;
if (!artifactTwinsExact) errors.Add("Committed production artifact twins are not byte-identical.");
if (!artifactHashPinned) errors.Add("Committed production artifact hash does not match the pinned Phase456 record.");
if (!authorizationHashPinned) errors.Add("Committed production authorization hash drifted.");
if (!packHashPinned) errors.Add("Committed preregistration pack concatenation hash drifted.");

bool productionDefaultsVerified = false, storageVerified = false, primitiveSamplerGatesPassed = false;
bool exactGaussianPipelinePassed = false, freeFieldGatePassed = false, allColumnNEffGatePassed = false;
bool phase456TerminalPinned = false, phase456AnalysisInvalid = false, phase456FamilyThresholdWithheld = false;
double sd2GenericNEff = double.NaN, sd2A2NEff = double.NaN, sd2KMinNEff = double.NaN;
var controlGateRows = new List<object>();
var rowDomainFindings = new List<object>();
int independentlyInvalidMassRows = 0;

try
{
    JsonElement root = artifact.Document!.RootElement;
    JsonElement run = root.GetProperty("phase456Run");
    productionDefaultsVerified = S(run, "mode") == "production" && B(run, "committedDefaults") &&
        B(run, "environmentOverridesRefused") && S(run, "pinnedPackSha256") == PinnedPackSha256 &&
        S(run, "computedPackSha256") == PinnedPackSha256;
    storageVerified = B(run, "perSiteCorrelatorStorage") && B(run, "perFaceTypeResolutionRetained");
    JsonElement batteries = root.GetProperty("batteries");
    primitiveSamplerGatesPassed = B(batteries, "samplerGates");
    exactGaussianPipelinePassed = B(batteries, "gaussSimBattery") && D(batteries, "gaussSimWorstZ") <= 5.0;
    freeFieldGatePassed = B(batteries, "freeFieldGate");
    allColumnNEffGatePassed = B(batteries, "nEffGateAll");

    JsonElement torus = root.GetProperty("tori").EnumerateArray().Single(x => I(x, "torusN") == 4);
    JsonElement prod = torus.GetProperty("columns").EnumerateArray().Single(x =>
        S(x, "member") == "sd2-id0/c0.5" && S(x, "kind") == "production" && D(x, "beta") == 1.0);
    sd2GenericNEff = D(prod, "nEff");
    JsonElement rowNEff = prod.GetProperty("phase456").GetProperty("rowEffectiveSampleSizes");
    sd2A2NEff = D(rowNEff, "nEffA2");
    sd2KMinNEff = D(rowNEff, "nEffKMin");

    foreach (JsonElement gate in torus.GetProperty("freeFieldGates").EnumerateArray())
    {
        double? analytic = DN(gate, "analyticMeffO1");
        double? measured = DN(gate, "measuredMeffO1");
        bool gatePassed = B(gate, "gateO1");
        controlGateRows.Add(new
        {
            member = S(gate, "member"), analyticMeffO1 = analytic, measuredMeffO1 = measured,
            measuredMeffO1Sigma = DN(gate, "measuredMeffO1Sigma"), gateO1 = gatePassed,
            analyticFinite = analytic is double a && double.IsFinite(a), measuredFinite = measured is double m && double.IsFinite(m),
        });
    }

    JsonElement gaussian = torus.GetProperty("gaussianSimBattery").EnumerateArray()
        .Single(x => S(x, "member") == "sd2-id0/c0.5").GetProperty("phase456ExactGaussianControl");
    JsonElement p456 = prod.GetProperty("phase456");
    AddDomain("a1-gevp-gap/production", p456.GetProperty("identityIrrep2x2Gevp").GetProperty("dominantEigenvalue"), rowDomainFindings, ref independentlyInvalidMassRows);
    AddDomain("a1-gevp-gap/exact-gaussian", gaussian.GetProperty("identityIrrep2x2Gevp").GetProperty("dominantEigenvalue"), rowDomainFindings, ref independentlyInvalidMassRows);
    AddDomain("a2-gap/production", p456.GetProperty("a2").GetProperty("o1").GetProperty("c"), rowDomainFindings, ref independentlyInvalidMassRows);
    AddDomain("a2-gap/exact-gaussian", gaussian.GetProperty("a2").GetProperty("o1").GetProperty("c"), rowDomainFindings, ref independentlyInvalidMassRows);
    AddDomain("kmin-dispersion/production", p456.GetProperty("kMin").GetProperty("spatialAxisAverageO1").GetProperty("c"), rowDomainFindings, ref independentlyInvalidMassRows);
    AddDomain("kmin-dispersion/exact-gaussian", gaussian.GetProperty("kMin").GetProperty("spatialAxisAverageO1").GetProperty("c"), rowDomainFindings, ref independentlyInvalidMassRows);

    JsonElement p456Root = phase456.Document!.RootElement;
    phase456TerminalPinned = S(p456Root, "verdictKind") == "production-analysis-invalid" &&
        S(p456Root, "terminalStatus") == "consolidated-n4-launch-production-analysis-invalid";
    JsonElement analysis = p456Root.GetProperty("productionAnalysis");
    phase456AnalysisInvalid = !B(analysis, "inputShapeValid") && S(analysis, "verdictKind") == "production-analysis-invalid";
    phase456FamilyThresholdWithheld = analysis.GetProperty("familyWiseSigmaThreshold").ValueKind == JsonValueKind.Null &&
        analysis.GetProperty("appliedSigmaThreshold").ValueKind == JsonValueKind.Null &&
        analysis.GetProperty("rows").EnumerateArray().Count(x => S(x, "terminal") == "invalid") == 5;
}
catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or NullReferenceException)
{
    errors.Add("Committed Phase456 schema/semantic reconstruction failed: " + ex.Message);
}

if (!productionDefaultsVerified) errors.Add("Production default/environment/pack provenance is not exact.");
if (!storageVerified) errors.Add("Required retained per-site/per-face storage flags are false.");
if (!phase456TerminalPinned || !phase456AnalysisInvalid) errors.Add("Phase456 fail-closed terminal is missing or drifted.");
if (!phase456FamilyThresholdWithheld) errors.Add("Phase456 any-invalid-row family firewall was not reproduced.");

bool inputsValid = errors.Count == 0;
bool samplerDefectSupported = inputsValid && (!primitiveSamplerGatesPassed || !exactGaussianPipelinePassed);
bool estimatorDomainFailureSupported = inputsValid && independentlyInvalidMassRows >= 2 &&
    controlGateRows.Count == 2 && controlGateRows.All(x => !BoolProperty(x, "measuredFinite"));
bool statisticsLimitationSupported = inputsValid && sd2GenericNEff < 100.0 && sd2KMinNEff < 100.0;
bool channelEstimatorCompatibilityFailureSupported = inputsValid && rowDomainFindings.Count >= 6 && independentlyInvalidMassRows >= 2;
const bool channelValidityFailureSupported = false;
const bool underlyingObservableInvalidSupported = false;
bool familyFirewallPropagationVerified = inputsValid && phase456FamilyThresholdWithheld;
int supportedPrimaryComponentCount = new[] { samplerDefectSupported, estimatorDomainFailureSupported, statisticsLimitationSupported }.Count(x => x);
string failureClassification = !inputsValid ? "invalid-committed-artifact"
    : supportedPrimaryComponentCount >= 2 ? "mixed-failure-supported"
    : samplerDefectSupported ? "sampler-defect-supported"
    : estimatorDomainFailureSupported ? "estimator-domain-failure-supported"
    : statisticsLimitationSupported ? "statistics-limited-supported"
    : "failure-source-unresolved";
bool taxonomyExact = taxonomy.Contains(failureClassification);

const bool phase456ArtifactMutated = false;
const bool phase456TerminalReinterpreted = false;
const bool hmcRun = false;
const bool benchmarkRun = false;
const bool phase481PackConstructed = false;
const bool phase458G3Satisfied = false;
const bool phase458G5Satisfied = false;
const bool samplingAuthorized = false;
const bool productionAuthorized = false;
const bool sourceContractApplicationAllowed = false;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
bool a8BoundaryHeld = !phase456ArtifactMutated && !phase456TerminalReinterpreted && !hmcRun && !benchmarkRun &&
    !phase481PackConstructed && !phase458G3Satisfied && !phase458G5Satisfied && !samplingAuthorized &&
    !productionAuthorized && !sourceContractApplicationAllowed && noGevPromotion && promotedPhysicalMassClaimCount == 0;

string decisionContractSha256 = Sha256Text(DecisionContractCanonical);
var result = new
{
    phaseId = "phase493-phase456-stored-artifact-failure-decomposition",
    terminalStatus = $"phase456-stored-artifact-failure-decomposition-{failureClassification}",
    verdictKind = failureClassification,
    inputsValid,
    failureClassification,
    decisionContractSha256,
    applicationSubjectKind = "phase456-stored-artifact-read-only-forensics",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A8; PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15 Phase493",
    explorationLane = true,
    explorationOnly = true,
    confirmationEvidence = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    decisionContract = new { schemaId = "phase493-a8-failure-decomposition-v1", canonical = DecisionContractCanonical, sha256 = decisionContractSha256, taxonomy, taxonomyExact },
    frozenInputs = new
    {
        artifact = FreezeView(artifact), artifactSummary = FreezeView(artifactSummary), artifactTwinsExact, artifactHashPinned,
        phase456Output = FreezeView(phase456), phase456Program = FreezeView(programSource), analyzerSource = FreezeView(analyzerSource),
        authorization = FreezeView(authorization), authorizationHashPinned,
        preregistrationPack = new { files = packFiles.Zip(packParts).Select(x => new { path = x.First, sha256 = x.Second.Sha256, byteCount = x.Second.ByteCount }), computedPackSha256, pinnedPackSha256 = PinnedPackSha256, packHashPinned },
        inputErrors = errors,
    },
    componentFindings = new
    {
        sampler = new
        {
            samplerDefectSupported, primitiveSamplerGatesPassed, exactGaussianPipelinePassed, freeFieldGatePassed,
            interpretation = "Primitive energy/virial sampler gates and the exact-Gaussian pipeline pass. The observable-level free-field gate is red because its frozen mass estimator is non-finite; retained evidence therefore does not affirm a sampler implementation defect.",
        },
        estimatorDomain = new
        {
            estimatorDomainFailureSupported, independentlyInvalidMassRows, rowDomainFindings,
            frozenDomain = "At T=4 the Phase456 estimator uses C(1)/C(2) and requires a finite ratio strictly greater than one.",
        },
        statistics = new
        {
            statisticsLimitationSupported, nEffFloor = 100.0, sd2GenericNEff, sd2A2NEff, sd2KMinNEff, allColumnNEffGatePassed,
            affectedVerdictRows = new[] { "a1-gevp-gap", "kmin-dispersion" },
        },
        channelValidity = new
        {
            channelValidityFailureSupported, channelEstimatorCompatibilityFailureSupported, underlyingObservableInvalidSupported,
            interpretation = "Retained sign/ratio patterns violate the single-cosh estimator domain in multiple production/control channels. This supports a channel/estimator compatibility problem, but Phase493 does not declare the underlying observable physically invalid.",
        },
        familyFirewall = new
        {
            familyFirewallPropagationVerified, allFiveRowsTerminalInvalid = phase456FamilyThresholdWithheld,
            familyWiseThresholdWithheld = phase456FamilyThresholdWithheld, appliedThresholdWithheld = phase456FamilyThresholdWithheld,
            interpretation = "A non-finite mass row prevents aligned family calibration; the committed any-invalid-row rule consequently marks all five rows invalid without erasing finite retained estimates.",
        },
        supportedPrimaryComponentCount,
    },
    controlGateRows,
    failureSourceResolution = new
    {
        classification = failureClassification,
        samplerCorrectnessAtObservableLevelResolved = false,
        estimatorFailureResolved = estimatorDomainFailureSupported,
        statisticsFailureResolved = statisticsLimitationSupported,
        retainedArtifactCanDiagnoseWithoutRehabilitation = inputsValid,
    },
    phase456ArtifactMutated,
    phase456TerminalReinterpreted,
    hmcRun,
    benchmarkRun,
    zeroNewSampling = true,
    zeroPhysicsCompute = true,
    phase481PackConstructed,
    phase458G3Satisfied,
    phase458G5Satisfied,
    phase458GateSatisfied = false,
    samplingAuthorized,
    productionAuthorized,
    humanRulingAuthored = false,
    o4Discharged = false,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    acceptedContractFieldCount = 0,
    fieldsAppliedToPhase201TemplateCount = 0,
    noGevPromotion,
    promotedPhysicalMassClaimCount,
    a8BoundaryHeld,
    decision = failureClassification switch
    {
        "mixed-failure-supported" => "The immutable Phase456 artifact supports both a frozen-estimator domain failure and independent effective-sample-size limitations. Primitive sampler checks do not support an implementation defect. Preserve the production-analysis-invalid terminal; use this decomposition only to design prospective oracle tests.",
        "estimator-domain-failure-supported" => "The retained correlators independently reproduce a failure of the frozen estimator domain. This does not rehabilitate Phase456 or prove the sampler correct.",
        "statistics-limited-supported" => "The retained effective sample sizes independently fail the pre-registered floor. This does not authorize additional sampling.",
        "sampler-defect-supported" => "At least one primitive sampler or exact-control check is defective in the retained record. No repair or rerun is authorized here.",
        "invalid-committed-artifact" => "An exact committed input, schema, hash, or fail-closed terminal drifted. Forensic classification fails closed.",
        _ => "The retained evidence does not isolate a supported failure source. Preserve the unresolved negative result.",
    },
};

Directory.CreateDirectory(OutputDir);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(OutputDir, $"{OutputSlug}.json"), json);
File.WriteAllText(Path.Combine(OutputDir, $"{OutputSlug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"inputsValid={inputsValid} estimatorDomain={estimatorDomainFailureSupported} statistics={statisticsLimitationSupported} samplerDefect={samplerDefectSupported} a8BoundaryHeld={a8BoundaryHeld}");

static void AddDomain(string rowId, JsonElement values, List<object> findings, ref int invalidCount)
{
    double? c1 = values.GetArrayLength() > 1 && values[1].ValueKind == JsonValueKind.Number ? values[1].GetDouble() : null;
    double? c2 = values.GetArrayLength() > 2 && values[2].ValueKind == JsonValueKind.Number ? values[2].GetDouble() : null;
    double? ratio = c1 is double a && c2 is double b && b != 0 ? a / b : null;
    bool inDomain = ratio is double r && double.IsFinite(r) && r > 1.0;
    if (!inDomain) invalidCount++;
    findings.Add(new { rowId, c1, c2, ratio, finite = ratio is double finiteRatio && double.IsFinite(finiteRatio), strictlyGreaterThanOne = ratio is > 1.0, inFrozenCoshDomain = inDomain });
}

static FrozenJson ReadJson(string path, List<string> errors)
{
    FrozenFile file = ReadFile(path, errors);
    if (file.Bytes is null) return new FrozenJson(path, null, 0, null, null);
    try { return new FrozenJson(path, file.Sha256, file.ByteCount, file.Bytes, JsonDocument.Parse(file.Bytes)); }
    catch (JsonException) { errors.Add("Malformed strict JSON: " + path); return new FrozenJson(path, file.Sha256, file.ByteCount, file.Bytes, null); }
}

static FrozenFile ReadFile(string path, List<string> errors)
{
    try { byte[] bytes = File.ReadAllBytes(path); return new FrozenFile(path, Sha256Bytes(bytes), bytes.LongLength, bytes); }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { errors.Add($"Unreadable input {path}: {ex.GetType().Name}"); return new FrozenFile(path, null, 0, null); }
}

static object FreezeView(FrozenFile file) => new { file.Path, file.Sha256, file.ByteCount, present = file.Bytes is not null };
static string Sha256Bytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string Sha256Text(string text) => Sha256Bytes(Encoding.UTF8.GetBytes(text));
static string S(JsonElement e, string p) => e.GetProperty(p).GetString() ?? throw new InvalidOperationException($"{p} is null");
static bool B(JsonElement e, string p) => e.GetProperty(p).GetBoolean();
static int I(JsonElement e, string p) => e.GetProperty(p).GetInt32();
static double D(JsonElement e, string p) => e.GetProperty(p).GetDouble();
static double? DN(JsonElement e, string p) => e.GetProperty(p).ValueKind == JsonValueKind.Null ? null : e.GetProperty(p).GetDouble();
static bool BoolProperty(object value, string name) => (bool)(value.GetType().GetProperty(name)?.GetValue(value) ?? false);

internal record FrozenFile(string Path, string? Sha256, long ByteCount, byte[]? Bytes);
internal sealed record FrozenJson(string Path, string? Sha256, long ByteCount, byte[]? Bytes, JsonDocument? Document)
    : FrozenFile(Path, Sha256, ByteCount, Bytes);
