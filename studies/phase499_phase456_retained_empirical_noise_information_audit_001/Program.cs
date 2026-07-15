using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string Phase456ArtifactPath = "studies/phase456_consolidated_n4_launch_001/production/phase452_n4/scalar_channel_spectroscopy_probe.json";
const string Phase496Path = "studies/phase496_phase456_retained_data_information_census_001/output/phase456_retained_data_information_census_summary.json";
const string OutputDir = "studies/phase499_phase456_retained_empirical_noise_information_audit_001/output";
const string OutputSlug = "phase456_retained_empirical_noise_information_audit";
const string Phase456ArtifactSha256 = "9b7e965a0b8ac906bc1352f908b28b6eb22511579c02ee91562f63beb67ed9cb";
const string Phase496Sha256 = "f87307724ec69e239b1b1354376274e7beced92559f00a7b5ef517012d92a9a9";
const string DecisionContractCanonical =
    "phase499-a11-v1|precedence=invalid-precursor>retained-t4-noise-information-recoverable-t16-calibration-unavailable>retained-calibration-sufficient-for-frozen-t16-envelope|" +
    "valid=exact-byte-schema-and-phase496-insufficient-terminal-and-five-columns-and-t4-and-fifty-aligned-finite-delete-block-rows|" +
    "finite-envelope=jackknife-variance-and-absolute-correlation-extrema-over-retained-values-only|" +
    "no-temporal-kernel-extrapolation-no-raw-configuration-reconstruction-no-t16-transfer-no-power-guarantee-no-sampling-no-authority";

var errors = new List<string>();
FrozenJson phase456 = Bind(Phase456ArtifactPath, Phase456ArtifactSha256, errors);
FrozenJson phase496 = Bind(Phase496Path, Phase496Sha256, errors);

bool phase496SemanticBinding = false;
int temporalExtent = 0, columnCount = 0, storedFamilyCount = 0, storedDeleteBlockRowCount = 0;
int retainedScalarTauCount = 0, retainedScalarNEffCount = 0;
double minimumVariance = double.PositiveInfinity, maximumVariance = double.NegativeInfinity;
double minimumAbsoluteNontrivialCorrelation = double.PositiveInfinity, maximumAbsoluteNontrivialCorrelation = double.NegativeInfinity;
double minimumTau = double.PositiveInfinity, maximumTau = double.NegativeInfinity;
double minimumNEff = double.PositiveInfinity, maximumNEff = double.NegativeInfinity;
bool allDeleteBlockRowsFiniteAndAligned = true;
bool allStoredFullEstimatesFinite = true;
bool periodicT4Exact = true;
var columnSummaries = new List<object>();

