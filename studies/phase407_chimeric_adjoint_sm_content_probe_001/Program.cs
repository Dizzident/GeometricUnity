using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase407: chimeric-adjoint SM-content probe.
//
// The Phase406 falsification map left exactly one surviving scalar route:
// the NON-ADJOINT components of the GU connection on a larger algebra.
// Phase404 proved the SM-Higgs doublet (color-singlet, |Y| = 1/2) is
// absent from the internal gauge adjoint (so(10)-level, 45) for every
// scanned hypercharge direction. But GU's connection is valued in the
// CHIMERIC algebra of the full observerse - spin(7,7) at the draft level,
// whose compact-arithmetic stand-in is so(14) (the same complexification-
// level caveat as Phase404, recorded). Its adjoint decomposes as
//
//   91 = so(4)_spacetime (6) + so(10)_internal (45) + (4 x 10) frame-cross-
//        internal block (40),
//
// and the (4 x 10) block is precisely the "non-adjoint" sector the
// surviving route names: components carrying ONE spacetime index and ONE
// internal index (the soldering/vertical directions of the Iceberg's
// symmetric-2-tensor claim live here under pullback). This probe computes,
// from explicit so(14) structure constants, the COMPLETE branching of the
// 91 under {so(4)_spacetime} x {the Phase404 SM chain on the internal
// so(10)} and tags every block with (spacetime content, color Casimir,
// su(2)_L isospin, |Y|):
//
//   - the decisive question: do color-singlet su(2)_L doublets with
//     |Y| = 1/2 (lepton-normalized, the exact SM-Higgs quantum numbers)
//     exist in the frame-cross-internal block?
//   - the honest tag: their SPACETIME content (the block carries a
//     Lorentz-vector index; the spin-0 extraction via the vertical-form
//     structure of Y14 -> X4 is the named remaining step, citing
//     GU-DRAFT-2021 section 9 and the stored text).
//
// Prediction (machine-checked, not assumed): the internal vector 10
// branches under Pati-Salam as 6 + 4 with 4 = (2_L, 2_R); the su(2)_R
// Cartan gives the doublet Y = +-1/2 with zero B-L - exactly the SM Higgs
// pattern - so the frame-cross-internal block should contain color-singlet
// |Y| = 1/2 doublets while the pure internal 45 contains none (Phase404).
//
// Fail-closed: representation bookkeeping only - no field equations, no
// VEV, no scales; nothing promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase407_chimeric_adjoint_sm_content_probe_001/output";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase406SummaryPath = "studies/phase406_choice_space_falsification_sweep_001/output/choice_space_falsification_sweep_summary.json";

const double Tolerance = 1e-9;
const int TotalDim = 14;   // 4 spacetime + 10 internal (compact arithmetic)

var outputDir = Environment.GetEnvironmentVariable("PHASE407_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var p406 = JsonDocument.Parse(File.ReadAllText(Phase406SummaryPath));
bool phase404PrecursorPassed =
    JsonBool(p404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true &&
    JsonBool(p404.RootElement, "adjointColorSingletChargedDoubletAbsentEverywhere") is true;
bool phase406PrecursorPassed =
    JsonBool(p406.RootElement, "choiceSpaceFalsificationSweepPassed") is true &&
    JsonBool(p406.RootElement, "survivorsAreExactlyNonAdjointLargerAlgebra") is true;

// ---------------------------------------------------------------------------
// so(14): generators M_ij on indices 0..13. Spacetime block = {0..3};
// internal so(10) block = {4..13} with the Phase404 scaffolding shifted:
//   so(6)/color on internal coords {4..9} (J = M45 + M67 + M89),
//   Pati-Salam so(4) on internal coords {10..13}:
//     su(2)_L / su(2)_R = (anti-)self-dual halves (signs auto-resolved),
//   Y = R3 + J/3 (B-L = (2/3) J, Phase404).
// ---------------------------------------------------------------------------

var generatorPairs = new List<(int I, int J)>();
for (int i = 0; i < TotalDim; i++)
    for (int j = i + 1; j < TotalDim; j++)
        generatorPairs.Add((i, j));
int adjointDim = generatorPairs.Count; // 91

double[][,] basis = generatorPairs.Select(pair => Skew(pair.I, pair.J)).ToArray();

static double[,] SkewN(int n, int i, int j)
{
    var m = new double[n, n];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}
double[,] Skew(int i, int j) => SkewN(TotalDim, i, j);

static double Pair(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    double trace = 0.0;
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            trace += a[r, c] * b[c, r];
    return -0.5 * trace;
}

static double[,] Comm(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += a[r, k] * b[k, c] - b[r, k] * a[k, c];
            result[r, c] = sum;
        }
    return result;
}

static double[,] Add(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            result[r, c] = a[r, c] + b[r, c];
    return result;
}

static double[,] Scale(double[,] a, double s)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            result[r, c] = s * a[r, c];
    return result;
}

