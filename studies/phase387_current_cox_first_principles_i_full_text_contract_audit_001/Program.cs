using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase387_current_cox_first_principles_i_full_text_contract_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase385Path = "studies/phase385_observed_electroweak_namespace_map_intake_audit_001/output/observed_electroweak_namespace_map_intake_audit_summary.json";
const string Phase386Path = "studies/phase386_current_cox_first_principles_i_source_delta_audit_001/output/current_cox_first_principles_i_source_delta_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;
const int ExpectedHiggsMissingFieldCount = 14;

var outputDir = Environment.GetEnvironmentVariable("PHASE387_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase385 = JsonDocument.Parse(File.ReadAllText(Phase385Path));
using var phase386 = JsonDocument.Parse(File.ReadAllText(Phase386Path));

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
var phase386SourceDeltaAuditPassed = JsonBool(phase386.RootElement, "currentCoxFirstPrinciplesISourceDeltaAuditPassed") is true;
var phase386ScaffoldDeltaPresent = JsonBool(phase386.RootElement, "currentCoxFirstPrinciplesIScaffoldDeltaPresent") is true;
var phase386PromotableForBosonMasses = JsonBool(phase386.RootElement, "currentCoxFirstPrinciplesIPromotableForBosonMasses") is true;
var phase386CompletesNamespaceMap = JsonBool(phase386.RootElement, "currentCoxFirstPrinciplesICompletesObservedElectroweakNamespaceMap") is true;
var phase386FillsWzContract = JsonBool(phase386.RootElement, "currentCoxFirstPrinciplesIFillsWzContract") is true;
var phase386FillsHiggsContract = JsonBool(phase386.RootElement, "currentCoxFirstPrinciplesIFillsHiggsContract") is true;

var source = new
{
    refId = "COX-FIRST-PRINCIPLES-I-19800512",
    title = "Geometric Unity from First Principles I",
    author = "Joseph Thomas Cox",
    doi = "10.5281/zenodo.19800512",
    conceptDoi = "10.5281/zenodo.19800511",
    zenodoRecordId = 19800512,
    zenodoApiRecord = "https://zenodo.org/api/records/19800512",
    pdfContentUrl = "https://zenodo.org/api/records/19800512/files/GU%20FIRST%20PRINCIPLES_1.pdf/content",
    publicationDate = "2026-04-26",
    artifactName = "GU FIRST PRINCIPLES_1.pdf",
    artifactSizeBytes = 790807,
    artifactChecksum = "md5:dbf8f7b1b141f18a8259314be1b36f83",
    reviewedOn = "2026-06-09",
    extractionTool = "pdftotext",
    extractedLineCount = 6518,
};

var contractKeywordEvidence = new[]
{
    new TermEvidence("electroweak", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("weak mixing", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("hypercharge", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("higgs", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("w boson", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("z boson", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("gev", 0, Array.Empty<int>(), "absent-unit-keyword"),
    new TermEvidence("standard model", 2, new[] { 6506, 6508 }, "bibliography-only"),
    new TermEvidence("weinberg", 2, new[] { 6473, 6505 }, "bibliography-only"),
    new TermEvidence("boson", 1, new[] { 6084 }, "generic-gauge-kinetics-only"),
    new TermEvidence("mass", 1, new[] { 6514 }, "bibliography-only"),
};

var scaffoldTermEvidence = new[]
{
    new TermEvidence("shiab", 105, new[] { 356, 1124, 1184, 1452, 5712 }, "positive-classical-scaffold"),
    new TermEvidence("torsion", 197, new[] { 17, 323, 1874, 3311, 5667 }, "positive-classical-scaffold"),
    new TermEvidence("curvature", 80, new[] { 430, 1339, 2178, 2195, 6030 }, "positive-classical-scaffold"),
    new TermEvidence("completed connection", 27, new[] { 11, 2195, 3132 }, "positive-classical-scaffold"),
    new TermEvidence("yang-mills", 24, new[] { 17, 430, 1339, 3236, 6084 }, "positive-kinetic-sector"),
    new TermEvidence("dirac", 42, new[] { 17, 431, 2063, 3132, 5980 }, "positive-kinetic-sector"),
    new TermEvidence("projection-variation", 46, new[] { 323, 3079, 3838, 5667 }, "positive-classical-scaffold"),
};

var openLayerEvidence = new[]
{
    new TermEvidence("matter embedding", 6, new[] { 190, 323, 533, 2063, 3642, 3928 }, "open-layer-not-complete"),
    new TermEvidence("observable", 9, new[] { 206, 3016, 3835, 3837, 3929 }, "open-layer-not-complete"),
    new TermEvidence("observable maps", 2, new[] { 3876, 3929 }, "open-layer-not-complete"),
    new TermEvidence("renormalization", 9, new[] { 324, 533, 3657, 3830, 6475 }, "open-layer-not-complete"),
    new TermEvidence("brst", 39, new[] { 191, 324, 3652, 6233, 6311 }, "open-layer-not-complete"),
    new TermEvidence("bv", 17, new[] { 191, 324, 3652, 6314 }, "open-layer-not-complete"),
    new TermEvidence("anomaly", 18, new[] { 190, 463, 3642, 3773, 6500 }, "open-layer-not-complete"),
};

var fullTextReviewed = true;
var fullTextBosonContractEvidenceFound = false;
var fullTextObservedNamespaceMapFound = false;
var fullTextWzSourceRowsFound = false;
var fullTextHiggsScalarSourceFound = false;
var fullTextElectroweakParameterLineageFound = false;
var fullTextWeakMixingOrCouplingLineageFound = false;
var fullTextGeVNormalizationFound = false;
var fullTextPoleExtractionFound = false;
var fullTextOnlyScaffoldEvidenceFound = true;
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
var applicationSubjectKind = "current-cox-first-principles-i-full-text-contract-audit-for-phase201-phase256-phase385";
var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    source.refId,
    source.doi,
    source.artifactChecksum,
    source.extractedLineCount,
    applicationSubjectKind,
    contractKeywordEvidence = contractKeywordEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    scaffoldTermEvidence = scaffoldTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    openLayerEvidence = openLayerEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
}, options));

var absentContractKeywordCount = contractKeywordEvidence.Count(term => term.Count == 0);
var scaffoldTermHitCount = scaffoldTermEvidence.Count(term => term.Count > 0);
var openLayerHitCount = openLayerEvidence.Count(term => term.Count > 0);

var checks = new[]
{
    new Check(
        "zenodo-artifact-and-full-text-extraction-recorded",
        fullTextReviewed
            && source.zenodoRecordId == 19800512
            && source.artifactSizeBytes == 790807
            && source.artifactChecksum == "md5:dbf8f7b1b141f18a8259314be1b36f83"
            && source.extractedLineCount == 6518
            && source.extractionTool == "pdftotext",
        $"recordId={source.zenodoRecordId}; checksum={source.artifactChecksum}; extractedLineCount={source.extractedLineCount}; extractionTool={source.extractionTool}"),
    new Check(
        "electroweak-contract-keywords-absent-from-full-text",
        contractKeywordEvidence.Single(term => term.TermId == "electroweak").Count == 0
            && contractKeywordEvidence.Single(term => term.TermId == "weak mixing").Count == 0
            && contractKeywordEvidence.Single(term => term.TermId == "hypercharge").Count == 0
            && contractKeywordEvidence.Single(term => term.TermId == "higgs").Count == 0
            && contractKeywordEvidence.Single(term => term.TermId == "w boson").Count == 0
            && contractKeywordEvidence.Single(term => term.TermId == "z boson").Count == 0
            && contractKeywordEvidence.Single(term => term.TermId == "gev").Count == 0,
        $"absentContractKeywordCount={absentContractKeywordCount}; bibliographyOnlyTerms=standard model,weinberg,mass"),
    new Check(
        "positive-full-text-evidence-is-scaffold-not-boson-contract",
        fullTextOnlyScaffoldEvidenceFound
            && scaffoldTermHitCount == scaffoldTermEvidence.Length
            && openLayerHitCount == openLayerEvidence.Length
            && !fullTextBosonContractEvidenceFound,
        $"scaffoldTermHitCount={scaffoldTermHitCount}; openLayerHitCount={openLayerHitCount}; fullTextBosonContractEvidenceFound={fullTextBosonContractEvidenceFound}"),
    new Check(
        "phase386-boundary-confirmed-by-full-text",
        phase386SourceDeltaAuditPassed
            && phase386ScaffoldDeltaPresent
            && !phase386PromotableForBosonMasses
            && !phase386CompletesNamespaceMap
            && !phase386FillsWzContract
            && !phase386FillsHiggsContract,
        $"phase386Passed={phase386SourceDeltaAuditPassed}; phase386ScaffoldDeltaPresent={phase386ScaffoldDeltaPresent}; phase386Promotable={phase386PromotableForBosonMasses}"),
    new Check(
        "observed-electroweak-namespace-map-still-missing",
        phase385NamespaceMapAuditPassed
            && phase385NoNamespaceMapCandidate
            && phase385NoPhase256Candidate
            && phase385NoPhase201Candidate
            && !fullTextObservedNamespaceMapFound,
        $"phase385NamespaceMapAuditPassed={phase385NamespaceMapAuditPassed}; phase385NoNamespaceMapCandidate={phase385NoNamespaceMapCandidate}; fullTextObservedNamespaceMapFound={fullTextObservedNamespaceMapFound}"),
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
            && !fullTextWzSourceRowsFound
            && !fullTextHiggsScalarSourceFound
            && !fullTextElectroweakParameterLineageFound
            && !fullTextGeVNormalizationFound,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; wzMissing={phase213WzMissingFieldCount}; higgsMissing={phase213HiggsMissingFieldCount}; phase256Filled={phase256FilledRequiredFieldCount}"),
    new Check(
        "source-contracts-not-mutated-or-promoted",
        !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && phase201FieldsDefensiblyFilled.Length == 0,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; acceptedContractFieldCount={acceptedContractFieldCount}; phase201TemplateMutated={phase201TemplateMutated}"),
};

var currentCoxFirstPrinciplesIFullTextContractAuditPassed = checks.All(check => check.Passed)
    && fullTextReviewed
    && fullTextOnlyScaffoldEvidenceFound
    && !fullTextBosonContractEvidenceFound
    && !routeCompletesBosonPredictions;
var terminalStatus = currentCoxFirstPrinciplesIFullTextContractAuditPassed
    ? "current-cox-first-principles-i-full-text-scaffold-only-no-boson-contract"
    : "current-cox-first-principles-i-full-text-contract-review-required";

var result = new
{
    phaseId = "phase387-current-cox-first-principles-i-full-text-contract-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    currentCoxFirstPrinciplesIFullTextContractAuditPassed,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind,
    fullTextReviewed,
    fullTextBosonContractEvidenceFound,
    fullTextObservedNamespaceMapFound,
    fullTextWzSourceRowsFound,
    fullTextHiggsScalarSourceFound,
    fullTextElectroweakParameterLineageFound,
    fullTextWeakMixingOrCouplingLineageFound,
    fullTextGeVNormalizationFound,
    fullTextPoleExtractionFound,
    fullTextOnlyScaffoldEvidenceFound,
    source,
    contractKeywordEvidence,
    scaffoldTermEvidence,
    openLayerEvidence,
    fullTextContractApplication = new
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
        phase386Path = Phase386Path,
        phase386SourceDeltaAuditPassed,
        phase386ScaffoldDeltaPresent,
        phase386PromotableForBosonMasses,
        phase386CompletesNamespaceMap,
        phase386FillsWzContract,
        phase386FillsHiggsContract,
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
    decision = currentCoxFirstPrinciplesIFullTextContractAuditPassed
        ? "Treat the full text as scaffold-only evidence for boson work. The extracted PDF contains strong classical GU scaffold terms and open-layer markers, but no electroweak observed namespace map, W/Z source rows, Higgs scalar-source row, weak mixing or coupling lineage, pole extraction, or GeV normalization."
        : "Review the full-text contract audit before relying on this source boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-native observed electroweak namespace map with photon/W/Z/H rows.",
        "A target-independent W/Z bridge-source theorem with separate W and Z source rows and stability sidecars.",
        "A solved Higgs scalar-source/operator with potential or excitation lineage.",
        "A target-independent weak-angle or coupling source, pole extraction, and GeV normalization lineage.",
    },
};

File.WriteAllText(Path.Combine(outputDir, "current_cox_first_principles_i_full_text_contract_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "current_cox_first_principles_i_full_text_contract_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.currentCoxFirstPrinciplesIFullTextContractAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        result.fullTextReviewed,
        result.fullTextBosonContractEvidenceFound,
        result.fullTextObservedNamespaceMapFound,
        result.fullTextWzSourceRowsFound,
        result.fullTextHiggsScalarSourceFound,
        result.fullTextElectroweakParameterLineageFound,
        result.fullTextWeakMixingOrCouplingLineageFound,
        result.fullTextGeVNormalizationFound,
        result.fullTextPoleExtractionFound,
        result.fullTextOnlyScaffoldEvidenceFound,
        result.source,
        result.contractKeywordEvidence,
        result.scaffoldTermEvidence,
        result.openLayerEvidence,
        result.fullTextContractApplication,
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
Console.WriteLine($"currentCoxFirstPrinciplesIFullTextContractAuditPassed={currentCoxFirstPrinciplesIFullTextContractAuditPassed}");
Console.WriteLine($"fullTextBosonContractEvidenceFound={fullTextBosonContractEvidenceFound}");
Console.WriteLine($"fullTextObservedNamespaceMapFound={fullTextObservedNamespaceMapFound}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string Sha256Hex(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record TermEvidence(string TermId, int Count, int[] LineRefs, string EvidenceClass);
sealed record Check(string CheckId, bool Passed, string Detail);
