using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase426: source-level audit of the Cox "Geometric Unity I-V" Zenodo series
// (June 2026). The 2026-07-01 literature sweep resolved the hinted
// "Geometric Unity V" monitor target: Cox published a renumbered five-part
// series. This phase records full-text keyword evidence for every member and
// classifies the one member with genuine electroweak-adjacent content
// (GU II, "The Matter Ledger"): its tree-level hypercharge-kernel relation
// g_Y^2 = (3/5) g^2 corroborates the repository's blind Phase404 embedding
// derivation, and its named Pati-Salam bi-doublet scalar channel (1,2,2)
// matches the Phase403/409 doublet-carrier requirement - but the source's
// own scope boundaries deny a scalar potential, VEV, mass spectrum,
// unification scale, or measured-coupling fit, so no contract field is
// filled and nothing is promoted.

const string DefaultOutputDir = "studies/phase426_cox_gu_series_boson_contract_audit_001/output";
const string Phase201SummaryPath = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213SummaryPath = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase421SummaryPath = "studies/phase421_cox_gu_iv_v2_lcdm_rig_boson_contract_audit_001/output/cox_gu_iv_v2_lcdm_rig_boson_contract_audit_summary.json";
const string Phase425SummaryPath = "studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001/output/cross_carrier_bilinear_sm_doublet_completion_audit_summary.json";
const string ApplicationSubjectKind = "cox-gu-series-boson-contract-audit";

