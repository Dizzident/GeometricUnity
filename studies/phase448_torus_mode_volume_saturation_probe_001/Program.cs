using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase448: TORUS MODE-VOLUME saturation probe -- the physics study both
// phase444 unlock projects were built for, answering the question phase444
// left "undetermined-tooling-blocked": does the Phase443 one-loop
// no-saturation verdict change when the mode volume grows?
//
// THE TWO UNLOCKS USED TOGETHER.
//   (i) The lattice-canonical torus (CreateUniform4DPeriodic(n,
//       latticeCanonical: true)) makes the discrete action EXACTLY
//       translation-covariant (measured 7.8e-15 vs the old 1.2e-4 failure).
//   (ii) The adjoint/joint-gradient path gives O(mesh) analytic gradients and
//       Hessian-vector products (~ms vs the old ~60 s).
// On a TRANSLATION-INVARIANT background (the constant-field configuration --
// the natural Coleman-Weinberg analogue of the constant rays used throughout
// the program), covariance makes the joint (omega, theta) Hessian
// BLOCK-CIRCULANT over the translation group (Z_n)^4: its entire spectrum is
// determined by the 48 orbit-representative columns (15 edge types + 1 vertex
// type, times su(2)), each one Hessian-vector product, assembled by discrete
// Fourier transform into n^4 Hermitian 48x48 momentum blocks. The
// 3888-dimensional (n=3) and 12288-dimensional (n=4) joint Hessians are eigen-
// solved EXACTLY at trivial cost -- no dense FD assembly, no SLQ variance.
//
// CONSTRUCTION.
//   (0) For each volume n in {3, 4}: build the lattice-canonical torus, map
//       every edge to its (base vertex, 0/1 minimal-image displacement type)
//       orbit coordinates, and draw translation-INVARIANT unit rays u (the
//       same per-type coefficients across volumes, seeded identically, so the
//       volume comparison is like-for-like).
//   (1) theta*-stationarity within the invariant sector (3 DOF) by Newton on
//       the PROJECTED analytic gradient; the EQUIVARIANCE battery then checks
//       the FULL theta gradient vanishes at theta* (an invariant configuration
//       of a covariant action has an invariant gradient, so invariant-sector
//       stationarity must imply full stationarity -- verified numerically).
//   (2) V_eff(t) = S_B + (1/2) sum log lambda_i over positive eigenvalues of
//       the joint Hessian (verbatim IR convention), with the spectrum from the
//       momentum blocks. Saturation classification on the RAW curve by the
//       strict-local-minimum rule (no fits -- the Phase446 lesson).
//   (3) BATTERIES: exact translation covariance of every member's S_B on each
//       torus (gates the whole construction); block-reconstruction (H*v via
//       blocks vs a direct Hessian-vector product on random full-space
//       vectors); objective consistency (ComputeJointGradient vs the
//       EvaluateWithTheta path); analytic-vs-FD gradient spot check; full-
//       gradient equivariance at theta*; identity-control theta independence;
//       trace consistency (sum of block traces vs direct Hv samples).
//
// MANDATORY FRAMING. Workbench-relative structure data ONLY (su(2) toy algebra
// on the reduced Spin(4) slice, lattice units, one loop); the invariant-ray
// convention is recorded (it differs from Phase443's random open-mesh rays --
// the volume TREND across tori is the controlled comparison, not a direct
// numeric comparison to Phase443's 16-vertex values); NO GeV/pole/VEV
// promotion either way. The phase PASSES on internal consistency REGARDLESS of
// the saturation verdict.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage;
// no Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase448_torus_mode_volume_saturation_probe_001/output";
const string Phase443SummaryPath = "studies/phase443_joint_effective_potential_saturation_probe_001/output/joint_effective_potential_saturation_probe_summary.json";
const string Phase444SummaryPath = "studies/phase444_mode_volume_scaled_saturation_probe_001/output/mode_volume_scaled_saturation_probe_summary.json";
const string Phase447SummaryPath = "studies/phase447_two_loop_saturation_probe_001/output/two_loop_saturation_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ApplicationSubjectKind = "torus-mode-volume-saturation-probe";

int[] TorusSizes = (Environment.GetEnvironmentVariable("PHASE448_TORI") ?? "3,4")
    .Split(',').Select(int.Parse).ToArray();
int RaySeedCount = int.TryParse(Environment.GetEnvironmentVariable("PHASE448_RAYS"), out int rs) ? rs : 2;
double TMax = double.TryParse(Environment.GetEnvironmentVariable("PHASE448_TMAX"), out double tm) ? tm : 3.0;
int GridN = int.TryParse(Environment.GetEnvironmentVariable("PHASE448_GRIDN"), out int gn) ? gn : 16;
const int RngSeed = 20260704;

const double HvStep = 1e-5;              // Hessian-vector FD step (analytic-gradient central difference)
const double ZeroModeRelTol = 1e-8;      // verbatim IR convention
const double ZeroModeAbsFloor = 1e-10;
const double CovarianceGate = 1e-10;     // S_B translation covariance (relative)
const double BlockReconstructGate = 1e-4; // H*v via blocks vs direct Hv (both carry FD noise)
const double ThetaTolRel = 1e-9;         // projected-Newton relative gate
const double ThetaResidualBatteryRel = 1e-8;
const double ThetaAbsGradFloor = 1e-10;  // Phase446 small-t dimensional-gate convention
const double EquivarianceGate = 1e-8;    // full theta-gradient at theta* (relative)
const double GradFdGate = 1e-6;          // analytic-vs-FD gradient spot check
const double ObjectiveConsistencyGate = 1e-10;

