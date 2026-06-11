using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase4.Dirac;
using Gu.Phase4.Spin;
using Gu.ReferenceCpu;
using Gu.Solvers;

// Phase400: full-bosonic-action flat-direction lift probe.
//
// Phase399 solved the self-consistent coupled critical point within the exact
// QUADRATIC model of the bosonic objective and recorded one residual physical
// question for the VO-7 coupled-stationarity component: the kernel component
// of the fermionic source (0.047 per unit kappa) is unrelaxable within the
// quadratic model, so the coupled critical point exists only MODULO the 18
// flat bosonic directions - and whether the full non-quadratic bosonic action
// lifts them was "beyond the persisted artifacts".
//
// This phase answers that question exactly, using the repo's own production
// residual assembly. On the toy control branch the full bosonic objective is
//
//   S(omega) = (1/2) <Upsilon(omega), M_R Upsilon(omega)>
//            + (lambda/2) ||d^*(omega - omega_ref)||^2,
//   Upsilon(omega) = F(omega) - T^aug(omega),  F = d omega + (1/2)[omega, omega],
//
// which is QUARTIC in omega (Upsilon is quadratic). For a Gauss-Newton kernel
// direction k (J k = 0 and d^* k = 0, both verified in-phase), the expansion
// terminates exactly:
//
//   S(omega0 + t k) - S(omega0)
//     = (t^2/2) <Upsilon0, M Q(k,k)> + (t^4/8) <Q(k,k), M Q(k,k)>,
//   Q(k,k) = d^2 Upsilon(k,k)  (the curvature term Gauss-Newton drops),
//
// with NO truncation error in the symmetric second difference
// Q(k,k) = (Upsilon(omega0 + t k) + Upsilon(omega0 - t k) - 2 Upsilon0)/t^2,
// because Upsilon is exactly quadratic. The probe therefore:
//
//   1. verifies the kernel basis (orthonormality, J- and d^*-annihilation),
//      the GN operator parity on positive modes, and the exact-quadraticity
//      of the second differences;
//   2. assembles the GN-dropped curvature form B_ij = <Upsilon0, M Q(k_i,k_j)>
//      on the 18-dimensional kernel by polarization and diagonalizes it;
//   3. classifies every kernel eigendirection: quadratically lifted
//      (beta > tol), SADDLE (beta < -tol: the full action DECREASES - the GN
//      critical point is not a local minimum of the full objective there),
//      quartically lifted (beta ~ 0, Q(d,d) != 0), or EXACTLY FLAT
//      (Q(d,d) = 0: flat to all orders of the full toy bosonic objective);
//   4. measures the gauge alignment of each eigendirection against the
//      infinitesimal gauge orbit map R_{z*} at the background;
//   5. decomposes the Phase393/394/399 fermionic source kernel component
//      (the flat-direction obstruction) onto the classified eigendirections,
//      answering whether the obstruction is relaxable at higher order in the
//      full action or genuinely unrelaxable.
//
// Fail-closed: the probe passes when the measurement battery is internally
// verified - whichever way the physical answer comes out. The toy objective
// is still the control-branch Mode-B objective, not a physical GU action; no
// coupling is physical; nothing is promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase400_full_bosonic_action_flat_direction_lift_probe_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase394Workdir = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/family_workdir";
const string Phase394WorkdirModes = $"{Phase394Workdir}/spectra/modes";
const string Phase394SummaryPath = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/positive_bosonic_spectrum_backreaction_construction_summary.json";
const string Phase399SummaryPath = "studies/phase399_quadratic_model_coupled_critical_point_solve_001/output/quadratic_model_coupled_critical_point_solve_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int FullModeCount = 156;
const int ExpectedKernelDimension = 18;
const int ExpectedShellSize = 4;
const double GaugeLambda = 0.1; // matches LinearizedOperatorSpec default used by compute-spectrum
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double SecondDifferenceStep = 0.5;
const double SecondDifferenceCheckStep = 0.25;
const double OrthonormalityTolerance = 1e-8;
const double KernelAnnihilationTolerance = 1e-7;
const double QuadraticExactnessTolerance = 1e-9;
const double GnParityRelativeTolerance = 1e-6;
const double PolarizationConsistencyTolerance = 1e-8;
const double GaugeBasisDropTolerance = 1e-10;
const double GaugeAlignmentThreshold = 0.99;

