using System.Security.Cryptography;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase144_fermion_sector_intake_persistence_gate_001/output";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";
const string Phase143Path = "studies/phase143_fermion_sector_evidence_request_package_001/output/fermion_sector_evidence_request_package.json";
const string IntakeTemplatePath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE144_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase141 = JsonDocument.Parse(File.ReadAllText(Phase141Path));
using var phase143 = JsonDocument.Parse(File.ReadAllText(Phase143Path));
using var intake = JsonDocument.Parse(File.ReadAllText(IntakeTemplatePath));

var intakeBytes = File.ReadAllBytes(IntakeTemplatePath);
var intakeSha256 = Convert.ToHexString(SHA256.HashData(intakeBytes)).ToLowerInvariant();
var rowAudits = intake.RootElement.GetProperty("rows").EnumerateArray()
    .Select(row => new RowAudit(
        FamilyId: RequiredString(row, "familyId"),
        CandidateId: RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId: RequiredString(row, "sourceCanonicalFermionModeId"),
        HasChargeSector: !string.IsNullOrWhiteSpace(JsonString(row, "chargeSector")),
        HasWeakSectorOrQuantumNumbers: !string.IsNullOrWhiteSpace(JsonString(row, "weakSector")) || HasObjectOrArray(row, "quantumNumbers"),
        HasDerivationId: !string.IsNullOrWhiteSpace(JsonString(row, "derivationId")),
        ExternalTargetValuesUsedFalse: JsonBool(row, "externalTargetValuesUsed") is false))
    .ToArray();

var transitionAudit = AuditTransitionRule(intake.RootElement);
bool p140PreservesExistingArtifact = JsonBool(phase140.RootElement, "existingArtifactProvided") is true;
bool p140Valid = JsonBool(phase140.RootElement, "intakeArtifactPromotable") is true;
bool p141Ready = JsonBool(phase141.RootElement, "intakeApplicationPromotable") is true;
bool rowsReady = rowAudits.Length > 0 && rowAudits.All(row => row.Complete);
bool transitionReady = transitionAudit.Complete;
bool evidencePresent = rowsReady || transitionReady;

string terminalStatus = p140Valid && p141Ready
    ? "fermion-sector-intake-persistence-ready"
    : p140PreservesExistingArtifact
        ? "fermion-sector-intake-persistence-awaiting-evidence"
        : "fermion-sector-intake-persistence-template-not-yet-preserved";

var blockers = new List<string>();
if (!p140PreservesExistingArtifact)
    blockers.Add("rerun P140 after the intake template exists so the preservation path is observed");
if (!evidencePresent)
    blockers.Add("current intake artifact does not contain complete row labels or a nontrivial transition rule");
if (!p140Valid)
    blockers.Add("P140 has not validated a promotable intake artifact");
if (!p141Ready)
    blockers.Add("P141 has not applied promotable fermion-sector labels");

var result = new
{
    phaseId = "phase144-fermion-sector-intake-persistence-gate",
    terminalStatus,
    intakePersistenceGateMaterialized = true,
    p140PreservesExistingArtifact,
    p140Valid,
    p141Ready,
    evidencePresent,
    rowsReady,
    transitionReady,
    intakeTemplatePath = IntakeTemplatePath,
    intakeSha256,
    rowAudits,
    transitionAudit,
    currentGateStatuses = new
    {
        phase140 = JsonString(phase140.RootElement, "terminalStatus"),
        phase141 = JsonString(phase141.RootElement, "terminalStatus"),
        phase143 = JsonString(phase143.RootElement, "terminalStatus"),
    },
    rerunChainAfterEvidence = new[]
    {
        "dotnet run --project studies/phase140_fermion_sector_artifact_intake_contract_001/Phase140FermionSectorArtifactIntakeContract.csproj",
        "dotnet run --project studies/phase141_fermion_sector_intake_application_gate_001/Phase141FermionSectorIntakeApplicationGate.csproj",
        "dotnet run --project studies/phase142_post_intake_rerun_plan_gate_001/Phase142PostIntakeRerunPlanGate.csproj",
        "dotnet run --project studies/phase143_fermion_sector_evidence_request_package_001/Phase143FermionSectorEvidenceRequestPackage.csproj",
        "dotnet run --project studies/phase144_fermion_sector_intake_persistence_gate_001/Phase144FermionSectorIntakePersistenceGate.csproj",
        "dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj",
    },
    blockers,
    sourceEvidence = new
    {
        phase140Path = Phase140Path,
        phase141Path = Phase141Path,
        phase143Path = Phase143Path,
        intakeTemplatePath = IntakeTemplatePath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_intake_persistence_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_intake_persistence_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.intakePersistenceGateMaterialized,
        result.p140PreservesExistingArtifact,
        result.evidencePresent,
        result.rowsReady,
        result.transitionReady,
        result.intakeTemplatePath,
        result.intakeSha256,
        result.rowAudits,
        result.transitionAudit,
        result.currentGateStatuses,
        result.rerunChainAfterEvidence,
        result.blockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"intakeSha256={intakeSha256}");
Console.WriteLine($"evidencePresent={evidencePresent}");

static TransitionAudit AuditTransitionRule(JsonElement artifact)
{
    if (!artifact.TryGetProperty("transitionRule", out var rule) || rule.ValueKind != JsonValueKind.Object)
        return new TransitionAudit(false, false, false, false, false, false);

    bool hasRuleId = !string.IsNullOrWhiteSpace(JsonString(rule, "ruleId"));
    bool hasRuleKind = !string.IsNullOrWhiteSpace(JsonString(rule, "ruleKind"));
    bool hasDerivationId = !string.IsNullOrWhiteSpace(JsonString(rule, "derivationId"));
    bool externalTargetValuesUsedFalse = JsonBool(rule, "externalTargetValuesUsed") is false;
    bool hasDirectedTransitions = rule.TryGetProperty("directedTransitions", out var transitions)
        && transitions.ValueKind == JsonValueKind.Array
        && transitions.GetArrayLength() > 0;
    return new TransitionAudit(
        Complete: hasRuleId && hasRuleKind && hasDerivationId && externalTargetValuesUsedFalse && hasDirectedTransitions,
        HasRuleId: hasRuleId,
        HasRuleKind: hasRuleKind,
        HasDerivationId: hasDerivationId,
        ExternalTargetValuesUsedFalse: externalTargetValuesUsedFalse,
        HasDirectedTransitions: hasDirectedTransitions);
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static bool HasObjectOrArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.Object or JsonValueKind.Array;

sealed record RowAudit(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    bool HasChargeSector,
    bool HasWeakSectorOrQuantumNumbers,
    bool HasDerivationId,
    bool ExternalTargetValuesUsedFalse)
{
    public bool Complete => HasChargeSector && HasWeakSectorOrQuantumNumbers && HasDerivationId && ExternalTargetValuesUsedFalse;
}

sealed record TransitionAudit(
    bool Complete,
    bool HasRuleId,
    bool HasRuleKind,
    bool HasDerivationId,
    bool ExternalTargetValuesUsedFalse,
    bool HasDirectedTransitions);
