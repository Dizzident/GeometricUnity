using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;

const string PhaseDir = "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001";
const string ContractPath = PhaseDir + "/preregistration/phase521_frozen_reflection_compatible_triangulation_census_contract_v1.json";
const string OutputDir = PhaseDir + "/output";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
Binding[] bindings = contract.GetProperty("exactInputBindings").EnumerateArray()
    .Select(row => Bind(S(row, "id"), S(row, "path"), S(row, "sha256"))).ToArray();
int[] extents = Ints(contract, "extentMenu");
ReflectionSpec[] reflections = contract.GetProperty("reflectionMenu").EnumerateArray()
    .Select(row => new ReflectionSpec(S(row, "id"), I(row, "axis"), I(row, "offset"),
        S(row, "inactiveAxis0BaseMap"), S(row, "activeAxis0BaseMap"))).ToArray();
JsonElement[] menu = contract.GetProperty("geometryMenu").EnumerateArray().ToArray();
string[] precedence = Strings(contract, "precedence");
int[] expectedCellRankCoefficients = Ints(contract, "expectedCubicalCellRankCoefficients");
int[] expectedBarycentricCoefficients = Ints(contract, "expectedBarycentricSimplexCoefficients");

bool contractValid = I(contract, "schemaVersion") == 1
    && S(contract, "contractId") == "phase521-a18-frozen-reflection-compatible-triangulation-census-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A18"
    && S(contract, "contractStatus") == "frozen-final-before-candidate-scoring"
    && B(contract, "menuFrozenBeforeScoring")
    && menu.Length == 2
    && S(menu[0], "id") == "lattice-canonical-kuhn-negative-control"
    && S(menu[0], "role") == "required-negative-control"
    && S(menu[1], "id") == "periodic-cubical-cell-poset-barycentric"
    && S(menu[1], "role") == "audit-authored-candidate"
    && extents.SequenceEqual(new[] { 3, 4 })
    && I(contract, "dimension") == 4
    && S(contract, "cubicalCellRepresentation") == "canonical-base-coordinate-in-Zn4-plus-active-axis-mask-0-through-15"
    && expectedCellRankCoefficients.SequenceEqual(new[] { 1, 4, 6, 4, 1 })
    && expectedBarycentricCoefficients.SequenceEqual(new[] { 16, 240, 800, 960, 384 })
    && I(contract, "expectedMaximalChainCoefficient") == 384
    && reflections.Length == 2
    && reflections[0] == new ReflectionSpec("site-centered-axis0", 0, 0,
        "b0 -> -b0 mod n", "b0 -> -b0-1 mod n")
    && reflections[1] == new ReflectionSpec("link-centered-axis0", 0, 1,
        "b0 -> 1-b0 mod n", "b0 -> -b0 mod n")
    && precedence.SequenceEqual(new[]
    {
        "invalid-or-drifted-input", "no-total-reflection-compatible-candidate",
        "finite-dual-reflection-compatible-candidate-survives",
    }, StringComparer.Ordinal)
    && !B(contract, "candidateSelectionAllowed") && !B(contract, "candidateRegistrationAllowed")
    && !B(contract, "finiteSurvivalSupportsAllVolumeInference") && !B(contract, "orientationTheoremAllowed")
    && !B(contract, "reflectionPositivityClaimAllowed") && !B(contract, "samplingOrProductionAllowed")
    && !B(contract, "physicalClaimAllowed") && I(contract, "promotedPhysicalMassClaimCount") == 0;

bool precursorSemanticsValid = JsonString(bindings[0].Path, "verdictKind") == "dual-candidate-oriented-complex-nonclosure"
    && JsonBool(bindings[0].Path, "inputsValid")
    && JsonBool(bindings[0].Path, "candidatesRemainUnregistered")
    && !JsonBool(bindings[0].Path, "reflectionPositivityRefuted")
    && JsonString(bindings[1].Path, "contractId") == "phase518-a5-dual-reflection-exact-consistency-contract-v1";

