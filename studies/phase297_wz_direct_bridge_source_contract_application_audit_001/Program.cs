using System.Text.Json;

const string DefaultOutputDir = "studies/phase297_wz_direct_bridge_source_contract_application_audit_001/output";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase206Path = "studies/phase206_direct_bridge_normalization_closure_001/output/direct_bridge_normalization_closure_summary.json";
const string Phase209WzRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/wz_absolute_source_lineage_evidence_request.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase222Path = "studies/phase222_wz_raw_amplitude_source_obstruction_audit_001/output/wz_raw_amplitude_source_obstruction_audit_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase280Path = "studies/phase280_direct_bridge_analytic_variation_upgrade_audit_001/output/direct_bridge_analytic_variation_upgrade_audit_summary.json";
const string Phase282Path = "studies/phase282_branch_local_direct_invariant_census_001/output/branch_local_direct_invariant_census_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE297_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase206 = JsonDocument.Parse(File.ReadAllText(Phase206Path));
using var phase209WzRequest = JsonDocument.Parse(File.ReadAllText(Phase209WzRequestPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));
using var phase222 = JsonDocument.Parse(File.ReadAllText(Phase222Path));
using var phase247 = JsonDocument.Parse(File.ReadAllText(Phase247Path));
using var phase280 = JsonDocument.Parse(File.ReadAllText(Phase280Path));
using var phase282 = JsonDocument.Parse(File.ReadAllText(Phase282Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));

var p190SiblingStability = phase190.RootElement.GetProperty("siblingStability");
var p190BestCandidate = p190SiblingStability.GetProperty("bestCandidate");
var p191Gates = phase191.RootElement.GetProperty("gates");
var p206DirectUnitScaleReplay = phase206.RootElement.GetProperty("directUnitScaleReplay");
var p206DownstreamNormalizationReplay = phase206.RootElement.GetProperty("downstreamNormalizationReplay");
var p222BestProductionReplay = phase222.RootElement.GetProperty("bestProductionReplay");
var p282Census = phase282.RootElement.GetProperty("census");

var p190CandidateLawConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed") is true;
var p190TargetIndependent = JsonBool(phase190.RootElement, "targetObservablesUsed") is false;
var p190TheoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var p190StableCandidateCount = JsonInt(p190SiblingStability, "stableCandidateCount") ?? -1;
var p190BestRelativeSpread = JsonDouble(p190BestCandidate, "relativeSpread");
var p191RawGatePassed = JsonBool(p191Gates, "rawGatePassed") is true;
var p191BestRawToTargetRatio = JsonDouble(p191Gates, "bestRawToTargetRatio");
var p191WzParticleSplitPresent = JsonBool(p191Gates, "wZParticleSplitPresent") is true;
var p191TargetComparisonRunnable = JsonBool(p191Gates, "targetComparisonRunnable") is true;
var p206CanPromote = JsonBool(phase206.RootElement, "canPromoteDirectBridgeNormalization") is true;
var p206DirectUnitScaleRawGatePassed = JsonBool(p206DirectUnitScaleReplay, "directUnitScaleRawGatePassed") is true;
var p206CommonScaleGatePassed = JsonBool(p206DownstreamNormalizationReplay, "p166CommonScaleGatePassed") is true;
var p206TargetComparisonPassed = JsonBool(p206DownstreamNormalizationReplay, "p166TargetComparisonPassed") is true;
var p221NumericalTargetComparisonPassed = JsonBool(phase221.RootElement, "numericalTargetComparisonPassed") is true;
var p221SourceLineagePromotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;
var p222RawObstructionCertified = JsonBool(phase222.RootElement, "rawAmplitudeSourceObstructionCertified") is true;
var p222BestProductionRawToTargetRatio = JsonDouble(p222BestProductionReplay, "bestRawToTargetRatio");
var p222BestProductionCommonBridgePassed = JsonBool(p222BestProductionReplay, "commonBridgePassed") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p247CurrentDirectBridgeCandidatePromotable = JsonBool(phase247.RootElement, "currentDirectBridgeCandidatePromotable") is true;
var p280CanRepair = JsonBool(phase280.RootElement, "canRepairDirectBridgeWithAnalyticVariation") is true;
var p280AnalyticRawGatePassed = JsonBool(phase280.RootElement, "analyticRawGatePassed") is true;
var p280BranchLocalAnalyticStabilityPassed = JsonBool(phase280.RootElement, "branchLocalAnalyticStabilityPassed") is true;
var p282NewLocalDirectInvariantSourceFound = JsonBool(phase282.RootElement, "newLocalDirectInvariantSourceFound") is true;
var p282StableRawSingleCandidateCount = JsonInt(p282Census, "stablePosthocRawGatePassingSingleCandidateCount") ?? -1;
var p282StableRawSubspaceCount = JsonInt(p282Census, "stablePosthocRawGatePassingSubspaceCount") ?? -1;
var p296Passed = JsonBool(phase296.RootElement, "sourceLineageContractFieldCandidateScanPassed") is true;
var p296IntakeReadyFieldCandidateCount = JsonInt(phase296.RootElement, "intakeReadySourceLineageFieldCandidateCount") ?? -1;
var p296AnyCandidateFillsContract = JsonBool(phase296.RootElement, "anySourceLineageCandidateFillsContract") is true;
var p201WzPromotable = phase201.RootElement.TryGetProperty("wzValidation", out var p201WzValidation)
    && JsonBool(p201WzValidation, "promotable") is true;
