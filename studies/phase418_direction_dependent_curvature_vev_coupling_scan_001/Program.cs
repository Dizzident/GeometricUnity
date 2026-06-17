using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Math;

// Phase418: direction-dependent curvature/VEV coupling scan.
//
// Phase410 closed the uniform bosonic curvature coupling: every rank-1 ray in
// the Phase405 su(3) control-branch vacuum manifold is bare-flat, so a uniform
// negative curvature mass term runs away and the lifted sector is
// block-degenerate. The restart prompt's next hypothesis is the only bosonic
// curvature route not yet tested: a gauge-chain-invariant but direction-
// dependent coupling.
//
// This phase scans the smallest target-blind block-isotypic menu on the same
// su(3) toy chain used by Phases403/405/410:
//
//   T = directions 0..2, the su(2) triplet block
//   D = directions 3..6, the charged doublet block
//   S = direction 7, the singlet/u(1) block
//
// The block projectors commute with the residual su(2)+u(1) adjoint action,
// so block-weighted quadratic/quartic terms are the finite direction-
// dependent gauge-chain-invariant ansatz. The scan deliberately separates:
//
//   1. pure block-weighted curvature quadratic terms: still run away on the
//      Phase405/410 rank-1 flat rays whenever any selected mass coefficient is
//      negative;
//   2. stabilized Landau-style block terms
//        V_b(t) = -(rho/2) m_b t^2 + (lambda/4) s_b t^4
//      with rho, lambda > 0 external; these can select the doublet for some
//      block weights, but the selector, stabilizer, scale, and projection
//      rows are not source-defined;
//   3. a minimal unlock contract saying exactly what a real GU source would
//      need to provide before this branch could fill Phase201 or Phase256.
//
// Fail-closed: this is a mathematical workbench, not a source law. It uses no
// W/Z/H targets, derives no GeV scale or pole rows, and mutates no contract.

const string DefaultOutputDir = "studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/output";
const string Phase405SummaryPath = "studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/output/vacuum_manifold_doublet_vev_orbit_scan_summary.json";
const string Phase410SummaryPath = "studies/phase410_curvature_coupled_vev_selection_probe_001/output/curvature_coupled_vev_selection_probe_summary.json";
const string Phase417SummaryPath = "studies/phase417_vector_spinor_144_decomposition_probe_001/output/vector_spinor_144_decomposition_probe_summary.json";
const string ApplicationSubjectKind = "direction-dependent-curvature-vev-coupling-scan";

var outputDir = Environment.GetEnvironmentVariable("PHASE418_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase405 = JsonDocument.Parse(File.ReadAllText(Phase405SummaryPath));
using var phase410 = JsonDocument.Parse(File.ReadAllText(Phase410SummaryPath));
using var phase417 = JsonDocument.Parse(File.ReadAllText(Phase417SummaryPath));

bool phase405PrecursorPassed =
    JsonBool(phase405.RootElement, "vacuumManifoldDoubletVevOrbitScanPassed") is true &&
    JsonBool(phase405.RootElement, "vacuumManifoldPermitsConstantDoubletVev") is true &&
    JsonBool(phase405.RootElement, "noSelectionMechanismAtConstantRank1Level") is true;
bool phase410PrecursorPassed =
    JsonBool(phase410.RootElement, "curvatureCoupledVevSelectionProbePassed") is true &&
    JsonBool(phase410.RootElement, "curvatureCouplingFailsToSelectDoublet") is true &&
    JsonBool(phase410.RootElement, "directionDependentCouplingProbed") is false;
bool phase417PrecursorPassed =
    JsonBool(phase417.RootElement, "vectorSpinor144DecompositionProbePassed") is true &&
    JsonBool(phase417.RootElement, "vectorSpinor144StillRequiresBosonicProjectionMap") is true;

var algebra = LieAlgebraFactory.CreateSu3();
const int DimG = 8;

var blocks = new[]
{
    new Block("T", "su2-triplet", new[] { 0, 1, 2 }, Dimension: 3, Su2Casimir: 2.0, U1ChargeSquared: 0.0),
    new Block("D", "charged-doublet", new[] { 3, 4, 5, 6 }, Dimension: 4, Su2Casimir: 0.75, U1ChargeSquared: 0.75),
    new Block("S", "singlet-u1", new[] { 7 }, Dimension: 1, Su2Casimir: 0.0, U1ChargeSquared: 0.0),
};

double[,] Projector(params int[] axes)
{
    var result = new double[DimG, DimG];
    foreach (int axis in axes)
        result[axis, axis] = 1.0;
    return result;
}

double[] Unit(int i)
{
    var result = new double[DimG];
    result[i] = 1.0;
    return result;
}

double[,] AdjointMatrix(int generator)
{
    var result = new double[DimG, DimG];
    for (int col = 0; col < DimG; col++)
    {
        var bracket = algebra.Bracket(Unit(generator), Unit(col));
        for (int row = 0; row < DimG; row++)
            result[row, col] = bracket[row];
    }
    return result;
}

double[,] Commutator(double[,] a, double[,] b)
{
    var result = new double[DimG, DimG];
    for (int r = 0; r < DimG; r++)
        for (int c = 0; c < DimG; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < DimG; k++)
                sum += a[r, k] * b[k, c] - b[r, k] * a[k, c];
            result[r, c] = sum;
        }
    return result;
}

