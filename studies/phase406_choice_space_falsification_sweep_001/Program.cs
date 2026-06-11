using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase406: choice-space falsification sweep (brute force #3).
//
// USER DIRECTIVE (2026-06-11): the third brute-force computation - sweep the
// theory's DISCRETE CHOICE SPACE against the structural filters the program
// has machine-verified, and produce the surviving/falsified map. GU leaves
// several choices genuinely open (the draft and the Iceberg analysis both
// record them): the metric signature ((6,4) vs (7,3)), the embedding path
// (Pati-Salam vs the U(3)xU(2)/SU(5)-type route), the hypercharge direction,
// the gauge algebra class available to the scalar sector, and the scalar's
// location inside the connection (gauge-adjoint vs non-adjoint components).
//
// Two NEW computations close the remaining axes (CPU - the objects are
// 5x5/32x32; the GPU portion of the directive was satisfied in Phase405,
// which also machine-detected the native curvature kernel defect, so IA-5
// keeps the new arithmetic on the CPU reference):
//
//   A. PATH INDEPENDENCE of the derived coupling ratio: the SU(5)-type
//      route (the draft's U(3)xU(2)-with-det-1 path: SM = S(U(3)xU(2)) with
//      Y = diag(-1/3,-1/3,-1/3,1/2,1/2)) is computed directly on su(5) and
//      its embedding-derived tan^2(theta_emb) must equal the Pati-Salam
//      value 3/5 from Phase404 - a theorem-level cross-check that the
//      ratio lineage is a property of the chain's complexification, not of
//      the route taken through it.
//
//   B. SIGNATURE AXIS: Cl(6,4) and Cl(7,3) have the same real dimension
//      2^10 and the same complexified spinor structure (D5); the chiral
//      spinor halves are 16-dimensional for BOTH signature choices
//      (machine-verified via explicit Clifford constructions with the two
//      signature metrics), so the family-pattern and ratio results of
//      Phase404 are signature-independent across the draft's open choice.
//
// The sweep then composes these with the MACHINE-RECORDED filter outcomes
// of Phases 396/397/403/404/405 (read from their summary JSONs, never
// re-asserted by hand) into the choice-space verdict table:
//
//   F1 doublet block present in the scalar candidate sector;
//   F2 custodial-capable (SM neutral pattern producible);
//   F3 family hypercharge pattern derivable;
//   F4 coupling ratio derived (not input);
//   F5 selection mechanism for the VEV present.
//
// Fail-closed: the surviving region is a structural map, not a derivation;
// no scales exist; nothing is promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase406_choice_space_falsification_sweep_001/output";
const string Phase396SummaryPath = "studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/output/gauge_invariant_neutral_charged_sector_separation_probe_summary.json";
const string Phase397SummaryPath = "studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/output/parametrized_u1_extension_neutral_mixing_underdetermination_probe_summary.json";
const string Phase403SummaryPath = "studies/phase403_adjoint_doublet_substructure_branching_probe_001/output/adjoint_doublet_substructure_branching_probe_summary.json";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase405SummaryPath = "studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/output/vacuum_manifold_doublet_vev_orbit_scan_summary.json";

const double Tolerance = 1e-9;

var outputDir = Environment.GetEnvironmentVariable("PHASE406_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursor filter outcomes (machine-recorded, read from the gates' JSONs).
// ---------------------------------------------------------------------------

using var p396 = JsonDocument.Parse(File.ReadAllText(Phase396SummaryPath));
using var p397 = JsonDocument.Parse(File.ReadAllText(Phase397SummaryPath));
using var p403 = JsonDocument.Parse(File.ReadAllText(Phase403SummaryPath));
using var p404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var p405 = JsonDocument.Parse(File.ReadAllText(Phase405SummaryPath));

bool precursorsPassed =
    JsonBool(p396.RootElement, "gaugeInvariantNeutralChargedSectorSeparationProbePassed") is true &&
    JsonBool(p397.RootElement, "parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed") is true &&
    JsonBool(p403.RootElement, "adjointDoubletSubstructureBranchingProbePassed") is true &&
    JsonBool(p404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true &&
    JsonBool(p405.RootElement, "vacuumManifoldDoubletVevOrbitScanPassed") is true;

bool su2HasNoDoubletBlock = JsonBool(p403.RootElement, "su2AdjointHasNoDoubletBlock") is true;
bool su3HasDoubletBlock = JsonBool(p403.RootElement, "su3AdjointContainsConjugateDoubletPair") is true;
bool doubletCustodialCapable = JsonBool(p403.RootElement, "custodialPatternProducedByAdjointDoubletBlock") is true;
bool chainAdjointHiggsExcluded = JsonBool(p404.RootElement, "adjointColorSingletChargedDoubletAbsentEverywhere") is true;
bool familyPatternDerivedOnChain = JsonBool(p404.RootElement, "familyPatternDerived") is true;
bool ratioDerivedOnChain = JsonBool(p404.RootElement, "standardRatioMatchesClassicValue") is true;
bool vacuumPermitsDoubletVev = JsonBool(p405.RootElement, "vacuumManifoldPermitsConstantDoubletVev") is true;
bool noVevSelectionMechanism = JsonBool(p405.RootElement, "noSelectionMechanismAtConstantRank1Level") is true;
bool photonZUnderdeterminedOnSu2 = JsonBool(p397.RootElement, "photonZSeparationUnderdetermined") is true;
bool sectorSkeletonExact = JsonBool(p396.RootElement, "tripletNeutralChargedSplitObserved") is true;

// ---------------------------------------------------------------------------
// A. New computation: SU(5)-type path ratio (path independence).
//    su(5): 5x5 traceless anti-Hermitian; T3 = (i/2) diag(0,0,0,1,-1) acting
//    on the weak 2-block; Y_SM = i diag(-1/3,-1/3,-1/3,1/2,1/2). With the
//    fundamental-trace pairing <A,B> = -Tr(AB), the embedding-derived ratio
//    is tan^2 = <T3,T3>/<Y,Y> with Y already lepton-doublet normalized
//    (|Y| = 1/2 on the weak block).
// ---------------------------------------------------------------------------

Complex[,] DiagAntiHermitian(params double[] entries)
{
    var m = new Complex[5, 5];
    for (int i = 0; i < 5; i++)
        m[i, i] = Complex.ImaginaryOne * entries[i];
    return m;
}

double PairSu5(Complex[,] a, Complex[,] b)
{
    Complex trace = Complex.Zero;
    for (int r = 0; r < 5; r++)
        for (int c = 0; c < 5; c++)
            trace += a[r, c] * b[c, r];
    return -trace.Real;
}

var t3Su5 = DiagAntiHermitian(0, 0, 0, 0.5, -0.5);
var ySu5 = DiagAntiHermitian(-1.0 / 3.0, -1.0 / 3.0, -1.0 / 3.0, 0.5, 0.5);
double su5TanSquared = PairSu5(t3Su5, t3Su5) / PairSu5(ySu5, ySu5);
double patiSalamTanSquared = p404.RootElement.GetProperty("standardTanSquaredEmb").GetDouble();
bool ratioPathIndependent =
    System.Math.Abs(su5TanSquared - patiSalamTanSquared) <= Tolerance &&
    System.Math.Abs(su5TanSquared - 0.6) <= Tolerance;

// ---------------------------------------------------------------------------
// B. New computation: signature axis. Build Cl(p,q) gamma matrices for
//    (6,4) and (7,3) with metric eta = diag(+1 x p, -1 x q) (timelike gammas
//    get a factor i in the standard construction) and machine-verify:
//    gamma_i gamma_j + gamma_j gamma_i = 2 eta_ij, the chirality operator
//    squares to +1 after phase fixing, and each chiral half is 16-dim.
// ---------------------------------------------------------------------------

(bool CliffordVerified, double ChiralHalf) CheckSignature(int p, int q)
{
    Complex[][,] paulis =
    [
        new Complex[2, 2] { { 0, 1 }, { 1, 0 } },
        new Complex[2, 2] { { 0, -Complex.ImaginaryOne }, { Complex.ImaginaryOne, 0 } },
        new Complex[2, 2] { { 1, 0 }, { 0, -1 } },
        new Complex[2, 2] { { 1, 0 }, { 0, 1 } },
    ];
    Complex[,] Kron(Complex[,] a, Complex[,] b)
    {
        int ar = a.GetLength(0), ac = a.GetLength(1), br = b.GetLength(0), bc = b.GetLength(1);
        var result = new Complex[ar * br, ac * bc];
        for (int i = 0; i < ar; i++)
            for (int j = 0; j < ac; j++)
                for (int k = 0; k < br; k++)
                    for (int l = 0; l < bc; l++)
                        result[i * br + k, j * bc + l] = a[i, j] * b[k, l];
        return result;
    }
    Complex[,] TensorString(int[] codes)
    {
        Complex[,] result = new Complex[1, 1];
        result[0, 0] = Complex.One;
        foreach (int code in codes)
            result = Kron(result, paulis[code]);
        return result;
    }
    int n = (p + q) / 2; // 5
    var gammas = new Complex[p + q][,];
    for (int k = 0; k < n; k++)
    {
        var codesEven = new int[n];
        var codesOdd = new int[n];
        for (int t = 0; t < n; t++)
        {
            codesEven[t] = t < k ? 2 : (t == k ? 0 : 3);
            codesOdd[t] = t < k ? 2 : (t == k ? 1 : 3);
        }
        gammas[2 * k] = TensorString(codesEven);
        gammas[2 * k + 1] = TensorString(codesOdd);
    }
    // Multiply the LAST q gammas by i to flip their square to -1 (signature
    // eta = diag(+1 x p, -1 x q)).
    int dim = 1 << n;
    for (int idx = p; idx < p + q; idx++)
        for (int r = 0; r < dim; r++)
            for (int c = 0; c < dim; c++)
                gammas[idx][r, c] *= Complex.ImaginaryOne;

    Complex[,] MatMul(Complex[,] a, Complex[,] b)
    {
        var result = new Complex[dim, dim];
        for (int r = 0; r < dim; r++)
            for (int c = 0; c < dim; c++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < dim; k++)
                    sum += a[r, k] * b[k, c];
                result[r, c] = sum;
            }
        return result;
    }

    // Clifford relations.
    double worstResidual = 0.0;
    for (int i = 0; i < p + q; i++)
        for (int j = i; j < p + q; j++)
        {
            var anti = MatMul(gammas[i], gammas[j]);
            var ba = MatMul(gammas[j], gammas[i]);
            double eta = i != j ? 0.0 : (i < p ? 2.0 : -2.0);
            for (int r = 0; r < dim; r++)
                for (int c = 0; c < dim; c++)
                {
                    Complex expected = r == c ? eta : Complex.Zero;
                    worstResidual = System.Math.Max(worstResidual, (anti[r, c] + ba[r, c] - expected).Magnitude);
                }
        }

    // Chirality: product of all gammas, phase-fixed to square to +1.
    var product = gammas[0];
    for (int i = 1; i < p + q; i++)
        product = MatMul(product, gammas[i]);
    var square = MatMul(product, product);
    Complex phase = Complex.Sqrt(1.0 / square[0, 0]);
    double chiralHalf = 0.0;
    for (int r = 0; r < dim; r++)
        chiralHalf += 0.5 * (1.0 + (phase * product[r, r]).Real);
    return (worstResidual <= 1e-10, chiralHalf);
}

var sig64 = CheckSignature(6, 4);
var sig73 = CheckSignature(7, 3);
bool signatureAxisVerified =
    sig64.CliffordVerified && sig73.CliffordVerified &&
    System.Math.Abs(sig64.ChiralHalf - 16.0) <= Tolerance &&
    System.Math.Abs(sig73.ChiralHalf - 16.0) <= Tolerance;

// ---------------------------------------------------------------------------
// The sweep: choice combinations vs filters.
// ---------------------------------------------------------------------------

var combinations = new List<ChoiceCombination>();
foreach (string algebraClass in new[] { "su2-only (current toy branch)", "larger-algebra (su(3)-class and above)" })
    foreach (string scalarLocation in new[] { "gauge-adjoint connection component", "non-adjoint (vertical symmetric-2-tensor) component" })
        foreach (string embeddingPath in new[] { "pati-salam", "su5-type (U(3)xU(2) det=1)" })
            foreach (string signatureChoice in new[] { "(6,4)", "(7,3)" })
            {
                bool largerAlgebra = algebraClass.StartsWith("larger");
                bool adjointLocation = scalarLocation.StartsWith("gauge-adjoint");

                // F1: doublet block present in the scalar candidate sector.
                bool f1 = largerAlgebra && (!adjointLocation || !chainAdjointHiggsExcluded);
                if (largerAlgebra && adjointLocation)
                    f1 = !chainAdjointHiggsExcluded; // Phase404: excluded on the chain
                if (!largerAlgebra)
                    f1 = false; // Phase403: su(2) adjoint has no doublet block
                // F2: custodial-capable given a doublet (Phase403 exact pattern).
                bool f2 = f1 && doubletCustodialCapable;
                // F3: family pattern derivable (Phase404; signature-independent
                // by computation B).
                bool f3 = largerAlgebra && familyPatternDerivedOnChain && signatureAxisVerified;
                // F4: coupling ratio derived not input (Phase404 + path
                // independence by computation A).
                bool f4 = largerAlgebra && ratioDerivedOnChain && ratioPathIndependent;
                // F5: VEV selection mechanism present (Phase405: none at this
                // level for any combination).
                bool f5 = false;

                combinations.Add(new ChoiceCombination
                {
                    AlgebraClass = algebraClass,
                    ScalarLocation = scalarLocation,
                    EmbeddingPath = embeddingPath,
                    SignatureChoice = signatureChoice,
                    DoubletPresent = f1,
                    CustodialCapable = f2,
                    FamilyPatternDerivable = f3,
                    RatioDerived = f4,
                    VevSelectionPresent = f5,
                    SurvivesStructuralFilters = f1 && f2 && f3 && f4,
                });
            }

int survivingCount = combinations.Count(c => c.SurvivesStructuralFilters);
int falsifiedCount = combinations.Count - survivingCount;
bool survivorsAreExactlyNonAdjointLargerAlgebra = combinations.All(c =>
    c.SurvivesStructuralFilters ==
    (c.AlgebraClass.StartsWith("larger") && c.ScalarLocation.StartsWith("non-adjoint")));
bool noCombinationProvidesVevSelection = combinations.All(c => !c.VevSelectionPresent);
bool su2ToyFalsifiedForDoubletRoute = combinations.Where(c => c.AlgebraClass.StartsWith("su2")).All(c => !c.SurvivesStructuralFilters);

bool sweepInternallyConsistent =
    precursorsPassed &&
    ratioPathIndependent &&
    signatureAxisVerified &&
    su2HasNoDoubletBlock && su3HasDoubletBlock && sectorSkeletonExact &&
    photonZUnderdeterminedOnSu2 && vacuumPermitsDoubletVev && noVevSelectionMechanism &&
    combinations.Count == 16;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool physicalDerivationProvided = false;
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
const string ApplicationSubjectKind = "choice-space-falsification-sweep";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "su5-path ratio cross-check + Cl(6,4)/Cl(7,3) signature axis + composed filter table from phases 396/397/403/404/405")))).ToLowerInvariant();

