using System.Text.Json;

const string DefaultOutputDir = "studies/phase248_higgs_scalar_repairability_audit_001/output";
const string RegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase246Path = "studies/phase246_minimal_unlock_candidate_inventory_001/output/minimal_unlock_candidate_inventory_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE248_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var registry = JsonDocument.Parse(File.ReadAllText(RegistryPath));
using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase246 = JsonDocument.Parse(File.ReadAllText(Phase246Path));

var registryCandidates = registry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(row => new ScalarRegistryCandidate(
        JsonString(row, "candidateId") ?? "missing",
        JsonString(row, "claimClass") ?? "missing",
        JsonDouble(row, "branchStabilityScore"),
        row.TryGetProperty("polarizationEnvelope", out var polarizationEnvelope) && polarizationEnvelope.ValueKind != JsonValueKind.Null,
        row.TryGetProperty("symmetryEnvelope", out var symmetryEnvelope) && symmetryEnvelope.ValueKind != JsonValueKind.Null,
        row.TryGetProperty("interactionProxyEnvelope", out var interactionProxyEnvelope) && interactionProxyEnvelope.ValueKind != JsonValueKind.Null,
        NumberArray(row, "massLikeEnvelope"),
        NumberArray(row, "gaugeLeakEnvelope")))
    .ToArray();

var p187HiggsSourceIdentityValidated = JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;
var p187PredictionAttemptAllowed = JsonBool(phase187.RootElement, "predictionAttemptAllowed") is true;
var p189CensusPromotable = JsonBool(phase189.RootElement, "censusPromotable") is true;
var p189PredictionAttemptAllowed = JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;
var p189CandidateSummary = phase189.RootElement.GetProperty("candidateSummary");
var p189ScalarFeatureEnvelopeCount = JsonInt(p189CandidateSummary, "scalarFeatureEnvelopeCount") ?? -1;
var p189BranchStableNonC0Count = JsonInt(p189CandidateSummary, "branchStableNonC0Count") ?? -1;
var p189MassiveScalarProfileCount = JsonInt(p189CandidateSummary, "massiveScalarProfileCount") ?? -1;
var p196CanPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var p199CanPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var p207CanPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var p207IntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? -1;
var p215CanPromoteTargetImpliedHiggsSelfCoupling = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var p229VevPromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var p213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p246AnyCandidateFillsHiggs = JsonBool(phase246.RootElement, "anyCandidateFillsHiggsScalarScaleUnlock") is true;

var p223NumericalLeadPresent = JsonBool(phase223.RootElement, "numericalLeadPresent") is true;
var p223CanPromoteLead = JsonBool(phase223.RootElement, "canPromoteHiggsCasimirQuarticLead") is true;
var p223SourceLineagePromotable = JsonBool(phase223.RootElement, "sourceLineagePromotable") is true;
var p223BestProbe = phase223.RootElement.GetProperty("bestProbe");
var p223BestFactor = JsonString(p223BestProbe, "factorExpression") ?? "missing";
var p223BestQuartic = JsonDouble(p223BestProbe, "quarticFromCasimirG2");
var p223BestHiggsMass = JsonDouble(p223BestProbe, "replayedHiggsMassGeV");
var p223BestProbeSourceDerived = JsonBool(p223BestProbe, "sourceDerived") is true;
var targetDiagnostic = phase223.RootElement.GetProperty("targetDiagnostic");
var targetImpliedQuartic = JsonDouble(targetDiagnostic, "targetImpliedQuartic");
var targetQuarticOverCasimirG2 = JsonDouble(targetDiagnostic, "targetQuarticOverCasimirG2");

var currentRegistryHasScalarIdentityFeatures = registryCandidates.Any(row =>
    row.PolarizationEnvelopePresent || row.SymmetryEnvelopePresent || row.InteractionProxyEnvelopePresent);
var currentRegistryHasBranchStableScalarCandidate = registryCandidates.Any(row =>
    (row.BranchStabilityScore ?? 0) >= 0.5 && row.ClaimClass != "C0_NumericalMode");
var currentRegistryHasMassiveScalarProfile = registryCandidates.Any(row =>
    row.MassLikeEnvelope.Any(value => Math.Abs(value) >= 0.01 && Math.Abs(value) <= 100.0)
    && row.GaugeLeakEnvelope.All(value => Math.Abs(value) <= 0.10));
