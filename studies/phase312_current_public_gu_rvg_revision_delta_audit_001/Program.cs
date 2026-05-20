using System.Text.Json;

const string DefaultOutputDir = "studies/phase312_current_public_gu_rvg_revision_delta_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase243Path = "studies/phase243_public_web_source_delta_audit_001/output/public_web_source_delta_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase281Path = "studies/phase281_geometric_refractive_unification_source_audit_001/output/geometric_refractive_unification_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE312_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase243 = JsonDocument.Parse(File.ReadAllText(Phase243Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase281 = JsonDocument.Parse(File.ReadAllText(Phase281Path));

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase243PriorGuRvgFound = JsonBool(phase243.RootElement, "recentGuRvgSynthesisFound") is true;
var phase243PriorGuRvgPromotable = JsonBool(phase243.RootElement, "recentGuRvgSynthesisPromotableForBosonMasses") is true;
var phase281PriorGuRvgAuditPassed = JsonBool(phase281.RootElement, "geometricRefractiveUnificationSourceAuditPassed") is true;
var phase281PriorGuRvgPromotesWz = JsonBool(phase281.RootElement, "guRvgPromotesWzMasses") is true;
var phase281PriorGuRvgPromotesHiggs = JsonBool(phase281.RootElement, "guRvgPromotesHiggsMass") is true;

var researchSources = new[]
{
    new PublicSourceRow(
        "ssrn-6571958-current-gu-rvg-koide-dilaton",
        "The Geometric-Refractive Unification: A Definitive Synthesis of the Koide Lepton Anomaly, the 95.4 GeV Dilaton Resonance, and Advanced Metric Engineering",
        "https://papers.ssrn.com/sol3/papers.cfm?abstract_id=6571958",
        "2026-05-01",
        "SSRN metadata reports the current GU-RVG synthesis as revised May 1, 2026 and points to Zenodo DOI 10.5281/zenodo.19297861.",
        PromotableForWzMasses: false,
        PromotableForHiggsMass: false,
        ProvidesSourceLineageFields: false,
        TargetOrExternalInputsUsed: true,
        MissingContracts: new[]
        {
            "W/Z theoremOrDerivationId",
            "separate W/Z source rows",
            "target-independent VEV or weak-coupling source",
            "observed photon/W/Z eigenstate projection",
            "Higgs scalar source/operator/profile/self-coupling lineage",
        }),
    new PublicSourceRow(
        "zenodo-19465254-current-gu-rvg-koide-dilaton",
        "The Geometric-Refractive Unification: A Definitive Synthesis of the Koide Lepton Anomaly, the 95.4 GeV Dilaton Resonance, and Advanced Metric Engineering",
        "https://zenodo.org/records/19465254",
        "2026-05-01",
        "Zenodo metadata and PDF text show Koide, 95.4 GeV dilaton, trace anomaly, Shiab, Zorro, and metric-engineering claims; the electroweak 246 GeV scale appears as an input in the dilaton/Koide correction, not as a GU-derived W/Z source row.",
        PromotableForWzMasses: false,
        PromotableForHiggsMass: false,
        ProvidesSourceLineageFields: false,
        TargetOrExternalInputsUsed: true,
        MissingContracts: new[]
        {
            "Phase201 W/Z source-lineage fields",
            "Phase245 W/Z absolute-scale unlock",
            "Phase256 observed-field extraction fields",
            "Phase201 Higgs scalar-source fields",
        }),
    new PublicSourceRow(
        "zenodo-19465143-holographic-gu-rvg-v8",
        "The Holographic Geometric-Refractive Unification: A Definitive Synthesis of the 14D Observerse, the 95.4 GeV Dilaton Resonance, and Advanced Metric Engineering",
        "https://zenodo.org/records/19465143",
        "2026-05-01",
        "The holographic v8 source adds Observerse, HKLL/Ryu-Takayanagi, anomaly-cancellation, Koide, and 95.4 GeV dilaton machinery, but still uses the 246 GeV electroweak VEV as an external parameter and supplies no physical W/Z projection rows.",
        PromotableForWzMasses: false,
        PromotableForHiggsMass: false,
        ProvidesSourceLineageFields: false,
        TargetOrExternalInputsUsed: true,
        MissingContracts: new[]
        {
            "photonEigenstateProjectionId",
            "wBosonSourceRowId",
            "zBosonSourceRowId",
            "quadraticElectroweakMassOperatorId",
            "higgsPotentialSelfCouplingRelationId",
        }),
    new PublicSourceRow(
        "ssrn-6713999-magnetic-material-strategy",
        "A Two-Tier Magnetic Material Strategy for Metric Engineering: From the Rare-Earth-Free Gap Magnet to the HeliFe-MADA Architecture for the GU-RVG Framework",
        "https://papers.ssrn.com/sol3/Delivery.cfm/6713999.pdf?abstractid=6713999&mirid=1",
        "2026-05-15",
        "The current material-strategy paper is a GU-RVG hardware/materials proposal for MADA/ADPG/SHD magnetic substrates and the master equation of levitation, not an electroweak source-lineage or Higgs scalar-source derivation.",
        PromotableForWzMasses: false,
        PromotableForHiggsMass: false,
        ProvidesSourceLineageFields: false,
        TargetOrExternalInputsUsed: false,
        MissingContracts: new[]
        {
            "W/Z boson mass derivation",
            "observed electroweak mass operator",
            "Higgs scalar source/operator",
            "source-lineage replay gates",
        }),
};

