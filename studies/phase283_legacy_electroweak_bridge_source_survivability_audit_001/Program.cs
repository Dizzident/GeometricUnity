using System.Text.Json;

const string DefaultOutputDir = "studies/phase283_legacy_electroweak_bridge_source_survivability_audit_001/output";
const string Phase53Path = "studies/phase53_electroweak_scale_source_intake_001/scale_source_intake.json";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase58Path = "studies/phase58_electroweak_absolute_scale_calibration_builder_001/calibration_builder_contract.json";
const string Phase60Path = "studies/phase60_electroweak_bridge_derivation_input_audit_001/bridge_derivation_input_audit.json";
const string Phase61Path = "studies/phase61_normalized_weak_coupling_input_audit_001/weak_coupling_input_audit.json";
const string Phase64Path = "studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json";
const string Phase68Path = "studies/phase68_normalized_weak_coupling_candidate_promotion_001/normalized_weak_coupling_candidate_promotion.json";
const string Phase69Path = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase194Path = "studies/phase194_draft_boson_source_evidence_audit_001/output/draft_boson_source_evidence_audit_summary.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase198Path = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output/weak_coupling_source_lineage_closure_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase273Path = "studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output/boson_fermion_coupling_proxy_source_audit_summary.json";
const string Phase282Path = "studies/phase282_branch_local_direct_invariant_census_001/output/branch_local_direct_invariant_census_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE283_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase53 = JsonDocument.Parse(File.ReadAllText(Phase53Path));
using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase58 = JsonDocument.Parse(File.ReadAllText(Phase58Path));
using var phase60 = JsonDocument.Parse(File.ReadAllText(Phase60Path));
using var phase61 = JsonDocument.Parse(File.ReadAllText(Phase61Path));
using var phase64 = JsonDocument.Parse(File.ReadAllText(Phase64Path));
using var phase68 = JsonDocument.Parse(File.ReadAllText(Phase68Path));
using var phase69 = JsonDocument.Parse(File.ReadAllText(Phase69Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase194 = JsonDocument.Parse(File.ReadAllText(Phase194Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase198 = JsonDocument.Parse(File.ReadAllText(Phase198Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase273 = JsonDocument.Parse(File.ReadAllText(Phase273Path));
using var phase282 = JsonDocument.Parse(File.ReadAllText(Phase282Path));

var phase53GuScaleBlocked = phase53.RootElement.GetProperty("candidateSources")
    .EnumerateArray()
    .Any(row => string.Equals(JsonString(row, "scaleId"), "phase53-gu-derived-internal-absolute-scale", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "status"), "blocked", StringComparison.Ordinal));
var phase53TargetFitRejected = phase53.RootElement.GetProperty("candidateSources")
    .EnumerateArray()
    .Any(row => string.Equals(JsonString(row, "scaleId"), "phase53-w-or-z-target-fit-scale", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "status"), "rejected", StringComparison.Ordinal));
var phase54ExternalVevIngested = phase54.RootElement.GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .Any(row => string.Equals(JsonString(row, "scaleId"), "phase54-fermi-derived-electroweak-vacuum-scale", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "status"), "ingested", StringComparison.Ordinal));
var phase54InternalBridgeBlocked = phase54.RootElement.TryGetProperty("internalBridgeStatus", out var phase54Bridge)
    && string.Equals(JsonString(phase54Bridge, "status"), "blocked", StringComparison.Ordinal);
var phase58BuilderStillNeedsRealBridge = phase58.RootElement.TryGetProperty("blockedUntil", out var phase58BlockedUntil)
    && phase58BlockedUntil.GetArrayLength() > 0;
var phase60BridgeInputsInitiallyBlocked = string.Equals(JsonString(phase60.RootElement, "terminalStatus"), "bridge-derivation-inputs-blocked", StringComparison.Ordinal);
var phase61ProxyAuditBlocked = string.Equals(JsonString(phase61.RootElement, "terminalStatus"), "normalized-weak-coupling-inputs-blocked", StringComparison.Ordinal)
    && JsonInt(phase61.RootElement, "acceptedCandidateCount") == 0;
var phase64NonProxyMatrixElementOnly = JsonBool(phase64.RootElement, "usesFiniteDifferenceProxy") is false
    && JsonBool(phase64.RootElement, "producesDimensionlessCouplingValue") is false;

var phase68Candidate = phase68.RootElement.GetProperty("candidate");
var phase68PromotedWeakCoupling = string.Equals(JsonString(phase68.RootElement, "terminalStatus"), "normalized-weak-coupling-candidate-promoted", StringComparison.Ordinal)
    && string.Equals(JsonString(phase68Candidate, "sourceKind"), "normalized-internal-weak-coupling", StringComparison.Ordinal);
var phase68ProvenanceId = JsonString(phase68Candidate, "provenanceId");
var phase68CouplingValue = JsonDouble(phase68Candidate, "couplingValue");
var phase69RelationDerived = string.Equals(JsonString(phase69.RootElement, "terminalStatus"), "electroweak-mass-generation-relation-derived", StringComparison.Ordinal);
var phase69DimensionlessBridgeValue = JsonDouble(phase69.RootElement, "dimensionlessBridgeValue");
var phase70UsesExternalScaleInput = string.Equals(JsonString(phase70.RootElement, "externalScaleInputId"), "phase54-fermi-derived-electroweak-vacuum-scale", StringComparison.Ordinal);

var phase197CanPromote = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is true;
var phase197ComparisonFailures = phase197.RootElement.GetProperty("comparisons")
    .EnumerateArray()
    .Count(row => JsonBool(row, "passed") is false);
var phase198CanPromoteAnyWeakCouplingSource = JsonBool(phase198.RootElement, "canPromoteAnyWeakCouplingSourceForWzAbsolute") is true;
var phase198Phase65Superseded = phase198.RootElement.GetProperty("sourceLineages")
    .EnumerateArray()
    .Any(row => string.Equals(JsonString(row, "lineageId"), "phase65-fixture-promoted-through-phase68", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "status"), "superseded-not-promotable-for-physical-wz", StringComparison.Ordinal));
var phase198ReplayLineageFailed = phase198.RootElement.GetProperty("sourceLineages")
    .EnumerateArray()
    .Any(row => string.Equals(JsonString(row, "lineageId"), "wz-route-replayed-analytic-matrix-element", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "status"), "executed-but-failed", StringComparison.Ordinal));

var phase194DraftProvidesDirectWzLaw = JsonBool(phase194.RootElement, "draftProvidesDirectWzLaw") is true;
var phase194DraftProvidesSolvedHiggsSource = JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource") is true;
var phase229TargetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var phase229ObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var phase245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var phase273CouplingProxyAuditPassed = JsonBool(phase273.RootElement, "couplingProxySourceAuditPassed") is true;
var phase273CouplingProxyPromotesWzMasses = JsonBool(phase273.RootElement, "couplingProxyPromotesWzMasses") is true;
var phase273CouplingProxyPromotesHiggsMass = JsonBool(phase273.RootElement, "couplingProxyPromotesHiggsMass") is true;
var phase273Phase12FiniteDifferenceOnly = JsonBool(phase273.RootElement, "phase12FiniteDifferenceOnly") is true;
var phase273Phase77RawMatrixElementEvidenceBlocked = JsonBool(phase273.RootElement, "phase77RawMatrixElementEvidenceBlocked") is true;
var phase273ProductionAnalyticInputsMaterialized = JsonBool(phase273.RootElement, "phase81ProductionInputsMaterialized") is true;
var phase282BranchLocalInvariantCensusPassed = JsonBool(phase282.RootElement, "branchLocalInvariantCensusPassed") is true;
var phase282NewLocalDirectInvariantSourceFound = JsonBool(phase282.RootElement, "newLocalDirectInvariantSourceFound") is true;
var phase282DirectInvariantPromotesWzMasses = JsonBool(phase282.RootElement, "directInvariantPromotesWzMasses") is true;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "legacy-scale-intake-blocked",
        phase53GuScaleBlocked && phase53TargetFitRejected && phase54ExternalVevIngested && phase54InternalBridgeBlocked && phase58BuilderStillNeedsRealBridge,
        $"phase53GuScaleBlocked={phase53GuScaleBlocked}; phase53TargetFitRejected={phase53TargetFitRejected}; phase54ExternalVevIngested={phase54ExternalVevIngested}; phase54InternalBridgeBlocked={phase54InternalBridgeBlocked}; phase58BuilderStillNeedsRealBridge={phase58BuilderStillNeedsRealBridge}"),
    new Check(
        "legacy-weak-coupling-route-materialized-but-source-limited",
        phase60BridgeInputsInitiallyBlocked && phase61ProxyAuditBlocked && phase64NonProxyMatrixElementOnly && phase68PromotedWeakCoupling && phase69RelationDerived && phase70UsesExternalScaleInput,
        $"phase60BridgeInputsInitiallyBlocked={phase60BridgeInputsInitiallyBlocked}; phase61ProxyAuditBlocked={phase61ProxyAuditBlocked}; phase64NonProxyMatrixElementOnly={phase64NonProxyMatrixElementOnly}; phase68PromotedWeakCoupling={phase68PromotedWeakCoupling}; phase68ProvenanceId={phase68ProvenanceId}; phase69RelationDerived={phase69RelationDerived}; phase70UsesExternalScaleInput={phase70UsesExternalScaleInput}"),
    new Check(
        "legacy-route-fails-physical-wz-comparison",
        !phase197CanPromote && phase197ComparisonFailures == 2,
        $"phase197CanPromote={phase197CanPromote}; phase197ComparisonFailures={phase197ComparisonFailures}"),
    new Check(
        "weak-coupling-source-lineage-superseded-or-failed",
        !phase198CanPromoteAnyWeakCouplingSource && phase198Phase65Superseded && phase198ReplayLineageFailed,
        $"phase198CanPromoteAnyWeakCouplingSource={phase198CanPromoteAnyWeakCouplingSource}; phase198Phase65Superseded={phase198Phase65Superseded}; phase198ReplayLineageFailed={phase198ReplayLineageFailed}"),
    new Check(
        "vev-lineage-remains-external",
        !phase229TargetIndependentGuVevSourcePromotable && phase229ObstructionCertified && phase70UsesExternalScaleInput,
        $"phase229TargetIndependentGuVevSourcePromotable={phase229TargetIndependentGuVevSourcePromotable}; phase229ObstructionCertified={phase229ObstructionCertified}; phase70UsesExternalScaleInput={phase70UsesExternalScaleInput}"),
    new Check(
        "coupling-proxy-production-source-missing",
        phase273CouplingProxyAuditPassed && phase273Phase12FiniteDifferenceOnly && phase273Phase77RawMatrixElementEvidenceBlocked && !phase273ProductionAnalyticInputsMaterialized && !phase273CouplingProxyPromotesWzMasses && !phase273CouplingProxyPromotesHiggsMass,
        $"phase273CouplingProxyAuditPassed={phase273CouplingProxyAuditPassed}; phase273Phase12FiniteDifferenceOnly={phase273Phase12FiniteDifferenceOnly}; phase273Phase77RawMatrixElementEvidenceBlocked={phase273Phase77RawMatrixElementEvidenceBlocked}; phase273ProductionAnalyticInputsMaterialized={phase273ProductionAnalyticInputsMaterialized}; phase273CouplingProxyPromotesWzMasses={phase273CouplingProxyPromotesWzMasses}; phase273CouplingProxyPromotesHiggsMass={phase273CouplingProxyPromotesHiggsMass}"),
    new Check(
        "branch-local-direct-invariants-exhausted",
        phase282BranchLocalInvariantCensusPassed && !phase282NewLocalDirectInvariantSourceFound && !phase282DirectInvariantPromotesWzMasses,
        $"phase282BranchLocalInvariantCensusPassed={phase282BranchLocalInvariantCensusPassed}; phase282NewLocalDirectInvariantSourceFound={phase282NewLocalDirectInvariantSourceFound}; phase282DirectInvariantPromotesWzMasses={phase282DirectInvariantPromotesWzMasses}"),
    new Check(
        "minimal-unlocks-still-unfilled",
        !phase245UnlockContractFilled && phase245NewSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"phase245UnlockContractFilled={phase245UnlockContractFilled}; phase245NewSourceEvidenceStillRequired={phase245NewSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "draft-does-not-supply-missing-source-law",
        !phase194DraftProvidesDirectWzLaw && !phase194DraftProvidesSolvedHiggsSource,
        $"phase194DraftProvidesDirectWzLaw={phase194DraftProvidesDirectWzLaw}; phase194DraftProvidesSolvedHiggsSource={phase194DraftProvidesSolvedHiggsSource}"),
};

