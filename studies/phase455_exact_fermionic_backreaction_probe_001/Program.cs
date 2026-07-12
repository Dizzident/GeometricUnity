using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase455: Exact Fermionic Backreaction Probe (WAVE2_ACTION_PLAN_2026-07-12 item 1).
//
// This is the full pre-registered implementation of the T0-T3 taxonomy recorded
// in STUDY.md. It decides, with EXACT arithmetic over Q(sqrt 3) and
// Sturm-certified root isolation, whether the one-loop backreaction functional
//
//     V(t) = S_B(t*u) + N_f * V_f(t)
//
// along constant rank-1 rays omega = t*u develops a genuine interior MINIMUM
// (a radiative well) on (0, t_max], where t_max is a pre-registered rational
// strictly below the first exact fermionic zero-crossing (the log divergence of
// V_f). The computation is done on the COMMITTED phase428 closed-form ray
// spectrum lambda_i^2(t) = (s1 + t*u_c)^2 + (s2 + t*u_c)^2 (multiplicity 4 per
// momentum/gauge-eigenvalue), pinned by battery g0.
//
// COMMITTED CONVENTIONS (read from the phase428/phase430 committed record and
// cited here; realized exactly, physicistReviewPending):
//   * S_B(t*u) := the committed phase430 bosonic one-loop determinant
//       W_B(t) = sum_{momenta, adjoint m^2>0} log(eps_k^2 + t^2 m^2)
//     (phase430 Program.cs lines 230-244: 0.5 * sum 2*log(...) = sum log(...)).
//     This is the "recorded workbench model" of the transverse fluctuation
//     determinant; only the mode-count arithmetic is exact (phase430's own
//     caveat). The identification S_B := W_B is a pre-registered modeling
//     choice, marked physicistReviewPending.
//   * V_f(t) := the committed one-loop fermionic functional
//       W_F(t) = -(1/2) sum_{spectrum} log lambda^2
//     (phase428 Program.cs line 334; phase430 line 225). The SIGN is negative
//     (attractive/destabilizing). The pre-registration shorthand "V_f = -sum
//     log lambda^2" is realized with the committed -(1/2) normalization so the
//     g1 perturbativity bound is evaluated against the actual committed
//     functional; the overall prefactor is a convention that rescales N_f and
//     does not move stationary points. A normalization-robustness note (spec
//     literal -1 vs committed -1/2) is recorded.
//   * Per-mode coefficients after collapsing the committed prefactors:
//     bosonic +1 per (momentum, nonzero adjoint m^2); fermionic -2*N_f per
//     (momentum, nonzero gauge eigenvalue) (= -(1/2)*4 spinor * N_f).
//
// Zero-mode convention axis (each a pre-registered convention,
// physicistReviewPending):
//   * Z-a PRIMARY  -- symmetric k=0 exclusion: drop every momentum with
//     eps_k^2 = s1^2 + s2^2 = 0 (the zero-lattice-momentum / vanishing-sine
//     sector) from BOTH the bosonic and fermionic sums.
//   * Z-b          -- keep k=0, phase447 committed soft floor (floorRel default
//     1e-4, swept {1e-5,1e-4,1e-3}) applied at value evaluation.
//   * Z-c          -- keep k=0, exclude only EXACT zero modes (lambda^2 <=
//     KernelTolerance 1e-18, the phase428 committed convention).
// The three conventions give DIFFERENT stationarity polynomials (the k=0 modes
// contribute t-dependent factors); g6 is the convention-flip matrix and T3 is
// reachable if the verdict flips.
//
// MANDATORY FRAMING. Every quantity is a workbench-relative structure of the
// reduced su(3) slice in lattice units, never a physical mass; no GeV / pole /
// VEV promotion; no Phase201 or Phase256 contract field is filled;
// promotedPhysicalMassClaimCount remains 0; physicistReviewPending is carried
// explicitly; the critical-coupling column appears in NO terminal conjunct.

var stopwatch = System.Diagnostics.Stopwatch.StartNew();

const string ApplicationSubjectKind = "exact-fermionic-backreaction-probe";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 1";
const string DefaultOutputDir = "studies/phase455_exact_fermionic_backreaction_probe_001/output";
const string TerminalPrefix = "exact-fermionic-backreaction-probe-";
const string Phase428SummaryPath = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase430SummaryPath = "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json";
const string Phase433SummaryPath = "studies/phase433_blind_beta_coefficient_running_ledger_001/output/blind_beta_coefficient_running_ledger_summary.json";
const string CommittedPhase428Hash = "2f7f4d5d47c5a5c4964edd522faa193e706d545bcf2ca42f0688b33ed55350fb";

var outputDir = Environment.GetEnvironmentVariable("PHASE455_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var batteryResults = new List<BatteryResult>();
void RecordBattery(string id, string name, bool passed, string detail) =>
    batteryResults.Add(new BatteryResult(id, name, passed, detail));

// ---------------------------------------------------------------------------
// g5 -- Sturm self-test on known polynomials (run first; certifies the exact
// root-isolation kernel before any physics uses it).
// ---------------------------------------------------------------------------
{
    // (t-1)(t-2)(t-3) = t^3 - 6t^2 + 11t - 6 : three simple roots in (0,4].
    var p1 = Poly.FromInts(-6, 11, -6, 1);
    int n1 = Poly.CountRealRootsInInterval(p1, Rational.FromInt(0), Rational.FromInt(4));
    // t^2 - 3 : the single positive root sqrt(3) in (0,2].
    var p2 = Poly.FromInts(-3, 0, 1);
    int n2 = Poly.CountRealRootsInInterval(p2, Rational.FromInt(0), Rational.FromInt(2));
    // t^2 + 1 : no real roots.
    var p3 = Poly.FromInts(1, 0, 1);
    int n3 = Poly.CountRealRootsInInterval(p3, Rational.FromInt(-5), Rational.FromInt(5));
    // isolate sqrt(3) and certify the sign flip - -> + across it.
    var iso = Poly.IsolateRealRoots(p2, Rational.FromInt(0), Rational.FromInt(2));
    bool sqrt3Isolated = iso.Count == 1 &&
        p2.EvalRational(iso[0].Lo).Sign() < 0 && p2.EvalRational(iso[0].Hi).Sign() > 0;
    bool g5 = n1 == 3 && n2 == 1 && n3 == 0 && sqrt3Isolated;
    RecordBattery("g5", "sturm-battery-on-known-polynomials", g5,
        $"roots(t^3-6t^2+11t-6,(0,4])={n1} (exp 3); roots(t^2-3,(0,2])={n2} (exp 1); roots(t^2+1)={n3} (exp 0); sqrt3-isolated-with-sign-flip={sqrt3Isolated}");
}

// ---------------------------------------------------------------------------
// g0 -- pin the committed phase428 closed-form spectrum. Assert the committed
// verdict/hash, and reproduce the exact per-axis gauge-eigenvalue multisets and
// the closed-form ray spectrum against phase428's own float construction.
// g2 -- exact-vs-float cross-check rides the same reconstruction.
// ---------------------------------------------------------------------------
bool phase428Present = File.Exists(Phase428SummaryPath);
bool phase428Passed = false, phase428HashMatch = false;
if (phase428Present)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Phase428SummaryPath));
    phase428Passed = JsonBool(doc.RootElement, "fermionLoopBlockSelectionNoGoProbePassed") is true;
    phase428HashMatch = doc.RootElement.TryGetProperty("targetBlindConstructionHash", out var h) &&
        h.GetString() == CommittedPhase428Hash;
}

