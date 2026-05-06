using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase120_analytic_variation_measure_consistency_001/output";
const string RunRoot = "studies/phase12_joined_calculation_001/output/background_family";
const string Phase119Path = "studies/phase119_operator_source_scale_derivation_audit_001/output/operator_source_scale_derivation_audit.json";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string WModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-0.json";
const string ZModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-2.json";
const string WFiniteDifferenceMatrixPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations/variation-bg-phase12-bg-a-20260315212202-candidate-0.matrix.json";
const string ZFiniteDifferenceMatrixPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations/variation-bg-phase12-bg-a-20260315212202-candidate-2.matrix.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE120_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase119 = JsonDocument.Parse(File.ReadAllText(Phase119Path));
using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));

var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase120-analytic-variation-measure-consistency",
        Branch = new() { BranchId = "phase120-analytic-variation-measure-consistency", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dimG = 3;
int spinorDim = spinorSpec.SpinorComponents;
var edgeLengths = new double[mesh.EdgeCount];
var edgeDirections = new double[mesh.EdgeCount][];
var cellsPerEdge = new int[mesh.EdgeCount][];
for (int edge = 0; edge < mesh.EdgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

var wRecord = BuildRecord("w-boson", "phase12-candidate-0", WModePath, WFiniteDifferenceMatrixPath);
var zRecord = BuildRecord("z-boson", "phase12-candidate-2", ZModePath, ZFiniteDifferenceMatrixPath);
double commonScaleMean = (wRecord.BestFitFiniteDifferenceToAnalyticScale + zRecord.BestFitFiniteDifferenceToAnalyticScale) / 2.0;
double commonScaleRelativeSpread = RelativeSpread(
    wRecord.BestFitFiniteDifferenceToAnalyticScale,
    zRecord.BestFitFiniteDifferenceToAnalyticScale);
double maxScaledResidual = Math.Max(wRecord.BestFitRelativeResidual, zRecord.BestFitRelativeResidual);

const double ResidualTolerance = 1e-8;
const double CommonScaleSpreadTolerance = 0.05;
bool promotable = maxScaledResidual <= ResidualTolerance &&
                  commonScaleRelativeSpread <= CommonScaleSpreadTolerance &&
                  double.IsFinite(commonScaleMean) &&
                  commonScaleMean > 0.0;

var terminalStatus = promotable
    ? "analytic-variation-amplitude-measure-derived"
    : "analytic-variation-amplitude-measure-consistency-blocked";

var closure = new List<string>();
if (maxScaledResidual > ResidualTolerance)
    closure.Add("analytic replay variation does not reproduce persisted Phase12 finite-difference variation matrices after best-fit scaling");
if (commonScaleRelativeSpread > CommonScaleSpreadTolerance)
    closure.Add("best-fit analytic-to-finite-difference scales are not common across W and Z");
if (!double.IsFinite(commonScaleMean) || commonScaleMean <= 0.0)
    closure.Add("best-fit analytic-to-finite-difference common scale is not finite and positive");

var result = new
{
    phaseId = "phase120-analytic-variation-measure-consistency",
    terminalStatus,
    promotableAmplitudeMeasureFound = promotable,
    acceptedScale = promotable
        ? new
        {
            scaleId = "phase120-analytic-dirac-variation-to-phase12-finite-difference-scale",
            finiteDifferenceToAnalyticScale = commonScaleMean,
            analyticToFiniteDifferenceScale = 1.0 / commonScaleMean,
            normalizationConvention = "phase12-finite-difference-dirac-variation-measure",
            targetIndependent = true,
        }
        : null,
    validationGate = new
    {
        residualTolerance = ResidualTolerance,
        commonScaleSpreadTolerance = CommonScaleSpreadTolerance,
        maxScaledResidual,
        commonScaleRelativeSpread,
        passes = promotable,
    },
    records = new[] { wRecord, zRecord },
    diagnosis = promotable
        ? new[]
        {
            "analytic replay variation has a derivation-backed target-independent amplitude measure against Phase12 finite-difference operators",
            "the fitted scale is common across W and Z within tolerance",
        }
        : new[]
        {
            "the analytic replay variation and persisted Phase12 finite-difference variation are not connected by a clean common scale",
            "the remaining blocker is an operator-form mismatch or source-mode interpretation mismatch, not a scalar normalization only",
        },
    closureRequirements = closure,
    sourceEvidence = new
    {
        phase119Path = Phase119Path,
        phase119TerminalStatus = JsonString(phase119.RootElement, "terminalStatus"),
        spinorRepresentationPath = SpinorRepresentationPath,
        runRoot = RunRoot,
        wModePath = WModePath,
        zModePath = ZModePath,
        wFiniteDifferenceMatrixPath = WFiniteDifferenceMatrixPath,
        zFiniteDifferenceMatrixPath = ZFiniteDifferenceMatrixPath,
    },
};

var options = JsonOptions();
var resultPath = Path.Combine(outputDir, "analytic_variation_measure_consistency.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "analytic_variation_measure_consistency_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        terminalStatus,
        promotableAmplitudeMeasureFound = promotable,
        commonScaleMean,
        commonScaleRelativeSpread,
        maxScaledResidual,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"promotableAmplitudeMeasureFound={promotable}");
Console.WriteLine($"commonScaleMean={commonScaleMean:R}");
Console.WriteLine($"commonScaleRelativeSpread={commonScaleRelativeSpread:R}");
Console.WriteLine($"maxScaledResidual={maxScaledResidual:R}");
Console.WriteLine($"resultPath={resultPath}");

VariationConsistencyRecord BuildRecord(
    string particleId,
    string sourceCandidateId,
    string modePath,
    string finiteDifferenceMatrixPath)
{
    var modeVector = LoadModeVector(modePath);
    using var matrixDoc = JsonDocument.Parse(File.ReadAllText(finiteDifferenceMatrixPath));
    var finiteRe = LoadMatrix(matrixDoc.RootElement.GetProperty("real"));
    var finiteIm = LoadMatrix(matrixDoc.RootElement.GetProperty("imag"));

    var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
        modeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);

    double finiteNorm = FrobeniusNorm(finiteRe, finiteIm);
    double analyticNorm = FrobeniusNorm(analyticRe, analyticIm);
    double scale = BestFitScale(finiteRe, finiteIm, analyticRe, analyticIm);
    double residual = RelativeResidual(finiteRe, finiteIm, analyticRe, analyticIm, scale);
    double unitScaleResidual = RelativeResidual(finiteRe, finiteIm, analyticRe, analyticIm, 1.0);

    return new VariationConsistencyRecord
    {
        ParticleId = particleId,
        SourceCandidateId = sourceCandidateId,
        ModePath = modePath,
        FiniteDifferenceMatrixPath = finiteDifferenceMatrixPath,
        ModeVectorLength = modeVector.Length,
        ModeVectorNorm = Math.Sqrt(modeVector.Sum(v => v * v)),
        MatrixShape = [finiteRe.GetLength(0), finiteRe.GetLength(1)],
        FiniteDifferenceFrobeniusNorm = finiteNorm,
        AnalyticFrobeniusNorm = analyticNorm,
        BestFitFiniteDifferenceToAnalyticScale = scale,
        BestFitRelativeResidual = residual,
        UnitScaleRelativeResidual = unitScaleResidual,
    };
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

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(v => v.GetDouble()).ToArray();
}

static double[,] LoadMatrix(JsonElement array)
{
    var rows = array.EnumerateArray().ToArray();
    int rowCount = rows.Length;
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rowCount, colCount];
    for (int r = 0; r < rowCount; r++)
    {
        var cols = rows[r].EnumerateArray().ToArray();
        if (cols.Length != colCount)
            throw new InvalidDataException("Matrix rows must have consistent lengths.");
        for (int c = 0; c < colCount; c++)
            matrix[r, c] = cols[c].GetDouble();
    }

    return matrix;
}

