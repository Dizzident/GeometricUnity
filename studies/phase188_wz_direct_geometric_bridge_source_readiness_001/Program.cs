using System.Text.Json;

const string DefaultOutputDir = "studies/phase188_wz_direct_geometric_bridge_source_readiness_001/output";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census.json";
const string Phase182Path = "studies/phase182_wz_operator_algebra_bridge_source_001/output/wz_operator_algebra_bridge_source.json";
const string Phase185Path = "studies/phase185_wz_operator_unit_scale_materialization_001/output/wz_operator_unit_scale_materialization.json";
const string Phase186Path = "studies/phase186_wz_source_shape_law_closure_001/output/wz_source_shape_law_closure.json";
const string GeometryPath = "studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json";
const string SpinorPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string DiracBundleDir = "studies/phase12_joined_calculation_001/output/background_family/fermions";
const string ModeSignatureDir = "studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures";

var outputDir = Environment.GetEnvironmentVariable("PHASE188_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase172 = JsonDocument.Parse(File.ReadAllText(Phase172Path));
using var phase182 = JsonDocument.Parse(File.ReadAllText(Phase182Path));
using var phase185 = JsonDocument.Parse(File.ReadAllText(Phase185Path));
using var phase186 = JsonDocument.Parse(File.ReadAllText(Phase186Path));
using var geometry = JsonDocument.Parse(File.ReadAllText(GeometryPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(SpinorPath));

var backgrounds = new[] { "bg-phase12-bg-a-20260315212202", "bg-phase12-bg-b-20260315212202" };
var diracBundleArtifacts = backgrounds
    .Select(backgroundId => new BackgroundArtifact(
        backgroundId,
        Path.Combine(DiracBundleDir, $"dirac_bundle_{backgroundId}.json"),
        File.Exists(Path.Combine(DiracBundleDir, $"dirac_bundle_{backgroundId}.json")),
        Path.Combine(DiracBundleDir, $"dirac_bundle_{backgroundId}.matrix.json"),
        File.Exists(Path.Combine(DiracBundleDir, $"dirac_bundle_{backgroundId}.matrix.json")),
        Directory.EnumerateFiles(ModeSignatureDir, $"{backgroundId}-mode-*.json").Count()))
    .ToArray();

int p172AssessmentCount = JsonInt(phase172.RootElement, "assessmentCount") ?? 0;
int p172RawPassingCount = JsonInt(phase172.RootElement, "rawGatePassingCount") ?? 0;
int p182CandidateCount = JsonInt(phase182.RootElement, "candidateCount") ?? 0;
int p182RawPassingCount = JsonInt(phase182.RootElement, "rawGatePassingCandidateCount") ?? 0;
int p182StableRawPassingCount = JsonInt(phase182.RootElement, "stableRawGatePassingCandidateCount") ?? 0;
bool p185UnitScaleMaterialized = JsonBool(phase185.RootElement, "unitScaleArtifactMaterialized") is true;
bool p185ReplayPromotable = phase185.RootElement.TryGetProperty("predictiveReplay", out var p185Replay)
    && JsonBool(p185Replay, "predictiveReplayPromotable") is true;
bool p186SourceShapePromotable = JsonBool(phase186.RootElement, "sourceShapeLawPromotable") is true;

bool geometryInputsPresent = File.Exists(GeometryPath) && File.Exists(SpinorPath);
bool siblingBackgroundInputsPresent = diracBundleArtifacts.All(artifact =>
    artifact.BundlePresent && artifact.MatrixPresent && artifact.ModeSignatureCount > 0);
bool exhaustedVariationFamiliesEstablished = p172AssessmentCount > 0
    && p172RawPassingCount == 0
    && p182CandidateCount > 0
    && p182RawPassingCount == 0
    && p182StableRawPassingCount == 0
    && !p186SourceShapePromotable;
bool targetIndependentConstruction = true;
bool explicitDirectSourceLawPresent = false;
bool stabilitySidecarPresent = false;
bool replayAllowed = explicitDirectSourceLawPresent
    && stabilitySidecarPresent
    && targetIndependentConstruction
    && geometryInputsPresent
    && siblingBackgroundInputsPresent;

var checks = new[]
{
    new ReadinessCheck("geometry-inputs-present", geometryInputsPresent, "Phase12 geometry and spinor representation artifacts are present."),
    new ReadinessCheck("sibling-background-inputs-present", siblingBackgroundInputsPresent, "Dirac bundles, matrices, and mode signatures exist for both sibling backgrounds."),
    new ReadinessCheck("exhausted-current-families-established", exhaustedVariationFamiliesEstablished, "P172/P182/P186 establish that current variation, operator-algebra, and source-shape families are exhausted."),
    new ReadinessCheck("operator-unit-scale-materialized", p185UnitScaleMaterialized, "P185 materialized the dimensional unit-scale artifact."),
    new ReadinessCheck("operator-unit-scale-replay-promotable", p185ReplayPromotable, "P185 replay must pass W/Z gates before the unit-scale artifact can promote masses."),
    new ReadinessCheck("direct-geometric-source-law-present", explicitDirectSourceLawPresent, "Missing explicit law mapping connection-mode geometry to W/Z Dirac-variation matrix elements outside exhausted families."),
    new ReadinessCheck("direct-source-stability-sidecar-present", stabilitySidecarPresent, "Missing sibling-background stability sidecar for a direct geometric W/Z bridge source."),
    new ReadinessCheck("target-independent-construction", targetIndependentConstruction, "This readiness contract excludes W/Z target values from construction."),
};

string terminalStatus = replayAllowed
    ? "wz-direct-geometric-bridge-source-readiness-replay-allowed"
    : "wz-direct-geometric-bridge-source-readiness-blocked-missing-direct-source-law";

var result = new
{
    phaseId = "phase188-wz-direct-geometric-bridge-source-readiness",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    replayAllowed,
    directBridgeSourceValidated = replayAllowed,
    checks,
    exhaustedFamilies = new
    {
        p172AssessmentCount,
        p172RawPassingCount,
        p182CandidateCount,
        p182RawPassingCount,
        p182StableRawPassingCount,
        p186SourceShapePromotable,
    },
    availableInputs = new
    {
        ambientSpace = geometry.RootElement.GetProperty("ambientSpace").Clone(),
        spinorComponents = JsonInt(spinor.RootElement, "spinorComponents"),
        diracBundleArtifacts,
    },
    forbiddenShortcuts = new[]
    {
        "target-implied W/Z scalar multipliers",
        "reusing P167/P169/P170 source-shape diagnostics as derivations",
        "rerunning P172/P182 exhausted variation/operator-algebra families as new evidence",
        "promoting P185 unit scale without a relative W/Z source-structure law",
    },
    decision = replayAllowed
        ? "A direct target-independent W/Z bridge-source law is present and stable; rerun W/Z prediction replay."
        : "Do not rerun W/Z as promotable. The missing artifact is an explicit target-independent geometric bridge-source law with sibling-background stability.",
    nextRequiredArtifact = "Direct target-independent W/Z geometric bridge-source law mapping connection-mode geometry to Dirac-variation matrix elements, with sibling-background stability and no W/Z target-fitted factors.",
    sourceEvidence = new
    {
        phase172Path = Phase172Path,
        phase182Path = Phase182Path,
        phase185Path = Phase185Path,
        phase186Path = Phase186Path,
        geometryPath = GeometryPath,
        spinorPath = SpinorPath,
        diracBundleDir = DiracBundleDir,
        modeSignatureDir = ModeSignatureDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_direct_geometric_bridge_source_readiness.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_direct_geometric_bridge_source_readiness_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.replayAllowed,
        result.directBridgeSourceValidated,
        result.checks,
        result.exhaustedFamilies,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"replayAllowed={replayAllowed}");
Console.WriteLine($"p172RawPassingCount={p172RawPassingCount}");
Console.WriteLine($"p182RawPassingCount={p182RawPassingCount}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record BackgroundArtifact(
    string BackgroundId,
    string BundlePath,
    bool BundlePresent,
    string MatrixPath,
    bool MatrixPresent,
    int ModeSignatureCount);

sealed record ReadinessCheck(string CheckId, bool Passed, string Detail);
