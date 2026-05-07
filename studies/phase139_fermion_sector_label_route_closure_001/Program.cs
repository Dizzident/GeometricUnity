using System.Text.Json;

const string DefaultOutputDir = "studies/phase139_fermion_sector_label_route_closure_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase132Path = "studies/phase132_fermion_sector_label_derivation_source_gate_001/output/fermion_sector_label_derivation_source_gate.json";
const string Phase133Path = "studies/phase133_fermion_identity_feature_extractor_001/output/fermion_identity_feature_extractor.json";
const string Phase134Path = "studies/phase134_fermion_chirality_conjugation_transition_table_001/output/fermion_chirality_conjugation_transition_table.json";
const string Phase135Path = "studies/phase135_corrected_wz_sweep_readiness_gate_001/output/corrected_wz_sweep_readiness_gate.json";
const string Phase136Path = "studies/phase136_numeric_alias_sector_label_transfer_audit_001/output/numeric_alias_sector_label_transfer_audit.json";
const string Phase137Path = "studies/phase137_base_chirality_route_audit_001/output/base_chirality_route_audit.json";
const string Phase138Path = "studies/phase138_repaired_row_coupling_transition_graph_001/output/repaired_row_coupling_transition_graph.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE139_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase132 = JsonDocument.Parse(File.ReadAllText(Phase132Path));
using var phase133 = JsonDocument.Parse(File.ReadAllText(Phase133Path));
using var phase134 = JsonDocument.Parse(File.ReadAllText(Phase134Path));
using var phase135 = JsonDocument.Parse(File.ReadAllText(Phase135Path));
using var phase136 = JsonDocument.Parse(File.ReadAllText(Phase136Path));
using var phase137 = JsonDocument.Parse(File.ReadAllText(Phase137Path));
using var phase138 = JsonDocument.Parse(File.ReadAllText(Phase138Path));

var routeReadiness = new[]
{
    Route(
        "existing-derivation-source",
        Phase132Path,
        JsonString(phase132.RootElement, "terminalStatus"),
        JsonBool(phase132.RootElement, "derivationSourcePromotable") is true,
        "existing Phase27/Phase46 sector-label sources do not match P131 fermion rows"),
    Route(
        "identity-feature-extractor",
        Phase133Path,
        JsonString(phase133.RootElement, "terminalStatus"),
        JsonBool(phase133.RootElement, "featureExtractorPromotable") is true,
        "P127/P128/P137 diagnostics are materialized but do not assign physical sector labels"),
    Route(
        "chirality-conjugation-transition-table",
        Phase134Path,
        JsonString(phase134.RootElement, "terminalStatus"),
        JsonBool(phase134.RootElement, "transitionTablePromotable") is true,
        "full chirality is trivial and no conjugation pairs exist for the repaired rows"),
    Route(
        "corrected-sweep-readiness",
        Phase135Path,
        JsonString(phase135.RootElement, "terminalStatus"),
        JsonBool(phase135.RootElement, "rerunReady") is true,
        "corrected W/Z sweep rerun is blocked until labels or a transition rule are promoted"),
    Route(
        "numeric-alias-transfer",
        Phase136Path,
        JsonString(phase136.RootElement, "terminalStatus"),
        JsonBool(phase136.RootElement, "aliasTransferPromotable") is true,
        "numeric alias reaches vector-boson source labels, not fermion-sector labels"),
    Route(
        "base-chirality-route",
        Phase137Path,
        JsonString(phase137.RootElement, "terminalStatus"),
        JsonBool(phase137.RootElement, "baseChiralityRoutePromotable") is true,
        "base X-chirality is mixed and diagnostic-only"),
    Route(
        "coupling-transition-graph",
        Phase138Path,
        JsonString(phase138.RootElement, "terminalStatus"),
        JsonBool(phase138.RootElement, "transitionGraphPromotable") is true,
        "coupling graph is nontrivial but symmetric and unlabeled"),
};

bool coverageRepaired = JsonBool(phase131.RootElement, "allRowsHaveCandidateCoverage") is true;
bool allRowsHaveLabels = JsonBool(phase131.RootElement, "allRowsHavePhysicalLabels") is true;
bool anyRoutePromotable = routeReadiness.Any(r => r.Promotable);
bool closureReady = coverageRepaired && allRowsHaveLabels && anyRoutePromotable;
string terminalStatus = closureReady
    ? "fermion-sector-label-route-closure-ready"
    : "fermion-sector-label-route-new-input-required";

var targetRows = phase131.RootElement.GetProperty("labelRecords").EnumerateArray()
    .Select(row => new
    {
        familyId = RequiredString(row, "familyId"),
        candidateId = RequiredString(row, "candidateId"),
        sourceCanonicalFermionModeId = RequiredString(row, "sourceCanonicalFermionModeId"),
        chargeSector = JsonString(row, "chargeSector"),
        weakSector = JsonString(row, "weakSector"),
        hasQuantumNumbers = row.TryGetProperty("quantumNumbers", out var qn) && qn.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined,
    })
    .ToArray();

var result = new
{
    phaseId = "phase139-fermion-sector-label-route-closure",
    terminalStatus,
    routeClosureMaterialized = true,
    coverageRepaired,
    allRowsHaveLabels,
    anyRoutePromotable,
    closureReady,
    targetRows,
    routeReadiness,
    requiredNewArtifactContract = new
    {
        contractId = "phase139-fermion-sector-label-or-transition-rule-contract-v1",
        acceptableArtifactKinds = new[]
        {
            "fermion-sector-label-table keyed by familyId/candidateId/sourceCanonicalFermionModeId",
            "nontrivial chirality/conjugation transition table keyed by repaired row ids",
            "directed coupling-transition rule combined with target-blind charge/weak-sector derivation",
        },
        requiredFields = new[]
        {
            "familyId",
            "candidateId",
            "sourceCanonicalFermionModeId",
            "chargeSector",
            "weakSector or quantumNumbers",
            "derivationId",
            "externalTargetValuesUsed=false",
        },
        rejectedShortcuts = new[]
        {
            "numeric suffix transfer from Phase46 vector-boson source candidates",
            "mixed gauge-basis or T3 fractions without a sector rule",
            "base X-chirality alone",
            "symmetric coupling magnitude graph without labels or direction",
        },
    },
    blockers = new[]
    {
        "all existing target-blind routes P132-P138 are diagnostic-only or blocked",
        "P131 repaired rows still lack chargeSector and weakSector/quantumNumbers",
        "a new fermion-specific sector-label artifact or nontrivial transition-rule artifact is required",
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase132Path = Phase132Path,
        phase133Path = Phase133Path,
        phase134Path = Phase134Path,
        phase135Path = Phase135Path,
        phase136Path = Phase136Path,
        phase137Path = Phase137Path,
        phase138Path = Phase138Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_label_route_closure.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_label_route_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.routeClosureMaterialized,
        result.coverageRepaired,
        result.allRowsHaveLabels,
        result.anyRoutePromotable,
        result.closureReady,
        result.routeReadiness,
        result.requiredNewArtifactContract,
        result.blockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"routeCount={routeReadiness.Length}");
Console.WriteLine($"anyRoutePromotable={anyRoutePromotable}");
Console.WriteLine($"closureReady={closureReady}");

static RouteReadiness Route(string routeId, string evidencePath, string? terminalStatus, bool promotable, string blocker) =>
    new(routeId, evidencePath, terminalStatus ?? "missing", promotable, promotable ? null : blocker);
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record RouteReadiness(
    string RouteId,
    string EvidencePath,
    string TerminalStatus,
    bool Promotable,
    string? Blocker);
