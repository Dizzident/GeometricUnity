using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase427: source-level audit of the Hofseth GU-RVG "Superluminal Metric
// Engineering" article, discharging the second NEW-LEAD from the 2026-07-01
// literature sweep. The originally-catalogued Zenodo record 21056575 was
// DELETED on 2026-07-01 (tombstone reason "duplicate"); the live successor is
// record 21117379 with identical title and claimed authorship. The audit
// records the deletion lineage, the externally-unverified Weinstein-Harvard
// co-authorship attribution (matching the fabricated-attribution pattern
// documented in arXiv:2606.02184), and the full-text contract evidence: the
// paper IMPORTS the electroweak VEV v = 246 GeV as an explicit input (its own
// derived/input/open convention marks it "I") to set a 27.2 TeV dilaton decay
// constant, and supplies no W/Z/H source rows, observed-field projection,
// weak-angle lineage, pole extraction, or GeV-from-geometry normalization.

const string DefaultOutputDir = "studies/phase427_hofseth_gu_rvg_superluminal_source_audit_001/output";
const string Phase201SummaryPath = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213SummaryPath = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase423SummaryPath = "studies/phase423_zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_001/output/zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_summary.json";
const string Phase426SummaryPath = "studies/phase426_cox_gu_series_boson_contract_audit_001/output/cox_gu_series_boson_contract_audit_summary.json";
const string ApplicationSubjectKind = "hofseth-gu-rvg-superluminal-source-audit";

var outputDir = Environment.GetEnvironmentVariable("PHASE427_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201SummaryPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213SummaryPath));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
using var phase423 = JsonDocument.Parse(File.ReadAllText(Phase423SummaryPath));
using var phase426 = JsonDocument.Parse(File.ReadAllText(Phase426SummaryPath));

// ---------------------------------------------------------------------------
// Recorded source identity (retrieved 2026-07-01 via the Zenodo API).
// ---------------------------------------------------------------------------

var source = new
{
    originalZenodoRecordId = 21056575,
    originalDoi = "10.5281/zenodo.21056575",
    originalRecordDeleted = true,
    originalRemovalDateUtc = "2026-07-01T22:51:40Z",
    originalRemovalReason = "duplicate",
    originalDeletionPolicy = "grace-period-v1",
    liveZenodoRecordId = 21117379,
    liveDoi = "10.5281/zenodo.21117379",
    publicationDate = "2026-06-18",
    title = "The Mechanics, Material Constraints, and Causal Preservation of Superluminal Metric Engineering within the GU-RVG Framework: with an Independently Verified Photonic Sector",
    claimedAuthors = "Hofseth, Jesse D. (Liberty University); Weinstein, Eric R. (Harvard University)",
    pdfSizeBytes = 3084101L,
    pdfChecksum = "md5:90be901bc227bc90e493c295aa276046",
    extractionTool = "pdftotext",
    extractedLineCount = 6465,
};
const bool originalRecordTombstoned = true;
const bool liveSuccessorRecordLocated = true;
const bool weinsteinCoAuthorshipExternallyVerified = false;
const bool attributionMatchesDocumentedFabricationPattern = true; // arXiv:2606.02184
const bool priorPhase423SourceSetDidNotIncludeThisRecord = true;

// full-text keyword evidence (lowercase substring counts)
var termEvidence = new[]
{
    new TermEvidence("electroweak", 4, "external-vev-import-and-precision-bound-context"),
    new TermEvidence("246", 2, "external-electroweak-vev-input-for-dilaton-decay-constant"),
    new TermEvidence("vacuum expectation", 1, "dilaton-vev-radiative-reading-uses-imported-v246"),
    new TermEvidence("95.4", 25, "external-collider-diphoton-signal-context"),
    new TermEvidence("dilaton", 104, "gu-rvg-dilaton-phenomenology-scope"),
    new TermEvidence("koide", 16, "koide-relation-phenomenology-scope"),
    new TermEvidence("condensate", 164, "warp-drive-condensate-scope"),
    new TermEvidence("fixed by observation", 2, "source-marks-condensate-amplitude-as-observational-input"),
    new TermEvidence("higgs", 2, "two-higgs-doublet-comparison-prose-only"),
    new TermEvidence("doublet", 2, "two-higgs-doublet-comparison-prose-only"),
    new TermEvidence("yukawa", 1, "koide-motivation-prose-only"),
    new TermEvidence("w boson", 0, "absent-contract-keyword"),
    new TermEvidence("z boson", 0, "absent-contract-keyword"),
    new TermEvidence("weak mixing", 0, "absent-contract-keyword"),
    new TermEvidence("weinberg angle", 0, "absent-contract-keyword"),
    new TermEvidence("hypercharge", 0, "absent-contract-keyword"),
    new TermEvidence("pole mass", 0, "absent-contract-keyword"),
    new TermEvidence("vector-spinor", 0, "absent-projection-map-keyword"),
};
int absentContractKeywordCount = termEvidence.Count(t => t.Count == 0);

const bool sourceScopeIsSuperluminalMetricEngineering = true;
const bool sourceUsesExternalElectroweakVev246Gev = true;   // eq. (5)-(6): v = 246 GeV input -> f_phi ~ 27.2 TeV
const bool sourceUsesExternalCollider95GevSignal = true;
const bool sourceMarksCondensateAmplitudeAsObservationalInput = true;
const bool sourceProvidesBosonContractEvidence = false;
const bool sourceProvidesObservedElectroweakNamespaceMap = false;
const bool sourceProvidesPhotonWzHProjectionRows = false;
const bool sourceProvidesWzSourceRows = false;
const bool sourceProvidesSeparateWzRows = false;
const bool sourceProvidesHiggsScalarSourceRow = false;
const bool sourceProvidesElectroweakVevMap = false;
const bool sourceProvidesTargetIndependentVevSource = false;
const bool sourceProvidesWeakAngleOrCouplingLineage = false;
const bool sourceProvidesCurvatureToElectroweakScaleLaw = false;
const bool sourceProvidesPoleExtraction = false;
const bool sourceProvidesGeVUnitNormalization = false;
const bool sourceProvidesVectorSpinor144ProjectionMap = false;
const bool sourceProvidesCrossCarrierProjectionMap = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase423PrecursorPassed = JsonBool(phase423.RootElement, "zenodoGuRvgSpinorialDarkSectorBosonContractAuditPassed") is true;
var phase426PrecursorPassed = JsonBool(phase426.RootElement, "coxGuSeriesBosonContractAuditPassed") is true;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "zenodo 21056575 tombstone + live 21117379 pdftotext keyword evidence; external-input classification; no target values")))).ToLowerInvariant();