var p213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase209WzCurrentMissingFieldCount = JsonInt(phase209WzRequest.RootElement, "currentMissingFieldCount") ?? -1;

var contractFieldAssessments = new[]
{
    new ContractFieldAssessment(
        "externalTargetValuesUsed=false",
        CandidateSignalPresent: p190CandidateLawConstructed && p190TargetIndependent,
        AcceptedForContract: false,
        Evidence: "P190 construction excludes W/Z target observables.",
        Blocker: "The signal is not applied as an intake artifact because the same candidate lacks theorem promotion, sourceLineageId, particle rows, and gates."),
    new ContractFieldAssessment(
        "theoremOrDerivationId",
        CandidateSignalPresent: p190TheoremClaimed,
        AcceptedForContract: false,
        Evidence: $"P190 theoremClaimed={p190TheoremClaimed}; P247 newDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}.",
        Blocker: "No derivation-backed direct W/Z bridge-source theorem is present."),
    new ContractFieldAssessment(
        "sourceLineageId",
        CandidateSignalPresent: false,
        AcceptedForContract: false,
        Evidence: "No current W/Z source lineage id is linked to a promoted theorem.",
        Blocker: "A sourceLineageId cannot be minted from a non-promotable diagnostic candidate."),
    WzRowAssessment("w-boson", "sourceRowId", p191WzParticleSplitPresent, "P191 reports no particle-specific W/Z source split.", "No separate W source row is derived."),
    WzRowAssessment("w-boson", "rawAmplitudeGatePassed=true", p191RawGatePassed, $"P191 best raw/target ratio={p191BestRawToTargetRatio:R}; P222 best production ratio={p222BestProductionRawToTargetRatio:R}.", "Raw amplitude gate fails before target comparison."),
    WzRowAssessment("w-boson", "commonBridgeGatePassed=true", p206CommonScaleGatePassed && p222BestProductionCommonBridgePassed, $"P206 common-scale gate={p206CommonScaleGatePassed}; P222 production common bridge={p222BestProductionCommonBridgePassed}.", "Current normalizations do not pass common W/Z bridge consistency."),
    WzRowAssessment("w-boson", "targetComparisonGatePassed=true", p191TargetComparisonRunnable && p206TargetComparisonPassed, $"P191 target comparison runnable={p191TargetComparisonRunnable}; P206 target comparison={p206TargetComparisonPassed}; P221 numerical comparison={p221NumericalTargetComparisonPassed} but promotable={p221SourceLineagePromotable}.", "Only a non-promotable numerical lead passes target comparison; the source-built W row is not runnable."),
    WzRowAssessment("w-boson", "stabilitySidecarsPresent=true", p190StableCandidateCount > 0 && p280BranchLocalAnalyticStabilityPassed, $"P190 stableCandidateCount={p190StableCandidateCount}; best relative spread={p190BestRelativeSpread:R}; P280 branch-local analytic stability={p280BranchLocalAnalyticStabilityPassed}.", "Sibling and analytic stability evidence is insufficient for a W source row."),
    WzRowAssessment("w-boson", "derivationId", false, "No W row derivation id exists.", "A derivation id requires a derived W source row."),
    WzRowAssessment("z-boson", "sourceRowId", p191WzParticleSplitPresent, "P191 reports no particle-specific W/Z source split.", "No separate Z source row is derived."),
    WzRowAssessment("z-boson", "rawAmplitudeGatePassed=true", p191RawGatePassed, $"P191 best raw/target ratio={p191BestRawToTargetRatio:R}; P222 best production ratio={p222BestProductionRawToTargetRatio:R}.", "Raw amplitude gate fails before target comparison."),
    WzRowAssessment("z-boson", "commonBridgeGatePassed=true", p206CommonScaleGatePassed && p222BestProductionCommonBridgePassed, $"P206 common-scale gate={p206CommonScaleGatePassed}; P222 production common bridge={p222BestProductionCommonBridgePassed}.", "Current normalizations do not pass common W/Z bridge consistency."),
    WzRowAssessment("z-boson", "targetComparisonGatePassed=true", p191TargetComparisonRunnable && p206TargetComparisonPassed, $"P191 target comparison runnable={p191TargetComparisonRunnable}; P206 target comparison={p206TargetComparisonPassed}; P221 numerical comparison={p221NumericalTargetComparisonPassed} but promotable={p221SourceLineagePromotable}.", "Only a non-promotable numerical lead passes target comparison; the source-built Z row is not runnable."),
    WzRowAssessment("z-boson", "stabilitySidecarsPresent=true", p190StableCandidateCount > 0 && p280BranchLocalAnalyticStabilityPassed, $"P190 stableCandidateCount={p190StableCandidateCount}; best relative spread={p190BestRelativeSpread:R}; P280 branch-local analytic stability={p280BranchLocalAnalyticStabilityPassed}.", "Sibling and analytic stability evidence is insufficient for a Z source row."),
    WzRowAssessment("z-boson", "derivationId", false, "No Z row derivation id exists.", "A derivation id requires a derived Z source row."),
};

