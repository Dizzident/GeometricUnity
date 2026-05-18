using System.Text.Json;

const string DefaultOutputDir = "studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output";
const string Phase4SourcePath = "studies/phase4_fermion_family_atlas_001/Phase4FermionFamilyAtlasStudy.cs";
const string Phase4CouplingAtlasPath = "studies/phase4_fermion_family_atlas_001/output/coupling_atlas.json";
const string Phase12ReportPath = "studies/phase12_joined_calculation_001/output/REPORT.md";
const string Phase12CouplingDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings";
const string Phase61Path = "studies/phase61_normalized_weak_coupling_input_audit_001/weak_coupling_input_audit.json";
const string Phase77Path = "studies/phase77_raw_weak_coupling_matrix_element_evidence_gate_001/raw_matrix_element_evidence_gate.json";
const string Phase78Path = "studies/phase78_replayed_raw_weak_coupling_matrix_element_builder_001/replayed_raw_matrix_element_builder.json";
const string Phase80Path = "studies/phase80_production_analytic_replay_input_materialization_audit_001/production_analytic_replay_input_materialization_audit.json";
const string Phase81Path = "studies/phase81_full_analytic_weak_coupling_replay_package_001/full_analytic_replay_package_contract.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE273_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase4Atlas = JsonDocument.Parse(File.ReadAllText(Phase4CouplingAtlasPath));
using var phase61 = JsonDocument.Parse(File.ReadAllText(Phase61Path));
using var phase77 = JsonDocument.Parse(File.ReadAllText(Phase77Path));
using var phase78 = JsonDocument.Parse(File.ReadAllText(Phase78Path));
using var phase80 = JsonDocument.Parse(File.ReadAllText(Phase80Path));
using var phase81 = JsonDocument.Parse(File.ReadAllText(Phase81Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));

var phase4Source = File.ReadAllText(Phase4SourcePath);
var phase12Report = File.ReadAllText(Phase12ReportPath);
var phase12AtlasPaths = Directory.Exists(Phase12CouplingDir)
    ? Directory.EnumerateFiles(Phase12CouplingDir, "coupling_atlas_*.json", SearchOption.TopDirectoryOnly)
        .Order(StringComparer.Ordinal)
        .ToArray()
    : Array.Empty<string>();
var phase12VariationPaths = Directory.Exists(Path.Combine(Phase12CouplingDir, "variations"))
    ? Directory.EnumerateFiles(Path.Combine(Phase12CouplingDir, "variations"), "variation-*.json", SearchOption.TopDirectoryOnly)
        .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
        .Order(StringComparer.Ordinal)
        .ToArray()
    : Array.Empty<string>();

var phase4SyntheticFallbackPresent = phase4Source.Contains("synthetic delta-omega perturbations", StringComparison.Ordinal)
    && phase4Source.Contains("These are NOT real bosonic modes", StringComparison.Ordinal);
var phase4NotPhysicalWarningPresent = phase4Source.Contains("They are NOT physical coupling constants, scattering amplitudes, or measured quantities", StringComparison.Ordinal);
var phase4TopCouplingSummaryOnly = phase4Atlas.RootElement.TryGetProperty("topCouplings", out var phase4TopCouplings)
    && phase4TopCouplings.ValueKind == JsonValueKind.Array
    && !phase4Atlas.RootElement.TryGetProperty("couplings", out _);
var phase4CouplingProxyLeadPresent = phase4TopCouplings.ValueKind == JsonValueKind.Array && phase4TopCouplings.GetArrayLength() > 0;

