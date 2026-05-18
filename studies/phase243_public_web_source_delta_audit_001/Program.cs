using System.Text.Json;

const string DefaultOutputDir = "studies/phase243_public_web_source_delta_audit_001/output";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase242Path = "studies/phase242_post_p241_external_lead_consolidation_audit_001/output/post_p241_external_lead_consolidation_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE243_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase242 = JsonDocument.Parse(File.ReadAllText(Phase242Path));

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var jsonIntakeReadyCount = JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? 0;
var textIntakeReadyCount = JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? 0;
var higgsQuarticIntakeReadyCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? 0;

var officialGuSiteStillDraftOnly = true;
var coxSequenceLatestReviewedIToIv = true;
var noCoxPaperVOrBosonMassSourceFound = true;
var recentGuRvgSynthesisFound = true;
var latestGuRvgSynthesisVersionReviewed = "v8";
var recentGuRvgSynthesisPromotableForBosonMasses = false;
var cms2026WMeasurementFound = true;
var cms2026WMeasurementIsTargetOnly = true;
var webDeltaPromotableForBosonMasses = false;
var webDeltaFillsWzSourceLineage = false;
var webDeltaFillsHiggsSourceLineage = false;

var p242BoundaryStillActive =
    JsonBool(phase242.RootElement, "postP241ExternalLeadConsolidationAuditPassed") is true
    && JsonBool(phase242.RootElement, "anyExternalLeadPromotableForBosonMasses") is false
    && JsonBool(phase242.RootElement, "newSourceLineageArtifactRequired") is true;

var newSourceLineageArtifactRequired =
    p242BoundaryStillActive
    && !webDeltaPromotableForBosonMasses
    && !webDeltaFillsWzSourceLineage
    && !webDeltaFillsHiggsSourceLineage
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var webSourceRows = new[]
{
    new WebSourceRow(
        "official-gu-site",
        "https://geometricunity.org/",
        "Official GU site still identifies the public GU draft release as the April 1, 2021 manuscript and provides the 2013 lecture route.",
        PromotableForBosonMasses: false,
        TargetOnly: false,
        MissingContracts: new[]
        {
            "W/Z theoremOrDerivationId",
            "separate W/Z source rows",
            "Higgs scalar-source operator",
            "W/Z/H prediction rows with replay and target-comparison gates",
        }),
    new WebSourceRow(
        "cox-gu-i-through-iv-search-delta",
        "https://www.researchgate.net/publication/396132548_Geometric_Unity_I_From_Heuristic_Proposal_to_Testable_Framework_Shiab_Uniqueness_Invariant_Curvature_Augmented_Torsion_and_Projection-Variation_with_Boundary_Control",
        "Fresh searches found the already-audited Cox I-IV sequence, not a new Cox W/Z/H mass-source paper or source-lineage artifact.",
        PromotableForBosonMasses: false,
        TargetOnly: false,
        MissingContracts: new[]
        {
            "fixed electroweak coupling source",
            "fixed GU-derived electroweak VEV source",
            "Higgs quartic or scalar potential source",
            "physical W/Z/H mass rows",
        }),
    new WebSourceRow(
        "zenodo-geometric-refractive-unification-delta",
        "https://zenodo.org/records/19465143",
        "The latest found 2026 GU/RVG synthesis version is an April 7, 2026 v8 holographic/metric-engineering synthesis lead, not a GU W/Z/H source-lineage derivation for the repository contracts.",
        PromotableForBosonMasses: false,
        TargetOnly: false,
        MissingContracts: new[]
        {
            "Phase201 W/Z source-lineage fields",
            "Phase201 Higgs scalar-source fields",
            "target-independent GU replay gates",
        }),
    new WebSourceRow(
        "cms-2026-w-mass-measurement",
        "https://news.mit.edu/2026/physicists-report-mass-fundamental-w-boson-particle-0408",
        "The 2026 CMS W-mass measurement is current experimental target context, not a source derivation. It cannot fill GU sourceLineageId, theoremOrDerivationId, scalarSourceOperatorId, or replay gates.",
        PromotableForBosonMasses: false,
        TargetOnly: true,
        MissingContracts: new[]
        {
            "target-independent source construction",
            "GU W/Z bridge theorem",
            "GU Higgs scalar-source derivation",
        }),
};

