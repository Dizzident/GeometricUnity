using System.Text.Json;

const string DefaultOutputDir = "studies/phase258_recent_electroweak_relation_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE258_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));

var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wRow = FindRow(comparisonRows, "w-boson");
var zRow = FindRow(comparisonRows, "z-boson");
var higgsRow = FindRow(comparisonRows, "higgs");
var wzRatioRow = FindRow(comparisonRows, "electroweak-sector");

var targetWGeV = RequiredDouble(wRow, "targetValue");
var targetZGeV = RequiredDouble(zRow, "targetValue");
var targetHiggsGeV = RequiredDouble(higgsRow, "targetValue");
var promotedWzRatio = RequiredDouble(wzRatioRow, "predictedValue");
var targetWzRatio = targetWGeV / targetZGeV;

var empiricalRelationLeftOverRight = targetHiggsGeV * targetZGeV * targetZGeV / (2.0 * targetWGeV * targetWGeV * targetWGeV);
var empiricalRelationRelativeError = empiricalRelationLeftOverRight - 1.0;
var empiricalRelationUsesTargetMasses = true;
var empiricalRelationHasGuDerivation = false;
var empiricalRelationProvidesAbsoluteScale = false;
var empiricalRelationProvidesWzSourceRows = false;
var empiricalRelationProvidesHiggsScalarSource = false;
var empiricalRelationProvidesObservedFieldExtraction = false;
var empiricalRelationPromotable = false;

var higgsToWFromPromotedRatio = 2.0 * promotedWzRatio * promotedWzRatio;
var higgsToZFromPromotedRatio = 2.0 * promotedWzRatio * promotedWzRatio * promotedWzRatio;
var targetAnchoredHiggsFromWGeV = targetWGeV * higgsToWFromPromotedRatio;
var targetAnchoredHiggsPullGeV = targetAnchoredHiggsFromWGeV - targetHiggsGeV;
var targetAnchoredUsesExternalScale = true;

var currentPromotedConstraintRank = JsonInt(phase244.RootElement, "currentPromotedConstraintRank") ?? 0;
var currentRemainingNullity = JsonInt(phase244.RootElement, "remainingNullity") ?? 0;
var hypotheticalRankIfEmpiricalRelationAccepted = Math.Min(3, currentPromotedConstraintRank + 1);
var hypotheticalRemainingNullityIfAccepted = Math.Max(0, 3 - hypotheticalRankIfEmpiricalRelationAccepted);
var hypotheticalAcceptedRelationWouldCompletePrediction = hypotheticalRemainingNullityIfAccepted == 0;

var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase257CurrentImplementationCanFillContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var recentElectroweakRelationPromotesBosonMasses = empiricalRelationPromotable
    && hypotheticalAcceptedRelationWouldCompletePrediction
    && allRequiredLineagesPromotable
    && wzMissingFieldCount == 0
    && higgsMissingFieldCount == 0
    && unlockContractFilled
    && phase256ContractPromotable
    && phase257CurrentImplementationCanFillContract;