var phase12AtlasRows = phase12AtlasPaths.Select(ReadCouplingAtlas).ToArray();
var phase12CouplingRecordCount = phase12AtlasRows.Sum(row => row.CouplingCount);
var phase12FiniteDifferenceCouplingCount = phase12AtlasRows.Sum(row => row.FiniteDifferenceCount);
var phase12AnalyticCouplingCount = phase12AtlasRows.Sum(row => row.AnalyticVariationCount);
var phase12MaxCouplingProxyMagnitude = phase12AtlasRows.Select(row => row.MaxCouplingProxyMagnitude).DefaultIfEmpty(null).Max();
var phase12MinBranchStabilityScore = phase12AtlasRows.Select(row => row.MinBranchStabilityScore).DefaultIfEmpty(null).Min();
var phase12MaxBranchStabilityScore = phase12AtlasRows.Select(row => row.MaxBranchStabilityScore).DefaultIfEmpty(null).Max();
var phase12CouplingAtlasesPresent = phase12AtlasRows.Length == 2 && phase12CouplingRecordCount > 0;
var phase12FiniteDifferenceOnly = phase12CouplingRecordCount > 0
    && phase12FiniteDifferenceCouplingCount == phase12CouplingRecordCount
    && phase12AnalyticCouplingCount == 0;

var phase12VariationRows = phase12VariationPaths.Select(ReadVariation).ToArray();
var phase12VariationBundleCount = phase12VariationRows.Length;
var phase12VariationFiniteDifferenceCount = phase12VariationRows.Count(row => row.VariationMethod == "finite-difference");
var phase12VariationBlockedCount = phase12VariationRows.Count(row => row.Blocked);
var phase12VariationRawBosonVectorCount = phase12VariationRows.Count(row => row.NormalizationConvention == "raw-boson-vector");
var phase12ReportRecordsPersistedFiniteDifference = phase12Report.Contains("persisted finite-difference Dirac variation artifacts", StringComparison.Ordinal);

var phase61AcceptedCandidateCount = JsonInt(phase61.RootElement, "acceptedCandidateCount") ?? -1;
var phase61RejectedCandidateCount = JsonInt(phase61.RootElement, "rejectedCandidateCount") ?? -1;
var phase61FiniteDifferenceProxiesRejected = phase61AcceptedCandidateCount == 0
    && phase61RejectedCandidateCount >= 2
    && phase61.RootElement.GetProperty("records").EnumerateArray().All(row =>
        JsonString(row, "sourceKind") == "finite-difference-coupling-proxy"
        && JsonString(row, "status") == "rejected");

var phase77RawMatrixElementEvidenceBlocked = JsonString(phase77.RootElement, "terminalStatus") == "raw-weak-coupling-matrix-element-evidence-blocked";
var phase77AcceptedRawMatrixElementMissing = phase77.RootElement.TryGetProperty("acceptedRawMatrixElementMagnitude", out var p77Accepted)
    && p77Accepted.ValueKind == JsonValueKind.Null;
var phase78ProductionAnalyticRecordsFound = phase78.RootElement.TryGetProperty("persistedStudySearchFinding", out var p78Finding)
    && JsonBool(p78Finding, "productionAnalyticMatrixElementRecordsFound") is true;
var phase78FiniteDifferenceAtlasesRemainBlocked = phase78.RootElement.TryGetProperty("persistedStudySearchFinding", out p78Finding)
    && JsonBool(p78Finding, "finiteDifferenceAtlasesRemainBlocked") is true;
var phase80ProductionAnalyticInputsBlocked = JsonString(phase80.RootElement, "terminalStatus") == "production-analytic-replay-inputs-blocked";
var phase80Artifact = phase80.RootElement.GetProperty("artifact");
var phase80BosonModeSourceKind = JsonString(phase80Artifact, "bosonModeSourceKind");
var phase80HasFullCouplingRecord = JsonBool(phase80Artifact, "hasFullCouplingRecord") is true;
var phase80IsTopCouplingSummaryOnly = JsonBool(phase80Artifact, "isTopCouplingSummaryOnly") is true;
var phase81ProductionInputsMaterialized = JsonBool(phase81.RootElement, "productionInputsMaterialized") is true;
var phase81SyntheticSourceBlocked = JsonBool(phase81.RootElement, "syntheticSourceBlocked") is true;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;

var couplingProxyPromotesWzMasses = false;
var couplingProxyPromotesHiggsMass = false;
var couplingProxyCompletesBosonPredictions = false;