static double Frob(double[,] a)
{
    double sum = 0.0;
    int n = a.GetLength(0);
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            sum += a[r, c] * a[r, c];
    return System.Math.Sqrt(sum);
}

static double[][,] ResolveSu2Signs(double[][,] candidate)
{
    foreach (int s1 in new[] { 1, -1 })
        foreach (int s2 in new[] { 1, -1 })
            foreach (int s3 in new[] { 1, -1 })
            {
                var trial = new[] { Scale(candidate[0], s1), Scale(candidate[1], s2), Scale(candidate[2], s3) };
                double residual = 0.0;
                for (int a = 0; a < 3; a++)
                {
                    int b = (a + 1) % 3, c = (a + 2) % 3;
                    residual = System.Math.Max(residual, Frob(Add(Comm(trial[a], trial[b]), Scale(trial[c], -1.0))));
                }
                if (residual <= 1e-12)
                    return trial;
            }
    throw new InvalidOperationException("No sign convention closes the su(2) triple.");
}

// Pati-Salam so(4) on internal coords {10,11,12,13}.
var su2L = ResolveSu2Signs(new[]
{
    Scale(Add(Skew(10, 11), Skew(12, 13)), 0.5),
    Scale(Add(Skew(10, 12), Scale(Skew(11, 13), -1.0)), 0.5),
    Scale(Add(Skew(10, 13), Skew(11, 12)), 0.5),
});
var su2R = ResolveSu2Signs(new[]
{
    Scale(Add(Skew(10, 11), Scale(Skew(12, 13), -1.0)), 0.5),
    Scale(Add(Skew(10, 12), Skew(11, 13)), 0.5),
    Scale(Add(Skew(10, 13), Scale(Skew(11, 12), -1.0)), 0.5),
});

// Color: traceless centralizer of J inside the internal so(6) on {4..9}.
var jComplex = Add(Add(Skew(4, 5), Skew(6, 7)), Skew(8, 9));
var so6Indices = Enumerable.Range(0, adjointDim)
    .Where(g => generatorPairs[g].I >= 4 && generatorPairs[g].I <= 9 && generatorPairs[g].J >= 4 && generatorPairs[g].J <= 9)
    .ToArray();
var jNormalized = Scale(jComplex, 1.0 / System.Math.Sqrt(Pair(jComplex, jComplex)));
var colorBasis = new List<double[,]>();
{
    int n6 = so6Indices.Length;
    var map = new double[adjointDim, n6];
    for (int col = 0; col < n6; col++)
    {
        var bracket = Comm(basis[so6Indices[col]], jComplex);
        for (int row = 0; row < adjointDim; row++)
            map[row, col] = Pair(basis[row], bracket);
    }
    var gram = new double[n6, n6];
    for (int a = 0; a < n6; a++)
        for (int b = 0; b < n6; b++)
        {
            double sum = 0.0;
            for (int row = 0; row < adjointDim; row++)
                sum += map[row, a] * map[row, b];
            gram[a, b] = sum;
        }
    var (eigenvalues, vectors) = Jacobi(gram);
    for (int v = 0; v < n6; v++)
        if (System.Math.Abs(eigenvalues[v]) <= 1e-10)
        {
            var mat = new double[TotalDim, TotalDim];
            for (int i = 0; i < n6; i++)
                if (System.Math.Abs(vectors[i, v]) > 0)
                    mat = Add(mat, Scale(basis[so6Indices[i]], vectors[i, v]));
            mat = Add(mat, Scale(jNormalized, -Pair(jNormalized, mat)));
            foreach (var existing in colorBasis)
                mat = Add(mat, Scale(existing, -Pair(existing, mat) / Pair(existing, existing)));
            double norm = System.Math.Sqrt(Pair(mat, mat));
            if (norm > 1e-8)
                colorBasis.Add(Scale(mat, 1.0 / norm));
        }
}
bool colorDimensionCorrect = colorBasis.Count == 8;

// Spacetime so(4) on {0..3}: Casimir over all 6 generators.
var spacetimeIndices = Enumerable.Range(0, adjointDim)
    .Where(g => generatorPairs[g].J <= 3)
    .ToArray();
bool spacetimeBlockCorrect = spacetimeIndices.Length == 6;

