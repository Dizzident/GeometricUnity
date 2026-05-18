using System.Text.Json;

const string DefaultOutputDir = "studies/phase175_massless_gauge_identity_split_feasibility_001/output";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string GeometryPath = "studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json";
const string SignatureDir = "studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures";
const string TargetsPath = "studies/phase18_experimental_targets_001/physical_targets.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE175_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));
using var geometry = JsonDocument.Parse(File.ReadAllText(GeometryPath));
using var targets = JsonDocument.Parse(File.ReadAllText(TargetsPath));
using var phase177 = File.Exists(Phase177Path) ? JsonDocument.Parse(File.ReadAllText(Phase177Path)) : null;

double dominantFractionThreshold = 0.80;
double dominanceGapThreshold = 0.20;
var targetIds = targets.RootElement.GetProperty("targets").EnumerateArray()
    .Select(target => RequiredString(target, "observableId"))
    .ToHashSet(StringComparer.Ordinal);
bool photonTargetContractPresent = targetIds.Contains("physical-photon-masslessness");
bool gluonTargetContractPresent = targetIds.Contains("physical-gluon-masslessness");
if (phase177 is not null && phase177.RootElement.TryGetProperty("contracts", out var benchmarkContracts))
{
    foreach (var contract in benchmarkContracts.EnumerateArray())
    {
        if (JsonBool(contract, "benchmarkContractPresent") is not true)
            continue;
        string observableId = RequiredString(contract, "observableId");
        if (observableId == "physical-photon-masslessness")
            photonTargetContractPresent = true;
        if (observableId == "physical-gluon-masslessness")
            gluonTargetContractPresent = true;
    }
}

var signaturePaths = Directory.EnumerateFiles(SignatureDir, "*.json")
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();
var modeAudits = signaturePaths
    .Select(path => AuditModeSignature(path, dominantFractionThreshold, dominanceGapThreshold))
    .ToArray();
var backgrounds = modeAudits
    .GroupBy(audit => audit.BackgroundId, StringComparer.Ordinal)
    .Select(group => new BackgroundAudit(
        group.Key,
        group.Count(),
        group.SelectMany(audit => audit.GaugeEnergyFractions.Select((fraction, index) => (fraction, index)))
            .GroupBy(item => item.index)
            .OrderBy(grouped => grouped.Key)
            .Select(grouped => grouped.Average(item => item.fraction))
            .ToArray(),
        group.Count(audit => audit.DominantGaugeComponent is not null),
        group.Where(audit => audit.DominantGaugeComponent is not null)
            .GroupBy(audit => audit.DominantGaugeComponent!.Value)
            .OrderBy(grouped => grouped.Key)
            .ToDictionary(grouped => $"gauge-component-{grouped.Key}", grouped => grouped.Count(), StringComparer.Ordinal)))
    .OrderBy(audit => audit.BackgroundId, StringComparer.Ordinal)
    .ToArray();

int gaugeComponentCount = modeAudits.FirstOrDefault()?.GaugeEnergyFractions.Count ?? 0;
int protectedSubspaceDimension = JsonInt(phase174.RootElement, "protectedSubspaceDimension") ?? 0;
bool protectedSubspaceReady = JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;
bool oneDimensionalAbelianSectorArtifactPresent = false;
bool colorOctetSectorArtifactPresent = false;
bool dominantOneComponentFamilyPresent = modeAudits.Any(audit => audit.DominantGaugeComponent is not null);
bool dominantComponentStableAcrossBackgrounds = backgrounds.Length > 1
    && backgrounds.Select(background => string.Join(",", background.DominantComponentCounts.Select(kvp => $"{kvp.Key}:{kvp.Value}"))).Distinct(StringComparer.Ordinal).Count() == 1;
bool u1IdentityCandidatePresent = oneDimensionalAbelianSectorArtifactPresent || (dominantOneComponentFamilyPresent && dominantComponentStableAcrossBackgrounds);
bool colorOctetIdentityCandidatePresent = colorOctetSectorArtifactPresent || gaugeComponentCount >= 8;
bool knownPhotonPredictionAllowed = protectedSubspaceReady && u1IdentityCandidatePresent && photonTargetContractPresent;
bool knownGluonPredictionAllowed = protectedSubspaceReady && colorOctetIdentityCandidatePresent && gluonTargetContractPresent;
bool anyKnownMasslessPredictionAllowed = knownPhotonPredictionAllowed || knownGluonPredictionAllowed;

string terminalStatus = anyKnownMasslessPredictionAllowed
    ? "massless-gauge-identity-split-known-prediction-ready"
    : protectedSubspaceReady
        ? "massless-gauge-identity-split-blocked-current-artifacts-su2-only"
        : "massless-gauge-identity-split-blocked-no-protected-subspace";

