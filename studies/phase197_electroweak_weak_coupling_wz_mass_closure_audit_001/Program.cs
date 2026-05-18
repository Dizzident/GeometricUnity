using System.Text.Json;

const string DefaultOutputDir = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output";
const string Phase68Path = "studies/phase68_normalized_weak_coupling_candidate_promotion_001/normalized_weak_coupling_candidate_promotion.json";
const string Phase69Path = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase74Path = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase75Path = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76Path = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE197_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase68 = JsonDocument.Parse(File.ReadAllText(Phase68Path));
using var phase69 = JsonDocument.Parse(File.ReadAllText(Phase69Path));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74Path));
using var phase75 = JsonDocument.Parse(File.ReadAllText(Phase75Path));
using var phase76 = JsonDocument.Parse(File.ReadAllText(Phase76Path));

var normalizedWeakCouplingPromoted = string.Equals(JsonString(phase68.RootElement, "terminalStatus"), "normalized-weak-coupling-candidate-promoted", StringComparison.Ordinal);
var massGenerationRelationDerived = string.Equals(JsonString(phase69.RootElement, "terminalStatus"), "electroweak-mass-generation-relation-derived", StringComparison.Ordinal);
var targetComparisonPassed = string.Equals(JsonString(phase74.RootElement, "terminalStatus"), "wz-absolute-mass-target-comparison-passed", StringComparison.Ordinal);
var generatorNormalizationCanExplainMiss = JsonBool(phase76.RootElement, "generatorNormalizationCanExplainMiss") is true;
var targetImpliedWeakCoupling = JsonDouble(phase75.RootElement, "requiredWeakCoupling");
var currentWeakCoupling = JsonDouble(phase75.RootElement, "currentWeakCoupling");

var comparisons = phase74.RootElement.GetProperty("comparisons")
    .EnumerateArray()
    .Select(row => new
    {
        observableId = JsonString(row, "observableId"),
        predictedValue = JsonDouble(row, "predictedValue"),
        targetValue = JsonDouble(row, "targetValue"),
        sigmaResidual = JsonDouble(row, "sigmaResidual"),
        passed = JsonBool(row, "passed"),
    })
    .ToArray();

var checks = new[]
{
    new Check("normalized-weak-coupling-promoted", normalizedWeakCouplingPromoted, $"P68 promoted current weak coupling g={currentWeakCoupling}."),
    new Check("mass-generation-relation-derived", massGenerationRelationDerived, "P69 derives m_W = g v / 2 and uses the internal W/Z ratio for Z."),
    new Check("absolute-target-comparison", targetComparisonPassed, "P74 absolute W/Z target comparison must pass before promotion."),
    new Check("generator-normalization-explains-miss", generatorNormalizationCanExplainMiss, "P76 shows canonical generator normalization cannot explain the coherent W/Z miss."),
};

var canPromoteWzFromWeakCouplingMassRelation = checks.All(check => check.Passed);
var terminalStatus = canPromoteWzFromWeakCouplingMassRelation
    ? "electroweak-weak-coupling-wz-mass-closure-promotable"
    : "electroweak-weak-coupling-wz-mass-closure-failed-comparison";

var result = new
{
    phaseId = "phase197-electroweak-weak-coupling-wz-mass-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteWzFromWeakCouplingMassRelation,
    currentWeakCoupling,
    targetImpliedWeakCoupling,
    requiredWeakCouplingScale = currentWeakCoupling is > 0 && targetImpliedWeakCoupling is not null
        ? targetImpliedWeakCoupling / currentWeakCoupling
        : null,
    comparisons,
    checks,
    decision = canPromoteWzFromWeakCouplingMassRelation
        ? "The promoted weak coupling and VEV mass-generation relation pass W/Z absolute target comparison."
        : "Do not promote W/Z absolute masses from the current promoted weak coupling. The relation is derived and target-independent, but it predicts W/Z masses that fail comparison; matching the targets would require the Phase75 target-implied weak coupling, which P76 treats as diagnostic-only.",
    nextRequiredArtifact = "A target-independent revision of the weak-coupling amplitude, raw matrix element, or scalar-sector relation that replaces the target-implied coupling and then passes W/Z absolute target comparison.",
    sourceEvidence = new
    {
        phase68Path = Phase68Path,
        phase69Path = Phase69Path,
        phase74Path = Phase74Path,
        phase75Path = Phase75Path,
        phase76Path = Phase76Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electroweak_weak_coupling_wz_mass_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_weak_coupling_wz_mass_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteWzFromWeakCouplingMassRelation,
        result.currentWeakCoupling,
        result.targetImpliedWeakCoupling,
        result.requiredWeakCouplingScale,
        result.comparisons,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteWzFromWeakCouplingMassRelation={canPromoteWzFromWeakCouplingMassRelation}");
Console.WriteLine($"currentWeakCoupling={currentWeakCoupling}");
Console.WriteLine($"targetImpliedWeakCoupling={targetImpliedWeakCoupling}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record Check(string CheckId, bool Passed, string Detail);
