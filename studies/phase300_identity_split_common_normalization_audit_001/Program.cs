using System.Text.Json;

const string DefaultOutputDir = "studies/phase300_identity_split_common_normalization_audit_001/output";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase299Path = "studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE300_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));
using var phase299 = JsonDocument.Parse(File.ReadAllText(Phase299Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var spinor = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));

var wSummary = phase299.RootElement.GetProperty("wSummary");
var zSummary = phase299.RootElement.GetProperty("zSummary");
var rows = phase299.RootElement.GetProperty("rows")
    .EnumerateArray()
    .Select(row => new ReplayRow(
        RequiredString(row, "particleId"),
        RequiredString(row, "candidateId"),
        RequiredString(row, "backgroundId"),
        RequiredString(row, "etaModeId"),
        RequiredDouble(row, "rawMatrixElementMagnitude"),
        RequiredDouble(row, "rawToTargetRatio"),
        JsonBool(row, "rawGatePassed") is true,
        RequiredString(row, "variationEvidenceId")))
    .ToArray();

double targetRaw = RequiredDouble(phase299.RootElement, "targetRaw");
double rawGateRatio = RequiredDouble(phase299.RootElement, "rawGateRatio");
double commonScaleSpreadTolerance = 0.05;
double wMeanRaw = RequiredDouble(wSummary, "meanRawMagnitude");
double zMeanRaw = RequiredDouble(zSummary, "meanRawMagnitude");
double wRawToTargetRatio = RequiredDouble(wSummary, "rawToTargetRatio");
double zRawToTargetRatio = RequiredDouble(zSummary, "rawToTargetRatio");
double wRequiredScale = targetRaw / wMeanRaw;
double zRequiredScale = targetRaw / zMeanRaw;
double requiredScaleRelativeSpread = RelativeSpread([wRequiredScale, zRequiredScale]);
bool commonRequiredScaleGatePassed = requiredScaleRelativeSpread <= commonScaleSpreadTolerance;
double meanRequiredScale = (wRequiredScale + zRequiredScale) / 2.0;
double leastSquaresCommonScale = targetRaw * (wMeanRaw + zMeanRaw) / (wMeanRaw * wMeanRaw + zMeanRaw * zMeanRaw);
double minimumCommonScaleForMeanRawGate = rawGateRatio / Math.Min(wRawToTargetRatio, zRawToTargetRatio);
double minimumCommonScaleMeanRawGateWRatio = minimumCommonScaleForMeanRawGate * wRawToTargetRatio;
double minimumCommonScaleMeanRawGateZRatio = minimumCommonScaleForMeanRawGate * zRawToTargetRatio;
double minimumCommonScaleMeanRawGateSpread = RelativeSpread([
    minimumCommonScaleMeanRawGateWRatio,
    minimumCommonScaleMeanRawGateZRatio,
]);
double minimumCommonScaleForAllRowsRawGate = rawGateRatio / rows.Min(row => row.RawToTargetRatio);
double rowRequiredScaleRelativeSpread = RelativeSpread(rows.Select(row => 1.0 / row.RawToTargetRatio));

var sourceModes = rows
    .Select(row => LoadModeStats(row.VariationEvidenceId))
    .DistinctBy(mode => mode.ModePath, StringComparer.Ordinal)
    .OrderBy(mode => mode.ModeId, StringComparer.Ordinal)
    .ToArray();
var sourceModeVectorLengths = sourceModes.Select(mode => mode.VectorLength).Distinct().Order().ToArray();
var sourceModeVectorNorms = sourceModes.Select(mode => mode.VectorL2Norm).ToArray();
int sourceVectorLength = sourceModeVectorLengths.Length == 1 ? sourceModeVectorLengths[0] : sourceModeVectorLengths.Min();
int dimG = 3;
int inferredEdgeCount = sourceVectorLength / dimG;
int spinorComponents = JsonInt(spinor.RootElement, "spinorComponents") ?? 0;
double phase120CommonScaleMean = RequiredDouble(phase120.RootElement, "commonScaleMean");
double phase221CasimirToTraceHalfScale = RequiredDouble(phase221.RootElement, "casimirToTraceHalfScale");

