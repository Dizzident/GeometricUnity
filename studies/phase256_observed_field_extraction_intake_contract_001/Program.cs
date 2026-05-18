using System.Text.Json;

const string DefaultOutputDir = "studies/phase256_observed_field_extraction_intake_contract_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase209Path = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/boson_source_lineage_evidence_request_package_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE256_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase209 = JsonDocument.Parse(File.ReadAllText(Phase209Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));

var requirementRows = new[]
{
    new RequirementRow("observedFieldExtractionTheoremId", "theorem", true, false, null, "A theorem or source artifact mapping GU variables to observed electroweak fields."),
    new RequirementRow("sourceReferenceIds", "provenance", true, false, Array.Empty<string>(), "Primary source references and local artifact paths for the extraction theorem."),
    new RequirementRow("canonicalOrDeclaredShiabBranchId", "branch", true, false, null, "A canonical Shiab/Upsilon operator choice, or an explicitly declared branch with noncanonicity labelled."),
    new RequirementRow("branchNormalizationSourceId", "normalization", true, false, null, "A target-independent normalization for the selected branch/operator and inner product."),
    new RequirementRow("fourDimensionalObservedVacuumArtifactId", "vacuum", true, false, null, "A source-derived four-dimensional observed-sector vacuum/background."),
    new RequirementRow("quadraticElectroweakMassOperatorId", "mass-operator", true, false, null, "The quadratic physical electroweak mass operator obtained by expansion around the source vacuum."),
    new RequirementRow("electroweakGaugeEmbeddingId", "embedding", true, false, null, "The observed SU(2)L x U(1)Y embedding and weak-mixing convention used by the mass operator."),
    new RequirementRow("photonEigenstateProjectionId", "projection", true, false, null, "A photon projection row with a massless gate before W/Z mass promotion."),
    new RequirementRow("wBosonSourceRowId", "w-source-row", true, false, null, "A W source row derived before target comparison."),
    new RequirementRow("zBosonSourceRowId", "z-source-row", true, false, null, "A Z source row derived before target comparison."),
    new RequirementRow("wBosonRawAmplitudeGatePassed", "w-gate", true, false, false, "The W raw-amplitude gate must pass from source replay, not target fit."),
    new RequirementRow("zBosonRawAmplitudeGatePassed", "z-gate", true, false, false, "The Z raw-amplitude gate must pass from source replay, not target fit."),
    new RequirementRow("wzCommonBridgeGatePassed", "wz-gate", true, false, false, "The W/Z common bridge must pass with a source-derived common normalization."),
    new RequirementRow("higgsScalarSourceOperatorId", "higgs-source", true, false, null, "A scalar source/operator identifying the Higgs sector."),
    new RequirementRow("higgsMassiveScalarProfileId", "higgs-profile", true, false, null, "A massive scalar profile/eigenmode for the physical Higgs."),
    new RequirementRow("higgsPotentialSelfCouplingRelationId", "higgs-coupling", true, false, null, "A target-independent Higgs potential/self-coupling or excitation relation."),
    new RequirementRow("targetBlindConstructionHash", "target-blindness", true, false, null, "A hash or provenance record proving construction before physical target comparison."),
    new RequirementRow("stabilitySidecarIds", "stability", true, false, Array.Empty<string>(), "Branch/refinement/stability sidecars for W, Z, photon, and Higgs projections."),
    new RequirementRow("targetComparisonAfterConstructionGatePassed", "comparison-order", true, false, false, "Target comparison is allowed only after all source construction gates are filled."),
    new RequirementRow("phase201And209ApplicationReady", "application", true, false, false, "The artifact must be ready to fill P201/P209/P210/P213 W/Z and Higgs fields."),
};

var requiredFieldCount = requirementRows.Count(row => row.Required);
var filledRequiredFieldCount = requirementRows.Count(row => row.Required && row.Filled);
var nullPlaceholderCount = requirementRows.Count(row => row.Value is null);
var falseGatePlaceholderCount = requirementRows.Count(row => row.Value is bool b && !b);
var arrayPlaceholderCount = requirementRows.Count(row => row.Value is string[] array && array.Length == 0);

var observedFieldExtractionNoGoPassed = JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed") is true;
var observedFieldExtractionBridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is true;
var newObservedFieldExtractionArtifactRequired = JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var rerunPromotionAllowed = JsonBool(phase209.RootElement, "rerunPromotionAllowed") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var remainingNullity = JsonInt(phase244.RootElement, "remainingNullity") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;

var allRequiredFieldsFilled = filledRequiredFieldCount == requiredFieldCount;
var observedFieldExtractionContractPromotable = allRequiredFieldsFilled
    && observedFieldExtractionBridgePromotable
    && allRequiredLineagesPromotable
    && rerunPromotionAllowed
    && unlockContractFilled;
var contractMaterialized = true;
var sourceLineageStillMissing = !observedFieldExtractionContractPromotable
    && observedFieldExtractionNoGoPassed
    && newObservedFieldExtractionArtifactRequired
    && wzMissingFieldCount == 15
    && higgsMissingFieldCount == 14
    && remainingNullity == 2;

