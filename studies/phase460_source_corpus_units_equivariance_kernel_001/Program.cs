// Phase460 - Team A Wave-1 rank-2 (A1): units-equivariance kernel theorem over
// the audited source corpus (TEAM_ELIMINATION_PROGRAM_2026-07-10.md, Wave-1
// item 2). ELIMINATION computation - exact integer arithmetic, deterministic,
// target-blind. Every pinned relation recorded by the committed phase330-345
// literature-audit outputs (referred to strictly by phase number) and by the
// committed reference-snapshot text is extracted into integer
// grading-constraint rows; the FULL integer kernel is computed per admissible
// reading via a Smith-normal-form routine written here in exact BigInteger
// arithmetic and unit-tested in-phase; the kernel is intersected with the
// one-parameter scale-transformation family; per-reading verdicts are decided
// on the pre-registered taxonomy {scaling-symmetry-closes,
// equivariance-breaking-relation-found(ids), corpus-dimensionally-ambiguous(ids)}.
//
// BINDING PRE-REGISTRATION (from the program spec): the reference-snapshot
// relation "cosmological constant as a VEV" (the final-method table of the
// mirror snapshot; phase328 is the committed audit of that snapshot) is
// pre-registered as the EXPECTED equivariance-breaking hit, decided by the
// machine-checkable pinned-relation criterion: a coefficient- and
// units-bearing statement is a PINNED kernel row; a coefficient-free slot
// statement is ROUTED to the anchor-reading-menu phase (A2/phase461) and is
// NEVER silently a kernel row. The extractor FAIL-CLOSES unless that relation
// either appears as the breaking hit or routes correctly under the criterion.
// Prose-only/unextractable relations enter a corpus-dimensionally-ambiguous
// BLOCKING set and are never dropped.
//
// Grading conventions are workbench conventions pending physicist review
// (physicistReviewPending = true). No GeV content anywhere; nothing promoted.

using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase460_source_corpus_units_equivariance_kernel_001/output";
const string MirrorSnapshotPath = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md";
const string ApplicationSubjectKind = "source-corpus-units-equivariance-kernel";
const string TerminalPrefix = "source-corpus-units-equivariance-kernel-";
const string ExpectedBreakingRelationId = "sp12d-cc";

