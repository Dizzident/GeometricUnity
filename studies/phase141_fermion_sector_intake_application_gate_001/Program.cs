using System.Text.Json;

const string DefaultOutputDir = "studies/phase141_fermion_sector_intake_application_gate_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase140TemplatePath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE141_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var intake = JsonDocument.Parse(File.ReadAllText(Phase140TemplatePath));

var labelRows = phase131.RootElement.GetProperty("labelRecords").EnumerateArray()
    .Select(row => row.Clone())
    .ToArray();
var intakeRows = intake.RootElement.GetProperty("rows").EnumerateArray()
    .Select(row => row.Clone())
    .ToArray();
var rowAudits = labelRows
    .Select(row => AuditRow(row, intakeRows))
    .ToArray();

bool p140Promotable = JsonBool(phase140.RootElement, "intakeArtifactPromotable") is true;
bool p140RowsPromotable = phase140.RootElement.TryGetProperty("validation", out var validation)
    && JsonBool(validation, "rowsPromotable") is true;
bool p140TransitionRulePromotable = phase140.RootElement.TryGetProperty("validation", out validation)
    && JsonBool(validation, "transitionRulePromotable") is true;
bool allRowsMatched = rowAudits.All(r => r.IntakeRowMatched);
bool allRowsComplete = rowAudits.All(r => r.IntakeRowComplete);
bool noExternalTargets = rowAudits.All(r => r.ExternalTargetValuesUsed is false);
bool noRejectedShortcutMarkers = rowAudits.All(r => !r.UsesRejectedShortcutMarker);
var transitionRuleAudit = AuditTransitionRule(intake.RootElement);
bool transitionRuleApplicationReady = p140Promotable
    && p140TransitionRulePromotable
    && transitionRuleAudit.Complete
    && !transitionRuleAudit.UsesRejectedShortcutMarker;
bool rowLabelApplicationReady = p140Promotable
    && p140RowsPromotable
    && allRowsMatched
    && allRowsComplete
    && noExternalTargets
    && noRejectedShortcutMarkers;
bool intakeApplicationPromotable = rowLabelApplicationReady || transitionRuleApplicationReady;
string terminalStatus = intakeApplicationPromotable
    ? "fermion-sector-intake-application-ready"
    : "fermion-sector-intake-application-blocked";

var appliedRows = rowAudits.Select(a => new
{
    a.FamilyId,
    a.CandidateId,
    a.SourceCanonicalFermionModeId,
    chargeSector = intakeApplicationPromotable ? a.ChargeSector : null,
    weakSector = intakeApplicationPromotable ? a.WeakSector : null,
    quantumNumbers = intakeApplicationPromotable ? a.QuantumNumbers : null,
    derivationSource = intakeApplicationPromotable ? a.DerivationId : null,
    hasPhysicalSectorLabels = intakeApplicationPromotable,
    applicationStatus = intakeApplicationPromotable ? "applied" : "blocked",
    a.RowBlockers,
}).ToArray();

var blockers = new List<string>();
if (!p140Promotable)
    blockers.Add("P140 intake artifact is not promotable");
if (!rowLabelApplicationReady && !transitionRuleApplicationReady && !allRowsMatched)
    blockers.Add("not every P131 repaired row has a matching intake row");
if (!rowLabelApplicationReady && !transitionRuleApplicationReady && !allRowsComplete)
    blockers.Add("not every intake row is complete");
if (!rowLabelApplicationReady && !transitionRuleApplicationReady && !noExternalTargets)
    blockers.Add("intake rows must have externalTargetValuesUsed=false");
if (!rowLabelApplicationReady && !transitionRuleApplicationReady && !noRejectedShortcutMarkers)
    blockers.Add("intake derivation IDs must not mark rejected shortcuts from P139");
if (!rowLabelApplicationReady && !transitionRuleApplicationReady)
    blockers.Add("or provide a promotable nontrivial transitionRule accepted by P140");

