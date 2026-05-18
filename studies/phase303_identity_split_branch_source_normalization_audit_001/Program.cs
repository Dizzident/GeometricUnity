using System.Text.Json;

const string DefaultOutputDir = "studies/phase303_identity_split_branch_source_normalization_audit_001/output";
const string Phase24InitialPath = "studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json";
const string Phase27IdentityPath = "studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json";
const string Phase27MixingPath = "studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json";
const string Phase251Path = "studies/phase251_upstream_wz_identity_rule_source_chain_audit_001/output/upstream_wz_identity_rule_source_chain_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase299Path = "studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE303_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase24Initial = JsonDocument.Parse(File.ReadAllText(Phase24InitialPath));
using var phase27Identity = JsonDocument.Parse(File.ReadAllText(Phase27IdentityPath));
using var phase27Mixing = JsonDocument.Parse(File.ReadAllText(Phase27MixingPath));
using var phase251 = JsonDocument.Parse(File.ReadAllText(Phase251Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase299 = JsonDocument.Parse(File.ReadAllText(Phase299Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));

double rawGateRatio = RequiredDouble(phase299.RootElement, "rawGateRatio");
double stabilitySpreadTolerance = RequiredDouble(phase299.RootElement, "stabilitySpreadTolerance");
double commonScaleSpreadTolerance = 0.05;
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
bool p299Passed = JsonBool(phase299.RootElement, "identitySplitProductionWzReplayAttemptPassed") is true;
bool p302Passed = JsonBool(phase302.RootElement, "identitySplitParticleNormalizationAuditPassed") is true;
bool p302CanFillContract = JsonBool(phase302.RootElement, "canFillPhase201WzContract") is true;
var p302Best = phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate");
string p302BestCandidateId = RequiredString(p302Best, "candidateId");
double p302BestWTotalScale = RequiredDouble(p302Best, "wTotalScale");
double p302BestZTotalScale = RequiredDouble(p302Best, "zTotalScale");
bool p302BestRawAndCommonPassed = JsonBool(p302Best, "rawAndCommonGatesPassed") is true;
bool p302BestStableRawCommonPassed = JsonBool(p302Best, "stableRawCommonGatesPassed") is true;
bool phase27IdentityRuleReady = string.Equals(JsonString(phase27Identity.RootElement, "terminalStatus"), "identity-rule-ready", StringComparison.Ordinal);
bool phase27MixingConventionReady = string.Equals(JsonString(phase27Mixing.RootElement, "terminalStatus"), "mixing-convention-ready", StringComparison.Ordinal);
bool phase251UpstreamIdentityReady = JsonBool(phase251.RootElement, "phase27InternalIdentityRuleReady") is true;
bool phase251UpstreamIdentityNotAbsoluteSource = JsonBool(phase251.RootElement, "upstreamIdentityRulePhysicalMassClaimPromotable") is false
    && JsonBool(phase251.RootElement, "upstreamFillsWzAbsoluteSourceContract") is false
    && JsonBool(phase251.RootElement, "newSourceEvidenceStillRequired") is true;
bool identitySidecarFillsWzAbsoluteSourceContract = JsonBool(phase251.RootElement, "upstreamFillsWzAbsoluteSourceContract") is true;

var rows = phase299.RootElement.GetProperty("rows")
    .EnumerateArray()
    .Select(row =>
    {
        string particleId = RequiredString(row, "particleId");
        string modePath = ExtractModePath(RequiredString(row, "variationEvidenceId"));
        var mode = LoadModeDescriptor(modePath);
        return new SourceRow(
            particleId,
            RequiredString(row, "candidateId"),
            RequiredString(row, "backgroundId"),
            RequiredString(row, "etaModeId"),
            modePath,
            RequiredDouble(row, "rawMatrixElementMagnitude"),
            RequiredDouble(row, "rawToTargetRatio"),
            particleId == "w-boson" ? p302BestWTotalScale : p302BestZTotalScale,
            mode);
    })
    .ToArray();

var descriptorDefinitions = new[]
{
    new DescriptorDefinition("mode-l1-norm", "source mode vector L1 norm", row => row.Mode.L1Norm),
    new DescriptorDefinition("mode-linf-norm", "source mode vector L-infinity norm", row => row.Mode.LInfinityNorm),
    new DescriptorDefinition("triple-l1-norm", "sum of SU(2)-triple L2 norms over ambient edges", row => row.Mode.TripleL1Norm),
    new DescriptorDefinition("triple-linf-norm", "maximum SU(2)-triple L2 norm over ambient edges", row => row.Mode.TripleLInfinityNorm),
    new DescriptorDefinition("dominant-axis-l2", "L2 norm of the dominant SU(2) axis in the source mode", row => row.Mode.DominantAxisL2),
    new DescriptorDefinition("dominant-axis-energy", "energy of the dominant SU(2) axis in the source mode", row => row.Mode.DominantAxisEnergy),
    new DescriptorDefinition("phase27-charged-plane-l2", "L2 norm of the Phase27 charged SU(2) plane, axes 0 and 1", row => Math.Sqrt(AxisEnergy(row, 0) + AxisEnergy(row, 1))),
    new DescriptorDefinition("phase27-charged-plane-energy", "energy of the Phase27 charged SU(2) plane, axes 0 and 1", row => AxisEnergy(row, 0) + AxisEnergy(row, 1)),
    new DescriptorDefinition("phase27-neutral-axis-l2", "L2 norm of the Phase27 neutral SU(2) axis, axis 2", row => row.Mode.Axes.Single(axis => axis.AxisIndex == 2).L2Norm),
    new DescriptorDefinition("phase27-neutral-axis-energy", "energy of the Phase27 neutral SU(2) axis, axis 2", row => AxisEnergy(row, 2)),
    new DescriptorDefinition("phase27-sector-projection-l2", "L2 norm of the Phase27 particle sector projection: W uses charged plane, Z uses neutral axis", row => row.ParticleId == "w-boson" ? Math.Sqrt(AxisEnergy(row, 0) + AxisEnergy(row, 1)) : row.Mode.Axes.Single(axis => axis.AxisIndex == 2).L2Norm),
    new DescriptorDefinition("phase27-sector-projection-energy", "energy of the Phase27 particle sector projection: W uses charged plane, Z uses neutral axis", row => row.ParticleId == "w-boson" ? AxisEnergy(row, 0) + AxisEnergy(row, 1) : AxisEnergy(row, 2)),
    new DescriptorDefinition("residual-norm", "mode solver residual norm", row => row.Mode.ResidualNorm),
};

var candidateAssessments = new[] { EvaluateCandidate("phase302-best-no-branch-normalization", "identity", "none", row => 1.0) }
    .Concat(descriptorDefinitions.SelectMany(descriptor => new[]
    {
        EvaluateDescriptorCandidate(descriptor, "value-over-particle-mean"),
        EvaluateDescriptorCandidate(descriptor, "particle-mean-over-value"),
    }))
    .OrderBy(candidate => candidate.MaxParticleRelativeSpread)
    .ThenByDescending(candidate => candidate.MinRowScaledRawToTargetRatio)
    .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();

var phase302BestCandidate = candidateAssessments.Single(candidate => candidate.CandidateId == "phase302-best-no-branch-normalization");
var allRowsRawPassingCandidates = candidateAssessments.Where(candidate => candidate.AllRowsRawGatePassed).ToArray();
var stableCandidates = candidateAssessments.Where(candidate => candidate.AllParticlesStabilityPassed).ToArray();
var stableRawCommonAllRowsCandidates = candidateAssessments.Where(candidate => candidate.StableRawCommonAllRowsPassed).ToArray();
var bestDescriptorCandidate = candidateAssessments.Where(candidate => candidate.CandidateId != "phase302-best-no-branch-normalization").FirstOrDefault();

bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool theoremClaimed = false;
bool sourceRowsPromotable = false;
bool canFillPhase201WzContract = false;

var checks = new[]
{
    new Check(
        "phase302-near-pass-available",
        p299Passed && p302Passed && p302BestRawAndCommonPassed && !p302BestStableRawCommonPassed,
        $"p299Passed={p299Passed}; p302Passed={p302Passed}; p302BestCandidateId={p302BestCandidateId}; p302BestRawAndCommonPassed={p302BestRawAndCommonPassed}; p302BestStableRawCommonPassed={p302BestStableRawCommonPassed}"),
    new Check(
        "branch-normalization-search-target-independent",
        !targetObservablesUsedForConstruction && targetValuesUsedOnlyForPostCandidateEvaluation,
        $"targetObservablesUsedForConstruction={targetObservablesUsedForConstruction}; targetValuesUsedOnlyForPostCandidateEvaluation={targetValuesUsedOnlyForPostCandidateEvaluation}"),
    new Check(
        "phase302-best-fails-row-sidecar-raw-and-stability",
        !phase302BestCandidate.AllRowsRawGatePassed && !phase302BestCandidate.AllParticlesStabilityPassed,
        $"minRowScaledRawToTargetRatio={phase302BestCandidate.MinRowScaledRawToTargetRatio:R}; maxParticleRelativeSpread={phase302BestCandidate.MaxParticleRelativeSpread:R}; rawGateRatio={rawGateRatio:R}; stabilitySpreadTolerance={stabilitySpreadTolerance:R}"),
    new Check(
        "source-descriptor-branch-normalizers-do-not-stabilize",
        candidateAssessments.Length == 1 + descriptorDefinitions.Length * 2 && stableRawCommonAllRowsCandidates.Length == 0,
        $"candidateAssessmentCount={candidateAssessments.Length}; stableRawCommonAllRowsCandidateCount={stableRawCommonAllRowsCandidates.Length}; bestDescriptorCandidate={bestDescriptorCandidate?.CandidateId}; bestDescriptorMaxParticleRelativeSpread={bestDescriptorCandidate?.MaxParticleRelativeSpread:R}"),
    new Check(
        "phase27-identity-sidecar-current-but-not-source-law",
        phase27IdentityRuleReady
        && phase27MixingConventionReady
        && phase251UpstreamIdentityReady
        && phase251UpstreamIdentityNotAbsoluteSource
        && !identitySidecarFillsWzAbsoluteSourceContract
        && !theoremClaimed,
        $"phase24InitialStatus={JsonString(phase24Initial.RootElement, "terminalStatus")}; phase27IdentityStatus={JsonString(phase27Identity.RootElement, "terminalStatus")}; phase27MixingStatus={JsonString(phase27Mixing.RootElement, "terminalStatus")}; upstreamFillsWzAbsoluteSourceContract={identitySidecarFillsWzAbsoluteSourceContract}; theoremClaimed={theoremClaimed}"),
    new Check(
        "source-contract-remains-blocked",
        !p302CanFillContract && !sourceRowsPromotable && !canFillPhase201WzContract && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"p302CanFillContract={p302CanFillContract}; sourceRowsPromotable={sourceRowsPromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

bool identitySplitBranchSourceNormalizationAuditPassed =
    checks.All(check => check.Passed)
    && allRowsRawPassingCandidates.Length == 0
    && stableCandidates.Length == 0
    && stableRawCommonAllRowsCandidates.Length == 0
    && !sourceRowsPromotable
    && !canFillPhase201WzContract;

var terminalStatus = identitySplitBranchSourceNormalizationAuditPassed
    ? "identity-split-branch-source-normalization-audit-no-stable-source-normalizer"
    : "identity-split-branch-source-normalization-audit-review-required";

var result = new
{
    phaseId = "phase303-identity-split-branch-source-normalization-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    identitySplitBranchSourceNormalizationAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    p302BestCandidateId,
    p302BestWTotalScale,
    p302BestZTotalScale,
    p302BestRawAndCommonPassed,
    p302BestStableRawCommonPassed,
    phase27IdentityRuleReady,
    phase27MixingConventionReady,
    phase251UpstreamIdentityReady,
    phase251UpstreamIdentityNotAbsoluteSource,
    identitySidecarFillsWzAbsoluteSourceContract,
    rawGateRatio,
    stabilitySpreadTolerance,
    commonScaleSpreadTolerance,
    rowCount = rows.Length,
    descriptorDefinitionCount = descriptorDefinitions.Length,
    candidateAssessmentCount = candidateAssessments.Length,
    allRowsRawPassingCandidateCount = allRowsRawPassingCandidates.Length,
    stableCandidateCount = stableCandidates.Length,
    stableRawCommonAllRowsCandidateCount = stableRawCommonAllRowsCandidates.Length,
    theoremClaimed,
    sourceRowsPromotable,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    phase302BestCandidate,
    bestDescriptorCandidate,
    topCandidateAssessments = candidateAssessments.Take(15).ToArray(),
    sourceRows = rows.Select(row => new
    {
        row.ParticleId,
        row.CandidateId,
        row.BackgroundId,
        row.EtaModeId,
        row.RawMatrixElementMagnitude,
        row.RawToTargetRatio,
        row.Phase302TotalScale,
        phase302ScaledRawToTargetRatio = row.RawToTargetRatio * row.Phase302TotalScale,
        row.Mode,
    }).ToArray(),
    descriptorDefinitions = descriptorDefinitions.Select(descriptor => new { descriptor.DescriptorId, descriptor.Description }).ToArray(),
    inheritedBlockers = new
    {
        phase24 = new
        {
            status = JsonString(phase24Initial.RootElement, "terminalStatus"),
            path = Phase24InitialPath,
        },
        phase27 = new
        {
            identityStatus = JsonString(phase27Identity.RootElement, "terminalStatus"),
            mixingStatus = JsonString(phase27Mixing.RootElement, "terminalStatus"),
            identityRuleReady = phase27IdentityRuleReady,
            mixingConventionReady = phase27MixingConventionReady,
        },
        phase251 = new
        {
            upstreamIdentityReady = phase251UpstreamIdentityReady,
            upstreamIdentityNotAbsoluteSource = phase251UpstreamIdentityNotAbsoluteSource,
            upstreamFillsWzAbsoluteSourceContract = identitySidecarFillsWzAbsoluteSourceContract,
        },
        phase302 = new
        {
            p302Passed,
            p302CanFillContract,
            bestCandidateId = p302BestCandidateId,
            sourceInvariantRawCommonPassingCandidateCount = JsonInt(phase302.RootElement, "sourceInvariantRawCommonPassingCandidateCount"),
            stableRawCommonPassingCandidateCount = JsonInt(phase302.RootElement, "stableRawCommonPassingCandidateCount"),
        },
    },
    checks,
    decision = identitySplitBranchSourceNormalizationAuditPassed
        ? "Do not promote the Phase302 identity-split near-pass by adding branch-local source descriptor normalizers. The best raw/common mean-scale lead still fails row-level raw and branch-stability sidecars, and simple target-independent source-mode descriptors do not produce a stable all-row source normalizer."
        : "Review identity-split branch source normalization before relying on this gate.",
    nextRequiredArtifact = new[]
    {
        "A branch-stable W/Z source law that derives the row normalization before comparison, not a post-hoc descriptor balancing rule.",
        "A bridge theorem transferring Phase27 internal W/Z identity labels into contract-grade source rows and the Phase64 fermion-current source.",
        "Phase201/P209 W/Z source rows with row-level raw sidecars, stability sidecars, common-bridge gates, target-comparison gates, and derivation ids filled.",
    },
    sourceEvidence = new
    {
        phase24InitialPath = Phase24InitialPath,
        phase27IdentityPath = Phase27IdentityPath,
        phase27MixingPath = Phase27MixingPath,
        phase251Path = Phase251Path,
        phase213Path = Phase213Path,
        phase299Path = Phase299Path,
        phase302Path = Phase302Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "identity_split_branch_source_normalization_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "identity_split_branch_source_normalization_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.identitySplitBranchSourceNormalizationAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.p302BestCandidateId,
        result.p302BestWTotalScale,
        result.p302BestZTotalScale,
        result.p302BestRawAndCommonPassed,
        result.p302BestStableRawCommonPassed,
        result.phase27IdentityRuleReady,
        result.phase27MixingConventionReady,
        result.phase251UpstreamIdentityReady,
        result.phase251UpstreamIdentityNotAbsoluteSource,
        result.identitySidecarFillsWzAbsoluteSourceContract,
        result.rowCount,
        result.descriptorDefinitionCount,
        result.candidateAssessmentCount,
        result.allRowsRawPassingCandidateCount,
        result.stableCandidateCount,
        result.stableRawCommonAllRowsCandidateCount,
        result.theoremClaimed,
        result.sourceRowsPromotable,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.phase302BestCandidate,
        result.bestDescriptorCandidate,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
        result.inheritedBlockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"identitySplitBranchSourceNormalizationAuditPassed={identitySplitBranchSourceNormalizationAuditPassed}");
Console.WriteLine($"allRowsRawPassingCandidateCount={allRowsRawPassingCandidates.Length}");
Console.WriteLine($"stableCandidateCount={stableCandidates.Length}");
Console.WriteLine($"stableRawCommonAllRowsCandidateCount={stableRawCommonAllRowsCandidates.Length}");
Console.WriteLine($"phase302BestMinRowScaledRawToTargetRatio={phase302BestCandidate.MinRowScaledRawToTargetRatio:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

CandidateAssessment EvaluateDescriptorCandidate(DescriptorDefinition descriptor, string direction)
{
    var meansByParticle = rows
        .GroupBy(row => row.ParticleId, StringComparer.Ordinal)
        .ToDictionary(
            group => group.Key,
            group => group.Average(row => descriptor.Value(row)),
            StringComparer.Ordinal);

    return EvaluateCandidate(
        $"{descriptor.DescriptorId}::{direction}",
        descriptor.DescriptorId,
        direction,
        row =>
        {
            double value = descriptor.Value(row);
            double mean = meansByParticle[row.ParticleId];
            if (!double.IsFinite(value) || value <= 0.0 || !double.IsFinite(mean) || mean <= 0.0)
                return double.NaN;

            return direction == "value-over-particle-mean"
                ? value / mean
                : mean / value;
        });
}

CandidateAssessment EvaluateCandidate(string candidateId, string descriptorId, string direction, Func<SourceRow, double> branchNormalizer)
{
    var rowAssessments = rows.Select(row =>
    {
        double normalizer = branchNormalizer(row);
        double scaled = row.RawToTargetRatio * row.Phase302TotalScale * normalizer;
        return new RowAssessment(
            row.ParticleId,
            row.CandidateId,
            row.BackgroundId,
            row.EtaModeId,
            row.RawToTargetRatio,
            row.Phase302TotalScale,
            normalizer,
            scaled,
            scaled >= rawGateRatio,
            row.Mode.DominantAxisIndex,
            row.Mode.DominantAxisEnergy);
    }).ToArray();

    var particleSummaries = rowAssessments
        .GroupBy(row => row.ParticleId, StringComparer.Ordinal)
        .Select(group =>
        {
            var values = group.Select(row => row.ScaledRawToTargetRatio).ToArray();
            double mean = values.Average();
            double spread = RelativeSpread(values);
            return new ParticleSummary(
                group.Key,
                mean,
                spread,
                mean >= rawGateRatio,
                group.All(row => row.RowRawGatePassed),
                spread <= stabilitySpreadTolerance);
        })
        .OrderBy(summary => summary.ParticleId, StringComparer.Ordinal)
        .ToArray();

    double[] particleMeans = particleSummaries.Select(summary => summary.MeanScaledRawToTargetRatio).ToArray();
    bool commonMeanGatePassed = RelativeSpread(particleMeans) <= commonScaleSpreadTolerance;
    bool allRowsRawGatePassed = rowAssessments.All(row => row.RowRawGatePassed);
    bool allParticleMeanRawGatesPassed = particleSummaries.All(summary => summary.MeanRawGatePassed);
    bool allParticlesStabilityPassed = particleSummaries.All(summary => summary.StabilityPassed);
    bool stableRawCommonAllRowsPassed = allRowsRawGatePassed && allParticleMeanRawGatesPassed && commonMeanGatePassed && allParticlesStabilityPassed;

    return new CandidateAssessment(
        candidateId,
        descriptorId,
        direction,
        rowAssessments.Min(row => row.ScaledRawToTargetRatio),
        rowAssessments.Max(row => row.ScaledRawToTargetRatio),
        particleSummaries.Max(summary => summary.RelativeSpread),
        commonMeanGatePassed,
        allRowsRawGatePassed,
        allParticleMeanRawGatesPassed,
        allParticlesStabilityPassed,
        stableRawCommonAllRowsPassed,
        false,
        rowAssessments,
        particleSummaries);
}

static double AxisEnergy(SourceRow row, int axisIndex) =>
    row.Mode.Axes.Single(axis => axis.AxisIndex == axisIndex).Energy;

static ModeDescriptor LoadModeDescriptor(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
    var vector = root.GetProperty("modeVector").EnumerateArray().Select(item => item.GetDouble()).ToArray();
    double l1 = vector.Sum(Math.Abs);
    double l2 = Math.Sqrt(vector.Sum(value => value * value));
    double lInf = vector.Select(Math.Abs).Max();
    var tripleNorms = vector
        .Chunk(3)
        .Select(chunk => Math.Sqrt(chunk.Sum(value => value * value)))
        .ToArray();
    double tripleL1 = tripleNorms.Sum();
    double tripleLInf = tripleNorms.Max();
    var axes = Enumerable.Range(0, 3)
        .Select(axis =>
        {
            var values = vector.Where((_, index) => index % 3 == axis).ToArray();
            double axisL2 = Math.Sqrt(values.Sum(value => value * value));
            return new AxisDescriptor(axis, values.Sum(Math.Abs), axisL2, axisL2 * axisL2, values.Select(Math.Abs).Max());
        })
        .ToArray();
    var dominant = axes.OrderByDescending(axis => axis.Energy).ThenBy(axis => axis.AxisIndex).First();

    return new ModeDescriptor(
        RequiredString(root, "modeId"),
        RequiredString(root, "backgroundId"),
        RequiredDouble(root, "eigenvalue"),
        RequiredDouble(root, "residualNorm"),
        RequiredString(root, "normalizationConvention"),
        vector.Length,
        l1,
        l2,
        lInf,
        tripleL1,
        tripleLInf,
        dominant.AxisIndex,
        dominant.L2Norm,
        dominant.Energy,
        axes);
}

static string ExtractModePath(string variationEvidenceId)
{
    int first = variationEvidenceId.IndexOf(":", StringComparison.Ordinal);
    int second = first < 0 ? -1 : variationEvidenceId.IndexOf(":", first + 1, StringComparison.Ordinal);
    if (second < 0 || second + 1 >= variationEvidenceId.Length)
        throw new InvalidDataException($"Unable to extract mode path from variation evidence id {variationEvidenceId}.");

    return variationEvidenceId[(second + 1)..];
}

static double RelativeSpread(IReadOnlyCollection<double> values)
{
    var finite = values.Where(double.IsFinite).ToArray();
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

sealed record SourceRow(
    string ParticleId,
    string CandidateId,
    string BackgroundId,
    string EtaModeId,
    string ModePath,
    double RawMatrixElementMagnitude,
    double RawToTargetRatio,
    double Phase302TotalScale,
    ModeDescriptor Mode);

sealed record ModeDescriptor(
    string ModeId,
    string BackgroundId,
    double Eigenvalue,
    double ResidualNorm,
    string NormalizationConvention,
    int VectorLength,
    double L1Norm,
    double L2Norm,
    double LInfinityNorm,
    double TripleL1Norm,
    double TripleLInfinityNorm,
    int DominantAxisIndex,
    double DominantAxisL2,
    double DominantAxisEnergy,
    AxisDescriptor[] Axes);

sealed record AxisDescriptor(int AxisIndex, double L1Norm, double L2Norm, double Energy, double LInfinityNorm);

sealed record DescriptorDefinition(string DescriptorId, string Description, Func<SourceRow, double> Value);

sealed record RowAssessment(
    string ParticleId,
    string CandidateId,
    string BackgroundId,
    string EtaModeId,
    double RawToTargetRatio,
    double Phase302TotalScale,
    double BranchNormalizer,
    double ScaledRawToTargetRatio,
    bool RowRawGatePassed,
    int DominantAxisIndex,
    double DominantAxisEnergy);

sealed record ParticleSummary(
    string ParticleId,
    double MeanScaledRawToTargetRatio,
    double RelativeSpread,
    bool MeanRawGatePassed,
    bool AllRowsRawGatePassed,
    bool StabilityPassed);

sealed record CandidateAssessment(
    string CandidateId,
    string DescriptorId,
    string Direction,
    double MinRowScaledRawToTargetRatio,
    double MaxRowScaledRawToTargetRatio,
    double MaxParticleRelativeSpread,
    bool CommonMeanGatePassed,
    bool AllRowsRawGatePassed,
    bool AllParticleMeanRawGatesPassed,
    bool AllParticlesStabilityPassed,
    bool StableRawCommonAllRowsPassed,
    bool PromotionEligible,
    RowAssessment[] RowAssessments,
    ParticleSummary[] ParticleSummaries);

sealed record Check(string CheckId, bool Passed, string Detail);