var searchRows = new[]
{
    new SearchRow("Geometric Unity W boson mass Higgs mass prediction source lineage 2026", "No new GU source-lineage artifact found; results were current W-mass measurement context, official GU site, and unrelated boson-mass models."),
    new SearchRow("Joseph Thomas Cox Geometric Unity boson mass W Z Higgs", "Returned the previously audited Cox I-IV sequence and generic particle-physics references, not a new W/Z/H source-lineage artifact."),
    new SearchRow("site:zenodo.org/records Geometric Unity Joseph Cox W Z Higgs mass", "Found unrelated or non-source-lineage GU-adjacent Zenodo records; no Cox W/Z/H source-lineage record was found."),
};

var checks = new[]
{
    new Check("p242-boundary-still-active", p242BoundaryStillActive, $"p242BoundaryStillActive={p242BoundaryStillActive}"),
    new Check("official-gu-web-delta-no-completion-source", officialGuSiteStillDraftOnly && !webSourceRows.Single(row => row.SourceId == "official-gu-site").PromotableForBosonMasses, $"officialGuSiteStillDraftOnly={officialGuSiteStillDraftOnly}"),
    new Check("cox-web-delta-no-new-source-lineage", coxSequenceLatestReviewedIToIv && noCoxPaperVOrBosonMassSourceFound && !webSourceRows.Single(row => row.SourceId == "cox-gu-i-through-iv-search-delta").PromotableForBosonMasses, $"coxSequenceLatestReviewedIToIv={coxSequenceLatestReviewedIToIv}; noCoxPaperVOrBosonMassSourceFound={noCoxPaperVOrBosonMassSourceFound}"),
    new Check("gu-rvg-delta-not-wzh-source-lineage", recentGuRvgSynthesisFound && latestGuRvgSynthesisVersionReviewed == "v8" && !recentGuRvgSynthesisPromotableForBosonMasses, $"recentGuRvgSynthesisFound={recentGuRvgSynthesisFound}; latestGuRvgSynthesisVersionReviewed={latestGuRvgSynthesisVersionReviewed}; recentGuRvgSynthesisPromotableForBosonMasses={recentGuRvgSynthesisPromotableForBosonMasses}"),
    new Check("cms-w-mass-target-only", cms2026WMeasurementFound && cms2026WMeasurementIsTargetOnly && !webSourceRows.Single(row => row.SourceId == "cms-2026-w-mass-measurement").PromotableForBosonMasses, $"cms2026WMeasurementFound={cms2026WMeasurementFound}; cms2026WMeasurementIsTargetOnly={cms2026WMeasurementIsTargetOnly}"),
    new Check("repo-scans-remain-empty", jsonIntakeReadyCount == 0 && textIntakeReadyCount == 0 && higgsQuarticIntakeReadyCount == 0, $"jsonIntakeReadyCount={jsonIntakeReadyCount}; textIntakeReadyCount={textIntakeReadyCount}; higgsQuarticIntakeReadyCount={higgsQuarticIntakeReadyCount}"),
    new Check("source-lineage-blockers-preserved", wzMissingFieldCount == 15 && higgsMissingFieldCount == 14, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("web-delta-not-promotable", !webDeltaPromotableForBosonMasses && !webDeltaFillsWzSourceLineage && !webDeltaFillsHiggsSourceLineage, $"webDeltaPromotableForBosonMasses={webDeltaPromotableForBosonMasses}; webDeltaFillsWzSourceLineage={webDeltaFillsWzSourceLineage}; webDeltaFillsHiggsSourceLineage={webDeltaFillsHiggsSourceLineage}"),
    new Check("new-source-lineage-artifact-still-required", newSourceLineageArtifactRequired, $"newSourceLineageArtifactRequired={newSourceLineageArtifactRequired}"),
};

var publicWebSourceDeltaAuditPassed = checks.All(check => check.Passed)
    && !webDeltaPromotableForBosonMasses
    && webSourceRows.All(row => !row.PromotableForBosonMasses)
    && newSourceLineageArtifactRequired;
var terminalStatus = publicWebSourceDeltaAuditPassed
    ? "public-web-source-delta-audit-no-new-wzh-source-lineage"
    : "public-web-source-delta-audit-review-required";

var result = new
{
    phaseId = "phase243-public-web-source-delta-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    webSearchPerformedOn = "2026-05-17",
    publicWebSourceDeltaAuditPassed,
    webDeltaPromotableForBosonMasses,
    webDeltaFillsWzSourceLineage,
    webDeltaFillsHiggsSourceLineage,
    officialGuSiteStillDraftOnly,
    coxSequenceLatestReviewedIToIv,
    noCoxPaperVOrBosonMassSourceFound,
    recentGuRvgSynthesisFound,
    latestGuRvgSynthesisVersionReviewed,
    recentGuRvgSynthesisPromotableForBosonMasses,
    cms2026WMeasurementFound,
    cms2026WMeasurementIsTargetOnly,
    newSourceLineageArtifactRequired,
    objective = "Record a fresh public web/source delta search after P242 and determine whether any newly found or missed public source fills W/Z/H boson mass source-lineage contracts.",
    searchRows,
    webSourceRows,
    currentRepoEvidence = new
    {
        phase204 = new
        {
            status = JsonString(phase204.RootElement, "terminalStatus"),
            jsonIntakeReadyCount,
        },
        phase205 = new
        {
            status = JsonString(phase205.RootElement, "terminalStatus"),
            textIntakeReadyCount,
        },
        phase207 = new
        {
            status = JsonString(phase207.RootElement, "terminalStatus"),
            higgsQuarticIntakeReadyCount,
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase242 = new
        {
            status = JsonString(phase242.RootElement, "terminalStatus"),
            p242BoundaryStillActive,
            anyExternalLeadPromotableForBosonMasses = JsonBool(phase242.RootElement, "anyExternalLeadPromotableForBosonMasses"),
            newSourceLineageArtifactRequired = JsonBool(phase242.RootElement, "newSourceLineageArtifactRequired"),
        },
    },
    checks,
    decision = publicWebSourceDeltaAuditPassed
        ? "Do not promote the fresh public web delta as W/Z/H predictions. The current web findings are either already-audited GU/Cox leads, non-source-lineage GU-adjacent material, or target measurements; new derivation-backed W/Z and Higgs source-lineage artifacts are still required."
        : "Review public web source delta evidence before relying on package boundaries.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed, target-independent W/Z absolute source lineage with separate W and Z source rows and all P209 gates true.",
        "A solved, target-independent Higgs scalar source/operator lineage with identity envelope, massive profile, coupling or excitation source, stability sidecars, and a passing prediction row.",
    },
    sourceEvidence = new
    {
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase242Path = Phase242Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "public_web_source_delta_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "public_web_source_delta_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.webSearchPerformedOn,
        result.publicWebSourceDeltaAuditPassed,
        result.webDeltaPromotableForBosonMasses,
        result.webDeltaFillsWzSourceLineage,
        result.webDeltaFillsHiggsSourceLineage,
        result.officialGuSiteStillDraftOnly,
        result.coxSequenceLatestReviewedIToIv,
        result.noCoxPaperVOrBosonMassSourceFound,
        result.recentGuRvgSynthesisFound,
        result.latestGuRvgSynthesisVersionReviewed,
        result.recentGuRvgSynthesisPromotableForBosonMasses,
        result.cms2026WMeasurementFound,
        result.cms2026WMeasurementIsTargetOnly,
        result.newSourceLineageArtifactRequired,
        result.searchRows,
        result.webSourceRows,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"publicWebSourceDeltaAuditPassed={publicWebSourceDeltaAuditPassed}");
Console.WriteLine($"webDeltaPromotableForBosonMasses={webDeltaPromotableForBosonMasses}");
Console.WriteLine($"newSourceLineageArtifactRequired={newSourceLineageArtifactRequired}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SearchRow(string Query, string Outcome);
sealed record WebSourceRow(string SourceId, string Url, string Finding, bool PromotableForBosonMasses, bool TargetOnly, string[] MissingContracts);
sealed record Check(string CheckId, bool Passed, string Detail);
