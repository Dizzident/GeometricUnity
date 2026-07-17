using System.Security.Cryptography;
using System.Text.Json;

const string PhaseId = "phase508-phase481-acquisition-geometry-closure";
const string PhaseDir = "studies/phase508_phase481_acquisition_geometry_closure_001";
const string ContractPath = PhaseDir + "/preregistration/phase508_acquisition_geometry_contract_v1.json";

// Read and validate the complete prospective contract before consuming evidence bytes.
byte[] contractBytes = File.ReadAllBytes(ContractPath);
string contractSha256 = Sha(contractBytes);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
string[] taxonomy = Strings(contract, "terminalTaxonomyInPrecedenceOrder");
JsonElement rules = contract.GetProperty("evidenceRules");
JsonElement countRules = contract.GetProperty("exactCountRules");
var expectedInputs = contract.GetProperty("expectedInputs").EnumerateArray()
    .Select(x => new InputBinding(S(x, "role"), S(x, "path"), S(x, "sha256")))
    .ToArray();
var candidates = contract.GetProperty("geometryCandidates").EnumerateArray()
    .Select(x => new Candidate(S(x, "id"), Ints(x, "t16Extents"), Ints(x, "t32Extents")))
    .ToArray();

bool contractValid = S(contract, "contractId") == "phase508-a14-acquisition-geometry-closure-v1"
    && B(contract, "frozenBeforeEvidenceConsumption")
    && expectedInputs.Length == 4 && expectedInputs.Select(x => x.Role).Distinct().Count() == 4
    && candidates.Length == 2
    && candidates.Single(x => x.Id == "spatial-four-temporal").T16.SequenceEqual(new[] { 4, 4, 4, 16 })
    && candidates.Single(x => x.Id == "spatial-four-temporal").T32.SequenceEqual(new[] { 4, 4, 4, 32 })
    && candidates.Single(x => x.Id == "isotropic-temporal").T16.SequenceEqual(new[] { 16, 16, 16, 16 })
    && candidates.Single(x => x.Id == "isotropic-temporal").T32.SequenceEqual(new[] { 32, 32, 32, 32 })
    && !B(rules, "targetValuesMayChooseGeometry") && !B(rules, "empiricalResultsMayChooseGeometry")
    && !B(rules, "unmentionedAxesMayBeChanged")
    && S(rules, "ambiguityAction") == "geometry-ambiguous-no-selection"
    && S(countRules, "relativeWorkProxy") == "site-count-times-required-effective-sample-size"
    && taxonomy.SequenceEqual(new[] { "invalid-precursor-or-contract", "geometry-ambiguous", "spatial-four-temporal-design-selected", "isotropic-temporal-design-selected" })
    && !B(contract, "phase481PackCreationAllowed") && !B(contract, "samplingOrReprocessingAllowed")
    && !B(contract, "benchmarkAllowed") && !B(contract, "hmcAllowed")
    && !B(contract, "phase480SatisfiedByThisPhase") && !B(contract, "physicalClaimAllowed");

var consumed = expectedInputs.Select(binding =>
{
    byte[] bytes = File.ReadAllBytes(binding.Path);
    string actual = Sha(bytes);
    return new ConsumedInput(binding.Role, binding.Path, binding.Sha256, actual, actual == binding.Sha256, bytes);
}).ToArray();

ConsumedInput phase481Input = consumed.Single(x => x.Role == "phase481-construction-plan");
ConsumedInput phase496Input = consumed.Single(x => x.Role == "phase496-retained-support");
ConsumedInput phase497Input = consumed.Single(x => x.Role == "phase497-temporal-acquisition-oracle");
ConsumedInput phase502Input = consumed.Single(x => x.Role == "phase502-adaptive-calibration-specification");
using JsonDocument phase481Document = JsonDocument.Parse(phase481Input.Bytes);
using JsonDocument phase496Document = JsonDocument.Parse(phase496Input.Bytes);
using JsonDocument phase497Document = JsonDocument.Parse(phase497Input.Bytes);
using JsonDocument phase502Document = JsonDocument.Parse(phase502Input.Bytes);
JsonElement phase481 = phase481Document.RootElement;
JsonElement phase496 = phase496Document.RootElement;
JsonElement phase497 = phase497Document.RootElement;
JsonElement phase502 = phase502Document.RootElement;

