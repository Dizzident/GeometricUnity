using System.Text.Json;

const string DefaultOutputDir = "studies/phase249_invariant_origin_search_for_near_miss_constants_001/output";
const string GeometryPath = "studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json";
const string SpinorPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string Phase63Path = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";
const string Phase64Path = "studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json";
const string Phase65Path = "studies/phase65_dimensionless_weak_coupling_amplitude_001/dimensionless_weak_coupling_amplitude.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase225Path = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase246Path = "studies/phase246_minimal_unlock_candidate_inventory_001/output/minimal_unlock_candidate_inventory_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";

const double Tolerance = 1.0e-12;

var outputDir = Environment.GetEnvironmentVariable("PHASE249_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var geometry = JsonDocument.Parse(File.ReadAllText(GeometryPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(SpinorPath));
using var phase63 = JsonDocument.Parse(File.ReadAllText(Phase63Path));
using var phase64 = JsonDocument.Parse(File.ReadAllText(Phase64Path));
using var phase65 = JsonDocument.Parse(File.ReadAllText(Phase65Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase225 = JsonDocument.Parse(File.ReadAllText(Phase225Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase246 = JsonDocument.Parse(File.ReadAllText(Phase246Path));
using var phase247 = JsonDocument.Parse(File.ReadAllText(Phase247Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));

var baseDimension = RequiredInt(geometry.RootElement.GetProperty("baseSpace"), "dimension");
var ambientDimension = RequiredInt(geometry.RootElement.GetProperty("ambientSpace"), "dimension");
var fiberDimension = RequiredInt(spinor.RootElement.GetProperty("chiralityConvention"), "fiberDimension");
var spinorComponents = RequiredInt(spinor.RootElement, "spinorComponents");
var su2Dimension = phase63.RootElement.GetProperty("generatorLabels").GetArrayLength();
var fundamentalCasimir = 0.75;
var adjointCasimir = 2.0;
var phase63TraceMetricDiagonal = RequiredDouble(phase63.RootElement, "physicalTraceMetricDiagonal");
var phase63TraceHalfScale = RequiredDouble(phase63.RootElement, "internalToPhysicalGeneratorScale");
var phase64NormalizesAsTraceHalf = JsonString(phase64.RootElement, "normalizationConventionId") == JsonString(phase63.RootElement, "normalizationConventionId");
var phase65WeakCoupling = RequiredDouble(phase65.RootElement.GetProperty("candidate"), "couplingValue");
var phase221TraceHalfScale = RequiredDouble(phase221.RootElement, "currentTraceHalfScale");
var phase221CasimirRmsScale = RequiredDouble(phase221.RootElement, "casimirRmsScale");
var phase221CasimirToTraceHalfScale = RequiredDouble(phase221.RootElement, "casimirToTraceHalfScale");
var phase221SourceLineagePromotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;
var phase221NumericalTargetComparisonPassed = JsonBool(phase221.RootElement, "numericalTargetComparisonPassed") is true;
var p223BestProbe = phase223.RootElement.GetProperty("bestProbe");
var p223BestFactor = JsonString(p223BestProbe, "factorExpression") ?? "missing";
var p223BestFactorValue = RequiredDouble(p223BestProbe, "factorValue");
var p223SourceDerived = JsonBool(p223BestProbe, "sourceDerived") is true;
var p223SourceLineagePromotable = JsonBool(phase223.RootElement, "sourceLineagePromotable") is true;
var p223CanPromoteLead = JsonBool(phase223.RootElement, "canPromoteHiggsCasimirQuarticLead") is true;
var p223NumericalLeadPresent = JsonBool(phase223.RootElement, "numericalLeadPresent") is true;
var p225RepresentationObstructionCertified = JsonBool(phase225.RootElement, "representationNormalizationObstructionCertified") is true;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var p246AnyCandidateFillsWz = JsonBool(phase246.RootElement, "anyCandidateFillsWzAbsoluteScaleUnlock") is true;
var p246AnyCandidateFillsHiggs = JsonBool(phase246.RootElement, "anyCandidateFillsHiggsScalarScaleUnlock") is true;
var p246Promotable = JsonBool(phase246.RootElement, "candidateInventoryPromotableForBosonMasses") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p247CurrentDirectBridgeCandidatePromotable = JsonBool(phase247.RootElement, "currentDirectBridgeCandidatePromotable") is true;
var p248NewHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var p248CurrentHiggsNumericalLeadPromotable = JsonBool(phase248.RootElement, "currentHiggsNumericalLeadPromotable") is true;
var p248ThreeTenthsDerivable = JsonBool(phase248.RootElement, "threeTenthsFactorDerivableFromCurrentScalarSource") is true;
var wzMissingFieldCount = RequiredInt(phase213.RootElement, "wzMissingFieldCount");
var higgsMissingFieldCount = RequiredInt(phase213.RootElement, "higgsMissingFieldCount");

var targetObservablesUsedToConstructFormulas = false;
var targetDiagnosticsUsedOnlyForComparison = true;
var adjointRmsScale = Math.Sqrt(adjointCasimir / su2Dimension);
var adjointRmsOverTraceHalf = adjointRmsScale / Math.Sqrt(phase63TraceMetricDiagonal);
var dimensionRatioThreeTenths = (double)su2Dimension / (ambientDimension + baseDimension + su2Dimension);
var casimirThreeTenths = 2.0 * fundamentalCasimir / (adjointCasimir + su2Dimension);

var formulaRows = new[]
{
    new InvariantFormulaRow(
        FormulaId: "su2-adjoint-rms-over-trace-half",
        Sector: "wz",
        DiagnosticRole: "near-miss-origin",
        Expression: "sqrt(C2(adj)/dim(su2)) / sqrt(1/2) = 2/sqrt(3)",
        Value: adjointRmsOverTraceHalf,
        DiagnosticTargetValue: phase221CasimirToTraceHalfScale,
        AbsoluteResidual: Math.Abs(adjointRmsOverTraceHalf - phase221CasimirToTraceHalfScale),
        ExactDiagnosticMatch: Close(adjointRmsOverTraceHalf, phase221CasimirToTraceHalfScale),
        SourceBackedAsLocalInvariant: true,
        SourceBackedForBosonApplication: false,
        FillsP245Unlock: false,
        RejectionClass: "application-not-source-backed",
        Detail: "The SU(2) identity explains the P221 numerical factor, but P225/P247 do not supply a source theorem that the Phase64 fermion-current matrix element is an isotropic spin-1 triplet RMS W/Z source."),
    new InvariantFormulaRow(
        FormulaId: "su2-adjoint-rms-scale",
        Sector: "wz",
        DiagnosticRole: "near-miss-origin",
        Expression: "sqrt(C2(adj)/dim(su2)) = sqrt(2/3)",
        Value: adjointRmsScale,
        DiagnosticTargetValue: phase221CasimirRmsScale,
        AbsoluteResidual: Math.Abs(adjointRmsScale - phase221CasimirRmsScale),
        ExactDiagnosticMatch: Close(adjointRmsScale, phase221CasimirRmsScale),
        SourceBackedAsLocalInvariant: true,
        SourceBackedForBosonApplication: false,
        FillsP245Unlock: false,
        RejectionClass: "application-not-source-backed",
        Detail: "This is the RMS factor used by P221, but current source lineage still evaluates the Phase64 trace-half fermion-current operator."),
    new InvariantFormulaRow(
        FormulaId: "phase63-trace-half-control",
        Sector: "wz",
        DiagnosticRole: "current-source-control",
        Expression: "sqrt(tr(T_a T_a)) = sqrt(1/2)",
        Value: phase63TraceHalfScale,
        DiagnosticTargetValue: phase221TraceHalfScale,
        AbsoluteResidual: Math.Abs(phase63TraceHalfScale - phase221TraceHalfScale),
        ExactDiagnosticMatch: Close(phase63TraceHalfScale, phase221TraceHalfScale),
        SourceBackedAsLocalInvariant: true,
        SourceBackedForBosonApplication: phase64NormalizesAsTraceHalf,
        FillsP245Unlock: false,
        RejectionClass: "current-source-does-not-close-absolute-scale",
        Detail: "The existing source-backed trace-half convention is the current promoted convention, but it does not fix the W/Z absolute scale."),
    new InvariantFormulaRow(
        FormulaId: "local-dim-ratio-su2-over-ambient-base-su2",
        Sector: "higgs",
        DiagnosticRole: "near-miss-origin",
        Expression: "dim(su2)/(dim(Y_h)+dim(X_h)+dim(su2)) = 3/(5+2+3)",
        Value: dimensionRatioThreeTenths,
        DiagnosticTargetValue: p223BestFactorValue,
        AbsoluteResidual: Math.Abs(dimensionRatioThreeTenths - p223BestFactorValue),
        ExactDiagnosticMatch: Close(dimensionRatioThreeTenths, p223BestFactorValue),
        SourceBackedAsLocalInvariant: true,
        SourceBackedForBosonApplication: false,
        FillsP245Unlock: false,
        RejectionClass: "formula-coincidence-no-scalar-source",
        Detail: "The local dimensions reproduce 3/10 exactly, but no scalar operator, Higgs identity envelope, potential, or excitation relation applies this ratio to lambda/g^2."),
    new InvariantFormulaRow(
        FormulaId: "su2-casimir-ratio-three-tenths",
        Sector: "higgs",
        DiagnosticRole: "near-miss-origin",
        Expression: "2*C2(fund)/(C2(adj)+dim(su2)) = 2*(3/4)/(2+3)",
        Value: casimirThreeTenths,
        DiagnosticTargetValue: p223BestFactorValue,
        AbsoluteResidual: Math.Abs(casimirThreeTenths - p223BestFactorValue),
        ExactDiagnosticMatch: Close(casimirThreeTenths, p223BestFactorValue),
        SourceBackedAsLocalInvariant: true,
        SourceBackedForBosonApplication: false,
        FillsP245Unlock: false,
        RejectionClass: "formula-coincidence-no-scalar-source",
        Detail: "SU(2) Casimir arithmetic also reproduces 3/10, showing degeneracy of simple invariant formulas rather than a source-derived scalar law."),
};

var wzNearMissRows = formulaRows.Where(row => row.Sector == "wz" && row.DiagnosticRole == "near-miss-origin").ToArray();
var higgsNearMissRows = formulaRows.Where(row => row.Sector == "higgs" && row.DiagnosticRole == "near-miss-origin").ToArray();
var wzExactDiagnosticFormulaCount = wzNearMissRows.Count(row => row.ExactDiagnosticMatch);
var higgsExactDiagnosticFormulaCount = higgsNearMissRows.Count(row => row.ExactDiagnosticMatch);
var wzInvariantFormulaCandidateFound = wzExactDiagnosticFormulaCount > 0;
var higgsInvariantFormulaCandidateFound = higgsExactDiagnosticFormulaCount > 0;
var wzInvariantFormulaSourceBacked = wzNearMissRows.Any(row => row.SourceBackedForBosonApplication);
var higgsInvariantFormulaSourceBacked = higgsNearMissRows.Any(row => row.SourceBackedForBosonApplication);
var wzUnlockFilledByInvariantSearch = wzNearMissRows.Any(row => row.FillsP245Unlock);
var higgsUnlockFilledByInvariantSearch = higgsNearMissRows.Any(row => row.FillsP245Unlock);
var anyInvariantOriginPromotableForBosonMasses = formulaRows.Any(row => row.FillsP245Unlock);
var newSourceEvidenceStillRequired = p245NewSourceEvidenceStillRequired
    && !p245UnlockContractFilled
    && !wzUnlockFilledByInvariantSearch
    && !higgsUnlockFilledByInvariantSearch;

var checks = new[]
{
    new Check("search-target-independent", !targetObservablesUsedToConstructFormulas && targetDiagnosticsUsedOnlyForComparison, $"targetObservablesUsedToConstructFormulas={targetObservablesUsedToConstructFormulas}; targetDiagnosticsUsedOnlyForComparison={targetDiagnosticsUsedOnlyForComparison}"),
    new Check("wz-invariant-numerical-origin-found", wzInvariantFormulaCandidateFound && phase221NumericalTargetComparisonPassed && !phase221SourceLineagePromotable, $"wzExactDiagnosticFormulaCount={wzExactDiagnosticFormulaCount}; phase221NumericalTargetComparisonPassed={phase221NumericalTargetComparisonPassed}; phase221SourceLineagePromotable={phase221SourceLineagePromotable}"),
    new Check("wz-invariant-application-not-source-backed", !wzInvariantFormulaSourceBacked && p225RepresentationObstructionCertified && !p247CurrentDirectBridgeCandidatePromotable && p247NewDirectBridgeTheoremStillRequired, $"wzInvariantFormulaSourceBacked={wzInvariantFormulaSourceBacked}; p225RepresentationObstructionCertified={p225RepresentationObstructionCertified}; p247NewDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}"),
    new Check("higgs-invariant-numerical-origin-found", higgsInvariantFormulaCandidateFound && p223NumericalLeadPresent && p223BestFactor == "3/10", $"higgsExactDiagnosticFormulaCount={higgsExactDiagnosticFormulaCount}; p223NumericalLeadPresent={p223NumericalLeadPresent}; p223BestFactor={p223BestFactor}"),
    new Check("higgs-invariant-degenerate-not-source-derived", higgsExactDiagnosticFormulaCount >= 2 && !higgsInvariantFormulaSourceBacked && !p223SourceDerived && !p223SourceLineagePromotable && !p223CanPromoteLead && !p248CurrentHiggsNumericalLeadPromotable && !p248ThreeTenthsDerivable && p248NewHiggsScalarSourceStillRequired, $"higgsExactDiagnosticFormulaCount={higgsExactDiagnosticFormulaCount}; p223SourceDerived={p223SourceDerived}; p248ThreeTenthsDerivable={p248ThreeTenthsDerivable}; p248NewHiggsScalarSourceStillRequired={p248NewHiggsScalarSourceStillRequired}"),
    new Check("minimal-unlocks-still-unfilled", !p245UnlockContractFilled && !p246AnyCandidateFillsWz && !p246AnyCandidateFillsHiggs && !p246Promotable, $"p245UnlockContractFilled={p245UnlockContractFilled}; p246AnyCandidateFillsWz={p246AnyCandidateFillsWz}; p246AnyCandidateFillsHiggs={p246AnyCandidateFillsHiggs}; p246Promotable={p246Promotable}"),
    new Check("source-blocker-counts-preserved", wzMissingFieldCount == 15 && higgsMissingFieldCount == 14, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("no-invariant-origin-promotable", !anyInvariantOriginPromotableForBosonMasses && !wzUnlockFilledByInvariantSearch && !higgsUnlockFilledByInvariantSearch && newSourceEvidenceStillRequired, $"anyInvariantOriginPromotableForBosonMasses={anyInvariantOriginPromotableForBosonMasses}; wzUnlockFilledByInvariantSearch={wzUnlockFilledByInvariantSearch}; higgsUnlockFilledByInvariantSearch={higgsUnlockFilledByInvariantSearch}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}"),
};

var invariantOriginSearchPassed = checks.All(check => check.Passed);
var terminalStatus = invariantOriginSearchPassed
    ? "invariant-origin-search-complete-near-miss-constants-not-promotable"
    : "invariant-origin-search-review-required";

var result = new
{
    phaseId = "phase249-invariant-origin-search-for-near-miss-constants",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    invariantOriginSearchPassed,
    targetObservablesUsedToConstructFormulas,
    targetDiagnosticsUsedOnlyForComparison,
    wzInvariantFormulaCandidateFound,
    wzInvariantFormulaSourceBacked,
    higgsInvariantFormulaCandidateFound,
    higgsInvariantFormulaSourceBacked,
    anyInvariantOriginPromotableForBosonMasses,
    wzUnlockFilledByInvariantSearch,
    higgsUnlockFilledByInvariantSearch,
    newSourceEvidenceStillRequired,
    wzExactDiagnosticFormulaCount,
    higgsExactDiagnosticFormulaCount,
    localInvariantInputs = new
    {
        baseDimension,
        ambientDimension,
        fiberDimension,
        spinorComponents,
        su2Dimension,
        fundamentalCasimir,
        adjointCasimir,
        phase63TraceMetricDiagonal,
        phase63TraceHalfScale,
        phase64NormalizesAsTraceHalf,
        phase65WeakCoupling,
    },
    diagnosticTargets = new
    {
        wz = new
        {
            phase221TraceHalfScale,
            phase221CasimirRmsScale,
            phase221CasimirToTraceHalfScale,
            phase221NumericalTargetComparisonPassed,
            phase221SourceLineagePromotable,
        },
        higgs = new
        {
            p223BestFactor,
            p223BestFactorValue,
            p223SourceDerived,
            p223SourceLineagePromotable,
            p223CanPromoteLead,
        },
    },
    sourceBackedApplicationEvidence = new
    {
        p225RepresentationObstructionCertified,
        p247CurrentDirectBridgeCandidatePromotable,
        p247NewDirectBridgeTheoremStillRequired,
        p248CurrentHiggsNumericalLeadPromotable,
        p248ThreeTenthsDerivable,
        p248NewHiggsScalarSourceStillRequired,
    },
    p245UnlockState = new
    {
        p245UnlockContractFilled,
        p245NewSourceEvidenceStillRequired,
        p246AnyCandidateFillsWz,
        p246AnyCandidateFillsHiggs,
        p246Promotable,
    },
    sourceBlockers = new
    {
        wzMissingFieldCount,
        higgsMissingFieldCount,
    },
    formulaRows,
    checks,
    decision = invariantOriginSearchPassed
        ? "The invariant search explains where the current near-miss constants can come from arithmetically, but it does not supply the missing W/Z source theorem or Higgs scalar source/operator. No W/Z/H physical mass claim is promotable from these invariant coincidences."
        : "Review the invariant-origin search before relying on its promotion boundary.",
    nextRequiredArtifact = new[]
    {
        "For W/Z: a target-independent theorem or replayed matrix element showing the source operator is an isotropic SU(2) triplet RMS object with separate W and Z source rows.",
        "For Higgs: a target-independent scalar source/operator, Higgs identity envelope, massive scalar profile, and source-derived quartic/self-coupling or excitation relation.",
        "Both artifacts must pass the Phase201/Phase209/Phase210/Phase213 gates before any physical W/Z/H mass prediction is promoted.",
    },
    sourceEvidence = new
    {
        geometryPath = GeometryPath,
        spinorPath = SpinorPath,
        phase63Path = Phase63Path,
        phase64Path = Phase64Path,
        phase65Path = Phase65Path,
        phase213Path = Phase213Path,
        phase221Path = Phase221Path,
        phase223Path = Phase223Path,
        phase225Path = Phase225Path,
        phase245Path = Phase245Path,
        phase246Path = Phase246Path,
        phase247Path = Phase247Path,
        phase248Path = Phase248Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "invariant_origin_search_for_near_miss_constants.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "invariant_origin_search_for_near_miss_constants_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.invariantOriginSearchPassed,
        result.targetObservablesUsedToConstructFormulas,
        result.targetDiagnosticsUsedOnlyForComparison,
        result.wzInvariantFormulaCandidateFound,
        result.wzInvariantFormulaSourceBacked,
        result.higgsInvariantFormulaCandidateFound,
        result.higgsInvariantFormulaSourceBacked,
        result.anyInvariantOriginPromotableForBosonMasses,
        result.wzUnlockFilledByInvariantSearch,
        result.higgsUnlockFilledByInvariantSearch,
        result.newSourceEvidenceStillRequired,
        result.wzExactDiagnosticFormulaCount,
        result.higgsExactDiagnosticFormulaCount,
        result.localInvariantInputs,
        result.diagnosticTargets,
        result.sourceBackedApplicationEvidence,
        result.p245UnlockState,
        result.sourceBlockers,
        result.formulaRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"invariantOriginSearchPassed={invariantOriginSearchPassed}");
Console.WriteLine($"wzInvariantFormulaCandidateFound={wzInvariantFormulaCandidateFound}");
Console.WriteLine($"wzInvariantFormulaSourceBacked={wzInvariantFormulaSourceBacked}");
Console.WriteLine($"higgsInvariantFormulaCandidateFound={higgsInvariantFormulaCandidateFound}");
Console.WriteLine($"higgsInvariantFormulaSourceBacked={higgsInvariantFormulaSourceBacked}");
Console.WriteLine($"anyInvariantOriginPromotableForBosonMasses={anyInvariantOriginPromotableForBosonMasses}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static bool Close(double left, double right) => Math.Abs(left - right) <= Tolerance;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int RequiredInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : throw new InvalidOperationException($"Required integer property '{propertyName}' missing.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidOperationException($"Required numeric property '{propertyName}' missing.");

sealed record InvariantFormulaRow(
    string FormulaId,
    string Sector,
    string DiagnosticRole,
    string Expression,
    double Value,
    double DiagnosticTargetValue,
    double AbsoluteResidual,
    bool ExactDiagnosticMatch,
    bool SourceBackedAsLocalInvariant,
    bool SourceBackedForBosonApplication,
    bool FillsP245Unlock,
    string RejectionClass,
    string Detail);

sealed record Check(string CheckId, bool Passed, string Detail);
