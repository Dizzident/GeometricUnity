using System.Text.Json;

const string DefaultOutputDir = "studies/phase304_phase27_sector_aggregate_wz_source_audit_001/output";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string VariationMatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";
const string Phase27IdentityPath = "studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json";
const string Phase27FamiliesPath = "studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json";
const string Phase91Dir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase282Path = "studies/phase282_branch_local_direct_invariant_census_001/output/branch_local_direct_invariant_census_summary.json";
const string Phase299Path = "studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase303Path = "studies/phase303_identity_split_branch_source_normalization_audit_001/output/identity_split_branch_source_normalization_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE304_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var phase27Identity = JsonDocument.Parse(File.ReadAllText(Phase27IdentityPath));
using var phase27Families = JsonDocument.Parse(File.ReadAllText(Phase27FamiliesPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase282 = JsonDocument.Parse(File.ReadAllText(Phase282Path));
using var phase299 = JsonDocument.Parse(File.ReadAllText(Phase299Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase303 = JsonDocument.Parse(File.ReadAllText(Phase303Path));

var backgrounds = phase299.RootElement.GetProperty("rows")
    .EnumerateArray()
    .Select(row => RequiredString(row, "backgroundId"))
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
double targetRaw = RequiredDouble(phase299.RootElement, "targetRaw");
double rawGateRatio = RequiredDouble(phase299.RootElement, "rawGateRatio");
double stabilitySpreadTolerance = RequiredDouble(phase299.RootElement, "stabilitySpreadTolerance");
double commonScaleSpreadTolerance = 0.05;
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
bool p282Passed = JsonBool(phase282.RootElement, "branchLocalInvariantCensusPassed") is true;
bool p282NewLocalDirectInvariantSourceFound = JsonBool(phase282.RootElement, "newLocalDirectInvariantSourceFound") is true;
bool p299Passed = JsonBool(phase299.RootElement, "identitySplitProductionWzReplayAttemptPassed") is true;
bool p302Passed = JsonBool(phase302.RootElement, "identitySplitParticleNormalizationAuditPassed") is true;
bool p303Passed = JsonBool(phase303.RootElement, "identitySplitBranchSourceNormalizationAuditPassed") is true;
bool p303CanFillContract = JsonBool(phase303.RootElement, "canFillPhase201WzContract") is true;
bool phase27IdentityReady = string.Equals(JsonString(phase27Identity.RootElement, "terminalStatus"), "identity-rule-ready", StringComparison.Ordinal);
double p302WTotalScale = RequiredDouble(phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate"), "wTotalScale");
double p302ZTotalScale = RequiredDouble(phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate"), "zTotalScale");

var candidates = bosonRegistry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(candidate => new CandidateRecord(
        RequiredString(candidate, "candidateId"),
        candidate.GetProperty("contributingModeIds")
            .EnumerateArray()
            .Select(item => item.GetString() ?? "")
            .Where(item => item.Length > 0)
            .ToArray()))
    .OrderBy(candidate => CandidateOrdinal(candidate.CandidateId))
    .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();
var candidateMetadata = LoadCandidateMetadata();
var chargedCandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "charged", StringComparison.Ordinal))
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var neutralCandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "neutral", StringComparison.Ordinal))
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var chargedPlaneCandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "charged", StringComparison.Ordinal) && candidate.DominantBasisIndex is 0 or 1)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var chargedAxis0CandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "charged", StringComparison.Ordinal) && candidate.DominantBasisIndex == 0)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var chargedAxis1CandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "charged", StringComparison.Ordinal) && candidate.DominantBasisIndex == 1)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var neutralAxisCandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "neutral", StringComparison.Ordinal) && candidate.DominantBasisIndex == 2)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var identityWCandidateId = ToRegistryCandidateId(phase27Identity.RootElement.GetProperty("derivedRules")
    .EnumerateArray()
    .Single(rule => RequiredString(rule, "particleId") == "w-boson")
    .GetProperty("sourceId")
    .GetString() ?? "");