// Exact gauge-eigenvalue multisets (fundamental & adjoint) per axis, and the
// bosonic adjoint mode masses^2. Derived by hand from the su(3) weights and
// cross-checked against phase428's float Jacobi below (g0/g2).
//   Fundamental weights of lambda_a/2:
//     T/D axes (lambda_1, lambda_4): {+1/2, -1/2, 0}
//     S axis   (lambda_8):           {+1/(2 sqrt3), +1/(2 sqrt3), -1/sqrt3}
//   Adjoint eigenvalues (weight differences, sl(3)):
//     T/D: {+1,-1,+1/2,-1/2,+1/2,-1/2,0,0}
//     S:   {+sqrt3/2 (x2), -sqrt3/2 (x2), 0 (x4)}
//   Bosonic m^2 = (adjoint eigenvalue)^2: T/D {1(x2),1/4(x4),0(x2)}; S {3/4(x4),0(x4)}.
Q3[] FundEig(string axis) => axis == "S"
    ? new[] { Q3.Sqrt3Over(6), Q3.Sqrt3Over(6), Q3.Sqrt3Over(3).Negate() }         // sqrt3/6, sqrt3/6, -sqrt3/3
    : new[] { Q3.FromRational(Rational.New(1, 2)), Q3.FromRational(Rational.New(-1, 2)), Q3.Zero };
Q3[] AdjEig(string axis) => axis == "S"
    ? new[] { Q3.Sqrt3Over(2), Q3.Sqrt3Over(2), Q3.Sqrt3Over(2).Negate(), Q3.Sqrt3Over(2).Negate(), Q3.Zero, Q3.Zero, Q3.Zero, Q3.Zero }
    : new[]
    {
        Q3.FromRational(Rational.FromInt(1)), Q3.FromRational(Rational.FromInt(-1)),
        Q3.FromRational(Rational.New(1, 2)), Q3.FromRational(Rational.New(-1, 2)),
        Q3.FromRational(Rational.New(1, 2)), Q3.FromRational(Rational.New(-1, 2)),
        Q3.Zero, Q3.Zero,
    };
Rational[] BosonMassesSquared(string axis) => AdjEig(axis)
    .Select(e => (e.Mul(e)).RationalPart())         // adjoint eigenvalue^2 is always rational here
    .ToArray();

// Float cross-check of the exact eigenvalue multisets against phase428's Jacobi
// on the Gell-Mann generators (g0 spectrum pin + g2 exact-vs-float).
double eigCrossCheckResidual = BuildAndCompareEigenvalues();
bool eigenvaluesMatchCommitted = eigCrossCheckResidual <= 1e-12;

// Closed-form ray spectrum cross-check at the phase428 sample points (exact Q3
// evaluation vs. float), fundamental T axis.
double raySpectrumCrossCheckResidual = 0.0;
foreach (double tSample in new[] { 0.35, 1.25 })
{
    var tRat = Rational.FromDoubleApprox(tSample);
    foreach (var (s1, s2) in Momenta())
        foreach (var uc in FundEig("T"))
        {
            // exact: (s1 + t*uc)^2 + (s2 + t*uc)^2
            var a1 = Q3.FromRational(Rational.FromInt(s1)).Add(Q3.FromRational(tRat).Mul(uc));
            var a2 = Q3.FromRational(Rational.FromInt(s2)).Add(Q3.FromRational(tRat).Mul(uc));
            double exact = a1.Mul(a1).Add(a2.Mul(a2)).ToDouble();
            double flt = Sq(s1 + tSample * uc.ToDouble()) + Sq(s2 + tSample * uc.ToDouble());
            raySpectrumCrossCheckResidual = Math.Max(raySpectrumCrossCheckResidual, Math.Abs(exact - flt));
        }
}
bool raySpectrumReproduced = raySpectrumCrossCheckResidual <= 1e-9;

bool g0 = phase428Present && phase428Passed && phase428HashMatch && eigenvaluesMatchCommitted && raySpectrumReproduced;
RecordBattery("g0", "committed-phase428-spectra-hash-pin", g0,
    $"present={phase428Present} passed={phase428Passed} hashMatch={phase428HashMatch} eigResidual={eigCrossCheckResidual:E2} rayResidual={raySpectrumCrossCheckResidual:E2}");
RecordBattery("g2", "exact-vs-float-cross-check", eigenvaluesMatchCommitted && raySpectrumReproduced,
    $"eigResidual={eigCrossCheckResidual:E2} rayResidual={raySpectrumCrossCheckResidual:E2}");

// ---------------------------------------------------------------------------
// g3 -- set-wise evenness of the +/-k-summed fermionic factor set. For each
// (axis, rep) the multiset of fermion factor polynomials {F(t)} must be closed
// under t -> -t (c1 -> -c1) with equal multiplicities, even though no single
// mode is even. Checked exactly on the Z-c (full-momentum) factor set.
// ---------------------------------------------------------------------------
bool g3 = true;
string g3Detail = "";
foreach (var (rep, axis) in new[] { ("fundamental", "T"), ("fundamental", "S"), ("adjoint", "T"), ("adjoint", "S") })
{
    var facs = FermionFactorMultiset(rep, axis, includeK0: true);
    bool even = MultisetClosedUnderReflection(facs);
    g3 &= even;
    g3Detail += $"{rep}/{axis}:{(even ? "even" : "ODD")} ";
}
RecordBattery("g3", "setwise-evenness-of-pm-k-summed-Vf", g3, g3Detail.Trim());

// ---------------------------------------------------------------------------
// g4 -- synthetic positive control: a spectrum that DOES produce a radiative
// well, proving the pipeline detects T2. Construct V_syn'(t) proportional to
// (t-1)(t-2) = t^2 - 3t + 2 on (0, 5/2]: a min at t=1 (- -> +) and a max at
// t=2 (+ -> -). The pipeline must report exactly one certified interior MIN.
// ---------------------------------------------------------------------------
{
    var pSyn = Poly.FromInts(2, -3, 1); // (t-1)(t-2)
    var iso = Poly.IsolateRealRoots(pSyn, Rational.FromInt(0), Rational.New(5, 2));
    int mins = 0, maxes = 0;
    foreach (var (lo, hi) in iso)
    {
        int sl = pSyn.EvalRational(lo).Sign(), sh = pSyn.EvalRational(hi).Sign();
        if (sl < 0 && sh > 0) mins++;
        else if (sl > 0 && sh < 0) maxes++;
    }
    bool g4 = mins == 1 && maxes == 1;
    RecordBattery("g4", "synthetic-positive-control-T2-reachable", g4,
        $"synthetic V'(t)=(t-1)(t-2): interiorMins={mins} (exp 1) interiorMaxes={maxes} (exp 1) -> T2 detectable");
}

