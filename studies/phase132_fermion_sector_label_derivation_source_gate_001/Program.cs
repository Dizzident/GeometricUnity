using System.Text.Json;

const string DefaultOutputDir = "studies/phase132_fermion_sector_label_derivation_source_gate_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase27MixingReadinessPath = "studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json";
const string Phase27IdentityFeaturesPath = "studies/phase27_charge_sector_convention_001/identity_features_with_charge_sectors.json";
const string Phase27ModeFamiliesPath = "studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json";
const string Phase46SourceCandidatesPath = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/source_candidates.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE132_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase27Mixing = JsonDocument.Parse(File.ReadAllText(Phase27MixingReadinessPath));
using var phase27Features = JsonDocument.Parse(File.ReadAllText(Phase27IdentityFeaturesPath));
using var phase27ModeFamilies = JsonDocument.Parse(File.ReadAllText(Phase27ModeFamiliesPath));
using var phase46Sources = JsonDocument.Parse(File.ReadAllText(Phase46SourceCandidatesPath));

var rows = phase131.RootElement.GetProperty("labelRecords").EnumerateArray().ToList();
var rowCandidateIds = rows.Select(row => RequiredString(row, "candidateId")).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
var rowFamilyIds = rows.Select(row => RequiredString(row, "familyId")).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
var sourceCandidateIds = rows
    .Select(row => JsonString(row, "sourceCandidateId"))
    .Where(id => id is not null)
    .Distinct(StringComparer.Ordinal)
    .ToHashSet(StringComparer.Ordinal);

var chargeAssignments = phase27Mixing.RootElement.GetProperty("chargeSectorAssignments")
    .EnumerateArray()
    .Select(a => new
    {
        sourceCandidateId = RequiredString(a, "sourceCandidateId"),
        electroweakMultipletId = JsonString(a, "electroweakMultipletId"),
        dominantBasisIndex = JsonInt(a, "dominantBasisIndex"),
        chargeSector = JsonString(a, "chargeSector"),
        derivationId = JsonString(a, "derivationId"),
    })
    .ToList();
var featureRecords = phase27Features.RootElement.GetProperty("featureRecords")
    .EnumerateArray()
    .Select(f => new
    {
        familyId = JsonString(f, "familyId"),
        sourceCandidateId = JsonString(f, "sourceCandidateId"),
        electroweakMultipletId = JsonString(f, "electroweakMultipletId"),
        dominantBasisIndex = JsonInt(f, "dominantBasisIndex"),
        chargeSector = JsonString(f, "chargeSector"),
        featureStatus = JsonString(f, "featureStatus"),
    })
    .ToList();
var phase27FamilyRows = phase27ModeFamilies.RootElement.GetProperty("families")
    .EnumerateArray()
    .Select(f => new
    {
        familyId = JsonString(f, "familyId"),
        sourceCandidateId = JsonString(f, "sourceCandidateId"),
        chargeSector = f.TryGetProperty("identityFeatures", out var features)
            ? JsonString(features, "chargeSector")
            : null,
    })
    .ToList();
var phase46Candidates = phase46Sources.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(c => new
    {
        sourceCandidateId = JsonString(c, "sourceCandidateId"),
        sourceFamilyId = JsonString(c, "sourceFamilyId"),
        modeRole = JsonString(c, "modeRole"),
        status = JsonString(c, "status"),
    })
    .ToList();

var rowAudits = rows.Select(row =>
{
    string familyId = RequiredString(row, "familyId");
    string candidateId = RequiredString(row, "candidateId");
    string? sourceCandidateId = JsonString(row, "sourceCandidateId");
    var matchedAssignments = chargeAssignments
        .Where(a => string.Equals(a.sourceCandidateId, candidateId, StringComparison.Ordinal)
            || string.Equals(a.sourceCandidateId, sourceCandidateId, StringComparison.Ordinal)
            || string.Equals(a.sourceCandidateId, familyId, StringComparison.Ordinal))
        .ToArray();
    var matchedFeatures = featureRecords
        .Where(f => string.Equals(f.sourceCandidateId, candidateId, StringComparison.Ordinal)
            || string.Equals(f.sourceCandidateId, sourceCandidateId, StringComparison.Ordinal)
            || string.Equals(f.familyId, familyId, StringComparison.Ordinal))
        .ToArray();
    var matchedFamilies = phase27FamilyRows
        .Where(f => string.Equals(f.sourceCandidateId, candidateId, StringComparison.Ordinal)
            || string.Equals(f.sourceCandidateId, sourceCandidateId, StringComparison.Ordinal)
            || string.Equals(f.familyId, familyId, StringComparison.Ordinal))
        .ToArray();
    var matchedPhase46Sources = phase46Candidates
        .Where(c => string.Equals(c.sourceCandidateId, candidateId, StringComparison.Ordinal)
            || string.Equals(c.sourceCandidateId, sourceCandidateId, StringComparison.Ordinal)
            || string.Equals(c.sourceFamilyId, familyId, StringComparison.Ordinal))
        .ToArray();

    return new
    {
        modeIndex = RequiredInt(row, "modeIndex"),
        familyId,
        candidateId,
        sourceCandidateId,
        matchedPhase27ChargeAssignments = matchedAssignments.Length,
        matchedPhase27IdentityFeatures = matchedFeatures.Length,
        matchedPhase27ModeFamilies = matchedFamilies.Length,
        matchedPhase46SourceCandidates = matchedPhase46Sources.Length,
        derivationSourcePromotable = false,
        blocker = "no target-blind fermion feature record or charge-sector assignment matches this coverage-repaired row",
    };
}).ToList();

bool phase27MixingReady = string.Equals(
    JsonString(phase27Mixing.RootElement, "terminalStatus"),
    "mixing-convention-ready",
    StringComparison.Ordinal);
