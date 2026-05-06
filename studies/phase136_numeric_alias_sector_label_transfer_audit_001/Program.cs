using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase136_numeric_alias_sector_label_transfer_audit_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase135Path = "studies/phase135_corrected_wz_sweep_readiness_gate_001/output/corrected_wz_sweep_readiness_gate.json";
const string Phase46SourceCandidatesPath = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/source_candidates.json";
const string Phase46SpectraDir = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/spectra";

var outputDir = Environment.GetEnvironmentVariable("PHASE136_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase135 = JsonDocument.Parse(File.ReadAllText(Phase135Path));
using var sourceCandidates = JsonDocument.Parse(File.ReadAllText(Phase46SourceCandidatesPath));

var roleByCandidateId = sourceCandidates.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(c => new
    SourceCandidateRecord(
        SourceCandidateId: RequiredString(c, "sourceCandidateId"),
        ModeRole: JsonString(c, "modeRole"),
        Status: JsonString(c, "status")))
    .ToDictionary(c => NormalizePhase46CandidateId(c.SourceCandidateId), c => c, StringComparer.Ordinal);

var spectraByCandidateId = Directory.GetFiles(Phase46SpectraDir, "*_spectrum.json")
    .Select(path =>
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        var term = root.GetProperty("operatorTermEvidence");
        return new SpectrumRecord(
            Path: path,
            SourceCandidateId: RequiredString(root, "sourceCandidateId"),
            ChargeSector: JsonString(term, "chargeSector"),
            ElectroweakMultipletId: JsonString(term, "electroweakMultipletId"),
            TermId: JsonString(term, "termId"));
    })
    .GroupBy(s => s.SourceCandidateId, StringComparer.Ordinal)
    .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);

var rowAudits = phase131.RootElement.GetProperty("labelRecords")
    .EnumerateArray()
    .Select(row => BuildRowAudit(row, roleByCandidateId, spectraByCandidateId))
    .ToArray();

bool anyAliasHasPhase46ChargeSector = rowAudits.Any(r => r.AliasHasPhase46ChargeSector);
bool allAliasesHavePhase46ChargeSector = rowAudits.All(r => r.AliasHasPhase46ChargeSector);
bool anyAliasPromotable = rowAudits.Any(r => r.AliasTransferPromotable);
bool aliasTransferPromotable = rowAudits.Length > 0 && rowAudits.All(r => r.AliasTransferPromotable);
string terminalStatus = aliasTransferPromotable
    ? "fermion-sector-numeric-alias-transfer-ready"
    : "fermion-sector-numeric-alias-transfer-rejected";

var blockers = new List<string>();
if (!allAliasesHavePhase46ChargeSector)
    blockers.Add("not every numeric alias has Phase46 charge-sector evidence");
if (!anyAliasPromotable)
    blockers.Add("numeric alias evidence is vector-boson source evidence, not fermion sector-label evidence");
blockers.Add("numeric alias evidence has no weakSector or fermion quantumNumbers assignment");
blockers.Add("numeric suffix equality is not an accepted derivationId or target-blind join key");