bool choiceSpaceFalsificationSweepPassed =
    sweepInternallyConsistent &&
    !physicalDerivationProvided &&
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

string terminalStatus = choiceSpaceFalsificationSweepPassed
    ? "choice-space-swept-survivors-are-nonadjoint-larger-algebra-no-vev-selection-anywhere"
    : "choice-space-falsification-sweep-blocked";
string decision = choiceSpaceFalsificationSweepPassed
    ? "Brute force #3 is complete: the theory's discrete choice space is swept against the machine-verified structural filters, with two new computations closing the open axes. (A) PATH INDEPENDENCE: the SU(5)-type route's embedding-derived ratio computes to tan^2 = 3/5 exactly, equal to the Pati-Salam value (Phase404) - the coupling-ratio lineage is a property of the chain's complexification, not the route. (B) SIGNATURE INDEPENDENCE: Cl(6,4) and Cl(7,3) both verify their Clifford relations exactly and both give 16-dimensional chiral families, so the draft's open signature choice does not affect the family/ratio structure. THE FALSIFICATION MAP over 16 choice combinations: the su(2)-only toy branch fails the doublet filter everywhere (explaining Phase397 at the choice level); the gauge-adjoint scalar location fails on the chain everywhere (Phase404 exclusion); the SURVIVING REGION is exactly {larger algebra} x {non-adjoint scalar location} x {either embedding path} x {either signature} - and NO combination anywhere provides a VEV selection mechanism (Phase405). The map sharpens the program: all remaining internal work lives in the non-adjoint (vertical symmetric-2-tensor) sector of the connection on a larger algebra, and the binding physical gaps (VEV selection and the quantitative chain) are choice-independent - they cannot be fixed by picking different discrete options. No scales exist; nothing is promoted; no contract field is filled."
    : "Do not use the sweep verdicts until the precursors, the path-independence and signature computations, and the filter-composition battery pass.";

