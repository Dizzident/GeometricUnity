using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase451: TWO-LOOP GAUGE-COUPLING UNIFICATION LEDGER (2026-07-05 task-force
// WS1, referee-certified step 1).
//
// MISSION: predict sin^2 theta_W(m_Z) from the DERIVED 3/8 boundary condition
// (alpha_1 = alpha_2 at the unification point, the blind Phase404 kernel) plus
// the standard renormalization-group running of the SM content row
// (n_f = 3 + one Higgs doublet; Phase433's committed exact-rational one-loop
// b's and the textbook two-loop gauge b_ij). The observed sin^2(m_Z) is NEVER
// imported into the derivation - the 2026-07-05 referee ruled the earlier
// one-loop "gap closure" (sqrt(5/8) -> 0.877 vs 0.881) CIRCULAR precisely
// because it imported the measured sin^2(m_Z) as its IR endpoint. The honest
// prediction uses alpha_em(m_Z) and alpha_s(m_Z) as the two declared IR
// inputs (alpha_em fixes the electroweak normalization; alpha_s is the ONE
// additional low-energy coupling) and solves the unification system for
// {mu*/m_Z, sin^2(m_Z)}. The known one-loop answer is ~0.2076 - the textbook
// ~10% GUT miss - which makes the two-loop ledger a genuinely falsifiable
// test of the derived b-content: hit ~0.23122 within the threshold band or
// the tension is quantified and reported.
//
// DERIVATION vs COMPARISON (Phase429/Phase433 discipline):
//   - DERIVED INPUTS (no measured values): sin^2 = 3/8 at the a1 = a2
//     crossing (Phase404 blind kernel); the one-loop b's recomputed here as
//     exact rationals from the Phase404 family content and cross-checked
//     EXACTLY against Phase433's committed witness row; the textbook SM
//     two-loop gauge b_ij matrix (pure group theory of the same content row,
//     GUT-normalized U(1); every coefficient documented in the JSON).
//   - DECLARED COMPARISON IMPORTS (strictly separated): alpha_em(m_Z) =
//     1/127.955 and alpha_s(m_Z) = 0.1179 with source labels;
//     measuredElectroweakValuesConsulted = true ONLY inside the
//     comparisonImports block. sin^2(m_Z)_observed = 0.23122 appears ONLY as
//     the falsification comparison target (and in the comparison-side
//     alpha_3 non-closure diagnostic) - never in producing the prediction.
//
// CONVENTIONS (Phase433, restated): asymptotic-freedom-positive one-loop b's,
//   d(1/alpha_i)/d ln mu = + b_i^AF/(2 pi)  (one loop),
// so 1/alpha_i grows toward the ultraviolet for b_i^AF > 0. The two-loop
// b_ij matrix is quoted in the STANDARD textbook convention
//   d g_i/d ln mu = b_i^std g_i^3/(16 pi^2)
//                 + (g_i^3/(16 pi^2)^2) sum_j b_ij^std g_j^2,
// with b_i^std = -b_i^AF; in 1/alpha form the coupled system integrated here
// is
//   d(1/alpha_i)/d ln mu = + b_i^AF/(2 pi) - sum_j b_ij^std alpha_j/(8 pi^2).
// GUT normalization g_1^2 = (5/3) g_Y^2 throughout, so at m_Z
//   1/alpha_1 = (3/5)(1 - s2)/alpha_em,  1/alpha_2 = s2/alpha_em,
//   1/alpha_3 = 1/alpha_s,               sin^2(mu) = 3 y2/(5 y1 + 3 y2)
// with y_i = 1/alpha_i (the last identity gives EXACTLY 3/8 when y1 = y2 -
// the derived boundary - which is verified as a battery, not assumed).
//
// FAIL-CLOSED FRAMING (mandatory): even success is GUT-GENERIC - it does not
// distinguish GU until the observerse's intermediate matter content is
// defined. No absolute GeV is emitted (the unification scale is the
// dimensionless ratio mu*/m_Z; an illustrative GeV display exists only inside
// the comparison block, labeled illustrative-with-declared-anchor and gated
// off from promotion). The referee-corrected hierarchy figure
// b*alpha_GUT = 2 pi/ln(M_Pl/m_W) = 0.159 is recorded as context only.
// Nothing is promoted; canFill*/routePromotes* are all false.

const string DefaultOutputDir = "studies/phase451_two_loop_unification_ledger_001/output";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase429SummaryPath = "studies/phase429_target_blind_dimensionless_ratio_ledger_001/output/target_blind_dimensionless_ratio_ledger_summary.json";
const string Phase433SummaryPath = "studies/phase433_blind_beta_coefficient_running_ledger_001/output/blind_beta_coefficient_running_ledger_summary.json";
const string ApplicationSubjectKind = "two-loop-unification-ledger";
const string TerminalPrefix = "two-loop-unification-ledger-";

var outputDir = Environment.GetEnvironmentVariable("PHASE451_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors (fail-closed).
// ---------------------------------------------------------------------------
using var phase404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var phase429 = JsonDocument.Parse(File.ReadAllText(Phase429SummaryPath));
using var phase433 = JsonDocument.Parse(File.ReadAllText(Phase433SummaryPath));

bool phase404PrecursorPassed =
    JsonBool(phase404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true &&
    JsonBool(phase404.RootElement, "familyPatternDerived") is true;
bool phase429PrecursorPassed =
    JsonBool(phase429.RootElement, "targetBlindDimensionlessRatioLedgerPassed") is true &&
    JsonBool(phase429.RootElement, "derivationComparisonSeparationMaintained") is true;
bool phase433PrecursorPassed =
    JsonBool(phase433.RootElement, "blindBetaCoefficientRunningLedgerPassed") is true &&
    JsonBool(phase433.RootElement, "exactRationalArithmeticVerified") is true;
bool precursorsPassed = phase404PrecursorPassed && phase429PrecursorPassed && phase433PrecursorPassed;

// ---------------------------------------------------------------------------
// DERIVED INPUT 1: one-loop b's recomputed as exact rationals from the
// Phase404-derived family content (verbatim Phase433 group theory), then
// cross-checked EXACTLY against Phase433's committed SM witness row.
// AF-positive convention: b = (11/3) C2(G) - (2/3) sum_Weyl T(R)
//                             - (1/3) sum_complex-scalar T(R).
// ---------------------------------------------------------------------------
var family = new[]
{
    //          name  colorDim  su2Dim   Y (rational)
    new WeylMultiplet("Q",   3, 2, new Rational(1, 6)),
    new WeylMultiplet("u^c", 3, 1, new Rational(-2, 3)),
    new WeylMultiplet("d^c", 3, 1, new Rational(1, 3)),
    new WeylMultiplet("L",   1, 2, new Rational(-1, 2)),
    new WeylMultiplet("e^c", 1, 1, new Rational(1, 1)),
    new WeylMultiplet("nu^c",1, 1, new Rational(0, 1)),
};

Rational tHalf = new(1, 2);
Rational gutNorm = new(3, 5);
Rational twoThirds = new(2, 3);
Rational oneThird = new(1, 3);

Rational sumTColorPerFamily = Rational.Zero;
foreach (var m in family)
    if (m.ColorDim == 3)
        sumTColorPerFamily += tHalf * m.Su2Dim;
Rational sumTWeakPerFamily = Rational.Zero;
foreach (var m in family)
    if (m.Su2Dim == 2)
        sumTWeakPerFamily += tHalf * m.ColorDim;
Rational sumYSquaredPerFamily = Rational.Zero;
foreach (var m in family)
    sumYSquaredPerFamily += m.Y * m.Y * (m.ColorDim * m.Su2Dim);
Rational sumTHyperPerFamily = gutNorm * sumYSquaredPerFamily;

const int ImportedObservedFamilyCount = 3; // imported structural parameter (Phase433), not derived
Rational gaugeColor = new Rational(11, 3) * new Rational(3, 1);
Rational gaugeWeak = new Rational(11, 3) * new Rational(2, 1);
Rational higgsDeltaWeak = -(oneThird * tHalf);                              // -1/6
Rational higgsDeltaHyper = -(oneThird * gutNorm * new Rational(1, 2));      // -1/10

// SM content row (n_f = 3 + one Higgs doublet), AF-positive convention.
Rational b3Sm = gaugeColor - twoThirds * sumTColorPerFamily * ImportedObservedFamilyCount;
Rational b2Sm = gaugeWeak - twoThirds * sumTWeakPerFamily * ImportedObservedFamilyCount + higgsDeltaWeak;
Rational b1Sm = -(twoThirds * sumTHyperPerFamily * ImportedObservedFamilyCount) + higgsDeltaHyper;

bool smRowGroupTheoryCorrect =
    b3Sm == new Rational(7, 1) &&
    b2Sm == new Rational(19, 6) &&
    b1Sm == new Rational(-41, 10);

// No-Higgs threshold variant row (Phase433 committed row n_f=3, Higgs=false).
Rational b3NoHiggs = b3Sm;
Rational b2NoHiggs = b2Sm - higgsDeltaWeak;
Rational b1NoHiggs = b1Sm - higgsDeltaHyper;
bool noHiggsRowCorrect =
    b3NoHiggs == new Rational(7, 1) && b2NoHiggs == new Rational(10, 3) && b1NoHiggs == new Rational(-4, 1);

// Top-removed threshold variant row (standard step-function decoupling of the
// top Weyl pair t_L + t_R below the matching point; removing half the Q
// doublet is the usual threshold bookkeeping and breaks SU(2) covariance
// below the step - recorded as a threshold-band convention, not physics).
Rational topDeltaColorSum = tHalf + tHalf;                                   // t_L + t_R color indices
Rational topDeltaWeakSum = tHalf * 3 * new Rational(1, 2);                   // half the color-weighted Q doublet index
Rational topDeltaHyperSum = gutNorm * (new Rational(1, 36) * 3 + new Rational(4, 9) * 3); // (3/5)(3 Y_Q^2 + 3 Y_uc^2) = (3/5)(17/12)
Rational b3Top = b3Sm + twoThirds * topDeltaColorSum;
Rational b2Top = b2Sm + twoThirds * topDeltaWeakSum;
Rational b1Top = b1Sm + twoThirds * topDeltaHyperSum;
bool topRemovedRowCorrect =
    b3Top == new Rational(23, 3) &&   // matches QCD n_f = 5: 11 - (2/3)*5 = 23/3
    b2Top == new Rational(11, 3) &&
    b1Top == new Rational(-53, 15);

// Phase433 committed-witness cross-check: the exact strings in the committed
// summary JSON for the n_f = 3, Higgs = true row.
string? witnessB3 = null, witnessB2 = null, witnessB1 = null;
foreach (var row in phase433.RootElement.GetProperty("betaLedger").EnumerateArray())
{
    if (row.GetProperty("familyCount").GetInt32() == 3 && row.GetProperty("higgsIncluded").GetBoolean())
    {
        witnessB3 = row.GetProperty("b3").GetString();
        witnessB2 = row.GetProperty("b2").GetString();
        witnessB1 = row.GetProperty("b1").GetString();
    }
}
bool phase433WitnessRowMatchesExactly =
    witnessB3 == b3Sm.ToString() && witnessB2 == b2Sm.ToString() && witnessB1 == b1Sm.ToString() &&
    witnessB3 == "7" && witnessB2 == "19/6" && witnessB1 == "-41/10";

// ---------------------------------------------------------------------------
// DERIVED INPUT 2: the standard textbook SM TWO-LOOP gauge b_ij matrix for
// the same content row (n_f = 3 + one Higgs doublet), GUT-normalized U(1),
// STANDARD convention (b_i^std = -b_i^AF; see header). Gauge part only: the
// two-loop Yukawa-trace terms are OMITTED (declared truncation; the dominant
// omitted piece is the top-Yukawa trace, whose effect is far below the
// threshold band recorded here and is listed in the nonclaims).
// Source: the standard Machacek-Vaughn gauge-part evaluation for the SM
// (identical in all textbook RGE compilations).
// ---------------------------------------------------------------------------
var bTwoLoopStd = new Rational[3, 3]
{
    { new(199, 50), new(27, 10), new(44, 5) },
    { new(27, 50),  new(35, 6),  new(12, 1) },
    { new(11, 10),  new(9, 2),   new(-26, 1) },
};
var bTwoLoopDocs = new[]
{
    new { i = 1, j = 1, value = "199/50", note = "U(1)_Y self-coupling two-loop gauge term, GUT-normalized ((3/5) per U(1) index); fermion Y^4-type sums over n_f = 3 families plus the Higgs-doublet scalar contribution; C2(U(1)) = 0 so no pure-gauge part." },
    new { i = 1, j = 2, value = "27/10",  note = "U(1)-SU(2) cross term: doublets carrying hypercharge (Q, L, Higgs), GUT-normalized." },
    new { i = 1, j = 3, value = "44/5",   note = "U(1)-SU(3) cross term: colored fermions weighted by Y^2, GUT-normalized." },
    new { i = 2, j = 1, value = "27/50",  note = "SU(2)-U(1) cross term: same mixed doublet-hypercharge content seen from the SU(2) line, GUT-normalized." },
    new { i = 2, j = 2, value = "35/6",   note = "SU(2) self term: -(136/3) pure Yang-Mills (C2 = 2) + fermion doublets (n_f = 3) + Higgs doublet." },
    new { i = 2, j = 3, value = "12",     note = "SU(2)-SU(3) cross term: the colored doublets Q (3 families)." },
    new { i = 3, j = 1, value = "11/10",  note = "SU(3)-U(1) cross term: colored fermions weighted by Y^2, GUT-normalized." },
    new { i = 3, j = 2, value = "9/2",    note = "SU(3)-SU(2) cross term: the colored doublets Q (3 families)." },
    new { i = 3, j = 3, value = "-26",    note = "SU(3) self term: -(34/3) C2^2 = -102 pure Yang-Mills + 38 n_q/3 fermions (n_q = 6 quark flavors) = -26; equals -beta_1(QCD, n_f = 6) = -(102 - 38*6/3)." },
};
bool twoLoopMatrixChecksPass =
    bTwoLoopStd[2, 2] == new Rational(-26, 1) &&
    -(bTwoLoopStd[2, 2]) == new Rational(102, 1) - new Rational(38, 3) * 6 && // QCD beta_1 witness at n_f = 6
    bTwoLoopStd[0, 0] == new Rational(199, 50) &&
    bTwoLoopStd[1, 1] == new Rational(35, 6) &&
    bTwoLoopStd[0, 1] / bTwoLoopStd[1, 0] == new Rational(5, 1) && // (b12/b21) = 5: GUT-normalization consistency of the mixed U(1)/SU(2) entries
    bTwoLoopStd[0, 2] / bTwoLoopStd[2, 0] == new Rational(8, 1);   // (b13/b31) = 8: GUT-normalization consistency of the mixed U(1)/SU(3) entries

// ---------------------------------------------------------------------------
// DECLARED COMPARISON IMPORTS (Phase429-style strict separation). These are
// the ONLY measured numbers the computation consumes: alpha_em(m_Z) fixes the
// electroweak normalization and alpha_s(m_Z) is the ONE additional low-energy
// coupling. The observed sin^2(m_Z) is declared FURTHER BELOW, strictly after
// the predictions are computed, and is used ONLY in the falsification
// comparison. measuredElectroweakValuesConsulted = true ONLY inside this
// declared-imports block (see JSON).
// ---------------------------------------------------------------------------
const double AlphaEmInverseAtMz = 127.955;   // declared exact input; label: "alpha_em^{-1}(m_Z), MS-bar, PDG-style world average"
const double AlphaSAtMz = 0.1179;            // declared exact input; label: "alpha_s(m_Z), MS-bar, PDG-style world average"

double aEmInv = AlphaEmInverseAtMz;           // A = 1/alpha_em(m_Z)
double aSInv = 1.0 / AlphaSAtMz;              // S = 1/alpha_s(m_Z)

double[] bAfSm = { b1Sm.ToDouble(), b2Sm.ToDouble(), b3Sm.ToDouble() };
double[] bAfNoHiggs = { b1NoHiggs.ToDouble(), b2NoHiggs.ToDouble(), b3NoHiggs.ToDouble() };
double[] bAfTopRemoved = { b1Top.ToDouble(), b2Top.ToDouble(), b3Top.ToDouble() };
var bStd = new double[3, 3];
for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
        bStd[i, j] = bTwoLoopStd[i, j].ToDouble();

const double TwoPi = 2.0 * System.Math.PI;
const double EightPiSq = 8.0 * System.Math.PI * System.Math.PI;
const double StepH = 1.0 / 256.0;        // RK4 step in t = ln(mu/m_Z)
const double CrossingSearchTMax = 80.0;  // fail-closed ceiling on the crossing search
const double S2BracketLo = 0.15;
const double S2BracketHi = 0.32;

// ---------------------------------------------------------------------------
// One-loop CLOSED FORM (the correctness witness). Imposing triple closure
// 1/alpha_1(mu*) = 1/alpha_2(mu*) = 1/alpha_3(mu*) with IR inputs A, S:
//   s2 = (3/5 + k S/A) / (8/5 + k),  k = (b2 - b1)/(b3 - b2)  (AF-positive),
//   t* = 2 pi (s2 A - S)/(b3 - b2)... in t = ln(mu/m_Z) units:
//   t* = 2 pi (s2 A - S)/(b3 - b2), 1/alpha_GUT = s2 A + b2 t*/(2 pi).
// ---------------------------------------------------------------------------
double kClosed = (bAfSm[1] - bAfSm[0]) / (bAfSm[2] - bAfSm[1]);
bool kClosedIsExactRational = System.Math.Abs(kClosed - 218.0 / 115.0) <= 1e-14; // (109/15)/(23/6) = 218/115
double s2OneLoopClosed = (3.0 / 5.0 + kClosed * (aSInv / aEmInv)) / (8.0 / 5.0 + kClosed);
double tStarOneLoopClosed = TwoPi * (s2OneLoopClosed * aEmInv - aSInv) / (bAfSm[2] - bAfSm[1]);
double alphaGutInverseOneLoopClosed = s2OneLoopClosed * aEmInv + bAfSm[1] * tStarOneLoopClosed / TwoPi;

// ---------------------------------------------------------------------------
// Numerical machinery: RK4 on y_i = 1/alpha_i with
//   dy_i/dt = b_i^AF/(2 pi) - [twoLoop] sum_j b_ij^std / (8 pi^2 y_j),
// t = ln(mu/m_Z). Content schedules are piecewise in t (threshold variants);
// integration is segment-aligned so no step straddles a content breakpoint.
// ---------------------------------------------------------------------------
double[] Rhs(double[] y, double[] bAf, bool twoLoop)
{
    var dy = new double[3];
    for (int i = 0; i < 3; i++)
    {
        double v = bAf[i] / TwoPi;
        if (twoLoop)
        {
            double sum = 0.0;
            for (int j = 0; j < 3; j++) sum += bStd[i, j] / y[j];
            v -= sum / EightPiSq;
        }
        dy[i] = v;
    }
    return dy;
}

double[] Rk4Step(double[] y, double h, double[] bAf, bool twoLoop)
{
    var k1 = Rhs(y, bAf, twoLoop);
    var y2 = new double[3];
    for (int i = 0; i < 3; i++) y2[i] = y[i] + 0.5 * h * k1[i];
    var k2 = Rhs(y2, bAf, twoLoop);
    var y3 = new double[3];
    for (int i = 0; i < 3; i++) y3[i] = y[i] + 0.5 * h * k2[i];
    var k3 = Rhs(y3, bAf, twoLoop);
    var y4 = new double[3];
    for (int i = 0; i < 3; i++) y4[i] = y[i] + h * k3[i];
    var k4 = Rhs(y4, bAf, twoLoop);
    var yn = new double[3];
    for (int i = 0; i < 3; i++) yn[i] = y[i] + (h / 6.0) * (k1[i] + 2.0 * k2[i] + 2.0 * k3[i] + k4[i]);
    return yn;
}

// content schedule: (tMin, bAf) segments sorted ascending; content at t is the
// last segment with tMin <= t.
double[] ContentAt((double TMin, double[] BAf)[] schedule, double t)
{
    var b = schedule[0].BAf;
    foreach (var seg in schedule)
        if (seg.TMin <= t + 1e-15) b = seg.BAf;
        else break;
    return b;
}

double[] Integrate(double[] y0, double tStart, double tEnd, double h, bool twoLoop, (double TMin, double[] BAf)[] schedule)
{
    // collect breakpoints strictly inside (tStart, tEnd) along the direction
    var pts = new List<double> { tStart };
    var interior = schedule.Select(s => s.TMin)
        .Where(b => tStart < tEnd ? (b > tStart + 1e-12 && b < tEnd - 1e-12) : (b < tStart - 1e-12 && b > tEnd + 1e-12))
        .ToList();
    interior.Sort();
    if (tStart > tEnd) interior.Reverse();
    pts.AddRange(interior);
    pts.Add(tEnd);
    var y = (double[])y0.Clone();
    for (int s = 0; s + 1 < pts.Count; s++)
    {
        double a = pts[s], b = pts[s + 1];
        double len = b - a;
        if (System.Math.Abs(len) < 1e-15) continue;
        var bAf = ContentAt(schedule, (a + b) / 2.0); // segment midpoint selects content (segment-aligned, so uniform)
        int n = System.Math.Max(1, (int)System.Math.Ceiling(System.Math.Abs(len) / h));
        double hs = len / n;
        for (int kk = 0; kk < n; kk++) y = Rk4Step(y, hs, bAf, twoLoop);
    }
    return y;
}

// Find the a1 = a2 crossing (y1 - y2 = 0) integrating upward from tStart.
(bool Found, double TCross, double[] YCross) FindCrossing(double[] y0, double tStart, double h, bool twoLoop, (double TMin, double[] BAf)[] schedule)
{
    var pts = new List<double> { tStart };
    pts.AddRange(schedule.Select(s => s.TMin).Where(b => b > tStart + 1e-12 && b < CrossingSearchTMax - 1e-12).OrderBy(b => b));
    pts.Add(CrossingSearchTMax);
    var y = (double[])y0.Clone();
    for (int s = 0; s + 1 < pts.Count; s++)
    {
        double a = pts[s], b = pts[s + 1];
        var bAf = ContentAt(schedule, (a + b) / 2.0);
        int n = System.Math.Max(1, (int)System.Math.Ceiling((b - a) / h));
        double hs = (b - a) / n;
        for (int kk = 0; kk < n; kk++)
        {
            var yPrev = y;
            double fPrev = yPrev[0] - yPrev[1];
            y = Rk4Step(y, hs, bAf, twoLoop);
            double fNew = y[0] - y[1];
            if (fPrev > 0.0 && fNew <= 0.0)
            {
                // refine within [0, hs] from yPrev by bisection on a single RK4 step
                double lo = 0.0, hi = hs;
                var yHit = y;
                for (int it = 0; it < 200 && hi - lo > 1e-16 * System.Math.Max(1.0, hs); it++)
                {
                    double mid = 0.5 * (lo + hi);
                    var yMid = Rk4Step(yPrev, mid, bAf, twoLoop);
                    if (yMid[0] - yMid[1] > 0.0) lo = mid; else { hi = mid; yHit = yMid; }
                }
                double tHit = a + kk * hs + hi;
                return (true, tHit, yHit);
            }
        }
    }
    return (false, double.NaN, y);
}

// Boundary values: the declared IR inputs parametrized by the unknown s2 =
// sin^2(m_Z): y1 = (3/5)(1 - s2) A, y2 = s2 A, y3 = S at m_Z. For a shifted
// matching point tb != 0 the values are evolved m_Z -> mu0 at ONE ORDER LOWER
// than the solve (oneLoopEvolveBoundary = true evolves at one loop for the
// two-loop solve; false = tree-level matching for the one-loop solve). The
// system is autonomous, so same-order evolution would be an exact relabel -
// the LOWER-order matching is what exposes the honest truncation ambiguity.
double[] BoundaryY(double s2, double tBoundary, bool oneLoopEvolveBoundary)
{
    var y = new[] { 0.6 * (1.0 - s2) * aEmInv, s2 * aEmInv, aSInv };
    if (oneLoopEvolveBoundary && tBoundary != 0.0)
        for (int i = 0; i < 3; i++) y[i] += bAfSm[i] * tBoundary / TwoPi;
    return y;
}

// Triple-unification residual r(s2) = y3(t*) - y1(t*) at the a1 = a2 crossing.
(bool Ok, double Residual, double TCross, double[] YCross) Residual(double s2, double tBoundary, double h, bool twoLoop, (double TMin, double[] BAf)[] schedule, bool oneLoopEvolveBoundary)
{
    var (found, tc, yc) = FindCrossing(BoundaryY(s2, tBoundary, oneLoopEvolveBoundary), tBoundary, h, twoLoop, schedule);
    return found ? (true, yc[2] - yc[0], tc, yc) : (false, double.NaN, double.NaN, yc);
}

// Solve r(s2) = 0 by bisection over the fixed bracket (fail-closed if the
// bracket does not straddle a sign change).
(bool Converged, double S2, double TStar, double[] YStar, double ResidualAtSolution) SolveS2(double tBoundary, double h, bool twoLoop, (double TMin, double[] BAf)[] schedule, bool oneLoopEvolveBoundary = false)
{
    var rLo = Residual(S2BracketLo, tBoundary, h, twoLoop, schedule, oneLoopEvolveBoundary);
    var rHi = Residual(S2BracketHi, tBoundary, h, twoLoop, schedule, oneLoopEvolveBoundary);
    if (!rLo.Ok || !rHi.Ok || !(rLo.Residual > 0.0 && rHi.Residual < 0.0))
        return (false, double.NaN, double.NaN, new double[3], double.NaN);
    double lo = S2BracketLo, hi = S2BracketHi;
    (bool Ok, double Residual, double TCross, double[] YCross) rMid = rLo;
    for (int it = 0; it < 200 && hi - lo > 1e-13; it++)
    {
        double mid = 0.5 * (lo + hi);
        rMid = Residual(mid, tBoundary, h, twoLoop, schedule, oneLoopEvolveBoundary);
        if (!rMid.Ok) return (false, double.NaN, double.NaN, new double[3], double.NaN);
        if (rMid.Residual > 0.0) lo = mid; else hi = mid;
    }
    double s2Final = 0.5 * (lo + hi);
    var rFinal = Residual(s2Final, tBoundary, h, twoLoop, schedule, oneLoopEvolveBoundary);
    return (rFinal.Ok, s2Final, rFinal.TCross, rFinal.YCross, rFinal.Residual);
}

var smSchedule = new (double TMin, double[] BAf)[] { (-100.0, bAfSm) };

// ---------------------------------------------------------------------------
// COMPUTATION (a): one loop, closed form vs numerical integrator.
// ---------------------------------------------------------------------------
var oneLoopNumeric = SolveS2(0.0, StepH, twoLoop: false, smSchedule);
double sin2MzPredictedOneLoop = s2OneLoopClosed;
double closedVsNumericS2 = System.Math.Abs(oneLoopNumeric.S2 - s2OneLoopClosed);
double closedVsNumericTStar = System.Math.Abs(oneLoopNumeric.TStar - tStarOneLoopClosed);
double closedVsNumericAlphaGutInv = System.Math.Abs(oneLoopNumeric.YStar[0] - alphaGutInverseOneLoopClosed);
bool oneLoopClosedFormBattery =
    oneLoopNumeric.Converged &&
    closedVsNumericS2 <= 1e-10 &&
    closedVsNumericTStar <= 1e-7 &&
    closedVsNumericAlphaGutInv <= 1e-8;

// Referee correctness witness: the honest one-loop prediction is ~0.2076
// (quoted 0.208 at three figures).
bool refereeWitnessBattery =
    System.Math.Abs(s2OneLoopClosed - 0.2076) <= 5e-4 &&
    System.Math.Abs(s2OneLoopClosed - 0.208) <= 1.5e-3;

// ---------------------------------------------------------------------------
// COMPUTATION (b): two loops, numerical (central prediction).
// ---------------------------------------------------------------------------
var twoLoopCentral = SolveS2(0.0, StepH, twoLoop: true, smSchedule);
double sin2MzPredictedTwoLoop = twoLoopCentral.S2;
double tStarTwoLoop = twoLoopCentral.TStar;
double alphaGutInverseTwoLoop = twoLoopCentral.YStar[0];

// Boundary-identity battery: sin^2(mu) = 3 y2/(5 y1 + 3 y2) evaluated at the
// crossing must be EXACTLY the derived 3/8 boundary (both loop orders).
double Sin2FromY(double[] y) => 3.0 * y[1] / (5.0 * y[0] + 3.0 * y[1]);
double boundaryIdentityOneLoopDev = System.Math.Abs(Sin2FromY(oneLoopNumeric.YStar) - 0.375);
double boundaryIdentityTwoLoopDev = System.Math.Abs(Sin2FromY(twoLoopCentral.YStar) - 0.375);
bool boundaryIdentityBattery =
    boundaryIdentityOneLoopDev <= 1e-10 && boundaryIdentityTwoLoopDev <= 1e-10;

// Solver residual battery (triple closure achieved).
bool solverResidualBattery =
    oneLoopNumeric.Converged && twoLoopCentral.Converged &&
    System.Math.Abs(oneLoopNumeric.ResidualAtSolution) <= 1e-9 &&
    System.Math.Abs(twoLoopCentral.ResidualAtSolution) <= 1e-9;

// RK4 step-halving Richardson battery on the couplings (two-loop, fixed
// horizon T = t* central): sup_i |y_i^{h} - y_i^{h/2}| <= 1e-8.
var yRichH = Integrate(BoundaryY(sin2MzPredictedTwoLoop, 0.0, false), 0.0, tStarTwoLoop, StepH, true, smSchedule);
var yRichH2 = Integrate(BoundaryY(sin2MzPredictedTwoLoop, 0.0, false), 0.0, tStarTwoLoop, StepH / 2.0, true, smSchedule);
double richardsonMaxDiff = 0.0;
for (int i = 0; i < 3; i++) richardsonMaxDiff = System.Math.Max(richardsonMaxDiff, System.Math.Abs(yRichH[i] - yRichH2[i]));
// solution-level step-halving: full two-loop solve at h/2
var twoLoopHalfStep = SolveS2(0.0, StepH / 2.0, twoLoop: true, smSchedule);
double richardsonS2Diff = System.Math.Abs(twoLoopHalfStep.S2 - sin2MzPredictedTwoLoop);
bool richardsonBattery = richardsonMaxDiff <= 1e-8 && twoLoopHalfStep.Converged && richardsonS2Diff <= 1e-8;

// Up-down round-trip battery (both directions cross-checked): (i) integrator
// round trip 0 -> t* -> 0; (ii) start EXACTLY unified at t* and run DOWN,
// recovering the imposed IR inputs and the predicted s2.
var yUp = Integrate(BoundaryY(sin2MzPredictedTwoLoop, 0.0, false), 0.0, tStarTwoLoop, StepH, true, smSchedule);
var yRoundTrip = Integrate(yUp, tStarTwoLoop, 0.0, StepH, true, smSchedule);
double roundTripMaxDiff = 0.0;
var yBase = BoundaryY(sin2MzPredictedTwoLoop, 0.0, false);
for (int i = 0; i < 3; i++) roundTripMaxDiff = System.Math.Max(roundTripMaxDiff, System.Math.Abs(yRoundTrip[i] - yBase[i]));
double yGut = twoLoopCentral.YStar[0];
var yDown = Integrate(new[] { yGut, yGut, yGut }, tStarTwoLoop, 0.0, StepH, true, smSchedule);
double downS2Recovered = yDown[1] / aEmInv;
double downRecoveryS2Diff = System.Math.Abs(downS2Recovered - sin2MzPredictedTwoLoop);
double downRecoveryAlphaSDiff = System.Math.Abs(yDown[2] - aSInv);
bool roundTripBattery =
    roundTripMaxDiff <= 1e-9 && downRecoveryS2Diff <= 1e-9 && downRecoveryAlphaSDiff <= 1e-7;
// (the alpha_s leg absorbs the outer-solver residual |r| <= ~1e-10 in 1/alpha units; 1e-7 is the honest gate)

// ---------------------------------------------------------------------------
// THRESHOLD / MATCHING UNCERTAINTY BAND (honest, declared conventions):
//   (i) matching-scale variation: move the matching point to mu0 in
//       [m_Z/2, 2 m_Z], evolving the declared inputs m_Z -> mu0 at one order
//       LOWER than the solve (tree matching for the one-loop solve, one-loop
//       matching for the two-loop solve). The system is autonomous, so
//       same-order evolution is an exact relabel; the lower-order matching
//       exposes the honest truncation ambiguity;
//   (ii) EW-threshold band: decouple the top Weyl pair, and separately the
//       Higgs doublet, below mu_th in (m_Z, 2 m_Z] (step-function
//       thresholds; no particle mass is imported - the factor-2 band is the
//       declared convention). Band = max |s2_variant - s2_central|.
// ---------------------------------------------------------------------------
double ln2 = System.Math.Log(2.0);
var variantSpecs = new List<(string Name, string Kind, double TBoundary, (double TMin, double[] BAf)[] Schedule)>
{
    ("matching-mu0-half-mz", "matching-scale", -ln2, smSchedule),
    ("matching-mu0-0.71-mz", "matching-scale", -0.5 * ln2, smSchedule),
    ("matching-mu0-1.41-mz", "matching-scale", 0.5 * ln2, smSchedule),
    ("matching-mu0-2-mz", "matching-scale", ln2, smSchedule),
    ("top-decoupled-below-1.5-mz", "ew-threshold", 0.0, new (double, double[])[] { (-100.0, bAfTopRemoved), (System.Math.Log(1.5), bAfSm) }),
    ("top-decoupled-below-2-mz", "ew-threshold", 0.0, new (double, double[])[] { (-100.0, bAfTopRemoved), (ln2, bAfSm) }),
    ("higgs-decoupled-below-1.5-mz", "ew-threshold", 0.0, new (double, double[])[] { (-100.0, bAfNoHiggs), (System.Math.Log(1.5), bAfSm) }),
    ("higgs-decoupled-below-2-mz", "ew-threshold", 0.0, new (double, double[])[] { (-100.0, bAfNoHiggs), (ln2, bAfSm) }),
};
var variantResults = new List<VariantResult>();
foreach (var (name, kind, tb, schedule) in variantSpecs)
{
    // tree-level matching for the one-loop solve; one-loop matching for the
    // two-loop solve (each one order lower than the solve, exposing the
    // truncation ambiguity; same-order evolution would be an exact relabel
    // because the system is autonomous).
    var v1 = SolveS2(tb, StepH, twoLoop: false, schedule);
    var v2 = SolveS2(tb, StepH, twoLoop: true, schedule, oneLoopEvolveBoundary: kind == "matching-scale");
    variantResults.Add(new VariantResult(name, kind, v1.Converged && v2.Converged,
        v1.S2, v2.S2, v2.TStar, v1.S2 - oneLoopNumeric.S2, v2.S2 - sin2MzPredictedTwoLoop));
}
bool allVariantsConverged = variantResults.All(v => v.Converged);
double thresholdBandOneLoop = variantResults.Count == 0 ? double.NaN : variantResults.Max(v => System.Math.Abs(v.DeltaS2OneLoop));
double thresholdBandTwoLoop = variantResults.Count == 0 ? double.NaN : variantResults.Max(v => System.Math.Abs(v.DeltaS2TwoLoop));
double thresholdBandTwoLoopQuadrature = System.Math.Sqrt(
    System.Math.Pow(variantResults.Where(v => v.Kind == "matching-scale").Max(v => System.Math.Abs(v.DeltaS2TwoLoop)), 2) +
    System.Math.Pow(variantResults.Where(v => v.Kind == "ew-threshold").Max(v => System.Math.Abs(v.DeltaS2TwoLoop)), 2));

bool batteriesAllPassed =
    smRowGroupTheoryCorrect &&
    noHiggsRowCorrect &&
    topRemovedRowCorrect &&
    phase433WitnessRowMatchesExactly &&
    twoLoopMatrixChecksPass &&
    kClosedIsExactRational &&
    oneLoopClosedFormBattery &&
    refereeWitnessBattery &&
    boundaryIdentityBattery &&
    solverResidualBattery &&
    richardsonBattery &&
    roundTripBattery &&
    allVariantsConverged;

// ---------------------------------------------------------------------------
// COMPARISON SECTION. Everything below this line may consult the observed
// electroweak value. The predictions above are already fixed; the observed
// sin^2 enters ONLY here, as the falsification target and in the
// comparison-side alpha_3 non-closure diagnostic.
// ---------------------------------------------------------------------------
const double Sin2ThetaWObservedMz = 0.23122; // MS-bar sin^2 theta_W(m_Z); FALSIFICATION COMPARISON TARGET ONLY
const double MzGeVDeclaredAnchor = 91.1876;  // illustrative-display anchor ONLY; gated off from promotion

double gapOneLoop = sin2MzPredictedOneLoop - Sin2ThetaWObservedMz;
double gapTwoLoop = sin2MzPredictedTwoLoop - Sin2ThetaWObservedMz;
double gapTwoLoopAbs = System.Math.Abs(gapTwoLoop);
bool twoLoopClosesWithinThresholdBand = gapTwoLoopAbs <= thresholdBandTwoLoop;

// alpha_3-at-unification mismatch (the triple-unification tension expressed
// in 1/alpha units): COMPARISON-SIDE diagnostic - fix sin^2(m_Z) to the
// OBSERVED value, find the a1 = a2 crossing, and record
// 1/alpha_3 - 1/alpha_{1=2} there. (Derivation-side this residual is zero by
// construction; its solver value is recorded above.)
double tCross12ObsOneLoop = TwoPi * (3.0 - 8.0 * Sin2ThetaWObservedMz) * aEmInv / (5.0 * (bAfSm[1] - bAfSm[0]));
double y12ObsOneLoop = Sin2ThetaWObservedMz * aEmInv + bAfSm[1] * tCross12ObsOneLoop / TwoPi;
double y3ObsOneLoop = aSInv + bAfSm[2] * tCross12ObsOneLoop / TwoPi;
double alpha3AtUnificationMismatchOneLoop = y3ObsOneLoop - y12ObsOneLoop;
var obsResidTwoLoop = Residual(Sin2ThetaWObservedMz, 0.0, StepH, twoLoop: true, smSchedule, oneLoopEvolveBoundary: false);
double alpha3AtUnificationMismatchTwoLoop = obsResidTwoLoop.Residual;
var obsResidOneLoopNumeric = Residual(Sin2ThetaWObservedMz, 0.0, StepH, twoLoop: false, smSchedule, oneLoopEvolveBoundary: false);
double alpha3MismatchClosedVsNumeric = System.Math.Abs(obsResidOneLoopNumeric.Residual - alpha3AtUnificationMismatchOneLoop);

string verdictKind = !batteriesAllPassed || !precursorsPassed
    ? "blocked"
    : twoLoopClosesWithinThresholdBand
        ? "two-loop-closes-within-threshold-band"
        : "tension-persists-quantified";

// Illustrative GeV display (comparison-side, gated): ratio times the declared
// m_Z anchor. NEVER promoted; the ledger's scale output is the ratio.
double unificationScaleOverMzOneLoop = System.Math.Exp(oneLoopNumeric.TStar);
double unificationScaleOverMzTwoLoop = System.Math.Exp(tStarTwoLoop);
double illustrativeUnificationScaleGeVOneLoop = unificationScaleOverMzOneLoop * MzGeVDeclaredAnchor;
double illustrativeUnificationScaleGeVTwoLoop = unificationScaleOverMzTwoLoop * MzGeVDeclaredAnchor;

// Referee-corrected hierarchy figure (recorded context ONLY; used nowhere):
// b*alpha_GUT = 2 pi / ln(M_Pl/m_W) = 0.159 (corrects the task-force
// package's "~2", wrong by ~12x).
const double RecordedCorrectedHierarchyFigure = 0.159;
const string RecordedCorrectedHierarchyFormula = "b*alpha_GUT = 2 pi / ln(M_Pl/m_W)";

// ---------------------------------------------------------------------------
// Separation flags and the standard fail-closed block (Phase433/449 pattern).
// ---------------------------------------------------------------------------
const bool sin2ObservedUsedInPredictionComputation = false;
const bool measuredInputsConfinedToDeclaredComparisonImports = true;
const bool derivationComparisonSeparationMaintained = true;
const bool comparisonAgainstObservationPerformed = true; // declared, comparison-side only
const bool successWouldBeGutGenericNotGuSpecific = true; // MANDATORY nonclaim
const bool noAbsoluteGevEmittedOutsideGatedIllustrativeBlock = true;
const bool sourceDefinesUnificationScale = false;
const bool sourceDefinesSymmetryBreakingSector = false;
const bool higgsBreakingSectorEstablished = false;
const bool familyCountIsImportedStructuralParameter = true;

const bool targetBlindConstruction = false; // this phase IS a declared comparison phase (429-style separation, not blindness)
const bool physicalTargetsConsultedForConstruction = false; // construction fixed by the derived boundary + textbook RGEs before any target
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
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string constructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "derived 3/8 boundary (alpha1=alpha2, phase404 kernel); phase433 exact-rational SM row (7, 19/6, -41/10) AF-positive; textbook SM two-loop gauge b_ij GUT-normalized; IR inputs alpha_em(mZ)=1/127.955 + alpha_s(mZ)=0.1179 declared comparison imports; sin2 observed NEVER in the derivation; one-loop closed form + two-loop RK4 both-direction cross-check; matching-scale [mZ/2,2mZ] + EW step-threshold band; dimensionless ratio only, no GeV promotion")))).ToLowerInvariant();

