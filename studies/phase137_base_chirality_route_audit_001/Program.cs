using System.Text.Json;
using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase137_base_chirality_route_audit_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase136Path = "studies/phase136_numeric_alias_sector_label_transfer_audit_001/output/numeric_alias_sector_label_transfer_audit.json";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string LayoutPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/layout_bg-phase12-bg-a-20260315212202.json";
const string FermionModesPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-a-20260315212202.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE137_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase136 = JsonDocument.Parse(File.ReadAllText(Phase136Path));
using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
using var modesDoc = JsonDocument.Parse(File.ReadAllText(FermionModesPath));

var layout = JsonSerializer.Deserialize<FermionFieldLayout>(File.ReadAllText(LayoutPath), jsonOptions)
    ?? throw new InvalidDataException("Could not deserialize Phase12 fermion layout.");
var spinorRoot = spinorDoc.RootElement;
var signature = new CliffordSignature
{
    Positive = RequiredInt(spinorRoot.GetProperty("cliffordSignature"), "positive"),
    Negative = RequiredInt(spinorRoot.GetProperty("cliffordSignature"), "negative"),
};
var gammaConvention = new GammaConventionSpec
{
    ConventionId = RequiredString(spinorRoot.GetProperty("gammaConvention"), "conventionId"),
    Signature = signature,
    Representation = RequiredString(spinorRoot.GetProperty("gammaConvention"), "representation"),
    SpinorDimension = RequiredInt(spinorRoot.GetProperty("gammaConvention"), "spinorDimension"),
    HasChirality = JsonBool(spinorRoot.GetProperty("gammaConvention"), "hasChirality") is true,
};
var chiralityConvention = new ChiralityConventionSpec
{
    ConventionId = RequiredString(spinorRoot.GetProperty("chiralityConvention"), "conventionId"),
    SignConvention = RequiredString(spinorRoot.GetProperty("chiralityConvention"), "signConvention"),
    PhaseFactor = RequiredString(spinorRoot.GetProperty("chiralityConvention"), "phaseFactor"),
    HasChirality = JsonBool(spinorRoot.GetProperty("chiralityConvention"), "hasChirality") is true,
    BaseDimension = JsonInt(spinorRoot.GetProperty("chiralityConvention"), "baseDimension"),
    FiberDimension = JsonInt(spinorRoot.GetProperty("chiralityConvention"), "fiberDimension"),
    BaseChiralityOperator = JsonString(spinorRoot.GetProperty("chiralityConvention"), "baseChiralityOperator"),
    FiberChiralityOperator = JsonString(spinorRoot.GetProperty("chiralityConvention"), "fiberChiralityOperator"),
};
var gammas = new GammaMatrixBuilder().Build(signature, gammaConvention, Provenance());
var analyzer = new ChiralityAnalyzer();

var modesById = modesDoc.RootElement.GetProperty("modes")
    .EnumerateArray()
    .Select(m => JsonSerializer.Deserialize<FermionModeRecord>(m.GetRawText(), jsonOptions)
        ?? throw new InvalidDataException("Could not deserialize fermion mode."))
    .ToDictionary(m => m.ModeId, m => m, StringComparer.Ordinal);

var rowAudits = phase131.RootElement.GetProperty("labelRecords")
    .EnumerateArray()
    .Select(row =>
    {
        string sourceModeId = RequiredString(row, "sourceCanonicalFermionModeId");
        modesById.TryGetValue(sourceModeId, out var mode);
        var record = mode is null
            ? null
            : analyzer.AnalyzeTriple(mode, gammas, chiralityConvention, layout, cellCount: 27);
        return new
        {
            familyId = RequiredString(row, "familyId"),
            candidateId = RequiredString(row, "candidateId"),
            sourceCanonicalFermionModeId = sourceModeId,
            modeFound = mode is not null,
            baseChiralityOperator = chiralityConvention.BaseChiralityOperator,
            fullYChiralityAvailable = record?.YChirality is not null,
            xChiralityStatus = record?.XChirality.ChiralityStatus,
            xChiralityTag = record?.XChirality.ChiralityTag,
            xLeftFraction = record?.XChirality.LeftFraction,
            xRightFraction = record?.XChirality.RightFraction,
            xLeakageDiagnostic = record?.XChirality.LeakageDiagnostic,
            fChiralityAvailable = record?.FChirality is not null,
            chargeSector = (string?)null,
            weakSector = (string?)null,
            quantumNumbers = (object?)null,
            promotableBaseChiralitySector = false,
            blocker = "base X-chirality is a useful diagnostic but does not by itself assign chargeSector plus weakSector/quantumNumbers",
        };
    })
    .ToArray();