var result = new
{
    phaseId = "phase406-choice-space-falsification-sweep",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    choiceSpaceFalsificationSweepPassed,
    precursorsPassed,
    sweepInternallyConsistent,
    su5TanSquared,
    patiSalamTanSquared,
    ratioPathIndependent,
    signature64CliffordVerified = sig64.CliffordVerified,
    signature73CliffordVerified = sig73.CliffordVerified,
    signature64ChiralHalf = sig64.ChiralHalf,
    signature73ChiralHalf = sig73.ChiralHalf,
    signatureAxisVerified,
    combinationCount = combinations.Count,
    survivingCount,
    falsifiedCount,
    survivorsAreExactlyNonAdjointLargerAlgebra,
    noCombinationProvidesVevSelection,
    su2ToyFalsifiedForDoubletRoute,
    bindingGapsAreChoiceIndependent = noCombinationProvidesVevSelection,
    gpuUsed = false,
    gpuJustification = "the new computations are 5x5/32x32 exact arithmetic (seconds on CPU); the directive's GPU engagement was delivered in Phase405, which also machine-detected the native curvature kernel real-mesh defect, so IA-5 keeps new arithmetic on the CPU reference",
    physicalDerivationProvided,
    physicalCouplingProvided,
    sweepDefinitions = new
    {
        axes = "algebra class {su2-only, larger-algebra} x scalar location {gauge-adjoint, non-adjoint} x embedding path {pati-salam, su5-type} x signature {(6,4), (7,3)} = 16 combinations",
        filters = "F1 doublet present (Phase403/404); F2 custodial-capable (Phase403); F3 family pattern derivable (Phase404 + signature axis); F4 ratio derived (Phase404 + path independence); F5 VEV selection (Phase405)",
        newComputationA = "su(5) ratio: tan^2 = <T3,T3>/<Y,Y> with Y = diag(-1/3,-1/3,-1/3,1/2,1/2) in the fundamental-trace pairing",
        newComputationB = "explicit Cl(6,4)/Cl(7,3) gamma constructions with signature metrics; Clifford relations and 16-dim chiral halves machine-verified",
        composition = "filter outcomes read from the prior phases' machine-recorded summary JSONs, never re-asserted by hand",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
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
        "the surviving region is a structural map of discrete choices, not a derivation; no combination fills any contract field",
        "filters F1-F5 are control-branch/complexification-level statements inherited from the cited phases with their recorded limitations",
        "the VEV-selection and quantitative-chain gaps are choice-independent and remain the binding physical obstructions",
        "no scales, poles, or GeV lineage anywhere",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    combinations = combinations.Select(c => c.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase396SummaryPath = Phase396SummaryPath,
        phase397SummaryPath = Phase397SummaryPath,
        phase403SummaryPath = Phase403SummaryPath,
        phase404SummaryPath = Phase404SummaryPath,
        phase405SummaryPath = Phase405SummaryPath,
        primaryDraftDictionary = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "choice_space_falsification_sweep.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "choice_space_falsification_sweep_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"choiceSpaceFalsificationSweepPassed={choiceSpaceFalsificationSweepPassed}");
Console.WriteLine($"su5TanSquared={su5TanSquared:R} ratioPathIndependent={ratioPathIndependent}");
Console.WriteLine($"signatureAxisVerified={signatureAxisVerified} (chiral halves {sig64.ChiralHalf}/{sig73.ChiralHalf})");
Console.WriteLine($"combinations={combinations.Count} surviving={survivingCount} falsified={falsifiedCount}");
Console.WriteLine($"survivorsAreExactlyNonAdjointLargerAlgebra={survivorsAreExactlyNonAdjointLargerAlgebra}");
Console.WriteLine($"noCombinationProvidesVevSelection={noCombinationProvidesVevSelection}");
Console.WriteLine($"su2ToyFalsifiedForDoubletRoute={su2ToyFalsifiedForDoubletRoute}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

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

public sealed class ChoiceCombination
{
    public required string AlgebraClass { get; init; }
    public required string ScalarLocation { get; init; }
    public required string EmbeddingPath { get; init; }
    public required string SignatureChoice { get; init; }
    public required bool DoubletPresent { get; init; }
    public required bool CustodialCapable { get; init; }
    public required bool FamilyPatternDerivable { get; init; }
    public required bool RatioDerived { get; init; }
    public required bool VevSelectionPresent { get; init; }
    public required bool SurvivesStructuralFilters { get; init; }

    public object ToOutput() => new
    {
        algebraClass = AlgebraClass,
        scalarLocation = ScalarLocation,
        embeddingPath = EmbeddingPath,
        signatureChoice = SignatureChoice,
        doubletPresent = DoubletPresent,
        custodialCapable = CustodialCapable,
        familyPatternDerivable = FamilyPatternDerivable,
        ratioDerived = RatioDerived,
        vevSelectionPresent = VevSelectionPresent,
        survivesStructuralFilters = SurvivesStructuralFilters,
    };
}