var currentRegistryCandidatesAreOnlyNumericalModes = registryCandidates.Length > 0
    && registryCandidates.All(row => row.ClaimClass == "C0_NumericalMode");
var threeTenthsFactorDerivableFromCurrentScalarSource = p223BestFactor == "3/10"
    && p223BestProbeSourceDerived
    && p189CensusPromotable
    && p196CanPromoteHiggsFromPotentialOrSelfCoupling;
var currentHiggsNumericalLeadPromotable = p223NumericalLeadPresent
    && p223CanPromoteLead
    && p223SourceLineagePromotable
    && threeTenthsFactorDerivableFromCurrentScalarSource;
var higgsScalarSourceRepairPossibleFromCurrentRegistry = p187HiggsSourceIdentityValidated
    && p189CensusPromotable
    && p196CanPromoteHiggsFromPotentialOrSelfCoupling
    && p207CanPromoteHiggsQuarticSelfCouplingSource
    && currentRegistryHasScalarIdentityFeatures
    && currentRegistryHasBranchStableScalarCandidate
    && currentRegistryHasMassiveScalarProfile;
var newHiggsScalarSourceStillRequired = !currentHiggsNumericalLeadPromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && !p245UnlockContractFilled
    && !p246AnyCandidateFillsHiggs
    && p213HiggsMissingFieldCount == 14;

var blockerRows = new[]
{
    new BlockerRow("scalar-source-identity", !p187HiggsSourceIdentityValidated, "P187 has no validated Higgs source identity or target-independent identity envelope."),
    new BlockerRow("scalar-source-operator", !p189CensusPromotable, "P189 finds no solved scalar source/operator, no identity feature envelopes, no branch-stable non-C0 scalar candidate, and no massive scalar profile."),
    new BlockerRow("higgs-potential-or-self-coupling-source", !p196CanPromoteHiggsFromPotentialOrSelfCoupling, "P196 finds no target-independent Higgs potential or self-coupling source."),
    new BlockerRow("higgs-scalar-source-lineage", !p199CanPromoteAnyHiggsScalarSourceLineage, "P199 finds no promotable Higgs scalar-source lineage."),
    new BlockerRow("quartic-self-coupling-source-scan", !p207CanPromoteHiggsQuarticSelfCouplingSource && p207IntakeReadyFindingCount == 0, "P207 finds zero intake-ready Higgs quartic/self-coupling source findings."),
    new BlockerRow("three-tenths-factor-source", !threeTenthsFactorDerivableFromCurrentScalarSource, "P223's 3/10 factor is numerical only; no scalar source/operator derives it."),
    new BlockerRow("target-implied-quartic-loophole", !p215CanPromoteTargetImpliedHiggsSelfCoupling, "P215 blocks deriving lambda from the observed Higgs mass and replaying it as a prediction."),
    new BlockerRow("external-vev-source", !p229VevPromotable, "P229 blocks using the Fermi-derived external VEV as a target-independent GU scalar source."),
    new BlockerRow("registry-scalar-profile", !currentRegistryHasScalarIdentityFeatures && !currentRegistryHasMassiveScalarProfile && currentRegistryCandidatesAreOnlyNumericalModes, "Current scalar registry candidates are C0 numerical modes with no scalar identity envelopes or massive scalar profile."),
};

