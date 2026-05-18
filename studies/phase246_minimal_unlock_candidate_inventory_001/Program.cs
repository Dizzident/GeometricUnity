using System.Text.Json;

const string DefaultOutputDir = "studies/phase246_minimal_unlock_candidate_inventory_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase234Path = "studies/phase234_cox_ii_electroweak_formula_dependency_audit_001/output/cox_ii_electroweak_formula_dependency_audit_summary.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase238Path = "studies/phase238_cox_ii_ready_to_fit_formula_dependency_audit_001/output/cox_ii_ready_to_fit_formula_dependency_audit_summary.json";
const string Phase241Path = "studies/phase241_cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit_001/output/cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE246_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase234 = JsonDocument.Parse(File.ReadAllText(Phase234Path));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase238 = JsonDocument.Parse(File.ReadAllText(Phase238Path));
using var phase241 = JsonDocument.Parse(File.ReadAllText(Phase241Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));

var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var currentPromotedConstraintRank = JsonInt(phase245.RootElement, "currentPromotedConstraintRank") ?? -1;
var remainingNullity = JsonInt(phase245.RootElement, "remainingNullity") ?? -1;
var minimumAdditionalIndependentSourceConstraints = JsonInt(phase245.RootElement, "minimumAdditionalIndependentSourceConstraints") ?? -1;
var p245Passed = JsonBool(phase245.RootElement, "rankDeficitMinimalUnlockContractPassed") is true;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;

var p221WComparison = FindComparison(phase221.RootElement, "physical-w-boson-mass-gev");
var p221ZComparison = FindComparison(phase221.RootElement, "physical-z-boson-mass-gev");
var p221NumericalPass = JsonBool(phase221.RootElement, "numericalTargetComparisonPassed") is true;
var p221Promotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;
var p221WValue = JsonDouble(p221WComparison, "casimirRescaledPredictedValue");
var p221ZValue = JsonDouble(p221ZComparison, "casimirRescaledPredictedValue");
var p221WeakCoupling = JsonDouble(phase221.RootElement, "casimirWeakCoupling");

var p223NumericalLeadPresent = JsonBool(phase223.RootElement, "numericalLeadPresent") is true;
var p223Promotable = JsonBool(phase223.RootElement, "sourceLineagePromotable") is true;
var p223CanPromoteLead = JsonBool(phase223.RootElement, "canPromoteHiggsCasimirQuarticLead") is true;
var p223BestProbe = phase223.RootElement.GetProperty("bestProbe");
var p223FactorExpression = JsonString(p223BestProbe, "factorExpression") ?? "unknown";
var p223Quartic = JsonDouble(p223BestProbe, "quarticFromCasimirG2");
var p223HiggsMass = JsonDouble(p223BestProbe, "replayedHiggsMassGeV");

var p229ExternalVev = JsonDouble(phase229.RootElement, "externalVevGeV");
var p229VevPromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var p234FormulaPromotable = JsonBool(phase234.RootElement, "symbolicFormulaLeadPromotableForAbsoluteMasses") is true;
var p237HiggsTexturePromotable = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true;
var p238ReadyToFitPromotable = JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaPromotableForBosonMasses") is true;
var p241QuarticGaugeProvidesHiggsLambda = JsonBool(phase241.RootElement, "quarticGaugeSignFalsifierProvidesHiggsQuarticLambda") is true;
var p241QuarticGaugePromotable = JsonBool(phase241.RootElement, "coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses") is true;
var p215TargetImpliedQuartic = phase215.RootElement.TryGetProperty("targetImpliedDiagnostic", out var targetDiagnostic)
    ? JsonDouble(targetDiagnostic, "targetImpliedQuartic")
    : null;
var p224Closure = phase224.RootElement.GetProperty("closure");
var p224AnyClosure = JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true
    || JsonBool(p224Closure, "zAbsoluteMassParameterClosure") is true
    || JsonBool(p224Closure, "higgsMassParameterClosure") is true;

