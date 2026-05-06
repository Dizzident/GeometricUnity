using System.Text.Json;
using System.Text.Json.Nodes;

const string DefaultOutputDir = "studies/phase124_phase95_source_join_metadata_materialization_001/output";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase95EvidencePath = "studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_evidence.json";
const string Phase123Path = "studies/phase123_wz_fermion_sector_metadata_audit_001/output/wz_fermion_sector_metadata_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE124_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase95Doc = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var evidenceDoc = JsonDocument.Parse(File.ReadAllText(Phase95EvidencePath));
using var phase123Doc = JsonDocument.Parse(File.ReadAllText(Phase123Path));

var selectedSources = evidenceDoc.RootElement.GetProperty("selectedPhase91Pair").EnumerateArray().Select(ModeSummary.FromJson).ToList();
var matchedTargets = evidenceDoc.RootElement.GetProperty("matchedPhase94L0Pair").EnumerateArray().Select(ModeSummary.FromJson).ToList();
if (selectedSources.Count != matchedTargets.Count)
    throw new InvalidDataException($"Source/target pair count mismatch: {selectedSources.Count} != {matchedTargets.Count}.");

var joinRecords = selectedSources.Zip(matchedTargets, (source, target) => new SourceJoinRecord(
    TargetModeId: target.ModeId,
    TargetModeIndex: target.ModeIndex,
    SourceFermionModeId: source.ModeId,
    SourceFermionModeIndex: source.ModeIndex,
    SourceBackgroundId: source.BackgroundId,
    TargetBackgroundId: target.BackgroundId,
    EigenvalueAbsoluteDifference: Math.Abs(Math.Abs(target.EigenvalueRe) - Math.Abs(source.EigenvalueRe)),
    SourceEigenvalueRe: source.EigenvalueRe,
    TargetEigenvalueRe: target.EigenvalueRe,
    SourceResidualNorm: source.ResidualNorm,
    TargetResidualNorm: target.ResidualNorm,
    EvidenceId: RequiredString(evidenceDoc.RootElement, "evidenceId"))).ToList();
var joinByTargetModeId = joinRecords.ToDictionary(r => r.TargetModeId, StringComparer.Ordinal);

var enrichedBundle = JsonNode.Parse(phase95Doc.RootElement.GetRawText())!.AsObject();
enrichedBundle["resultId"] = "phase124-source-join-enriched-phase95-l0-fermion-modes";
enrichedBundle["sourceJoinMetadata"] = JsonSerializer.SerializeToNode(new
{
    materializationId = "phase124-phase95-source-join-metadata-materialization",
    evidenceId = RequiredString(evidenceDoc.RootElement, "evidenceId"),
    evidencePath = Phase95EvidencePath,
    sourceSelectedModesPath = RequiredString(evidenceDoc.RootElement, "sourceSelectedModesPath"),
    targetModesPath = Phase95ModesPath,
    externalTargetsUsed = JsonBool(evidenceDoc.RootElement, "externalTargetsUsed") is true,
    matchingBasis = StringArray(evidenceDoc.RootElement, "matchingBasis"),
    materializedFields = new[]
    {
        "sourceFermionModeId",
        "sourceFermionModeIndex",
        "sourceFermionBackgroundId",
        "sourceModeJoin",
    },
});

int enrichedModeCount = 0;
int qualityEnrichedModeCount = 0;
var enrichedModeRecords = new List<EnrichedModeRecord>();
var modes = enrichedBundle["modes"]?.AsArray() ?? throw new InvalidDataException("Phase95 bundle has no modes array.");
foreach (var node in modes)
{
    var mode = node?.AsObject() ?? throw new InvalidDataException("Mode entry is not an object.");
    string modeId = RequiredNodeString(mode, "modeId");
    int modeIndex = RequiredNodeInt(mode, "modeIndex");
    bool qualityPassed = (JsonNodeDouble(mode, "branchStabilityScore") ?? 0.0) >= 0.5
        && (JsonNodeDouble(mode, "refinementStabilityScore") ?? 0.0) >= 0.5
        && (JsonNodeDouble(mode, "residualNorm") ?? double.PositiveInfinity) <= 1e-6
        && JsonNodeBool(mode, "gaugeReductionApplied") is true;

    if (joinByTargetModeId.TryGetValue(modeId, out var join))
    {
        mode["sourceFermionModeId"] = join.SourceFermionModeId;
        mode["sourceFermionModeIndex"] = join.SourceFermionModeIndex;
        mode["sourceFermionBackgroundId"] = join.SourceBackgroundId;
        mode["sourceModeJoin"] = JsonSerializer.SerializeToNode(new
        {
            evidenceId = join.EvidenceId,
            evidencePath = Phase95EvidencePath,
            sourceFermionModeId = join.SourceFermionModeId,
            sourceFermionModeIndex = join.SourceFermionModeIndex,
            sourceBackgroundId = join.SourceBackgroundId,
            targetBackgroundId = join.TargetBackgroundId,
            eigenvalueAbsoluteDifference = join.EigenvalueAbsoluteDifference,
            matchingBasis = "same-sign nearest absolute Dirac eigenvalue within Phase95 target-blind evidence",
            externalTargetsUsed = false,
        });

        enrichedModeCount++;
        if (qualityPassed)
            qualityEnrichedModeCount++;
    }

    enrichedModeRecords.Add(new EnrichedModeRecord(
        ModeId: modeId,
        ModeIndex: modeIndex,
        QualityPassed: qualityPassed,
        SourceJoinMaterialized: joinByTargetModeId.ContainsKey(modeId),
        SourceFermionModeId: joinByTargetModeId.TryGetValue(modeId, out var record) ? record.SourceFermionModeId : null));
}