var negativeControls = new List<GeometryAudit>();
var barycentricCandidates = new List<GeometryAudit>();
var cubicalComplexes = new List<CubicalOrderComplex>();
var meshCoordinateControls = new List<MeshCoordinateControl>();
foreach (int extent in extents)
{
    SimplicialMesh mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
    SimplexComplex negative = FromMesh(mesh, extent);
    meshCoordinateControls.Add(new MeshCoordinateControl(extent, negative.VertexCount, negative.CoordinateDomainValid));
    CubicalOrderComplex barycentric = BuildCubicalOrderComplex(extent);
    cubicalComplexes.Add(barycentric);
    foreach (ReflectionSpec reflection in reflections)
    {
        negativeControls.Add(AuditSimplexComplex(
            "lattice-canonical-kuhn-negative-control", "required-negative-control", extent,
            reflection, negative, vertex =>
            {
                int[] coordinates = (int[])negative.Coordinates[vertex].Clone();
                coordinates[reflection.Axis] = Mod(reflection.Offset - coordinates[reflection.Axis], extent);
                return negative.CoordinateToVertex[CoordinateKey(coordinates)];
            }));
        barycentricCandidates.Add(AuditCubicalOrderComplex(barycentric, reflection));
    }
}

bool exactNegativeControlReproduced = negativeControls
    .Where(x => x.Extent == 4)
    .All(x => x.VertexCount == 256 && x.SimplexCounts.SequenceEqual(new[] { 256, 3840, 12800, 15360, 6144 })
        && x.MappedSimplexCounts[1] == 2048 && x.MappedSimplexCounts[2] == 3072
        && x.MappedSimplexCounts[4] == 0 && !x.TotalSimplexClosure);
bool committedMeshCoordinatesValid = meshCoordinateControls.Count == extents.Length
    && meshCoordinateControls.All(control => control.FiniteIntegralInRangeUniqueCoordinates);
bool negativeControlsReject = negativeControls.All(x => x.VertexBijection && x.VertexInvolution && !x.TotalSimplexClosure);
bool independentBarycentricFormulasValid = cubicalComplexes.All(complex =>
{
    int volume = checked(complex.Extent * complex.Extent * complex.Extent * complex.Extent);
    return complex.CanonicalRepresentationValid
        && complex.CellRankCounts.SequenceEqual(expectedCellRankCoefficients.Select(x => x * volume))
        && complex.Simplices.Select(x => x.Count).SequenceEqual(expectedBarycentricCoefficients.Select(x => x * volume))
        && complex.MaximalChainGenerationCount == 384L * volume
        && complex.MaximalChainCount == 384 * volume
        && complex.AllCellsCoveredByMaximalChains
        && complex.MaximalChainCoverIncidenceValid;
});
ReflectionCellControl[] reflectionCellControls = cubicalComplexes
    .SelectMany(complex => reflections.Select(reflection => AuditCellReflection(complex, reflection))).ToArray();
bool reflectionRepresentationControlsValid = reflectionCellControls.All(control =>
    control.AllMappedCellsCanonical && control.ActiveMaskPreserved && control.CellMapBijection
    && control.CellMapInvolution && control.CellRankCountsPreserved);
bool candidateIncidenceValid = barycentricCandidates.All(x => x.IncidenceValid && x.ReflectedIncidencePreserved);
bool finiteCandidateSurvives = barycentricCandidates.All(x => x.VertexBijection && x.VertexInvolution
    && x.TotalSimplexClosure && x.RestrictedTwiceIdentityCounts.SequenceEqual(x.SimplexCounts));
bool inputsValid = contractValid && bindings.Length == 6 && bindings.All(x => x.HashMatches)
    && precursorSemanticsValid && exactNegativeControlReproduced && committedMeshCoordinatesValid && negativeControlsReject
    && independentBarycentricFormulasValid && reflectionRepresentationControlsValid && candidateIncidenceValid;
string verdict = !inputsValid ? precedence[0] : !finiteCandidateSurvives ? precedence[1] : precedence[2];

if (!inputsValid)
    Console.Error.WriteLine($"contractValid={contractValid} bindingsValid={bindings.All(x => x.HashMatches)} precursorSemanticsValid={precursorSemanticsValid} exactNegativeControlReproduced={exactNegativeControlReproduced} negativeControlsReject={negativeControlsReject} candidateIncidenceValid={candidateIncidenceValid}");

