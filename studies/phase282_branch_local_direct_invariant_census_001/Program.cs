using System.Text.Json;

const string DefaultOutputDir = "studies/phase282_branch_local_direct_invariant_census_001/output";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string VariationMatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";
const string Phase91Dir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census_summary.json";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase280Path = "studies/phase280_direct_bridge_analytic_variation_upgrade_audit_001/output/direct_bridge_analytic_variation_upgrade_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE282_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var phase172 = JsonDocument.Parse(File.ReadAllText(Phase172Path));
using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase280 = JsonDocument.Parse(File.ReadAllText(Phase280Path));

var backgrounds = new[] { "bg-phase12-bg-a-20260315212202", "bg-phase12-bg-b-20260315212202" };
double stabilitySpreadTolerance = JsonDouble(phase172.RootElement, "stabilitySpreadTolerance") ?? 0.05;
double posthocTargetRaw = JsonDouble(phase172.RootElement, "targetRaw") ?? double.NaN;
double posthocRawGateRatio = JsonDouble(phase172.RootElement, "rawGateRatio") ?? 0.95;
var p190StableCandidateCount = JsonInt(
    phase190.RootElement.GetProperty("siblingStability"),
    "stableCandidateCount") ?? -1;
var p172RawGatePassingAssessmentCount = JsonInt(phase172.RootElement, "rawGatePassingAssessmentCount") ?? -1;
var p172StableRawGatePassingAssessmentCount = JsonInt(phase172.RootElement, "stableRawGatePassingAssessmentCount") ?? -1;
var p280BranchLocalFiniteVariationReplayed = JsonBool(phase280.RootElement, "branchLocalFiniteVariationReplayed") is true;
var p280CanRepairDirectBridgeWithAnalyticVariation = JsonBool(phase280.RootElement, "canRepairDirectBridgeWithAnalyticVariation") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;

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

var backgroundModes = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadModes(Path.Combine(Phase91Dir, backgroundId, "branch_stability_promoted_fermion_modes.json")),
    StringComparer.Ordinal);

var matrices = LoadMatrices(backgrounds, candidates);
var pairKeys = backgroundModes[backgrounds[0]]
    .SelectMany(
        from => backgroundModes[backgrounds[0]].Where(to => to.ModeIndex != from.ModeIndex),
        (from, to) => new PairKey(from.ModeIndex, to.ModeIndex))
    .OrderBy(pair => pair.FromModeIndex)
    .ThenBy(pair => pair.ToModeIndex)
    .ToArray();

var subspaceAssessments = new List<SubspaceAssessment>();
var singleCandidateAssessments = new List<SingleCandidateAssessment>();

foreach (var pair in pairKeys)
{
    var perBackgroundComponents = new Dictionary<string, CandidateComponent[]>(StringComparer.Ordinal);
    foreach (var backgroundId in backgrounds)
    {
        var modes = backgroundModes[backgroundId];
        var from = modes.Single(mode => mode.ModeIndex == pair.FromModeIndex);
        var to = modes.Single(mode => mode.ModeIndex == pair.ToModeIndex);
        perBackgroundComponents[backgroundId] = candidates
            .Select(candidate =>
            {
                var matrix = matrices[(backgroundId, candidate.CandidateId)];
                var value = MatrixElement(matrix.Real, matrix.Imaginary, from.Coefficients, to.Coefficients);
                return new CandidateComponent(candidate.CandidateId, value.Real, value.Imaginary, Magnitude(value.Real, value.Imaginary));
            })
            .ToArray();
    }

    var subspaceRecords = backgrounds
        .Select(backgroundId =>
        {
            var components = perBackgroundComponents[backgroundId];
            double subspaceNorm = Math.Sqrt(components.Sum(component => component.Magnitude * component.Magnitude));
            return new SubspaceBackgroundRecord(
                backgroundId,
                subspaceNorm,
                RatioOrZero(subspaceNorm, posthocTargetRaw),
                components.OrderByDescending(component => component.Magnitude).Take(6).ToArray());
        })
        .ToArray();

    var subspaceSpread = RelativeSpread(subspaceRecords.Select(record => record.SubspaceNorm));
    var subspaceMinRawToTargetRatio = subspaceRecords.Min(record => record.PosthocRawToTargetRatio);
    var subspaceStabilityPassed = subspaceSpread <= stabilitySpreadTolerance;
    var subspacePosthocRawGatePassed = subspaceMinRawToTargetRatio >= posthocRawGateRatio;
    subspaceAssessments.Add(new SubspaceAssessment(
        pair,
        subspaceRecords,
        subspaceRecords.Min(record => record.SubspaceNorm),
        subspaceRecords.Max(record => record.SubspaceNorm),
        subspaceSpread,
        subspaceStabilityPassed,
        subspaceMinRawToTargetRatio,
        subspacePosthocRawGatePassed));

    foreach (var candidate in candidates)
    {
        var candidateRecords = backgrounds
            .Select(backgroundId =>
            {
                var component = perBackgroundComponents[backgroundId].Single(item => item.CandidateId == candidate.CandidateId);
                var subspaceNorm = subspaceRecords.Single(record => record.BackgroundId == backgroundId).SubspaceNorm;
                return new SingleCandidateBackgroundRecord(
                    backgroundId,
                    BranchLocalModeId(candidate, backgroundId),
                    component.Real,
                    component.Imaginary,
                    component.Magnitude,
                    RatioOrZero(component.Magnitude, subspaceNorm),
                    RatioOrZero(component.Magnitude, posthocTargetRaw));
            })
            .ToArray();

        var magnitudeSpread = RelativeSpread(candidateRecords.Select(record => record.Magnitude));
        var shareSpread = RelativeSpread(candidateRecords.Select(record => record.SubspaceShare));
        var minRawToTargetRatio = candidateRecords.Min(record => record.PosthocRawToTargetRatio);
        singleCandidateAssessments.Add(new SingleCandidateAssessment(
            pair,
            candidate.CandidateId,
            candidateRecords,
            candidateRecords.Min(record => record.Magnitude),
            candidateRecords.Max(record => record.Magnitude),
            magnitudeSpread,
            magnitudeSpread <= stabilitySpreadTolerance,
            candidateRecords.Min(record => record.SubspaceShare),
            candidateRecords.Max(record => record.SubspaceShare),
            shareSpread,
            shareSpread <= stabilitySpreadTolerance,
            minRawToTargetRatio,
            minRawToTargetRatio >= posthocRawGateRatio));
    }
}