var wzCandidateRows = new[]
{
    new CandidateRow(
        "su2-casimir-rms-normalization",
        "wz-absolute-scale-unlock",
        "phase221-su2-casimir-wz-normalization-probe",
        "numerical-diagnostic",
        Promotable: false,
        RejectionClass: "nonpromotable-diagnostic",
        BestNumericalValue: $"MW={FormatMaybe(p221WValue)} GeV; MZ={FormatMaybe(p221ZValue)} GeV; g={FormatMaybe(p221WeakCoupling)}",
        RequirementFailures: new[]
        {
            "no upstream GU source derives replacing trace-half single-generator normalization with isotropic SU(2) triplet RMS normalization",
            "Phase64 matrix element is not proven to be an SU(2)-triplet RMS amplitude",
            "rawAmplitudeGatePassed remains unfilled for both W and Z",
            "sourceLineagePromotable=false",
        },
        WhyDoesNotSatisfyP245: "Numerically close W/Z absolute masses do not fill the P245 common W/Z absolute-scale source row because the normalization is not source-derived."),
    new CandidateRow(
        "external-fermi-vev-bridge",
        "wz-absolute-scale-unlock",
        "phase229-electroweak-vev-source-lineage-obstruction-audit",
        "external-input",
        Promotable: false,
        RejectionClass: "external-input",
        BestNumericalValue: $"v={FormatMaybe(p229ExternalVev)} GeV",
        RequirementFailures: new[]
        {
            "VEV is Fermi-derived external comparison context",
            "no GU vacuum solution source id",
            "no GU VEV source derivation",
            "externalTargetValuesUsed=false is not satisfied as a GU source-lineage row",
        },
        WhyDoesNotSatisfyP245: "The P245 W/Z unlock accepts a GU source for log(v g) or independent GU rows for v and g; an imported electroweak VEV is not that source."),
    new CandidateRow(
        "cox-ii-symbolic-electroweak-formula",
        "wz-absolute-scale-unlock",
        "phase234-cox-ii-electroweak-formula-dependency-audit",
        "symbolic-formula-lead",
        Promotable: false,
        RejectionClass: "parameterized-not-prediction",
        BestNumericalValue: "mW^2=gL^2*kappa^2/4; mZ^2=(gL^2+gY^2)*kappa^2/4",
        RequirementFailures: new[]
        {
            "no promotable GU weak-coupling source",
            "no promotable GU hypercharge or equivalent mixing source",
            "no promotable kappa or electroweak VEV source",
        },
        WhyDoesNotSatisfyP245: "The symbolic bridge has the right dependency shape but leaves the rank-deficit coordinates free."),
    new CandidateRow(
        "target-w-or-z-mass-inversion",
        "wz-absolute-scale-unlock",
        "phase224-electroweak-parameter-dependency-audit",
        "target-inversion",
        Promotable: false,
        RejectionClass: "target-leakage",
        BestNumericalValue: "target-implied electroweak parameters reproduce W/Z by construction",
        RequirementFailures: new[]
        {
            "uses observed W/Z masses to solve source parameters",
            "not target-independent",
            "does not create theoremOrDerivationId or W/Z sourceRowId rows",
        },
        WhyDoesNotSatisfyP245: "P245 requires source rows before target comparison; target inversion reverses that order."),
    new CandidateRow(
        "w-z-ratio-only",
        "wz-absolute-scale-unlock",
        "phase245-rank-deficit-minimal-unlock-contract",
        "rank-deficient-constraint",
        Promotable: false,
        RejectionClass: "rank-deficient",
        BestNumericalValue: "dimensionless W/Z ratio only",
        RequirementFailures: new[]
        {
            "fixes weak-angle-like ratio but not common W/Z scale",
            "leaves log(v g) null direction free",
        },
        WhyDoesNotSatisfyP245: "The W/Z ratio is already one promoted constraint; P245 shows two additional independent source constraints are still required."),
};