var couplingProxySourceAuditPassed =
    phase4SyntheticFallbackPresent
    && phase4NotPhysicalWarningPresent
    && phase4TopCouplingSummaryOnly
    && phase4CouplingProxyLeadPresent
    && phase12CouplingAtlasesPresent
    && phase12FiniteDifferenceOnly
    && phase12VariationBundleCount == 24
    && phase12VariationFiniteDifferenceCount == phase12VariationBundleCount
    && phase12VariationRawBosonVectorCount == phase12VariationBundleCount
    && phase12VariationBlockedCount == 0
    && phase12ReportRecordsPersistedFiniteDifference
    && phase61FiniteDifferenceProxiesRejected
    && phase77RawMatrixElementEvidenceBlocked
    && phase77AcceptedRawMatrixElementMissing
    && !phase78ProductionAnalyticRecordsFound
    && phase78FiniteDifferenceAtlasesRemainBlocked
    && phase80ProductionAnalyticInputsBlocked
    && phase80BosonModeSourceKind == "synthetic-boson-perturbation"
    && !phase80HasFullCouplingRecord
    && phase80IsTopCouplingSummaryOnly
    && !phase81ProductionInputsMaterialized
    && phase81SyntheticSourceBlocked
    && !unlockContractFilled
    && newSourceEvidenceStillRequired
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0
    && !couplingProxyPromotesWzMasses
    && !couplingProxyPromotesHiggsMass
    && !couplingProxyCompletesBosonPredictions;

var checks = new[]
{
    new Check(
        "phase4-coupling-proxy-not-physical-source",
        phase4SyntheticFallbackPresent && phase4NotPhysicalWarningPresent && phase4TopCouplingSummaryOnly,
        $"syntheticFallbackPresent={phase4SyntheticFallbackPresent}; notPhysicalWarningPresent={phase4NotPhysicalWarningPresent}; topCouplingSummaryOnly={phase4TopCouplingSummaryOnly}"),
    new Check(
        "phase12-coupling-atlases-are-finite-difference-proxies",
        phase12CouplingAtlasesPresent && phase12FiniteDifferenceOnly && phase12ReportRecordsPersistedFiniteDifference,
        $"atlasCount={phase12AtlasRows.Length}; couplingRecordCount={phase12CouplingRecordCount}; finiteDifferenceCount={phase12FiniteDifferenceCouplingCount}; analyticCount={phase12AnalyticCouplingCount}; reportRecordsPersistedFiniteDifference={phase12ReportRecordsPersistedFiniteDifference}"),
    new Check(
        "phase12-variation-bundles-not-analytic-production-replay",
        phase12VariationBundleCount == 24 && phase12VariationFiniteDifferenceCount == phase12VariationBundleCount && phase12VariationRawBosonVectorCount == phase12VariationBundleCount,
        $"variationBundleCount={phase12VariationBundleCount}; finiteDifferenceCount={phase12VariationFiniteDifferenceCount}; rawBosonVectorCount={phase12VariationRawBosonVectorCount}; blockedCount={phase12VariationBlockedCount}"),
    new Check(
        "weak-coupling-input-and-raw-evidence-gates-block-proxies",
        phase61FiniteDifferenceProxiesRejected && phase77RawMatrixElementEvidenceBlocked && phase77AcceptedRawMatrixElementMissing,
        $"phase61AcceptedCandidateCount={phase61AcceptedCandidateCount}; phase61RejectedCandidateCount={phase61RejectedCandidateCount}; phase77RawMatrixElementEvidenceBlocked={phase77RawMatrixElementEvidenceBlocked}; phase77AcceptedRawMatrixElementMissing={phase77AcceptedRawMatrixElementMissing}"),
    new Check(
        "analytic-production-replay-package-missing",
        !phase78ProductionAnalyticRecordsFound && phase78FiniteDifferenceAtlasesRemainBlocked && phase80ProductionAnalyticInputsBlocked && !phase80HasFullCouplingRecord && !phase81ProductionInputsMaterialized,
        $"phase78ProductionAnalyticRecordsFound={phase78ProductionAnalyticRecordsFound}; phase80ProductionAnalyticInputsBlocked={phase80ProductionAnalyticInputsBlocked}; phase80BosonModeSourceKind={phase80BosonModeSourceKind}; phase80HasFullCouplingRecord={phase80HasFullCouplingRecord}; phase81ProductionInputsMaterialized={phase81ProductionInputsMaterialized}"),
    new Check(
        "minimal-unlock-and-source-blockers-preserved",
        !unlockContractFilled && newSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "coupling-proxies-do-not-complete-boson-predictions",
        !couplingProxyPromotesWzMasses && !couplingProxyPromotesHiggsMass && !couplingProxyCompletesBosonPredictions,
        $"couplingProxyPromotesWzMasses={couplingProxyPromotesWzMasses}; couplingProxyPromotesHiggsMass={couplingProxyPromotesHiggsMass}; couplingProxyCompletesBosonPredictions={couplingProxyCompletesBosonPredictions}"),
};