var result = new
{
    phaseId = "phase141-fermion-sector-intake-application-gate",
    terminalStatus,
    intakeApplicationGateMaterialized = true,
    intakeApplicationPromotable,
    p140Promotable,
    p140RowsPromotable,
    p140TransitionRulePromotable,
    rowLabelApplicationReady,
    transitionRuleApplicationReady,
    allRowsMatched,
    allRowsComplete,
    noExternalTargets,
    noRejectedShortcutMarkers,
    rowAudits,
    appliedSectorLabelTable = new
    {
        tableId = "phase141-applied-fermion-sector-label-table",
        status = intakeApplicationPromotable ? "ready" : "blocked",
        sourceTemplatePath = Phase140TemplatePath,
        labelRecords = appliedRows,
    },
    appliedTransitionRule = new
    {
        ruleId = transitionRuleApplicationReady ? transitionRuleAudit.RuleId : null,
        ruleKind = transitionRuleApplicationReady ? transitionRuleAudit.RuleKind : null,
        derivationSource = transitionRuleApplicationReady ? transitionRuleAudit.DerivationId : null,
        status = transitionRuleApplicationReady ? "ready" : "blocked",
        sourceTemplatePath = Phase140TemplatePath,
        transitionRuleAudit,
    },
    blockers,
    closureRequirements = new[]
    {
        "fill and validate P140 intake before applying labels or a nontrivial transition rule",
        "rerun P141 after P140 is promotable",
        "rerun sector-label gates and corrected W/Z sweep only after P141 reports application-ready",
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase140Path = Phase140Path,
        phase140TemplatePath = Phase140TemplatePath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_intake_application_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_intake_application_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.intakeApplicationGateMaterialized,
        result.intakeApplicationPromotable,
        result.p140Promotable,
        result.p140RowsPromotable,
        result.p140TransitionRulePromotable,
        result.rowLabelApplicationReady,
        result.transitionRuleApplicationReady,
        result.allRowsMatched,
        result.allRowsComplete,
        result.noExternalTargets,
        result.noRejectedShortcutMarkers,
        result.rowAudits,
        result.appliedTransitionRule,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rowAuditCount={rowAudits.Length}");
Console.WriteLine($"intakeApplicationPromotable={intakeApplicationPromotable}");
Console.WriteLine($"transitionRuleApplicationReady={transitionRuleApplicationReady}");

static RowAudit AuditRow(JsonElement labelRow, IReadOnlyList<JsonElement> intakeRows)
{
    string familyId = RequiredString(labelRow, "familyId");
    string candidateId = RequiredString(labelRow, "candidateId");
    string sourceModeId = RequiredString(labelRow, "sourceCanonicalFermionModeId");
    var intakeRow = intakeRows.FirstOrDefault(row =>
        JsonString(row, "familyId") == familyId
        && JsonString(row, "candidateId") == candidateId
        && JsonString(row, "sourceCanonicalFermionModeId") == sourceModeId);
    bool matched = intakeRow.ValueKind != JsonValueKind.Undefined;
    string? chargeSector = matched ? JsonString(intakeRow, "chargeSector") : null;
    string? weakSector = matched ? JsonString(intakeRow, "weakSector") : null;
    JsonElement? quantumNumbers = matched && intakeRow.TryGetProperty("quantumNumbers", out var qn) && qn.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined
        ? qn.Clone()
        : null;
    string? derivationId = matched ? JsonString(intakeRow, "derivationId") : null;
    bool? externalTargetValuesUsed = matched ? JsonBool(intakeRow, "externalTargetValuesUsed") : null;
    bool complete = !string.IsNullOrWhiteSpace(chargeSector)
        && (!string.IsNullOrWhiteSpace(weakSector) || quantumNumbers is not null)
        && !string.IsNullOrWhiteSpace(derivationId)
        && externalTargetValuesUsed is false;
    bool rejectedShortcut = derivationId is not null
        && (derivationId.Contains("numeric-alias", StringComparison.OrdinalIgnoreCase)
            || derivationId.Contains("phase46", StringComparison.OrdinalIgnoreCase)
            || derivationId.Contains("base-chirality-alone", StringComparison.OrdinalIgnoreCase));

    var blockers = new List<string>();
    if (!matched)
        blockers.Add("missing matching intake row");
    if (string.IsNullOrWhiteSpace(chargeSector))
        blockers.Add("missing chargeSector");
    if (string.IsNullOrWhiteSpace(weakSector) && quantumNumbers is null)
        blockers.Add("missing weakSector or quantumNumbers");
    if (string.IsNullOrWhiteSpace(derivationId))
        blockers.Add("missing derivationId");
    if (externalTargetValuesUsed is not false)
        blockers.Add("externalTargetValuesUsed must be false");
    if (rejectedShortcut)
        blockers.Add("derivationId marks a rejected shortcut");

    return new RowAudit(
        FamilyId: familyId,
        CandidateId: candidateId,
        SourceCanonicalFermionModeId: sourceModeId,
        IntakeRowMatched: matched,
        IntakeRowComplete: complete,
        ChargeSector: chargeSector,
        WeakSector: weakSector,
        QuantumNumbers: quantumNumbers,
        DerivationId: derivationId,
        ExternalTargetValuesUsed: externalTargetValuesUsed,
        UsesRejectedShortcutMarker: rejectedShortcut,
        RowBlockers: blockers);
}

static TransitionRuleAudit AuditTransitionRule(JsonElement artifact)
{
    if (!artifact.TryGetProperty("transitionRule", out var rule) || rule.ValueKind != JsonValueKind.Object)
        return new TransitionRuleAudit(null, null, null, false, false, false, false);

    string? ruleId = JsonString(rule, "ruleId");
    string? ruleKind = JsonString(rule, "ruleKind");
    string? derivationId = JsonString(rule, "derivationId");
    bool noExternalTargets = JsonBool(rule, "externalTargetValuesUsed") is false;
    bool hasDirectedTransitions = rule.TryGetProperty("directedTransitions", out var transitions)
        && transitions.ValueKind == JsonValueKind.Array
        && transitions.GetArrayLength() > 0;
    bool rejectedShortcut = derivationId is not null
        && (derivationId.Contains("numeric-alias", StringComparison.OrdinalIgnoreCase)
            || derivationId.Contains("phase46", StringComparison.OrdinalIgnoreCase)
            || derivationId.Contains("base-chirality-alone", StringComparison.OrdinalIgnoreCase));
    bool complete = !string.IsNullOrWhiteSpace(ruleId)
        && !string.IsNullOrWhiteSpace(ruleKind)
        && !string.IsNullOrWhiteSpace(derivationId)
        && noExternalTargets
        && hasDirectedTransitions;

    return new TransitionRuleAudit(
        RuleId: ruleId,
        RuleKind: ruleKind,
        DerivationId: derivationId,
        ExternalTargetValuesUsedFalse: noExternalTargets,
        HasDirectedTransitions: hasDirectedTransitions,
        UsesRejectedShortcutMarker: rejectedShortcut,
        Complete: complete);
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record RowAudit(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    bool IntakeRowMatched,
    bool IntakeRowComplete,
    string? ChargeSector,
    string? WeakSector,
    JsonElement? QuantumNumbers,
    string? DerivationId,
    bool? ExternalTargetValuesUsed,
    bool UsesRejectedShortcutMarker,
    IReadOnlyList<string> RowBlockers);

sealed record TransitionRuleAudit(
    string? RuleId,
    string? RuleKind,
    string? DerivationId,
    bool ExternalTargetValuesUsedFalse,
    bool HasDirectedTransitions,
    bool UsesRejectedShortcutMarker,
    bool Complete);
