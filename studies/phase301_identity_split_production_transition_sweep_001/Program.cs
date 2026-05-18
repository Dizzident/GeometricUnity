using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;
using Gu.Phase5.Reporting;

const string DefaultOutputDir = "studies/phase301_identity_split_production_transition_sweep_001/output";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string BosonModeVectorDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";
const string PromotedFermionModeDir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase282Path = "studies/phase282_branch_local_direct_invariant_census_001/output/branch_local_direct_invariant_census_summary.json";
const string Phase299Path = "studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json";
const string Phase300Path = "studies/phase300_identity_split_common_normalization_audit_001/output/identity_split_common_normalization_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE301_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var jsonOptions = JsonOptions();
using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase282 = JsonDocument.Parse(File.ReadAllText(Phase282Path));
using var phase299 = JsonDocument.Parse(File.ReadAllText(Phase299Path));
using var phase300 = JsonDocument.Parse(File.ReadAllText(Phase300Path));

var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(jsonOptions)
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}");
var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase301-identity-split-production-transition-sweep",
    Branch = new()
    {
        BranchId = "phase301-identity-split-production-transition-sweep",
        SchemaVersion = "1.0.0",
    },
    Backend = "cpu-reference",
    Notes = "Production analytic matrix-element sweep over all promoted fermion transitions for the Phase299 identity-selected W/Z source modes.",
};
var gammas = new GammaMatrixBuilder().Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
var geometry = BuildGeometry(mesh);
int spinorDim = spinorSpec.SpinorComponents;
int dimG = 3;
double targetRaw = RequiredDouble(phase299.RootElement, "targetRaw");
double rawGateRatio = RequiredDouble(phase299.RootElement, "rawGateRatio");
double stabilitySpreadTolerance = RequiredDouble(phase299.RootElement, "stabilitySpreadTolerance");
double commonScaleSpreadTolerance = RequiredDouble(phase300.RootElement, "commonScaleSpreadTolerance");
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
bool p282NewLocalDirectInvariantSourceFound = JsonBool(phase282.RootElement, "newLocalDirectInvariantSourceFound") is true;
bool p299IdentitySplitPassed = JsonBool(phase299.RootElement, "identitySplitProductionWzReplayAttemptPassed") is true;
bool p300CommonNormalizationPassed = JsonBool(phase300.RootElement, "identitySplitCommonNormalizationAuditPassed") is true;
bool p300CommonNormalizationCanFillContract = JsonBool(phase300.RootElement, "commonNormalizationCanFillPhase201WzContract") is true;

var rows = phase299.RootElement.GetProperty("rows")
    .EnumerateArray()
    .Select(row => new IdentitySourceRow(
        RequiredString(row, "particleId"),
        RequiredString(row, "candidateId"),
        RequiredString(row, "backgroundId"),
        RequiredString(row, "etaModeId")))
    .ToArray();
