using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase122_corrected_operator_selection_rule_sweep_001/output";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase69RelationPath = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency.json";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string WModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-0.json";
const string ZModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-2.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE122_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var modesDoc = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var relation = JsonDocument.Parse(File.ReadAllText(Phase69RelationPath));
using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));

double targetRaw = RequiredDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedRawMatrixElementMagnitude");
double targetWeak = RequiredDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedWeakCoupling");
double generatorScale = targetWeak / targetRaw;
double wInternal = RequiredDouble(relation.RootElement, "wInternalMass");
double zInternal = RequiredDouble(relation.RootElement, "zInternalMass");

var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase122-corrected-operator-selection-rule-sweep",
        Branch = new BranchRef { BranchId = "phase122-corrected-operator-selection-rule-sweep", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
const int DimG = 3;
var geometry = BuildGeometry(mesh);
var wVariation = BuildVariation(WModePath);
var zVariation = BuildVariation(ZModePath);
var modes = LoadModes(modesDoc.RootElement);

var records = new List<TransitionRecord>();
for (int i = 0; i < modes.Count; i++)
{
    for (int j = 0; j < modes.Count; j++)
    {
        var w = MatrixElement(wVariation.Re, wVariation.Im, modes[i].Coefficients, modes[j].Coefficients);
        var z = MatrixElement(zVariation.Re, zVariation.Im, modes[i].Coefficients, modes[j].Coefficients);
        var wRaw = Magnitude(w.Real, w.Imaginary);
        var zRaw = Magnitude(z.Real, z.Imaginary);
        var wWeak = wRaw * generatorScale;
        var zWeak = zRaw * generatorScale;
        var wBridge = wWeak / 2.0 / wInternal;
        var zBridge = zWeak / 2.0 / zInternal;
        var bridgeSpread = RelativeSpread(wBridge, zBridge);

        records.Add(new TransitionRecord
        {
            TransitionId = $"transition-{i}-{j}",
            ModeI = i,
            ModeJ = j,
            ModeIId = modes[i].ModeId,
            ModeJId = modes[j].ModeId,
            Diagonal = i == j,
            WReal = w.Real,
            WImaginary = w.Imaginary,
            WRawMatrixElementMagnitude = wRaw,
            ZReal = z.Real,
            ZImaginary = z.Imaginary,
            ZRawMatrixElementMagnitude = zRaw,
            WRawToTargetRatio = wRaw / targetRaw,
            ZRawToTargetRatio = zRaw / targetRaw,
            MinRawToTargetRatio = Math.Min(wRaw / targetRaw, zRaw / targetRaw),
            MaxRawToTargetRatio = Math.Max(wRaw / targetRaw, zRaw / targetRaw),
            WBridge = wBridge,
            ZBridge = zBridge,
            CommonBridgeRelativeSpread = bridgeSpread,
            CommonBridgePassed = bridgeSpread <= 0.05,
            QualityPassed = modes[i].QualityPassed && modes[j].QualityPassed,
            Blockers = modes[i].Blockers.Concat(modes[j].Blockers).Distinct(StringComparer.Ordinal).ToList(),
        });
    }
}

var qualityRecords = records.Where(r => r.QualityPassed).ToList();
var strongest = qualityRecords
    .OrderByDescending(r => r.MinRawToTargetRatio)
    .ThenBy(r => r.CommonBridgeRelativeSpread)
    .FirstOrDefault();
var strongestOffDiagonal = qualityRecords
    .Where(r => !r.Diagonal)
    .OrderByDescending(r => r.MinRawToTargetRatio)
    .ThenBy(r => r.CommonBridgeRelativeSpread)
    .FirstOrDefault();
var bestBridge = qualityRecords
    .OrderBy(r => r.CommonBridgeRelativeSpread)
    .ThenByDescending(r => r.MinRawToTargetRatio)
    .FirstOrDefault();
var selected03 = records.Single(r => r.ModeI == 0 && r.ModeJ == 3);
var projectionCandidate = qualityRecords
    .Where(r => r.CommonBridgePassed && r.WRawToTargetRatio >= 0.95 && r.ZRawToTargetRatio >= 0.95)
    .OrderByDescending(r => r.MinRawToTargetRatio)
    .FirstOrDefault();

string terminalStatus = projectionCandidate is not null
    ? "corrected-operator-selection-rule-sweep-found-projection-candidate"
    : "corrected-operator-selection-rule-sweep-no-transition-repair";

var result = new
{
    phaseId = "phase122-corrected-operator-selection-rule-sweep",
    terminalStatus,
    transitionCount = records.Count,
    qualityTransitionCount = qualityRecords.Count,
    targetImpliedRawMatrixElementMagnitude = targetRaw,
    targetImpliedWeakCoupling = targetWeak,
    generatorScale,
    phase120Gate = new
    {
        terminalStatus = JsonString(phase120.RootElement, "terminalStatus"),
        promotableAmplitudeMeasureFound = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound"),
        acceptedScale = phase120.RootElement.TryGetProperty("acceptedScale", out var acceptedScale)
            ? acceptedScale.Clone()
            : default(JsonElement?),
    },
    selectedPair03 = selected03,
    strongest,
    strongestOffDiagonal,
    bestBridge,
    projectionCandidate,
    topTransitionsByRaw = qualityRecords
        .OrderByDescending(r => r.MinRawToTargetRatio)
        .ThenBy(r => r.CommonBridgeRelativeSpread)
        .Take(20)
        .ToList(),
    topTransitionsByBridge = qualityRecords
        .OrderBy(r => r.CommonBridgeRelativeSpread)
        .ThenByDescending(r => r.MinRawToTargetRatio)
        .Take(20)
        .ToList(),
    diagnosis = projectionCandidate is not null
        ? new[]
        {
            "a corrected-operator fermion transition reaches the Phase110 raw target and W/Z common-bridge gate",
        }
        : new[]
        {
            "no corrected-operator Phase95 fermion transition reaches the Phase110 raw target with common W/Z bridge consistency",
            "the selected pair (0,3) is a near-null transition under the corrected operator",
            "the blocker is now transition selection/sector interpretation or bridge construction, not analytic operator normalization",
        },
    closureRequirements = projectionCandidate is not null
        ? new[]
        {
            "rerun Phase115 and Phase116 with the selected corrected-operator transition",
        }
        : new[]
        {
            "do not repair by scalar normalization or by reusing the stale pre-repair pair sweep",
            "derive the physical fermion transition/sector selection rule for W/Z source modes or revise the W/Z bridge construction",
        },
    sourceEvidence = new
    {
        phase95ModesPath = Phase95ModesPath,
        phase110ContractPath = Phase110ContractPath,
        phase69RelationPath = Phase69RelationPath,
        phase120Path = Phase120Path,
        wModePath = WModePath,
        zModePath = ZModePath,
    },
};

var options = JsonOptions();
File.WriteAllText(
    Path.Combine(outputDir, "corrected_operator_selection_rule_sweep.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "corrected_operator_selection_rule_sweep_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        terminalStatus,
        result.transitionCount,
        result.qualityTransitionCount,
        selectedPair03MinRawToTargetRatio = selected03.MinRawToTargetRatio,
        strongestTransitionId = strongest?.TransitionId,
        strongestMinRawToTargetRatio = strongest?.MinRawToTargetRatio,
        bestBridgeTransitionId = bestBridge?.TransitionId,
        bestBridgeSpread = bestBridge?.CommonBridgeRelativeSpread,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"transitionCount={records.Count}");
Console.WriteLine($"strongest={strongest?.TransitionId} minRawToTarget={strongest?.MinRawToTargetRatio:R}");
Console.WriteLine($"selected03MinRawToTarget={selected03.MinRawToTargetRatio:R}");

(double[,] Re, double[,] Im) BuildVariation(string modePath)
{
    var modeVector = LoadModeVector(modePath);
    return DiracVariationComputer.ComputeAnalytical(
        modeVector,
        gammas,
        mesh.VertexCount,
        spinorSpec.SpinorComponents,
        DimG,
        geometry.EdgeLengths,
        geometry.CellsPerEdge,
        geometry.EdgeDirections);
}

static IReadOnlyList<ModeRecord> LoadModes(JsonElement root)
{
    return root.GetProperty("modes")
        .EnumerateArray()
        .Select((mode, index) =>
        {
            var blockers = new List<string>();
            bool gaugeReduced = JsonBool(mode, "gaugeReductionApplied") is true;
            double residual = RequiredDouble(mode, "residualNorm");
            double branch = RequiredDouble(mode, "branchStabilityScore");
            double refinement = RequiredDouble(mode, "refinementStabilityScore");
            if (!gaugeReduced)
                blockers.Add("fermion mode was not gauge-reduced");
            if (residual > 1e-6)
                blockers.Add($"fermion residual norm {residual:R} exceeds 1E-06");
            if (branch < 0.5)
                blockers.Add($"fermion branch stability {branch:R} is below 0.5");
            if (refinement < 0.5)
                blockers.Add($"fermion refinement stability {refinement:R} is below 0.5");

            return new ModeRecord(
                index,
                RequiredString(mode, "modeId"),
                mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(v => v.GetDouble()).ToArray(),
                blockers.Count == 0,
                blockers);
        })
        .ToList();
}

static (double Real, double Imaginary) MatrixElement(double[,] matrixRe, double[,] matrixIm, double[] phiI, double[] phiJ)
{
    int n = matrixRe.GetLength(0);
    var iNorm = Normalize(phiI);
    var jNorm = Normalize(phiJ);
    var deltaJ = new double[n * 2];
    for (int row = 0; row < n; row++)
    {
        double sumRe = 0.0;
        double sumIm = 0.0;
        for (int col = 0; col < n; col++)
        {
            double aRe = matrixRe[row, col];
            double aIm = matrixIm[row, col];
            double bRe = jNorm[col * 2];
            double bIm = jNorm[col * 2 + 1];
            sumRe += aRe * bRe - aIm * bIm;
            sumIm += aRe * bIm + aIm * bRe;
        }

        deltaJ[row * 2] = sumRe;
        deltaJ[row * 2 + 1] = sumIm;
    }

    double real = 0.0;
    double imaginary = 0.0;
    for (int k = 0; k < n; k++)
    {
        double iRe = iNorm[k * 2];
        double iIm = iNorm[k * 2 + 1];
        double dRe = deltaJ[k * 2];
        double dIm = deltaJ[k * 2 + 1];
        real += iRe * dRe + iIm * dIm;
        imaginary += iRe * dIm - iIm * dRe;
    }

    return (real, imaginary);
}

static double[] Normalize(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(v => v * v));
    if (norm < 1e-30)
        return vector;
    return vector.Select(v => v / norm).ToArray();
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property {propertyName}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : throw new InvalidDataException($"Missing numeric property {propertyName}");

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);

static double RelativeSpread(double a, double b)
{
    double mean = (a + b) / 2.0;
    return Math.Abs(mean) > 0.0 ? Math.Abs(a - b) / Math.Abs(mean) : double.NaN;
}

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(v => v.GetDouble()).ToArray();
}

