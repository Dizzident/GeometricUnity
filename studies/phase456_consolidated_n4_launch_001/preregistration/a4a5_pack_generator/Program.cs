using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// =====================================================================================
// phase456 PRE-REGISTRATION PACK GENERATOR -- A4 + A5 Stage-A (WAVE2 item 9 / standing
// item 12 & 14). Standalone deterministic tooling; NO HMC, NO sampling. Emits the
// hash-pinned pre-registration pack into the parent preregistration/ directory. The
// phase456 production run only ever READS the committed pack and hash-gates on it.
//
// A4 Stage-A (analytic + exact):
//   * enumerate the realized spacetime symmetries of the committed n=4 lattice-canonical
//     (Coxeter-Freudenthal-Kuhn) torus action S_B: the translation group (Z_4)^4 (already
//     established exactly covariant by phase448) and the point symmetries (signed
//     permutations B_4) that ALSO commute with S_B. Realization is decided TWO ways that
//     must agree: (i) combinatorial -- the induced vertex map is a simplicial automorphism
//     (preserves the edge/face/cell sets); (ii) numeric -- S_B(pushforward) == S_B to a
//     tight gate. The pushforward carries the phase448-documented Faces[f][0]/edge
//     orientation gauge (stored index order is not translation-covariant; handled by an
//     orientation sign at the operator boundary, exactly as phase448/452).
//   * derive the rest-frame little group H_s (point elements fixing the Euclidean-time
//     axis 0 forward) and its irreps; channel labels come from REALIZED irreps ONLY.
//     Parity (0-+ / pseudoscalar) is admissible IFF a realized improper element implements
//     spatial inversion; the Kuhn body-diagonal is not preserved by any spatial reflection,
//     so this is expected FALSE => 0-+-like labels BANNED.
//   * exact-rational projector kernels over the face-type interpolator space for the
//     identity-irrep 2x2 GEVP {O1,O2} and (at least) one non-identity realized channel.
//   * k_min = 2*pi/4 dispersion-row spec; it needs per-site (un-slice-summed) correlators,
//     so the per-site storage flag is written into the pack (mandatory).
//
// A5 Stage-A is worked analytically in AttemptA5GaussianDomination() and committed as a
// verdict string with the structural facts of the exactly-quartic S_B even sector.
//
// MANDATORY FRAMING. Workbench-relative STRUCTURE data only (su(2) toy algebra, reduced
// Spin(4) slice, lattice units). Nothing measured or promoted; promotedPhysicalMassClaimCount
// stays 0. This tool computes group theory + projector coefficients, not physics numbers.
// =====================================================================================

const int N = 4;                       // committed launch volume
const int TimeAxis = 0;                // documented convention: lattice axis 0 = Euclidean time
const double SbInvarianceGate = 1e-9;  // S_B(pushforward) vs S_B relative gate
const int RngSeed = 20260712;

string outputDir = Environment.GetEnvironmentVariable("P456_PACK_DIR")
    ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
// The generator lives at preregistration/a4a5_pack_generator/bin/<cfg>/net10.0/; four
// levels up is the preregistration/ directory. Allow override for reproducibility.
if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

Console.WriteLine($"[a4a5] pack output dir: {outputDir}");

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
var manifest = BuildManifest();
var geometry = BuildGeometry();

var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(N, latticeCanonical: true);
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;

// --- integer lattice coordinates + lookups ------------------------------------------
var coords = new int[mesh.VertexCount][];
double spacing = double.MaxValue;
for (int v = 0; v < mesh.VertexCount; v++)
{
    var p = mesh.GetVertexCoordinates(v);
    for (int d = 0; d < 4; d++) if (p[d] > 1e-12) spacing = System.Math.Min(spacing, p[d]);
    coords[v] = new int[4];
}
for (int v = 0; v < mesh.VertexCount; v++)
{
    var p = mesh.GetVertexCoordinates(v);
    for (int d = 0; d < 4; d++) coords[v][d] = (int)System.Math.Round(p[d] / spacing);
}
var vertexAt = new Dictionary<(int, int, int, int), int>();
for (int v = 0; v < mesh.VertexCount; v++)
    vertexAt[(coords[v][0], coords[v][1], coords[v][2], coords[v][3])] = v;

// edge set / index by sorted pair (+ stored orientation)
var edgeIndexByPair = new Dictionary<(int, int), int>();
for (int e = 0; e < mesh.EdgeCount; e++)
{
    int a = mesh.Edges[e][0], b = mesh.Edges[e][1];
    edgeIndexByPair[(System.Math.Min(a, b), System.Math.Max(a, b))] = e;
}
// face set by sorted triple
var faceIndexByTriple = new Dictionary<(int, int, int), int>();
for (int f = 0; f < mesh.FaceCount; f++)
{
    var vs = mesh.Faces[f].OrderBy(x => x).ToArray();
    faceIndexByTriple[(vs[0], vs[1], vs[2])] = f;
}
// cell set by sorted 5-tuple
var cellSet = new HashSet<string>();
for (int c = 0; c < mesh.CellCount; c++)
    cellSet.Add(string.Join(",", mesh.CellVertices[c].OrderBy(x => x)));

