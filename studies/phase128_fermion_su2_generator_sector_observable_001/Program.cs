using System.Numerics;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase128_fermion_su2_generator_sector_observable_001/output";
const string Phase125EnrichedModesPath = "studies/phase125_source_join_family_metadata_materialization_001/output/phase95_l0_source_family_enriched_fermion_modes.json";
const string Phase126Path = "studies/phase126_fermion_sector_identity_source_audit_001/output/fermion_sector_identity_source_audit.json";
const string Phase127Path = "studies/phase127_fermion_gauge_basis_content_observable_001/output/fermion_gauge_basis_content_observable.json";
const string Phase12SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const int DimG = 3;
const double T3DominanceThreshold = 0.80;
const double T3DominanceGapThreshold = 0.20;
const double GeneratorPolarizationThreshold = 0.50;

var outputDir = Environment.GetEnvironmentVariable("PHASE128_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase126 = JsonDocument.Parse(File.ReadAllText(Phase126Path));
using var phase127 = JsonDocument.Parse(File.ReadAllText(Phase127Path));
using var enriched = JsonDocument.Parse(File.ReadAllText(Phase125EnrichedModesPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(Phase12SpinorRepresentationPath));

int spinorComponents = RequiredInt(spinor.RootElement, "spinorComponents");
var chiralityConvention = spinor.RootElement.GetProperty("chiralityConvention");
bool hasChirality = JsonBool(chiralityConvention, "hasChirality") is true;
string chiralityPhaseFactor = JsonString(chiralityConvention, "phaseFactor") ?? "unknown";

var records = enriched.RootElement.GetProperty("modes")
    .EnumerateArray()
    .Where(IsQualityMode)
    .Select(mode => BuildGeneratorSectorRecord(mode, spinorComponents))
    .ToList();

bool allQualityModesHavePromotableT3Sector = records.Count > 0 && records.All(r => r.PromotableT3Sector);
bool allQualityModesHaveCoherentGeneratorPolarization = records.Count > 0 && records.All(r => r.GeneratorPolarizationMagnitude >= GeneratorPolarizationThreshold);
bool generatorSectorObservablePromotable = allQualityModesHavePromotableT3Sector && allQualityModesHaveCoherentGeneratorPolarization;
bool wzTransitionRulePromotable = generatorSectorObservablePromotable && hasChirality && !string.Equals(chiralityPhaseFactor, "trivial", StringComparison.Ordinal);
string terminalStatus = wzTransitionRulePromotable
    ? "fermion-su2-generator-sector-observable-wz-rule-ready"
    : "fermion-su2-generator-sector-observable-mixed-sector-blocked";

var blockers = new List<string>();
if (records.Count == 0)
    blockers.Add("no quality repaired modes are available for SU(2) generator-sector extraction");
if (!allQualityModesHavePromotableT3Sector)
    blockers.Add("quality repaired fermion modes are mixed in the T3 eigenbasis and do not provide a dominant target-blind charge-sector label");
if (!allQualityModesHaveCoherentGeneratorPolarization)
    blockers.Add("quality repaired fermion modes have near-zero SU(2) generator polarization");
if (JsonBool(phase126.RootElement, "fermionSectorIdentityPromotable") is not true)
    blockers.Add("Phase126 found no independent target-blind fermion chargeSector or weak-sector identity table to validate generator-sector labels");
if (JsonBool(phase127.RootElement, "sectorRulePromotable") is not true)
    blockers.Add("Phase127 gauge-basis content was diagnostic but not promotable");
if (!hasChirality || string.Equals(chiralityPhaseFactor, "trivial", StringComparison.Ordinal))
    blockers.Add("Phase12 chirality remains trivial, so a W charged-current transition rule is still not defined");

var result = new
{
    phaseId = "phase128-fermion-su2-generator-sector-observable",
    terminalStatus,
    generatorMomentObservableMaterialized = true,
    generatorSectorObservablePromotable,
    wzTransitionRulePromotable,
    allQualityModesHavePromotableT3Sector,
    allQualityModesHaveCoherentGeneratorPolarization,
    t3DominanceThreshold = T3DominanceThreshold,
    t3DominanceGapThreshold = T3DominanceGapThreshold,
    generatorPolarizationThreshold = GeneratorPolarizationThreshold,
    representationConvention = new
    {
        gaugeRepresentation = "su2-adjoint-spin1",
        generatorConvention = "(T_a)_{bc} = -i epsilon_{abc}; trace(T_a T_b)=2 delta_ab",
        t3EigenbasisOrder = new[] { "m=-1", "m=0", "m=+1" },
    },
    spinorRepresentation = new
    {
        path = Phase12SpinorRepresentationPath,
        spinorComponents,
        hasChirality,
        chiralityPhaseFactor,
    },
    upstreamGates = new
    {
        phase126TerminalStatus = JsonString(phase126.RootElement, "terminalStatus"),
        phase126FermionSectorIdentityPromotable = JsonBool(phase126.RootElement, "fermionSectorIdentityPromotable"),
        phase127TerminalStatus = JsonString(phase127.RootElement, "terminalStatus"),
        phase127SectorRulePromotable = JsonBool(phase127.RootElement, "sectorRulePromotable"),
    },
    qualityModeCount = records.Count,
    qualityFamilyIds = records.Select(r => r.FamilyId).Where(id => id is not null).Distinct(StringComparer.Ordinal).ToArray(),
    modeGeneratorSectorRecords = records,
    diagnosis = wzTransitionRulePromotable
        ? "SU(2) generator-sector evidence is promotable to a W/Z transition rule."
        : "SU(2) generator-sector moments are materialized, but repaired quality modes are mixed and chirality remains trivial.",
    blockers,
    closureRequirements = new[]
    {
        "derive a target-blind fermion sector observable that produces stable non-mixed charge/weak-sector labels for the repaired families",
        "derive a nontrivial chirality or conjugation transition table before claiming a W charged-current rule",
        "rerun the corrected-operator transition sweep only after a promotable fermion sector table exists",
    },
    sourceEvidence = new
    {
        phase125EnrichedModesPath = Phase125EnrichedModesPath,
        phase126Path = Phase126Path,
        phase127Path = Phase127Path,
        phase12SpinorRepresentationPath = Phase12SpinorRepresentationPath,
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "fermion_su2_generator_sector_observable.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_su2_generator_sector_observable_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.generatorMomentObservableMaterialized,
        result.generatorSectorObservablePromotable,
        result.wzTransitionRulePromotable,
        result.qualityModeCount,
        result.qualityFamilyIds,
        modeGeneratorSectorRecords = records.Select(r => new
        {
            r.ModeIndex,
            r.FamilyId,
            r.GeneratorExpectation,
            r.GeneratorPolarizationMagnitude,
            r.T3EigenbasisFractions,
            r.DominantT3Eigenvalue,
            r.DominantT3Fraction,
            r.T3DominanceGap,
            r.PromotableT3Sector,
        }),
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"qualityModeCount={records.Count}");
Console.WriteLine($"generatorSectorObservablePromotable={generatorSectorObservablePromotable}");
Console.WriteLine($"wzTransitionRulePromotable={wzTransitionRulePromotable}");

static GeneratorSectorRecord BuildGeneratorSectorRecord(JsonElement mode, int spinorComponents)
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
    var generatorExpectation = new Complex[DimG];
    var generatorSecondMoment = new double[DimG];
    var t3EigenbasisWeights = new double[DimG];
    double totalEnergy = 0.0;

    for (int vertex = 0; vertex < vertexCount; vertex++)
    {
        for (int spinor = 0; spinor < spinorComponents; spinor++)
        {
            var psi = new[]
            {
                ReadCoefficient(coefficients, vertex, 0, spinor, spinorComponents),
                ReadCoefficient(coefficients, vertex, 1, spinor, spinorComponents),
                ReadCoefficient(coefficients, vertex, 2, spinor, spinorComponents),
            };
            totalEnergy += psi.Sum(Abs2);

            var tPsi = new[]
            {
                new[] { Complex.Zero, -Complex.ImaginaryOne * psi[2], Complex.ImaginaryOne * psi[1] },
                new[] { Complex.ImaginaryOne * psi[2], Complex.Zero, -Complex.ImaginaryOne * psi[0] },
                new[] { -Complex.ImaginaryOne * psi[1], Complex.ImaginaryOne * psi[0], Complex.Zero },
            };

            for (int a = 0; a < DimG; a++)
            {
                generatorExpectation[a] += Inner(psi, tPsi[a]);
                generatorSecondMoment[a] += tPsi[a].Sum(Abs2);
            }

            var t3Minus = (psi[0] + Complex.ImaginaryOne * psi[1]) / Math.Sqrt(2.0);
            var t3Zero = psi[2];
            var t3Plus = (psi[0] - Complex.ImaginaryOne * psi[1]) / Math.Sqrt(2.0);
            t3EigenbasisWeights[0] += Abs2(t3Minus);
            t3EigenbasisWeights[1] += Abs2(t3Zero);
            t3EigenbasisWeights[2] += Abs2(t3Plus);
        }
    }

    if (totalEnergy <= 0.0)
        throw new InvalidDataException($"Mode {RequiredString(mode, "modeId")} has zero eigenvector energy.");

    var expectation = generatorExpectation.Select(x => x.Real / totalEnergy).ToArray();
    var expectationImaginaryResiduals = generatorExpectation.Select(x => x.Imaginary / totalEnergy).ToArray();
    var secondMoments = generatorSecondMoment.Select(x => x / totalEnergy).ToArray();
    var variances = secondMoments.Select((x, i) => x - (expectation[i] * expectation[i])).ToArray();
    var t3Fractions = t3EigenbasisWeights.Select(x => x / totalEnergy).ToArray();
    var orderedT3 = t3Fractions
        .Select((fraction, index) => new { fraction, index, eigenvalue = index - 1 })
        .OrderByDescending(x => x.fraction)
        .ToArray();
    double t3Gap = orderedT3[0].fraction - orderedT3[1].fraction;
    bool promotableT3 = orderedT3[0].fraction >= T3DominanceThreshold && t3Gap >= T3DominanceGapThreshold;

    return new GeneratorSectorRecord(
        ModeId: RequiredString(mode, "modeId"),
        ModeIndex: RequiredInt(mode, "modeIndex"),
        FamilyId: JsonString(mode, "familyId"),
        SourceFermionModeId: JsonString(mode, "sourceFermionModeId"),
        SourceCanonicalFermionModeId: JsonString(mode, "sourceCanonicalFermionModeId"),
        VertexCount: vertexCount,
        ComplexCoefficientCount: complexLength,
        TotalEnergy: totalEnergy,
        GeneratorExpectation: expectation,
        GeneratorExpectationImaginaryResiduals: expectationImaginaryResiduals,
        GeneratorPolarizationMagnitude: Math.Sqrt(expectation.Sum(x => x * x)),
        GeneratorSecondMoments: secondMoments,
        GeneratorVariances: variances,
        QuadraticCasimirExpectation: secondMoments.Sum(),
        T3EigenbasisFractions: t3Fractions,
        DominantT3Eigenvalue: orderedT3[0].eigenvalue,
        DominantT3Fraction: orderedT3[0].fraction,
        T3DominanceGap: t3Gap,
        PromotableT3Sector: promotableT3);
}

static Complex ReadCoefficient(IReadOnlyList<double> coefficients, int vertex, int gauge, int spinor, int spinorComponents)
{
    int complexIndex = ((vertex * DimG + gauge) * spinorComponents) + spinor;
    return new Complex(coefficients[2 * complexIndex], coefficients[(2 * complexIndex) + 1]);
}

static Complex Inner(IReadOnlyList<Complex> left, IReadOnlyList<Complex> right)
{
    var sum = Complex.Zero;
    for (int i = 0; i < left.Count; i++)
        sum += Complex.Conjugate(left[i]) * right[i];
    return sum;
}

static double Abs2(Complex z) => (z.Real * z.Real) + (z.Imaginary * z.Imaginary);

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

sealed record GeneratorSectorRecord(
    string ModeId,
    int ModeIndex,
    string? FamilyId,
    string? SourceFermionModeId,
    string? SourceCanonicalFermionModeId,
    int VertexCount,
    int ComplexCoefficientCount,
    double TotalEnergy,
    IReadOnlyList<double> GeneratorExpectation,
    IReadOnlyList<double> GeneratorExpectationImaginaryResiduals,
    double GeneratorPolarizationMagnitude,
    IReadOnlyList<double> GeneratorSecondMoments,
    IReadOnlyList<double> GeneratorVariances,
    double QuadraticCasimirExpectation,
    IReadOnlyList<double> T3EigenbasisFractions,
    int DominantT3Eigenvalue,
    double DominantT3Fraction,
    double T3DominanceGap,
    bool PromotableT3Sector);