try
{
    JsonElement p496 = phase496.Root;
    phase496SemanticBinding = S(p496, "phaseId") == "phase496-phase456-retained-data-information-census"
        && S(p496, "verdictKind") == "retained-data-insufficient-for-identifiable-repair"
        && B(p496, "inputsValid")
        && !B(p496, "retainedDataSufficientForProspectiveEstimatorDesign")
        && I(p496.GetProperty("temporalInventory"), "temporalExtent") == 4
        && B(p496.GetProperty("covarianceInventory"), "alignedDeleteBlockSummariesRetained")
        && !B(p496.GetProperty("covarianceInventory"), "configurationLevelSamplesRetained");

    JsonElement torus = phase456.Root.GetProperty("tori").EnumerateArray().Single(x => I(x, "torusN") == 4);
    temporalExtent = I(torus, "torusN");
    JsonElement[] columns = torus.GetProperty("columns").EnumerateArray().ToArray();
    columnCount = columns.Length;
    foreach (JsonElement column in columns)
    {
        JsonElement p = column.GetProperty("phase456");
        var families = new List<(string Name, JsonElement Full, JsonElement Jackknife)>
        {
            ("identity-irrep-dominant", p.GetProperty("identityIrrep2x2Gevp").GetProperty("dominantEigenvalue"), p.GetProperty("identityIrrep2x2Gevp").GetProperty("jackknifeDominantEigenvalue")),
            ("a2-o1", p.GetProperty("a2").GetProperty("o1").GetProperty("c"), p.GetProperty("a2").GetProperty("o1").GetProperty("jackknife")),
            ("a2-o2", p.GetProperty("a2").GetProperty("o2").GetProperty("c"), p.GetProperty("a2").GetProperty("o2").GetProperty("jackknife")),
            ("kmin-o1", p.GetProperty("kMin").GetProperty("spatialAxisAverageO1").GetProperty("c"), p.GetProperty("kMin").GetProperty("spatialAxisAverageO1").GetProperty("jackknife")),
        };
        int spatialModeCount = 0;
        foreach (JsonElement mode in p.GetProperty("perSiteSpatialCorrelators").EnumerateArray())
        {
            families.Add(("spatial-mode-" + spatialModeCount, mode.GetProperty("c"), mode.GetProperty("jackknife")));
            spatialModeCount++;
        }

        double columnMinVariance = double.PositiveInfinity, columnMaxVariance = double.NegativeInfinity;
        double columnMaxAbsCorrelation = double.NegativeInfinity;
        foreach ((string _, JsonElement full, JsonElement jackknife) in families)
        {
            double[] fullValues = FiniteArray(full);
            double[][] rows = FiniteRows(jackknife);
            allStoredFullEstimatesFinite &= fullValues.Length == temporalExtent;
            allDeleteBlockRowsFiniteAndAligned &= rows.Length == 50 && rows.All(x => x.Length == temporalExtent);
            periodicT4Exact &= fullValues.Length == 4 && ApproximatelyEqual(fullValues[1], fullValues[3]);
            if (rows.Length != 50 || rows.Any(x => x.Length != temporalExtent)) continue;

            double[,] covariance = JackknifeCovariance(rows);
            for (int i = 0; i < temporalExtent; i++)
            {
                double variance = covariance[i, i];
                Update(ref minimumVariance, ref maximumVariance, variance);
                Update(ref columnMinVariance, ref columnMaxVariance, variance);
                for (int j = i + 1; j < temporalExtent; j++)
                {
                    double denom = System.Math.Sqrt(covariance[i, i] * covariance[j, j]);
                    if (!(denom > 0.0)) continue;
                    double absCorrelation = System.Math.Min(1.0, System.Math.Abs(covariance[i, j] / denom));
                    Update(ref minimumAbsoluteNontrivialCorrelation, ref maximumAbsoluteNontrivialCorrelation, absCorrelation);
                    columnMaxAbsCorrelation = System.Math.Max(columnMaxAbsCorrelation, absCorrelation);
                }
            }
        }

        double[] taus =
        {
            D(column, "tauIntO1"), D(column, "tauIntO2"), D(column, "tauIntS"),
            D(p.GetProperty("rowEffectiveSampleSizes"), "tauA2"), D(p.GetProperty("rowEffectiveSampleSizes"), "tauKMin"),
        };
        double[] nEffs =
        {
            D(column, "nEff"), D(p.GetProperty("rowEffectiveSampleSizes"), "nEffA2"), D(p.GetProperty("rowEffectiveSampleSizes"), "nEffKMin"),
        };
        foreach (double value in taus) Update(ref minimumTau, ref maximumTau, value);
        foreach (double value in nEffs) Update(ref minimumNEff, ref maximumNEff, value);
        retainedScalarTauCount += taus.Length;
        retainedScalarNEffCount += nEffs.Length;
        storedFamilyCount += families.Count;
        storedDeleteBlockRowCount += families.Sum(x => x.Jackknife.GetArrayLength());
        columnSummaries.Add(new
        {
            member = S(column, "member"), kind = S(column, "kind"), beta = D(column, "beta"),
            storedFamilyCount = families.Count, spatialModeCount, deleteBlockRowsPerFamily = 50,
            finiteJackknifeVarianceMinimum = columnMinVariance,
            finiteJackknifeVarianceMaximum = columnMaxVariance,
            finiteAbsoluteTemporalCorrelationMaximum = columnMaxAbsCorrelation,
            retainedTauRange = new[] { taus.Min(), taus.Max() },
            retainedNEffRange = new[] { nEffs.Min(), nEffs.Max() },
        });
    }
}
catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or JsonException)
{
    errors.Add("Committed schema/finite reconstruction failed: " + ex.Message);
}

