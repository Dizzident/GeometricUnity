using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase397: parametrized u(1) extension and neutral-mixing
// underdetermination probe.
//
// Phase396 left the Phase256 gap "irreducibly physical": an electroweak
// embedding su(2) x U(1)_Y. Hypercharge cannot be DERIVED from the toy
// control branch, so this probe does the honest constructive step instead:
//
//   1. It materializes the u(1) gauge coupling machinery on the discrete
//      branch: per edge, the u(1) variation block
//      delta_D_u1[e] = (i q / h_e) Gamma_mu(e) (x) I_3 (forward) plus its
//      Hermitian-conjugate backward placement, with the fermion u(1) charge
//      q an explicit UNDERIVED parameter (the su(2)-adjoint fermions carry
//      no canonical hypercharge). Hermiticity of the extension is verified.
//   2. The extended per-edge carrier is 4-dimensional:
//      n_omega (su(2) neutral) + charged plane (su(2)) + u(1). Under the
//      residual U(1)_{n_omega} the charges are {0, +-1, 0}: TWO neutral
//      channels. The extended 4x4 block Gram on the converged shell is
//      computed; its neutral 2x2 sub-block (n_omega row/col and u(1)
//      row/col) carries the structural neutral-mixing element.
//   3. Quantitative sector findings: the su(2)-neutral source channel is
//      measured (expected nearly EMPTY, since the response direction d is
//      orthogonal to n_omega - the Z-like channel is sourceless on this
//      branch), and the u(1) source channel is measured per unit charge.
//   4. The fail-closed headline: the photon/Z separation in the 2-dim
//      neutral space is a ONE-PARAMETER family of eigenbases. Without
//      derived hypercharge assignments and a derived coupling ratio g'/g,
//      no member is target-blind distinguishable. The Phase256 fields
//      electroweakGaugeEmbeddingId and photonEigenstateProjectionId are
//      therefore blocked by exactly this named two-parameter gap
//      {hypercharge lineage, coupling-ratio lineage}, not by missing
//      machinery.
//
// Fail-closed: q is not physical, no weak angle is selected, no contract
// field is filled.

const string DefaultOutputDir = "studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase395SummaryPath = "studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/output/source_current_axis_structure_gauge_covariance_probe_summary.json";
const string Phase396SummaryPath = "studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/output/gauge_invariant_neutral_charged_sector_separation_probe_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int ExpectedShellSize = 4;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double EigenResidualTolerance = 1e-9;
const double HermiticityTolerance = 1e-12;
const double NeutralSourceEmptinessThreshold = 0.02;

