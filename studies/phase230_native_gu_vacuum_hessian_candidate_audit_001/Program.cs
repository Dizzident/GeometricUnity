using System.Text.Json;

const string DefaultOutputDir = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output";
const string ReadmePath = "README.md";
const string ShiabSummaryPath = "reports/post_phase11_evidence_campaign/20260315T165000Z/shiab_companion/shiab_path_comparison_summary.json";
const string ShiabScopeSu2Path = "reports/post_phase11_evidence_campaign/20260315T165000Z/shiab_companion/shiab_scope_record_su2.json";
const string ShiabScopeSu3Path = "reports/post_phase11_evidence_campaign/20260315T165000Z/shiab_companion/shiab_scope_record_su3.json";
const string Phase9AtlasPath = "studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json";
const string Phase9GeometryPath = "studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/manifest/geometry.json";
const string TheoryCompletionPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE230_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var readmeText = File.ReadAllText(ReadmePath);
var theoryText = File.ReadAllText(TheoryCompletionPath);
using var shiabSummary = JsonDocument.Parse(File.ReadAllText(ShiabSummaryPath));
using var shiabScopeSu2 = JsonDocument.Parse(File.ReadAllText(ShiabScopeSu2Path));
using var shiabScopeSu3 = JsonDocument.Parse(File.ReadAllText(ShiabScopeSu3Path));
using var phase9Atlas = JsonDocument.Parse(File.ReadAllText(Phase9AtlasPath));
using var phase9Geometry = JsonDocument.Parse(File.ReadAllText(Phase9GeometryPath));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));

var readmeDeniesPhysicalObservable = readmeText.Contains("no current observable is validated as a physical W/Z/Higgs/photon property", StringComparison.Ordinal);
var readmeDeniesGeVCalibration = readmeText.Contains("no physical unit calibration to GeV or measured couplings exists yet", StringComparison.Ordinal);
var readmeHasUpsilonJacobianPipeline = readmeText.Contains("Upsilon = S - T", StringComparison.Ordinal)
    && readmeText.Contains("J = dUpsilon/domega", StringComparison.Ordinal)
    && readmeText.Contains("sigma_h* pullback", StringComparison.Ordinal);

var bindingConstraint = JsonString(shiabSummary.RootElement, "bindingConstraint") ?? "";
var overallConclusion = JsonString(shiabSummary.RootElement, "overallConclusion") ?? "";
var canonicalDraftShiabSelected = !bindingConstraint.Contains("Neither standard nor paired operator is the canonically selected draft Shiab", StringComparison.Ordinal)
    && !overallConclusion.Contains("Shiab family remains open", StringComparison.OrdinalIgnoreCase);
var standardPathBranchResult = JsonString(shiabSummary.RootElement.GetProperty("standardPath"), "branchRobustnessResult");
var activeFatalCount = JsonInt(shiabSummary.RootElement.GetProperty("standardPath"), "activeFatalCount") ?? 0;
var activeHighCount = JsonInt(shiabSummary.RootElement.GetProperty("standardPath"), "activeHighCount") ?? 0;

var su2FamilyOpenStatement = JsonString(shiabScopeSu2.RootElement, "familyOpenStatement") ?? "";
var su3FamilyOpenStatement = JsonString(shiabScopeSu3.RootElement, "familyOpenStatement") ?? "";
var shiabBranchesAreSurrogate = su2FamilyOpenStatement.Contains("draftAlignmentStatus=surrogate", StringComparison.Ordinal)
    && su3FamilyOpenStatement.Contains("draftAlignmentStatus=surrogate", StringComparison.Ordinal);
var fullShiabFamilyRequiresFourDimensions = su2FamilyOpenStatement.Contains("dimX >= 4", StringComparison.Ordinal)
    && su3FamilyOpenStatement.Contains("dimX >= 4", StringComparison.Ordinal);