var result = new
{
    schemaVersion = 1,
    phase = 521,
    phaseId = "phase521-a5-frozen-reflection-compatible-triangulation-census",
    contractId = S(contract, "contractId"),
    terminalStatus = "a5-frozen-reflection-compatible-triangulation-census-" + verdict,
    verdictKind = verdict,
    inputsValid,
    contractValid,
    precursorSemanticsValid,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A18",
    contract = new { path = ContractPath, sha256 = Sha(File.ReadAllBytes(ContractPath)), menuFrozenBeforeScoring = true },
    exactInputBindings = bindings,
    frozenMenu = new
    {
        geometryIds = menu.Select(x => S(x, "id")).ToArray(),
        extentMenu = extents,
        reflectionIds = reflections.Select(x => x.Id).ToArray(),
        menuFrozenBeforeScoring = true,
        physicalTargetsConsultedForConstruction = false,
        resultBasedTuningPerformed = false,
    },
    negativeControls,
    meshCoordinateControls,
    constructionControls = cubicalComplexes.Select(complex => new
    {
        complex.Extent,
        cellRepresentation = "canonical-base-coordinate-in-Zn4-plus-active-axis-mask-0-through-15",
        complex.CanonicalRepresentationValid,
        complex.CellRankCounts,
        expectedCellRankCounts = expectedCellRankCoefficients.Select(x => x * checked(complex.Extent * complex.Extent * complex.Extent * complex.Extent)).ToArray(),
        barycentricSimplexCounts = complex.Simplices.Select(x => x.Count).ToArray(),
        expectedBarycentricSimplexCounts = expectedBarycentricCoefficients.Select(x => x * checked(complex.Extent * complex.Extent * complex.Extent * complex.Extent)).ToArray(),
        complex.MaximalChainGenerationCount,
        complex.MaximalChainCount,
        expectedMaximalChainCount = 384 * checked(complex.Extent * complex.Extent * complex.Extent * complex.Extent),
        complex.AllCellsCoveredByMaximalChains,
        complex.MaximalChainCoverIncidenceCheckCount,
        complex.MaximalChainCoverIncidenceValid,
    }).ToArray(),
    auditAuthoredCandidates = barycentricCandidates,
    reflectionCellControls,
    exactNegativeControlReproduced,
    committedMeshCoordinatesValid,
    negativeControlsReject,
    candidateIncidenceValid,
    independentBarycentricFormulasValid,
    reflectionRepresentationControlsValid,
    finiteCandidateSurvives,
    survivingGeometryIds = finiteCandidateSurvives ? new[] { "periodic-cubical-cell-poset-barycentric" } : Array.Empty<string>(),
    survivingReflectionIds = finiteCandidateSurvives ? reflections.Select(x => x.Id).ToArray() : Array.Empty<string>(),
    exactIntegerCombinatorialControlsOnly = true,
    floatingPointGeometryOrToleranceUsedForClosure = false,
    missingImagesSilentlyZeroFilled = false,
    restrictedClosurePromotedToTotalClosure = false,
    currentCommittedMeshMutated = false,
    candidateSelectedForProduction = false,
    candidateRegistrationPerformed = false,
    candidatesRemainUnregistered = true,
    finiteSurvivalSupportsAllVolumeInference = false,
    allVolumeEmbeddingTheoremEstablished = false,
    orientationTheoremEstablished = false,
    boundaryOrPullbackRulesRegistered = false,
    actionOrObservableEvaluated = false,
    normalizedMeasureEvaluated = false,
    candidateToTargetEqualityEstablished = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    theoremClaimed = false,
    targetCounterexampleEstablished = false,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    closesLimbL8 = false,
    phase458G1Satisfied = false,
    phase458Evaluated = false,
    zeroPhysicsCompute = true,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    launchAuthorized = false,
    accelerationAuthorized = false,
    phase481PackCreatedOrMutated = false,
    phase482ArtifactMutated = false,
    o4Discharged = false,
    humanRulingAuthoredOrInferred = false,
    targetBlindConstruction = true,
    externalReviewPending = true,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a18BoundaryHeld = true,
    decision = "On the frozen extents 3 and 4, both axis-zero reflections reproduce the expected total-simplex failure of the committed negative control and preserve every simplex of the audit-authored periodic cubical cell-poset order complex. This is finite exact candidate survival only: it neither selects nor registers a geometry or reflection and supplies no all-volume, orientation, target-equality, measure, or reflection-positivity result.",
};