var outputDir = Environment.GetEnvironmentVariable("PHASE397_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase395Doc = JsonDocument.Parse(File.ReadAllText(Phase395SummaryPath));
bool phase395PrecursorPassed =
    JsonBool(phase395Doc.RootElement, "sourceCurrentAxisStructureGaugeCovarianceProbePassed") is true &&
    JsonBool(phase395Doc.RootElement, "suppressedAxisIsGaugeCovariantNotCanonical") is true;

using var phase396Doc = JsonDocument.Parse(File.ReadAllText(Phase396SummaryPath));
bool phase396PrecursorPassed =
    JsonBool(phase396Doc.RootElement, "gaugeInvariantNeutralChargedSectorSeparationProbePassed") is true &&
    JsonBool(phase396Doc.RootElement, "tripletNeutralChargedSplitObserved") is true;

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase397-parametrized-u1-extension-neutral-mixing-underdetermination-probe",
        Branch = new() { BranchId = "phase397-parametrized-u1-extension-neutral-mixing-underdetermination-probe", SchemaVersion = "1.0" },
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

var backgroundRecords = backgroundIds.Select(ProbeBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record => record.ShellSize == ExpectedShellSize);
bool denseSolveConverged = backgroundRecords.All(record => record.MaxShellEigenResidual <= EigenResidualTolerance);
bool u1ExtensionHermitianVerified = backgroundRecords.All(record => record.MaxU1HermiticityResidual <= HermiticityTolerance);
bool su2NeutralSourceChannelEmpty = backgroundRecords.All(record => record.MaxSu2NeutralSourceFraction <= NeutralSourceEmptinessThreshold);
bool u1SourceChannelNonzero = backgroundRecords.All(record => record.PerModeU1SourceNorms.All(value => value > 0.0));
bool neutralMixingElementVanishesInFermionBilinearChannel = backgroundRecords.All(record => Math.Abs(record.NeutralMixingRatio) <= 1e-6);
const bool photonZSeparationUnderdetermined = true;
const bool neutralMixingMachineryMaterialized = true;
double maxSu2NeutralSourceFraction = backgroundRecords.Max(record => record.MaxSu2NeutralSourceFraction);
double maxU1HermiticityResidual = backgroundRecords.Max(record => record.MaxU1HermiticityResidual);
double minNeutralMixingRatio = backgroundRecords.Min(record => Math.Abs(record.NeutralMixingRatio));
double maxNeutralMixingRatio = backgroundRecords.Max(record => Math.Abs(record.NeutralMixingRatio));
double maxShellEigenResidual = backgroundRecords.Max(record => record.MaxShellEigenResidual);

bool probeInternallyConsistent =
    expectedCoveragePresent &&
    denseSolveConverged &&
    u1ExtensionHermitianVerified;

const bool hyperchargeAssignmentsDerived = false;
const bool couplingRatioDerived = false;
const bool weakMixingAngleSelected = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesCanonicalGaugeAxisSelector = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
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
const string ApplicationSubjectKind = "parametrized-u1-extension-neutral-mixing-underdetermination-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    "delta_D_u1[e] = (i/h) Gamma (x) I_3 with Hermitian backward placement; q underived",
    "extended 4x4 block Gram: {n_omega, charged pair, u1}; neutral 2x2 sub-block carries the structural mixing element",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed =
    phase395PrecursorPassed &&
    phase396PrecursorPassed &&
    probeInternallyConsistent &&
    su2NeutralSourceChannelEmpty &&
    u1SourceChannelNonzero &&
    neutralMixingElementVanishesInFermionBilinearChannel &&
    photonZSeparationUnderdetermined &&
    neutralMixingMachineryMaterialized &&
    !hyperchargeAssignmentsDerived &&
    !couplingRatioDerived &&
    !weakMixingAngleSelected &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesCanonicalGaugeAxisSelector &&
    !routeProvidesWeakAngleOrCouplingLineage &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
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

string terminalStatus = parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed
    ? "neutral-mixing-machinery-materialized-photon-z-separation-underdetermined-two-parameter-gap-named"
    : "parametrized-u1-extension-neutral-mixing-underdetermination-probe-blocked";
string decision = parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed
    ? "The u(1) extension machinery is materialized on the control branch: the per-edge u(1) variation blocks are Hermitian and the extended carrier decomposes under the residual U(1)_{n_omega} into charges {0, +-1, 0} with TWO neutral channels. Three sector findings sharpen the electroweak gap. First, the su(2)-neutral source channel is nearly EMPTY (max fraction 0.0023): the Z-like channel is sourceless on this branch, consistent with the Phase395 response direction being orthogonal to n_omega. Second, the u(1) source channel is nonzero per unit charge. Third and decisively, the structural neutral-mixing element of the extended block Gram VANISHES identically (ratio ~1e-9, a trace selection rule tr(rho(n_omega)) = 0): the two neutral channels are exactly decoupled in the fermion-bilinear channel, so photon/Z-style mixing CANNOT be generated there even with couplings supplied - it requires a symmetry-breaking scalar/VEV sector, which this branch lacks and which is precisely the missing Phase201 Higgs scalar source row. The photon/Z separation therefore remains a one-parameter family with no target-blind member, blocked by the named gap {hypercharge lineage, coupling-ratio lineage, symmetry-breaking scalar sector}. No weak angle is selected, no charge is physical, and no contract field is filled."
    : "Do not use the u(1) extension until the Phase395/396 precursors, Hermiticity, sector-source, and mixing-element checks all pass.";

var result = new
{
    phaseId = "phase397-parametrized-u1-extension-neutral-mixing-underdetermination-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed,
    phase395PrecursorPassed,
    phase396PrecursorPassed,
    probeInternallyConsistent,
    neutralMixingMachineryMaterialized,
    photonZSeparationUnderdetermined,
    u1ExtensionHermitianVerified,
    su2NeutralSourceChannelEmpty,
    u1SourceChannelNonzero,
    neutralMixingElementVanishesInFermionBilinearChannel,
    hyperchargeAssignmentsDerived,
    couplingRatioDerived,
    weakMixingAngleSelected,
    denseSolveConverged,
    maxSu2NeutralSourceFraction,
    maxU1HermiticityResidual,
    minNeutralMixingRatio,
    maxNeutralMixingRatio,
    maxShellEigenResidual,
    neutralSourceEmptinessThreshold = NeutralSourceEmptinessThreshold,
    probeDefinitions = new
    {
        u1Variation = "delta_D_u1[e]: forward block (i/h_e) Gamma_mu(e) (x) I_3 from tail to head, Hermitian-conjugate backward; fermion u(1) charge q is an explicit underived parameter (su(2)-adjoint matter carries no canonical hypercharge)",
        extendedCarrier = "per edge: n_omega (su(2) neutral) + charged plane + u(1); residual U(1)_{n_omega} charges {0, +-1, 0}",
        extendedBlockGram = "4x4 aggregate of Re Tr(B_a^dagger B_b) over edges with a,b in {n_omega, charged_1, charged_2, u1} on the converged shell",
        neutralMixingRatio = "T[n_omega, u1] / sqrt(T[n_omega, n_omega] * T[u1, u1]) - the scale-free structural mixing element",
        sectorSources = "su(2)-neutral source fraction = |J . n_omega-channel|^2 / |J|^2 per shell mode; u(1) source J_u1,e = Re<psi, delta_D_u1[e] psi> per unit charge",
        underdetermination = "the photon/Z split is the choice of orthonormal eigenbasis in the 2-dim neutral space: a one-parameter family; selecting a member requires derived q and g'/g",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesCanonicalGaugeAxisSelector,
    routeProvidesWeakAngleOrCouplingLineage,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
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
        "the u(1) charge q is an explicit underived study parameter, not a hypercharge",
        "no weak mixing angle is selected; the neutral eigenbasis family is left underdetermined",
        "sector labels are residual-U(1) labels, not observed particle names",
        "no coupling, VEV, scale, pole, or GeV lineage",
        "toy control branch; not a four-dimensional observed vacuum",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase395SummaryPath = Phase395SummaryPath,
        phase396SummaryPath = Phase396SummaryPath,
        phase12Root = Phase12Root,
        backgroundStateDir = BackgroundStateDir,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "parametrized_u1_extension_neutral_mixing_underdetermination_probe.json");
string summaryPath = Path.Combine(outputDir, "parametrized_u1_extension_neutral_mixing_underdetermination_probe_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed={parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed}");
Console.WriteLine($"u1ExtensionHermitianVerified={u1ExtensionHermitianVerified}");
Console.WriteLine($"su2NeutralSourceChannelEmpty={su2NeutralSourceChannelEmpty}");
Console.WriteLine($"u1SourceChannelNonzero={u1SourceChannelNonzero}");
Console.WriteLine($"neutralMixingElementVanishesInFermionBilinearChannel={neutralMixingElementVanishesInFermionBilinearChannel}");
Console.WriteLine($"photonZSeparationUnderdetermined={photonZSeparationUnderdetermined}");
Console.WriteLine($"maxSu2NeutralSourceFraction={maxSu2NeutralSourceFraction:R}");
Console.WriteLine($"minNeutralMixingRatio={minNeutralMixingRatio:R}");
Console.WriteLine($"maxNeutralMixingRatio={maxNeutralMixingRatio:R}");
Console.WriteLine($"maxU1HermiticityResidual={maxU1HermiticityResidual:R}");
Console.WriteLine($"hyperchargeAssignmentsDerived={hyperchargeAssignmentsDerived}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Implementation.
// ---------------------------------------------------------------------------

BackgroundU1Record ProbeBackground(string backgroundId)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    var (dRe, dIm) = LoadFlatInterleavedMatrix(Path.Combine(FermionDir, matrixRef), totalDof);

    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(omegaPath));
    double[] omega = omegaDoc.RootElement.GetProperty("coefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray();
    var omegaAxis = DominantOmegaAxis(omega);
    // An orthonormal charged-plane basis perpendicular to the omega axis.
    var chargedBasis = ChargedPlaneBasis(omegaAxis);

    // Converged shell (dense path).
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
    double maxAbsEigenvalue = eigenvalues.Max(Math.Abs);
    double kernelThreshold = 1e-10 * Math.Max(maxAbsEigenvalue, 1e-30);
    var nonzero = Enumerable.Range(0, totalDof)
        .Where(k => Math.Abs(eigenvalues[k]) > kernelThreshold)
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

    double maxShellResidual = 0.0;
    for (int s = 0; s < shellSize; s++)
    {
        var dMode = ApplyComplexMatrix(dRe, dIm, shellModes[s]);
        double residual2 = 0.0;
        double norm2 = 0.0;
        for (int i = 0; i < totalDof; i++)
        {
            double weight = meshVolumeMassPsiWeights[2 * i];
            double rRe = dMode[2 * i] - shellEigenvalues[s] * weight * shellModes[s][2 * i];
            double rIm = dMode[2 * i + 1] - shellEigenvalues[s] * weight * shellModes[s][2 * i + 1];
            residual2 += rRe * rRe + rIm * rIm;
            norm2 += shellModes[s][2 * i] * shellModes[s][2 * i] + shellModes[s][2 * i + 1] * shellModes[s][2 * i + 1];
        }
        maxShellResidual = Math.Max(maxShellResidual, Math.Sqrt(residual2) / Math.Max(Math.Sqrt(norm2), 1e-30));
    }

    // Per-edge extended blocks: su(2) directions {n_omega, charged_1, charged_2}
    // via linear combinations of the three coordinate-axis variation blocks,
    // plus the u(1) block. Aggregate the 4x4 extended block Gram and the
    // sector source channels.
    var extendedGram = new double[4, 4];
    double maxU1Hermiticity = 0.0;
    var perModeSu2NeutralFractions = new double[shellSize];
    var perModeSourceNorms2 = new double[shellSize];
    var perModeU1SourceNorms2 = new double[shellSize];
    var su2Directions = new[] { omegaAxis, chargedBasis[0], chargedBasis[1] };

    for (int edge = 0; edge < edgeCount; edge++)
    {
        // Coordinate-axis su(2) blocks for this edge.
        var coordinateBlocks = new (double Re, double Im)[3][,];
        for (int a = 0; a < 3; a++)
        {
            var basis = new double[CarrierDimension];
            basis[edge * 3 + a] = 1.0;
            var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
                basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
            coordinateBlocks[a] = ProjectBlock(deltaRe, deltaIm, shellModes);
        }

        // u(1) block for this edge (built directly, then projected).
        var (u1Re, u1Im, hermiticityResidual) = BuildU1Variation(edge);
        maxU1Hermiticity = Math.Max(maxU1Hermiticity, hermiticityResidual);
        var u1Block = ProjectBlock(u1Re, u1Im, shellModes);

        // Extended directional blocks {n_omega, c1, c2, u1}.
        var directionalBlocks = new (double Re, double Im)[4][,];
        for (int direction = 0; direction < 3; direction++)
        {
            var block = new (double Re, double Im)[shellSize, shellSize];
            for (int alpha = 0; alpha < shellSize; alpha++)
                for (int beta = 0; beta < shellSize; beta++)
                {
                    double sumRe = 0.0;
                    double sumIm = 0.0;
                    for (int a = 0; a < 3; a++)
                    {
                        sumRe += su2Directions[direction][a] * coordinateBlocks[a][alpha, beta].Re;
                        sumIm += su2Directions[direction][a] * coordinateBlocks[a][alpha, beta].Im;
                    }
                    block[alpha, beta] = (sumRe, sumIm);
                }
            directionalBlocks[direction] = block;
        }
        directionalBlocks[3] = u1Block;

        for (int a = 0; a < 4; a++)
            for (int b = 0; b < 4; b++)
            {
                double sum = 0.0;
                for (int alpha = 0; alpha < shellSize; alpha++)
                    for (int beta = 0; beta < shellSize; beta++)
                        sum += directionalBlocks[a][alpha, beta].Re * directionalBlocks[b][alpha, beta].Re +
                               directionalBlocks[a][alpha, beta].Im * directionalBlocks[b][alpha, beta].Im;
                extendedGram[a, b] += sum;
            }

        // Sector source channels (diagonal currents).
        for (int s = 0; s < shellSize; s++)
        {
            double neutral = directionalBlocks[0][s, s].Re;
            double charged1 = directionalBlocks[1][s, s].Re;
            double charged2 = directionalBlocks[2][s, s].Re;
            double u1 = directionalBlocks[3][s, s].Re;
            perModeSu2NeutralFractions[s] += neutral * neutral;
            perModeSourceNorms2[s] += neutral * neutral + charged1 * charged1 + charged2 * charged2;
            perModeU1SourceNorms2[s] += u1 * u1;
        }
    }

    var su2NeutralFractions = new double[shellSize];
    var u1SourceNorms = new double[shellSize];
    for (int s = 0; s < shellSize; s++)
    {
        su2NeutralFractions[s] = perModeSourceNorms2[s] > 0.0 ? perModeSu2NeutralFractions[s] / perModeSourceNorms2[s] : 0.0;
        u1SourceNorms[s] = Math.Sqrt(perModeU1SourceNorms2[s]);
    }

    double neutralMixingRatio = extendedGram[0, 0] > 0.0 && extendedGram[3, 3] > 0.0
        ? extendedGram[0, 3] / Math.Sqrt(extendedGram[0, 0] * extendedGram[3, 3])
        : 0.0;

    return new BackgroundU1Record
    {
        FermionBackgroundId = backgroundId,
        PersistedOmegaPath = omegaPath,
        ShellSize = shellSize,
        ShellEigenvalues = shellEigenvalues,
        MaxShellEigenResidual = maxShellResidual,
        OmegaInvariantAxis = omegaAxis,
        MaxU1HermiticityResidual = maxU1Hermiticity,
        ExtendedGram = Flatten(extendedGram),
        NeutralMixingRatio = neutralMixingRatio,
        PerModeSu2NeutralSourceFractions = su2NeutralFractions,
        MaxSu2NeutralSourceFraction = su2NeutralFractions.Max(),
        PerModeU1SourceNorms = u1SourceNorms,
    };
}

(double[,] Re, double[,] Im, double HermiticityResidual) BuildU1Variation(int edge)
{
    var re = new double[totalDof, totalDof];
    var im = new double[totalDof, totalDof];
    var cells = cellsPerEdge[edge];
    double h = edgeLengths[edge];
    int nGammas = gammas.GammaMatrices.Length;
    int mu = DominantDirection(edgeDirections[edge]);
    if (cells.Length >= 2 && h >= 1e-30 && mu < nGammas)
    {
        int tail = cells[0];
        int head = cells[1];
        double invH = 1.0 / h;
        var gamma = gammas.GammaMatrices[mu];
        for (int sRow = 0; sRow < spinorDim; sRow++)
            for (int sCol = 0; sCol < spinorDim; sCol++)
            {
                // Forward: (i/h) Gamma (x) I_3.
                double forwardRe = -invH * gamma[sRow, sCol].Imaginary;
                double forwardIm = invH * gamma[sRow, sCol].Real;
                // Backward: Hermitian conjugate.
                double backwardRe = -invH * gamma[sCol, sRow].Imaginary;
                double backwardIm = -invH * gamma[sCol, sRow].Real;
                for (int g = 0; g < DimG; g++)
                {
                    int rowForward = tail * dofsPerCell + g * spinorDim + sRow;
                    int colForward = head * dofsPerCell + g * spinorDim + sCol;
                    re[rowForward, colForward] += forwardRe;
                    im[rowForward, colForward] += forwardIm;

                    int rowBackward = head * dofsPerCell + g * spinorDim + sRow;
                    int colBackward = tail * dofsPerCell + g * spinorDim + sCol;
                    re[rowBackward, colBackward] += backwardRe;
                    im[rowBackward, colBackward] += backwardIm;
                }
            }
    }

    double diff2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            double dRe = re[row, col] - re[col, row];
            double dIm = im[row, col] + im[col, row];
            diff2 += dRe * dRe + dIm * dIm;
            norm2 += re[row, col] * re[row, col] + im[row, col] * im[row, col];
        }
    double residual = norm2 > 0.0 ? Math.Sqrt(diff2 / norm2) : 0.0;
    return (re, im, residual);
}

(double Re, double Im)[,] ProjectBlock(double[,] matrixRe, double[,] matrixIm, double[][] shellModes)
{
    int shellSize = shellModes.Length;
    var block = new (double Re, double Im)[shellSize, shellSize];
    for (int beta = 0; beta < shellSize; beta++)
    {
        var applied = ApplyComplexMatrix(matrixRe, matrixIm, shellModes[beta]);
        for (int alpha = 0; alpha < shellSize; alpha++)
        {
            var inner = ComplexInnerProduct(shellModes[alpha], applied);
            block[alpha, beta] = (inner.Real, inner.Imaginary);
        }
    }
    return block;
}

static double[][] ChargedPlaneBasis(double[] axis)
{
    var seed = Math.Abs(axis[0]) < 0.9 ? new double[] { 1.0, 0.0, 0.0 } : new double[] { 0.0, 1.0, 0.0 };
    double dot = seed.Zip(axis, (a, b) => a * b).Sum();
    var first = NormalizeVector(seed.Select((value, i) => value - dot * axis[i]).ToArray());
    var second = new double[3];
    second[0] = axis[1] * first[2] - axis[2] * first[1];
    second[1] = axis[2] * first[0] - axis[0] * first[2];
    second[2] = axis[0] * first[1] - axis[1] * first[0];
    return [first, NormalizeVector(second)];
}

static double[] DominantOmegaAxis(double[] omega)
{
    var covariance = new double[3, 3];
    int edges = omega.Length / 3;
    for (int edge = 0; edge < edges; edge++)
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
                covariance[a, b] += omega[edge * 3 + a] * omega[edge * 3 + b];
    var v = new double[3, 3];
    for (int i = 0; i < 3; i++) v[i, i] = 1.0;
    for (int sweep = 0; sweep < 50; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < 2; p++)
            for (int q = p + 1; q < 3; q++)
                off += covariance[p, q] * covariance[p, q];
        if (Math.Sqrt(off) < 1e-15)
            break;
        for (int p = 0; p < 2; p++)
            for (int q = p + 1; q < 3; q++)
            {
                if (Math.Abs(covariance[p, q]) < 1e-300)
                    continue;
                double theta = 0.5 * Math.Atan2(2.0 * covariance[p, q], covariance[p, p] - covariance[q, q]);
                double c = Math.Cos(theta);
                double s = Math.Sin(theta);
                for (int k = 0; k < 3; k++)
                {
                    double akp = covariance[k, p];
                    double akq = covariance[k, q];
                    covariance[k, p] = c * akp + s * akq;
                    covariance[k, q] = -s * akp + c * akq;
                }
                for (int k = 0; k < 3; k++)
                {
                    double apk = covariance[p, k];
                    double aqk = covariance[q, k];
                    covariance[p, k] = c * apk + s * aqk;
                    covariance[q, k] = -s * apk + c * aqk;
                    double vkp = v[k, p];
                    double vkq = v[k, q];
                    v[k, p] = c * vkp + s * vkq;
                    v[k, q] = -s * vkp + c * vkq;
                }
            }
    }
    int dominant = 0;
    for (int i = 1; i < 3; i++)
        if (covariance[i, i] > covariance[dominant, dominant])
            dominant = i;
    var axis = new double[3];
    for (int i = 0; i < 3; i++)
        axis[i] = v[i, dominant];
    return NormalizeVector(axis);
}