double Frobenius(double[,] m)
{
    double sum = 0.0;
    for (int r = 0; r < m.GetLength(0); r++)
        for (int c = 0; c < m.GetLength(1); c++)
            sum += m[r, c] * m[r, c];
    return Math.Sqrt(sum);
}

var residualGaugeGenerators = new[] { 0, 1, 2, 7 };
var fullGenerators = Enumerable.Range(0, DimG).ToArray();
var projectors = blocks.ToDictionary(b => b.Id, b => Projector(b.Axes));

double maxResidualGaugeProjectorCommutator = 0.0;
foreach (var block in blocks)
    foreach (int generator in residualGaugeGenerators)
        maxResidualGaugeProjectorCommutator = Math.Max(
            maxResidualGaugeProjectorCommutator,
            Frobenius(Commutator(AdjointMatrix(generator), projectors[block.Id])));
bool blockProjectorsCommuteWithResidualGaugeAction = maxResidualGaugeProjectorCommutator <= 1e-12;

double maxFullSu3ProjectorCommutator = 0.0;
foreach (var block in blocks)
    foreach (int generator in fullGenerators)
        maxFullSu3ProjectorCommutator = Math.Max(
            maxFullSu3ProjectorCommutator,
            Frobenius(Commutator(AdjointMatrix(generator), projectors[block.Id])));
bool blockProjectorsCommuteWithFullSu3 = maxFullSu3ProjectorCommutator <= 1e-12;

// Bracket-degree is a target-blind algebraic diagnostic: for each block, count
// how many basis-pair brackets with that source block are nonzero.
var bracketDegrees = blocks.ToDictionary(
    b => b.Id,
    b =>
    {
        int nonzero = 0;
        int total = 0;
        foreach (int i in b.Axes)
            for (int j = 0; j < DimG; j++)
            {
                total++;
                if (NormSquared(algebra.Bracket(Unit(i), Unit(j))) > 1e-15)
                    nonzero++;
            }
        return (nonzero, total, averagePerAxis: (double)nonzero / b.Axes.Length);
    });