var outputDir = Environment.GetEnvironmentVariable("PHASE460_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var checks = new List<Check>();

// ---------------------------------------------------------------------------
// 1. Exact BigInteger Smith normal form + in-phase unit battery.
// ---------------------------------------------------------------------------

var snfBatteryCases = new List<object>();
var snfBatteryPassed = true;

void RunSnfBatteryCase(string caseId, BigInteger[][] matrix, BigInteger[]? expectedDiagonal)
{
    var snf = SmithNormalForm.Compute(matrix);
    var diag = snf.Diagonal;
    var ok = true;
    var detail = new StringBuilder();

    // Divisibility chain d1 | d2 | ... on nonzero entries; all nonnegative.
    for (var i = 0; i < diag.Length; i++)
    {
        if (diag[i] < 0)
        {
            ok = false;
            detail.Append($"negative-diagonal@{i};");
        }

        if (i > 0 && diag[i] != 0 && diag[i - 1] != 0 && diag[i] % diag[i - 1] != 0)
        {
            ok = false;
            detail.Append($"divisibility-broken@{i};");
        }

        if (i > 0 && diag[i - 1] == 0 && diag[i] != 0)
        {
            ok = false;
            detail.Append($"zero-before-nonzero@{i};");
        }
    }

    if (expectedDiagonal is not null)
    {
        if (expectedDiagonal.Length != diag.Length || !expectedDiagonal.SequenceEqual(diag))
        {
            ok = false;
            detail.Append($"expected=[{string.Join(",", expectedDiagonal)}]actual=[{string.Join(",", diag)}];");
        }
    }

    // Independent determinantal-divisor cross-check for small matrices:
    // gcd of all k x k minors must equal d1*...*dk.
    var rows = matrix.Length;
    var cols = matrix[0].Length;
    if (rows <= 4 && cols <= 4)
    {
        var running = BigInteger.One;
        for (var k = 1; k <= Math.Min(rows, cols); k++)
        {
            var gk = MinorGcd(matrix, k);
            var dk = diag[k - 1];
            running *= dk < 0 ? -dk : dk;
            if (gk != running)
            {
                ok = false;
                detail.Append($"determinantal-divisor-mismatch@k={k}(gcdMinors={gk},prodDiag={running});");
            }

            if (gk == 0)
            {
                break;
            }
        }
    }

    // Kernel verification: A * v == 0 for every kernel basis vector; kernel
    // count == cols - rank.
    var rank = diag.Count(d => d != 0);
    if (snf.KernelBasis.Length != cols - rank)
    {
        ok = false;
        detail.Append($"kernel-count-mismatch(kernel={snf.KernelBasis.Length},cols-rank={cols - rank});");
    }

    foreach (var v in snf.KernelBasis)
    {
        for (var r = 0; r < rows; r++)
        {
            var acc = BigInteger.Zero;
            for (var c = 0; c < cols; c++)
            {
                acc += matrix[r][c] * v[c];
            }

            if (acc != 0)
            {
                ok = false;
                detail.Append($"kernel-vector-not-annihilated@row={r};");
            }
        }
    }

    if (!ok)
    {
        snfBatteryPassed = false;
    }

    snfBatteryCases.Add(new
    {
        caseId,
        passed = ok,
        diagonal = diag.Select(d => d.ToString()).ToArray(),
        rank,
        kernelCount = snf.KernelBasis.Length,
        detail = detail.Length == 0 ? "ok" : detail.ToString(),
    });
}

static BigInteger MinorGcd(BigInteger[][] m, int k)
{
    var rows = m.Length;
    var cols = m[0].Length;
    var rowSets = Combinations(rows, k);
    var colSets = Combinations(cols, k);
    var g = BigInteger.Zero;
    foreach (var rs in rowSets)
    {
        foreach (var cs in colSets)
        {
            var sub = rs.Select(r => cs.Select(c => m[r][c]).ToArray()).ToArray();
            var det = Determinant(sub);
            g = BigInteger.GreatestCommonDivisor(g, det);
        }
    }

    return g < 0 ? -g : g;
}

static List<int[]> Combinations(int n, int k)
{
    var result = new List<int[]>();
    var idx = Enumerable.Range(0, k).ToArray();
    while (true)
    {
        result.Add((int[])idx.Clone());
        var i = k - 1;
        while (i >= 0 && idx[i] == n - k + i)
        {
            i--;
        }

        if (i < 0)
        {
            return result;
        }

        idx[i]++;
        for (var j = i + 1; j < k; j++)
        {
            idx[j] = idx[j - 1] + 1;
        }
    }
}

static BigInteger Determinant(BigInteger[][] m)
{
    var n = m.Length;
    if (n == 1)
    {
        return m[0][0];
    }

    var det = BigInteger.Zero;
    for (var c = 0; c < n; c++)
    {
        var sub = new BigInteger[n - 1][];
        for (var r = 1; r < n; r++)
        {
            sub[r - 1] = new BigInteger[n - 1];
            var cc = 0;
            for (var c2 = 0; c2 < n; c2++)
            {
                if (c2 == c)
                {
                    continue;
                }

                sub[r - 1][cc++] = m[r][c2];
            }
        }

        var term = m[0][c] * Determinant(sub);
        det += (c % 2 == 0) ? term : -term;
    }

    return det;
}

static BigInteger[][] M(params long[][] rows) =>
    rows.Select(r => r.Select(v => new BigInteger(v)).ToArray()).ToArray();

// Battery: classical hand-checked matrices (determinantal divisors verified
// independently by brute-force minor gcds), identity, zero, rank-deficient.
RunSnfBatteryCase("classic-3x3", M(
    [2, 4, 4],
    [-6, 6, 12],
    [10, 4, 16]), [new BigInteger(2), new BigInteger(2), new BigInteger(156)]);
RunSnfBatteryCase("identity-3x3", M(
    [1, 0, 0],
    [0, 1, 0],
    [0, 0, 1]), [BigInteger.One, BigInteger.One, BigInteger.One]);
RunSnfBatteryCase("zero-2x3", M(
    [0, 0, 0],
    [0, 0, 0]), [BigInteger.Zero, BigInteger.Zero]);
RunSnfBatteryCase("rank1-2x2", M(
    [1, 2],
    [2, 4]), [BigInteger.One, BigInteger.Zero]);
RunSnfBatteryCase("rect-2x4-rank2", M(
    [2, 0, 4, 6],
    [0, 3, 6, 9]), null);
RunSnfBatteryCase("mixed-4x3", M(
    [3, 1, 2],
    [6, 2, 4],
    [0, 5, 5],
    [9, 3, 6]), null);

checks.Add(new Check(
    "snf-exact-battery",
    snfBatteryPassed,
    $"cases={snfBatteryCases.Count}; allPassed={snfBatteryPassed} (divisibility chain, determinantal-divisor cross-check, kernel annihilation, kernel count)"));

// ---------------------------------------------------------------------------
// 2. Symbol table and admissible readings (pre-registered conventions).
//    Prescribed base reading (length grading): metric +2, connection -1,
//    curvature (field strength / scalar curvature) -2, mass -1. Canonical
//    extensions: scalar fields/VEVs -1, potential density -4, inverse
//    gravitational-coupling-squared symbol +2, compact/warp radii +1,
//    dimensionless couplings/angles/twists 0. The cc term carries the
//    curvature-class grade -2 in the base reading and is re-graded across the
//    admissible family below.
// ---------------------------------------------------------------------------

string[] symbols =
[
    "metricG", "connOmega", "curvF", "curvR", "massGeneric",
    "massW", "massZ", "massH", "massDirac", "massMode",
    "massVectorGeom", "massVectorGauge", "massVectorExtra",
    "massComposite", "massElementary", "massPlanck", "massTopo",
    "massParamVector", "condensateScale", "vevScalar", "scalarField",
    "scalarCompensator", "potentialV", "ccTerm", "newtonConst",
    "radiusCompact", "radiusWarp", "couplingA", "couplingB", "couplingQ",
    "quarticSelf", "twistParam", "wilsonPhase",
];
var symbolIndex = symbols.Select((s, i) => (s, i)).ToDictionary(t => t.s, t => t.i);

var baseGrading = new Dictionary<string, int>
{
    ["metricG"] = 2,
    ["connOmega"] = -1,
    ["curvF"] = -2,
    ["curvR"] = -2,
    ["massGeneric"] = -1,
    ["massW"] = -1,
    ["massZ"] = -1,
    ["massH"] = -1,
    ["massDirac"] = -1,
    ["massMode"] = -1,
    ["massVectorGeom"] = -1,
    ["massVectorGauge"] = -1,
    ["massVectorExtra"] = -1,
    ["massComposite"] = -1,
    ["massElementary"] = -1,
    ["massPlanck"] = -1,
    ["massTopo"] = -1,
    ["massParamVector"] = -1,
    ["condensateScale"] = -1,
    ["vevScalar"] = -1,
    ["scalarField"] = -1,
    ["scalarCompensator"] = -1,
    ["potentialV"] = -4,
    ["ccTerm"] = -2,
    ["newtonConst"] = 2,
    ["radiusCompact"] = 1,
    ["radiusWarp"] = 1,
    ["couplingA"] = 0,
    ["couplingB"] = 0,
    ["couplingQ"] = 0,
    ["quarticSelf"] = 0,
    ["twistParam"] = 0,
    ["wilsonPhase"] = 0,
};

BigInteger[] GradingVector(int ccGrade)
{
    var v = new BigInteger[symbols.Length];
    foreach (var (s, i) in symbolIndex)
    {
        v[i] = s == "ccTerm" ? ccGrade : baseGrading[s];
    }

    return v;
}

// Admissible reading family (pre-registered, finite):
//   matrix variant "literal"  = the mirror's literal first-power curvature
//                               mass statement as written;
//   matrix variant "squared"  = the mirror's own recorded squared-mass repair;
//   cc re-grades: -2 (curvature-class, prescribed), -1 (the mass-class
//   reading that the CC-as-a-VEV slot statement itself suggests), -4 (the
//   energy-density-class reading).
var readingDefs = new (string ReadingId, string MatrixVariant, int CcGrade, string Description)[]
{
    ("R0-prescribed-literal", "literal", -2, "prescribed grading; literal curvature-mass row"),
    ("R1-prescribed-squared", "squared", -2, "prescribed grading; squared-mass repair row"),
    ("R2-ccMass-literal", "literal", -1, "cc term re-graded to mass class; literal curvature-mass row"),
    ("R3-ccMass-squared", "squared", -1, "cc term re-graded to mass class; squared-mass repair row"),
    ("R4-ccDensity-squared", "squared", -4, "cc term re-graded to energy-density class; squared-mass repair row"),
};

// ---------------------------------------------------------------------------
// 3. Relation inventory (pre-registered extraction of every pinned relation).
//    Classifications: pinned-row | pinned-trivial-homogeneous |
//    routed-to-anchor-menu | dimensionally-ambiguous.
// ---------------------------------------------------------------------------

var inventory = new List<InventoryItem>
{
    // Mirror snapshot (quote-based items; machine criterion applies).
    new("sp12c-lit", MirrorSnapshotPath, "snapshot-quote", "m = R(y)/4", "pinned-row",
        new() { ["massDirac"] = 1, ["curvR"] = -1 },
        "first-power curvature-mass statement as written (literal matrix variant)"),
    new("sp12c-sq", MirrorSnapshotPath, "snapshot-quote", "m^2 = R/4", "pinned-row",
        new() { ["massDirac"] = 2, ["curvR"] = -1 },
        "the snapshot's own recorded squared-mass repair (squared matrix variant)"),
    new("sp12-upsilon-first", MirrorSnapshotPath, "snapshot-quote", "Upsilon_omega = 0", "pinned-trivial-homogeneous",
        new(),
        "homogeneous field equation (zero right-hand side); no grading constraint"),
    new("sp12-upsilon-second", MirrorSnapshotPath, "snapshot-quote", "D_omega^* Upsilon_omega = 0", "pinned-trivial-homogeneous",
        new(),
        "homogeneous field equation (zero right-hand side); no grading constraint"),
    new("sp12d-cc", MirrorSnapshotPath, "snapshot-quote", "cosmological constant as a VEV", "routed-to-anchor-menu",
        new() { ["ccTerm"] = 1, ["massGeneric"] = -1 },
        "PRE-REGISTERED expected breaking relation; coefficient-free anchor slot statement; exponents recorded as diagnostic only, never a kernel row"),
    new("sp12d-yukawa", MirrorSnapshotPath, "snapshot-quote", "Yukawa couplings as a VEV", "dimensionally-ambiguous",
        new(),
        "coefficient-free slot statement outside the pre-registered anchor route; blocking set"),

    // phase330 (committed audit output keys; curated extraction).
    new("p330-r1", AuditPath(330), "committed-audit-key", "43a429a4b8f0", "pinned-row",
        new() { ["massW"] = 1, ["couplingA"] = -1, ["vevScalar"] = -1 },
        "charged vector mass = coupling x scalar VEV / 2"),
    new("p330-r2", AuditPath(330), "committed-audit-key", "43a429a4b8f0", "pinned-row",
        new() { ["massZ"] = 1, ["couplingA"] = -1, ["vevScalar"] = -1 },
        "neutral vector mass = root-sum-square coupling x scalar VEV / 2"),
    new("p330-r2b", AuditPath(330), "committed-audit-key", "43a429a4b8f0", "pinned-row",
        new() { ["couplingA"] = 1, ["couplingB"] = -1 },
        "summands under the root-sum-square must share a grade"),
    new("p330-r3", AuditPath(330), "committed-audit-key", "153f904ab06d", "pinned-row",
        new() { ["massH"] = 2, ["quarticSelf"] = -1, ["vevScalar"] = -2 },
        "scalar mass squared = quartic x VEV squared"),
    new("p330-r4", AuditPath(330), "committed-audit-key", "90f18274d508", "pinned-row",
        new() { ["massVectorGeom"] = 1, ["couplingQ"] = -1, ["scalarCompensator"] = -1 },
        "geometric vector mass = charge x compensator scalar"),
    new("p330-r5", AuditPath(330), "committed-audit-key", "65f66d78d5fb", "pinned-row",
        new() { ["massPlanck"] = 1, ["scalarCompensator"] = -1 },
        "gravity-scale normalization pins the compensator scalar to the mass class"),
    new("p330-r6", AuditPath(330), "committed-audit-key", "fb1210f749b4", "pinned-row",
        new() { ["scalarField"] = 1, ["vevScalar"] = -1 },
        "quartic potential shape forces field and VEV into the same grade"),

    // phase331.
    new("p331-a1", AuditPath(331), "committed-audit-key", "1ed0fa73cd18", "dimensionally-ambiguous",
        new(),
        "field/potential location statement, prose-only; blocking set"),
    new("p331-a2", AuditPath(331), "committed-audit-key", "73d5b9c40061", "dimensionally-ambiguous",
        new(),
        "internal-charge location statement, prose-only; blocking set"),

    // phase332.
    new("p332-a1", AuditPath(332), "committed-audit-key", "183c2b8f397c", "dimensionally-ambiguous",
        new(),
        "framework-level scalar-mass claim without a committed symbolic relation; blocking set"),
    new("p332-a2", AuditPath(332), "committed-audit-key", "7b1932df11a4", "dimensionally-ambiguous",
        new(),
        "numeric-interval scalar-mass claim without a committed symbolic relation; blocking set"),

    // phase333.
    new("p333-r1", AuditPath(333), "committed-audit-key", "4b7e3275769a", "pinned-row",
        new() { ["massMode"] = 1, ["radiusCompact"] = 1 },
        "classical mode mass = inverse internal length scale"),
    new("p333-a1", AuditPath(333), "committed-audit-key", "f8fafad6ee93", "dimensionally-ambiguous",
        new(),
        "arbitrarily-light-mass statement, prose-only; blocking set"),
    new("p333-a2", AuditPath(333), "committed-audit-key", "6a8a86d68490", "dimensionally-ambiguous",
        new(),
        "scale-selection requirement slot outside the pre-registered anchor route; blocking set"),

    // phase334.
    new("p334-r1", AuditPath(334), "committed-audit-key", "ce150b68e2b7", "pinned-row",
        new() { ["massH"] = 1, ["massW"] = -1 },
        "tree-level scalar-to-vector mass ratio"),
    new("p334-r1b", AuditPath(334), "committed-audit-key", "ce150b68e2b7", "pinned-row",
        new() { ["massW"] = 1, ["massZ"] = -1 },
        "tree-level vector-to-vector mass ratio"),
    new("p334-r2", AuditPath(334), "committed-audit-key", "09e7eb9eae22", "pinned-trivial-homogeneous",
        new(),
        "dimensionless mixing-angle relation; no grading constraint"),
    new("p334-r3", AuditPath(334), "committed-audit-key", "2ef764abd92a", "pinned-row",
        new() { ["quarticSelf"] = 1, ["couplingA"] = -2 },
        "quartic constrained to coupling squared (both dimensionless-class)"),
    new("p334-a1", AuditPath(334), "committed-audit-key", "3eef77c4aaba", "dimensionally-ambiguous",
        new(),
        "observed-comparison conflict statement, prose-only; blocking set"),

    // phase335.
    new("p335-r1", AuditPath(335), "committed-audit-key", "df894626c1f5", "pinned-row",
        new() { ["massPlanck"] = 2, ["newtonConst"] = 1 },
        "gravity-scale squared x gravitational coupling = dimensionless"),
    new("p335-r2", AuditPath(335), "committed-audit-key", "03c549b3dbe0", "pinned-row",
        new() { ["couplingA"] = 2, ["newtonConst"] = -1, ["ccTerm"] = -1 },
        "gauge coupling squared = gravitational coupling x cc term (the corpus's pinned cc-bearing relation)"),
    new("p335-a1", AuditPath(335), "committed-audit-key", "2a5d26b4bdf3", "dimensionally-ambiguous",
        new(),
        "bare-coupling/VEV relation statement, prose-only; blocking set"),
    new("p335-a2", AuditPath(335), "committed-audit-key", "08eab7840440", "dimensionally-ambiguous",
        new(),
        "scalar-mass interval lead, prose-only; blocking set"),
    new("p335-a3", AuditPath(335), "committed-audit-key", "c972643befbb", "dimensionally-ambiguous",
        new(),
        "second-sector scale lead, prose-only; blocking set"),

    // phase336.
    new("p336-r1", AuditPath(336), "committed-audit-key", "64f07dfb027a", "pinned-row",
        new() { ["massW"] = 1, ["couplingA"] = -1, ["vevScalar"] = -1 },
        "vector masses from VEV, couplings, and scalar-manifold metric"),
    new("p336-r2", AuditPath(336), "committed-audit-key", "fa8dba3c85b8", "pinned-row",
        new() { ["massH"] = 2, ["potentialV"] = -1, ["scalarField"] = 2 },
        "scalar mass squared = second field derivative of the potential density"),
    new("p336-a1", AuditPath(336), "committed-audit-key", "df2bdc5bddbc", "dimensionally-ambiguous",
        new(),
        "assumed massive-pattern statement, prose-only; blocking set"),

    // phase337.
    new("p337-r1", AuditPath(337), "committed-audit-key", "667980142c41", "pinned-row",
        new() { ["massH"] = 1, ["massW"] = -1 },
        "scalar-to-vector mass relation (external ratio lead)"),
    new("p337-r2", AuditPath(337), "committed-audit-key", "bded60be7783", "pinned-trivial-homogeneous",
        new(),
        "dimensionless mixing-angle relation; no grading constraint"),

    // phase338.
    new("p338-r1", AuditPath(338), "committed-audit-key", "061167142cd7", "pinned-row",
        new() { ["massVectorGauge"] = 1, ["couplingA"] = -1, ["condensateScale"] = -1 },
        "vector mass = coupling x condensate scale"),
    new("p338-r2", AuditPath(338), "committed-audit-key", "c952f4bff7b0", "pinned-row",
        new() { ["massVectorGauge"] = 1, ["massParamVector"] = -1 },
        "explicit quadratic vector mass term pins the mass parameter"),
    new("p338-r3", AuditPath(338), "committed-audit-key", "8a0df29a5137", "pinned-row",
        new() { ["massW"] = 1, ["couplingA"] = -1, ["vevScalar"] = -1 },
        "textbook mass structure recovered: vector mass = coupling x VEV"),
    new("p338-r4", AuditPath(338), "committed-audit-key", "56c6a1ba0fe7", "pinned-row",
        new() { ["massH"] = 2, ["quarticSelf"] = -1, ["vevScalar"] = -2 },
        "formulated breaking mechanism includes the standard scalar-sector mass-squared = quartic x VEV squared"),
    new("p338-a1", AuditPath(338), "committed-audit-key", "fa788f3dfff5", "dimensionally-ambiguous",
        new(),
        "scale-parameter requirement slot outside the pre-registered anchor route; blocking set"),

    // phase339.
    new("p339-r1", AuditPath(339), "committed-audit-key", "839eb96f99cf", "pinned-row",
        new() { ["newtonConst"] = 1, ["ccTerm"] = 1 },
        "broken-gauge gravity action normalization: gravitational coupling x cc term = dimensionless"),
    new("p339-r2", AuditPath(339), "committed-audit-key", "a3bc4e7a7fc2", "pinned-row",
        new() { ["massW"] = 1, ["couplingA"] = -1, ["vevScalar"] = -1 },
        "electroweak branch uses standard vector mass = coupling x VEV"),
    new("p339-a1", AuditPath(339), "committed-audit-key", "e1e0c1c0e343", "dimensionally-ambiguous",
        new(),
        "mass-assignment claim, prose-only; blocking set"),
    new("p339-a2", AuditPath(339), "committed-audit-key", "9ae5036cdaf8", "dimensionally-ambiguous",
        new(),
        "mass matrix depends on breaking parameters, prose-only; blocking set"),
    new("p339-a3", AuditPath(339), "committed-audit-key", "a8c0b3ee9e47", "dimensionally-ambiguous",
        new(),
        "scale-for-observed-mass trade statement, prose-only; blocking set"),

    // phase340.
    new("p340-r1", AuditPath(340), "committed-audit-key", "b84ff1d2c703", "pinned-row",
        new() { ["massVectorGauge"] = 1, ["massTopo"] = -1 },
        "two-form coupling generates vector mass equal to the topological mass parameter"),
    new("p340-r2", AuditPath(340), "committed-audit-key", "7c467beabc66", "pinned-row",
        new() { ["massW"] = 1, ["radiusCompact"] = 1 },
        "vector masses set by inverse curvature radius"),
    new("p340-a1", AuditPath(340), "committed-audit-key", "34f8e54d5722", "dimensionally-ambiguous",
        new(),
        "mass-parameter source requirement slot outside the pre-registered anchor route; blocking set"),
    new("p340-a2", AuditPath(340), "committed-audit-key", "2e211bc7b684", "dimensionally-ambiguous",
        new(),
        "tensor-sector mass-gap lead, prose-only; blocking set"),
    new("p340-a3", AuditPath(340), "committed-audit-key", "ff89f1a39f9f", "dimensionally-ambiguous",
        new(),
        "topological-mass lead, prose-only; blocking set"),

    // phase341.
    new("p341-r1", AuditPath(341), "committed-audit-key", "be1e5fea4fe5", "pinned-row",
        new() { ["massMode"] = 1, ["twistParam"] = -1, ["radiusCompact"] = 1 },
        "mode mass = twist over compact radius"),
    new("p341-r2", AuditPath(341), "committed-audit-key", "24fe1e8c94b2", "pinned-row",
        new() { ["massW"] = 1, ["wilsonPhase"] = -1, ["radiusCompact"] = 1 },
        "vector mass = holonomy phase over compact radius"),
    new("p341-a1", AuditPath(341), "committed-audit-key", "9630cda3bad0", "dimensionally-ambiguous",
        new(),
        "small-phase matching statement (observed-comparison), prose-only; blocking set"),
    new("p341-a2", AuditPath(341), "committed-audit-key", "5d03aeda500f", "dimensionally-ambiguous",
        new(),
        "spectrum-difficulty statement, prose-only; blocking set"),

    // phase342.
    new("p342-r1", AuditPath(342), "committed-audit-key", "0077bf3365bb", "pinned-row",
        new() { ["massW"] = 1, ["radiusWarp"] = 1 },
        "boundary-condition vector mass = inverse warp/interval scale"),
    new("p342-r2", AuditPath(342), "committed-audit-key", "23d94059d514", "pinned-row",
        new() { ["massZ"] = 1, ["radiusCompact"] = 1 },
        "second vector mass arranged by a second compactification scale"),
    new("p342-a1", AuditPath(342), "committed-audit-key", "91da67aafa32", "dimensionally-ambiguous",
        new(),
        "precision-constraint boundary statement, prose-only; blocking set"),
    new("p342-a1b", AuditPath(342), "committed-audit-key", "90ede7928227", "dimensionally-ambiguous",
        new(),
        "precision-constraint lead, prose-only; blocking set"),
    new("p342-a2", AuditPath(342), "committed-audit-key", "f64b6a79a83f", "dimensionally-ambiguous",
        new(),
        "fit-by-varying-couplings statement, prose-only; blocking set"),
    new("p342-a3", AuditPath(342), "committed-audit-key", "e3bccf2a1f59", "dimensionally-ambiguous",
        new(),
        "resonance-prediction statement, prose-only; blocking set"),

    // phase343.
    new("p343-r1", AuditPath(343), "committed-audit-key", "a679dc50bd3d", "pinned-row",
        new() { ["massW"] = 1, ["massParamVector"] = -1 },
        "explicit vector mass parameter carries the charged vector mass"),
    new("p343-r1b", AuditPath(343), "committed-audit-key", "a679dc50bd3d", "pinned-row",
        new() { ["massZ"] = 1, ["massParamVector"] = -1 },
        "explicit vector mass parameter carries the neutral vector mass"),
    new("p343-r2", AuditPath(343), "committed-audit-key", "3818304e1841", "pinned-row",
        new() { ["massVectorExtra"] = 1, ["massParamVector"] = -1 },
        "extra neutral vector mass from the same explicit parameter class"),
    new("p343-r3", AuditPath(343), "committed-audit-key", "57141158e9c6", "pinned-row",
        new() { ["massVectorGauge"] = 1, ["massParamVector"] = -1 },
        "gauge-invariant compensator construction pins the abelian vector mass parameter"),
    new("p343-a1", AuditPath(343), "committed-audit-key", "03c6cabe92aa", "dimensionally-ambiguous",
        new(),
        "mass-parameter source requirement slot outside the pre-registered anchor route; blocking set"),

    // phase344.
    new("p344-r1", AuditPath(344), "committed-audit-key", "21a7c1dc4447", "pinned-row",
        new() { ["massComposite"] = 1, ["massElementary"] = -1 },
        "composite-state mass maps to elementary-state mass at leading order"),
    new("p344-a1", AuditPath(344), "committed-audit-key", "10364a196bed", "dimensionally-ambiguous",
        new(),
        "doublet/VEV requirement slot, prose-only; blocking set"),
    new("p344-a2", AuditPath(344), "committed-audit-key", "a5fccc95bfaa", "dimensionally-ambiguous",
        new(),
        "pole-extraction requirement, prose-only; blocking set"),

    // phase345.
    new("p345-a1", AuditPath(345), "committed-audit-key", "0d8765bc73b9", "dimensionally-ambiguous",
        new(),
        "phase-structure continuity statement; no symbolic mass relation; blocking set"),
    new("p345-a2", AuditPath(345), "committed-audit-key", "44508c5ef370", "dimensionally-ambiguous",
        new(),
        "local-symmetry order-parameter obstruction statement; blocking set"),
    new("p345-a3", AuditPath(345), "committed-audit-key", "f98f98569195", "dimensionally-ambiguous",
        new(),
        "analytic-continuity statement; blocking set"),
};

static string AuditPath(int phase) => phase switch
{
    330 => "studies/phase330_weyl_geometric_mass_generation_source_audit_001/output/weyl_geometric_mass_generation_source_audit.json",
    331 => "studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/output/theta_omega_inhomogeneous_gauge_source_audit.json",
    332 => "studies/phase332_string_m_theory_compactification_source_audit_001/output/string_m_theory_compactification_source_audit.json",
    333 => "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit.json",
    334 => "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit.json",
    335 => "studies/phase335_graviweak_plebanski_source_audit_001/output/graviweak_plebanski_source_audit.json",
    336 => "studies/phase336_heft_scalar_geometry_source_law_audit_001/output/heft_scalar_geometry_source_law_audit.json",
    337 => "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit.json",
    338 => "studies/phase338_metric_affine_torsion_source_audit_001/output/metric_affine_torsion_source_audit.json",
    339 => "studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/output/macdowell_mansouri_cartan_breaking_source_audit.json",
    340 => "studies/phase340_bf_topological_mass_source_audit_001/output/bf_topological_mass_source_audit.json",
    341 => "studies/phase341_scherk_schwarz_twisted_compactification_source_audit_001/output/scherk_schwarz_twisted_compactification_source_audit.json",
    342 => "studies/phase342_higgsless_boundary_condition_source_audit_001/output/higgsless_boundary_condition_source_audit.json",
    343 => "studies/phase343_stueckelberg_vector_mass_source_audit_001/output/stueckelberg_vector_mass_source_audit.json",
    344 => "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit.json",
    345 => "studies/phase345_fradkin_shenker_complementarity_source_audit_001/output/fradkin_shenker_complementarity_source_audit.json",
    _ => throw new InvalidOperationException($"unregistered audit phase {phase}"),
};

// ---------------------------------------------------------------------------
// 4. Corpus evidence checks.
//    (a) Snapshot quotes must appear verbatim in the committed snapshot text
//        (whitespace-normalized).
//    (b) Committed-audit-key evidence must match the pre-registered key
//        hashes, key counts, and full sorted-true-key-list hashes (freshness
//        pin: ANY change to a committed audit's boolean surface fails closed).
//    (c) Coverage gate: every equation-indicator key (generic regex net over
//        ALL true keys) must be mapped by the inventory; every pre-registered
//        key hash must exist in the committed file.
// ---------------------------------------------------------------------------

var corpusFilesPresent = true;
var evidenceDetail = new StringBuilder();

string normalizedMirrorText;
if (File.Exists(MirrorSnapshotPath))
{
    normalizedMirrorText = Regex.Replace(File.ReadAllText(MirrorSnapshotPath), @"\s+", " ");
}
else
{
    normalizedMirrorText = string.Empty;
    corpusFilesPresent = false;
    evidenceDetail.Append("mirror-snapshot-missing;");
}

var quoteEvidencePassed = true;
foreach (var item in inventory.Where(i => i.EvidenceKind == "snapshot-quote"))
{
    if (!normalizedMirrorText.Contains(item.EvidenceRef, StringComparison.Ordinal))
    {
        quoteEvidencePassed = false;
        evidenceDetail.Append($"quote-not-found:{item.ItemId};");
    }
}

checks.Add(new Check(
    "snapshot-quote-evidence",
    corpusFilesPresent && quoteEvidencePassed,
    corpusFilesPresent && quoteEvidencePassed
        ? $"all {inventory.Count(i => i.EvidenceKind == "snapshot-quote")} snapshot quotes located (whitespace-normalized)"
        : evidenceDetail.ToString()));

// Pre-registered per-audit freshness pins and coverage maps. The key hash is
// the first 12 hex chars of SHA-256 over the camelCase key name; the list
// hash is the first 16 hex chars of SHA-256 over the newline-joined sorted
// true-key names. Key names are held as hashes (not literals) deliberately:
// the audits are referred to by phase number only.
var auditPins = new (int Phase, int TrueKeyCount, string TrueKeyListSha16)[]
{
    (330, 19, "fefb97c31a50574e"),
    (331, 14, "e03c3708666a4088"),
    (332, 19, "6cec6183aa92b58f"),
    (333, 20, "b24aa34f7ee62a01"),
    (334, 23, "c8ec258efc04923d"),
    (335, 25, "2f555cbb567e47f5"),
    (336, 23, "b0455fabc446ae0d"),
    (337, 25, "171b9fe6d92d8b3f"),
    (338, 36, "814c97980797e2fd"),
    (339, 34, "509cecb5f87a5e70"),
    (340, 31, "9792f2013d868652"),
    (341, 31, "7f8818b0d5f74edf"),
    (342, 37, "9463bb28a99622b2"),
    (343, 29, "ae0536267d1cbea6"),
    (344, 22, "5e34dcb99e255d4f"),
    (345, 20, "c2316083c8484dfd"),
};

// Generic equation-indicator net (neutral fragments only). Any TRUE key whose
// name matches this net must be covered by the inventory (fail-closed sweep).
var indicatorNet = new Regex("(Formula|MassTerm|MassStructure|MassRatio|MassParameter|Relation|Constraint|DependsOn|DependOn)", RegexOptions.CultureInvariant);

static string Sha16(string s)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
    return Convert.ToHexStringLower(bytes)[..16];
}