bool byteBindingsValid = consumed.All(x => x.ExactHashMatch);
bool phase481Valid = S(phase481, "planId") == "phase481-pack-construction-plan-v1"
    && B(phase481, "planOnly") && !B(phase481, "phase481PackCreated")
    && phase481.GetProperty("unresolvedConstructionDecisions").EnumerateArray().Any(x => S(x, "id") == "geometry" && S(x, "status") == "blocking")
    && I(phase481.GetProperty("frozenRequirementsForwardedForConstruction"), "initialTemporalExtent") == 16
    && I(phase481.GetProperty("frozenRequirementsForwardedForConstruction"), "conditionalTemporalExtent") == 32
    && I(phase481, "promotedPhysicalMassClaimCount") == 0;
JsonElement spatialInventory = phase496.GetProperty("spatialInventory");
JsonElement temporalInventory = phase496.GetProperty("temporalInventory");
bool phase496SpatialFact = I(spatialInventory, "spatialLinearExtent") == 4
    && I(spatialInventory, "spatialDimensions") == 3
    && I(spatialInventory, "spatialMomentumCount") == 64
    && I(temporalInventory, "temporalExtent") == 4;
JsonElement selected497 = phase497.GetProperty("selectedSpecification");
JsonElement frozen497 = phase497.GetProperty("frozenConfiguration");
bool phase497TemporalOnlyFact = S(phase497, "verdictKind") == "acquisition-specification-viable"
    && I(selected497, "temporalExtent") == 16
    && S(selected497, "costMetric") == "temporalExtent*effectiveSampleSize"
    && Ints(frozen497, "temporalExtents").SequenceEqual(new[] { 4, 8, 12, 16 });
JsonElement forwarded502 = phase502.GetProperty("protocol");
bool phase502TemporalOnlyFact = I(forwarded502.GetProperty("t16Rules"), "temporalExtent") == 16
    && I(forwarded502.GetProperty("t32Rules"), "temporalExtent") == 32
    && I(phase502, "promotedPhysicalMassClaimCount") == 0;
bool inputsValid = contractValid && byteBindingsValid && phase481Valid && phase496SpatialFact
    && phase497TemporalOnlyFact && phase502TemporalOnlyFact;

bool spatialFourEvidenceComplete = phase496SpatialFact && phase497TemporalOnlyFact && phase502TemporalOnlyFact;
bool isotropicExplicitlyRequired = false; // No exact-bound input says that a temporal change changes the three spatial axes.
bool ambiguous = spatialFourEvidenceComplete == isotropicExplicitlyRequired;
string verdictKind = !inputsValid ? "invalid-precursor-or-contract"
    : ambiguous ? "geometry-ambiguous"
    : spatialFourEvidenceComplete ? "spatial-four-temporal-design-selected"
    : "isotropic-temporal-design-selected";
string? selectedCandidate = verdictKind == "spatial-four-temporal-design-selected" ? "spatial-four-temporal"
    : verdictKind == "isotropic-temporal-design-selected" ? "isotropic-temporal" : null;

const long T16Ess = 2048;
const long T32Ess = 8192;
var comparisons = candidates.Select(candidate => new
{
    id = candidate.Id,
    t16 = Counts(candidate.T16, T16Ess),
    t32 = Counts(candidate.T32, T32Ess),
}).ToArray();
long baselineSites = Product(new[] { 4, 4, 4, 4 });

