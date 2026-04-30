using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;
using Gu.Phase5.Reporting;

const string RunRoot = "studies/phase12_joined_calculation_001/output/background_family";
const string BackgroundId = "bg-phase12-bg-a-20260315212202";
const string BosonModeId = "bg-phase12-bg-a-20260315212202-mode-0";
const string OutputDir = "studies/phase84_first_boson_prediction_attempt_001/output";

Directory.CreateDirectory(OutputDir);
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
};

var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase84-first-boson-prediction-attempt",
    Branch = new BranchRef
    {
        BranchId = "phase84-first-boson-prediction-attempt",
        SchemaVersion = "1.0.0",
    },
    Backend = "cpu-reference",
    Notes = "First source-backed boson replay prediction attempt over persisted Phase12 artifacts.",
};

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
var edgeLengths = new double[mesh.EdgeCount];
var edgeDirections = new double[mesh.EdgeCount][];
var cellsPerEdge = new int[mesh.EdgeCount][];
for (int edge = 0; edge < mesh.EdgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

var spinorSpec = LoadJson<SpinorRepresentationSpec>(Path.Combine(RunRoot, "fermions", "spinor_representation.json"));
var gammas = new GammaMatrixBuilder().Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);
int spinorDim = spinorSpec.SpinorComponents;
int dimG = 3;

var fermionModes = LoadJson<FermionModeBundle>(Path.Combine(RunRoot, "fermions", $"fermion_modes_{BackgroundId}.json"));
var modeI = fermionModes.Modes[0];
var modeJ = fermionModes.Modes.Count > 1 ? fermionModes.Modes[1] : fermionModes.Modes[0];

var bosonModePath = Path.Combine(RunRoot, "spectra", "modes", $"{BosonModeId}.json");
var bosonModeJson = File.ReadAllText(bosonModePath);

var replay = SourceBackedAnalyticReplayPackageRunner.Run(
    packageId: "phase84-first-source-backed-boson-replay-package",
    sourceArtifactId: bosonModePath,
    bosonModeJson: bosonModeJson,
    gammas: gammas,
    cellCount: mesh.VertexCount,
    spinorDim: spinorDim,
    dimG: dimG,
    edgeLengths: edgeLengths,
    cellsPerEdge: cellsPerEdge,
    edgeDirections: edgeDirections,
    modeI: modeI,
    modeJ: modeJ,
    provenanceId: "phase84:first-source-backed-boson-replay:cpu",
    provenance: provenance);

var physicalGateBlockers = BuildPhysicalGateBlockers(modeI, modeJ);
var predictionAttempt = new FirstBosonPredictionAttemptArtifact
{
    PhaseId = "phase84-first-boson-prediction-attempt",
    TerminalStatus = replay.TerminalStatus == "source-backed-analytic-replay-package-built" && physicalGateBlockers.Count == 0
        ? "first-boson-prediction-ready-for-comparison"
        : "first-boson-prediction-blocked",
    SelectedBosonModeId = BosonModeId,
    SelectedFermionModeIds = [modeI.ModeId, modeJ.ModeId],
    Geometry = new GeometrySummary
    {
        MeshSource = "ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh",
        VertexCount = mesh.VertexCount,
        EdgeCount = mesh.EdgeCount,
        EmbeddingDimension = mesh.EmbeddingDimension,
        ExpectedBosonVectorLength = mesh.EdgeCount * dimG,
        ExpectedFermionEigenvectorLength = mesh.VertexCount * spinorDim * dimG * 2,
    },
    ReplayTerminalStatus = replay.TerminalStatus,
    ReplayClosureRequirements = replay.ClosureRequirements,
    CouplingMagnitude = replay.FullReplayPackage?.CouplingRecord.CouplingProxyMagnitude,
    CouplingReal = replay.FullReplayPackage?.CouplingRecord.CouplingProxyReal,
    CouplingImaginary = replay.FullReplayPackage?.CouplingRecord.CouplingProxyImaginary,
    RawMatrixElementEvidenceStatus = replay.FullReplayPackage?.EvidenceBuild.EvidenceValidation.TerminalStatus,
    ProductionMaterializationStatus = replay.FullReplayPackage?.MaterializationAudit.TerminalStatus,
    PhysicalPredictionGateBlockers = physicalGateBlockers,
    CanCompareToExternalBosonValues = replay.TerminalStatus == "source-backed-analytic-replay-package-built" && physicalGateBlockers.Count == 0,
    Notes =
    [
        "This is a source-backed replay attempt, not a validated physical Standard Model boson prediction unless the physical gate is clear.",
        "No external W/Z target value is used to choose this candidate or compute the coupling.",
    ],
};