var currentPublicGuRvgRevisionFound = true;
var currentPublicGuRvgResearchPerformedOn = "2026-05-20";
var currentPublicGuRvgMentionsShiabObserverseTraceAnomaly = true;
var currentPublicGuRvgMentions95GeVDilaton = true;
var currentPublicGuRvgMentionsKoideOr246GevScale = true;
var currentPublicGuRvgUsesExternalElectroweakVev246Gev = true;
var currentPublicGuRvgProvidesGuLocalWzTheorem = false;
var currentPublicGuRvgProvidesSeparateWzSourceRows = false;
var currentPublicGuRvgProvidesRawAmplitudeGate = false;
var currentPublicGuRvgProvidesCommonBridgeGate = false;
var currentPublicGuRvgProvidesTargetIndependentVevSource = false;
var currentPublicGuRvgProvidesWzMassMatrixSource = false;
var currentPublicGuRvgProvidesPhotonWzEigenstateProjectionRows = false;
var currentPublicGuRvgProvidesObservedFieldExtraction = false;
var currentPublicGuRvgProvidesHiggsScalarSourceOperator = false;
var currentPublicGuRvgProvidesHiggsIdentityEnvelope = false;
var currentPublicGuRvgProvidesObservedHiggsMassiveScalarProfile = false;
var currentPublicGuRvgProvidesHiggsSelfCouplingSource = false;
var currentPublicGuRvgPromotesWzMasses = false;
var currentPublicGuRvgPromotesHiggsMass = false;
var currentPublicGuRvgCompletesBosonPredictions = false;
var currentMaterialStrategyPromotesBosonMasses = false;
var currentMaterialStrategyFillsSourceLineage = false;

var priorAuditsAlreadyRejectedGuRvgPromotion =
    phase243PriorGuRvgFound
    && !phase243PriorGuRvgPromotable
    && phase281PriorGuRvgAuditPassed
    && !phase281PriorGuRvgPromotesWz
    && !phase281PriorGuRvgPromotesHiggs;
var currentPublicSourcesAllNonPromotable = researchSources.All(row =>
    !row.PromotableForWzMasses
    && !row.PromotableForHiggsMass
    && !row.ProvidesSourceLineageFields);
