using System.Text.Json;

const string DefaultOutputDir = "studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase300Path = "studies/phase300_identity_split_common_normalization_audit_001/output/identity_split_common_normalization_audit.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase308Path = "studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE309_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase300 = JsonDocument.Parse(File.ReadAllText(Phase300Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase308 = JsonDocument.Parse(File.ReadAllText(Phase308Path));

var modeStats = phase300.RootElement.GetProperty("sourceModeStats")
    .EnumerateArray()
    .Select(row => LoadModeNormStats(RequiredString(row, "modePath")))
    .OrderBy(row => row.ModeId, StringComparer.Ordinal)
    .ToArray();

var distinctVectorLengths = modeStats.Select(row => row.VectorLength).Distinct().Order().ToArray();
int commonVectorLength = distinctVectorLengths.Length == 1 ? distinctVectorLengths[0] : -1;
double sqrtCommonVectorLength = commonVectorLength > 0 ? Math.Sqrt(commonVectorLength) : double.NaN;
double maxModeL2NormDeviationFromUnity = modeStats.Length == 0
    ? double.NaN
    : modeStats.Max(row => Math.Abs(row.L2Norm - 1.0));
double expectedUnitModeComponentRms = commonVectorLength > 0 ? 1.0 / sqrtCommonVectorLength : double.NaN;
double maxComponentRmsDeviationFromUnitModeExpectation = modeStats.Length == 0 || !double.IsFinite(expectedUnitModeComponentRms)
    ? double.NaN
    : modeStats.Max(row => Math.Abs(row.RmsComponent - expectedUnitModeComponentRms));

var sourceScaleCandidates = phase300.RootElement.GetProperty("sourceScaleCandidates").EnumerateArray().ToArray();
var sqrtVectorLengthScale = sourceScaleCandidates.Single(row => RequiredString(row, "scaleId") == "sqrt-source-mode-vector-length");
var vectorLengthScale = sourceScaleCandidates.Single(row => RequiredString(row, "scaleId") == "source-mode-vector-length");
var p302Best = phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate");

bool phase120PromotableAmplitudeMeasureFound = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true;
double phase120CommonScaleMean = RequiredDouble(phase120.RootElement, "commonScaleMean");
double phase120CommonScaleRelativeSpread = RequiredDouble(phase120.RootElement, "commonScaleRelativeSpread");
double phase120MaxScaledResidual = RequiredDouble(phase120.RootElement, "maxScaledResidual");
bool phase120MeasureScaleCompatibleWithIdentity =
    phase120PromotableAmplitudeMeasureFound
    && Math.Abs(phase120CommonScaleMean - 1.0) <= 1.0e-9
    && phase120CommonScaleRelativeSpread <= 1.0e-9
    && phase120MaxScaledResidual <= 1.0e-9;

bool sourceModesAreUnitNormCoordinateVectors =
    modeStats.Length > 0
    && commonVectorLength == 156
    && modeStats.All(row => row.NormalizationConvention == "unit-M-norm")
    && maxModeL2NormDeviationFromUnity <= 1.0e-9
    && maxComponentRmsDeviationFromUnitModeExpectation <= 1.0e-9;

bool sqrtVectorLengthScaleIsNormConversion =
    Math.Abs(RequiredDouble(sqrtVectorLengthScale, "scaleValue") - sqrtCommonVectorLength) <= 1.0e-12;
bool vectorLengthScaleIsCoordinateCount =
    RequiredDouble(vectorLengthScale, "scaleValue") == commonVectorLength;
bool vectorLengthScaleIsNotL2MeasureConversion =
    vectorLengthScaleIsCoordinateCount
    && sqrtVectorLengthScaleIsNormConversion
    && RequiredDouble(vectorLengthScale, "scaleValue") > RequiredDouble(sqrtVectorLengthScale, "scaleValue") * 10.0
    && JsonBool(sqrtVectorLengthScale, "rawGatePassedForBoth") is false
    && JsonBool(vectorLengthScale, "rawGatePassedForBoth") is false
    && JsonBool(vectorLengthScale, "promotionEligible") is false
    && JsonBool(phase300.RootElement, "vectorLengthScaleAccidentallyRepairsZOnly") is true
    && JsonInt(phase300.RootElement, "sourceDeclaredCommonScaleCandidatePassCount") == 0;