var backgrounds = phase9Atlas.RootElement.GetProperty("backgrounds").EnumerateArray().ToArray();
var backgroundCount = backgrounds.Length;
var lowResidualBackgroundCount = backgrounds.Count(row => (JsonDouble(row, "residualNorm") ?? double.PositiveInfinity) <= 1.0e-8);
var convergedBackgroundCount = backgrounds.Count(row => row.TryGetProperty("metrics", out var metrics) && JsonBool(metrics, "solverConverged") is true);
var toyBackgroundCount = backgrounds.Count(row => string.Equals(JsonString(row, "environmentTier"), "toy", StringComparison.Ordinal));
var baseDimension = JsonInt(phase9Geometry.RootElement.GetProperty("baseSpace"), "dimension") ?? 0;
var ambientDimension = JsonInt(phase9Geometry.RootElement.GetProperty("ambientSpace"), "dimension") ?? 0;

var theoryRequiresLinearizationPackage = theoryText.Contains("D\\Upsilon_{(A_\\star,\\omega_\\star)}", StringComparison.Ordinal)
    && theoryText.Contains("D\\Upsilon_{(A_\\star,\\omega_\\star)}^*D\\Upsilon_{(A_\\star,\\omega_\\star)}", StringComparison.Ordinal);
var theoryDemotesObservedFieldMappings = theoryText.Contains("Any statement identifying a GU field component directly with Einstein, Yang", StringComparison.Ordinal)
    && theoryText.Contains("Phenomenological Mapping", StringComparison.Ordinal);
var theoryListsObservedBosonRepresentationBlocker = theoryText.Contains("Representation decomposition of observed bosons", StringComparison.Ordinal)
    && theoryText.Contains("Standard Model correspondence remains interpretive rather than derivational", StringComparison.Ordinal);
var theoryListsHiggsYukawaBlocker = theoryText.Contains("Boson", StringComparison.Ordinal)
    && theoryText.Contains("Higgs/Yukawa reinterpretation", StringComparison.Ordinal)
    && theoryText.Contains("remain speculative", StringComparison.Ordinal);

var phase228MassMatrixPromotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var phase228MassMatrixObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;
var phase229VevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var phase229VevSourceObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;

var canonicalShiabOperatorPresent = canonicalDraftShiabSelected && !shiabBranchesAreSurrogate;
var sourceDerivedGuVacuumPresent = false;
var observedBosonExtractionTheoremPresent = false;
var physicalBosonMassMatrixPresent = false;
var physicalUnitNormalizationPresent = false;
var guVevSourceDerivationPresent = false;
var branchIndependentFourDimensionalShiabEvidencePresent = baseDimension >= 4
    && !shiabBranchesAreSurrogate
    && canonicalShiabOperatorPresent;

var extractionRequirements = new[]
{
    new ExtractionRequirement("canonical-draft-aligned-shiab-operator", canonicalShiabOperatorPresent, "The active Shiab branch must be draft-aligned or canonically selected, not a surrogate branch from toy comparison runs."),
    new ExtractionRequirement("source-derived-gu-vacuum-background", sourceDerivedGuVacuumPresent, "A low-residual toy background is not enough; the vacuum must be a source-derived GU electroweak background with a selection rule."),
    new ExtractionRequirement("four-dimensional-observed-sector-background", baseDimension >= 4 && toyBackgroundCount == 0, "The inspected first-order atlas uses a 2D toy base geometry, while the observed-sector extraction must be four-dimensional and physical."),
    new ExtractionRequirement("observed-boson-extraction-theorem", observedBosonExtractionTheoremPresent, "A theorem must map native GU components to W, Z, photon, and Higgs observed eigenstates."),
    new ExtractionRequirement("physical-boson-mass-matrix", physicalBosonMassMatrixPresent, "A Hessian-style DUpsilon*DUpsilon diagnostic is not yet a gauge-fixed physical W/Z/H mass matrix."),
    new ExtractionRequirement("physical-unit-normalization", physicalUnitNormalizationPresent, "The extraction must include target-independent normalization to GeV masses."),
    new ExtractionRequirement("gu-vev-source-derivation", guVevSourceDerivationPresent, "The electroweak VEV must be derived from GU source data, not imported from the external Fermi scale."),
};

