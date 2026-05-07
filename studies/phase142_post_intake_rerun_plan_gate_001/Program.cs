using System.Text.Json;

const string DefaultOutputDir = "studies/phase142_post_intake_rerun_plan_gate_001/output";
const string Phase130Path = "studies/phase130_fermion_sector_label_table_gate_001/output/fermion_sector_label_table_gate.json";
const string Phase135Path = "studies/phase135_corrected_wz_sweep_readiness_gate_001/output/corrected_wz_sweep_readiness_gate.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";
const string Phase122Project = "studies/phase122_corrected_operator_selection_rule_sweep_001/Phase122CorrectedOperatorSelectionRuleSweep.csproj";
const string Phase130Project = "studies/phase130_fermion_sector_label_table_gate_001/Phase130FermionSectorLabelTableGate.csproj";
const string Phase135Project = "studies/phase135_corrected_wz_sweep_readiness_gate_001/Phase135CorrectedWzSweepReadinessGate.csproj";
const string Phase101Project = "studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj";

var outputDir = Environment.GetEnvironmentVariable("PHASE142_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase130 = JsonDocument.Parse(File.ReadAllText(Phase130Path));
using var phase135 = JsonDocument.Parse(File.ReadAllText(Phase135Path));
using var phase141 = JsonDocument.Parse(File.ReadAllText(Phase141Path));

bool p141Ready = JsonBool(phase141.RootElement, "intakeApplicationPromotable") is true;
bool appliedTableReady = phase141.RootElement.TryGetProperty("appliedSectorLabelTable", out var table)
    && string.Equals(JsonString(table, "status"), "ready", StringComparison.Ordinal);
bool transitionRuleReady = phase141.RootElement.TryGetProperty("appliedTransitionRule", out var transitionRule)
    && string.Equals(JsonString(transitionRule, "status"), "ready", StringComparison.Ordinal);
bool previousSectorGateReady = JsonBool(phase130.RootElement, "sectorLabelTablePromotable") is true;
bool previousSweepReady = JsonBool(phase135.RootElement, "rerunReady") is true;
bool appliedIntakeReady = appliedTableReady || transitionRuleReady;
bool rerunPlanExecutable = p141Ready && appliedIntakeReady;
string terminalStatus = rerunPlanExecutable
    ? "post-intake-rerun-plan-ready"
    : "post-intake-rerun-plan-blocked";

var rerunSteps = new[]
{
    Step(1, "apply-intake-artifact", transitionRuleReady
        ? "Use the P141 applied transition rule as the repaired W/Z sector-selection source."
        : "Use the P141 applied label table as the repaired sector-label source.", p141Ready && appliedIntakeReady),
    Step(2, "rerun-sector-label-or-rule-gate", transitionRuleReady
        ? "Skip P130 label-table rerun unless row labels are supplied; carry the applied transition rule into P135/P122."
        : $"dotnet run --project {Phase130Project}", p141Ready && appliedIntakeReady),
    Step(3, "rerun-readiness-gate", $"dotnet run --project {Phase135Project}", false),
    Step(4, "rerun-corrected-wz-sweep", $"dotnet run --project {Phase122Project}", false),
    Step(5, "refresh-prediction-package", $"dotnet run --project {Phase101Project}", false),
};

var blockers = new List<string>();
if (!p141Ready)
    blockers.Add("P141 intake application is not promotable");
if (!appliedIntakeReady)
    blockers.Add("P141 has neither an applied sector-label table nor an applied transition rule ready");
if (!previousSectorGateReady && !transitionRuleReady)
    blockers.Add("existing P130 sector-label gate remains blocked before applying intake labels");
if (!previousSweepReady)
    blockers.Add("existing P135 corrected W/Z sweep readiness gate remains blocked before applying intake labels");

var result = new
{
    phaseId = "phase142-post-intake-rerun-plan-gate",
    terminalStatus,
    postIntakeRerunPlanMaterialized = true,
    rerunPlanExecutable,
    p141Ready,
    appliedTableReady,
    transitionRuleReady,
    appliedIntakeReady,
    previousSectorGateReady,
    previousSweepReady,
    rerunSteps,
    blockers,
    closureRequirements = new[]
    {
        "make P141 application-ready by filling and validating P140 intake",
        "run the rerun plan only after P141 applied sector-label table is ready",
        "refresh Phase101 after sector-label and corrected W/Z sweep reruns complete",
    },
    sourceEvidence = new
    {
        phase130Path = Phase130Path,
        phase135Path = Phase135Path,
        phase141Path = Phase141Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "post_intake_rerun_plan_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "post_intake_rerun_plan_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.postIntakeRerunPlanMaterialized,
        result.rerunPlanExecutable,
        result.p141Ready,
        result.appliedTableReady,
        result.transitionRuleReady,
        result.appliedIntakeReady,
        result.rerunSteps,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rerunPlanExecutable={rerunPlanExecutable}");
Console.WriteLine($"p141Ready={p141Ready}");
Console.WriteLine($"appliedTableReady={appliedTableReady}");
Console.WriteLine($"transitionRuleReady={transitionRuleReady}");

static RerunStep Step(int order, string stepId, string action, bool ready) =>
    new(order, stepId, action, ready ? "ready" : "blocked");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record RerunStep(int Order, string StepId, string Action, string Status);
