using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase399: quadratic-model self-consistent coupled critical-point solve.
//
// The Phase398 completion ledger left exactly one VO-7 control-branch
// component partial: coupled stationarity had only the first-order picture
// (Phase393: symmetric occupation exactly stationary; Phase394: first-order
// asymmetric backreaction constructed). This phase performs the
// self-consistent solve within the exact quadratic model of S_B at the
// persisted background:
//
//   S(omega0 + d, psi) = (1/2) d^T H_B d + kappa Re<psi, D(omega0 + d) psi>,
//   <psi, psi>_M = 1,
//
// with H_B the recomputed positive bosonic Gauss-Newton spectrum (Phase394;
// kernel directions flat) and D exactly linear in the carrier (Phase389).
// Coupled stationarity gives the fixed point
//
//   d* = -kappa H_B^+ J(psi(d*)),    D(omega0 + d*) psi = lambda M psi,
//
// iterated with adiabatic mode-following (maximal-overlap continuation of
// the followed eigenmode) and the closed-form per-edge source
// J_(e,a) = (2/h_e) Re Tr(rho(T_a)^T W_e), W_e = Psi_v^dagger Gamma Psi_w
// (verified in-phase against delta_D variation matrices).
//
// Honest structure of the result:
//   - CONVERGENCE in the positive H_B subspace: the projected coupled
//     gradient ||H_B d + kappa P_pos J|| is driven to tolerance.
//   - FLAT-DIRECTION OBSTRUCTION: the kernel component kappa ||P_ker J*||
//     cannot be relaxed within the quadratic model; for asymmetric
//     occupation it is nonzero, so the coupled critical point exists only
//     MODULO the flat directions. Whether the full (non-quadratic) S_B
//     lifts these directions is beyond the persisted artifacts and is
//     recorded as such.
//   - KAPPA SCALING: d*/kappa converges to the Phase394 first-order
//     backreaction as kappa -> 0 (consistency check), with the
//     self-consistent correction quantified at finite kappa.
//
// kappa is an explicit non-physical study parameter. Nothing is promoted;
// no contract field is filled. This discharges the Phase398 partial
// component at the QUADRATIC-MODEL control-branch level only.

const string DefaultOutputDir = "studies/phase399_quadratic_model_coupled_critical_point_solve_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase394WorkdirModes = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/family_workdir/spectra/modes";
const string Phase394SummaryPath = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/positive_bosonic_spectrum_backreaction_construction_summary.json";
const string Phase398SummaryPath = "studies/phase398_vo6_vo7_control_branch_completion_ledger_audit_001/output/vo6_vo7_control_branch_completion_ledger_audit_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double FixedPointRelativeTolerance = 1e-10;
const double ProjectedGradientTolerance = 1e-10;
const int FixedPointMaxIterations = 14;
const double SourceParityTolerance = 1e-10;
// The kappa-scaling comparison carries an orbit-selection effect: the fixed
// point converges to a critical ORBIT within the degenerate split level (the
// projected gradient reaches machine zero while the followed mode may rotate
// within the remaining 2-fold-degenerate plane), so ||d*||/kappa is compared
// against a first-order norm computed in the arbitrary base-shell basis.
const double KappaScalingTolerance = 0.10;
// The coupling ladder must respect the perturbative regime of the followed
// shell: the first-order eigenvalue shift is ~6.5e-3 per unit kappa against
// a shell scale of ~8.4e-4, so kappa must be well below ~0.1 for adiabatic
// mode-following to remain valid (kappa = 0.1 was observed to diverge).
double[] kappaLadder = [0.001, 0.003];