var identityZCandidateId = ToRegistryCandidateId(phase27Identity.RootElement.GetProperty("derivedRules")
    .EnumerateArray()
    .Single(rule => RequiredString(rule, "particleId") == "z-boson")
    .GetProperty("sourceId")
    .GetString() ?? "");

var sectorDefinitions = new[]
{
    new SectorPairDefinition("phase27-identity-singletons", [identityWCandidateId], [identityZCandidateId], "Phase27 derived W/Z singleton identities."),
    new SectorPairDefinition("phase27-all-charged-vs-all-neutral", chargedCandidateIds, neutralCandidateIds, "Root-sum-square over every Phase27 charged candidate for W and every neutral candidate for Z."),
    new SectorPairDefinition("phase27-charged-plane-vs-neutral-axis", chargedPlaneCandidateIds, neutralAxisCandidateIds, "Root-sum-square over Phase27 charged plane candidates with dominant axes 0/1 for W and neutral axis-2 candidates for Z."),
    new SectorPairDefinition("phase27-charged-axis-0-vs-neutral-axis", chargedAxis0CandidateIds, neutralAxisCandidateIds, "Root-sum-square over Phase27 charged axis-0 candidates for W and neutral axis-2 candidates for Z."),
    new SectorPairDefinition("phase27-charged-axis-1-vs-neutral-axis", chargedAxis1CandidateIds, neutralAxisCandidateIds, "Root-sum-square over Phase27 charged axis-1 candidates for W and neutral axis-2 candidates for Z."),
};

var backgroundModes = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadModes(Path.Combine(Phase91Dir, backgroundId, "branch_stability_promoted_fermion_modes.json")),
    StringComparer.Ordinal);
var pairKeys = backgroundModes[backgrounds[0]]
    .SelectMany(
        from => backgroundModes[backgrounds[0]].Where(to => to.ModeIndex != from.ModeIndex),
        (from, to) => new PairKey(from.ModeIndex, to.ModeIndex))
    .OrderBy(pair => pair.FromModeIndex)
    .ThenBy(pair => pair.ToModeIndex)
    .ToArray();
var matrices = LoadMatrices(backgrounds, candidates);

var sectorAssessments = pairKeys
    .SelectMany(pair => sectorDefinitions.Select(definition => AssessSectorPair(definition, pair)))
    .OrderBy(assessment => assessment.MaxParticleRelativeSpread)
    .ThenByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.SectorPairId, StringComparer.Ordinal)
    .ThenBy(assessment => assessment.Pair.FromModeIndex)
    .ThenBy(assessment => assessment.Pair.ToModeIndex)
    .ToArray();

var allRowsRawPassingAssessments = sectorAssessments.Where(assessment => assessment.AllRowsRawGatePassed).ToArray();
var stableAssessments = sectorAssessments.Where(assessment => assessment.AllParticlesStabilityPassed).ToArray();
var stableRawCommonAssessments = sectorAssessments.Where(assessment => assessment.StableRawCommonPassed).ToArray();
var stableP302ScaledAssessments = sectorAssessments.Where(assessment => assessment.P302ScaledStableRawCommonPassed).ToArray();
var bestAssessment = sectorAssessments.FirstOrDefault();
var bestP302ScaledAssessment = sectorAssessments
    .OrderByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
    .ThenBy(assessment => assessment.RequiredScaleRelativeSpread)
    .FirstOrDefault();
bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool theoremClaimed = false;
bool sourceRowsPromotable = false;
bool canFillPhase201WzContract = false;