Require(inputsValid, "Phase521 inputs, frozen menu, incidence checks, or negative control drifted.");
Require(verdict == S(contract, "expectedCurrentVerdict"), "Phase521 expected terminal drifted.");
Require(finiteCandidateSurvives && result.candidatesRemainUnregistered && !result.candidateSelectedForProduction,
    "Phase521 finite-candidate boundary drifted.");
Require(!result.finiteSurvivalSupportsAllVolumeInference && !result.allVolumeEmbeddingTheoremEstablished
    && !result.orientationTheoremEstablished && !result.reflectionPositivityEstablished && !result.reflectionPositivityRefuted,
    "Phase521 theorem firewall drifted.");
Require(result.a18BoundaryHeld && result.zeroPhysicsCompute && !result.samplingOrReprocessingRun
    && !result.productionAuthorized && result.targetBlindConstruction && result.externalReviewPending
    && result.promotedPhysicalMassClaimCount == 0, "Phase521 execution or claim firewall drifted.");

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "a5_frozen_reflection_compatible_triangulation_census.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "a5_frozen_reflection_compatible_triangulation_census_summary.json"), json);
Console.WriteLine(result.terminalStatus);

static CubicalOrderComplex BuildCubicalOrderComplex(int extent)
{
    var cells = new List<CubicalCell>();
    var index = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (int[] coordinates in Coordinates(extent))
    for (int mask = 0; mask < 16; mask++)
    {
        var cell = new CubicalCell(coordinates, mask);
        index.Add(cell.Key, cells.Count);
        cells.Add(cell);
    }
    int[] cellRankCounts = Enumerable.Range(0, 5).Select(rank => cells.Count(cell => PopCount(cell.Mask) == rank)).ToArray();
    bool canonicalRepresentationValid = cells.Count == checked(extent * extent * extent * extent * 16)
        && index.Count == cells.Count
        && cells.All(cell => cell.Mask >= 0 && cell.Mask < 16
            && cell.Base.Length == 4 && cell.Base.All(value => value >= 0 && value < extent))
        && cells.Select(cell => cell.Key).Distinct(StringComparer.Ordinal).Count() == cells.Count;

    var top = new HashSet<string>(StringComparer.Ordinal);
    long maximalChainGenerationCount = 0;
    foreach (int[] cubeBase in Coordinates(extent))
    for (int cornerMask = 0; cornerMask < 16; cornerMask++)
    foreach (int[] permutation in Permutations4())
    {
        maximalChainGenerationCount++;
        int activeMask = 0;
        var chain = new int[5];
        for (int rank = 0; rank <= 4; rank++)
        {
            int[] cellBase = new int[4];
            for (int axis = 0; axis < 4; axis++)
                cellBase[axis] = Mod(cubeBase[axis] + (((activeMask >> axis) & 1) == 0 && ((cornerMask >> axis) & 1) == 1 ? 1 : 0), extent);
            chain[rank] = index[new CubicalCell(cellBase, activeMask).Key];
            if (rank < 4) activeMask |= 1 << permutation[rank];
        }
        top.Add(SimplexKey(chain));
    }

    HashSet<string>[] simplices = Enumerable.Range(0, 5).Select(_ => new HashSet<string>(StringComparer.Ordinal)).ToArray();
    foreach (string topKey in top)
    {
        int[] chain = ParseKey(topKey);
        for (int subset = 1; subset < 32; subset++)
        {
            int[] simplex = Enumerable.Range(0, 5).Where(bit => ((subset >> bit) & 1) == 1).Select(bit => chain[bit]).ToArray();
            simplices[simplex.Length - 1].Add(SimplexKey(simplex));
        }
    }
    bool incidenceValid = top.All(key =>
    {
        int[] chain = ParseKey(key).OrderBy(vertex => PopCount(cells[vertex].Mask)).ToArray();
        return Enumerable.Range(0, 4).All(i => Contains(cells[chain[i + 1]], cells[chain[i]], extent));
    });
    bool allCellsCoveredByMaximalChains = simplices[0].Count == cells.Count;
    long maximalChainCoverIncidenceCheckCount = 4L * top.Count;
    return new CubicalOrderComplex(extent, cells.ToArray(), index, simplices,
        canonicalRepresentationValid, cellRankCounts, maximalChainGenerationCount, top.Count,
        allCellsCoveredByMaximalChains, maximalChainCoverIncidenceCheckCount, incidenceValid);
}