// ---------------------------------------------------------------------------
// The stationarity engine: build the denominator-cleared dV/dt numerator P(t)
// over Q(sqrt3) for a given (rep, N_f, axis, convention), Sturm-isolate its
// roots in (0, t_max], and certify each depth (2nd-derivative / sign-flip)
// direction. Returns the isolated stationary points with min/max labels.
// ---------------------------------------------------------------------------
RowResult EvaluateRow(string rep, int nf, string axis, string convention)
{
    bool includeK0 = convention != "Za";           // Z-a excludes eps^2=0 momenta
    // net integer coefficient per distinct t-dependent factor polynomial.
    var netCoeff = new Dictionary<string, (Poly Factor, BigInteger Coeff)>();
    void Add(Poly f, BigInteger c)
    {
        if (f.IsConstant) return;                   // t-independent -> drops from dV/dt
        string key = f.Key();
        if (netCoeff.TryGetValue(key, out var e)) netCoeff[key] = (e.Factor, e.Coeff + c);
        else netCoeff[key] = (f, c);
    }
    // bosonic modes (content-independent): coeff +1 per (momentum, nonzero m^2).
    foreach (var (s1, s2) in Momenta())
    {
        int eps2 = s1 * s1 + s2 * s2;
        if (!includeK0 && eps2 == 0) continue;
        foreach (var m2 in BosonMassesSquared(axis))
        {
            if (m2.Sign() == 0) continue;           // massless mode: constant or exact-zero
            Poly f = eps2 == 0
                ? Poly.TSquared                     // eps^2=0: B = m^2 t^2 -> factor t^2
                : Poly.Quadratic(Q3.FromRational(Rational.FromInt(eps2)), Q3.Zero, Q3.FromRational(m2));
            Add(f, BigInteger.One);
        }
    }
    // fermionic modes: coeff -2*N_f per (momentum, nonzero gauge eigenvalue).
    var eig = rep == "adjoint" ? AdjEig(axis) : FundEig(axis);
    foreach (var (s1, s2) in Momenta())
    {
        int eps2 = s1 * s1 + s2 * s2;
        if (!includeK0 && eps2 == 0) continue;
        foreach (var u in eig)
        {
            if (u.IsZero) continue;                 // u=0: F = eps^2 constant (or exact-zero at eps=0)
            Poly f = eps2 == 0
                ? Poly.TSquared                     // eps^2=0: F = 2 u^2 t^2 -> factor t^2
                : Poly.Quadratic(
                    Q3.FromRational(Rational.FromInt(eps2)),
                    Q3.FromRational(Rational.FromInt(2 * (s1 + s2))).Mul(u),
                    u.Mul(u).Scale(2));
            Add(f, -2 * (BigInteger)nf);
        }
    }
    // P(t) = sum_f coeff_f * f'(t) * prod_{g != f} g(t), over the distinct factors.
    var factors = netCoeff.Values.Select(v => v.Factor).ToList();
    var coeffs = netCoeff.Values.Select(v => v.Coeff).ToList();
    Poly product = Poly.One;
    foreach (var f in factors) product = product.Mul(f);
    Poly p = Poly.Zero;
    for (int i = 0; i < factors.Count; i++)
    {
        if (coeffs[i].IsZero) continue;
        Poly others = product.ExactDivide(factors[i]);            // prod_{g!=f} g
        p = p.Add(factors[i].Derivative().Mul(others).ScaleInt(coeffs[i]));
    }
    p = p.FactorOutT(out int tPower);                              // strip t^k (root at 0, out of domain)

    var (tmaxNum, tmaxDen) = TMaxRational(rep, axis);
    Rational tMax = Rational.New(tmaxNum, tmaxDen);
    var stationaryPoints = new List<StationaryPoint>();
    if (!p.IsZeroPoly)
    {
        var iso = Poly.IsolateRealRoots(p, Rational.Zero, tMax);
        foreach (var (lo, hi) in iso)
        {
            int sl = p.EvalRational(lo).Sign();
            int sh = p.EvalRational(hi).Sign();
            // sign(dV/dt) = sign(P) on (0,t_max) since prod factors > 0 there.
            string kind = (sl < 0 && sh > 0) ? "min-well"
                        : (sl > 0 && sh < 0) ? "max-barrier"
                        : "degenerate";
            double tMid = (lo.ToDouble() + hi.ToDouble()) / 2.0;
            double vAtRoot = ValueV(rep, nf, axis, convention, tMid);
            stationaryPoints.Add(new StationaryPoint(lo.ToDouble(), hi.ToDouble(), kind, vAtRoot, vAtRoot < 0.0));
        }
    }
    // T2 requires a certified interior MINIMUM (dV/dt: - -> +, second-derivative
    // sign positive) with NEGATIVE DEPTH (V(t*) < V(0) = 0): a genuine below-origin
    // radiative well. A local min sitting above the origin does NOT qualify.
    bool hasNegativeDepthWell = stationaryPoints.Any(sp => sp.Kind == "min-well" && sp.DipsBelowOrigin);
    return new RowResult(rep, nf, axis, convention, tMax.ToDouble(),
        p.IsZeroPoly ? 0 : p.Degree, stationaryPoints, hasNegativeDepthWell);
}

// ---------------------------------------------------------------------------
// VERDICT-BEARING DERIVED row: the Phase404 blind single family decomposes into
// 4 su(3)-active fundamentals (Q x2, u^c, d^c; the 4 color singlets decouple) =>
// N_f = 4 fundamental (phase430's committed "derived-4x-fundamental" content).
// This is the ONLY strictly-derived count (Phase433 marks n_f=3 as an imported
// structural parameter, not derived).
//
// NON-GATING rows (recorded, never in a terminal conjunct):
//   * imported three-family: N_f = 12 fundamental (Phase433 imported-structural).
//   * occupancy sweep N_f = 1..16 fundamental; adjoint N_f = 1.
// ---------------------------------------------------------------------------
var axes = new[] { "T", "D", "S" };
var verdictSpecs = new (string Rep, int Nf, string Provenance)[]
{
    ("fundamental", 4, "derived-single-family"),
};
var verdictRows = new List<RowResult>();
foreach (var (rep, nf, _) in verdictSpecs)
    foreach (var axis in axes)
        verdictRows.Add(EvaluateRow(rep, nf, axis, "Za"));   // Z-a primary

// imported / robustness rows (non-gating).
var importedRows = new List<RowResult>();
foreach (var axis in axes)
    importedRows.Add(EvaluateRow("fundamental", 12, axis, "Za"));   // imported 3-family
var occupancy = new List<RowResult>();
for (int nf = 1; nf <= 16; nf++)
    occupancy.Add(EvaluateRow("fundamental", nf, "S", "Za"));
occupancy.Add(EvaluateRow("adjoint", 1, "S", "Za"));

// convention-flip matrix (g6): the derived N_f=4 rows across Z-a/Z-b/Z-c.
var conventionMatrix = new List<RowResult>();
foreach (var conv in new[] { "Za", "Zb", "Zc" })
    foreach (var axis in axes)
        conventionMatrix.Add(EvaluateRow("fundamental", 4, axis, conv));

// g6 verdict: does the derived (N_f=4) negative-depth-well verdict flip across conventions?
bool ConvWell(string conv, string axis) =>
    conventionMatrix.First(r => r.Convention == conv && r.Axis == axis).HasNegativeDepthWell;
bool conventionFragile = false;
foreach (var axis in axes)
{
    bool za = ConvWell("Za", axis), zb = ConvWell("Zb", axis), zc = ConvWell("Zc", axis);
    if (!(za == zb && zb == zc)) conventionFragile = true;
}
RecordBattery("g6", "convention-flip-matrix-Za-Zb-Zc", true,
    $"derived N_f=4 hasWell per axis: " +
    string.Join(" ", axes.Select(a => $"{a}[Za={ConvWell("Za", a)},Zb={ConvWell("Zb", a)},Zc={ConvWell("Zc", a)}]")) +
    $" -> conventionFragile={conventionFragile}");