// --- face canonicalization (verbatim phase452 conventions) --------------------------
int TypeOf(int[] disp) => ((disp[0] << 3) | (disp[1] << 2) | (disp[2] << 1) | disp[3]) - 1;
var faceTypeId = new int[mesh.FaceCount];
var faceBase = new int[mesh.FaceCount];
var faceTypeKeyToId = new Dictionary<(int, int), int>();
for (int f = 0; f < mesh.FaceCount; f++)
{
    var verts = mesh.Faces[f];
    int baseV = -1; int[]? dA = null, dB = null;
    for (int i = 0; i < verts.Length; i++)
    {
        int cand = verts[i];
        int[]? d1 = null, d2 = null; bool ok = true; int slot = 0;
        for (int j = 0; j < verts.Length; j++)
        {
            if (j == i) continue;
            var d = MinImage01(coords[cand], coords[verts[j]], N);
            if (d == null) { ok = false; break; }
            if (slot++ == 0) d1 = d; else d2 = d;
        }
        if (ok && d1 != null && d2 != null) { baseV = cand; dA = d1; dB = d2; }
    }
    if (baseV < 0 || dA == null || dB == null) throw new InvalidOperationException("face canonicalization failed");
    var (dLo, dHi) = dA.Sum() <= dB.Sum() ? (dA, dB) : (dB, dA);
    var key = (TypeOf(dLo), TypeOf(dHi));
    if (!faceTypeKeyToId.TryGetValue(key, out int tid)) { tid = faceTypeKeyToId.Count; faceTypeKeyToId[key] = tid; }
    faceTypeId[f] = tid; faceBase[f] = baseV;
}
int faceTypeCount = faceTypeKeyToId.Count;
var repFace = new int[faceTypeCount];        // one representative face index per type
var repFacePresent = new bool[faceTypeCount];
for (int f = 0; f < mesh.FaceCount; f++)
    if (!repFacePresent[faceTypeId[f]]) { repFace[faceTypeId[f]] = f; repFacePresent[faceTypeId[f]] = true; }

// --- operator + mass for the numeric S_B-invariance battery (identity control member) --
var controlMember = new EinsteinianShiabFamilyMember
{ Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None, EpsilonMode = "trivial" };
var op = new EinsteinianShiabOperator(mesh, algebra, controlMember, latticePeriod: N);
var mass = new CpuMassMatrix(mesh, algebra);
double SbOf(double[] omega) => op.ComputeJointGradient(omega, new double[nTheta], mass).Objective;

var rng = new Random(RngSeed);
var probeOmega = new double[3][];
for (int r = 0; r < probeOmega.Length; r++)
{
    var w = new double[nOmega];
    for (int i = 0; i < nOmega; i++) w[i] = rng.NextDouble() - 0.5;
    probeOmega[r] = w;
}
var probeSb = probeOmega.Select(SbOf).ToArray();

// --- enumerate B_4 candidate point maps (signed permutations of 4 axes) -------------
// A candidate is a 4x4 signed-permutation matrix A: (A x)_i = eps[i] * x[perm[i]].
// Vertex map phi(v) = vertexAt[ (A * coords[v]) mod N ]. Origin -> origin.
var perms = Permutations4();
var realized = new List<SymElement>();
var candidateCount = 0;
foreach (var perm in perms)
    for (int signMask = 0; signMask < 16; signMask++)
    {
        candidateCount++;
        var A = new int[4, 4];
        var eps = new int[4];
        for (int i = 0; i < 4; i++) { eps[i] = ((signMask >> i) & 1) == 0 ? 1 : -1; A[i, perm[i]] = eps[i]; }

        // induced vertex map
        var phi = new int[mesh.VertexCount];
        bool bij = true;
        var hit = new bool[mesh.VertexCount];
        for (int v = 0; v < mesh.VertexCount && bij; v++)
        {
            var x = coords[v];
            int[] y = new int[4];
            for (int i = 0; i < 4; i++)
            {
                int s = 0;
                for (int j = 0; j < 4; j++) s += A[i, j] * x[j];
                y[i] = Mod(s, N);
            }
            int w = vertexAt[(y[0], y[1], y[2], y[3])];
            phi[v] = w;
            if (hit[w]) { bij = false; } else hit[w] = true;
        }
        if (!bij) continue;

        // (i) combinatorial simplicial automorphism: edges, faces, cells preserved
        bool combOk = true;
        for (int e = 0; e < mesh.EdgeCount && combOk; e++)
        {
            int a = phi[mesh.Edges[e][0]], b = phi[mesh.Edges[e][1]];
            if (!edgeIndexByPair.ContainsKey((System.Math.Min(a, b), System.Math.Max(a, b)))) combOk = false;
        }
        for (int f = 0; f < mesh.FaceCount && combOk; f++)
        {
            var t = new[] { phi[mesh.Faces[f][0]], phi[mesh.Faces[f][1]], phi[mesh.Faces[f][2]] }.OrderBy(x => x).ToArray();
            if (!faceIndexByTriple.ContainsKey((t[0], t[1], t[2]))) combOk = false;
        }
        for (int c = 0; c < mesh.CellCount && combOk; c++)
        {
            var key = string.Join(",", mesh.CellVertices[c].Select(x => phi[x]).OrderBy(x => x));
            if (!cellSet.Contains(key)) combOk = false;
        }
        if (!combOk) continue;

        // (ii) numeric S_B invariance under the pushforward connection (orientation gauge)
        double sbResid = 0.0;
        for (int r = 0; r < probeOmega.Length; r++)
        {
            var wPrime = new double[nOmega];
            for (int e = 0; e < mesh.EdgeCount; e++)
            {
                int v0 = mesh.Edges[e][0], v1 = mesh.Edges[e][1];
                int p0 = phi[v0], p1 = phi[v1];
                int eImg = edgeIndexByPair[(System.Math.Min(p0, p1), System.Math.Max(p0, p1))];
                double sgn = (mesh.Edges[eImg][0] == p0 && mesh.Edges[eImg][1] == p1) ? 1.0 : -1.0;
                for (int l = 0; l < dimG; l++) wPrime[eImg * dimG + l] = sgn * probeOmega[r][e * dimG + l];
            }
            double sb = SbOf(wPrime);
            sbResid = System.Math.Max(sbResid, System.Math.Abs(sb - probeSb[r]) / System.Math.Max(1e-30, System.Math.Abs(probeSb[r])));
        }
        bool sbOk = sbResid <= SbInvarianceGate;

        // classification
        int det4 = SignedPermDet(A);
        bool fixesTimeFwd = A[TimeAxis, TimeAxis] == 1;               // A e_time = +e_time
        bool timeReflect = A[TimeAxis, TimeAxis] == -1 && Enumerable.Range(0, 4).All(i => i == TimeAxis || A[TimeAxis, i] == 0); // A e_time = -e_time
        int spatialDet = SpatialDet(A);                              // det of the 3x3 spatial block if time is fixed setwise

        realized.Add(new SymElement(A, eps, perm, det4, fixesTimeFwd, timeReflect, spatialDet, combOk, sbOk, sbResid));
    }