var terminalStatus = couplingProxySourceAuditPassed
    ? "boson-fermion-coupling-proxy-source-audit-finite-difference-not-promotion"
    : "boson-fermion-coupling-proxy-source-audit-review-required";

var result = new
{
    phaseId = "phase273-boson-fermion-coupling-proxy-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    couplingProxySourceAuditPassed,
    couplingProxyLeadPresent = phase4CouplingProxyLeadPresent || phase12CouplingAtlasesPresent,
    phase4SyntheticFallbackPresent,
    phase4NotPhysicalWarningPresent,
    phase4TopCouplingSummaryOnly,
    phase12CouplingAtlasesPresent,
    phase12FiniteDifferenceOnly,
    phase12CouplingRecordCount,
    phase12FiniteDifferenceCouplingCount,
    phase12AnalyticCouplingCount,
    phase12VariationBundleCount,
    phase12VariationFiniteDifferenceCount,
    phase12VariationBlockedCount,
    phase12MaxCouplingProxyMagnitude,
    phase12MinBranchStabilityScore,
    phase12MaxBranchStabilityScore,
    phase61FiniteDifferenceProxiesRejected,
    phase77RawMatrixElementEvidenceBlocked,
    phase78ProductionAnalyticRecordsFound,
    phase80ProductionAnalyticInputsBlocked,
    phase80BosonModeSourceKind,
    phase80HasFullCouplingRecord,
    phase80IsTopCouplingSummaryOnly,
    phase81ProductionInputsMaterialized,
    phase81SyntheticSourceBlocked,
    couplingProxyPromotesWzMasses,
    couplingProxyPromotesHiggsMass,
    couplingProxyCompletesBosonPredictions,
    phase12AtlasRows,
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
    },
    requiredPromotionReplacement = new[]
    {
        "selected physical W/Z boson perturbation vector, not synthetic or raw profile-only perturbation",
        "analytic Dirac-variation matrix-element record with variationMethod=analytic-dirac-variation-matrix-element:v1",
        "unit-mode normalization and full real/imaginary coupling record",
        "validated replay evidence accepted by Phase77/Phase78/Phase80/Phase81 gates",
        "source-lineage rows that pass Phase201/Phase209/Phase210/Phase213 without target leakage",
    },
    checks,
    decision = couplingProxySourceAuditPassed
        ? "Do not promote W/Z or Higgs physical masses from Phase4/Phase12 boson-fermion coupling proxy artifacts. Phase4 records synthetic/top-summary coupling proxies, Phase12 records finite-difference coupling proxies, and the analytic production replay package required for physical weak-current evidence is still missing."
        : "Review coupling-proxy source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A production analytic Dirac-variation replay package for selected physical W/Z perturbation vectors.",
        "A source-lineage application theorem connecting that replayed matrix element to separate W and Z source rows.",
        "A separate solved Higgs scalar source/operator; coupling proxies alone do not fill the Higgs scalar-scale unlock.",
    },
    sourceEvidence = new
    {
        phase4SourcePath = Phase4SourcePath,
        phase4CouplingAtlasPath = Phase4CouplingAtlasPath,
        phase12ReportPath = Phase12ReportPath,
        phase12CouplingDir = Phase12CouplingDir,
        phase61Path = Phase61Path,
        phase77Path = Phase77Path,
        phase78Path = Phase78Path,
        phase80Path = Phase80Path,
        phase81Path = Phase81Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_fermion_coupling_proxy_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_fermion_coupling_proxy_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.couplingProxySourceAuditPassed,
        result.couplingProxyLeadPresent,
        result.phase4SyntheticFallbackPresent,
        result.phase4NotPhysicalWarningPresent,
        result.phase4TopCouplingSummaryOnly,
        result.phase12CouplingAtlasesPresent,
        result.phase12FiniteDifferenceOnly,
        result.phase12CouplingRecordCount,
        result.phase12FiniteDifferenceCouplingCount,
        result.phase12AnalyticCouplingCount,
        result.phase12VariationBundleCount,
        result.phase61FiniteDifferenceProxiesRejected,
        result.phase77RawMatrixElementEvidenceBlocked,
        result.phase78ProductionAnalyticRecordsFound,
        result.phase80ProductionAnalyticInputsBlocked,
        result.phase81ProductionInputsMaterialized,
        result.couplingProxyPromotesWzMasses,
        result.couplingProxyPromotesHiggsMass,
        result.couplingProxyCompletesBosonPredictions,
        result.currentBlockerEvidence,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"couplingProxySourceAuditPassed={couplingProxySourceAuditPassed}");