bool twoLoopUnificationLedgerPassed =
    precursorsPassed &&
    batteriesAllPassed &&
    sin2ObservedUsedInPredictionComputation == false &&
    measuredInputsConfinedToDeclaredComparisonImports &&
    derivationComparisonSeparationMaintained &&
    successWouldBeGutGenericNotGuSpecific &&
    noAbsoluteGevEmittedOutsideGatedIllustrativeBlock &&
    !sourceDefinesUnificationScale &&
    !sourceDefinesSymmetryBreakingSector &&
    !higgsBreakingSectorEstablished &&
    familyCountIsImportedStructuralParameter &&
    !physicalTargetsConsultedForConstruction &&
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
    !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = twoLoopUnificationLedgerPassed
    ? TerminalPrefix + verdictKind
    : TerminalPrefix + "blocked";

string decision = twoLoopUnificationLedgerPassed
    ? $"The two-loop unification ledger is decided fail-closed. DERIVED INPUTS: the blind Phase404 kernel boundary sin^2 = 3/8 (alpha_1 = alpha_2 at mu*) and the Phase433 exact-rational SM content row (b3, b2, b1) = (7, 19/6, -41/10) (AF-positive; witness row matched EXACTLY against the committed Phase433 JSON) plus the textbook SM two-loop gauge b_ij (GUT-normalized; QCD beta_1 witness -b_33 = 26 at n_f = 6). DECLARED COMPARISON IMPORTS: alpha_em(m_Z) = 1/{AlphaEmInverseAtMz} and alpha_s(m_Z) = {AlphaSAtMz}; the observed sin^2(m_Z) = {Sin2ThetaWObservedMz} entered ONLY the falsification comparison, never the derivation. PREDICTIONS: sin^2(m_Z) = {sin2MzPredictedOneLoop:F6} at one loop (closed form, reproducing the referee's 0.2076/0.208 witness; numeric agreement {closedVsNumericS2:E1}) and {sin2MzPredictedTwoLoop:F6} at two loops (RK4, step-halving {richardsonMaxDiff:E1} <= 1e-8, up-down round trip {roundTripMaxDiff:E1} <= 1e-9, exact-unified downward recovery {downRecoveryS2Diff:E1}); unification at mu*/m_Z = {unificationScaleOverMzTwoLoop:E3} (dimensionless; no GeV promoted), 1/alpha_GUT = {alphaGutInverseTwoLoop:F3}. FALSIFICATION COMPARISON: predicted-minus-observed = {gapTwoLoop:+0.00000;-0.00000} against the threshold/matching band {thresholdBandTwoLoop:E2} (matching scale in [m_Z/2, 2 m_Z] plus step EW thresholds); the comparison-side triple-unification tension is 1/alpha_3 - 1/alpha_(1=2) = {alpha3AtUnificationMismatchOneLoop:F3} (one loop) / {alpha3AtUnificationMismatchTwoLoop:F3} (two loops) at the observed-sin^2 crossing. VERDICT: {verdictKind}. MANDATORY FRAMING: even a within-band closure would be GUT-GENERIC (this ledger cannot distinguish GU until the observerse's intermediate content is defined); the referee-corrected hierarchy figure b*alpha_GUT = 2 pi/ln(M_Pl/m_W) = {RecordedCorrectedHierarchyFigure} is recorded as context only; two-loop Yukawa traces are omitted (declared truncation); nothing is promoted, no Phase201/Phase256 field is filled."
    : "Do not use the ledger numbers until the precursor, witness, closed-form, Richardson, round-trip, and boundary-identity batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------