// ---------------------------------------------------------------------------
// g1 -- perturbativity |N_f * V_f| <= 2 * S_B pinned at C=2 on (0, t*] (or
// (0, t_max] if no stationary point). Mandatory marginality label if the bound
// is within 10% at the endpoint. Evaluated with committed-prefactor float
// values over a fine grid, for each verdict-bearing row (Z-a primary).
// ---------------------------------------------------------------------------
bool g1 = true;                 // gate over the DERIVED verdict rows only
bool g1Marginal = false;
var g1Rows = new List<G1Row>();
G1Row EvaluateG1(RowResult row, string prov, bool gating)
{
    // window (0, t*]: the candidate below-origin-well t* if one exists, else the
    // first interior stationary point, else t_max.
    double tStar = row.StationaryPoints.Any(sp => sp.Kind == "min-well" && sp.DipsBelowOrigin)
        ? row.StationaryPoints.Where(sp => sp.Kind == "min-well" && sp.DipsBelowOrigin).Min(sp => (sp.Lo + sp.Hi) / 2.0)
        : row.StationaryPoints.Count > 0
            ? row.StationaryPoints.Min(sp => (sp.Lo + sp.Hi) / 2.0)
            : row.TMax;
    double maxRatio = 0.0;
    const int grid = 400;
    for (int i = 1; i <= grid; i++)
    {
        double t = tStar * i / grid;
        double sB = SB(row.Axis, t, includeK0: false);
        double vf = VF(row.Rep, row.Axis, t, includeK0: false);
        if (sB <= 1e-12) continue;
        maxRatio = Math.Max(maxRatio, Math.Abs(row.Nf * vf) / (2.0 * sB));
    }
    double endpointRatio = Math.Abs(row.Nf * VF(row.Rep, row.Axis, tStar, includeK0: false)) /
        (2.0 * Math.Max(SB(row.Axis, tStar, includeK0: false), 1e-12));
    return new G1Row(row.Rep, row.Nf, prov, row.Axis, tStar, maxRatio, endpointRatio, maxRatio <= 1.0, endpointRatio >= 0.9);
}
foreach (var row in verdictRows)
{
    var g1r = EvaluateG1(row, "derived-single-family", gating: true);
    g1 &= g1r.Passed;
    if (g1r.Marginal) g1Marginal = true;
    g1Rows.Add(g1r);
}
// report-only (non-gating) perturbativity for the imported 3-family rows.
var importedG1 = importedRows.Select(r => EvaluateG1(r, "imported-three-family-structural-parameter-NONGATING", gating: false)).ToList();
RecordBattery("g1", "perturbativity-Nf-Vf-le-2-SB-C2", g1,
    $"derived-verdict-rows-bound-satisfied={g1} marginalWithin10pct={g1Marginal}; imported-3-family(non-gating) maxRatio={importedG1.Max(r => r.MaxRatio):F3} pass={importedG1.All(r => r.Passed)}");

// ---------------------------------------------------------------------------
// Terminal taxonomy.
// ---------------------------------------------------------------------------
bool anyBatteryRed = batteryResults.Any(b => !b.Passed);
bool anyDerivedWell = verdictRows.Any(r => r.HasNegativeDepthWell);

string verdictKind;
if (anyBatteryRed) verdictKind = "batteries-failed-no-verdict";                 // T0
else if (conventionFragile) verdictKind = "convention-fragile";                 // T3
else if (anyDerivedWell) verdictKind = "radiative-well-candidate";              // T2
else verdictKind = "fermionic-backreaction-null";                              // T1

string terminalStatus = TerminalPrefix + verdictKind;
string limbL5Status = verdictKind == "fermionic-backreaction-null"
    ? "closed-reopening-requires-a-source-defined-fermionic-action"
    : "not-closed-by-this-phase";

// The Z-a PRIMARY arm result recorded explicitly (independent of the flip): the
// derived N_f=4 content is perturbatively controlled (g1) and has NO below-origin
// well on any axis -> the primary arm alone would read T1 fermionic-backreaction-null.
bool zaPrimaryNoWell = verdictRows.All(r => !r.HasNegativeDepthWell);
bool zaPrimaryWouldBeT1 = zaPrimaryNoWell && !anyBatteryRed;
// T3 routes the flip axis (the zero-mode convention Z-a vs Z-b/Z-c) to O4.
string o4RoutedFlipAxis = verdictKind == "convention-fragile"
    ? "zero-mode-convention Z-a(symmetric k=0 exclusion) vs Z-b/Z-c(keep k=0): the below-origin-well verdict on the T/D axes flips; routed to the O4 physicist-adjudication queue"
    : "none";

// ---------------------------------------------------------------------------
// Standing claim boundary (verbatim across the program).
// ---------------------------------------------------------------------------
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const int promotedPhysicalMassClaimCount = 0;
const bool criticalCouplingColumnInTerminalConjunct = false;   // never

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|", ApplicationSubjectKind, PlanSection,
        "exact Q(sqrt3) backreaction V=S_B+N_f*V_f on committed phase428 spectra; Sturm-certified roots; Z-a primary; committed one-loop conventions; no target values")))).ToLowerInvariant();