var checks = new[]
{
    new Check(
        "empirical-relation-is-target-mass-relation-not-gu-source",
        empiricalRelationUsesTargetMasses && !empiricalRelationHasGuDerivation && !empiricalRelationPromotable,
        $"empiricalRelationUsesTargetMasses={empiricalRelationUsesTargetMasses}; empiricalRelationHasGuDerivation={empiricalRelationHasGuDerivation}; empiricalRelationPromotable={empiricalRelationPromotable}; empiricalRelationLeftOverRight={empiricalRelationLeftOverRight:R}; empiricalRelationRelativeError={empiricalRelationRelativeError:R}"),
    new Check(
        "relation-does-not-fill-source-lineage-fields",
        !empiricalRelationProvidesWzSourceRows
            && !empiricalRelationProvidesHiggsScalarSource
            && !empiricalRelationProvidesObservedFieldExtraction
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"empiricalRelationProvidesWzSourceRows={empiricalRelationProvidesWzSourceRows}; empiricalRelationProvidesHiggsScalarSource={empiricalRelationProvidesHiggsScalarSource}; empiricalRelationProvidesObservedFieldExtraction={empiricalRelationProvidesObservedFieldExtraction}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "relation-does-not-close-electroweak-rank-deficit",
        !empiricalRelationProvidesAbsoluteScale
            && hypotheticalRankIfEmpiricalRelationAccepted == 2
            && hypotheticalRemainingNullityIfAccepted == 1
            && !hypotheticalAcceptedRelationWouldCompletePrediction,
        $"currentPromotedConstraintRank={currentPromotedConstraintRank}; currentRemainingNullity={currentRemainingNullity}; hypotheticalRankIfEmpiricalRelationAccepted={hypotheticalRankIfEmpiricalRelationAccepted}; hypotheticalRemainingNullityIfAccepted={hypotheticalRemainingNullityIfAccepted}; empiricalRelationProvidesAbsoluteScale={empiricalRelationProvidesAbsoluteScale}"),
    new Check(
        "target-anchored-diagnostic-is-not-a-prediction",
        targetAnchoredUsesExternalScale && !recentElectroweakRelationPromotesBosonMasses,
        $"targetAnchoredHiggsFromWGeV={targetAnchoredHiggsFromWGeV:R}; targetAnchoredHiggsPullGeV={targetAnchoredHiggsPullGeV:R}; targetAnchoredUsesExternalScale={targetAnchoredUsesExternalScale}; recentElectroweakRelationPromotesBosonMasses={recentElectroweakRelationPromotesBosonMasses}"),
    new Check(
        "current-source-contract-blockers-preserved",
        !allRequiredLineagesPromotable
            && !unlockContractFilled
            && !phase256ContractPromotable
            && !phase257CurrentImplementationCanFillContract,
        $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; unlockContractFilled={unlockContractFilled}; phase256ContractPromotable={phase256ContractPromotable}; phase257CurrentImplementationCanFillContract={phase257CurrentImplementationCanFillContract}"),
};

var recentElectroweakRelationSourceAuditPassed = checks.All(check => check.Passed);
var terminalStatus = recentElectroweakRelationSourceAuditPassed
    ? "recent-electroweak-relation-source-audit-no-promotion"
    : "recent-electroweak-relation-source-audit-review-required";

var result = new
{
    phaseId = "phase258-recent-electroweak-relation-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    recentElectroweakRelationSourceAuditPassed,
    recentElectroweakRelationPromotesBosonMasses,
    empiricalRelation = new
    {
        relationId = "empirical-mh-mz2-two-mw3-2026",
        expression = "m_H * m_Z^2 ~= 2 * m_W^3",
        sourceKind = "recent-external-empirical-relation",
        empiricalRelationUsesTargetMasses,
        empiricalRelationHasGuDerivation,
        empiricalRelationPromotable,
        empiricalRelationProvidesAbsoluteScale,
        empiricalRelationProvidesWzSourceRows,
        empiricalRelationProvidesHiggsScalarSource,
        empiricalRelationProvidesObservedFieldExtraction,
        targetWGeV,
        targetZGeV,
        targetHiggsGeV,
        targetWzRatio,
        empiricalRelationLeftOverRight,
        empiricalRelationRelativeError,
    },
    promotedRatioDiagnostic = new
    {
        promotedWzRatio,
        higgsToWFromPromotedRatio,
        higgsToZFromPromotedRatio,
        targetAnchoredHiggsFromWGeV,
        targetAnchoredHiggsPullGeV,
        targetAnchoredUsesExternalScale,
    },
    rankEffect = new
    {
        coordinateBasis = new[]
        {
            "wz-absolute-scale-log",
            "weak-mixing-log",
            "higgs-absolute-scale-log",
        },
        currentPromotedConstraintRank,
        currentRemainingNullity,
        empiricalRelationConstraintVector = new[] { -1, 2, 1 },
        hypotheticalRankIfEmpiricalRelationAccepted,
        hypotheticalRemainingNullityIfAccepted,
        hypotheticalAcceptedRelationWouldCompletePrediction,
    },
    currentBlockerEvidence = new
    {
        phase201 = new
        {
            allRequiredLineagesPromotable,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase245 = new
        {
            unlockContractFilled,
        },
        phase256 = new
        {
            phase256ContractPromotable,
        },
        phase257 = new
        {
            phase257CurrentImplementationCanFillContract,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "official-gu-site",
            "https://geometricunity.org/",
            "Official site still describes the latest public GU manuscript as the April 1, 2021 draft; no source-lineage W/Z/H completion row was found in the refresh."),
        new ExternalSource(
            "official-gu-draft-appendix-locations",
            "https://saismaran.org/geometricunity.pdf",
            "The public draft locates Higgs field/potential and gauge fields inside GU notation, but does not solve physical W/Z/H mass rows."),
        new ExternalSource(
            "cox-gu-ii-researchgate",
            "https://www.researchgate.net/publication/396557260_Geometric_Unity_II_Matter_Symmetry_on_the_Observation_Slice_One-Family_Factorization_Pati-Salam_Embedding_Anomaly_Closure_and_Embryo_HiggsYukawa_Textures",
            "Cox II gives symbolic stage-II electroweak relations m_W^2=g_L^2 kappa^2/4 and m_Z^2=(g_L^2+g_Y^2) kappa^2/4, leaving low-energy couplings and kappa as source inputs."),
        new ExternalSource(
            "empirical-electroweak-boson-relation-2026",
            "https://zenodo.org/records/19962846",
            "A recent empirical relation m_H m_Z^2 ~= 2 m_W^3 is a numerical regularity, not a GU derivation or target-independent source-lineage artifact."),
    },
    checks,
    decision = recentElectroweakRelationSourceAuditPassed
        ? "Do not promote W/Z/H physical masses from the recent empirical electroweak relation or Cox II symbolic mass-matrix refresh. The empirical relation uses target masses and would still leave one absolute scale free even if accepted as a constraint; Cox II supplies symbolic electroweak mass formulas but not GU-derived low-energy g_L, g_Y, kappa, Higgs scalar-source, or observed-field extraction rows."
        : "Review recent electroweak relation source audit before relying on the external-source boundary.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed W/Z absolute-scale source row that fixes the common W/Z mass scale without target anchoring.",
        "A solved Higgs scalar-source/self-coupling row that fixes the Higgs scalar scale without using the observed Higgs mass.",
        "A physical observed-field extraction bridge that maps GU operators to photon/W/Z/H rows before target comparison.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase244Path = Phase244Path,
        phase245Path = Phase245Path,
        phase256Path = Phase256Path,
        phase257Path = Phase257Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "recent_electroweak_relation_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "recent_electroweak_relation_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.recentElectroweakRelationSourceAuditPassed,
        result.recentElectroweakRelationPromotesBosonMasses,
        result.empiricalRelation,
        result.promotedRatioDiagnostic,
        result.rankEffect,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"recentElectroweakRelationSourceAuditPassed={recentElectroweakRelationSourceAuditPassed}");
Console.WriteLine($"recentElectroweakRelationPromotesBosonMasses={recentElectroweakRelationPromotesBosonMasses}");
Console.WriteLine($"hypotheticalRemainingNullityIfAccepted={hypotheticalRemainingNullityIfAccepted}");

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

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExternalSource(string SourceId, string Url, string Finding);
