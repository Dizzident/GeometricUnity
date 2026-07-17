using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string PhaseId = "phase509-phase481-anisotropic-cpu-reference-feasibility";
const string PhaseDir = "studies/phase509_phase481_anisotropic_cpu_reference_feasibility_001";
const string ContractPath = PhaseDir + "/preregistration/phase509_anisotropic_cpu_reference_contract_v1.json";
const string HistoricalScalarN3TopologySha256 = "c5de1e32d34c4e7b8b2af61f30d150d8847165658e7d66890772934712dd6722";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
JsonElement expected = contract.GetProperty("expectedPhase508");
string[] taxonomy = Strings(contract, "terminalTaxonomyInPrecedenceOrder");
int[] fixtureExtents = Ints(contract, "reducedFixtureExtents");
bool contractValid = S(contract, "contractId") == "phase509-a14-anisotropic-cpu-reference-feasibility-v1"
    && B(contract, "frozenBeforeEvaluation")
    && fixtureExtents.SequenceEqual(new[] { 3, 3, 3, 4 })
    && Strings(contract, "requiredControls").Length == 9
    && taxonomy.SequenceEqual(new[] { "invalid-precursor-or-contract", "anisotropic-cpu-reference-controls-failed", "anisotropic-cpu-reference-controls-ready" })
    && !B(contract, "fullT16OrT32MeshConstructionAllowed") && !B(contract, "phase481PackCreationAllowed")
    && !B(contract, "samplingOrReprocessingAllowed") && !B(contract, "benchmarkAllowed")
    && !B(contract, "hmcAllowed") && !B(contract, "phase480SatisfiedByThisPhase")
    && !B(contract, "physicalClaimAllowed");

string phase508Path = S(expected, "path");
byte[] phase508Bytes = File.ReadAllBytes(phase508Path);
string phase508Sha = Sha(phase508Bytes);
using JsonDocument phase508Document = JsonDocument.Parse(phase508Bytes);
JsonElement phase508 = phase508Document.RootElement;
bool phase508Valid = phase508Sha == S(expected, "sha256")
    && S(phase508, "phaseId") == "phase508-phase481-acquisition-geometry-closure"
    && S(phase508, "verdictKind") == S(expected, "requiredVerdictKind")
    && B(phase508, "inputsValid") && B(phase508, "a14BoundaryHeld")
    && Ints(phase508.GetProperty("selectedExtents"), "t16").SequenceEqual(Ints(expected, "requiredT16Extents"))
    && Ints(phase508.GetProperty("selectedExtents"), "t32").SequenceEqual(Ints(expected, "requiredT32Extents"))
    && I(phase508, "promotedPhysicalMassClaimCount") == 0;

var controls = new List<ControlResult>();
void Control(string id, Func<(bool Passed, string Detail)> evaluate)
{
    try
    {
        var outcome = evaluate();
        controls.Add(new(id, outcome.Passed, outcome.Detail));
    }
    catch (Exception exception)
    {
        controls.Add(new(id, false, exception.GetType().Name + ": " + exception.Message));
    }
}

SimplicialMesh scalarDefault = SimplicialMeshGenerator.CreateUniform4DPeriodic(3);
SimplicialMesh scalar = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true);
SimplicialMesh isotropicVector = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, 3, 3, 3, latticeCanonical: true);
SimplicialMesh anisotropic = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, 3, 3, 4, latticeCanonical: true);