static GeometryAudit AuditCubicalOrderComplex(CubicalOrderComplex complex, ReflectionSpec reflection)
{
    int[] map = complex.Cells.Select(cell => complex.Index[ReflectCell(cell, complex.Extent, reflection).Key]).ToArray();
    return AuditSets("periodic-cubical-cell-poset-barycentric", "audit-authored-candidate", complex.Extent,
        reflection, complex.Simplices, map, complex.MaximalChainCoverIncidenceValid && complex.CanonicalRepresentationValid);
}

static ReflectionCellControl AuditCellReflection(CubicalOrderComplex complex, ReflectionSpec reflection)
{
    CubicalCell[] mapped = complex.Cells.Select(cell => ReflectCell(cell, complex.Extent, reflection)).ToArray();
    bool allMappedCellsCanonical = mapped.All(cell => complex.Index.ContainsKey(cell.Key)
        && cell.Mask >= 0 && cell.Mask < 16 && cell.Base.All(value => value >= 0 && value < complex.Extent));
    bool activeMaskPreserved = complex.Cells.Zip(mapped).All(pair => pair.First.Mask == pair.Second.Mask);
    bool cellMapBijection = mapped.Select(cell => cell.Key).Distinct(StringComparer.Ordinal).Count() == complex.Cells.Length;
    bool cellMapInvolution = complex.Cells.All(cell => ReflectCell(ReflectCell(cell, complex.Extent, reflection), complex.Extent, reflection).Key == cell.Key);
    int[] mappedRankCounts = Enumerable.Range(0, 5).Select(rank => mapped.Count(cell => PopCount(cell.Mask) == rank)).ToArray();
    bool cellRankCountsPreserved = mappedRankCounts.SequenceEqual(complex.CellRankCounts);
    return new ReflectionCellControl(complex.Extent, reflection.Id, reflection.Axis, reflection.Offset,
        reflection.InactiveAxis0BaseMap, reflection.ActiveAxis0BaseMap, complex.Cells.Length,
        allMappedCellsCanonical, activeMaskPreserved, cellMapBijection, cellMapInvolution,
        complex.CellRankCounts, mappedRankCounts, cellRankCountsPreserved);
}

static GeometryAudit AuditSimplexComplex(string id, string role, int extent, ReflectionSpec reflection,
    SimplexComplex complex, Func<int, int> mapVertex) => AuditSets(id, role, extent, reflection, complex.Simplices,
        Enumerable.Range(0, complex.VertexCount).Select(mapVertex).ToArray(), constructionIncidenceValid: complex.CoordinateDomainValid);

