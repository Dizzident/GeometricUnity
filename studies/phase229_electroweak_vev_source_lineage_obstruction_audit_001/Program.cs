using System.Text.Json;

const string DefaultOutputDir = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase69Path = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase195Path = "studies/phase195_electroweak_vev_wz_absolute_closure_audit_001/output/electroweak_vev_wz_absolute_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE229_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase69 = JsonDocument.Parse(File.ReadAllText(Phase69Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase195 = JsonDocument.Parse(File.ReadAllText(Phase195Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));

var fermiInput = phase54.RootElement
    .GetProperty("externalInputs")
    .EnumerateArray()
    .First(row => JsonString(row, "inputId") == "codata-2022-fermi-coupling-constant");
var fermiScale = phase54.RootElement
    .GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => JsonString(row, "scaleId") == "phase54-fermi-derived-electroweak-vacuum-scale");

var externalVevGeV = RequiredDouble(fermiScale, "value");
var phase54ExternalSourceKind = JsonString(fermiInput, "sourceKind");
var phase54ExternalScaleExcludedTargets = JsonStringArray(fermiScale, "excludedTargetObservableIds");

var phase69RelationDerived = string.Equals(JsonString(phase69.RootElement, "terminalStatus"), "electroweak-mass-generation-relation-derived", StringComparison.Ordinal);
var phase69ExcludedTargets = JsonStringArray(phase69.RootElement, "excludedTargetObservableIds");
var phase69ExcludesWzTargets = phase69ExcludedTargets.Contains("physical-w-boson-mass-gev", StringComparer.Ordinal)
    && phase69ExcludedTargets.Contains("physical-z-boson-mass-gev", StringComparer.Ordinal);

var phase70ScalarBridgeDerived = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
var phase70UsesExternalScaleInput = string.Equals(JsonString(phase70.RootElement, "externalScaleInputId"), "phase54-fermi-derived-electroweak-vacuum-scale", StringComparison.Ordinal);
var phase70OrderParameterRole = JsonString(phase70.RootElement, "scalarOrderParameterRole");

var phase195CanPromoteWzAbsoluteFromVevScale = JsonBool(phase195.RootElement, "canPromoteWzAbsoluteFromVevScale") is true;
var phase199CanPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var phase214CanPromoteExternalElectroweakBridge = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var phase228BosonMassMatrixExtractionPromotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var phase228BosonMassMatrixExtractionObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;

var guVacuumSolutionSourceIdPresent = false;
var guVacuumSelectionRulePresent = false;
var guVevSourceLineageIdPresent = false;
var guVevSourceLineageDerivationPresent = false;
var targetIndependentGuVevSourcePromotable =
    guVacuumSolutionSourceIdPresent
    && guVacuumSelectionRulePresent
    && guVevSourceLineageIdPresent
    && guVevSourceLineageDerivationPresent
    && !phase70UsesExternalScaleInput;

