using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase421: source-level audit of Cox GU IV v2, "The Testing Rig for LambdaCDM".
//
// This is a checkpoint-cadence theorem/source search, not a promotion route.
// The phase records reduced full-text evidence from the Zenodo PDF and asks
// whether the source fills Phase201/Phase256 boson contracts. It does not use
// W/Z/H targets, mutate contract templates, or import cosmology hooks as
// electroweak observed-field rows.

const string DefaultOutputDir = "studies/phase421_cox_gu_iv_v2_lcdm_rig_boson_contract_audit_001/output";
const string Phase201SummaryPath = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213SummaryPath = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase419SummaryPath = "studies/phase419_observed_field_symbolic_extraction_template_001/output/observed_field_symbolic_extraction_template_summary.json";
const string Phase420SummaryPath = "studies/phase420_naive_curvature_mass_scale_sanity_check_001/output/naive_curvature_mass_scale_sanity_check_summary.json";
const string ApplicationSubjectKind = "cox-gu-iv-v2-lcdm-rig-boson-contract-audit";

var outputDir = Environment.GetEnvironmentVariable("PHASE421_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201SummaryPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213SummaryPath));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
using var phase419 = JsonDocument.Parse(File.ReadAllText(Phase419SummaryPath));
using var phase420 = JsonDocument.Parse(File.ReadAllText(Phase420SummaryPath));

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool fullTextReviewed = true;
const bool lcdmRigScopeConfirmed = true;
const bool projectorVariationTheoremPresent = true;
const bool etheringtonGuardrailPresent = true;
const bool sevenCosmologyHooksPresent = true;
const bool cosmologyHooksAreNotElectroweakObservedFields = true;
const bool sourceProvidesBosonContractEvidence = false;
const bool sourceProvidesObservedElectroweakNamespaceMap = false;
const bool sourceProvidesPhotonWzHProjectionRows = false;
const bool sourceProvidesWzSourceRows = false;
const bool sourceProvidesHiggsScalarSourceRow = false;
const bool sourceProvidesElectroweakVevMap = false;
const bool sourceProvidesWeakAngleOrCouplingLineage = false;
const bool sourceProvidesCurvatureToElectroweakScaleLaw = false;
const bool sourceProvidesPoleExtraction = false;
const bool sourceProvidesGeVUnitNormalization = false;
const bool sourceProvidesPhase419TemplateFields = false;
const bool sourceSatisfiesPhase420ScaleFields = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;

var source = new
{
    refId = "COX-GU-IV-V2-17402261",
    title = "Geometric Unity IV (v2): The Testing Rig for LambdaCDM -- A Standards-Driven Framework",
    author = "Joseph Cox",
    doi = "10.5281/zenodo.17402261",
    conceptDoi = "10.5281/zenodo.17402260",
    zenodoRecordId = 17402261,
    zenodoApiRecord = "https://zenodo.org/api/records/17402261",
    pdfContentUrl = "https://zenodo.org/api/records/17402261/files/GUT.4.1.pdf/content",
    publicationDate = "2025-10-21",
    metadataCreated = "2025-10-20T23:57:37Z",
    metadataModified = "2025-11-05T20:25:14Z",
    artifactName = "GUT.4.1.pdf",
    artifactSizeBytes = 702258,
    artifactChecksum = "md5:1d51f99a44cf51c8023dbc500e58ed3c",
    reviewedOn = "2026-06-18",
    extractionTool = "pdftotext",
    extractedLineCount = 3305,
};

var absentBosonContractTermEvidence = new[]
{
    new TermEvidence("electroweak", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("weak mixing", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("weak angle", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("weinberg", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("hypercharge", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("higgs", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("w boson", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("z boson", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("standard model", 0, Array.Empty<int>(), "absent-contract-keyword"),
    new TermEvidence("gev", 0, Array.Empty<int>(), "absent-unit-keyword"),
    new TermEvidence("vev", 0, Array.Empty<int>(), "absent-scale-keyword"),
    new TermEvidence("pole", 0, Array.Empty<int>(), "absent-pole-keyword"),
    new TermEvidence("dirac", 0, Array.Empty<int>(), "absent-yukawa-or-spinor-contract-keyword"),
    new TermEvidence("yang-mills", 0, Array.Empty<int>(), "absent-gauge-boson-mass-law-keyword"),
};

var nonContractTermEvidence = new[]
{
    new TermEvidence("photon", 41, new[] { 16, 144, 366, 953, 969, 971, 976, 978 }, "etherington-and-optics-only"),
    new TermEvidence("mass", 6, new[] { 979, 995, 1007, 1479, 1904, 3091 }, "generic-null-shell-positivity-or-glossary-only"),
    new TermEvidence("curvature", 20, new[] { 18, 99, 316, 375, 1147, 1171, 1347, 1364 }, "cosmological-curvature-closure-only"),
    new TermEvidence("observed", 3, new[] { 2729, 2982, 3124 }, "ordinary-observation-distance-language-only"),
    new TermEvidence("particle", 2, new[] { 1007, 3222 }, "cosmology-or-reference-language-only"),
    new TermEvidence("spinor", 3, new[] { 266, 481, 3096 }, "field-list-or-glossary-only"),
};

var rigScopeTermEvidence = new[]
{
    new TermEvidence("brst", 93, new[] { 15, 28, 214, 260, 363, 369, 462, 465 }, "positive-rig-guardrail"),
    new TermEvidence("bv", 38, new[] { 12, 17, 28, 260, 359, 366, 369, 462 }, "positive-rig-guardrail"),
    new TermEvidence("projector", 35, new[] { 11, 51, 67, 215, 252, 359, 464, 468 }, "positive-rig-guardrail"),
    new TermEvidence("variation", 65, new[] { 11, 28, 51, 64, 67, 212, 215, 260 }, "positive-rig-guardrail"),
    new TermEvidence("etherington", 70, new[] { 16, 18, 24, 28, 51, 83, 96, 216 }, "positive-cosmology-hook"),
    new TermEvidence("boundary", 65, new[] { 13, 213, 259, 360, 460, 463, 466, 467 }, "positive-rig-guardrail"),
    new TermEvidence("gauge", 77, new[] { 12, 252, 266, 359, 370, 464, 466, 470 }, "positive-rig-guardrail"),
    new TermEvidence("lambda-symbol", 36, new[] { 1, 27, 29, 34, 346, 387, 428, 431 }, "positive-cosmology-scope"),
    new TermEvidence("cosmology", 10, new[] { 9, 31, 431, 713, 935, 2348, 3118, 3164 }, "positive-cosmology-scope"),
    new TermEvidence("lcdm", 2, new[] { 2263, 2626 }, "positive-cosmology-scope"),
};

var rigHookRows = new[]
{
    new HookRow("etherington-epsilon", "Distance-duality residual for light propagation.", ElectroweakBosonProjectionRow: false),
    new HookRow("sn-residual-tilt-gamma", "Supernova Hubble-diagram residual tilt.", ElectroweakBosonProjectionRow: false),
    new HookRow("bao-stretch-kappa", "BAO ruler stretch/fiducial consistency hook.", ElectroweakBosonProjectionRow: false),
    new HookRow("curvature-closure-omega-k", "Cosmological spatial-curvature closure.", ElectroweakBosonProjectionRow: false),
    new HookRow("growth-slip-mu-eta", "Large-scale structure growth and lensing slip.", ElectroweakBosonProjectionRow: false),
    new HookRow("standard-siren-xi0", "Gravitational-wave propagation hook.", ElectroweakBosonProjectionRow: false),
    new HookRow("inflation-positivity", "Inflationary EFT positivity corridor.", ElectroweakBosonProjectionRow: false),
};

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var observedFieldSymbolicExtractionTemplatePassed = JsonBool(phase419.RootElement, "observedFieldSymbolicExtractionTemplatePassed") is true;
var phase419SourceDefinedPhase256FieldCount = JsonInt(phase419.RootElement, "sourceDefinedPhase256FieldCount") ?? -1;
var phase419RequiredGuInputProvidedCount = JsonInt(phase419.RootElement, "requiredGuInputProvidedCount") ?? -1;
var phase419CanFillObserved = JsonBool(phase419.RootElement, "canFillPhase256ObservedFieldExtractionContract") is true;
var naiveCurvatureMassScaleSanityCheckPassed = JsonBool(phase420.RootElement, "naiveCurvatureMassScaleSanityCheckPassed") is true;
var phase420MissingScaleSpecificationFieldCount = JsonInt(phase420.RootElement, "missingScaleSpecificationFieldCount") ?? -1;
var phase420SourceProvidesGeVUnitNormalization = JsonBool(phase420.RootElement, "sourceProvidesGeVUnitNormalization") is true;
var phase420CanFillWz = JsonBool(phase420.RootElement, "canFillPhase201WzContract") is true;

var absentContractKeywordCount = absentBosonContractTermEvidence.Count(term => term.Count == 0);
var nonContractHitCount = nonContractTermEvidence.Count(term => term.Count > 0);
var rigScopeHitCount = rigScopeTermEvidence.Count(term => term.Count > 0);
var electroweakProjectionHookCount = rigHookRows.Count(row => row.ElectroweakBosonProjectionRow);
var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    ApplicationSubjectKind,
    source.refId,
    source.doi,
    source.artifactChecksum,
    source.extractedLineCount,
    absentBosonContractTermEvidence = absentBosonContractTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    nonContractTermEvidence = nonContractTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    rigScopeTermEvidence = rigScopeTermEvidence.Select(term => new { term.TermId, term.Count, term.EvidenceClass }),
    rigHookRows = rigHookRows.Select(row => new { row.HookId, row.ElectroweakBosonProjectionRow }),
}, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

var checks = new[]
{
    new Check(
        "zenodo-artifact-and-full-text-extraction-recorded",
        fullTextReviewed
            && source.zenodoRecordId == 17402261
            && source.artifactSizeBytes == 702258
            && source.artifactChecksum == "md5:1d51f99a44cf51c8023dbc500e58ed3c"
            && source.extractedLineCount == 3305
            && source.extractionTool == "pdftotext",
        $"recordId={source.zenodoRecordId}; checksum={source.artifactChecksum}; extractedLineCount={source.extractedLineCount}; extractionTool={source.extractionTool}"),
    new Check(
        "full-text-scope-is-lcdm-rig-not-electroweak-source-law",
        lcdmRigScopeConfirmed
            && projectorVariationTheoremPresent
            && etheringtonGuardrailPresent
            && sevenCosmologyHooksPresent
            && rigHookRows.Length == 7
            && electroweakProjectionHookCount == 0
            && rigScopeHitCount == rigScopeTermEvidence.Length,
        $"rigScopeHitCount={rigScopeHitCount}; hookCount={rigHookRows.Length}; electroweakProjectionHookCount={electroweakProjectionHookCount}"),
    new Check(
        "electroweak-contract-keywords-absent-from-full-text",
        absentContractKeywordCount == absentBosonContractTermEvidence.Length
            && absentBosonContractTermEvidence.Single(term => term.TermId == "electroweak").Count == 0
            && absentBosonContractTermEvidence.Single(term => term.TermId == "higgs").Count == 0
            && absentBosonContractTermEvidence.Single(term => term.TermId == "w boson").Count == 0
            && absentBosonContractTermEvidence.Single(term => term.TermId == "z boson").Count == 0
            && absentBosonContractTermEvidence.Single(term => term.TermId == "gev").Count == 0
            && absentBosonContractTermEvidence.Single(term => term.TermId == "vev").Count == 0,
        $"absentContractKeywordCount={absentContractKeywordCount}; absentTerms={string.Join(",", absentBosonContractTermEvidence.Select(term => term.TermId))}"),
    new Check(
        "positive-term-hits-are-cosmology-or-guardrail-only",
        nonContractHitCount == nonContractTermEvidence.Length
            && nonContractTermEvidence.Single(term => term.TermId == "photon").EvidenceClass == "etherington-and-optics-only"
            && nonContractTermEvidence.Single(term => term.TermId == "curvature").EvidenceClass == "cosmological-curvature-closure-only"
            && nonContractTermEvidence.Single(term => term.TermId == "mass").EvidenceClass == "generic-null-shell-positivity-or-glossary-only"
            && !sourceProvidesPhotonWzHProjectionRows
            && !sourceProvidesCurvatureToElectroweakScaleLaw,
        $"nonContractHitCount={nonContractHitCount}; photonCount={nonContractTermEvidence.Single(term => term.TermId == "photon").Count}; curvatureCount={nonContractTermEvidence.Single(term => term.TermId == "curvature").Count}; massCount={nonContractTermEvidence.Single(term => term.TermId == "mass").Count}"),
    new Check(
        "phase419-and-phase420-contract-gaps-preserved",
        observedFieldSymbolicExtractionTemplatePassed
            && phase419SourceDefinedPhase256FieldCount == 0
            && phase419RequiredGuInputProvidedCount == 0
            && !phase419CanFillObserved
            && naiveCurvatureMassScaleSanityCheckPassed
            && phase420MissingScaleSpecificationFieldCount == 9
            && !phase420SourceProvidesGeVUnitNormalization
            && !phase420CanFillWz
            && !sourceProvidesPhase419TemplateFields
            && !sourceSatisfiesPhase420ScaleFields,
        $"p419SourceDefined={phase419SourceDefinedPhase256FieldCount}; p419Inputs={phase419RequiredGuInputProvidedCount}; p420MissingScaleFields={phase420MissingScaleSpecificationFieldCount}; p420Gev={phase420SourceProvidesGeVUnitNormalization}"),
    new Check(
        "phase201-and-phase256-contracts-remain-unfilled",
        !phase201AllRequiredLineagesPromotable
            && phase213WzMissingFieldCount == 15
            && phase213HiggsMissingFieldCount == 14
            && phase256RequiredFieldCount == 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ContractPromotable
            && !sourceProvidesBosonContractEvidence
            && !sourceProvidesObservedElectroweakNamespaceMap
            && !sourceProvidesWzSourceRows
            && !sourceProvidesHiggsScalarSourceRow
            && !sourceProvidesWeakAngleOrCouplingLineage
            && !sourceProvidesElectroweakVevMap
            && !sourceProvidesPoleExtraction
            && !sourceProvidesGeVUnitNormalization,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; wzMissing={phase213WzMissingFieldCount}; higgsMissing={phase213HiggsMissingFieldCount}; p256Filled={phase256FilledRequiredFieldCount}"),
    new Check(
        "source-contracts-not-mutated-or-promoted",
        targetBlindConstruction
            && !physicalTargetsConsultedForConstruction
            && targetBlindConstructionHash.Length == 64
            && !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; targetBlindHashLength={targetBlindConstructionHash.Length}; acceptedContractFieldCount={acceptedContractFieldCount}; phase201TemplateMutated={phase201TemplateMutated}"),
};

var coxGuIvV2LcdmRigBosonContractAuditPassed = checks.All(check => check.Passed)
    && fullTextReviewed
    && lcdmRigScopeConfirmed
    && !sourceProvidesBosonContractEvidence
    && !routeCompletesBosonPredictions;
var terminalStatus = coxGuIvV2LcdmRigBosonContractAuditPassed
    ? "cox-gu-iv-v2-lcdm-rig-boson-contract-audit-no-electroweak-source"
    : "cox-gu-iv-v2-lcdm-rig-boson-contract-review-required";

var decision = "Treat GU IV v2 as a cosmology testing-rig and guardrail source, not a boson-prediction source. The PDF supplies BRST/BV projection-variation, Etherington, LCDM hook, and reproducibility/corridor content, but no electroweak observed-field map, W/Z/H projection rows, Higgs scalar source, weak-angle or coupling lineage, curvature-to-electroweak scale law, pole extraction, or GeV normalization.";

var result = new
{
    phaseId = "phase421-cox-gu-iv-v2-lcdm-rig-boson-contract-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxGuIvV2LcdmRigBosonContractAuditPassed,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind = ApplicationSubjectKind,
    fullTextReviewed,
    lcdmRigScopeConfirmed,
    projectorVariationTheoremPresent,
    etheringtonGuardrailPresent,
    sevenCosmologyHooksPresent,
    cosmologyHooksAreNotElectroweakObservedFields,
    sourceProvidesBosonContractEvidence,
    sourceProvidesObservedElectroweakNamespaceMap,
    sourceProvidesPhotonWzHProjectionRows,
    sourceProvidesWzSourceRows,
    sourceProvidesHiggsScalarSourceRow,
    sourceProvidesElectroweakVevMap,
    sourceProvidesWeakAngleOrCouplingLineage,
    sourceProvidesCurvatureToElectroweakScaleLaw,
    sourceProvidesPoleExtraction,
    sourceProvidesGeVUnitNormalization,
    sourceProvidesPhase419TemplateFields,
    sourceSatisfiesPhase420ScaleFields,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    source,
    absentBosonContractTermEvidence,
    nonContractTermEvidence,
    rigScopeTermEvidence,
    rigHookRows,
    contractBoundary = new
    {
        phase201SummaryPath = Phase201SummaryPath,
        phase201AllRequiredLineagesPromotable,
        phase213SummaryPath = Phase213SummaryPath,
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
        phase256SummaryPath = Phase256SummaryPath,
        phase256RequiredFieldCount,
        phase256FilledRequiredFieldCount,
        phase256ContractPromotable,
        phase419SummaryPath = Phase419SummaryPath,
        observedFieldSymbolicExtractionTemplatePassed,
        phase419SourceDefinedPhase256FieldCount,
        phase419RequiredGuInputProvidedCount,
        phase419CanFillObserved,
        phase420SummaryPath = Phase420SummaryPath,
        naiveCurvatureMassScaleSanityCheckPassed,
        phase420MissingScaleSpecificationFieldCount,
        phase420SourceProvidesGeVUnitNormalization,
        phase420CanFillWz,
    },
    checks,
    decision,
};

var summary = new
{
    result.phaseId,
    result.terminalStatus,
    result.generatedAt,
    result.coxGuIvV2LcdmRigBosonContractAuditPassed,
    result.targetBlindConstruction,
    result.physicalTargetsConsultedForConstruction,
    result.targetBlindConstructionHash,
    result.applicationSubjectKind,
    result.fullTextReviewed,
    result.lcdmRigScopeConfirmed,
    result.projectorVariationTheoremPresent,
    result.etheringtonGuardrailPresent,
    result.sevenCosmologyHooksPresent,
    rigHookCount = rigHookRows.Length,
    electroweakProjectionHookCount,
    absentContractKeywordCount,
    nonContractHitCount,
    rigScopeHitCount,
    result.sourceProvidesBosonContractEvidence,
    result.sourceProvidesObservedElectroweakNamespaceMap,
    result.sourceProvidesPhotonWzHProjectionRows,
    result.sourceProvidesWzSourceRows,
    result.sourceProvidesHiggsScalarSourceRow,
    result.sourceProvidesElectroweakVevMap,
    result.sourceProvidesWeakAngleOrCouplingLineage,
    result.sourceProvidesCurvatureToElectroweakScaleLaw,
    result.sourceProvidesPoleExtraction,
    result.sourceProvidesGeVUnitNormalization,
    result.sourceProvidesPhase419TemplateFields,
    result.sourceSatisfiesPhase420ScaleFields,
    result.sourceContractApplicationAllowed,
    result.phase201TemplateMutated,
    result.fieldsAppliedToPhase201TemplateCount,
    result.acceptedContractFieldCount,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.source,
    result.contractBoundary,
    result.checks,
    result.decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_gu_iv_v2_lcdm_rig_boson_contract_audit.json"), JsonSerializer.Serialize(result, options) + Environment.NewLine);
File.WriteAllText(Path.Combine(outputDir, "cox_gu_iv_v2_lcdm_rig_boson_contract_audit_summary.json"), JsonSerializer.Serialize(summary, options) + Environment.NewLine);

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxGuIvV2LcdmRigBosonContractAuditPassed={coxGuIvV2LcdmRigBosonContractAuditPassed}");
Console.WriteLine($"lcdmRigScopeConfirmed={lcdmRigScopeConfirmed}");
Console.WriteLine($"absentContractKeywordCount={absentContractKeywordCount}");
Console.WriteLine($"sourceProvidesBosonContractEvidence={sourceProvidesBosonContractEvidence}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

static bool? JsonBool(JsonElement element, string name)
{
    return element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;
}

static int? JsonInt(JsonElement element, string name)
{
    return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
        ? result
        : null;
}

static string Sha256Hex(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

public sealed record TermEvidence(string TermId, int Count, int[] SampleLineNumbers, string EvidenceClass);
public sealed record HookRow(string HookId, string Description, bool ElectroweakBosonProjectionRow);
public sealed record Check(string CheckId, bool Passed, string Evidence);