File.WriteAllText(
    Path.Combine(OutputDir, "first_boson_prediction_attempt.json"),
    JsonSerializer.Serialize(predictionAttempt, jsonOptions));

if (replay.FullReplayPackage is not null)
{
    var summaryPackage = new
    {
        replay.FullReplayPackage.PackageId,
        replay.FullReplayPackage.BosonModeId,
        replay.FullReplayPackage.BosonModeSourceKind,
        Coupling = replay.FullReplayPackage.CouplingRecord,
        EvidenceStatus = replay.FullReplayPackage.EvidenceBuild.EvidenceValidation.TerminalStatus,
        MaterializationStatus = replay.FullReplayPackage.MaterializationAudit.TerminalStatus,
        replay.FullReplayPackage.ClosureRequirements,
    };
    File.WriteAllText(
        Path.Combine(OutputDir, "first_boson_replay_package_summary.json"),
        JsonSerializer.Serialize(summaryPackage, jsonOptions));
}

Console.WriteLine(predictionAttempt.TerminalStatus);
Console.WriteLine($"couplingMagnitude={predictionAttempt.CouplingMagnitude:R}");
foreach (var blocker in physicalGateBlockers)
    Console.WriteLine($"blocker: {blocker}");

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

static List<string> BuildPhysicalGateBlockers(FermionModeRecord modeI, FermionModeRecord modeJ)
{
    var blockers = new List<string>();
    AddModeBlockers("fermion mode I", modeI, blockers);
    AddModeBlockers("fermion mode J", modeJ, blockers);
    return blockers.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
}

static void AddModeBlockers(string label, FermionModeRecord mode, List<string> blockers)
{
    const double ResidualTolerance = 1e-6;
    if (!mode.GaugeReductionApplied)
        blockers.Add($"{label} was not gauge-reduced");
    if (!double.IsFinite(mode.ResidualNorm) || mode.ResidualNorm > ResidualTolerance)
        blockers.Add($"{label} residual norm {mode.ResidualNorm:R} exceeds {ResidualTolerance:R}");
    if (mode.BranchStabilityScore < 0.5)
        blockers.Add($"{label} branch stability {mode.BranchStabilityScore:R} is below 0.5");
    if (mode.RefinementStabilityScore < 0.5)
        blockers.Add($"{label} refinement stability {mode.RefinementStabilityScore:R} is below 0.5");
}

T LoadJson<T>(string path)
{
    return JsonSerializer.Deserialize<T>(File.ReadAllText(path), jsonOptions)
        ?? throw new InvalidDataException($"Failed to deserialize {path}");
}

public sealed class FermionModeBundle
{
    [JsonPropertyName("modes")]
    public required List<FermionModeRecord> Modes { get; init; }
}

public sealed class FirstBosonPredictionAttemptArtifact
{
    public required string PhaseId { get; init; }
    public required string TerminalStatus { get; init; }
    public required string SelectedBosonModeId { get; init; }
    public required IReadOnlyList<string> SelectedFermionModeIds { get; init; }
    public required GeometrySummary Geometry { get; init; }
    public required string ReplayTerminalStatus { get; init; }
    public required IReadOnlyList<string> ReplayClosureRequirements { get; init; }
    public required double? CouplingMagnitude { get; init; }
    public required double? CouplingReal { get; init; }
    public required double? CouplingImaginary { get; init; }
    public required string? RawMatrixElementEvidenceStatus { get; init; }
    public required string? ProductionMaterializationStatus { get; init; }
    public required IReadOnlyList<string> PhysicalPredictionGateBlockers { get; init; }
    public required bool CanCompareToExternalBosonValues { get; init; }
    public required IReadOnlyList<string> Notes { get; init; }
}

public sealed class GeometrySummary
{
    public required string MeshSource { get; init; }
    public required int VertexCount { get; init; }
    public required int EdgeCount { get; init; }
    public required int EmbeddingDimension { get; init; }
    public required int ExpectedBosonVectorLength { get; init; }
    public required int ExpectedFermionEigenvectorLength { get; init; }
}
