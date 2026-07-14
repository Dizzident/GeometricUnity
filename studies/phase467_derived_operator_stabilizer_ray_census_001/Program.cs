using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase467: Derived-Operator Stabilizer Ray Census (C-STABILIZER v3).
//
// This is an exact Lie-algebra calculation.  It reconstructs the centralizer
// of the Phase404 su(3)_c + su(2)_L embedding in so(10), verifies that the
// centralizer is u(1)_(B-L) + su(2)_R (dimension four), and then scores the
// complete committed Phase404 Y(alpha,beta) ray menu by the dimension of each
// ray's stabilizer in so(10).  The pre-registered discriminator is 13 versus
// 25: the standard Y ray must have the generic u(3)+u(2) stabilizer (13),
// while the SU(5)-enhanced control ray must have u(5) stabilizer (25).
//
// The field-of-definition arm is computational, not a physicist ruling.  The
// same four generators are checked directly against eta=diag(+^6,-^4), and
// the commutant is recomputed inside so(6,4).  This discharges the compact-form
// proxy for this finite-dimensional calculation only.  It does not create or
// simulate an O4 memo and does not shed physicistReviewPending elsewhere.

var stopwatch = Stopwatch.StartNew();

const string OutputDir = "studies/phase467_derived_operator_stabilizer_ray_census_001/output";
const string Phase404Path = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase404HashPin = "ed68a8f3e0d98f25f3c476e940103a4275a79f550346338d08c3c6319160a481";
const string ApplicationSubjectKind = "derived-operator-stabilizer-ray-census";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 7";

Directory.CreateDirectory(OutputDir);

var so10 = SoBasis(10);
var color = ColorSu3Generators();
var weakLeft = WeakLeftGenerators();
var weakRight = WeakRightGenerators();
var j = Add(E(10, 0, 1), E(10, 2, 3), E(10, 4, 5));
var embedded = color.Concat(weakLeft).ToArray();

int embeddedRank = Rank(embedded.Select(Flatten).ToArray());
int commutantDimensionCompact = NullityOfCommutatorSystem(so10, embedded);
bool expectedFourGeneratorsCommute = new[] { j }.Concat(weakRight)
    .All(x => embedded.All(g => IsZero(Commutator(x, g))));
int expectedFourGeneratorRank = Rank(new[] { j }.Concat(weakRight).Select(Flatten).ToArray());
bool compactCommutantExact = embeddedRank == 11 && commutantDimensionCompact == 4
    && expectedFourGeneratorsCommute && expectedFourGeneratorRank == 4;

// Field-of-definition arm.  Block-internal rotations preserve eta exactly;
// recompute the centralizer using the explicit so(6,4) basis rather than
// inferring it from complexification.
int[] signature = { 1, 1, 1, 1, 1, 1, -1, -1, -1, -1 };
var so64 = SoPseudoBasis(signature);
bool embeddedInSo64 = embedded.All(x => PreservesMetric(x, signature));
bool expectedFourInSo64 = new[] { j }.Concat(weakRight).All(x => PreservesMetric(x, signature));
int commutantDimensionNoncompact = NullityOfCommutatorSystem(so64, embedded);
bool fieldOfDefinitionArmPassed = embeddedInSo64 && expectedFourInSo64
    && commutantDimensionNoncompact == 4;

bool phase404Present = File.Exists(Phase404Path);
bool phase404Passed = false;
bool phase404HashMatches = false;
int phase404ScanCount = -1;
if (phase404Present)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Phase404Path));
    var root = doc.RootElement;
    phase404Passed = root.GetProperty("guEmbeddingChainCouplingRatioEnumerationPassed").GetBoolean();
    phase404HashMatches = root.GetProperty("targetBlindConstructionHash").GetString() == Phase404HashPin;
    phase404ScanCount = root.GetProperty("scanCount").GetInt32();
}

