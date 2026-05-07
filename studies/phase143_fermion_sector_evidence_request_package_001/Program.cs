using System.Text.Json;

const string DefaultOutputDir = "studies/phase143_fermion_sector_evidence_request_package_001/output";
const string Phase139Path = "studies/phase139_fermion_sector_label_route_closure_001/output/fermion_sector_label_route_closure.json";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";
const string Phase142Path = "studies/phase142_post_intake_rerun_plan_gate_001/output/post_intake_rerun_plan_gate.json";
const string IntakeTemplatePath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE143_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase139 = JsonDocument.Parse(File.ReadAllText(Phase139Path));
using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase141 = JsonDocument.Parse(File.ReadAllText(Phase141Path));
using var phase142 = JsonDocument.Parse(File.ReadAllText(Phase142Path));

var targetRows = phase139.RootElement.GetProperty("targetRows").EnumerateArray()
    .Select(row => new
    {
        familyId = RequiredString(row, "familyId"),
        candidateId = RequiredString(row, "candidateId"),
        sourceCanonicalFermionModeId = RequiredString(row, "sourceCanonicalFermionModeId"),
        requiredFields = new[] { "chargeSector", "weakSector or quantumNumbers", "derivationId", "externalTargetValuesUsed=false" },
    })
    .ToArray();
bool intakeReady = JsonBool(phase140.RootElement, "intakeArtifactPromotable") is true;
bool applicationReady = JsonBool(phase141.RootElement, "intakeApplicationPromotable") is true;
bool rerunReady = JsonBool(phase142.RootElement, "rerunPlanExecutable") is true;
string terminalStatus = intakeReady && applicationReady && rerunReady
    ? "fermion-sector-evidence-request-satisfied"
    : "fermion-sector-evidence-request-built";

var request = new
{
    phaseId = "phase143-fermion-sector-evidence-request-package",
    terminalStatus,
    evidenceRequestMaterialized = true,
    requestStatus = intakeReady && applicationReady ? "satisfied" : "awaiting-new-evidence",
    intakeTemplatePath = IntakeTemplatePath,
    targetRows,
    acceptableArtifactKinds = phase139.RootElement
        .GetProperty("requiredNewArtifactContract")
        .GetProperty("acceptableArtifactKinds")
        .Clone(),
    rejectedShortcuts = phase139.RootElement
        .GetProperty("requiredNewArtifactContract")
        .GetProperty("rejectedShortcuts")
        .Clone(),
    currentGateStatuses = new
    {
        phase139 = JsonString(phase139.RootElement, "terminalStatus"),
        phase140 = JsonString(phase140.RootElement, "terminalStatus"),
        phase141 = JsonString(phase141.RootElement, "terminalStatus"),
        phase142 = JsonString(phase142.RootElement, "terminalStatus"),
    },
    requiredNextCommandsAfterEvidence = new[]
    {
        "dotnet run --project studies/phase140_fermion_sector_artifact_intake_contract_001/Phase140FermionSectorArtifactIntakeContract.csproj",
        "dotnet run --project studies/phase141_fermion_sector_intake_application_gate_001/Phase141FermionSectorIntakeApplicationGate.csproj",
        "dotnet run --project studies/phase142_post_intake_rerun_plan_gate_001/Phase142PostIntakeRerunPlanGate.csproj",
        "dotnet run --project studies/phase144_fermion_sector_intake_persistence_gate_001/Phase144FermionSectorIntakePersistenceGate.csproj",
        "dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj",
    },
    blockers = new[]
    {
        "no target-blind fermion-specific sector-label derivation is present",
        "P140 intake template remains unfilled",
        "P141 cannot apply repaired labels until P140 is valid",
        "P142 rerun plan cannot execute until P141 is ready",
    },
    sourceEvidence = new
    {
        phase139Path = Phase139Path,
        phase140Path = Phase140Path,
        phase141Path = Phase141Path,
        phase142Path = Phase142Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_evidence_request_package.json"),
    JsonSerializer.Serialize(request, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_evidence_request_package_summary.json"),
    JsonSerializer.Serialize(new
    {
        request.phaseId,
        request.terminalStatus,
        request.evidenceRequestMaterialized,
        request.requestStatus,
        request.intakeTemplatePath,
        request.targetRows,
        request.currentGateStatuses,
        request.requiredNextCommandsAfterEvidence,
        request.blockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"requestStatus={request.requestStatus}");
Console.WriteLine($"intakeTemplatePath={IntakeTemplatePath}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
