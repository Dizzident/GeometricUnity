using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase155_fermion_sector_transition_evidence_derivation_001/output";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase138Path = "studies/phase138_repaired_row_coupling_transition_graph_001/output/repaired_row_coupling_transition_graph.json";
const string Phase139Path = "studies/phase139_fermion_sector_label_route_closure_001/output/fermion_sector_label_route_closure.json";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";
const string Phase142Path = "studies/phase142_post_intake_rerun_plan_gate_001/output/post_intake_rerun_plan_gate.json";
const string Phase146Path = "studies/phase146_fermion_sector_evidence_census_001/output/fermion_sector_evidence_census.json";
const string Phase154Path = "studies/phase154_wz_transition_bridge_root_cause_audit_001/output/wz_transition_bridge_root_cause_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE155_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase138 = JsonDocument.Parse(File.ReadAllText(Phase138Path));
using var phase139 = JsonDocument.Parse(File.ReadAllText(Phase139Path));
using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase141 = JsonDocument.Parse(File.ReadAllText(Phase141Path));
using var phase142 = JsonDocument.Parse(File.ReadAllText(Phase142Path));
using var phase146 = JsonDocument.Parse(File.ReadAllText(Phase146Path));
using var phase154 = JsonDocument.Parse(File.ReadAllText(Phase154Path));

bool projectionCandidateFound = !IsNull(phase122.RootElement, "projectionCandidate");
bool transitionGraphPromotable = JsonBool(phase138.RootElement, "transitionGraphPromotable") is true;
bool routeClosureReady = JsonBool(phase139.RootElement, "closureReady") is true;
bool intakePromotable = JsonBool(phase140.RootElement, "intakeArtifactPromotable") is true;
bool applicationPromotable = JsonBool(phase141.RootElement, "intakeApplicationPromotable") is true;
bool transitionRuleReady = JsonBool(phase141.RootElement, "transitionRuleApplicationReady") is true;
bool rerunExecutable = JsonBool(phase142.RootElement, "rerunPlanExecutable") is true;
bool censusEvidencePresent = JsonBool(phase146.RootElement, "currentEvidencePresent") is true;

var evidenceAssessments = new[]
{
    Assessment("phase122-corrected-operator-projection-candidate", projectionCandidateFound, Phase122Path,
        projectionCandidateFound ? "Phase122 already has a projection candidate." : "Phase122 projectionCandidate is null."),
    Assessment("phase138-transition-graph", transitionGraphPromotable, Phase138Path,
        transitionGraphPromotable ? "P138 transition graph is promotable." : "P138 transition graph is diagnostic-only."),
    Assessment("phase139-route-closure", routeClosureReady, Phase139Path,
        routeClosureReady ? "P139 route closure is ready." : "P139 requires new target-independent input."),
    Assessment("phase140-intake-artifact", intakePromotable, Phase140Path,
        intakePromotable ? "P140 intake artifact is promotable." : "P140 awaits a valid real artifact."),
    Assessment("phase141-intake-application", applicationPromotable || transitionRuleReady, Phase141Path,
        applicationPromotable || transitionRuleReady ? "P141 application can proceed." : "P141 application is blocked."),
    Assessment("phase142-post-intake-rerun", rerunExecutable, Phase142Path,
        rerunExecutable ? "P142 rerun plan is executable." : "P142 rerun plan is blocked."),
    Assessment("phase146-local-evidence-census", censusEvidencePresent, Phase146Path,
        censusEvidencePresent ? "P146 found current local evidence." : "P146 found no non-synthetic local evidence candidate."),
};

bool anyPromotableEvidence = evidenceAssessments.Any(assessment => assessment.Promotable);
string terminalStatus = projectionCandidateFound
    ? "fermion-sector-transition-evidence-derived-projection-ready"
    : anyPromotableEvidence
        ? "fermion-sector-transition-evidence-available-rerun-required"
        : "fermion-sector-transition-evidence-derivation-blocked";

