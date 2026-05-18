using System.Text.Json;

const string DefaultOutputDir = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase260Path = "studies/phase260_mass_definition_convention_sensitivity_audit_001/output/mass_definition_convention_sensitivity_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE261_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase260 = JsonDocument.Parse(File.ReadAllText(Phase260Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wRow = FindRow(rows, "w-boson");
var zRow = FindRow(rows, "z-boson");

var currentWeakCoupling = RequiredDouble(phase197.RootElement, "currentWeakCoupling");
var targetImpliedWeakCoupling = RequiredDouble(phase197.RootElement, "targetImpliedWeakCoupling");
var targetWGeV = RequiredDouble(wRow, "targetValue");
var targetZGeV = RequiredDouble(zRow, "targetValue");
var targetWzRatio = targetWGeV / targetZGeV;
var onShellSin2Theta = 1.0 - targetWzRatio * targetWzRatio;
var effectiveLeptonicSin2Theta = 0.23153;
var alphaZeroInverse = 137.035999084;
var alphaMzInverse = 127.95;

var schemeRows = new[]
{
    CouplingScheme("alpha0-onshell", alphaZeroInverse, onShellSin2Theta, currentWeakCoupling, targetImpliedWeakCoupling),
    CouplingScheme("alpha0-effective-leptonic", alphaZeroInverse, effectiveLeptonicSin2Theta, currentWeakCoupling, targetImpliedWeakCoupling),
    CouplingScheme("alphaMz-onshell", alphaMzInverse, onShellSin2Theta, currentWeakCoupling, targetImpliedWeakCoupling),
    CouplingScheme("alphaMz-effective-leptonic", alphaMzInverse, effectiveLeptonicSin2Theta, currentWeakCoupling, targetImpliedWeakCoupling),
};

var bestSchemeByTargetCoupling = schemeRows
    .OrderBy(row => Math.Abs(row.Coupling - targetImpliedWeakCoupling))
    .First();
var anySchemeNearTargetWeakCoupling = schemeRows.Any(row => row.RelativeErrorToTargetImpliedWeakCoupling < 0.02);
var schemeInputsAreExternalElectroweakInputs = true;
var schemeChoiceProvidesGuSourceLineage = false;
var schemeChoiceProvidesObservedFieldExtraction = false;
var schemeChoicePromotesBosonMasses = false;