var outputDir = Environment.GetEnvironmentVariable("PHASE448_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase443 = JsonDocument.Parse(File.ReadAllText(Phase443SummaryPath));
bool phase443PrecursorPassed =
    JsonBool(phase443.RootElement, "jointEffectivePotentialSaturationProbePassed") is true &&
    JsonBool(phase443.RootElement, "einsteinianLogSaturationObserved") is false;

using var phase444 = JsonDocument.Parse(File.ReadAllText(Phase444SummaryPath));
bool phase444PrecursorPassed = JsonBool(phase444.RootElement, "phase444Passed") is true;

using var phase447 = JsonDocument.Parse(File.ReadAllText(Phase447SummaryPath));
bool phase447PrecursorPassed = JsonBool(phase447.RootElement, "twoLoopSaturationProbePassed") is true;

bool precursorsPassed = phase443PrecursorPassed && phase444PrecursorPassed && phase447PrecursorPassed;

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
var manifest = BuildManifest();
var geometry = BuildGeometry();

var stopwatch = Stopwatch.StartNew();
var volumeRecords = new List<VolumeRecord>();

double[] tGrid = new double[GridN];
for (int i = 0; i < GridN; i++) tGrid[i] = TMax * (i + 1) / GridN;

// Shared per-type ray coefficients (same across volumes => like-for-like trend).
var rayTypeCoeffs = new List<double[]>();   // per seed: 48 coefficients (16 types x 3 lie)
for (int seed = 0; seed < RaySeedCount; seed++)
{
    var rng = new Random(RngSeed + 31 * seed);
    var c = new double[16 * dimG];
    for (int i = 0; i < c.Length; i++) c[i] = rng.NextDouble() - 0.5;
    rayTypeCoeffs.Add(c);
}

foreach (int n in TorusSizes)
{
    var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
    int nOmega = mesh.EdgeCount * dimG;
    int nTheta = mesh.VertexCount * dimG;
    int nJoint = nOmega + nTheta;
    int volume = mesh.VertexCount;
    var mass = new CpuMassMatrix(mesh, algebra);

    // --- Orbit coordinates: vertex lattice coords, edge (base, type). -------
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

    // Displacement types: nonzero 0/1 vectors, index 0..14; type 15 = vertex (theta).
    int TypeOf(int[] disp) => ((disp[0] << 3) | (disp[1] << 2) | (disp[2] << 1) | disp[3]) - 1;
    var edgeBase = new int[mesh.EdgeCount];
    var edgeType = new int[mesh.EdgeCount];
    bool orbitMapOk = true;
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int a = mesh.Edges[e][0], b = mesh.Edges[e][1];
        int[]? disp = MinImage01(coords[a], coords[b], n);
        if (disp != null) { edgeBase[e] = a; edgeType[e] = TypeOf(disp); }
        else
        {
            disp = MinImage01(coords[b], coords[a], n);
            if (disp != null) { edgeBase[e] = b; edgeType[e] = TypeOf(disp); }
            else orbitMapOk = false;
        }
    }
    // edge lookup by (base, type)
    var edgeAt = new int[mesh.VertexCount, 15];
    for (int e = 0; e < mesh.EdgeCount; e++) edgeAt[edgeBase[e], edgeType[e]] = e;

    // LATTICE GAUGE: the stored edge direction is index-ordered (v0 < v1), which
    // does NOT commute with translation -- same-orbit edges can be stored with
    // opposite orientation relative to their lattice displacement. oSign(e)
    // converts a stored 1-form component to the canonical base->tip orientation;
    // all orbit-space machinery works in that gauge and converts at the operator
    // boundary. (The first smoke run's covariance battery caught this at 1.6e-2.)
    var oSign = new double[mesh.EdgeCount];
    for (int e = 0; e < mesh.EdgeCount; e++)
        oSign[e] = mesh.Edges[e][0] == edgeBase[e] ? 1.0 : -1.0;

    // Translation of a joint vector by lattice vector a (lattice-gauge signs).
    double[] Translate(double[] x, int[] a)
    {
        var y = new double[nJoint];
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var c0 = coords[edgeBase[e]];
            int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
            int te = edgeAt[tv, edgeType[e]];
            double sgn = oSign[e] * oSign[te];
            for (int l = 0; l < dimG; l++) y[te * dimG + l] = sgn * x[e * dimG + l];
        }
        for (int v = 0; v < mesh.VertexCount; v++)
        {
            var c0 = coords[v];
            int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
            for (int l = 0; l < dimG; l++) y[nOmega + tv * dimG + l] = x[nOmega + v * dimG + l];
        }
        return y;
    }

    // --- Members (verbatim family specs; latticePeriod engages minimal-image bivectors).
    var controlMember = new EinsteinianShiabFamilyMember
    {
        Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None, EpsilonMode = "trivial",
    };
    var members = new List<(string Name, EinsteinianShiabOperator Op, bool IsControl)>
    {
        ("identity", new EinsteinianShiabOperator(mesh, algebra, controlMember, latticePeriod: n), true),
    };
    foreach (var (phi1, phi2, tag) in new[]
             {
                 (InvariantElementSpec.Sd2, InvariantElementSpec.Id0, "sd2-id0"),
                 (InvariantElementSpec.Asd2, InvariantElementSpec.Id0, "asd2-id0"),
             })
    {
        var m = new EinsteinianShiabFamilyMember
        {
            Phi1 = phi1, Phi2 = phi2, EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta",
        };
        members.Add(($"{tag}/c0.5", new EinsteinianShiabOperator(mesh, algebra, m, latticePeriod: n), false));
    }

    double SBOf(EinsteinianShiabOperator op, double[] omega, double[] theta) =>
        op.ComputeJointGradient(omega, theta, mass).Objective;

    // --- Batteries: covariance, objective consistency, gradient FD. ---------
    var batRng = new Random(RngSeed + 997 * n);
    double covarianceMax = 0.0, objConsistencyMax = 0.0, gradFdMax = 0.0;
    foreach (var (name, op, isControl) in members)
    {
        var xw = RandomVector(batRng, nOmega);
        var xt = RandomVector(batRng, nTheta);
        double s0 = SBOf(op, xw, xt);
        // Direct-path consistency (EvaluateWithTheta route).
        var conn = new ConnectionField(mesh, algebra, xw);
        var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
        var sTensor = FaceTensor(mesh, algebra, (isControl
            ? op.Evaluate(f, conn.ToFieldTensor(), manifest, geometry)
            : op.EvaluateWithTheta(f, conn.ToFieldTensor(), xt, manifest, geometry)).Coefficients);
        double sDirect = 0.5 * mass.InnerProduct(sTensor, sTensor);
        objConsistencyMax = System.Math.Max(objConsistencyMax, RelDiff(s0, sDirect));
        // Covariance under a few random translations.
        var xj = new double[nJoint];
        Array.Copy(xw, 0, xj, 0, nOmega); Array.Copy(xt, 0, xj, nOmega, nTheta);
        for (int rep = 0; rep < 3; rep++)
        {
            var a = new[] { batRng.Next(n), batRng.Next(n), batRng.Next(n), batRng.Next(n) };
            var y = Translate(xj, a);
            double sT = SBOf(op, y.Take(nOmega).ToArray(), y.Skip(nOmega).ToArray());
            covarianceMax = System.Math.Max(covarianceMax, RelDiff(s0, sT));
        }
        // Analytic-vs-FD gradient spot check (5 random coordinates).
        var g = op.ComputeJointGradient(xw, xt, mass);
        for (int rep = 0; rep < 5; rep++)
        {
            int i = batRng.Next(nJoint);
            double h = 1e-6;
            var xp = (double[])xj.Clone(); xp[i] += h;
            var xm = (double[])xj.Clone(); xm[i] -= h;
            double fd = (SBOf(op, xp.Take(nOmega).ToArray(), xp.Skip(nOmega).ToArray())
                       - SBOf(op, xm.Take(nOmega).ToArray(), xm.Skip(nOmega).ToArray())) / (2 * h);
            double an = i < nOmega ? g.GradOmega[i] : g.GradTheta[i - nOmega];
            gradFdMax = System.Math.Max(gradFdMax,
                System.Math.Abs(fd - an) / System.Math.Max(1.0, System.Math.Abs(fd)));
        }
    }

    // Identity-control theta independence.
    double identityThetaGradNorm;
    {
        var op = members[0].Op;
        var g = op.ComputeJointGradient(RandomVector(batRng, nOmega), RandomVector(batRng, nTheta), mass);
        identityThetaGradNorm = Norm(g.GradTheta);
    }

    // --- Per-member rays. ----------------------------------------------------
    var memberRecs = new List<TorusMemberRecord>();
    double blockReconstructMax = 0.0, traceConsistencyMax = 0.0;
    double maxThetaResid = 0.0, maxEquivarianceResid = 0.0;
    bool allThetaOk = true;
    int nk = volume; // number of momenta

    foreach (var (name, op, isControl) in members)
    {
        var seedRecs = new List<TorusSeedRecord>();
        for (int seed = 0; seed < RaySeedCount; seed++)
        {
            // Invariant unit ray from the shared per-type coefficients (omega types only).
            var u = new double[nOmega];
            var cShared = rayTypeCoeffs[seed];
            for (int e = 0; e < mesh.EdgeCount; e++)
                for (int l = 0; l < dimG; l++)
                    u[e * dimG + l] = oSign[e] * cShared[edgeType[e] * dimG + l];
            double un = Norm(u);
            for (int i = 0; i < nOmega; i++) u[i] /= un;

            var pts = new List<TorusPoint>();
            var thetaInv = new double[dimG]; // invariant-sector theta (warm-started along the ray)
            foreach (double t in tGrid)
            {
                var omega = Scale(u, t);

                // theta* in the invariant sector by Newton on the projected gradient.
                double thetaResidRel = 0.0, equivResid = 0.0;
                bool thetaOk = true;
                if (!isControl)
                {
                    double[] ProjGrad(double[] cInv)
                    {
                        var theta = ExpandTheta(cInv, mesh.VertexCount, dimG);
                        var g = op.ComputeJointGradient(omega, theta, mass);
                        var p = new double[dimG];
                        for (int v = 0; v < mesh.VertexCount; v++)
                            for (int l = 0; l < dimG; l++) p[l] += g.GradTheta[v * dimG + l];
                        return p;
                    }
                    var c = (double[])thetaInv.Clone();
                    var gStart = op.ComputeJointGradient(omega, ExpandTheta(c, mesh.VertexCount, dimG), mass);
                    double equivScale = System.Math.Max(Norm(gStart.GradTheta), 1e-12);
                    var g0 = new double[dimG];
                    for (int v = 0; v < mesh.VertexCount; v++)
                        for (int l = 0; l < dimG; l++) g0[l] += gStart.GradTheta[v * dimG + l];
                    double scale = System.Math.Max(Norm(g0), 1e-12);
                    for (int it = 0; it < 200; it++)
                    {
                        var g = ProjGrad(c);
                        if (Norm(g) <= System.Math.Max(ThetaTolRel * scale, 1e-13)) break;
                        // 3x3 FD Jacobian of the projected gradient.
                        var J = new double[dimG, dimG];
                        for (int j = 0; j < dimG; j++)
                        {
                            var cp = (double[])c.Clone(); cp[j] += 1e-6;
                            var gp = ProjGrad(cp);
                            for (int i = 0; i < dimG; i++) J[i, j] = (gp[i] - g[i]) / 1e-6;
                        }
                        var step = Solve3(J, g);
                        if (step == null) break;
                        for (int i = 0; i < dimG; i++) c[i] -= step[i];
                    }
                    thetaInv = c;
                    var thetaFull = ExpandTheta(c, mesh.VertexCount, dimG);
                    var gFin = op.ComputeJointGradient(omega, thetaFull, mass);
                    var pFin = new double[dimG];
                    for (int v = 0; v < mesh.VertexCount; v++)
                        for (int l = 0; l < dimG; l++) pFin[l] += gFin.GradTheta[v * dimG + l];
                    thetaResidRel = Norm(pFin) / scale;
                    equivResid = Norm(gFin.GradTheta) / equivScale;
                    bool relOk = thetaResidRel <= ThetaResidualBatteryRel && equivResid <= EquivarianceGate;
                    bool absOk = Norm(gFin.GradTheta) <= ThetaAbsGradFloor;
                    thetaOk = relOk || absOk;
                    maxThetaResid = System.Math.Max(maxThetaResid, thetaResidRel);
                    maxEquivarianceResid = System.Math.Max(maxEquivarianceResid, equivResid);
                    allThetaOk &= thetaOk;
                }
                var theta = isControl ? new double[nTheta] : ExpandTheta(thetaInv, mesh.VertexCount, dimG);
                double sB = SBOf(op, omega, theta);

                // --- Block-circulant Hessian: 48 representative columns via Hv.
                int nTypes = 16;
                int blockDim = nTypes * dimG;
                var cols = new double[blockDim][];   // LATTICE-GAUGE columns
                for (int ty = 0; ty < nTypes; ty++)
                    for (int l = 0; l < dimG; l++)
                    {
                        var vO = new double[nOmega]; var vT = new double[nTheta];
                        if (ty < 15)
                        {
                            int e0 = edgeAt[vertexAt[(0, 0, 0, 0)], ty];
                            vO[e0 * dimG + l] = oSign[e0]; // unit base->tip input
                        }
                        else vT[vertexAt[(0, 0, 0, 0)] * dimG + l] = 1.0;
                        var (hvO, hvT) = op.JointHessianVectorProduct(omega, theta, vO, vT, mass, HvStep);
                        var col = new double[nJoint];
                        for (int e = 0; e < mesh.EdgeCount; e++)
                            for (int ll = 0; ll < dimG; ll++)
                                col[e * dimG + ll] = oSign[e] * hvO[e * dimG + ll]; // to lattice gauge
                        Array.Copy(hvT, 0, col, nOmega, nTheta);
                        cols[ty * dimG + l] = col;
                    }

                // Kernel C(w)[row=(ty',l'), col=(ty,l)] from the lattice-gauge columns.
                double Kern(int[] w, int tyR, int lR, int colIdx)
                {
                    int v = vertexAt[(w[0], w[1], w[2], w[3])];
                    return tyR < 15
                        ? cols[colIdx][edgeAt[v, tyR] * dimG + lR]
                        : cols[colIdx][nOmega + v * dimG + lR];
                }

                // Momentum blocks: pair {k, -k}; real symmetric embedding for k != -k.
                var allEigs = new List<double>(nJoint);
                var seenK = new HashSet<int>();
                int KeyOf(int[] k) => ((k[0] * n + k[1]) * n + k[2]) * n + k[3];
                var wsAll = new List<int[]>();
                for (int w0 = 0; w0 < n; w0++) for (int w1 = 0; w1 < n; w1++)
                for (int w2 = 0; w2 < n; w2++) for (int w3 = 0; w3 < n; w3++)
                    wsAll.Add(new[] { w0, w1, w2, w3 });

                foreach (var k in wsAll)
                {
                    int key = KeyOf(k);
                    if (seenK.Contains(key)) continue;
                    var kNeg = new[] { Mod(-k[0], n), Mod(-k[1], n), Mod(-k[2], n), Mod(-k[3], n) };
                    int keyNeg = KeyOf(kNeg);
                    seenK.Add(key); seenK.Add(keyNeg);
                    bool selfConj = key == keyNeg;

                    var re = new double[blockDim, blockDim];
                    var im = new double[blockDim, blockDim];
                    foreach (var w in wsAll)
                    {
                        double phase = -2.0 * System.Math.PI * (k[0] * w[0] + k[1] * w[1] + k[2] * w[2] + k[3] * w[3]) / n;
                        double cph = System.Math.Cos(phase), sph = System.Math.Sin(phase);
                        for (int col = 0; col < blockDim; col++)
                            for (int tyR = 0; tyR < nTypes; tyR++)
                                for (int lR = 0; lR < dimG; lR++)
                                {
                                    double cv = Kern(w, tyR, lR, col);
                                    int row = tyR * dimG + lR;
                                    re[row, col] += cph * cv;
                                    im[row, col] += sph * cv;
                                }
                    }
                    // Hermitize (FD noise): H <- (H + H^dagger)/2.
                    for (int i = 0; i < blockDim; i++)
                        for (int j = 0; j < blockDim; j++)
                        {
                            double reS = 0.5 * (re[i, j] + re[j, i]);
                            double imS = 0.5 * (im[i, j] - im[j, i]);
                            re[i, j] = reS; re[j, i] = reS;
                            im[i, j] = imS; im[j, i] = -imS;
                        }

                    if (selfConj)
                    {
                        var eig = JacobiEigenvalues(re);
                        allEigs.AddRange(eig);
                    }
                    else
                    {
                        // Real embedding [[Re,-Im],[Im,Re]]: eigenvalues = spec(H(k)) each
                        // doubled = spec(H(k)) U spec(H(-k)) -- exactly the pair's modes.
                        int d2 = 2 * blockDim;
                        var emb = new double[d2, d2];
                        for (int i = 0; i < blockDim; i++)
                            for (int j = 0; j < blockDim; j++)
                            {
                                emb[i, j] = re[i, j];
                                emb[i + blockDim, j + blockDim] = re[i, j];
                                emb[i, j + blockDim] = -im[i, j];
                                emb[i + blockDim, j] = im[i, j];
                            }
                        allEigs.AddRange(JacobiEigenvalues(emb));
                    }
                }

                // IR convention + one-loop.
                double maxAbs = 0.0;
                foreach (double e in allEigs) maxAbs = System.Math.Max(maxAbs, System.Math.Abs(e));
                double zeroTol = System.Math.Max(ZeroModeAbsFloor, ZeroModeRelTol * maxAbs);
                int pos = 0, neg = 0, zero = 0;
                double oneLoop = 0.0;
                foreach (double e in allEigs)
                {
                    if (e > zeroTol) { pos++; oneLoop += 0.5 * System.Math.Log(e); }
                    else if (e < -zeroTol) neg++;
                    else zero++;
                }

                // Block-reconstruction + trace batteries at the anchor point only (cost).
                if (System.Math.Abs(t - tGrid[GridN / 2]) < 1e-12 && seed == 0)
                {
                    var rngB = new Random(RngSeed + 1717);
                    // trace of H from columns' orbit structure: volume * sum_col Kern(0)[diag].
                    double trBlocks = 0.0;
                    var w0v = new[] { 0, 0, 0, 0 };
                    for (int ty = 0; ty < nTypes; ty++)
                        for (int l = 0; l < dimG; l++)
                            trBlocks += Kern(w0v, ty, l, ty * dimG + l);
                    trBlocks *= volume;
                    double trDirect = 0.0;
                    for (int rep = 0; rep < 12; rep++)
                    {
                        int i = rngB.Next(nJoint);
                        var vO = new double[nOmega]; var vT = new double[nTheta];
                        if (i < nOmega) vO[i] = 1.0; else vT[i - nOmega] = 1.0;
                        var (hvO, hvT) = op.JointHessianVectorProduct(omega, theta, vO, vT, mass, HvStep);
                        trDirect += i < nOmega ? hvO[i] : hvT[i - nOmega];
                    }
                    trDirect *= (double)nJoint / 12.0;
                    traceConsistencyMax = System.Math.Max(traceConsistencyMax,
                        System.Math.Abs(trBlocks - trDirect) / System.Math.Max(1.0, System.Math.Abs(trBlocks)));

                    // Block reconstruction: H*v from translated lattice-gauge columns vs direct Hv.
                    var vFull = RandomVector(rngB, nJoint);
                    var hvRecon = new double[nJoint]; // accumulated in lattice gauge
                    for (int e = 0; e < mesh.EdgeCount; e++)
                        for (int l = 0; l < dimG; l++)
                            AccumulateTranslatedColumn(hvRecon, cols[edgeType[e] * dimG + l],
                                coords[edgeBase[e]], oSign[e] * vFull[e * dimG + l]);
                    for (int v = 0; v < mesh.VertexCount; v++)
                        for (int l = 0; l < dimG; l++)
                            AccumulateTranslatedColumn(hvRecon, cols[15 * dimG + l],
                                coords[v], vFull[nOmega + v * dimG + l]);
                    for (int e = 0; e < mesh.EdgeCount; e++)
                        for (int l = 0; l < dimG; l++)
                            hvRecon[e * dimG + l] *= oSign[e]; // back to stored gauge
                    var (dO, dT) = op.JointHessianVectorProduct(omega, theta,
                        vFull.Take(nOmega).ToArray(), vFull.Skip(nOmega).ToArray(), mass, HvStep);
                    var hvDirect = new double[nJoint];
                    Array.Copy(dO, 0, hvDirect, 0, nOmega); Array.Copy(dT, 0, hvDirect, nOmega, nTheta);
                    double num = 0, den = 0;
                    for (int i = 0; i < nJoint; i++)
                    {
                        num += (hvRecon[i] - hvDirect[i]) * (hvRecon[i] - hvDirect[i]);
                        den += hvDirect[i] * hvDirect[i];
                    }
                    blockReconstructMax = System.Math.Max(blockReconstructMax,
                        System.Math.Sqrt(num) / System.Math.Max(1e-30, System.Math.Sqrt(den)));
                }

                pts.Add(new TorusPoint(t, sB, oneLoop, sB + oneLoop, pos, zero, neg,
                    thetaResidRel, equivResid, thetaOk));
            }
            seedRecs.Add(new TorusSeedRecord(seed, pts.ToArray()));
        }
        memberRecs.Add(new TorusMemberRecord(name, isControl, seedRecs));
    }

    volumeRecords.Add(new VolumeRecord(n, volume, mesh.EdgeCount, nOmega, nTheta, nJoint,
        orbitMapOk, covarianceMax, objConsistencyMax, gradFdMax, identityThetaGradNorm,
        blockReconstructMax, traceConsistencyMax, maxThetaResid, maxEquivarianceResid, allThetaOk,
        memberRecs));

    // Local per-vertex translated-column accumulator (lattice-gauge in/out;
    // captures the maps of THIS volume).
    void AccumulateTranslatedColumn(double[] target, double[] col, int[] site, double weight)
    {
        if (weight == 0.0) return;
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var c0 = coords[edgeBase[e]];
            int sv = vertexAt[(Mod(c0[0] - site[0], n), Mod(c0[1] - site[1], n), Mod(c0[2] - site[2], n), Mod(c0[3] - site[3], n))];
            int se = edgeAt[sv, edgeType[e]];
            for (int l = 0; l < dimG; l++)
                target[e * dimG + l] += weight * col[se * dimG + l];
        }
        for (int v = 0; v < mesh.VertexCount; v++)
        {
            var c0 = coords[v];
            int sv = vertexAt[(Mod(c0[0] - site[0], n), Mod(c0[1] - site[1], n), Mod(c0[2] - site[2], n), Mod(c0[3] - site[3], n))];
            for (int l = 0; l < dimG; l++)
                target[nOmega + v * dimG + l] += weight * col[nOmega + sv * dimG + l];
        }
    }
}

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Classification + verdicts.
// ---------------------------------------------------------------------------