var higgsCandidateRows = new[]
{
    new CandidateRow(
        "three-tenths-casimir-quartic",
        "higgs-scalar-scale-unlock",
        "phase223-higgs-casimir-quartic-numerical-probe",
        "numerical-diagnostic",
        Promotable: false,
        RejectionClass: "nonpromotable-diagnostic",
        BestNumericalValue: $"factor={p223FactorExpression}; lambda={FormatMaybe(p223Quartic)}; replayedHiggsMass={FormatMaybe(p223HiggsMass)} GeV",
        RequirementFailures: new[]
        {
            "no solved scalar source/operator derives the factor",
            "no Higgs potential or self-coupling source id",
            "no massive scalar profile",
            "sourceLineagePromotable=false",
        },
        WhyDoesNotSatisfyP245: "A close factor for lambda/g^2 does not fill the Higgs scalar-scale source row without scalar-source lineage."),
    new CandidateRow(
        "external-fermi-vev-as-scalar-order-parameter",
        "higgs-scalar-scale-unlock",
        "phase229-electroweak-vev-source-lineage-obstruction-audit",
        "external-input",
        Promotable: false,
        RejectionClass: "external-input",
        BestNumericalValue: $"v={FormatMaybe(p229ExternalVev)} GeV",
        RequirementFailures: new[]
        {
            "VEV source is external/Fermi-derived",
            "no GU vacuum solution source id",
            "no scalar self-coupling source",
            "no Higgs prediction row",
        },
        WhyDoesNotSatisfyP245: "Even if v is imported, the Higgs unlock still needs a target-independent source for lambda or log(v sqrt(lambda))."),
    new CandidateRow(
        "higgs-target-implied-quartic",
        "higgs-scalar-scale-unlock",
        "phase215-higgs-target-implied-self-coupling-loophole-audit",
        "target-inversion",
        Promotable: false,
        RejectionClass: "target-leakage",
        BestNumericalValue: $"target-implied lambda={FormatMaybe(p215TargetImpliedQuartic)}",
        RequirementFailures: new[]
        {
            "uses observed Higgs mass to solve lambda",
            "not target-independent",
            "no scalarSourceOperatorId",
            "no potentialOrSelfCouplingSourceId or excitationRelationId",
        },
        WhyDoesNotSatisfyP245: "P245 requires source construction before comparison; target-implied lambda is the comparison target folded back into the source."),
    new CandidateRow(
        "quartic-gauge-sign-falsifier",
        "higgs-scalar-scale-unlock",
        "phase241-cox-iv-quartic-gauge-sign-falsifier-boson-mass-audit",
        "scope-mismatch",
        Promotable: false,
        RejectionClass: "category-error",
        BestNumericalValue: "sign constraint for anomalous quartic gauge coupling, not Higgs lambda",
        RequirementFailures: new[]
        {
            "does not provide Higgs quartic lambda",
            "does not provide scalar source operator",
            "does not provide VEV or physical Higgs mass row",
        },
        WhyDoesNotSatisfyP245: "A quartic gauge-coupling sign falsifier is not a Higgs scalar self-coupling source."),
    new CandidateRow(
        "cox-ii-higgs-yukawa-texture",
        "higgs-scalar-scale-unlock",
        "phase237-cox-ii-higgs-yukawa-texture-dependency-audit",
        "external-model-building-lead",
        Promotable: false,
        RejectionClass: "missing-scalar-operator-and-quartic-source",
        BestNumericalValue: "Higgs-like moduli and schematic Yukawa overlaps; no mass value",
        RequirementFailures: new[]
        {
            "no solved scalar-source operator",
            "no Higgs identity envelope",
            "no massive scalar profile",
            "no potential or self-coupling source",
            "no stability sidecars or prediction row",
        },
        WhyDoesNotSatisfyP245: "The lead is useful scalar-sector context but does not fix log(v sqrt(lambda)) or equivalent GU rows for v and lambda."),
    new CandidateRow(
        "cox-ii-ready-to-fit-formula",
        "higgs-scalar-scale-unlock",
        "phase238-cox-ii-ready-to-fit-formula-dependency-audit",
        "parameterized-formula-lead",
        Promotable: false,
        RejectionClass: "parameterized-not-prediction",
        BestNumericalValue: "ready-to-fit formula structure; free parameters remain",
        RequirementFailures: new[]
        {
            "free scalar-sector parameters remain",
            "no target-independent lambda source",
            "no post-construction Higgs prediction row",
        },
        WhyDoesNotSatisfyP245: "Parameterized formulas can organize a future prediction but do not fill the independent Higgs scalar-scale source constraint."),
};

var allCandidateRows = wzCandidateRows.Concat(higgsCandidateRows).ToArray();
var anyCandidateFillsWzAbsoluteScaleUnlock = wzCandidateRows.Any(row => row.Promotable);
var anyCandidateFillsHiggsScalarScaleUnlock = higgsCandidateRows.Any(row => row.Promotable);
var candidateInventoryPromotableForBosonMasses = anyCandidateFillsWzAbsoluteScaleUnlock || anyCandidateFillsHiggsScalarScaleUnlock;
var newSourceEvidenceStillRequired = p245NewSourceEvidenceStillRequired
    && !p245UnlockContractFilled
    && !candidateInventoryPromotableForBosonMasses;