var candidates = new List<CouplingCandidate>
{
    new(
        "uniform-control",
        "all block masses and stabilizers equal; reproduces Phase410 direction-blindness when quartic stabilization is ignored",
        new() { ["T"] = 1.0, ["D"] = 1.0, ["S"] = 1.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "block-dimension",
        "negative curvature mass proportional to block dimension",
        new() { ["T"] = 3.0, ["D"] = 4.0, ["S"] = 1.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "inverse-block-dimension",
        "negative curvature mass proportional to inverse block dimension",
        new() { ["T"] = 1.0 / 3.0, ["D"] = 1.0 / 4.0, ["S"] = 1.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "su2-casimir",
        "negative curvature mass proportional to residual su(2) Casimir",
        new() { ["T"] = 2.0, ["D"] = 0.75, ["S"] = 0.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "inverse-nonzero-su2-casimir",
        "negative curvature mass proportional to inverse nonzero residual su(2) Casimir",
        new() { ["T"] = 0.5, ["D"] = 4.0 / 3.0, ["S"] = 0.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "u1-charge-squared",
        "negative curvature mass proportional to residual u(1) charge squared in the su(3) toy branch",
        new() { ["T"] = 0.0, ["D"] = 0.75, ["S"] = 0.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "su2-plus-u1",
        "negative curvature mass proportional to residual su(2) Casimir plus u(1) charge squared",
        new() { ["T"] = 2.0, ["D"] = 1.5, ["S"] = 0.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "bracket-degree",
        "negative curvature mass proportional to average nonzero bracket partners per basis axis",
        new()
        {
            ["T"] = bracketDegrees["T"].averagePerAxis,
            ["D"] = bracketDegrees["D"].averagePerAxis,
            ["S"] = bracketDegrees["S"].averagePerAxis,
        },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: false),
    new(
        "doublet-projector-control",
        "control row: a hand-declared doublet projector, included only to verify the selection cone algebra",
        new() { ["T"] = 0.0, ["D"] = 1.0, ["S"] = 0.0 },
        UnitStabilizer(),
        SourceDefined: false,
        TautologicalDoubletProjector: true),
};

var candidateResults = candidates.Select(Evaluate).ToArray();

int pureQuadraticCandidateCount = candidateResults.Count(r => r.HasNegativeMassDirection);
int pureQuadraticFiniteVevCandidateCount = 0;
bool pureQuadraticDirectionDependentCouplingsStillRunAway =
    phase410PrecursorPassed && pureQuadraticCandidateCount == candidates.Count && pureQuadraticFiniteVevCandidateCount == 0;

int stabilizedLandauMenuCandidateCount = candidateResults.Length;
int stabilizedCandidateSelectsDoubletCount = candidateResults.Count(r => r.SelectedBlocks.SequenceEqual(new[] { "D" }));
int nonTautologicalDoubletSelectorCount = candidateResults.Count(r =>
    r.SelectedBlocks.SequenceEqual(new[] { "D" }) && !r.TautologicalDoubletProjector);
int sourceDefinedCandidateCount = candidateResults.Count(r => r.SourceDefined);
int sourceDefinedDoubletSelectorCount = candidateResults.Count(r =>
    r.SourceDefined && r.SelectedBlocks.SequenceEqual(new[] { "D" }));
bool directionDependentCouplingCanSelectDoubletInAdHocLandauAnsatz =
    stabilizedCandidateSelectsDoubletCount > 0;
bool sourceDefinedDirectionDependentCurvatureCouplingFound = sourceDefinedCandidateCount > 0;
bool sourceDefinedDoubletSelectingCurvatureCouplingFound = sourceDefinedDoubletSelectorCount > 0;
bool directionDependentCouplingSourceLawStillMissing =
    !sourceDefinedDirectionDependentCurvatureCouplingFound &&
    directionDependentCouplingCanSelectDoubletInAdHocLandauAnsatz;
bool finiteVevScaleStillExternal = true;
bool quarticStabilizerStillExternal = true;

var requiredSpecificationFields = new[]
{
    "source equation placing a direction-dependent curvature kernel in the bosonic or Dirac-like action",
    "residual gauge-chain projector or equivalent block selector, not chosen post hoc",
    "sign and normalization of the curvature mass kernel",
    "source-defined quartic or higher stabilizer on the selected rank-1 rays",
    "VEV amplitude or dimensionful scale lineage",
    "observed photon/W/Z/H projection rows",
    "weak-angle and coupling lineage",
    "pole extraction and GeV/unit normalization",
};

const bool physicalTargetsConsultedForConstruction = false;
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
    "su3 residual su2+u1 block projectors T(0-2), D(3-6), S(7)",
    "pure block-weighted curvature quadratics still run away on Phase405/410 rank-1 flat rays",
    "stabilized Landau block menu evaluated by depth score m_b^2/s_b",
    "all selectors sourceDefined=false and no observed targets")))).ToLowerInvariant();

bool directionDependentCurvatureVevCouplingScanPassed =
    phase405PrecursorPassed &&
    phase410PrecursorPassed &&
    phase417PrecursorPassed &&
    blockProjectorsCommuteWithResidualGaugeAction &&
    !blockProjectorsCommuteWithFullSu3 &&
    pureQuadraticDirectionDependentCouplingsStillRunAway &&
    directionDependentCouplingCanSelectDoubletInAdHocLandauAnsatz &&
    sourceDefinedDoubletSelectorCount == 0 &&
    directionDependentCouplingSourceLawStillMissing &&
    finiteVevScaleStillExternal &&
    quarticStabilizerStillExternal &&
    !physicalTargetsConsultedForConstruction &&
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

string terminalStatus = directionDependentCurvatureVevCouplingScanPassed
    ? "direction-dependent-curvature-couplings-can-select-doublet-only-with-extra-source-law"
    : "direction-dependent-curvature-vev-coupling-scan-blocked";

string decision = directionDependentCurvatureVevCouplingScanPassed
    ? "The direction-dependent curvature branch is now characterized at the minimal block-isotypic level. Residual su(2)+u(1)-invariant block projectors exist and stabilized Landau-style block weights can select the doublet in target-blind algebraic menus, so the branch is not mathematically empty. However, pure direction-dependent quadratic curvature terms still run away on the Phase405/410 rank-1 flat rays, and every finite-VEV selector in the stabilized menu imports an extra block mass law plus an extra quartic stabilizer. The current GU sources define neither the direction-dependent curvature kernel nor its sign, scale, stabilizer, VEV amplitude, observed-field rows, weak-angle lineage, pole extraction, or GeV normalization. This phase therefore materializes the exact source-law shape needed for progress, but fills no Phase201 or Phase256 field and promotes no W/Z/H mass."
    : "Do not use the scan until the precursor, projector, and fail-closed checks pass.";

var result = new
{
    phaseId = "phase418-direction-dependent-curvature-vev-coupling-scan",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    directionDependentCurvatureVevCouplingScanPassed,
    phase405PrecursorPassed,
    phase410PrecursorPassed,
    phase417PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    blockDefinitions = blocks.Select(b => new
    {
        b.Id,
        b.Description,
        b.Axes,
        b.Dimension,
        b.Su2Casimir,
        b.U1ChargeSquared,
        bracketDegreeNonzero = bracketDegrees[b.Id].nonzero,
        bracketDegreeTotal = bracketDegrees[b.Id].total,
        bracketDegreeAveragePerAxis = bracketDegrees[b.Id].averagePerAxis,
    }).ToArray(),
    blockProjectorsCommuteWithResidualGaugeAction,
    maxResidualGaugeProjectorCommutator,
    blockProjectorsCommuteWithFullSu3,
    maxFullSu3ProjectorCommutator,
    directionDependentAnsatzIsResidualGaugeInvariantOnly = blockProjectorsCommuteWithResidualGaugeAction && !blockProjectorsCommuteWithFullSu3,
    pureQuadraticCandidateCount,
    pureQuadraticFiniteVevCandidateCount,
    pureQuadraticDirectionDependentCouplingsStillRunAway,
    stabilizedLandauMenuCandidateCount,
    stabilizedCandidateSelectsDoubletCount,
    nonTautologicalDoubletSelectorCount,
    sourceDefinedCandidateCount,
    sourceDefinedDoubletSelectorCount,
    directionDependentCouplingCanSelectDoubletInAdHocLandauAnsatz,
    sourceDefinedDirectionDependentCurvatureCouplingFound,
    sourceDefinedDoubletSelectingCurvatureCouplingFound,
    directionDependentCouplingSourceLawStillMissing,
    finiteVevScaleStillExternal,
    quarticStabilizerStillExternal,
    candidateResults,
    requiredSpecificationFieldCount = requiredSpecificationFields.Length,
    suppliedRequiredSpecificationFieldCount = 0,
    requiredSpecificationFields,
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
    explicitCandidateOnlyNonclaims = new[]
    {
        "block-weighted terms are residual su(2)+u(1)-invariant on the su(3) toy branch, not full-su(3)-invariant",
        "pure quadratic direction-dependent curvature couplings still run away on Phase405/410 rank-1 flat rays",
        "the stabilized Landau ansatz adds a quartic stabilizer not supplied by the current source",
        "candidate block selectors are target-blind algebraic workbench rows, not source-defined GU laws",
        "rho/lambda fixes only a symbolic VEV amplitude ratio; no GeV scale or pole extraction is derived",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase405SummaryPath = Phase405SummaryPath,
        phase410SummaryPath = Phase410SummaryPath,
        phase417SummaryPath = Phase417SummaryPath,
        motivatingPromptBranch = "docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md: direction-dependent curvature/VEV coupling scan",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "direction_dependent_curvature_vev_coupling_scan.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "direction_dependent_curvature_vev_coupling_scan_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"directionDependentCurvatureVevCouplingScanPassed={directionDependentCurvatureVevCouplingScanPassed}");
Console.WriteLine($"blockProjectorsCommuteWithResidualGaugeAction={blockProjectorsCommuteWithResidualGaugeAction} maxResidual={maxResidualGaugeProjectorCommutator:E3}");
Console.WriteLine($"blockProjectorsCommuteWithFullSu3={blockProjectorsCommuteWithFullSu3} maxFull={maxFullSu3ProjectorCommutator:E3}");
Console.WriteLine($"pureQuadraticDirectionDependentCouplingsStillRunAway={pureQuadraticDirectionDependentCouplingsStillRunAway}");
Console.WriteLine($"stabilizedLandauMenuCandidateCount={stabilizedLandauMenuCandidateCount}");
Console.WriteLine($"stabilizedCandidateSelectsDoubletCount={stabilizedCandidateSelectsDoubletCount}");
Console.WriteLine($"nonTautologicalDoubletSelectorCount={nonTautologicalDoubletSelectorCount}");
Console.WriteLine($"sourceDefinedDoubletSelectorCount={sourceDefinedDoubletSelectorCount}");
Console.WriteLine($"directionDependentCouplingSourceLawStillMissing={directionDependentCouplingSourceLawStillMissing}");
Console.WriteLine($"finiteVevScaleStillExternal={finiteVevScaleStillExternal}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

CouplingCandidateResult Evaluate(CouplingCandidate candidate)
{
    var scores = blocks.ToDictionary(
        b => b.Id,
        b =>
        {
            double mass = candidate.MassWeights[b.Id];
            double stabilizer = candidate.StabilizerWeights[b.Id];
            return stabilizer > 0.0 && mass > 0.0
                ? mass * mass / stabilizer
                : 0.0;
        });
    double maxScore = scores.Values.Max();
    var selectedBlocks = scores
        .Where(kv => maxScore > 0.0 && Math.Abs(kv.Value - maxScore) <= 1e-12 * Math.Max(1.0, maxScore))
        .Select(kv => kv.Key)
        .OrderBy(x => x)
        .ToArray();

    var symbolicMinima = blocks.ToDictionary(
        b => b.Id,
        b =>
        {
            double mass = candidate.MassWeights[b.Id];
            double stabilizer = candidate.StabilizerWeights[b.Id];
            bool finite = mass > 0.0 && stabilizer > 0.0;
            return new SymbolicMinimum(
                FiniteMinimumExists: finite,
                TSquaredInUnitsOfRhoOverLambda: finite ? mass / stabilizer : 0.0,
                DepthScoreInUnitsOfRhoSquaredOverLambda: finite ? mass * mass / (4.0 * stabilizer) : 0.0);
        });

    return new CouplingCandidateResult(
        candidate.Id,
        candidate.Description,
        candidate.MassWeights,
        candidate.StabilizerWeights,
        scores,
        selectedBlocks,
        candidate.SourceDefined,
        candidate.TautologicalDoubletProjector,
        candidate.MassWeights.Values.Any(v => v > 0.0),
        PureQuadraticRunawayOnSelectedFlatRay: candidate.MassWeights.Values.Any(v => v > 0.0),
        symbolicMinima);
}

Dictionary<string, double> UnitStabilizer() => new()
{
    ["T"] = 1.0,
    ["D"] = 1.0,
    ["S"] = 1.0,
};

static double NormSquared(double[] values)
{
    double sum = 0.0;
    foreach (double value in values)
        sum += value * value;
    return sum;
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

sealed record Block(
    string Id,
    string Description,
    int[] Axes,
    int Dimension,
    double Su2Casimir,
    double U1ChargeSquared);

sealed record CouplingCandidate(
    string Id,
    string Description,
    Dictionary<string, double> MassWeights,
    Dictionary<string, double> StabilizerWeights,
    bool SourceDefined,
    bool TautologicalDoubletProjector);

sealed record CouplingCandidateResult(
    string Id,
    string Description,
    Dictionary<string, double> MassWeights,
    Dictionary<string, double> StabilizerWeights,
    Dictionary<string, double> StabilizedDepthScores,
    string[] SelectedBlocks,
    bool SourceDefined,
    bool TautologicalDoubletProjector,
    bool HasNegativeMassDirection,
    bool PureQuadraticRunawayOnSelectedFlatRay,
    Dictionary<string, SymbolicMinimum> SymbolicMinima);

sealed record SymbolicMinimum(
    bool FiniteMinimumExists,
    double TSquaredInUnitsOfRhoOverLambda,
    double DepthScoreInUnitsOfRhoSquaredOverLambda);