static GeometryAudit AuditSets(string id, string role, int extent, ReflectionSpec reflection,
    HashSet<string>[] simplices, int[] map, bool constructionIncidenceValid)
{
    bool bijection = map.Distinct().Count() == map.Length;
    bool involution = Enumerable.Range(0, map.Length).All(i => map[map[i]] == i);
    long incidenceCheckCount = 0;
    bool incidenceValid = constructionIncidenceValid;
    for (int dimension = 1; dimension <= 4; dimension++)
    foreach (string key in simplices[dimension])
    {
        int[] vertices = ParseKey(key);
        for (int omitted = 0; omitted < vertices.Length; omitted++)
        {
            incidenceCheckCount++;
            string face = SimplexKey(vertices.Where((_, index) => index != omitted));
            incidenceValid &= simplices[dimension - 1].Contains(face);
        }
    }
    int[] counts = simplices.Select(x => x.Count).ToArray();
    int[] mapped = new int[5];
    int[] twice = new int[5];
    MissingWitness? witness = null;
    for (int dimension = 0; dimension <= 4; dimension++)
    foreach (string key in simplices[dimension])
    {
        int[] source = ParseKey(key);
        int[] image = source.Select(v => map[v]).ToArray();
        string imageKey = SimplexKey(image);
        if (!simplices[dimension].Contains(imageKey))
        {
            witness ??= new MissingWitness(dimension, key, imageKey);
            continue;
        }
        mapped[dimension]++;
        int[] imageCanonical = ParseKey(imageKey);
        string twiceKey = SimplexKey(imageCanonical.Select(v => map[v]).ToArray());
        if (twiceKey == key) twice[dimension]++;
    }
    return new GeometryAudit(id, role, extent, reflection.Id, reflection.Axis, reflection.Offset,
        map.Length, bijection, involution, incidenceCheckCount, incidenceValid, counts, mapped, twice, witness,
        mapped.SequenceEqual(counts), mapped.SequenceEqual(counts));
}

static SimplexComplex FromMesh(SimplicialMesh mesh, int extent)
{
    HashSet<string>[] simplices =
    {
        Enumerable.Range(0, mesh.VertexCount).Select(v => SimplexKey(new[] { v })).ToHashSet(StringComparer.Ordinal),
        mesh.Edges.Select(SimplexKey).ToHashSet(StringComparer.Ordinal),
        mesh.Faces.Select(SimplexKey).ToHashSet(StringComparer.Ordinal),
        mesh.Volumes.Select(SimplexKey).ToHashSet(StringComparer.Ordinal),
        mesh.CellVertices.Select(SimplexKey).ToHashSet(StringComparer.Ordinal),
    };
    double[][] rawCoordinates = Enumerable.Range(0, mesh.VertexCount)
        .Select(v => mesh.GetVertexCoordinates(v).ToArray()).ToArray();
    bool coordinateDomainValid = rawCoordinates.All(row => row.Length == 4 && row.All(value =>
        double.IsFinite(value) && value == Math.Truncate(value) && value >= 0 && value < extent));
    if (!coordinateDomainValid)
        throw new InvalidOperationException("Phase521 refuses a nonfinite, nonintegral, or out-of-range committed mesh coordinate.");
    var coordinates = rawCoordinates.Select(row => row.Select(value => checked((int)value)).ToArray()).ToArray();
    coordinateDomainValid &= coordinates.Select(CoordinateKey).Distinct(StringComparer.Ordinal).Count() == mesh.VertexCount;
    if (!coordinateDomainValid)
        throw new InvalidOperationException("Phase521 refuses duplicate committed mesh coordinates.");
    var lookup = coordinates.Select((c, i) => (c, i)).ToDictionary(x => CoordinateKey(x.c), x => x.i, StringComparer.Ordinal);
    return new SimplexComplex(extent, mesh.VertexCount, simplices, coordinates, lookup, coordinateDomainValid);
}

static CubicalCell ReflectCell(CubicalCell cell, int extent, ReflectionSpec reflection)
{
    int[] mapped = (int[])cell.Base.Clone();
    bool active = ((cell.Mask >> reflection.Axis) & 1) == 1;
    mapped[reflection.Axis] = Mod(reflection.Offset - cell.Base[reflection.Axis] - (active ? 1 : 0), extent);
    return new CubicalCell(mapped, cell.Mask);
}

static bool Contains(CubicalCell outer, CubicalCell inner, int extent)
{
    if ((inner.Mask & ~outer.Mask) != 0) return false;
    for (int axis = 0; axis < 4; axis++)
    {
        bool outerActive = ((outer.Mask >> axis) & 1) == 1;
        bool innerActive = ((inner.Mask >> axis) & 1) == 1;
        if (!outerActive && inner.Base[axis] != outer.Base[axis]) return false;
        if (outerActive && !innerActive && inner.Base[axis] != outer.Base[axis] && inner.Base[axis] != Mod(outer.Base[axis] + 1, extent)) return false;
        if (outerActive && innerActive && inner.Base[axis] != outer.Base[axis]) return false;
    }
    return true;
}