var scaleInputs = new[]
{
    new ScaleInput("identity-scale", 1.0, "Current accepted source-backed replay convention.", true),
    new ScaleInput("phase120-finite-analytic-common-scale", phase120CommonScaleMean, "Phase120 finite-to-analytic common-scale diagnostic.", true),
    new ScaleInput("phase221-casimir-to-trace-half-scale", phase221CasimirToTraceHalfScale, "Phase221 SU(2) Casimir/RMS numerical lead; already non-promotional upstream.", true),
    new ScaleInput("sqrt-su2-adjoint-dimension", Math.Sqrt(dimG), "Square root of SU(2) adjoint component count.", true),
    new ScaleInput("su2-adjoint-dimension", dimG, "SU(2) adjoint component count.", true),
    new ScaleInput("spinor-components", spinorComponents, "Phase12 spinor component count.", true),
    new ScaleInput("sqrt-source-edge-count", Math.Sqrt(inferredEdgeCount), "Square root of edge count inferred from source perturbation vector length / dimG.", true),
    new ScaleInput("source-edge-count", inferredEdgeCount, "Edge count inferred from source perturbation vector length / dimG.", true),
    new ScaleInput("sqrt-source-mode-vector-length", Math.Sqrt(sourceVectorLength), "Square root of source perturbation vector length.", true),
    new ScaleInput("source-mode-vector-length", sourceVectorLength, "Source perturbation vector length; diagnostic only, not a theorem-backed scale.", true),
};

var sourceScaleCandidates = scaleInputs
    .Where(scale => double.IsFinite(scale.ScaleValue) && scale.ScaleValue > 0.0)
    .Select(scale => EvaluateScale(scale, wRawToTargetRatio, zRawToTargetRatio, rawGateRatio, commonScaleSpreadTolerance))
    .ToArray();
var sourceDeclaredCommonScaleCandidatePassCount = sourceScaleCandidates.Count(scale => scale.PromotionEligible);
var vectorLengthScale = sourceScaleCandidates.Single(scale => scale.ScaleId == "source-mode-vector-length");
bool vectorLengthScaleAccidentallyRepairsZOnly =
    vectorLengthScale.ZScaledRawToTargetRatio >= rawGateRatio &&
    vectorLengthScale.WScaledRawToTargetRatio < rawGateRatio;
bool targetDerivedMinimumCommonScaleRawGatePassed =
    minimumCommonScaleMeanRawGateWRatio >= rawGateRatio &&
    minimumCommonScaleMeanRawGateZRatio >= rawGateRatio;
bool targetDerivedMinimumCommonScaleCommonGatePassed =
    minimumCommonScaleMeanRawGateSpread <= commonScaleSpreadTolerance;

