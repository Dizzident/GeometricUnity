using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase158_external_gu_source_unblocker_audit_001/output";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase155Path = "studies/phase155_fermion_sector_transition_evidence_derivation_001/output/fermion_sector_transition_evidence_derivation.json";
const string Phase157Path = "studies/phase157_scientific_defensibility_boundary_001/output/scientific_defensibility_boundary.json";
const string LocalDraftPath = "/home/josh/Documents/Geometric_Unity-Draft-April-1st-2021.pdf";

var outputDir = Environment.GetEnvironmentVariable("PHASE158_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase155 = JsonDocument.Parse(File.ReadAllText(Phase155Path));
using var phase157 = JsonDocument.Parse(File.ReadAllText(Phase157Path));

var sourceClaims = new[]
{
    SourceClaim(
        "gu-draft-topological-spinor-internal-quantum-numbers",
        "Geometric Unity draft, section 4 and 4.2",
        "The draft motivates internal quantum numbers from topological spinors and reduction of Spin(6,4) through Spin(6) x Spin(4), identified with SU(4) x SU(2) x SU(2), then toward SU(3) x SU(2) x U(1).",
        "supports-source-derived-sector-taxonomy",
        true),
    SourceClaim(
        "gu-draft-rabi-order-fermion-table",
        "Geometric Unity draft, sections 11.1-11.4",
        "The draft lists predicted fermion quantum-number blocks, including structures such as [3 x 2], [1 x 2], anti-quark and anti-lepton blocks, chirality marker L, n labels, and electric-charge rows for additional sectors.",
        "supports-source-derived-fermion-label-candidates",
        true),
    SourceClaim(
        "gu-draft-location-table",
        "Geometric Unity draft appendix",
        "The draft location table maps Standard Model first generation fermions to the pulled-back nu field, later generations to the pulled-back zeta field, and weak isospin/hypercharge to Spin(4) and Spin(6) x Spin(4) normal-bundle locations.",
        "supports-source-derived-family-and-force-locations",
        true),
    SourceClaim(
        "official-gu-site-and-2013-lecture",
        "https://geometricunity.org/ and https://geometricunity.org/2013-oxford-lecture/",
        "Official public GU pages describe the draft release and the 2013 lecture as a broad overview with general predictions and fermionic field content.",
        "supports-provenance-only",
        true),
    SourceClaim(
        "user-supplied-video-athfaxf7mgw",
        "https://www.youtube.com/watch?v=AThFAxF7Mgw",
        "Public search results identify this as a 2025 Curt Jaimungal Geometric Unity explainer, but no primary transcript or row-keyed mathematical derivation was available in the accessible public text.",
        "metadata-only-not-promotable",
        false),
    SourceClaim(
        "standard-electroweak-current-rules",
        "PDG Standard Model/electroweak review and standard Pati-Salam context",
        "Standard sources support generic W charged-current and Z neutral-current rules, but those rules do not identify the repo's repaired GU fermion modes or close the GU-specific bridge.",
        "supports-generic-rule-context-only",
        true),
};

var p140Assessment = new[]
{
    Requirement(
        "familyId/candidateId/sourceCanonicalFermionModeId keyed rows",
        "not-satisfied",
        "The draft and public pages name GU fields and representation blocks, not the current repaired repo rows or source canonical fermion mode ids."),
    Requirement(
        "chargeSector",
        "partially-supported",
        "The draft gives charge-like and electric-charge sector information at representation-block level."),
    Requirement(
        "weakSector or quantumNumbers",
        "partially-supported",
        "The draft gives weak-isospin/Pati-Salam quantum-number blocks and normal-bundle locations."),
    Requirement(
        "derivationId",
        "satisfied-for-source-metadata",
        "A provenance derivation id can be assigned to the GU draft audit, but it cannot by itself identify repaired rows."),
    Requirement(
        "externalTargetValuesUsed=false",
        "satisfied",
        "The draft/source labels predate this W/Z residual workflow and are target-independent with respect to current W/Z targets."),
    Requirement(
        "nontrivial transition rule or directed coupling bridge",
        "not-satisfied",
        "No accessible source provides a directed W/Z transition rule keyed to the current repaired family rows or a canonical map from repaired numerical modes to Pati-Salam branch weights."),
};

bool hasModeKeyedRows = p140Assessment.Any(x => x.RequirementId.StartsWith("familyId/", StringComparison.Ordinal) && x.Status == "satisfied");
bool hasTransitionBridge = p140Assessment.Any(x => x.RequirementId.StartsWith("nontrivial", StringComparison.Ordinal) && x.Status == "satisfied");
bool sourceBackedMetadataPresent = sourceClaims.Any(x => x.TargetIndependent);
bool p140Promotable = hasModeKeyedRows && hasTransitionBridge;
string terminalStatus = p140Promotable
    ? "external-gu-source-unblocker-p140-ready"
    : "external-gu-source-unblocker-insufficient-for-p140";

var result = new
{
    phaseId = "phase158-external-gu-source-unblocker-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canDefensiblyProgressFromExternalGuSources = p140Promotable,
    sourceBackedMetadataPresent,
    sourceClaims,
    p140Assessment,
    recommendedArtifactClassification = p140Promotable
        ? "promotable-p140-intake-candidate"
        : "source-backed-fermion-sector-derivation-metadata-not-promotable",
    concreteBlocker = "A canonical target-independent map is still missing from current repaired fermion rows to GU/Pati-Salam branch labels, or equivalently a directed W/Z transition bridge keyed to those rows.",
    allowedNextArtifacts = new[]
    {
        "mode-to-branch derivation keyed by familyId/candidateId/sourceCanonicalFermionModeId",
        "nontrivial chirality/conjugation transition table keyed by repaired row ids",
        "directed coupling-transition rule combined with target-blind charge/weak-sector derivation",
    },
    rejectedNextArtifacts = new[]
    {
        "representation-block labels without a row-keyed map",
        "generic Standard Model W/Z current rules without GU repaired-mode identity",
        "video summary or commentary without a primary transcript and mathematical derivation",
        "assignments selected because they improve W/Z residuals",
    },
    bosonPredictionState = new
    {
        phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
        validatedPredictionCount = JsonInt(phase151.RootElement, "validatedPredictionCount"),
        failedAttemptCount = JsonInt(phase151.RootElement, "failedAttemptCount"),
        blockedRowCount = JsonInt(phase151.RootElement, "blockedRowCount"),
    },
    upstreamBlockers = new
    {
        phase140Status = JsonString(phase140.RootElement, "terminalStatus"),
        phase155Status = JsonString(phase155.RootElement, "terminalStatus"),
        phase155FinalKnownBlocker = JsonString(phase155.RootElement, "finalKnownBlocker"),
        phase157Status = JsonString(phase157.RootElement, "terminalStatus"),
        phase157CanDefensiblyProgress = JsonBool(phase157.RootElement, "canDefensiblyProgressFromCurrentLocalArtifacts"),
    },
    sourceEvidence = new
    {
        localDraftPath = File.Exists(LocalDraftPath) ? LocalDraftPath : null,
        officialGuSite = "https://geometricunity.org/",
        officialLectureTranscript = "https://geometricunity.org/2013-oxford-lecture/",
        userSuppliedVideo = "https://www.youtube.com/watch?v=AThFAxF7Mgw",
        phase140Path = Phase140Path,
        phase151Path = Phase151Path,
        phase155Path = Phase155Path,
        phase157Path = Phase157Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "external_gu_source_unblocker_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "external_gu_source_unblocker_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canDefensiblyProgressFromExternalGuSources,
        result.recommendedArtifactClassification,
        result.concreteBlocker,
        result.bosonPredictionState,
        result.upstreamBlockers,
        result.allowedNextArtifacts,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "external_gu_source_unblocker_audit.md"),
    BuildMarkdown(terminalStatus, result.recommendedArtifactClassification, result.concreteBlocker, sourceClaims, p140Assessment));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sourceBackedMetadataPresent={sourceBackedMetadataPresent}");
Console.WriteLine($"canDefensiblyProgressFromExternalGuSources={p140Promotable}");

static SourceClaimRecord SourceClaim(string claimId, string source, string claim, string assessment, bool targetIndependent) =>
    new(claimId, source, claim, assessment, targetIndependent);

static RequirementAssessment Requirement(string requirementId, string status, string evidence) =>
    new(requirementId, status, evidence);

static string BuildMarkdown(
    string terminalStatus,
    string recommendedArtifactClassification,
    string concreteBlocker,
    IReadOnlyList<SourceClaimRecord> sourceClaims,
    IReadOnlyList<RequirementAssessment> p140Assessment)
{
    var builder = new StringBuilder();
    builder.AppendLine("# External GU Source Unblocker Audit");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine($"Classification: `{recommendedArtifactClassification}`");
    builder.AppendLine();
    builder.AppendLine($"Concrete blocker: {concreteBlocker}");
    builder.AppendLine();
    builder.AppendLine("## Source Claims");
    foreach (var claim in sourceClaims)
        builder.AppendLine($"- `{claim.ClaimId}`: {claim.Assessment}");
    builder.AppendLine();
    builder.AppendLine("## P140 Assessment");
    foreach (var requirement in p140Assessment)
        builder.AppendLine($"- `{requirement.RequirementId}`: `{requirement.Status}` - {requirement.Evidence}");
    return builder.ToString();
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record SourceClaimRecord(string ClaimId, string Source, string Claim, string Assessment, bool TargetIndependent);
sealed record RequirementAssessment(string RequirementId, string Status, string Evidence);
