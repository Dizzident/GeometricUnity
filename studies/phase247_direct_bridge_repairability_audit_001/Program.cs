using System.Text.Json;

const string DefaultOutputDir = "studies/phase247_direct_bridge_repairability_audit_001/output";
const string GeometryPath = "studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json";
const string BackgroundAPath = "studies/phase12_joined_calculation_001/output/background_family/background_states/bg-phase12-bg-a-20260315212202_manifest.json";
const string BackgroundBPath = "studies/phase12_joined_calculation_001/output/background_family/background_states/bg-phase12-bg-b-20260315212202_manifest.json";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string UnifiedParticleRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/particle_registry/unified_particle_registry.json";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase206Path = "studies/phase206_direct_bridge_normalization_closure_001/output/direct_bridge_normalization_closure_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase246Path = "studies/phase246_minimal_unlock_candidate_inventory_001/output/minimal_unlock_candidate_inventory_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE247_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var geometry = JsonDocument.Parse(File.ReadAllText(GeometryPath));
using var backgroundA = JsonDocument.Parse(File.ReadAllText(BackgroundAPath));
using var backgroundB = JsonDocument.Parse(File.ReadAllText(BackgroundBPath));
using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var unifiedParticleRegistry = JsonDocument.Parse(File.ReadAllText(UnifiedParticleRegistryPath));
using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase206 = JsonDocument.Parse(File.ReadAllText(Phase206Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase246 = JsonDocument.Parse(File.ReadAllText(Phase246Path));

var phase190SiblingStability = phase190.RootElement.GetProperty("siblingStability");
var phase190BestCandidate = phase190SiblingStability.GetProperty("bestCandidate");
var phase191Gates = phase191.RootElement.GetProperty("gates");
var phase206DirectReplay = phase206.RootElement.GetProperty("directUnitScaleReplay");

var p190CandidateLawConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed") is true;
var p190TargetObservablesUsed = JsonBool(phase190.RootElement, "targetObservablesUsed") is true;
var p190TheoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var p190StableCandidateCount = JsonInt(phase190SiblingStability, "stableCandidateCount") ?? 0;
var p190BestCandidateId = JsonString(phase190BestCandidate, "candidateId") ?? "missing";
var p190BestRelativeSpread = JsonDouble(phase190BestCandidate, "relativeSpread");
var p191CanCompleteSuccessfulPrediction = JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction") is true;
var p191BestRawToTargetRatio = JsonDouble(phase191Gates, "bestRawToTargetRatio");
var p191RawGatePassed = JsonBool(phase191Gates, "rawGatePassed") is true;
var p191WzParticleSplitPresent = JsonBool(phase191Gates, "wZParticleSplitPresent") is true;
var p206CanPromoteDirectBridgeNormalization = JsonBool(phase206.RootElement, "canPromoteDirectBridgeNormalization") is true;
var p206DirectUnitScaledToTargetRaw = JsonDouble(phase206DirectReplay, "directUnitScaledToTargetRaw");
var p206DirectUnitScaleOvershootRatio = JsonDouble(phase206DirectReplay, "directUnitScaleOvershootRatio");
var p213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var p213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p246AnyCandidateFillsWz = JsonBool(phase246.RootElement, "anyCandidateFillsWzAbsoluteScaleUnlock") is true;

var bosonCandidates = bosonRegistry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(row => new RegistryCandidate(
        JsonString(row, "candidateId") ?? "missing",
        JsonString(row, "primaryFamilyId") ?? "missing",
        JsonString(row, "claimClass") ?? "missing",
        JsonDouble(row, "branchStabilityScore"),
        JsonInt(row, "ambiguityCount"),
        row.TryGetProperty("contributingModeIds", out var modes)
            ? modes.EnumerateArray().Select(item => item.GetString() ?? "").Where(item => item.Length > 0).ToArray()
            : Array.Empty<string>()))
    .ToArray();

var unifiedParticles = unifiedParticleRegistry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(row => new UnifiedParticleCandidate(
        JsonString(row, "particleId") ?? "missing",
        JsonString(row, "particleType") ?? "missing",
        JsonString(row, "primarySourceId") ?? "missing",
        JsonString(row, "claimClass") ?? "missing",
        JsonDouble(row, "branchStabilityScore"),
        JsonDouble(row, "comparisonEvidenceScore"),
        row.TryGetProperty("contributingSourceIds", out var ids)
            ? ids.EnumerateArray().Select(item => item.GetString() ?? "").Where(item => item.Length > 0).ToArray()
            : Array.Empty<string>()))
    .ToArray();
var unifiedBosonParticles = unifiedParticles
    .Where(row => row.ParticleType == "Boson")
    .ToArray();

var backgroundSummaries = new[]
{
    BackgroundSummary(backgroundA.RootElement),
    BackgroundSummary(backgroundB.RootElement),
};

var particleIds = unifiedBosonParticles.Select(row => row.ParticleId).ToArray();
var joinedParticleIdText = string.Join(" ", particleIds).ToLowerInvariant();
var modeRegistryHasObservedWzLabels = joinedParticleIdText.Contains("w-boson", StringComparison.Ordinal)
    || joinedParticleIdText.Contains("z-boson", StringComparison.Ordinal)
    || joinedParticleIdText.Contains("w_boson", StringComparison.Ordinal)
    || joinedParticleIdText.Contains("z_boson", StringComparison.Ordinal);
var allBosonCandidatesAreNumericalModes = bosonCandidates.Length > 0
    && bosonCandidates.All(row => row.ClaimClass == "C0_NumericalMode");
var allUnifiedParticlesAreNumericalModes = unifiedParticles.Length > 0
    && unifiedBosonParticles.Length > 0
    && unifiedBosonParticles.All(row => row.ClaimClass == "C0_NumericalMode");
var allBosonCandidatesAmbiguous = bosonCandidates.Length > 0
    && bosonCandidates.All(row => (row.AmbiguityCount ?? 0) > 0);
var phase12BackgroundsUseOnlySu2 = backgroundSummaries.All(row => row.LieAlgebraId == "su2");
var phase12BackgroundsUseToySurrogateBranches = backgroundSummaries.All(row =>
    row.BaseDimension == 2
    && row.ActiveShiabBranch == "identity-shiab"
    && row.ActiveTorsionBranch == "trivial-torsion"
    && row.TorsionDraftAlignment == "surrogate-first-order");
var currentGeometrySupportsLowEnergyElectroweakSplit = false;
var wzParticleSplitDerivableFromCurrentRegistry = modeRegistryHasObservedWzLabels
    && !allUnifiedParticlesAreNumericalModes
    && !allBosonCandidatesAmbiguous
    && phase12BackgroundsUseOnlySu2 is false;
var sourceRowRepairPossibleFromCurrentRegistry = wzParticleSplitDerivableFromCurrentRegistry
    && p190TheoremClaimed
    && p191RawGatePassed;
var rawGateRepairPossibleWithoutNewTheorem = p191RawGatePassed
    && p206CanPromoteDirectBridgeNormalization;
var currentDirectBridgeCandidatePromotable = p190CandidateLawConstructed
    && !p190TargetObservablesUsed
    && p190TheoremClaimed
    && p191RawGatePassed
    && p191WzParticleSplitPresent
    && sourceRowRepairPossibleFromCurrentRegistry;
var newDirectBridgeTheoremStillRequired = !currentDirectBridgeCandidatePromotable
    && !p245UnlockContractFilled
    && !p246AnyCandidateFillsWz
    && p213WzMissingFieldCount == 15;

var blockerRows = new[]
{
    new BlockerRow("sibling-stability-gate", p190StableCandidateCount == 0, $"P190 branch-local replay has {p190StableCandidateCount} sibling-stable candidates; best relative spread is {FormatMaybe(p190BestRelativeSpread)} against the stability gate."),
    new BlockerRow("direct-bridge-theorem", !p190TheoremClaimed, "P190 constructs a target-independent branch-local candidate law but does not promote it as a theorem or derivation."),
    new BlockerRow("raw-amplitude-gate", !p191RawGatePassed, $"P191 best raw-to-target ratio is {FormatMaybe(p191BestRawToTargetRatio)}, below the required raw gate."),
    new BlockerRow("wz-particle-specific-source-split", !p191WzParticleSplitPresent && !wzParticleSplitDerivableFromCurrentRegistry, "P191 has no separate W and Z source rows, and the current registry has no observed W/Z identity labels to derive them."),
    new BlockerRow("registry-claim-class", allBosonCandidatesAreNumericalModes && allUnifiedParticlesAreNumericalModes, "Phase12 boson and unified particle registries classify all candidates as C0 numerical modes, not source-lineage prediction rows."),
    new BlockerRow("registry-ambiguity", allBosonCandidatesAmbiguous, "Every Phase12 boson candidate has ambiguityCount > 0, so the registry cannot select a unique W/Z source row."),
    new BlockerRow("surrogate-background-scope", phase12BackgroundsUseToySurrogateBranches && !currentGeometrySupportsLowEnergyElectroweakSplit, "The inspected backgrounds are 2D surrogate SU(2) branches with identity Shiab and trivial torsion, not a low-energy electroweak SU(2)xU(1) split with VEV/source extraction."),
    new BlockerRow("direct-unit-scale-normalization", !p206CanPromoteDirectBridgeNormalization, $"P206 direct unit-scale replay is diagnostic only and overshoots the target raw scale by {FormatMaybe(p206DirectUnitScaleOvershootRatio)}."),
};

var checks = new[]
{
    new Check("p190-target-independent-candidate-evaluated", p190CandidateLawConstructed && !p190TargetObservablesUsed, $"candidateLawConstructed={p190CandidateLawConstructed}; targetObservablesUsed={p190TargetObservablesUsed}; stableCandidateCount={p190StableCandidateCount}; bestCandidate={p190BestCandidateId}; relativeSpread={FormatMaybe(p190BestRelativeSpread)}"),
    new Check("p190-not-theorem-promoted", !p190TheoremClaimed, $"theoremClaimed={p190TheoremClaimed}"),
    new Check("p191-prediction-not-completable", !p191CanCompleteSuccessfulPrediction && !p191RawGatePassed && !p191WzParticleSplitPresent, $"canCompleteSuccessfulPrediction={p191CanCompleteSuccessfulPrediction}; rawGatePassed={p191RawGatePassed}; wZParticleSplitPresent={p191WzParticleSplitPresent}; bestRawToTargetRatio={FormatMaybe(p191BestRawToTargetRatio)}"),
    new Check("p206-normalization-not-promotable", !p206CanPromoteDirectBridgeNormalization, $"canPromoteDirectBridgeNormalization={p206CanPromoteDirectBridgeNormalization}; directUnitScaledToTargetRaw={FormatMaybe(p206DirectUnitScaledToTargetRaw)}; directUnitScaleOvershootRatio={FormatMaybe(p206DirectUnitScaleOvershootRatio)}"),
    new Check("registry-has-no-observed-wz-labels", !modeRegistryHasObservedWzLabels && allBosonCandidatesAreNumericalModes && allUnifiedParticlesAreNumericalModes, $"modeRegistryHasObservedWzLabels={modeRegistryHasObservedWzLabels}; bosonCandidateCount={bosonCandidates.Length}; unifiedParticleCount={unifiedParticles.Length}; allBosonCandidatesAreNumericalModes={allBosonCandidatesAreNumericalModes}; allUnifiedParticlesAreNumericalModes={allUnifiedParticlesAreNumericalModes}"),
    new Check("registry-cannot-derive-particle-split", !wzParticleSplitDerivableFromCurrentRegistry, $"wzParticleSplitDerivableFromCurrentRegistry={wzParticleSplitDerivableFromCurrentRegistry}; allBosonCandidatesAmbiguous={allBosonCandidatesAmbiguous}; phase12BackgroundsUseOnlySu2={phase12BackgroundsUseOnlySu2}"),
    new Check("phase12-scope-not-low-energy-electroweak-split", !currentGeometrySupportsLowEnergyElectroweakSplit && phase12BackgroundsUseToySurrogateBranches, $"currentGeometrySupportsLowEnergyElectroweakSplit={currentGeometrySupportsLowEnergyElectroweakSplit}; phase12BackgroundsUseToySurrogateBranches={phase12BackgroundsUseToySurrogateBranches}"),
    new Check("p245-p246-wz-unlock-still-unfilled", !p245UnlockContractFilled && !p246AnyCandidateFillsWz && p213WzMissingFieldCount == 15, $"p245UnlockContractFilled={p245UnlockContractFilled}; p246AnyCandidateFillsWz={p246AnyCandidateFillsWz}; p213WzMissingFieldCount={p213WzMissingFieldCount}"),
    new Check("new-direct-bridge-theorem-still-required", newDirectBridgeTheoremStillRequired, $"newDirectBridgeTheoremStillRequired={newDirectBridgeTheoremStillRequired}"),
};

var directBridgeRepairabilityAuditPassed = checks.All(check => check.Passed)
    && blockerRows.All(row => row.Active)
    && !currentDirectBridgeCandidatePromotable
    && !sourceRowRepairPossibleFromCurrentRegistry;
var terminalStatus = directBridgeRepairabilityAuditPassed
    ? "direct-bridge-repairability-audit-complete-new-theorem-required"
    : "direct-bridge-repairability-audit-review-required";

var result = new
{
    phaseId = "phase247-direct-bridge-repairability-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    directBridgeRepairabilityAuditPassed,
    currentDirectBridgeCandidatePromotable,
    sourceRowRepairPossibleFromCurrentRegistry,
    wzParticleSplitDerivableFromCurrentRegistry,
    rawGateRepairPossibleWithoutNewTheorem,
    modeRegistryHasObservedWzLabels,
    currentGeometrySupportsLowEnergyElectroweakSplit,
    newDirectBridgeTheoremStillRequired,
    p190 = new
    {
        candidateLawConstructed = p190CandidateLawConstructed,
        targetObservablesUsed = p190TargetObservablesUsed,
        theoremClaimed = p190TheoremClaimed,
        stableCandidateCount = p190StableCandidateCount,
        bestCandidateId = p190BestCandidateId,
        bestRelativeSpread = p190BestRelativeSpread,
    },
    p191 = new
    {
        canCompleteSuccessfulPrediction = p191CanCompleteSuccessfulPrediction,
        bestRawToTargetRatio = p191BestRawToTargetRatio,
        rawGatePassed = p191RawGatePassed,
        wZParticleSplitPresent = p191WzParticleSplitPresent,
    },
    p206 = new
    {
        canPromoteDirectBridgeNormalization = p206CanPromoteDirectBridgeNormalization,
        directUnitScaledToTargetRaw = p206DirectUnitScaledToTargetRaw,
        directUnitScaleOvershootRatio = p206DirectUnitScaleOvershootRatio,
    },
    registryEvidence = new
    {
        bosonCandidateCount = bosonCandidates.Length,
        unifiedParticleCount = unifiedParticles.Length,
        allBosonCandidatesAreNumericalModes,
        allUnifiedParticlesAreNumericalModes,
        allBosonCandidatesAmbiguous,
        modeRegistryHasObservedWzLabels,
        bosonCandidates = bosonCandidates
            .Select(row => new { row.CandidateId, row.PrimaryFamilyId, row.ClaimClass, row.BranchStabilityScore, row.AmbiguityCount, row.ContributingModeIds })
            .ToArray(),
        unifiedBosonParticleCount = unifiedBosonParticles.Length,
        unifiedBosonParticleSample = unifiedBosonParticles
            .Take(12)
            .Select(row => new { row.ParticleId, row.PrimarySourceId, row.ClaimClass, row.BranchStabilityScore, row.ComparisonEvidenceScore, row.ContributingSourceIds })
            .ToArray(),
    },
    backgroundSummaries,
    phase213Blockers = new
    {
        wzMissingFieldCount = p213WzMissingFieldCount,
        higgsMissingFieldCount = p213HiggsMissingFieldCount,
    },
    blockerRows,
    checks,
    decision = directBridgeRepairabilityAuditPassed
        ? "Do not patch P190/P191 into a W/Z prediction from current registry data. The direct bridge route is target-independent, but current branch-local artifacts do not supply sibling stability, theorem promotion, raw-gate closure, or separate W/Z source rows. A new source theorem/registry artifact is required."
        : "Review direct bridge repairability before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed W/Z direct bridge theorem or branch-local proof discharging the mixed-linearization obligation.",
        "A registry/source-lineage artifact that identifies separate W and Z source rows, not only unified numerical boson candidates.",
        "A normalization/source-shape law that clears raw-amplitude, common-bridge, and target-comparison gates without target-fitted construction.",
    },
    sourceEvidence = new
    {
        geometryPath = GeometryPath,
        backgroundAPath = BackgroundAPath,
        backgroundBPath = BackgroundBPath,
        bosonRegistryPath = BosonRegistryPath,
        unifiedParticleRegistryPath = UnifiedParticleRegistryPath,
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase206Path = Phase206Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase246Path = Phase246Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "direct_bridge_repairability_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "direct_bridge_repairability_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.directBridgeRepairabilityAuditPassed,
        result.currentDirectBridgeCandidatePromotable,
        result.sourceRowRepairPossibleFromCurrentRegistry,
        result.wzParticleSplitDerivableFromCurrentRegistry,
        result.rawGateRepairPossibleWithoutNewTheorem,
        result.modeRegistryHasObservedWzLabels,
        result.currentGeometrySupportsLowEnergyElectroweakSplit,
        result.newDirectBridgeTheoremStillRequired,
        result.p190,
        result.p191,
        result.p206,
        result.registryEvidence,
        result.backgroundSummaries,
        result.phase213Blockers,
        result.blockerRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"directBridgeRepairabilityAuditPassed={directBridgeRepairabilityAuditPassed}");
Console.WriteLine($"currentDirectBridgeCandidatePromotable={currentDirectBridgeCandidatePromotable}");
Console.WriteLine($"sourceRowRepairPossibleFromCurrentRegistry={sourceRowRepairPossibleFromCurrentRegistry}");
Console.WriteLine($"wzParticleSplitDerivableFromCurrentRegistry={wzParticleSplitDerivableFromCurrentRegistry}");
Console.WriteLine($"newDirectBridgeTheoremStillRequired={newDirectBridgeTheoremStillRequired}");

static BackgroundScope BackgroundSummary(JsonElement root) => new(
    JsonString(root, "branchId") ?? "missing",
    JsonInt(root, "baseDimension"),
    JsonInt(root, "ambientDimension"),
    JsonString(root, "lieAlgebraId") ?? "missing",
    JsonString(root, "activeShiabBranch") ?? "missing",
    JsonString(root, "activeTorsionBranch") ?? "missing",
    JsonString(root, "activeObservationBranch") ?? "missing",
    JsonString(root, "torsionDraftAlignment") ?? "missing",
    root.TryGetProperty("insertedAssumptionIds", out var assumptions)
        ? assumptions.EnumerateArray().Select(item => item.GetString() ?? "").Where(item => item.Length > 0).ToArray()
        : Array.Empty<string>(),
    root.TryGetProperty("insertedChoiceIds", out var choices)
        ? choices.EnumerateArray().Select(item => item.GetString() ?? "").Where(item => item.Length > 0).ToArray()
        : Array.Empty<string>());

static string FormatMaybe(double? value) => value.HasValue ? value.Value.ToString("G17") : "missing";

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record RegistryCandidate(string CandidateId, string PrimaryFamilyId, string ClaimClass, double? BranchStabilityScore, int? AmbiguityCount, string[] ContributingModeIds);
sealed record UnifiedParticleCandidate(string ParticleId, string ParticleType, string PrimarySourceId, string ClaimClass, double? BranchStabilityScore, double? ComparisonEvidenceScore, string[] ContributingSourceIds);
sealed record BackgroundScope(string BranchId, int? BaseDimension, int? AmbientDimension, string LieAlgebraId, string ActiveShiabBranch, string ActiveTorsionBranch, string ActiveObservationBranch, string TorsionDraftAlignment, string[] InsertedAssumptionIds, string[] InsertedChoiceIds);
sealed record BlockerRow(string BlockerId, bool Active, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
