using System.Text.Json;

const string DefaultOutputDir = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output";
const string Phase46StudyPath = "studies/phase46_electroweak_term_wz_source_spectra_001/STUDY.md";
const string Phase46ManifestPath = "studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/spectra_manifest.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE228_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase46Manifest = JsonDocument.Parse(File.ReadAllText(Phase46ManifestPath));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));

var phase46StudyText = File.Exists(Phase46StudyPath) ? File.ReadAllText(Phase46StudyPath) : "";
var phase46Computed = string.Equals(JsonString(phase46Manifest.RootElement, "status"), "computed", StringComparison.Ordinal);
var phase46MatrixCellCount = JsonInt(phase46Manifest.RootElement, "matrixCellCount") ?? 0;
var phase46MentionsPhysicalRatio = phase46StudyText.Contains("selected W/Z physical ratio", StringComparison.OrdinalIgnoreCase);
var phase46MentionsTargetIndependentExtraction = phase46StudyText.Contains("physical target is not used", StringComparison.OrdinalIgnoreCase);
var fullHessianModeFileCount = Directory.Exists("studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/modes")
    ? Directory.EnumerateFiles("studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/modes", "*.json", SearchOption.AllDirectories)
        .Count(path => File.ReadAllText(path).Contains("\"operatorType\": \"FullHessian\"", StringComparison.Ordinal))
    : 0;

var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var wParameterClosure = phase224.RootElement.TryGetProperty("closure", out var phase224Closure)
    && JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zParameterClosure = phase224.RootElement.TryGetProperty("closure", out phase224Closure)
    && JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsParameterClosure = phase224.RootElement.TryGetProperty("closure", out phase224Closure)
    && JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var shiabExtractionPromotable = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true;
var shiabExtractionObstructionCertified = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionObstructionCertified") is true;

var vacuumSolutionSourceIdPresent = false;
var vacuumSelectionRulePresent = false;
var gaugeFixedQuadraticExpansionPresent = false;
var physicalBosonMassMatrixOperatorPresent = false;
var particleEigenstateProjectionPresent = false;
var eigenvalueToPhysicalUnitsNormalizationPresent = false;
var higgsScalarHessianOrSelfCouplingPresent = false;
var sourceLineageRowsFilled = allRequiredLineagesPromotable;

var extractionRequirements = new[]
{
    new ExtractionRequirement("gu-vacuum-solution-source-id", vacuumSolutionSourceIdPresent, "A target-independent GU vacuum/background solution with electroweak symmetry-breaking role is required before a physical mass Hessian can be expanded."),
    new ExtractionRequirement("vacuum-selection-rule", vacuumSelectionRulePresent, "The vacuum selection rule must be source-derived, not chosen to match W/Z/H masses."),
    new ExtractionRequirement("gauge-fixed-quadratic-expansion", gaugeFixedQuadraticExpansionPresent, "Masses are read from quadratic terms after gauge fixing or equivalent constraint handling; no checked GU quadratic expansion around the physical vacuum exists."),
    new ExtractionRequirement("physical-boson-mass-matrix-operator", physicalBosonMassMatrixOperatorPresent, "Existing FullHessian spectra are source/mode diagnostics, not a W/Z/H physical mass matrix with units and electroweak sector projection."),
    new ExtractionRequirement("particle-eigenstate-projection", particleEigenstateProjectionPresent, "No checked projection maps the mass matrix eigenvectors to W+, W-, Z, photon, and Higgs rows."),
    new ExtractionRequirement("eigenvalue-to-physical-units-normalization", eigenvalueToPhysicalUnitsNormalizationPresent, "No target-independent conversion maps GU Hessian eigenvalues to GeV masses."),
    new ExtractionRequirement("higgs-scalar-hessian-or-self-coupling", higgsScalarHessianOrSelfCouplingPresent, "No scalar Hessian/self-coupling source currently fills the Higgs source-lineage contract."),
    new ExtractionRequirement("source-lineage-rows-filled", sourceLineageRowsFilled, "Phase201/Phase213 W/Z/H source-lineage rows remain unfilled."),
};

var bosonMassMatrixExtractionPromotable =
    extractionRequirements.All(requirement => requirement.Filled)
    && wParameterClosure
    && zParameterClosure
    && higgsParameterClosure
    && shiabExtractionPromotable;

