using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase468: Two-Loop Content-Row Closure Filter (C-CLOSURE items ii/iii).
//
// The row menu is committed below before any score is evaluated.  It contains
// the mandatory Phase451 SM witness, the complete Phase404-family extension,
// and the two Phase407-induced frame-cross rows: the isolated four-complex-
// doublet scalar proxy and the full four-copy 10 scalar proxy (four doublets
// plus four color triplets).  Phase407's carrier is spacetime-vector-valued,
// so both proxy rows are explicitly conditional bookkeeping tests, never a
// claim that the corresponding low-energy fields or thresholds exist.
//
// Each row is run through the Phase451 two-loop gauge-only system.  The exact
// Phase451 SM b_ij matrix is the mandatory witness.  Non-SM matrices are formed
// by adding standard gauge-only matter deltas to that committed matrix; this
// preserves the Phase451 convention byte-for-byte at the witness while making
// every extension explicit.  Yukawa terms and unknown intermediate thresholds
// remain declared omissions.  A within-band row is candidate-only; a full-menu
// miss is a scoped exhaustion record for C-PERMANENCE P4.

var stopwatch = Stopwatch.StartNew();

const string OutputDir = "studies/phase468_two_loop_content_row_closure_filter_001/output";
const string Phase404Path = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase407Path = "studies/phase407_chimeric_adjoint_sm_content_probe_001/output/chimeric_adjoint_sm_content_probe_summary.json";
const string Phase451Path = "studies/phase451_two_loop_unification_ledger_001/output/two_loop_unification_ledger_summary.json";
const string ApplicationSubjectKind = "two-loop-content-row-closure-filter";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 8";
const double AlphaEmInverse = 127.955;
const double AlphaS = 0.1179;
const double ObservedSin2 = 0.23122;
const double IntegrationStep = 1.0 / 256.0;

Directory.CreateDirectory(OutputDir);

// PRE-COMMITTED MENU.  Do not add or remove a row after inspecting scores.
var menu = new[]
{
    new ContentRow("sm-witness", 3, 1, 0, "Phase451 mandatory witness: imported three-family multiplicity plus one conditional scalar doublet", false),
    new ContentRow("phase404-complete-family-extension", 4, 1, 0, "Phase404-derived complete 16 added to the Phase451 witness content", true),
    new ContentRow("phase407-doublet-subblock-scalar-proxy", 3, 5, 0, "Phase407 16-real-state frame-cross doublet block represented as four complex scalar doublets, in addition to the witness doublet", true),
    new ContentRow("phase407-full-frame-cross-10-scalar-proxy", 3, 5, 4, "Phase407 full 4x10 frame-cross block represented as four complete scalar 10 contents: four complex doublets plus four complex color triplets", true),
};
string menuCanonical = string.Join("\n", menu.Select(r => r.Canonical));
string menuHash = Sha256(menuCanonical);
const string MenuHashPin = "1fcff9e863df9b06d89b67802fad216976d8fbbdd8d160fe5f9c5d7f958867d5";
bool menuHashMatches = menuHash == MenuHashPin;

bool phase404Present = File.Exists(Phase404Path);
bool phase407Present = File.Exists(Phase407Path);
bool phase451Present = File.Exists(Phase451Path);
bool phase404Passed = false, phase407Passed = false, phase451Passed = false;
bool phase407DoubletCountMatches = false;
double phase451WitnessPrediction = double.NaN;
double thresholdBand = double.NaN;
string phase451B3 = "", phase451B2 = "", phase451B1 = "";
double[][] phase451Matrix = Array.Empty<double[]>();

