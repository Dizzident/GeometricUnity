using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase394: positive bosonic spectrum recomputation and first-order
// backreaction construction.
//
// Phase393 identified the concrete missing artifact for the coupled-critical-
// point program: the persisted Phase12 bosonic Gauss-Newton spectrum holds
// only the 12 lowest modes, all numerical kernel (~1e-15), so the asymmetric
// first-order backreaction delta_omega = -kappa H_B^+ J was not constructible.
//
// This phase removes that gap with full provenance:
//
//   1. It copies the Phase12 background_family artifacts (excluding the
//      fermion and registry trees, which compute-spectrum does not read) into
//      a study-local working directory, so the persisted Phase12 artifacts
//      are never mutated.
//   2. It re-runs the repo's own production pipeline,
//      `Gu.Cli compute-spectrum <workdir> <backgroundId> --num-modes 156`,
//      on the copy, computing the FULL 156-mode Gauss-Newton spectrum per
//      background through the same OperatorBundleBuilder / EigensolverPipeline
//      path that produced the persisted kernel modes.
//   3. It verifies: PSD (no negative eigenvalues), kernel dimension, spectral
//      gap, su(2) triplet clustering, and that every persisted kernel mode is
//      contained in the recomputed kernel subspace.
//   4. With the positive spectrum in hand it CONSTRUCTS the per-mode
//      first-order backreaction for asymmetric shell occupation,
//      delta_omega^(s) = -kappa sum_{mu_i > tol} m_i (m_i . J^(s)) / mu_i,
//      per unit coupling, together with the unabsorbable kernel component of
//      each source and the second-order bosonic relaxation energy
//      sum (m_i . J)^2 / mu_i.
//
// Fail-closed: kappa is not physical, no coupled critical point is solved,
// the relaxation is first-order only, and no Phase201/Phase256 contract
// field is filled.

const string DefaultOutputDir = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string PersistedBosonModeDir = $"{Phase12Root}/spectra/modes";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase393SummaryPath = "studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/output/coupled_stationarity_fermionic_source_residual_probe_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int ExpectedShellSize = 4;
const int FullModeCount = 156;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double SpectrumResidualTolerance = 1e-7;
const double PsdTolerance = 1e-10;
const double KernelContainmentTolerance = 1e-8;
const double TripletClusterRelativeTolerance = 1e-2;

var outputDir = Environment.GetEnvironmentVariable("PHASE394_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
string workDir = Path.Combine(outputDir, "family_workdir");

// ---------------------------------------------------------------------------
// Precursor.
// ---------------------------------------------------------------------------

using var phase393Doc = JsonDocument.Parse(File.ReadAllText(Phase393SummaryPath));
bool phase393PrecursorPassed =
    JsonBool(phase393Doc.RootElement, "coupledStationarityFermionicSourceResidualProbePassed") is true &&
    JsonBool(phase393Doc.RootElement, "persistedBosonicSpectrumIsNumericalKernelOnly") is true &&
    JsonBool(phase393Doc.RootElement, "firstOrderBackreactionConstructibleFromPersistedArtifacts") is false;

// ---------------------------------------------------------------------------
// Step 1: stage the family working copy (never mutate Phase12 artifacts).
// ---------------------------------------------------------------------------

if (Directory.Exists(workDir))
    Directory.Delete(workDir, recursive: true);
Directory.CreateDirectory(workDir);
string[] copyEntries =
[
    "atlas.json", "background_records", "background_states", "bosons",
    "campaigns", "manifest", "modes", "observables", "reports", "spectra",
];
foreach (string entry in copyEntries)
{
    string source = Path.Combine(Phase12Root, entry);
    string target = Path.Combine(workDir, entry);
    if (File.Exists(source))
        File.Copy(source, target);
    else if (Directory.Exists(source))
        CopyDirectory(source, target);
}

// ---------------------------------------------------------------------------
// Step 2: run the production compute-spectrum pipeline at full mode count.
// ---------------------------------------------------------------------------

var backgroundIds = Directory.GetFiles(Path.Combine(workDir, "background_records"), "bg-*.json")
    .Select(Path.GetFileNameWithoutExtension)
    .OrderBy(id => id, StringComparer.Ordinal)
    .Select(id => id!)
    .ToArray();

bool cliRunsSucceeded = true;
foreach (string backgroundId in backgroundIds)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        WorkingDirectory = Environment.CurrentDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    startInfo.ArgumentList.Add("run");
    startInfo.ArgumentList.Add("--project");
    startInfo.ArgumentList.Add("apps/Gu.Cli");
    startInfo.ArgumentList.Add("--");
    startInfo.ArgumentList.Add("compute-spectrum");
    startInfo.ArgumentList.Add(workDir);
    startInfo.ArgumentList.Add(backgroundId);
    startInfo.ArgumentList.Add("--num-modes");
    startInfo.ArgumentList.Add(FullModeCount.ToString());
    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start compute-spectrum.");
    string stdout = process.StandardOutput.ReadToEnd();
    string stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        cliRunsSucceeded = false;
        Console.Error.WriteLine($"compute-spectrum failed for {backgroundId}: {stderr}");
    }
    File.WriteAllText(Path.Combine(outputDir, $"compute_spectrum_{backgroundId}.log"), stdout + stderr);
}
if (!cliRunsSucceeded)
    throw new InvalidOperationException("Full-spectrum recomputation failed; aborting fail-closed.");