bool anyMatchingChargeAssignment = rowAudits.Any(a => a.matchedPhase27ChargeAssignments > 0);
bool anyMatchingIdentityFeature = rowAudits.Any(a => a.matchedPhase27IdentityFeatures > 0);
bool anyMatchingModeFamily = rowAudits.Any(a => a.matchedPhase27ModeFamilies > 0);
bool anyMatchingPhase46Source = rowAudits.Any(a => a.matchedPhase46SourceCandidates > 0);
bool derivationSourcePromotable = rowAudits.Count > 0
    && rowAudits.All(a => a.matchedPhase27ChargeAssignments > 0 || a.matchedPhase27IdentityFeatures > 0)
    && JsonBool(phase131.RootElement, "allRowsHaveCandidateCoverage") is true;
string terminalStatus = derivationSourcePromotable
    ? "fermion-sector-label-derivation-source-ready"
    : "fermion-sector-label-derivation-source-blocked";

var blockers = new List<string>();
if (!phase27MixingReady)
    blockers.Add("Phase27 mixing convention is not ready");
if (!anyMatchingChargeAssignment)
    blockers.Add("no Phase27 charge-sector assignment matches coverage-repaired fermion candidate or family IDs");
if (!anyMatchingIdentityFeature)
    blockers.Add("no Phase27 identity feature record matches coverage-repaired fermion candidate or family IDs");
if (!anyMatchingModeFamily)
    blockers.Add("no Phase27 mode-family charge-sector row matches coverage-repaired fermion candidate or family IDs");
if (!anyMatchingPhase46Source)
    blockers.Add("Phase46 source spectra are vector-boson source candidates and do not match coverage-repaired fermion rows");
if (JsonBool(phase131.RootElement, "allRowsHavePhysicalLabels") is not true)
    blockers.Add("P131 coverage-repaired rows still have unassigned physical sector labels");

var derivationContract = new
{
    contractId = "phase132-fermion-sector-label-derivation-contract-v1",
    targetRows = rows.Select(row => new
    {
        familyId = RequiredString(row, "familyId"),
        candidateId = RequiredString(row, "candidateId"),
        sourceCanonicalFermionModeId = JsonString(row, "sourceCanonicalFermionModeId"),
    }).ToArray(),
    requiredFeatureRecordFields = new[]
    {
        "familyId",
        "candidateId",
        "sourceCanonicalFermionModeId",
        "electroweakMultipletId",
        "dominantBasisIndex or non-basis sector observable",
        "chargeSector",
        "weakSector or quantumNumbers",
        "derivationId",
        "externalTargetValuesUsed=false",
    },
    acceptableDerivationSources = new[]
    {
        "new target-blind fermion identity feature extractor over coverage-repaired rows",
        "validated nontrivial chirality/conjugation transition table for the repaired families",
        "validated fermion-specific electroweak mixing convention with assignments keyed to P131 candidate/family IDs",
    },
    explicitlyRejectedTransfers = new[]
    {
        "Phase27 boson/vector source charge sectors keyed to phase12-candidate-*",
        "Phase46 vector-boson source spectra chargeSector values",
        "P127 gauge-basis fractions without a physical derivation",
        "P128 SU(2) generator moments without a promotable sector rule",
    },
};

var result = new
{
    phaseId = "phase132-fermion-sector-label-derivation-source-gate",
    terminalStatus,
    derivationSourceGateMaterialized = true,
    derivationSourcePromotable,
    phase27MixingReady,
    anyMatchingChargeAssignment,
    anyMatchingIdentityFeature,
    anyMatchingModeFamily,
    anyMatchingPhase46Source,
    rowAudits,
    derivationContract,
    phase131Gate = new
    {
        terminalStatus = JsonString(phase131.RootElement, "terminalStatus"),
        allRowsHaveCandidateCoverage = JsonBool(phase131.RootElement, "allRowsHaveCandidateCoverage"),
        allRowsHavePhysicalLabels = JsonBool(phase131.RootElement, "allRowsHavePhysicalLabels"),
        sectorLabelTablePromotable = JsonBool(phase131.RootElement, "sectorLabelTablePromotable"),
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase27MixingReadinessPath = Phase27MixingReadinessPath,
        phase27IdentityFeaturesPath = Phase27IdentityFeaturesPath,
        phase27ModeFamiliesPath = Phase27ModeFamiliesPath,
        phase46SourceCandidatesPath = Phase46SourceCandidatesPath,
    },
    blockers,
    closureRequirements = new[]
    {
        "implement a target-blind fermion identity feature extractor keyed to the P131 coverage-repaired candidate/family rows",
        "emit chargeSector and weakSector/quantumNumbers with derivationId and externalTargetValuesUsed=false",
        "rerun the sector-label table gate only after derivation-source records match every P131 row",
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_label_derivation_source_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_label_derivation_source_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.derivationSourceGateMaterialized,
        result.derivationSourcePromotable,
        result.phase27MixingReady,
        result.anyMatchingChargeAssignment,
        result.anyMatchingIdentityFeature,
        result.anyMatchingModeFamily,
        result.anyMatchingPhase46Source,
        result.rowAudits,
        result.derivationContract,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase27MixingReady={phase27MixingReady}");
Console.WriteLine($"anyMatchingChargeAssignment={anyMatchingChargeAssignment}");
Console.WriteLine($"derivationSourcePromotable={derivationSourcePromotable}");

static string RequiredString(JsonElement element, string propertyName)
{
    return JsonString(element, propertyName)
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static int RequiredInt(JsonElement element, string propertyName)
{
    return JsonInt(element, propertyName)
        ?? throw new InvalidDataException($"Missing integer property '{propertyName}'.");
}

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;
}

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property)
        ? property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        }
        : null;
}