var checks = new[]
{
    new Check("p223-three-tenths-lead-recorded", p223NumericalLeadPresent && p223BestFactor == "3/10" && !p223SourceLineagePromotable && !p223CanPromoteLead, $"numericalLeadPresent={p223NumericalLeadPresent}; factor={p223BestFactor}; sourceLineagePromotable={p223SourceLineagePromotable}; canPromoteLead={p223CanPromoteLead}; replayedHiggsMass={FormatMaybe(p223BestHiggsMass)}"),
    new Check("three-tenths-not-source-derived", !threeTenthsFactorDerivableFromCurrentScalarSource && !p223BestProbeSourceDerived, $"threeTenthsFactorDerivableFromCurrentScalarSource={threeTenthsFactorDerivableFromCurrentScalarSource}; sourceDerived={p223BestProbeSourceDerived}; quartic={FormatMaybe(p223BestQuartic)}"),
    new Check("p187-p189-source-identity-operator-blocked", !p187HiggsSourceIdentityValidated && !p187PredictionAttemptAllowed && !p189CensusPromotable && !p189PredictionAttemptAllowed, $"p187HiggsSourceIdentityValidated={p187HiggsSourceIdentityValidated}; p189CensusPromotable={p189CensusPromotable}; p189PredictionAttemptAllowed={p189PredictionAttemptAllowed}"),
    new Check("registry-has-no-scalar-repair-data", !currentRegistryHasScalarIdentityFeatures && !currentRegistryHasBranchStableScalarCandidate && !currentRegistryHasMassiveScalarProfile && currentRegistryCandidatesAreOnlyNumericalModes, $"registryCandidateCount={registryCandidates.Length}; scalarIdentityFeatures={currentRegistryHasScalarIdentityFeatures}; branchStableScalar={currentRegistryHasBranchStableScalarCandidate}; massiveScalarProfile={currentRegistryHasMassiveScalarProfile}; onlyNumericalModes={currentRegistryCandidatesAreOnlyNumericalModes}"),
    new Check("potential-quartic-lineage-blocked", !p196CanPromoteHiggsFromPotentialOrSelfCoupling && !p199CanPromoteAnyHiggsScalarSourceLineage && !p207CanPromoteHiggsQuarticSelfCouplingSource && p207IntakeReadyFindingCount == 0, $"p196CanPromote={p196CanPromoteHiggsFromPotentialOrSelfCoupling}; p199CanPromote={p199CanPromoteAnyHiggsScalarSourceLineage}; p207CanPromote={p207CanPromoteHiggsQuarticSelfCouplingSource}; p207IntakeReadyFindingCount={p207IntakeReadyFindingCount}"),
    new Check("target-and-external-loopholes-closed", !p215CanPromoteTargetImpliedHiggsSelfCoupling && !p229VevPromotable, $"p215CanPromoteTargetImpliedHiggsSelfCoupling={p215CanPromoteTargetImpliedHiggsSelfCoupling}; p229VevPromotable={p229VevPromotable}; targetImpliedQuartic={FormatMaybe(targetImpliedQuartic)}; targetQuarticOverCasimirG2={FormatMaybe(targetQuarticOverCasimirG2)}"),
    new Check("p245-p246-higgs-unlock-still-unfilled", !p245UnlockContractFilled && !p246AnyCandidateFillsHiggs && p213HiggsMissingFieldCount == 14, $"p245UnlockContractFilled={p245UnlockContractFilled}; p246AnyCandidateFillsHiggs={p246AnyCandidateFillsHiggs}; p213HiggsMissingFieldCount={p213HiggsMissingFieldCount}"),
    new Check("new-higgs-scalar-source-still-required", newHiggsScalarSourceStillRequired, $"newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}"),
};

var higgsScalarRepairabilityAuditPassed = checks.All(check => check.Passed)
    && blockerRows.All(row => row.Active)
    && !currentHiggsNumericalLeadPromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry;
var terminalStatus = higgsScalarRepairabilityAuditPassed
    ? "higgs-scalar-repairability-audit-complete-new-source-required"
    : "higgs-scalar-repairability-audit-review-required";