var result = new
{
    phaseId = "phase451-two-loop-unification-ledger",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    twoLoopUnificationLedgerPassed,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    constructionHash,
    precursors = new
    {
        phase404PrecursorPassed,
        phase429PrecursorPassed,
        phase433PrecursorPassed,
        phase404SummaryPath = Phase404SummaryPath,
        phase429SummaryPath = Phase429SummaryPath,
        phase433SummaryPath = Phase433SummaryPath,
    },
    conventions = new
    {
        oneLoopSign = "asymptotic-freedom-positive (Phase433): d(1/alpha_i)/d ln mu = + b_i^AF/(2 pi) at one loop",
        twoLoopSystem = "d(1/alpha_i)/d ln mu = + b_i^AF/(2 pi) - sum_j b_ij^std alpha_j/(8 pi^2), with b_i^std = -b_i^AF (standard textbook convention for b_ij)",
        gutNormalization = "g_1^2 = (5/3) g_Y^2; at m_Z: 1/alpha_1 = (3/5)(1-s2)/alpha_em, 1/alpha_2 = s2/alpha_em",
        sinSquaredFromCouplings = "sin^2(mu) = 3 y_2 / (5 y_1 + 3 y_2), y_i = 1/alpha_i; equals 3/8 exactly at y_1 = y_2 (the derived boundary; verified as a battery)",
        runningVariable = "t = ln(mu/m_Z); integrator RK4, fixed step h = 1/256, segment-aligned at content breakpoints",
    },
    derivedInputs = new
    {
        measuredValuesConsultedInDerivedInputs = false,
        boundaryCondition = new
        {
            statement = "sin^2 theta_W = 3/8 at the unification point, equivalent to alpha_1(mu*) = alpha_2(mu*)",
            provenance = "Phase404 blind embedding-chain kernel (tan^2 theta_W = 3/5); Phase429 ledger row",
        },
        oneLoopBetaAfPositive = new
        {
            b3 = b3Sm.ToString(),
            b2 = b2Sm.ToString(),
            b1 = b1Sm.ToString(),
            contentRow = "n_f = 3 (imported structural parameter) + one Higgs doublet (conditional; recorded per Phase433)",
            recomputedFromGroupTheory = smRowGroupTheoryCorrect,
            phase433WitnessRowMatchesExactly,
            phase433WitnessStrings = new { b3 = witnessB3, b2 = witnessB2, b1 = witnessB1 },
        },
        thresholdVariantRows = new
        {
            noHiggs = new { b3 = b3NoHiggs.ToString(), b2 = b2NoHiggs.ToString(), b1 = b1NoHiggs.ToString(), exactCheck = noHiggsRowCorrect },
            topRemoved = new { b3 = b3Top.ToString(), b2 = b2Top.ToString(), b1 = b1Top.ToString(), exactCheck = topRemovedRowCorrect, note = "step-function decoupling of t_L + t_R; removing half the Q doublet is the standard threshold bookkeeping (SU(2)-covariance-breaking below the step is a declared band convention); b_3 = 23/3 matches QCD n_f = 5" },
        },
        twoLoopBijStandardConvention = new
        {
            matrix = new[]
            {
                new[] { bTwoLoopStd[0, 0].ToString(), bTwoLoopStd[0, 1].ToString(), bTwoLoopStd[0, 2].ToString() },
                new[] { bTwoLoopStd[1, 0].ToString(), bTwoLoopStd[1, 1].ToString(), bTwoLoopStd[1, 2].ToString() },
                new[] { bTwoLoopStd[2, 0].ToString(), bTwoLoopStd[2, 1].ToString(), bTwoLoopStd[2, 2].ToString() },
            },
            coefficientDocumentation = bTwoLoopDocs,
            source = "textbook SM two-loop gauge b_ij (Machacek-Vaughn gauge part for n_f = 3 + one Higgs doublet; GUT-normalized U(1)); Yukawa-trace two-loop terms omitted (declared truncation)",
            checks = new
            {
                qcdBeta1WitnessNf6 = (-(bTwoLoopStd[2, 2])).ToString() + " = 102 - 38*6/3",
                mixedU1Su2NormalizationRatio = (bTwoLoopStd[0, 1] / bTwoLoopStd[1, 0]).ToString(),
                mixedU1Su3NormalizationRatio = (bTwoLoopStd[0, 2] / bTwoLoopStd[2, 0]).ToString(),
                allChecksPass = twoLoopMatrixChecksPass,
            },
        },
    },
    comparisonImports = new
    {
        measuredElectroweakValuesConsulted = true,
        declaration = "the ONLY measured numbers consumed by the computation; alpha_em fixes the electroweak normalization, alpha_s is the ONE additional low-energy coupling; sin^2 observed is falsification-target-only",
        alphaEmInverseAtMz = AlphaEmInverseAtMz,
        alphaEmSourceLabel = "alpha_em^{-1}(m_Z), MS-bar, PDG-style world average, declared exact",
        alphaSAtMz = AlphaSAtMz,
        alphaSSourceLabel = "alpha_s(m_Z), MS-bar, PDG-style world average, declared exact",
        sin2ThetaWObservedMz = Sin2ThetaWObservedMz,
        sin2ObservedSourceLabel = "MS-bar sin^2 theta_W(m_Z), PDG-style; FALSIFICATION COMPARISON TARGET ONLY - never used in the derivation",
        sin2ObservedUsedInPredictionComputation,
        mzGeVDeclaredAnchor = MzGeVDeclaredAnchor,
        mzAnchorRole = "illustrative-display-only; gated off from promotion",
    },
    predictions = new
    {
        sin2MzPredictedOneLoop = FiniteOrNull(sin2MzPredictedOneLoop),
        sin2MzPredictedOneLoopClosedForm = "s2 = (3/5 + k S/A)/(8/5 + k), k = (b2-b1)/(b3-b2) = 218/115 exactly",
        sin2MzPredictedTwoLoop = FiniteOrNull(sin2MzPredictedTwoLoop),
        twoLoopMinusOneLoopShift = FiniteOrNull(sin2MzPredictedTwoLoop - sin2MzPredictedOneLoop),
        unificationScaleGeVOverMz = new
        {
            note = "dimensionless ratio mu*/m_Z; NO absolute GeV is emitted by this field despite the requested name",
            oneLoop = FiniteOrNull(unificationScaleOverMzOneLoop),
            twoLoop = FiniteOrNull(unificationScaleOverMzTwoLoop),
            lnOneLoop = FiniteOrNull(oneLoopNumeric.TStar),
            lnTwoLoop = FiniteOrNull(tStarTwoLoop),
        },
        alphaGutInverseOneLoop = FiniteOrNull(alphaGutInverseOneLoopClosed),
        alphaGutInverseTwoLoop = FiniteOrNull(alphaGutInverseTwoLoop),
        solverResidualOneLoop = FiniteOrNull(oneLoopNumeric.ResidualAtSolution),
        solverResidualTwoLoop = FiniteOrNull(twoLoopCentral.ResidualAtSolution),
    },
    thresholdUncertainty = new
    {
        conventions = "matching-scale variation moves the matching point to mu0 in [m_Z/2, 2 m_Z], evolving the declared m_Z inputs to mu0 at one order LOWER than the solve (tree-level matching for the one-loop solve, one-loop matching for the two-loop solve; same-order evolution would be an exact relabel of the autonomous system) - the residual mu0-dependence is the truncation ambiguity; EW-threshold band decouples the top Weyl pair / the Higgs doublet below mu_th in (m_Z, 2 m_Z] by step functions; no particle mass imported",
        variants = variantResults.Select(v => new
        {
            name = v.Name,
            kind = v.Kind,
            converged = v.Converged,
            sin2OneLoop = FiniteOrNull(v.S2OneLoop),
            sin2TwoLoop = FiniteOrNull(v.S2TwoLoop),
            lnUnificationOverMzTwoLoop = FiniteOrNull(v.TStarTwoLoop),
            deltaSin2OneLoop = FiniteOrNull(v.DeltaS2OneLoop),
            deltaSin2TwoLoop = FiniteOrNull(v.DeltaS2TwoLoop),
        }).ToArray(),
        thresholdBandOneLoop = FiniteOrNull(thresholdBandOneLoop),
        thresholdBandTwoLoop = FiniteOrNull(thresholdBandTwoLoop),
        thresholdBandTwoLoopQuadrature = FiniteOrNull(thresholdBandTwoLoopQuadrature),
        bandDefinition = "max over variants of |sin2_variant - sin2_central| (conservative single-source spread); quadrature combination recorded",
        oneLoopMatchingVariantsAreExactRelabels = "the strict one-loop prediction is matching-scale INVARIANT (tree matching of an autonomous system is an exact relabel; the closed form contains no mu0), so the one-loop band contains only the EW-threshold spread - the honest one-loop truncation proxy is the recorded two-loop shift itself",
        omittedEffectsDeclared = new[]
        {
            "two-loop Yukawa-trace terms (dominant omitted piece: top Yukawa)",
            "three-loop running",
            "finite (non-logarithmic) matching corrections at m_Z and at mu*",
            "GUT-scale thresholds (no intermediate observerse content is defined - the same gap that makes success GUT-generic)",
        },
    },
    falsificationComparison = new
    {
        target = Sin2ThetaWObservedMz,
        gapOneLoop = FiniteOrNull(gapOneLoop),
        gapTwoLoop = FiniteOrNull(gapTwoLoop),
        gapTwoLoopAbs = FiniteOrNull(gapTwoLoopAbs),
        thresholdBandTwoLoop = FiniteOrNull(thresholdBandTwoLoop),
        twoLoopClosesWithinThresholdBand,
        gapOverBand = FiniteOrNull(gapTwoLoopAbs / thresholdBandTwoLoop),
        alpha3AtUnificationMismatch = new
        {
            definition = "COMPARISON-SIDE diagnostic: with sin^2(m_Z) fixed to the OBSERVED value, 1/alpha_3 - 1/alpha_(1=2) at the a1 = a2 crossing (1/alpha units); the derivation-side residual is zero by construction (solver residual recorded under predictions)",
            oneLoop = FiniteOrNull(alpha3AtUnificationMismatchOneLoop),
            twoLoop = FiniteOrNull(alpha3AtUnificationMismatchTwoLoop),
            oneLoopClosedVsNumeric = FiniteOrNull(alpha3MismatchClosedVsNumeric),
        },
        illustrativeScaleDisplay = new
        {
            gatedOffFromPromotion = true,
            label = "illustrative-with-declared-anchor (m_Z = 91.1876 GeV declared above); NOT a prediction, NOT promoted",
            illustrativeUnificationScaleGeVOneLoop = FiniteOrNull(illustrativeUnificationScaleGeVOneLoop),
            illustrativeUnificationScaleGeVTwoLoop = FiniteOrNull(illustrativeUnificationScaleGeVTwoLoop),
        },
        recordedCorrectedHierarchyFigure = new
        {
            value = RecordedCorrectedHierarchyFigure,
            formula = RecordedCorrectedHierarchyFormula,
            provenance = "2026-07-05 adversarial referee correction (the task-force package's '~2' was wrong by ~12x)",
            role = "recorded-context-only; used nowhere in this computation",
        },
    },
    batteries = new
    {
        smRowGroupTheoryCorrect,
        noHiggsRowCorrect,
        topRemovedRowCorrect,
        phase433WitnessRowMatchesExactly,
        twoLoopMatrixChecksPass,
        kClosedIsExactRational,
        oneLoopClosedFormBattery,
        closedVsNumericS2 = FiniteOrNull(closedVsNumericS2),
        closedVsNumericTStar = FiniteOrNull(closedVsNumericTStar),
        closedVsNumericAlphaGutInv = FiniteOrNull(closedVsNumericAlphaGutInv),
        refereeWitnessBattery,
        refereeWitnessValue = 0.2076,
        boundaryIdentityBattery,
        boundaryIdentityOneLoopDev = FiniteOrNull(boundaryIdentityOneLoopDev),
        boundaryIdentityTwoLoopDev = FiniteOrNull(boundaryIdentityTwoLoopDev),
        solverResidualBattery,
        richardsonBattery,
        richardsonMaxDiff = FiniteOrNull(richardsonMaxDiff),
        richardsonS2Diff = FiniteOrNull(richardsonS2Diff),
        roundTripBattery,
        roundTripMaxDiff = FiniteOrNull(roundTripMaxDiff),
        downRecoveryS2Diff = FiniteOrNull(downRecoveryS2Diff),
        downRecoveryAlphaSDiff = FiniteOrNull(downRecoveryAlphaSDiff),
        allVariantsConverged,
        batteriesAllPassed,
    },
    separation = new
    {
        sin2ObservedUsedInPredictionComputation,
        measuredInputsConfinedToDeclaredComparisonImports,
        derivationComparisonSeparationMaintained,
        comparisonAgainstObservationPerformed,
        successWouldBeGutGenericNotGuSpecific,
        noAbsoluteGevEmittedOutsideGatedIllustrativeBlock,
        sourceDefinesUnificationScale,
        sourceDefinesSymmetryBreakingSector,
        higgsBreakingSectorEstablished,
        familyCountIsImportedStructuralParameter,
    },
    failClosed = new
    {
        targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        physicalCouplingProvided,
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
        canFillPhase256Contract,
        canFillPhase256ObservedFieldExtractionContract,
    },
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    explicitNonclaims = new[]
    {
        "GUT-GENERIC (mandatory): a within-band closure would confirm generic grand unification of the SM content row, NOT Geometric Unity - nothing here distinguishes GU until the observerse's intermediate matter content is defined.",
        "The observed sin^2(m_Z) never entered the derivation; it appears only as the falsification target and in the comparison-side alpha_3 non-closure diagnostic (the 2026-07-05 referee circularity ruling is binding).",
        "alpha_em(m_Z) and alpha_s(m_Z) are DECLARED measured comparison imports (the electroweak normalization plus exactly ONE additional low-energy coupling); no other measured value is consumed.",
        "No absolute scale is predicted: the unification scale is emitted only as the dimensionless ratio mu*/m_Z; the GeV display is illustrative-with-declared-anchor and gated off from promotion.",
        "n_f = 3 is an imported observed structural parameter and the Higgs doublet is a conditional content assumption (Phase433 provenance rows apply verbatim).",
        "Two-loop Yukawa traces, three-loop running, finite matching terms, and GUT-scale thresholds are omitted and declared; the threshold band covers only the declared matching/EW-step variations.",
        "The referee-corrected hierarchy figure b*alpha_GUT = 2 pi/ln(M_Pl/m_W) = 0.159 is recorded context, used nowhere.",
        "No Phase201 or Phase256 contract field is filled; nothing is promoted.",
    },
    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "two_loop_unification_ledger.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "two_loop_unification_ledger_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"twoLoopUnificationLedgerPassed={twoLoopUnificationLedgerPassed} verdictKind={verdictKind}");
