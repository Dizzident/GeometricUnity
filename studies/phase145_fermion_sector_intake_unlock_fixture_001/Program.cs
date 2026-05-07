using System.Text.Json;

const string DefaultOutputDir = "studies/phase145_fermion_sector_intake_unlock_fixture_001/output";
const string Phase139Path = "studies/phase139_fermion_sector_label_route_closure_001/output/fermion_sector_label_route_closure.json";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";
const string Phase142Path = "studies/phase142_post_intake_rerun_plan_gate_001/output/post_intake_rerun_plan_gate.json";
const string Phase144Path = "studies/phase144_fermion_sector_intake_persistence_gate_001/output/fermion_sector_intake_persistence_gate.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE145_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase139 = JsonDocument.Parse(File.ReadAllText(Phase139Path));
using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase141 = JsonDocument.Parse(File.ReadAllText(Phase141Path));
using var phase142 = JsonDocument.Parse(File.ReadAllText(Phase142Path));
using var phase144 = JsonDocument.Parse(File.ReadAllText(Phase144Path));

var targetRows = phase139.RootElement.GetProperty("targetRows").EnumerateArray()
    .Select(row => new TargetRow(
        RequiredString(row, "familyId"),
        RequiredString(row, "candidateId"),
        RequiredString(row, "sourceCanonicalFermionModeId")))
    .ToArray();

var rowLabelFixture = targetRows.Select(row => new FixtureRow(
    row.FamilyId,
    row.CandidateId,
    row.SourceCanonicalFermionModeId,
    ChargeSector: "synthetic-fixture-charge-sector",
    WeakSector: "synthetic-fixture-weak-sector",
    DerivationId: "phase145-synthetic-target-blind-contract-fixture",
    ExternalTargetValuesUsed: false)).ToArray();
var transitionRuleFixture = new TransitionRuleFixture(
    RuleId: "phase145-synthetic-transition-rule-fixture",
    RuleKind: "synthetic-directed-fermion-sector-transition-rule",
    DerivationId: "phase145-synthetic-target-blind-contract-fixture",
    ExternalTargetValuesUsed: false,
    DirectedTransitionCount: Math.Max(1, targetRows.Length - 1));

var rowLabelAudit = AuditRowLabelFixture(targetRows, rowLabelFixture);
var transitionRuleAudit = AuditTransitionRuleFixture(transitionRuleFixture);
bool fixtureUnlocksP140 = rowLabelAudit.P140Promotable || transitionRuleAudit.P140Promotable;
bool fixtureUnlocksP141 = rowLabelAudit.P141ApplicationReady || transitionRuleAudit.P141ApplicationReady;
bool fixtureUnlocksP142 = fixtureUnlocksP141;
bool currentEvidencePresent = JsonBool(phase144.RootElement, "evidencePresent") is true;
string terminalStatus = fixtureUnlocksP140 && fixtureUnlocksP141 && fixtureUnlocksP142
    ? "fermion-sector-intake-unlock-fixture-validated-real-evidence-required"
    : "fermion-sector-intake-unlock-fixture-blocked";

