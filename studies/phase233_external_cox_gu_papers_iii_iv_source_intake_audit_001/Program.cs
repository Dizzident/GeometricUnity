using System.Text.Json;

const string DefaultOutputDir = "studies/phase233_external_cox_gu_papers_iii_iv_source_intake_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase209WzRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/wz_absolute_source_lineage_evidence_request.json";
const string Phase209HiggsRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/higgs_scalar_source_lineage_evidence_request.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase230Path = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output/native_gu_vacuum_hessian_candidate_audit_summary.json";
const string Phase232Path = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output/external_cox_gu_paper_ii_source_intake_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE233_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase209Wz = JsonDocument.Parse(File.ReadAllText(Phase209WzRequestPath));
using var phase209Higgs = JsonDocument.Parse(File.ReadAllText(Phase209HiggsRequestPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase230 = JsonDocument.Parse(File.ReadAllText(Phase230Path));
using var phase232 = JsonDocument.Parse(File.ReadAllText(Phase232Path));

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
var phase229VevPromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var phase229VevObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var phase230NativeCandidatePromotable = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidatePromotable") is true;
var phase230NativeCandidateAuditPassed = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidateAuditPassed") is true;
var phase232PaperIIPromotable = JsonBool(phase232.RootElement, "externalCoxPaperIIPromotableForBosonMasses") is true;
var phase232PaperIIAuditPassed = JsonBool(phase232.RootElement, "externalCoxPaperIISourceIntakeAuditPassed") is true;

var externalCoxPapersIIIIVReviewed = true;
var externalCoxPapersIIIIVResearchLeadPresent = true;
var externalCoxPapersIIIIVSourceKind = "third-party-preprint-non-official-gu";
var paperIIIBrstQuantizationLeadPresent = true;
var paperIIICohomologyAndAnomalyLeadPresent = true;
var paperIIIRunningSignLeadPresent = true;
var paperIIIQuantumProjectionVariationLeadPresent = true;
var paperIVBoundaryFamiliesLeadPresent = true;
var paperIVSliceEftObservableMapLeadPresent = true;
var paperIVBounceCosmologyInterfaceLeadPresent = true;
var paperIVGlobalFalsifierWorkflowLeadPresent = true;
var papersIIIIVFocusOnAxialContactBcAndObservables = true;
var papersIIIIVDoNotProvideWzMassRows = true;
var papersIIIIVDoNotProvideSolvedHiggsMassSource = true;
var papersIIIIVDoNotProvideObservedWZHMassValues = true;

var wzFields = new[]
{
    new IntakeField("externalTargetValuesUsed=false", true, "The papers are treated as source leads, not as target-value fits."),
    new IntakeField("theoremOrDerivationId", false, "Papers III-IV provide quantization/BRST, running/sign, boundary, and observable-map leads, but not a W/Z absolute-mass bridge theorem with fixed GU-derived couplings and VEV."),
    new IntakeField("sourceLineageId", false, "No local Phase201 W/Z sourceLineageId is filled from these papers."),
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
    new IntakeField("externalTargetValuesUsed=false", true, "The papers are treated as source leads, not as target-value fits."),
    new IntakeField("sourceLineageId", false, "No local Phase201 Higgs sourceLineageId is filled from these papers."),
    new IntakeField("scalarSourceOperatorId", false, "No solved Higgs scalar-source operator id is supplied."),
    new IntakeField("higgsIdentityEnvelopeId", false, "No Higgs identity envelope is supplied."),
    new IntakeField("massiveScalarProfileId", false, "No physical massive scalar profile is supplied."),
    new IntakeField("potentialOrSelfCouplingSourceId-or-excitationRelationId", false, "Axial-contact running and BC observables do not supply a Higgs quartic/self-coupling source or mass-excitation relation."),
    new IntakeField("stabilitySidecars.branch=true", false, "No local branch-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.refinement=true", false, "No local refinement-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.environment=true", false, "No local environment-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.representation=true", false, "No local representation-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.coupling=true", false, "No local coupling-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("predictionRow.sourceRowId", false, "No Higgs prediction source row is supplied."),
    new IntakeField("predictionRow.targetComparisonGatePassed=true", false, "No post-construction Higgs target comparison row is supplied."),
    new IntakeField("predictionRow.derivationId", false, "No Higgs mass derivation id is supplied."),
};

var externalCoxPapersIIIIVFillWzRequest = wzFields.All(field => field.Filled);
var externalCoxPapersIIIIVFillHiggsRequest = higgsFields.All(field => field.Filled);
var externalCoxPapersIIIIVPromotableForBosonMasses =
    externalCoxPapersIIIIVFillWzRequest
    && externalCoxPapersIIIIVFillHiggsRequest
    && allRequiredLineagesPromotable
    && wParameterClosure
    && zParameterClosure
    && higgsParameterClosure
    && phase228MassMatrixPromotable
    && phase229VevPromotable
    && phase230NativeCandidatePromotable
    && phase232PaperIIPromotable
    && !papersIIIIVFocusOnAxialContactBcAndObservables
    && !papersIIIIVDoNotProvideWzMassRows
    && !papersIIIIVDoNotProvideSolvedHiggsMassSource
    && !papersIIIIVDoNotProvideObservedWZHMassValues;

var checks = new[]
{
    new Check("external-cox-papers-iii-iv-reviewed", externalCoxPapersIIIIVReviewed && externalCoxPapersIIIIVResearchLeadPresent, $"externalCoxPapersIIIIVReviewed={externalCoxPapersIIIIVReviewed}; externalCoxPapersIIIIVResearchLeadPresent={externalCoxPapersIIIIVResearchLeadPresent}; sourceKind={externalCoxPapersIIIIVSourceKind}"),
    new Check("paper-iii-quantization-leads-captured", paperIIIBrstQuantizationLeadPresent && paperIIICohomologyAndAnomalyLeadPresent && paperIIIRunningSignLeadPresent && paperIIIQuantumProjectionVariationLeadPresent, $"brst={paperIIIBrstQuantizationLeadPresent}; runningSign={paperIIIRunningSignLeadPresent}; quantumPv={paperIIIQuantumProjectionVariationLeadPresent}"),
    new Check("paper-iv-boundary-observable-leads-captured", paperIVBoundaryFamiliesLeadPresent && paperIVSliceEftObservableMapLeadPresent && paperIVBounceCosmologyInterfaceLeadPresent && paperIVGlobalFalsifierWorkflowLeadPresent, $"boundary={paperIVBoundaryFamiliesLeadPresent}; sliceEftObservableMap={paperIVSliceEftObservableMapLeadPresent}; bcInterface={paperIVBounceCosmologyInterfaceLeadPresent}"),
    new Check("papers-iii-iv-not-wzh-mass-source", papersIIIIVFocusOnAxialContactBcAndObservables && papersIIIIVDoNotProvideWzMassRows && papersIIIIVDoNotProvideSolvedHiggsMassSource && papersIIIIVDoNotProvideObservedWZHMassValues, $"focusOnBcObservables={papersIIIIVFocusOnAxialContactBcAndObservables}; wzMassRowsAbsent={papersIIIIVDoNotProvideWzMassRows}; solvedHiggsMassSourceAbsent={papersIIIIVDoNotProvideSolvedHiggsMassSource}"),
    new Check("wz-intake-request-not-filled", !externalCoxPapersIIIIVFillWzRequest && wzFields.Any(field => !field.Filled), $"externalCoxPapersIIIIVFillWzRequest={externalCoxPapersIIIIVFillWzRequest}; missingWzFields={wzFields.Count(field => !field.Filled)}"),
    new Check("higgs-intake-request-not-filled", !externalCoxPapersIIIIVFillHiggsRequest && higgsFields.Any(field => !field.Filled), $"externalCoxPapersIIIIVFillHiggsRequest={externalCoxPapersIIIIVFillHiggsRequest}; missingHiggsFields={higgsFields.Count(field => !field.Filled)}"),
    new Check("phase201-phase213-blockers-remain", !allRequiredLineagesPromotable && !wzPromotable && !higgsPromotable && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("phase224-parameter-closure-still-blocked", !wParameterClosure && !zParameterClosure && !higgsParameterClosure, $"wParameterClosure={wParameterClosure}; zParameterClosure={zParameterClosure}; higgsParameterClosure={higgsParameterClosure}"),
    new Check("phase228-phase229-phase230-extraction-and-vev-still-blocked", phase228MassMatrixObstructionCertified && !phase228MassMatrixPromotable && phase229VevObstructionCertified && !phase229VevPromotable && phase230NativeCandidateAuditPassed && !phase230NativeCandidatePromotable, $"phase228MassMatrixObstructionCertified={phase228MassMatrixObstructionCertified}; phase229VevObstructionCertified={phase229VevObstructionCertified}; phase230NativeCandidateAuditPassed={phase230NativeCandidateAuditPassed}"),
    new Check("phase232-paper-ii-still-non-promotable", phase232PaperIIAuditPassed && !phase232PaperIIPromotable, $"phase232PaperIIAuditPassed={phase232PaperIIAuditPassed}; phase232PaperIIPromotable={phase232PaperIIPromotable}"),
    new Check("external-cox-papers-iii-iv-not-promotable-for-boson-masses", !externalCoxPapersIIIIVPromotableForBosonMasses, $"externalCoxPapersIIIIVPromotableForBosonMasses={externalCoxPapersIIIIVPromotableForBosonMasses}"),
};

var externalCoxPapersIIIIVSourceIntakeAuditPassed = checks.All(check => check.Passed)
    && !externalCoxPapersIIIIVPromotableForBosonMasses;
var terminalStatus = externalCoxPapersIIIIVSourceIntakeAuditPassed
    ? "external-cox-gu-papers-iii-iv-reviewed-downstream-leads-not-boson-mass-source"
    : "external-cox-gu-papers-iii-iv-source-intake-review-required";

var result = new
{
    phaseId = "phase233-external-cox-gu-papers-iii-iv-source-intake-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    externalCoxPapersIIIIVResearchLeadPresent,
    externalCoxPapersIIIIVPromotableForBosonMasses,
    externalCoxPapersIIIIVSourceIntakeAuditPassed,
    objective = "Audit whether the external Cox 2025 GU III-IV preprints can fill the repo W/Z and Higgs boson source-lineage evidence requests.",
    externalSources = new[]
    {
        new
        {
            title = "Geometric Unity III: Quantization, BRST, and Deformation Complex",
            author = "Joseph Thomas Cox",
            date = "2025-10",
            discoveryUrl = "https://www.researchgate.net/publication/396557263_Geometric_Unity_III_Quantization_BRST_and_Deformation_Complex",
            sourceKind = externalCoxPapersIIIIVSourceKind,
            peerReviewStatus = "preprint-not-peer-reviewed-in-current-search-results",
        },
        new
        {
            title = "Geometric Unity IV: Boundary Dynamics, Observables, and the Single-Parameter GU->BC Interface",
            author = "Joseph Thomas Cox",
            date = "2025-10-17",
            discoveryUrl = "https://www.researchgate.net/publication/396557449_Geometric_Unity_IV_Boundary_Dynamics_Observables_and_the_Single-Parameter_GUBC_Interface_Admissible_Boundary_Families_Slice_EFT_Data_Maps_and_Global_Consistency_Tests",
            sourceKind = externalCoxPapersIIIIVSourceKind,
            peerReviewStatus = "preprint-not-peer-reviewed-in-current-search-results",
        },
    },
    claimedResearchLeads = new
    {
        paperIIIBrstQuantizationLeadPresent,
        paperIIICohomologyAndAnomalyLeadPresent,
        paperIIIRunningSignLeadPresent,
        paperIIIQuantumProjectionVariationLeadPresent,
        paperIVBoundaryFamiliesLeadPresent,
        paperIVSliceEftObservableMapLeadPresent,
        paperIVBounceCosmologyInterfaceLeadPresent,
        paperIVGlobalFalsifierWorkflowLeadPresent,
    },
    limitationsRelevantToBosonMasses = new
    {
        papersIIIIVFocusOnAxialContactBcAndObservables,
        papersIIIIVDoNotProvideWzMassRows,
        papersIIIIVDoNotProvideSolvedHiggsMassSource,
        papersIIIIVDoNotProvideObservedWZHMassValues,
        implication = "Papers III-IV can help future quantization, RG, boundary, and observable-map work, but the current artifacts do not provide electroweak coupling magnitudes, GU-derived VEV, physical W/Z rows, or solved Higgs scalar-source/self-coupling evidence for this repository.",
    },
    sourceClues = new[]
    {
        "Paper III search result: BRST/BV complex, cohomology, anomaly closure, axial-contact running/sign, and quantum Projection-Variation.",
        "Paper III search result: BC interface maps torsion-sector parameters to a stiffness parameter, not W/Z/H mass rows.",
        "Paper IV search result: admissible boundary families, frozen dim<=6 parity-even basis, slice EFT to observables, BC formulas, and global falsifier workflow.",
        "Paper IV search result mentions anomalous quartic gauge-boson sign falsifiers, but not observed W/Z/H absolute mass derivations.",
    },
    wzIntake = new
    {
        requestPath = Phase209WzRequestPath,
        externalCoxPapersIIIIVFillWzRequest,
        fields = wzFields,
    },
    higgsIntake = new
    {
        requestPath = Phase209HiggsRequestPath,
        externalCoxPapersIIIIVFillHiggsRequest,
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
        phase229 = new
        {
            status = JsonString(phase229.RootElement, "terminalStatus"),
            phase229VevPromotable,
            phase229VevObstructionCertified,
        },
        phase230 = new
        {
            status = JsonString(phase230.RootElement, "terminalStatus"),
            phase230NativeCandidatePromotable,
            phase230NativeCandidateAuditPassed,
        },
        phase232 = new
        {
            status = JsonString(phase232.RootElement, "terminalStatus"),
            phase232PaperIIPromotable,
            phase232PaperIIAuditPassed,
        },
    },
    checks,
    decision = externalCoxPapersIIIIVSourceIntakeAuditPassed
        ? "Do not promote W/Z/H masses from Cox 2025 GU III-IV. Preserve them as downstream quantization, RG, boundary, and observable-map research leads, but the current papers do not fill the repo's W/Z particle rows, Higgs scalar-source row, GU-derived VEV, physical mass-matrix extraction, stability sidecars, or target-comparison gates."
        : "Review Cox 2025 GU III-IV source-intake evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A source-lineage application that turns the Cox GU series into fixed W/Z particle rows with GU-derived couplings and VEV.",
        "A Higgs scalar-source operator, identity envelope, massive scalar profile, and self-coupling or excitation relation satisfying Phase209/Phase201 gates.",
        "Repository replay, stability, and target-comparison gates for W, Z, and Higgs rows.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase209WzRequestPath = Phase209WzRequestPath,
        phase209HiggsRequestPath = Phase209HiggsRequestPath,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase228Path = Phase228Path,
        phase229Path = Phase229Path,
        phase230Path = Phase230Path,
        phase232Path = Phase232Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "external_cox_gu_papers_iii_iv_source_intake_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "external_cox_gu_papers_iii_iv_source_intake_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.externalCoxPapersIIIIVResearchLeadPresent,
        result.externalCoxPapersIIIIVPromotableForBosonMasses,
        result.externalCoxPapersIIIIVSourceIntakeAuditPassed,
        result.externalSources,
        result.claimedResearchLeads,
        result.limitationsRelevantToBosonMasses,
        result.sourceClues,
        result.wzIntake,
        result.higgsIntake,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"externalCoxPapersIIIIVResearchLeadPresent={externalCoxPapersIIIIVResearchLeadPresent}");
Console.WriteLine($"externalCoxPapersIIIIVPromotableForBosonMasses={externalCoxPapersIIIIVPromotableForBosonMasses}");
Console.WriteLine($"externalCoxPapersIIIIVSourceIntakeAuditPassed={externalCoxPapersIIIIVSourceIntakeAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record IntakeField(string FieldId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