var legacyBridgeRoutePromotableForBosonMasses = false;
var wZAbsoluteScaleSourceLawFound = false;
var higgsScalarScaleSourceLawFound = false;
var sourceContractsFilled = false;
var legacyBridgeSourceSurvivabilityAuditPassed = checks.All(check => check.Passed)
    && !legacyBridgeRoutePromotableForBosonMasses
    && !wZAbsoluteScaleSourceLawFound
    && !higgsScalarScaleSourceLawFound
    && !sourceContractsFilled;
var terminalStatus = legacyBridgeSourceSurvivabilityAuditPassed
    ? "legacy-electroweak-bridge-source-survivability-audit-no-promotable-source"
    : "legacy-electroweak-bridge-source-survivability-audit-review-required";

var result = new
{
    phaseId = "phase283-legacy-electroweak-bridge-source-survivability-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    legacyBridgeSourceSurvivabilityAuditPassed,
    legacyBridgeRoutePromotableForBosonMasses,
    wZAbsoluteScaleSourceLawFound,
    higgsScalarScaleSourceLawFound,
    sourceContractsFilled,
    legacyBridgeRoute = new
    {
        phase68PromotedWeakCoupling,
        phase68ProvenanceId,
        phase68CouplingValue,
        phase69RelationDerived,
        phase69DimensionlessBridgeValue,
        phase70UsesExternalScaleInput,
        phase197CanPromoteWzFromWeakCouplingMassRelation = phase197CanPromote,
        phase197ComparisonFailures,
        phase198CanPromoteAnyWeakCouplingSourceForWzAbsolute = phase198CanPromoteAnyWeakCouplingSource,
        phase198Phase65Superseded,
        phase198ReplayLineageFailed,
    },
    sourceLawBoundaries = new
    {
        phase53GuScaleBlocked,
        phase54ExternalVevIngested,
        phase54InternalBridgeBlocked,
        phase229TargetIndependentGuVevSourcePromotable,
        phase273Phase12FiniteDifferenceOnly,
        phase273Phase77RawMatrixElementEvidenceBlocked,
        phase273ProductionAnalyticInputsMaterialized,
        phase282NewLocalDirectInvariantSourceFound,
        phase245UnlockContractFilled,
        phase245NewSourceEvidenceStillRequired,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        phase194DraftProvidesDirectWzLaw,
        phase194DraftProvidesSolvedHiggsSource,
    },
    checks,
    decision = legacyBridgeSourceSurvivabilityAuditPassed
        ? "Do not promote the legacy Phase68/69/70 electroweak bridge as a W/Z/H prediction source. It materializes useful relation context, but the coupling lineage is superseded or fails comparison, the VEV is external, production analytic source evidence is missing, branch-local direct invariants do not pass the raw gate, and the rank-deficit unlock rows remain unfilled."
        : "Review the legacy electroweak bridge source-survivability inputs before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A target-independent W/Z absolute-scale source law for log(v g), or independent GU rows for v and g, with production source-lineage rows and W/Z promotion gates filled.",
        "A target-independent Higgs scalar-scale source law for log(v sqrt(lambda)), or independent GU rows for v and lambda, with scalar-source operator, potential/self-coupling lineage, massive profile, and target comparison filled.",
    },
    sourceEvidence = new
    {
        phase53Path = Phase53Path,
        phase54Path = Phase54Path,
        phase58Path = Phase58Path,
        phase60Path = Phase60Path,
        phase61Path = Phase61Path,
        phase64Path = Phase64Path,
        phase68Path = Phase68Path,
        phase69Path = Phase69Path,
        phase70Path = Phase70Path,
        phase194Path = Phase194Path,
        phase197Path = Phase197Path,
        phase198Path = Phase198Path,
        phase213Path = Phase213Path,
        phase229Path = Phase229Path,
        phase245Path = Phase245Path,
        phase273Path = Phase273Path,
        phase282Path = Phase282Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "legacy_electroweak_bridge_source_survivability_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "legacy_electroweak_bridge_source_survivability_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.legacyBridgeSourceSurvivabilityAuditPassed,
        result.legacyBridgeRoutePromotableForBosonMasses,
        result.wZAbsoluteScaleSourceLawFound,
        result.higgsScalarScaleSourceLawFound,
        result.sourceContractsFilled,
        result.legacyBridgeRoute,
        result.sourceLawBoundaries,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"legacyBridgeSourceSurvivabilityAuditPassed={legacyBridgeSourceSurvivabilityAuditPassed}");
Console.WriteLine($"legacyBridgeRoutePromotableForBosonMasses={legacyBridgeRoutePromotableForBosonMasses}");
Console.WriteLine($"sourceContractsFilled={sourceContractsFilled}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record Check(string CheckId, bool Passed, string Detail);