var outputDir = Environment.GetEnvironmentVariable("PHASE426_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201SummaryPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213SummaryPath));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
using var phase421 = JsonDocument.Parse(File.ReadAllText(Phase421SummaryPath));
using var phase425 = JsonDocument.Parse(File.ReadAllText(Phase425SummaryPath));

// ---------------------------------------------------------------------------
// Recorded source identity (retrieved 2026-07-01 via the Zenodo API; PDFs
// downloaded, md5-verified against record metadata, extracted with pdftotext).
// ---------------------------------------------------------------------------

var seriesRecords = new[]
{
    new SeriesRecord("GU I", 20550275, "10.5281/zenodo.20550275", "2026-06-05",
        "Geometric Unity I: From Heuristic Proposal to Testable Classical Framework",
        "Geometric Unity I.pdf", 851223, "md5:781776e80a7f6bfa05d48048da354709", 8214),
    new SeriesRecord("GU II", 20517363, "10.5281/zenodo.20517363", "2026-06-02",
        "Geometric Unity II: The Matter Ledger. Minimal Chiral Completion, One-Family Pati-Salam, Spin(10), Anomaly Closure",
        "Geometric Unity II_The Matter Ledger.pdf", 812590, "md5:3bd0362c0eeaba243b7d99e40e17325a", 5030),
    new SeriesRecord("GU III", 20517502, "10.5281/zenodo.20517502", "2026-06-02",
        "Geometric Unity III: The Quantum Legality Layer. Semidirect BRST/BV, Anomaly Closure, Counterterms",
        "Geometric Unity III_The Quantum Legality Layer.pdf", 570657, "md5:20b077215324dfcfd8589deb6d4817fe", 2616),
    new SeriesRecord("GU IV", 20518853, "10.5281/zenodo.20518853", "2026-06-03",
        "Geometric Unity IV: The Observable Interface. The sigma0 Export Packet, Negative-Stiff Backgrounds, Source Adapters",
        "Geometric Unity IV_The Observable Interface.pdf", 687393, "md5:d023411476979a2aac72b7538dc85ce1", 4904),
    new SeriesRecord("GU V", 20531776, "10.5281/zenodo.20531776", "2026-06-03",
        "Geometric Unity V: The Lambda Rig. A Sign-Aware and Pre-Registered Audit Protocol for Declared Cosmological Export Packets",
        "Geometric Unity V_The Lambda Rig.pdf", 645497, "md5:1907f2f643907355bc9310d26955382b", 3705),
};
const string SeriesAuthor = "Cox, Joseph";
const bool seriesAuthorMatchesPhase421Lineage = true; // same author as GU IV v2 (zenodo 17402261)
const bool priorPhase421SourceSetDidNotIncludeSeries = true; // Phase421 audited zenodo 17402261 only

// contract keyword evidence per record (lowercase substring counts over the
// pdftotext extraction; the battery mirrors Phase421/423)
var contractKeywordEvidence = new[]
{
    // record, keyword, count, classification
    new TermEvidence("GU I", "246", 1, "equation-number-not-vev"),
    new TermEvidence("GU I", "yukawa", 2, "principal-symbol-prose-no-values"),
    new TermEvidence("GU I", "electroweak", 0, "absent-contract-keyword"),
    new TermEvidence("GU I", "higgs", 0, "absent-contract-keyword"),
    new TermEvidence("GU II", "hypercharge", 65, "representation-ledger-kernel-relation-no-values"),
    new TermEvidence("GU II", "doublet", 17, "pati-salam-bi-doublet-candidate-channel-only"),
    new TermEvidence("GU II", "yukawa", 36, "selection-rule-no-texture-or-values"),
    new TermEvidence("GU II", "electroweak", 0, "absent-contract-keyword"),
    new TermEvidence("GU II", "higgs", 0, "absent-contract-keyword"),
    new TermEvidence("GU II", "vev", 0, "absent-contract-keyword"),
    new TermEvidence("GU II", "gev", 0, "absent-contract-keyword"),
    new TermEvidence("GU II", "weinberg", 1, "textbook-citation-only"),
    new TermEvidence("GU III", "hypercharge", 4, "imported-gu-ii-map-bookkeeping"),
    new TermEvidence("GU III", "doublet", 1, "witten-anomaly-count-only"),
    new TermEvidence("GU III", "electroweak", 0, "absent-contract-keyword"),
    new TermEvidence("GU IV", "electroweak", 0, "absent-contract-keyword"),
    new TermEvidence("GU IV", "higgs", 0, "absent-contract-keyword"),
    new TermEvidence("GU IV", "hypercharge", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "electroweak", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "higgs", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "hypercharge", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "w boson", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "z boson", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "weak mixing", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "246", 0, "absent-contract-keyword"),
    new TermEvidence("GU V", "pole mass", 0, "absent-contract-keyword"),
};
int absentContractKeywordCount = contractKeywordEvidence.Count(t => t.Count == 0);
int presentContractAdjacentKeywordCount = contractKeywordEvidence.Count(t => t.Count > 0);

// series scope evidence: GU IV/V are cosmology export/audit rigs (BAO,
// supernovae, export packets, pre-registered audits); GU III is BRST/BV
// bookkeeping; GU I is the classical-framework base paper.
const bool guVScopeIsCosmologicalAuditRig = true;      // "lambda" x83, "bao" x42, "audit" x67, "export packet" x26
const bool guIvScopeIsCosmologicalExportPacket = true; // "bao" x69, "export packet" x9, "brst" x10
const bool guIiiScopeIsBrstBvLegalityLayer = true;     // "brst" x109
const bool guIScopeIsClassicalFrameworkBase = true;    // no electroweak content

// GU II classification: the one series member with electroweak-adjacent
// structure, read against its own scope boundaries (Scope Boundary 8.4,
// H.4; the "does not export" clauses in sections 10-11).
const bool guIiProvidesPatiSalamMatterLedger = true;
const bool guIiNamesPatiSalamBiDoubletScalarChannel = true;   // Phi_H ~ (1,2,2), candidate seed channel only
const bool guIiDerivesTreeLevelHyperchargeKernel = true;      // Y = T_R3 + (B-L)/2, g_BL = sqrt(3/2) g4
const bool guIiDerivesTanSquaredThreeFifthsAtUnification = true; // Theorem H.3: if gR = g4 = g then gY^2 = (3/5) g^2
const bool guIiKernelRelationCorroboratesPhase404 = true;     // matches the repo's blind tan^2 = 3/5 (Phase404)
const bool guIiDoubletChannelMatchesPhase403Requirement = true; // (1,2,2) is the doublet-carrier shape Phase403/409 named missing
const bool guIiProvidesScalarPotential = false;               // Scope Boundary H.4 / section 10 "does not export"
const bool guIiProvidesVevOrScale = false;                    // no VEV, no unification scale, no thresholds
const bool guIiProvidesMassSpectrumOrPole = false;            // Scope Boundary 8.4: no mass spectrum
const bool guIiProvidesMeasuredCouplingFit = false;           // H.4: no measured coupling fit
const bool guIiProvesInternalFluctuationContainsBiDoublet = false; // section 10: explicitly not proven
const bool guIiProvidesGeVUnitNormalization = false;

// contract verdicts for the whole series
const bool sourceProvidesBosonContractEvidence = false;
const bool sourceProvidesObservedElectroweakNamespaceMap = false;
const bool sourceProvidesPhotonWzHProjectionRows = false;
const bool sourceProvidesWzSourceRows = false;
const bool sourceProvidesSeparateWzRows = false;
const bool sourceProvidesHiggsScalarSourceRow = false;
const bool sourceProvidesElectroweakVevMap = false;
const bool sourceProvidesTargetIndependentVevSource = false;
const bool sourceProvidesWeakAngleOrCouplingLineage = false; // tree-level kernel relation only, no scale/running/fit
const bool sourceProvidesCurvatureToElectroweakScaleLaw = false;
const bool sourceProvidesPoleExtraction = false;
const bool sourceProvidesGeVUnitNormalization = false;
const bool sourceProvidesVectorSpinor144ProjectionMap = false;
const bool sourceProvidesCrossCarrierProjectionMap = false;

// upstream contract state (fail-closed cross-checks)
var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase421PrecursorPassed = JsonBool(phase421.RootElement, "coxGuIvV2LcdmRigBosonContractAuditPassed") is true;
var phase425PrecursorPassed =
    JsonBool(phase425.RootElement, "crossCarrierBilinearSmDoubletCompletionAuditPassed") is true &&
    JsonBool(phase425.RootElement, "bilinearCompositeLayerClosedOnAllSourcePinnedCarriers") is true;

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
    "zenodo 20550275/20517363/20517502/20518853/20531776 pdftotext keyword evidence; GU II scope-boundary classification; no target values")))).ToLowerInvariant();

