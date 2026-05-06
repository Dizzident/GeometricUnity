using System.Text.Json;

const string DefaultOutputDir = "studies/phase127_fermion_gauge_basis_content_observable_001/output";
const string Phase125EnrichedModesPath = "studies/phase125_source_join_family_metadata_materialization_001/output/phase95_l0_source_family_enriched_fermion_modes.json";
const string Phase125Path = "studies/phase125_source_join_family_metadata_materialization_001/output/source_join_family_metadata_materialization.json";
const string Phase126Path = "studies/phase126_fermion_sector_identity_source_audit_001/output/fermion_sector_identity_source_audit.json";
const string Phase12SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const int DimG = 3;
const double DominanceThreshold = 0.80;
const double DominanceGapThreshold = 0.20;

var outputDir = Environment.GetEnvironmentVariable("PHASE127_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase125 = JsonDocument.Parse(File.ReadAllText(Phase125Path));
using var phase126 = JsonDocument.Parse(File.ReadAllText(Phase126Path));
using var enriched = JsonDocument.Parse(File.ReadAllText(Phase125EnrichedModesPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(Phase12SpinorRepresentationPath));

int spinorComponents = RequiredInt(spinor.RootElement, "spinorComponents");
bool hasChirality = JsonBool(spinor.RootElement.GetProperty("chiralityConvention"), "hasChirality") is true;
string chiralityPhaseFactor = JsonString(spinor.RootElement.GetProperty("chiralityConvention"), "phaseFactor") ?? "unknown";

var qualityRecords = enriched.RootElement.GetProperty("modes")
    .EnumerateArray()
    .Where(IsQualityMode)
    .Select(mode => BuildGaugeBasisRecord(mode, spinorComponents))
    .ToList();

bool allQualityModesHavePromotableGaugeBasisSector = qualityRecords.Count > 0 && qualityRecords.All(r => r.PromotableGaugeBasisSector);
bool sectorRulePromotable = allQualityModesHavePromotableGaugeBasisSector
    && JsonBool(phase126.RootElement, "fermionSectorIdentityPromotable") is true;
string terminalStatus = sectorRulePromotable
    ? "fermion-gauge-basis-content-observable-sector-ready"
    : "fermion-gauge-basis-content-observable-mixed-sector-blocked";

var blockers = new List<string>();
if (qualityRecords.Count == 0)
    blockers.Add("no quality repaired modes are available for gauge-basis content extraction");
if (!allQualityModesHavePromotableGaugeBasisSector)
    blockers.Add("quality repaired fermion modes are mixed across SU(2) gauge-basis components and do not provide a dominant target-blind sector label");
if (JsonBool(phase126.RootElement, "fermionSectorIdentityPromotable") is not true)
    blockers.Add("Phase126 found no target-blind fermion chargeSector or weak-sector identity source to combine with gauge-basis content");
if (!hasChirality || string.Equals(chiralityPhaseFactor, "trivial", StringComparison.Ordinal))
    blockers.Add("Phase12 spinor representation has trivial chirality, so gauge-basis content alone cannot define W charged-current chirality selection");

var result = new
{
    phaseId = "phase127-fermion-gauge-basis-content-observable",
    terminalStatus,
    gaugeBasisObservableMaterialized = true,
    sectorRulePromotable,
    allQualityModesHavePromotableGaugeBasisSector,
    dominanceThreshold = DominanceThreshold,
    dominanceGapThreshold = DominanceGapThreshold,
    layoutConvention = "complex coefficients indexed as ((vertex * dimG + gauge) * spinorComponents + spinor), with real/imag interleaved in the stored vector",
    spinorRepresentation = new
    {
        path = Phase12SpinorRepresentationPath,
        spinorComponents,
        hasChirality,
        chiralityPhaseFactor,
    },
    phase125Gate = new
    {
        terminalStatus = JsonString(phase125.RootElement, "terminalStatus"),
        familyMetadataMaterialized = JsonBool(phase125.RootElement, "familyMetadataMaterialized"),
    },
    phase126Gate = new
    {
        terminalStatus = JsonString(phase126.RootElement, "terminalStatus"),
        fermionSectorIdentityPromotable = JsonBool(phase126.RootElement, "fermionSectorIdentityPromotable"),
    },
    qualityModeCount = qualityRecords.Count,
    qualityFamilyIds = qualityRecords.Select(r => r.FamilyId).Where(id => id is not null).Distinct(StringComparer.Ordinal).ToArray(),
    modeGaugeBasisRecords = qualityRecords,
    diagnosis = sectorRulePromotable
        ? "Gauge-basis content and Phase126 sector identity evidence are both promotable."
        : "Gauge-basis content is materialized, but the repaired quality modes remain mixed and Phase126 sector identity labels are still absent.",
    blockers,
    closureRequirements = new[]
    {
        "derive or materialize a target-blind fermion sector identity observable beyond gauge-basis energy fractions",
        "require nontrivial charge/weak-sector labels, or an equivalent nontrivial chirality/conjugation transition table, before rerunning the corrected W/Z transition sweep",
        "keep the gauge-basis content observable as diagnostic evidence, not as a physical charge-sector assignment, unless a future derivation validates that interpretation",
    },
    sourceEvidence = new
    {
        phase125EnrichedModesPath = Phase125EnrichedModesPath,
        phase125Path = Phase125Path,
        phase126Path = Phase126Path,
        phase12SpinorRepresentationPath = Phase12SpinorRepresentationPath,
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "fermion_gauge_basis_content_observable.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_gauge_basis_content_observable_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.gaugeBasisObservableMaterialized,
        result.sectorRulePromotable,
        result.allQualityModesHavePromotableGaugeBasisSector,
        result.qualityModeCount,
        result.qualityFamilyIds,
        modeGaugeBasisRecords = qualityRecords.Select(r => new
        {
            r.ModeIndex,
            r.FamilyId,
            r.DominantGaugeBasisIndex,
            r.DominantGaugeBasisFraction,
            r.DominanceGap,
            r.GaugeBasisEntropyNormalized,
            r.PromotableGaugeBasisSector,
        }),
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"qualityModeCount={qualityRecords.Count}");
Console.WriteLine($"allQualityModesHavePromotableGaugeBasisSector={allQualityModesHavePromotableGaugeBasisSector}");
Console.WriteLine($"sectorRulePromotable={sectorRulePromotable}");

static GaugeBasisRecord BuildGaugeBasisRecord(JsonElement mode, int spinorComponents)
{
    var coefficients = mode.GetProperty("eigenvectorCoefficients")
        .EnumerateArray()
        .Select(v => v.GetDouble())
        .ToArray();
    if (coefficients.Length % 2 != 0)
        throw new InvalidDataException($"Mode {RequiredString(mode, "modeId")} has an odd real/imag coefficient vector length.");

    int complexLength = coefficients.Length / 2;
    int dofsPerVertex = DimG * spinorComponents;
    if (complexLength % dofsPerVertex != 0)
        throw new InvalidDataException(
            $"Mode {RequiredString(mode, "modeId")} complex length {complexLength} is not divisible by dimG*spinorComponents={dofsPerVertex}.");

    int vertexCount = complexLength / dofsPerVertex;
    var gaugeEnergy = new double[DimG];
    var spinorEnergy = new double[spinorComponents];
    double totalEnergy = 0.0;

    for (int vertex = 0; vertex < vertexCount; vertex++)
    {
        for (int gauge = 0; gauge < DimG; gauge++)
        {
            for (int spinor = 0; spinor < spinorComponents; spinor++)
            {
                int complexIndex = ((vertex * DimG + gauge) * spinorComponents) + spinor;
                double re = coefficients[2 * complexIndex];
                double im = coefficients[(2 * complexIndex) + 1];
                double energy = (re * re) + (im * im);
                gaugeEnergy[gauge] += energy;
                spinorEnergy[spinor] += energy;
                totalEnergy += energy;
            }
        }
    }

    if (totalEnergy <= 0.0)
        throw new InvalidDataException($"Mode {RequiredString(mode, "modeId")} has zero eigenvector energy.");

    var gaugeFractions = gaugeEnergy.Select(e => e / totalEnergy).ToArray();
    var spinorFractions = spinorEnergy.Select(e => e / totalEnergy).ToArray();
    var ordered = gaugeFractions
        .Select((fraction, index) => new { fraction, index })
        .OrderByDescending(x => x.fraction)
        .ToArray();
    double dominanceGap = ordered[0].fraction - ordered[1].fraction;
    bool promotable = ordered[0].fraction >= DominanceThreshold && dominanceGap >= DominanceGapThreshold;

    return new GaugeBasisRecord(
        ModeId: RequiredString(mode, "modeId"),
        ModeIndex: RequiredInt(mode, "modeIndex"),
        FamilyId: JsonString(mode, "familyId"),
        SourceFermionModeId: JsonString(mode, "sourceFermionModeId"),
        SourceCanonicalFermionModeId: JsonString(mode, "sourceCanonicalFermionModeId"),
        VertexCount: vertexCount,
        ComplexCoefficientCount: complexLength,
        TotalEnergy: totalEnergy,
        GaugeBasisEnergyFractions: gaugeFractions,
        SpinorEnergyFractions: spinorFractions,
        DominantGaugeBasisIndex: ordered[0].index,
        DominantGaugeBasisFraction: ordered[0].fraction,
        DominanceGap: dominanceGap,
        GaugeBasisEntropyNormalized: NormalizedEntropy(gaugeFractions),
        PromotableGaugeBasisSector: promotable);
}

static double NormalizedEntropy(IReadOnlyList<double> fractions)
{
    double entropy = 0.0;
    foreach (double fraction in fractions)
    {
        if (fraction > 0.0)
            entropy -= fraction * Math.Log(fraction);
    }

    return entropy / Math.Log(fractions.Count);
}

static bool IsQualityMode(JsonElement mode)
{
    return (JsonDouble(mode, "branchStabilityScore") ?? 0.0) >= 0.5
        && (JsonDouble(mode, "refinementStabilityScore") ?? 0.0) >= 0.5
        && (JsonDouble(mode, "residualNorm") ?? double.PositiveInfinity) <= 1e-6
        && JsonBool(mode, "gaugeReductionApplied") is true;
}

static string RequiredString(JsonElement element, string propertyName)
{
    return JsonString(element, propertyName)
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static int RequiredInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetInt32()
        : throw new InvalidDataException($"Missing integer property '{propertyName}'.");
}

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static double? JsonDouble(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
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

sealed record GaugeBasisRecord(
    string ModeId,
    int ModeIndex,
    string? FamilyId,
    string? SourceFermionModeId,
    string? SourceCanonicalFermionModeId,
    int VertexCount,
    int ComplexCoefficientCount,
    double TotalEnergy,
    IReadOnlyList<double> GaugeBasisEnergyFractions,
    IReadOnlyList<double> SpinorEnergyFractions,
    int DominantGaugeBasisIndex,
    double DominantGaugeBasisFraction,
    double DominanceGap,
    double GaugeBasisEntropyNormalized,
    bool PromotableGaugeBasisSector);