static string Sha12(string s)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
    return Convert.ToHexStringLower(bytes)[..12];
}

var coverageRows = new List<object>();
var coveragePassed = true;
var coverageDetail = new StringBuilder();

foreach (var (phase, expectedCount, expectedSha) in auditPins)
{
    var path = AuditPath(phase);
    if (!File.Exists(path))
    {
        coveragePassed = false;
        corpusFilesPresent = false;
        coverageDetail.Append($"p{phase}:missing-file;");
        coverageRows.Add(new { phase, path, present = false });
        continue;
    }

    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var trueKeys = doc.RootElement.EnumerateObject()
        .Where(p => p.Value.ValueKind == JsonValueKind.True)
        .Select(p => p.Name)
        .OrderBy(n => n, StringComparer.Ordinal)
        .ToArray();
    var listSha = Sha16(string.Join("\n", trueKeys));
    var keyHashes = trueKeys.ToDictionary(Sha12, k => k, StringComparer.Ordinal);

    var pinOk = trueKeys.Length == expectedCount && listSha == expectedSha;
    if (!pinOk)
    {
        coveragePassed = false;
        coverageDetail.Append($"p{phase}:freshness-pin-mismatch(count={trueKeys.Length}/{expectedCount},sha={listSha}/{expectedSha});");
    }

    var itemsForPhase = inventory.Where(i => i.CorpusPath == path).ToArray();
    var mappedHashes = itemsForPhase.Select(i => i.EvidenceRef).ToHashSet(StringComparer.Ordinal);

    // Every pre-registered evidence hash must exist among the file's true keys.
    var missingEvidence = mappedHashes.Where(h => !keyHashes.ContainsKey(h)).ToArray();
    if (missingEvidence.Length > 0)
    {
        coveragePassed = false;
        coverageDetail.Append($"p{phase}:evidence-key-absent[{string.Join(",", missingEvidence)}];");
    }

    // Every indicator-net key must be mapped by the inventory.
    var indicatorHashes = trueKeys.Where(k => indicatorNet.IsMatch(k)).Select(Sha12).ToArray();
    var unmapped = indicatorHashes.Where(h => !mappedHashes.Contains(h)).ToArray();
    if (unmapped.Length > 0)
    {
        coveragePassed = false;
        coverageDetail.Append($"p{phase}:unmapped-indicator-keys[{string.Join(",", unmapped)}];");
    }

    coverageRows.Add(new
    {
        phase,
        path,
        present = true,
        trueKeyCount = trueKeys.Length,
        trueKeyListSha16 = listSha,
        freshnessPinOk = pinOk,
        indicatorKeyCount = indicatorHashes.Length,
        unmappedIndicatorKeyCount = unmapped.Length,
        inventoryItemCount = itemsForPhase.Length,
        pinnedRowCount = itemsForPhase.Count(i => i.Classification == "pinned-row"),
        ambiguousCount = itemsForPhase.Count(i => i.Classification == "dimensionally-ambiguous"),
    });
}