(string Cls, double TStar) ClassifyCurve(double[] v)
{
    if (v.Length < 3) return ("insufficient-points", double.NaN);
    int am = 0;
    for (int j = 1; j < v.Length; j++) if (v[j] < v[am]) am = j;
    if (am == 0) return ("trivial-origin", double.NaN);
    if (am == v.Length - 1) return ("runaway", double.NaN);
    if (v[am - 1] > v[am] && v[am + 1] > v[am]) return ("interior-finite", tGrid[am]);
    return ("interior-inflection-not-verified", double.NaN);
}

var classifications = new List<object>();
bool anySaturationAnyVolume = false;
bool volumeTrendSeedStable = true;
var perVolumeEinsteinianCls = new Dictionary<int, List<string>>();
foreach (var vr in volumeRecords)
{
    perVolumeEinsteinianCls[vr.N] = new List<string>();
    foreach (var m in vr.Members)
    {
        var perSeed = m.Seeds.Select(s => ClassifyCurve(s.Points.Select(p => p.VEff).ToArray()).Cls).ToArray();
        if (perSeed.Distinct().Count() > 1) volumeTrendSeedStable = false;
        if (!m.IsControl)
        {
            perVolumeEinsteinianCls[vr.N].Add(perSeed[0]);
            if (perSeed[0] == "interior-finite") anySaturationAnyVolume = true;
        }
        classifications.Add(new
        {
            torusN = vr.N,
            member = m.Name,
            isControl = m.IsControl,
            classificationBySeed = perSeed,
            tStar = ClassifyCurve(m.Seeds[0].Points.Select(p => p.VEff).ToArray()).TStar is double ts && double.IsFinite(ts) ? (double?)ts : null,
        });
    }
}