bool coxGuSeriesBosonContractAuditPassed =
    phase421PrecursorPassed &&
    phase425PrecursorPassed &&
    !phase201AllRequiredLineagesPromotable &&
    phase213WzMissingFieldCount == 15 &&
    phase213HiggsMissingFieldCount == 14 &&
    !phase256ContractPromotable &&
    seriesRecords.Length == 5 &&
    seriesAuthorMatchesPhase421Lineage &&
    priorPhase421SourceSetDidNotIncludeSeries &&
    absentContractKeywordCount >= 14 &&
    guVScopeIsCosmologicalAuditRig &&
    guIvScopeIsCosmologicalExportPacket &&
    guIiiScopeIsBrstBvLegalityLayer &&
    guIScopeIsClassicalFrameworkBase &&
    guIiProvidesPatiSalamMatterLedger &&
    guIiNamesPatiSalamBiDoubletScalarChannel &&
    guIiDerivesTreeLevelHyperchargeKernel &&
    guIiDerivesTanSquaredThreeFifthsAtUnification &&
    guIiKernelRelationCorroboratesPhase404 &&
    guIiDoubletChannelMatchesPhase403Requirement &&
    !guIiProvidesScalarPotential &&
    !guIiProvidesVevOrScale &&
    !guIiProvidesMassSpectrumOrPole &&
    !guIiProvidesMeasuredCouplingFit &&
    !guIiProvesInternalFluctuationContainsBiDoublet &&
    !guIiProvidesGeVUnitNormalization &&
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

string terminalStatus = coxGuSeriesBosonContractAuditPassed
    ? "cox-gu-series-audited-gu-ii-corroborates-embedding-no-contract-fill"
    : "cox-gu-series-boson-contract-audit-blocked";

string decision = coxGuSeriesBosonContractAuditPassed
    ? "The June 2026 Cox Geometric Unity I-V Zenodo series is audited and non-promotional. GU I/III/IV/V carry no electroweak contract content (classical framework base, BRST/BV legality layer, and cosmological export/audit rigs). GU II, The Matter Ledger, is genuinely electroweak-adjacent: it derives the tree-level hypercharge kernel with g_Y^2 = (3/5) g^2 at a Pati-Salam unification point - independently corroborating the repository's blind Phase404 embedding result - and names the minimal Pati-Salam bi-doublet scalar channel (1,2,2), matching the doublet-carrier requirement Phase403/409 identified. But GU II's own scope boundaries deny a scalar potential, VEV, mass spectrum, unification scale, threshold model, or measured-coupling fit, and it explicitly does not prove any internal geometric fluctuation contains the (1,2,2) channel. No Phase201 or Phase256 field can be filled; no W/Z/H mass is promoted."
    : "Do not use the Cox GU series audit until the precursor and fail-closed batteries pass.";