// combinatorial and numeric verdicts must agree on the realized set
bool realizationConsistent = realized.All(s => s.SbOk);
double sbResidWorst = realized.Count == 0 ? 0.0 : realized.Max(s => s.SbResid);

// realized point group H (all realized elements are, by construction, simplicial automorphisms)
var H = realized.Select(s => s.A).ToList();
bool closedGroup = IsClosedGroup(H);

// rest-frame little group H_s = elements fixing the time axis forward
var HsElems = realized.Where(s => s.FixesTimeFwd).Select(s => s.A).ToList();
bool hsClosed = IsClosedGroup(HsElems);
int hsOrder = HsElems.Count;

// time reflection realized? spatial inversion realized (=> parity admissible)?
bool timeReflectionRealized = realized.Any(s => s.TimeReflect);
var spatialInversion = new int[4, 4]; spatialInversion[TimeAxis, TimeAxis] = 1;
for (int i = 0; i < 4; i++) if (i != TimeAxis) spatialInversion[i, i] = -1;
bool spatialInversionRealized = H.Any(a => MatEq(a, spatialInversion));
// any realized H_s element acting improperly on the 3 spatial axes (det spatial block = -1)
bool anyImproperSpatial = realized.Any(s => s.FixesTimeFwd && s.SpatialDet == -1);
bool parityLabelAdmissible = spatialInversionRealized;

// --- group theory on H_s: conjugacy classes, commutator subgroup, +/-1 characters ----
int nClasses = ConjugacyClassCount(HsElems);
var commutator = CommutatorSubgroup(HsElems);
int abelianizationOrder = hsOrder / commutator.Count;
var pm1Characters = PlusMinusCharacters(HsElems);   // list of char vectors aligned to HsElems order
// identity char index and a nontrivial char index
int nontrivialCharIdx = -1;
for (int i = 0; i < pm1Characters.Count; i++)
    if (pm1Characters[i].Any(v => v == -1)) { nontrivialCharIdx = i; break; }

// irrep dimension multiset: #linear = abelianizationOrder (each dim 1); remaining classes carry
// higher dims with sum of squares = |H_s| - abelianizationOrder.
int higherSqSum = hsOrder - abelianizationOrder;
int higherCount = nClasses - abelianizationOrder;

// --- face-type permutation representation of H_s + exact-rational isotypic projectors --
// rho(g)[t'] = type of phi_g(repFace[t]); permutation of the faceTypeCount types.
var HsSymElems = realized.Where(s => s.FixesTimeFwd).ToList();
var typePerm = new int[HsSymElems.Count][];
bool typePermOk = true;
for (int gi = 0; gi < HsSymElems.Count; gi++)
{
    var A = HsSymElems[gi].A;
    var phi = VertexMap(A);
    var pmt = new int[faceTypeCount];
    for (int t = 0; t < faceTypeCount; t++)
    {
        var img = mesh.Faces[repFace[t]].Select(x => phi[x]).OrderBy(x => x).ToArray();
        if (!faceIndexByTriple.TryGetValue((img[0], img[1], img[2]), out int fImg)) { typePermOk = false; break; }
        pmt[t] = faceTypeId[fImg];
    }
    typePerm[gi] = pmt;
}

// identity-isotypic and nontrivial-char isotypic projectors: P_chi = (1/|H_s|) sum_g chi(g) rho(g)
// entries are integers / |H_s| (exact rational). rho(g) permutation matrix: rho(g)[pmt[j], j] = 1.
RationalMatrix ProjectorForChar(int[] chi)
{
    var num = new long[faceTypeCount, faceTypeCount];
    for (int gi = 0; gi < HsSymElems.Count; gi++)
    {
        var pmt = typePerm[gi];
        int c = chi[gi];
        for (int j = 0; j < faceTypeCount; j++) num[pmt[j], j] += c;   // row=image, col=source
    }
    return new RationalMatrix(num, hsOrder);
}
// character orders align: pm1Characters entries are aligned to HsElems; map to HsSymElems order.
// HsElems and HsSymElems index the same matrices in the same order, so reuse directly.
var idChar = Enumerable.Repeat(1, HsSymElems.Count).ToArray();
var pId = ProjectorForChar(idChar);
int identityTypeOrbits = MatrixRank01Orbits(typePerm, faceTypeCount);  // # H_s orbits on face types = mult of trivial

RationalMatrix? pNonId = null;
int nonIdMultiplicity = 0;
int[]? nonIdChar = null;
if (nontrivialCharIdx >= 0)
{
    nonIdChar = pm1Characters[nontrivialCharIdx];
    pNonId = ProjectorForChar(nonIdChar);
    // multiplicity of chi in the perm rep = (1/|H_s|) sum_g chi(g) fix(g)
    long acc = 0;
    for (int gi = 0; gi < HsSymElems.Count; gi++)
    {
        int fix = 0; var pmt = typePerm[gi];
        for (int t = 0; t < faceTypeCount; t++) if (pmt[t] == t) fix++;
        acc += (long)nonIdChar[gi] * fix;
    }
    nonIdMultiplicity = (int)(acc / hsOrder);
}

// a concrete non-identity kernel row (first nonzero row of P_nonId), as exact rationals
int[]? nonIdKernelNumerators = null;
if (pNonId != null && nonIdMultiplicity > 0)
    for (int i = 0; i < faceTypeCount; i++)
    {
        bool anyNz = false;
        for (int j = 0; j < faceTypeCount; j++) if (pNonId.Num[i, j] != 0) anyNz = true;
        if (anyNz) { nonIdKernelNumerators = Enumerable.Range(0, faceTypeCount).Select(j => (int)pNonId.Num[i, j]).ToArray(); break; }
    }

// identity 2x2 GEVP kernel: the trivial (all-ones) type weight -> uniform slice sum for O1 (F^2)
// and O2 (Upsilon^2). Exact rational uniform weight 1/faceTypeCount over types.
var identityGevpKernelNumerators = Enumerable.Repeat(1, faceTypeCount).ToArray();
int identityGevpKernelDenominator = faceTypeCount;