var acceptedContractFieldCount = contractFieldAssessments.Count(field => field.AcceptedForContract);
var candidateSupportedButNotAppliedFieldCount = contractFieldAssessments.Count(field => field.CandidateSignalPresent && !field.AcceptedForContract);
var blockedContractFieldCount = contractFieldAssessments.Count(field => !field.AcceptedForContract);
var canFillWzSourceContractNow = acceptedContractFieldCount == contractFieldAssessments.Length;
var sourceContractApplicationAllowed = canFillWzSourceContractNow && p201WzPromotable;
var fieldsAppliedToPhase201TemplateCount = 0;
var phase201TemplateMutated = false;

var checks = new[]
{
    new Check(
        "direct-bridge-candidate-is-target-independent",
        p190CandidateLawConstructed && p190TargetIndependent,
        $"candidateLawConstructed={p190CandidateLawConstructed}; targetObservablesUsed={JsonBool(phase190.RootElement, "targetObservablesUsed")}"),
    new Check(
        "direct-bridge-candidate-not-theorem-promoted",
        !p190TheoremClaimed && p247NewDirectBridgeTheoremStillRequired,
        $"theoremClaimed={p190TheoremClaimed}; p247NewDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}"),
    new Check(
        "raw-amplitude-gate-remains-blocked",
        !p191RawGatePassed && p222RawObstructionCertified,
        $"p191RawGatePassed={p191RawGatePassed}; p191BestRawToTargetRatio={p191BestRawToTargetRatio:R}; p222RawObstructionCertified={p222RawObstructionCertified}; p222BestProductionRawToTargetRatio={p222BestProductionRawToTargetRatio:R}"),
    new Check(
        "particle-specific-wz-split-absent",
        !p191WzParticleSplitPresent && !p247CurrentDirectBridgeCandidatePromotable,
        $"p191WzParticleSplitPresent={p191WzParticleSplitPresent}; p247CurrentDirectBridgeCandidatePromotable={p247CurrentDirectBridgeCandidatePromotable}"),
    new Check(
        "normalization-and-target-comparison-not-source-promotable",
        p206DirectUnitScaleRawGatePassed && !p206CanPromote && !p206CommonScaleGatePassed && !p206TargetComparisonPassed && p221NumericalTargetComparisonPassed && !p221SourceLineagePromotable,
        $"p206DirectUnitScaleRawGatePassed={p206DirectUnitScaleRawGatePassed}; p206CanPromote={p206CanPromote}; p206CommonScaleGatePassed={p206CommonScaleGatePassed}; p206TargetComparisonPassed={p206TargetComparisonPassed}; p221NumericalTargetComparisonPassed={p221NumericalTargetComparisonPassed}; p221SourceLineagePromotable={p221SourceLineagePromotable}"),
    new Check(
        "analytic-and-invariant-upgrades-do-not-repair",
        !p280CanRepair && !p280AnalyticRawGatePassed && !p282NewLocalDirectInvariantSourceFound && p282StableRawSingleCandidateCount == 0 && p282StableRawSubspaceCount == 0,
        $"p280CanRepair={p280CanRepair}; p280AnalyticRawGatePassed={p280AnalyticRawGatePassed}; p282NewLocalDirectInvariantSourceFound={p282NewLocalDirectInvariantSourceFound}; p282StableRawSingleCandidateCount={p282StableRawSingleCandidateCount}; p282StableRawSubspaceCount={p282StableRawSubspaceCount}"),
    new Check(
        "source-contract-field-scan-remains-empty",
        p296Passed && p296IntakeReadyFieldCandidateCount == 0 && !p296AnyCandidateFillsContract,
        $"p296Passed={p296Passed}; p296IntakeReadyFieldCandidateCount={p296IntakeReadyFieldCandidateCount}; p296AnyCandidateFillsContract={p296AnyCandidateFillsContract}"),
    new Check(
        "phase201-wz-contract-not-mutated-or-filled",
        !phase201TemplateMutated && fieldsAppliedToPhase201TemplateCount == 0 && !p201WzPromotable && p213WzMissingFieldCount == 15 && phase209WzCurrentMissingFieldCount == 15,
        $"phase201TemplateMutated={phase201TemplateMutated}; fieldsAppliedToPhase201TemplateCount={fieldsAppliedToPhase201TemplateCount}; p201WzPromotable={p201WzPromotable}; p213WzMissingFieldCount={p213WzMissingFieldCount}; phase209WzCurrentMissingFieldCount={phase209WzCurrentMissingFieldCount}"),
};

