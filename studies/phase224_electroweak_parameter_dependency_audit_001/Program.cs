using System.Text.Json;

const string DefaultOutputDir = "studies/phase224_electroweak_parameter_dependency_audit_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase192Path = "studies/phase192_boson_scientific_defensibility_ledger_001/output/boson_scientific_defensibility_ledger_summary.json";
const string Phase198Path = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output/weak_coupling_source_lineage_closure_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE224_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase192 = JsonDocument.Parse(File.ReadAllText(Phase192Path));
using var phase198 = JsonDocument.Parse(File.ReadAllText(Phase198Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));

var fermiScale = phase54.RootElement
    .GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => JsonString(row, "scaleId") == "phase54-fermi-derived-electroweak-vacuum-scale");

var externalVev = JsonDouble(fermiScale, "value");
var allKnownBosonValuesDefensible = JsonBool(phase192.RootElement, "allKnownBosonValuesDefensible") is true;
var weakCouplingSourcePromotable = JsonBool(phase198.RootElement, "canPromoteAnyWeakCouplingSourceForWzAbsolute") is true;
var canPromoteExternalElectroweakBridge = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var canPromoteTargetImpliedHiggsSelfCoupling = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var su2CasimirPromotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;
var higgsCasimirQuarticPromotable = JsonBool(phase223.RootElement, "sourceLineagePromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var promotedDimensionlessWzRatioAvailable = JsonInt(phase192.RootElement, "defensibleValueCount") is >= 1
    && !allKnownBosonValuesDefensible;
var promotableVevSourceAvailable = false;
var promotableWeakCouplingMagnitudeAvailable = weakCouplingSourcePromotable || su2CasimirPromotable;
var promotableWeakMixingOrHyperchargeClosureAvailable = promotedDimensionlessWzRatioAvailable;
var promotableHiggsQuarticAvailable = higgsCasimirQuarticPromotable || canPromoteTargetImpliedHiggsSelfCoupling;

var parameterRows = new[]
{
    new ParameterDependency(
        "electroweak-vev-v",
        "dimensionful electroweak vacuum scale",
        "MW=g v/2; MZ=sqrt(g^2+g'^2) v/2; MH requires v and the scalar potential parameter",
        externalVev,
        "GeV",
        "external-fermi-derived-diagnostic",
        promotableVevSourceAvailable,
        "P214 keeps external/Fermi-derived VEV as comparison or disjoint scale context, not a GU source-lineage prediction."),
    new ParameterDependency(
        "su2-weak-coupling-g",
        "SU(2) weak coupling magnitude",
        "MW=g v/2",
        JsonDouble(phase221.RootElement, "casimirWeakCoupling"),
        "dimensionless",
        "p221-casimir-diagnostic",
        promotableWeakCouplingMagnitudeAvailable,
        "P198 has no promotable weak-coupling source; P221 is numerically close but sourceLineagePromotable=false."),
    new ParameterDependency(
        "hypercharge-or-weak-angle",
        "U(1) hypercharge coupling or weak angle closure",
        "MZ=sqrt(g^2+g'^2) v/2 and MW/MZ=cos(thetaW)",
        null,
        "dimensionless",
        "wz-ratio-dimensionless-closure",
        promotableWeakMixingOrHyperchargeClosureAvailable,
        "The W/Z ratio is the current dimensionless defensible electroweak relation, but it does not supply the absolute coupling magnitude or VEV source."),
    new ParameterDependency(
        "higgs-scalar-self-coupling",
        "Higgs scalar potential/self-coupling or excitation parameter",
        "In standard normalization MH^2=2 lambda v^2; PDG convention equivalently ties MH to the scalar potential parameter and v",
        phase223.RootElement.TryGetProperty("bestProbe", out var bestProbe)
            ? JsonDouble(bestProbe, "quarticFromCasimirG2")
            : null,
        "dimensionless",
        "p223-three-tenths-diagnostic",
        promotableHiggsQuarticAvailable,
        "P215 target-implied quartic and P223 3/10 factor are diagnostic only; no scalar source/operator derives lambda."),
};

var wAbsoluteMassParameterClosure = promotableVevSourceAvailable && promotableWeakCouplingMagnitudeAvailable;
var zAbsoluteMassParameterClosure = wAbsoluteMassParameterClosure && promotableWeakMixingOrHyperchargeClosureAvailable;
var higgsMassParameterClosure = promotableVevSourceAvailable && promotableHiggsQuarticAvailable;

var checks = new[]
{
    new Check("pdg-electroweak-dependency-recorded", true, "P224 records the current PDG tree-level dependency structure for W/Z/H masses."),
    new Check("dimensionless-ratio-not-absolute-mass", promotedDimensionlessWzRatioAvailable && !wAbsoluteMassParameterClosure, $"promotedDimensionlessWzRatioAvailable={promotedDimensionlessWzRatioAvailable}; wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}"),
    new Check("external-vev-not-gu-source-lineage", !canPromoteExternalElectroweakBridge && !promotableVevSourceAvailable, $"canPromoteExternalElectroweakBridge={canPromoteExternalElectroweakBridge}; promotableVevSourceAvailable={promotableVevSourceAvailable}"),
    new Check("weak-coupling-source-not-promotable", !weakCouplingSourcePromotable && !su2CasimirPromotable, $"weakCouplingSourcePromotable={weakCouplingSourcePromotable}; su2CasimirPromotable={su2CasimirPromotable}"),
    new Check("higgs-quartic-source-not-promotable", !canPromoteTargetImpliedHiggsSelfCoupling && !higgsCasimirQuarticPromotable, $"canPromoteTargetImpliedHiggsSelfCoupling={canPromoteTargetImpliedHiggsSelfCoupling}; higgsCasimirQuarticPromotable={higgsCasimirQuarticPromotable}"),
    new Check("phase213-source-lineage-blockers-remain", wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var electroweakParameterAuditPassed = checks.All(check => check.Passed)
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure;
var terminalStatus = electroweakParameterAuditPassed
    ? "electroweak-parameter-dependency-audit-blocked-missing-source-parameters"
    : "electroweak-parameter-dependency-audit-review-required";

var result = new
{
    phaseId = "phase224-electroweak-parameter-dependency-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    electroweakParameterAuditPassed,
    objective = "Map W/Z/H mass prediction requirements to source-lineage parameters and current repo evidence.",
    externalPhysicsContext = new
    {
        source = "Particle Data Group 2025 Review, Electroweak Model and Constraints on New Physics",
        url = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        citedRelations = new[]
        {
            "MW = g v / 2",
            "MZ = sqrt(g^2 + g'^2) v / 2",
            "M_gamma = 0",
            "Higgs mass depends on the scalar potential parameter and v",
        },
        interpretation = "External electroweak theory identifies the parameter dependencies only; it does not provide GU source-lineage evidence for those parameters.",
    },
    parameterRows,
    closure = new
    {
        wAbsoluteMassParameterClosure,
        zAbsoluteMassParameterClosure,
        higgsMassParameterClosure,
        allKnownBosonValuesDefensible,
        canPromoteExternalElectroweakBridge,
        weakCouplingSourcePromotable,
        su2CasimirPromotable,
        canPromoteTargetImpliedHiggsSelfCoupling,
        higgsCasimirQuarticPromotable,
    },
    checks,
    decision = electroweakParameterAuditPassed
        ? "Do not promote W, Z, or Higgs absolute masses. The external electroweak mass formulas show exactly which source parameters are required, and the current repo has diagnostics but no promotable GU source lineage for the needed absolute weak-coupling/VEV and scalar self-coupling inputs."
        : "Review electroweak parameter audit inputs before relying on this obstruction result.",
    nextRequiredArtifact = new[]
    {
        "For W/Z: a target-independent GU source lineage for the dimensionful electroweak scale and SU(2) weak-coupling magnitude, plus W/Z particle-specific source rows that pass raw-amplitude, common-bridge, target-comparison, and stability gates.",
        "For Higgs: a solved target-independent scalar source/operator that derives the scalar potential/self-coupling or excitation relation, Higgs identity, massive profile, and stability sidecars.",
        "Only after those source parameters are promotable should Phase201/P209/P210/P213 be filled and the prediction package rerun.",
    },
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase192Path = Phase192Path,
        phase198Path = Phase198Path,
        phase213Path = Phase213Path,
        phase214Path = Phase214Path,
        phase215Path = Phase215Path,
        phase221Path = Phase221Path,
        phase223Path = Phase223Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electroweak_parameter_dependency_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_parameter_dependency_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.electroweakParameterAuditPassed,
        result.externalPhysicsContext,
        result.parameterRows,
        result.closure,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"electroweakParameterAuditPassed={electroweakParameterAuditPassed}");
Console.WriteLine($"wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}");
Console.WriteLine($"zAbsoluteMassParameterClosure={zAbsoluteMassParameterClosure}");
Console.WriteLine($"higgsMassParameterClosure={higgsMassParameterClosure}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ParameterDependency(
    string ParameterId,
    string Role,
    string RequiredBy,
    double? CurrentDiagnosticValue,
    string Unit,
    string CurrentEvidenceClass,
    bool PromotableSourceLineageAvailable,
    string Blocker);

sealed record Check(string CheckId, bool Passed, string Detail);
