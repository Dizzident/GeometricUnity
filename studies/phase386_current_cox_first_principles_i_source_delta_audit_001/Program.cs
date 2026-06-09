using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase386_current_cox_first_principles_i_source_delta_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase231Path = "studies/phase231_external_cox_gu_paper_i_source_intake_audit_001/output/external_cox_gu_paper_i_source_intake_audit_summary.json";
const string Phase242Path = "studies/phase242_post_p241_external_lead_consolidation_audit_001/output/post_p241_external_lead_consolidation_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase385Path = "studies/phase385_observed_electroweak_namespace_map_intake_audit_001/output/observed_electroweak_namespace_map_intake_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;
const int ExpectedHiggsMissingFieldCount = 14;

var outputDir = Environment.GetEnvironmentVariable("PHASE386_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase231 = JsonDocument.Parse(File.ReadAllText(Phase231Path));
using var phase242 = JsonDocument.Parse(File.ReadAllText(Phase242Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase385 = JsonDocument.Parse(File.ReadAllText(Phase385Path));

var phase201ContractMaterialized = JsonBool(phase201.RootElement, "intakeContractMaterialized") is true;
var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var phase256ContractMaterialized = JsonBool(phase256.RootElement, "contractMaterialized") is true
    && JsonBool(phase256.RootElement, "observedFieldExtractionIntakeContractPassed") is true;
var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ObservedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase385NamespaceMapAuditPassed = JsonBool(phase385.RootElement, "observedElectroweakNamespaceMapIntakeAuditPassed") is true;
var phase385NoNamespaceMapCandidate = JsonBool(phase385.RootElement, "noCandidateProvidesGuNativeObservedElectroweakNamespaceMap") is true;
var phase385NoPhase256Candidate = JsonBool(phase385.RootElement, "noCandidateCanFillPhase256ObservedFieldExtractionContract") is true;
var phase385NoPhase201Candidate = JsonBool(phase385.RootElement, "noCandidateCanFillPhase201WzContract") is true
    && JsonBool(phase385.RootElement, "noCandidateCanFillPhase201HiggsContract") is true;
var phase231ResearchLeadPresent = JsonBool(phase231.RootElement, "externalCoxPaperIResearchLeadPresent") is true;
var phase231PromotableForBosonMasses = JsonBool(phase231.RootElement, "externalCoxPaperIPromotableForBosonMasses") is true;
var phase231Doi = JsonNestedString(phase231.RootElement, "externalSource", "doi");
var phase242AnyExternalLeadPromotable = JsonBool(phase242.RootElement, "anyExternalLeadPromotableForBosonMasses") is true;
var phase242NewSourceLineageArtifactRequired = JsonBool(phase242.RootElement, "newSourceLineageArtifactRequired") is true;

var source = new
{
    refId = "COX-FIRST-PRINCIPLES-I-19800512",
    title = "Geometric Unity from First Principles I: Semidirect-Covariant Geometry on Y and Induced Dynamics on X",
    author = "Joseph Thomas Cox",
    date = "April 2026",
    doi = "10.5281/zenodo.19800512",
    discoveryUrl = "https://www.researchgate.net/publication/404210034_Geometric_Unity_from_First_Principles_I_Semidirect-Covariant_Geometry_on_Y_and_Induced_Dynamics_on_X_Shiab_Pairings_Completed_Curvature_and_Torsion_and_Projection-Variation_with_Boundary_Control",
    sourceKind = "third-party-preprint-non-official-gu",
    peerReviewStatus = "preprint-not-peer-reviewed-in-current-search-results",
    currentSearchEvidence = new[]
    {
        "ResearchGate lists DOI 10.5281/zenodo.19800512 and uploaded author content in April 2026.",
        "The abstract describes a classical semidirect-covariant scaffold on ambient Y with observation slice X.",
        "The abstract says the purpose is a rigorous classical scaffold for later matter embedding, anomaly closure, BRST/BV quantization, renormalization, boundary dynamics, and observable tests."
    }
};

var scaffoldClaims = new[]
{
    new SourceClaim("shiab-pairing-canonical-wedge-star-density", true, "Defines Shiab as a canonical wedge-star pairing on admissible same-degree bundle-valued forms."),
    new SourceClaim("completed-gauge-connection-a-minus-b", true, "Uses an affine-invariant completed gauge connection A - B."),
    new SourceClaim("completed-curvature-f-minus-dab-plus-b-wedge-b", true, "Uses completed curvature F(A - B) = F - D_A B + B wedge B."),
    new SourceClaim("augmented-torsion-completed-spin-connection", true, "Uses augmented torsion from the completed spin connection."),
    new SourceClaim("fixed-embedding-projection-variation", true, "Proves a fixed-embedding Projection-Variation theorem under boundary and corner hypotheses."),
    new SourceClaim("completed-slice-levi-civita-reduction", true, "Distinguishes completed-slice Levi-Civita reduction from raw ambient connection restrictions."),
    new SourceClaim("quadratic-slice-kinetic-sectors", true, "Reports Einstein-Hilbert, Yang-Mills, and Dirac kinetic sectors in the quadratic slice theory."),
    new SourceClaim("no-irreducible-bilinear-graviton-gauge-mixing", true, "Reports no irreducible bilinear graviton-gauge mixing in that quadratic slice scaffold."),
    new SourceClaim("fixed-sign-axial-contact", true, "Records a fixed-sign local axial contact after eliminating nondynamical axial torsion."),
};

var missingBosonArtifacts = new[]
{
    new SourceClaim("matter-embedding-complete", false, "The current paper explicitly leaves matter embedding to later work."),
    new SourceClaim("anomaly-closure-complete", false, "The current paper explicitly leaves anomaly closure to later work."),
    new SourceClaim("brst-bv-quantization-complete", false, "The current paper explicitly leaves BRST/BV quantization to later work."),
    new SourceClaim("renormalization-complete", false, "The current paper explicitly leaves renormalization to later work."),
    new SourceClaim("observable-tests-complete", false, "The current paper frames observable tests as later work."),
    new SourceClaim("observed-electroweak-namespace-map", false, "No GU-native photon/W/Z/H namespace map is supplied."),
    new SourceClaim("weak-mixing-or-coupling-source", false, "No target-independent low-energy weak angle or coupling lineage is supplied."),
    new SourceClaim("electroweak-vev-source", false, "No target-independent electroweak VEV or branch normalization source is supplied."),
    new SourceClaim("w-boson-source-row", false, "No W particle-specific source row is supplied."),
    new SourceClaim("z-boson-source-row", false, "No Z particle-specific source row is supplied."),
    new SourceClaim("higgs-scalar-source-row", false, "No solved Higgs scalar-source row is supplied."),
    new SourceClaim("gev-unit-normalization", false, "No physical GeV normalization lineage is supplied."),
};

var currentCoxFirstPrinciplesIResearchLeadPresent = true;
var currentCoxFirstPrinciplesIScaffoldDeltaPresent =
    scaffoldClaims.All(claim => claim.Present)
    && phase231ResearchLeadPresent
    && phase231Doi != "10.5281/zenodo.19800512";
var currentCoxFirstPrinciplesIFillsPhase256ObservedFieldExtractionContract = false;
var currentCoxFirstPrinciplesIFillsWzContract = false;
var currentCoxFirstPrinciplesIFillsHiggsContract = false;
var currentCoxFirstPrinciplesIPromotableForBosonMasses = false;
var currentCoxFirstPrinciplesICompletesObservedElectroweakNamespaceMap = false;
var currentCoxFirstPrinciplesICompletesBosonPredictions = false;
var sourceContractApplicationAllowed = false;
var canFillPhase201WzContract = false;
var canFillPhase201HiggsContract = false;
var canFillPhase256ObservedFieldExtractionContract = false;
var routePromotesWzMasses = false;
var routePromotesHiggsMass = false;
var routeCompletesBosonPredictions = false;
var phase201TemplateMutated = false;
var fieldsAppliedToPhase201TemplateCount = 0;
var acceptedContractFieldCount = 0;
var blockedContractFieldCount = phase213WzMissingFieldCount;
var phase201FieldsDefensiblyFilled = Array.Empty<string>();

var targetBlindConstruction = true;
var physicalTargetsConsultedForConstruction = false;
var applicationSubjectKind = "current-cox-first-principles-i-source-delta-for-phase201-phase256-phase385";
var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    source.refId,
    source.doi,
    source.date,
    applicationSubjectKind,
    scaffoldClaims = scaffoldClaims.Select(claim => new { claim.ClaimId, claim.Present }),
    missingBosonArtifacts = missingBosonArtifacts.Select(claim => new { claim.ClaimId, claim.Present }),
}, options));