var wzDirectBridgeSourceContractApplicationAuditPassed = checks.All(check => check.Passed)
    && !sourceContractApplicationAllowed
    && !canFillWzSourceContractNow
    && acceptedContractFieldCount == 0
    && blockedContractFieldCount == 15;
var terminalStatus = wzDirectBridgeSourceContractApplicationAuditPassed
    ? "wz-direct-bridge-source-contract-application-audit-blocked-new-theorem-required"
    : "wz-direct-bridge-source-contract-application-audit-review-required";

var result = new
{
    phaseId = "phase297-wz-direct-bridge-source-contract-application-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    wzDirectBridgeSourceContractApplicationAuditPassed,
    applicationAttempted = true,
    sourceContractApplicationAllowed,
    canFillWzSourceContractNow,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    contractFieldCount = contractFieldAssessments.Length,
    acceptedContractFieldCount,
    candidateSupportedButNotAppliedFieldCount,
    blockedContractFieldCount,
    candidateLaw = new
    {
        candidateLawConstructed = p190CandidateLawConstructed,
        targetIndependent = p190TargetIndependent,
        theoremClaimed = p190TheoremClaimed,
        stableCandidateCount = p190StableCandidateCount,
        bestRelativeSpread = p190BestRelativeSpread,
        rawGatePassed = p191RawGatePassed,
        bestRawToTargetRatio = p191BestRawToTargetRatio,
        wZParticleSplitPresent = p191WzParticleSplitPresent,
        targetComparisonRunnable = p191TargetComparisonRunnable,
    },
    repairEvidence = new
    {
        p206DirectUnitScaleRawGatePassed,
        p206CanPromoteDirectBridgeNormalization = p206CanPromote,
        p206CommonScaleGatePassed,
        p206TargetComparisonPassed,
        p221NumericalTargetComparisonPassed,
        p221SourceLineagePromotable,
        p222RawObstructionCertified,
        p222BestProductionRawToTargetRatio,
        p222BestProductionCommonBridgePassed,
        p247NewDirectBridgeTheoremStillRequired,
        p280CanRepairWithAnalyticVariation = p280CanRepair,
        p280AnalyticRawGatePassed,
        p280BranchLocalAnalyticStabilityPassed,
        p282NewLocalDirectInvariantSourceFound,
        p282StableRawSingleCandidateCount,
        p282StableRawSubspaceCount,
    },
    contractFieldAssessments,
    checks,
    inheritedEvidence = new
    {
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase201Path = Phase201Path,
        phase206Path = Phase206Path,
        phase209WzRequestPath = Phase209WzRequestPath,
        phase213Path = Phase213Path,
        phase221Path = Phase221Path,
        phase222Path = Phase222Path,
        phase247Path = Phase247Path,
        phase280Path = Phase280Path,
        phase282Path = Phase282Path,
        phase296Path = Phase296Path,
        phase201 = new
        {
            wzPromotable = p201WzPromotable,
        },
        phase213 = new
        {
            wzMissingFieldCount = p213WzMissingFieldCount,
        },
        phase296 = new
        {
            p296Passed,
            p296IntakeReadyFieldCandidateCount,
            p296AnyCandidateFillsContract,
        },
    },
    decision = "Do not fill or mutate the Phase201 W/Z source-lineage intake template from the current direct-bridge candidate. The candidate is target-independent, but it lacks theorem/source-lineage promotion, separate W/Z rows, raw-amplitude closure, common-bridge closure, stability sidecars, and row derivations.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed direct W/Z bridge-source theorem that discharges the mixed-linearization obligation.",
        "Separate W and Z source rows with derivation ids, not a single W/Z-like fermion pair.",
        "A source-derived normalization or raw-amplitude law that clears raw and common W/Z bridge gates before target comparison.",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "wz_direct_bridge_source_contract_application_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_direct_bridge_source_contract_application_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.wzDirectBridgeSourceContractApplicationAuditPassed,
        result.applicationAttempted,
        result.sourceContractApplicationAllowed,
        result.canFillWzSourceContractNow,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.contractFieldCount,
        result.acceptedContractFieldCount,
        result.candidateSupportedButNotAppliedFieldCount,
        result.blockedContractFieldCount,
        result.candidateLaw,
        result.repairEvidence,
        result.checks,
        result.inheritedEvidence,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"wzDirectBridgeSourceContractApplicationAuditPassed={wzDirectBridgeSourceContractApplicationAuditPassed}");
Console.WriteLine($"canFillWzSourceContractNow={canFillWzSourceContractNow}");
Console.WriteLine($"acceptedContractFieldCount={acceptedContractFieldCount}");
Console.WriteLine($"blockedContractFieldCount={blockedContractFieldCount}");

static ContractFieldAssessment WzRowAssessment(
    string particleId,
    string fieldId,
    bool candidateSignalPresent,
    string evidence,
    string blocker) =>
    new($"{particleId}.{fieldId}", candidateSignalPresent, AcceptedForContract: false, evidence, blocker);

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : null;

record ContractFieldAssessment(
    string FieldId,
    bool CandidateSignalPresent,
    bool AcceptedForContract,
    string Evidence,
    string Blocker);

record Check(string CheckId, bool Passed, string Detail);