checks.Add(new Check(
    "corpus-coverage-gate",
    coveragePassed,
    coveragePassed
        ? $"all {auditPins.Length} committed audit outputs pinned (key counts + sorted-true-key-list hashes) and every equation-indicator key is inventory-mapped"
        : coverageDetail.ToString()));

// ---------------------------------------------------------------------------
// 5. Pre-registered part-12d routing gate (machine-checkable criterion).
//    The criterion runs on the quoted committed text itself: a statement that
//    carries an equation glyph AND a numeric coefficient is PINNED; a
//    coefficient-free slot statement ROUTES to the anchor-reading-menu phase.
//    FAIL-CLOSED: the expected relation must either (a) be classified
//    pinned-row AND appear among the prescribed-reading breaking ids, or
//    (b) be classified routed-to-anchor-menu in agreement with the criterion.
// ---------------------------------------------------------------------------

var expectedItem = inventory.Single(i => i.ItemId == ExpectedBreakingRelationId);
var expectedQuote = expectedItem.EvidenceRef;
var quoteHasEquationGlyph = expectedQuote.Contains('=');
var quoteHasNumericCoefficient = expectedQuote.Any(char.IsDigit);
var criterionSaysPinned = quoteHasEquationGlyph && quoteHasNumericCoefficient;
var criterionExpectedClassification = criterionSaysPinned ? "pinned-row" : "routed-to-anchor-menu";
var routingClassificationMatchesCriterion = expectedItem.Classification == criterionExpectedClassification;