var result = new
{
    phaseId = "phase145-fermion-sector-intake-unlock-fixture",
    terminalStatus,
    syntheticFixtureOnly = true,
    promotesPhysicalEvidence = false,
    currentEvidencePresent,
    fixtureUnlocksP140,
    fixtureUnlocksP141,
    fixtureUnlocksP142,
    rowLabelAudit,
    transitionRuleAudit,
    currentGateStatuses = new
    {
        phase140 = JsonString(phase140.RootElement, "terminalStatus"),
        phase141 = JsonString(phase141.RootElement, "terminalStatus"),
        phase142 = JsonString(phase142.RootElement, "terminalStatus"),
        phase144 = JsonString(phase144.RootElement, "terminalStatus"),
    },
    fixtureArtifacts = new
    {
        rowLabelFixturePath = Path.Combine(outputDir, "synthetic_row_label_fixture.json"),
        transitionRuleFixturePath = Path.Combine(outputDir, "synthetic_transition_rule_fixture.json"),
    },
    blockers = currentEvidencePresent
        ? Array.Empty<string>()
        : new[] { "real target-blind fermion-sector evidence is still absent; synthetic fixtures are contract validation only" },
    nextRealEvidenceRerunChain = new[]
    {
        "dotnet run --project studies/phase140_fermion_sector_artifact_intake_contract_001/Phase140FermionSectorArtifactIntakeContract.csproj",
        "dotnet run --project studies/phase141_fermion_sector_intake_application_gate_001/Phase141FermionSectorIntakeApplicationGate.csproj",
        "dotnet run --project studies/phase142_post_intake_rerun_plan_gate_001/Phase142PostIntakeRerunPlanGate.csproj",
        "dotnet run --project studies/phase143_fermion_sector_evidence_request_package_001/Phase143FermionSectorEvidenceRequestPackage.csproj",
        "dotnet run --project studies/phase144_fermion_sector_intake_persistence_gate_001/Phase144FermionSectorIntakePersistenceGate.csproj",
        "dotnet run --project studies/phase145_fermion_sector_intake_unlock_fixture_001/Phase145FermionSectorIntakeUnlockFixture.csproj",
        "dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj",
    },
    sourceEvidence = new
    {
        phase139Path = Phase139Path,
        phase140Path = Phase140Path,
        phase141Path = Phase141Path,
        phase142Path = Phase142Path,
        phase144Path = Phase144Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "synthetic_row_label_fixture.json"),
    JsonSerializer.Serialize(new
    {
        artifactId = "phase145-synthetic-row-label-fixture",
        syntheticFixtureOnly = true,
        rows = rowLabelFixture,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "synthetic_transition_rule_fixture.json"),
    JsonSerializer.Serialize(new
    {
        artifactId = "phase145-synthetic-transition-rule-fixture",
        syntheticFixtureOnly = true,
        transitionRule = transitionRuleFixture,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_intake_unlock_fixture.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_intake_unlock_fixture_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.syntheticFixtureOnly,
        result.promotesPhysicalEvidence,
        result.currentEvidencePresent,
        result.fixtureUnlocksP140,
        result.fixtureUnlocksP141,
        result.fixtureUnlocksP142,
        result.rowLabelAudit,
        result.transitionRuleAudit,
        result.currentGateStatuses,
        result.blockers,
        result.nextRealEvidenceRerunChain,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fixtureUnlocksP140={fixtureUnlocksP140}");
Console.WriteLine($"fixtureUnlocksP141={fixtureUnlocksP141}");
Console.WriteLine($"fixtureUnlocksP142={fixtureUnlocksP142}");

static RowLabelFixtureAudit AuditRowLabelFixture(IReadOnlyList<TargetRow> targets, IReadOnlyList<FixtureRow> rows)
{
    var rowAudits = targets.Select(target =>
    {
        var row = rows.FirstOrDefault(candidate =>
            candidate.FamilyId == target.FamilyId
            && candidate.CandidateId == target.CandidateId
            && candidate.SourceCanonicalFermionModeId == target.SourceCanonicalFermionModeId);
        bool matched = row is not null;
        bool complete = matched
            && !string.IsNullOrWhiteSpace(row!.ChargeSector)
            && !string.IsNullOrWhiteSpace(row.WeakSector)
            && !string.IsNullOrWhiteSpace(row.DerivationId)
            && row.ExternalTargetValuesUsed is false;
        return new FixtureRowAudit(target.FamilyId, target.CandidateId, target.SourceCanonicalFermionModeId, matched, complete);
    }).ToArray();

    bool ready = rowAudits.Length > 0 && rowAudits.All(row => row.Matched && row.Complete);
    return new RowLabelFixtureAudit(
        TargetRowCount: targets.Count,
        FixtureRowCount: rows.Count,
        CompleteRowCount: rowAudits.Count(row => row.Complete),
        P140Promotable: ready,
        P141ApplicationReady: ready,
        RowAudits: rowAudits);
}

static TransitionRuleFixtureAudit AuditTransitionRuleFixture(TransitionRuleFixture fixture)
{
    bool complete = !string.IsNullOrWhiteSpace(fixture.RuleId)
        && !string.IsNullOrWhiteSpace(fixture.RuleKind)
        && !string.IsNullOrWhiteSpace(fixture.DerivationId)
        && fixture.ExternalTargetValuesUsed is false
        && fixture.DirectedTransitionCount > 0;
    return new TransitionRuleFixtureAudit(
        Complete: complete,
        P140Promotable: complete,
        P141ApplicationReady: complete,
        fixture.DirectedTransitionCount);
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record TargetRow(string FamilyId, string CandidateId, string SourceCanonicalFermionModeId);
sealed record FixtureRow(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    string ChargeSector,
    string WeakSector,
    string DerivationId,
    bool ExternalTargetValuesUsed);
sealed record TransitionRuleFixture(
    string RuleId,
    string RuleKind,
    string DerivationId,
    bool ExternalTargetValuesUsed,
    int DirectedTransitionCount);
sealed record FixtureRowAudit(string FamilyId, string CandidateId, string SourceCanonicalFermionModeId, bool Matched, bool Complete);
sealed record RowLabelFixtureAudit(
    int TargetRowCount,
    int FixtureRowCount,
    int CompleteRowCount,
    bool P140Promotable,
    bool P141ApplicationReady,
    IReadOnlyList<FixtureRowAudit> RowAudits);
sealed record TransitionRuleFixtureAudit(bool Complete, bool P140Promotable, bool P141ApplicationReady, int DirectedTransitionCount);