// ---------------------------------------------------------------------------
// Shared geometry for the fermionic source recomputation (Phase393 path).
// ---------------------------------------------------------------------------

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase394-positive-bosonic-spectrum-backreaction-construction",
        Branch = new() { BranchId = "phase394-positive-bosonic-spectrum-backreaction-construction", SchemaVersion = "1.0" },
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

// ---------------------------------------------------------------------------
// Step 3 + 4: per-background analysis and backreaction construction.
// ---------------------------------------------------------------------------

var backgroundRecords = backgroundIds.Select(AnalyzeBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record => record.FullModeCount == FullModeCount && record.ShellSize == ExpectedShellSize);
bool fullSpectrumConverged = backgroundRecords.All(record => record.MaxSpectrumResidual <= SpectrumResidualTolerance);
bool bosonicGaussNewtonPsdVerified = backgroundRecords.All(record => record.NegativeCount == 0);
bool persistedKernelModesContainedInRecomputedKernel = backgroundRecords.All(record => record.MinPersistedKernelContainment >= 1.0 - KernelContainmentTolerance);
bool positiveBosonicSpectrumRecomputed = expectedCoveragePresent && fullSpectrumConverged && backgroundRecords.All(record => record.PositiveCount > 0);
bool firstOrderAsymmetricBackreactionConstructed = backgroundRecords.All(record => record.PerModeBackreactionNorms.All(value => value > 0.0));
bool tripletClusteringObserved = backgroundRecords.All(record => record.TripletClusterFraction >= 0.9);
int minKernelDimension = backgroundRecords.Min(record => record.KernelDimension);
int maxKernelDimension = backgroundRecords.Max(record => record.KernelDimension);
double minSpectralGap = backgroundRecords.Min(record => record.SpectralGap);
double maxSourceKernelFraction = backgroundRecords.Max(record => record.PerModeSourceKernelFractions.Max());
double maxBackreactionNorm = backgroundRecords.Max(record => record.PerModeBackreactionNorms.Max());
double maxRelaxationEnergy = backgroundRecords.Max(record => record.PerModeRelaxationEnergies.Max());
double maxSpectrumResidual = backgroundRecords.Max(record => record.MaxSpectrumResidual);

bool constructionInternallyConsistent =
    expectedCoveragePresent &&
    fullSpectrumConverged &&
    bosonicGaussNewtonPsdVerified &&
    persistedKernelModesContainedInRecomputedKernel &&
    positiveBosonicSpectrumRecomputed &&
    firstOrderAsymmetricBackreactionConstructed;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool coupledCriticalPointConstructed = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesCanonicalGaugeAxisSelector = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
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
const string ApplicationSubjectKind = "positive-bosonic-spectrum-backreaction-construction";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    FullModeCount.ToString(),
    "Gu.Cli compute-spectrum on a study-local family copy",
    "delta_omega^(s) = -sum_{mu_i>tol} m_i (m_i . J^(s)) / mu_i per unit coupling",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool positiveBosonicSpectrumBackreactionConstructionPassed =
    phase393PrecursorPassed &&
    cliRunsSucceeded &&
    constructionInternallyConsistent &&
    !coupledCriticalPointConstructed &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesCoupledResidual &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesCanonicalGaugeAxisSelector &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
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

string terminalStatus = positiveBosonicSpectrumBackreactionConstructionPassed
    ? "positive-bosonic-spectrum-recomputed-first-order-backreaction-constructed-discrete-diagnostic-only"
    : "positive-bosonic-spectrum-backreaction-construction-blocked";