var stableSingleCandidateMagnitudes = singleCandidateAssessments
    .Where(assessment => assessment.MagnitudeStabilityPassed)
    .ToArray();
var stableSingleCandidateShares = singleCandidateAssessments
    .Where(assessment => assessment.SubspaceShareStabilityPassed)
    .ToArray();
var posthocRawGatePassingSingleCandidates = singleCandidateAssessments
    .Where(assessment => assessment.PosthocRawGatePassed)
    .ToArray();
var stablePosthocRawGatePassingSingleCandidates = singleCandidateAssessments
    .Where(assessment => assessment.MagnitudeStabilityPassed && assessment.PosthocRawGatePassed)
    .ToArray();
var stableSubspaces = subspaceAssessments
    .Where(assessment => assessment.StabilityPassed)
    .ToArray();
var posthocRawGatePassingSubspaces = subspaceAssessments
    .Where(assessment => assessment.PosthocRawGatePassed)
    .ToArray();
var stablePosthocRawGatePassingSubspaces = subspaceAssessments
    .Where(assessment => assessment.StabilityPassed && assessment.PosthocRawGatePassed)
    .ToArray();

var targetObservablesUsedForSearch = false;
var theoremClaimed = false;
var wZParticleSplitPresent = false;
var directInvariantPromotesWzMasses = false;
var newLocalDirectInvariantSourceFound =
    stablePosthocRawGatePassingSingleCandidates.Length > 0
    || stablePosthocRawGatePassingSubspaces.Length > 0;

var branchLocalInvariantCensusPassed =
    targetObservablesUsedForSearch == false
    && p280BranchLocalFiniteVariationReplayed
    && !p280CanRepairDirectBridgeWithAnalyticVariation
    && p190StableCandidateCount == 0
    && p172RawGatePassingAssessmentCount == 0
    && p172StableRawGatePassingAssessmentCount == 0
    && posthocRawGatePassingSingleCandidates.Length == 0
    && posthocRawGatePassingSubspaces.Length == 0
    && !newLocalDirectInvariantSourceFound
    && !theoremClaimed
    && !wZParticleSplitPresent
    && !directInvariantPromotesWzMasses
    && !unlockContractFilled
    && newSourceEvidenceStillRequired
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var terminalStatus = branchLocalInvariantCensusPassed
    ? "branch-local-direct-invariant-census-no-promotable-local-source"
    : "branch-local-direct-invariant-census-review-required";