bool baseChiralityAvailable = chiralityConvention.BaseDimension is not null
    && chiralityConvention.BaseDimension.Value % 2 == 0
    && !string.IsNullOrWhiteSpace(chiralityConvention.BaseChiralityOperator);
bool allRowsHaveBaseChirality = rowAudits.All(r => r.modeFound && r.xChiralityStatus is not null and not "trivial" and not "not-applicable");
bool anyDefiniteBaseChirality = rowAudits.Any(r => r.xChiralityStatus is "definite-left" or "definite-right");
bool baseChiralityRoutePromotable = false;
string terminalStatus = baseChiralityRoutePromotable
    ? "fermion-base-chirality-route-sector-labels-ready"
    : "fermion-base-chirality-route-diagnostic-only";

var blockers = new List<string>();
if (!baseChiralityAvailable)
    blockers.Add("Phase12 spinor representation does not expose a base X-chirality operator");
if (!allRowsHaveBaseChirality)
    blockers.Add("not every repaired row has nontrivial base X-chirality diagnostics");
if (!anyDefiniteBaseChirality)
    blockers.Add("base X-chirality diagnostics are not definite enough to define a sector rule");
blockers.Add("base X-chirality does not assign chargeSector and weakSector/quantumNumbers");
blockers.Add("no conjugation-pair evidence accompanies the base-chirality diagnostic");

var result = new
{
    phaseId = "phase137-base-chirality-route-audit",
    terminalStatus,
    baseChiralityRouteMaterialized = true,
    baseChiralityAvailable,
    allRowsHaveBaseChirality,
    anyDefiniteBaseChirality,
    baseChiralityRoutePromotable,
    rowAudits,
    phase136Gate = new
    {
        terminalStatus = JsonString(phase136.RootElement, "terminalStatus"),
        aliasTransferPromotable = JsonBool(phase136.RootElement, "aliasTransferPromotable"),
    },
    blockers,
    closureRequirements = new[]
    {
        "combine base-chirality diagnostics with an independent fermion charge/weak-sector derivation before assigning labels",
        "or materialize conjugation-pair evidence that turns chirality diagnostics into a transition rule",
        "rerun P133/P135 only after every repaired row receives chargeSector and weakSector/quantumNumbers with derivationId",
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase136Path = Phase136Path,
        spinorRepresentationPath = SpinorRepresentationPath,
        layoutPath = LayoutPath,
        fermionModesPath = FermionModesPath,
    },
};

var outOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "base_chirality_route_audit.json"), JsonSerializer.Serialize(result, outOptions));
File.WriteAllText(Path.Combine(outputDir, "base_chirality_route_audit_summary.json"), JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.baseChiralityRouteMaterialized,
    result.baseChiralityAvailable,
    result.allRowsHaveBaseChirality,
    result.anyDefiniteBaseChirality,
    result.baseChiralityRoutePromotable,
    result.rowAudits,
    result.blockers,
    result.closureRequirements,
}, outOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"baseChiralityAvailable={baseChiralityAvailable}");
Console.WriteLine($"allRowsHaveBaseChirality={allRowsHaveBaseChirality}");
Console.WriteLine($"baseChiralityRoutePromotable={baseChiralityRoutePromotable}");

static ProvenanceMeta Provenance() => new()
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "working-tree",
    Branch = new BranchRef { BranchId = "phase137-base-chirality-route-audit", SchemaVersion = "1.0.0" },
    Backend = "cpu",
};

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