var checks = new[]
{
    new Check(
        "current-cox-first-principles-i-source-materialized",
        currentCoxFirstPrinciplesIResearchLeadPresent
            && source.doi == "10.5281/zenodo.19800512"
            && source.date == "April 2026",
        $"doi={source.doi}; date={source.date}; sourceKind={source.sourceKind}"),
    new Check(
        "scaffold-delta-over-phase231-captured",
        currentCoxFirstPrinciplesIScaffoldDeltaPresent
            && !phase231PromotableForBosonMasses,
        $"phase231ResearchLeadPresent={phase231ResearchLeadPresent}; phase231Doi={phase231Doi}; scaffoldClaimCount={scaffoldClaims.Count(claim => claim.Present)}; phase231Promotable={phase231PromotableForBosonMasses}"),
    new Check(
        "explicit-scope-limitations-preserve-noncompletion",
        missingBosonArtifacts.All(claim => !claim.Present)
            && !currentCoxFirstPrinciplesICompletesBosonPredictions,
        $"missingBosonArtifactCount={missingBosonArtifacts.Count(claim => !claim.Present)}; completesBosonPredictions={currentCoxFirstPrinciplesICompletesBosonPredictions}"),
    new Check(
        "observed-electroweak-namespace-map-still-missing",
        phase385NamespaceMapAuditPassed
            && phase385NoNamespaceMapCandidate
            && phase385NoPhase256Candidate
            && phase385NoPhase201Candidate
            && !currentCoxFirstPrinciplesICompletesObservedElectroweakNamespaceMap,
        $"phase385NamespaceMapAuditPassed={phase385NamespaceMapAuditPassed}; phase385NoNamespaceMapCandidate={phase385NoNamespaceMapCandidate}; completesObservedNamespaceMap={currentCoxFirstPrinciplesICompletesObservedElectroweakNamespaceMap}"),
    new Check(
        "phase201-and-phase256-contracts-remain-unfilled",
        phase201ContractMaterialized
            && !phase201AllRequiredLineagesPromotable
            && phase213WzMissingFieldCount == ExpectedWzMissingFieldCount
            && phase213HiggsMissingFieldCount == ExpectedHiggsMissingFieldCount
            && phase256ContractMaterialized
            && phase256RequiredFieldCount == 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ObservedFieldExtractionContractPromotable
            && !currentCoxFirstPrinciplesIFillsWzContract
            && !currentCoxFirstPrinciplesIFillsHiggsContract
            && !currentCoxFirstPrinciplesIFillsPhase256ObservedFieldExtractionContract,
        $"phase201AllRequiredLineagesPromotable={phase201AllRequiredLineagesPromotable}; wzMissing={phase213WzMissingFieldCount}; higgsMissing={phase213HiggsMissingFieldCount}; phase256Filled={phase256FilledRequiredFieldCount}"),
    new Check(
        "post-p242-external-lead-status-unchanged",
        !phase242AnyExternalLeadPromotable
            && phase242NewSourceLineageArtifactRequired
            && !currentCoxFirstPrinciplesIPromotableForBosonMasses,
        $"phase242AnyExternalLeadPromotable={phase242AnyExternalLeadPromotable}; phase242NewSourceLineageArtifactRequired={phase242NewSourceLineageArtifactRequired}; currentPromotable={currentCoxFirstPrinciplesIPromotableForBosonMasses}"),
    new Check(
        "source-contracts-not-mutated-or-promoted",
        !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && phase201FieldsDefensiblyFilled.Length == 0,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; acceptedContractFieldCount={acceptedContractFieldCount}; phase201TemplateMutated={phase201TemplateMutated}"),
};

var currentCoxFirstPrinciplesISourceDeltaAuditPassed = checks.All(check => check.Passed)
    && currentCoxFirstPrinciplesIResearchLeadPresent
    && currentCoxFirstPrinciplesIScaffoldDeltaPresent
    && !currentCoxFirstPrinciplesIPromotableForBosonMasses
    && !currentCoxFirstPrinciplesICompletesBosonPredictions;
var terminalStatus = currentCoxFirstPrinciplesISourceDeltaAuditPassed
    ? "current-cox-first-principles-i-source-delta-scaffold-only-no-boson-prediction"
    : "current-cox-first-principles-i-source-delta-review-required";

var result = new
{
    phaseId = "phase386-current-cox-first-principles-i-source-delta-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    currentCoxFirstPrinciplesISourceDeltaAuditPassed,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind,
    currentCoxFirstPrinciplesIResearchLeadPresent,
    currentCoxFirstPrinciplesIScaffoldDeltaPresent,
    currentCoxFirstPrinciplesIPromotableForBosonMasses,
    currentCoxFirstPrinciplesIFillsPhase256ObservedFieldExtractionContract,
    currentCoxFirstPrinciplesIFillsWzContract,
    currentCoxFirstPrinciplesIFillsHiggsContract,
    currentCoxFirstPrinciplesICompletesObservedElectroweakNamespaceMap,
    currentCoxFirstPrinciplesICompletesBosonPredictions,
    source,
    sourceDeltaRelativeToPhase231 = new
    {
        phase231Path = Phase231Path,
        phase231Doi,
        currentDoi = source.doi,
        phase231ResearchLeadPresent,
        phase231PromotableForBosonMasses,
        scaffoldDeltaMaterialized = currentCoxFirstPrinciplesIScaffoldDeltaPresent,
        bosonPromotionDeltaFound = currentCoxFirstPrinciplesIPromotableForBosonMasses,
    },
    scaffoldClaims,
    missingBosonArtifacts,
    currentContractEvidence = new
    {
        phase201Path = Phase201Path,
        phase201ContractMaterialized,
        phase201AllRequiredLineagesPromotable,
        phase213Path = Phase213Path,
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
        phase256Path = Phase256Path,
        phase256ContractMaterialized,
        phase256RequiredFieldCount,
        phase256FilledRequiredFieldCount,
        phase256ObservedFieldExtractionContractPromotable,
        phase385Path = Phase385Path,
        phase385NamespaceMapAuditPassed,
        phase385NoNamespaceMapCandidate,
        phase385NoPhase256Candidate,
        phase385NoPhase201Candidate,
        phase242Path = Phase242Path,
        phase242AnyExternalLeadPromotable,
        phase242NewSourceLineageArtifactRequired,
    },
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    blockedContractFieldCount,
    phase201FieldsDefensiblyFilled,
    checks,
    decision = currentCoxFirstPrinciplesISourceDeltaAuditPassed
        ? "Preserve the 2026 Cox first-principles paper as a stronger classical scaffold lead, but do not promote W/Z/H masses from it. It supplies no observed electroweak namespace map, W/Z source rows, Higgs scalar-source row, target-independent electroweak scale/coupling lineage, or GeV normalization."
        : "Review the current Cox first-principles source delta before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A GU-native observed electroweak namespace map with photon/W/Z/H rows.",
        "A target-independent W/Z bridge-source theorem with separate W and Z source rows and stability sidecars.",
        "A solved Higgs scalar-source/operator with potential or excitation lineage.",
        "Target-independent electroweak scale/coupling and GeV normalization lineages.",
    },
};