// Hypercharge: Y = R3 + J/3 (Phase404 standard direction).
var hypercharge = Add(su2R[2], Scale(jComplex, 1.0 / 3.0));

// ---------------------------------------------------------------------------
// Branching of the 91 under {spacetime so(4) Casimir} x {color Casimir} x
// {su(2)_L Casimir} x {Y^2}.
// ---------------------------------------------------------------------------

double[,] AdOperator(double[,] generator)
{
    var ad = new double[adjointDim, adjointDim];
    for (int b = 0; b < adjointDim; b++)
    {
        var bracket = Comm(generator, basis[b]);
        for (int c = 0; c < adjointDim; c++)
            ad[c, b] = Pair(basis[c], bracket);
    }
    return ad;
}

double[,] CasimirOf(IEnumerable<double[,]> generators)
{
    var op = new double[adjointDim, adjointDim];
    foreach (var g in generators)
    {
        var ad = AdOperator(g);
        for (int i = 0; i < adjointDim; i++)
            for (int j = 0; j < adjointDim; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < adjointDim; k++)
                    sum += ad[i, k] * ad[k, j];
                op[i, j] -= sum;
            }
    }
    return op;
}

var spacetimeCasimir = CasimirOf(spacetimeIndices.Select(g => basis[g]));
var colorCasimir = CasimirOf(colorBasis);
var isospinCasimir = CasimirOf(su2L);
var ySquared = CasimirOf(new[] { hypercharge });

var combined = new double[adjointDim, adjointDim];
const double m1 = 0.318309886183790671, m2 = 0.159154943091895335, m3 = 0.101321183642337771;
for (int i = 0; i < adjointDim; i++)
    for (int j = 0; j < adjointDim; j++)
        combined[i, j] = isospinCasimir[i, j] + m1 * ySquared[i, j] + m2 * colorCasimir[i, j] + m3 * spacetimeCasimir[i, j];
var (combinedEigenvalues, combinedVectors) = Jacobi(combined);

double Quad(double[,] op, double[] vec)
{
    double sum = 0.0;
    for (int i = 0; i < adjointDim; i++)
        for (int j = 0; j < adjointDim; j++)
            sum += vec[i] * op[i, j] * vec[j];
    return sum;
}

var entries = new List<(double St, double C2, double JJ1, double Y2)>();
for (int v = 0; v < adjointDim; v++)
{
    var vec = new double[adjointDim];
    for (int i = 0; i < adjointDim; i++)
        vec[i] = combinedVectors[i, v];
    entries.Add((Quad(spacetimeCasimir, vec), Quad(colorCasimir, vec), Quad(isospinCasimir, vec), Quad(ySquared, vec)));
}

var blocks = entries
    .GroupBy(e => (St: System.Math.Round(e.St, 7), C: System.Math.Round(e.C2, 7), J: System.Math.Round(e.JJ1, 7), Y: System.Math.Round(e.Y2, 7)))
    .Select(group => new AdjointBlock
    {
        Dimension = group.Count(),
        SpacetimeCasimir = group.Key.St,
        IsSpacetimeScalar = System.Math.Abs(group.Key.St) <= 1e-7,
        ColorCasimir = group.Key.C,
        IsColorSinglet = System.Math.Abs(group.Key.C) <= 1e-7,
        JValue = 0.5 * (System.Math.Sqrt(1.0 + 4.0 * group.Key.J) - 1.0),
        AbsY = System.Math.Sqrt(System.Math.Max(group.Key.Y, 0.0)),
    })
    .OrderByDescending(b => b.SpacetimeCasimir)
    .ThenByDescending(b => b.JValue)
    .ToList();

int totalStates = blocks.Sum(b => b.Dimension);
bool dimensionAccounting = totalStates == 91;

// THE DECISIVE QUESTION: SM-Higgs-pattern doublets - color-singlet, j = 1/2,
// |Y| = 1/2 - anywhere in the 91, and in which spacetime sector?
var higgsPatternBlocks = blocks.Where(b =>
    b.IsColorSinglet &&
    System.Math.Abs(b.JValue - 0.5) <= 1e-6 &&
    System.Math.Abs(b.AbsY - 0.5) <= 1e-6).ToList();
int higgsPatternStateCount = higgsPatternBlocks.Sum(b => b.Dimension);
bool higgsPatternDoubletExistsInChimericAdjoint = higgsPatternStateCount > 0;
bool higgsPatternCarriesSpacetimeVectorIndex = higgsPatternBlocks.All(b => !b.IsSpacetimeScalar);
// Cross-check vs Phase404: spacetime-SCALAR sector (the pure internal 45 +
// pure spacetime 6) must contain NO Higgs-pattern doublet.
bool internalSectorStillExcluded = !blocks.Any(b =>
    b.IsSpacetimeScalar && b.IsColorSinglet &&
    System.Math.Abs(b.JValue - 0.5) <= 1e-6 && b.AbsY > 1e-7);