var checks = new[]
{
    new Check("standard-ew-mass-matrix-dependency-recorded", true, "PDG electroweak review derives W/Z/H masses from the bosonic Lagrangian after the Higgs doublet develops a VEV; this requires a vacuum and quadratic mass terms."),
    new Check("phase46-hessian-like-spectra-recorded", phase46Computed && phase46MatrixCellCount > 0 && fullHessianModeFileCount > 0, $"phase46Computed={phase46Computed}; matrixCellCount={phase46MatrixCellCount}; fullHessianModeFileCount={fullHessianModeFileCount}"),
    new Check("phase46-not-physical-mass-matrix", phase46MentionsPhysicalRatio && phase46MentionsTargetIndependentExtraction && !physicalBosonMassMatrixOperatorPresent, $"phase46MentionsPhysicalRatio={phase46MentionsPhysicalRatio}; physicalBosonMassMatrixOperatorPresent={physicalBosonMassMatrixOperatorPresent}"),
    new Check("vacuum-and-quadratic-expansion-missing", !vacuumSolutionSourceIdPresent && !vacuumSelectionRulePresent && !gaugeFixedQuadraticExpansionPresent, $"vacuumSolutionSourceIdPresent={vacuumSolutionSourceIdPresent}; vacuumSelectionRulePresent={vacuumSelectionRulePresent}; gaugeFixedQuadraticExpansionPresent={gaugeFixedQuadraticExpansionPresent}"),
    new Check("mass-eigenstate-projection-and-units-missing", !particleEigenstateProjectionPresent && !eigenvalueToPhysicalUnitsNormalizationPresent, $"particleEigenstateProjectionPresent={particleEigenstateProjectionPresent}; eigenvalueToPhysicalUnitsNormalizationPresent={eigenvalueToPhysicalUnitsNormalizationPresent}"),
    new Check("phase201-phase213-source-lineage-blockers-remain", !allRequiredLineagesPromotable && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("phase224-parameter-closure-still-blocked", !wParameterClosure && !zParameterClosure && !higgsParameterClosure, $"wParameterClosure={wParameterClosure}; zParameterClosure={zParameterClosure}; higgsParameterClosure={higgsParameterClosure}"),
    new Check("phase227-action-extraction-still-blocked", shiabExtractionObstructionCertified && !shiabExtractionPromotable, $"shiabExtractionObstructionCertified={shiabExtractionObstructionCertified}; shiabExtractionPromotable={shiabExtractionPromotable}"),
};

var bosonMassMatrixExtractionObstructionCertified = checks.All(check => check.Passed)
    && extractionRequirements.Any(requirement => !requirement.Filled)
    && !bosonMassMatrixExtractionPromotable;
var terminalStatus = bosonMassMatrixExtractionObstructionCertified
    ? "boson-mass-matrix-extraction-blocked-no-vacuum-hessian-sector-projection"
    : "boson-mass-matrix-extraction-review-required";

var result = new
{
    phaseId = "phase228-boson-mass-matrix-extraction-obstruction-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    bosonMassMatrixExtractionPromotable,
    bosonMassMatrixExtractionObstructionCertified,
    objective = "Audit whether existing GU Hessian-like artifacts constitute a physical W/Z/H mass matrix extraction.",
    externalPhysicsContext = new
    {
        electroweakReview = "Particle Data Group 2025 Review, Electroweak Model and Constraints on New Physics",
        electroweakReviewUrl = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        higgsReview = "Particle Data Group 2025 Review, Status of Higgs Boson Physics",
        higgsReviewUrl = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-higgs-boson.pdf",
        dependencySummary = "In the Standard Model, the Higgs doublet VEV and quadratic bosonic Lagrangian terms determine W/Z/H tree-level masses. A GU prediction therefore needs an analogous source-derived vacuum, quadratic mass matrix, eigenstate projection, and unit normalization.",
    },
    localHessianLikeArtifacts = new
    {
        phase46StudyPath = Phase46StudyPath,
        phase46ManifestPath = Phase46ManifestPath,
        phase46Computed,
        phase46MatrixCellCount,
        fullHessianModeFileCount,
        phase46MentionsPhysicalRatio,
        phase46MentionsTargetIndependentExtraction,
        interpretation = "Phase46 contains target-independent W/Z ratio source spectra with FullHessian operator diagnostics. It does not provide a vacuum-expanded physical mass matrix, GeV normalization, or Higgs scalar Hessian.",
    },
    extractionRequirements,
    currentRepoEvidence = new
    {
        phase201 = new
        {
            status = JsonString(phase201.RootElement, "terminalStatus"),
            allRequiredLineagesPromotable,
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
            higgsMissingFieldCount,
            wzMissingFields = JsonStringArray(phase213.RootElement, "wzMissingFields"),
            higgsMissingFields = JsonStringArray(phase213.RootElement, "higgsMissingFields"),
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wParameterClosure,
            zParameterClosure,
            higgsParameterClosure,
        },
        phase227 = new
        {
            status = JsonString(phase227.RootElement, "terminalStatus"),
            officialGuShiabUpsilonExtractionPromotable = shiabExtractionPromotable,
            officialGuShiabUpsilonExtractionObstructionCertified = shiabExtractionObstructionCertified,
        },
    },
    checks,
    decision = bosonMassMatrixExtractionObstructionCertified
        ? "Do not promote W/Z/H physical masses from existing Hessian-like spectra. The repo has operator spectra and numerical leads, but not the source-derived vacuum, quadratic mass matrix, eigenstate projection, and units required for a physical boson mass prediction."
        : "Review mass-matrix extraction evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU vacuum/background solution and vacuum-selection rule.",
        "A gauge-fixed or constraint-consistent quadratic expansion of the GU action around that vacuum.",
        "A physical boson mass matrix with W/Z/photon/Higgs eigenstate projection and unit normalization.",
        "Filled W/Z/H Phase201 source-lineage rows passing Phase209/Phase210/Phase213 and the top-level Phase101/Phase202 gates.",
    },
    sourceEvidence = new
    {
        phase46StudyPath = Phase46StudyPath,
        phase46ManifestPath = Phase46ManifestPath,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase227Path = Phase227Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_mass_matrix_extraction_obstruction_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_mass_matrix_extraction_obstruction_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.bosonMassMatrixExtractionPromotable,
        result.bosonMassMatrixExtractionObstructionCertified,
        result.externalPhysicsContext,
        result.localHessianLikeArtifacts,
        result.extractionRequirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"bosonMassMatrixExtractionPromotable={bosonMassMatrixExtractionPromotable}");
Console.WriteLine($"bosonMassMatrixExtractionObstructionCertified={bosonMassMatrixExtractionObstructionCertified}");
Console.WriteLine($"fullHessianModeFileCount={fullHessianModeFileCount}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

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
sealed record ExtractionRequirement(string RequirementId, bool Filled, string Detail);