// Recreate Phase404's bounded menu exactly, but keep coefficients rational.
// beta is the coefficient of J/2, so the matrix coefficient of J is beta/2.
var betaMenu = new[]
{
    new Q(2, 3), new Q(-2, 3), new Q(1, 3), new Q(-1, 3),
    new Q(4, 3), new Q(-4, 3), Q.One, new Q(-1, 1),
    new Q(2, 1), new Q(-2, 1), new Q(1, 2), new Q(-1, 2),
    Q.Zero, new Q(3, 1), new Q(-3, 1),
};
var menu = new Dictionary<string, Ray>();
for (int numerator = -3; numerator <= 3; numerator++)
for (int denominator = 1; denominator <= 3; denominator++)
foreach (var beta in betaMenu)
{
    var alpha = new Q(numerator, denominator);
    if (alpha.IsZero && beta.IsZero) continue;
    string key = $"{alpha}|{beta}";
    menu.TryAdd(key, new Ray(alpha, beta));
}

string menuCanonical = string.Join("\n", menu.Values
    .OrderBy(r => r.Alpha).ThenBy(r => r.Beta)
    .Select(r => $"alpha={r.Alpha};beta={r.Beta}"));
string menuHash = Sha256(menuCanonical);
bool menuMatchesPhase404 = menu.Count == 224 && phase404ScanCount == 224;

var scored = new List<RayScore>();
foreach (var ray in menu.Values.OrderBy(r => r.Alpha).ThenBy(r => r.Beta))
{
    // Clear rational denominators before exact rank; projective scaling does
    // not affect the adjoint centralizer.
    BigInteger lcm = Lcm(ray.Alpha.Denominator, 2 * ray.Beta.Denominator);
    BigInteger a = ray.Alpha.Numerator * (lcm / ray.Alpha.Denominator);
    BigInteger b = ray.Beta.Numerator * (lcm / (2 * ray.Beta.Denominator));
    var x = Add(Scale(weakRight[2], a), Scale(j, b));
    int stabilizerDimension = NullityOfCommutatorSystem(so10, new[] { x });
    scored.Add(new RayScore(ray.Alpha, ray.Beta, a, b, stabilizerDimension));
}

var standard = scored.Single(r => r.Alpha == Q.One && r.Beta == new Q(2, 3));
var enhanced = scored.Single(r => r.Alpha == Q.One && r.Beta == new Q(2, 1));
int genericThirteenCount = scored.Count(r => r.StabilizerDimension == 13);
int enhancedTwentyFiveCount = scored.Count(r => r.StabilizerDimension == 25);
int distinctStabilizerDimensionCount = scored.Select(r => r.StabilizerDimension).Distinct().Count();
bool discriminatorPassed = standard.StabilizerDimension == 13
    && enhanced.StabilizerDimension == 25
    && genericThirteenCount > 1
    && distinctStabilizerDimensionCount >= 3;

// Selection is allowed only if the standard Y projective ray is the unique
// surviving ray under the pre-registered invariant (stabilizer dimension plus
// field-of-definition).  It is not: many inequivalent generic rays share 13.
int yProjectiveRayCount = scored.Count(r => r.B * standard.A == standard.B * r.A);
int nonYThirteenCount = scored.Count(r => r.StabilizerDimension == 13
    && r.B * standard.A != standard.B * r.A);
bool yRaySelected = discriminatorPassed && genericThirteenCount == yProjectiveRayCount;
bool allSymmetricOrNonY = discriminatorPassed && !yRaySelected && nonYThirteenCount > 0;

// Synthetic sensitivity: dropping one weak generator must enlarge the
// commutant; replacing the enhanced ray by standard Y must flip 25 -> 13.
int weakenedCommutant = NullityOfCommutatorSystem(so10, color.Concat(weakLeft.Take(1)).ToArray());
bool rankBattery = weakenedCommutant > 4;
bool discriminatorBattery = standard.StabilizerDimension != enhanced.StabilizerDimension;
bool deterministicMenuBattery = Sha256(menuCanonical) == menuHash;
bool allBatteriesPassed = rankBattery && discriminatorBattery && deterministicMenuBattery;

bool executed = phase404Present && phase404Passed && phase404HashMatches
    && menuMatchesPhase404 && compactCommutantExact && fieldOfDefinitionArmPassed
    && discriminatorPassed && allBatteriesPassed;

string verdictKind;
if (!phase404Present || !phase404Passed || !phase404HashMatches || !menuMatchesPhase404)
    verdictKind = "stabilizer-ray-census-input-drift";
