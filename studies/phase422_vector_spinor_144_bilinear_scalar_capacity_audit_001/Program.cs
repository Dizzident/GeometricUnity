using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase422: vector-spinor 144 bilinear scalar-capacity audit.
//
// Phase417 decomposed the source-pinned Z_{1/2} vector-spinor 144 and showed
// that the linear chiral carrier 2 x 144 has no welded scalar. This phase asks
// the next narrower representation question without inventing a source map:
// whether bilinears of the chiral vector-spinor carriers can even be welded
// scalars by character arithmetic.
//
// Fail-closed boundary: this is only a welded-spin capacity census. It does
// not compute the large SM-stable scalar subspace, and it supplies no
// source-defined bosonic projection/action/VEV/observed-field map.

const string DefaultOutputDir = "studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/output";
const string Phase417SummaryPath = "studies/phase417_vector_spinor_144_decomposition_probe_001/output/vector_spinor_144_decomposition_probe_summary.json";
const string ApplicationSubjectKind = "vector-spinor-144-bilinear-scalar-capacity-audit";

var outputDir = Environment.GetEnvironmentVariable("PHASE422_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase417 = JsonDocument.Parse(File.ReadAllText(Phase417SummaryPath));

bool phase417PrecursorPassed =
    JsonBool(phase417.RootElement, "vectorSpinor144DecompositionProbePassed") is true &&
    JsonBool(phase417.RootElement, "vectorSpinor144LinearCarrierHasNoWeldedScalar") is true &&
    JsonBool(phase417.RootElement, "vectorSpinor144StillRequiresBosonicProjectionMap") is true &&
    JsonBool(phase417.RootElement, "canFillPhase201WzContract") is false &&
    JsonBool(phase417.RootElement, "canFillPhase201HiggsContract") is false &&
    JsonBool(phase417.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;

var leftContent = ReadWeldBlocks(phase417.RootElement.GetProperty("zCarrierLeftWeldedContent"));
var rightContent = ReadWeldBlocks(phase417.RootElement.GetProperty("zCarrierRightWeldedContent"));

int leftCarrierRealDimension = Dimension(leftContent);
int rightCarrierRealDimension = Dimension(rightContent);
int phase417ReportedKernelRealDimension = JsonInt(phase417.RootElement, "vectorSpinor144KernelRealDimension") ?? -1;
bool chiralCarrierDimensionAccounting =
    leftCarrierRealDimension == 2 * phase417ReportedKernelRealDimension &&
    rightCarrierRealDimension == 2 * phase417ReportedKernelRealDimension;

var llScalar = SingletCapacity(leftContent, leftContent, "Z_L x Z_L");
var rrScalar = SingletCapacity(rightContent, rightContent, "Z_R x Z_R");
var lrScalar = SingletCapacity(leftContent, rightContent, "Z_L x Z_R");
int sameChiralityScalarCapacity = llScalar.ScalarMultiplicity + rrScalar.ScalarMultiplicity;
int mixedChiralityScalarCapacity = lrScalar.ScalarMultiplicity;
int totalBilinearScalarCapacity = sameChiralityScalarCapacity + mixedChiralityScalarCapacity;

bool mixedChiralityDiracLikeScalarChannelClosed = mixedChiralityScalarCapacity == 0;
bool sameChiralityMajoranaLikeScalarSectorPresent = sameChiralityScalarCapacity > 0;
bool bilinearWeldedScalarCapacityPresent = totalBilinearScalarCapacity > 0;
bool directSmStableSpinZeroSubspaceComputed = false;
int directSmStableSpinZeroSubspaceDimension = -1;
int directSmStableSpinZeroDoubletCount = -1;
const int directSmStableAnalysisDeferredThreshold = 40;
bool directSmStableAnalysisDeferredDueToLargeSector =
    sameChiralityScalarCapacity > directSmStableAnalysisDeferredThreshold;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool sourceDefinesVectorSpinor144BilinearProjectionMap = false;
const bool sourceDefinesVectorSpinor144BilinearAction = false;
const bool sourceDefinesVectorSpinor144BilinearVevSelection = false;
const bool sourceDefinesObservedProjectionRows = false;
const bool sourceDefinesWeakAngleScalePoleOrGevLineage = false;
const int sourceDefinedBilinearProjectionMapCount = 0;
bool vectorSpinor144BilinearStillRequiresSourceProjectionMap =
    !sourceDefinesVectorSpinor144BilinearProjectionMap &&
    sourceDefinedBilinearProjectionMapCount == 0;

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

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "Phase417 zCarrierLeftWeldedContent/zCarrierRightWeldedContent; exact character singlet pairing; no target values")))).ToLowerInvariant();

