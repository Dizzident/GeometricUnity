using System.Text.Json;

const string DefaultOutputDir = "studies/phase140_fermion_sector_artifact_intake_contract_001/output";
const string Phase139Path = "studies/phase139_fermion_sector_label_route_closure_001/output/fermion_sector_label_route_closure.json";
const string IntakeArtifactName = "fermion_sector_or_transition_rule_intake_template.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE140_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase139 = JsonDocument.Parse(File.ReadAllText(Phase139Path));
var targetRows = phase139.RootElement.GetProperty("targetRows").EnumerateArray()
    .Select(row => new IntakeRow(
        FamilyId: RequiredString(row, "familyId"),
        CandidateId: RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId: RequiredString(row, "sourceCanonicalFermionModeId"),
        ChargeSector: null,
        WeakSector: null,
        QuantumNumbers: null,
        DerivationId: null,
        ExternalTargetValuesUsed: null))
    .ToArray();
var templatePath = Path.Combine(outputDir, IntakeArtifactName);
bool existingArtifactProvided = File.Exists(templatePath);

var intakeTemplate = new
{
    artifactId = "phase140-fermion-sector-or-transition-rule-intake-template",
    schemaVersion = "1.0.0",
    artifactKind = "fermion-sector-label-table",
    status = "template-unfilled",
    targetContractId = "phase139-fermion-sector-label-or-transition-rule-contract-v1",
    rows = targetRows,
    transitionRule = new
    {
        ruleId = (string?)null,
        ruleKind = (string?)null,
        derivationId = (string?)null,
        externalTargetValuesUsed = (bool?)null,
        directedTransitions = Array.Empty<object>(),
    },
    instructions = new[]
    {
        "Populate chargeSector and weakSector or quantumNumbers for every row, or populate a nontrivial transitionRule.",
        "derivationId must identify a target-blind fermion-specific derivation.",
        "externalTargetValuesUsed must be false.",
        "Do not use rejected shortcuts from P139.",
    },
};

if (!existingArtifactProvided)
    File.WriteAllText(templatePath, JsonSerializer.Serialize(intakeTemplate, jsonOptions));

using var intakeArtifact = JsonDocument.Parse(File.ReadAllText(templatePath));
var validation = ValidateArtifact(intakeArtifact.RootElement, targetRows);
string terminalStatus = validation.Promotable
    ? "fermion-sector-artifact-intake-valid"
    : "fermion-sector-artifact-intake-awaiting-valid-artifact";

var result = new
{
    phaseId = "phase140-fermion-sector-artifact-intake-contract",
    terminalStatus,
    intakeContractMaterialized = true,
    existingArtifactProvided,
    intakeArtifactPromotable = validation.Promotable,
    templatePath,
    targetContract = phase139.RootElement.GetProperty("requiredNewArtifactContract").Clone(),
    targetRows,
    validation,
    blockers = validation.Blockers,
    closureRequirements = new[]
    {
        "fill the intake template with a target-blind fermion-specific sector-label table or transition rule",
        "include derivationId and externalTargetValuesUsed=false for every promoted assignment",
        "rerun P140, then rerun P130/P135/P122 only after P140 reports a valid intake artifact",
    },
    sourceEvidence = new
    {
        phase139Path = Phase139Path,
    },
};

File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_artifact_intake_contract.json"),
    JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_artifact_intake_contract_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.intakeContractMaterialized,
        result.existingArtifactProvided,
        result.intakeArtifactPromotable,
        result.templatePath,
        result.validation,
        result.blockers,
        result.closureRequirements,
    }, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"templatePath={templatePath}");
Console.WriteLine($"intakeArtifactPromotable={validation.Promotable}");

