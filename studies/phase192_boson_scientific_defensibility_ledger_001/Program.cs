using System.Text.Json;

const string DefaultOutputDir = "studies/phase192_boson_scientific_defensibility_ledger_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase149Path = "studies/phase149_known_boson_predictability_contracts_001/output/known_boson_predictability_contracts.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase132Path = "studies/phase132_fermion_sector_label_derivation_source_gate_001/output/fermion_sector_label_derivation_source_gate_summary.json";
const string Phase184Path = "studies/phase184_massive_boson_prediction_closure_001/output/massive_boson_prediction_closure_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase195Path = "studies/phase195_electroweak_vev_wz_absolute_closure_audit_001/output/electroweak_vev_wz_absolute_closure_audit_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase198Path = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output/weak_coupling_source_lineage_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE192_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase149 = JsonDocument.Parse(File.ReadAllText(Phase149Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase132 = JsonDocument.Parse(File.ReadAllText(Phase132Path));
using var phase184 = JsonDocument.Parse(File.ReadAllText(Phase184Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase195 = JsonDocument.Parse(File.ReadAllText(Phase195Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase198 = JsonDocument.Parse(File.ReadAllText(Phase198Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));

var rows = phase148.RootElement.GetProperty("comparisonRows")
    .EnumerateArray()
    .Select(row => new LedgerRow(
        RequiredString(row, "particleId"),
        RequiredString(row, "observableId"),
        RequiredString(row, "status"),
        JsonDouble(row, "predictedValue"),
        JsonDouble(row, "predictedUncertainty"),
        JsonDouble(row, "targetValue"),
        JsonDouble(row, "targetUncertainty"),
        JsonString(row, "unit"),
        JsonDouble(row, "pullOrSigmaResidual"),
        JsonBool(row, "passed"),
        string.Equals(JsonString(row, "readinessStatus"), "predicted", StringComparison.Ordinal)
            && string.Equals(JsonString(row, "claimGateStatus"), "predicted", StringComparison.Ordinal),
        StringArray(row, "closureRequirements")))
    .ToArray();

var promotedRows = rows.Where(row => row.Status == "predicted" && row.Passed is true && row.GatePromoted).ToArray();
var failedRows = rows.Where(row => row.Status == "failed-comparison-attempt-not-promoted").ToArray();
var blockedRows = rows.Where(row => row.Status.StartsWith("blocked-", StringComparison.Ordinal)).ToArray();

bool allKnownBosonValuesDefensible = rows.Length > 0 && rows.All(row => row.Status == "predicted" && row.Passed is true && row.GatePromoted);
string terminalStatus = allKnownBosonValuesDefensible
    ? "boson-scientific-defensibility-ledger-complete"
    : "boson-scientific-defensibility-ledger-partial";

var blockerSummary = new[]
{
    new BlockerSummary(
        "wz-absolute-masses",
        failedRows.Where(row => row.ParticleId is "w-boson" or "z-boson").Select(row => row.ObservableId).ToArray(),
        new[]
        {
            "existing absolute W/Z comparison fails sigma policy",
            "P191 corrected direct bridge source does not clear raw-amplitude gate",
            "P191 has no derivation-promoted bridge theorem",
            "P191 has no particle-specific W/Z source split",
            "P185 unit scale is materialized but explicitly non-promotable downstream",
            "P132 rejects Phase46 vector-boson source spectra as a fermion-row sector-label transfer source",
            "P195 confirms the validated electroweak VEV/order-parameter scale is insufficient without a W/Z source-shape law",
            "P197 confirms the promoted weak-coupling mass relation fails W/Z absolute comparison",
            "P198 confirms no existing weak-coupling source lineage is promotable for W/Z absolute masses",
        },
        new
        {
            p191CanCompleteSuccessfulPrediction = JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction"),
            p191BestRawToTargetRatio = phase191.RootElement.TryGetProperty("gates", out var p191Gates)
                ? JsonDouble(p191Gates, "bestRawToTargetRatio")
                : null,
            p191RawGatePassed = phase191.RootElement.TryGetProperty("gates", out p191Gates)
                ? JsonBool(p191Gates, "rawGatePassed")
                : null,
            p132DerivationSourcePromotable = JsonBool(phase132.RootElement, "derivationSourcePromotable"),
            p132AnyMatchingPhase46Source = JsonBool(phase132.RootElement, "anyMatchingPhase46Source"),
            p195CanPromoteWzAbsoluteFromVevScale = JsonBool(phase195.RootElement, "canPromoteWzAbsoluteFromVevScale"),
            p197CanPromoteWzFromWeakCouplingMassRelation = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation"),
            p198CanPromoteAnyWeakCouplingSourceForWzAbsolute = JsonBool(phase198.RootElement, "canPromoteAnyWeakCouplingSourceForWzAbsolute"),
        }),
    new BlockerSummary(
        "higgs-mass",
        blockedRows.Where(row => row.ParticleId == "higgs").Select(row => row.ObservableId).ToArray(),
        new[]
        {
            "no solved scalar-sector source/operator for a Higgs-like mode",
            "no target-independent Higgs identity feature table",
            "no scalar-sector stability sidecars",
            "no physical-higgs-mass-gev comparison row can be emitted",
            "P196 rejects current scalar/selector artifacts as a Higgs potential or self-coupling source",
            "P199 confirms no existing Higgs scalar-source lineage is promotable",
        },
        new
        {
            p184TerminalStatus = JsonString(phase184.RootElement, "terminalStatus"),
            p184AnyNewMassiveAttemptAllowed = JsonBool(phase184.RootElement, "anyNewMassiveAttemptAllowed"),
            p196CanPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling"),
            p199CanPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage"),
        }),
};

var result = new
{
    phaseId = "phase192-boson-scientific-defensibility-ledger",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    allKnownBosonValuesDefensible,
    knownBosonRowCount = rows.Length,
    defensibleValueCount = promotedRows.Length,
    failedAttemptCount = failedRows.Length,
    blockedCount = blockedRows.Length,
    defensibleValues = promotedRows,
    failedAttempts = failedRows,
    blockedValues = blockedRows,
    blockerSummary,
    currentAnswer = allKnownBosonValuesDefensible
        ? "All known boson rows have scientifically defensible promoted values."
        : "The repository currently has scientifically defensible promoted values only for the W/Z mass ratio and the protected masslessness indicators for photon and gluon. Absolute W, absolute Z, and Higgs mass values remain non-promotable.",
    nextRequiredArtifacts = new[]
    {
        "W/Z: derivation-backed direct bridge-source theorem or branch-local proof discharging the mixed-linearization obligation, with particle-specific W/Z source rows clearing raw-amplitude, common-scale, and target-comparison gates.",
        "Higgs: solved scalar-sector source/operator, target-independent Higgs identity features, stability sidecars, and a physical-higgs-mass-gev comparison row.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase132Path = Phase132Path,
        phase149Path = Phase149Path,
        phase151Path = Phase151Path,
        phase184Path = Phase184Path,
        phase191Path = Phase191Path,
        phase195Path = Phase195Path,
        phase196Path = Phase196Path,
        phase197Path = Phase197Path,
        phase198Path = Phase198Path,
        phase199Path = Phase199Path,
        phase149TerminalStatus = JsonString(phase149.RootElement, "terminalStatus"),
        phase151TerminalStatus = JsonString(phase151.RootElement, "terminalStatus"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_scientific_defensibility_ledger.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_scientific_defensibility_ledger_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.allKnownBosonValuesDefensible,
        result.knownBosonRowCount,
        result.defensibleValueCount,
        result.failedAttemptCount,
        result.blockedCount,
        result.defensibleValues,
        result.failedAttempts,
        result.blockedValues,
        result.blockerSummary,
        result.currentAnswer,
        result.nextRequiredArtifacts,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"defensibleValueCount={promotedRows.Length}");
Console.WriteLine($"failedAttemptCount={failedRows.Length}");
Console.WriteLine($"blockedCount={blockedRows.Length}");
Console.WriteLine($"allKnownBosonValuesDefensible={allKnownBosonValuesDefensible}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();

sealed record LedgerRow(
    string ParticleId,
    string ObservableId,
    string Status,
    double? PredictedValue,
    double? PredictedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    string? Unit,
    double? PullOrSigmaResidual,
    bool? Passed,
    bool GatePromoted,
    IReadOnlyList<string> ClosureRequirements);

sealed record BlockerSummary(
    string BlockerId,
    IReadOnlyList<string> ObservableIds,
    IReadOnlyList<string> Reasons,
    object Evidence);