var checks = new[]
{
    new Check(
        "external-fermi-vev-recorded",
        externalVevGeV > 0.0
            && string.Equals(phase54ExternalSourceKind, "external-disjoint-physical-input", StringComparison.Ordinal)
            && phase54ExternalScaleExcludedTargets.Contains("physical-w-boson-mass-gev", StringComparer.Ordinal)
            && phase54ExternalScaleExcludedTargets.Contains("physical-z-boson-mass-gev", StringComparer.Ordinal),
        $"externalVevGeV={externalVevGeV}; sourceKind={phase54ExternalSourceKind}; excludedTargets={string.Join(",", phase54ExternalScaleExcludedTargets)}"),
    new Check(
        "phase69-relation-derived-but-not-vev-source",
        phase69RelationDerived && phase69ExcludesWzTargets,
        $"phase69RelationDerived={phase69RelationDerived}; phase69ExcludesWzTargets={phase69ExcludesWzTargets}; relationId={JsonString(phase69.RootElement, "massGenerationRelationId")}"),
    new Check(
        "phase70-bridge-derived-from-external-scale",
        phase70ScalarBridgeDerived && phase70UsesExternalScaleInput,
        $"phase70ScalarBridgeDerived={phase70ScalarBridgeDerived}; phase70UsesExternalScaleInput={phase70UsesExternalScaleInput}; role={phase70OrderParameterRole}"),
    new Check(
        "phase195-vev-wz-promotion-blocked",
        !phase195CanPromoteWzAbsoluteFromVevScale,
        $"canPromoteWzAbsoluteFromVevScale={phase195CanPromoteWzAbsoluteFromVevScale}"),
    new Check(
        "phase199-higgs-source-lineage-blocked",
        !phase199CanPromoteAnyHiggsScalarSourceLineage,
        $"canPromoteAnyHiggsScalarSourceLineage={phase199CanPromoteAnyHiggsScalarSourceLineage}"),
    new Check(
        "phase214-external-input-loophole-closed",
        !phase214CanPromoteExternalElectroweakBridge,
        $"canPromoteExternalElectroweakBridge={phase214CanPromoteExternalElectroweakBridge}"),
    new Check(
        "phase224-vev-parameter-closure-blocked",
        !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure,
        $"wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; zAbsoluteMassParameterClosure={zAbsoluteMassParameterClosure}; higgsMassParameterClosure={higgsMassParameterClosure}"),
    new Check(
        "phase228-mass-matrix-blocked",
        phase228BosonMassMatrixExtractionObstructionCertified && !phase228BosonMassMatrixExtractionPromotable,
        $"bosonMassMatrixExtractionObstructionCertified={phase228BosonMassMatrixExtractionObstructionCertified}; bosonMassMatrixExtractionPromotable={phase228BosonMassMatrixExtractionPromotable}"),
    new Check(
        "gu-vacuum-vev-source-lineage-missing",
        !guVacuumSolutionSourceIdPresent
            && !guVacuumSelectionRulePresent
            && !guVevSourceLineageIdPresent
            && !guVevSourceLineageDerivationPresent
            && !targetIndependentGuVevSourcePromotable,
        $"guVacuumSolutionSourceIdPresent={guVacuumSolutionSourceIdPresent}; guVacuumSelectionRulePresent={guVacuumSelectionRulePresent}; guVevSourceLineageIdPresent={guVevSourceLineageIdPresent}; guVevSourceLineageDerivationPresent={guVevSourceLineageDerivationPresent}; targetIndependentGuVevSourcePromotable={targetIndependentGuVevSourcePromotable}"),
};

var electroweakVevSourceLineageObstructionCertified = checks.All(check => check.Passed)
    && !targetIndependentGuVevSourcePromotable;
var terminalStatus = electroweakVevSourceLineageObstructionCertified
    ? "electroweak-vev-source-lineage-blocked-external-scale-not-gu-vacuum"
    : "electroweak-vev-source-lineage-review-required";