var result = new
{
    phaseId = "phase248-higgs-scalar-repairability-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    higgsScalarRepairabilityAuditPassed,
    currentHiggsNumericalLeadPromotable,
    higgsScalarSourceRepairPossibleFromCurrentRegistry,
    threeTenthsFactorDerivableFromCurrentScalarSource,
    currentRegistryHasScalarIdentityFeatures,
    currentRegistryHasBranchStableScalarCandidate,
    currentRegistryHasMassiveScalarProfile,
    currentRegistryCandidatesAreOnlyNumericalModes,
    newHiggsScalarSourceStillRequired,
    p223 = new
    {
        numericalLeadPresent = p223NumericalLeadPresent,
        bestFactor = p223BestFactor,
        quarticFromCasimirG2 = p223BestQuartic,
        replayedHiggsMassGeV = p223BestHiggsMass,
        sourceLineagePromotable = p223SourceLineagePromotable,
        canPromoteHiggsCasimirQuarticLead = p223CanPromoteLead,
        sourceDerived = p223BestProbeSourceDerived,
    },
    scalarSourceEvidence = new
    {
        p187HiggsSourceIdentityValidated,
        p187PredictionAttemptAllowed,
        p189CensusPromotable,
        p189PredictionAttemptAllowed,
        p189ScalarFeatureEnvelopeCount,
        p189BranchStableNonC0Count,
        p189MassiveScalarProfileCount,
        p196CanPromoteHiggsFromPotentialOrSelfCoupling,
        p199CanPromoteAnyHiggsScalarSourceLineage,
        p207CanPromoteHiggsQuarticSelfCouplingSource,
        p207IntakeReadyFindingCount,
    },
    loopholeEvidence = new
    {
        p215CanPromoteTargetImpliedHiggsSelfCoupling,
        p229VevPromotable,
        targetImpliedQuartic,
        targetQuarticOverCasimirG2,
    },
    registryEvidence = new
    {
        registryCandidateCount = registryCandidates.Length,
        currentRegistryHasScalarIdentityFeatures,
        currentRegistryHasBranchStableScalarCandidate,
        currentRegistryHasMassiveScalarProfile,
        currentRegistryCandidatesAreOnlyNumericalModes,
        topCandidates = registryCandidates
            .Take(12)
            .Select(row => new
            {
                row.CandidateId,
                row.ClaimClass,
                row.BranchStabilityScore,
                row.PolarizationEnvelopePresent,
                row.SymmetryEnvelopePresent,
                row.InteractionProxyEnvelopePresent,
                row.MassLikeEnvelope,
                row.GaugeLeakEnvelope,
            })
            .ToArray(),
    },
    phase213Blockers = new
    {
        higgsMissingFieldCount = p213HiggsMissingFieldCount,
    },
    blockerRows,
    checks,
    decision = higgsScalarRepairabilityAuditPassed
        ? "Do not patch the P223 3/10 Higgs lead into a prediction from current scalar artifacts. The current repo lacks the scalar source/operator, identity envelope, massive scalar profile, and quartic/self-coupling source needed to make the lead promotable."
        : "Review Higgs scalar repairability before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A solved target-independent Higgs scalar source/operator.",
        "A scalar identity envelope and massive scalar profile that identify the physical Higgs row before target comparison.",
        "A source-derived quartic/self-coupling or excitation relation deriving the scalar scale, not the observed Higgs mass.",
        "Branch/refinement/environment/representation/coupling stability sidecars and a post-construction target-comparison gate.",
    },
    sourceEvidence = new
    {
        registryPath = RegistryPath,
        phase187Path = Phase187Path,
        phase189Path = Phase189Path,
        phase196Path = Phase196Path,
        phase199Path = Phase199Path,
        phase207Path = Phase207Path,
        phase215Path = Phase215Path,
        phase223Path = Phase223Path,
        phase229Path = Phase229Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase246Path = Phase246Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_scalar_repairability_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_scalar_repairability_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.higgsScalarRepairabilityAuditPassed,
        result.currentHiggsNumericalLeadPromotable,
        result.higgsScalarSourceRepairPossibleFromCurrentRegistry,
        result.threeTenthsFactorDerivableFromCurrentScalarSource,
        result.currentRegistryHasScalarIdentityFeatures,
        result.currentRegistryHasBranchStableScalarCandidate,
        result.currentRegistryHasMassiveScalarProfile,
        result.currentRegistryCandidatesAreOnlyNumericalModes,
        result.newHiggsScalarSourceStillRequired,
        result.p223,
        result.scalarSourceEvidence,
        result.loopholeEvidence,
        result.registryEvidence,
        result.phase213Blockers,
        result.blockerRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"higgsScalarRepairabilityAuditPassed={higgsScalarRepairabilityAuditPassed}");
Console.WriteLine($"currentHiggsNumericalLeadPromotable={currentHiggsNumericalLeadPromotable}");
Console.WriteLine($"higgsScalarSourceRepairPossibleFromCurrentRegistry={higgsScalarSourceRepairPossibleFromCurrentRegistry}");
Console.WriteLine($"threeTenthsFactorDerivableFromCurrentScalarSource={threeTenthsFactorDerivableFromCurrentScalarSource}");
Console.WriteLine($"newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}");

static string FormatMaybe(double? value) => value.HasValue ? value.Value.ToString("G17") : "missing";

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static IReadOnlyList<double> NumberArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Number).Select(item => item.GetDouble()).ToArray()
        : Array.Empty<double>();

sealed record ScalarRegistryCandidate(
    string CandidateId,
    string ClaimClass,
    double? BranchStabilityScore,
    bool PolarizationEnvelopePresent,
    bool SymmetryEnvelopePresent,
    bool InteractionProxyEnvelopePresent,
    IReadOnlyList<double> MassLikeEnvelope,
    IReadOnlyList<double> GaugeLeakEnvelope);

sealed record BlockerRow(string BlockerId, bool Active, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