var checks = new[]
{
    new Check("phase245-unlock-contract-active", p245Passed && !p245UnlockContractFilled && p245NewSourceEvidenceStillRequired, $"p245Passed={p245Passed}; unlockContractFilled={p245UnlockContractFilled}; newSourceEvidenceStillRequired={p245NewSourceEvidenceStillRequired}"),
    new Check("rank-deficit-inherited", currentPromotedConstraintRank == 1 && remainingNullity == 2 && minimumAdditionalIndependentSourceConstraints == 2, $"currentPromotedConstraintRank={currentPromotedConstraintRank}; remainingNullity={remainingNullity}; minimumAdditionalIndependentSourceConstraints={minimumAdditionalIndependentSourceConstraints}"),
    new Check("phase213-blockers-preserved", phase213WzMissingFieldCount == 15 && phase213HiggsMissingFieldCount == 14, $"wzMissingFieldCount={phase213WzMissingFieldCount}; higgsMissingFieldCount={phase213HiggsMissingFieldCount}"),
    new Check("wz-candidate-inventory-covered", wzCandidateRows.Length >= 5 && wzCandidateRows.Any(row => row.CandidateId == "su2-casimir-rms-normalization") && wzCandidateRows.Any(row => row.CandidateId == "external-fermi-vev-bridge") && wzCandidateRows.Any(row => row.CandidateId == "cox-ii-symbolic-electroweak-formula"), $"wzCandidateCount={wzCandidateRows.Length}"),
    new Check("higgs-candidate-inventory-covered", higgsCandidateRows.Length >= 6 && higgsCandidateRows.Any(row => row.CandidateId == "three-tenths-casimir-quartic") && higgsCandidateRows.Any(row => row.CandidateId == "higgs-target-implied-quartic") && higgsCandidateRows.Any(row => row.CandidateId == "quartic-gauge-sign-falsifier"), $"higgsCandidateCount={higgsCandidateRows.Length}"),
    new Check("phase221-diagnostic-lead-recorded-not-promoted", p221NumericalPass && !p221Promotable && wzCandidateRows.Any(row => row.CandidateId == "su2-casimir-rms-normalization" && row.RejectionClass == "nonpromotable-diagnostic"), $"p221NumericalPass={p221NumericalPass}; p221Promotable={p221Promotable}; MW={FormatMaybe(p221WValue)}; MZ={FormatMaybe(p221ZValue)}"),
    new Check("phase223-diagnostic-lead-recorded-not-promoted", p223NumericalLeadPresent && !p223Promotable && !p223CanPromoteLead && p223FactorExpression == "3/10" && higgsCandidateRows.Any(row => row.CandidateId == "three-tenths-casimir-quartic" && row.RejectionClass == "nonpromotable-diagnostic"), $"p223NumericalLeadPresent={p223NumericalLeadPresent}; p223Promotable={p223Promotable}; p223CanPromoteLead={p223CanPromoteLead}; factor={p223FactorExpression}"),
    new Check("external-and-symbolic-leads-remain-nonpromotable", !p229VevPromotable && !p234FormulaPromotable && !p237HiggsTexturePromotable && !p238ReadyToFitPromotable, $"p229VevPromotable={p229VevPromotable}; p234FormulaPromotable={p234FormulaPromotable}; p237HiggsTexturePromotable={p237HiggsTexturePromotable}; p238ReadyToFitPromotable={p238ReadyToFitPromotable}"),
    new Check("target-and-category-loopholes-remain-closed", !p224AnyClosure && !p241QuarticGaugeProvidesHiggsLambda && !p241QuarticGaugePromotable, $"p224AnyClosure={p224AnyClosure}; p241ProvidesHiggsLambda={p241QuarticGaugeProvidesHiggsLambda}; p241Promotable={p241QuarticGaugePromotable}"),
    new Check("no-candidate-marked-promotable", allCandidateRows.All(row => !row.Promotable) && !candidateInventoryPromotableForBosonMasses, $"promotableCandidateCount={allCandidateRows.Count(row => row.Promotable)}"),
    new Check("new-source-evidence-still-required", newSourceEvidenceStillRequired, $"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}"),
};

var minimalUnlockCandidateInventoryPassed = checks.All(check => check.Passed)
    && !anyCandidateFillsWzAbsoluteScaleUnlock
    && !anyCandidateFillsHiggsScalarScaleUnlock
    && !candidateInventoryPromotableForBosonMasses;

var terminalStatus = minimalUnlockCandidateInventoryPassed
    ? "minimal-unlock-candidate-inventory-complete-no-current-candidate-fills-contract"
    : "minimal-unlock-candidate-inventory-review-required";