var currentBlockersStillBinding =
    wzMissingFieldCount == 15
    && higgsMissingFieldCount == 14
    && !unlockContractFilled
    && newSourceEvidenceStillRequired
    && observedFieldExtractionRequiredFieldCount == 20
    && observedFieldExtractionFilledRequiredFieldCount == 0
    && !observedFieldExtractionContractPromotable;

var checks = new[]
{
    new Check(
        "current-public-gu-rvg-sources-reviewed",
        currentPublicGuRvgRevisionFound
            && researchSources.Any(row => row.SourceId == "ssrn-6571958-current-gu-rvg-koide-dilaton")
            && researchSources.Any(row => row.SourceId == "zenodo-19465254-current-gu-rvg-koide-dilaton")
            && researchSources.Any(row => row.SourceId == "zenodo-19465143-holographic-gu-rvg-v8"),
        $"researchPerformedOn={currentPublicGuRvgResearchPerformedOn}; sourceCount={researchSources.Length}"),
    new Check(
        "current-public-gu-rvg-claims-recorded",
        currentPublicGuRvgMentionsShiabObserverseTraceAnomaly
            && currentPublicGuRvgMentions95GeVDilaton
            && currentPublicGuRvgMentionsKoideOr246GevScale
            && currentPublicGuRvgUsesExternalElectroweakVev246Gev,
        $"mentionsShiabObserverseTraceAnomaly={currentPublicGuRvgMentionsShiabObserverseTraceAnomaly}; mentions95GeVDilaton={currentPublicGuRvgMentions95GeVDilaton}; mentionsKoideOr246GevScale={currentPublicGuRvgMentionsKoideOr246GevScale}; usesExternalElectroweakVev246Gev={currentPublicGuRvgUsesExternalElectroweakVev246Gev}"),
    new Check(
        "current-public-gu-rvg-does-not-fill-wz-contract",
        !currentPublicGuRvgProvidesGuLocalWzTheorem
            && !currentPublicGuRvgProvidesSeparateWzSourceRows
            && !currentPublicGuRvgProvidesRawAmplitudeGate
            && !currentPublicGuRvgProvidesCommonBridgeGate
            && !currentPublicGuRvgProvidesTargetIndependentVevSource
            && !currentPublicGuRvgProvidesWzMassMatrixSource
            && !currentPublicGuRvgProvidesPhotonWzEigenstateProjectionRows
            && !currentPublicGuRvgProvidesObservedFieldExtraction
            && !currentPublicGuRvgPromotesWzMasses,
        $"guLocalWzTheorem={currentPublicGuRvgProvidesGuLocalWzTheorem}; separateWzSourceRows={currentPublicGuRvgProvidesSeparateWzSourceRows}; targetIndependentVevSource={currentPublicGuRvgProvidesTargetIndependentVevSource}; photonWzProjectionRows={currentPublicGuRvgProvidesPhotonWzEigenstateProjectionRows}; promotesWzMasses={currentPublicGuRvgPromotesWzMasses}"),
    new Check(
        "current-public-gu-rvg-does-not-fill-higgs-contract",
        !currentPublicGuRvgProvidesHiggsScalarSourceOperator
            && !currentPublicGuRvgProvidesHiggsIdentityEnvelope
            && !currentPublicGuRvgProvidesObservedHiggsMassiveScalarProfile
            && !currentPublicGuRvgProvidesHiggsSelfCouplingSource
            && !currentPublicGuRvgPromotesHiggsMass,
        $"higgsScalarSourceOperator={currentPublicGuRvgProvidesHiggsScalarSourceOperator}; higgsIdentityEnvelope={currentPublicGuRvgProvidesHiggsIdentityEnvelope}; observedHiggsMassiveScalarProfile={currentPublicGuRvgProvidesObservedHiggsMassiveScalarProfile}; higgsSelfCouplingSource={currentPublicGuRvgProvidesHiggsSelfCouplingSource}; promotesHiggsMass={currentPublicGuRvgPromotesHiggsMass}"),
    new Check(
        "material-strategy-is-hardware-not-boson-source-lineage",
        researchSources.Any(row => row.SourceId == "ssrn-6713999-magnetic-material-strategy")
            && !currentMaterialStrategyPromotesBosonMasses
            && !currentMaterialStrategyFillsSourceLineage,
        $"materialStrategyPromotesBosonMasses={currentMaterialStrategyPromotesBosonMasses}; materialStrategyFillsSourceLineage={currentMaterialStrategyFillsSourceLineage}"),
    new Check(
        "prior-gu-rvg-audits-remain-consistent",
        priorAuditsAlreadyRejectedGuRvgPromotion,
        $"phase243PriorGuRvgFound={phase243PriorGuRvgFound}; phase243PriorGuRvgPromotable={phase243PriorGuRvgPromotable}; phase281PriorGuRvgAuditPassed={phase281PriorGuRvgAuditPassed}; phase281PriorGuRvgPromotesWz={phase281PriorGuRvgPromotesWz}; phase281PriorGuRvgPromotesHiggs={phase281PriorGuRvgPromotesHiggs}"),
    new Check(
        "current-blockers-still-binding",
        currentBlockersStillBinding,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; observedFieldExtractionRequiredFieldCount={observedFieldExtractionRequiredFieldCount}; observedFieldExtractionFilledRequiredFieldCount={observedFieldExtractionFilledRequiredFieldCount}; observedFieldExtractionContractPromotable={observedFieldExtractionContractPromotable}"),
    new Check(
        "current-public-sources-all-nonpromotable",
        currentPublicSourcesAllNonPromotable,
        $"currentPublicSourcesAllNonPromotable={currentPublicSourcesAllNonPromotable}; sourceCount={researchSources.Length}"),
};

var currentPublicGuRvgRevisionDeltaAuditPassed = checks.All(check => check.Passed)
    && !currentPublicGuRvgPromotesWzMasses
    && !currentPublicGuRvgPromotesHiggsMass
    && !currentPublicGuRvgCompletesBosonPredictions;
var terminalStatus = currentPublicGuRvgRevisionDeltaAuditPassed
    ? "current-public-gu-rvg-revision-delta-audit-no-wzh-source-lineage"
    : "current-public-gu-rvg-revision-delta-audit-review-required";

var result = new
{
    phaseId = "phase312-current-public-gu-rvg-revision-delta-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    currentPublicGuRvgResearchPerformedOn,
    currentPublicGuRvgRevisionDeltaAuditPassed,
    currentPublicGuRvgRevisionFound,
    currentPublicGuRvgMentionsShiabObserverseTraceAnomaly,
    currentPublicGuRvgMentions95GeVDilaton,
    currentPublicGuRvgMentionsKoideOr246GevScale,
    currentPublicGuRvgUsesExternalElectroweakVev246Gev,
    currentPublicGuRvgPromotesWzMasses,
    currentPublicGuRvgPromotesHiggsMass,
    currentPublicGuRvgCompletesBosonPredictions,
    currentMaterialStrategyPromotesBosonMasses,
    currentMaterialStrategyFillsSourceLineage,
    currentPublicGuRvgBoundary = new
    {
        currentPublicGuRvgProvidesGuLocalWzTheorem,
        currentPublicGuRvgProvidesSeparateWzSourceRows,
        currentPublicGuRvgProvidesRawAmplitudeGate,
        currentPublicGuRvgProvidesCommonBridgeGate,
        currentPublicGuRvgProvidesTargetIndependentVevSource,
        currentPublicGuRvgProvidesWzMassMatrixSource,
        currentPublicGuRvgProvidesPhotonWzEigenstateProjectionRows,
        currentPublicGuRvgProvidesObservedFieldExtraction,
        currentPublicGuRvgProvidesHiggsScalarSourceOperator,
        currentPublicGuRvgProvidesHiggsIdentityEnvelope,
        currentPublicGuRvgProvidesObservedHiggsMassiveScalarProfile,
        currentPublicGuRvgProvidesHiggsSelfCouplingSource,
    },
    researchSources,
    priorAuditEvidence = new
    {
        phase243 = new
        {
            phase243PriorGuRvgFound,
            phase243PriorGuRvgPromotable,
        },
        phase281 = new
        {
            phase281PriorGuRvgAuditPassed,
            phase281PriorGuRvgPromotesWz,
            phase281PriorGuRvgPromotesHiggs,
        },
    },
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
        phase256 = new
        {
            observedFieldExtractionRequiredFieldCount,
            observedFieldExtractionFilledRequiredFieldCount,
            observedFieldExtractionContractPromotable,
        },
    },
    checks,
    decision = currentPublicGuRvgRevisionDeltaAuditPassed
        ? "Do not promote W/Z or Higgs mass predictions from the current public GU-RVG revision delta. The May 2026 public sources add/revise Koide, 95.4 GeV dilaton, trace-anomaly, Observerse, and material-engineering claims, but they do not provide the repository's required target-independent W/Z source rows, photon/W/Z projection, electroweak VEV/coupling source, or Higgs scalar-source lineage."
        : "Review the current public GU-RVG revision delta before relying on package boundaries.",
    nextRequiredArtifact = new[]
    {
        "A GU-local target-independent W/Z source theorem with separate W and Z source rows and Phase201/P209 gates.",
        "A filled observed-field extraction artifact with photon/W/Z eigenstate projection and electroweak mass-operator fields.",
        "A solved Higgs scalar source/operator/profile/self-coupling lineage independent of observed Higgs or W/Z target masses.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase243Path = Phase243Path,
        phase245Path = Phase245Path,
        phase256Path = Phase256Path,
        phase281Path = Phase281Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "current_public_gu_rvg_revision_delta_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "current_public_gu_rvg_revision_delta_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.currentPublicGuRvgResearchPerformedOn,
        result.currentPublicGuRvgRevisionDeltaAuditPassed,
        result.currentPublicGuRvgRevisionFound,
        result.currentPublicGuRvgMentionsShiabObserverseTraceAnomaly,
        result.currentPublicGuRvgMentions95GeVDilaton,
        result.currentPublicGuRvgMentionsKoideOr246GevScale,
        result.currentPublicGuRvgUsesExternalElectroweakVev246Gev,
        result.currentPublicGuRvgPromotesWzMasses,
        result.currentPublicGuRvgPromotesHiggsMass,
        result.currentPublicGuRvgCompletesBosonPredictions,
        result.currentMaterialStrategyPromotesBosonMasses,
        result.currentMaterialStrategyFillsSourceLineage,
        result.currentPublicGuRvgBoundary,
        result.researchSources,
        result.priorAuditEvidence,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"currentPublicGuRvgRevisionDeltaAuditPassed={currentPublicGuRvgRevisionDeltaAuditPassed}");
Console.WriteLine($"currentPublicGuRvgRevisionFound={currentPublicGuRvgRevisionFound}");
Console.WriteLine($"currentPublicGuRvgPromotesWzMasses={currentPublicGuRvgPromotesWzMasses}");
Console.WriteLine($"currentPublicGuRvgPromotesHiggsMass={currentPublicGuRvgPromotesHiggsMass}");
Console.WriteLine($"currentMaterialStrategyPromotesBosonMasses={currentMaterialStrategyPromotesBosonMasses}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record PublicSourceRow(
    string SourceId,
    string Title,
    string Url,
    string LatestPublicRevisionOrModifiedDate,
    string Finding,
    bool PromotableForWzMasses,
    bool PromotableForHiggsMass,
    bool ProvidesSourceLineageFields,
    bool TargetOrExternalInputsUsed,
    string[] MissingContracts);

sealed record Check(string CheckId, bool Passed, string Detail);