var backgroundIds = rows.Select(row => row.BackgroundId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
var particleIds = rows.Select(row => row.ParticleId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
var backgroundModes = backgroundIds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadJson<PromotedFermionModeBundle>(PromotedFermionModePath(backgroundId)).Modes.OrderBy(mode => mode.ModeIndex).ToArray(),
    StringComparer.Ordinal);
var pairKeys = backgroundModes[backgroundIds[0]]
    .SelectMany(
        from => backgroundModes[backgroundIds[0]].Where(to => to.ModeIndex != from.ModeIndex),
        (from, to) => new PairKey(from.ModeIndex, to.ModeIndex))
    .OrderBy(pair => pair.FromModeIndex)
    .ThenBy(pair => pair.ToModeIndex)
    .ToArray();
var analyticSources = rows
    .Select(row => BuildAnalyticSource(row))
    .ToArray();
var assessments = pairKeys
    .Select(pair => AssessPair(pair, analyticSources, backgroundModes, targetRaw, rawGateRatio, stabilitySpreadTolerance, commonScaleSpreadTolerance))
    .OrderByDescending(assessment => assessment.MinParticleRawToTargetRatio)
    .ThenBy(assessment => assessment.RequiredScaleRelativeSpread)
    .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
    .ThenBy(assessment => assessment.Pair.FromModeIndex)
    .ThenBy(assessment => assessment.Pair.ToModeIndex)
    .ToArray();

var wRawGatePassingPairs = assessments.Where(assessment => assessment.ParticleSummaries.Any(summary => summary.ParticleId == "w-boson" && summary.RawGatePassed)).ToArray();
var zRawGatePassingPairs = assessments.Where(assessment => assessment.ParticleSummaries.Any(summary => summary.ParticleId == "z-boson" && summary.RawGatePassed)).ToArray();
var bothRawGatePassingPairs = assessments.Where(assessment => assessment.BothParticleRawGatesPassed).ToArray();
var bothStabilityPassingPairs = assessments.Where(assessment => assessment.BothParticleStabilityPassed).ToArray();
var commonRequiredScalePassingPairs = assessments.Where(assessment => assessment.CommonRequiredScaleGatePassed).ToArray();
var rawAndCommonPassingPairs = assessments.Where(assessment => assessment.BothParticleRawGatesPassed && assessment.CommonRequiredScaleGatePassed).ToArray();
var stableRawCommonPassingPairs = assessments.Where(assessment =>
    assessment.BothParticleRawGatesPassed &&
    assessment.BothParticleStabilityPassed &&
    assessment.CommonRequiredScaleGatePassed).ToArray();
var currentP299Pair = assessments.SingleOrDefault(assessment => assessment.Pair.FromModeIndex == 4 && assessment.Pair.ToModeIndex == 6);
var best = assessments.FirstOrDefault();
bool targetObservablesUsedForSearch = false;
bool theoremClaimed = false;
bool sourceRowsPromotable = false;
bool canFillPhase201WzContract = false;
var identitySplitProductionTransitionSweepPassed =
    p299IdentitySplitPassed
    && p300CommonNormalizationPassed
    && !p300CommonNormalizationCanFillContract
    && !p282NewLocalDirectInvariantSourceFound
    && targetObservablesUsedForSearch == false
    && assessments.Length == pairKeys.Length
    && analyticSources.Length == rows.Length
    && analyticSources.All(source => source.Materialized)
    && bothRawGatePassingPairs.Length == 0
    && rawAndCommonPassingPairs.Length == 0
    && stableRawCommonPassingPairs.Length == 0
    && !theoremClaimed
    && !sourceRowsPromotable
    && !canFillPhase201WzContract
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var terminalStatus = identitySplitProductionTransitionSweepPassed
    ? "identity-split-production-transition-sweep-no-promotable-transition"
    : "identity-split-production-transition-sweep-review-required";

var checks = new[]
{
    new Check(
        "production-analytic-sources-materialized",
        analyticSources.Length == rows.Length && analyticSources.All(source => source.Materialized),
        $"sourceCount={analyticSources.Length}; materializedCount={analyticSources.Count(source => source.Materialized)}"),
    new Check(
        "all-promoted-ordered-transitions-swept",
        assessments.Length == pairKeys.Length && pairKeys.Length > 0,
        $"pairCount={pairKeys.Length}; assessmentCount={assessments.Length}; modeCount={backgroundModes[backgroundIds[0]].Length}"),
    new Check(
        "no-transition-clears-both-wz-raw-gates",
        bothRawGatePassingPairs.Length == 0,
        $"wRawGatePassingPairCount={wRawGatePassingPairs.Length}; zRawGatePassingPairCount={zRawGatePassingPairs.Length}; bothRawGatePassingPairCount={bothRawGatePassingPairs.Length}; bestMinParticleRawToTargetRatio={best?.MinParticleRawToTargetRatio:R}"),
    new Check(
        "no-transition-clears-raw-and-common-gates",
        rawAndCommonPassingPairs.Length == 0 && stableRawCommonPassingPairs.Length == 0,
        $"commonRequiredScalePassingPairCount={commonRequiredScalePassingPairs.Length}; rawAndCommonPassingPairCount={rawAndCommonPassingPairs.Length}; stableRawCommonPassingPairCount={stableRawCommonPassingPairs.Length}"),
    new Check(
        "current-phase299-transition-preserved",
        currentP299Pair is not null && currentP299Pair.Pair.FromModeIndex == 4 && currentP299Pair.Pair.ToModeIndex == 6,
        currentP299Pair is null
            ? "current P299 pair 4->6 not found in sweep"
            : $"currentPairMinParticleRawToTargetRatio={currentP299Pair.MinParticleRawToTargetRatio:R}; currentPairRequiredScaleRelativeSpread={currentP299Pair.RequiredScaleRelativeSpread:R}"),
    new Check(
        "source-contract-remains-blocked",
        !theoremClaimed && !sourceRowsPromotable && !canFillPhase201WzContract && wzMissingFieldCount > 0,
        $"theoremClaimed={theoremClaimed}; sourceRowsPromotable={sourceRowsPromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}"),
};

var result = new
{
    phaseId = "phase301-identity-split-production-transition-sweep",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    identitySplitProductionTransitionSweepPassed,
    targetObservablesUsedForSearch,
    targetValuesUsedOnlyForPostSweepEvaluation = true,
    targetRaw,
    rawGateRatio,
    stabilitySpreadTolerance,
    commonScaleSpreadTolerance,
    backgroundIds,
    particleIds,
    promotedModeCountPerBackground = backgroundModes.ToDictionary(pair => pair.Key, pair => pair.Value.Length, StringComparer.Ordinal),
    sourceCount = analyticSources.Length,
    materializedSourceCount = analyticSources.Count(source => source.Materialized),
    pairCount = pairKeys.Length,
    assessmentCount = assessments.Length,
    wRawGatePassingPairCount = wRawGatePassingPairs.Length,
    zRawGatePassingPairCount = zRawGatePassingPairs.Length,
    bothRawGatePassingPairCount = bothRawGatePassingPairs.Length,
    bothStabilityPassingPairCount = bothStabilityPassingPairs.Length,
    commonRequiredScalePassingPairCount = commonRequiredScalePassingPairs.Length,
    rawAndCommonPassingPairCount = rawAndCommonPassingPairs.Length,
    stableRawCommonPassingPairCount = stableRawCommonPassingPairs.Length,
    theoremClaimed,
    sourceRowsPromotable,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    bestAssessment = best,
    currentPhase299PairAssessment = currentP299Pair,
    topAssessments = assessments.Take(12).ToArray(),
    topCommonScaleAssessments = assessments
        .OrderBy(assessment => assessment.RequiredScaleRelativeSpread)
        .ThenByDescending(assessment => assessment.MinParticleRawToTargetRatio)
        .Take(12)
        .ToArray(),
    analyticSources = analyticSources.Select(source => new
    {
        source.ParticleId,
        source.CandidateId,
        source.BackgroundId,
        source.EtaModeId,
        source.ModePath,
        source.Materialized,
        source.MaterializationStatus,
        source.ClosureRequirements,
    }).ToArray(),
    inheritedBlockers = new
    {
        phase282 = new
        {
            p282NewLocalDirectInvariantSourceFound,
        },
        phase299 = new
        {
            p299IdentitySplitPassed,
            identitySplitRawGatePassed = JsonBool(phase299.RootElement, "identitySplitRawGatePassed"),
            sourceRowsPromotable = JsonBool(phase299.RootElement, "sourceRowsPromotable"),
            canFillPhase201WzContract = JsonBool(phase299.RootElement, "canFillPhase201WzContract"),
        },
        phase300 = new
        {
            p300CommonNormalizationPassed,
            p300CommonNormalizationCanFillContract,
            sourceDeclaredCommonScaleCandidatePassCount = JsonInt(phase300.RootElement, "sourceDeclaredCommonScaleCandidatePassCount"),
        },
    },
    checks,
    decision = identitySplitProductionTransitionSweepPassed
        ? "Do not repair the Phase27 identity split by changing the promoted fermion transition. A production analytic sweep over every promoted ordered transition found no transition that clears both W/Z raw gates, and none clears the raw/common/stability gate stack."
        : "Review the identity-split production transition sweep before relying on transition selection.",
    nextRequiredArtifact = new[]
    {
        "A theorem-backed W/Z bridge source that derives the transition/source-row rule rather than selecting it from target comparison.",
        "A source-derived raw-amplitude law that clears W and Z raw gates for the same particle-specific source construction.",
        "A common W/Z bridge normalization and Phase201/P209 source-lineage rows with stability and derivation sidecars.",
    },
    sourceEvidence = new
    {
        spinorRepresentationPath = SpinorRepresentationPath,
        bosonModeVectorDir = BosonModeVectorDir,
        promotedFermionModeDir = PromotedFermionModeDir,
        phase213Path = Phase213Path,
        phase282Path = Phase282Path,
        phase299Path = Phase299Path,
        phase300Path = Phase300Path,
    },
};

File.WriteAllText(Path.Combine(outputDir, "identity_split_production_transition_sweep.json"), JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(
    Path.Combine(outputDir, "identity_split_production_transition_sweep_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.identitySplitProductionTransitionSweepPassed,
        result.targetObservablesUsedForSearch,
        result.targetValuesUsedOnlyForPostSweepEvaluation,
        result.targetRaw,
        result.rawGateRatio,
        result.stabilitySpreadTolerance,
        result.commonScaleSpreadTolerance,
        result.backgroundIds,
        result.particleIds,
        result.promotedModeCountPerBackground,
        result.sourceCount,
        result.materializedSourceCount,
        result.pairCount,
        result.assessmentCount,
        result.wRawGatePassingPairCount,
        result.zRawGatePassingPairCount,
        result.bothRawGatePassingPairCount,
        result.bothStabilityPassingPairCount,
        result.commonRequiredScalePassingPairCount,
        result.rawAndCommonPassingPairCount,
        result.stableRawCommonPassingPairCount,
        result.theoremClaimed,
        result.sourceRowsPromotable,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.bestAssessment,
        result.currentPhase299PairAssessment,
        result.topAssessments,
        result.topCommonScaleAssessments,
        result.inheritedBlockers,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"identitySplitProductionTransitionSweepPassed={identitySplitProductionTransitionSweepPassed}");
Console.WriteLine($"pairCount={pairKeys.Length}");
Console.WriteLine($"assessmentCount={assessments.Length}");
Console.WriteLine($"bothRawGatePassingPairCount={bothRawGatePassingPairs.Length}");
Console.WriteLine($"commonRequiredScalePassingPairCount={commonRequiredScalePassingPairs.Length}");
Console.WriteLine($"rawAndCommonPassingPairCount={rawAndCommonPassingPairs.Length}");
Console.WriteLine($"stableRawCommonPassingPairCount={stableRawCommonPassingPairs.Length}");
Console.WriteLine($"bestPair={best?.Pair.FromModeIndex}->{best?.Pair.ToModeIndex}");
Console.WriteLine($"bestMinParticleRawToTargetRatio={best?.MinParticleRawToTargetRatio:R}");
Console.WriteLine($"bestRequiredScaleRelativeSpread={best?.RequiredScaleRelativeSpread:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

AnalyticSource BuildAnalyticSource(IdentitySourceRow row)
{
    var modePath = Path.Combine(BosonModeVectorDir, row.EtaModeId + ".json");
    var materialization = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
        modePath,
        File.ReadAllText(modePath),
        mesh.EdgeCount * dimG);
    var closure = new List<string>(materialization.ClosureRequirements);
    double[,]? variationRe = null;
    double[,]? variationIm = null;
    if (closure.Count == 0)
    {
        try
        {
            (variationRe, variationIm) = DiracVariationComputer.ComputeAnalytical(
                materialization.PerturbationVector.ToArray(),
                gammas,
                mesh.VertexCount,
                spinorDim,
                dimG,
                geometry.EdgeLengths,
                geometry.CellsPerEdge,
                geometry.EdgeDirections);
        }
        catch (Exception ex)
        {
            closure.Add($"analytic variation matrix computation failed: {ex.Message}");
        }
    }

    return new AnalyticSource(
        row.ParticleId,
        row.CandidateId,
        row.BackgroundId,
        row.EtaModeId,
        modePath,
        materialization.TerminalStatus,
        closure.Count == 0 && variationRe is not null && variationIm is not null,
        closure,
        variationRe,
        variationIm);
}

TransitionAssessment AssessPair(
    PairKey pair,
    IReadOnlyList<AnalyticSource> sources,
    IReadOnlyDictionary<string, FermionModeRecord[]> modesByBackground,
    double targetRaw,
    double rawGateRatio,
    double stabilitySpreadTolerance,
    double commonScaleSpreadTolerance)
{
    var records = sources.Select(source =>
    {
        if (!source.Materialized || source.VariationRe is null || source.VariationIm is null)
        {
            return new TransitionRecord(
                source.ParticleId,
                source.CandidateId,
                source.BackgroundId,
                source.EtaModeId,
                "",
                "",
                double.NaN,
                double.NaN,
                double.NaN,
                false);
        }

        var modes = modesByBackground[source.BackgroundId];
        var modeI = modes.Single(mode => mode.ModeIndex == pair.FromModeIndex);
        var modeJ = modes.Single(mode => mode.ModeIndex == pair.ToModeIndex);
        var coupling = new CouplingProxyEngine(new Gu.Phase4.Dirac.CpuDiracOperatorAssembler()).ComputeCoupling(
            modeI,
            modeJ,
            source.EtaModeId,
            source.VariationRe,
            source.VariationIm,
            RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
            provenance,
            RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
            $"analytic-variation:{source.EtaModeId}:{source.ModePath}",
            ["phase301 production analytic transition sweep"]);
        var raw = coupling.CouplingProxyMagnitude;
        var ratio = raw / targetRaw;
        return new TransitionRecord(
            source.ParticleId,
            source.CandidateId,
            source.BackgroundId,
            source.EtaModeId,
            modeI.ModeId,
            modeJ.ModeId,
            raw,
            ratio,
            RequiredScale: raw > 0.0 ? targetRaw / raw : double.PositiveInfinity,
            RawGatePassed: ratio >= rawGateRatio);
    }).ToArray();

    var particleSummaries = records
        .GroupBy(record => record.ParticleId, StringComparer.Ordinal)
        .OrderBy(group => group.Key, StringComparer.Ordinal)
        .Select(group =>
        {
            var groupRows = group.ToArray();
            var raws = groupRows.Select(row => row.RawMagnitude).Where(double.IsFinite).ToArray();
            var meanRaw = raws.Length == 0 ? double.NaN : raws.Average();
            var rawToTargetRatio = meanRaw / targetRaw;
            var relativeSpread = RelativeSpread(raws);
            var requiredScale = meanRaw > 0.0 ? targetRaw / meanRaw : double.PositiveInfinity;
            return new ParticleTransitionSummary(
                group.Key,
                groupRows.First().CandidateId,
                groupRows.Length,
                meanRaw,
                rawToTargetRatio,
                rawToTargetRatio >= rawGateRatio,
                relativeSpread,
                relativeSpread <= stabilitySpreadTolerance,
                requiredScale);
        })
        .ToArray();

    var requiredScales = particleSummaries.Select(summary => summary.RequiredScaleToTargetRaw).Where(double.IsFinite).ToArray();
    var requiredScaleRelativeSpread = RelativeSpread(requiredScales);
    var minParticleRatio = particleSummaries.Min(summary => summary.RawToTargetRatio);
    var maxParticleSpread = particleSummaries.Max(summary => summary.RelativeSpread);
    var bothRaw = particleSummaries.Length == 2 && particleSummaries.All(summary => summary.RawGatePassed);
    var bothStable = particleSummaries.Length == 2 && particleSummaries.All(summary => summary.StabilityPassed);
    var commonScalePassed = requiredScaleRelativeSpread <= commonScaleSpreadTolerance;
    return new TransitionAssessment(
        pair,
        records,
        particleSummaries,
        minParticleRatio,
        maxParticleSpread,
        requiredScaleRelativeSpread,
        bothRaw,
        bothStable,
        commonScalePassed);
}

static GeometryReplayInputs BuildGeometry(SimplicialMesh mesh)
{
    var edgeLengths = new double[mesh.EdgeCount];
    var edgeDirections = new double[mesh.EdgeCount][];
    var cellsPerEdge = new int[mesh.EdgeCount][];
    for (int edge = 0; edge < mesh.EdgeCount; edge++)
    {
        edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
        edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
        cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
    }

    return new GeometryReplayInputs(edgeLengths, cellsPerEdge, edgeDirections);
}

static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    double norm = 0.0;
    for (int k = 0; k < coords0.Length; k++)
    {
        double d = coords1[k] - coords0[k];
        norm += d * d;
    }

    return Math.Sqrt(norm);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    int dim = mesh.EmbeddingDimension;
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    var dir = new double[dim];
    double norm = 0.0;
    for (int k = 0; k < dim; k++)
    {
        dir[k] = coords1[k] - coords0[k];
        norm += dir[k] * dir[k];
    }

    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
    {
        for (int k = 0; k < dim; k++)
        {
            dir[k] /= norm;
        }
    }

    return dir;
}

T LoadJson<T>(string path) =>
    JsonSerializer.Deserialize<T>(File.ReadAllText(path), jsonOptions)
    ?? throw new InvalidDataException($"Failed to deserialize {path}");

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
};

static string PromotedFermionModePath(string backgroundId) =>
    Path.Combine(PromotedFermionModeDir, backgroundId, "branch_stability_promoted_fermion_modes.json");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? property.GetDouble() : null;

static double RelativeSpread(IReadOnlyCollection<double> values)
{
    if (values.Count == 0)
    {
        return double.NaN;
    }

    var mean = values.Average();
    return (values.Max() - values.Min()) / Math.Max(Math.Abs(mean), 1e-300);
}

sealed class PromotedFermionModeBundle
{
    [JsonPropertyName("modes")]
    public required List<FermionModeRecord> Modes { get; init; }
}

sealed record GeometryReplayInputs(double[] EdgeLengths, int[][] CellsPerEdge, double[][] EdgeDirections);
sealed record IdentitySourceRow(string ParticleId, string CandidateId, string BackgroundId, string EtaModeId);
sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record AnalyticSource(
    string ParticleId,
    string CandidateId,
    string BackgroundId,
    string EtaModeId,
    string ModePath,
    string MaterializationStatus,
    bool Materialized,
    IReadOnlyList<string> ClosureRequirements,
    double[,]? VariationRe,
    double[,]? VariationIm);

sealed record TransitionRecord(
    string ParticleId,
    string CandidateId,
    string BackgroundId,
    string EtaModeId,
    string FromFermionModeId,
    string ToFermionModeId,
    double RawMagnitude,
    double RawToTargetRatio,
    double RequiredScale,
    bool RawGatePassed);

sealed record ParticleTransitionSummary(
    string ParticleId,
    string CandidateId,
    int RowCount,
    double MeanRawMagnitude,
    double RawToTargetRatio,
    bool RawGatePassed,
    double RelativeSpread,
    bool StabilityPassed,
    double RequiredScaleToTargetRaw);

sealed record TransitionAssessment(
    PairKey Pair,
    IReadOnlyList<TransitionRecord> Records,
    IReadOnlyList<ParticleTransitionSummary> ParticleSummaries,
    double MinParticleRawToTargetRatio,
    double MaxParticleRelativeSpread,
    double RequiredScaleRelativeSpread,
    bool BothParticleRawGatesPassed,
    bool BothParticleStabilityPassed,
    bool CommonRequiredScaleGatePassed);

sealed record Check(string CheckId, bool Passed, string Detail);