var result = new
{
    phaseId = "phase282-branch-local-direct-invariant-census",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    branchLocalInvariantCensusPassed,
    targetObservablesUsedForSearch,
    theoremClaimed,
    wZParticleSplitPresent,
    directInvariantPromotesWzMasses,
    newLocalDirectInvariantSourceFound,
    searchScope = new
    {
        backgroundIds = backgrounds,
        pairCount = pairKeys.Length,
        candidateCount = candidates.Length,
        singleCandidateAssessmentCount = singleCandidateAssessments.Count,
        subspaceAssessmentCount = subspaceAssessments.Count,
        invariantFamilies = new[]
        {
            "single-candidate branch-local finite-difference matrix-element magnitude",
            "single-candidate contribution share within the full branch-local variation subspace",
            "full branch-local variation-subspace root-sum-square norm",
        },
        stabilitySpreadTolerance,
        posthocTargetRaw,
        posthocRawGateRatio,
        posthocTargetRawUsedForSearch = false,
    },
    census = new
    {
        stableSingleCandidateMagnitudeCount = stableSingleCandidateMagnitudes.Length,
        stableSingleCandidateShareCount = stableSingleCandidateShares.Length,
        stableSubspacePairCount = stableSubspaces.Length,
        posthocRawGatePassingSingleCandidateCount = posthocRawGatePassingSingleCandidates.Length,
        stablePosthocRawGatePassingSingleCandidateCount = stablePosthocRawGatePassingSingleCandidates.Length,
        posthocRawGatePassingSubspaceCount = posthocRawGatePassingSubspaces.Length,
        stablePosthocRawGatePassingSubspaceCount = stablePosthocRawGatePassingSubspaces.Length,
        bestSingleCandidateByMagnitudeStability = singleCandidateAssessments
            .OrderBy(assessment => assessment.MagnitudeRelativeSpread)
            .ThenByDescending(assessment => assessment.MinMagnitude)
            .FirstOrDefault(),
        largestStableSingleCandidateMagnitude = stableSingleCandidateMagnitudes
            .OrderByDescending(assessment => assessment.MinMagnitude)
            .ThenBy(assessment => assessment.MagnitudeRelativeSpread)
            .FirstOrDefault(),
        bestSubspaceByStability = subspaceAssessments
            .OrderBy(assessment => assessment.RelativeSpread)
            .ThenByDescending(assessment => assessment.MinSubspaceNorm)
            .FirstOrDefault(),
        largestStableSubspace = stableSubspaces
            .OrderByDescending(assessment => assessment.MinSubspaceNorm)
            .ThenBy(assessment => assessment.RelativeSpread)
            .FirstOrDefault(),
    },
    upstreamEvidence = new
    {
        phase190 = new
        {
            status = JsonString(phase190.RootElement, "terminalStatus"),
            p190StableCandidateCount,
            p190BestRelativeSpread = phase190.RootElement.TryGetProperty("siblingStability", out var p190Stability)
                && p190Stability.TryGetProperty("bestCandidate", out var p190BestCandidate)
                ? JsonDouble(p190BestCandidate, "relativeSpread")
                : null,
        },
        phase172 = new
        {
            status = JsonString(phase172.RootElement, "terminalStatus"),
            p172RawGatePassingAssessmentCount,
            p172StableRawGatePassingAssessmentCount,
        },
        phase280 = new
        {
            status = JsonString(phase280.RootElement, "terminalStatus"),
            p280BranchLocalFiniteVariationReplayed,
            p280CanRepairDirectBridgeWithAnalyticVariation,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    checks = new[]
    {
        new Check("target-independent-search", !targetObservablesUsedForSearch, "Search ordering and stability decisions use branch-local matrix invariants only; targetRaw is reported only for post-construction comparison."),
        new Check("branch-local-replay-preserved", p280BranchLocalFiniteVariationReplayed, $"p280BranchLocalFiniteVariationReplayed={p280BranchLocalFiniteVariationReplayed}"),
        new Check("p190-direct-candidate-still-not-stable", p190StableCandidateCount == 0, $"p190StableCandidateCount={p190StableCandidateCount}"),
        new Check("phase172-raw-gate-still-empty", p172RawGatePassingAssessmentCount == 0 && p172StableRawGatePassingAssessmentCount == 0, $"p172RawGatePassingAssessmentCount={p172RawGatePassingAssessmentCount}; p172StableRawGatePassingAssessmentCount={p172StableRawGatePassingAssessmentCount}"),
        new Check("no-posthoc-raw-gate-local-invariant", posthocRawGatePassingSingleCandidates.Length == 0 && posthocRawGatePassingSubspaces.Length == 0, $"posthocRawGatePassingSingleCandidateCount={posthocRawGatePassingSingleCandidates.Length}; posthocRawGatePassingSubspaceCount={posthocRawGatePassingSubspaces.Length}"),
        new Check("no-direct-invariant-promotion", !newLocalDirectInvariantSourceFound && !directInvariantPromotesWzMasses && !theoremClaimed && !wZParticleSplitPresent, $"newLocalDirectInvariantSourceFound={newLocalDirectInvariantSourceFound}; directInvariantPromotesWzMasses={directInvariantPromotesWzMasses}; theoremClaimed={theoremClaimed}; wZParticleSplitPresent={wZParticleSplitPresent}"),
        new Check("source-lineage-blockers-preserved", !unlockContractFilled && newSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    },
    decision = branchLocalInvariantCensusPassed
        ? "Do not promote a local direct W/Z source from additional branch-local invariants. The repaired matrices can be searched target-independently, but no single-candidate or full-subspace invariant clears the post-construction raw-scale gate, and theorem/source-row blockers remain open."
        : "Review the branch-local direct invariant census before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed direct W/Z theorem or source-row construction outside the current branch-local finite-difference invariant family.",
        "Separate W and Z source rows with raw-amplitude, common-bridge, target-comparison, derivation, and stability gates.",
        "The existing Higgs scalar-source lineage remains separately required.",
    },
    sourceEvidence = new
    {
        bosonRegistryPath = BosonRegistryPath,
        variationMatrixDir = VariationMatrixDir,
        phase91Dir = Phase91Dir,
        phase172Path = Phase172Path,
        phase190Path = Phase190Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase280Path = Phase280Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "branch_local_direct_invariant_census.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "branch_local_direct_invariant_census_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.branchLocalInvariantCensusPassed,
        result.targetObservablesUsedForSearch,
        result.theoremClaimed,
        result.wZParticleSplitPresent,
        result.directInvariantPromotesWzMasses,
        result.newLocalDirectInvariantSourceFound,
        result.searchScope,
        result.census,
        result.upstreamEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"branchLocalInvariantCensusPassed={branchLocalInvariantCensusPassed}");
Console.WriteLine($"targetObservablesUsedForSearch={targetObservablesUsedForSearch}");
Console.WriteLine($"singleCandidateAssessmentCount={singleCandidateAssessments.Count}");
Console.WriteLine($"stableSingleCandidateMagnitudeCount={stableSingleCandidateMagnitudes.Length}");
Console.WriteLine($"stableSubspacePairCount={stableSubspaces.Length}");
Console.WriteLine($"posthocRawGatePassingSingleCandidateCount={posthocRawGatePassingSingleCandidates.Length}");
Console.WriteLine($"posthocRawGatePassingSubspaceCount={posthocRawGatePassingSubspaces.Length}");
Console.WriteLine($"newLocalDirectInvariantSourceFound={newLocalDirectInvariantSourceFound}");

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
            JsonInt(mode, "modeIndex") ?? throw new InvalidDataException("Missing modeIndex"),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray()))
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
}