var nativeGuVacuumHessianCandidatePromotable = extractionRequirements.All(requirement => requirement.Filled)
    && !phase228MassMatrixPromotable
    && !phase229VevSourcePromotable;

var checks = new[]
{
    new Check("readme-nonprediction-boundary-recorded", readmeDeniesPhysicalObservable && readmeDeniesGeVCalibration && readmeHasUpsilonJacobianPipeline, $"readmeDeniesPhysicalObservable={readmeDeniesPhysicalObservable}; readmeDeniesGeVCalibration={readmeDeniesGeVCalibration}; readmeHasUpsilonJacobianPipeline={readmeHasUpsilonJacobianPipeline}"),
    new Check("shiab-operator-not-canonical", !canonicalDraftShiabSelected && shiabBranchesAreSurrogate && fullShiabFamilyRequiresFourDimensions, $"canonicalDraftShiabSelected={canonicalDraftShiabSelected}; shiabBranchesAreSurrogate={shiabBranchesAreSurrogate}; fullShiabFamilyRequiresFourDimensions={fullShiabFamilyRequiresFourDimensions}"),
    new Check("phase9-backgrounds-are-toy-not-physical-vacua", backgroundCount > 0 && lowResidualBackgroundCount > 0 && convergedBackgroundCount > 0 && toyBackgroundCount == backgroundCount && baseDimension == 2, $"backgroundCount={backgroundCount}; lowResidualBackgroundCount={lowResidualBackgroundCount}; convergedBackgroundCount={convergedBackgroundCount}; toyBackgroundCount={toyBackgroundCount}; baseDimension={baseDimension}; ambientDimension={ambientDimension}"),
    new Check("theory-notes-require-extraction-theorem", theoryRequiresLinearizationPackage && theoryDemotesObservedFieldMappings && theoryListsObservedBosonRepresentationBlocker && theoryListsHiggsYukawaBlocker, $"theoryRequiresLinearizationPackage={theoryRequiresLinearizationPackage}; theoryDemotesObservedFieldMappings={theoryDemotesObservedFieldMappings}; theoryListsObservedBosonRepresentationBlocker={theoryListsObservedBosonRepresentationBlocker}; theoryListsHiggsYukawaBlocker={theoryListsHiggsYukawaBlocker}"),
    new Check("phase228-physical-mass-matrix-still-blocked", phase228MassMatrixObstructionCertified && !phase228MassMatrixPromotable, $"phase228MassMatrixObstructionCertified={phase228MassMatrixObstructionCertified}; phase228MassMatrixPromotable={phase228MassMatrixPromotable}"),
    new Check("phase229-gu-vev-source-still-blocked", phase229VevSourceObstructionCertified && !phase229VevSourcePromotable, $"phase229VevSourceObstructionCertified={phase229VevSourceObstructionCertified}; phase229VevSourcePromotable={phase229VevSourcePromotable}"),
    new Check("native-candidates-remain-nonpromotable", !nativeGuVacuumHessianCandidatePromotable && extractionRequirements.Any(requirement => !requirement.Filled), $"nativeGuVacuumHessianCandidatePromotable={nativeGuVacuumHessianCandidatePromotable}; missingRequirementCount={extractionRequirements.Count(requirement => !requirement.Filled)}"),
};

var nativeGuVacuumHessianCandidateAuditPassed = checks.All(check => check.Passed)
    && !nativeGuVacuumHessianCandidatePromotable;
var terminalStatus = nativeGuVacuumHessianCandidateAuditPassed
    ? "native-gu-vacuum-hessian-candidate-audit-no-promotable-physical-extraction"
    : "native-gu-vacuum-hessian-candidate-audit-review-required";

