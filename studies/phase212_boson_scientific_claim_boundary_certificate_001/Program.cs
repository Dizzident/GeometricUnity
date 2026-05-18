using System.Text.Json;

const string DefaultOutputDir = "studies/phase212_boson_scientific_claim_boundary_certificate_001/output";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package_summary.json";
const string Phase202Path = "studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase208Path = "studies/phase208_boson_local_route_exhaustion_certificate_001/output/boson_local_route_exhaustion_certificate_summary.json";
const string Phase209Path = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/boson_source_lineage_evidence_request_package_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase211Path = "studies/phase211_boson_prediction_promotion_readiness_gate_001/output/boson_prediction_promotion_readiness_gate_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE212_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase202 = JsonDocument.Parse(File.ReadAllText(Phase202Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase208 = JsonDocument.Parse(File.ReadAllText(Phase208Path));
using var phase209 = JsonDocument.Parse(File.ReadAllText(Phase209Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase211 = JsonDocument.Parse(File.ReadAllText(Phase211Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));

var defensibleValues = phase203.RootElement.GetProperty("defensibleValues").EnumerateArray().Select(row => row.Clone()).ToArray();
var nonDefensibleRows = phase203.RootElement.GetProperty("nonDefensibleRows").EnumerateArray().Select(row => row.Clone()).ToArray();
var objectiveAchieved = JsonBool(phase202.RootElement, "objectiveAchieved") is true;
var allKnownBosonValuesDefensible = JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible") is true;
var localRouteExhaustionCertified = JsonBool(phase208.RootElement, "localRouteExhaustionCertified") is true;
var evidenceRequestPackageMaterialized = JsonBool(phase209.RootElement, "evidenceRequestPackageMaterialized") is true;
var rerunPromotionAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;
var promotionAttemptReady = JsonBool(phase211.RootElement, "promotionAttemptReady") is true;
var predictionSetComplete = JsonBool(phase211.RootElement, "predictionSetComplete") is true;
var externalElectroweakInputLoopholeClosed = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is false;
var higgsTargetImpliedSelfCouplingLoopholeClosed = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is false;
var officialPublicSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;
var officialPublicSourceNonclaimClosed = officialPublicSourceAuditMaterialized && !officialDraftProvidesCompletionSource;

var claimBoundaryReady = defensibleValues.Length == 3
    && nonDefensibleRows.Length == 3
    && localRouteExhaustionCertified
    && evidenceRequestPackageMaterialized
    && externalElectroweakInputLoopholeClosed
    && higgsTargetImpliedSelfCouplingLoopholeClosed
    && officialPublicSourceNonclaimClosed
    && !rerunPromotionAllowed
    && !promotionAttemptReady
    && !predictionSetComplete
    && !allKnownBosonValuesDefensible
    && !objectiveAchieved;

var terminalStatus = claimBoundaryReady
    ? "boson-scientific-claim-boundary-certified-partial-values"
    : predictionSetComplete
        ? "boson-scientific-claim-boundary-complete-all-values"
        : "boson-scientific-claim-boundary-indeterminate";

var result = new
{
    phaseId = "phase212-boson-scientific-claim-boundary-certificate",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    claimBoundaryReady,
    objectiveAchieved,
    allKnownBosonValuesDefensible,
    localRouteExhaustionCertified,
    evidenceRequestPackageMaterialized,
    rerunPromotionAllowed,
    promotionAttemptReady,
    predictionSetComplete,
    externalElectroweakInputLoopholeClosed,
    higgsTargetImpliedSelfCouplingLoopholeClosed,
    officialPublicSourceAuditMaterialized,
    officialDraftProvidesCompletionSource,
    officialPublicSourceNonclaimClosed,
    defensibleValueCount = defensibleValues.Length,
    nonDefensibleRowCount = nonDefensibleRows.Length,
    defensibleClaims = defensibleValues,
    explicitNonClaims = nonDefensibleRows,
    allowedClaims = new[]
    {
        "physical-w-z-mass-ratio",
        "physical-photon-masslessness",
        "physical-gluon-masslessness",
    },
    prohibitedClaimsUntilNewEvidence = new[]
    {
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
        "physical-higgs-mass-gev",
    },
    prohibitedShortcutClaims = new[]
    {
        "external-or-target-implied-electroweak-coupling-as-gu-wz-prediction",
        "target-implied-higgs-quartic-or-self-coupling-as-gu-higgs-prediction",
    },
    prohibitedSourceClaims = new[]
    {
        "official-public-gu-draft-as-wz-or-higgs-source-lineage-promotion",
    },
    publicationBoundary = claimBoundaryReady
        ? "Publish only the defensible-promoted rows. Treat W absolute, Z absolute, Higgs, official public GU draft passages, external/target-implied W/Z coupling, and target-implied Higgs self-coupling as explicit non-claims until Phase201/P210/P211 gates change."
        : "Do not publish from this boundary until the upstream gates are reconciled.",
    nextRequiredAction = JsonString(phase211.RootElement, "requiredNextAction"),
    sourceEvidence = new
    {
        phase101Path = Phase101Path,
        phase202Path = Phase202Path,
        phase203Path = Phase203Path,
        phase208Path = Phase208Path,
        phase209Path = Phase209Path,
        phase210Path = Phase210Path,
        phase211Path = Phase211Path,
        phase214Path = Phase214Path,
        phase215Path = Phase215Path,
        phase218Path = Phase218Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_scientific_claim_boundary_certificate.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_scientific_claim_boundary_certificate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.claimBoundaryReady,
        result.objectiveAchieved,
        result.allKnownBosonValuesDefensible,
        result.localRouteExhaustionCertified,
        result.evidenceRequestPackageMaterialized,
        result.rerunPromotionAllowed,
        result.promotionAttemptReady,
        result.predictionSetComplete,
        result.externalElectroweakInputLoopholeClosed,
        result.higgsTargetImpliedSelfCouplingLoopholeClosed,
        result.officialPublicSourceAuditMaterialized,
        result.officialDraftProvidesCompletionSource,
        result.officialPublicSourceNonclaimClosed,
        result.defensibleValueCount,
        result.nonDefensibleRowCount,
        result.allowedClaims,
        result.prohibitedClaimsUntilNewEvidence,
        result.prohibitedShortcutClaims,
        result.prohibitedSourceClaims,
        result.publicationBoundary,
        result.nextRequiredAction,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"claimBoundaryReady={claimBoundaryReady}");
Console.WriteLine($"defensibleValueCount={defensibleValues.Length}");
Console.WriteLine($"nonDefensibleRowCount={nonDefensibleRows.Length}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