var closureRequirements = terminalStatus switch
{
    "fermion-sector-transition-evidence-derived-projection-ready" => new[]
    {
        "rerun Phase116 with the Phase122 projection candidate",
        "rerun Phase150 and Phase151",
    },
    "fermion-sector-transition-evidence-available-rerun-required" => new[]
    {
        "apply available evidence through P140-P142 if not already applied",
        "rerun Phase122",
        "rerun Phase116 only if Phase122 emits a projection candidate",
    },
    _ => new[]
    {
        "derive or supply a target-independent fermion-sector label table, transition rule, or bridge-revision artifact",
        "use the P140 intake contract; do not infer labels from W/Z target residuals",
        "rerun P140-P146 and require P122 projectionCandidate to become non-null",
        "rerun Phase116, Phase150, and Phase151 only after P122 passes",
    },
};

var result = new
{
    phaseId = "phase155-fermion-sector-transition-evidence-derivation",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    requestedByPhase154Status = JsonString(phase154.RootElement, "terminalStatus"),
    anyPromotableEvidence,
    projectionCandidateFound,
    evidenceAssessments,
    preservedIntakeContract = new
    {
        contractPath = Phase140Path,
        templatePath = JsonString(phase140.RootElement, "templatePath"),
        intakeArtifactPath = JsonString(phase140.RootElement, "intakeArtifactPath"),
    },
    closureRequirements,
    finalKnownBlocker = terminalStatus.EndsWith("-blocked", StringComparison.Ordinal)
        ? "No current local artifact derives the target-independent fermion-sector transition or bridge evidence required to make W/Z absolute masses promotable."
        : null,
    sourceEvidence = new
    {
        phase122Path = Phase122Path,
        phase138Path = Phase138Path,
        phase139Path = Phase139Path,
        phase140Path = Phase140Path,
        phase141Path = Phase141Path,
        phase142Path = Phase142Path,
        phase146Path = Phase146Path,
        phase154Path = Phase154Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "fermion_sector_transition_evidence_derivation.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_transition_evidence_derivation_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.anyPromotableEvidence,
        result.projectionCandidateFound,
        result.evidenceAssessments,
        result.preservedIntakeContract,
        result.closureRequirements,
        result.finalKnownBlocker,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_transition_evidence_derivation.md"),
    BuildMarkdown(terminalStatus, evidenceAssessments, closureRequirements, result.finalKnownBlocker));

Console.WriteLine(terminalStatus);
Console.WriteLine($"anyPromotableEvidence={anyPromotableEvidence}");
Console.WriteLine($"projectionCandidateFound={projectionCandidateFound}");
Console.WriteLine($"finalKnownBlocker={result.finalKnownBlocker}");

static EvidenceAssessment Assessment(string id, bool promotable, string path, string diagnosis) =>
    new(id, promotable, path, diagnosis);

static string BuildMarkdown(
    string terminalStatus,
    IReadOnlyList<EvidenceAssessment> assessments,
    IReadOnlyList<string> closureRequirements,
    string? finalKnownBlocker)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Fermion Sector Transition Evidence Derivation");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    if (!string.IsNullOrWhiteSpace(finalKnownBlocker))
    {
        builder.AppendLine();
        builder.AppendLine($"Final known blocker: {finalKnownBlocker}");
    }

    builder.AppendLine();
    builder.AppendLine("| Evidence | Promotable | Diagnosis |");
    builder.AppendLine("|---|---|---|");
    foreach (var assessment in assessments)
        builder.AppendLine($"| {assessment.EvidenceId} | {(assessment.Promotable ? "yes" : "no")} | {assessment.Diagnosis.Replace("|", "\\|", StringComparison.Ordinal)} |");

    builder.AppendLine();
    builder.AppendLine("## Closure Requirements");
    foreach (var requirement in closureRequirements)
        builder.AppendLine($"- {requirement}");

    return builder.ToString();
}

static bool IsNull(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record EvidenceAssessment(
    string EvidenceId,
    bool Promotable,
    string Path,
    string Diagnosis);
