using System.Diagnostics;
using System.Text.Json;

var stopwatch = Stopwatch.StartNew();
const string outputDir = "studies/phase486_committed_evidence_sensitivity_triage_001/output";
const string p447Path = "studies/phase447_two_loop_saturation_probe_001/output/two_loop_saturation_probe_summary.json";
const string p453Path = "studies/phase453_wham_parity_error_model_repair_001/output/wham_parity_error_model_repair_summary.json";
const string p455Path = "studies/phase455_exact_fermionic_backreaction_probe_001/output/exact_fermionic_backreaction_probe_summary.json";
const string p467Path = "studies/phase467_derived_operator_stabilizer_ray_census_001/output/derived_operator_stabilizer_ray_census_summary.json";

using var p447 = JsonDocument.Parse(File.ReadAllText(p447Path));
using var p453 = JsonDocument.Parse(File.ReadAllText(p453Path));
using var p455 = JsonDocument.Parse(File.ReadAllText(p455Path));
using var p467 = JsonDocument.Parse(File.ReadAllText(p467Path));
bool softFloorFragile = p447.RootElement.GetProperty("floorSweepStable").GetBoolean() is false;
bool ladderRepairRecorded = p453.RootElement.GetProperty("correctedLadder").GetProperty("productionSubLadderDropped").GetBoolean();
bool zeroModeFragile = p455.RootElement.GetProperty("verdictKind").GetString() == "convention-fragile";
bool compactTransferScoped = p467.RootElement.GetProperty("fieldOfDefinitionArm").GetProperty("fieldOfDefinitionArmPassed").GetBoolean();

var priorities = new[]
{
    new { rank = 1, item = "O4-F3-THETA-HAAR", nextTest = "independent small-lattice measure and proposal-invariance control", reason = "load-bearing and not yet alternative-checked" },
    new { rank = 2, item = "O4-P455-SB-MODEL", nextTest = "leave-one-workbench-model-out terminal sensitivity", reason = "zero-mode axis already flips and the model choice remains external" },
    new { rank = 3, item = "checkpoint-restart-equivalence", nextTest = "reduced deterministic uninterrupted-versus-resumed equality battery", reason = "required before any future expensive sampler" },
};
var result = new
{
    phaseId = "phase486-committed-evidence-sensitivity-triage",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = "committed-evidence-sensitivity-triage-fragilities-exposed-next-tests-ranked",
    verdictKind = "fragilities-exposed-next-tests-ranked",
    applicationSubjectKind = "committed-evidence-sensitivity-triage",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A5",
    amendmentOrder = 3,
    committedEvidenceOnly = true,
    findings = new { softFloorFragile, ladderRepairRecorded, zeroModeFragile, compactTransferScoped },
    priorityCount = priorities.Length,
    priorities,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    productionAuthorized = false,
    sourceContractApplicationAllowed = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = "Existing artifacts already expose soft-floor and zero-mode fragility, record the ladder repair, and provide a scoped compact-form machine check. Reduced theta-measure, model-sensitivity, and restart-equivalence tests are next; no review gate is discharged.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "committed_evidence_sensitivity_triage.json"), json);
File.WriteAllText(Path.Combine(outputDir, "committed_evidence_sensitivity_triage_summary.json"), json);
Console.WriteLine(result.terminalStatus);
