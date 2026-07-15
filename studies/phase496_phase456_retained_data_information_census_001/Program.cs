using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string ArtifactPath = "studies/phase456_consolidated_n4_launch_001/production/phase452_n4/scalar_channel_spectroscopy_probe.json";
const string Phase456Path = "studies/phase456_consolidated_n4_launch_001/output/consolidated_n4_launch_summary.json";
const string Phase493Path = "studies/phase493_phase456_stored_artifact_failure_decomposition_001/output/phase456_stored_artifact_failure_decomposition_summary.json";
const string Phase494Path = "studies/phase494_phase456_estimator_oracle_battery_001/output/phase456_estimator_oracle_battery_summary.json";
const string Phase495Path = "studies/phase495_phase456_prospective_repair_readiness_adjudicator_001/output/phase456_prospective_repair_readiness_adjudicator_summary.json";
const string OutputDir = "studies/phase496_phase456_retained_data_information_census_001/output";
const string OutputSlug = "phase456_retained_data_information_census";
const string ArtifactSha256 = "9b7e965a0b8ac906bc1352f908b28b6eb22511579c02ee91562f63beb67ed9cb";
const string Phase456Sha256 = "1668cf2e7b477c24181afa04fcd76089d974c407c08257a62d72a81b8cb9138b";
const string Phase493Sha256 = "dd2d00e7bb1a5dc3869a0b8362d8f2a855cd6b2c80cee62818ad4eb0671b5d85";
const string Phase494Sha256 = "e90f8e008c7d413275699f353603bb6dab9c4f3490ec5039ca6c9a50a6dbc25f";
const string Phase495Sha256 = "9df00d53e9be384787f9ec55636cabd32a08764950d2aca613fea1a181f13b4a";
const string DecisionContractCanonical =
    "phase496-a10-v1|precedence=invalid-precursor>retained-data-insufficient-for-identifiable-repair>retained-data-sufficient-for-prospective-estimator-design|" +
    "invalid=missing-byte-hash-schema-or-semantic-binding|insufficient=less-than-four-independent-temporal-values-or-no-configuration-level-samples-or-admitted-two-pole-jacobian-rank-bound-below-four|" +
    "sufficient=valid-inputs-and-at-least-four-independent-temporal-values-and-configuration-level-samples-and-two-pole-rank-bound-at-least-four|" +
    "inventory-only-no-invalid-row-reinterpretation-no-sampling-no-authority";

string[] taxonomy =
{
    "invalid-precursor",
    "retained-data-insufficient-for-identifiable-repair",
    "retained-data-sufficient-for-prospective-estimator-design",
};
var errors = new List<string>();

FrozenJson artifact = Bind(ArtifactPath, ArtifactSha256, errors);
FrozenJson phase456 = Bind(Phase456Path, Phase456Sha256, errors);
FrozenJson phase493 = Bind(Phase493Path, Phase493Sha256, errors);
FrozenJson phase494 = Bind(Phase494Path, Phase494Sha256, errors);
FrozenJson phase495 = Bind(Phase495Path, Phase495Sha256, errors);

int temporalExtent = 0, uniquePeriodicSeparations = 0, independentNonzeroSeparations = 0;
int spatialMomentumCount = 0, spatialOffsetCount = 0;
int ensembleColumnCount = 0, productionColumnCount = 0, controlColumnCount = 0;
int actionMemberCount = 0, betaRegimeCount = 0, minimumCorrelatorLength = int.MaxValue;
int minimumJackknifeCount = int.MaxValue, maximumJackknifeCount = 0;
int productionColumnsBelowNEffFloor = 0;
double minimumProductionGenericNEff = double.PositiveInfinity;
double minimumProductionRowNEff = double.PositiveInfinity;
bool periodicSymmetryExact = false, allSpatialModesComplete = false, allJackknifesAligned = false;
bool perFaceTypeProjectionRetained = false, configurationLevelSamplesRetained = false;
bool phase456InvalidTerminalExact = false, phase493Exact = false, phase494Exact = false, phase495Exact = false;
var columns = new List<object>();