var result = new
{
    phaseId = "phase136-numeric-alias-sector-label-transfer-audit",
    terminalStatus,
    aliasAuditMaterialized = true,
    aliasTransferPromotable,
    anyAliasHasPhase46ChargeSector,
    allAliasesHavePhase46ChargeSector,
    anyAliasPromotable,
    rowAudits,
    phase135Gate = new
    {
        terminalStatus = JsonString(phase135.RootElement, "terminalStatus"),
        rerunReady = JsonBool(phase135.RootElement, "rerunReady"),
        sectorLabelsReady = JsonBool(phase135.RootElement, "sectorLabelsReady"),
        transitionTableReady = JsonBool(phase135.RootElement, "transitionTableReady"),
    },
    rejectedTransferRule = new
    {
        ruleId = "phase136-reject-numeric-suffix-transfer-v1",
        rejectedSource = "Phase46 phase12-candidate-N chargeSector values reached only through source-mode numeric suffix aliasing",
        reason = "the Phase46 records are vector-boson source spectra and do not supply fermion weak-sector labels, quantumNumbers, or a derivation source keyed to P131 rows",
    },
    blockers,
    closureRequirements = new[]
    {
        "derive labels from a fermion-specific observable keyed to P131 familyId/candidateId/sourceCanonicalFermionModeId",
        "or materialize a nontrivial chirality/conjugation transition observable for the repaired rows",
        "do not transfer Phase46 vector-boson charge sectors by numeric suffix alias alone",
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase135Path = Phase135Path,
        phase46SourceCandidatesPath = Phase46SourceCandidatesPath,
        phase46SpectraDir = Phase46SpectraDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "numeric_alias_sector_label_transfer_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "numeric_alias_sector_label_transfer_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.aliasAuditMaterialized,
        result.aliasTransferPromotable,
        result.anyAliasHasPhase46ChargeSector,
        result.allAliasesHavePhase46ChargeSector,
        result.anyAliasPromotable,
        result.rowAudits,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rowCount={rowAudits.Length}");
Console.WriteLine($"allAliasesHavePhase46ChargeSector={allAliasesHavePhase46ChargeSector}");
Console.WriteLine($"aliasTransferPromotable={aliasTransferPromotable}");

static RowAudit BuildRowAudit(
    JsonElement row,
    IReadOnlyDictionary<string, SourceCandidateRecord> roleByCandidateId,
    IReadOnlyDictionary<string, SpectrumRecord[]> spectraByCandidateId)
{
    string sourceModeId = RequiredString(row, "sourceCanonicalFermionModeId");
    int? suffix = ExtractModeSuffix(sourceModeId);
    string? aliasCandidateId = suffix is null ? null : $"phase12-candidate-{suffix.Value}";
    roleByCandidateId.TryGetValue(aliasCandidateId ?? "", out var role);
    spectraByCandidateId.TryGetValue(aliasCandidateId ?? "", out var spectra);

    var chargeSectors = spectra is null
        ? []
        : spectra.Select(s => s.ChargeSector).Where(s => s is not null).Select(s => s!).Distinct(StringComparer.Ordinal).ToArray();
    var multiplets = spectra is null
        ? []
        : spectra.Select(s => s.ElectroweakMultipletId).Where(s => s is not null).Select(s => s!).Distinct(StringComparer.Ordinal).ToArray();
    bool aliasHasCharge = chargeSectors.Length > 0;
    bool roleIsFermion = string.Equals(role?.ModeRole, "fermion-sector-label-source", StringComparison.Ordinal);

    return new RowAudit(
        FamilyId: RequiredString(row, "familyId"),
        CandidateId: RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId: sourceModeId,
        NumericSuffix: suffix,
        AliasCandidateId: aliasCandidateId,
        AliasModeRole: role?.ModeRole,
        AliasStatus: role?.Status,
        AliasSpectrumCount: spectra?.Length ?? 0,
        AliasChargeSectors: chargeSectors,
        AliasElectroweakMultipletIds: multiplets,
        AliasHasPhase46ChargeSector: aliasHasCharge,
        AliasHasWeakSectorOrQuantumNumbers: false,
        AliasTransferPromotable: aliasHasCharge && roleIsFermion,
        Blocker: "Phase46 alias evidence is not a fermion-specific sector-label derivation for this P131 row");
}

static int? ExtractModeSuffix(string sourceModeId)
{
    var match = Regex.Match(sourceModeId, @"-(\d+)$", RegexOptions.CultureInvariant);
    return match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : null;
}

static string NormalizePhase46CandidateId(string sourceCandidateId) =>
    sourceCandidateId.StartsWith("phase22-", StringComparison.Ordinal)
        ? sourceCandidateId["phase22-".Length..]
        : sourceCandidateId;
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
    int? NumericSuffix,
    string? AliasCandidateId,
    string? AliasModeRole,
    string? AliasStatus,
    int AliasSpectrumCount,
    IReadOnlyList<string> AliasChargeSectors,
    IReadOnlyList<string> AliasElectroweakMultipletIds,
    bool AliasHasPhase46ChargeSector,
    bool AliasHasWeakSectorOrQuantumNumbers,
    bool AliasTransferPromotable,
    string Blocker);

sealed record SourceCandidateRecord(string SourceCandidateId, string? ModeRole, string? Status);

sealed record SpectrumRecord(
    string Path,
    string SourceCandidateId,
    string? ChargeSector,
    string? ElectroweakMultipletId,
    string? TermId);