Control("scalar-api-prechange-topology-hash", () =>
{
    string hash = TopologyHash(scalarDefault);
    return (hash == HistoricalScalarN3TopologySha256, hash);
});
Control("isotropic-vector-overload-topology-parity", () =>
{
    string scalarHash = TopologyHash(scalar);
    string vectorHash = TopologyHash(isotropicVector);
    return (scalarHash == vectorHash, $"scalar={scalarHash};vector={vectorHash}");
});
Control("anisotropic-exact-simplex-counts", () =>
{
    const int sites = 108;
    bool passed = anisotropic.VertexCount == sites && anisotropic.EdgeCount == 15 * sites
        && anisotropic.FaceCount == 50 * sites && anisotropic.VolumeCount == 60 * sites
        && anisotropic.CellCount == 24 * sites;
    return (passed, $"V={anisotropic.VertexCount};E={anisotropic.EdgeCount};F={anisotropic.FaceCount};W={anisotropic.VolumeCount};C={anisotropic.CellCount}");
});
Control("anisotropic-closed-volume-stars", () =>
{
    var multiplicity = new int[anisotropic.VolumeCount];
    foreach (int[] volumes in anisotropic.CellVolumes)
        foreach (int volume in volumes) multiplicity[volume]++;
    int violations = multiplicity.Count(count => count != 2);
    return (violations == 0, $"violations={violations}");
});
Control("anisotropic-boundary-of-boundary", () =>
{
    int violations = BoundaryViolations(anisotropic);
    return (violations == 0, $"nonzeroEdgeCoefficients={violations}");
});
Control("anisotropic-four-axis-tuple-translation-equivariance", () =>
{
    int violations = TranslationViolations(anisotropic, fixtureExtents);
    return (violations == 0, $"tupleViolations={violations}");
});
Control("scalar-versus-isotropic-vector-operator-map-parity", () =>
{
    var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
    var member = OperatorMember();
    var scalarOperator = new EinsteinianShiabOperator(scalar, algebra, member, latticePeriod: 3);
    var vectorOperator = new EinsteinianShiabOperator(isotropicVector, algebra, member, latticePeriods: [3, 3, 3, 3]);
    string scalarHash = OperatorMapHash(scalarOperator);
    string vectorHash = OperatorMapHash(vectorOperator);
    return (scalarHash == vectorHash, $"scalar={scalarHash};vector={vectorHash}");
});
Control("anisotropic-minimal-image-operator-map-activation", () =>
{
    var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
    var member = OperatorMember();
    var raw = new EinsteinianShiabOperator(anisotropic, algebra, member);
    var minimal = new EinsteinianShiabOperator(anisotropic, algebra, member, latticePeriods: fixtureExtents);
    string rawHash = OperatorMapHash(raw);
    string minimalHash = OperatorMapHash(minimal);
    return (rawHash != minimalHash, $"raw={rawHash};minimal={minimalHash}");
});
Control("invalid-period-refusal", () =>
{
    bool generatorRefused = Throws<ArgumentOutOfRangeException>(() =>
        SimplicialMeshGenerator.CreateUniform4DPeriodic(3, 3, 3, 2, latticeCanonical: true));
    var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
    bool operatorRefused = Throws<ArgumentOutOfRangeException>(() =>
        new EinsteinianShiabOperator(anisotropic, algebra, OperatorMember(), latticePeriods: [3, 3, 3, 2]));
    return (generatorRefused && operatorRefused, $"generator={generatorRefused};operator={operatorRefused}");
});

bool inputsValid = contractValid && phase508Valid;
bool allControlsPassed = controls.Count == 9 && controls.All(control => control.Passed);
string verdictKind = !inputsValid ? "invalid-precursor-or-contract"
    : !allControlsPassed ? "anisotropic-cpu-reference-controls-failed"
    : "anisotropic-cpu-reference-controls-ready";