Console.WriteLine($"derived b (AF-positive): b3={b3Sm} b2={b2Sm} b1={b1Sm} | phase433 witness exact match={phase433WitnessRowMatchesExactly}");
Console.WriteLine($"sin2(mZ) predicted: one-loop={sin2MzPredictedOneLoop:F6} (referee witness 0.2076: {refereeWitnessBattery}) two-loop={sin2MzPredictedTwoLoop:F6} (shift {sin2MzPredictedTwoLoop - sin2MzPredictedOneLoop:+0.000000;-0.000000})");
Console.WriteLine($"mu*/mZ: one-loop={unificationScaleOverMzOneLoop:E3} two-loop={unificationScaleOverMzTwoLoop:E3} (ln: {oneLoopNumeric.TStar:F4} / {tStarTwoLoop:F4}); 1/alpha_GUT two-loop={alphaGutInverseTwoLoop:F4}");
Console.WriteLine($"batteries: closedVsNumeric={closedVsNumericS2:E2} richardson={richardsonMaxDiff:E2} (s2 {richardsonS2Diff:E2}) roundTrip={roundTripMaxDiff:E2} downRecovery={downRecoveryS2Diff:E2} boundary3/8 dev={boundaryIdentityTwoLoopDev:E2} allPassed={batteriesAllPassed}");
Console.WriteLine($"threshold band: one-loop={thresholdBandOneLoop:E2} two-loop={thresholdBandTwoLoop:E2} (quadrature {thresholdBandTwoLoopQuadrature:E2})");
Console.WriteLine($"falsification: observed={Sin2ThetaWObservedMz} gapTwoLoop={gapTwoLoop:+0.00000;-0.00000} |gap|/band={gapTwoLoopAbs / thresholdBandTwoLoop:F1} closesWithinBand={twoLoopClosesWithinThresholdBand}");
Console.WriteLine($"alpha3 mismatch at observed-sin2 crossing (1/alpha units): one-loop={alpha3AtUnificationMismatchOneLoop:F3} two-loop={alpha3AtUnificationMismatchTwoLoop:F3}");
Console.WriteLine($"illustrative (gated, declared mZ anchor): mu* ~ {illustrativeUnificationScaleGeVTwoLoop:E3} GeV [illustrative-with-declared-anchor]");
Console.WriteLine($"hierarchy figure recorded: {RecordedCorrectedHierarchyFormula} = {RecordedCorrectedHierarchyFigure} (context only)");
Console.WriteLine($"summaryPath={summaryPath}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static double? FiniteOrNull(double v) => double.IsFinite(v) ? v : null;

public sealed record WeylMultiplet(string Name, int ColorDim, int Su2Dim, Rational Y);

public sealed record VariantResult(
    string Name, string Kind, bool Converged,
    double S2OneLoop, double S2TwoLoop, double TStarTwoLoop,
    double DeltaS2OneLoop, double DeltaS2TwoLoop);

// Exact rational arithmetic over BigInteger; always stored in lowest terms with
// a positive denominator (verbatim Phase433 conventions).
public readonly struct Rational : IEquatable<Rational>
{
    public BigInteger Num { get; }
    public BigInteger Den { get; }

    public static readonly Rational Zero = new(0, 1);

    public Rational(BigInteger num, BigInteger den)
    {
        if (den.IsZero) throw new DivideByZeroException("Rational denominator is zero.");
        if (den.Sign < 0) { num = -num; den = -den; }
        BigInteger g = BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den);
        if (g > BigInteger.One) { num /= g; den /= g; }
        Num = num;
        Den = den;
    }

    public bool IsZero => Num.IsZero;

    public static Rational operator +(Rational a, Rational b) => new(a.Num * b.Den + b.Num * a.Den, a.Den * b.Den);
    public static Rational operator -(Rational a, Rational b) => new(a.Num * b.Den - b.Num * a.Den, a.Den * b.Den);
    public static Rational operator -(Rational a) => new(-a.Num, a.Den);
    public static Rational operator *(Rational a, Rational b) => new(a.Num * b.Num, a.Den * b.Den);
    public static Rational operator *(Rational a, int b) => new(a.Num * b, a.Den);
    public static Rational operator /(Rational a, Rational b) => new(a.Num * b.Den, a.Den * b.Num);

    public static bool operator ==(Rational a, Rational b) => a.Num == b.Num && a.Den == b.Den;
    public static bool operator !=(Rational a, Rational b) => !(a == b);

    public bool Equals(Rational other) => this == other;
    public override bool Equals(object? obj) => obj is Rational r && this == r;
    public override int GetHashCode() => HashCode.Combine(Num, Den);
    public double ToDouble() => (double)Num / (double)Den;
    public override string ToString() => Den == BigInteger.One ? Num.ToString() : $"{Num}/{Den}";
}