var result = new
{
    phaseId = "phase246-minimal-unlock-candidate-inventory",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    minimalUnlockCandidateInventoryPassed,
    anyCandidateFillsWzAbsoluteScaleUnlock,
    anyCandidateFillsHiggsScalarScaleUnlock,
    candidateInventoryPromotableForBosonMasses,
    newSourceEvidenceStillRequired,
    currentPromotedConstraintRank,
    remainingNullity,
    minimumAdditionalIndependentSourceConstraints,
    phase213WzMissingFieldCount,
    phase213HiggsMissingFieldCount,
    wzCandidateRows,
    higgsCandidateRows,
    checks,
    decision = minimalUnlockCandidateInventoryPassed
        ? "The best current W/Z and Higgs unlock candidates are inventoried and none fills the Phase245 unlock contract. Numerical leads remain diagnostics; external inputs, target inversions, category mismatches, and parameterized formulas remain nonpromotable."
        : "Review the candidate inventory before relying on it.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU source for the W/Z common absolute scale, such as log(v g) or independent GU rows for v and g, satisfying Phase209 W/Z gates.",
        "A target-independent GU scalar source for the Higgs scale, such as log(v sqrt(lambda)) or independent GU rows for v and lambda, satisfying Phase209 Higgs gates.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase215Path = Phase215Path,
        phase221Path = Phase221Path,
        phase223Path = Phase223Path,
        phase224Path = Phase224Path,
        phase229Path = Phase229Path,
        phase234Path = Phase234Path,
        phase237Path = Phase237Path,
        phase238Path = Phase238Path,
        phase241Path = Phase241Path,
        phase245Path = Phase245Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var lineOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "minimal_unlock_candidate_inventory.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "minimal_unlock_candidate_inventory_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.minimalUnlockCandidateInventoryPassed,
        result.anyCandidateFillsWzAbsoluteScaleUnlock,
        result.anyCandidateFillsHiggsScalarScaleUnlock,
        result.candidateInventoryPromotableForBosonMasses,
        result.newSourceEvidenceStillRequired,
        result.currentPromotedConstraintRank,
        result.remainingNullity,
        result.minimumAdditionalIndependentSourceConstraints,
        result.phase213WzMissingFieldCount,
        result.phase213HiggsMissingFieldCount,
        result.wzCandidateRows,
        result.higgsCandidateRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

var traceLines = allCandidateRows
    .Select(row => JsonSerializer.Serialize(new { eventType = "candidate-row", row }, lineOptions))
    .Concat(checks.Select(check => JsonSerializer.Serialize(new { eventType = "check", check }, lineOptions)));
File.WriteAllLines(Path.Combine(outputDir, "minimal_unlock_candidate_inventory_trace.jsonl"), traceLines);

Console.WriteLine(terminalStatus);
Console.WriteLine($"minimalUnlockCandidateInventoryPassed={minimalUnlockCandidateInventoryPassed}");
Console.WriteLine($"anyCandidateFillsWzAbsoluteScaleUnlock={anyCandidateFillsWzAbsoluteScaleUnlock}");
Console.WriteLine($"anyCandidateFillsHiggsScalarScaleUnlock={anyCandidateFillsHiggsScalarScaleUnlock}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static JsonElement FindComparison(JsonElement root, string observableId)
{
    if (!root.TryGetProperty("comparisons", out var comparisons) || comparisons.ValueKind != JsonValueKind.Array)
    {
        return default;
    }

    foreach (var comparison in comparisons.EnumerateArray())
    {
        if (JsonString(comparison, "observableId") == observableId)
        {
            return comparison.Clone();
        }
    }

    return default;
}

static string FormatMaybe(double? value) => value.HasValue ? value.Value.ToString("G17") : "missing";

static string? JsonString(JsonElement element, string propertyName) =>
    element.ValueKind != JsonValueKind.Undefined
    && element.TryGetProperty(propertyName, out var property)
    && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.ValueKind != JsonValueKind.Undefined
    && element.TryGetProperty(propertyName, out var property)
    && property.ValueKind == JsonValueKind.Number
    && property.TryGetInt32(out var value)
        ? value
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.ValueKind != JsonValueKind.Undefined
    && element.TryGetProperty(propertyName, out var property)
    && property.ValueKind == JsonValueKind.Number
    && property.TryGetDouble(out var value)
        ? value
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty(propertyName, out var property)
        ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null }
        : null;

sealed record CandidateRow(
    string CandidateId,
    string UnlockId,
    string SourcePhase,
    string EvidenceKind,
    bool Promotable,
    string RejectionClass,
    string BestNumericalValue,
    string[] RequirementFailures,
    string WhyDoesNotSatisfyP245);

sealed record Check(string CheckId, bool Passed, string Detail);