var result = new
{
    phaseId = PhaseId,
    terminalStatus = "phase481-anisotropic-cpu-reference-feasibility-" + verdictKind,
    verdictKind,
    inputsValid,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A14",
    contract = new { path = ContractPath, byteSha256 = Sha(contractBytes), valid = contractValid, frozenBeforeEvaluation = true, taxonomy },
    phase508Binding = new { path = phase508Path, expectedSha256 = S(expected, "sha256"), actualSha256 = phase508Sha, exactHashMatch = phase508Sha == S(expected, "sha256"), valid = phase508Valid },
    selectedGeometry = new { t16 = Ints(expected, "requiredT16Extents"), t32 = Ints(expected, "requiredT32Extents") },
    reducedFixtureExtents = fixtureExtents,
    controls,
    passedControlCount = controls.Count(control => control.Passed),
    requiredControlCount = 9,
    allControlsPassed,
    scalarApiBackwardParityHeld = controls.Single(control => control.Id == "scalar-api-prechange-topology-hash").Passed,
    anisotropicTopologyReady = controls.Take(6).All(control => control.Passed),
    anisotropicOperatorMinimalImageReady = controls.Skip(6).Take(2).All(control => control.Passed),
    targetBlindConstruction = true,
    deterministicReducedControlsOnly = true,
    fullT16OrT32MeshConstructed = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    phase480Satisfied = false,
    productionAuthorized = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a14BoundaryHeld = true,
    decision = verdictKind == "anisotropic-cpu-reference-controls-ready"
        ? "Reduced anisotropic topology and CPU-reference minimal-image controls pass with exact scalar-path parity. This is technical feasibility evidence only; it creates no pack or launch authority."
        : "At least one exact precursor, contract, topology, translation, parity, minimal-image, or refusal control failed. Preserve the failure and create no pack.",
};

string outputDirectory = Path.Combine(PhaseDir, "output");
Directory.CreateDirectory(outputDirectory);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDirectory, "phase481_anisotropic_cpu_reference_feasibility.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase481_anisotropic_cpu_reference_feasibility_summary.json"), json);

Require(inputsValid, result.terminalStatus);
Require(allControlsPassed && verdictKind == "anisotropic-cpu-reference-controls-ready", result.terminalStatus);
Require(!result.fullT16OrT32MeshConstructed && !result.phase481PackCreated && !result.samplingOrReprocessingRun && !result.benchmarkRun, "authority firewall failed");
Require(result.promotedPhysicalMassClaimCount == 0, "claim firewall failed");
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"controls={result.passedControlCount}/{result.requiredControlCount} fixture=3x3x3x4");

static EinsteinianShiabFamilyMember OperatorMember() => new()
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "trivial",
};

static int BoundaryViolations(SimplicialMesh mesh)
{
    int violations = 0;
    for (int volume = 0; volume < mesh.VolumeCount; volume++)
    {
        var coefficients = new Dictionary<int, int>();
        for (int i = 0; i < 4; i++)
        {
            int face = mesh.VolumeBoundaryFaces[volume][i];
            int faceSign = mesh.VolumeBoundaryOrientations[volume][i];
            for (int j = 0; j < 3; j++)
            {
                int edge = mesh.FaceBoundaryEdges[face][j];
                coefficients[edge] = coefficients.GetValueOrDefault(edge) + faceSign * mesh.FaceBoundaryOrientations[face][j];
            }
        }
        violations += coefficients.Values.Count(value => value != 0);
    }
    return violations;
}