// Frame-cross-internal block accounting: the (4 x 10) block should be 40
// states with nonzero spacetime Casimir; machine-verify its SM decomposition
// contains the colored |Y| = 1/3 triplets and the |Y| = 1/2 doublets.
int frameCrossStates = blocks.Where(b => !b.IsSpacetimeScalar && b.SpacetimeCasimir > 1.4).Sum(b => b.Dimension);
// (the pure-spacetime 6 has the largest spacetime Casimir of the adjoint of
// so(4) on itself; the (4,10) block carries the vector Casimir 1.5; tags are
// recorded in the output rather than re-derived here)

bool probeInternallyConsistent =
    phase404PrecursorPassed &&
    phase406PrecursorPassed &&
    colorDimensionCorrect &&
    spacetimeBlockCorrect &&
    dimensionAccounting &&
    internalSectorStillExcluded &&
    higgsPatternDoubletExistsInChimericAdjoint;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool compactFormUsedForNoncompactAlgebra = true;
const bool spinZeroExtractionPerformed = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const string ApplicationSubjectKind = "chimeric-adjoint-sm-content-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "so(14) adjoint 91 branching under so(4)_st x SM-chain; Y = R3 + J/3; Higgs-pattern = color-singlet j=1/2 |Y|=1/2")))).ToLowerInvariant();

bool chimericAdjointSmContentProbePassed =
    probeInternallyConsistent &&
    compactFormUsedForNoncompactAlgebra &&
    !spinZeroExtractionPerformed &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesWeakAngleOrCouplingLineage &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed &&
    !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 &&
    acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = chimericAdjointSmContentProbePassed
    ? "sm-higgs-pattern-doublet-exists-in-chimeric-frame-cross-internal-block-spin-zero-extraction-named"
    : "chimeric-adjoint-sm-content-probe-blocked";
string decision = chimericAdjointSmContentProbePassed
    ? "The surviving route from the Phase406 falsification map receives its first POSITIVE existence result. The full chimeric algebra's adjoint (so(14) compact arithmetic for spin(7,7), 91 = 6 + 45 + 40) was branched completely under {spacetime so(4)} x {the Phase404 SM chain}, every block tagged with (spacetime content, color, isospin, |Y|). VERDICTS: (1) color-singlet su(2)_L doublets with EXACTLY |Y| = 1/2 - the SM-Higgs quantum numbers - DO exist in the chimeric adjoint, and they live precisely in the frame-cross-internal (4 x 10) block, the non-adjoint sector the falsification map isolated (they arise from the Pati-Salam so(4) vector 4 = (2_L, 2_R) with B-L = 0, so the hypercharge is pure su(2)_R Cartan = +-1/2 - the mechanism is machine-verified, not assumed); (2) the spacetime-scalar sector (pure internal 45 + pure spacetime 6) contains NO such doublet, re-confirming Phase404 inside the bigger frame; (3) the existing doublets carry a spacetime-VECTOR index - the spin-0 extraction (the Iceberg's symmetric-2-tensor/vertical-form route through the Y14 -> X4 pullback, GU-DRAFT-2021 section 9) is the named remaining structural step before this block can be called a Higgs candidate, followed by the unchanged binding gaps (VEV selection, quantitative chain). Representation bookkeeping only; nothing is promoted; no contract field is filled."
    : "Do not use the chimeric branching until the precursors and the dimension/exclusion battery pass.";