// Consistency: the criterion must agree with the classification of the two
// control quotes (the literal curvature-mass row must be pinned by the same
// criterion).
var controlItem = inventory.Single(i => i.ItemId == "sp12c-lit");
var controlPinned = controlItem.EvidenceRef.Contains('=') && controlItem.EvidenceRef.Any(char.IsDigit);
var criterionControlOk = controlPinned && controlItem.Classification == "pinned-row";

// ---------------------------------------------------------------------------
// 6. Build the constraint matrices and decide each admissible reading.
// ---------------------------------------------------------------------------

BigInteger[][] BuildMatrix(string matrixVariant, out string[] rowIds)
{
    var rows = new List<(string Id, BigInteger[] Row)>();
    foreach (var item in inventory)
    {
        var isKernelRow = item.Classification == "pinned-row";
        if (!isKernelRow)
        {
            continue;
        }

        // Matrix-variant selection for the curvature-mass pair: exactly one of
        // the literal/squared rows enters per variant.
        if (item.ItemId == "sp12c-lit" && matrixVariant != "literal")
        {
            continue;
        }

        if (item.ItemId == "sp12c-sq" && matrixVariant != "squared")
        {
            continue;
        }

        var row = new BigInteger[symbols.Length];
        foreach (var (sym, exp) in item.Exponents)
        {
            row[symbolIndex[sym]] = exp;
        }

        rows.Add((item.ItemId, row));
    }

    rowIds = rows.Select(r => r.Id).ToArray();
    return rows.Select(r => r.Row).ToArray();
}