// --- k_min = 2*pi/4 dispersion-row spec (needs per-site storage) --------------------
// k_min per spatial axis = 2*pi/N = 2*pi/4 = pi/2; phases e^{-i k.x} are 4th roots of unity.
var kMinPhases = new object[N];
for (int x = 0; x < N; x++)
    kMinPhases[x] = new { latticeCoord = x, reNum = (int)System.Math.Round(System.Math.Cos(-2 * System.Math.PI * x / N)), imNum = (int)System.Math.Round(System.Math.Sin(-2 * System.Math.PI * x / N)) };

// =====================================================================================
// A5 Stage-A: Gaussian-domination pre-theorem attempt.
// =====================================================================================
var a5 = AttemptA5GaussianDomination();

// =====================================================================================
// Serialize the pack.
// =====================================================================================
var jsonOpts = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var a4 = new
{
    packItem = "A4_stage_a_symmetry_irrep_projectors",
    generatedBy = "phase456 preregistration a4a5_pack_generator (deterministic; no HMC)",
    volumeN = N,
    timeAxis = TimeAxis,
    latticeCanonical = true,
    triangulation = "Coxeter-Freudenthal-Kuhn 4-cube (24 axis-ordered simplices per hypercube)",
    lieAlgebra = "su2-trace-pairing",
    realizationMethod = new
    {
        combinatorial = "induced vertex map is a simplicial automorphism (edge + face + cell sets preserved)",
        numeric = "S_B(pushforward connection) == S_B, orientation-gauge carried at the operator boundary (phase448/452 Faces[f][0] non-covariance record)",
        sbInvarianceGate = SbInvarianceGate,
        sbResidualWorst = sbResidWorst,
        combinatorialAndNumericAgree = realizationConsistent,
        probeOmegaCount = probeOmega.Length,
    },
    translationGroup = new
    {
        group = "(Z_4)^4",
        order = mesh.VertexCount,
        realized = true,
        note = "established exactly translation-covariant by phase448 (n=4 covariance 1.4e-14); momentum labels k in (Z_4)^4",
    },
    candidatePointGroup = new { group = "B_4 signed permutations of 4 axes", order = candidateCount },
    realizedPointGroup = new
    {
        order = H.Count,
        isClosedGroup = closedGroup,
        properCount = realized.Count(s => s.Det4 == 1),
        improperCount = realized.Count(s => s.Det4 == -1),
        derivation = "signed permutations preserving the Kuhn body-diagonal structure; a signed permutation preserves the CFK triangulation iff it preserves the distinguished long diagonal (1,1,1,1) up to sign -> all-equal sign patterns only (pure axis permutations S_4 and the central element), consistent with the enumerated realized set",
    },
    restFrameLittleGroup = new
    {
        definition = "point elements fixing the Euclidean-time axis 0 forward (A e_time = +e_time)",
        order = hsOrder,
        isClosedGroup = hsClosed,
        conjugacyClassCount = nClasses,
        irrepCount = nClasses,
        abelianizationOrder = abelianizationOrder,
        linearIrrepCount = abelianizationOrder,
        realRealizedPlusMinusCharacterCount = pm1Characters.Count,
        higherDimIrrepCount = higherCount,
        higherDimSquareSum = higherSqSum,
        faceTypePermutationRepValid = typePermOk,
    },
    parity = new
    {
        spatialInversionRealized,
        parityLabelAdmissible,
        anyImproperSpatialElementRealized = anyImproperSpatial,
        timeReflectionRealized,
        ruling = parityLabelAdmissible
            ? "spatial inversion realized: parity is a good quantum number"
            : "NO realized improper element implements spatial inversion (Kuhn body-diagonal broken by spatial reflection); parity (P) is NOT a good quantum number -> pseudoscalar '0-+-like' channel labels are BANNED; channels labeled by realized irreps only",
        improperSpatialCaveat = "axis-swap elements act with 3D spatial determinant -1 but do NOT implement spatial inversion; they are cubic/permutation irreps of H_s, not a parity operation, and confer no P label",
    },
    irrepChannelTable = new
    {
        note = "channel labels from REALIZED irreps of H_s ONLY; no J^{PC} parity label emitted (parity inadmissible)",
        faceTypeCount,
        trivialIrrep = new { label = "A1-like (scalar 0++-analogue singlet)", faceTypeMultiplicity = identityTypeOrbits, projectorDenominator = pId.Denominator },
        nonTrivialLinearIrrep = nontrivialCharIdx >= 0 ? new
        {
            label = "A2-like (sign character of H_s; realized, non-parity)",
            faceTypeMultiplicity = nonIdMultiplicity,
            realizableFromFaceTypeInterpolator = nonIdMultiplicity > 0,
            projectorDenominator = pNonId?.Denominator ?? hsOrder,
        } : null,
    },
    projectorKernels = new
    {
        identityIrrep2x2Gevp = new
        {
            description = "identity-irrep (A1) zero-spatial-momentum kernel for the 2x2 GEVP variational basis {O1=Tr(F^2), O2=Tr(Upsilon^2)}; both interpolators use the trivial (all-ones) face-type weight -> the covariant time-slice sum; exact rational uniform weight",
            faceTypeWeightNumerators = identityGevpKernelNumerators,
            faceTypeWeightDenominator = identityGevpKernelDenominator,
            gevp = "solve C(t) v = lambda C(t0) v on the 2x2 {O1,O2} connected correlator matrix; ground-state a*m from the dominant generalized eigenvalue; t0 pinned mechanically (see manifest AIC rule)",
            isotypicProjectorNumerators = pId.RowMajorNumerators(),
            isotypicProjectorDenominator = pId.Denominator,
        },
        nonIdentityChannel = nonIdKernelNumerators != null ? new
        {
            description = "A2-like realized channel kernel over face types: P_chi = (1/|H_s|) sum_g chi(g) rho_facetype(g); exact rational (integer numerators over |H_s|). Requires per-face-type (=> per-site) resolution, NOT the plain slice sum.",
            characterValuesAlignedToHs = (int[]?)nonIdChar,
            kernelRowNumerators = (int[]?)nonIdKernelNumerators,
            kernelDenominator = pNonId?.Denominator ?? hsOrder,
            isotypicProjectorNumerators = pNonId?.RowMajorNumerators(),
            isotypicProjectorDenominator = pNonId?.Denominator ?? hsOrder,
        } : new
        {
            description = "no realized non-identity linear channel is contained in the face-type interpolator representation; the higher-dimensional realized irreps require the Stage-B 2-D irrep row structure (isotypic projector recorded; per-channel GEVP deferred)",
            characterValuesAlignedToHs = (int[]?)null,
            kernelRowNumerators = (int[]?)null,
            kernelDenominator = hsOrder,
            isotypicProjectorNumerators = (long[]?)null,
            isotypicProjectorDenominator = hsOrder,
        },
    },
    dispersionRow = new
    {
        kMin = "2*pi/4 (one spatial momentum unit on the n=4 torus; = pi/2 per spatial axis)",
        spec = "O(k,tau) = sum over faces in slice tau of e^{-i k . x_base(f)} * density(f); measure a*m(k_min) and check the lattice cosh dispersion vs the k=0 gap",
        requiresPerSiteCorrelators = true,
        perSiteStorageFlag = "PHASE456_STORE_PER_SITE_CORRELATORS",
        phaseFactorsAreExact = "4th roots of unity {1, -i, -1, i}",
        kMinPhasesPerLatticeCoord = kMinPhases,
    },
    storageFlags = new
    {
        perSiteCorrelatorStorage = new
        {
            flag = "PHASE456_STORE_PER_SITE_CORRELATORS",
            mandatory = true,
            reason = "the non-identity channel projector and the k_min dispersion row both require un-slice-summed per-site (per-face) correlators; committed phase452 outputs are slice-summed, so retroactive extraction is impossible (standing item 12)",
        },
    },
    mandatoryFraming = "workbench-relative structure data only; su(2) toy algebra, reduced Spin(4) slice, lattice units; nothing promoted; promotedPhysicalMassClaimCount=0",
};