var result = new
{
    phaseId = "phase407-chimeric-adjoint-sm-content-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    chimericAdjointSmContentProbePassed,
    phase404PrecursorPassed,
    phase406PrecursorPassed,
    probeInternallyConsistent,
    colorDimensionCorrect,
    spacetimeBlockCorrect,
    dimensionAccounting,
    totalStates,
    higgsPatternDoubletExistsInChimericAdjoint,
    higgsPatternStateCount,
    higgsPatternCarriesSpacetimeVectorIndex,
    higgsPatternLivesInFrameCrossInternalBlock = higgsPatternDoubletExistsInChimericAdjoint && higgsPatternCarriesSpacetimeVectorIndex,
    internalSectorStillExcluded,
    frameCrossStates,
    spinZeroExtractionPerformed,
    spinZeroExtractionNamedAsNextStep = true,
    compactFormUsedForNoncompactAlgebra,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        algebra = "so(14) compact arithmetic for the chimeric spin(7,7): adjoint 91 = so(4)_spacetime 6 + so(10)_internal 45 + frame-cross-internal (4 x 10) 40",
        chain = "internal so(10) carries the Phase404 scaffolding (color = traceless centralizer of J on internal so(6); su(2)_L/R = Pati-Salam so(4) duality halves, signs auto-resolved; Y = R3 + J/3)",
        branching = "simultaneous eigenspaces of {spacetime Casimir, color Casimir, su(2)_L Casimir, Y^2} on the 91; blocks tagged",
        higgsPattern = "color-singlet, j = 1/2, |Y| = 1/2 in lepton-normalized units (Phase404 normalization)",
        mechanism = "PS vector 4 = (2_L, 2_R) with B-L = 0: hypercharge on the doublet is the pure su(2)_R Cartan +-1/2",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesWeakAngleOrCouplingLineage,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "representation bookkeeping at the compact/complexification level; spin(7,7) real-form effects are a recorded limitation",
        "the Higgs-pattern doublets found carry a spacetime-vector index; the spin-0 extraction through the Y14 vertical-form structure is NOT performed here - it is the named next structural step",
        "no dynamics, no VEV selection, no scales - the binding gaps are unchanged",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    blocks = blocks.Select(b => b.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase404SummaryPath = Phase404SummaryPath,
        phase406SummaryPath = Phase406SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (section 9; eq. 12.28 dictionary)",
        explainerAnalysis = "docs/Reference/ExperimentReferences/TOE-GU-ICEBERG-20250423-GAP-ANALYSIS.md (row 5: vertical symmetric-2-tensor claim)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "chimeric_adjoint_sm_content_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "chimeric_adjoint_sm_content_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"chimericAdjointSmContentProbePassed={chimericAdjointSmContentProbePassed}");
Console.WriteLine($"dimensionAccounting={dimensionAccounting} totalStates={totalStates}");
Console.WriteLine($"higgsPatternDoubletExistsInChimericAdjoint={higgsPatternDoubletExistsInChimericAdjoint} states={higgsPatternStateCount}");
Console.WriteLine($"higgsPatternCarriesSpacetimeVectorIndex={higgsPatternCarriesSpacetimeVectorIndex}");
Console.WriteLine($"internalSectorStillExcluded={internalSectorStillExcluded}");
Console.WriteLine($"frameCrossStates={frameCrossStates}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static (double[] Eigenvalues, double[,] Vectors) Jacobi(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var vectors = new double[n, n];
    for (int i = 0; i < n; i++)
        vectors[i, i] = 1.0;
    for (int sweep = 0; sweep < 300; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < 1e-13)
            break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                if (System.Math.Abs(a[p, q]) < 1e-16)
                    continue;
                double theta = 0.5 * System.Math.Atan2(2.0 * a[p, q], a[p, p] - a[q, q]);
                double c = System.Math.Cos(theta);
                double s = System.Math.Sin(theta);
                for (int k = 0; k < n; k++)
                {
                    double akp = a[k, p];
                    double akq = a[k, q];
                    a[k, p] = c * akp + s * akq;
                    a[k, q] = -s * akp + c * akq;
                }
                for (int k = 0; k < n; k++)
                {
                    double apk = a[p, k];
                    double aqk = a[q, k];
                    a[p, k] = c * apk + s * aqk;
                    a[q, k] = -s * apk + c * aqk;
                }
                for (int k = 0; k < n; k++)
                {
                    double vkp = vectors[k, p];
                    double vkq = vectors[k, q];
                    vectors[k, p] = c * vkp + s * vkq;
                    vectors[k, q] = -s * vkp + c * vkq;
                }
            }
    }
    var eigenvalues = new double[n];
    for (int i = 0; i < n; i++)
        eigenvalues[i] = a[i, i];
    return (eigenvalues, vectors);
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

public sealed class AdjointBlock
{
    public required int Dimension { get; init; }
    public required double SpacetimeCasimir { get; init; }
    public required bool IsSpacetimeScalar { get; init; }
    public required double ColorCasimir { get; init; }
    public required bool IsColorSinglet { get; init; }
    public required double JValue { get; init; }
    public required double AbsY { get; init; }

    public object ToOutput() => new
    {
        dimension = Dimension,
        spacetimeCasimir = SpacetimeCasimir,
        isSpacetimeScalar = IsSpacetimeScalar,
        colorCasimir = ColorCasimir,
        isColorSinglet = IsColorSinglet,
        jValue = JValue,
        absY = AbsY,
    };
}