static (double[] EdgeLengths, int[][] CellsPerEdge, double[][] EdgeDirections) BuildGeometry(SimplicialMesh mesh)
{
    var edgeLengths = new double[mesh.EdgeCount];
    var cellsPerEdge = new int[mesh.EdgeCount][];
    var edgeDirections = new double[mesh.EdgeCount][];
    for (int edge = 0; edge < mesh.EdgeCount; edge++)
    {
        edgeLengths[edge] = EdgeLength(mesh, edge);
        cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
        edgeDirections[edge] = EdgeDirection(mesh, edge);
    }

    return (edgeLengths, cellsPerEdge, edgeDirections);
}

static double EdgeLength(SimplicialMesh mesh, int edge)
{
    var a = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
    var b = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++)
    {
        double d = b[i] - a[i];
        sum += d * d;
    }

    return Math.Sqrt(sum);
}

static double[] EdgeDirection(SimplicialMesh mesh, int edge)
{
    var a = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
    var b = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
    var direction = new double[a.Length];
    double norm = 0.0;
    for (int i = 0; i < a.Length; i++)
    {
        direction[i] = b[i] - a[i];
        norm += direction[i] * direction[i];
    }

    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
        for (int i = 0; i < direction.Length; i++)
            direction[i] /= norm;
    return direction;
}