// Batteries roll-up.
bool orbitMapsOk = volumeRecords.All(v => v.OrbitMapOk);
double covarianceWorst = volumeRecords.Max(v => v.CovarianceMax);
bool covarianceBattery = covarianceWorst <= CovarianceGate;
double objConsistencyWorst = volumeRecords.Max(v => v.ObjConsistencyMax);
bool objectiveConsistencyBattery = objConsistencyWorst <= ObjectiveConsistencyGate;
double gradFdWorst = volumeRecords.Max(v => v.GradFdMax);
bool gradientBattery = gradFdWorst <= GradFdGate;
double identityThetaGradWorst = volumeRecords.Max(v => v.IdentityThetaGradNorm);
bool identityThetaIndependent = identityThetaGradWorst <= 1e-10;
double blockReconWorst = volumeRecords.Max(v => v.BlockReconstructMax);
bool blockReconstructionBattery = blockReconWorst <= BlockReconstructGate;
double traceWorst = volumeRecords.Max(v => v.TraceConsistencyMax);
bool traceConsistencyRecorded = double.IsFinite(traceWorst);
bool thetaGate = volumeRecords.All(v => v.AllThetaOk);
double thetaResidWorst = volumeRecords.Max(v => v.MaxThetaResid);
double equivarianceWorst = volumeRecords.Max(v => v.MaxEquivarianceResid);