var result = new
{
    phaseId = "phase426-cox-gu-series-boson-contract-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    coxGuSeriesBosonContractAuditPassed,
    phase421PrecursorPassed,
    phase425PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    seriesAuthor = SeriesAuthor,
    seriesAuthorMatchesPhase421Lineage,
    priorPhase421SourceSetDidNotIncludeSeries,
    seriesRecordCount = seriesRecords.Length,
    seriesRecords = seriesRecords.Select(r => new
    {
        label = r.Label,
        zenodoRecordId = r.RecordId,
        doi = r.Doi,
        publicationDate = r.PublicationDate,
        title = r.Title,
        pdfFileName = r.FileName,
        pdfSizeBytes = r.SizeBytes,
        pdfChecksum = r.Checksum,
        extractedLineCount = r.ExtractedLineCount,
        extractionTool = "pdftotext",
    }).ToArray(),
    contractKeywordEvidence = contractKeywordEvidence.Select(t => new
    {
        record = t.Record,
        term = t.Term,
        count = t.Count,
        evidenceClass = t.EvidenceClass,
    }).ToArray(),
    absentContractKeywordCount,
    presentContractAdjacentKeywordCount,
    guVScopeIsCosmologicalAuditRig,
    guIvScopeIsCosmologicalExportPacket,
    guIiiScopeIsBrstBvLegalityLayer,
    guIScopeIsClassicalFrameworkBase,
    guIiProvidesPatiSalamMatterLedger,
    guIiNamesPatiSalamBiDoubletScalarChannel,
    guIiDerivesTreeLevelHyperchargeKernel,
    guIiDerivesTanSquaredThreeFifthsAtUnification,
    guIiKernelRelationCorroboratesPhase404,
    guIiDoubletChannelMatchesPhase403Requirement,
    guIiProvidesScalarPotential,
    guIiProvidesVevOrScale,
    guIiProvidesMassSpectrumOrPole,
    guIiProvidesMeasuredCouplingFit,
    guIiProvesInternalFluctuationContainsBiDoublet,
    guIiProvidesGeVUnitNormalization,
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
        phase421SummaryPath = Phase421SummaryPath,
        phase425SummaryPath = Phase425SummaryPath,
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
        "GU II's tree-level hypercharge-kernel relation is representation bookkeeping at a declared unification point; it supplies no scale, running, threshold model, or measured fit and is recorded only as corroboration of the repository's blind Phase404 result.",
        "GU II's (1,2,2) bi-doublet is a named candidate seed channel; the source explicitly does not prove any internal geometric fluctuation contains it, and supplies no potential, VEV, or mass operator.",
        "GU I/III/IV/V are classical-framework, BRST/BV, and cosmology export/audit papers with no electroweak contract content.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "cox_gu_series_boson_contract_audit.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "cox_gu_series_boson_contract_audit_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxGuSeriesBosonContractAuditPassed={coxGuSeriesBosonContractAuditPassed}");
Console.WriteLine($"seriesRecordCount={seriesRecords.Length} absentContractKeywordCount={absentContractKeywordCount}");
Console.WriteLine($"guIiDerivesTanSquaredThreeFifthsAtUnification={guIiDerivesTanSquaredThreeFifthsAtUnification} guIiKernelRelationCorroboratesPhase404={guIiKernelRelationCorroboratesPhase404}");
Console.WriteLine($"guIiNamesPatiSalamBiDoubletScalarChannel={guIiNamesPatiSalamBiDoubletScalarChannel} guIiProvidesScalarPotential={guIiProvidesScalarPotential} guIiProvidesVevOrScale={guIiProvidesVevOrScale}");
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

public sealed record SeriesRecord(
    string Label,
    int RecordId,
    string Doi,
    string PublicationDate,
    string Title,
    string FileName,
    long SizeBytes,
    string Checksum,
    int ExtractedLineCount);

public sealed record TermEvidence(string Record, string Term, int Count, string EvidenceClass);