var checks = new[]
{
    new Check(
        "phase27-sector-metadata-available",
        phase27IdentityReady && chargedCandidateIds.Length > 0 && neutralCandidateIds.Length > 0 && chargedPlaneCandidateIds.Length > 0 && neutralAxisCandidateIds.Length > 0,
        $"phase27IdentityReady={phase27IdentityReady}; chargedCandidateCount={chargedCandidateIds.Length}; neutralCandidateCount={neutralCandidateIds.Length}; chargedPlaneCandidateCount={chargedPlaneCandidateIds.Length}; chargedAxis0CandidateCount={chargedAxis0CandidateIds.Length}; chargedAxis1CandidateCount={chargedAxis1CandidateIds.Length}; neutralAxisCandidateCount={neutralAxisCandidateIds.Length}"),
    new Check(
        "upstream-audits-preserved",
        p282Passed && !p282NewLocalDirectInvariantSourceFound && p299Passed && p302Passed && p303Passed && !p303CanFillContract,
        $"p282Passed={p282Passed}; p282NewLocalDirectInvariantSourceFound={p282NewLocalDirectInvariantSourceFound}; p299Passed={p299Passed}; p302Passed={p302Passed}; p303Passed={p303Passed}; p303CanFillContract={p303CanFillContract}"),
    new Check(
        "sector-aggregate-search-target-independent",
        !targetObservablesUsedForConstruction && targetValuesUsedOnlyForPostCandidateEvaluation,
        $"targetObservablesUsedForConstruction={targetObservablesUsedForConstruction}; targetValuesUsedOnlyForPostCandidateEvaluation={targetValuesUsedOnlyForPostCandidateEvaluation}"),
    new Check(
        "no-sector-aggregate-clears-current-raw-stability-common-gates",
        stableRawCommonAssessments.Length == 0,
        $"stableRawCommonAssessmentCount={stableRawCommonAssessments.Length}; allRowsRawPassingAssessmentCount={allRowsRawPassingAssessments.Length}; stableAssessmentCount={stableAssessments.Length}; bestAssessment={bestAssessment?.AssessmentId}; bestMaxParticleRelativeSpread={bestAssessment?.MaxParticleRelativeSpread:R}; bestMinRowRawToTargetRatio={bestAssessment?.MinRowRawToTargetRatio:R}"),
    new Check(
        "no-sector-aggregate-repairs-phase302-scaled-near-pass",
        stableP302ScaledAssessments.Length == 0,
        $"p302ScaledStableRawCommonAssessmentCount={stableP302ScaledAssessments.Length}; bestP302ScaledAssessment={bestP302ScaledAssessment?.AssessmentId}; bestP302ScaledMinRowRawToTargetRatio={bestP302ScaledAssessment?.MinP302ScaledRowRawToTargetRatio:R}; bestP302ScaledMaxParticleRelativeSpread={bestP302ScaledAssessment?.MaxParticleRelativeSpread:R}"),
    new Check(
        "source-contract-remains-blocked",
        !theoremClaimed && !sourceRowsPromotable && !canFillPhase201WzContract && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"theoremClaimed={theoremClaimed}; sourceRowsPromotable={sourceRowsPromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

bool phase27SectorAggregateWzSourceAuditPassed =
    checks.All(check => check.Passed)
    && stableRawCommonAssessments.Length == 0
    && stableP302ScaledAssessments.Length == 0
    && !sourceRowsPromotable
    && !canFillPhase201WzContract;

var terminalStatus = phase27SectorAggregateWzSourceAuditPassed
    ? "phase27-sector-aggregate-wz-source-audit-no-stable-aggregate-source"
    : "phase27-sector-aggregate-wz-source-audit-review-required";

var result = new
{
    phaseId = "phase304-phase27-sector-aggregate-wz-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    phase27SectorAggregateWzSourceAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    targetRaw,
    rawGateRatio,
    stabilitySpreadTolerance,
    commonScaleSpreadTolerance,
    p302WTotalScale,
    p302ZTotalScale,
    backgroundIds = backgrounds,
    pairCount = pairKeys.Length,
    sectorDefinitionCount = sectorDefinitions.Length,
    assessmentCount = sectorAssessments.Length,
    phase27IdentityReady,
    identityWCandidateId,
    identityZCandidateId,
    chargedCandidateIds,
    neutralCandidateIds,
    chargedPlaneCandidateIds,
    chargedAxis0CandidateIds,
    chargedAxis1CandidateIds,
    neutralAxisCandidateIds,
    allRowsRawPassingAssessmentCount = allRowsRawPassingAssessments.Length,
    stableAssessmentCount = stableAssessments.Length,
    stableRawCommonAssessmentCount = stableRawCommonAssessments.Length,
    p302ScaledStableRawCommonAssessmentCount = stableP302ScaledAssessments.Length,
    theoremClaimed,
    sourceRowsPromotable,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    bestAssessment,
    bestP302ScaledAssessment,
    topAssessments = sectorAssessments.Take(18).ToArray(),
    sectorDefinitions,
    inheritedBlockers = new
    {
        phase282 = new
        {
            p282Passed,
            p282NewLocalDirectInvariantSourceFound,
        },
        phase299 = new
        {
            p299Passed,
            identitySplitRawGatePassed = JsonBool(phase299.RootElement, "identitySplitRawGatePassed"),
            identitySplitStabilityPassed = JsonBool(phase299.RootElement, "identitySplitStabilityPassed"),
        },
        phase302 = new
        {
            p302Passed,
            p302CanFillContract = JsonBool(phase302.RootElement, "canFillPhase201WzContract"),
        },
        phase303 = new
        {
            p303Passed,
            p303CanFillContract,
            stableRawCommonAllRowsCandidateCount = JsonInt(phase303.RootElement, "stableRawCommonAllRowsCandidateCount"),
        },
    },
    checks,
    decision = phase27SectorAggregateWzSourceAuditPassed
        ? "Do not repair the W/Z source law by replacing the Phase27 singleton W/Z rows with charged/neutral sector aggregates. The aggregates are target-independent, but they do not produce a stable raw/common source construction, and applying the Phase302 scale lead does not repair the all-row gate stack."
        : "Review Phase27 sector aggregate W/Z source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A theorem-backed W/Z source law that derives the charged/neutral source projection and its normalization before target comparison.",
        "A branch-stable aggregate or particle-specific source row construction that clears raw, common, and stability gates without post-hoc target fitting.",
        "Phase201/P209 W/Z source rows with derivation ids, raw sidecars, common-bridge sidecars, target-comparison sidecars, and stability sidecars filled.",
    },
    sourceEvidence = new
    {
        bosonRegistryPath = BosonRegistryPath,
        variationMatrixDir = VariationMatrixDir,
        phase27IdentityPath = Phase27IdentityPath,
        phase27FamiliesPath = Phase27FamiliesPath,
        phase91Dir = Phase91Dir,
        phase213Path = Phase213Path,
        phase282Path = Phase282Path,
        phase299Path = Phase299Path,
        phase302Path = Phase302Path,
        phase303Path = Phase303Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "phase27_sector_aggregate_wz_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase27_sector_aggregate_wz_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase27SectorAggregateWzSourceAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.targetRaw,
        result.rawGateRatio,
        result.stabilitySpreadTolerance,
        result.commonScaleSpreadTolerance,
        result.p302WTotalScale,
        result.p302ZTotalScale,
        result.backgroundIds,
        result.pairCount,
        result.sectorDefinitionCount,
        result.assessmentCount,
        result.phase27IdentityReady,
        result.identityWCandidateId,
        result.identityZCandidateId,
        result.chargedCandidateIds,
        result.neutralCandidateIds,
        result.chargedPlaneCandidateIds,
        result.chargedAxis0CandidateIds,
        result.chargedAxis1CandidateIds,
        result.neutralAxisCandidateIds,
        result.allRowsRawPassingAssessmentCount,
        result.stableAssessmentCount,
        result.stableRawCommonAssessmentCount,
        result.p302ScaledStableRawCommonAssessmentCount,
        result.theoremClaimed,
        result.sourceRowsPromotable,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.bestAssessment,
        result.bestP302ScaledAssessment,
        result.inheritedBlockers,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase27SectorAggregateWzSourceAuditPassed={phase27SectorAggregateWzSourceAuditPassed}");
Console.WriteLine($"chargedCandidateCount={chargedCandidateIds.Length}");
Console.WriteLine($"neutralCandidateCount={neutralCandidateIds.Length}");
Console.WriteLine($"pairCount={pairKeys.Length}");
Console.WriteLine($"assessmentCount={sectorAssessments.Length}");
Console.WriteLine($"allRowsRawPassingAssessmentCount={allRowsRawPassingAssessments.Length}");
Console.WriteLine($"stableAssessmentCount={stableAssessments.Length}");
Console.WriteLine($"stableRawCommonAssessmentCount={stableRawCommonAssessments.Length}");
Console.WriteLine($"p302ScaledStableRawCommonAssessmentCount={stableP302ScaledAssessments.Length}");
Console.WriteLine($"bestAssessment={bestAssessment?.AssessmentId}");
Console.WriteLine($"bestMaxParticleRelativeSpread={bestAssessment?.MaxParticleRelativeSpread:R}");
Console.WriteLine($"bestP302ScaledMinRowRawToTargetRatio={bestP302ScaledAssessment?.MinP302ScaledRowRawToTargetRatio:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

SectorAssessment AssessSectorPair(SectorPairDefinition definition, PairKey pair)
{
    var rows = backgrounds
        .SelectMany(backgroundId => new[]
        {
            BuildAggregateRow("w-boson", definition.WCandidateIds, backgroundId, pair),
            BuildAggregateRow("z-boson", definition.ZCandidateIds, backgroundId, pair),
        })
        .ToArray();
    var particleSummaries = rows
        .GroupBy(row => row.ParticleId, StringComparer.Ordinal)
        .OrderBy(group => group.Key, StringComparer.Ordinal)
        .Select(group =>
        {
            var groupRows = group.ToArray();
            var rawRatios = groupRows.Select(row => row.RawToTargetRatio).ToArray();
            var p302ScaledRatios = groupRows.Select(row => row.P302ScaledRawToTargetRatio).ToArray();
            double meanRawRatio = rawRatios.Average();
            double meanP302Scaled = p302ScaledRatios.Average();
            double relativeSpread = RelativeSpread(rawRatios);
            return new ParticleAggregateSummary(
                group.Key,
                groupRows.Length,
                meanRawRatio,
                meanP302Scaled,
                relativeSpread,
                relativeSpread <= stabilitySpreadTolerance,
                rawRatios.All(ratio => ratio >= rawGateRatio),
                p302ScaledRatios.All(ratio => ratio >= rawGateRatio));
        })
        .ToArray();
    var requiredScales = particleSummaries.Select(summary => summary.MeanRawToTargetRatio > 0.0 ? 1.0 / summary.MeanRawToTargetRatio : double.PositiveInfinity).ToArray();
    double requiredScaleSpread = RelativeSpread(requiredScales);
    double commonMeanSpread = RelativeSpread(particleSummaries.Select(summary => summary.MeanP302ScaledRawToTargetRatio));
    bool commonRawPassed = requiredScaleSpread <= commonScaleSpreadTolerance;
    bool p302CommonPassed = commonMeanSpread <= commonScaleSpreadTolerance;
    bool allRowsRawGatePassed = rows.All(row => row.RawToTargetRatio >= rawGateRatio);
    bool p302ScaledAllRowsRawGatePassed = rows.All(row => row.P302ScaledRawToTargetRatio >= rawGateRatio);
    bool allParticlesStabilityPassed = particleSummaries.All(summary => summary.StabilityPassed);
    bool stableRawCommonPassed = allRowsRawGatePassed && commonRawPassed && allParticlesStabilityPassed;
    bool p302ScaledStableRawCommonPassed = p302ScaledAllRowsRawGatePassed && p302CommonPassed && allParticlesStabilityPassed;
    return new SectorAssessment(
        $"{definition.SectorPairId}:{pair.FromModeIndex}->{pair.ToModeIndex}",
        definition.SectorPairId,
        pair,
        rows,
        particleSummaries,
        rows.Min(row => row.RawToTargetRatio),
        rows.Min(row => row.P302ScaledRawToTargetRatio),
        particleSummaries.Max(summary => summary.RelativeSpread),
        requiredScaleSpread,
        commonMeanSpread,
        allRowsRawGatePassed,
        p302ScaledAllRowsRawGatePassed,
        allParticlesStabilityPassed,
        commonRawPassed,
        p302CommonPassed,
        stableRawCommonPassed,
        p302ScaledStableRawCommonPassed);
}

AggregateRow BuildAggregateRow(string particleId, IReadOnlyList<string> candidateIds, string backgroundId, PairKey pair)
{
    var modes = backgroundModes[backgroundId];
    var from = modes.Single(mode => mode.ModeIndex == pair.FromModeIndex);
    var to = modes.Single(mode => mode.ModeIndex == pair.ToModeIndex);
    var components = candidateIds.Select(candidateId =>
    {
        var matrix = matrices[(backgroundId, candidateId)];
        var value = MatrixElement(matrix.Real, matrix.Imaginary, from.Coefficients, to.Coefficients);
        return new CandidateComponent(candidateId, value.Real, value.Imaginary, Magnitude(value.Real, value.Imaginary));
    }).ToArray();
    double aggregateNorm = Math.Sqrt(components.Sum(component => component.Magnitude * component.Magnitude));
    double rawToTargetRatio = aggregateNorm / targetRaw;
    double p302Scale = particleId == "w-boson" ? p302WTotalScale : p302ZTotalScale;
    return new AggregateRow(
        particleId,
        backgroundId,
        from.ModeId,
        to.ModeId,
        candidateIds,
        aggregateNorm,
        rawToTargetRatio,
        p302Scale,
        rawToTargetRatio * p302Scale,
        components.OrderByDescending(component => component.Magnitude).Take(8).ToArray());
}

Dictionary<string, CandidateMetadata> LoadCandidateMetadata()
{
    var metadata = phase27Families.RootElement.GetProperty("families")
        .EnumerateArray()
        .Select(family =>
        {
            string candidateId = RequiredString(family, "sourceCandidateId").Replace("phase12-", "", StringComparison.Ordinal);
            var features = family.GetProperty("identityFeatures");
            return new CandidateMetadata(
                candidateId,
                RequiredString(features, "chargeSector"),
                RequiredString(features, "algebraBasisSector"),
                JsonInt(features, "dominantBasisIndex") ?? -1);
        })
        .ToDictionary(candidate => candidate.CandidateId, StringComparer.Ordinal);

    foreach (var candidate in candidates)
    {
        if (!metadata.ContainsKey(candidate.CandidateId))
            throw new InvalidDataException($"Phase27 metadata missing for {candidate.CandidateId}.");
    }

    return metadata;
}

static IReadOnlyDictionary<(string BackgroundId, string CandidateId), MatrixRecord> LoadMatrices(
    IReadOnlyList<string> backgrounds,
    IReadOnlyList<CandidateRecord> candidates)
{
    var matrices = new Dictionary<(string BackgroundId, string CandidateId), MatrixRecord>();
    foreach (var backgroundId in backgrounds)
    {
        foreach (var candidate in candidates)
        {
            var path = Path.Combine(VariationMatrixDir, $"variation-{backgroundId}-{candidate.CandidateId}.matrix.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            matrices[(backgroundId, candidate.CandidateId)] = new MatrixRecord(
                backgroundId,
                candidate.CandidateId,
                LoadMatrix(doc.RootElement.GetProperty("real")),
                LoadMatrix(doc.RootElement.GetProperty("imag")));
        }
    }

    return matrices;
}

static IReadOnlyList<ModeRecord> LoadModes(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modes")
        .EnumerateArray()
        .Select(mode => new ModeRecord(
            RequiredString(mode, "modeId"),
            JsonInt(mode, "modeIndex") ?? throw new InvalidDataException("Missing modeIndex."),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray()))
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
}

static double[,] LoadMatrix(JsonElement rows)
{
    int rowCount = rows.GetArrayLength();
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rowCount, colCount];
    int row = 0;
    foreach (var rowElement in rows.EnumerateArray())
    {
        int col = 0;
        foreach (var value in rowElement.EnumerateArray())
            matrix[row, col++] = value.GetDouble();
        row++;
    }
    return matrix;
}

static (double Real, double Imaginary) MatrixElement(double[,] matrixRe, double[,] matrixIm, double[] phiI, double[] phiJ)
{
    int n = matrixRe.GetLength(0);
    var iNorm = Normalize(phiI);
    var jNorm = Normalize(phiJ);
    var deltaJ = new double[n * 2];
    for (int row = 0; row < n; row++)
    {
        double sumRe = 0.0;
        double sumIm = 0.0;
        for (int col = 0; col < n; col++)
        {
            double aRe = matrixRe[row, col];
            double aIm = matrixIm[row, col];
            double bRe = jNorm[col * 2];
            double bIm = jNorm[col * 2 + 1];
            sumRe += aRe * bRe - aIm * bIm;
            sumIm += aRe * bIm + aIm * bRe;
        }
        deltaJ[row * 2] = sumRe;
        deltaJ[row * 2 + 1] = sumIm;
    }

    double real = 0.0;
    double imaginary = 0.0;
    for (int k = 0; k < n; k++)
    {
        double iRe = iNorm[k * 2];
        double iIm = iNorm[k * 2 + 1];
        double dRe = deltaJ[k * 2];
        double dIm = deltaJ[k * 2 + 1];
        real += iRe * dRe + iIm * dIm;
        imaginary += iRe * dIm - iIm * dRe;
    }
    return (real, imaginary);
}

static double[] Normalize(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(value => value * value));
    return norm < 1e-30 ? vector : vector.Select(value => value / norm).ToArray();
}

static double RelativeSpread(IEnumerable<double> values)
{
    var array = values.Where(double.IsFinite).ToArray();
    if (array.Length == 0)
        return double.NaN;
    double mean = array.Average();
    return (array.Max() - array.Min()) / Math.Max(Math.Abs(mean), 1e-300);
}

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);

static int CandidateOrdinal(string candidateId)
{
    const string prefix = "candidate-";
    return candidateId.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(candidateId[prefix.Length..], out int value)
        ? value
        : int.MaxValue;
}

static string ToRegistryCandidateId(string sourceId)
{
    const string prefix = "phase22-phase12-";
    return sourceId.StartsWith(prefix, StringComparison.Ordinal)
        ? sourceId[prefix.Length..]
        : throw new InvalidDataException($"Cannot convert source id '{sourceId}' to registry candidate id.");
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

sealed record CandidateRecord(string CandidateId, IReadOnlyList<string> ContributingModeIds);
sealed record CandidateMetadata(string CandidateId, string ChargeSector, string AlgebraBasisSector, int DominantBasisIndex);
sealed record ModeRecord(string ModeId, int ModeIndex, double[] Coefficients);
sealed record MatrixRecord(string BackgroundId, string CandidateId, double[,] Real, double[,] Imaginary);
sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record CandidateComponent(string CandidateId, double Real, double Imaginary, double Magnitude);
sealed record SectorPairDefinition(string SectorPairId, IReadOnlyList<string> WCandidateIds, IReadOnlyList<string> ZCandidateIds, string Description);
sealed record AggregateRow(
    string ParticleId,
    string BackgroundId,
    string FromFermionModeId,
    string ToFermionModeId,
    IReadOnlyList<string> CandidateIds,
    double AggregateNorm,
    double RawToTargetRatio,
    double P302Scale,
    double P302ScaledRawToTargetRatio,
    IReadOnlyList<CandidateComponent> DominantComponents);
sealed record ParticleAggregateSummary(
    string ParticleId,
    int RowCount,
    double MeanRawToTargetRatio,
    double MeanP302ScaledRawToTargetRatio,
    double RelativeSpread,
    bool StabilityPassed,
    bool AllRowsRawGatePassed,
    bool P302ScaledAllRowsRawGatePassed);
sealed record SectorAssessment(
    string AssessmentId,
    string SectorPairId,
    PairKey Pair,
    IReadOnlyList<AggregateRow> Rows,
    IReadOnlyList<ParticleAggregateSummary> ParticleSummaries,
    double MinRowRawToTargetRatio,
    double MinP302ScaledRowRawToTargetRatio,
    double MaxParticleRelativeSpread,
    double RequiredScaleRelativeSpread,
    double P302ScaledCommonMeanRelativeSpread,
    bool AllRowsRawGatePassed,
    bool P302ScaledAllRowsRawGatePassed,
    bool AllParticlesStabilityPassed,
    bool CommonRawGatePassed,
    bool P302ScaledCommonGatePassed,
    bool StableRawCommonPassed,
    bool P302ScaledStableRawCommonPassed);
sealed record Check(string CheckId, bool Passed, string Detail);
