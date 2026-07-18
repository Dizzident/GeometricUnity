using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;

const string PhaseDir = "studies/phase518_a5_dual_reflection_exact_consistency_001";
const string ContractPath = PhaseDir + "/preregistration/phase518_dual_reflection_exact_consistency_contract_v1.json";
const string OutputDir = PhaseDir + "/output";
const string Phase514SummaryPath = "studies/phase514_a5_registered_reflection_foundation_audit_001/output/a5_registered_reflection_foundation_audit_summary.json";
const string Phase482SummaryPath = "studies/phase482_a5_theorem_scout_001/output/a5_theorem_scout_summary.json";
const string Phase482ReflectionPath = "studies/phase482_a5_theorem_scout_001/ReflectionLocalityAnalysis.cs";
const string Phase452ProgramPath = "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs";
const string MeshGeneratorPath = "src/Gu.Geometry/SimplicialMeshGenerator.cs";
const string SimplicialMeshPath = "src/Gu.Geometry/SimplicialMesh.cs";
const string TopologyBuilderPath = "src/Gu.Geometry/MeshTopologyBuilder.cs";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement phase517 = contract.GetProperty("phase517Bindings");
JsonElement phase517Summary = phase517.GetProperty("summary");
JsonElement phase517Contract = phase517.GetProperty("contract");
JsonElement phase517Program = phase517.GetProperty("program");
JsonElement phase517Site = phase517.GetProperty("siteCandidate");
JsonElement phase517Link = phase517.GetProperty("linkCandidate");
string phase517SummaryPath = S(phase517Summary, "path");
string phase517ContractPath = S(phase517Contract, "path");
string phase517ProgramPath = S(phase517Program, "path");
string phase517SitePath = S(phase517Site, "path");
string phase517LinkPath = S(phase517Link, "path");

using var phase517SummaryDoc = JsonDocument.Parse(File.ReadAllBytes(phase517SummaryPath));
using var phase517SiteDoc = JsonDocument.Parse(File.ReadAllBytes(phase517SitePath));
using var phase517LinkDoc = JsonDocument.Parse(File.ReadAllBytes(phase517LinkPath));
JsonElement phase517SummaryRoot = phase517SummaryDoc.RootElement;
JsonElement phase517SiteRoot = phase517SiteDoc.RootElement;
JsonElement phase517LinkRoot = phase517LinkDoc.RootElement;

JsonElement expected = contract.GetProperty("expectedBindings");
var bindings = new[]
{
    Bind("phase517-summary", phase517SummaryPath, S(phase517Summary, "sha256")),
    Bind("phase517-contract", phase517ContractPath, S(phase517Contract, "sha256")),
    Bind("phase517-program", phase517ProgramPath, S(phase517Program, "sha256")),
    Bind("phase517-site-candidate", phase517SitePath, S(phase517Site, "sha256")),
    Bind("phase517-link-candidate", phase517LinkPath, S(phase517Link, "sha256")),
    Bind("phase514-summary", Phase514SummaryPath, S(expected, "phase514SummarySha256")),
    Bind("phase482-summary", Phase482SummaryPath, S(expected, "phase482SummarySha256")),
    Bind("phase482-reflection-analysis", Phase482ReflectionPath, S(expected, "phase482ReflectionSha256")),
    Bind("phase452-action-observable-source", Phase452ProgramPath, S(expected, "phase452ProgramSha256")),
    Bind("mesh-generator-source", MeshGeneratorPath, S(expected, "meshGeneratorSha256")),
    Bind("simplicial-mesh-source", SimplicialMeshPath, S(expected, "simplicialMeshSha256")),
    Bind("topology-builder-source", TopologyBuilderPath, S(expected, "topologyBuilderSha256")),
};