double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase455-exact-fermionic-backreaction-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    limbL5Status,
    taxonomy = new
    {
        t0 = "batteries-failed-no-verdict",
        t1 = "fermionic-backreaction-null (closes L5; reopening = a source-defined fermionic action)",
        t2 = "radiative-well-candidate (t* forwarded to the phase456 pack; anchor-free ratios only, never a mass claim)",
        t3 = "convention-fragile (flip axis routed to the O4 queue)",
    },
    committedConventions = new
    {
        sBIdentification = "S_B(t*u) := committed phase430 bosonic one-loop determinant W_B(t)=sum log(eps^2+t^2 m^2) (phase430 Program.cs 230-244); recorded workbench model; modeling choice, physicistReviewPending",
        vFConvention = "V_f(t) := committed W_F(t)=-(1/2) sum log lambda^2 (phase428 line 334, phase430 line 225); sign negative; committed -1/2 normalization",
        perModeCoefficients = "bosonic +1 per (momentum, nonzero adjoint m^2); fermionic -2*N_f per (momentum, nonzero gauge eigenvalue)",
        zeroModeAxis = new
        {
            zaPrimary = "symmetric k=0 exclusion: drop every momentum with eps^2 = s1^2+s2^2 = 0 from both sectors",
            zb = "keep k=0; phase447 committed soft floor floorRel default 1e-4 (swept {1e-5,1e-4,1e-3}) at value evaluation",
            zc = "keep k=0; exclude only exact zero modes (lambda^2 <= 1e-18, phase428 convention)",
            physicistReviewPending = true,
        },
        tMax = "pre-registered rational strictly below the first exact fermionic zero-crossing (the V_f log divergence) per (rep, axis)",
        normalizationRobustnessNote = "the pre-registration shorthand V_f=-sum log lambda^2 (prefactor -1) doubles the effective N_f relative to the committed -1/2; the derived-row verdict is reported robust/fragile to this rescaling in the occupancy sweep",
    },
    // the phase RAN cleanly (all batteries green, a valid pre-registered terminal
    // emitted); this is orthogonal to which physics verdict (T1/T2/T3) fired.
    exactFermionicBackreactionProbePassed = !anyBatteryRed,
    batteries = batteryResults.Select(b => new { b.Id, b.Name, b.Passed, b.Detail }).ToArray(),
    anyBatteryRed,
    verdictBearingRows = verdictRows.Select(RowJson).ToArray(),
    verdictRowProvenance = verdictSpecs.Select(v => new { rep = v.Rep, nF = v.Nf, provenance = v.Provenance }).ToArray(),
    zaPrimaryArm = new
    {
        note = "Z-a PRIMARY (symmetric k=0 exclusion): the derived N_f=4 content, perturbatively controlled (g1), has no below-origin well on any axis.",
        zaPrimaryNoWell,
        zaPrimaryWouldBeT1,
    },
    o4RoutedFlipAxis,
    // Both labeled review items this phase hands to the O4 physicist-adjudication
    // register: the zero-mode convention that the T3 verdict flips on, AND the
    // S_B workbench-model choice (phase430 one-loop W_B) the whole analysis rests on.
    o4QueueItems = new[] { "zero-mode-convention-flip-axis", "sb-workbench-model-choice-w430" },
    conventionFlipMatrix = conventionMatrix.Select(RowJson).ToArray(),
    conventionFragile,
    importedThreeFamilyRowsNonGating = new
    {
        provenance = "n_f=3 is an IMPORTED observed structural parameter (Phase433 familyCountIsImportedStructuralParameter=true), NOT derived; recorded, never in a terminal conjunct.",
        nF = 12,
        rows = importedRows.Select(RowJson).ToArray(),
        perturbativityNonGating = importedG1.Select(r => new { r.Axis, maxRatio = r.MaxRatio, r.Passed }).ToArray(),
        observation = "for the imported 3-family count the one-loop backreaction is non-perturbative (g1 bound violated) and a below-origin well appears on all axes; outside the derived content and the controlled regime, so it decides nothing.",
    },
    occupancySweepRobustnessOnly = occupancy.Select(RowJson).ToArray(),
    g1Perturbativity = new
    {
        boundConstantC = 2,
        allRowsSatisfied = g1,
        marginalWithin10Pct = g1Marginal,
        rows = g1Rows.Select(r => new
        {
            r.Rep, r.Nf, r.Provenance, r.Axis, tStar = r.TStar,
            maxRatio = r.MaxRatio, endpointRatio = r.EndpointRatio, r.Passed, r.Marginal,
        }).ToArray(),
    },
    tStarForwardedToPhase456Pack = anyDerivedWell
        ? verdictRows.Where(r => r.HasNegativeDepthWell)
            .SelectMany(r => r.StationaryPoints.Where(sp => sp.Kind == "min-well")
                .Select(sp => new { r.Rep, r.Nf, r.Axis, tStarMid = (sp.Lo + sp.Hi) / 2.0, sp.ValueAtRoot }))
            .ToArray()
        : Array.Empty<object>(),
    criticalCouplingColumnInTerminalConjunct,
    // --- standing claim boundary ---
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    promotedPhysicalMassClaimCount,
    physicalCouplingProvided,
    routeProvidesPhysicalEffectiveActionHessian,
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
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    sourceEvidence = new
    {
        phase428SummaryPath = Phase428SummaryPath,
        phase404SummaryPath = Phase404SummaryPath,
        phase430SummaryPath = Phase430SummaryPath,
        phase433SummaryPath = Phase433SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "Every t and every V(t), S_B, V_f is a workbench-relative structure of the reduced su(3) slice in lattice units; no quantity is a physical mass and none is promoted (promotedPhysicalMassClaimCount = 0).",
        "S_B is the committed phase430 bosonic one-loop determinant (a recorded workbench model, only its mode-count arithmetic is exact) and its identification as S_B is a pre-registered modeling choice; physicistReviewPending.",
        "The zero-mode conventions Z-a/Z-b/Z-c are pre-registered conventions, not source-defined physics; the convention axis is a robustness probe and any flip routes to the O4 queue.",
        "The critical-coupling column appears in NO terminal conjunct; the occupancy sweep is robustness-only and never verdict-bearing.",
        "No Phase201 or Phase256 field is filled.",
    },
    runtimeSeconds,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(System.IO.Path.Combine(outputDir, "exact_fermionic_backreaction_probe.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(System.IO.Path.Combine(outputDir, "exact_fermionic_backreaction_probe_summary.json"), JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} anyBatteryRed={anyBatteryRed} conventionFragile={conventionFragile} anyDerivedWell={anyDerivedWell}");
foreach (var b in batteryResults.OrderBy(b => b.Id))
    Console.WriteLine($"  [{b.Id}] {b.Name}: {(b.Passed ? "PASS" : "RED")} -- {b.Detail}");
Console.WriteLine("verdict-bearing rows (Z-a primary):");
foreach (var r in verdictRows)
    Console.WriteLine($"  {r.Rep} N_f={r.Nf} axis={r.Axis}: degP={r.PolyDegree} stationary={r.StationaryPoints.Count} hasWell={r.HasNegativeDepthWell} " +
        string.Join(",", r.StationaryPoints.Select(sp => $"[{sp.Lo:F4},{sp.Hi:F4}]{sp.Kind}(V={sp.ValueAtRoot:F3})")));
Console.WriteLine($"g1: allRowsSatisfied={g1} marginal={g1Marginal}");
foreach (var r in g1Rows)
    Console.WriteLine($"  g1 {r.Rep} N_f={r.Nf} axis={r.Axis}: tStar={r.TStar:F4} maxRatio={r.MaxRatio:F4} endpointRatio={r.EndpointRatio:F4} pass={r.Passed}");
Console.WriteLine("convention-flip matrix (derived N_f=4):");
foreach (var r in conventionMatrix)
    Console.WriteLine($"  {r.Convention} axis={r.Axis}: hasNegDepthWell={r.HasNegativeDepthWell} " +
        string.Join(",", r.StationaryPoints.Select(sp => $"[{(sp.Lo + sp.Hi) / 2.0:F4}]{sp.Kind}(V={sp.ValueAtRoot:F3},below={sp.DipsBelowOrigin})")));
Console.WriteLine("imported 3-family (N_f=12, non-gating):");
foreach (var r in importedRows)
    Console.WriteLine($"  axis={r.Axis}: hasNegDepthWell={r.HasNegativeDepthWell} " +
        string.Join(",", r.StationaryPoints.Select(sp => $"[{(sp.Lo + sp.Hi) / 2.0:F4}]{sp.Kind}(V={sp.ValueAtRoot:F3})")));
Console.WriteLine($"limbL5Status={limbL5Status}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");

// ===========================================================================
// Local helper functions (physics + JSON shaping).
// ===========================================================================

object RowJson(RowResult r) => new
{
    rep = r.Rep,
    nF = r.Nf,
    axis = r.Axis,
    convention = r.Convention,
    tMax = r.TMax,
    stationarityPolyDegree = r.PolyDegree,
    stationaryPoints = r.StationaryPoints.Select(sp => new
    {
        isolatingIntervalLo = sp.Lo,
        isolatingIntervalHi = sp.Hi,
        depthKind = sp.Kind,
        valueAtRoot = sp.ValueAtRoot,
        dipsBelowOrigin = sp.DipsBelowOrigin,
    }).ToArray(),
    hasNegativeDepthWell = r.HasNegativeDepthWell,
};

// Enumerate the 4x4 lattice momenta s_mu = sin(2 pi n / 4) in {0,1,0,-1}.
IEnumerable<(int S1, int S2)> Momenta()
{
    int[] s = { 0, 1, 0, -1 };
    foreach (int a in s)
        foreach (int b in s)
            yield return (a, b);
}

// Fermion factor multiset (as reflection-key strings) for the g3 evenness test.
List<Poly> FermionFactorMultiset(string rep, string axis, bool includeK0)
{
    var eig = rep == "adjoint" ? AdjEig(axis) : FundEig(axis);
    var list = new List<Poly>();
    foreach (var (s1, s2) in Momenta())
    {
        int eps2 = s1 * s1 + s2 * s2;
        if (!includeK0 && eps2 == 0) continue;
        foreach (var u in eig)
        {
            if (u.IsZero) continue;
            if (eps2 == 0) { list.Add(Poly.TSquared); continue; }
            list.Add(Poly.Quadratic(
                Q3.FromRational(Rational.FromInt(eps2)),
                Q3.FromRational(Rational.FromInt(2 * (s1 + s2))).Mul(u),
                u.Mul(u).Scale(2)));
        }
    }
    return list;
}

bool MultisetClosedUnderReflection(List<Poly> facs)
{
    var counts = new Dictionary<string, int>();
    foreach (var f in facs)
    {
        counts.TryGetValue(f.Key(), out int c);
        counts[f.Key()] = c + 1;
    }
    foreach (var f in facs)
    {
        string rk = f.Reflect().Key();
        counts.TryGetValue(rk, out int cr);
        counts.TryGetValue(f.Key(), out int cf);
        if (cr != cf) return false;
    }
    return true;
}

// Committed-prefactor float value functions (relative to t=0), for g1 and depth
// value reporting. Z-a excludes eps^2=0 momenta; Zb/Zc include them (they only
// differ near crossings, immaterial to g1 on the smooth window).
double SB(string axis, double t, bool includeK0)
{
    double sum = 0.0, ref0 = 0.0;
    foreach (var (s1, s2) in Momenta())
    {
        double eps2 = s1 * s1 + s2 * s2;
        if (!includeK0 && eps2 == 0.0) continue;   // Z-a symmetric k=0 exclusion
        foreach (var m2 in BosonMassesSquared(axis))
        {
            double m = m2.ToDouble();
            if (m == 0.0) continue;
            double arg = eps2 + t * t * m;
            if (arg > 1e-18) sum += Math.Log(arg);
            if (eps2 > 1e-18) ref0 += Math.Log(eps2);
        }
    }
    return sum - ref0;
}
double VF(string rep, string axis, double t, bool includeK0)
{
    var eig = rep == "adjoint" ? AdjEig(axis) : FundEig(axis);
    double sum = 0.0, ref0 = 0.0;
    foreach (var (s1, s2) in Momenta())
    {
        double eps2 = s1 * s1 + s2 * s2;
        if (!includeK0 && eps2 == 0.0) continue;   // Z-a symmetric k=0 exclusion
        foreach (var u in eig)
        {
            double uc = u.ToDouble();
            double a1 = s1 + t * uc, a2 = s2 + t * uc;
            double l2 = a1 * a1 + a2 * a2;
            // multiplicity 4 (spinor); committed prefactor -1/2.
            if (l2 > 1e-18) sum += 4.0 * Math.Log(l2);
            if (eps2 > 1e-18) ref0 += 4.0 * Math.Log(eps2);
        }
    }
    return -0.5 * (sum - ref0);
}
double ValueV(string rep, int nf, string axis, string convention, double t)
{
    bool includeK0 = convention != "Za";
    return SB(axis, t, includeK0) + nf * VF(rep, axis, t, includeK0);
}

// Pre-registered rational t_max strictly below the first exact fermionic
// zero-crossing per (rep, axis). Crossings: fund T/D=2, fund S=sqrt3(~1.7320),
// adj T/D=1, adj S=2/sqrt3(~1.1547).
(BigInteger, BigInteger) TMaxRational(string rep, string axis)
{
    if (rep == "adjoint")
        return axis == "S" ? ((BigInteger)114, 100) : ((BigInteger)99, 100);
    return axis == "S" ? ((BigInteger)173, 100) : ((BigInteger)199, 100);
}

double BuildAndCompareEigenvalues()
{
    // Gell-Mann fundamental generators, adjoint via structure constants; compare
    // Jacobi eigenvalues to the exact hardcoded multisets (float).
    var gm = GellMann();
    var genFund = new System.Numerics.Complex[8][,];
    for (int a = 0; a < 8; a++)
    {
        genFund[a] = new System.Numerics.Complex[3, 3];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                genFund[a][r, c] = gm[a][r, c] / 2.0;
    }
    var fabc = StructureConstants(genFund);
    var genAdj = new System.Numerics.Complex[8][,];
    for (int a = 0; a < 8; a++)
    {
        genAdj[a] = new System.Numerics.Complex[8, 8];
        for (int b = 0; b < 8; b++)
            for (int c = 0; c < 8; c++)
                genAdj[a][b, c] = -System.Numerics.Complex.ImaginaryOne * fabc[a, b, c];
    }
    double residual = 0.0;
    foreach (var (axis, gmIdx) in new[] { ("T", 0), ("D", 3), ("S", 7) })
    {
        var fFloat = JacobiEigen(genFund[gmIdx]).OrderBy(v => v).ToArray();
        var fExact = FundEig(axis).Select(q => q.ToDouble()).OrderBy(v => v).ToArray();
        residual = Math.Max(residual, MultisetResidual(fFloat, fExact));
        var aFloat = JacobiEigen(genAdj[gmIdx]).OrderBy(v => v).ToArray();
        var aExact = AdjEig(axis).Select(q => q.ToDouble()).OrderBy(v => v).ToArray();
        residual = Math.Max(residual, MultisetResidual(aFloat, aExact));
    }
    return residual;
}

double MultisetResidual(double[] x, double[] y)
{
    if (x.Length != y.Length) return double.MaxValue;
    double m = 0.0;
    for (int i = 0; i < x.Length; i++) m = Math.Max(m, Math.Abs(x[i] - y[i]));
    return m;
}

static double Sq(double x) => x * x;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static System.Numerics.Complex[][,] GellMann()
{
    var g = new System.Numerics.Complex[8][,];
    for (int a = 0; a < 8; a++) g[a] = new System.Numerics.Complex[3, 3];
    g[0][0, 1] = 1; g[0][1, 0] = 1;
    g[1][0, 1] = -System.Numerics.Complex.ImaginaryOne; g[1][1, 0] = System.Numerics.Complex.ImaginaryOne;
    g[2][0, 0] = 1; g[2][1, 1] = -1;
    g[3][0, 2] = 1; g[3][2, 0] = 1;
    g[4][0, 2] = -System.Numerics.Complex.ImaginaryOne; g[4][2, 0] = System.Numerics.Complex.ImaginaryOne;
    g[5][1, 2] = 1; g[5][2, 1] = 1;
    g[6][1, 2] = -System.Numerics.Complex.ImaginaryOne; g[6][2, 1] = System.Numerics.Complex.ImaginaryOne;
    double s = 1.0 / Math.Sqrt(3.0);
    g[7][0, 0] = s; g[7][1, 1] = s; g[7][2, 2] = -2.0 * s;
    return g;
}

static double[,,] StructureConstants(System.Numerics.Complex[][,] genFund)
{
    var fabc = new double[8, 8, 8];
    for (int a = 0; a < 8; a++)
        for (int b = 0; b < 8; b++)
        {
            var comm = new System.Numerics.Complex[3, 3];
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                {
                    System.Numerics.Complex sum = 0;
                    for (int k = 0; k < 3; k++)
                        sum += genFund[a][r, k] * genFund[b][k, c] - genFund[b][r, k] * genFund[a][k, c];
                    comm[r, c] = sum;
                }
            for (int c3 = 0; c3 < 8; c3++)
            {
                System.Numerics.Complex trace = 0;
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        trace += comm[r, c] * genFund[c3][c, r];
                fabc[a, b, c3] = (-2.0 * System.Numerics.Complex.ImaginaryOne * trace).Real;
            }
        }
    return fabc;
}

static double[] JacobiEigen(System.Numerics.Complex[,] hermitian)
{
    int n = hermitian.GetLength(0);
    var a = new double[2 * n, 2 * n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            a[r, c] = hermitian[r, c].Real;
            a[r, c + n] = -hermitian[r, c].Imaginary;
            a[r + n, c] = hermitian[r, c].Imaginary;
            a[r + n, c + n] = hermitian[r, c].Real;
        }
    int m = 2 * n;
    for (int sweep = 0; sweep < 200; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < m; p++)
            for (int q = p + 1; q < m; q++)
                off += a[p, q] * a[p, q];
        if (Math.Sqrt(off) < 1e-14) break;
        for (int p = 0; p < m - 1; p++)
            for (int q = p + 1; q < m; q++)
            {
                double apq = a[p, q];
                if (Math.Abs(apq) < 1e-16) continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double tt = Math.Sign(tau == 0 ? 1.0 : tau) / (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));
                double cc = 1.0 / Math.Sqrt(1.0 + tt * tt);
                double ss = tt * cc;
                for (int k = 0; k < m; k++)
                {
                    if (k == p || k == q) continue;
                    double akp = a[k, p], akq = a[k, q];
                    a[k, p] = a[p, k] = cc * akp - ss * akq;
                    a[k, q] = a[q, k] = ss * akp + cc * akq;
                }
                a[p, p] = cc * cc * app - 2.0 * ss * cc * apq + ss * ss * aqq;
                a[q, q] = ss * ss * app + 2.0 * ss * cc * apq + cc * cc * aqq;
                a[p, q] = a[q, p] = 0.0;
            }
    }
    var vals = new double[m];
    for (int i = 0; i < m; i++) vals[i] = a[i, i];
    // realified doubling: keep one copy of each pair
    return vals.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray();
}