public sealed record ModeRecord(
    int Index,
    string ModeId,
    double[] Coefficients,
    bool QualityPassed,
    IReadOnlyList<string> Blockers);

public sealed class TransitionRecord
{
    [JsonPropertyName("transitionId")]
    public required string TransitionId { get; init; }

    [JsonPropertyName("modeI")]
    public required int ModeI { get; init; }

    [JsonPropertyName("modeJ")]
    public required int ModeJ { get; init; }

    [JsonPropertyName("modeIId")]
    public required string ModeIId { get; init; }

    [JsonPropertyName("modeJId")]
    public required string ModeJId { get; init; }

    [JsonPropertyName("diagonal")]
    public required bool Diagonal { get; init; }

    [JsonPropertyName("wReal")]
    public required double WReal { get; init; }

    [JsonPropertyName("wImaginary")]
    public required double WImaginary { get; init; }

    [JsonPropertyName("wRawMatrixElementMagnitude")]
    public required double WRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("zReal")]
    public required double ZReal { get; init; }

    [JsonPropertyName("zImaginary")]
    public required double ZImaginary { get; init; }

    [JsonPropertyName("zRawMatrixElementMagnitude")]
    public required double ZRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("wRawToTargetRatio")]
    public required double WRawToTargetRatio { get; init; }

    [JsonPropertyName("zRawToTargetRatio")]
    public required double ZRawToTargetRatio { get; init; }

    [JsonPropertyName("minRawToTargetRatio")]
    public required double MinRawToTargetRatio { get; init; }

    [JsonPropertyName("maxRawToTargetRatio")]
    public required double MaxRawToTargetRatio { get; init; }

    [JsonPropertyName("wBridge")]
    public required double WBridge { get; init; }

    [JsonPropertyName("zBridge")]
    public required double ZBridge { get; init; }

    [JsonPropertyName("commonBridgeRelativeSpread")]
    public required double CommonBridgeRelativeSpread { get; init; }

    [JsonPropertyName("commonBridgePassed")]
    public required bool CommonBridgePassed { get; init; }

    [JsonPropertyName("qualityPassed")]
    public required bool QualityPassed { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}