var result = new
{
    phaseId = "phase230-native-gu-vacuum-hessian-candidate-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    nativeGuVacuumHessianCandidatePromotable,
    nativeGuVacuumHessianCandidateAuditPassed,
    objective = "Audit whether local native GU Upsilon/Shiab/background/Hessian artifacts can fill the missing physical W/Z/H vacuum and mass-matrix extraction source lineages.",
    currentRepoEvidence = new
    {
        readme = new
        {
            path = ReadmePath,
            readmeDeniesPhysicalObservable,
            readmeDeniesGeVCalibration,
            readmeHasUpsilonJacobianPipeline,
        },
        shiabCompanion = new
        {
            summaryPath = ShiabSummaryPath,
            scopeSu2Path = ShiabScopeSu2Path,
            scopeSu3Path = ShiabScopeSu3Path,
            canonicalDraftShiabSelected,
            shiabBranchesAreSurrogate,
            fullShiabFamilyRequiresFourDimensions,
            standardPathBranchResult,
            activeFatalCount,
            activeHighCount,
        },
        phase9FirstOrderAtlas = new
        {
            atlasPath = Phase9AtlasPath,
            geometryPath = Phase9GeometryPath,
            backgroundCount,
            lowResidualBackgroundCount,
            convergedBackgroundCount,
            toyBackgroundCount,
            baseDimension,
            ambientDimension,
            interpretation = "The first-order Shiab atlas supplies branch-local toy background solutions, not a four-dimensional observed electroweak vacuum with W/Z/H mass extraction.",
        },
        theoryCompletionNotes = new
        {
            path = TheoryCompletionPath,
            theoryRequiresLinearizationPackage,
            theoryDemotesObservedFieldMappings,
            theoryListsObservedBosonRepresentationBlocker,
            theoryListsHiggsYukawaBlocker,
            interpretation = "The local completion notes treat observed field mappings and Higgs/Yukawa reinterpretation as extraction obligations, not completed physical predictions.",
        },
        phase228 = new
        {
            path = Phase228Path,
            phase228MassMatrixPromotable,
            phase228MassMatrixObstructionCertified,
        },
        phase229 = new
        {
            path = Phase229Path,
            phase229VevSourcePromotable,
            phase229VevSourceObstructionCertified,
        },
    },
    extractionRequirements,
    checks,
    decision = nativeGuVacuumHessianCandidateAuditPassed
        ? "Do not promote local native GU background or Hessian-style artifacts as W/Z/H physical mass predictions. They are useful branch-local computational evidence, but current inspected artifacts use surrogate Shiab operators, toy 2D backgrounds, no observed-boson extraction theorem, no physical mass matrix, no GeV normalization, and no GU-derived VEV source."
        : "Review native GU vacuum/Hessian candidate evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A draft-aligned or canonically justified Shiab operator branch with four-dimensional observed-sector support.",
        "A target-independent GU electroweak vacuum/background solution and selection rule.",
        "A completed observed-boson extraction theorem mapping native modes to W, Z, photon, and Higgs eigenstates.",
        "A gauge-consistent physical mass matrix with target-independent GeV normalization and GU VEV source derivation.",
    },
    sourceEvidence = new
    {
        readmePath = ReadmePath,
        shiabSummaryPath = ShiabSummaryPath,
        shiabScopeSu2Path = ShiabScopeSu2Path,
        shiabScopeSu3Path = ShiabScopeSu3Path,
        phase9AtlasPath = Phase9AtlasPath,
        phase9GeometryPath = Phase9GeometryPath,
        theoryCompletionPath = TheoryCompletionPath,
        phase228Path = Phase228Path,
        phase229Path = Phase229Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "native_gu_vacuum_hessian_candidate_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "native_gu_vacuum_hessian_candidate_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.nativeGuVacuumHessianCandidatePromotable,
        result.nativeGuVacuumHessianCandidateAuditPassed,
        result.currentRepoEvidence,
        result.extractionRequirements,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"nativeGuVacuumHessianCandidatePromotable={nativeGuVacuumHessianCandidatePromotable}");
Console.WriteLine($"nativeGuVacuumHessianCandidateAuditPassed={nativeGuVacuumHessianCandidateAuditPassed}");
Console.WriteLine($"backgroundCount={backgroundCount}");
Console.WriteLine($"baseDimension={baseDimension}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExtractionRequirement(string RequirementId, bool Filled, string Detail);