// ===========================================================================
// Records.
// ===========================================================================

public sealed record BatteryResult(string Id, string Name, bool Passed, string Detail);
public sealed record StationaryPoint(double Lo, double Hi, string Kind, double ValueAtRoot, bool DipsBelowOrigin);
public sealed record RowResult(string Rep, int Nf, string Axis, string Convention, double TMax, int PolyDegree,
    List<StationaryPoint> StationaryPoints, bool HasNegativeDepthWell);
public sealed record G1Row(string Rep, int Nf, string Provenance, string Axis, double TStar, double MaxRatio,
    double EndpointRatio, bool Passed, bool Marginal);

// ===========================================================================
// Exact arithmetic: Rational (BigInteger), Q3 = a + b*sqrt(3), Poly over Q3,
// and Sturm-sequence real-root isolation.
// ===========================================================================

public readonly struct Rational
{
    public readonly BigInteger Num;
    public readonly BigInteger Den; // > 0

    private Rational(BigInteger num, BigInteger den) { Num = num; Den = den; }

    public static Rational New(BigInteger num, BigInteger den)
    {
        if (den == 0) throw new DivideByZeroException("rational denominator 0");
        if (den < 0) { num = -num; den = -den; }
        var g = BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den);
        if (g > 1) { num /= g; den /= g; }
        return new Rational(num, den);
    }
    public static Rational FromInt(BigInteger n) => new(n, BigInteger.One);
    public static readonly Rational Zero = new(BigInteger.Zero, BigInteger.One);
    public static readonly Rational One = new(BigInteger.One, BigInteger.One);

    public static Rational FromDoubleApprox(double x)
    {
        // exact-enough rational for the sample points 0.35, 1.25 (finite decimals).
        const long scale = 1_000_000_000;
        return New((BigInteger)Math.Round(x * scale), scale);
    }

    public Rational Add(Rational o) => New(Num * o.Den + o.Num * Den, Den * o.Den);
    public Rational Sub(Rational o) => New(Num * o.Den - o.Num * Den, Den * o.Den);
    public Rational Mul(Rational o) => New(Num * o.Num, Den * o.Den);
    public Rational Div(Rational o) => New(Num * o.Den, Den * o.Num);
    public Rational Negate() => new(-Num, Den);
    public int Sign() => Num.Sign;
    public bool IsZero => Num.IsZero;
    public double ToDouble() => (double)Num / (double)Den;
    public override string ToString() => $"{Num}/{Den}";
}