bool batteriesAllPassed =
    precursorsPassed &&
    orbitMapsOk &&
    covarianceBattery &&
    objectiveConsistencyBattery &&
    gradientBattery &&
    identityThetaIndependent &&
    blockReconstructionBattery &&
    traceConsistencyRecorded &&
    thetaGate;

// The phase444 question, now DETERMINED.
string modeVolumeVerdict;
if (!batteriesAllPassed)
    modeVolumeVerdict = "blocked";
else if (!anySaturationAnyVolume)
    modeVolumeVerdict = "no-saturation-persists-across-mode-volumes";
else
{
    bool everywhere = perVolumeEinsteinianCls.Values.All(l => l.All(c => c == "interior-finite"));
    modeVolumeVerdict = everywhere
        ? "saturation-observed-at-all-probed-volumes"
        : "saturation-appears-at-some-volumes-recorded";
}

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool invariantRayConventionRecorded = true;
const bool physicistReviewPending = true;
const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase256Contract = false;
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su2 trace-pairing; lattice-canonical tori n in {{{string.Join(",", TorusSizes)}}}",
    "translation-invariant unit rays (shared per-type coefficients across volumes); theta* by projected-Newton in the invariant sector with full-gradient equivariance battery; joint Hessian spectrum via block-circulant momentum decomposition from 48 orbit-representative Hessian-vector products (adjoint path) with covariance/reconstruction/trace batteries; verbatim positive-mode one-loop IR convention; raw-curve strict-minimum classification, no fits; no target values")))).ToLowerInvariant();

bool torusModeVolumeSaturationProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    invariantRayConventionRecorded &&
    physicistReviewPending &&
    definition81Scope == "reduced-spin4-slice" &&
    !ambientSevenSevenRealized &&
    !internalGaugeContentRealized &&
    !weldRealized &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian &&
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

string terminalStatus = torusModeVolumeSaturationProbePassed
    ? modeVolumeVerdict switch
    {
        "no-saturation-persists-across-mode-volumes" => "torus-mode-volume-saturation-probe-passed-no-saturation-persists-across-mode-volumes-frontier-sharpened",
        "saturation-observed-at-all-probed-volumes" => "torus-mode-volume-saturation-probe-passed-saturation-candidate-observed-workbench-relative-no-gev",
        _ => "torus-mode-volume-saturation-probe-passed-mixed-volume-verdict-recorded",
    }
    : "torus-mode-volume-saturation-probe-blocked";

string decision = torusModeVolumeSaturationProbePassed
    ? $"The torus mode-volume saturation probe is decided on internal consistency, answering the question Phase444 left undetermined-tooling-blocked using BOTH unlock projects. On lattice-canonical tori n in {{{string.Join(",", TorusSizes)}}} (joint DOF {string.Join(", ", volumeRecords.Select(v => v.NJoint))}) with translation-INVARIANT unit rays (shared per-type coefficients across volumes, {RaySeedCount} seeds), the joint (omega,theta) Hessian is block-circulant and its EXACT spectrum was obtained from 48 orbit-representative Hessian-vector products per point via momentum decomposition. " +
      $"BATTERIES: S_B translation covariance worst {covarianceWorst:E2} <= {CovarianceGate:E0} (every member, every torus); objective consistency {objConsistencyWorst:E2}; analytic-vs-FD gradient {gradFdWorst:E2}; identity theta-independence {identityThetaGradWorst:E2}; block reconstruction (H*v via translated columns vs direct Hv) {blockReconWorst:E2} <= {BlockReconstructGate:E0}; trace consistency {traceWorst:E2} (recorded; stochastic-diagonal comparator); theta* projected-Newton relative residual worst {thetaResidWorst:E2} with FULL-gradient equivariance worst {equivarianceWorst:E2} <= {EquivarianceGate:E0}. " +
      $"MODE-VOLUME VERDICT: {modeVolumeVerdict} (per-volume Einsteinian classifications: {string.Join("; ", perVolumeEinsteinianCls.Select(kv => $"n={kv.Key}: {string.Join(",", kv.Value)}"))}; seed-stable {volumeTrendSeedStable}). " +
      $"RECORDED CONVENTION: invariant rays differ from Phase443's random open-mesh rays -- the controlled comparison is the TREND ACROSS TORI, not a direct numeric comparison to the 16-vertex values. MANDATORY FRAMING: workbench-relative structure data only (su(2) toy algebra, reduced Spin(4) slice, lattice units, one loop); NO GeV/pole/VEV promotion. Everything is target-blind; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the mode-volume verdicts until the precursor, covariance, block-reconstruction, and theta batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