static string BranchLocalModeId(CandidateRecord candidate, string backgroundId) =>
    candidate.ContributingModeIds.Single(mode => mode.StartsWith(backgroundId + "-mode-", StringComparison.Ordinal));

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
    var array = values.ToArray();
    double mean = array.Average();
    return (array.Max() - array.Min()) / Math.Max(Math.Abs(mean), 1e-300);
}

static double RatioOrZero(double numerator, double denominator) =>
    double.IsFinite(denominator) && Math.Abs(denominator) > 1e-300 ? numerator / denominator : 0.0;

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);

static int CandidateOrdinal(string candidateId)
{
    const string prefix = "candidate-";
    return candidateId.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(candidateId[prefix.Length..], out int value)
        ? value
        : int.MaxValue;
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record CandidateRecord(string CandidateId, IReadOnlyList<string> ContributingModeIds);
sealed record ModeRecord(string ModeId, int ModeIndex, double[] Coefficients);
sealed record MatrixRecord(string BackgroundId, string CandidateId, double[,] Real, double[,] Imaginary);
sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record CandidateComponent(string CandidateId, double Real, double Imaginary, double Magnitude);
sealed record SubspaceBackgroundRecord(string BackgroundId, double SubspaceNorm, double PosthocRawToTargetRatio, IReadOnlyList<CandidateComponent> DominantComponents);
sealed record SubspaceAssessment(
    PairKey Pair,
    IReadOnlyList<SubspaceBackgroundRecord> BackgroundRecords,
    double MinSubspaceNorm,
    double MaxSubspaceNorm,
    double RelativeSpread,
    bool StabilityPassed,
    double MinPosthocRawToTargetRatio,
    bool PosthocRawGatePassed);
sealed record SingleCandidateBackgroundRecord(
    string BackgroundId,
    string BranchLocalModeId,
    double Real,
    double Imaginary,
    double Magnitude,
    double SubspaceShare,
    double PosthocRawToTargetRatio);
sealed record SingleCandidateAssessment(
    PairKey Pair,
    string CandidateId,
    IReadOnlyList<SingleCandidateBackgroundRecord> BackgroundRecords,
    double MinMagnitude,
    double MaxMagnitude,
    double MagnitudeRelativeSpread,
    bool MagnitudeStabilityPassed,
    double MinSubspaceShare,
    double MaxSubspaceShare,
    double SubspaceShareRelativeSpread,
    bool SubspaceShareStabilityPassed,
    double MinPosthocRawToTargetRatio,
    bool PosthocRawGatePassed);
sealed record Check(string CheckId, bool Passed, string Detail);