static int TranslationViolations(SimplicialMesh mesh, int[] extents)
{
    var coordinateToVertex = new Dictionary<(int, int, int, int), int>();
    var faceLookup = new Dictionary<(int, int, int), int>();
    var volumeLookup = new Dictionary<(int, int, int, int), int>();
    for (int vertex = 0; vertex < mesh.VertexCount; vertex++)
    {
        ReadOnlySpan<double> c = mesh.GetVertexCoordinates(vertex);
        coordinateToVertex[((int)c[0], (int)c[1], (int)c[2], (int)c[3])] = vertex;
    }
    for (int face = 0; face < mesh.FaceCount; face++)
    {
        int[] sorted = (int[])mesh.Faces[face].Clone(); Array.Sort(sorted);
        faceLookup[(sorted[0], sorted[1], sorted[2])] = face;
    }
    for (int volume = 0; volume < mesh.VolumeCount; volume++)
    {
        int[] sorted = (int[])mesh.Volumes[volume].Clone(); Array.Sort(sorted);
        volumeLookup[(sorted[0], sorted[1], sorted[2], sorted[3])] = volume;
    }
    int Translate(int vertex, int axis)
    {
        ReadOnlySpan<double> c = mesh.GetVertexCoordinates(vertex);
        int[] p = [(int)c[0], (int)c[1], (int)c[2], (int)c[3]];
        p[axis] = (p[axis] + 1) % extents[axis];
        return coordinateToVertex[(p[0], p[1], p[2], p[3])];
    }
    int violations = 0;
    for (int axis = 0; axis < 4; axis++)
    {
        for (int face = 0; face < mesh.FaceCount; face++)
        {
            int[] image = mesh.Faces[face].Select(vertex => Translate(vertex, axis)).ToArray();
            int[] sorted = (int[])image.Clone(); Array.Sort(sorted);
            if (!image.SequenceEqual(mesh.Faces[faceLookup[(sorted[0], sorted[1], sorted[2])]])) violations++;
        }
        for (int volume = 0; volume < mesh.VolumeCount; volume++)
        {
            int[] image = mesh.Volumes[volume].Select(vertex => Translate(vertex, axis)).ToArray();
            int[] sorted = (int[])image.Clone(); Array.Sort(sorted);
            if (!image.SequenceEqual(mesh.Volumes[volumeLookup[(sorted[0], sorted[1], sorted[2], sorted[3])]])) violations++;
        }
    }
    return violations;
}

static string OperatorMapHash(EinsteinianShiabOperator value)
{
    FieldInfo field = typeof(EinsteinianShiabOperator).GetField("_cellFaceMap", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("operator map field missing");
    var maps = (double[][,])(field.GetValue(value) ?? throw new InvalidOperationException("operator map unavailable"));
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
    writer.Write(maps.Length);
    foreach (double[,] map in maps)
    {
        writer.Write(map.GetLength(0)); writer.Write(map.GetLength(1));
        foreach (double component in map) writer.Write(component);
    }
    writer.Flush();
    return Sha(stream.ToArray());
}

static string TopologyHash(SimplicialMesh mesh)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
    writer.Write(mesh.EmbeddingDimension); writer.Write(mesh.SimplicialDimension); writer.Write(mesh.VertexCount);
    writer.Write(mesh.VertexCoordinates.Length); foreach (double coordinate in mesh.VertexCoordinates) writer.Write(coordinate);
    Append(writer, mesh.CellVertices); Append(writer, mesh.Edges); Append(writer, mesh.Faces);
    Append(writer, mesh.CellEdges); Append(writer, mesh.CellFaces); Append(writer, mesh.FaceBoundaryEdges);
    Append(writer, mesh.FaceBoundaryOrientations); Append(writer, mesh.VertexEdges); Append(writer, mesh.VertexEdgeOrientations);
    Append(writer, mesh.Volumes); Append(writer, mesh.VolumeBoundaryFaces); Append(writer, mesh.VolumeBoundaryOrientations); Append(writer, mesh.CellVolumes);
    writer.Flush();
    return Sha(stream.ToArray());
}
static void Append(BinaryWriter writer, int[][] arrays)
{
    writer.Write(arrays.Length);
    foreach (int[] array in arrays) { writer.Write(array.Length); foreach (int value in array) writer.Write(value); }
}
static bool Throws<T>(Action action) where T : Exception { try { action(); return false; } catch (T) { return true; } }
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement value, string name) => value.GetProperty(name).GetString() ?? "";
static int I(JsonElement value, string name) => value.GetProperty(name).GetInt32();
static bool B(JsonElement value, string name) => value.GetProperty(name).ValueKind == JsonValueKind.True;
static string[] Strings(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(item => item.GetString() ?? "").ToArray();
static int[] Ints(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(item => item.GetInt32()).ToArray();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record ControlResult(string Id, bool Passed, string Detail);