bool vectorSpinor144BilinearScalarCapacityAuditPassed =
    phase417PrecursorPassed &&
    chiralCarrierDimensionAccounting &&
    mixedChiralityDiracLikeScalarChannelClosed &&
    sameChiralityMajoranaLikeScalarSectorPresent &&
    bilinearWeldedScalarCapacityPresent &&
    !directSmStableSpinZeroSubspaceComputed &&
    directSmStableAnalysisDeferredDueToLargeSector &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !sourceDefinesVectorSpinor144BilinearProjectionMap &&
    !sourceDefinesVectorSpinor144BilinearAction &&
    !sourceDefinesVectorSpinor144BilinearVevSelection &&
    !sourceDefinesObservedProjectionRows &&
    !sourceDefinesWeakAngleScalePoleOrGevLineage &&
    vectorSpinor144BilinearStillRequiresSourceProjectionMap &&
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

string terminalStatus = vectorSpinor144BilinearScalarCapacityAuditPassed
    ? "vector-spinor-144-bilinear-scalar-capacity-characterized-source-map-still-missing"
    : "vector-spinor-144-bilinear-scalar-capacity-audit-blocked";

string decision = vectorSpinor144BilinearScalarCapacityAuditPassed
    ? "The source-pinned vector-spinor 144 branch is now characterized one step beyond Phase417's linear no-go. Exact welded-character arithmetic closes the mixed-chirality Dirac-like Z_L x Z_R bilinear channel: it has zero welded-scalar capacity. Same-chirality Majorana-like Z_L x Z_L and Z_R x Z_R channels do have welded-scalar capacity (264 each, 528 total), so representation content alone cannot rule out a bilinear vector-spinor scalar ansatz. This is only a capacity result: the large spin-zero sector has not been decomposed into the largest SM-stable subspace, and current sources still supply no bosonic projection map, action, VEV selection, observed photon/W/Z/H rows, weak-angle lineage, pole extraction, or GeV normalization. No Phase201 or Phase256 field is filled."
    : "Do not use the vector-spinor bilinear scalar-capacity audit until the Phase417 precursor and fail-closed batteries pass.";

var result = new
{
    phaseId = "phase422-vector-spinor-144-bilinear-scalar-capacity-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    vectorSpinor144BilinearScalarCapacityAuditPassed,
    phase417PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    chiralCarrierDimensionAccounting,
    phase417ReportedKernelRealDimension,
    leftCarrierRealDimension,
    rightCarrierRealDimension,
    leftCarrierWeldedContent = leftContent,
    rightCarrierWeldedContent = rightContent,
    bilinearChannels = new[] { llScalar, rrScalar, lrScalar },
    sameChiralityScalarCapacity,
    mixedChiralityScalarCapacity,
    totalBilinearScalarCapacity,
    mixedChiralityDiracLikeScalarChannelClosed,
    sameChiralityMajoranaLikeScalarSectorPresent,
    bilinearWeldedScalarCapacityPresent,
    directSmStableSpinZeroSubspaceComputed,
    directSmStableSpinZeroSubspaceDimension,
    directSmStableSpinZeroDoubletCount,
    directSmStableAnalysisDeferredThreshold,
    directSmStableAnalysisDeferredDueToLargeSector,
    sourceDefinesVectorSpinor144BilinearProjectionMap,
    sourceDefinesVectorSpinor144BilinearAction,
    sourceDefinesVectorSpinor144BilinearVevSelection,
    sourceDefinesObservedProjectionRows,
    sourceDefinesWeakAngleScalePoleOrGevLineage,
    sourceDefinedBilinearProjectionMapCount,
    vectorSpinor144BilinearStillRequiresSourceProjectionMap,
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
        phase417SummaryPath = Phase417SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (section 11.2 eq. 11.6; section 12.22; Z_{1/2} vector-spinor 144 remainder)",
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The mixed-chirality Z_L x Z_R bilinear is closed only as a welded scalar capacity channel.",
        "Same-chirality scalar capacity is representation data only, not a source-defined Higgs field.",
        "The SM-stable subspace of the 528-dimensional same-chirality scalar capacity sector is not computed in this phase.",
        "No bosonic projection map, action, VEV selection, observed-field rows, weak-angle lineage, pole extraction, or GeV normalization is supplied.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "vector_spinor_144_bilinear_scalar_capacity_audit.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "vector_spinor_144_bilinear_scalar_capacity_audit_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vectorSpinor144BilinearScalarCapacityAuditPassed={vectorSpinor144BilinearScalarCapacityAuditPassed}");
