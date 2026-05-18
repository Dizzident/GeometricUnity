using System.Text.Json;

const string DefaultOutputDir = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase112Path = "studies/phase112_scalar_sector_relation_revision_attempt_001/output/scalar_sector_relation_revision_attempt_summary.json";
const string Phase184Path = "studies/phase184_massive_boson_prediction_closure_001/output/massive_boson_prediction_closure_summary.json";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase194Path = "studies/phase194_draft_boson_source_evidence_audit_001/output/draft_boson_source_evidence_audit_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE199_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase112 = JsonDocument.Parse(File.ReadAllText(Phase112Path));
using var phase184 = JsonDocument.Parse(File.ReadAllText(Phase184Path));
using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));
using var phase194 = JsonDocument.Parse(File.ReadAllText(Phase194Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));

var p70ScalarBridgeDerived = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
var p112IndependentRevisionEvidencePresent = JsonBool(phase112.RootElement, "independentRevisionEvidencePresent") is true;
var p112RepairAccepted = JsonBool(phase112.RootElement, "repairAccepted") is true;
var p184AnyNewMassiveAttemptAllowed = JsonBool(phase184.RootElement, "anyNewMassiveAttemptAllowed") is true;
var p187ScaffoldMaterialized = JsonBool(phase187.RootElement, "scaffoldMaterialized") is true;
var p187HiggsSourceIdentityValidated = JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;
var p187PredictionAttemptAllowed = JsonBool(phase187.RootElement, "predictionAttemptAllowed") is true;
var p189CensusPromotable = JsonBool(phase189.RootElement, "censusPromotable") is true;
var p189PredictionAttemptAllowed = JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;
var p194DraftProvidesSolvedHiggsSource = JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource") is true;
var p196PotentialClosurePassed = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;

var candidateSummary = phase189.RootElement.TryGetProperty("candidateSummary", out var p189CandidateSummary)
    ? new
    {
        candidateCount = JsonInt(p189CandidateSummary, "candidateCount"),
        scalarFeatureEnvelopeCount = JsonInt(p189CandidateSummary, "scalarFeatureEnvelopeCount"),
        branchStableNonC0Count = JsonInt(p189CandidateSummary, "branchStableNonC0Count"),
        massiveScalarProfileCount = JsonInt(p189CandidateSummary, "massiveScalarProfileCount"),
        maxAbsMassLike = JsonDouble(p189CandidateSummary, "maxAbsMassLike"),
    }
    : null;