static IntakeValidation ValidateArtifact(JsonElement artifact, IReadOnlyList<IntakeRow> targetRows)
{
    var blockers = new List<string>();
    var rows = artifact.TryGetProperty("rows", out var rowsElement) && rowsElement.ValueKind == JsonValueKind.Array
        ? rowsElement.EnumerateArray().Select(ReadRow).ToArray()
        : Array.Empty<IntakeRow>();
    bool transitionRulePromotable = IsTransitionRulePromotable(artifact);

    if (targetRows.Count == 0)
        blockers.Add("intake template has no target rows");
    if (rows.Length == 0)
        blockers.Add("intake artifact has no rows");

    var rowAudits = targetRows.Select(target =>
    {
        var row = rows.FirstOrDefault(candidate =>
            candidate.FamilyId == target.FamilyId
            && candidate.CandidateId == target.CandidateId
            && candidate.SourceCanonicalFermionModeId == target.SourceCanonicalFermionModeId);
        bool matched = row is not null;
        bool complete = matched
            && !string.IsNullOrWhiteSpace(row!.ChargeSector)
            && (!string.IsNullOrWhiteSpace(row.WeakSector) || row.QuantumNumbers is not null)
            && !string.IsNullOrWhiteSpace(row.DerivationId)
            && row.ExternalTargetValuesUsed is false;

        return new IntakeRowAudit(
            target.FamilyId,
            target.CandidateId,
            target.SourceCanonicalFermionModeId,
            matched,
            complete,
            matched ? row!.ChargeSector : null,
            matched ? row!.WeakSector : null,
            matched ? row!.QuantumNumbers : null,
            matched ? row!.DerivationId : null,
            matched ? row!.ExternalTargetValuesUsed : null);
    }).ToArray();

    bool rowsPromotable = rowAudits.Length > 0 && rowAudits.All(a => a.Matched && a.Complete);
    if (!rowsPromotable && !transitionRulePromotable)
    {
        if (rowAudits.Any(a => !a.Matched))
            blockers.Add("every target row must have a matching intake row");
        if (rowAudits.Any(a => string.IsNullOrWhiteSpace(a.ChargeSector)))
            blockers.Add("every target row must include chargeSector");
        if (rowAudits.Any(a => string.IsNullOrWhiteSpace(a.WeakSector) && a.QuantumNumbers is null))
            blockers.Add("every target row must include weakSector or quantumNumbers");
        if (rowAudits.Any(a => string.IsNullOrWhiteSpace(a.DerivationId)))
            blockers.Add("every target row must include derivationId");
        if (rowAudits.Any(a => a.ExternalTargetValuesUsed is not false))
            blockers.Add("externalTargetValuesUsed must be false for every target row");
        blockers.Add("or provide a nontrivial transitionRule with derivationId and externalTargetValuesUsed=false");
    }

    return new IntakeValidation(
        Promotable: rowsPromotable || transitionRulePromotable,
        RowCount: rows.Length,
        TargetRowCount: targetRows.Count,
        CompleteRowCount: rowAudits.Count(r => r.Complete),
        RowsPromotable: rowsPromotable,
        TransitionRulePromotable: transitionRulePromotable,
        RowAudits: rowAudits,
        Blockers: blockers);
}

static IntakeRow ReadRow(JsonElement row) => new(
    FamilyId: RequiredString(row, "familyId"),
    CandidateId: RequiredString(row, "candidateId"),
    SourceCanonicalFermionModeId: RequiredString(row, "sourceCanonicalFermionModeId"),
    ChargeSector: JsonString(row, "chargeSector"),
    WeakSector: JsonString(row, "weakSector"),
    QuantumNumbers: row.TryGetProperty("quantumNumbers", out var quantumNumbers)
        && quantumNumbers.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined
        ? quantumNumbers.Clone()
        : null,
    DerivationId: JsonString(row, "derivationId"),
    ExternalTargetValuesUsed: JsonBool(row, "externalTargetValuesUsed"));

static bool IsTransitionRulePromotable(JsonElement artifact)
{
    if (!artifact.TryGetProperty("transitionRule", out var rule) || rule.ValueKind != JsonValueKind.Object)
        return false;

    bool hasRuleId = !string.IsNullOrWhiteSpace(JsonString(rule, "ruleId"));
    bool hasRuleKind = !string.IsNullOrWhiteSpace(JsonString(rule, "ruleKind"));
    bool hasDerivationId = !string.IsNullOrWhiteSpace(JsonString(rule, "derivationId"));
    bool noExternalTargets = JsonBool(rule, "externalTargetValuesUsed") is false;
    bool hasDirectedTransitions = rule.TryGetProperty("directedTransitions", out var transitions)
        && transitions.ValueKind == JsonValueKind.Array
        && transitions.GetArrayLength() > 0;
    return hasRuleId && hasRuleKind && hasDerivationId && noExternalTargets && hasDirectedTransitions;
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record IntakeRow(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    string? ChargeSector,
    string? WeakSector,
    JsonElement? QuantumNumbers,
    string? DerivationId,
    bool? ExternalTargetValuesUsed);

sealed record IntakeValidation(
    bool Promotable,
    int RowCount,
    int TargetRowCount,
    int CompleteRowCount,
    bool RowsPromotable,
    bool TransitionRulePromotable,
    IReadOnlyList<IntakeRowAudit> RowAudits,
    IReadOnlyList<string> Blockers);

sealed record IntakeRowAudit(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    bool Matched,
    bool Complete,
    string? ChargeSector,
    string? WeakSector,
    JsonElement? QuantumNumbers,
    string? DerivationId,
    bool? ExternalTargetValuesUsed);