File.WriteAllText(Path.Combine(outputDir, "a4_symmetry_irrep_projectors.json"), JsonSerializer.Serialize(a4, jsonOpts));
File.WriteAllText(Path.Combine(outputDir, "a5_gaussian_domination_stage_a.json"), JsonSerializer.Serialize(a5, jsonOpts));

// --- pack manifest --------------------------------------------------------------------
var manifestObj = new
{
    packItem = "phase456_preregistration_pack_manifest",
    planSection = "WAVE2_ACTION_PLAN_2026-07-12 item 9; standing items 12 & 14",
    generatedBy = "phase456 preregistration a4a5_pack_generator (deterministic; no HMC)",
    volumeN = N,
    thresholdSpec = new
    {
        rule = "max( calibratedFamilyWiseAtLeast3Sigma , perRow3Sigma )",
        familyWise = "family-wise significance calibrated across all pack rows to control the family-wise error at the >=3 sigma level (calibration is mechanical, computed from the row count and the null covariance; NOT a bare percentile)",
        perRow = "each row must independently clear 3 sigma against its exact free-field control",
        bare99thPercentileForbidden = true,
        rationale = "O5 standing: the committed spectrum is free-field-compatible (0.9 sigma); a bare 99th percentile is forbidden as a dynamical-structure threshold",
    },
    powerGate = new
    {
        rule = "each row must reach pre-registered statistical power >= 0.8 to detect a true 3 sigma effect at its threshold, computed from the achieved N_eff (tau_int-based) and the row's null variance",
        underpoweredTerminalRow = "underpowered",
        underpoweredMeaning = "no claim either way; the row records insufficient power, never a null-by-default",
        neffFloor = 100.0,
    },
    aicWindowAggregation = new
    {
        rule = "MECHANICAL: enumerate every contiguous cosh-fit window within the informative range (1 <= t <= T/2 - 1, or the single-point {0} window with the excited-state caveat when T/2-1 < 1); fit the constant-mass cosh model on each; select/aggregate by AICc weights (Akaike weights normalized over the enumerated window set)",
        aggregation = "AIC-weighted model average of a*m across windows; the systematic spread across windows enters the reported error in quadrature",
        analystChosenWindowExists = false,
        note = "no analyst-chosen window exists anywhere in the pipeline; the window set and the AICc formula are pinned here ex ante",
    },
    mv456Fallback = new
    {
        definition = "MV-456 (minimum viable) scopes WHICH rows are attempted if the full pack cannot run to budget: the n=4 exact free-field gate + the identity-irrep 2x2 GEVP zero-momentum gap only; the k_min dispersion row and the non-identity channel are dropped from the ATTEMPTED set",
        isNotAGateBypass = true,
        invariant = "MV-456 never relaxes a gate and never bypasses A4: the A4 Stage-A kernels (identity + non-identity + dispersion) still exist ex ante and stay hash-pinned; MV-456 only reduces scope, never thresholds/power/AIC rules",
    },
    hardGatesBeforeProductionSampling = new[]
    {
        "phase455 terminal recorded",
        "A4 Stage-A kernels committed (this pack)",
        "A5 Stage-A verdict committed (this pack)",
        "O4 memo or explicit user-renewed risk acceptance before production HMC",
    },
    consumers = new
    {
        limbL6 = "closes at probed-volume scope on T1 with a Gaussian-null Binder column",
        limbL8 = "O5 null at two-volume strength (Binder Gaussian-null at both volumes)",
        n5Escalation = "T2 = coherent >=3 sigma departure in >=2 distinct-irrep channels (|delta_ch|<=2) => mandatory n=5 escalation (contingent consumer of registry B:471-476) before ANY claim",
    },
    refuseToRunGate = new
    {
        description = "phase456 production refuses to run unless the committed pack hash matches its pinned constant; awaiting-pack stays reachable when the pack is absent; pack-committed-awaiting-gates is emitted when the pack is present and the hash matches; a mismatch is a blocked refuse-to-run condition",
        packFiles = new[]
        {
            "a4_symmetry_irrep_projectors.json",
            "a5_gaussian_domination_stage_a.json",
            "pack_manifest.json",
        },
        hashAlgorithm = "SHA-256 over the concatenation of the pinned pack files in listed order (byte-exact file contents, listed-order concatenation)",
    },
    mandatoryFraming = "workbench-relative structure data only; lattice units; nothing measured or promoted at Stage A; promotedPhysicalMassClaimCount=0",
};
File.WriteAllText(Path.Combine(outputDir, "pack_manifest.json"), JsonSerializer.Serialize(manifestObj, jsonOpts));