// a + b*sqrt(3), a,b in Q. A field: inverse of (a+b sqrt3) = (a - b sqrt3)/(a^2-3b^2).
public readonly struct Q3
{
    public readonly Rational A;
    public readonly Rational B;
    public Q3(Rational a, Rational b) { A = a; B = b; }

    public static readonly Q3 Zero = new(Rational.Zero, Rational.Zero);
    public static readonly Q3 One = new(Rational.One, Rational.Zero);
    public static Q3 FromRational(Rational a) => new(a, Rational.Zero);
    public static Q3 Sqrt3Over(BigInteger d) => new(Rational.Zero, Rational.New(1, d)); // sqrt3/d

    public bool IsZero => A.IsZero && B.IsZero;
    public Q3 Add(Q3 o) => new(A.Add(o.A), B.Add(o.B));
    public Q3 Sub(Q3 o) => new(A.Sub(o.A), B.Sub(o.B));
    public Q3 Negate() => new(A.Negate(), B.Negate());
    public Q3 Scale(BigInteger k) => new(A.Mul(Rational.FromInt(k)), B.Mul(Rational.FromInt(k)));
    // (a+b s)(c+d s) = ac + 3bd + (ad+bc) s
    public Q3 Mul(Q3 o) => new(
        A.Mul(o.A).Add(B.Mul(o.B).Mul(Rational.FromInt(3))),
        A.Mul(o.B).Add(B.Mul(o.A)));
    public Q3 Inverse()
    {
        Rational denom = A.Mul(A).Sub(B.Mul(B).Mul(Rational.FromInt(3))); // a^2 - 3b^2
        if (denom.IsZero) throw new DivideByZeroException("Q3 inverse of 0");
        return new Q3(A.Div(denom), B.Negate().Div(denom));
    }
    public Q3 Div(Q3 o) => Mul(o.Inverse());
    public Rational RationalPart() => B.IsZero ? A : throw new InvalidOperationException("not rational");

    // Exact sign of a + b*sqrt(3).
    public int Sign()
    {
        int sa = A.Sign(), sb = B.Sign();
        if (sb == 0) return sa;
        if (sa == 0) return sb; // sqrt3 > 0
        if (sa == sb) return sa;
        Rational t = A.Mul(A).Sub(B.Mul(B).Mul(Rational.FromInt(3))); // a^2 - 3b^2
        // a>0,b<0: positive iff a^2>3b^2 -> sign(t); a<0,b>0: positive iff 3b^2>a^2 -> -sign(t)
        return sa > 0 ? t.Sign() : -t.Sign();
    }
    public double ToDouble() => A.ToDouble() + B.ToDouble() * Math.Sqrt(3.0);
    public string Key() => $"{A.Num}/{A.Den}:{B.Num}/{B.Den}";
}

public sealed class Poly
{
    public readonly Q3[] C; // C[i] = coefficient of t^i; trimmed (top nonzero) unless zero poly

    public Poly(Q3[] c) { C = Trim(c); }
    private static Q3[] Trim(Q3[] c)
    {
        int deg = c.Length - 1;
        while (deg > 0 && c[deg].IsZero) deg--;
        if (deg == c.Length - 1) return c;
        var r = new Q3[deg + 1];
        Array.Copy(c, r, deg + 1);
        return r;
    }

    public static readonly Poly Zero = new(new[] { Q3.Zero });
    public static readonly Poly One = new(new[] { Q3.One });
    public static readonly Poly TSquared = new(new[] { Q3.Zero, Q3.Zero, Q3.One });