var result = new
{
    phaseId = "phase448-torus-mode-volume-saturation-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    torusModeVolumeSaturationProbePassed,

    phase443PrecursorPassed,
    phase444PrecursorPassed,
    phase447PrecursorPassed,
    precursorsPassed,

    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,

    probeConfiguration = new
    {
        lieAlgebraId = "su2-trace-pairing",
        torusSizes = TorusSizes,
        latticeCanonical = true,
        raySeedCount = RaySeedCount,
        tMax = TMax,
        gridN = GridN,
        tGrid,
        hvStep = HvStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        covarianceGate = CovarianceGate,
        blockReconstructGate = BlockReconstructGate,
        thetaTol = ThetaTolRel,
        thetaResidualBattery = ThetaResidualBatteryRel,
        equivarianceGate = EquivarianceGate,
        rayConvention = "translation-INVARIANT unit rays (constant per edge-type/Lie coefficients, shared across volumes and normalized per volume) -- the constant-field Coleman-Weinberg analogue; RECORDED as a convention difference vs Phase443's random open-mesh rays",
        spectrumMethod = "block-circulant momentum decomposition: 48 orbit-representative columns (15 minimal-image edge-displacement types + 1 vertex type, x su(2)) each from ONE adjoint-path Hessian-vector product; DFT over (Z_n)^4 into Hermitian 48x48 momentum blocks; {k,-k} pairs eigensolved via the real symmetric embedding; validity gated by the covariance, block-reconstruction, and trace batteries",
        thetaStationarity = "projected Newton in the translation-invariant theta sector (3 DOF) on the analytic joint gradient; the equivariance battery requires the FULL theta gradient to vanish at theta* (covariance => an invariant configuration has an invariant gradient)",
        irConvention = "exact zero modes excluded at zeroTol = max(absFloor, relTol*maxAbsEig); positive eigenvalues enter the one-loop log sum; negative modes recorded",
        unlocksUsed = "lattice-canonical conventions (commit 82d43559) + adjoint/joint-gradient path (commit 7a7e397d)",
    },

    // Headline verdicts.
    modeVolumeVerdict,
    anySaturationAnyVolume,
    volumeTrendSeedStable,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    invariantRayConventionRecorded,
    physicistReviewPending,
    classifications,

    batteries = new
    {
        batteriesAllPassed,
        orbitMapsOk,
        covarianceBattery,
        covarianceWorst,
        objectiveConsistencyBattery,
        objConsistencyWorst,
        gradientBattery,
        gradFdWorst,
        identityThetaIndependent,
        identityThetaGradWorst,
        blockReconstructionBattery,
        blockReconWorst,
        traceConsistencyRecorded,
        traceWorst,
        thetaGate,
        thetaResidWorst,
        equivarianceWorst,
    },

    volumes = volumeRecords.Select(vr => new
    {
        torusN = vr.N,
        vertexCount = vr.Volume,
        edgeCount = vr.Edges,
        nOmega = vr.NOmega,
        nTheta = vr.NTheta,
        jointDof = vr.NJoint,
        covarianceMax = vr.CovarianceMax,
        blockReconstructMax = vr.BlockReconstructMax,
        members = vr.Members.Select(m => new
        {
            member = m.Name,
            isControl = m.IsControl,
            seeds = m.Seeds.Select(s => new
            {
                seed = s.Seed,
                points = s.Points.Select(p => new
                {
                    t = p.T,
                    sB = p.SB,
                    oneLoop = p.OneLoop,
                    vEff = p.VEff,
                    positiveModes = p.Pos,
                    zeroModes = p.Zero,
                    negativeModes = p.Neg,
                    thetaResidualRel = p.ThetaResidRel,
                    equivarianceResidual = p.EquivResid,
                }).ToArray(),
            }).ToArray(),
        }).ToArray(),
    }).ToArray(),

    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        physicistReviewPending,
        invariantRayConventionRecorded,
        baseSignature = "Cl(4,0)-euclidean-slice",
        spinorUsedAsShiabCarrierNotFiber = true,
        draft14dObserverseNotRealized = true,
    },

    runtimeSeconds,

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
        canFillPhase256Contract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },

    sourceEvidence = new
    {
        phase443SummaryPath = Phase443SummaryPath,
        phase444SummaryPath = Phase444SummaryPath,
        phase447SummaryPath = Phase447SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "V_eff, its classifications, and any t* are candidate-only structure data of the reduced spin-4 slice on lattice-canonical tori, NOT physical masses, VEVs, or a scale in GeV.",
        "The invariant-ray convention is recorded; the controlled statement is the saturation TREND across mode volumes, not a numeric comparison to Phase443's open-mesh values.",
        "A saturation observation, where stable, is a workbench-relative CANDIDATE only; a no-saturation result sharpens the frontier. Neither promotes anything.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "torus_mode_volume_saturation_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "torus_mode_volume_saturation_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"torusModeVolumeSaturationProbePassed={torusModeVolumeSaturationProbePassed}");
Console.WriteLine($"precursors: p443={phase443PrecursorPassed} p444={phase444PrecursorPassed} p447={phase447PrecursorPassed}");
foreach (var vr in volumeRecords)
    Console.WriteLine($"torus n={vr.N}: V={vr.Volume} E={vr.Edges} jointDof={vr.NJoint} cov={vr.CovarianceMax:E2} recon={vr.BlockReconstructMax:E2} thetaResid={vr.MaxThetaResid:E2} equiv={vr.MaxEquivarianceResid:E2}");
Console.WriteLine($"BATTERIES: all={batteriesAllPassed} cov={covarianceBattery}({covarianceWorst:E2}) obj={objectiveConsistencyBattery}({objConsistencyWorst:E2}) grad={gradientBattery}({gradFdWorst:E2}) idTheta={identityThetaIndependent}({identityThetaGradWorst:E2}) recon={blockReconstructionBattery}({blockReconWorst:E2}) trace={traceWorst:E2} theta={thetaGate}");
Console.WriteLine($"VERDICT: {modeVolumeVerdict} anySaturation={anySaturationAnyVolume} seedStable={volumeTrendSeedStable}");
foreach (var vr in volumeRecords)
    foreach (var m in vr.Members)
    {
        var cls = ClassifyCurve(m.Seeds[0].Points.Select(p => p.VEff).ToArray());
        Console.WriteLine($"  n={vr.N} {m.Name,-16} cls={cls.Cls,-34} vEff[first,last]=[{m.Seeds[0].Points[0].VEff:F2},{m.Seeds[0].Points[^1].VEff:F2}] modes(p/z/n)@mid={m.Seeds[0].Points[GridN / 2].Pos}/{m.Seeds[0].Points[GridN / 2].Zero}/{m.Seeds[0].Points[GridN / 2].Neg}");
    }
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static int Mod(int a, int n) => ((a % n) + n) % n;

static int[]? MinImage01(int[] from, int[] to, int n)
{
    var d = new int[4];
    for (int i = 0; i < 4; i++)
    {
        int raw = Mod(to[i] - from[i], n);
        if (raw == 0) d[i] = 0;
        else if (raw == 1) d[i] = 1;
        else return null;
    }
    return d[0] + d[1] + d[2] + d[3] == 0 ? null : d;
}

static double[] ExpandTheta(double[] cInv, int vertexCount, int dimG)
{
    var theta = new double[vertexCount * dimG];
    for (int v = 0; v < vertexCount; v++)
        for (int l = 0; l < dimG; l++) theta[v * dimG + l] = cInv[l];
    return theta;
}

static double[]? Solve3(double[,] a, double[] b)
{
    int n = b.Length;
    var m = (double[,])a.Clone();
    var x = (double[])b.Clone();
    for (int col = 0; col < n; col++)
    {
        int piv = col;
        for (int r = col + 1; r < n; r++)
            if (System.Math.Abs(m[r, col]) > System.Math.Abs(m[piv, col])) piv = r;
        if (System.Math.Abs(m[piv, col]) < 1e-14) return null;
        if (piv != col)
        {
            for (int c2 = 0; c2 < n; c2++) (m[col, c2], m[piv, c2]) = (m[piv, c2], m[col, c2]);
            (x[col], x[piv]) = (x[piv], x[col]);
        }
        for (int r = col + 1; r < n; r++)
        {
            double f = m[r, col] / m[col, col];
            for (int c2 = col; c2 < n; c2++) m[r, c2] -= f * m[col, c2];
            x[r] -= f * x[col];
        }
    }
    for (int r = n - 1; r >= 0; r--)
    {
        for (int c2 = r + 1; c2 < n; c2++) x[r] -= m[r, c2] * x[c2];
        x[r] /= m[r, r];
    }
    return x;
}

static double Norm(double[] v)
{
    double s = 0; foreach (double x in v) s += x * x; return System.Math.Sqrt(s);
}

static double RelDiff(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1e-30, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));