// --- committed pack hash --------------------------------------------------------------
string[] packFiles = { "a4_symmetry_irrep_projectors.json", "a5_gaussian_domination_stage_a.json", "pack_manifest.json" };
var hashInput = new List<byte>();
foreach (var pf in packFiles) hashInput.AddRange(File.ReadAllBytes(Path.Combine(outputDir, pf)));
string packHash = Convert.ToHexString(SHA256.HashData(hashInput.ToArray())).ToLowerInvariant();
var hashObj = new
{
    packItem = "phase456_preregistration_pack_hash",
    hashAlgorithm = "SHA-256 over concatenation of packFiles in listed order",
    packFiles,
    packSha256 = packHash,
    note = "phase456 Program.cs pins this value as PinnedPackSha256; production refuses to run on any mismatch",
};
File.WriteAllText(Path.Combine(outputDir, "pack_hash.json"), JsonSerializer.Serialize(hashObj, jsonOpts));

// --- console report -------------------------------------------------------------------
Console.WriteLine($"[a4a5] realized point group order = {H.Count} (closed={closedGroup}); H_s order = {hsOrder} (closed={hsClosed})");
Console.WriteLine($"[a4a5] proper/improper in realized set = {realized.Count(s => s.Det4 == 1)}/{realized.Count(s => s.Det4 == -1)}");
Console.WriteLine($"[a4a5] combinatorial & numeric S_B agree = {realizationConsistent}; sbResidWorst = {sbResidWorst:E2}");
Console.WriteLine($"[a4a5] H_s conjugacy classes/irreps = {nClasses}; abelianization order (linear irreps) = {abelianizationOrder}; +/-1 chars = {pm1Characters.Count}");
Console.WriteLine($"[a4a5] higher-dim irreps: count={higherCount}, sum of squares={higherSqSum}");
Console.WriteLine($"[a4a5] spatialInversionRealized={spatialInversionRealized} => parityLabelAdmissible={parityLabelAdmissible} (0-+-like banned={!parityLabelAdmissible})");
Console.WriteLine($"[a4a5] faceTypeCount={faceTypeCount}; identity type-orbits(mult A1)={identityTypeOrbits}; nonId char mult={nonIdMultiplicity}");
Console.WriteLine($"[a4a5] A5 verdict: {a5.verdict}");
Console.WriteLine($"[a4a5] PACK SHA-256 = {packHash}");
Console.WriteLine($"[a4a5] wrote: a4_symmetry_irrep_projectors.json, a5_gaussian_domination_stage_a.json, pack_manifest.json, pack_hash.json");

// =====================================================================================
// Helpers.
// =====================================================================================

int[] VertexMap(int[,] A)
{
    var phi = new int[mesh.VertexCount];
    for (int v = 0; v < mesh.VertexCount; v++)
    {
        var x = coords[v]; var y = new int[4];
        for (int i = 0; i < 4; i++) { int s = 0; for (int j = 0; j < 4; j++) s += A[i, j] * x[j]; y[i] = Mod(s, N); }
        phi[v] = vertexAt[(y[0], y[1], y[2], y[3])];
    }
    return phi;
}

A5Result AttemptA5GaussianDomination()
{
    // Work the inequality m(beta) >= m_free on the exactly-quartic S_B even sector.
    //
    // Structure of S_B (committed toy control branch, memory + phase448/449): S_B(omega) is a
    // QUARTIC polynomial in omega with an exact Z_2 symmetry omega -> -omega (Upsilon = F - T^aug
    // is quadratic in omega for the control branch; the full objective 0.5|Upsilon|^2 is quartic
    // and even). The even sector is the omega -> -omega invariant observable algebra; the 0++
    // interpolators O1 = Tr(F^2), O2 = Tr(Upsilon^2) are even (quadratic) observables.
    //
    // Gaussian domination / infrared bounds (Frohlich-Simon-Spencer / Brydges) would give, for a
    // reflection-positive theory whose single-site perturbation is a Gaussian-domination measure,
    // an UPPER bound on the FIELD (omega) two-point function by the free one, hence a lower bound
    // on the field-channel mass gap m_field(beta) >= m_free.
    //
    // The 0++ mass gap here is defined via the COMPOSITE (glueball-like) correlator
    // <O(t) O(0)>_c, a connected FOUR-point function of omega. Two independent obstructions block
    // the passage from a field-level Gaussian-domination bound to m(beta) >= m_free in this channel:
    //
    //  (O-A) COMPOSITENESS. Field-level infrared bounds control <omega omega>; they do not bound
    //        the connected 4-omega composite correlator that defines the 0++ pole. There is no
    //        elementary (Griffiths/GKS-type) inequality transporting the field bound to the
    //        composite channel for a NON-ferromagnetic, matrix-valued (su(2)) quartic; the sign of
    //        the induced composite coupling is not controlled.
    //
    //  (O-B) REFLECTION POSITIVITY OF THE KUHN-SIMPLICIAL ACTION. Gaussian domination presupposes
    //        reflection positivity across the axis-0 time reflection. The Coxeter-Freudenthal-Kuhn
    //        triangulation's distinguished body-diagonal (1,1,1,1) is NOT preserved by the naive
    //        link/site reflection in the time hyperplane (A4 established: no realized improper
    //        element implements the reflection). Link-reflection positivity is therefore not
    //        automatic for this simplicial S_B, and is not established at Stage A.
    //
    // Additionally the quartic coupling is non-local (couples plaquettes/faces, not single sites),
    // so the single-site Gaussian-domination measure hypothesis does not even type-check.
    //
    // VERDICT: NOT provable at Stage A. The obstruction is the deliverable. This does NOT cancel
    // the beta ladders (WS4/Binder sampling is not analytically closed by A5); it feeds phase458 G1
    // as the A5 terminal string.
    return new A5Result
    {
        packItem = "A5_stage_a_gaussian_domination_attempt",
        sector = "even (omega -> -omega Z_2 invariant) sector of the exactly-quartic S_B",
        target = "m(beta) >= m_free in the 0++ composite channel",
        sbStructure = "S_B(omega) quartic, even under omega -> -omega; O1=Tr(F^2), O2=Tr(Upsilon^2) are even quadratic interpolators; the 0++ gap is a connected 4-omega composite correlator",
        provable = false,
        verdict = "a5-stage-a-gaussian-domination-not-provable-obstruction-recorded",
        obstructions = new[]
        {
            "O-A compositeness: field-level infrared/Gaussian-domination bounds control <omega omega>, not the connected 4-omega composite correlator defining the 0++ pole; no elementary inequality transports the field bound to the composite channel for the matrix-valued su(2) quartic (induced composite-coupling sign uncontrolled)",
            "O-B reflection positivity: Gaussian domination presupposes reflection positivity across the axis-0 time reflection; the Kuhn body-diagonal is not preserved by the time-hyperplane reflection (A4: no realized improper element), so link-reflection positivity is not established for this simplicial S_B at Stage A",
            "non-locality: the quartic coupling is plaquette/face-local, not single-site, so the single-site Gaussian-domination measure hypothesis does not apply",
        },
        whatItWouldHaveCancelled = "if provable, m(beta) >= m_free would have bounded the interacting gap below by the free gap and removed the need to sample the beta ladder for the gap lower bound; it does NOT, so the beta ladders stand",
        consumes = "feeds phase458 gate G1 as the A5 terminal string; A5 does NOT close L8 analytically (no NO-GO-theorem-closed via A5)",
        mandatoryFraming = "no promotion; the obstruction record is a first-class negative result; promotedPhysicalMassClaimCount=0",
    };
}