var sourceLineages = new[]
{
    new SourceLineage(
        "phase70-electroweak-vev-order-parameter",
        "insufficient-not-higgs-excitation",
        false,
        "Phase70 derives an electroweak VEV/order-parameter bridge for mass generation, but it is not a solved Higgs excitation, potential, or scalar source/operator.",
        new[]
        {
            $"phase70ScalarBridgeDerived={p70ScalarBridgeDerived}",
            $"sourceRelationId={JsonString(phase70.RootElement, "sourceRelationId")}",
        }),
    new SourceLineage(
        "phase112-scalar-relation-revision",
        p112IndependentRevisionEvidencePresent && p112RepairAccepted ? "promotable" : "diagnostic-only",
        p112IndependentRevisionEvidencePresent && p112RepairAccepted,
        "Phase112 rejects scalar-sector relation repair because no independent revision evidence exists; its target-implied quantities are diagnostic only.",
        new[]
        {
            $"independentRevisionEvidencePresent={p112IndependentRevisionEvidencePresent}",
            $"repairAccepted={p112RepairAccepted}",
        }),
    new SourceLineage(
        "phase187-higgs-source-identity-scaffold",
        p187HiggsSourceIdentityValidated && p187PredictionAttemptAllowed ? "promotable" : "scaffold-only",
        p187HiggsSourceIdentityValidated && p187PredictionAttemptAllowed,
        "Phase187 materializes a fail-closed scaffold, but source/operator, identity features, and stability sidecars are absent.",
        new[]
        {
            $"scaffoldMaterialized={p187ScaffoldMaterialized}",
            $"higgsSourceIdentityValidated={p187HiggsSourceIdentityValidated}",
            $"predictionAttemptAllowed={p187PredictionAttemptAllowed}",
        }),
    new SourceLineage(
        "phase189-higgs-scalar-source-operator-census",
        p189CensusPromotable && p189PredictionAttemptAllowed ? "promotable" : "no-solved-source",
        p189CensusPromotable && p189PredictionAttemptAllowed,
        "Phase189 finds no solved scalar-sector source/operator, no scalar identity envelopes, no branch-stable non-C0 scalar candidate, and no massive scalar profile.",
        new[]
        {
            $"censusPromotable={p189CensusPromotable}",
            $"predictionAttemptAllowed={p189PredictionAttemptAllowed}",
            $"candidateSummary={JsonSerializer.Serialize(candidateSummary)}",
        }),
    new SourceLineage(
        "phase196-potential-self-coupling",
        p196PotentialClosurePassed ? "promotable" : "no-potential-or-self-coupling-source",
        p196PotentialClosurePassed,
        "Phase196 rejects current scalar/selector artifacts as a Higgs potential or self-coupling source.",
        new[]
        {
            $"canPromoteHiggsFromPotentialOrSelfCoupling={p196PotentialClosurePassed}",
        }),
    new SourceLineage(
        "phase194-draft-higgs-source",
        p194DraftProvidesSolvedHiggsSource ? "promotable" : "draft-open-conjectural",
        p194DraftProvidesSolvedHiggsSource,
        "Phase194 finds the draft classifies the Higgs-like sector as open, approximate, conjectural, or requiring explicit scalar spectrum/potential/vacuum/coupling extraction.",
        new[]
        {
            $"draftProvidesSolvedHiggsSource={p194DraftProvidesSolvedHiggsSource}",
        }),
};

var canPromoteAnyHiggsScalarSourceLineage = sourceLineages.Any(row => row.Promotable);
var terminalStatus = canPromoteAnyHiggsScalarSourceLineage
    ? "higgs-scalar-source-lineage-closure-promotable"
    : "higgs-scalar-source-lineage-closure-no-promotable-source";

var result = new
{
    phaseId = "phase199-higgs-scalar-source-lineage-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteAnyHiggsScalarSourceLineage,
    p184AnyNewMassiveAttemptAllowed,
    candidateSummary,
    sourceLineages,
    decision = canPromoteAnyHiggsScalarSourceLineage
        ? "A Higgs scalar-source lineage is promotable for a physical mass prediction attempt."
        : "Do not promote a Higgs mass prediction from current scalar lineages. The VEV bridge is not a Higgs excitation source, scalar-relation repair is diagnostic-only, the scaffold/census find no solved source/operator or massive scalar profile, potential/self-coupling closure fails, and the draft does not supply a solved Higgs source.",
    nextRequiredArtifact = "A solved target-independent scalar-sector source/operator with Higgs identity envelopes, massive scalar profile, potential/self-coupling or excitation relation, and branch/refinement/environment/representation/coupling stability sidecars.",
    sourceEvidence = new
    {
        phase70Path = Phase70Path,
        phase112Path = Phase112Path,
        phase184Path = Phase184Path,
        phase187Path = Phase187Path,
        phase189Path = Phase189Path,
        phase194Path = Phase194Path,
        phase196Path = Phase196Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_scalar_source_lineage_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_scalar_source_lineage_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteAnyHiggsScalarSourceLineage,
        result.p184AnyNewMassiveAttemptAllowed,
        result.candidateSummary,
        result.sourceLineages,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteAnyHiggsScalarSourceLineage={canPromoteAnyHiggsScalarSourceLineage}");
Console.WriteLine($"p184AnyNewMassiveAttemptAllowed={p184AnyNewMassiveAttemptAllowed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record SourceLineage(
    string LineageId,
    string Status,
    bool Promotable,
    string Finding,
    IReadOnlyList<string> Evidence);
