using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase396: gauge-invariant neutral/charged sector separation probe.
//
// Phase395 proved that raw carrier coordinate axes are gauge-frame artifacts
// and that any defensible observed-field extraction must be built from
// gauge-invariant data relative to the background invariant axis
// n_omega ~ (1,1,1)/sqrt(3) (the symmetric-ansatz direction). This probe
// constructs the simplest such extraction - the dressing-style
// neutral/charged sector separation - and tests it against the recomputed
// positive bosonic Gauss-Newton spectrum (Phase394):
//
//   1. Extraction map. For a carrier vector b (156 = 52 edges x su(2)),
//      the invariant-axis ("neutral-sector") fraction is
//      f(b) = sum_e (b_e . n_omega)^2 / sum_e |b_e|^2, and the charged-plane
//      fraction is 1 - f(b). Under a global gauge rotation R both b and
//      omega rotate, n_omega(R omega) = R n_omega, and f is exactly
//      invariant - verified numerically here.
//   2. Cluster-invariant sector content. The positive bosonic spectrum
//      clusters in EXACT su(2) triplets (Phase394). Within a degenerate
//      triplet the individual eigenvectors are an arbitrary basis, so the
//      invariant object is the TRACE of the invariant-axis projector over
//      the cluster span: neutralContent(cluster) = sum_{modes in cluster}
//      f(mode). The residual-U(1) hypothesis (the discrete skeleton of
//      {Z-like, W+-like, W--like} classification) predicts neutralContent
//      ~ 1.0 per triplet: one neutral direction plus a charged pair.
//   3. Honest Phase256 audit. The contract requires 20 fields including
//      electroweakGaugeEmbeddingId, photonEigenstateProjectionId, W/Z source
//      rows, and Higgs operators. The su(2)-only control branch has no
//      hypercharge U(1)_Y, hence no photon/Z mixing, no weak angle, and no
//      physical scale: the extraction supplies a gauge-invariant
//      neutral/charged separation (the structural prerequisite) but can fill
//      ZERO contract fields. Recorded fail-closed with per-field reasons.
//
// Precursor artifact: this probe reads the recomputed full bosonic spectrum
// from the Phase394 working directory; run Phase394 first.

const string DefaultOutputDir = "studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string Phase394WorkdirModes = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/family_workdir/spectra/modes";
const string Phase394SummaryPath = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/positive_bosonic_spectrum_backreaction_construction_summary.json";
const string Phase395SummaryPath = "studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/output/source_current_axis_structure_gauge_covariance_probe_summary.json";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int EdgeCount = CarrierDimension / DimG;
const double TripletClusterRelativeTolerance = 1e-2;
const double NeutralContentTolerance = 0.05;
const double InvarianceTolerance = 1e-12;
const int ExpectedPhase256RequiredFieldCount = 20;