static int Mod(int a, int n) => ((a % n) + n) % n;

static int[]? MinImage01(int[] from, int[] to, int n)
{
    var d = new int[4];
    for (int i = 0; i < 4; i++)
    {
        int raw = Mod(to[i] - from[i], n);
        if (raw == 0) d[i] = 0; else if (raw == 1) d[i] = 1; else return null;
    }
    return d[0] + d[1] + d[2] + d[3] == 0 ? null : d;
}

static int[][] Permutations4()
{
    var res = new List<int[]>(); Permute(new[] { 0, 1, 2, 3 }, 0, res); return res.ToArray();
    static void Permute(int[] a, int s, List<int[]> r)
    {
        if (s == a.Length - 1) { r.Add((int[])a.Clone()); return; }
        for (int i = s; i < a.Length; i++) { (a[s], a[i]) = (a[i], a[s]); Permute(a, s + 1, r); (a[s], a[i]) = (a[i], a[s]); }
    }
}

static int SignedPermDet(int[,] A)
{
    // determinant of a 4x4 signed-permutation matrix = sign(perm) * product(signs)
    int det = 1;
    var used = new bool[4]; var perm = new int[4]; var sign = 1;
    for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
            if (A[i, j] != 0) { perm[i] = j; sign *= A[i, j]; }
    // parity of perm
    for (int i = 0; i < 4; i++)
        for (int j = i + 1; j < 4; j++)
            if (perm[i] > perm[j]) det = -det;
    return det * sign;
}

static int SpatialDet(int[,] A)
{
    // determinant of the 3x3 spatial block (axes 1..3), meaningful when time axis maps to itself.
    // If time (axis 0) does not map to axis 0, return 0 (block undefined for rest-frame classification).
    if (A[0, 0] == 0) return 0;
    var perm = new int[3]; var sign = 1; bool ok = true;
    for (int i = 1; i < 4; i++)
    {
        int col = -1;
        for (int j = 1; j < 4; j++) if (A[i, j] != 0) { col = j; sign *= A[i, j]; }
        if (col < 0) { ok = false; break; }
        perm[i - 1] = col - 1;
    }
    if (!ok) return 0;
    int det = 1;
    for (int i = 0; i < 3; i++) for (int j = i + 1; j < 3; j++) if (perm[i] > perm[j]) det = -det;
    return det * sign;
}

static bool MatEq(int[,] a, int[,] b)
{
    for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) if (a[i, j] != b[i, j]) return false;
    return true;
}

static int[,] MatMul(int[,] a, int[,] b)
{
    var c = new int[4, 4];
    for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) { int s = 0; for (int k = 0; k < 4; k++) s += a[i, k] * b[k, j]; c[i, j] = s; }
    return c;
}

static string Key(int[,] a)
{
    var sb = new StringBuilder(16);
    for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) sb.Append((char)(a[i, j] + 2));
    return sb.ToString();
}

static bool IsClosedGroup(List<int[,]> g)
{
    var set = new HashSet<string>(g.Select(Key));
    foreach (var a in g) foreach (var b in g) if (!set.Contains(Key(MatMul(a, b)))) return false;
    return true;
}

static int ConjugacyClassCount(List<int[,]> g)
{
    var keys = g.Select(Key).ToList();
    var index = new Dictionary<string, int>();
    for (int i = 0; i < g.Count; i++) index[keys[i]] = i;
    var seen = new bool[g.Count]; int classes = 0;
    // precompute inverses (transpose for signed perms)
    var inv = g.Select(Transpose).ToList();
    for (int i = 0; i < g.Count; i++)
    {
        if (seen[i]) continue; classes++;
        for (int h = 0; h < g.Count; h++)
        {
            var conj = MatMul(MatMul(g[h], g[i]), inv[h]);
            if (index.TryGetValue(Key(conj), out int idx)) seen[idx] = true;
        }
    }
    return classes;

    static int[,] Transpose(int[,] a) { var t = new int[4, 4]; for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) t[j, i] = a[i, j]; return t; }
}