bool phase302VectorLengthCasimirLeadRecorded =
    JsonString(p302Best, "commonScaleId") == "source-mode-vector-length"
    && JsonString(p302Best, "particleLawId") == "adjoint-casimir-over-fundamental-casimir"
    && JsonBool(p302Best, "rawAndCommonGatesPassed") is true
    && JsonBool(p302Best, "stableRawCommonGatesPassed") is false
    && JsonBool(p302Best, "commonScaleApplicationTheoremPresent") is false
    && JsonBool(p302Best, "particleLawApplicationTheoremPresent") is false
    && JsonBool(p302Best, "promotionEligible") is false
    && JsonBool(phase302.RootElement, "canFillPhase201WzContract") is false;

bool phase308ScaleTransferStillBlocked =
    JsonBool(phase308.RootElement, "phase302ScaleTransferToDecoupledChargedLadderAuditPassed") is true
    && JsonBool(phase308.RootElement, "scaleTransferAllowed") is false
    && JsonBool(phase308.RootElement, "transferredScaleSourceRowsPromotable") is false
    && JsonBool(phase308.RootElement, "canFillPhase201WzContract") is false;

bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool hiddenMeasureConversionPresent = false;
bool sourceModeVectorLengthApplicationTheoremPresent = false;
bool sourceModeVectorLengthScalePromotable = false;
bool canFillPhase201WzContract = false;
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "analytic-variation-measure-is-already-unity",
        phase120MeasureScaleCompatibleWithIdentity,
        $"promotableAmplitudeMeasureFound={phase120PromotableAmplitudeMeasureFound}; commonScaleMean={phase120CommonScaleMean:R}; commonScaleRelativeSpread={phase120CommonScaleRelativeSpread:R}; maxScaledResidual={phase120MaxScaledResidual:R}"),
    new Check(
        "source-mode-vectors-are-unit-norm-coordinate-vectors",
        sourceModesAreUnitNormCoordinateVectors,
        $"modeCount={modeStats.Length}; commonVectorLength={commonVectorLength}; maxModeL2NormDeviationFromUnity={maxModeL2NormDeviationFromUnity:R}; expectedUnitModeComponentRms={expectedUnitModeComponentRms:R}; maxComponentRmsDeviationFromUnitModeExpectation={maxComponentRmsDeviationFromUnitModeExpectation:R}"),
    new Check(
        "vector-length-scale-is-not-l2-measure-conversion",
        vectorLengthScaleIsNotL2MeasureConversion,
        $"sqrtVectorLengthScale={RequiredDouble(sqrtVectorLengthScale, "scaleValue"):R}; vectorLengthScale={RequiredDouble(vectorLengthScale, "scaleValue"):R}; vectorLengthScaleAccidentallyRepairsZOnly={JsonBool(phase300.RootElement, "vectorLengthScaleAccidentallyRepairsZOnly")}; sourceDeclaredCommonScaleCandidatePassCount={JsonInt(phase300.RootElement, "sourceDeclaredCommonScaleCandidatePassCount")}"),
    new Check(
        "phase302-vector-length-casimir-lead-remains-nonpromotable",
        phase302VectorLengthCasimirLeadRecorded,
        $"candidateId={JsonString(p302Best, "candidateId")}; rawAndCommonGatesPassed={JsonBool(p302Best, "rawAndCommonGatesPassed")}; stableRawCommonGatesPassed={JsonBool(p302Best, "stableRawCommonGatesPassed")}; commonScaleApplicationTheoremPresent={JsonBool(p302Best, "commonScaleApplicationTheoremPresent")}; particleLawApplicationTheoremPresent={JsonBool(p302Best, "particleLawApplicationTheoremPresent")}; promotionEligible={JsonBool(p302Best, "promotionEligible")}"),
    new Check(
        "phase308-transfer-still-blocked",
        phase308ScaleTransferStillBlocked,
        $"phase302ScaleTransferToDecoupledChargedLadderAuditPassed={JsonBool(phase308.RootElement, "phase302ScaleTransferToDecoupledChargedLadderAuditPassed")}; scaleTransferAllowed={JsonBool(phase308.RootElement, "scaleTransferAllowed")}; transferredScaleSourceRowsPromotable={JsonBool(phase308.RootElement, "transferredScaleSourceRowsPromotable")}; canFillPhase201WzContract={JsonBool(phase308.RootElement, "canFillPhase201WzContract")}"),
    new Check(
        "source-contract-remains-blocked",
        !hiddenMeasureConversionPresent && !sourceModeVectorLengthApplicationTheoremPresent && !sourceModeVectorLengthScalePromotable && !canFillPhase201WzContract && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"hiddenMeasureConversionPresent={hiddenMeasureConversionPresent}; sourceModeVectorLengthApplicationTheoremPresent={sourceModeVectorLengthApplicationTheoremPresent}; sourceModeVectorLengthScalePromotable={sourceModeVectorLengthScalePromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

bool sourceModeVectorLengthMeasureNormalizationAuditPassed =
    checks.All(check => check.Passed)
    && !hiddenMeasureConversionPresent
    && !sourceModeVectorLengthApplicationTheoremPresent
    && !sourceModeVectorLengthScalePromotable
    && !canFillPhase201WzContract;

var terminalStatus = sourceModeVectorLengthMeasureNormalizationAuditPassed
    ? "source-mode-vector-length-measure-normalization-audit-vector-length-scale-not-measure-derived"
    : "source-mode-vector-length-measure-normalization-audit-review-required";

var result = new
{
    phaseId = "phase309-source-mode-vector-length-measure-normalization-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    sourceModeVectorLengthMeasureNormalizationAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    phase120PromotableAmplitudeMeasureFound,
    phase120CommonScaleMean,
    phase120CommonScaleRelativeSpread,
    phase120MaxScaledResidual,
    phase120MeasureScaleCompatibleWithIdentity,
    modeCount = modeStats.Length,
    commonVectorLength,
    sqrtCommonVectorLength,
    expectedUnitModeComponentRms,
    maxModeL2NormDeviationFromUnity,
    maxComponentRmsDeviationFromUnitModeExpectation,
    sourceModesAreUnitNormCoordinateVectors,
    modeStats,
    sqrtVectorLengthScale = new
    {
        scaleValue = RequiredDouble(sqrtVectorLengthScale, "scaleValue"),
        rawGatePassedForBoth = JsonBool(sqrtVectorLengthScale, "rawGatePassedForBoth"),
        commonScaleGatePassed = JsonBool(sqrtVectorLengthScale, "commonScaleGatePassed"),
        promotionEligible = JsonBool(sqrtVectorLengthScale, "promotionEligible"),
    },
    vectorLengthScale = new
    {
        scaleValue = RequiredDouble(vectorLengthScale, "scaleValue"),
        rawGatePassedForBoth = JsonBool(vectorLengthScale, "rawGatePassedForBoth"),
        commonScaleGatePassed = JsonBool(vectorLengthScale, "commonScaleGatePassed"),
        promotionEligible = JsonBool(vectorLengthScale, "promotionEligible"),
        vectorLengthScaleAccidentallyRepairsZOnly = JsonBool(phase300.RootElement, "vectorLengthScaleAccidentallyRepairsZOnly"),
    },
    sqrtVectorLengthScaleIsNormConversion,
    vectorLengthScaleIsCoordinateCount,
    vectorLengthScaleIsNotL2MeasureConversion,
    hiddenMeasureConversionPresent,
    sourceModeVectorLengthApplicationTheoremPresent,
    sourceModeVectorLengthScalePromotable,
    phase302VectorLengthCasimirLeadRecorded,
    phase302BestSourceInvariantRawCommonCandidate = new
    {
        candidateId = JsonString(p302Best, "candidateId"),
        commonScaleId = JsonString(p302Best, "commonScaleId"),
        particleLawId = JsonString(p302Best, "particleLawId"),
        commonScaleValue = JsonDouble(p302Best, "commonScaleValue"),
        wTotalScale = JsonDouble(p302Best, "wTotalScale"),
        zTotalScale = JsonDouble(p302Best, "zTotalScale"),
        rawAndCommonGatesPassed = JsonBool(p302Best, "rawAndCommonGatesPassed"),
        stableRawCommonGatesPassed = JsonBool(p302Best, "stableRawCommonGatesPassed"),
        commonScaleApplicationTheoremPresent = JsonBool(p302Best, "commonScaleApplicationTheoremPresent"),
        particleLawApplicationTheoremPresent = JsonBool(p302Best, "particleLawApplicationTheoremPresent"),
        promotionEligible = JsonBool(p302Best, "promotionEligible"),
    },
    phase308ScaleTransferStillBlocked,
    phase308ScaleTransferAllowed = JsonBool(phase308.RootElement, "scaleTransferAllowed"),
    phase308CanFillPhase201WzContract = JsonBool(phase308.RootElement, "canFillPhase201WzContract"),
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    checks,
    decision = sourceModeVectorLengthMeasureNormalizationAuditPassed
        ? "Do not treat the Phase302 source-mode-vector-length factor as a hidden amplitude-measure normalization. The relevant Phase12 modes are already unit-M-norm coordinate vectors, Phase120 validates the analytic/finite-difference amplitude measure at scale one, and the legitimate L2-to-component RMS conversion would be sqrt(156), not 156. The vector-length factor remains a coordinate-count diagnostic with no application theorem or Phase201 source-lineage fill."
        : "Review source-mode-vector-length measure normalization before relying on the Phase302 scale lead.",
    nextRequiredArtifact = new[]
    {
        "A source-side theorem deriving a non-unit amplitude measure factor from the GU variational operator before target comparison.",
        "A proof that the derived measure factor is vector length rather than sqrt(vector length), edge count, or the already validated Phase120 unit scale.",
        "W/Z source-lineage rows whose raw, common, target-comparison, stability, and derivation gates remain true under that theorem-backed measure.",
    },
    sourceEvidence = new
    {
        phase120Path = Phase120Path,
        phase213Path = Phase213Path,
        phase300Path = Phase300Path,
        phase302Path = Phase302Path,
        phase308Path = Phase308Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_mode_vector_length_measure_normalization_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_mode_vector_length_measure_normalization_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.sourceModeVectorLengthMeasureNormalizationAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.phase120PromotableAmplitudeMeasureFound,
        result.phase120CommonScaleMean,
        result.phase120CommonScaleRelativeSpread,
        result.phase120MaxScaledResidual,
        result.phase120MeasureScaleCompatibleWithIdentity,
        result.modeCount,
        result.commonVectorLength,
        result.sqrtCommonVectorLength,
        result.expectedUnitModeComponentRms,
        result.maxModeL2NormDeviationFromUnity,
        result.maxComponentRmsDeviationFromUnitModeExpectation,
        result.sourceModesAreUnitNormCoordinateVectors,
        result.sqrtVectorLengthScale,
        result.vectorLengthScale,
        result.sqrtVectorLengthScaleIsNormConversion,
        result.vectorLengthScaleIsCoordinateCount,
        result.vectorLengthScaleIsNotL2MeasureConversion,
        result.hiddenMeasureConversionPresent,
        result.sourceModeVectorLengthApplicationTheoremPresent,
        result.sourceModeVectorLengthScalePromotable,
        result.phase302VectorLengthCasimirLeadRecorded,
        result.phase302BestSourceInvariantRawCommonCandidate,
        result.phase308ScaleTransferStillBlocked,
        result.phase308ScaleTransferAllowed,
        result.phase308CanFillPhase201WzContract,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sourceModeVectorLengthMeasureNormalizationAuditPassed={sourceModeVectorLengthMeasureNormalizationAuditPassed}");
Console.WriteLine($"phase120CommonScaleMean={phase120CommonScaleMean:R}");
Console.WriteLine($"commonVectorLength={commonVectorLength}");
Console.WriteLine($"sqrtCommonVectorLength={sqrtCommonVectorLength:R}");
Console.WriteLine($"maxModeL2NormDeviationFromUnity={maxModeL2NormDeviationFromUnity:R}");
Console.WriteLine($"vectorLengthScaleIsNotL2MeasureConversion={vectorLengthScaleIsNotL2MeasureConversion}");
Console.WriteLine($"hiddenMeasureConversionPresent={hiddenMeasureConversionPresent}");
Console.WriteLine($"sourceModeVectorLengthScalePromotable={sourceModeVectorLengthScalePromotable}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static ModeNormStats LoadModeNormStats(string modePath)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(modePath));
    var vector = doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
    double l2Norm = Math.Sqrt(vector.Sum(value => value * value));
    double l1Norm = vector.Sum(Math.Abs);
    double signedSum = vector.Sum();
    return new ModeNormStats(
        RequiredString(doc.RootElement, "modeId"),
        modePath,
        RequiredString(doc.RootElement, "normalizationConvention"),
        vector.Length,
        l2Norm,
        l1Norm,
        Math.Sqrt(vector.Sum(value => value * value) / vector.Length),
        vector.Sum(Math.Abs) / vector.Length,
        signedSum,
        l1Norm > 0.0 ? Math.Abs(signedSum) / l1Norm : 0.0,
        vector.Select(Math.Abs).DefaultIfEmpty(0.0).Max());
}

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? ""
        : throw new InvalidDataException($"Missing string property '{propertyName}'.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidDataException($"Missing numeric property '{propertyName}'.");

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);

sealed record ModeNormStats(
    string ModeId,
    string ModePath,
    string NormalizationConvention,
    int VectorLength,
    double L2Norm,
    double L1Norm,
    double RmsComponent,
    double MeanAbsComponent,
    double SignedSum,
    double AbsoluteSignedSumToL1Ratio,
    double MaxAbsComponent);