if (!phase496SemanticBinding) errors.Add("Phase496 semantic binding drifted.");
if (temporalExtent != 4 || columnCount != 5) errors.Add("Expected retained T=4 five-column scope drifted.");
if (!allStoredFullEstimatesFinite || !allDeleteBlockRowsFiniteAndAligned) errors.Add("A stored estimate or delete-block row is absent, non-finite, or misaligned.");
if (!periodicT4Exact) errors.Add("Stored T=4 periodic symmetry drifted.");
if (!FiniteEnvelope(minimumVariance, maximumVariance) || !FiniteEnvelope(minimumTau, maximumTau) || !FiniteEnvelope(minimumNEff, maximumNEff))
    errors.Add("A finite retained empirical envelope could not be constructed.");

bool inputsValid = errors.Count == 0;
bool t4AggregateVarianceRecoverable = inputsValid;
bool t4AggregateCovarianceRecoverable = inputsValid;
bool t4AlignedChannelAndMomentumCovarianceRecoverable = inputsValid;
bool retainedScalarAutocorrelationDiagnosticsRecoverable = inputsValid;
const bool rawTrajectorySeriesRetained = false;
const bool configurationLevelSamplesRetained = false;
const bool t16TemporalCovarianceKernelRecoverable = false;
const bool t16AutocorrelationPenaltyRecoverable = false;
const bool t16ChannelCovarianceScaleRecoverable = false;
const bool t16MomentumCovarianceScaleRecoverable = false;
const bool t16DeleteBlockDesignRecoverable = false;
bool retainedCalibrationSufficientForT16 = inputsValid && t16TemporalCovarianceKernelRecoverable
    && t16AutocorrelationPenaltyRecoverable && t16ChannelCovarianceScaleRecoverable
    && t16MomentumCovarianceScaleRecoverable && t16DeleteBlockDesignRecoverable;
string classification = !inputsValid ? "invalid-precursor"
    : retainedCalibrationSufficientForT16 ? "retained-calibration-sufficient-for-frozen-t16-envelope"
    : "retained-t4-noise-information-recoverable-t16-calibration-unavailable";

string[] taxonomy =
{
    "invalid-precursor",
    "retained-t4-noise-information-recoverable-t16-calibration-unavailable",
    "retained-calibration-sufficient-for-frozen-t16-envelope",
};
string decisionContractSha256 = Sha256(Encoding.UTF8.GetBytes(DecisionContractCanonical));
const bool phase456ArtifactMutated = false;
const bool unsupportedTemporalExtrapolationPerformed = false;
const bool aggregateSummariesRelabeledAsRawSamples = false;
const bool empiricalEnvelopeUsedAsProductionPowerGuarantee = false;
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
bool a11BoundaryHeld = !phase456ArtifactMutated && !unsupportedTemporalExtrapolationPerformed
    && !aggregateSummariesRelabeledAsRawSamples && !empiricalEnvelopeUsedAsProductionPowerGuarantee
    && !phase481PackCreated && !hmcRun && !benchmarkRun && zeroNewSampling && !samplingAuthorized
    && !productionAuthorized && !phase458G3Satisfied && !phase458G5Satisfied && !phase458EvaluationAuthorized
    && !o4Discharged && !sourceContractApplicationAllowed && noGevPromotion && promotedPhysicalMassClaimCount == 0;