Console.WriteLine($"phase12CouplingRecordCount={phase12CouplingRecordCount}");
Console.WriteLine($"phase12FiniteDifferenceOnly={phase12FiniteDifferenceOnly}");
Console.WriteLine($"phase77RawMatrixElementEvidenceBlocked={phase77RawMatrixElementEvidenceBlocked}");
Console.WriteLine($"phase81ProductionInputsMaterialized={phase81ProductionInputsMaterialized}");
Console.WriteLine($"couplingProxyPromotesWzMasses={couplingProxyPromotesWzMasses}");
Console.WriteLine($"couplingProxyPromotesHiggsMass={couplingProxyPromotesHiggsMass}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static CouplingAtlasAuditRow ReadCouplingAtlas(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    var couplings = document.RootElement.GetProperty("couplings").EnumerateArray().ToArray();
    var magnitudes = couplings.Select(row => JsonDouble(row, "couplingProxyMagnitude")).Where(value => value.HasValue).Select(value => value!.Value).ToArray();
    var branchScores = couplings.Select(row => JsonDouble(row, "branchStabilityScore")).Where(value => value.HasValue).Select(value => value!.Value).ToArray();
    return new CouplingAtlasAuditRow(
        Path: path,
        CouplingCount: couplings.Length,
        FiniteDifferenceCount: couplings.Count(row => JsonString(row, "variationMethod") == "finite-difference"),
        AnalyticVariationCount: couplings.Count(row => JsonString(row, "variationMethod") == "analytic-dirac-variation-matrix-element:v1"),
        MaxCouplingProxyMagnitude: magnitudes.Length > 0 ? magnitudes.Max() : null,
        MinBranchStabilityScore: branchScores.Length > 0 ? branchScores.Min() : null,
        MaxBranchStabilityScore: branchScores.Length > 0 ? branchScores.Max() : null);
}

static VariationAuditRow ReadVariation(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return new VariationAuditRow(
        Path: path,
        VariationMethod: JsonString(document.RootElement, "variationMethod"),
        NormalizationConvention: JsonString(document.RootElement, "normalizationConvention"),
        Blocked: JsonBool(document.RootElement, "blocked") is true);
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

record CouplingAtlasAuditRow(
    string Path,
    int CouplingCount,
    int FiniteDifferenceCount,
    int AnalyticVariationCount,
    double? MaxCouplingProxyMagnitude,
    double? MinBranchStabilityScore,
    double? MaxBranchStabilityScore);

record VariationAuditRow(string Path, string? VariationMethod, string? NormalizationConvention, bool Blocked);

record Check(string CheckId, bool Passed, string Detail);