try
{
    JsonElement p456 = phase456.Root;
    phase456InvalidTerminalExact = S(p456, "phaseId") == "phase456-consolidated-n4-launch"
        && S(p456, "verdictKind") == "production-analysis-invalid"
        && S(p456, "terminalStatus") == "consolidated-n4-launch-production-analysis-invalid"
        && !B(p456.GetProperty("productionAnalysis"), "inputShapeValid");
    phase493Exact = S(phase493.Root, "phaseId") == "phase493-phase456-stored-artifact-failure-decomposition"
        && S(phase493.Root, "verdictKind") == "mixed-failure-supported" && B(phase493.Root, "inputsValid")
        && B(phase493.Root.GetProperty("componentFindings").GetProperty("estimatorDomain"), "estimatorDomainFailureSupported");
    phase494Exact = S(phase494.Root, "phaseId") == "phase494-phase456-estimator-oracle-battery"
        && S(phase494.Root, "verdictKind") == "mixed-estimator-outcome" && B(phase494.Root, "inputsValid")
        && !B(phase494.Root, "originalEstimatorStructurallyIdentifiable")
        && B(phase494.Root, "t4OriginalRatioUnderdeterminesSpectralContent");
    phase495Exact = S(phase495.Root, "phaseId") == "phase495-phase456-prospective-repair-readiness-adjudicator"
        && S(phase495.Root, "verdictKind") == "estimator-structurally-unidentifiable" && B(phase495.Root, "inputsValid");

    JsonElement root = artifact.Root;
    JsonElement torus = root.GetProperty("tori").EnumerateArray().Single(x => I(x, "torusN") == 4);
    temporalExtent = I(torus, "torusN");
    uniquePeriodicSeparations = temporalExtent / 2 + 1;
    independentNonzeroSeparations = uniquePeriodicSeparations - 1;
    spatialMomentumCount = I(torus, "vertexCount") / temporalExtent;
    spatialOffsetCount = spatialMomentumCount;
    JsonElement[] columnElements = torus.GetProperty("columns").EnumerateArray().ToArray();
    ensembleColumnCount = columnElements.Length;
    productionColumnCount = columnElements.Count(x => S(x, "kind") == "production");
    controlColumnCount = ensembleColumnCount - productionColumnCount;
    actionMemberCount = columnElements.Select(x => S(x, "member")).Distinct(StringComparer.Ordinal).Count();
    betaRegimeCount = columnElements.Select(x => D(x, "beta")).Distinct().Count();

    bool allPeriodic = true, allModes = true, allJk = true;
    foreach (JsonElement column in columnElements)
    {
        JsonElement c = column.GetProperty("phase456");
        JsonElement[] spatial = c.GetProperty("perSiteSpatialCorrelators").EnumerateArray().ToArray();
        JsonElement a1 = c.GetProperty("identityIrrep2x2Gevp");
        JsonElement a2o1 = c.GetProperty("a2").GetProperty("o1");
        JsonElement a2o2 = c.GetProperty("a2").GetProperty("o2");
        JsonElement kmin = c.GetProperty("kMin").GetProperty("spatialAxisAverageO1");
        int[] lengths = { Len(a1, "dominantEigenvalue"), Len(a2o1, "c"), Len(a2o2, "c"), Len(kmin, "c") };
        int[] jkCounts = { Len(a1, "jackknifeDominantEigenvalue"), Len(a2o1, "jackknife"), Len(a2o2, "jackknife"), Len(kmin, "jackknife") };
        minimumCorrelatorLength = System.Math.Min(minimumCorrelatorLength, lengths.Min());
        minimumJackknifeCount = System.Math.Min(minimumJackknifeCount, jkCounts.Min());
        maximumJackknifeCount = System.Math.Max(maximumJackknifeCount, jkCounts.Max());
        allPeriodic &= lengths.All(x => x == temporalExtent)
            && ExactPeriodic(a1.GetProperty("dominantEigenvalue")) && ExactPeriodic(a2o1.GetProperty("c"))
            && ExactPeriodic(a2o2.GetProperty("c")) && ExactPeriodic(kmin.GetProperty("c"));
        allModes &= spatial.Length == spatialMomentumCount && spatial.All(x => Len(x, "c") == temporalExtent && Len(x, "jackknife") == 50);
        allJk &= jkCounts.All(x => x == 50) && spatial.All(x => Len(x, "jackknife") == 50);
        perFaceTypeProjectionRetained |= B(c, "perFaceTypeResolutionRetained");
        double genericNEff = D(column, "nEff");
        JsonElement row = c.GetProperty("rowEffectiveSampleSizes");
        double rowMin = new[] { D(row, "nEffA2"), D(row, "nEffKMin") }.Min();
        if (S(column, "kind") == "production")
        {
            minimumProductionGenericNEff = System.Math.Min(minimumProductionGenericNEff, genericNEff);
            minimumProductionRowNEff = System.Math.Min(minimumProductionRowNEff, rowMin);
            if (genericNEff < 100.0 || rowMin < 100.0) productionColumnsBelowNEffFloor++;
        }
        columns.Add(new
        {
            member = S(column, "member"), beta = D(column, "beta"), kind = S(column, "kind"),
            trajectories = I(column, "trajectories"), warmup = I(column, "warmup"), genericNEff,
            minimumRecordedRowNEff = rowMin, correlatorLength = lengths.Min(), jackknifeBlocks = jkCounts.Min(),
            spatialMomentumModes = spatial.Length,
        });
    }
    periodicSymmetryExact = allPeriodic;
    allSpatialModesComplete = allModes;
    allJackknifesAligned = allJk && minimumJackknifeCount == maximumJackknifeCount;

    // The artifact contains aggregated correlators and delete-block summaries. It does not store one row per retained configuration.
    configurationLevelSamplesRetained = false;
}
catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or NullReferenceException)
{
    errors.Add("Committed schema/semantic reconstruction failed: " + ex.Message);
}