var result = new
{
    phaseId = "phase499-phase456-retained-empirical-noise-information-audit",
    terminalStatus = "phase456-retained-empirical-noise-information-audit-" + classification,
    verdictKind = classification,
    inputsValid,
    classification,
    retainedCalibrationSufficientForT16,
    applicationSubjectKind = "phase456-committed-retained-empirical-noise-information-read-only-audit",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A11; Phase499",
    taxonomy,
    decisionContract = new { schemaId = "phase499-a11-retained-empirical-noise-audit-v1", canonical = DecisionContractCanonical, sha256 = decisionContractSha256 },
    precursorBindings = new { phase456 = phase456.View(), phase496 = phase496.View(), phase496SemanticBinding, errors },
    retainedScope = new
    {
        temporalExtent, columnCount, storedFamilyCount, storedDeleteBlockRowCount,
        retainedScalarTauCount, retainedScalarNEffCount,
        fullSpatialMomentumModesPerColumn = 64, alignedDeleteBlockRowsPerFamily = 50,
        rawTrajectorySeriesRetained, configurationLevelSamplesRetained,
    },
    informationClassification = new object[]
    {
        Item("aggregate variances", "directly-recoverable-retained-t4", t4AggregateVarianceRecoverable, "Finite aligned delete-block summaries reconstruct aggregate jackknife variances at the four stored separations."),
        Item("aggregate temporal covariance", "t4-only-nontransferable-to-t16", t4AggregateCovarianceRecoverable, "The retained 4x4 covariance is descriptive at T=4; no additional temporal lags determine a T=16 kernel."),
        Item("channel and momentum aggregate covariance", "t4-only-nontransferable-to-t16", t4AlignedChannelAndMomentumCovarianceRecoverable, "Aligned delete-block labels support within-column aggregate cross-covariance, but not a prospective T=16 scale law."),
        Item("reported autocorrelation and effective-sample-size diagnostics", "directly-recoverable-retained-t4", retainedScalarAutocorrelationDiagnosticsRecoverable, "Stored scalar diagnostics are finite; their source trajectories are not retained for re-estimation."),
        Item("T=16 temporal covariance kernel", "unavailable-for-t16", t16TemporalCovarianceKernelRecoverable, "T=4 has only three periodic-independent values and does not identify twelve additional separations or a kernel family."),
        Item("T=16 autocorrelation penalty", "unavailable-for-t16", t16AutocorrelationPenaltyRecoverable, "No raw Markov-chain series permits a changed-volume or changed-observable autocorrelation estimate."),
        Item("T=16 channel/momentum covariance scale", "unavailable-for-t16", t16ChannelCovarianceScaleRecoverable && t16MomentumCovarianceScaleRecoverable, "Retained aggregate variation provides no frozen temporal-extent scaling law."),
        Item("T=16 delete-block design", "unavailable-for-t16", t16DeleteBlockDesignRecoverable, "Fifty stored delete-block aggregates do not recover raw blocking histories or establish a prospective block length."),
    },
    finiteRetainedEmpiricalEnvelope = new
    {
        scope = "descriptive extrema over finite committed T=4 aggregate summaries only",
        minimumJackknifeVariance = minimumVariance,
        maximumJackknifeVariance = maximumVariance,
        minimumAbsoluteNontrivialTemporalCorrelation = minimumAbsoluteNontrivialCorrelation,
        maximumAbsoluteNontrivialTemporalCorrelation = maximumAbsoluteNontrivialCorrelation,
        minimumRetainedTau = minimumTau,
        maximumRetainedTau = maximumTau,
        minimumRetainedNEff = minimumNEff,
        maximumRetainedNEff = maximumNEff,
        isT16NoiseModel = false,
        isProductionPowerGuarantee = false,
        columnSummaries,
    },
    t16TransferAudit = new
    {
        t16TemporalCovarianceKernelRecoverable,
        t16AutocorrelationPenaltyRecoverable,
        t16ChannelCovarianceScaleRecoverable,
        t16MomentumCovarianceScaleRecoverable,
        t16DeleteBlockDesignRecoverable,
        unsupportedTemporalExtrapolationPerformed,
        conclusion = "Retained T=4 summaries constrain descriptive adversarial envelopes only. They cannot calibrate or guarantee a T=16 acquisition without explicitly frozen additional assumptions.",
    },
    phase456ArtifactMutated,
    aggregateSummariesRelabeledAsRawSamples,
    empiricalEnvelopeUsedAsProductionPowerGuarantee,
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
    a11BoundaryHeld,
    decision = !inputsValid
        ? "A committed byte, schema, finite-value, or semantic binding failed. No empirical-noise information conclusion is available."
        : "Finite aligned Phase456 summaries recover descriptive T=4 aggregate variance, covariance, and stored autocorrelation diagnostics. They do not identify a T=16 covariance kernel, autocorrelation penalty, channel or momentum scaling, or delete-block design. Any prospective oracle must freeze those as assumptions and may not call them empirical calibration.",
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, OutputSlug + ".json"), json);
File.WriteAllText(Path.Combine(OutputDir, OutputSlug + "_summary.json"), json);