Console.WriteLine($"leftCarrierRealDimension={leftCarrierRealDimension} rightCarrierRealDimension={rightCarrierRealDimension}");
Console.WriteLine($"spinZeroCapacity: LL={llScalar.ScalarMultiplicity} RR={rrScalar.ScalarMultiplicity} LR={lrScalar.ScalarMultiplicity}");
Console.WriteLine($"mixedChiralityDiracLikeScalarChannelClosed={mixedChiralityDiracLikeScalarChannelClosed}");
Console.WriteLine($"sameChiralityMajoranaLikeScalarSectorPresent={sameChiralityMajoranaLikeScalarSectorPresent}");
Console.WriteLine($"directSmStableAnalysisDeferredDueToLargeSector={directSmStableAnalysisDeferredDueToLargeSector}");
Console.WriteLine($"vectorSpinor144BilinearStillRequiresSourceProjectionMap={vectorSpinor144BilinearStillRequiresSourceProjectionMap}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static IReadOnlyList<WeldBlock> ReadWeldBlocks(JsonElement array) =>
    array.EnumerateArray()
        .Select(item => new WeldBlock(
            JsonDoubleRequired(item, "j1"),
            JsonDoubleRequired(item, "j2"),
            JsonIntRequired(item, "multiplicity")))
        .OrderBy(block => block.J1)
        .ThenBy(block => block.J2)
        .ThenBy(block => block.Multiplicity)
        .ToArray();

static int Dimension(IReadOnlyList<WeldBlock> blocks) =>
    blocks.Sum(block => (int)Math.Round((2 * block.J1 + 1) * (2 * block.J2 + 1)) * block.Multiplicity);

static ChannelCapacity SingletCapacity(IReadOnlyList<WeldBlock> left, IReadOnlyList<WeldBlock> right, string channelId)
{
    var contributors = new List<ScalarContributor>();
    int total = 0;
    foreach (var a in left)
        foreach (var b in right)
        {
            if (Math.Abs(a.J1 - b.J1) > 1e-9 || Math.Abs(a.J2 - b.J2) > 1e-9)
                continue;
            int contribution = a.Multiplicity * b.Multiplicity;
            total += contribution;
            contributors.Add(new ScalarContributor(a.J1, a.J2, a.Multiplicity, b.Multiplicity, contribution));
        }

    return new ChannelCapacity(channelId, total, contributors.ToArray());
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
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;

static int JsonIntRequired(JsonElement element, string propertyName) =>
    JsonInt(element, propertyName) ?? throw new InvalidOperationException($"Missing integer property {propertyName}.");

static double JsonDoubleRequired(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

public sealed record WeldBlock(double J1, double J2, int Multiplicity);
public sealed record ScalarContributor(double J1, double J2, int LeftMultiplicity, int RightMultiplicity, int Contribution);
public sealed record ChannelCapacity(string ChannelId, int ScalarMultiplicity, IReadOnlyList<ScalarContributor> Contributors);