if (phase404Present)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Phase404Path));
    phase404Passed = doc.RootElement.GetProperty("guEmbeddingChainCouplingRatioEnumerationPassed").GetBoolean()
        && doc.RootElement.GetProperty("familyPatternDerived").GetBoolean();
}
if (phase407Present)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Phase407Path));
    var root = doc.RootElement;
    phase407Passed = root.GetProperty("chimericAdjointSmContentProbePassed").GetBoolean();
    phase407DoubletCountMatches = root.GetProperty("higgsPatternStateCount").GetInt32() == 16;
}
if (phase451Present)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Phase451Path));
    var root = doc.RootElement;
    phase451Passed = root.GetProperty("twoLoopUnificationLedgerPassed").GetBoolean();
    phase451WitnessPrediction = root.GetProperty("predictions").GetProperty("sin2MzPredictedTwoLoop").GetDouble();
    thresholdBand = root.GetProperty("thresholdUncertainty").GetProperty("thresholdBandTwoLoop").GetDouble();
    var b = root.GetProperty("derivedInputs").GetProperty("oneLoopBetaAfPositive");
    phase451B3 = b.GetProperty("b3").GetString() ?? "";
    phase451B2 = b.GetProperty("b2").GetString() ?? "";
    phase451B1 = b.GetProperty("b1").GetString() ?? "";
    phase451Matrix = root.GetProperty("derivedInputs").GetProperty("twoLoopBijStandardConvention")
        .GetProperty("matrix").EnumerateArray()
        .Select(row => row.EnumerateArray().Select(x => ParseFraction(x.GetString() ?? "0")).ToArray()).ToArray();
}

bool phase451OneLoopWitnessMatches = phase451B3 == "7" && phase451B2 == "19/6" && phase451B1 == "-41/10";
bool phase451MatrixWitnessMatches = phase451Matrix.Length == 3
    && Nearly(phase451Matrix[0][0], 199.0 / 50.0)
    && Nearly(phase451Matrix[0][1], 27.0 / 10.0)
    && Nearly(phase451Matrix[0][2], 44.0 / 5.0)
    && Nearly(phase451Matrix[1][0], 27.0 / 50.0)
    && Nearly(phase451Matrix[1][1], 35.0 / 6.0)
    && Nearly(phase451Matrix[1][2], 12.0)
    && Nearly(phase451Matrix[2][0], 11.0 / 10.0)
    && Nearly(phase451Matrix[2][1], 9.0 / 2.0)
    && Nearly(phase451Matrix[2][2], -26.0);

var smFields = BuildFields(3, 1, 0);
double[][] genericSmMatrix = TwoLoopMatrix(smFields);
var scores = new List<RowScore>();
foreach (var row in menu)
{
    var fields = BuildFields(row.FamilyCount, row.DoubletCount, row.ColorTripletCount);
    double[] bAf = OneLoopAf(fields);
    double[][] generic = TwoLoopMatrix(fields);
    double[][] matrix = AddMatrix(phase451Matrix, SubtractMatrix(generic, genericSmMatrix));
    var solve = SolveUnification(bAf, matrix);
    double gap = solve.Sin2 - ObservedSin2;
    bool withinBand = solve.Converged && System.Math.Abs(gap) <= thresholdBand;
    scores.Add(new RowScore(row, bAf, matrix, solve, gap, withinBand));
}

var witness = scores.Single(s => s.Row.Id == "sm-witness");
bool smWitnessReproduced = witness.Solve.Converged
    && System.Math.Abs(witness.Solve.Sin2 - phase451WitnessPrediction) <= 2e-10;
bool stepHalvingPassed = scores.All(score =>
{
    var half = SolveUnification(score.BAf, score.Matrix, IntegrationStep / 2.0);
    return half.Converged && System.Math.Abs(half.Sin2 - score.Solve.Sin2) <= 1e-8;
});

// Synthetic reachability: a deliberately shifted b2 must be able to cross the
// band.  This proves that the classifier is not structurally stuck on "miss".
double[] syntheticB = witness.BAf.ToArray();
double syntheticLo = -2.0, syntheticHi = 0.0;
SolveResult synthetic = default;
for (int iter = 0; iter < 60; iter++)
{
    double shift = 0.5 * (syntheticLo + syntheticHi);
    syntheticB[1] = witness.BAf[1] + shift;
    synthetic = SolveUnification(syntheticB, witness.Matrix);
    if (!synthetic.Converged) break;
    if (synthetic.Sin2 > ObservedSin2) syntheticLo = shift;
    else syntheticHi = shift;
}
bool syntheticWithinBand = synthetic.Converged && System.Math.Abs(synthetic.Sin2 - ObservedSin2) <= thresholdBand;

int withinBandRowCount = scores.Count(s => s.WithinBand);
bool fullMenuOutOfBand = scores.All(s => !s.WithinBand);
bool allBatteriesPassed = phase451OneLoopWitnessMatches && phase451MatrixWitnessMatches
    && smWitnessReproduced && stepHalvingPassed && syntheticWithinBand;
bool inputsPassed = phase404Passed && phase407Passed && phase407DoubletCountMatches
    && phase451Passed && menuHashMatches;