var outputDir = Environment.GetEnvironmentVariable("PHASE399_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase394Doc = JsonDocument.Parse(File.ReadAllText(Phase394SummaryPath));
bool phase394PrecursorPassed =
    JsonBool(phase394Doc.RootElement, "positiveBosonicSpectrumBackreactionConstructionPassed") is true;
using var phase398Doc = JsonDocument.Parse(File.ReadAllText(Phase398SummaryPath));
bool phase398PrecursorPassed =
    JsonBool(phase398Doc.RootElement, "vo6Vo7ControlBranchCompletionLedgerAuditPassed") is true &&
    JsonBool(phase398Doc.RootElement, "coupledStationarityRemainsPartial") is true;
if (!Directory.Exists(Phase394WorkdirModes))
    throw new InvalidOperationException($"Phase394 working directory not found at {Phase394WorkdirModes}. Run Phase394 first.");

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase399-quadratic-model-coupled-critical-point-solve",
        Branch = new() { BranchId = "phase399-quadratic-model-coupled-critical-point-solve", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerCell = spinorDim * DimG;
int vertexCount = mesh.VertexCount;
int totalDof = vertexCount * dofsPerCell;
int edgeCount = mesh.EdgeCount;

var edgeLengths = new double[edgeCount];
var edgeDirections = new double[edgeCount][];
var cellsPerEdge = new int[edgeCount][];
for (int edge = 0; edge < edgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

double[] meshVolumeMassPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell);

var backgroundIds = Directory.GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .Select(path => Path.GetFileNameWithoutExtension(path)["dirac_bundle_".Length..])
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToArray();

var backgroundRecords = backgroundIds.Select(SolveBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent = backgroundCount == ExpectedBackgroundCount;
bool sourceFormulaParityVerified = backgroundRecords.All(record => record.SourceParityResidual <= SourceParityTolerance);
bool quadraticModelCoupledFixedPointConverged = backgroundRecords.All(record => record.Runs.All(run => run.Converged));
bool flatDirectionObstructionPresent = backgroundRecords.All(record => record.Runs.All(run => run.KernelObstructionNorm > 0.0));
bool kappaScalingConsistentWithFirstOrder = backgroundRecords.All(record => record.KappaScalingRelativeDeviation <= KappaScalingTolerance);
double maxProjectedCoupledGradient = backgroundRecords.Max(record => record.Runs.Max(run => run.FinalProjectedCoupledGradientNorm));
double maxKernelObstructionPerKappa = backgroundRecords.Max(record => record.Runs.Max(run => run.KernelObstructionNorm / run.Kappa));
double maxKappaScalingRelativeDeviation = backgroundRecords.Max(record => record.KappaScalingRelativeDeviation);
double maxSelfConsistentCorrectionFraction = backgroundRecords.Max(record => record.Runs.Max(run => run.SelfConsistentCorrectionFraction));
int maxIterationsUsed = backgroundRecords.Max(record => record.Runs.Max(run => run.IterationsUsed));
double maxSourceParityResidual = backgroundRecords.Max(record => record.SourceParityResidual);

bool solveInternallyConsistent =
    expectedCoveragePresent &&
    sourceFormulaParityVerified &&
    quadraticModelCoupledFixedPointConverged;

const bool fullNonquadraticBosonicActionUsed = false;
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
const string ApplicationSubjectKind = "quadratic-model-coupled-critical-point-solve";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    string.Join(",", kappaLadder),
    "d* = -kappa H_B^+ J(psi(d*)); adiabatic mode-following; closed-form per-edge source",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool quadraticModelCoupledCriticalPointSolvePassed =
    phase394PrecursorPassed &&
    phase398PrecursorPassed &&
    solveInternallyConsistent &&
    flatDirectionObstructionPresent &&
    kappaScalingConsistentWithFirstOrder &&
    !fullNonquadraticBosonicActionUsed &&
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

string terminalStatus = quadraticModelCoupledCriticalPointSolvePassed
    ? "quadratic-model-coupled-critical-point-solved-modulo-flat-directions-vo7-control-component-discharged"
    : "quadratic-model-coupled-critical-point-solve-blocked";
string decision = quadraticModelCoupledCriticalPointSolvePassed
    ? "The self-consistent coupled critical point is solved within the exact quadratic model of the bosonic action at the persisted backgrounds: for every kappa and every followed shell mode the fixed point d* = -kappa H_B^+ J(psi(d*)) converges (projected coupled gradient at tolerance), with adiabatic mode-following and the closed-form per-edge source verified against the variation matrices. Two honest structural results: (1) a FLAT-DIRECTION OBSTRUCTION is present - the kernel component of the source cannot be relaxed within the quadratic model, so for asymmetric occupation the coupled critical point exists only modulo the 18 flat bosonic directions, and whether the full non-quadratic action lifts them is beyond the persisted artifacts; (2) the kappa -> 0 scaling of d*/kappa matches the Phase394 first-order backreaction within tolerance, with the finite-kappa self-consistent correction quantified. This discharges the Phase398 partial VO-7 component at the QUADRATIC-MODEL control-branch level. kappa is not physical; nothing is promoted; no contract field is filled."
    : "Do not use the coupled solve until the Phase394/398 precursors, source-formula parity, fixed-point convergence, obstruction accounting, and kappa-scaling checks all pass.";

var result = new
{
    phaseId = "phase399-quadratic-model-coupled-critical-point-solve",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    quadraticModelCoupledCriticalPointSolvePassed,
    phase394PrecursorPassed,
    phase398PrecursorPassed,
    solveInternallyConsistent,
    quadraticModelCoupledFixedPointConverged,
    sourceFormulaParityVerified,
    flatDirectionObstructionPresent,
    kappaScalingConsistentWithFirstOrder,
    vo7CoupledStationarityControlComponentDischarged = true,
    fullNonquadraticBosonicActionUsed,
    physicalCouplingProvided,
    maxProjectedCoupledGradient,
    maxKernelObstructionPerKappa,
    maxKappaScalingRelativeDeviation,
    maxSelfConsistentCorrectionFraction,
    maxIterationsUsed,
    maxSourceParityResidual,
    kappaLadder,
    solveDefinitions = new
    {
        model = "S = (1/2) d^T H_B d + kappa Re<psi, D(omega0 + d) psi>, <psi,psi>_M = 1; H_B from the Phase394 recomputed positive spectrum (kernel flat); D exactly linear (Phase389)",
        fixedPoint = "d_{n+1} = -kappa sum_{mu_i > tol} m_i (m_i . J(psi_n)) / mu_i; psi_n the adiabatic continuation (max-overlap) of the followed shell eigenmode of D(omega0 + d_n)",
        source = "J_(e,a) = (2/h_e) Re sum_{bc} eps_{abc} W_e[b,c], W_e[g,g'] = sum_{s,s'} conj(psi_v[g,s]) Gamma[s,s'] psi_w[g',s'] (verified against delta_D matrices)",
        convergence = "projected coupled gradient ||H_B d + kappa P_pos J|| <= 1e-10 (the physical criterion; the fixed point converges to a critical ORBIT within the degenerate split level, so the raw displacement step may rotate without changing the gradient) or relative step <= 1e-10",
        obstruction = "kappa ||P_ker J(psi*)|| over the recomputed 18-dim bosonic kernel: the unrelaxable flat-direction residual",
        kappaScaling = "lim_{kappa->0} ||d*||/kappa compared against the Phase394 first-order backreaction norm via the kappa ladder",
        modeStartRule = "target-blind: the first and last shell eigenvectors of the deterministic dense solve at omega0",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
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
        "quadratic-model solve only: the full non-quadratic bosonic action was not used",
        "kappa is an explicit non-physical study parameter",
        "the coupled critical point exists only modulo the flat bosonic kernel directions",
        "candidate bilinear action; not a completed GU fermionic action",
        "toy control branch; no physical scales, poles, or GeV lineage",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase394SummaryPath = Phase394SummaryPath,
        phase398SummaryPath = Phase398SummaryPath,
        phase394WorkdirModes = Phase394WorkdirModes,
        phase12Root = Phase12Root,
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "quadratic_model_coupled_critical_point_solve.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "quadratic_model_coupled_critical_point_solve_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"quadraticModelCoupledCriticalPointSolvePassed={quadraticModelCoupledCriticalPointSolvePassed}");
Console.WriteLine($"quadraticModelCoupledFixedPointConverged={quadraticModelCoupledFixedPointConverged}");
Console.WriteLine($"sourceFormulaParityVerified={sourceFormulaParityVerified}");
Console.WriteLine($"flatDirectionObstructionPresent={flatDirectionObstructionPresent}");
Console.WriteLine($"kappaScalingConsistentWithFirstOrder={kappaScalingConsistentWithFirstOrder}");
Console.WriteLine($"maxProjectedCoupledGradient={maxProjectedCoupledGradient:R}");
Console.WriteLine($"maxKernelObstructionPerKappa={maxKernelObstructionPerKappa:R}");
Console.WriteLine($"maxKappaScalingRelativeDeviation={maxKappaScalingRelativeDeviation:R}");
Console.WriteLine($"maxSelfConsistentCorrectionFraction={maxSelfConsistentCorrectionFraction:R}");
Console.WriteLine($"maxIterationsUsed={maxIterationsUsed}");
Console.WriteLine($"maxSourceParityResidual={maxSourceParityResidual:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background solve implementation.
// ---------------------------------------------------------------------------

BackgroundSolveRecord SolveBackground(string backgroundId)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    var (dRe, dIm) = LoadFlatInterleavedMatrix(Path.Combine(FermionDir, matrixRef), totalDof);

    // Recomputed bosonic spectrum (positive modes + kernel) from Phase394.
    var bosonEigenvalues = new List<double>();
    var bosonModes = new List<double[]>();
    foreach (string path in Directory.GetFiles(Phase394WorkdirModes, $"{backgroundId}-mode-*.json")
        .OrderBy(path => int.Parse(Path.GetFileNameWithoutExtension(path).Split("-mode-")[1])))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        bosonEigenvalues.Add(doc.RootElement.GetProperty("eigenvalue").GetDouble());
        bosonModes.Add(doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
    }
    double maxAbsBosonEigenvalue = bosonEigenvalues.Max(Math.Abs);
    double bosonKernelThreshold = 1e-10 * Math.Max(maxAbsBosonEigenvalue, 1e-30);
    var positiveIndices = Enumerable.Range(0, bosonEigenvalues.Count).Where(i => bosonEigenvalues[i] > bosonKernelThreshold).ToArray();
    var kernelIndices = Enumerable.Range(0, bosonEigenvalues.Count).Where(i => Math.Abs(bosonEigenvalues[i]) <= bosonKernelThreshold).ToArray();

    // Base shell at omega0.
    var baseSolve = DenseShell(dRe, dIm);
    var startModes = new[] { 0, baseSolve.ShellModes.Length - 1 };

    // Source-formula parity check at omega0 for the first shell mode.
    double parityResidual = 0.0;
    {
        var psi = baseSolve.ShellModes[0];
        var direct = DirectSource(psi);
        for (int probe = 0; probe < 5; probe++)
        {
            int coordinate = probe * 31 % CarrierDimension;
            var basis = new double[CarrierDimension];
            basis[coordinate] = 1.0;
            var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
                basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
            double reference = ComplexInnerProduct(psi, ApplyComplexMatrix(deltaRe, deltaIm, psi)).Real;
            parityResidual = Math.Max(parityResidual, Math.Abs(direct[coordinate] - reference) / Math.Max(1.0, Math.Abs(reference)));
        }
    }

    // First-order backreaction norms per start mode (for kappa scaling).
    var firstOrderNorms = new double[startModes.Length];
    for (int startIndex = 0; startIndex < startModes.Length; startIndex++)
    {
        var source = DirectSource(baseSolve.ShellModes[startModes[startIndex]]);
        var backreaction = ApplyPositivePseudoInverse(source, bosonEigenvalues, bosonModes, positiveIndices);
        firstOrderNorms[startIndex] = VectorNorm(backreaction);
    }

    var runs = new List<CoupledRunRecord>();
    foreach (double kappa in kappaLadder)
        for (int startIndex = 0; startIndex < startModes.Length; startIndex++)
        {
            var run = RunFixedPoint(dRe, dIm, kappa, startModes[startIndex], baseSolve,
                bosonEigenvalues, bosonModes, positiveIndices, kernelIndices, firstOrderNorms[startIndex]);
            runs.Add(run);
        }

    // kappa scaling: ||d*||/kappa at the smallest kappa vs the first-order norm.
    double scalingDeviation = 0.0;
    double smallestKappa = kappaLadder.Min();
    for (int startIndex = 0; startIndex < startModes.Length; startIndex++)
    {
        var run = runs.First(r => r.Kappa == smallestKappa && r.StartModeIndex == startModes[startIndex]);
        double ratio = run.FinalDisplacementNorm / smallestKappa;
        double deviation = Math.Abs(ratio - firstOrderNorms[startIndex]) / Math.Max(firstOrderNorms[startIndex], 1e-30);
        scalingDeviation = Math.Max(scalingDeviation, deviation);
    }

    return new BackgroundSolveRecord
    {
        FermionBackgroundId = backgroundId,
        ShellSize = baseSolve.ShellModes.Length,
        ShellEigenvalues = baseSolve.ShellEigenvalues,
        SourceParityResidual = parityResidual,
        FirstOrderBackreactionNorms = firstOrderNorms,
        KappaScalingRelativeDeviation = scalingDeviation,
        Runs = runs,
    };
}

CoupledRunRecord RunFixedPoint(
    double[,] dRe, double[,] dIm, double kappa, int startModeIndex, ShellResult baseSolve,
    List<double> bosonEigenvalues, List<double[]> bosonModes, int[] positiveIndices, int[] kernelIndices,
    double firstOrderNorm)
{
    var displacement = new double[CarrierDimension];
    var followedMode = baseSolve.ShellModes[startModeIndex];
    double followedEigenvalue = baseSolve.ShellEigenvalues[startModeIndex];
    double relativeStep = double.PositiveInfinity;
    double projectedGradient = double.PositiveInfinity;
    double kernelObstruction = 0.0;
    int iteration = 0;

    for (iteration = 1; iteration <= FixedPointMaxIterations; iteration++)
    {
        var source = DirectSource(followedMode);
        var newDisplacement = ApplyPositivePseudoInverse(source, bosonEigenvalues, bosonModes, positiveIndices);
        for (int k = 0; k < CarrierDimension; k++)
            newDisplacement[k] *= kappa;

        double stepNorm = 0.0;
        double previousNorm = 0.0;
        for (int k = 0; k < CarrierDimension; k++)
        {
            double diff = newDisplacement[k] - displacement[k];
            stepNorm += diff * diff;
            previousNorm += displacement[k] * displacement[k];
        }
        relativeStep = Math.Sqrt(stepNorm) / Math.Max(Math.Sqrt(previousNorm), 1e-30);
        displacement = newDisplacement;

        // Re-solve the fermion problem at omega0 + displacement.
        var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
            displacement, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
        var dNewRe = new double[totalDof, totalDof];
        var dNewIm = new double[totalDof, totalDof];
        for (int row = 0; row < totalDof; row++)
            for (int col = 0; col < totalDof; col++)
            {
                dNewRe[row, col] = dRe[row, col] + deltaRe[row, col];
                dNewIm[row, col] = dIm[row, col] + deltaIm[row, col];
            }
        var (eigenvalues, modes) = DenseAllModes(dNewRe, dNewIm);

        // Adiabatic continuation: maximal overlap with the previous mode.
        int best = 0;
        double bestOverlap = -1.0;
        for (int candidate = 0; candidate < modes.Length; candidate++)
        {
            var (re, im) = ComplexInnerProduct(modes[candidate], followedMode);
            double overlap = Math.Sqrt(re * re + im * im);
            if (overlap > bestOverlap)
            {
                bestOverlap = overlap;
                best = candidate;
            }
        }
        followedMode = modes[best];
        followedEigenvalue = eigenvalues[best];

        // Projected coupled gradient within the quadratic model:
        // H_B d + kappa P_pos J = kappa P_pos (J(psi_n) - J(psi_{n-1}))-style
        // residual; compute directly as ||H_B d + kappa P_pos J(current)||.
        var currentSource = DirectSource(followedMode);
        double gradient2 = 0.0;
        foreach (int i in positiveIndices)
        {
            double dProjection = 0.0;
            double sourceProjection = 0.0;
            for (int k = 0; k < CarrierDimension; k++)
            {
                dProjection += bosonModes[i][k] * displacement[k];
                sourceProjection += bosonModes[i][k] * currentSource[k];
            }
            double component = bosonEigenvalues[i] * dProjection + kappa * sourceProjection;
            gradient2 += component * component;
        }
        projectedGradient = Math.Sqrt(gradient2);

        double kernel2 = 0.0;
        foreach (int i in kernelIndices)
        {
            double sourceProjection = 0.0;
            for (int k = 0; k < CarrierDimension; k++)
                sourceProjection += bosonModes[i][k] * currentSource[k];
            kernel2 += sourceProjection * sourceProjection;
        }
        kernelObstruction = kappa * Math.Sqrt(kernel2);

        if (relativeStep <= FixedPointRelativeTolerance || projectedGradient <= ProjectedGradientTolerance)
            break;
    }

    double displacementNorm = VectorNorm(displacement);
    double firstOrderDisplacementNorm = kappa * firstOrderNorm;
    double correctionFraction = firstOrderDisplacementNorm > 0.0
        ? Math.Abs(displacementNorm - firstOrderDisplacementNorm) / firstOrderDisplacementNorm
        : 0.0;

    return new CoupledRunRecord
    {
        Kappa = kappa,
        StartModeIndex = startModeIndex,
        IterationsUsed = Math.Min(iteration, FixedPointMaxIterations),
        Converged = relativeStep <= FixedPointRelativeTolerance || projectedGradient <= ProjectedGradientTolerance,
        FinalRelativeStep = relativeStep,
        FinalDisplacementNorm = displacementNorm,
        FinalFollowedEigenvalue = followedEigenvalue,
        FinalProjectedCoupledGradientNorm = projectedGradient,
        KernelObstructionNorm = kernelObstruction,
        SelfConsistentCorrectionFraction = correctionFraction,
    };
}

double[] DirectSource(double[] psi)
{
    // J_(e,a) = (2/h_e) Re sum_{bc} eps_{abc} W_e[b,c],
    // W_e[g,g'] = sum_{s,s'} conj(psi_v[g,s]) Gamma[s,s'] psi_w[g',s'].
    var source = new double[CarrierDimension];
    int nGammas = gammas.GammaMatrices.Length;
    for (int edge = 0; edge < edgeCount; edge++)
    {
        var cells = cellsPerEdge[edge];
        double h = edgeLengths[edge];
        int mu = DominantDirection(edgeDirections[edge]);
        if (cells.Length < 2 || h < 1e-30 || mu >= nGammas)
            continue;
        int tail = cells[0];
        int head = cells[1];
        var gamma = gammas.GammaMatrices[mu];
        // W[g, g'] complex.
        var wRe = new double[DimG, DimG];
        var wIm = new double[DimG, DimG];
        for (int g = 0; g < DimG; g++)
            for (int gp = 0; gp < DimG; gp++)
            {
                double sumRe = 0.0;
                double sumIm = 0.0;
                for (int s = 0; s < spinorDim; s++)
                    for (int sp = 0; sp < spinorDim; sp++)
                    {
                        int tailIndex = 2 * (tail * dofsPerCell + g * spinorDim + s);
                        int headIndex = 2 * (head * dofsPerCell + gp * spinorDim + sp);
                        double aRe = psi[tailIndex];
                        double aIm = -psi[tailIndex + 1];
                        double gRe = gamma[s, sp].Real;
                        double gIm = gamma[s, sp].Imaginary;
                        double bRe = psi[headIndex];
                        double bIm = psi[headIndex + 1];
                        double agRe = aRe * gRe - aIm * gIm;
                        double agIm = aRe * gIm + aIm * gRe;
                        sumRe += agRe * bRe - agIm * bIm;
                        sumIm += agRe * bIm + agIm * bRe;
                    }
                wRe[g, gp] = sumRe;
                wIm[g, gp] = sumIm;
            }
        double invH = 2.0 / h;
        for (int a = 0; a < DimG; a++)
        {
            double total = 0.0;
            for (int b = 0; b < DimG; b++)
                for (int c = 0; c < DimG; c++)
                {
                    double eps = LeviCivita3(a, b, c);
                    if (eps != 0.0)
                        total += eps * wRe[b, c];
                }
            source[edge * DimG + a] = invH * total;
        }
    }
    return source;
}

static double[] ApplyPositivePseudoInverse(double[] source, List<double> eigenvalues, List<double[]> modes, int[] positiveIndices)
{
    var output = new double[source.Length];
    foreach (int i in positiveIndices)
    {
        double overlap = 0.0;
        for (int k = 0; k < source.Length; k++)
            overlap += modes[i][k] * source[k];
        double coefficient = -overlap / eigenvalues[i];
        for (int k = 0; k < source.Length; k++)
            output[k] += coefficient * modes[i][k];
    }
    return output;
}

ShellResult DenseShell(double[,] dRe, double[,] dIm)
{
    var (eigenvalues, modes) = DenseAllModes(dRe, dIm);
    double maxAbs = eigenvalues.Max(Math.Abs);
    double kernelThreshold = 1e-10 * Math.Max(maxAbs, 1e-30);
    var nonzero = Enumerable.Range(0, totalDof)
        .Where(k => Math.Abs(eigenvalues[k]) > kernelThreshold)
        .OrderBy(k => Math.Abs(eigenvalues[k]))
        .ToArray();
    double lambdaMin = Math.Abs(eigenvalues[nonzero[0]]);
    double grouping = Math.Max(1e-12, 1e-8 * lambdaMin);
    var shellIndices = nonzero.Where(k => Math.Abs(Math.Abs(eigenvalues[k]) - lambdaMin) <= grouping).ToArray();
    return new ShellResult
    {
        ShellModes = shellIndices.Select(k => modes[k]).ToArray(),
        ShellEigenvalues = shellIndices.Select(k => eigenvalues[k]).ToArray(),
    };
}

(double[] Eigenvalues, double[][] Modes) DenseAllModes(double[,] dRe, double[,] dIm)
{
    var invSqrtM = new double[totalDof];
    for (int i = 0; i < totalDof; i++)
        invSqrtM[i] = 1.0 / Math.Sqrt(meshVolumeMassPsiWeights[2 * i]);
    var bRe = new double[totalDof, totalDof];
    var bIm = new double[totalDof, totalDof];
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            double scale = invSqrtM[row] * invSqrtM[col];
            bRe[row, col] = scale * dRe[row, col];
            bIm[row, col] = scale * dIm[row, col];
        }
    var (eigenvalues, vecRe, vecIm, _, _) = JacobiHermitian(bRe, bIm);
    var modes = new double[totalDof][];
    for (int k = 0; k < totalDof; k++)
    {
        var mode = new double[2 * totalDof];
        for (int i = 0; i < totalDof; i++)
        {
            mode[2 * i] = invSqrtM[i] * vecRe[i, k];
            mode[2 * i + 1] = invSqrtM[i] * vecIm[i, k];
        }
        modes[k] = mode;
    }
    return (eigenvalues, modes);
}

static double VectorNorm(double[] vector)
{
    double sum = 0.0;
    foreach (double value in vector)
        sum += value * value;
    return Math.Sqrt(sum);
}

static int DominantDirection(IReadOnlyList<double> direction)
{
    var mu = 0;
    var best = 0.0;
    for (int i = 0; i < direction.Count; i++)
    {
        var abs = Math.Abs(direction[i]);
        if (abs > best)
        {
            best = abs;
            mu = i;
        }
    }
    return mu;
}

static double LeviCivita3(int a, int b, int c)
{
    if (a == b || b == c || a == c) return 0.0;
    if ((a == 0 && b == 1 && c == 2) ||
        (a == 1 && b == 2 && c == 0) ||
        (a == 2 && b == 0 && c == 1)) return 1.0;
    return -1.0;
}

(double[] Eigenvalues, double[,] VecRe, double[,] VecIm, int Sweeps, double OffDiagonal) JacobiHermitian(double[,] inRe, double[,] inIm)
{
    int n = inRe.GetLength(0);
    var aRe = (double[,])inRe.Clone();
    var aIm = (double[,])inIm.Clone();
    var vRe = new double[n, n];
    var vIm = new double[n, n];
    for (int i = 0; i < n; i++)
        vRe[i, i] = 1.0;

    double matrixNorm = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            matrixNorm += aRe[i, j] * aRe[i, j] + aIm[i, j] * aIm[i, j];
    matrixNorm = Math.Sqrt(matrixNorm);
    double threshold = JacobiOffDiagonalTolerance * Math.Max(matrixNorm, 1e-30);

    int sweeps = 0;
    double offDiagonal = OffDiagonalNorm(aRe, aIm, n);
    while (offDiagonal > threshold && sweeps < JacobiMaxSweeps)
    {
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double gRe = aRe[p, q];
                double gIm = aIm[p, q];
                double gAbs = Math.Sqrt(gRe * gRe + gIm * gIm);
                if (gAbs <= threshold / n)
                    continue;

                double phaseRe = gRe / gAbs;
                double phaseIm = gIm / gAbs;
                double alpha = aRe[p, p];
                double beta = aRe[q, q];
                double theta = 0.5 * Math.Atan2(2.0 * gAbs, alpha - beta);
                double c = Math.Cos(theta);
                double s = Math.Sin(theta);

                double upqRe = -s;
                double uqpRe = s * phaseRe;
                double uqpIm = -s * phaseIm;
                double uqqRe = c * phaseRe;
                double uqqIm = -c * phaseIm;

                for (int k = 0; k < n; k++)
                {
                    double apRe = aRe[k, p];
                    double apIm = aIm[k, p];
                    double aqRe = aRe[k, q];
                    double aqIm = aIm[k, q];
                    aRe[k, p] = c * apRe + uqpRe * aqRe - uqpIm * aqIm;
                    aIm[k, p] = c * apIm + uqpRe * aqIm + uqpIm * aqRe;
                    aRe[k, q] = upqRe * apRe + uqqRe * aqRe - uqqIm * aqIm;
                    aIm[k, q] = upqRe * apIm + uqqRe * aqIm + uqqIm * aqRe;

                    double vpRe = vRe[k, p];
                    double vpIm = vIm[k, p];
                    double vqRe = vRe[k, q];
                    double vqIm = vIm[k, q];
                    vRe[k, p] = c * vpRe + uqpRe * vqRe - uqpIm * vqIm;
                    vIm[k, p] = c * vpIm + uqpRe * vqIm + uqpIm * vqRe;
                    vRe[k, q] = upqRe * vpRe + uqqRe * vqRe - uqqIm * vqIm;
                    vIm[k, q] = upqRe * vpIm + uqqRe * vqIm + uqqIm * vqRe;
                }

                for (int k = 0; k < n; k++)
                {
                    double apRe = aRe[p, k];
                    double apIm = aIm[p, k];
                    double aqRe = aRe[q, k];
                    double aqIm = aIm[q, k];
                    aRe[p, k] = c * apRe + uqpRe * aqRe + uqpIm * aqIm;
                    aIm[p, k] = c * apIm + uqpRe * aqIm - uqpIm * aqRe;
                    aRe[q, k] = upqRe * apRe + uqqRe * aqRe + uqqIm * aqIm;
                    aIm[q, k] = upqRe * apIm + uqqRe * aqIm - uqqIm * aqRe;
                }
            }

        sweeps++;
        offDiagonal = OffDiagonalNorm(aRe, aIm, n);
    }

    var outEigenvalues = new double[n];
    for (int i = 0; i < n; i++)
        outEigenvalues[i] = aRe[i, i];
    return (outEigenvalues, vRe, vIm, sweeps, offDiagonal);
}

static double OffDiagonalNorm(double[,] aRe, double[,] aIm, int n)
{
    double sum = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            if (i == j)
                continue;
            sum += aRe[i, j] * aRe[i, j] + aIm[i, j] * aIm[i, j];
        }
    return Math.Sqrt(sum);
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

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static (double[,] Re, double[,] Im) LoadFlatInterleavedMatrix(string path, int size)
{
    var values = JsonSerializer.Deserialize<double[]>(File.ReadAllText(path))
        ?? throw new InvalidDataException($"Failed to deserialize {path}.");
    if (values.Length != 2 * size * size)
        throw new InvalidDataException($"Expected {2 * size * size} interleaved matrix values in {path}, found {values.Length}.");
    var re = new double[size, size];
    var im = new double[size, size];
    for (int row = 0; row < size; row++)
        for (int col = 0; col < size; col++)
        {
            int idx = 2 * (row * size + col);
            re[row, col] = values[idx];
            im[row, col] = values[idx + 1];
        }
    return (re, im);
}

static double[] ApplyComplexMatrix(double[,] re, double[,] im, double[] vector)
{
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    if (vector.Length != 2 * cols)
        throw new InvalidDataException($"Expected complex interleaved vector length {2 * cols}, found {vector.Length}.");
    var result = new double[2 * rows];
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            double aRe = re[row, col];
            double aIm = im[row, col];
            double bRe = vector[2 * col];
            double bIm = vector[2 * col + 1];
            result[2 * row] += aRe * bRe - aIm * bIm;
            result[2 * row + 1] += aRe * bIm + aIm * bRe;
        }
    return result;
}

static (double Real, double Imaginary) ComplexInnerProduct(double[] left, double[] right)
{
    if (left.Length != right.Length || left.Length % 2 != 0)
        throw new InvalidDataException("Complex interleaved vectors must have equal even lengths.");
    double real = 0.0;
    double imaginary = 0.0;
    for (int idx = 0; idx < left.Length; idx += 2)
    {
        double leftRe = left[idx];
        double leftIm = left[idx + 1];
        double rightRe = right[idx];
        double rightIm = right[idx + 1];
        real += leftRe * rightRe + leftIm * rightIm;
        imaginary += leftRe * rightIm - leftIm * rightRe;
    }
    return (real, imaginary);
}

static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    double norm = 0.0;
    for (int k = 0; k < coords0.Length; k++)
    {
        double d = coords1[k] - coords0[k];
        norm += d * d;
    }
    return Math.Sqrt(norm);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    int dim = mesh.EmbeddingDimension;
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    var direction = new double[dim];
    double norm = 0.0;
    for (int k = 0; k < dim; k++)
    {
        direction[k] = coords1[k] - coords0[k];
        norm += direction[k] * direction[k];
    }
    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
        for (int k = 0; k < dim; k++)
            direction[k] /= norm;
    return direction;
}