bool hofsethGuRvgSuperluminalSourceAuditPassed =
    phase423PrecursorPassed &&
    phase426PrecursorPassed &&
    !phase201AllRequiredLineagesPromotable &&
    phase213WzMissingFieldCount == 15 &&
    phase213HiggsMissingFieldCount == 14 &&
    !phase256ContractPromotable &&
    originalRecordTombstoned &&
    liveSuccessorRecordLocated &&
    !weinsteinCoAuthorshipExternallyVerified &&
    attributionMatchesDocumentedFabricationPattern &&
    priorPhase423SourceSetDidNotIncludeThisRecord &&
    absentContractKeywordCount >= 7 &&
    sourceScopeIsSuperluminalMetricEngineering &&
    sourceUsesExternalElectroweakVev246Gev &&
    sourceUsesExternalCollider95GevSignal &&
    sourceMarksCondensateAmplitudeAsObservationalInput &&
    !sourceProvidesBosonContractEvidence &&
    !sourceProvidesObservedElectroweakNamespaceMap &&
    !sourceProvidesPhotonWzHProjectionRows &&
    !sourceProvidesWzSourceRows &&
    !sourceProvidesSeparateWzRows &&
    !sourceProvidesHiggsScalarSourceRow &&
    !sourceProvidesElectroweakVevMap &&
    !sourceProvidesTargetIndependentVevSource &&
    !sourceProvidesWeakAngleOrCouplingLineage &&
    !sourceProvidesCurvatureToElectroweakScaleLaw &&
    !sourceProvidesPoleExtraction &&
    !sourceProvidesGeVUnitNormalization &&
    !sourceProvidesVectorSpinor144ProjectionMap &&
    !sourceProvidesCrossCarrierProjectionMap &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesWeakAngleOrCouplingLineage &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed &&
    !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 &&
    acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = hofsethGuRvgSuperluminalSourceAuditPassed
    ? "hofseth-gu-rvg-superluminal-audited-external-inputs-no-contract-fill"
    : "hofseth-gu-rvg-superluminal-source-audit-blocked";