string decision = positiveBosonicSpectrumBackreactionConstructionPassed
    ? "The full 156-mode bosonic Gauss-Newton spectrum was recomputed per background through the repo's production compute-spectrum pipeline on a study-local copy of the Phase12 family (persisted artifacts untouched). The operator is PSD with a finite kernel and a clear spectral gap, the eigenvalues cluster in su(2) triplets, and every persisted kernel mode is contained in the recomputed kernel subspace. With the positive spectrum in hand, the per-mode first-order backreaction for asymmetric shell occupation is now CONSTRUCTED per unit coupling, together with each source's unabsorbable kernel component and the second-order bosonic relaxation energy. This closes the Phase393 artifact gap. It remains a toy control-branch diagnostic: the coupling is not physical, no coupled critical point is solved, and no Phase201/Phase256 contract field is filled."
    : "Do not use the recomputed spectrum or backreaction until the Phase393 precursor, CLI recomputation, PSD and kernel-containment checks, and backreaction coverage all pass.";

var result = new
{
    phaseId = "phase394-positive-bosonic-spectrum-backreaction-construction",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    positiveBosonicSpectrumBackreactionConstructionPassed,
    phase393PrecursorPassed,
    cliRunsSucceeded,
    constructionInternallyConsistent,
    positiveBosonicSpectrumRecomputed,
    bosonicGaussNewtonPsdVerified,
    persistedKernelModesContainedInRecomputedKernel,
    firstOrderAsymmetricBackreactionConstructed,
    tripletClusteringObserved,
    coupledCriticalPointConstructed,
    physicalCouplingProvided,
    minKernelDimension,
    maxKernelDimension,
    minSpectralGap,
    maxSourceKernelFraction,
    maxBackreactionNorm,
    maxRelaxationEnergy,
    maxSpectrumResidual,
    fullModeCount = FullModeCount,
    constructionDefinitions = new
    {
        recomputation = "Gu.Cli compute-spectrum <study-local family copy> <backgroundId> --num-modes 156 (production OperatorBundleBuilder / EigensolverPipeline path; Phase12 artifacts never mutated)",
        backreaction = "delta_omega^(s) = -sum_{mu_i > tol} m_i (m_i . J^(s)) / mu_i per unit coupling, with J^(s)_k = Re<psi_s, delta_D[e_k] psi_s> on the converged shell and m_i the M-orthonormal recomputed bosonic modes",
        kernelComponent = "the projection of J^(s) onto the recomputed kernel span is the unabsorbable first-order obstruction for asymmetric occupation",
        relaxationEnergy = "E^(s) = sum_{mu_i > tol} (m_i . J^(s))^2 / mu_i per unit coupling squared",
        tripletClustering = "fraction of positive eigenvalues belonging to clusters of size 3 at relative tolerance 1e-2 (su(2) adjoint degeneracy)",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesCoupledResidual,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesCanonicalGaugeAxisSelector,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
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
        "first-order backreaction per unit coupling only; no physical coupling constant",
        "no coupled critical point solved",
        "candidate bilinear action only, not a completed GU fermionic action",
        "toy control branch only",
        "not a physical effective-action Hessian",
        "not an observed electroweak namespace map",
        "no W/Z bridge law",
        "no Higgs scalar operator",
        "no weak-angle or coupling lineage",
        "no VEV or source-scale lineage",
        "no pole extraction or GeV normalization",
        "no physical predictions",
        "no Phase201 or Phase256 fill",
    },
    backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase393SummaryPath = Phase393SummaryPath,
        phase12Root = Phase12Root,
        familyWorkdir = workDir,
        persistedBosonModeDir = PersistedBosonModeDir,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "positive_bosonic_spectrum_backreaction_construction.json");
