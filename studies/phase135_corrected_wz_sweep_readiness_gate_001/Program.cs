using System.Text.Json;

const string DefaultOutputDir = "studies/phase135_corrected_wz_sweep_readiness_gate_001/output";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase133Path = "studies/phase133_fermion_identity_feature_extractor_001/output/fermion_identity_feature_extractor.json";
const string Phase134Path = "studies/phase134_fermion_chirality_conjugation_transition_table_001/output/fermion_chirality_conjugation_transition_table.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE135_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase133 = JsonDocument.Parse(File.ReadAllText(Phase133Path));
using var phase134 = JsonDocument.Parse(File.ReadAllText(Phase134Path));
using var phase141 = File.Exists(Phase141Path) ? JsonDocument.Parse(File.ReadAllText(Phase141Path)) : null;

bool correctedOperatorSweepAvailable = string.Equals(
    JsonString(phase122.RootElement, "terminalStatus"),
    "corrected-operator-selection-rule-sweep-no-transition-repair",
    StringComparison.Ordinal)
    || string.Equals(
        JsonString(phase122.RootElement, "terminalStatus"),
        "corrected-operator-selection-rule-sweep-found-projection-candidate",
        StringComparison.Ordinal);
bool coverageRepaired = JsonBool(phase131.RootElement, "allRowsHaveCandidateCoverage") is true;
bool sectorLabelsReady = JsonBool(phase131.RootElement, "sectorLabelTablePromotable") is true;
bool featuresMaterialized = JsonBool(phase133.RootElement, "fermionIdentityFeaturesMaterialized") is true;
bool featureLabelsReady = JsonBool(phase133.RootElement, "featureExtractorPromotable") is true;
bool transitionTableMaterialized = JsonBool(phase134.RootElement, "transitionTableMaterialized") is true;
bool transitionTableReady = JsonBool(phase134.RootElement, "transitionTablePromotable") is true;
bool intakeTransitionRuleReady = phase141?.RootElement.TryGetProperty("appliedTransitionRule", out var appliedTransitionRule) is true
    && string.Equals(JsonString(appliedTransitionRule, "status"), "ready", StringComparison.Ordinal);
bool rerunReady = correctedOperatorSweepAvailable
    && coverageRepaired
    && featuresMaterialized
    && (sectorLabelsReady || intakeTransitionRuleReady)
    && (featureLabelsReady || transitionTableReady || intakeTransitionRuleReady);
string terminalStatus = rerunReady
    ? "corrected-wz-sweep-rerun-ready"
    : "corrected-wz-sweep-rerun-sector-labels-blocked";

var blockers = new List<string>();
if (!correctedOperatorSweepAvailable)
    blockers.Add("corrected-operator selection-rule sweep evidence is missing or not usable");
if (!coverageRepaired)
    blockers.Add("coverage-repaired fermion label rows are incomplete");
if (!sectorLabelsReady)
    blockers.Add(intakeTransitionRuleReady
        ? "coverage-repaired fermion label table still lacks explicit labels, but a P141 transition rule is ready"
        : "coverage-repaired fermion label table still lacks explicit chargeSector and weak-sector/quantum-number labels");
if (!featuresMaterialized)
    blockers.Add("fermion identity feature records are not materialized for every repaired row");
if (!featureLabelsReady)
    blockers.Add("fermion identity feature extractor has no promotable physical labels");
if (!transitionTableReady)
    blockers.Add(intakeTransitionRuleReady
        ? "chirality/conjugation transition table is not promotable, but a P141 transition rule is ready"
        : "chirality/conjugation transition table is materialized but not promotable");

var readinessRows = phase133.RootElement.GetProperty("featureRecords")
    .EnumerateArray()
    .Select(row => new
    {
        familyId = RequiredString(row, "familyId"),
        candidateId = RequiredString(row, "candidateId"),
        modeIndex = RequiredInt(row, "modeIndex"),
        chargeSector = JsonString(row, "chargeSector"),
        weakSector = JsonString(row, "weakSector"),
        quantumNumbers = row.TryGetProperty("quantumNumbers", out var qn) ? qn.Clone() : default,
        promotableGaugeBasisSector = JsonBool(row, "promotableGaugeBasisSector"),
        promotableT3Sector = JsonBool(row, "promotableT3Sector"),
        featureStatus = JsonString(row, "featureStatus"),
    })
    .ToArray();

var result = new
{
    phaseId = "phase135-corrected-wz-sweep-readiness-gate",
    terminalStatus,
    rerunReady,
    correctedOperatorSweepAvailable,
    coverageRepaired,
    sectorLabelsReady,
    featuresMaterialized,
    featureLabelsReady,
    transitionTableMaterialized,
    transitionTableReady,
    intakeTransitionRuleReady,
    readinessRows,
    sourceGateStatuses = new
    {
        phase122 = JsonString(phase122.RootElement, "terminalStatus"),
        phase131 = JsonString(phase131.RootElement, "terminalStatus"),
        phase133 = JsonString(phase133.RootElement, "terminalStatus"),
        phase134 = JsonString(phase134.RootElement, "terminalStatus"),
        phase141 = phase141 is null ? null : JsonString(phase141.RootElement, "terminalStatus"),
    },
    blockers,
    closureRequirements = new[]
    {
        "derive explicit target-blind chargeSector labels for every P131 repaired row",
        "derive explicit target-blind weakSector or quantumNumbers labels for every P131 repaired row",
        "or replace the trivial Phase12 chirality/conjugation evidence with a nontrivial promotable transition observable",
        "rerun sector-label gates before rerunning the corrected W/Z transition sweep",
    },
    sourceEvidence = new
    {
        phase122Path = Phase122Path,
        phase131Path = Phase131Path,
        phase133Path = Phase133Path,
        phase134Path = Phase134Path,
        phase141Path = File.Exists(Phase141Path) ? Phase141Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "corrected_wz_sweep_readiness_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "corrected_wz_sweep_readiness_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.rerunReady,
        result.correctedOperatorSweepAvailable,
        result.coverageRepaired,
        result.sectorLabelsReady,
        result.featuresMaterialized,
        result.featureLabelsReady,
        result.transitionTableMaterialized,
        result.transitionTableReady,
        result.intakeTransitionRuleReady,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rerunReady={rerunReady}");
Console.WriteLine($"sectorLabelsReady={sectorLabelsReady}");
Console.WriteLine($"transitionTableReady={transitionTableReady}");
Console.WriteLine($"intakeTransitionRuleReady={intakeTransitionRuleReady}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static int RequiredInt(JsonElement element, string propertyName) =>
    JsonInt(element, propertyName) ?? throw new InvalidDataException($"Missing integer property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