string decision = hofsethGuRvgSuperluminalSourceAuditPassed
    ? "The Hofseth GU-RVG superluminal metric engineering lead is audited and non-promotional. The originally-catalogued record 21056575 was deleted on 2026-07-01 (tombstone reason: duplicate); the live successor 21117379 carries the same title and the same externally-unverified Weinstein-Harvard co-authorship attribution, which matches the fabricated-attribution pattern documented in arXiv:2606.02184. The full text is 95.4 GeV dilaton/Koide/warp-condensate phenomenology that IMPORTS the electroweak VEV v = 246 GeV as an explicit input to set a 27.2 TeV dilaton decay constant, and its own bookkeeping marks the condensate amplitude as fixed by observation rather than computation. No W/Z/H source rows, observed-field projection, weak-angle lineage, pole extraction, or GeV-from-geometry normalization exists anywhere in the text. No Phase201 or Phase256 field is filled; no W/Z/H mass is promoted."
    : "Do not use the Hofseth GU-RVG audit until the precursor and fail-closed batteries pass.";

var result = new
{
    phaseId = "phase427-hofseth-gu-rvg-superluminal-source-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    hofsethGuRvgSuperluminalSourceAuditPassed,
    phase423PrecursorPassed,
    phase426PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    source,
    originalRecordTombstoned,
    liveSuccessorRecordLocated,
    weinsteinCoAuthorshipExternallyVerified,
    attributionMatchesDocumentedFabricationPattern,
    priorPhase423SourceSetDidNotIncludeThisRecord,
    termEvidence = termEvidence.Select(t => new { term = t.Term, count = t.Count, evidenceClass = t.EvidenceClass }).ToArray(),
    absentContractKeywordCount,
    sourceScopeIsSuperluminalMetricEngineering,
    sourceUsesExternalElectroweakVev246Gev,
    sourceUsesExternalCollider95GevSignal,
    sourceMarksCondensateAmplitudeAsObservationalInput,
    sourceProvidesBosonContractEvidence,
    sourceProvidesObservedElectroweakNamespaceMap,
    sourceProvidesPhotonWzHProjectionRows,
    sourceProvidesWzSourceRows,
    sourceProvidesSeparateWzRows,
    sourceProvidesHiggsScalarSourceRow,
    sourceProvidesElectroweakVevMap,
    sourceProvidesTargetIndependentVevSource,
    sourceProvidesWeakAngleOrCouplingLineage,
    sourceProvidesCurvatureToElectroweakScaleLaw,
    sourceProvidesPoleExtraction,
    sourceProvidesGeVUnitNormalization,
    sourceProvidesVectorSpinor144ProjectionMap,
    sourceProvidesCrossCarrierProjectionMap,
    upstreamContractState = new
    {
        phase201SummaryPath = Phase201SummaryPath,
        phase201AllRequiredLineagesPromotable,
        phase213SummaryPath = Phase213SummaryPath,
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
        phase256SummaryPath = Phase256SummaryPath,
        phase256ContractPromotable,
        phase423SummaryPath = Phase423SummaryPath,
        phase426SummaryPath = Phase426SummaryPath,
    },
    physicalCouplingProvided,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesWeakAngleOrCouplingLineage,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The paper's electroweak content is imported (v = 246 GeV as input; 95.4 GeV as external collider signal), not derived.",
        "The claimed Weinstein-Harvard co-authorship is externally unverified and matches a documented fabricated-attribution pattern; the audit records the attribution without endorsing it.",
        "The original record's deletion (reason: duplicate) and the live successor's identity are recorded for lineage; neither changes the non-promotional verdict.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "hofseth_gu_rvg_superluminal_source_audit.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "hofseth_gu_rvg_superluminal_source_audit_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"hofsethGuRvgSuperluminalSourceAuditPassed={hofsethGuRvgSuperluminalSourceAuditPassed}");
Console.WriteLine($"originalRecordTombstoned={originalRecordTombstoned} liveSuccessorRecordLocated={liveSuccessorRecordLocated}");
Console.WriteLine($"sourceUsesExternalElectroweakVev246Gev={sourceUsesExternalElectroweakVev246Gev} sourceMarksCondensateAmplitudeAsObservationalInput={sourceMarksCondensateAmplitudeAsObservationalInput}");
Console.WriteLine($"sourceProvidesWzSourceRows={sourceProvidesWzSourceRows} sourceProvidesGeVUnitNormalization={sourceProvidesGeVUnitNormalization}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record TermEvidence(string Term, int Count, string EvidenceClass);