static IEnumerable<int[]> Coordinates(int extent)
{
    for (int a = 0; a < extent; a++) for (int b = 0; b < extent; b++)
    for (int c = 0; c < extent; c++) for (int d = 0; d < extent; d++) yield return new[] { a, b, c, d };
}

static IEnumerable<int[]> Permutations4()
{
    for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++) if (b != a)
    for (int c = 0; c < 4; c++) if (c != a && c != b)
    for (int d = 0; d < 4; d++) if (d != a && d != b && d != c) yield return new[] { a, b, c, d };
}

static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;
static int PopCount(int value)
{
    int count = 0;
    while (value != 0) { count += value & 1; value >>= 1; }
    return count;
}
static string CoordinateKey(int[] coordinates) => string.Join(",", coordinates);
static string SimplexKey(IEnumerable<int> vertices) => string.Join(",", vertices.Order());
static int[] ParseKey(string key) => key.Split(',').Select(int.Parse).ToArray();
static Binding Bind(string id, string path, string expected)
{
    string actual = File.Exists(path) ? Sha(File.ReadAllBytes(path)) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement value, string name) => value.GetProperty(name).GetString() ?? "";
static int I(JsonElement value, string name) => value.GetProperty(name).GetInt32();
static bool B(JsonElement value, string name) => value.GetProperty(name).GetBoolean();
static int[] Ints(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();
static string[] Strings(JsonElement value, string name) => value.GetProperty(name).EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
static string JsonString(string path, string name) { using var doc = JsonDocument.Parse(File.ReadAllBytes(path)); return S(doc.RootElement, name); }
static bool JsonBool(string path, string name) { using var doc = JsonDocument.Parse(File.ReadAllBytes(path)); return B(doc.RootElement, name); }
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record ReflectionSpec(string Id, int Axis, int Offset, string InactiveAxis0BaseMap, string ActiveAxis0BaseMap);
sealed record MissingWitness(int Dimension, string SourceSimplex, string ReflectedSimplex);
sealed record MeshCoordinateControl(int Extent, int VertexCount, bool FiniteIntegralInRangeUniqueCoordinates);
sealed record ReflectionCellControl(int Extent, string ReflectionId, int Axis, int Offset,
    string InactiveAxis0BaseMap, string ActiveAxis0BaseMap, int CellCount,
    bool AllMappedCellsCanonical, bool ActiveMaskPreserved, bool CellMapBijection, bool CellMapInvolution,
    int[] SourceCellRankCounts, int[] MappedCellRankCounts, bool CellRankCountsPreserved);
sealed record GeometryAudit(string GeometryId, string Role, int Extent, string ReflectionId, int Axis, int Offset,
    int VertexCount, bool VertexBijection, bool VertexInvolution, long IncidenceCheckCount, bool IncidenceValid, int[] SimplexCounts,
    int[] MappedSimplexCounts, int[] RestrictedTwiceIdentityCounts, MissingWitness? FirstMissingWitness,
    bool ReflectedIncidencePreserved, bool TotalSimplexClosure);
sealed record SimplexComplex(int Extent, int VertexCount, HashSet<string>[] Simplices, int[][] Coordinates,
    Dictionary<string, int> CoordinateToVertex, bool CoordinateDomainValid);
sealed record CubicalOrderComplex(int Extent, CubicalCell[] Cells, Dictionary<string, int> Index,
    HashSet<string>[] Simplices, bool CanonicalRepresentationValid, int[] CellRankCounts,
    long MaximalChainGenerationCount, int MaximalChainCount, bool AllCellsCoveredByMaximalChains,
    long MaximalChainCoverIncidenceCheckCount, bool MaximalChainCoverIncidenceValid);
sealed class CubicalCell
{
    public CubicalCell(int[] cellBase, int mask) { Base = (int[])cellBase.Clone(); Mask = mask; }
    public int[] Base { get; }
    public int Mask { get; }
    public string Key => $"{string.Join(",", Base)}|{Mask}";
}