string summaryPath = Path.Combine(outputDir, "positive_bosonic_spectrum_backreaction_construction_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        positiveBosonicSpectrumBackreactionConstructionPassed,
        phase393PrecursorPassed,
        cliRunsSucceeded,
        constructionInternallyConsistent,
        positiveBosonicSpectrumRecomputed,
        bosonicGaussNewtonPsdVerified,
        persistedKernelModesContainedInRecomputedKernel,
        firstOrderAsymmetricBackreactionConstructed,
        tripletClusteringObserved,
        coupledCriticalPointConstructed,
        physicalCouplingProvided,
        minKernelDimension,
        maxKernelDimension,
        minSpectralGap,
        maxSourceKernelFraction,
        maxBackreactionNorm,
        maxRelaxationEnergy,
        maxSpectrumResidual,
        result.constructionDefinitions,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        backgroundCount,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        routeProvidesCompletedFermionicAction,
        routeProvidesCoupledResidual,
        routeProvidesPhysicalEffectiveActionHessian,
        routeProvidesObservedElectroweakNamespaceMap,
        routeProvidesCanonicalGaugeAxisSelector,
        routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
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
        result.predictionContractImpact,
        result.explicitCandidateOnlyNonclaims,
        backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"positiveBosonicSpectrumBackreactionConstructionPassed={positiveBosonicSpectrumBackreactionConstructionPassed}");
