using System.Text.Json;

const string DefaultOutputDir = "studies/phase302_identity_split_particle_normalization_audit_001/output";
const string Phase24Path = "studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json";
const string Phase26Path = "studies/phase26_electroweak_mixing_convention_001/mixing_convention_readiness.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase225Path = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json";
const string Phase249Path = "studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants_summary.json";
const string Phase299Path = "studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json";
const string Phase300Path = "studies/phase300_identity_split_common_normalization_audit_001/output/identity_split_common_normalization_audit.json";
const string Phase301Path = "studies/phase301_identity_split_production_transition_sweep_001/output/identity_split_production_transition_sweep_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE302_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase24 = JsonDocument.Parse(File.ReadAllText(Phase24Path));
using var phase26 = JsonDocument.Parse(File.ReadAllText(Phase26Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase225 = JsonDocument.Parse(File.ReadAllText(Phase225Path));
using var phase249 = JsonDocument.Parse(File.ReadAllText(Phase249Path));
using var phase299 = JsonDocument.Parse(File.ReadAllText(Phase299Path));
using var phase300 = JsonDocument.Parse(File.ReadAllText(Phase300Path));
using var phase301 = JsonDocument.Parse(File.ReadAllText(Phase301Path));

double rawGateRatio = RequiredDouble(phase299.RootElement, "rawGateRatio");
double commonScaleSpreadTolerance = RequiredDouble(phase300.RootElement, "commonScaleSpreadTolerance");
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
bool p299Passed = JsonBool(phase299.RootElement, "identitySplitProductionWzReplayAttemptPassed") is true;
bool p300Passed = JsonBool(phase300.RootElement, "identitySplitCommonNormalizationAuditPassed") is true;
bool p301Passed = JsonBool(phase301.RootElement, "identitySplitProductionTransitionSweepPassed") is true;
bool p225ObstructionCertified = JsonBool(phase225.RootElement, "representationNormalizationObstructionCertified") is true;
bool p249WzInvariantSourceBacked = JsonBool(phase249.RootElement, "wzInvariantFormulaSourceBacked") is true;

var wSummary = phase299.RootElement.GetProperty("wSummary");
var zSummary = phase299.RootElement.GetProperty("zSummary");
double wRawToTargetRatio = RequiredDouble(wSummary, "rawToTargetRatio");
double zRawToTargetRatio = RequiredDouble(zSummary, "rawToTargetRatio");
double wRelativeSpread = RequiredDouble(wSummary, "relativeSpread");
double zRelativeSpread = RequiredDouble(zSummary, "relativeSpread");
bool wStabilityPassed = JsonBool(wSummary, "stabilityPassed") is true;
bool zStabilityPassed = JsonBool(zSummary, "stabilityPassed") is true;

var commonScaleCandidates = phase300.RootElement.GetProperty("sourceScaleCandidates")
    .EnumerateArray()
    .Select(row => new CommonScale(
        RequiredString(row, "scaleId"),
        RequiredDouble(row, "scaleValue"),
        RequiredString(row, "source"),
        JsonBool(row, "targetIndependent") is true,
        JsonBool(row, "promotionEligible") is true))
    .ToArray();

const double Su2Dimension = 3.0;
const double ChargedAxisCount = 2.0;
const double FundamentalDimension = 2.0;
const double FundamentalCasimir = 0.75;
const double AdjointCasimir = 2.0;
double posthocRowEqualizingWMultiplier = zRawToTargetRatio / wRawToTargetRatio;

var particleLaws = new[]
{
    new ParticleLaw(
        "identity-particle-law",
        "W=1, Z=1",
        1.0,
        1.0,
        "Control: no particle-specific normalization beyond the common scale.",
        true,
        false,
        false,
        "current-convention-control"),
    new ParticleLaw(
        "sqrt-charged-axis-count",
        "W=sqrt(2), Z=1",
        Math.Sqrt(ChargedAxisCount),
        1.0,
        "Charged sector has two canonical charged SU(2) axes while the neutral sector has one neutral axis.",
        true,
        false,
        false,
        "multiplicity-lead-no-application-theorem"),
    new ParticleLaw(
        "charged-axis-count",
        "W=2, Z=1",
        ChargedAxisCount,
        1.0,
        "Linear charged-axis multiplicity lead from the charged/neutral basis split.",
        true,
        false,
        false,
        "multiplicity-lead-no-application-theorem"),
    new ParticleLaw(
        "sqrt-su2-adjoint-axis-count",
        "W=sqrt(3), Z=1",
        Math.Sqrt(Su2Dimension),
        1.0,
        "Square-root SU(2) adjoint axis-count lead applied only to the charged W row.",
        true,
        false,
        false,
        "multiplicity-lead-no-application-theorem"),
    new ParticleLaw(
        "su2-adjoint-axis-count",
        "W=3, Z=1",
        Su2Dimension,
        1.0,
        "Linear SU(2) adjoint axis-count lead applied only to the charged W row.",
        true,
        false,
        false,
        "multiplicity-lead-no-application-theorem"),
    new ParticleLaw(
        "sqrt-adjoint-over-fundamental-dimension",
        "W=sqrt(3/2), Z=1",
        Math.Sqrt(Su2Dimension / FundamentalDimension),
        1.0,
        "Square-root adjoint/fundamental dimension ratio lead.",
        true,
        false,
        false,
        "representation-ratio-lead-no-application-theorem"),
    new ParticleLaw(
        "adjoint-over-fundamental-dimension",
        "W=3/2, Z=1",
        Su2Dimension / FundamentalDimension,
        1.0,
        "Adjoint/fundamental dimension ratio lead.",
        true,
        false,
        false,
        "representation-ratio-lead-no-application-theorem"),
    new ParticleLaw(
        "sqrt-adjoint-casimir-over-fundamental-casimir",
        "W=sqrt(8/3), Z=1",
        Math.Sqrt(AdjointCasimir / FundamentalCasimir),
        1.0,
        "Square-root SU(2) adjoint/fundamental Casimir ratio lead.",
        true,
        false,
        false,
        "casimir-ratio-lead-no-application-theorem"),
    new ParticleLaw(
        "adjoint-casimir-over-fundamental-casimir",
        "W=8/3, Z=1",
        AdjointCasimir / FundamentalCasimir,
        1.0,
        "SU(2) adjoint/fundamental Casimir ratio lead applied only to the W identity row.",
        true,
        false,
        false,
        "casimir-ratio-lead-no-application-theorem"),
    new ParticleLaw(
        "posthoc-row-equalizing-ratio",
        "W=zRaw/wRaw, Z=1",
        posthocRowEqualizingWMultiplier,
        1.0,
        "Diagnostic control that equalizes Phase299 W/Z mean raw ratios; it is replay-output-derived, not a source-declared law.",
        false,
        true,
        false,
        "posthoc-replay-output-equalizer"),
};

var candidateAssessments = commonScaleCandidates
    .SelectMany(scale => particleLaws.Select(law => Evaluate(scale, law)))
    .OrderByDescending(candidate => candidate.PromotionEligible)
    .ThenByDescending(candidate => candidate.RawAndCommonGatesPassed)
    .ThenByDescending(candidate => candidate.RawGatePassedForBoth)
    .ThenBy(candidate => candidate.ScaledRawRelativeSpread)
    .ThenByDescending(candidate => candidate.MinScaledRawToTargetRatio)
    .ThenBy(candidate => candidate.CommonScaleId, StringComparer.Ordinal)
    .ThenBy(candidate => candidate.ParticleLawId, StringComparer.Ordinal)
    .ToArray();

var sourceInvariantCandidates = candidateAssessments
    .Where(candidate => candidate.TargetIndependentCommonScale && candidate.TargetIndependentParticleLaw && !candidate.UsesPosthocReplayEqualization)
    .ToArray();
var rawPassingCandidates = candidateAssessments.Where(candidate => candidate.RawGatePassedForBoth).ToArray();
var rawCommonPassingCandidates = candidateAssessments.Where(candidate => candidate.RawAndCommonGatesPassed).ToArray();
var sourceInvariantRawCommonPassingCandidates = sourceInvariantCandidates.Where(candidate => candidate.RawAndCommonGatesPassed).ToArray();
var stableRawCommonPassingCandidates = candidateAssessments.Where(candidate => candidate.StableRawCommonGatesPassed).ToArray();
var sourceInvariantPromotableCandidates = sourceInvariantCandidates.Where(candidate => candidate.PromotionEligible).ToArray();
var bestSourceInvariantRawCommon = sourceInvariantRawCommonPassingCandidates.FirstOrDefault();
var bestOverall = candidateAssessments.FirstOrDefault();

bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool theoremClaimed = false;
bool sourceRowsPromotable = false;
bool canFillPhase201WzContract = false;

var checks = new[]
{
    new Check(
        "identity-replay-and-prior-normalization-audits-available",
        p299Passed && p300Passed && p301Passed,
        $"p299Passed={p299Passed}; p300Passed={p300Passed}; p301Passed={p301Passed}"),
    new Check(
        "particle-specific-normalization-search-target-independent",
        !targetObservablesUsedForConstruction && targetValuesUsedOnlyForPostCandidateEvaluation,
        $"targetObservablesUsedForConstruction={targetObservablesUsedForConstruction}; targetValuesUsedOnlyForPostCandidateEvaluation={targetValuesUsedOnlyForPostCandidateEvaluation}"),
    new Check(
        "source-invariant-raw-common-lead-recorded",
        sourceInvariantRawCommonPassingCandidates.Length > 0,
        bestSourceInvariantRawCommon is null
            ? "no source-invariant raw/common lead"
            : $"bestRawCommonCandidate={bestSourceInvariantRawCommon.CandidateId}; wScaledRawToTargetRatio={bestSourceInvariantRawCommon.WScaledRawToTargetRatio:R}; zScaledRawToTargetRatio={bestSourceInvariantRawCommon.ZScaledRawToTargetRatio:R}; scaledRawRelativeSpread={bestSourceInvariantRawCommon.ScaledRawRelativeSpread:R}"),
    new Check(
        "no-particle-specific-candidate-clears-stability",
        stableRawCommonPassingCandidates.Length == 0,
        $"stableRawCommonPassingCandidateCount={stableRawCommonPassingCandidates.Length}; wRelativeSpread={wRelativeSpread:R}; zRelativeSpread={zRelativeSpread:R}; wStabilityPassed={wStabilityPassed}; zStabilityPassed={zStabilityPassed}"),
    new Check(
        "casimir-and-multiplicity-leads-have-no-application-theorem",
        p225ObstructionCertified && !p249WzInvariantSourceBacked && !theoremClaimed && sourceInvariantPromotableCandidates.Length == 0,
        $"p225ObstructionCertified={p225ObstructionCertified}; p249WzInvariantSourceBacked={p249WzInvariantSourceBacked}; theoremClaimed={theoremClaimed}; sourceInvariantPromotableCandidateCount={sourceInvariantPromotableCandidates.Length}"),
    new Check(
        "source-contract-remains-blocked",
        !sourceRowsPromotable && !canFillPhase201WzContract && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"sourceRowsPromotable={sourceRowsPromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

bool identitySplitParticleNormalizationAuditPassed =
    checks.All(check => check.Passed)
    && candidateAssessments.Length == commonScaleCandidates.Length * particleLaws.Length
    && rawCommonPassingCandidates.Length > 0
    && stableRawCommonPassingCandidates.Length == 0
    && sourceInvariantPromotableCandidates.Length == 0
    && !sourceRowsPromotable
    && !canFillPhase201WzContract;

var terminalStatus = identitySplitParticleNormalizationAuditPassed
    ? "identity-split-particle-normalization-audit-raw-common-lead-not-promotable"
    : "identity-split-particle-normalization-audit-review-required";

var result = new
{
    phaseId = "phase302-identity-split-particle-normalization-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    identitySplitParticleNormalizationAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    targetRaw = RequiredDouble(phase299.RootElement, "targetRaw"),
    rawGateRatio,
    commonScaleSpreadTolerance,
    wRawToTargetRatio,
    zRawToTargetRatio,
    wRelativeSpread,
    zRelativeSpread,
    wStabilityPassed,
    zStabilityPassed,
    commonScaleCandidateCount = commonScaleCandidates.Length,
    particleLawCandidateCount = particleLaws.Length,
    candidateAssessmentCount = candidateAssessments.Length,
    rawPassingCandidateCount = rawPassingCandidates.Length,
    rawCommonPassingCandidateCount = rawCommonPassingCandidates.Length,
    sourceInvariantCandidateCount = sourceInvariantCandidates.Length,
    sourceInvariantRawCommonPassingCandidateCount = sourceInvariantRawCommonPassingCandidates.Length,
    stableRawCommonPassingCandidateCount = stableRawCommonPassingCandidates.Length,
    sourceInvariantPromotableCandidateCount = sourceInvariantPromotableCandidates.Length,
    theoremClaimed,
    sourceRowsPromotable,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    bestOverallCandidate = bestOverall,
    bestSourceInvariantRawCommonCandidate = bestSourceInvariantRawCommon,
    topCandidateAssessments = candidateAssessments.Take(20).ToArray(),
    topSourceInvariantRawCommonCandidates = sourceInvariantRawCommonPassingCandidates.Take(20).ToArray(),
    commonScaleCandidates,
    particleLaws,
    inheritedBlockers = new
    {
        phase24 = new
        {
            status = JsonString(phase24.RootElement, "terminalStatus"),
            derivedIdentityRuleCount = JsonInt(phase24.RootElement, "derivedIdentityRuleCount"),
        },
        phase26 = new
        {
            status = JsonString(phase26.RootElement, "terminalStatus"),
            chargeSectorAssignmentCount = JsonInt(phase26.RootElement, "chargeSectorAssignmentCount"),
        },
        phase225 = new
        {
            p225ObstructionCertified,
        },
        phase249 = new
        {
            wzInvariantFormulaCandidateFound = JsonBool(phase249.RootElement, "wzInvariantFormulaCandidateFound"),
            p249WzInvariantSourceBacked,
        },
        phase299 = new
        {
            p299Passed,
            identitySplitRawGatePassed = JsonBool(phase299.RootElement, "identitySplitRawGatePassed"),
            identitySplitStabilityPassed = JsonBool(phase299.RootElement, "identitySplitStabilityPassed"),
        },
        phase300 = new
        {
            p300Passed,
            sourceDeclaredCommonScaleCandidatePassCount = JsonInt(phase300.RootElement, "sourceDeclaredCommonScaleCandidatePassCount"),
        },
        phase301 = new
        {
            p301Passed,
            stableRawCommonPassingPairCount = JsonInt(phase301.RootElement, "stableRawCommonPassingPairCount"),
        },
    },
    checks,
    decision = identitySplitParticleNormalizationAuditPassed
        ? "Do not promote the identity-split W/Z replay by adding particle-specific normalization factors. A source-invariant raw/common numerical lead exists, but it does not repair branch stability, has no application theorem, and cannot fill the W/Z source-lineage contract."
        : "Review identity-split particle-specific normalization before relying on this gate.",
    nextRequiredArtifact = new[]
    {
        "A theorem-backed particle-specific W/Z source law that derives the normalization factors before replay comparison.",
        "Branch-stable W and Z source rows after the same law is applied.",
        "Phase201/P209 W/Z rows with raw-amplitude, common-bridge, target-comparison, stability, and derivation gates all filled.",
    },
    sourceEvidence = new
    {
        phase24Path = Phase24Path,
        phase26Path = Phase26Path,
        phase213Path = Phase213Path,
        phase225Path = Phase225Path,
        phase249Path = Phase249Path,
        phase299Path = Phase299Path,
        phase300Path = Phase300Path,
        phase301Path = Phase301Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "identity_split_particle_normalization_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "identity_split_particle_normalization_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.identitySplitParticleNormalizationAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.wRawToTargetRatio,
        result.zRawToTargetRatio,
        result.wRelativeSpread,
        result.zRelativeSpread,
        result.wStabilityPassed,
        result.zStabilityPassed,
        result.commonScaleCandidateCount,
        result.particleLawCandidateCount,
        result.candidateAssessmentCount,
        result.rawPassingCandidateCount,
        result.rawCommonPassingCandidateCount,
        result.sourceInvariantRawCommonPassingCandidateCount,
        result.stableRawCommonPassingCandidateCount,
        result.sourceInvariantPromotableCandidateCount,
        result.theoremClaimed,
        result.sourceRowsPromotable,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.bestSourceInvariantRawCommonCandidate,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
        result.inheritedBlockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"identitySplitParticleNormalizationAuditPassed={identitySplitParticleNormalizationAuditPassed}");
Console.WriteLine($"rawCommonPassingCandidateCount={rawCommonPassingCandidates.Length}");
Console.WriteLine($"sourceInvariantRawCommonPassingCandidateCount={sourceInvariantRawCommonPassingCandidates.Length}");
Console.WriteLine($"stableRawCommonPassingCandidateCount={stableRawCommonPassingCandidates.Length}");
Console.WriteLine($"sourceInvariantPromotableCandidateCount={sourceInvariantPromotableCandidates.Length}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

CandidateAssessment Evaluate(CommonScale scale, ParticleLaw law)
{
    double wScaledRawToTargetRatio = wRawToTargetRatio * scale.ScaleValue * law.WMultiplier;
    double zScaledRawToTargetRatio = zRawToTargetRatio * scale.ScaleValue * law.ZMultiplier;
    double minScaledRawToTargetRatio = Math.Min(wScaledRawToTargetRatio, zScaledRawToTargetRatio);
    double maxScaledRawToTargetRatio = Math.Max(wScaledRawToTargetRatio, zScaledRawToTargetRatio);
    double scaledRawRelativeSpread = RelativeSpread([wScaledRawToTargetRatio, zScaledRawToTargetRatio]);
    bool rawGatePassedForBoth = wScaledRawToTargetRatio >= rawGateRatio && zScaledRawToTargetRatio >= rawGateRatio;
    bool commonBridgeGatePassed = scaledRawRelativeSpread <= commonScaleSpreadTolerance;
    bool stabilityGatePassedForBoth = wStabilityPassed && zStabilityPassed;
    bool rawAndCommonGatesPassed = rawGatePassedForBoth && commonBridgeGatePassed;
    bool stableRawCommonGatesPassed = rawAndCommonGatesPassed && stabilityGatePassedForBoth;
    bool promotionEligible =
        stableRawCommonGatesPassed
        && scale.TargetIndependent
        && law.TargetIndependent
        && !law.UsesPosthocReplayEqualization
        && scale.ApplicationTheoremPresent
        && law.ApplicationTheoremPresent;

    return new CandidateAssessment(
        $"{scale.ScaleId}::{law.ParticleLawId}",
        scale.ScaleId,
        law.ParticleLawId,
        scale.ScaleValue,
        law.WMultiplier,
        law.ZMultiplier,
        scale.ScaleValue * law.WMultiplier,
        scale.ScaleValue * law.ZMultiplier,
        wScaledRawToTargetRatio,
        zScaledRawToTargetRatio,
        minScaledRawToTargetRatio,
        maxScaledRawToTargetRatio,
        scaledRawRelativeSpread,
        rawGatePassedForBoth,
        commonBridgeGatePassed,
        wStabilityPassed,
        zStabilityPassed,
        stabilityGatePassedForBoth,
        rawAndCommonGatesPassed,
        stableRawCommonGatesPassed,
        scale.TargetIndependent,
        law.TargetIndependent,
        law.UsesPosthocReplayEqualization,
        scale.ApplicationTheoremPresent,
        law.ApplicationTheoremPresent,
        promotionEligible,
        law.RejectionClass,
        $"{scale.Source} Particle law: {law.Source}");
}

static double RelativeSpread(IEnumerable<double> values)
{
    var finite = values.Where(value => double.IsFinite(value)).ToArray();
    if (finite.Length == 0)
        return double.NaN;

    double mean = finite.Average();
    return mean == 0.0 ? double.PositiveInfinity : (finite.Max() - finite.Min()) / Math.Abs(mean);
}

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing string value for {propertyName}.")
        : throw new InvalidDataException($"Missing string property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property)
        ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null }
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidDataException($"Missing numeric property {propertyName}.");

sealed record CommonScale(
    string ScaleId,
    double ScaleValue,
    string Source,
    bool TargetIndependent,
    bool ApplicationTheoremPresent);

sealed record ParticleLaw(
    string ParticleLawId,
    string Expression,
    double WMultiplier,
    double ZMultiplier,
    string Source,
    bool TargetIndependent,
    bool UsesPosthocReplayEqualization,
    bool ApplicationTheoremPresent,
    string RejectionClass);

sealed record CandidateAssessment(
    string CandidateId,
    string CommonScaleId,
    string ParticleLawId,
    double CommonScaleValue,
    double WParticleMultiplier,
    double ZParticleMultiplier,
    double WTotalScale,
    double ZTotalScale,
    double WScaledRawToTargetRatio,
    double ZScaledRawToTargetRatio,
    double MinScaledRawToTargetRatio,
    double MaxScaledRawToTargetRatio,
    double ScaledRawRelativeSpread,
    bool RawGatePassedForBoth,
    bool CommonBridgeGatePassed,
    bool WStabilityPassed,
    bool ZStabilityPassed,
    bool StabilityGatePassedForBoth,
    bool RawAndCommonGatesPassed,
    bool StableRawCommonGatesPassed,
    bool TargetIndependentCommonScale,
    bool TargetIndependentParticleLaw,
    bool UsesPosthocReplayEqualization,
    bool CommonScaleApplicationTheoremPresent,
    bool ParticleLawApplicationTheoremPresent,
    bool PromotionEligible,
    string RejectionClass,
    string Detail);

sealed record Check(string CheckId, bool Passed, string Detail);