File.WriteAllText(Path.Combine(outputDir, "current_cox_first_principles_i_source_delta_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "current_cox_first_principles_i_source_delta_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.currentCoxFirstPrinciplesISourceDeltaAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        result.currentCoxFirstPrinciplesIResearchLeadPresent,
        result.currentCoxFirstPrinciplesIScaffoldDeltaPresent,
        result.currentCoxFirstPrinciplesIPromotableForBosonMasses,
        result.currentCoxFirstPrinciplesIFillsPhase256ObservedFieldExtractionContract,
        result.currentCoxFirstPrinciplesIFillsWzContract,
        result.currentCoxFirstPrinciplesIFillsHiggsContract,
        result.currentCoxFirstPrinciplesICompletesObservedElectroweakNamespaceMap,
        result.currentCoxFirstPrinciplesICompletesBosonPredictions,
        result.source,
        result.sourceDeltaRelativeToPhase231,
        result.scaffoldClaims,
        result.missingBosonArtifacts,
        result.currentContractEvidence,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.acceptedContractFieldCount,
        result.blockedContractFieldCount,
        result.phase201FieldsDefensiblyFilled,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"currentCoxFirstPrinciplesISourceDeltaAuditPassed={currentCoxFirstPrinciplesISourceDeltaAuditPassed}");
Console.WriteLine($"currentCoxFirstPrinciplesIScaffoldDeltaPresent={currentCoxFirstPrinciplesIScaffoldDeltaPresent}");
Console.WriteLine($"currentCoxFirstPrinciplesIPromotableForBosonMasses={currentCoxFirstPrinciplesIPromotableForBosonMasses}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static string? JsonNestedString(JsonElement element, string propertyName, string nestedPropertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Object
        ? JsonString(property, nestedPropertyName)
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string Sha256Hex(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record SourceClaim(string ClaimId, bool Present, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