bool identitySplitReplayPassed = JsonBool(phase299.RootElement, "identitySplitProductionWzReplayAttemptPassed") is true;
bool productionInputsClosed = JsonBool(phase299.RootElement, "productionInputGapClosedForIdentitySplitCandidates") is true;
bool sourceRowsPromotable = false;
bool commonNormalizationCanFillPhase201WzContract = false;
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var identitySplitCommonNormalizationAuditPassed =
    identitySplitReplayPassed
    && productionInputsClosed
    && !commonRequiredScaleGatePassed
    && sourceDeclaredCommonScaleCandidatePassCount == 0
    && vectorLengthScaleAccidentallyRepairsZOnly
    && targetDerivedMinimumCommonScaleRawGatePassed
    && !targetDerivedMinimumCommonScaleCommonGatePassed
    && !sourceRowsPromotable
    && !commonNormalizationCanFillPhase201WzContract
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "identity-split-production-replay-available",
        identitySplitReplayPassed && productionInputsClosed && rows.Length == 4,
        $"identitySplitProductionWzReplayAttemptPassed={identitySplitReplayPassed}; productionInputGapClosedForIdentitySplitCandidates={productionInputsClosed}; rowCount={rows.Length}"),
    new Check(
        "w-z-required-scales-are-not-common",
        !commonRequiredScaleGatePassed,
        $"wRequiredScale={wRequiredScale:R}; zRequiredScale={zRequiredScale:R}; requiredScaleRelativeSpread={requiredScaleRelativeSpread:R}; tolerance={commonScaleSpreadTolerance:R}"),
    new Check(
        "source-declared-common-scales-do-not-repair-identity-split",
        sourceDeclaredCommonScaleCandidatePassCount == 0,
        $"testedSourceScaleCandidateCount={sourceScaleCandidates.Length}; sourceDeclaredCommonScaleCandidatePassCount={sourceDeclaredCommonScaleCandidatePassCount}"),
    new Check(
        "source-vector-length-scale-is-z-only-near-miss",
        vectorLengthScaleAccidentallyRepairsZOnly,
        $"scale={vectorLengthScale.ScaleValue:R}; wScaledRawToTargetRatio={vectorLengthScale.WScaledRawToTargetRatio:R}; zScaledRawToTargetRatio={vectorLengthScale.ZScaledRawToTargetRatio:R}; rawGateRatio={rawGateRatio:R}"),
    new Check(
        "target-derived-minimum-common-scale-is-not-a-common-bridge",
        targetDerivedMinimumCommonScaleRawGatePassed && !targetDerivedMinimumCommonScaleCommonGatePassed,
        $"minimumCommonScaleForMeanRawGate={minimumCommonScaleForMeanRawGate:R}; wScaledRawToTargetRatio={minimumCommonScaleMeanRawGateWRatio:R}; zScaledRawToTargetRatio={minimumCommonScaleMeanRawGateZRatio:R}; scaledRawRelativeSpread={minimumCommonScaleMeanRawGateSpread:R}"),
    new Check(
        "source-contract-remains-blocked",
        !sourceRowsPromotable && !commonNormalizationCanFillPhase201WzContract && wzMissingFieldCount > 0,
        $"sourceRowsPromotable={sourceRowsPromotable}; commonNormalizationCanFillPhase201WzContract={commonNormalizationCanFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}"),
};

var terminalStatus = identitySplitCommonNormalizationAuditPassed
    ? "identity-split-common-normalization-audit-common-scale-blocked"
    : "identity-split-common-normalization-audit-review-required";