var result = new
{
    phaseId = PhaseId,
    terminalStatus = "phase481-acquisition-geometry-closure-" + verdictKind,
    verdictKind,
    geometryClassification = verdictKind,
    inputsValid,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A14",
    contract = new { path = ContractPath, byteSha256 = contractSha256, valid = contractValid, frozenBeforeEvidenceConsumption = true, taxonomy },
    exactInputBindings = consumed.Select(x => new { role = x.Role, path = x.Path, expectedSha256 = x.ExpectedSha256, actualSha256 = x.ActualSha256, x.ExactHashMatch }).ToArray(),
    evidence = new
    {
        phase481GeometryGapPresent = phase481Valid,
        phase496SpatialLinearExtent = I(spatialInventory, "spatialLinearExtent"),
        phase496SpatialDimensions = I(spatialInventory, "spatialDimensions"),
        phase496TemporalExtent = I(temporalInventory, "temporalExtent"),
        phase497VariedAxis = "temporal-only",
        phase497CostMetric = S(selected497, "costMetric"),
        phase502InitialTemporalExtent = I(forwarded502.GetProperty("t16Rules"), "temporalExtent"),
        phase502ConditionalTemporalExtent = I(forwarded502.GetProperty("t32Rules"), "temporalExtent"),
        exactBoundInputExplicitlyRequiresIsotropy = isotropicExplicitlyRequired,
        unmentionedSpatialAxesChanged = false,
    },
    decisionRules = new
    {
        spatialFourEvidenceComplete,
        isotropicExplicitlyRequired,
        ambiguous,
        targetValuesConsulted = false,
        empiricalResultsConsulted = false,
        selectionPrinciple = "preserve-explicit-spatial-support-when-only-temporal-extent-is-varied",
    },
    selectedCandidate,
    selectedExtents = selectedCandidate is null ? null : new { t16 = candidates.Single(x => x.Id == selectedCandidate).T16, t32 = candidates.Single(x => x.Id == selectedCandidate).T32 },
    exactTopologyAndCostComparisons = comparisons,
    baselineN4Sites = baselineSites,
    exactSiteRatios = new
    {
        spatialFourT16OverN4 = Product(new[] { 4, 4, 4, 16 }) / baselineSites,
        spatialFourT32OverN4 = Product(new[] { 4, 4, 4, 32 }) / baselineSites,
        isotropicT16OverN4 = Product(new[] { 16, 16, 16, 16 }) / baselineSites,
        isotropicT32OverN4 = Product(new[] { 32, 32, 32, 32 }) / baselineSites,
        isotropicOverSpatialFourAtT16 = Product(new[] { 16, 16, 16, 16 }) / Product(new[] { 4, 4, 4, 16 }),
        isotropicOverSpatialFourAtT32 = Product(new[] { 32, 32, 32, 32 }) / Product(new[] { 4, 4, 4, 32 }),
    },
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    deterministicArithmeticOnly = true,
    geometryImplemented = false,
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
    decision = verdictKind == "spatial-four-temporal-design-selected"
        ? "The exact-bound evidence changes temporal support while retaining an explicit three-axis spatial extent of four. Freeze 4^3 x T for the later CPU-reference implementation; this phase grants no pack or launch authority."
        : "The exact-bound evidence did not uniquely determine acquisition geometry. Preserve the ambiguity and create no implementation or pack.",
};

string outputDirectory = Path.Combine(PhaseDir, "output");
Directory.CreateDirectory(outputDirectory);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDirectory, "phase481_acquisition_geometry_closure.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase481_acquisition_geometry_closure_summary.json"), json);

Require(inputsValid, result.terminalStatus);
Require(verdictKind == "spatial-four-temporal-design-selected" && selectedCandidate == "spatial-four-temporal", "target-blind geometry selection failed");
Require(comparisons.Single(x => x.id == "spatial-four-temporal").t16.Vertices == 1024, "T16 spatial-four count failed");
Require(comparisons.Single(x => x.id == "spatial-four-temporal").t32.Vertices == 2048, "T32 spatial-four count failed");
Require(comparisons.Single(x => x.id == "isotropic-temporal").t32.Vertices == 1_048_576, "T32 isotropic count failed");
Require(!result.phase481PackCreated && !result.samplingOrReprocessingRun && !result.benchmarkRun && result.promotedPhysicalMassClaimCount == 0, "authority or claim firewall failed");
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"selected={selectedCandidate} t16Sites=1024 t32Sites=2048 isoPenalties=64x/512x");

static CountsResult Counts(int[] extents, long ess)
{
    long n = Product(extents);
    return new CountsResult(extents, n, checked(15 * n), checked(50 * n), checked(60 * n),
        checked(24 * n), ess, checked(n * ess));
}

static long Product(int[] values)
{
    long result = 1;
    foreach (int value in values) result = checked(result * value);
    return result;
}

static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement x, string name) => x.GetProperty(name).GetString() ?? "";
static int I(JsonElement x, string name) => x.GetProperty(name).GetInt32();
static bool B(JsonElement x, string name) => x.GetProperty(name).ValueKind == JsonValueKind.True;
static string[] Strings(JsonElement x, string name) => x.GetProperty(name).EnumerateArray().Select(v => v.GetString() ?? "").ToArray();
static int[] Ints(JsonElement x, string name) => x.GetProperty(name).EnumerateArray().Select(v => v.GetInt32()).ToArray();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record InputBinding(string Role, string Path, string Sha256);
sealed record ConsumedInput(string Role, string Path, string ExpectedSha256, string ActualSha256, bool ExactHashMatch, byte[] Bytes);
sealed record Candidate(string Id, int[] T16, int[] T32);
sealed record CountsResult(int[] Extents, long Vertices, long Edges, long Triangles, long Tetrahedra,
    long FourSimplices, long RequiredEffectiveSampleSize, long SiteEssWorkProxy);