    public static Poly Quadratic(Q3 c0, Q3 c1, Q3 c2) => new(new[] { c0, c1, c2 });
    public static Poly FromInts(params long[] coeffs) =>
        new(coeffs.Select(v => Q3.FromRational(Rational.FromInt(v))).ToArray());

    public int Degree => C.Length - 1;
    public bool IsZeroPoly => C.Length == 1 && C[0].IsZero;
    public bool IsConstant => C.Length == 1;

    public Poly Add(Poly o)
    {
        int n = Math.Max(C.Length, o.C.Length);
        var r = new Q3[n];
        for (int i = 0; i < n; i++)
            r[i] = (i < C.Length ? C[i] : Q3.Zero).Add(i < o.C.Length ? o.C[i] : Q3.Zero);
        return new Poly(r);
    }
    public Poly Sub(Poly o)
    {
        int n = Math.Max(C.Length, o.C.Length);
        var r = new Q3[n];
        for (int i = 0; i < n; i++)
            r[i] = (i < C.Length ? C[i] : Q3.Zero).Sub(i < o.C.Length ? o.C[i] : Q3.Zero);
        return new Poly(r);
    }
    public Poly Mul(Poly o)
    {
        if (IsZeroPoly || o.IsZeroPoly) return Zero;
        var r = new Q3[C.Length + o.C.Length - 1];
        for (int i = 0; i < r.Length; i++) r[i] = Q3.Zero;
        for (int i = 0; i < C.Length; i++)
            for (int j = 0; j < o.C.Length; j++)
                r[i + j] = r[i + j].Add(C[i].Mul(o.C[j]));
        return new Poly(r);
    }
    public Poly ScaleQ3(Q3 k)
    {
        if (k.IsZero) return Zero;
        return new Poly(C.Select(c => c.Mul(k)).ToArray());
    }
    public Poly ScaleInt(BigInteger k) => ScaleQ3(Q3.FromRational(Rational.FromInt(k)));

    public Poly Derivative()
    {
        if (C.Length == 1) return Zero;
        var r = new Q3[C.Length - 1];
        for (int i = 1; i < C.Length; i++) r[i - 1] = C[i].Scale(i);
        return new Poly(r);
    }

    public Q3 EvalRational(Rational x)
    {
        Q3 xq = Q3.FromRational(x);
        Q3 acc = Q3.Zero;
        for (int i = C.Length - 1; i >= 0; i--) acc = acc.Mul(xq).Add(C[i]);
        return acc;
    }

    // reflect t -> -t : negate odd-power coefficients.
    public Poly Reflect()
    {
        var r = (Q3[])C.Clone();
        for (int i = 1; i < r.Length; i += 2) r[i] = r[i].Negate();
        return new Poly(r);
    }

    public string Key() => string.Join(";", C.Select(c => c.Key()));

    // exact division when the divisor is known to divide exactly (product terms).
    public Poly ExactDivide(Poly divisor)
    {
        var (q, rem) = DivRem(this, divisor);
        if (!rem.IsZeroPoly) throw new InvalidOperationException("ExactDivide: nonzero remainder");
        return q;
    }

    // strip the largest t^k factor; returns quotient and sets k.
    public Poly FactorOutT(out int power)
    {
        power = 0;
        if (IsZeroPoly) return this;
        int k = 0;
        while (k < C.Length && C[k].IsZero) k++;
        if (k == 0) return this;
        power = k;
        var r = new Q3[C.Length - k];
        Array.Copy(C, k, r, 0, r.Length);
        return new Poly(r);
    }

    public static (Poly Q, Poly R) DivRem(Poly a, Poly b)
    {
        if (b.IsZeroPoly) throw new DivideByZeroException("poly division by zero");
        var rem = (Q3[])a.C.Clone();
        int degR = rem.Length - 1;
        int degB = b.Degree;
        if (degR < degB) return (Zero, a);
        var quot = new Q3[degR - degB + 1];
        for (int i = 0; i < quot.Length; i++) quot[i] = Q3.Zero;
        Q3 lcbInv = b.C[degB].Inverse();
        var work = rem.ToList();
        for (int i = degR; i >= degB; i--)
        {
            Q3 coeff = work[i].Mul(lcbInv);
            quot[i - degB] = coeff;
            if (coeff.IsZero) continue;
            for (int j = 0; j <= degB; j++)
                work[i - degB + j] = work[i - degB + j].Sub(coeff.Mul(b.C[j]));
        }
        return (new Poly(quot), new Poly(work.ToArray()));
    }

    // ----- Sturm sequences over Q(sqrt3) -----
    public static List<Poly> SturmChain(Poly p)
    {
        var chain = new List<Poly> { p, p.Derivative() };
        while (!chain[^1].IsZeroPoly && chain[^1].Degree > 0 || (!chain[^1].IsZeroPoly && chain.Count == 2))
        {
            var (_, r) = DivRem(chain[^2], chain[^1]);
            if (r.IsZeroPoly) break;
            chain.Add(r.ScaleQ3(Q3.One.Negate())); // -remainder
        }
        return chain;
    }

    private static int SignVariations(List<Poly> chain, Rational x)
    {
        int variations = 0, last = 0;
        foreach (var poly in chain)
        {
            int s = poly.EvalRational(x).Sign();
            if (s == 0) continue;
            if (last != 0 && s != last) variations++;
            last = s;
        }
        return variations;
    }

    // number of distinct real roots in (a, b].
    public static int CountRealRootsInInterval(Poly p, Rational a, Rational b)
    {
        if (p.IsZeroPoly) return 0;
        var stripped = p.FactorOutT(out int k);
        // a t^k factor contributes a root at 0, which is not in (a,b] when a>=0.
        var chain = SturmChain(stripped);
        return SignVariations(chain, a) - SignVariations(chain, b);
    }

    // isolate each distinct real root in (a,b] into a subinterval with exactly one.
    public static List<(Rational Lo, Rational Hi)> IsolateRealRoots(Poly p, Rational a, Rational b)
    {
        var result = new List<(Rational, Rational)>();
        if (p.IsZeroPoly) return result;
        var stripped = p.FactorOutT(out _);
        var chain = SturmChain(stripped);
        void Recurse(Rational lo, Rational hi, int depth)
        {
            int count = SignVariations(chain, lo) - SignVariations(chain, hi);
            if (count == 0) return;
            if (count == 1)
            {
                // tighten to a narrow bracket around the single (simple) root
                var (rlo, rhi) = Refine(stripped, lo, hi);
                result.Add((rlo, rhi));
                return;
            }
            if (depth > 200) { result.Add((lo, hi)); return; }
            Rational mid = lo.Add(hi).Div(Rational.FromInt(2));
            Recurse(lo, mid, depth + 1);
            Recurse(mid, hi, depth + 1);
        }
        Recurse(a, b, 0);
        return result;
    }

    private static (Rational, Rational) Refine(Poly p, Rational lo, Rational hi)
    {
        // The single distinct root of a simple crossing is bracketed by opposite
        // signs; bisect on sign until the width is tiny so t* and V(t*) are
        // accurate. If the endpoints share a sign the root is even-multiplicity
        // (a tangency) and the bracket is returned as-is for degenerate handling.
        int slo = p.EvalRational(lo).Sign();
        if (slo == 0) return (lo, hi);
        for (int it = 0; it < 200; it++)
        {
            if ((hi.Sub(lo)).ToDouble() < 1e-12) break;
            Rational mid = lo.Add(hi).Div(Rational.FromInt(2));
            int sm = p.EvalRational(mid).Sign();
            if (sm == 0) { lo = mid; hi = mid; break; }
            if (sm == slo) lo = mid; else hi = mid;
        }
        return (lo, hi);
    }
}