var result = new
{
    phaseId = "phase175-massless-gauge-identity-split-feasibility",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    dominantFractionThreshold,
    dominanceGapThreshold,
    protectedSubspaceReady,
    protectedSubspaceDimension,
    geometrySummary = new
    {
        baseFaceCount = JsonInt(geometry.RootElement.GetProperty("baseSpace"), "faceCount"),
        ambientFaceCount = JsonInt(geometry.RootElement.GetProperty("ambientSpace"), "faceCount"),
    },
    signatureCount = modeAudits.Length,
    gaugeComponentCount,
    backgroundAudits = backgrounds,
    modeAudits,
    identitySplitAudit = new
    {
        oneDimensionalAbelianSectorArtifactPresent,
        colorOctetSectorArtifactPresent,
        dominantOneComponentFamilyPresent,
        dominantComponentStableAcrossBackgrounds,
        u1IdentityCandidatePresent,
        colorOctetIdentityCandidatePresent,
        photonTargetContractPresent,
        gluonTargetContractPresent,
        knownPhotonPredictionAllowed,
        knownGluonPredictionAllowed,
        anyKnownMasslessPredictionAllowed,
    },
    blockers = anyKnownMasslessPredictionAllowed
        ? Array.Empty<string>()
        : new[]
        {
            "current mode signatures expose a 3-component canonical gauge basis, not an 8-component color basis",
            "no one-dimensional Abelian U(1) sector artifact is present",
            "no color-octet sector artifact is present",
            photonTargetContractPresent && gluonTargetContractPresent
                ? "photon/gluon masslessness benchmark contracts are present, but identity split evidence is still absent"
                : "no active photon/gluon masslessness benchmark contracts are present",
            "the protected massless subspace remains a generic gauge-sector diagnostic rather than a known photon/gluon identity split",
        },
    nextWork = anyKnownMasslessPredictionAllowed
        ? "wire allowed known massless prediction rows into Phase148-151"
        : "derive a target-independent U(1) and/or color-octet sector decomposition artifact for the protected massless subspace, then add photon/gluon benchmark contracts",
    sourceEvidence = new
    {
        phase174Path = Phase174Path,
        geometryPath = GeometryPath,
        signatureDir = SignatureDir,
        targetsPath = TargetsPath,
        phase177Path = File.Exists(Phase177Path) ? Phase177Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "massless_gauge_identity_split_feasibility.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "massless_gauge_identity_split_feasibility_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.protectedSubspaceDimension,
        result.gaugeComponentCount,
        result.identitySplitAudit,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"protectedSubspaceDimension={protectedSubspaceDimension}");
Console.WriteLine($"gaugeComponentCount={gaugeComponentCount}");
Console.WriteLine($"knownPhotonPredictionAllowed={knownPhotonPredictionAllowed}");
Console.WriteLine($"knownGluonPredictionAllowed={knownGluonPredictionAllowed}");

static ModeSignatureAudit AuditModeSignature(string path, double dominantFractionThreshold, double dominanceGapThreshold)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
    var shape = root.GetProperty("observedShape").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    var values = root.GetProperty("observedCoefficients").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    int faceCount = shape[0];
    int gaugeCount = shape[1];
    var energy = new double[gaugeCount];
    for (int face = 0; face < faceCount; face++)
    {
        for (int gauge = 0; gauge < gaugeCount; gauge++)
        {
            double value = values[face * gaugeCount + gauge];
            energy[gauge] += value * value;
        }
    }

    double total = energy.Sum();
    var fractions = total > 0.0 ? energy.Select(value => value / total).ToArray() : new double[gaugeCount];
    var ranked = fractions.Select((fraction, index) => (fraction, index)).OrderByDescending(x => x.fraction).ToArray();
    double top = ranked.Length > 0 ? ranked[0].fraction : 0.0;
    double second = ranked.Length > 1 ? ranked[1].fraction : 0.0;
    int? dominant = top >= dominantFractionThreshold && top - second >= dominanceGapThreshold ? ranked[0].index : null;
    return new ModeSignatureAudit(
        RequiredString(root, "modeId"),
        RequiredString(root, "backgroundId"),
        shape,
        fractions,
        top,
        second,
        top - second,
        dominant,
        JsonString(root.GetProperty("observedSignature"), "lieAlgebraBasisId") ?? "unknown",
        JsonString(root.GetProperty("observedSignature"), "carrierType") ?? "unknown");
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ModeSignatureAudit(
    string ModeId,
    string BackgroundId,
    IReadOnlyList<int> ObservedShape,
    IReadOnlyList<double> GaugeEnergyFractions,
    double TopGaugeFraction,
    double SecondGaugeFraction,
    double DominanceGap,
    int? DominantGaugeComponent,
    string LieAlgebraBasisId,
    string CarrierType);
sealed record BackgroundAudit(
    string BackgroundId,
    int ModeCount,
    IReadOnlyList<double> MeanGaugeEnergyFractions,
    int DominantModeCount,
    IReadOnlyDictionary<string, int> DominantComponentCounts);