if (!phase456InvalidTerminalExact) errors.Add("Phase456 invalid terminal drifted.");
if (!phase493Exact || !phase494Exact || !phase495Exact) errors.Add("A Phase493-495 semantic binding drifted.");
if (!periodicSymmetryExact) errors.Add("The retained T=4 periodic symmetry or correlator lengths drifted.");
if (!allSpatialModesComplete || !allJackknifesAligned) errors.Add("Spatial-mode or aligned-jackknife storage drifted.");

bool inputsValid = errors.Count == 0;
const int admittedTwoPoleParameterCount = 4; // two unconstrained amplitudes and two masses
int maximumTemporalJacobianRank = uniquePeriodicSeparations;
int maximumUnconstrainedExponentialCount = uniquePeriodicSeparations / 2;
bool twoPoleStructurallyIdentifiableFromTemporalSupport = maximumTemporalJacobianRank >= admittedTwoPoleParameterCount;
bool covarianceOfAggregatesRecoverableFromJackknifes = inputsValid && allJackknifesAligned;
bool covarianceOfIndividualConfigurationsRecoverable = false;
bool retainedDataSufficientForProspectiveEstimatorDesign = inputsValid
    && uniquePeriodicSeparations >= admittedTwoPoleParameterCount
    && configurationLevelSamplesRetained
    && twoPoleStructurallyIdentifiableFromTemporalSupport;
string censusClassification = !inputsValid ? "invalid-precursor"
    : !retainedDataSufficientForProspectiveEstimatorDesign ? "retained-data-insufficient-for-identifiable-repair"
    : "retained-data-sufficient-for-prospective-estimator-design";

const bool phase456InvalidRowsReinterpreted = false;
const bool phase456ArtifactMutated = false;
const bool phase481PackCreated = false;
const bool hmcRun = false;
const bool benchmarkRun = false;
const bool zeroNewSampling = true;
const bool samplingAuthorized = false;
const bool productionAuthorized = false;
const bool phase458G3Satisfied = false;
const bool phase458G5Satisfied = false;
const bool phase458EvaluationAuthorized = false;
const bool o4Discharged = false;
const bool sourceContractApplicationAllowed = false;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
bool a10BoundaryHeld = !phase456InvalidRowsReinterpreted && !phase456ArtifactMutated && !phase481PackCreated
    && !hmcRun && !benchmarkRun && zeroNewSampling && !samplingAuthorized && !productionAuthorized
    && !phase458G3Satisfied && !phase458G5Satisfied && !phase458EvaluationAuthorized && !o4Discharged
    && !sourceContractApplicationAllowed && noGevPromotion && promotedPhysicalMassClaimCount == 0;
string decisionContractSha256 = Sha256(Encoding.UTF8.GetBytes(DecisionContractCanonical));