var qualityModeCount = enrichedModeRecords.Count(r => r.QualityPassed);
bool allQualityModesHaveSourceJoin = qualityModeCount > 0 && qualityEnrichedModeCount == qualityModeCount;
bool sourceJoinMetadataMaterialized = enrichedModeCount > 0 && allQualityModesHaveSourceJoin;
bool sectorRulePromotable = false;
string terminalStatus = sourceJoinMetadataMaterialized
    ? "phase95-source-join-metadata-materialized-sector-labels-blocked"
    : "phase95-source-join-metadata-materialization-blocked";

var blockers = new List<string>();
if (!sourceJoinMetadataMaterialized)
    blockers.Add("target-blind source-mode join keys could not be materialized for all quality Phase95 modes");
blockers.Add("fermion chargeSector remains absent");
blockers.Add("fermion familyId remains absent");
blockers.Add("weak-sector or quantum-number fields remain absent");
blockers.Add("chirality and conjugation pairing remain not evaluated for repaired exact modes");

var enrichedPath = Path.Combine(outputDir, "phase95_l0_source_join_enriched_fermion_modes.json");
var result = new
{
    phaseId = "phase124-phase95-source-join-metadata-materialization",
    terminalStatus,
    sourceJoinMetadataMaterialized,
    sectorRulePromotable,
    enrichedModeBundlePath = enrichedPath,
    phase123Gate = new
    {
        terminalStatus = JsonString(phase123Doc.RootElement, "terminalStatus"),
        fermionSectorRulePromotable = JsonBool(phase123Doc.RootElement, "fermionSectorRulePromotable"),
        fermionSectorMetadataAvailable = JsonBool(phase123Doc.RootElement, "fermionSectorMetadataAvailable"),
    },
    sourceJoinAudit = new
    {
        sourcePairCount = selectedSources.Count,
        matchedPairCount = matchedTargets.Count,
        modeCount = modes.Count,
        qualityModeCount,
        enrichedModeCount,
        qualityEnrichedModeCount,
        allQualityModesHaveSourceJoin,
        joinRecords,
        enrichedModeRecords,
    },
    diagnosis = sourceJoinMetadataMaterialized
        ? new[]
        {
            "target-blind source-mode join keys are now materialized for the quality Phase95 L0 modes",
            "the remaining W/Z transition-rule blocker is fermion sector identity, not source join provenance",
        }
        : new[]
        {
            "the Phase95 evidence does not cover all quality modes with source-mode join keys",
        },
    blockers,
    closureRequirements = new[]
    {
        "derive or materialize target-blind fermion chargeSector/familyId/weak-sector labels on the source-join-enriched repaired modes",
        "evaluate chirality and conjugation pairing for the repaired exact modes or provide a separate target-blind sector identity table",
        "rerun the corrected-operator transition sweep only after the sector labels define a W/Z transition rule",
    },
    sourceEvidence = new
    {
        phase95ModesPath = Phase95ModesPath,
        phase95EvidencePath = Phase95EvidencePath,
        phase123Path = Phase123Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(enrichedPath, enrichedBundle.ToJsonString(options));
File.WriteAllText(
    Path.Combine(outputDir, "phase95_source_join_metadata_materialization.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase95_source_join_metadata_materialization_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.sourceJoinMetadataMaterialized,
        result.sectorRulePromotable,
        result.enrichedModeBundlePath,
        result.sourceJoinAudit.modeCount,
        result.sourceJoinAudit.qualityModeCount,
        result.sourceJoinAudit.qualityEnrichedModeCount,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sourceJoinMetadataMaterialized={sourceJoinMetadataMaterialized}");
Console.WriteLine($"qualityEnrichedModeCount={qualityEnrichedModeCount}/{qualityModeCount}");
Console.WriteLine($"enrichedModeBundlePath={enrichedPath}");

static string RequiredString(JsonElement element, string propertyName)
{
    return JsonString(element, propertyName)
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static string RequiredNodeString(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValue<string>()
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static int RequiredNodeInt(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValue<int>()
        ?? throw new InvalidDataException($"Missing integer property '{propertyName}'.");
}

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property)
        ? property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        }
        : null;
}

static bool? JsonNodeBool(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValueKind() switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}

static double? JsonNodeDouble(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValueKind() == JsonValueKind.Number
        ? element[propertyName]!.GetValue<double>()
        : null;
}

static IReadOnlyList<string> StringArray(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToList()
        : Array.Empty<string>();
}

sealed record ModeSummary(
    string ModeId,
    int ModeIndex,
    string BackgroundId,
    double EigenvalueRe,
    double ResidualNorm)
{
    public static ModeSummary FromJson(JsonElement element)
    {
        return new ModeSummary(
            RequiredElementString(element, "modeId"),
            element.GetProperty("modeIndex").GetInt32(),
            RequiredElementString(element, "backgroundId"),
            element.GetProperty("eigenvalueRe").GetDouble(),
            element.GetProperty("residualNorm").GetDouble());
    }

    private static string RequiredElementString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? throw new InvalidDataException($"Null string property '{propertyName}'.")
            : throw new InvalidDataException($"Missing string property '{propertyName}'.");
    }
}

sealed record EnrichedModeRecord(
    string ModeId,
    int ModeIndex,
    bool QualityPassed,
    bool SourceJoinMaterialized,
    string? SourceFermionModeId);

sealed record SourceJoinRecord(
    string TargetModeId,
    int TargetModeIndex,
    string SourceFermionModeId,
    int SourceFermionModeIndex,
    string SourceBackgroundId,
    string TargetBackgroundId,
    double EigenvalueAbsoluteDifference,
    double SourceEigenvalueRe,
    double TargetEigenvalueRe,
    double SourceResidualNorm,
    double TargetResidualNorm,
    string EvidenceId);