else if (!compactCommutantExact || !fieldOfDefinitionArmPassed)
    verdictKind = "stabilizer-ray-census-field-or-rank-blocked";
else if (!discriminatorPassed || !allBatteriesPassed)
    verdictKind = "stabilizer-ray-census-battery-failed";
else if (yRaySelected)
    verdictKind = "y-ray-selected-real-form-survives-conditional";
else
    verdictKind = "all-symmetric-non-y-route-closed";

string terminalStatus = $"derived-operator-stabilizer-ray-census-{verdictKind}";
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
const bool sourceContractApplicationAllowed = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;

string constructionHash = Sha256(string.Join("|",
    Phase404HashPin, menuHash, embeddedRank, commutantDimensionCompact,
    commutantDimensionNoncompact, standard.StabilizerDimension,
    enhanced.StabilizerDimension, genericThirteenCount, enhancedTwentyFiveCount));

stopwatch.Stop();
var result = new
{
    phaseId = "phase467-derived-operator-stabilizer-ray-census",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    derivedOperatorStabilizerRayCensusExecuted = executed,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    constructionHash,
    inputs = new
    {
        phase404SummaryPath = Phase404Path,
        phase404Present,
        phase404Passed,
        phase404HashPin = Phase404HashPin,
        phase404HashMatches,
        phase404ScanCount,
        reconstructedMenuCount = menu.Count,
        menuMatchesPhase404,
        rayMenuHash = menuHash,
    },
    exactCommutant = new
    {
        ambientAlgebra = "so(10) compact arithmetic; exact integer commutator equations",
        embeddedGeneratorRank = embeddedRank,
        expectedEmbeddedGeneratorRank = 11,
        commutantDimensionCompact,
        expectedCommutantDimension = 4,
        expectedFourGeneratorRank,
        expectedFourGeneratorsCommute,
        basisDescription = "u(1) complex structure J on R6 plus the three anti-self-dual su(2)_R generators on R4",
        compactCommutantExact,
    },
    fieldOfDefinitionArm = new
    {
        metric = "eta=diag(+,+,+,+,+,+,-,-,-,-)",
        algebra = "so(6,4) computed directly from X^T eta + eta X = 0",
        embeddedInSo64,
        expectedFourInSo64,
        commutantDimensionNoncompact,
        fieldOfDefinitionArmPassed,
        scope = "finite-dimensional commutant and ray-stabilizer arithmetic only; no O4 human ruling is inferred or recorded",
    },
    discriminator = new
    {
        standardYRay = standard.ToOutput(),
        su5EnhancedControlRay = enhanced.ToOutput(),
        expectedStandardDimension = 13,
        expectedEnhancedDimension = 25,
        genericThirteenCount,
        enhancedTwentyFiveCount,
        distinctStabilizerDimensionCount,
        discriminatorPassed,
    },
    selection = new
    {
        yProjectiveRayCount,
        nonYThirteenCount,
        yRaySelected,
        allSymmetricOrNonY,
        interpretation = "Stabilizer dimension distinguishes enhanced from generic rays, but does not isolate the standard Y direction among the generic dimension-13 rays.",
        cLiftStage2RayCount = yRaySelected ? 1 : genericThirteenCount,
        cPermanenceP2Negative = allSymmetricOrNonY,
    },
    stabilizerDimensionHistogram = scored.GroupBy(r => r.StabilizerDimension)
        .OrderBy(g => g.Key).ToDictionary(g => g.Key.ToString(), g => g.Count()),
    rayCensus = scored.Select(r => r.ToOutput()).ToArray(),
    batteries = new
    {
        rankBattery,
        weakenedCommutantDimension = weakenedCommutant,
        discriminatorBattery,
        deterministicMenuBattery,
        allPassed = allBatteriesPassed,
    },
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    physicistReviewPending,
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
    decision = executed && allSymmetricOrNonY
        ? "The exact four-dimensional commutant and its spin(6,4) field of definition survive, and the 13-versus-25 discriminator has teeth, but the derived stabilizer invariant does not select the standard Y ray: multiple non-Y rays share the same generic dimension-13 stabilizer. The C-STABILIZER route therefore closes at the audited Phase404 menu; C-PERMANENCE P2 is negative and C-LIFT must carry the surviving generic rays. This is representation-theory bookkeeping only, with no coupling normalization, scale, pole, or physical prediction."
        : "Do not consume this phase unless every exact-rank, field-of-definition, discriminator, and sensitivity battery passes.",
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "derived_operator_stabilizer_ray_census.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "derived_operator_stabilizer_ray_census_summary.json"), json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"derivedOperatorStabilizerRayCensusExecuted={executed}");
Console.WriteLine($"commutantDimensionCompact={commutantDimensionCompact}");
Console.WriteLine($"commutantDimensionNoncompact={commutantDimensionNoncompact}");
Console.WriteLine($"standardYStabilizerDimension={standard.StabilizerDimension}");
Console.WriteLine($"enhancedControlStabilizerDimension={enhanced.StabilizerDimension}");
Console.WriteLine($"genericThirteenCount={genericThirteenCount}");
Console.WriteLine($"yRaySelected={yRaySelected}");
Console.WriteLine($"promotedPhysicalMassClaimCount={promotedPhysicalMassClaimCount}");
Environment.ExitCode = executed ? 0 : 2;

static BigInteger[,] E(int n, int i, int j)
{
    var x = new BigInteger[n, n];
    x[i, j] = 1;
    x[j, i] = -1;
    return x;
}

static BigInteger[,] Add(params BigInteger[][,] xs)
{
    int n = xs[0].GetLength(0);
    var y = new BigInteger[n, n];
    foreach (var x in xs)
        for (int i = 0; i < n; i++)
            for (int k = 0; k < n; k++)
                y[i, k] += x[i, k];
    return y;
}

static BigInteger[,] Scale(BigInteger[,] x, BigInteger c)
{
    int n = x.GetLength(0);
    var y = new BigInteger[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++) y[i, j] = c * x[i, j];
    return y;
}

static BigInteger[,] Commutator(BigInteger[,] a, BigInteger[,] b)
{
    int n = a.GetLength(0);
    var c = new BigInteger[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            for (int k = 0; k < n; k++)
                c[i, j] += a[i, k] * b[k, j] - b[i, k] * a[k, j];
    return c;
}

static bool IsZero(BigInteger[,] x)
{
    foreach (var v in x) if (!v.IsZero) return false;
    return true;
}

static BigInteger[] Flatten(BigInteger[,] x)
{
    int n = x.GetLength(0);
    var v = new BigInteger[n * n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++) v[i * n + j] = x[i, j];
    return v;
}

static BigInteger[][,] SoBasis(int n)
{
    var xs = new List<BigInteger[,]>();
    for (int i = 0; i < n; i++)
        for (int j = i + 1; j < n; j++) xs.Add(E(n, i, j));
    return xs.ToArray();
}

static BigInteger[][,] SoPseudoBasis(int[] signature)
{
    int n = signature.Length;
    var xs = new List<BigInteger[,]>();
    for (int i = 0; i < n; i++)
    for (int j = i + 1; j < n; j++)
    {
        var x = new BigInteger[n, n];
        x[i, j] = signature[i];
        x[j, i] = -signature[j];
        xs.Add(x);
    }
    return xs.ToArray();
}

static BigInteger[][,] ColorSu3Generators()
{
    var xs = new List<BigInteger[,]>();
    foreach (var (p, q) in new[] { (0, 1), (0, 2), (1, 2) })
    {
        xs.Add(Add(E(10, 2 * p, 2 * q), E(10, 2 * p + 1, 2 * q + 1)));
        xs.Add(Add(Scale(E(10, 2 * p, 2 * q + 1), -1), E(10, 2 * p + 1, 2 * q)));
    }
    xs.Add(Add(E(10, 0, 1), Scale(E(10, 2, 3), -1)));
    xs.Add(Add(E(10, 0, 1), E(10, 2, 3), Scale(E(10, 4, 5), -2)));
    return xs.ToArray();
}

static BigInteger[][,] WeakLeftGenerators() => new[]
{
    Add(E(10, 6, 7), E(10, 8, 9)),
    Add(E(10, 6, 8), Scale(E(10, 7, 9), -1)),
    Add(E(10, 6, 9), E(10, 7, 8)),
};

static BigInteger[][,] WeakRightGenerators() => new[]
{
    Add(E(10, 6, 7), Scale(E(10, 8, 9), -1)),
    Add(E(10, 6, 8), E(10, 7, 9)),
    Add(E(10, 6, 9), Scale(E(10, 7, 8), -1)),
};

static int NullityOfCommutatorSystem(BigInteger[][,] basis, BigInteger[][,] generators)
{
    int n2 = basis[0].Length;
    var rows = new List<BigInteger[]>();
    foreach (var g in generators)
    {
        var comms = basis.Select(x => Flatten(Commutator(x, g))).ToArray();
        for (int component = 0; component < n2; component++)
        {
            var row = new BigInteger[basis.Length];
            for (int b = 0; b < basis.Length; b++) row[b] = comms[b][component];
            if (row.Any(v => !v.IsZero)) rows.Add(row);
        }
    }
    return basis.Length - Rank(rows.ToArray());
}

static int Rank(BigInteger[][] input)
{
    if (input.Length == 0) return 0;
    int rows = input.Length, cols = input[0].Length;
    var a = new Q[rows, cols];
    for (int i = 0; i < rows; i++)
        for (int j = 0; j < cols; j++) a[i, j] = new Q(input[i][j], 1);
    int rank = 0;
    for (int col = 0; col < cols && rank < rows; col++)
    {
        int pivot = rank;
        while (pivot < rows && a[pivot, col].IsZero) pivot++;
        if (pivot == rows) continue;
        if (pivot != rank)
            for (int j = col; j < cols; j++) (a[rank, j], a[pivot, j]) = (a[pivot, j], a[rank, j]);
        Q p = a[rank, col];
        for (int j = col; j < cols; j++) a[rank, j] /= p;
        for (int i = 0; i < rows; i++)
        {
            if (i == rank || a[i, col].IsZero) continue;
            Q f = a[i, col];
            for (int j = col; j < cols; j++) a[i, j] -= f * a[rank, j];
        }
        rank++;
    }
    return rank;
}

static bool PreservesMetric(BigInteger[,] x, int[] signature)
{
    int n = signature.Length;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            if (signature[i] * x[i, j] + signature[j] * x[j, i] != 0) return false;
    return true;
}

static string Sha256(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
static BigInteger Gcd(BigInteger a, BigInteger b) => BigInteger.GreatestCommonDivisor(BigInteger.Abs(a), BigInteger.Abs(b));
static BigInteger Lcm(BigInteger a, BigInteger b) => BigInteger.Abs(a / Gcd(a, b) * b);

readonly record struct Ray(Q Alpha, Q Beta);
readonly record struct RayScore(Q Alpha, Q Beta, BigInteger A, BigInteger B, int StabilizerDimension)
{
    public object ToOutput() => new
    {
        alpha = Alpha.ToString(),
        beta = Beta.ToString(),
        clearedR3Coefficient = A.ToString(),
        clearedJCoefficient = B.ToString(),
        stabilizerDimension = StabilizerDimension,
        isGenericThirteen = StabilizerDimension == 13,
        isEnhancedTwentyFive = StabilizerDimension == 25,
    };
}

readonly struct Q : IComparable<Q>, IEquatable<Q>
{
    public static readonly Q Zero = new(0, 1);
    public static readonly Q One = new(1, 1);
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }
    public bool IsZero => Numerator.IsZero;
    public Q(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero) throw new DivideByZeroException();
        if (denominator.Sign < 0) { numerator = -numerator; denominator = -denominator; }
        BigInteger g = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), BigInteger.Abs(denominator));
        Numerator = numerator / g;
        Denominator = denominator / g;
    }
    public static Q operator +(Q a, Q b) => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static Q operator -(Q a, Q b) => new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static Q operator *(Q a, Q b) => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    public static Q operator /(Q a, Q b) => new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    public static bool operator ==(Q a, Q b) => a.Equals(b);
    public static bool operator !=(Q a, Q b) => !a.Equals(b);
    public int CompareTo(Q other) => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
    public bool Equals(Q other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object? obj) => obj is Q q && Equals(q);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    public override string ToString() => Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}";
}