JsonElement geometry = contract.GetProperty("geometry");
string[] precedence = Strings(contract, "precedence");
JsonElement[] contractCandidateMaps = contract.GetProperty("candidateMaps").EnumerateArray().ToArray();
bool contractValid = I(contract, "schemaVersion") == 1
    && S(contract, "contractId") == "phase518-a5-dual-reflection-exact-consistency-contract-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A17"
    && S(contract, "contractStatus") == "frozen-final-before-phase517-consumption"
    && B(contract, "frozenBeforePrecursorConsumption")
    && I(geometry, "extent") == 4 && I(geometry, "dimension") == 4 && I(geometry, "timeAxis") == 0
    && B(geometry, "latticeCanonical")
    && contractCandidateMaps.Length == 2
    && precedence.SequenceEqual(new[]
    {
        "invalid-or-drifted-input", "dual-candidate-oriented-complex-nonclosure",
        "candidate-closure-outcome-unexpected", "reduced-dual-candidate-full-closure",
    }, StringComparer.Ordinal)
    && S(contract, "expectedCurrentVerdict") == "dual-candidate-oriented-complex-nonclosure"
    && !B(contract, "missingImageMayBeSilentlyZeroFilled")
    && !B(contract, "restrictedClosureMayBePromotedToFullClosure")
    && !B(contract, "finiteControlMaySupportAllVolumeInference")
    && !B(contract, "phase515Or516Authorized") && !B(contract, "theoremClaimAllowed")
    && !B(contract, "samplingOrProductionAllowed") && !B(contract, "physicalClaimAllowed")
    && I(contract, "promotedPhysicalMassClaimCount") == 0;
var candidateSpecs = new[]
{
    CandidateSpecFrom(
        phase517SiteRoot, phase517SummaryRoot, phase517Site, contractCandidateMaps,
        expectedCandidateId: "site-centered-axis0", expectedCentering: "site-centered",
        expectedVertexMap: "r0(t,x)=(-t mod 4,x)", expectedContractTimeMap: "t -> -t mod 4",
        expectedOffset: 0, expectedPositive: new[] { 1 }, expectedBoundary: new[] { 0, 2 }, expectedNegative: new[] { 3 },
        expectedBoundaryRule: "vertices on slices 0 and 2; incident boundary-edge treatment remains candidate-only"),
    CandidateSpecFrom(
        phase517LinkRoot, phase517SummaryRoot, phase517Link, contractCandidateMaps,
        expectedCandidateId: "link-centered-axis0", expectedCentering: "link-centered",
        expectedVertexMap: "r1(t,x)=(1-t mod 4,x)", expectedContractTimeMap: "t -> 1-t mod 4",
        expectedOffset: 1, expectedPositive: new[] { 1, 2 }, expectedBoundary: Array.Empty<int>(), expectedNegative: new[] { 0, 3 },
        expectedBoundaryRule: "no fixed vertices; setwise-fixed plane-crossing links are candidate boundary objects"),
};
bool candidateLineageValid = candidateSpecs.All(x => x.Valid)
    && candidateSpecs.Select(x => x.SourceCandidateId).Distinct(StringComparer.Ordinal).Count() == 2
    && candidateSpecs.Select(x => x.Offset).Distinct().Count() == 2;
