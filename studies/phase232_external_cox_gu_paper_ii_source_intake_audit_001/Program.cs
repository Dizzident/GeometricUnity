using System.Text.Json;

const string DefaultOutputDir = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase209WzRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/wz_absolute_source_lineage_evidence_request.json";
const string Phase209HiggsRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/higgs_scalar_source_lineage_evidence_request.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase230Path = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output/native_gu_vacuum_hessian_candidate_audit_summary.json";
const string Phase231Path = "studies/phase231_external_cox_gu_paper_i_source_intake_audit_001/output/external_cox_gu_paper_i_source_intake_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE232_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase209Wz = JsonDocument.Parse(File.ReadAllText(Phase209WzRequestPath));
using var phase209Higgs = JsonDocument.Parse(File.ReadAllText(Phase209HiggsRequestPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase230 = JsonDocument.Parse(File.ReadAllText(Phase230Path));
using var phase231 = JsonDocument.Parse(File.ReadAllText(Phase231Path));

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
var phase231PaperIPromotable = JsonBool(phase231.RootElement, "externalCoxPaperIPromotableForBosonMasses") is true;
var phase231PaperIAuditPassed = JsonBool(phase231.RootElement, "externalCoxPaperISourceIntakeAuditPassed") is true;

var externalCoxPaperIIReviewed = true;
var externalCoxPaperIIResearchLeadPresent = true;
var externalCoxPaperIISourceKind = "third-party-preprint-non-official-gu";
var paperIIMatterSymmetryLeadPresent = true;
var paperIIPatiSalamEmbeddingLeadPresent = true;
var paperIIGaugeNormalizationLeadPresent = true;
var paperIIGaugeMassMatrixLeadPresent = true;
var paperIIGeometrySourcedScalarLeadPresent = true;
var paperIIYukawaTextureLeadPresent = true;
var paperIIUsesSymbolicGaugeAndBreakingParameters = true;
var paperIITreatsScalarsConservatively = true;
var paperIIDefersQuantizationRunningToPaperIII = true;
var paperIIDefersDetailedPhenomenologyToPaperIV = true;
var paperIIDoesNotProvideObservedWZHMassValues = true;

var wzFields = new[]
{
    new IntakeField("externalTargetValuesUsed=false", true, "The paper is treated as a source lead, not as a target-value fit."),
    new IntakeField("theoremOrDerivationId", false, "Paper II records gauge-sector mass-matrix and normalization leads, but not a repository W/Z absolute-mass bridge theorem with fixed GU-derived couplings and VEV."),
    new IntakeField("sourceLineageId", false, "No local Phase201 W/Z sourceLineageId is filled from this paper."),
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
    new IntakeField("scalarSourceOperatorId", false, "Geometry-sourced scalar/Higgs-like moduli are a lead, but no solved Higgs scalar-source operator id is supplied."),
    new IntakeField("higgsIdentityEnvelopeId", false, "No Higgs identity envelope is supplied."),
    new IntakeField("massiveScalarProfileId", false, "No physical massive scalar profile is supplied."),
    new IntakeField("potentialOrSelfCouplingSourceId-or-excitationRelationId", false, "Yukawa texture leads do not supply a Higgs quartic/self-coupling source or mass-excitation relation."),
    new IntakeField("stabilitySidecars.branch=true", false, "No local branch-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.refinement=true", false, "No local refinement-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.environment=true", false, "No local environment-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.representation=true", false, "No local representation-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("stabilitySidecars.coupling=true", false, "No local coupling-stability sidecar is supplied for a Higgs prediction row."),
    new IntakeField("predictionRow.sourceRowId", false, "No Higgs prediction source row is supplied."),
    new IntakeField("predictionRow.targetComparisonGatePassed=true", false, "No post-construction Higgs target comparison row is supplied."),
    new IntakeField("predictionRow.derivationId", false, "No Higgs mass derivation id is supplied."),
};

var externalCoxPaperIIFillsWzRequest = wzFields.All(field => field.Filled);
var externalCoxPaperIIFillsHiggsRequest = higgsFields.All(field => field.Filled);
var externalCoxPaperIIPromotableForBosonMasses =
    externalCoxPaperIIFillsWzRequest
    && externalCoxPaperIIFillsHiggsRequest
    && allRequiredLineagesPromotable
    && wParameterClosure
    && zParameterClosure
    && higgsParameterClosure
    && phase228MassMatrixPromotable
    && phase229VevPromotable
    && phase230NativeCandidatePromotable
    && phase231PaperIPromotable
    && !paperIIUsesSymbolicGaugeAndBreakingParameters
    && !paperIITreatsScalarsConservatively
    && !paperIIDoesNotProvideObservedWZHMassValues;

var checks = new[]
{
    new Check("external-cox-paper-ii-reviewed", externalCoxPaperIIReviewed && externalCoxPaperIIResearchLeadPresent, $"externalCoxPaperIIReviewed={externalCoxPaperIIReviewed}; externalCoxPaperIIResearchLeadPresent={externalCoxPaperIIResearchLeadPresent}; sourceKind={externalCoxPaperIISourceKind}"),
    new Check("matter-symmetry-gauge-scalar-leads-captured", paperIIMatterSymmetryLeadPresent && paperIIPatiSalamEmbeddingLeadPresent && paperIIGaugeNormalizationLeadPresent && paperIIGaugeMassMatrixLeadPresent && paperIIGeometrySourcedScalarLeadPresent && paperIIYukawaTextureLeadPresent, $"patiSalam={paperIIPatiSalamEmbeddingLeadPresent}; gaugeMassMatrix={paperIIGaugeMassMatrixLeadPresent}; geometryScalar={paperIIGeometrySourcedScalarLeadPresent}"),
    new Check("paper-ii-needed-absolute-mass-parameters-not-fixed", paperIIUsesSymbolicGaugeAndBreakingParameters && paperIITreatsScalarsConservatively && paperIIDoesNotProvideObservedWZHMassValues, $"symbolicGaugeAndBreakingParameters={paperIIUsesSymbolicGaugeAndBreakingParameters}; conservativeScalars={paperIITreatsScalarsConservatively}; observedWZHMassValuesAbsent={paperIIDoesNotProvideObservedWZHMassValues}"),
    new Check("paper-ii-defers-downstream-quantization-and-phenomenology", paperIIDefersQuantizationRunningToPaperIII && paperIIDefersDetailedPhenomenologyToPaperIV, $"quantizationRunningDeferred={paperIIDefersQuantizationRunningToPaperIII}; detailedPhenomenologyDeferred={paperIIDefersDetailedPhenomenologyToPaperIV}"),
    new Check("wz-intake-request-not-filled", !externalCoxPaperIIFillsWzRequest && wzFields.Any(field => !field.Filled), $"externalCoxPaperIIFillsWzRequest={externalCoxPaperIIFillsWzRequest}; missingWzFields={wzFields.Count(field => !field.Filled)}"),
    new Check("higgs-intake-request-not-filled", !externalCoxPaperIIFillsHiggsRequest && higgsFields.Any(field => !field.Filled), $"externalCoxPaperIIFillsHiggsRequest={externalCoxPaperIIFillsHiggsRequest}; missingHiggsFields={higgsFields.Count(field => !field.Filled)}"),
    new Check("phase201-phase213-blockers-remain", !allRequiredLineagesPromotable && !wzPromotable && !higgsPromotable && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("phase224-parameter-closure-still-blocked", !wParameterClosure && !zParameterClosure && !higgsParameterClosure, $"wParameterClosure={wParameterClosure}; zParameterClosure={zParameterClosure}; higgsParameterClosure={higgsParameterClosure}"),
    new Check("phase228-phase229-phase230-extraction-and-vev-still-blocked", phase228MassMatrixObstructionCertified && !phase228MassMatrixPromotable && phase229VevObstructionCertified && !phase229VevPromotable && phase230NativeCandidateAuditPassed && !phase230NativeCandidatePromotable, $"phase228MassMatrixObstructionCertified={phase228MassMatrixObstructionCertified}; phase229VevObstructionCertified={phase229VevObstructionCertified}; phase230NativeCandidateAuditPassed={phase230NativeCandidateAuditPassed}"),
    new Check("phase231-paper-i-still-non-promotable", phase231PaperIAuditPassed && !phase231PaperIPromotable, $"phase231PaperIAuditPassed={phase231PaperIAuditPassed}; phase231PaperIPromotable={phase231PaperIPromotable}"),
    new Check("external-cox-paper-ii-not-promotable-for-boson-masses", !externalCoxPaperIIPromotableForBosonMasses, $"externalCoxPaperIIPromotableForBosonMasses={externalCoxPaperIIPromotableForBosonMasses}"),
};

var externalCoxPaperIISourceIntakeAuditPassed = checks.All(check => check.Passed)
    && !externalCoxPaperIIPromotableForBosonMasses;
var terminalStatus = externalCoxPaperIISourceIntakeAuditPassed
    ? "external-cox-gu-paper-ii-reviewed-gauge-scalar-leads-not-boson-mass-source"
    : "external-cox-gu-paper-ii-source-intake-review-required";

var result = new
{
    phaseId = "phase232-external-cox-gu-paper-ii-source-intake-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    externalCoxPaperIIResearchLeadPresent,
    externalCoxPaperIIPromotableForBosonMasses,
    externalCoxPaperIISourceIntakeAuditPassed,
    objective = "Audit whether the external Cox 2025 GU II preprint can fill the repo W/Z and Higgs boson source-lineage evidence requests.",
    externalSource = new
    {
        title = "Geometric Unity II: Matter & Symmetry on the Observation Slice One-Family Factorization, Pati-Salam Embedding, Anomaly Closure, and Embryo Higgs/Yukawa Textures",
        author = "Joseph Thomas Cox",
        date = "2025-10-16",
        doi = "10.5281/zenodo.17373503",
        doiUrl = "https://doi.org/10.5281/zenodo.17373503",
        discoveryUrl = "https://www.researchgate.net/publication/396557260_Geometric_Unity_II_Matter_Symmetry_on_the_Observation_Slice_One-Family_Factorization_Pati-Salam_Embedding_Anomaly_Closure_and_Embryo_HiggsYukawa_Textures",
        sourceKind = externalCoxPaperIISourceKind,
        peerReviewStatus = "preprint-not-peer-reviewed-in-current-search-results",
        intakeInterpretation = "A useful external lead for slice matter, Pati-Salam embedding, gauge normalizations, gauge mass matrices, and geometry-sourced scalars, but not a completed W/Z/H mass source-lineage artifact for this repository.",
    },
    claimedResearchLeads = new
    {
        paperIIMatterSymmetryLeadPresent,
        paperIIPatiSalamEmbeddingLeadPresent,
        paperIIGaugeNormalizationLeadPresent,
        paperIIGaugeMassMatrixLeadPresent,
        paperIIGeometrySourcedScalarLeadPresent,
        paperIIYukawaTextureLeadPresent,
    },
    limitationsRelevantToBosonMasses = new
    {
        paperIIUsesSymbolicGaugeAndBreakingParameters,
        paperIITreatsScalarsConservatively,
        paperIIDefersQuantizationRunningToPaperIII,
        paperIIDefersDetailedPhenomenologyToPaperIV,
        paperIIDoesNotProvideObservedWZHMassValues,
        implication = "Paper II may help build a future source-lineage application, but the current artifact does not provide fixed electroweak coupling magnitudes, a GU-derived VEV, observed W/Z particle rows, a solved Higgs scalar source/self-coupling, or repository replay/target-comparison gates.",
    },
    sourceClues = new[]
    {
        "ResearchGate abstract: spinor split, 16-state block, Pati-Salam embedding, anomaly closure, and geometry-sourced scalar/Higgs-like moduli.",
        "ResearchGate abstract: minimal nonvanishing overlap integral generating schematic Yukawa textures.",
        "Executive recap: gauge sector masses, mixing, normalizations, hypercharge/electric-charge kernels, and appendices P-R.",
        "Outlook: quantization/running deferred to Paper III and detailed phenomenology deferred to Paper IV.",
    },
    wzIntake = new
    {
        requestPath = Phase209WzRequestPath,
        externalCoxPaperIIFillsWzRequest,
        fields = wzFields,
    },
    higgsIntake = new
    {
        requestPath = Phase209HiggsRequestPath,
        externalCoxPaperIIFillsHiggsRequest,
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
        phase231 = new
        {
            status = JsonString(phase231.RootElement, "terminalStatus"),
            phase231PaperIPromotable,
            phase231PaperIAuditPassed,
        },
    },
    checks,
    decision = externalCoxPaperIISourceIntakeAuditPassed
        ? "Do not promote W/Z/H masses from Cox 2025 GU II alone. Preserve it as an external gauge-sector/scalar-sector research lead, but the current paper keeps the needed absolute-mass parameters symbolic or downstream and does not fill the repo's W/Z particle rows, Higgs scalar-source row, GU-derived VEV, physical mass-matrix extraction, stability sidecars, or target-comparison gates."
        : "Review Cox 2025 GU II source-intake evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "If using Cox GU II, materialize a repository source-lineage application that turns its gauge-sector mass matrices into fixed W/Z particle rows with GU-derived couplings and VEV.",
        "Supply a Higgs scalar-source operator, identity envelope, massive scalar profile, and self-coupling or excitation relation that satisfy Phase209/Phase201 gates.",
        "Attach branch/refinement/environment/representation/coupling stability sidecars and repository replay/target-comparison gates.",
        "Rerun Phase210, Phase213, Phase224, Phase228, Phase229, Phase230, P101, and P202 before any promotion.",
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
        phase231Path = Phase231Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "external_cox_gu_paper_ii_source_intake_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "external_cox_gu_paper_ii_source_intake_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.externalCoxPaperIIResearchLeadPresent,
        result.externalCoxPaperIIPromotableForBosonMasses,
        result.externalCoxPaperIISourceIntakeAuditPassed,
        result.externalSource,
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
Console.WriteLine($"externalCoxPaperIIResearchLeadPresent={externalCoxPaperIIResearchLeadPresent}");
Console.WriteLine($"externalCoxPaperIIPromotableForBosonMasses={externalCoxPaperIIPromotableForBosonMasses}");
Console.WriteLine($"externalCoxPaperIISourceIntakeAuditPassed={externalCoxPaperIISourceIntakeAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record IntakeField(string FieldId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