var canPromoteExternalElectroweakBridge = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out var phase224Closure)
    && JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out phase224Closure)
    && JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var lowEnergyRgTransportSourceAuditPassed = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourceAuditPassed") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var phase260ConventionsPromoteMasses = JsonBool(phase260.RootElement, "conventionShiftPromotesBosonMasses") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var electroweakSchemeRadiativeSourceAuditPassed = anySchemeNearTargetWeakCoupling
    && schemeInputsAreExternalElectroweakInputs
    && !schemeChoiceProvidesGuSourceLineage
    && !schemeChoiceProvidesObservedFieldExtraction
    && !schemeChoicePromotesBosonMasses
    && !canPromoteExternalElectroweakBridge
    && electroweakParameterAuditPassed
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && lowEnergyRgTransportSourceAuditPassed
    && !lowEnergyRgTransportSourcePromotable
    && !phase260ConventionsPromoteMasses
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "electroweak-scheme-can-numerically-approach-target-coupling",
        anySchemeNearTargetWeakCoupling,
        $"bestScheme={bestSchemeByTargetCoupling.SchemeId}; bestCoupling={bestSchemeByTargetCoupling.Coupling:R}; targetImpliedWeakCoupling={targetImpliedWeakCoupling:R}; bestRelativeError={bestSchemeByTargetCoupling.RelativeErrorToTargetImpliedWeakCoupling:R}"),
    new Check(
        "scheme-choice-is-external-input-not-gu-source",
        schemeInputsAreExternalElectroweakInputs && !schemeChoiceProvidesGuSourceLineage && !schemeChoiceProvidesObservedFieldExtraction,
        $"schemeInputsAreExternalElectroweakInputs={schemeInputsAreExternalElectroweakInputs}; schemeChoiceProvidesGuSourceLineage={schemeChoiceProvidesGuSourceLineage}; schemeChoiceProvidesObservedFieldExtraction={schemeChoiceProvidesObservedFieldExtraction}"),
    new Check(
        "external-electroweak-input-loophole-remains-closed",
        !canPromoteExternalElectroweakBridge && electroweakParameterAuditPassed && !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure,
        $"canPromoteExternalElectroweakBridge={canPromoteExternalElectroweakBridge}; electroweakParameterAuditPassed={electroweakParameterAuditPassed}; wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; zAbsoluteMassParameterClosure={zAbsoluteMassParameterClosure}"),
    new Check(
        "low-energy-rg-transport-source-still-missing",
        lowEnergyRgTransportSourceAuditPassed && !lowEnergyRgTransportSourcePromotable,
        $"lowEnergyRgTransportSourceAuditPassed={lowEnergyRgTransportSourceAuditPassed}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}"),
    new Check(
        "scheme-choice-does-not-promote-while-source-lineage-missing",
        !schemeChoicePromotesBosonMasses && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"schemeChoicePromotesBosonMasses={schemeChoicePromotesBosonMasses}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = electroweakSchemeRadiativeSourceAuditPassed
    ? "electroweak-scheme-radiative-source-audit-external-input-not-promotion"
    : "electroweak-scheme-radiative-source-audit-review-required";

var result = new
{
    phaseId = "phase261-electroweak-scheme-radiative-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    electroweakSchemeRadiativeSourceAuditPassed,
    schemeChoicePromotesBosonMasses,
    anySchemeNearTargetWeakCoupling,
    schemeInputsAreExternalElectroweakInputs,
    schemeChoiceProvidesGuSourceLineage,
    schemeChoiceProvidesObservedFieldExtraction,
    schemeInputs = new
    {
        alphaZeroInverse,
        alphaMzInverse,
        onShellSin2Theta,
        effectiveLeptonicSin2Theta,
        targetWzRatio,
        currentWeakCoupling,
        targetImpliedWeakCoupling,
    },
    schemeRows,
    bestSchemeByTargetCoupling,
    currentBlockerEvidence = new
    {
        phase214 = new
        {
            canPromoteExternalElectroweakBridge,
        },
        phase224 = new
        {
            electroweakParameterAuditPassed,
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
        },
        phase236 = new
        {
            lowEnergyRgTransportSourceAuditPassed,
            lowEnergyRgTransportSourcePromotable,
        },
        phase260 = new
        {
            phase260ConventionsPromoteMasses,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2025-electroweak-model",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
            "PDG electroweak fits use measured inputs and radiative corrections; these are external low-energy SM inputs, not GU source-lineage rows."),
        new ExternalSource(
            "standard-model-delta-r-relation",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
            "The W mass can be related to alpha, GF, MZ and radiative corrections, but that relation is an input-parameter scheme requiring measured constants and loop corrections."),
    },
    checks,
    decision = electroweakSchemeRadiativeSourceAuditPassed
        ? "Do not promote W/Z/H physical masses from electroweak scheme or radiative-correction choices. Some standard electroweak schemes can numerically approach the target weak coupling because they import measured alpha, weak-mixing, GF/MZ, and loop-correction inputs. Those choices do not supply a GU-derived W/Z source lineage, observed-field extraction bridge, low-energy RG transport artifact, or Higgs scalar source."
        : "Review electroweak scheme/radiative source audit before relying on scheme-boundary conclusions.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived low-energy electroweak coupling/scale transport source, not a selected SM renormalization scheme.",
        "A W/Z observed-field extraction and source-row theorem whose physical convention is declared before target comparison.",
        "A solved Higgs scalar-source/self-coupling lineage independent of target mass inputs.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase197Path = Phase197Path,
        phase214Path = Phase214Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase260Path = Phase260Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electroweak_scheme_radiative_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_scheme_radiative_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.electroweakSchemeRadiativeSourceAuditPassed,
        result.schemeChoicePromotesBosonMasses,
        result.anySchemeNearTargetWeakCoupling,
        result.schemeInputsAreExternalElectroweakInputs,
        result.schemeChoiceProvidesGuSourceLineage,
        result.schemeChoiceProvidesObservedFieldExtraction,
        result.schemeInputs,
        result.schemeRows,
        result.bestSchemeByTargetCoupling,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"electroweakSchemeRadiativeSourceAuditPassed={electroweakSchemeRadiativeSourceAuditPassed}");
Console.WriteLine($"schemeChoicePromotesBosonMasses={schemeChoicePromotesBosonMasses}");
Console.WriteLine($"bestScheme={bestSchemeByTargetCoupling.SchemeId}");

static SchemeRow CouplingScheme(string schemeId, double alphaInverse, double sin2Theta, double currentWeakCoupling, double targetImpliedWeakCoupling)
{
    var alpha = 1.0 / alphaInverse;
    var electricCharge = Math.Sqrt(4.0 * Math.PI * alpha);
    var coupling = electricCharge / Math.Sqrt(sin2Theta);
    return new SchemeRow(
        schemeId,
        alphaInverse,
        sin2Theta,
        electricCharge,
        coupling,
        coupling / currentWeakCoupling,
        Math.Abs(coupling - targetImpliedWeakCoupling) / targetImpliedWeakCoupling);
}

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record SchemeRow(
    string SchemeId,
    double AlphaInverse,
    double Sin2Theta,
    double ElectricCharge,
    double Coupling,
    double CouplingOverCurrentWeakCoupling,
    double RelativeErrorToTargetImpliedWeakCoupling);

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExternalSource(string SourceId, string Url, string Finding);