var result = new
{
    phaseId = "phase496-phase456-retained-data-information-census",
    terminalStatus = "phase456-retained-data-information-census-" + censusClassification,
    verdictKind = censusClassification,
    inputsValid,
    censusClassification,
    decisionContractSha256,
    retainedDataSufficientForProspectiveEstimatorDesign,
    applicationSubjectKind = "phase456-committed-retained-data-read-only-information-census",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A10; Phase496",
    taxonomy,
    decisionContract = new { schemaId = "phase496-a10-retained-data-census-v1", canonical = DecisionContractCanonical, sha256 = decisionContractSha256 },
    precursorBindings = new
    {
        artifact = artifact.View(), phase456 = phase456.View(), phase493 = phase493.View(), phase494 = phase494.View(), phase495 = phase495.View(),
        phase456InvalidTerminalExact, phase493Exact, phase494Exact, phase495Exact, errors,
    },
    temporalInventory = new
    {
        temporalExtent, storedSeparations = Enumerable.Range(0, temporalExtent).ToArray(), uniquePeriodicSeparations,
        independentNonzeroSeparations, preregisteredFitWindow = new[] { 1 }, preregisteredFitWindowPointCount = 1,
        periodicSymmetryExact, minimumCorrelatorLength,
        interpretation = "T=4 stores t=0,1,2,3, but periodic symmetry makes t=3 duplicate t=1. Only three temporal values, two nonzero-separation values, are independent.",
    },
    spatialInventory = new
    {
        spatialLinearExtent = 4, spatialDimensions = 3, spatialMomentumCount, spatialOffsetCount,
        fullDiscreteSpatialTransformRetained = allSpatialModesComplete, perBaseSiteAggregateReconstructable = allSpatialModesComplete,
        rawPerSitePerConfigurationMeasurementsRetained = false,
        interpretation = "All 4^3 spatial momenta of the aggregate O1 correlator are retained and invertible to 64 spatial offsets; these are correlator summaries, not raw site measurements for each configuration.",
    },
    channelInventory = new
    {
        ensembleColumnCount, productionColumnCount, controlColumnCount, actionMemberCount, betaRegimeCount,
        retainedFamilies = new[] { "identity-irrep-2x2-gevp", "a2-o1", "a2-o2", "kmin-spatial-axis-average-o1", "full-spatial-momentum-o1", "binder-and-susceptibility" },
        perFaceTypeProjectionRetained, exactFaceTypeProjectorAppliedBeforeSliceAggregation = perFaceTypeProjectionRetained,
        rawPerFacePerConfigurationMeasurementsRetained = false, columns,
    },
    covarianceInventory = new
    {
        jackknifeBlockCount = minimumJackknifeCount, jackknifeBlockCountMaximum = maximumJackknifeCount,
        alignedDeleteBlockSummariesRetained = allJackknifesAligned,
        covarianceOfAggregatesRecoverableFromJackknifes,
        covarianceOfIndividualConfigurationsRecoverable,
        configurationLevelSamplesRetained,
        crossColumnConfigurationCovarianceRecoverable = false,
        interpretation = "Fifty aligned delete-block summaries permit covariance reconstruction within each stored aggregate family. They do not restore configuration-level likelihoods or cross-ensemble covariance.",
    },
    statisticalInventory = new
    {
        preRegisteredNEffFloor = 100.0, minimumProductionGenericNEff, minimumProductionRowNEff,
        productionColumnsBelowNEffFloor, allProductionColumnsMeetNEffFloor = productionColumnsBelowNEffFloor == 0,
    },
    identifiabilityBounds = new
    {
        admittedTwoPoleParameterCount, maximumTemporalJacobianRank,
        maximumUnconstrainedExponentialCount, twoPoleStructurallyIdentifiableFromTemporalSupport,
        singlePoleCouldBeIdentifiableOnlyUnderAdditionalModelRestriction = uniquePeriodicSeparations >= 2,
        phase494OriginalEstimatorStructurallyIdentifiable = false,
        phase494T4RatioUnderdeterminesSpectralContent = true,
        identifiableRepairOfAdmittedTwoPoleClassPossibleFromRetainedData = retainedDataSufficientForProspectiveEstimatorDesign,
        bound = "Three independent T=4 temporal values give Jacobian rank at most three for a four-parameter unconstrained two-pole model. Spatial modes add momentum dependence but do not add Euclidean-time separations without a frozen dispersion or shared-spectrum assumption.",
    },
    phase456InvalidRowsReinterpreted,
    phase456ArtifactMutated,
    phase481PackCreated,
    hmcRun,
    benchmarkRun,
    zeroNewSampling,
    samplingAuthorized,
    productionAuthorized,
    phase458G3Satisfied,
    phase458G5Satisfied,
    phase458EvaluationAuthorized,
    phase458GateSatisfied = false,
    confirmationEvidence = false,
    explorationOnly = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    humanRulingAuthored = false,
    o4Discharged,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion,
    promotedPhysicalMassClaimCount,
    a10BoundaryHeld,
    decision = censusClassification switch
    {
        "invalid-precursor" => "At least one committed byte, schema, terminal, or semantic binding failed. No retained-data conclusion is available.",
        "retained-data-insufficient-for-identifiable-repair" => "The retained artifact preserves useful aggregate spatial correlators and aligned jackknife summaries, but T=4 supplies only three independent temporal values and no configuration-level samples. It cannot identify the admitted four-parameter two-pole class. Preserve Phase456's invalid terminal and specify prospective acquisition separately.",
        _ => "The retained artifact meets the frozen minimum support for prospective estimator design. This does not validate an estimator or authorize sampling.",
    },
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, OutputSlug + ".json"), json);
File.WriteAllText(Path.Combine(OutputDir, OutputSlug + "_summary.json"), json);