var result = new
{
    phaseId = "phase229-electroweak-vev-source-lineage-obstruction-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetIndependentGuVevSourcePromotable,
    electroweakVevSourceLineageObstructionCertified,
    objective = "Audit whether the current electroweak VEV/order-parameter bridge supplies a target-independent GU vacuum/VEV source lineage.",
    externalVevGeV,
    currentRepoEvidence = new
    {
        phase54 = new
        {
            status = JsonString(phase54.RootElement, "terminalStatus"),
            externalVevGeV,
            sourceKind = phase54ExternalSourceKind,
            excludedTargetObservableIds = phase54ExternalScaleExcludedTargets,
            interpretation = "Phase54 ingests a Fermi-derived electroweak scale as an external-disjoint physical input; it is not a GU vacuum solution or source-lineage derivation.",
        },
        phase69 = new
        {
            status = JsonString(phase69.RootElement, "terminalStatus"),
            relationDerived = phase69RelationDerived,
            massGenerationRelationId = JsonString(phase69.RootElement, "massGenerationRelationId"),
            weakCouplingNormalizationConvention = JsonString(phase69.RootElement, "weakCouplingNormalizationConvention"),
            excludesWzTargets = phase69ExcludesWzTargets,
            interpretation = "Phase69 supplies the electroweak mass-generation relation over internal modes; it does not derive the vacuum scale from GU geometry.",
        },
        phase70 = new
        {
            status = JsonString(phase70.RootElement, "terminalStatus"),
            scalarBridgeDerived = phase70ScalarBridgeDerived,
            externalScaleInputId = JsonString(phase70.RootElement, "externalScaleInputId"),
            usesExternalScaleInput = phase70UsesExternalScaleInput,
            scalarOrderParameterRole = phase70OrderParameterRole,
            sourceRelationId = JsonString(phase70.RootElement, "sourceRelationId"),
            interpretation = "Phase70 classifies the external electroweak scale as an order parameter role; it does not solve a GU vacuum or derive v from the GU action.",
        },
        phase195 = new
        {
            status = JsonString(phase195.RootElement, "terminalStatus"),
            canPromoteWzAbsoluteFromVevScale = phase195CanPromoteWzAbsoluteFromVevScale,
        },
        phase199 = new
        {
            status = JsonString(phase199.RootElement, "terminalStatus"),
            canPromoteAnyHiggsScalarSourceLineage = phase199CanPromoteAnyHiggsScalarSourceLineage,
        },
        phase214 = new
        {
            status = JsonString(phase214.RootElement, "terminalStatus"),
            canPromoteExternalElectroweakBridge = phase214CanPromoteExternalElectroweakBridge,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
        },
        phase228 = new
        {
            status = JsonString(phase228.RootElement, "terminalStatus"),
            bosonMassMatrixExtractionPromotable = phase228BosonMassMatrixExtractionPromotable,
            bosonMassMatrixExtractionObstructionCertified = phase228BosonMassMatrixExtractionObstructionCertified,
        },
    },
    guVevSourceRequirements = new[]
    {
        new SourceRequirement("gu-vacuum-solution-source-id", guVacuumSolutionSourceIdPresent, "A target-independent GU vacuum/background solution that carries the electroweak order-parameter role."),
        new SourceRequirement("gu-vacuum-selection-rule", guVacuumSelectionRulePresent, "A source-derived vacuum selection rule not chosen from W/Z/H target values."),
        new SourceRequirement("gu-vev-source-lineage-id", guVevSourceLineageIdPresent, "A replayable source-lineage artifact identifying how GU geometry fixes the electroweak VEV scale."),
        new SourceRequirement("gu-vev-source-derivation", guVevSourceLineageDerivationPresent, "A derivation that converts the GU vacuum/order parameter to the VEV used by the physical W/Z/H mass matrix."),
        new SourceRequirement("not-external-scale-import", !phase70UsesExternalScaleInput, "The promoted VEV source cannot be merely the Phase54 Fermi-derived external scale imported through Phase70."),
    },
    checks,
    decision = electroweakVevSourceLineageObstructionCertified
        ? "Do not promote the current VEV/order-parameter bridge as a target-independent GU VEV prediction. Phase54/69/70 provide useful relation and order-parameter context, but the scale is external Fermi-derived and the repo still lacks a GU vacuum solution, selection rule, VEV source-lineage derivation, and physical mass-matrix extraction."
        : "Review electroweak VEV source-lineage evidence before relying on this obstruction result.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU vacuum/background solution and source-derived vacuum-selection rule.",
        "A replayable GU VEV source-lineage derivation independent of W/Z/H mass targets and independent of the Phase54 external Fermi scale.",
        "A quadratic bosonic mass-matrix extraction around that vacuum with W/Z/photon/Higgs eigenstate projection and unit normalization.",
    },
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase69Path = Phase69Path,
        phase70Path = Phase70Path,
        phase195Path = Phase195Path,
        phase199Path = Phase199Path,
        phase214Path = Phase214Path,
        phase224Path = Phase224Path,
        phase228Path = Phase228Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electroweak_vev_source_lineage_obstruction_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_vev_source_lineage_obstruction_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetIndependentGuVevSourcePromotable,
        result.electroweakVevSourceLineageObstructionCertified,
        result.externalVevGeV,
        result.currentRepoEvidence,
        result.guVevSourceRequirements,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"targetIndependentGuVevSourcePromotable={targetIndependentGuVevSourcePromotable}");
Console.WriteLine($"electroweakVevSourceLineageObstructionCertified={electroweakVevSourceLineageObstructionCertified}");
Console.WriteLine($"externalVevGeV={externalVevGeV}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record SourceRequirement(string RequirementId, bool Filled, string Detail);