var checks = new[]
{
    new Check(
        "contract-template-materialized",
        contractMaterialized && requiredFieldCount >= 20,
        $"contractMaterialized={contractMaterialized}; requiredFieldCount={requiredFieldCount}"),
    new Check(
        "template-is-intentionally-unfilled",
        !allRequiredFieldsFilled && filledRequiredFieldCount == 0 && nullPlaceholderCount > 0 && falseGatePlaceholderCount > 0,
        $"filledRequiredFieldCount={filledRequiredFieldCount}; nullPlaceholderCount={nullPlaceholderCount}; falseGatePlaceholderCount={falseGatePlaceholderCount}; arrayPlaceholderCount={arrayPlaceholderCount}"),
    new Check(
        "phase255-no-go-inherited",
        observedFieldExtractionNoGoPassed && !observedFieldExtractionBridgePromotable && newObservedFieldExtractionArtifactRequired,
        $"observedFieldExtractionNoGoPassed={observedFieldExtractionNoGoPassed}; observedFieldExtractionBridgePromotable={observedFieldExtractionBridgePromotable}; newObservedFieldExtractionArtifactRequired={newObservedFieldExtractionArtifactRequired}"),
    new Check(
        "source-lineage-contracts-still-unfilled",
        !allRequiredLineagesPromotable && !rerunPromotionAllowed && !unlockContractFilled && wzMissingFieldCount == 15 && higgsMissingFieldCount == 14,
        $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; rerunPromotionAllowed={rerunPromotionAllowed}; unlockContractFilled={unlockContractFilled}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "contract-not-promotable-until-filled",
        !observedFieldExtractionContractPromotable && sourceLineageStillMissing,
        $"observedFieldExtractionContractPromotable={observedFieldExtractionContractPromotable}; sourceLineageStillMissing={sourceLineageStillMissing}"),
};

var observedFieldExtractionIntakeContractPassed = checks.All(check => check.Passed);
var terminalStatus = observedFieldExtractionIntakeContractPassed
    ? "observed-field-extraction-intake-contract-awaiting-artifact"
    : "observed-field-extraction-intake-contract-review-required";

var template = new
{
    templateId = "observed-field-extraction-wzh-intake-template-v1",
    status = "template-unfilled",
    generatedAt = DateTimeOffset.UtcNow,
    promotionAllowed = observedFieldExtractionContractPromotable,
    requirementRows,
    applicationInstructions = new[]
    {
        "Fill every required field from a target-independent source artifact before rerunning promotion gates.",
        "Do not fill raw gates or target-comparison gates from observed W/Z/H masses.",
        "After filling, rerun P201/P209/P210/P213, then the full boson generator and P202.",
    },
};

var templatePath = Path.Combine(outputDir, "observed_field_extraction_intake_template.json");
var result = new
{
    phaseId = "phase256-observed-field-extraction-intake-contract",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    observedFieldExtractionIntakeContractPassed,
    contractMaterialized,
    templatePath,
    requiredFieldCount,
    filledRequiredFieldCount,
    nullPlaceholderCount,
    falseGatePlaceholderCount,
    arrayPlaceholderCount,
    allRequiredFieldsFilled,
    observedFieldExtractionContractPromotable,
    sourceLineageStillMissing,
    currentBlockerEvidence = new
    {
        phase201 = new
        {
            allRequiredLineagesPromotable,
        },
        phase209 = new
        {
            rerunPromotionAllowed,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase244 = new
        {
            remainingNullity,
        },
        phase245 = new
        {
            unlockContractFilled,
        },
        phase255 = new
        {
            observedFieldExtractionNoGoPassed,
            observedFieldExtractionBridgePromotable,
            newObservedFieldExtractionArtifactRequired,
        },
    },
    checks,
    decision = observedFieldExtractionIntakeContractPassed
        ? "The observed-field extraction bridge required by Phase255 is now represented as an executable intake contract, but the template is unfilled and cannot promote W/Z/H physical mass predictions."
        : "Review the observed-field extraction intake contract before relying on it.",
    nextRequiredArtifact = new[]
    {
        "A filled observed-field extraction theorem/source artifact satisfying every required field in the template.",
        "Then apply that artifact through P201/P209/P210/P213 and rerun the full boson generator.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase209Path = Phase209Path,
        phase213Path = Phase213Path,
        phase244Path = Phase244Path,
        phase245Path = Phase245Path,
        phase255Path = Phase255Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(templatePath, JsonSerializer.Serialize(template, options));
File.WriteAllText(Path.Combine(outputDir, "observed_field_extraction_intake_contract.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "observed_field_extraction_intake_contract_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.observedFieldExtractionIntakeContractPassed,
        result.contractMaterialized,
        result.templatePath,
        result.requiredFieldCount,
        result.filledRequiredFieldCount,
        result.nullPlaceholderCount,
        result.falseGatePlaceholderCount,
        result.arrayPlaceholderCount,
        result.allRequiredFieldsFilled,
        result.observedFieldExtractionContractPromotable,
        result.sourceLineageStillMissing,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"observedFieldExtractionIntakeContractPassed={observedFieldExtractionIntakeContractPassed}");
Console.WriteLine($"observedFieldExtractionContractPromotable={observedFieldExtractionContractPromotable}");
Console.WriteLine($"requiredFieldCount={requiredFieldCount}");
Console.WriteLine($"filledRequiredFieldCount={filledRequiredFieldCount}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record RequirementRow(
    string FieldId,
    string Category,
    bool Required,
    bool Filled,
    object? Value,
    string Acceptance);

sealed record Check(string CheckId, bool Passed, string Detail);
