using System.Text.Json;

const string DefaultOutputDir = "studies/phase231_external_cox_gu_paper_i_source_intake_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase209WzRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/wz_absolute_source_lineage_evidence_request.json";
const string Phase209HiggsRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/higgs_scalar_source_lineage_evidence_request.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase230Path = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output/native_gu_vacuum_hessian_candidate_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE231_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase209Wz = JsonDocument.Parse(File.ReadAllText(Phase209WzRequestPath));
using var phase209Higgs = JsonDocument.Parse(File.ReadAllText(Phase209HiggsRequestPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase230 = JsonDocument.Parse(File.ReadAllText(Phase230Path));

var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzPromotable = phase201.RootElement.TryGetProperty("wzValidation", out var wzValidation)
    && JsonBool(wzValidation, "promotable") is true;
var higgsPromotable = phase201.RootElement.TryGetProperty("higgsValidation", out var higgsValidation)
    && JsonBool(higgsValidation, "promotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var phase224Closure = phase224.RootElement.GetProperty("closure");
var wParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var phase228MassMatrixPromotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var phase228MassMatrixObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;
var phase230NativeCandidatePromotable = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidatePromotable") is true;
var phase230NativeCandidateAuditPassed = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidateAuditPassed") is true;

var externalCoxPaperIReviewed = true;
var externalCoxPaperIResearchLeadPresent = true;
var externalCoxPaperISourceKind = "third-party-preprint-non-official-gu";
var coxPaperIClaimsShiabUniquenessLead = true;
var coxPaperIClaimsInvariantCurvatureAndAugmentedTorsionLead = true;
var coxPaperIClaimsProjectionVariationLead = true;
var coxPaperIClaimsInduced4DDynamicsLead = true;
var coxPaperILeavesRelativeCouplingsSymbolicForPaperII = true;
var coxPaperIDefersQuantizationToPaperIII = true;
var coxPaperIDefersPhenomenologyToPaperIV = true;

var wzFields = new[]
{
    new IntakeField("externalTargetValuesUsed=false", true, "The paper is treated as a source lead, not as a target-value fit."),
    new IntakeField("theoremOrDerivationId", false, "The paper's Shiab/projection-variation theorem is not a W/Z absolute-mass bridge theorem."),
    new IntakeField("sourceLineageId", false, "No local Phase201 sourceLineageId is filled from this paper."),
    new IntakeField("w-boson.sourceRowId", false, "No W particle-specific source row is supplied."),
    new IntakeField("w-boson.rawAmplitudeGatePassed=true", false, "No repository raw-amplitude replay gate is passed for W."),
    new IntakeField("w-boson.commonBridgeGatePassed=true", false, "No common W/Z bridge-scale gate is passed for W."),
    new IntakeField("w-boson.targetComparisonGatePassed=true", false, "No post-construction W target comparison row is supplied."),
    new IntakeField("w-boson.stabilitySidecarsPresent=true", false, "No branch/refinement/environment/representation/coupling sidecars are supplied for W."),
    new IntakeField("w-boson.derivationId", false, "No W mass derivation id is supplied."),
    new IntakeField("z-boson.sourceRowId", false, "No Z particle-specific source row is supplied."),
    new IntakeField("z-boson.rawAmplitudeGatePassed=true", false, "No repository raw-amplitude replay gate is passed for Z."),
    new IntakeField("z-boson.commonBridgeGatePassed=true", false, "No common W/Z bridge-scale gate is passed for Z."),
    new IntakeField("z-boson.targetComparisonGatePassed=true", false, "No post-construction Z target comparison row is supplied."),
    new IntakeField("z-boson.stabilitySidecarsPresent=true", false, "No branch/refinement/environment/representation/coupling sidecars are supplied for Z."),
    new IntakeField("z-boson.derivationId", false, "No Z mass derivation id is supplied."),
};

var higgsFields = new[]
{
    new IntakeField("externalTargetValuesUsed=false", true, "The paper is treated as a source lead, not as a target-value fit."),
    new IntakeField("sourceLineageId", false, "No local Phase201 Higgs sourceLineageId is filled from this paper."),
    new IntakeField("scalarSourceOperatorId", false, "The paper does not supply a solved Higgs scalar-source operator id for the repo gate."),
    new IntakeField("higgsIdentityEnvelopeId", false, "No Higgs identity envelope is supplied."),
    new IntakeField("massiveScalarProfileId", false, "No physical massive scalar profile is supplied."),
    new IntakeField("potentialOrSelfCouplingSourceId-or-excitationRelationId", false, "Relative couplings are left symbolic and phenomenology is deferred, so no Higgs self-coupling source is supplied."),
    new IntakeField("stabilitySidecars.branch=true", false, "No local branch-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.refinement=true", false, "No local refinement-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.environment=true", false, "No local environment-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.representation=true", false, "No local representation-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.coupling=true", false, "No local coupling-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("predictionRow.sourceRowId", false, "No Higgs prediction source row is supplied."),
    new IntakeField("predictionRow.targetComparisonGatePassed=true", false, "No post-construction Higgs target comparison row is supplied."),
    new IntakeField("predictionRow.derivationId", false, "No Higgs mass derivation id is supplied."),
};

var externalCoxPaperIFillsWzRequest = wzFields.All(field => field.Filled);
var externalCoxPaperIFillsHiggsRequest = higgsFields.All(field => field.Filled);
var externalCoxPaperIPromotableForBosonMasses =
    externalCoxPaperIFillsWzRequest
    && externalCoxPaperIFillsHiggsRequest
    && allRequiredLineagesPromotable
    && wParameterClosure
    && zParameterClosure
    && higgsParameterClosure
    && phase228MassMatrixPromotable
    && phase230NativeCandidatePromotable;

var checks = new[]
{
    new Check("external-cox-paper-i-reviewed", externalCoxPaperIReviewed && externalCoxPaperIResearchLeadPresent, $"externalCoxPaperIReviewed={externalCoxPaperIReviewed}; externalCoxPaperIResearchLeadPresent={externalCoxPaperIResearchLeadPresent}; sourceKind={externalCoxPaperISourceKind}"),
    new Check("shiab-invariant-geometry-lead-captured", coxPaperIClaimsShiabUniquenessLead && coxPaperIClaimsInvariantCurvatureAndAugmentedTorsionLead && coxPaperIClaimsProjectionVariationLead, $"shiabLead={coxPaperIClaimsShiabUniquenessLead}; invariantGeometryLead={coxPaperIClaimsInvariantCurvatureAndAugmentedTorsionLead}; projectionVariationLead={coxPaperIClaimsProjectionVariationLead}"),
    new Check("paper-i-defers-needed-prediction-parameters", coxPaperILeavesRelativeCouplingsSymbolicForPaperII && coxPaperIDefersQuantizationToPaperIII && coxPaperIDefersPhenomenologyToPaperIV, $"couplingsSymbolic={coxPaperILeavesRelativeCouplingsSymbolicForPaperII}; quantizationDeferred={coxPaperIDefersQuantizationToPaperIII}; phenomenologyDeferred={coxPaperIDefersPhenomenologyToPaperIV}"),
    new Check("wz-intake-request-not-filled", !externalCoxPaperIFillsWzRequest && wzFields.Any(field => !field.Filled), $"externalCoxPaperIFillsWzRequest={externalCoxPaperIFillsWzRequest}; missingWzFields={wzFields.Count(field => !field.Filled)}"),
    new Check("higgs-intake-request-not-filled", !externalCoxPaperIFillsHiggsRequest && higgsFields.Any(field => !field.Filled), $"externalCoxPaperIFillsHiggsRequest={externalCoxPaperIFillsHiggsRequest}; missingHiggsFields={higgsFields.Count(field => !field.Filled)}"),
    new Check("phase201-phase213-blockers-remain", !allRequiredLineagesPromotable && !wzPromotable && !higgsPromotable && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("phase224-parameter-closure-still-blocked", !wParameterClosure && !zParameterClosure && !higgsParameterClosure, $"wParameterClosure={wParameterClosure}; zParameterClosure={zParameterClosure}; higgsParameterClosure={higgsParameterClosure}"),
    new Check("phase228-phase230-extraction-still-blocked", phase228MassMatrixObstructionCertified && !phase228MassMatrixPromotable && phase230NativeCandidateAuditPassed && !phase230NativeCandidatePromotable, $"phase228MassMatrixObstructionCertified={phase228MassMatrixObstructionCertified}; phase230NativeCandidateAuditPassed={phase230NativeCandidateAuditPassed}"),
    new Check("external-cox-paper-i-not-promotable-for-boson-masses", !externalCoxPaperIPromotableForBosonMasses, $"externalCoxPaperIPromotableForBosonMasses={externalCoxPaperIPromotableForBosonMasses}"),
};

var externalCoxPaperISourceIntakeAuditPassed = checks.All(check => check.Passed)
    && !externalCoxPaperIPromotableForBosonMasses;
var terminalStatus = externalCoxPaperISourceIntakeAuditPassed
    ? "external-cox-gu-paper-i-reviewed-shiab-lead-not-boson-prediction-source"
    : "external-cox-gu-paper-i-source-intake-review-required";

var result = new
{
    phaseId = "phase231-external-cox-gu-paper-i-source-intake-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    externalCoxPaperIResearchLeadPresent,
    externalCoxPaperIPromotableForBosonMasses,
    externalCoxPaperISourceIntakeAuditPassed,
    objective = "Audit whether the external Cox 2025 GU I preprint can fill the repo W/Z and Higgs boson source-lineage evidence requests.",
    externalSource = new
    {
        title = "Geometric Unity I: From Heuristic Proposal to Testable Framework. Shiab Uniqueness, Invariant Curvature, Augmented Torsion, and Projection-Variation with Boundary Control",
        author = "Joseph Thomas Cox",
        date = "2025-10-02",
        doi = "10.5281/zenodo.17252989",
        doiUrl = "https://doi.org/10.5281/zenodo.17252989",
        discoveryUrl = "https://www.researchgate.net/publication/396132548_Geometric_Unity_I_From_Heuristic_Proposal_to_Testable_Framework_Shiab_Uniqueness_Invariant_Curvature_Augmented_Torsion_and_Projection-Variation_with_Boundary_Control",
        sourceKind = externalCoxPaperISourceKind,
        peerReviewStatus = "preprint-not-peer-reviewed-in-current-search-results",
        intakeInterpretation = "A useful external research lead for Shiab/invariant-geometry machinery, but not a completed W/Z/H boson mass source-lineage artifact for this repository.",
    },
    claimedResearchLeads = new
    {
        coxPaperIClaimsShiabUniquenessLead,
        coxPaperIClaimsInvariantCurvatureAndAugmentedTorsionLead,
        coxPaperIClaimsProjectionVariationLead,
        coxPaperIClaimsInduced4DDynamicsLead,
    },
    declaredLimitationsRelevantToBosonMasses = new
    {
        coxPaperILeavesRelativeCouplingsSymbolicForPaperII,
        coxPaperIDefersQuantizationToPaperIII,
        coxPaperIDefersPhenomenologyToPaperIV,
        implication = "The paper may help future Shiab/operator work, but the current artifact does not provide electroweak coupling magnitudes, Higgs self-coupling, physical W/Z/H particle rows, or target-comparison gates.",
    },
    wzIntake = new
    {
        requestPath = Phase209WzRequestPath,
        externalCoxPaperIFillsWzRequest,
        fields = wzFields,
    },
    higgsIntake = new
    {
        requestPath = Phase209HiggsRequestPath,
        externalCoxPaperIFillsHiggsRequest,
        fields = higgsFields,
    },
    currentRepoEvidence = new
    {
        phase201 = new
        {
            status = JsonString(phase201.RootElement, "terminalStatus"),
            allRequiredLineagesPromotable,
            wzPromotable,
            higgsPromotable,
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wParameterClosure,
            zParameterClosure,
            higgsParameterClosure,
        },
        phase228 = new
        {
            status = JsonString(phase228.RootElement, "terminalStatus"),
            phase228MassMatrixPromotable,
            phase228MassMatrixObstructionCertified,
        },
        phase230 = new
        {
            status = JsonString(phase230.RootElement, "terminalStatus"),
            phase230NativeCandidatePromotable,
            phase230NativeCandidateAuditPassed,
        },
    },
    checks,
    decision = externalCoxPaperISourceIntakeAuditPassed
        ? "Do not promote W/Z/H masses from Cox 2025 GU I alone. Preserve it as an external Shiab/invariant-geometry research lead, but the current paper leaves coupling/phenomenology work downstream and does not fill the repo's W/Z particle rows, Higgs scalar-source row, mass-matrix extraction, VEV source, stability sidecars, or target-comparison gates."
        : "Review Cox 2025 GU I source-intake evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "If using Cox GU I, materialize a repository source-lineage application that maps its Shiab/invariant-geometry claims to a fixed physical-sector operator branch.",
        "Supply the downstream Paper II-style relative coupling constants or equivalent target-independent derivations needed by W/Z and Higgs mass formulas.",
        "Supply W and Z particle-specific rows plus a Higgs scalar-source row satisfying Phase209/Phase201 gates.",
        "Rerun Phase210, Phase213, Phase228, Phase230, P101, and P202 before any promotion.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase209WzRequestPath = Phase209WzRequestPath,
        phase209HiggsRequestPath = Phase209HiggsRequestPath,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase228Path = Phase228Path,
        phase230Path = Phase230Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "external_cox_gu_paper_i_source_intake_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "external_cox_gu_paper_i_source_intake_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.externalCoxPaperIResearchLeadPresent,
        result.externalCoxPaperIPromotableForBosonMasses,
        result.externalCoxPaperISourceIntakeAuditPassed,
        result.externalSource,
        result.claimedResearchLeads,
        result.declaredLimitationsRelevantToBosonMasses,
        result.wzIntake,
        result.higgsIntake,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"externalCoxPaperIResearchLeadPresent={externalCoxPaperIResearchLeadPresent}");
Console.WriteLine($"externalCoxPaperIPromotableForBosonMasses={externalCoxPaperIPromotableForBosonMasses}");
Console.WriteLine($"externalCoxPaperISourceIntakeAuditPassed={externalCoxPaperISourceIntakeAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record IntakeField(string FieldId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