var result = new
{
    phaseId = "phase300-identity-split-common-normalization-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    identitySplitCommonNormalizationAuditPassed,
    targetValuesUsedOnlyForRequiredScaleDiagnostic = true,
    sourceScaleCandidatesUseTargets = false,
    identitySplitReplayPassed,
    productionInputsClosed,
    targetRaw,
    rawGateRatio,
    commonScaleSpreadTolerance,
    wMeanRawMagnitude = wMeanRaw,
    zMeanRawMagnitude = zMeanRaw,
    wRawToTargetRatio,
    zRawToTargetRatio,
    wRequiredScaleToTargetRaw = wRequiredScale,
    zRequiredScaleToTargetRaw = zRequiredScale,
    requiredScaleRelativeSpread,
    commonRequiredScaleGatePassed,
    meanRequiredScale,
    leastSquaresCommonScale,
    minimumCommonScaleForMeanRawGate,
    minimumCommonScaleMeanRawGateWRatio,
    minimumCommonScaleMeanRawGateZRatio,
    minimumCommonScaleMeanRawGateSpread,
    minimumCommonScaleForAllRowsRawGate,
    rowRequiredScaleRelativeSpread,
    sourceModeStats = sourceModes,
    sourceScaleCandidates,
    testedSourceScaleCandidateCount = sourceScaleCandidates.Length,
    sourceDeclaredCommonScaleCandidatePassCount,
    vectorLengthScaleAccidentallyRepairsZOnly,
    targetDerivedMinimumCommonScaleRawGatePassed,
    targetDerivedMinimumCommonScaleCommonGatePassed,
    sourceRowsPromotable,
    commonNormalizationCanFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    inheritedBlockers = new
    {
        phase299 = new
        {
            identitySplitProductionWzReplayAttemptPassed = JsonBool(phase299.RootElement, "identitySplitProductionWzReplayAttemptPassed"),
            identitySplitRawGatePassed = JsonBool(phase299.RootElement, "identitySplitRawGatePassed"),
            identitySplitStabilityPassed = JsonBool(phase299.RootElement, "identitySplitStabilityPassed"),
            sourceRowsPromotable = JsonBool(phase299.RootElement, "sourceRowsPromotable"),
            canFillPhase201WzContract = JsonBool(phase299.RootElement, "canFillPhase201WzContract"),
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    checks,
    decision = "Do not repair the Phase27 identity split by applying a common normalization factor. The W and Z production replay rows require incompatible target-implied scales, and audited target-independent scale candidates do not satisfy both raw and common-bridge gates.",
    nextRequiredArtifact = new[]
    {
        "A theorem-backed particle-specific W/Z bridge source with a common normalization law derived before target comparison.",
        "A raw-amplitude law that lifts both identity-selected rows while preserving W/Z common-bridge consistency.",
        "Phase201/P209 source-lineage rows with raw, common, target-comparison, stability, and derivation gates.",
    },
    sourceEvidence = new
    {
        phase120Path = Phase120Path,
        phase221Path = Phase221Path,
        phase299Path = Phase299Path,
        phase213Path = Phase213Path,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "identity_split_common_normalization_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "identity_split_common_normalization_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.identitySplitCommonNormalizationAuditPassed,
        result.targetValuesUsedOnlyForRequiredScaleDiagnostic,
        result.sourceScaleCandidatesUseTargets,
        result.wRawToTargetRatio,
        result.zRawToTargetRatio,
        result.wRequiredScaleToTargetRaw,
        result.zRequiredScaleToTargetRaw,
        result.commonScaleSpreadTolerance,
        result.requiredScaleRelativeSpread,
        result.commonRequiredScaleGatePassed,
        result.minimumCommonScaleForMeanRawGate,
        result.minimumCommonScaleMeanRawGateWRatio,
        result.minimumCommonScaleMeanRawGateZRatio,
        result.minimumCommonScaleMeanRawGateSpread,
        result.minimumCommonScaleForAllRowsRawGate,
        result.rowRequiredScaleRelativeSpread,
        result.testedSourceScaleCandidateCount,
        result.sourceDeclaredCommonScaleCandidatePassCount,
        result.vectorLengthScaleAccidentallyRepairsZOnly,
        result.targetDerivedMinimumCommonScaleRawGatePassed,
        result.targetDerivedMinimumCommonScaleCommonGatePassed,
        result.sourceRowsPromotable,
        result.commonNormalizationCanFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"identitySplitCommonNormalizationAuditPassed={identitySplitCommonNormalizationAuditPassed}");
Console.WriteLine($"wRequiredScaleToTargetRaw={wRequiredScale:R}");
Console.WriteLine($"zRequiredScaleToTargetRaw={zRequiredScale:R}");
Console.WriteLine($"requiredScaleRelativeSpread={requiredScaleRelativeSpread:R}");
Console.WriteLine($"sourceDeclaredCommonScaleCandidatePassCount={sourceDeclaredCommonScaleCandidatePassCount}");
Console.WriteLine($"minimumCommonScaleForMeanRawGate={minimumCommonScaleForMeanRawGate:R}");
Console.WriteLine($"minimumCommonScaleMeanRawGateSpread={minimumCommonScaleMeanRawGateSpread:R}");
Console.WriteLine($"commonNormalizationCanFillPhase201WzContract={commonNormalizationCanFillPhase201WzContract}");

static ScaleCandidate EvaluateScale(
    ScaleInput input,
    double wRawToTargetRatio,
    double zRawToTargetRatio,
    double rawGateRatio,
    double commonScaleSpreadTolerance)
{
    double wScaled = input.ScaleValue * wRawToTargetRatio;
    double zScaled = input.ScaleValue * zRawToTargetRatio;
    double spread = RelativeSpread([wScaled, zScaled]);
    bool rawGatePassedForBoth = wScaled >= rawGateRatio && zScaled >= rawGateRatio;
    bool commonScaleGatePassed = spread <= commonScaleSpreadTolerance;
    bool promotionEligible = input.TargetIndependent && rawGatePassedForBoth && commonScaleGatePassed;
    return new ScaleCandidate(
        input.ScaleId,
        input.ScaleValue,
        input.Source,
        input.TargetIndependent,
        wScaled,
        zScaled,
        Math.Min(wScaled, zScaled),
        Math.Max(wScaled, zScaled),
        spread,
        rawGatePassedForBoth,
        commonScaleGatePassed,
        promotionEligible);
}

static SourceModeStats LoadModeStats(string variationEvidenceId)
{
    var parts = variationEvidenceId.Split(':', 3);
    if (parts.Length != 3)
        throw new InvalidDataException($"Cannot parse variation evidence id: {variationEvidenceId}");

    var modePath = parts[2];
    using var doc = JsonDocument.Parse(File.ReadAllText(modePath));
    var root = doc.RootElement;
    var modeId = RequiredString(root, "modeId");
    string sourceFieldName;
    JsonElement vectorElement;
    if (root.TryGetProperty("modeVector", out var modeVectorElement))
    {
        sourceFieldName = "modeVector";
        vectorElement = modeVectorElement;
    }
    else if (root.TryGetProperty("eigenvectorCoefficients", out var eigenvectorElement))
    {
        sourceFieldName = "eigenvectorCoefficients";
        vectorElement = eigenvectorElement;
    }
    else
    {
        throw new InvalidDataException($"Mode JSON has no vector field: {modePath}");
    }

    var vector = vectorElement.EnumerateArray().Select(value => value.GetDouble()).ToArray();
    var norm = Math.Sqrt(vector.Sum(value => value * value));
    return new SourceModeStats(
        modeId,
        modePath,
        sourceFieldName,
        JsonString(root, "normalizationConvention"),
        vector.Length,
        norm);
}

static double RelativeSpread(IEnumerable<double> values)
{
    var finite = values.Where(double.IsFinite).ToArray();
    if (finite.Length == 0)
        return double.NaN;
    var min = finite.Min();
    var max = finite.Max();
    var mean = finite.Average();
    return (max - min) / Math.Max(Math.Abs(mean), 1e-300);
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record ReplayRow(
    string ParticleId,
    string CandidateId,
    string BackgroundId,
    string EtaModeId,
    double RawMatrixElementMagnitude,
    double RawToTargetRatio,
    bool RawGatePassed,
    string VariationEvidenceId);

sealed record SourceModeStats(
    string ModeId,
    string ModePath,
    string SourceFieldName,
    string? NormalizationConvention,
    int VectorLength,
    double VectorL2Norm);

sealed record ScaleInput(
    string ScaleId,
    double ScaleValue,
    string Source,
    bool TargetIndependent);

sealed record ScaleCandidate(
    string ScaleId,
    double ScaleValue,
    string Source,
    bool TargetIndependent,
    double WScaledRawToTargetRatio,
    double ZScaledRawToTargetRatio,
    double MinScaledRawToTargetRatio,
    double MaxScaledRawToTargetRatio,
    double ScaledRawRelativeSpread,
    bool RawGatePassedForBoth,
    bool CommonScaleGatePassed,
    bool PromotionEligible);

sealed record Check(string CheckId, bool Passed, string Detail);