static double BestFitScale(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm)
{
    double dot = 0.0;
    double norm2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            dot += targetRe[r, c] * sourceRe[r, c] + targetIm[r, c] * sourceIm[r, c];
            norm2 += sourceRe[r, c] * sourceRe[r, c] + sourceIm[r, c] * sourceIm[r, c];
        }

    return norm2 > 0.0 ? dot / norm2 : double.NaN;
}

static double RelativeResidual(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm, double scale)
{
    double residual2 = 0.0;
    double target2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            double dRe = targetRe[r, c] - scale * sourceRe[r, c];
            double dIm = targetIm[r, c] - scale * sourceIm[r, c];
            residual2 += dRe * dRe + dIm * dIm;
            target2 += targetRe[r, c] * targetRe[r, c] + targetIm[r, c] * targetIm[r, c];
        }

    return target2 > 0.0 ? Math.Sqrt(residual2 / target2) : double.NaN;
}

static double FrobeniusNorm(double[,] re, double[,] im)
{
    double sum = 0.0;
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            sum += re[r, c] * re[r, c] + im[r, c] * im[r, c];
    return Math.Sqrt(sum);
}

static double RelativeSpread(double a, double b)
{
    double mean = (a + b) / 2.0;
    return Math.Abs(mean) > 0.0 ? Math.Abs(a - b) / Math.Abs(mean) : double.NaN;
}

static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    double norm = 0.0;
    for (int k = 0; k < coords0.Length; k++)
    {
        double d = coords1[k] - coords0[k];
        norm += d * d;
    }

    return Math.Sqrt(norm);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    int dim = mesh.EmbeddingDimension;
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    var dir = new double[dim];
    double norm = 0.0;
    for (int k = 0; k < dim; k++)
    {
        dir[k] = coords1[k] - coords0[k];
        norm += dir[k] * dir[k];
    }

    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
        for (int k = 0; k < dim; k++)
            dir[k] /= norm;
    return dir;
}

public sealed class VariationConsistencyRecord
{
    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("modePath")]
    public required string ModePath { get; init; }

    [JsonPropertyName("finiteDifferenceMatrixPath")]
    public required string FiniteDifferenceMatrixPath { get; init; }

    [JsonPropertyName("modeVectorLength")]
    public required int ModeVectorLength { get; init; }

    [JsonPropertyName("modeVectorNorm")]
    public required double ModeVectorNorm { get; init; }

    [JsonPropertyName("matrixShape")]
    public required IReadOnlyList<int> MatrixShape { get; init; }

    [JsonPropertyName("finiteDifferenceFrobeniusNorm")]
    public required double FiniteDifferenceFrobeniusNorm { get; init; }

    [JsonPropertyName("analyticFrobeniusNorm")]
    public required double AnalyticFrobeniusNorm { get; init; }

    [JsonPropertyName("bestFitFiniteDifferenceToAnalyticScale")]
    public required double BestFitFiniteDifferenceToAnalyticScale { get; init; }

    [JsonPropertyName("bestFitRelativeResidual")]
    public required double BestFitRelativeResidual { get; init; }

    [JsonPropertyName("unitScaleRelativeResidual")]
    public required double UnitScaleRelativeResidual { get; init; }
}