var outputDir = Environment.GetEnvironmentVariable("PHASE396_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase394Doc = JsonDocument.Parse(File.ReadAllText(Phase394SummaryPath));
bool phase394PrecursorPassed =
    JsonBool(phase394Doc.RootElement, "positiveBosonicSpectrumBackreactionConstructionPassed") is true &&
    JsonBool(phase394Doc.RootElement, "tripletClusteringObserved") is true;

using var phase395Doc = JsonDocument.Parse(File.ReadAllText(Phase395SummaryPath));
bool phase395PrecursorPassed =
    JsonBool(phase395Doc.RootElement, "sourceCurrentAxisStructureGaugeCovarianceProbePassed") is true &&
    JsonBool(phase395Doc.RootElement, "suppressedAxisIsGaugeCovariantNotCanonical") is true &&
    JsonBool(phase395Doc.RootElement, "backgroundOmegaNearSymmetricAnsatz") is true;

using var phase256Doc = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
bool phase256ContractPresent =
    JsonBool(phase256Doc.RootElement, "observedFieldExtractionIntakeContractPassed") is true &&
    JsonInt(phase256Doc.RootElement, "requiredFieldCount") == ExpectedPhase256RequiredFieldCount &&
    JsonInt(phase256Doc.RootElement, "filledRequiredFieldCount") == 0;

bool phase394WorkdirPresent = Directory.Exists(Phase394WorkdirModes);
if (!phase394WorkdirPresent)
    throw new InvalidOperationException(
        $"Phase394 working directory not found at {Phase394WorkdirModes}. Run Phase394 first.");

// ---------------------------------------------------------------------------
// Per-background probe.
// ---------------------------------------------------------------------------

var backgroundIds = Directory.GetFiles(BackgroundStateDir, "*_omega.json")
    .Select(path => Path.GetFileNameWithoutExtension(path).Replace("_omega", ""))
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToArray();

var backgroundRecords = backgroundIds.Select(ProbeBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record => record.FullModeCount == CarrierDimension);
bool extractionGaugeInvarianceVerified = backgroundRecords.All(record => record.MaxInvarianceResidual <= InvarianceTolerance);
bool tripletNeutralChargedSplitObserved = backgroundRecords.All(record =>
    record.TripletClusterCount > 0 &&
    record.TripletClustersWithUnitNeutralContent == record.TripletClusterCount);
int totalTripletClusterCount = backgroundRecords.Sum(record => record.TripletClusterCount);
double maxNeutralContentDeviation = backgroundRecords.Max(record => record.MaxNeutralContentDeviation);
double maxInvarianceResidual = backgroundRecords.Max(record => record.MaxInvarianceResidual);
double minKernelNeutralContent = backgroundRecords.Min(record => record.KernelNeutralContent);
double maxKernelNeutralContent = backgroundRecords.Max(record => record.KernelNeutralContent);

bool probeInternallyConsistent =
    expectedCoveragePresent &&
    extractionGaugeInvarianceVerified;

// ---------------------------------------------------------------------------
// Honest Phase256 audit: the extraction supplies the structural skeleton but
// zero contract fields on the su(2)-only control branch.
// ---------------------------------------------------------------------------

var phase256FieldAudit = new[]
{
    new { fieldId = "observedFieldExtractionTheoremId", suppliable = false, reason = "the neutral/charged separation is a study-defined construction with numerical verification, not a proven theorem with source lineage" },
    new { fieldId = "electroweakGaugeEmbeddingId", suppliable = false, reason = "the control branch is su(2)-only; there is no hypercharge U(1)_Y and hence no electroweak embedding" },
    new { fieldId = "photonEigenstateProjectionId", suppliable = false, reason = "without U(1)_Y there is no photon/Z mixing; the invariant-axis component is a neutral su(2) direction, not a photon eigenstate" },
    new { fieldId = "wBosonSourceRowId", suppliable = false, reason = "the charged-plane pair is a gauge-invariant charged sector, but no observed-W identification, coupling, or scale lineage exists" },
    new { fieldId = "zBosonSourceRowId", suppliable = false, reason = "the invariant-axis mode is a neutral su(2) direction; identifying it with the observed Z requires the missing weak-angle and scale lineage" },
    new { fieldId = "higgsScalarSourceOperatorId", suppliable = false, reason = "no scalar sector exists on this branch" },
    new { fieldId = "quadraticElectroweakMassOperatorId", suppliable = false, reason = "the bosonic Gauss-Newton operator is study-defined at a toy background, not a physical electroweak mass operator" },
    new { fieldId = "fourDimensionalObservedVacuumArtifactId", suppliable = false, reason = "the control branch is a toy 2D fiber-bundle mesh, not a four-dimensional observed vacuum" },
};

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
const string ApplicationSubjectKind = "gauge-invariant-neutral-charged-sector-separation-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    CarrierDimension.ToString(),
    DimG.ToString(),
    "f(b) = sum_e (b_e . n_omega)^2 / |b|^2; neutralContent(cluster) = trace of the invariant-axis projector over the cluster span",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool gaugeInvariantNeutralChargedSectorSeparationProbePassed =
    phase394PrecursorPassed &&
    phase395PrecursorPassed &&
    phase256ContractPresent &&
    phase394WorkdirPresent &&
    probeInternallyConsistent &&
    tripletNeutralChargedSplitObserved &&
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

string terminalStatus = gaugeInvariantNeutralChargedSectorSeparationProbePassed
    ? "gauge-invariant-neutral-charged-sector-separation-materialized-contract-fields-still-unfillable"
    : "gauge-invariant-neutral-charged-sector-separation-probe-blocked";
string decision = gaugeInvariantNeutralChargedSectorSeparationProbePassed
    ? "The first gauge-invariant observed-field-extraction skeleton is materialized on the control branch: the invariant-axis/charged-plane separation relative to the background symmetric-ansatz direction n_omega is exactly gauge-invariant (verified under simultaneous rotations at machine precision), and EVERY exact su(2) triplet of the recomputed positive bosonic spectrum splits as one neutral plus one charged pair (cluster neutral content 1.0 within tolerance on both backgrounds). This is the discrete residual-U(1) skeleton of {Z-like, W-pair-like} sector classification, built from invariants as Phase395 requires. It cannot fill any Phase256 contract field: the su(2)-only branch has no hypercharge, photon mixing, weak angle, scalar sector, physical vacuum, or scale/pole/GeV lineage, and the construction is study-defined rather than theorem-backed. The structural prerequisite now exists; the contract remains fail-closed awaiting the genuine electroweak embedding and source-lineage artifacts."
    : "Do not use the sector separation until the Phase394/395 precursors, extraction invariance, and triplet neutral-content checks all pass.";

var result = new
{
    phaseId = "phase396-gauge-invariant-neutral-charged-sector-separation-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    gaugeInvariantNeutralChargedSectorSeparationProbePassed,
    phase394PrecursorPassed,
    phase395PrecursorPassed,
    phase256ContractPresent,
    phase394WorkdirPresent,
    probeInternallyConsistent,
    extractionGaugeInvarianceVerified,
    tripletNeutralChargedSplitObserved,
    totalTripletClusterCount,
    maxNeutralContentDeviation,
    maxInvarianceResidual,
    minKernelNeutralContent,
    maxKernelNeutralContent,
    probeDefinitions = new
    {
        invariantAxis = "n_omega = dominant eigenvector of C = sum_e omega_e omega_e^T (the symmetric-ansatz direction)",
        extraction = "f(b) = sum_e (b_e . n_omega)^2 / sum_e |b_e|^2 (neutral fraction); 1 - f (charged fraction); plain per-edge Euclidean sums (study-defined weight)",
        clusterInvariant = "neutralContent(cluster) = sum over cluster modes of f(mode) = trace of the invariant-axis projector over the cluster span (basis-invariant)",
        hypothesis = "residual-U(1) skeleton: each exact su(2) triplet splits as one neutral direction plus one charged pair, i.e. neutralContent ~ 1.0",
        invarianceCheck = "f(R b; n_omega(R omega)) = f(b; n_omega(omega)) for a quarter turn about e2 and a generic rotation",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    expectedPhase256RequiredFieldCount = ExpectedPhase256RequiredFieldCount,
    phase256FieldAudit,
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
        "study-defined gauge-invariant separation, not a theorem with source lineage",
        "su(2)-only control branch: no hypercharge, no photon/Z mixing, no weak angle",
        "neutral/charged labels are residual-U(1) sector labels, NOT observed particle names",
        "no scalar sector, no physical vacuum, no scale, no pole, no GeV lineage",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase394SummaryPath = Phase394SummaryPath,
        phase395SummaryPath = Phase395SummaryPath,
        phase256SummaryPath = Phase256SummaryPath,
        phase394WorkdirModes = Phase394WorkdirModes,
        backgroundStateDir = BackgroundStateDir,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "gauge_invariant_neutral_charged_sector_separation_probe.json");
string summaryPath = Path.Combine(outputDir, "gauge_invariant_neutral_charged_sector_separation_probe_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"gaugeInvariantNeutralChargedSectorSeparationProbePassed={gaugeInvariantNeutralChargedSectorSeparationProbePassed}");
Console.WriteLine($"extractionGaugeInvarianceVerified={extractionGaugeInvarianceVerified}");
Console.WriteLine($"tripletNeutralChargedSplitObserved={tripletNeutralChargedSplitObserved}");
Console.WriteLine($"totalTripletClusterCount={totalTripletClusterCount}");
Console.WriteLine($"maxNeutralContentDeviation={maxNeutralContentDeviation:R}");
Console.WriteLine($"maxInvarianceResidual={maxInvarianceResidual:R}");
Console.WriteLine($"minKernelNeutralContent={minKernelNeutralContent:R}");
Console.WriteLine($"maxKernelNeutralContent={maxKernelNeutralContent:R}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Implementation.
// ---------------------------------------------------------------------------

BackgroundSectorRecord ProbeBackground(string backgroundId)
{
    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(omegaPath));
    double[] omega = omegaDoc.RootElement.GetProperty("coefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray();
    var omegaAxis = DominantOmegaAxis(omega);

    // Recomputed full bosonic spectrum from the Phase394 working directory.
    var modeFiles = Directory.GetFiles(Phase394WorkdirModes, $"{backgroundId}-mode-*.json")
        .OrderBy(path => int.Parse(Path.GetFileNameWithoutExtension(path).Split("-mode-")[1]))
        .ToArray();
    var eigenvalues = new List<double>();
    var modes = new List<double[]>();
    foreach (string path in modeFiles)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        eigenvalues.Add(doc.RootElement.GetProperty("eigenvalue").GetDouble());
        modes.Add(doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
    }
    int fullModeCount = modes.Count;
    double maxAbsEigenvalue = eigenvalues.Max(Math.Abs);
    double kernelTolerance = 1e-10 * Math.Max(maxAbsEigenvalue, 1e-30);

    // Per-mode neutral fractions.
    var neutralFractions = modes.Select(mode => NeutralFraction(mode, omegaAxis)).ToArray();

    // Kernel content.
    var kernelIndices = Enumerable.Range(0, fullModeCount).Where(i => Math.Abs(eigenvalues[i]) <= kernelTolerance).ToArray();
    double kernelNeutralContent = kernelIndices.Sum(i => neutralFractions[i]);

    // Triplet clusters of the positive spectrum (Phase394 rule) and their
    // basis-invariant neutral content.
    var positive = Enumerable.Range(0, fullModeCount)
        .Where(i => eigenvalues[i] > kernelTolerance)
        .OrderBy(i => eigenvalues[i])
        .ToArray();
    var clusterRecords = new List<ClusterRecord>();
    int index = 0;
    while (index < positive.Length)
    {
        int clusterSize = 1;
        while (index + clusterSize < positive.Length &&
               Math.Abs(eigenvalues[positive[index + clusterSize]] - eigenvalues[positive[index]]) <=
               TripletClusterRelativeTolerance * Math.Abs(eigenvalues[positive[index]]))
            clusterSize++;
        if (clusterSize == 3)
        {
            double neutralContent = 0.0;
            for (int member = 0; member < 3; member++)
                neutralContent += neutralFractions[positive[index + member]];
            clusterRecords.Add(new ClusterRecord
            {
                ClusterEigenvalue = eigenvalues[positive[index]],
                NeutralContent = neutralContent,
                NeutralContentDeviation = Math.Abs(neutralContent - 1.0),
            });
        }
        index += clusterSize;
    }
    int tripletClusterCount = clusterRecords.Count;
    int unitNeutralCount = clusterRecords.Count(cluster => cluster.NeutralContentDeviation <= NeutralContentTolerance);
    double maxDeviation = clusterRecords.Count > 0 ? clusterRecords.Max(cluster => cluster.NeutralContentDeviation) : double.NaN;

    // Exact gauge invariance of the extraction under simultaneous rotation.
    double maxInvarianceResidualLocal = 0.0;
    var rotations = new[]
    {
        RodriguesAdjoint([0.0, 0.0, 1.0], Math.PI / 2.0),
        RodriguesAdjoint(NormalizeVector([1.0, 2.0, 3.0]), 0.7),
    };
    foreach (var rotation in rotations)
    {
        var rotatedOmega = RotateCarrier(omega, rotation);
        var rotatedAxis = DominantOmegaAxis(rotatedOmega);
        foreach (int i in kernelIndices.Take(3).Concat(positive.Take(9)))
        {
            var rotatedMode = RotateCarrier(modes[i], rotation);
            double residual = Math.Abs(NeutralFraction(rotatedMode, rotatedAxis) - neutralFractions[i]);
            maxInvarianceResidualLocal = Math.Max(maxInvarianceResidualLocal, residual);
        }
    }

    return new BackgroundSectorRecord
    {
        FermionBackgroundId = backgroundId,
        PersistedOmegaPath = omegaPath,
        FullModeCount = fullModeCount,
        KernelDimension = kernelIndices.Length,
        KernelNeutralContent = kernelNeutralContent,
        OmegaInvariantAxis = omegaAxis,
        TripletClusterCount = tripletClusterCount,
        TripletClustersWithUnitNeutralContent = unitNeutralCount,
        MaxNeutralContentDeviation = maxDeviation,
        MaxInvarianceResidual = maxInvarianceResidualLocal,
        Clusters = clusterRecords,
    };
}

static double[] DominantOmegaAxis(double[] omega)
{
    var covariance = new double[3, 3];
    int edges = omega.Length / 3;
    for (int edge = 0; edge < edges; edge++)
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
                covariance[a, b] += omega[edge * 3 + a] * omega[edge * 3 + b];
    // 3x3 symmetric eigenvector via cyclic Jacobi.
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

static double NeutralFraction(double[] carrier, double[] axis)
{
    double neutral = 0.0;
    double total = 0.0;
    int edges = carrier.Length / 3;
    for (int edge = 0; edge < edges; edge++)
    {
        double dot = 0.0;
        for (int a = 0; a < 3; a++)
        {
            double value = carrier[edge * 3 + a];
            dot += value * axis[a];
            total += value * value;
        }
        neutral += dot * dot;
    }
    return total > 0.0 ? neutral / total : 0.0;
}

static double[] RotateCarrier(double[] carrier, double[,] rotation)
{
    var rotated = new double[carrier.Length];
    int edges = carrier.Length / 3;
    for (int edge = 0; edge < edges; edge++)
        for (int a = 0; a < 3; a++)
        {
            double sum = 0.0;
            for (int b = 0; b < 3; b++)
                sum += rotation[a, b] * carrier[edge * 3 + b];
            rotated[edge * 3 + a] = sum;
        }
    return rotated;
}

static double[,] RodriguesAdjoint(double[] axis, double theta)
{
    var n = NormalizeVector(axis);
    var k = new double[3, 3];
    for (int a = 0; a < 3; a++)
        for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
                k[b, c] += n[a] * LeviCivita3(a, b, c);
    var k2 = new double[3, 3];
    for (int a = 0; a < 3; a++)
        for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
                k2[a, b] += k[a, c] * k[c, b];
    var rotation = new double[3, 3];
    double sin = Math.Sin(theta);
    double cos = Math.Cos(theta);
    for (int a = 0; a < 3; a++)
        for (int b = 0; b < 3; b++)
            rotation[a, b] = (a == b ? 1.0 : 0.0) + sin * k[a, b] + (1.0 - cos) * k2[a, b];
    return rotation;
}

static double[] NormalizeVector(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(value => value * value));
    return vector.Select(value => value / norm).ToArray();
}

static double LeviCivita3(int a, int b, int c)
{
    if (a == b || b == c || a == c) return 0.0;
    if ((a == 0 && b == 1 && c == 2) ||
        (a == 1 && b == 2 && c == 0) ||
        (a == 2 && b == 0 && c == 1)) return 1.0;
    return -1.0;
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

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    value.ValueKind == JsonValueKind.Number &&
    value.TryGetInt32(out var parsed)
        ? parsed
        : null;

public sealed class ClusterRecord
{
    public required double ClusterEigenvalue { get; init; }
    public required double NeutralContent { get; init; }
    public required double NeutralContentDeviation { get; init; }
}

public sealed class BackgroundSectorRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required int FullModeCount { get; init; }
    public required int KernelDimension { get; init; }
    public required double KernelNeutralContent { get; init; }
    public required double[] OmegaInvariantAxis { get; init; }
    public required int TripletClusterCount { get; init; }
    public required int TripletClustersWithUnitNeutralContent { get; init; }
    public required double MaxNeutralContentDeviation { get; init; }
    public required double MaxInvarianceResidual { get; init; }
    public required IReadOnlyList<ClusterRecord> Clusters { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        persistedOmegaPath = PersistedOmegaPath,
        fullModeCount = FullModeCount,
        kernelDimension = KernelDimension,
        kernelNeutralContent = KernelNeutralContent,
        omegaInvariantAxis = OmegaInvariantAxis,
        tripletClusterCount = TripletClusterCount,
        tripletClustersWithUnitNeutralContent = TripletClustersWithUnitNeutralContent,
        maxNeutralContentDeviation = MaxNeutralContentDeviation,
        maxInvarianceResidual = MaxInvarianceResidual,
        clusters = Clusters.Select(cluster => new
        {
            clusterEigenvalue = cluster.ClusterEigenvalue,
            neutralContent = cluster.NeutralContent,
            neutralContentDeviation = cluster.NeutralContentDeviation,
        }).ToArray(),
    };
}