Require(taxonomy.Contains(censusClassification, StringComparer.Ordinal), "taxonomy drifted");
Require(inputsValid, result.terminalStatus);
Require(censusClassification == "retained-data-insufficient-for-identifiable-repair", "expected conservative census terminal drifted");
Require(!retainedDataSufficientForProspectiveEstimatorDesign && !twoPoleStructurallyIdentifiableFromTemporalSupport, "identifiability firewall failed");
Require(a10BoundaryHeld && promotedPhysicalMassClaimCount == 0, "A10 authority or claim firewall failed");
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"T={temporalExtent} uniqueT={uniquePeriodicSeparations} k={spatialMomentumCount} jackknife={minimumJackknifeCount} productionBelowNEff={productionColumnsBelowNEffFloor}");

static FrozenJson Bind(string path, string expectedSha256, List<string> errors)
{
    if (!File.Exists(path))
    {
        errors.Add("Missing input: " + path);
        return new FrozenJson(path, expectedSha256, "missing", 0, null);
    }
    byte[] bytes = File.ReadAllBytes(path);
    string actual = Sha256(bytes);
    if (actual != expectedSha256) errors.Add("Byte hash drifted: " + path);
    try { return new FrozenJson(path, expectedSha256, actual, bytes.LongLength, JsonDocument.Parse(bytes)); }
    catch (JsonException) { errors.Add("Malformed JSON: " + path); return new FrozenJson(path, expectedSha256, actual, bytes.LongLength, null); }
}

static bool ExactPeriodic(JsonElement array)
{
    if (array.GetArrayLength() != 4) return false;
    double t1 = array[1].GetDouble(), t3 = array[3].GetDouble();
    double scale = System.Math.Max(1.0, System.Math.Max(System.Math.Abs(t1), System.Math.Abs(t3)));
    return System.Math.Abs(t1 - t3) <= 1e-10 * scale;
}

static int Len(JsonElement element, string property) => element.GetProperty(property).GetArrayLength();
static string S(JsonElement element, string property) => element.GetProperty(property).GetString() ?? throw new InvalidOperationException(property + " is null");
static bool B(JsonElement element, string property) => element.GetProperty(property).GetBoolean();
static int I(JsonElement element, string property) => element.GetProperty(property).GetInt32();
static double D(JsonElement element, string property) => element.GetProperty(property).GetDouble();
static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed class FrozenJson
{
    public FrozenJson(string path, string expectedSha256, string actualSha256, long byteCount, JsonDocument? document)
        => (Path, ExpectedSha256, ActualSha256, ByteCount, Document) = (path, expectedSha256, actualSha256, byteCount, document);
    public string Path { get; }
    public string ExpectedSha256 { get; }
    public string ActualSha256 { get; }
    public long ByteCount { get; }
    public JsonDocument? Document { get; }
    public JsonElement Root => Document?.RootElement ?? throw new InvalidOperationException("Unavailable JSON: " + Path);
    public object View() => new { path = Path, expectedSha256 = ExpectedSha256, actualSha256 = ActualSha256, byteHashMatches = ExpectedSha256 == ActualSha256, byteCount = ByteCount, schemaReadable = Document is not null };
}