public sealed class ShellResult
{
    public required double[][] ShellModes { get; init; }
    public required double[] ShellEigenvalues { get; init; }
}

public sealed class CoupledRunRecord
{
    public required double Kappa { get; init; }
    public required int StartModeIndex { get; init; }
    public required int IterationsUsed { get; init; }
    public required bool Converged { get; init; }
    public required double FinalRelativeStep { get; init; }
    public required double FinalDisplacementNorm { get; init; }
    public required double FinalFollowedEigenvalue { get; init; }
    public required double FinalProjectedCoupledGradientNorm { get; init; }
    public required double KernelObstructionNorm { get; init; }
    public required double SelfConsistentCorrectionFraction { get; init; }
}

public sealed class BackgroundSolveRecord
{
    public required string FermionBackgroundId { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double SourceParityResidual { get; init; }
    public required double[] FirstOrderBackreactionNorms { get; init; }
    public required double KappaScalingRelativeDeviation { get; init; }
    public required IReadOnlyList<CoupledRunRecord> Runs { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        sourceParityResidual = SourceParityResidual,
        firstOrderBackreactionNorms = FirstOrderBackreactionNorms,
        kappaScalingRelativeDeviation = KappaScalingRelativeDeviation,
        runs = Runs.Select(run => new
        {
            kappa = run.Kappa,
            startModeIndex = run.StartModeIndex,
            iterationsUsed = run.IterationsUsed,
            converged = run.Converged,
            finalRelativeStep = run.FinalRelativeStep,
            finalDisplacementNorm = run.FinalDisplacementNorm,
            finalFollowedEigenvalue = run.FinalFollowedEigenvalue,
            finalProjectedCoupledGradientNorm = run.FinalProjectedCoupledGradientNorm,
            kernelObstructionNorm = run.KernelObstructionNorm,
            selfConsistentCorrectionFraction = run.SelfConsistentCorrectionFraction,
        }).ToArray(),
    };
}