bool executed = inputsPassed && allBatteriesPassed;

string verdictKind;
if (!inputsPassed) verdictKind = "content-row-filter-input-or-menu-drift";
else if (!allBatteriesPassed) verdictKind = "content-row-filter-battery-failed";
else if (withinBandRowCount > 0) verdictKind = "lineage-relevant-candidate-rows-found";
else verdictKind = "full-menu-out-of-band-scoped-exhaustion";
string terminalStatus = $"two-loop-content-row-closure-filter-{verdictKind}";

const bool targetBlindMenuConstruction = true;
const bool observedSin2UsedInScoringOnly = true;
const bool observedSin2UsedToConstructMenu = false;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
const bool sourceContractApplicationAllowed = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;

stopwatch.Stop();
var result = new
{
    phaseId = "phase468-two-loop-content-row-closure-filter",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    twoLoopContentRowClosureFilterExecuted = executed,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    menu = new
    {
        menuHash,
        menuHashPin = MenuHashPin,
        menuHashMatches,
        rowCount = menu.Length,
        rows = menu.Select(r => new { r.Id, r.FamilyCount, r.DoubletCount, r.ColorTripletCount, r.Provenance, r.ConditionalProxy }).ToArray(),
        committedBeforeScoring = true,
    },
    precursors = new
    {
        phase404SummaryPath = Phase404Path,
        phase407SummaryPath = Phase407Path,
        phase451SummaryPath = Phase451Path,
        phase404Present,
        phase407Present,
        phase451Present,
        phase404Passed,
        phase407Passed,
        phase407DoubletCountMatches,
        phase451Passed,
    },
    filter = new
    {
        alphaEmInverse = AlphaEmInverse,
        alphaS = AlphaS,
        observedSin2ScoringTarget = ObservedSin2,
        thresholdBand,
        integrationStep = IntegrationStep,
        twoLoopConvention = "Phase451 gauge-only b_ij witness plus standard gauge-only matter deltas; Yukawa traces omitted",
        contentActiveInterval = "full m_Z-to-crossing interval (maximal fixed-content proxy; no source-defined intermediate threshold exists)",
        fullMenuOutOfBand,
        withinBandRowCount,
        cPermanenceP4ScopedExhaustion = executed && fullMenuOutOfBand,
    },
    scores = scores.Select(s => s.ToOutput(thresholdBand)).ToArray(),
    batteries = new
    {
        phase451OneLoopWitnessMatches,
        phase451MatrixWitnessMatches,
        smWitnessReproduced,
        smWitnessDeviation = witness.Solve.Sin2 - phase451WitnessPrediction,
        stepHalvingPassed,
        syntheticWithinBand,
        allPassed = allBatteriesPassed,
    },
    limitations = new[]
    {
        "Phase407 supplies representation content but not a low-energy action, statistics assignment, or threshold; its rows are explicitly conditional scalar proxies.",
        "The full-interval content placement is a fixed maximal proxy, not a source-derived intermediate scale.",
        "The Phase451 gauge-only two-loop convention omits Yukawa traces; this filter inherits that declared truncation.",
        "Even a within-band row would be lineage-relevant candidate bookkeeping only and could not promote a coupling, mass, or GU-specific prediction.",
    },
    targetBlindMenuConstruction,
    observedSin2UsedInScoringOnly,
    observedSin2UsedToConstructMenu,
    noGevPromotion,
    promotedPhysicalMassClaimCount,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
    decision = executed && fullMenuOutOfBand
        ? "Every pre-committed Phase404/407-induced content row remains outside the Phase451 honest two-loop threshold band. This is a scoped C-CLOSURE exhaustion record for the audited full-interval scalar-proxy menu, not an absolute running theorem: Phase407 does not define field statistics, an action, or intermediate thresholds. No coupling normalization, scale, pole, or physical prediction is promoted."
        : executed
            ? "At least one pre-committed row enters the Phase451 threshold band and is recorded as a lineage-relevant candidate only. It cannot be promoted without a source-defined action, statistics, threshold, and the standing lineage contracts."
            : "Do not consume this phase unless precursor pins, the pre-committed menu hash, the Phase451 witness, convergence, and reachability batteries all pass.",
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "two_loop_content_row_closure_filter.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "two_loop_content_row_closure_filter_summary.json"), json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"twoLoopContentRowClosureFilterExecuted={executed}");
Console.WriteLine($"menuHash={menuHash}");
Console.WriteLine($"smWitnessReproduced={smWitnessReproduced}");
Console.WriteLine($"withinBandRowCount={withinBandRowCount}");
foreach (var score in scores) Console.WriteLine($"{score.Row.Id}: sin2={score.Solve.Sin2:R}; gap={score.Gap:R}; withinBand={score.WithinBand}");
Console.WriteLine($"promotedPhysicalMassClaimCount={promotedPhysicalMassClaimCount}");
Environment.ExitCode = executed ? 0 : 2;

static List<Field> BuildFields(int families, int doublets, int colorTriplets)
{
    var fields = new List<Field>();
    for (int f = 0; f < families; f++)
    {
        fields.Add(new Field("Q", true, 3, 2, 1.0 / 6.0));
        fields.Add(new Field("u", true, 3, 1, -2.0 / 3.0));
        fields.Add(new Field("d", true, 3, 1, 1.0 / 3.0));
        fields.Add(new Field("L", true, 1, 2, -1.0 / 2.0));
        fields.Add(new Field("e", true, 1, 1, 1.0));
        fields.Add(new Field("n", true, 1, 1, 0.0));
    }
    for (int h = 0; h < doublets; h++) fields.Add(new Field("H", false, 1, 2, 1.0 / 2.0));
    for (int t = 0; t < colorTriplets; t++) fields.Add(new Field("T", false, 3, 1, -1.0 / 3.0));
    return fields;
}

static double[] OneLoopAf(List<Field> fields)
{
    double[] b = { 0.0, 22.0 / 3.0, 11.0 }; // order U1, SU2, SU3
    foreach (var field in fields)
    for (int i = 0; i < 3; i++)
        b[i] -= (field.Weyl ? 2.0 / 3.0 : 1.0 / 3.0) * Index(field, i);
    return b;
}

static double[][] TwoLoopMatrix(List<Field> fields)
{
    double[] ca = { 0, 2, 3 };
    var m = new[] { new double[3], new double[3], new double[3] };
    for (int i = 0; i < 3; i++) m[i][i] = -34.0 / 3.0 * ca[i] * ca[i];
    foreach (var field in fields)
    for (int i = 0; i < 3; i++)
    {
        double ti = Index(field, i);
        if (ti == 0) continue;
        for (int j = 0; j < 3; j++)
        {
            double cj = Casimir(field, j);
            if (i == j)
                m[i][j] += (field.Weyl ? 2 * cj + 10.0 / 3.0 * ca[i] : 4 * cj + 2.0 / 3.0 * ca[i]) * ti;
            else
                m[i][j] += (field.Weyl ? 2.0 : 4.0) * ti * cj;
        }
    }
    return m;
}

static double Index(Field f, int group) => group switch
{
    0 => 3.0 / 5.0 * f.Y * f.Y * f.ColorDim * f.WeakDim,
    1 => f.WeakDim == 2 ? 0.5 * f.ColorDim : 0,
    2 => f.ColorDim == 3 ? 0.5 * f.WeakDim : 0,
    _ => 0,
};

static double Casimir(Field f, int group) => group switch
{
    0 => 3.0 / 5.0 * f.Y * f.Y,
    1 => f.WeakDim == 2 ? 3.0 / 4.0 : 0,
    2 => f.ColorDim == 3 ? 4.0 / 3.0 : 0,
    _ => 0,
};

static SolveResult SolveUnification(double[] bAfU23, double[][] bijU23, double step = IntegrationStep)
{
    // Convert U1,SU2,SU3 storage to the y1,y2,y3 order already used here.
    double lo = 0.12, hi = 0.34;
    (bool Ok, double Mismatch, double T, double[] Y) Eval(double s2)
    {
        double[] y = { (3.0 / 5.0) * (1.0 - s2) * AlphaEmInverse, s2 * AlphaEmInverse, 1.0 / AlphaS };
        double t = 0;
        double prev = y[0] - y[1];
        double[] prevY = y.ToArray();
        double prevT = 0;
        while (t < 60)
        {
            prevY = y.ToArray(); prevT = t;
            y = Rk4(y, step, bAfU23, bijU23);
            t += step;
            double now = y[0] - y[1];
            if (prev == 0 || now == 0 || prev * now < 0)
            {
                double fraction = prev / (prev - now);
                double crossT = prevT + fraction * step;
                double[] crossY = new double[3];
                for (int i = 0; i < 3; i++) crossY[i] = prevY[i] + fraction * (y[i] - prevY[i]);
                return (true, crossY[2] - 0.5 * (crossY[0] + crossY[1]), crossT, crossY);
            }
            prev = now;
            if (y.Any(v => !double.IsFinite(v) || v <= 0)) break;
        }
        return (false, double.NaN, double.NaN, y);
    }

    var eLo = Eval(lo); var eHi = Eval(hi);
    if (!eLo.Ok || !eHi.Ok || eLo.Mismatch * eHi.Mismatch > 0)
        return new SolveResult(false, double.NaN, double.NaN, double.NaN);
    (bool Ok, double Mismatch, double T, double[] Y) mid = eLo;
    for (int iter = 0; iter < 90; iter++)
    {
        double s = 0.5 * (lo + hi);
        mid = Eval(s);
        if (!mid.Ok) return new SolveResult(false, double.NaN, double.NaN, double.NaN);
        if (System.Math.Abs(mid.Mismatch) < 1e-11) { lo = hi = s; break; }
        if (eLo.Mismatch * mid.Mismatch <= 0) { hi = s; eHi = mid; }
        else { lo = s; eLo = mid; }
    }
    double sin2 = 0.5 * (lo + hi);
    mid = Eval(sin2);
    return new SolveResult(mid.Ok, sin2, mid.T, mid.Ok ? 0.5 * (mid.Y[0] + mid.Y[1]) : double.NaN);
}

static double[] Rk4(double[] y, double h, double[] b, double[][] m)
{
    double[] F(double[] x)
    {
        var d = new double[3];
        for (int i = 0; i < 3; i++)
        {
            d[i] = b[i] / (2 * System.Math.PI);
            for (int j = 0; j < 3; j++) d[i] -= m[i][j] / (8 * System.Math.PI * System.Math.PI * x[j]);
        }
        return d;
    }
    var k1 = F(y);
    var k2 = F(y.Select((v, i) => v + 0.5 * h * k1[i]).ToArray());
    var k3 = F(y.Select((v, i) => v + 0.5 * h * k2[i]).ToArray());
    var k4 = F(y.Select((v, i) => v + h * k3[i]).ToArray());
    return y.Select((v, i) => v + h * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]) / 6.0).ToArray();
}