var literalMatrix = BuildMatrix("literal", out var literalRowIds);
var squaredMatrix = BuildMatrix("squared", out var squaredRowIds);
var literalSnf = SmithNormalForm.Compute(literalMatrix);
var squaredSnf = SmithNormalForm.Compute(squaredMatrix);

// Kernel verification on the corpus matrices (battery-grade, fail-closed).
bool VerifyKernel(BigInteger[][] a, SnfResult snf)
{
    var rank = snf.Diagonal.Count(d => d != 0);
    if (snf.KernelBasis.Length != symbols.Length - rank)
    {
        return false;
    }

    foreach (var v in snf.KernelBasis)
    {
        foreach (var row in a)
        {
            var acc = BigInteger.Zero;
            for (var c = 0; c < symbols.Length; c++)
            {
                acc += row[c] * v[c];
            }

            if (acc != 0)
            {
                return false;
            }
        }
    }

    return true;
}

var corpusKernelVerified = VerifyKernel(literalMatrix, literalSnf) && VerifyKernel(squaredMatrix, squaredSnf);
checks.Add(new Check(
    "corpus-kernel-verification",
    corpusKernelVerified,
    $"literal: rank={literalSnf.Diagonal.Count(d => d != 0)}, nullity={literalSnf.KernelBasis.Length}; squared: rank={squaredSnf.Diagonal.Count(d => d != 0)}, nullity={squaredSnf.KernelBasis.Length}; every kernel basis vector annihilated exactly"));

var ambiguousIds = inventory
    .Where(i => i.Classification == "dimensionally-ambiguous")
    .Select(i => i.ItemId)
    .ToArray();

var readingResults = new List<ReadingResult>();
foreach (var (readingId, matrixVariant, ccGrade, description) in readingDefs)
{
    var matrix = matrixVariant == "literal" ? literalMatrix : squaredMatrix;
    var rowIds = matrixVariant == "literal" ? literalRowIds : squaredRowIds;
    var snf = matrixVariant == "literal" ? literalSnf : squaredSnf;
    var d = GradingVector(ccGrade);

    var breaking = new List<(string Id, BigInteger Residual)>();
    for (var r = 0; r < matrix.Length; r++)
    {
        var acc = BigInteger.Zero;
        for (var c = 0; c < symbols.Length; c++)
        {
            acc += matrix[r][c] * d[c];
        }

        if (acc != 0)
        {
            breaking.Add((rowIds[r], acc));
        }
    }

    var scaleVectorInKernel = breaking.Count == 0;
    var verdict = breaking.Count > 0
        ? "equivariance-breaking-relation-found"
        : ambiguousIds.Length > 0
            ? "corpus-dimensionally-ambiguous"
            : "scaling-symmetry-closes";

    readingResults.Add(new ReadingResult(
        readingId,
        description,
        matrixVariant,
        ccGrade,
        snf.Diagonal.Count(x => x != 0),
        snf.KernelBasis.Length,
        scaleVectorInKernel,
        scaleVectorInKernel ? 1 : 0,
        breaking.Select(b => b.Id).ToArray(),
        breaking.Select(b => $"{b.Id}:{b.Residual}").ToArray(),
        verdict,
        verdict == "corpus-dimensionally-ambiguous" ? ambiguousIds : []));
}

// Pre-registered taxonomy completeness: every reading must land on exactly
// one of the three pre-registered verdicts.
string[] allowedVerdicts = ["scaling-symmetry-closes", "equivariance-breaking-relation-found", "corpus-dimensionally-ambiguous"];
var taxonomyClosed = readingResults.All(r => allowedVerdicts.Contains(r.Verdict));
checks.Add(new Check(
    "pre-registered-verdict-taxonomy",
    taxonomyClosed,
    $"readings={readingResults.Count}; verdicts=[{string.Join(", ", readingResults.Select(r => $"{r.ReadingId}={r.Verdict}"))}]"));