static double[] Scale(double[] a, double s)
{
    var r = new double[a.Length];
    for (int i = 0; i < a.Length; i++) r[i] = s * a[i];
    return r;
}

static double[] RandomVector(Random rng, int len)
{
    var v = new double[len];
    for (int i = 0; i < len; i++) v[i] = rng.NextDouble() - 0.5;
    double nn = Norm(v);
    for (int i = 0; i < len; i++) v[i] /= nn;
    return v;
}

static FieldTensor FaceTensor(SimplicialMesh mesh, LieAlgebra algebra, double[] coeffs) => new()
{
    Label = "coeffs",
    Signature = new TensorSignature
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "curvature-2form",
        Degree = "2",
        LieAlgebraBasisId = algebra.BasisOrderId,
        ComponentOrderId = "face-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    },
    Coefficients = coeffs,
    Shape = new[] { mesh.FaceCount, algebra.Dimension },
};

static double[] JacobiEigenvalues(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    for (int sweep = 0; sweep < 100; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < 1e-11) break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (System.Math.Abs(apq) < 1e-15) continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double tt = System.Math.Sign(tau == 0 ? 1.0 : tau) / (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
                double cc = 1.0 / System.Math.Sqrt(1.0 + tt * tt);
                double ss = tt * cc;
                for (int k = 0; k < n; k++)
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
    var values = new double[n];
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    return values;
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase448-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase448",
    ActiveGeometryBranch = "simplicial",
    ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = "trivial",
    ActiveShiabBranch = "einsteinian-shiab",
    ActiveGaugeStrategy = "penalty",
    BaseDimension = 4,
    AmbientDimension = 4,
    LieAlgebraId = "su2",
    BasisConventionId = "canonical",
    ComponentOrderId = "face-major",
    AdjointConventionId = "adjoint-explicit",
    PairingConventionId = "pairing-trace",
    NormConventionId = "norm-l2-quadrature",
    DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(),
    InsertedChoiceIds = new[] { "IX-2" },
};

GeometryContext BuildGeometry()
{
    var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
    var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 4 };
    return new GeometryContext
    {
        BaseSpace = baseSpace,
        AmbientSpace = ambientSpace,
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding
        { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
        ObservationBinding = new GeometryBinding
        { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
        Patches = Array.Empty<PatchInfo>(),
    };
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

public sealed record TorusPoint(
    double T, double SB, double OneLoop, double VEff,
    int Pos, int Zero, int Neg,
    double ThetaResidRel, double EquivResid, bool ThetaOk);

public sealed record TorusSeedRecord(int Seed, TorusPoint[] Points);

public sealed record TorusMemberRecord(string Name, bool IsControl, List<TorusSeedRecord> Seeds);

public sealed record VolumeRecord(
    int N, int Volume, int Edges, int NOmega, int NTheta, int NJoint,
    bool OrbitMapOk, double CovarianceMax, double ObjConsistencyMax, double GradFdMax,
    double IdentityThetaGradNorm, double BlockReconstructMax, double TraceConsistencyMax,
    double MaxThetaResid, double MaxEquivarianceResid, bool AllThetaOk,
    List<TorusMemberRecord> Members);