Require(taxonomy.Contains(classification, StringComparer.Ordinal), "taxonomy drifted");
Require(inputsValid, result.terminalStatus);
Require(classification == "retained-t4-noise-information-recoverable-t16-calibration-unavailable", "expected T=16 calibration firewall drifted");
Require(!retainedCalibrationSufficientForT16 && !unsupportedTemporalExtrapolationPerformed, "T=16 transfer firewall failed");
Require(a11BoundaryHeld && promotedPhysicalMassClaimCount == 0, "A11 authority or claim firewall failed");
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"T={temporalExtent} columns={columnCount} families={storedFamilyCount} deleteBlockRows={storedDeleteBlockRowCount}");

static object Item(string feature, string classification, bool retainedEvidenceAvailable, string evidence) =>
    new { feature, classification, retainedEvidenceAvailable, evidence };

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

static double[] FiniteArray(JsonElement array)
{
    double[] values = array.EnumerateArray().Select(x => x.GetDouble()).ToArray();
    if (values.Any(x => !double.IsFinite(x))) throw new InvalidOperationException("Non-finite retained scalar.");
    return values;
}

static double[][] FiniteRows(JsonElement array)
{
    double[][] rows = array.EnumerateArray().Select(FiniteArray).ToArray();
    if (rows.Select(x => x.Length).Distinct().Count() > 1) throw new InvalidOperationException("Ragged retained rows.");
    return rows;
}

static double[,] JackknifeCovariance(double[][] rows)
{
    int n = rows.Length, p = rows[0].Length;
    double[] means = Enumerable.Range(0, p).Select(j => rows.Average(x => x[j])).ToArray();
    var covariance = new double[p, p];
    for (int i = 0; i < p; i++)
        for (int j = 0; j < p; j++)
            covariance[i, j] = (n - 1.0) / n * rows.Sum(x => (x[i] - means[i]) * (x[j] - means[j]));
    return covariance;
}

static bool ApproximatelyEqual(double a, double b)
{
    double scale = System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
    return System.Math.Abs(a - b) <= 1e-10 * scale;
}

static void Update(ref double minimum, ref double maximum, double value)
{
    if (!double.IsFinite(value) || value < 0.0) throw new InvalidOperationException("Invalid finite envelope value.");
    minimum = System.Math.Min(minimum, value);
    maximum = System.Math.Max(maximum, value);
}

static bool FiniteEnvelope(double minimum, double maximum) =>
    double.IsFinite(minimum) && double.IsFinite(maximum) && minimum >= 0.0 && maximum >= minimum;
static string S(JsonElement element, string property) => element.GetProperty(property).GetString() ?? throw new InvalidOperationException(property + " is null");
static bool B(JsonElement element, string property) => element.GetProperty(property).GetBoolean();
static int I(JsonElement element, string property) => element.GetProperty(property).GetInt32();
static double D(JsonElement element, string property)
{
    double value = element.GetProperty(property).GetDouble();
    if (!double.IsFinite(value) || value < 0.0) throw new InvalidOperationException(property + " is not finite nonnegative");
    return value;
}
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