Console.WriteLine($"positiveBosonicSpectrumRecomputed={positiveBosonicSpectrumRecomputed}");
Console.WriteLine($"bosonicGaussNewtonPsdVerified={bosonicGaussNewtonPsdVerified}");
Console.WriteLine($"persistedKernelModesContainedInRecomputedKernel={persistedKernelModesContainedInRecomputedKernel}");
Console.WriteLine($"firstOrderAsymmetricBackreactionConstructed={firstOrderAsymmetricBackreactionConstructed}");
Console.WriteLine($"tripletClusteringObserved={tripletClusteringObserved}");
Console.WriteLine($"minKernelDimension={minKernelDimension}");
Console.WriteLine($"maxKernelDimension={maxKernelDimension}");
Console.WriteLine($"minSpectralGap={minSpectralGap:R}");
Console.WriteLine($"maxSourceKernelFraction={maxSourceKernelFraction:R}");
Console.WriteLine($"maxBackreactionNorm={maxBackreactionNorm:R}");
Console.WriteLine($"maxRelaxationEnergy={maxRelaxationEnergy:R}");
Console.WriteLine($"coupledCriticalPointConstructed={coupledCriticalPointConstructed}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background analysis implementation.
// ---------------------------------------------------------------------------

BackgroundSpectrumRecord AnalyzeBackground(string backgroundId)
{
    // Recomputed full spectrum from the working copy.
    var modeFiles = Directory.GetFiles(Path.Combine(workDir, "spectra", "modes"), $"{backgroundId}-mode-*.json");
    var bosonEigenvalues = new List<double>();
    var bosonVectors = new List<double[]>();
    double maxResidual = 0.0;
    foreach (string path in modeFiles
        .OrderBy(path => int.Parse(Path.GetFileNameWithoutExtension(path).Split("-mode-")[1])))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        bosonEigenvalues.Add(doc.RootElement.GetProperty("eigenvalue").GetDouble());
        maxResidual = Math.Max(maxResidual, doc.RootElement.GetProperty("residualNorm").GetDouble());
        bosonVectors.Add(doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
    }
    int fullModeCount = bosonEigenvalues.Count;
    double maxAbsBosonEigenvalue = bosonEigenvalues.Max(Math.Abs);
    double kernelTolerance = 1e-10 * Math.Max(maxAbsBosonEigenvalue, 1e-30);
    var kernelIndices = Enumerable.Range(0, fullModeCount).Where(i => Math.Abs(bosonEigenvalues[i]) <= kernelTolerance).ToArray();
    var positiveIndices = Enumerable.Range(0, fullModeCount).Where(i => bosonEigenvalues[i] > kernelTolerance).ToArray();
    int negativeCount = Enumerable.Range(0, fullModeCount).Count(i => bosonEigenvalues[i] < -PsdTolerance);
    var positiveEigenvalues = positiveIndices.Select(i => bosonEigenvalues[i]).OrderBy(value => value).ToArray();
    double spectralGap = positiveEigenvalues.Length > 0 ? positiveEigenvalues[0] : double.NaN;

    // su(2) triplet clustering of the positive spectrum.
    int clustered = 0;
    int index = 0;
    while (index < positiveEigenvalues.Length)
    {
        int clusterSize = 1;
        while (index + clusterSize < positiveEigenvalues.Length &&
               Math.Abs(positiveEigenvalues[index + clusterSize] - positiveEigenvalues[index]) <=
               TripletClusterRelativeTolerance * Math.Abs(positiveEigenvalues[index]))
            clusterSize++;
        if (clusterSize % 3 == 0)
            clustered += clusterSize;
        index += clusterSize;
    }
    double tripletClusterFraction = positiveEigenvalues.Length > 0 ? (double)clustered / positiveEigenvalues.Length : 0.0;

    // Containment of the persisted 12 kernel modes in the recomputed kernel span.
    var kernelBasis = Orthonormalize(kernelIndices.Select(i => bosonVectors[i]).ToList());
    double minContainment = double.PositiveInfinity;
    for (int persistedIndex = 0; ; persistedIndex++)
    {
        string path = Path.Combine(PersistedBosonModeDir, $"{backgroundId}-mode-{persistedIndex}.json");
        if (!File.Exists(path))
            break;
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var vector = doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
        minContainment = Math.Min(minContainment, SubspaceFraction(vector, kernelBasis));
    }

    // Converged fermionic shell and per-mode source currents (Phase393 path).
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    var (dRe, dIm) = LoadFlatInterleavedMatrix(Path.Combine(FermionDir, matrixRef), totalDof);

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
    double maxAbsFermionEigenvalue = eigenvalues.Max(Math.Abs);
    double fermionKernelThreshold = 1e-10 * Math.Max(maxAbsFermionEigenvalue, 1e-30);
    var nonzero = Enumerable.Range(0, totalDof)
        .Where(k => Math.Abs(eigenvalues[k]) > fermionKernelThreshold)
        .OrderBy(k => Math.Abs(eigenvalues[k]))
        .ToArray();
    double lambdaMinMagnitude = Math.Abs(eigenvalues[nonzero[0]]);
    double groupingTolerance = Math.Max(1e-12, 1e-8 * lambdaMinMagnitude);
    var shellIndices = nonzero
        .Where(k => Math.Abs(Math.Abs(eigenvalues[k]) - lambdaMinMagnitude) <= groupingTolerance)
        .ToArray();
    int shellSize = shellIndices.Length;
    var shellModes = shellIndices.Select(k =>
    {
        var mode = new double[2 * totalDof];
        for (int i = 0; i < totalDof; i++)
        {
            mode[2 * i] = invSqrtM[i] * vecRe[i, k];
            mode[2 * i + 1] = invSqrtM[i] * vecIm[i, k];
        }
        return mode;
    }).ToArray();
    var shellEigenvalues = shellIndices.Select(k => eigenvalues[k]).ToArray();

    var perModeSources = new double[shellSize][];
    for (int s = 0; s < shellSize; s++)
        perModeSources[s] = new double[CarrierDimension];
    for (int coordinate = 0; coordinate < CarrierDimension; coordinate++)
    {
        var basis = new double[CarrierDimension];
        basis[coordinate] = 1.0;
        var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
            basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
        for (int s = 0; s < shellSize; s++)
        {
            var deltaPsi = ApplyComplexMatrix(deltaRe, deltaIm, shellModes[s]);
            perModeSources[s][coordinate] = ComplexInnerProduct(shellModes[s], deltaPsi).Real;
        }
    }

    // First-order backreaction per shell mode using the positive spectrum.
    var perModeBackreactionNorms = new double[shellSize];
    var perModeRelaxationEnergies = new double[shellSize];
    var perModeSourceKernelFractions = new double[shellSize];
    var perModeBackreactionAxisFractions = new double[shellSize][];
    for (int s = 0; s < shellSize; s++)
    {
        var source = perModeSources[s];
        var backreaction = new double[CarrierDimension];
        double relaxationEnergy = 0.0;
        foreach (int i in positiveIndices)
        {
            double overlap = 0.0;
            for (int k = 0; k < CarrierDimension; k++)
                overlap += bosonVectors[i][k] * source[k];
            double coefficient = -overlap / bosonEigenvalues[i];
            relaxationEnergy += overlap * overlap / bosonEigenvalues[i];
            for (int k = 0; k < CarrierDimension; k++)
                backreaction[k] += coefficient * bosonVectors[i][k];
        }
        perModeBackreactionNorms[s] = VectorNorm(backreaction);
        perModeRelaxationEnergies[s] = relaxationEnergy;
        perModeSourceKernelFractions[s] = SubspaceFraction(source, kernelBasis);
        var axisCapture = new double[DimG];
        double norm2 = perModeBackreactionNorms[s] * perModeBackreactionNorms[s];
        for (int k = 0; k < CarrierDimension; k++)
            axisCapture[k % DimG] += backreaction[k] * backreaction[k];
        perModeBackreactionAxisFractions[s] = axisCapture.Select(value => norm2 > 0.0 ? value / norm2 : 0.0).ToArray();
    }

    return new BackgroundSpectrumRecord
    {
        FermionBackgroundId = backgroundId,
        FullModeCount = fullModeCount,
        MaxSpectrumResidual = maxResidual,
        KernelDimension = kernelIndices.Length,
        PositiveCount = positiveIndices.Length,
        NegativeCount = negativeCount,
        SpectralGap = spectralGap,
        MaxBosonicEigenvalue = bosonEigenvalues.Max(),
        TripletClusterFraction = tripletClusterFraction,
        LowestPositiveEigenvalues = positiveEigenvalues.Take(9).ToArray(),
        MinPersistedKernelContainment = minContainment,
        ShellSize = shellSize,
        ShellEigenvalues = shellEigenvalues,
        PerModeSourceKernelFractions = perModeSourceKernelFractions,
        PerModeBackreactionNorms = perModeBackreactionNorms,
        PerModeRelaxationEnergies = perModeRelaxationEnergies,
        PerModeBackreactionAxisFractions = perModeBackreactionAxisFractions,
    };
}

static List<double[]> Orthonormalize(IReadOnlyList<double[]> vectors)
{
    var basis = new List<double[]>();
    foreach (var vector in vectors)
    {
        var candidate = (double[])vector.Clone();
        foreach (var basisVector in basis)
        {
            double dot = 0.0;
            for (int i = 0; i < candidate.Length; i++)
                dot += candidate[i] * basisVector[i];
            for (int i = 0; i < candidate.Length; i++)
                candidate[i] -= dot * basisVector[i];
        }
        double norm = VectorNorm(candidate);
        if (norm <= 1e-10)
            continue;
        for (int i = 0; i < candidate.Length; i++)
            candidate[i] /= norm;
        basis.Add(candidate);
    }
    return basis;
}

static void CopyDirectory(string source, string target)
{
    Directory.CreateDirectory(target);
    foreach (string file in Directory.GetFiles(source))
        File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
    foreach (string directory in Directory.GetDirectories(source))
        CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
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

static double VectorNorm(double[] vector)
{
    double sum = 0.0;
    foreach (double value in vector)
        sum += value * value;
    return Math.Sqrt(sum);
}

static double SubspaceFraction(double[] vector, IReadOnlyList<double[]> orthonormalBasis)
{
    double norm2 = 0.0;
    foreach (double value in vector)
        norm2 += value * value;
    if (norm2 <= 0.0)
        return 0.0;
    double captured = 0.0;
    foreach (var basisVector in orthonormalBasis)
    {
        double dot = 0.0;
        for (int i = 0; i < vector.Length; i++)
            dot += vector[i] * basisVector[i];
        captured += dot * dot;
    }
    return captured / norm2;
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

public sealed class BackgroundSpectrumRecord
{
    public required string FermionBackgroundId { get; init; }
    public required int FullModeCount { get; init; }
    public required double MaxSpectrumResidual { get; init; }
    public required int KernelDimension { get; init; }
    public required int PositiveCount { get; init; }
    public required int NegativeCount { get; init; }
    public required double SpectralGap { get; init; }
    public required double MaxBosonicEigenvalue { get; init; }
    public required double TripletClusterFraction { get; init; }
    public required double[] LowestPositiveEigenvalues { get; init; }
    public required double MinPersistedKernelContainment { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double[] PerModeSourceKernelFractions { get; init; }
    public required double[] PerModeBackreactionNorms { get; init; }
    public required double[] PerModeRelaxationEnergies { get; init; }
    public required double[][] PerModeBackreactionAxisFractions { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        fullModeCount = FullModeCount,
        maxSpectrumResidual = MaxSpectrumResidual,
        kernelDimension = KernelDimension,
        positiveCount = PositiveCount,
        negativeCount = NegativeCount,
        spectralGap = SpectralGap,
        maxBosonicEigenvalue = MaxBosonicEigenvalue,
        tripletClusterFraction = TripletClusterFraction,
        lowestPositiveEigenvalues = LowestPositiveEigenvalues,
        minPersistedKernelContainment = MinPersistedKernelContainment,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        perModeSourceKernelFractions = PerModeSourceKernelFractions,
        perModeBackreactionNorms = PerModeBackreactionNorms,
        perModeRelaxationEnergies = PerModeRelaxationEnergies,
        perModeBackreactionAxisFractions = PerModeBackreactionAxisFractions,
    };
}