// Part-12d gate final decision.
var prescribedReading = readingResults.Single(r => r.ReadingId == "R0-prescribed-literal");
var part12dIsBreakingHit = expectedItem.Classification == "pinned-row"
    && prescribedReading.BreakingRelationIds.Contains(ExpectedBreakingRelationId);
var part12dRoutedCorrectly = expectedItem.Classification == "routed-to-anchor-menu"
    && routingClassificationMatchesCriterion;
var part12dNotSilentKernelRow = !literalRowIds.Contains(ExpectedBreakingRelationId)
    && !squaredRowIds.Contains(ExpectedBreakingRelationId)
    || expectedItem.Classification == "pinned-row";
var part12dGatePassed = (part12dIsBreakingHit || part12dRoutedCorrectly)
    && part12dNotSilentKernelRow
    && criterionControlOk;

checks.Add(new Check(
    "part12d-pre-registered-routing-gate",
    part12dGatePassed,
    $"expectedRelation={ExpectedBreakingRelationId}; criterion(equationGlyph={quoteHasEquationGlyph},numericCoefficient={quoteHasNumericCoefficient})=>{criterionExpectedClassification}; classification={expectedItem.Classification}; appearsAsBreakingHit={part12dIsBreakingHit}; routedToAnchorMenu={part12dRoutedCorrectly}; neverSilentKernelRow={part12dNotSilentKernelRow}; controlQuotePinnedByCriterion={criterionControlOk}"));

// Blocking set is never dropped.
var blockingSetPreserved = ambiguousIds.Length > 0
    && readingResults.Where(r => r.BreakingRelationIds.Length == 0).All(r => r.Verdict == "corpus-dimensionally-ambiguous");
checks.Add(new Check(
    "ambiguous-blocking-set-preserved",
    blockingSetPreserved,
    $"blockingSetCount={ambiguousIds.Length}; no reading can reach scaling-symmetry-closes while the blocking set is non-empty"));

// ---------------------------------------------------------------------------
// 7. Verdict + output.
// ---------------------------------------------------------------------------

var hardGatesPassed = snfBatteryPassed && corpusFilesPresent && quoteEvidencePassed
    && coveragePassed && corpusKernelVerified && taxonomyClosed && part12dGatePassed
    && blockingSetPreserved;

var breakingReadings = readingResults.Where(r => r.Verdict == "equivariance-breaking-relation-found").ToArray();
var verdictKind = !hardGatesPassed
    ? "review-required-gates-failed"
    : breakingReadings.Any(r => r.ReadingId == "R0-prescribed-literal")
        ? "breaking-relation-found-under-prescribed-reading"
        : readingResults.All(r => r.Verdict == "scaling-symmetry-closes")
            ? "scaling-symmetry-closes-all-readings"
            : "corpus-dimensionally-ambiguous";

var unitsEquivarianceKernelAuditPassed = hardGatesPassed;
var terminalStatus = TerminalPrefix + verdictKind;

var decision = hardGatesPassed
    ? $"The units-equivariance kernel theorem is decided in exact integer arithmetic over {literalMatrix.Length} pinned grading-constraint rows in {symbols.Length} symbol grades extracted from the 16 committed literature-audit outputs (phase330-345, referred to by number) plus the committed reference-snapshot relations. PRESCRIBED READING (metric +2, connection -1, curvature -2, mass -1): the one-parameter scale family does NOT lie in the kernel - the single breaking row is the snapshot's literal first-power curvature-mass statement (residual +1), reproducing the committed phase420 dimensional-invalidity finding; every audited external-model mass relation is scale-homogeneous. SQUARED-REPAIR READING: all pinned rows close (scale family lies in the full SNF kernel, rank {squaredSnf.Diagonal.Count(x => x != 0)}, nullity {squaredSnf.KernelBasis.Length}) but the corpus-dimensionally-ambiguous BLOCKING set ({ambiguousIds.Length} prose-only relations, never dropped) forbids the closure verdict. CC RE-GRADED READINGS (mass-class -1 and density-class -4, the readings the CC-as-a-VEV slot itself suggests): the corpus's own pinned cc-bearing relations (p335-r2, p339-r1) BREAK - the anchor re-grading demanded by the pre-registered part-12d relation is units-inconsistent with the audited corpus. The part-12d relation itself is coefficient-free under the machine criterion and ROUTES to the phase461 anchor-reading menu (never a silent kernel row). ELIMINATION content only: no reading yields a units-equivariant corpus with an anchor-bearing scale; nothing is promoted."
    : "Review required: one or more fail-closed gates (battery, corpus freshness pins, coverage, routing, taxonomy) did not pass; no verdict is emitted.";

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var result = new
{
    phaseId = "phase460-source-corpus-units-equivariance-kernel",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    unitsEquivarianceKernelAuditPassed,
    targetBlindConstruction = true,
    applicationSubjectKind = ApplicationSubjectKind,
    verdictKind,
    preRegisteredExpectedBreakingRelationId = ExpectedBreakingRelationId,
    part12dRoutingGate = new
    {
        passed = part12dGatePassed,
        quoteHasEquationGlyph,
        quoteHasNumericCoefficient,
        criterionExpectedClassification,
        actualClassification = expectedItem.Classification,
        appearsAsBreakingHitUnderPrescribedReading = part12dIsBreakingHit,
        routedToAnchorMenu = part12dRoutedCorrectly,
        neverSilentKernelRow = part12dNotSilentKernelRow,
        routedToPhase = "phase461",
        hypotheticalResidualIfPinnedUnderPrescribedReading = "+1 (cc term grade -2 vs mass-class right-hand side grade -1)",
    },
    gradingConventions = new
    {
        prescribedBase = new { metricG = 2, connOmega = -1, curvFAndCurvR = -2, massClass = -1 },
        canonicalExtensions = new { scalarAndVevClass = -1, potentialDensity = -4, inverseGravCouplingSquaredSymbol = 2, compactAndWarpRadii = 1, dimensionlessCouplings = 0 },
        ccTermReadings = new[] { -2, -1, -4 },
        gradingConventionsAreWorkbenchConventions = true,
    },
    symbols,
    pinnedRowCount = literalMatrix.Length,
    readings = readingResults.Select(r => new
    {
        readingId = r.ReadingId,
        description = r.Description,
        matrixVariant = r.MatrixVariant,
        ccGrade = r.CcGrade,
        kernelRankValue = r.KernelRank,
        kernelNullityValue = r.KernelNullity,
        scaleVectorInKernel = r.ScaleVectorInKernel,
        scaleFamilyIntersectionDim = r.ScaleFamilyIntersectionDim,
        breakingRelationIds = r.BreakingRelationIds,
        breakingResiduals = r.BreakingResiduals,
        verdict = r.Verdict,
        blockingAmbiguousIds = r.BlockingAmbiguousIds,
    }).ToArray(),
    inventory = inventory.Select(i => new
    {
        itemId = i.ItemId,
        corpusPath = i.CorpusPath,
        evidenceKind = i.EvidenceKind,
        evidenceRef = i.EvidenceRef,
        classification = i.Classification,
        exponents = i.Exponents,
        note = i.Note,
    }).ToArray(),
    corpusDimensionallyAmbiguousBlockingSet = ambiguousIds,
    coverage = coverageRows,
    snfBattery = new { passed = snfBatteryPassed, cases = snfBatteryCases },
    checks = checks.Select(c => new { checkId = c.Id, passed = c.Passed, detail = c.Detail }).ToArray(),
    physicistReviewPending = true,
    noGevPromotion = true,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    decision,
    sourceEvidence = new
    {
        mirrorSnapshotPath = MirrorSnapshotPath,
        auditOutputPaths = auditPins.Select(p => AuditPath(p.Phase)).ToArray(),
        programSpecPath = "docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md",
        registryPath = "docs/Phases/PHASE_NUMBER_REGISTRY.md",
    },
};