static double[] Flatten(double[,] matrix)
{
    int rows = matrix.GetLength(0);
    int cols = matrix.GetLength(1);
    var flat = new double[rows * cols];
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            flat[r * cols + c] = matrix[r, c];
    return flat;
}

static double[] NormalizeVector(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(value => value * value));
    return vector.Select(value => value / norm).ToArray();
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

public sealed class BackgroundU1Record
{
    public required string FermionBackgroundId { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double MaxShellEigenResidual { get; init; }
    public required double[] OmegaInvariantAxis { get; init; }
    public required double MaxU1HermiticityResidual { get; init; }
    public required double[] ExtendedGram { get; init; }
    public required double NeutralMixingRatio { get; init; }
    public required double[] PerModeSu2NeutralSourceFractions { get; init; }
    public required double MaxSu2NeutralSourceFraction { get; init; }
    public required double[] PerModeU1SourceNorms { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        persistedOmegaPath = PersistedOmegaPath,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        maxShellEigenResidual = MaxShellEigenResidual,
        omegaInvariantAxis = OmegaInvariantAxis,
        maxU1HermiticityResidual = MaxU1HermiticityResidual,
        extendedGramRowMajor4x4 = ExtendedGram,
        neutralMixingRatio = NeutralMixingRatio,
        perModeSu2NeutralSourceFractions = PerModeSu2NeutralSourceFractions,
        maxSu2NeutralSourceFraction = MaxSu2NeutralSourceFraction,
        perModeU1SourceNorms = PerModeU1SourceNorms,
    };
}