static double[][] AddMatrix(double[][] a, double[][] b) => a.Select((row, i) => row.Select((x, j) => x + b[i][j]).ToArray()).ToArray();
static double[][] SubtractMatrix(double[][] a, double[][] b) => a.Select((row, i) => row.Select((x, j) => x - b[i][j]).ToArray()).ToArray();
static double ParseFraction(string s)
{
    var parts = s.Split('/');
    return parts.Length == 1 ? double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture)
        : double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture) / double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
}
static bool Nearly(double a, double b) => System.Math.Abs(a - b) <= 1e-12;
static string Sha256(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

sealed record Field(string Name, bool Weyl, int ColorDim, int WeakDim, double Y);
sealed record ContentRow(string Id, int FamilyCount, int DoubletCount, int ColorTripletCount, string Provenance, bool ConditionalProxy)
{
    public string Canonical => $"{Id}|families={FamilyCount}|doublets={DoubletCount}|triplets={ColorTripletCount}|conditional={ConditionalProxy}|{Provenance}";
}
readonly record struct SolveResult(bool Converged, double Sin2, double LogUnificationRatio, double AlphaGutInverse);
sealed record RowScore(ContentRow Row, double[] BAf, double[][] Matrix, SolveResult Solve, double Gap, bool WithinBand)
{
    public object ToOutput(double band) => new
    {
        rowId = Row.Id,
        Row.Provenance,
        Row.ConditionalProxy,
        oneLoopBetaAfPositive = new { b1 = BAf[0], b2 = BAf[1], b3 = BAf[2] },
        twoLoopMatrix = Matrix,
        converged = Solve.Converged,
        sin2PredictedTwoLoop = Solve.Sin2,
        predictedMinusScoringTarget = Gap,
        thresholdBand = band,
        withinBand = WithinBand,
        unificationScaleOverInputScale = Solve.Converged ? System.Math.Exp(Solve.LogUnificationRatio) : double.NaN,
        scaleFieldIsDimensionlessRatio = true,
        alphaGutInverse = Solve.AlphaGutInverse,
        candidateOnly = WithinBand,
    };
}