var outputDir = Environment.GetEnvironmentVariable("PHASE400_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase394Doc = JsonDocument.Parse(File.ReadAllText(Phase394SummaryPath));
bool phase394PrecursorPassed =
    JsonBool(phase394Doc.RootElement, "positiveBosonicSpectrumBackreactionConstructionPassed") is true &&
    JsonBool(phase394Doc.RootElement, "bosonicGaussNewtonPsdVerified") is true;
using var phase399Doc = JsonDocument.Parse(File.ReadAllText(Phase399SummaryPath));
bool phase399PrecursorPassed =
    JsonBool(phase399Doc.RootElement, "quadraticModelCoupledCriticalPointSolvePassed") is true &&
    JsonBool(phase399Doc.RootElement, "flatDirectionObstructionPresent") is true &&
    JsonBool(phase399Doc.RootElement, "fullNonquadraticBosonicActionUsed") is false;
if (!Directory.Exists(Phase394WorkdirModes))
    throw new InvalidOperationException($"Phase394 working directory not found at {Phase394WorkdirModes}. Run Phase394 first.");

// ---------------------------------------------------------------------------
// Shared geometry (identical to the compute-spectrum / Phase394 / Phase399 path).
// ---------------------------------------------------------------------------

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int edgeCount = mesh.EdgeCount;
int vertexCount = mesh.VertexCount;
if (edgeCount * DimG != CarrierDimension)
    throw new InvalidOperationException($"Carrier dimension mismatch: {edgeCount * DimG} != {CarrierDimension}.");

var torsion = new TrivialTorsionCpu(mesh, algebra);
var shiab = new IdentityShiabCpu(mesh, algebra);
var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
var residualMass = new CpuMassMatrix(mesh, algebra);
var gaugePenalty = new CoulombGaugePenalty(mesh, DimG, GaugeLambda);

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase400-full-bosonic-action-flat-direction-lift-probe",
        Branch = new() { BranchId = "phase400-full-bosonic-action-flat-direction-lift-probe", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerCell = spinorDim * DimG;
int totalDof = vertexCount * dofsPerCell;

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

var geometry = GuJsonDefaults.Deserialize<GeometryContext>(
        File.ReadAllText(Path.Combine(Phase394Workdir, "manifest", "geometry.json")))
    ?? throw new InvalidDataException("Failed to deserialize the family geometry context.");

var backgroundIds = Directory.GetFiles(Path.Combine(Phase394Workdir, "background_records"), "bg-*.json")
    .Select(Path.GetFileNameWithoutExtension)
    .OrderBy(id => id, StringComparer.Ordinal)
    .Select(id => id!)
    .ToArray();

var backgroundRecords = backgroundIds.Select(ProbeBackground).ToArray();

// ---------------------------------------------------------------------------
// Aggregation.
// ---------------------------------------------------------------------------

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record => record.KernelDimension == ExpectedKernelDimension && record.ShellSize == ExpectedShellSize);
bool modeOrthonormalityVerified = backgroundRecords.All(record => record.MaxGramDeviation <= OrthonormalityTolerance);
bool kernelAnnihilationVerified = backgroundRecords.All(record => record.MaxKernelQuadraticForm <= KernelAnnihilationTolerance);
bool quadraticExactnessVerified = backgroundRecords.All(record => record.MaxQuadraticExactnessResidual <= QuadraticExactnessTolerance);
bool gnOperatorParityVerified = backgroundRecords.All(record => record.MaxGnParityRelativeDeviation <= GnParityRelativeTolerance);
bool polarizationConsistencyVerified = backgroundRecords.All(record => record.MaxPolarizationResidual <= PolarizationConsistencyTolerance);

int totalKernelDirections = backgroundRecords.Sum(record => record.KernelDimension);
int totalQuadraticallyLifted = backgroundRecords.Sum(record => record.QuadraticallyLiftedCount);
int totalSaddle = backgroundRecords.Sum(record => record.SaddleCount);
int totalQuarticallyLifted = backgroundRecords.Sum(record => record.QuarticallyLiftedCount);
int totalExactlyFlat = backgroundRecords.Sum(record => record.ExactlyFlatCount);
bool allFlatDirectionsLifted = totalExactlyFlat == 0;
bool anySaddleDirectionPresent = totalSaddle > 0;
double minObstructionLiftedFraction = backgroundRecords.Min(record => record.ObstructionLiftedFractions.Min());
double maxObstructionExactlyFlatFraction = backgroundRecords.Max(record => record.ObstructionExactlyFlatFractions.Max());
int totalGaugeAlignedExactlyFlat = backgroundRecords.Sum(record => record.GaugeAlignedExactlyFlatCount);
double maxResidualNormAtBackground = backgroundRecords.Max(record => record.ResidualNormAtBackground);
double maxAbsCurvatureFormEntry = backgroundRecords.Max(record => record.MaxAbsCurvatureFormEntry);
double maxKernelEigenvalueBound = backgroundRecords.Max(record => record.MaxKernelEigenvalue);
double maxSaddleDepth = backgroundRecords.Max(record => record.MaxSaddleDepth);
double minQuarticNorm = backgroundRecords.Min(record => record.MinQuarticNorm);
bool quadraticCoefficientsResidualScaleBounded = backgroundRecords.All(record => record.BetaWithinCauchySchwarzBound);

string liftVerdict = allFlatDirectionsLifted
    ? (anySaddleDirectionPresent
        ? "all-lifted-with-saddle-directions"
        : "all-lifted")
    : (totalExactlyFlat == totalKernelDirections
        ? "none-lifted"
        : "partially-lifted-exactly-flat-residual-present");
bool kernelObstructionFullyRelaxableAtHigherOrder = maxObstructionExactlyFlatFraction <= 1e-8;

bool probeInternallyConsistent =
    expectedCoveragePresent &&
    modeOrthonormalityVerified &&
    kernelAnnihilationVerified &&
    quadraticExactnessVerified &&
    gnOperatorParityVerified &&
    polarizationConsistencyVerified &&
    quadraticCoefficientsResidualScaleBounded;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool physicalBosonicActionUsed = false;
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
const string ApplicationSubjectKind = "full-bosonic-action-flat-direction-lift-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    SecondDifferenceStep.ToString("R"),
    "S(omega0+tk)-S0 = (t^2/2)<U0,M Q(k,k)> + (t^4/8)<Q,MQ>; Q by exact symmetric second difference; B_ij = <U0, M Q(k_i,k_j)> by polarization",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool fullBosonicActionFlatDirectionLiftProbePassed =
    phase394PrecursorPassed &&
    phase399PrecursorPassed &&
    probeInternallyConsistent &&
    !physicalBosonicActionUsed &&
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

string terminalStatus = fullBosonicActionFlatDirectionLiftProbePassed
    ? $"full-bosonic-action-flat-direction-lift-probe-completed-{liftVerdict}"
    : "full-bosonic-action-flat-direction-lift-probe-blocked";
string decision = fullBosonicActionFlatDirectionLiftProbePassed
    ? "The Phase399 residual question is answered exactly on the toy control branch: because the production residual Upsilon is exactly quadratic in omega, the full bosonic objective restricted to the 18 Gauss-Newton flat directions terminates at quartic order and every term is computable without truncation error. The GN-dropped curvature form B_ij = <Upsilon0, M Q(k_i,k_j)> was assembled by polarization (verified against direct second differences), diagonalized, and every kernel eigendirection was classified as quadratically lifted, saddle, quartically lifted, or exactly flat, with gauge alignment measured against the infinitesimal gauge-orbit map at the background. The fermionic source kernel component (the Phase399 flat-direction obstruction) was decomposed onto the classified eigendirections to decide whether the obstruction is relaxable at higher order in the full action. The verdict fields carry the physical answer; the probe gates only on the verified measurement battery. The toy objective remains the control-branch Mode-B objective, not a physical GU action; nothing is promoted; no contract field is filled."
    : "Do not use the lift classification until the Phase394/399 precursors and the full measurement battery (orthonormality, kernel annihilation, exact quadraticity, GN parity, polarization consistency) pass.";

var result = new
{
    phaseId = "phase400-full-bosonic-action-flat-direction-lift-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    fullBosonicActionFlatDirectionLiftProbePassed,
    phase394PrecursorPassed,
    phase399PrecursorPassed,
    probeInternallyConsistent,
    expectedCoveragePresent,
    modeOrthonormalityVerified,
    kernelAnnihilationVerified,
    quadraticExactnessVerified,
    gnOperatorParityVerified,
    polarizationConsistencyVerified,
    liftVerdict,
    allFlatDirectionsLifted,
    anySaddleDirectionPresent,
    kernelObstructionFullyRelaxableAtHigherOrder,
    totalKernelDirections,
    totalQuadraticallyLifted,
    totalSaddle,
    totalQuarticallyLifted,
    totalExactlyFlat,
    totalGaugeAlignedExactlyFlat,
    minObstructionLiftedFraction,
    maxObstructionExactlyFlatFraction,
    maxResidualNormAtBackground,
    maxAbsCurvatureFormEntry,
    maxKernelEigenvalueBound,
    maxSaddleDepth,
    minQuarticNorm,
    quadraticCoefficientsResidualScaleBounded,
    physicalBosonicActionUsed,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        objective = "S(omega) = (1/2)<Upsilon(omega), M_R Upsilon(omega)> + (lambda/2)||d^*(omega - ref)||^2 with lambda = 0.1 (the compute-spectrum operator's penalty); Upsilon = F - T^aug exactly quadratic in omega on the toy branch",
        exactExpansion = "for kernel directions (J k = 0, d^* k = 0): S(omega0 + t k) - S0 = (t^2/2)<Upsilon0, M Q(k,k)> + (t^4/8)<Q(k,k), M Q(k,k)> with zero truncation error",
        secondDifference = "Q(k,k) = (Upsilon(omega0 + t k) + Upsilon(omega0 - t k) - 2 Upsilon0)/t^2, exact for quadratic Upsilon at any t; cross-checked at t = 0.5 and t = 0.25",
        curvatureForm = "B_ij = <Upsilon0, M Q(k_i, k_j)> by polarization Q(u,v) = (Q(u+v,u+v) - Q(u,u) - Q(v,v))/2; this is exactly the term Gauss-Newton drops",
        classification = "per B-eigendirection d: beta > tol => quadratically lifted; beta < -tol => saddle; |beta| <= tol and ||Q(d,d)||_M > tol => quartically lifted; else exactly flat",
        gaugeAlignment = "squared projection onto the orthonormalized range of the infinitesimal gauge map R_{z*}(xi) = d xi + [omega*, xi] at the background",
        obstructionDecomposition = "the kernel component of the fermionic shell source J^(s) expressed in the B-eigenbasis; lifted fraction = weight on non-exactly-flat directions",
        saddleScaleAccounting = "|beta| <= ||Upsilon0||_M ||Q(d,d)||_M (Cauchy-Schwarz, asserted): the quadratic coefficients are bounded by the converged background residual, and any saddle has depth beta^2 / (2 ||Q||_M^2) - residual-scale, not action-scale",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
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
        "the full objective probed is the toy control-branch Mode-B objective (quartic in omega), not a physical GU bosonic action",
        "the lift classification is a structural statement about the toy branch only",
        "no physical coupling constant; kappa remains a non-physical study parameter",
        "candidate bilinear fermionic action; not a completed GU fermionic action",
        "toy control branch; no physical scales, poles, or GeV lineage",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase394SummaryPath = Phase394SummaryPath,
        phase399SummaryPath = Phase399SummaryPath,
        phase394Workdir = Phase394Workdir,
        phase12Root = Phase12Root,
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "full_bosonic_action_flat_direction_lift_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "full_bosonic_action_flat_direction_lift_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fullBosonicActionFlatDirectionLiftProbePassed={fullBosonicActionFlatDirectionLiftProbePassed}");
Console.WriteLine($"liftVerdict={liftVerdict}");
Console.WriteLine($"allFlatDirectionsLifted={allFlatDirectionsLifted}");
Console.WriteLine($"anySaddleDirectionPresent={anySaddleDirectionPresent}");
Console.WriteLine($"kernelObstructionFullyRelaxableAtHigherOrder={kernelObstructionFullyRelaxableAtHigherOrder}");
Console.WriteLine($"totalKernelDirections={totalKernelDirections}");
Console.WriteLine($"totalQuadraticallyLifted={totalQuadraticallyLifted}");
Console.WriteLine($"totalSaddle={totalSaddle}");
Console.WriteLine($"totalQuarticallyLifted={totalQuarticallyLifted}");
Console.WriteLine($"totalExactlyFlat={totalExactlyFlat}");
Console.WriteLine($"totalGaugeAlignedExactlyFlat={totalGaugeAlignedExactlyFlat}");
Console.WriteLine($"minObstructionLiftedFraction={minObstructionLiftedFraction:R}");
Console.WriteLine($"maxObstructionExactlyFlatFraction={maxObstructionExactlyFlatFraction:R}");
Console.WriteLine($"maxResidualNormAtBackground={maxResidualNormAtBackground:R}");
Console.WriteLine($"minQuarticNorm={minQuarticNorm:R}");
Console.WriteLine($"maxSaddleDepth={maxSaddleDepth:R}");
Console.WriteLine($"quadraticCoefficientsResidualScaleBounded={quadraticCoefficientsResidualScaleBounded}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Per-background probe.
// ---------------------------------------------------------------------------

BackgroundProbeRecord ProbeBackground(string backgroundId)
{
    // Background state, manifest (production artifacts staged by Phase394).
    var manifest = GuJsonDefaults.Deserialize<BranchManifest>(
            File.ReadAllText(Path.Combine(Phase394Workdir, "background_states", $"{backgroundId}_manifest.json")))
        ?? throw new InvalidDataException($"Failed to deserialize manifest for {backgroundId}.");
    var omega0 = GuJsonDefaults.Deserialize<FieldTensor>(
            File.ReadAllText(Path.Combine(Phase394Workdir, "background_states", $"{backgroundId}_omega.json")))
        ?? throw new InvalidDataException($"Failed to deserialize omega for {backgroundId}.");
    var a0 = GuJsonDefaults.Deserialize<FieldTensor>(
            File.ReadAllText(Path.Combine(Phase394Workdir, "background_states", "a0.json")))
        ?? throw new InvalidDataException("Failed to deserialize a0.");
    if (omega0.Coefficients.Length != CarrierDimension || a0.Coefficients.Length != CarrierDimension)
        throw new InvalidDataException($"State dimension mismatch for {backgroundId}.");
    var a0Conn = new ConnectionField(mesh, algebra, (double[])a0.Coefficients.Clone());

    // Residual evaluation closure (production assembly path).
    FieldTensor ResidualAt(double[] coefficients)
    {
        var conn = new ConnectionField(mesh, algebra, (double[])coefficients.Clone());
        return assembler.AssembleDerivedState(conn, a0Conn, manifest, geometry).ResidualUpsilon;
    }

    var upsilon0 = ResidualAt(omega0.Coefficients);
    double residualNormAtBackground = System.Math.Sqrt(residualMass.InnerProduct(upsilon0, upsilon0));
    double objectiveAtBackground = gaugePenalty.AddToObjective(residualMass.EvaluateObjective(upsilon0), omega0);

    // Recomputed bosonic spectrum (Phase394 working directory).
    var bosonEigenvalues = new List<double>();
    var bosonModes = new List<double[]>();
    foreach (string path in Directory.GetFiles(Phase394WorkdirModes, $"{backgroundId}-mode-*.json")
        .OrderBy(path => int.Parse(Path.GetFileNameWithoutExtension(path).Split("-mode-")[1])))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        bosonEigenvalues.Add(doc.RootElement.GetProperty("eigenvalue").GetDouble());
        bosonModes.Add(doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
    }
    if (bosonEigenvalues.Count != FullModeCount)
        throw new InvalidDataException($"Expected {FullModeCount} recomputed modes for {backgroundId}, found {bosonEigenvalues.Count}.");
    double maxAbsBosonEigenvalue = bosonEigenvalues.Max(System.Math.Abs);
    double bosonKernelThreshold = 1e-10 * System.Math.Max(maxAbsBosonEigenvalue, 1e-30);
    var kernelIndices = Enumerable.Range(0, bosonEigenvalues.Count).Where(i => System.Math.Abs(bosonEigenvalues[i]) <= bosonKernelThreshold).ToArray();
    var positiveIndices = Enumerable.Range(0, bosonEigenvalues.Count).Where(i => bosonEigenvalues[i] > bosonKernelThreshold).ToArray();
    int kernelDimension = kernelIndices.Length;
    double maxKernelEigenvalue = kernelIndices.Length > 0 ? kernelIndices.Max(i => System.Math.Abs(bosonEigenvalues[i])) : 0.0;
    var kernelBasis = kernelIndices.Select(i => bosonModes[i]).ToArray();

    // --- Verification battery -------------------------------------------------

    // 1. Plain-orthonormality of the kernel basis (M_state is uniform-unit).
    double maxGramDeviation = 0.0;
    for (int i = 0; i < kernelDimension; i++)
        for (int j = i; j < kernelDimension; j++)
        {
            double dot = Dot(kernelBasis[i], kernelBasis[j]);
            maxGramDeviation = System.Math.Max(maxGramDeviation, System.Math.Abs(dot - (i == j ? 1.0 : 0.0)));
        }

    // 2. Kernel annihilation: full GN quadratic form <J k, M J k> + lambda ||d^* k||^2.
    //    J k is recovered exactly from the symmetric first difference because
    //    Upsilon is quadratic: J k = (Upsilon(+t) - Upsilon(-t)) / (2t).
    double maxKernelQuadraticForm = 0.0;
    var kernelSecondDifferences = new FieldTensor[kernelDimension];
    double maxQuadraticExactnessResidual = 0.0;
    for (int i = 0; i < kernelDimension; i++)
    {
        var (jk, q) = FirstAndSecondDifference(ResidualAt, omega0.Coefficients, kernelBasis[i], upsilon0, SecondDifferenceStep);
        var (_, qCheck) = FirstAndSecondDifference(ResidualAt, omega0.Coefficients, kernelBasis[i], upsilon0, SecondDifferenceCheckStep);
        double qNorm = System.Math.Sqrt(residualMass.InnerProduct(q, q));
        double qDiff = 0.0;
        for (int c = 0; c < q.Coefficients.Length; c++)
        {
            double diff = q.Coefficients[c] - qCheck.Coefficients[c];
            qDiff += diff * diff;
        }
        maxQuadraticExactnessResidual = System.Math.Max(
            maxQuadraticExactnessResidual, System.Math.Sqrt(qDiff) / System.Math.Max(qNorm, 1.0));
        kernelSecondDifferences[i] = q;

        double jkNormSq = residualMass.InnerProduct(jk, jk);
        var dStarK = gaugePenalty.ApplyCodifferential(kernelBasis[i]);
        double penaltyForm = GaugeLambda * Dot(dStarK, dStarK);
        maxKernelQuadraticForm = System.Math.Max(maxKernelQuadraticForm, jkNormSq + penaltyForm);
    }

    // 3. GN operator parity on a positive-mode sample: <J m, M J m> + lambda||d^* m||^2 == mu.
    double maxGnParityRelativeDeviation = 0.0;
    var paritySample = new[]
    {
        positiveIndices[0],
        positiveIndices[positiveIndices.Length / 2],
        positiveIndices[^1],
    };
    foreach (int modeIndex in paritySample)
    {
        var (jm, _) = FirstAndSecondDifference(ResidualAt, omega0.Coefficients, bosonModes[modeIndex], upsilon0, SecondDifferenceStep);
        var dStarM = gaugePenalty.ApplyCodifferential(bosonModes[modeIndex]);
        double quadraticForm = residualMass.InnerProduct(jm, jm) + GaugeLambda * Dot(dStarM, dStarM);
        double mu = bosonEigenvalues[modeIndex];
        maxGnParityRelativeDeviation = System.Math.Max(
            maxGnParityRelativeDeviation, System.Math.Abs(quadraticForm - mu) / System.Math.Max(System.Math.Abs(mu), 1e-30));
    }

    // --- GN-dropped curvature form on the kernel ------------------------------

    var curvatureForm = new double[kernelDimension, kernelDimension];
    for (int i = 0; i < kernelDimension; i++)
        curvatureForm[i, i] = residualMass.InnerProduct(upsilon0, kernelSecondDifferences[i]);
    for (int i = 0; i < kernelDimension; i++)
        for (int j = i + 1; j < kernelDimension; j++)
        {
            var combined = new double[CarrierDimension];
            for (int c = 0; c < CarrierDimension; c++)
                combined[c] = kernelBasis[i][c] + kernelBasis[j][c];
            var (_, qSum) = FirstAndSecondDifference(ResidualAt, omega0.Coefficients, combined, upsilon0, SecondDifferenceStep);
            var qCross = new double[qSum.Coefficients.Length];
            for (int c = 0; c < qCross.Length; c++)
                qCross[c] = 0.5 * (qSum.Coefficients[c]
                    - kernelSecondDifferences[i].Coefficients[c]
                    - kernelSecondDifferences[j].Coefficients[c]);
            var qCrossTensor = WrapLike(upsilon0, qCross);
            double entry = residualMass.InnerProduct(upsilon0, qCrossTensor);
            curvatureForm[i, j] = entry;
            curvatureForm[j, i] = entry;
        }
    double maxAbsCurvatureFormEntry = 0.0;
    for (int i = 0; i < kernelDimension; i++)
        for (int j = 0; j < kernelDimension; j++)
            maxAbsCurvatureFormEntry = System.Math.Max(maxAbsCurvatureFormEntry, System.Math.Abs(curvatureForm[i, j]));

    var (curvatureEigenvalues, curvatureVectorsRe, _, _, _) = JacobiHermitian(curvatureForm, new double[kernelDimension, kernelDimension]);
    var eigenOrder = Enumerable.Range(0, kernelDimension).OrderBy(i => curvatureEigenvalues[i]).ToArray();

    // --- Gauge-orbit basis -----------------------------------------------------

    var gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omega0);
    var gaugeBasis = new List<double[]>();
    var xiTemplate = new FieldTensor
    {
        Label = "xi",
        Signature = gaugeMap.InputSignature,
        Coefficients = new double[vertexCount * DimG],
        Shape = new[] { vertexCount, DimG },
    };
    for (int j = 0; j < vertexCount * DimG; j++)
    {
        var xiCoeffs = new double[vertexCount * DimG];
        xiCoeffs[j] = 1.0;
        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = xiTemplate.Signature,
            Coefficients = xiCoeffs,
            Shape = xiTemplate.Shape,
        };
        var column = (double[])gaugeMap.Apply(xi).Coefficients.Clone();
        foreach (var existing in gaugeBasis)
        {
            double overlap = Dot(existing, column);
            for (int c = 0; c < column.Length; c++)
                column[c] -= overlap * existing[c];
        }
        double norm = System.Math.Sqrt(Dot(column, column));
        if (norm > GaugeBasisDropTolerance)
        {
            for (int c = 0; c < column.Length; c++)
                column[c] /= norm;
            gaugeBasis.Add(column);
        }
    }

    // --- Eigendirection classification -----------------------------------------

    double betaTolerance = System.Math.Max(1e-12, 1e-6 * System.Math.Max(maxAbsCurvatureFormEntry, 1e-30));
    var directionRecords = new List<KernelDirectionRecord>();
    double maxPolarizationResidual = 0.0;
    double maxQuarticNorm = 0.0;
    var quarticNorms = new double[kernelDimension];
    var carrierDirections = new double[kernelDimension][];
    for (int rank = 0; rank < kernelDimension; rank++)
    {
        int eigIdx = eigenOrder[rank];
        var direction = new double[CarrierDimension];
        for (int i = 0; i < kernelDimension; i++)
        {
            double coefficient = curvatureVectorsRe[i, eigIdx];
            for (int c = 0; c < CarrierDimension; c++)
                direction[c] += coefficient * kernelBasis[i][c];
        }
        carrierDirections[rank] = direction;
        var (_, qdd) = FirstAndSecondDifference(ResidualAt, omega0.Coefficients, direction, upsilon0, SecondDifferenceStep);
        double directBeta = residualMass.InnerProduct(upsilon0, qdd);
        maxPolarizationResidual = System.Math.Max(
            maxPolarizationResidual,
            System.Math.Abs(directBeta - curvatureEigenvalues[eigIdx]) / System.Math.Max(maxAbsCurvatureFormEntry, 1e-12));
        quarticNorms[rank] = System.Math.Sqrt(residualMass.InnerProduct(qdd, qdd));
        maxQuarticNorm = System.Math.Max(maxQuarticNorm, quarticNorms[rank]);
    }
    double quarticTolerance = System.Math.Max(1e-12, 1e-6 * System.Math.Max(maxQuarticNorm, 1e-30));

    int quadraticallyLiftedCount = 0;
    int saddleCount = 0;
    int quarticallyLiftedCount = 0;
    int exactlyFlatCount = 0;
    int gaugeAlignedExactlyFlatCount = 0;
    double maxSaddleDepth = 0.0;
    double minQuarticNorm = double.PositiveInfinity;
    // Cauchy-Schwarz: |beta| = |<U0, M Q(d,d)>| <= ||U0||_M ||Q(d,d)||_M, so the
    // quadratic coefficients are bounded by the (converged, near-zero) background
    // residual - any saddle structure is residual-scale, not action-scale.
    double cauchySchwarzBetaBound = 0.0;
    var classifications = new string[kernelDimension];
    for (int rank = 0; rank < kernelDimension; rank++)
    {
        double beta = curvatureEigenvalues[eigenOrder[rank]];
        double quarticNorm = quarticNorms[rank];
        minQuarticNorm = System.Math.Min(minQuarticNorm, quarticNorm);
        cauchySchwarzBetaBound = System.Math.Max(cauchySchwarzBetaBound, residualNormAtBackground * quarticNorm);
        if (beta < 0.0 && quarticNorm > 0.0)
            maxSaddleDepth = System.Math.Max(maxSaddleDepth, beta * beta / (2.0 * quarticNorm * quarticNorm));
        double gaugeFraction = 0.0;
        foreach (var g in gaugeBasis)
        {
            double overlap = Dot(g, carrierDirections[rank]);
            gaugeFraction += overlap * overlap;
        }

        string classification;
        if (beta > betaTolerance)
        {
            classification = "quadratically-lifted";
            quadraticallyLiftedCount++;
        }
        else if (beta < -betaTolerance)
        {
            classification = "saddle";
            saddleCount++;
        }
        else if (quarticNorm > quarticTolerance)
        {
            classification = "quartically-lifted";
            quarticallyLiftedCount++;
        }
        else
        {
            classification = "exactly-flat";
            exactlyFlatCount++;
            if (gaugeFraction >= GaugeAlignmentThreshold)
                gaugeAlignedExactlyFlatCount++;
        }
        classifications[rank] = classification;
        directionRecords.Add(new KernelDirectionRecord
        {
            Rank = rank,
            QuadraticCoefficient = beta,
            QuarticResidualNorm = quarticNorm,
            GaugeFraction = gaugeFraction,
            Classification = classification,
        });
    }

    // --- Fermionic source obstruction decomposition -----------------------------

    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    var (dRe, dIm) = LoadFlatInterleavedMatrix(Path.Combine(FermionDir, matrixRef), totalDof);
    var baseSolve = DenseShell(dRe, dIm);
    int shellSize = baseSolve.ShellModes.Length;

    var obstructionLiftedFractions = new double[shellSize];
    var obstructionExactlyFlatFractions = new double[shellSize];
    var sourceKernelFractions = new double[shellSize];
    for (int s = 0; s < shellSize; s++)
    {
        var source = DirectSource(baseSolve.ShellModes[s]);
        double sourceNormSq = Dot(source, source);
        double kernelWeightSq = 0.0;
        double liftedWeightSq = 0.0;
        double exactlyFlatWeightSq = 0.0;
        for (int rank = 0; rank < kernelDimension; rank++)
        {
            double coefficient = Dot(carrierDirections[rank], source);
            double weight = coefficient * coefficient;
            kernelWeightSq += weight;
            if (classifications[rank] == "exactly-flat")
                exactlyFlatWeightSq += weight;
            else
                liftedWeightSq += weight;
        }
        sourceKernelFractions[s] = sourceNormSq > 0.0 ? System.Math.Sqrt(kernelWeightSq / sourceNormSq) : 0.0;
        obstructionLiftedFractions[s] = kernelWeightSq > 0.0 ? liftedWeightSq / kernelWeightSq : 1.0;
        obstructionExactlyFlatFractions[s] = kernelWeightSq > 0.0 ? exactlyFlatWeightSq / kernelWeightSq : 0.0;
    }

    bool betaWithinCauchySchwarzBound = true;
    for (int rank = 0; rank < kernelDimension; rank++)
        if (System.Math.Abs(curvatureEigenvalues[eigenOrder[rank]]) > 1.0001 * residualNormAtBackground * quarticNorms[rank] + 1e-13)
            betaWithinCauchySchwarzBound = false;

    return new BackgroundProbeRecord
    {
        FermionBackgroundId = backgroundId,
        KernelDimension = kernelDimension,
        MaxKernelEigenvalue = maxKernelEigenvalue,
        ShellSize = shellSize,
        MaxSaddleDepth = maxSaddleDepth,
        MinQuarticNorm = minQuarticNorm,
        CauchySchwarzBetaBound = cauchySchwarzBetaBound,
        BetaWithinCauchySchwarzBound = betaWithinCauchySchwarzBound,
        ResidualNormAtBackground = residualNormAtBackground,
        ObjectiveAtBackground = objectiveAtBackground,
        MaxGramDeviation = maxGramDeviation,
        MaxKernelQuadraticForm = maxKernelQuadraticForm,
        MaxQuadraticExactnessResidual = maxQuadraticExactnessResidual,
        MaxGnParityRelativeDeviation = maxGnParityRelativeDeviation,
        MaxPolarizationResidual = maxPolarizationResidual,
        MaxAbsCurvatureFormEntry = maxAbsCurvatureFormEntry,
        BetaTolerance = betaTolerance,
        QuarticTolerance = quarticTolerance,
        GaugeOrbitRank = gaugeBasis.Count,
        QuadraticallyLiftedCount = quadraticallyLiftedCount,
        SaddleCount = saddleCount,
        QuarticallyLiftedCount = quarticallyLiftedCount,
        ExactlyFlatCount = exactlyFlatCount,
        GaugeAlignedExactlyFlatCount = gaugeAlignedExactlyFlatCount,
        Directions = directionRecords,
        SourceKernelFractions = sourceKernelFractions,
        ObstructionLiftedFractions = obstructionLiftedFractions,
        ObstructionExactlyFlatFractions = obstructionExactlyFlatFractions,
    };
}

(FieldTensor FirstDifference, FieldTensor SecondDifference) FirstAndSecondDifference(
    Func<double[], FieldTensor> residualAt, double[] baseCoefficients, double[] direction, FieldTensor upsilon0, double step)
{
    var plus = new double[CarrierDimension];
    var minus = new double[CarrierDimension];
    for (int c = 0; c < CarrierDimension; c++)
    {
        plus[c] = baseCoefficients[c] + step * direction[c];
        minus[c] = baseCoefficients[c] - step * direction[c];
    }
    var upsilonPlus = residualAt(plus);
    var upsilonMinus = residualAt(minus);
    int length = upsilon0.Coefficients.Length;
    var first = new double[length];
    var second = new double[length];
    for (int c = 0; c < length; c++)
    {
        first[c] = (upsilonPlus.Coefficients[c] - upsilonMinus.Coefficients[c]) / (2.0 * step);
        second[c] = (upsilonPlus.Coefficients[c] + upsilonMinus.Coefficients[c] - 2.0 * upsilon0.Coefficients[c]) / (step * step);
    }
    return (WrapLike(upsilon0, first), WrapLike(upsilon0, second));
}

static FieldTensor WrapLike(FieldTensor template, double[] coefficients) => new()
{
    Label = template.Label,
    Signature = template.Signature,
    Coefficients = coefficients,
    Shape = template.Shape,
};

static double Dot(double[] left, double[] right)
{
    double sum = 0.0;
    for (int i = 0; i < left.Length; i++)
        sum += left[i] * right[i];
    return sum;
}

double[] DirectSource(double[] psi)
{
    // J_(e,a) = (2/h_e) Re sum_{bc} eps_{abc} W_e[b,c],
    // W_e[g,g'] = sum_{s,s'} conj(psi_v[g,s]) Gamma[s,s'] psi_w[g',s']
    // (the Phase393/394/399 closed-form per-edge source, verified there
    // against the delta_D variation matrices at machine precision).
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
        var wRe = new double[DimG, DimG];
        for (int g = 0; g < DimG; g++)
            for (int gp = 0; gp < DimG; gp++)
            {
                double sumRe = 0.0;
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
                    }
                wRe[g, gp] = sumRe;
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

ShellResult DenseShell(double[,] dRe, double[,] dIm)
{
    var (eigenvalues, modes) = DenseAllModes(dRe, dIm);
    double maxAbs = eigenvalues.Max(System.Math.Abs);
    double kernelThreshold = 1e-10 * System.Math.Max(maxAbs, 1e-30);
    var nonzero = Enumerable.Range(0, totalDof)
        .Where(k => System.Math.Abs(eigenvalues[k]) > kernelThreshold)
        .OrderBy(k => System.Math.Abs(eigenvalues[k]))
        .ToArray();
    double lambdaMin = System.Math.Abs(eigenvalues[nonzero[0]]);
    double grouping = System.Math.Max(1e-12, 1e-8 * lambdaMin);
    var shellIndices = nonzero.Where(k => System.Math.Abs(System.Math.Abs(eigenvalues[k]) - lambdaMin) <= grouping).ToArray();
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
        invSqrtM[i] = 1.0 / System.Math.Sqrt(meshVolumeMassPsiWeights[2 * i]);
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

static int DominantDirection(IReadOnlyList<double> direction)
{
    var mu = 0;
    var best = 0.0;
    for (int i = 0; i < direction.Count; i++)
    {
        var abs = System.Math.Abs(direction[i]);
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
    matrixNorm = System.Math.Sqrt(matrixNorm);
    double threshold = JacobiOffDiagonalTolerance * System.Math.Max(matrixNorm, 1e-30);

    int sweeps = 0;
    double offDiagonal = OffDiagonalNorm(aRe, aIm, n);
    while (offDiagonal > threshold && sweeps < JacobiMaxSweeps)
    {
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double gRe = aRe[p, q];
                double gIm = aIm[p, q];
                double gAbs = System.Math.Sqrt(gRe * gRe + gIm * gIm);
                if (gAbs <= threshold / n)
                    continue;

                double phaseRe = gRe / gAbs;
                double phaseIm = gIm / gAbs;
                double alpha = aRe[p, p];
                double beta = aRe[q, q];
                double theta = 0.5 * System.Math.Atan2(2.0 * gAbs, alpha - beta);
                double c = System.Math.Cos(theta);
                double s = System.Math.Sin(theta);

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
    return System.Math.Sqrt(sum);
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
    return System.Math.Sqrt(norm);
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
    norm = System.Math.Sqrt(norm);
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

public sealed class KernelDirectionRecord
{
    public required int Rank { get; init; }
    public required double QuadraticCoefficient { get; init; }
    public required double QuarticResidualNorm { get; init; }
    public required double GaugeFraction { get; init; }
    public required string Classification { get; init; }
}

public sealed class BackgroundProbeRecord
{
    public required string FermionBackgroundId { get; init; }
    public required int KernelDimension { get; init; }
    public required double MaxKernelEigenvalue { get; init; }
    public required int ShellSize { get; init; }
    public required double MaxSaddleDepth { get; init; }
    public required double MinQuarticNorm { get; init; }
    public required double CauchySchwarzBetaBound { get; init; }
    public required bool BetaWithinCauchySchwarzBound { get; init; }
    public required double ResidualNormAtBackground { get; init; }
    public required double ObjectiveAtBackground { get; init; }
    public required double MaxGramDeviation { get; init; }
    public required double MaxKernelQuadraticForm { get; init; }
    public required double MaxQuadraticExactnessResidual { get; init; }
    public required double MaxGnParityRelativeDeviation { get; init; }
    public required double MaxPolarizationResidual { get; init; }
    public required double MaxAbsCurvatureFormEntry { get; init; }
    public required double BetaTolerance { get; init; }
    public required double QuarticTolerance { get; init; }
    public required int GaugeOrbitRank { get; init; }
    public required int QuadraticallyLiftedCount { get; init; }
    public required int SaddleCount { get; init; }
    public required int QuarticallyLiftedCount { get; init; }
    public required int ExactlyFlatCount { get; init; }
    public required int GaugeAlignedExactlyFlatCount { get; init; }
    public required IReadOnlyList<KernelDirectionRecord> Directions { get; init; }
    public required double[] SourceKernelFractions { get; init; }
    public required double[] ObstructionLiftedFractions { get; init; }
    public required double[] ObstructionExactlyFlatFractions { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        kernelDimension = KernelDimension,
        maxKernelEigenvalue = MaxKernelEigenvalue,
        shellSize = ShellSize,
        maxSaddleDepth = MaxSaddleDepth,
        minQuarticNorm = MinQuarticNorm,
        cauchySchwarzBetaBound = CauchySchwarzBetaBound,
        betaWithinCauchySchwarzBound = BetaWithinCauchySchwarzBound,
        residualNormAtBackground = ResidualNormAtBackground,
        objectiveAtBackground = ObjectiveAtBackground,
        maxGramDeviation = MaxGramDeviation,
        maxKernelQuadraticForm = MaxKernelQuadraticForm,
        maxQuadraticExactnessResidual = MaxQuadraticExactnessResidual,
        maxGnParityRelativeDeviation = MaxGnParityRelativeDeviation,
        maxPolarizationResidual = MaxPolarizationResidual,
        maxAbsCurvatureFormEntry = MaxAbsCurvatureFormEntry,
        betaTolerance = BetaTolerance,
        quarticTolerance = QuarticTolerance,
        gaugeOrbitRank = GaugeOrbitRank,
        quadraticallyLiftedCount = QuadraticallyLiftedCount,
        saddleCount = SaddleCount,
        quarticallyLiftedCount = QuarticallyLiftedCount,
        exactlyFlatCount = ExactlyFlatCount,
        gaugeAlignedExactlyFlatCount = GaugeAlignedExactlyFlatCount,
        directions = Directions.Select(direction => new
        {
            rank = direction.Rank,
            quadraticCoefficient = direction.QuadraticCoefficient,
            quarticResidualNorm = direction.QuarticResidualNorm,
            gaugeFraction = direction.GaugeFraction,
            classification = direction.Classification,
        }).ToArray(),
        sourceKernelFractions = SourceKernelFractions,
        obstructionLiftedFractions = ObstructionLiftedFractions,
        obstructionExactlyFlatFractions = ObstructionExactlyFlatFractions,
    };
}