bool precursorSemanticsValid = I(phase517SummaryRoot, "schemaVersion") == 1
    && I(phase517SummaryRoot, "phase") == 517
    && S(phase517SummaryRoot, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A17"
    && S(phase517SummaryRoot, "verdictKind") == S(phase517Summary, "requiredVerdictKind")
    && S(phase517SummaryRoot, "terminalStatus") == S(phase517Summary, "requiredVerdictKind")
    && B(phase517SummaryRoot, "inputsValid")
    && JsonString(phase517ContractPath, "contractId") == S(phase517Contract, "requiredContractId")
    && candidateLineageValid
    && JsonString(Phase514SummaryPath, "verdictKind") == "registered-foundation-definition-missing"
    && !JsonBool(Phase514SummaryPath, "phase515MayBeCreated")
    && JsonString(Phase482SummaryPath, "verdictKind") == "obstructions-survive-no-theorem"
    && !JsonBool(Phase482SummaryPath, "theoremClaimed");

int extent = I(geometry, "extent");
int dimension = I(geometry, "dimension");
int timeAxis = I(geometry, "timeAxis");
SimplicialMesh mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
bool coordinateDomainValid = mesh.EmbeddingDimension == dimension
    && mesh.SimplicialDimension == dimension
    && mesh.VertexCoordinates.Length == mesh.VertexCount * dimension
    && Enumerable.Range(0, mesh.VertexCount).All(vertex => mesh.GetVertexCoordinates(vertex).ToArray().All(value =>
        double.IsFinite(value) && value == Math.Truncate(value) && value >= 0 && value < extent));
bool coordinateKeysUnique = Enumerable.Range(0, mesh.VertexCount)
    .Select(v => CoordinateKey(Coordinates(mesh, v))).Distinct(StringComparer.Ordinal).Count() == mesh.VertexCount;
bool canonicalEdgesValid = mesh.Edges.All(edge => edge.Length == 2 && edge[0] >= 0 && edge[1] < mesh.VertexCount && edge[0] < edge[1]);
bool simplexKeysUnique = mesh.Edges.Select(UnorientedKey).Distinct(StringComparer.Ordinal).Count() == mesh.EdgeCount
    && mesh.Faces.All(face => face.Length == 3 && face.Distinct().Count() == 3 && face.All(v => v >= 0 && v < mesh.VertexCount))
    && mesh.Faces.Select(UnorientedKey).Distinct(StringComparer.Ordinal).Count() == mesh.FaceCount
    && mesh.CellVertices.All(cell => cell.Length == dimension + 1 && cell.Distinct().Count() == dimension + 1 && cell.All(v => v >= 0 && v < mesh.VertexCount))
    && mesh.CellVertices.Select(UnorientedKey).Distinct(StringComparer.Ordinal).Count() == mesh.CellCount;
var coordinateToVertex = Enumerable.Range(0, mesh.VertexCount)
    .ToDictionary(v => CoordinateKey(Coordinates(mesh, v)), v => v, StringComparer.Ordinal);
var edgeLookup = mesh.Edges.Select((vertices, index) => (vertices, index))
    .ToDictionary(x => UnorientedKey(x.vertices), x => x.index, StringComparer.Ordinal);
var faceLookup = mesh.Faces.Select((vertices, index) => (vertices, index))
    .ToDictionary(x => UnorientedKey(x.vertices), x => x.index, StringComparer.Ordinal);
var cellLookup = mesh.CellVertices.Select((vertices, index) => (vertices, index))
    .ToDictionary(x => UnorientedKey(x.vertices), x => x.index, StringComparer.Ordinal);

CandidateResult Audit(CandidateSpec spec)
{
    int[] vertexMap = new int[mesh.VertexCount];
    for (int vertex = 0; vertex < mesh.VertexCount; vertex++)
    {
        int[] coordinates = Coordinates(mesh, vertex);
        coordinates[spec.Axis] = Mod(spec.Offset - coordinates[spec.Axis], extent);
        vertexMap[vertex] = coordinateToVertex[CoordinateKey(coordinates)];
    }

    bool vertexBijection = vertexMap.Distinct().Count() == mesh.VertexCount;
    bool vertexInvolution = Enumerable.Range(0, mesh.VertexCount).All(v => vertexMap[vertexMap[v]] == v);

    int mappedEdges = 0, reversedEdges = 0, edgeTwiceIdentity = 0;
    ClosureWitness? edgeWitness = null;
    for (int edgeIndex = 0; edgeIndex < mesh.EdgeCount; edgeIndex++)
    {
        int[] edge = mesh.Edges[edgeIndex];
        int mapped0 = vertexMap[edge[0]], mapped1 = vertexMap[edge[1]];
        if (!edgeLookup.TryGetValue(UnorientedKey(mapped0, mapped1), out int target))
        {
            edgeWitness ??= Witness(edgeIndex, edge, new[] { mapped0, mapped1 }, mesh);
            continue;
        }
        mappedEdges++;
        int sign1 = mapped0 < mapped1 ? 1 : -1;
        if (sign1 < 0) reversedEdges++;
        int[] targetEdge = mesh.Edges[target];
        int twice0 = vertexMap[targetEdge[0]], twice1 = vertexMap[targetEdge[1]];
        if (edgeLookup.TryGetValue(UnorientedKey(twice0, twice1), out int twiceTarget))
        {
            int sign2 = twice0 < twice1 ? 1 : -1;
            if (twiceTarget == edgeIndex && sign1 * sign2 == 1) edgeTwiceIdentity++;
        }
    }

    int mappedFaces = 0, faceTwiceIdentity = 0;
    ClosureWitness? faceWitness = null;
    for (int faceIndex = 0; faceIndex < mesh.FaceCount; faceIndex++)
    {
        int[] face = mesh.Faces[faceIndex];
        int[] mapped = face.Select(v => vertexMap[v]).ToArray();
        if (!faceLookup.TryGetValue(UnorientedKey(mapped), out int target))
        {
            faceWitness ??= Witness(faceIndex, face, mapped, mesh);
            continue;
        }
        mappedFaces++;
        int[] twice = mesh.Faces[target].Select(v => vertexMap[v]).ToArray();
        if (faceLookup.TryGetValue(UnorientedKey(twice), out int twiceTarget) && twiceTarget == faceIndex)
            faceTwiceIdentity++;
    }

    int mappedCells = 0, cellTwiceIdentity = 0;
    ClosureWitness? cellWitness = null;
    for (int cellIndex = 0; cellIndex < mesh.CellCount; cellIndex++)
    {
        int[] cell = mesh.CellVertices[cellIndex];
        int[] mapped = cell.Select(v => vertexMap[v]).ToArray();
        if (!cellLookup.TryGetValue(UnorientedKey(mapped), out int target))
        {
            cellWitness ??= Witness(cellIndex, cell, mapped, mesh);
            continue;
        }
        mappedCells++;
        int[] twice = mesh.CellVertices[target].Select(v => vertexMap[v]).ToArray();
        if (cellLookup.TryGetValue(UnorientedKey(twice), out int twiceTarget) && twiceTarget == cellIndex)
            cellTwiceIdentity++;
    }

    return new CandidateResult(
        spec.SourceCandidateId, spec.Centering, spec.Axis, spec.Offset, spec.VertexMap,
        vertexBijection, vertexInvolution,
        mesh.EdgeCount, mappedEdges, reversedEdges, edgeTwiceIdentity, edgeWitness,
        mesh.FaceCount, mappedFaces, faceTwiceIdentity, faceWitness,
        mesh.CellCount, mappedCells, cellTwiceIdentity, cellWitness);
}

CandidateResult[] candidates = candidateSpecs.Select(Audit).ToArray();
bool exactGeometryCounts = mesh.VertexCount == I(geometry, "expectedVertices")
    && mesh.EdgeCount == I(geometry, "expectedEdges")
    && mesh.FaceCount == I(geometry, "expectedFaces")
    && mesh.CellCount == I(geometry, "expectedFourSimplices")
    && coordinateDomainValid && coordinateKeysUnique && canonicalEdgesValid && simplexKeysUnique;
bool restrictedControlsValid = candidates.All(c =>
    c.VertexBijection && c.VertexInvolution
    && c.EdgeMappedCount > 0 && c.EdgeMappedCount < c.EdgeCount
    && c.EdgeRestrictedTwiceIdentityCount == c.EdgeMappedCount
    && c.FaceMappedCount >= 0 && c.FaceRestrictedTwiceIdentityCount == c.FaceMappedCount
    && c.CellMappedCount >= 0 && c.CellRestrictedTwiceIdentityCount == c.CellMappedCount
    && c.EdgeMissingWitness is not null && c.FaceMissingWitness is not null && c.CellMissingWitness is not null);
bool mutationControlsPassed = candidates.Select(c => c.Offset).Distinct().Count() == 2
    && candidates.All(c => c.EdgeReversalCount > 0)
    && candidates.All(c => !c.FullEdgeClosure && !c.FullFaceClosure && !c.FullCellClosure)
    && candidates.All(c => c.RestrictedClosureNotPromotedToFull);
bool inputsValid = contractValid && bindings.Length == 12 && bindings.All(x => x.HashMatches)
    && precursorSemanticsValid && exactGeometryCounts && restrictedControlsValid && mutationControlsPassed;
bool dualNonclosure = candidates.All(c => c.VertexBijection && c.VertexInvolution
    && (!c.FullEdgeClosure || !c.FullFaceClosure || !c.FullCellClosure));
bool underlyingSetNonclosureSufficesForOrientedComplexNonclosure = dualNonclosure
    && candidates.All(c => c.EdgeMissingCount > 0 || c.FaceMissingCount > 0 || c.CellMissingCount > 0);
bool dualFullClosure = candidates.All(c => c.VertexBijection && c.VertexInvolution
    && c.FullEdgeClosure && c.FullFaceClosure && c.FullCellClosure);
string verdict = !inputsValid ? precedence[0] : dualNonclosure ? precedence[1]
    : dualFullClosure ? precedence[3] : precedence[2];

var result = new
{
    schemaVersion = 1,
    phase = 518,
    phaseId = "phase518-a5-dual-reflection-exact-consistency",
    contractId = S(contract, "contractId"),
    terminalStatus = "a5-dual-reflection-exact-consistency-" + verdict,
    verdictKind = verdict,
    inputsValid,
    contractValid,
    precursorSemanticsValid,
    candidateLineageValid,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A17",
    contract = new { path = ContractPath, sha256 = Sha(File.ReadAllBytes(ContractPath)), frozenBeforePrecursorConsumption = B(contract, "frozenBeforePrecursorConsumption") },
    exactInputBindings = bindings,
    geometry = new
    {
        extent, dimension, timeAxis, latticeCanonical = true,
        vertices = mesh.VertexCount, edges = mesh.EdgeCount, faces = mesh.FaceCount,
        fourSimplices = mesh.CellCount, exactGeometryCounts, coordinateDomainValid, coordinateKeysUnique,
        canonicalEdgesValid, simplexKeysUnique,
    },
    candidates,
    exactIntegerCombinatorialControlsOnly = true,
    floatingPointGeometryOrToleranceUsedForClosure = false,
    missingImagesSilentlyZeroFilled = false,
    restrictedClosurePromotedToFullClosure = false,
    restrictedControlsValid,
    mutationControlsPassed,
    dualCandidateOrientedComplexNonclosure = dualNonclosure,
    underlyingSetNonclosureSufficesForOrientedComplexNonclosure,
    candidateSelectionPerformed = false,
    candidateRankingPerformed = false,
    candidatesCombined = false,
    candidateRegistrationPerformed = false,
    candidatesRemainUnregistered = true,
    observableMembershipEvaluated = false,
    measureNormalizationEvaluated = false,
    allCouplingAllVolumeClaim = false,
    fullCandidateActionCovarianceEvaluated = false,
    osBilinearHermiticityEvaluated = false,
    finiteControlSupportsAllVolumeInference = false,
    registeredTheoremReflectionEstablished = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    targetCounterexampleEstablished = false,
    theoremClaimed = false,
    closesLimbL8 = false,
    phase458G1Satisfied = false,
    phase458Evaluated = false,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase482ArtifactMutated = false,
    zeroPhysicsCompute = true,
    symbolicFalsifierRun = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    launchAuthorized = false,
    binderLaunchAuthorized = false,
    accelerationAuthorized = false,
    phase480Satisfied = false,
    o4Discharged = false,
    humanRulingAuthoredOrInferred = false,
    targetBlindConstruction = true,
    externalReviewPending = true,
    physicalTargetsConsultedForConstruction = false,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a17BoundaryHeld = true,
    a16Unchanged = true,
    decision = "Both finite n=4 candidate vertex maps are exact involutive bijections, but each fails closure already on the underlying simplex sets. Because every oriented-complex automorphism must first be a total automorphism of the underlying simplex sets, this is sufficient for oriented-complex nonclosure without claiming face or four-simplex orientation-parity results. Restricted twice-identity controls are recorded separately and cannot be promoted to full closure. No action, OS bilinear, theorem, or counterexample conclusion follows.",
};

Require(inputsValid, "Phase518 inputs, exact census, or mutation controls drifted.");
Require(verdict == S(contract, "expectedCurrentVerdict"), "Phase518 expected nonclosure terminal drifted.");
Require(result.dualCandidateOrientedComplexNonclosure && !result.restrictedClosurePromotedToFullClosure && !result.missingImagesSilentlyZeroFilled, "Phase518 full-versus-restricted closure firewall drifted.");
Require(result.underlyingSetNonclosureSufficesForOrientedComplexNonclosure, "Phase518 oriented-complex implication was not established from underlying-set nonclosure.");
Require(!result.theoremClaimed && !result.reflectionPositivityEstablished && !result.reflectionPositivityRefuted && !result.targetCounterexampleEstablished && !result.closesLimbL8 && !result.phase458G1Satisfied, "Phase518 theorem firewall drifted.");
Require(result.a17BoundaryHeld && result.a16Unchanged && result.zeroPhysicsCompute && !result.samplingOrReprocessingRun && !result.productionAuthorized
    && result.targetBlindConstruction && result.externalReviewPending && result.promotedPhysicalMassClaimCount == 0,
    "Phase518 execution or claim firewall drifted.");
Require(!result.candidateSelectionPerformed && !result.candidateRankingPerformed && !result.candidatesCombined && !result.candidateRegistrationPerformed
    && result.candidatesRemainUnregistered && !result.observableMembershipEvaluated && !result.measureNormalizationEvaluated && !result.allCouplingAllVolumeClaim,
    "Phase518 candidate or theorem-scope firewall drifted.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "a5_dual_reflection_exact_consistency.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_dual_reflection_exact_consistency_summary.json"), json);
Console.WriteLine(result.terminalStatus);

static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;
static int[] Coordinates(SimplicialMesh mesh, int vertex) => Enumerable.Range(0, 4)
    .Select(axis => checked((int)mesh.GetVertexCoordinates(vertex)[axis])).ToArray();
static string CoordinateKey(int[] coordinates) => string.Join(",", coordinates);
static string UnorientedKey(params int[] vertices) => string.Join(",", vertices.Order());
static ClosureWitness Witness(int index, int[] source, int[] reflected, SimplicialMesh mesh) => new(
    index,
    source.Select(v => CoordinateKey(Coordinates(mesh, v))).ToArray(),
    reflected.Select(v => CoordinateKey(Coordinates(mesh, v))).ToArray());
static Binding Bind(string id, string path, string expected)
{
    string actual = File.Exists(path) ? Sha(File.ReadAllBytes(path)) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement value, string name) => value.GetProperty(name).GetString() ?? "";
static int I(JsonElement value, string name) => value.GetProperty(name).GetInt32();
static bool B(JsonElement value, string name) => value.GetProperty(name).GetBoolean();
static string[] Strings(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
static string JsonString(string path, string name) { using var doc = JsonDocument.Parse(File.ReadAllBytes(path)); return S(doc.RootElement, name); }
static bool JsonBool(string path, string name) { using var doc = JsonDocument.Parse(File.ReadAllBytes(path)); return B(doc.RootElement, name); }
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

static CandidateSpec CandidateSpecFrom(
    JsonElement standalone,
    JsonElement summaryRoot,
    JsonElement binding,
    JsonElement[] contractMaps,
    string expectedCandidateId,
    string expectedCentering,
    string expectedVertexMap,
    string expectedContractTimeMap,
    int expectedOffset,
    int[] expectedPositive,
    int[] expectedBoundary,
    int[] expectedNegative,
    string expectedBoundaryRule)
{
    string candidateId = S(standalone, "candidateId");
    string centering = S(standalone, "centering");
    string vertexMap = S(standalone, "vertexMap");
    bool mapParsed = TryParseMap(vertexMap, out int axis, out int offset);
    JsonElement foundation = summaryRoot.GetProperty("formalCandidateFoundation");
    JsonElement[] summaryRows = foundation.GetProperty("candidates").EnumerateArray()
        .Where(row => S(row, "candidateId") == candidateId).ToArray();
    JsonElement[] summaryArtifacts = foundation.GetProperty("candidateArtifacts").EnumerateArray()
        .Where(row => S(row, "candidateId") == candidateId).ToArray();
    JsonElement[] maps = contractMaps.Where(row => S(row, "id") == centering).ToArray();
    int[] positive = Ints(standalone, "positiveVertexSlices");
    int[] boundary = Ints(standalone, "boundaryVertexSlices");
    int[] negative = Ints(standalone, "negativeVertexSlices");
    int[] allSlices = positive.Concat(boundary).Concat(negative).Order().ToArray();
    bool sliceReflectionValid = positive.All(t => negative.Contains(Mod(offset - t, 4)))
        && negative.All(t => positive.Contains(Mod(offset - t, 4)))
        && boundary.All(t => boundary.Contains(Mod(offset - t, 4)));
    string boundPath = S(binding, "path");
    string boundSha = S(binding, "sha256");
    bool valid = candidateId == expectedCandidateId
        && candidateId == S(binding, "requiredCandidateId")
        && centering == expectedCentering
        && vertexMap == expectedVertexMap
        && mapParsed && axis == 0 && offset == expectedOffset
        && positive.SequenceEqual(expectedPositive)
        && boundary.SequenceEqual(expectedBoundary)
        && negative.SequenceEqual(expectedNegative)
        && allSlices.SequenceEqual(Enumerable.Range(0, 4))
        && sliceReflectionValid
        && S(standalone, "boundaryRule") == expectedBoundaryRule
        && S(standalone, "status") == S(binding, "requiredStatus")
        && S(standalone, "candidateRole") == "formal-diagnostic-row-only"
        && !B(standalone, "registered") && !B(standalone, "a5Validated")
        && summaryRows.Length == 1 && JsonElement.DeepEquals(summaryRows[0], standalone)
        && summaryArtifacts.Length == 1
        && S(summaryArtifacts[0], "path") == boundPath
        && S(summaryArtifacts[0], "sha256") == boundSha
        && maps.Length == 1 && S(maps[0], "timeMap") == expectedContractTimeMap;
    return new CandidateSpec(candidateId, centering, vertexMap, axis, offset, valid);
}

static bool TryParseMap(string vertexMap, out int axis, out int offset)
{
    axis = 0;
    offset = 0;
    if (vertexMap == "r0(t,x)=(-t mod 4,x)") return true;
    if (vertexMap == "r1(t,x)=(1-t mod 4,x)") { offset = 1; return true; }
    return false;
}

static int[] Ints(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();

sealed record ClosureWitness(int SourceIndex, string[] SourceVertices, string[] ReflectedVertices);
sealed record CandidateSpec(string SourceCandidateId, string Centering, string VertexMap, int Axis, int Offset, bool Valid);
sealed record CandidateResult(
    string SourceCandidateId,
    string Centering,
    int Axis,
    int Offset,
    string VertexMap,
    bool VertexBijection,
    bool VertexInvolution,
    int EdgeCount,
    int EdgeMappedCount,
    int EdgeReversalCount,
    int EdgeRestrictedTwiceIdentityCount,
    ClosureWitness? EdgeMissingWitness,
    int FaceCount,
    int FaceMappedCount,
    int FaceRestrictedTwiceIdentityCount,
    ClosureWitness? FaceMissingWitness,
    int CellCount,
    int CellMappedCount,
    int CellRestrictedTwiceIdentityCount,
    ClosureWitness? CellMissingWitness)
{
    public int EdgeMissingCount => EdgeCount - EdgeMappedCount;
    public int FaceMissingCount => FaceCount - FaceMappedCount;
    public int CellMissingCount => CellCount - CellMappedCount;
    public bool FullEdgeClosure => EdgeMappedCount == EdgeCount;
    public bool FullFaceClosure => FaceMappedCount == FaceCount;
    public bool FullCellClosure => CellMappedCount == CellCount;
    public bool RestrictedClosureNotPromotedToFull =>
        EdgeRestrictedTwiceIdentityCount == EdgeMappedCount && !FullEdgeClosure
        && FaceRestrictedTwiceIdentityCount == FaceMappedCount && !FullFaceClosure
        && CellRestrictedTwiceIdentityCount == CellMappedCount && !FullCellClosure;
}

sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