File.WriteAllText(Path.Combine(outputDir, "source_corpus_units_equivariance_kernel.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_corpus_units_equivariance_kernel_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.unitsEquivarianceKernelAuditPassed,
        result.targetBlindConstruction,
        result.applicationSubjectKind,
        result.verdictKind,
        result.preRegisteredExpectedBreakingRelationId,
        result.part12dRoutingGate,
        result.pinnedRowCount,
        corpusDimensionallyAmbiguousBlockingSetCount = ambiguousIds.Length,
        readings = result.readings,
        snfBatteryPassed,
        result.checks,
        result.physicistReviewPending,
        result.noGevPromotion,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"unitsEquivarianceKernelAuditPassed={unitsEquivarianceKernelAuditPassed}");
Console.WriteLine($"verdictKind={verdictKind}");
Console.WriteLine($"pinnedRowCount={literalMatrix.Length}; symbolCount={symbols.Length}; blockingSetCount={ambiguousIds.Length}");
foreach (var r in readingResults)
{
    Console.WriteLine($"{r.ReadingId}: verdict={r.Verdict}; kernelRank={r.KernelRank}; nullity={r.KernelNullity}; scaleVectorInKernel={r.ScaleVectorInKernel}; breaking=[{string.Join(",", r.BreakingRelationIds)}]");
}

Console.WriteLine($"part12dRoutingGatePassed={part12dGatePassed} (classification={expectedItem.Classification})");
foreach (var c in checks)
{
    Console.WriteLine($"check {c.Id}: {(c.Passed ? "passed" : "FAILED")} - {c.Detail}");
}

if (!hardGatesPassed)
{
    Environment.Exit(1);
}

internal sealed record Check(string Id, bool Passed, string Detail);

internal sealed record InventoryItem(
    string ItemId,
    string CorpusPath,
    string EvidenceKind,
    string EvidenceRef,
    string Classification,
    Dictionary<string, int> Exponents,
    string Note);

internal sealed record ReadingResult(
    string ReadingId,
    string Description,
    string MatrixVariant,
    int CcGrade,
    int KernelRank,
    int KernelNullity,
    bool ScaleVectorInKernel,
    int ScaleFamilyIntersectionDim,
    string[] BreakingRelationIds,
    string[] BreakingResiduals,
    string Verdict,
    string[] BlockingAmbiguousIds);

internal sealed record SnfResult(BigInteger[] Diagonal, BigInteger[][] KernelBasis);

/// <summary>
/// Exact integer Smith normal form over BigInteger with right-transform
/// tracking (D = U * A * V with U, V unimodular; U is not materialized).
/// The integer kernel of A is spanned by the columns of V whose index is at
/// least rank(D), because A * V has those columns identically zero and V is
/// unimodular.
/// </summary>
internal static class SmithNormalForm
{
    public static SnfResult Compute(BigInteger[][] input)
    {
        var rows = input.Length;
        var cols = rows == 0 ? 0 : input[0].Length;
        var a = input.Select(r => (BigInteger[])r.Clone()).ToArray();

        // V starts as the identity; every column operation on A is mirrored on V.
        var v = new BigInteger[cols][];
        for (var i = 0; i < cols; i++)
        {
            v[i] = new BigInteger[cols];
            v[i][i] = BigInteger.One;
        }

        var t = 0;
        var limit = Math.Min(rows, cols);
        while (t < limit)
        {
            // Find a pivot: the nonzero entry of smallest absolute value in the
            // trailing submatrix.
            var pr = -1;
            var pc = -1;
            BigInteger best = 0;
            for (var r = t; r < rows; r++)
            {
                for (var c = t; c < cols; c++)
                {
                    var x = BigInteger.Abs(a[r][c]);
                    if (x != 0 && (pr < 0 || x < best))
                    {
                        best = x;
                        pr = r;
                        pc = c;
                    }
                }
            }

            if (pr < 0)
            {
                break; // trailing submatrix is zero
            }

            SwapRows(a, t, pr);
            SwapCols(a, v, t, pc);

            var clean = true;

            // Clear column t below the pivot by Euclidean row reduction.
            for (var r = t + 1; r < rows; r++)
            {
                if (a[r][t] == 0)
                {
                    continue;
                }

                var q = BigInteger.Divide(a[r][t], a[t][t]);
                if (q != 0)
                {
                    for (var c = t; c < cols; c++)
                    {
                        a[r][c] -= q * a[t][c];
                    }
                }

                if (a[r][t] != 0)
                {
                    SwapRows(a, t, r);
                    clean = false;
                }
            }

            // Clear row t right of the pivot by Euclidean column reduction.
            for (var c = t + 1; c < cols; c++)
            {
                if (a[t][c] == 0)
                {
                    continue;
                }

                var q = BigInteger.Divide(a[t][c], a[t][t]);
                if (q != 0)
                {
                    for (var r = 0; r < rows; r++)
                    {
                        a[r][c] -= q * a[r][t];
                    }

                    for (var k = 0; k < cols; k++)
                    {
                        v[k][c] -= q * v[k][t];
                    }
                }

                if (a[t][c] != 0)
                {
                    SwapCols(a, v, t, c);
                    clean = false;
                }
            }

            if (!clean)
            {
                continue; // repeat elimination at the same pivot index
            }

            // Divisibility repair: if any trailing entry is not divisible by the
            // pivot, fold its column into column t and redo the elimination.
            var repaired = false;
            for (var r = t + 1; r < rows && !repaired; r++)
            {
                for (var c = t + 1; c < cols && !repaired; c++)
                {
                    if (a[r][c] % a[t][t] != 0)
                    {
                        for (var rr = 0; rr < rows; rr++)
                        {
                            a[rr][t] += a[rr][c];
                        }

                        for (var k = 0; k < cols; k++)
                        {
                            v[k][t] += v[k][c];
                        }

                        repaired = true;
                    }
                }
            }

            if (repaired)
            {
                continue;
            }

            if (a[t][t] < 0)
            {
                for (var c = t; c < cols; c++)
                {
                    a[t][c] = -a[t][c];
                }
            }

            t++;
        }

        var diag = new BigInteger[limit];
        for (var i = 0; i < limit; i++)
        {
            diag[i] = a[i][i] < 0 ? -a[i][i] : a[i][i];
        }

        var rank = diag.Count(d => d != 0);
        var kernel = new BigInteger[cols - rank][];
        for (var j = rank; j < cols; j++)
        {
            var col = new BigInteger[cols];
            for (var k = 0; k < cols; k++)
            {
                col[k] = v[k][j];
            }

            kernel[j - rank] = col;
        }

        return new SnfResult(diag, kernel);
    }

    private static void SwapRows(BigInteger[][] a, int i, int j)
    {
        if (i == j)
        {
            return;
        }

        (a[i], a[j]) = (a[j], a[i]);
    }

    private static void SwapCols(BigInteger[][] a, BigInteger[][] v, int i, int j)
    {
        if (i == j)
        {
            return;
        }

        foreach (var row in a)
        {
            (row[i], row[j]) = (row[j], row[i]);
        }

        foreach (var row in v)
        {
            (row[i], row[j]) = (row[j], row[i]);
        }
    }
}