static List<int[,]> CommutatorSubgroup(List<int[,]> g)
{
    var gen = new HashSet<string>();
    var elems = new List<int[,]>();
    var inv = g.ToDictionary(Key, a => Transpose(a));
    foreach (var a in g)
        foreach (var b in g)
        {
            var comm = MatMul(MatMul(a, b), MatMul(inv[Key(a)], inv[Key(b)]));
            if (gen.Add(Key(comm))) elems.Add(comm);
        }
    // close under multiplication
    var closed = new HashSet<string>(gen);
    var list = new List<int[,]>(elems);
    // ensure identity present
    var id = new int[4, 4]; for (int i = 0; i < 4; i++) id[i, i] = 1;
    if (closed.Add(Key(id))) list.Add(id);
    bool grew = true;
    while (grew)
    {
        grew = false;
        var snapshot = list.ToArray();
        foreach (var a in snapshot) foreach (var b in snapshot)
        {
            var p = MatMul(a, b);
            if (closed.Add(Key(p))) { list.Add(p); grew = true; }
        }
    }
    return list;

    static int[,] Transpose(int[,] a) { var t = new int[4, 4]; for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) t[j, i] = a[i, j]; return t; }
}

static List<int[]> PlusMinusCharacters(List<int[,]> g)
{
    // enumerate all homomorphisms chi: G -> {+1,-1}. Greedy generating set, BFS word assignment,
    // then verify the homomorphism property on all pairs. Returns char vectors aligned to g order.
    int nEl = g.Count;
    var keys = g.Select(Key).ToList();
    var index = new Dictionary<string, int>();
    for (int i = 0; i < nEl; i++) index[keys[i]] = i;
    int idIdx = index[Key(Identity())];

    // greedy generators
    var gens = new List<int>();
    var span = new HashSet<string> { Key(Identity()) };
    foreach (var _ in g)
    {
        for (int i = 0; i < nEl; i++)
        {
            if (span.Contains(keys[i])) continue;
            gens.Add(i);
            // grow span with all products
            bool grew = true; span.Add(keys[i]);
            while (grew)
            {
                grew = false; var snap = span.ToArray();
                foreach (var sk in snap) foreach (var gg in gens)
                {
                    var prod = MatMul(g[index[sk]], g[gg]);
                    if (span.Add(Key(prod))) grew = true;
                }
            }
            break;
        }
        if (span.Count == nEl) break;
    }

    var results = new List<int[]>();
    int k = gens.Count;
    for (int mask = 0; mask < (1 << k); mask++)
    {
        var s = new int[k];
        for (int i = 0; i < k; i++) s[i] = ((mask >> i) & 1) == 0 ? 1 : -1;
        // BFS assign chi
        var chi = new int?[nEl]; chi[idIdx] = 1;
        var queue = new Queue<int>(); queue.Enqueue(idIdx);
        while (queue.Count > 0)
        {
            int cur = queue.Dequeue();
            for (int gi = 0; gi < k; gi++)
            {
                var prod = MatMul(g[cur], g[gens[gi]]);
                int pj = index[Key(prod)];
                int val = chi[cur]!.Value * s[gi];
                if (chi[pj] == null) { chi[pj] = val; queue.Enqueue(pj); }
            }
        }
        if (chi.Any(x => x == null)) continue;
        var chiV = chi.Select(x => x!.Value).ToArray();
        // verify homomorphism on all pairs
        bool ok = true;
        for (int a = 0; a < nEl && ok; a++)
            for (int b = 0; b < nEl && ok; b++)
            {
                int pj = index[Key(MatMul(g[a], g[b]))];
                if (chiV[pj] != chiV[a] * chiV[b]) ok = false;
            }
        if (ok) results.Add(chiV);
    }
    return results;

    static int[,] Identity() { var id = new int[4, 4]; for (int i = 0; i < 4; i++) id[i, i] = 1; return id; }
}

static int MatrixRank01Orbits(int[][] typePerm, int nTypes)
{
    // number of orbits of the group (generated by the given permutations) acting on {0..nTypes-1}
    var parent = Enumerable.Range(0, nTypes).ToArray();
    int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
    void Union(int a, int b) { int ra = Find(a), rb = Find(b); if (ra != rb) parent[ra] = rb; }
    foreach (var pmt in typePerm) for (int t = 0; t < nTypes; t++) Union(t, pmt[t]);
    return Enumerable.Range(0, nTypes).Select(Find).Distinct().Count();
}

static BranchManifest BuildManifest() => new()
{
    BranchId = "phase456-preregistration-a4a5", SchemaVersion = "1.0.0", SourceEquationRevision = "draft-2021",
    CodeRevision = "phase456-pack", ActiveGeometryBranch = "simplicial", ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = "trivial", ActiveShiabBranch = "einsteinian-shiab", ActiveGaugeStrategy = "penalty",
    BaseDimension = 4, AmbientDimension = 4, LieAlgebraId = "su2", BasisConventionId = "canonical",
    ComponentOrderId = "face-major", AdjointConventionId = "adjoint-explicit", PairingConventionId = "pairing-trace",
    NormConventionId = "norm-l2-quadrature", DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(), InsertedChoiceIds = new[] { "IX-2" },
};

static GeometryContext BuildGeometry()
{
    var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
    var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 4 };
    return new GeometryContext
    {
        BaseSpace = baseSpace, AmbientSpace = ambientSpace, DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid", BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
        ObservationBinding = new GeometryBinding { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
        Patches = Array.Empty<PatchInfo>(),
    };
}

sealed record SymElement(int[,] A, int[] Eps, int[] Perm, int Det4, bool FixesTimeFwd, bool TimeReflect, int SpatialDet, bool CombOk, bool SbOk, double SbResid);

sealed class RationalMatrix
{
    public long[,] Num { get; }
    public int Denominator { get; }
    public RationalMatrix(long[,] num, int den) { Num = num; Denominator = den; }
    public long[] RowMajorNumerators()
    {
        int n = Num.GetLength(0), m = Num.GetLength(1);
        var r = new long[n * m];
        for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) r[i * m + j] = Num[i, j];
        return r;
    }
}

sealed class A5Result
{
    public string packItem { get; set; } = "";
    public string sector { get; set; } = "";
    public string target { get; set; } = "";
    public string sbStructure { get; set; } = "";
    public bool provable { get; set; }
    public string verdict { get; set; } = "";
    public string[] obstructions { get; set; } = Array.Empty<string>();
    public string whatItWouldHaveCancelled { get; set; } = "";
    public string consumes { get; set; } = "";
    public string mandatoryFraming { get; set; } = "";
}
