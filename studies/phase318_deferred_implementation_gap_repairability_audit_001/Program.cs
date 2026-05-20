using System.Text.Json;

const string DefaultOutputDir = "studies/phase318_deferred_implementation_gap_repairability_audit_001/output";
const string Phase3OpenIssuesPath = "docs/Phases/OpenIssues/PHASE_3_OPEN_ISSUES.md";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string InteractionProxyRecordPath = "src/Gu.Phase3.Properties/InteractionProxyRecord.cs";
const string SimpleInteractionProxyComputerPath = "src/Gu.Phase3.Properties/SimpleInteractionProxyComputer.cs";
const string InteractionProxyComputerPath = "src/Gu.Phase3.Properties/InteractionProxyComputer.cs";
const string CandidateBosonRecordPath = "src/Gu.Phase3.Registry/CandidateBosonRecord.cs";

var outputDir = Environment.GetEnvironmentVariable("PHASE318_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var phase3OpenIssuesText = File.ReadAllText(Phase3OpenIssuesPath);
var interactionProxyRecordText = File.ReadAllText(InteractionProxyRecordPath);
var simpleInteractionProxyComputerText = File.ReadAllText(SimpleInteractionProxyComputerPath);
var interactionProxyComputerText = File.ReadAllText(InteractionProxyComputerPath);
var candidateBosonRecordText = File.ReadAllText(CandidateBosonRecordPath);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));

var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var canPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var currentHiggsNumericalLeadPromotable = JsonBool(phase248.RootElement, "currentHiggsNumericalLeadPromotable") is true;
var currentRegistryHasScalarIdentityFeatures = JsonBool(phase248.RootElement, "currentRegistryHasScalarIdentityFeatures") is true;
var currentRegistryHasMassiveScalarProfile = JsonBool(phase248.RootElement, "currentRegistryHasMassiveScalarProfile") is true;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var currentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;

var issueRows = new[]
{
    IssueRow(
        "issue-1-fermionic-spectrum",
        "fermionic-sector-infrastructure",
        "Out of Scope",
        "Fermionic extraction may be needed for a full physical particle dictionary, but it does not by itself provide W/Z source rows, VEV/coupling sources, or a Higgs scalar-source lineage."),
    IssueRow(
        "issue-2-full-physical-particle-dictionary",
        "physical-identification",
        "Out of Scope",
        "The open issue explicitly says a unique Standard Model boson dictionary requires more than current Phase III candidate families."),
    IssueRow(
        "issue-3-quotient-aware-formulation",
        "gauge-invariant-spectral-formulation",
        "Deferred",
        "A cleaner quotient spectral operator could improve mode hygiene, but it does not define observed electroweak embedding, W/Z rows, or scalar potential lineage."),
    IssueRow(
        "issue-4-dispersion-relation-mass-extraction",
        "mass-like-scale-estimation",
        "Deferred",
        "A true dispersion fit would improve numerical mass-like scales, but it still needs an observed-particle map and source-derived normalization."),
    IssueRow(
        "issue-5-gpu-cuda-kernel-linkage",
        "backend-stability",
        "Deferred",
        "CUDA parity can raise backend confidence, but backend confidence is not the missing W/Z/H source theorem."),
    IssueRow(
        "issue-6-cosmological-environments",
        "environment-campaigns",
        "Out of Scope",
        "Larger physical environments can test stability, but they do not fill target-independent source-lineage fields."),
    IssueRow(
        "issue-7-interaction-proxy-beyond-cubic",
        "quartic-and-higher-interactions",
        "Deferred",
        "Quartic proxies are relevant Higgs-like diagnostics, but a proxy is not a solved scalar source operator, identity envelope, massive profile, or self-coupling source."),
    IssueRow(
        "issue-8-global-well-posedness",
        "mathematical-foundations",
        "Out of Scope",
        "Existence and uniqueness proofs are foundational, not direct W/Z/H prediction rows."),
    IssueRow(
        "issue-9-symplectic-canonical-quantization",
        "quantum-structure",
        "Out of Scope",
        "Quantization is prerequisite to a scattering program, not a source-lineage fill for tree-level W/Z/H mass rows."),
    IssueRow(
        "issue-10-background-independent-convergence",
        "continuum-convergence",
        "Deferred",
        "Richardson/Cauchy convergence improves numerical credibility but cannot identify observed W/Z/H fields or source parameters."),
    IssueRow(
        "issue-11-scattering-s-matrix",
        "interaction-amplitudes",
        "Out of Scope",
        "Scattering amplitudes require quantization and do not replace the missing mass-source contracts."),
    IssueRow(
        "issue-12-symbolic-derivation-engine",
        "symbolic-support",
        "Out of Scope",
        "A CAS could help derive or verify future artifacts, but no current symbolic artifact fills the contracts."),
};

var issueCoverageRows = issueRows
    .Select(row => row with { PresentInOpenIssues = phase3OpenIssuesText.Contains(row.IssueId.Replace("issue-", "ISSUE-").Split('-')[0] + "-" + row.IssueId.Split('-')[1], StringComparison.OrdinalIgnoreCase) })
    .ToArray();

var fullParticleDictionaryRequiresFermionicSector = phase3OpenIssuesText.Contains("resolution of the fermionic sector", StringComparison.OrdinalIgnoreCase);
var phase3ForbidsUniquePhysicalTruth = phase3OpenIssuesText.Contains("forbids collapsing candidate identifications into unique physical truth", StringComparison.OrdinalIgnoreCase);
var quotientAwareDeferred = phase3OpenIssuesText.Contains("QuotientAware", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("NotSupportedException", StringComparison.Ordinal);
var dispersionMassDeferred = phase3OpenIssuesText.Contains("True Dispersion-Relation Mass Extraction", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("requires multiple momentum values", StringComparison.OrdinalIgnoreCase);
var cudaBackendDeferred = phase3OpenIssuesText.Contains("IsCudaAvailable()` always returns `false`", StringComparison.Ordinal);
var quarticInteractionProxyDeferred = phase3OpenIssuesText.Contains("Interaction Proxy Beyond Cubic", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("Quartic and higher-order interaction terms", StringComparison.Ordinal);
var convergenceDeferred = phase3OpenIssuesText.Contains("Background-Independent Convergence Criteria", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("Richardson extrapolation", StringComparison.Ordinal);
var quantizationOutOfScope = phase3OpenIssuesText.Contains("Symplectic / Canonical Quant", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("None of these are implemented", StringComparison.Ordinal);
var scatteringOutOfScope = phase3OpenIssuesText.Contains("Scattering / S-Matrix Program", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("no machinery for computing actual scattering amplitudes", StringComparison.Ordinal);
var symbolicDerivationOutOfScope = phase3OpenIssuesText.Contains("Fully Automatic Symbolic Derivation", StringComparison.Ordinal)
    && phase3OpenIssuesText.Contains("not implemented", StringComparison.OrdinalIgnoreCase);

var interactionProxyRecordDefinesCubicResponse = interactionProxyRecordText.Contains("CubicResponse", StringComparison.Ordinal);
var interactionProxyRecordDefinesQuarticResponse = interactionProxyRecordText.Contains("QuarticResponse", StringComparison.Ordinal);
var simpleInteractionProxyComputerReturnsCubicOnly = simpleInteractionProxyComputerText.Contains("CubicResponse = cubicResponse", StringComparison.Ordinal)
    && !simpleInteractionProxyComputerText.Contains("QuarticResponse", StringComparison.Ordinal);
var interactionProxyComputerReturnsCubicOnly = interactionProxyComputerText.Contains("CubicResponse = cubicResponse", StringComparison.Ordinal)
    && !interactionProxyComputerText.Contains("QuarticResponse", StringComparison.Ordinal);
var registryInteractionEnvelopeIsCubicOnly = candidateBosonRecordText.Contains("MinCubicResponse", StringComparison.Ordinal)
    && candidateBosonRecordText.Contains("MaxCubicResponse", StringComparison.Ordinal)
    && !candidateBosonRecordText.Contains("QuarticResponse", StringComparison.Ordinal);

const bool codeOnlyFixCanDeriveGuLocalWzTheorem = false;
const bool codeOnlyFixCanDeriveObservedElectroweakEmbedding = false;
const bool codeOnlyFixCanDeriveVevOrCouplingSource = false;
const bool codeOnlyFixCanDeriveHiggsScalarSource = false;
const bool deferredIssueImplementationCanFillPhase201WzContract = false;
const bool deferredIssueImplementationCanFillPhase201HiggsContract = false;
const bool deferredIssueImplementationCanFillPhase256ObservedFieldExtractionContract = false;
const bool quarticProxyImplementationPromotesHiggsMass = false;
const bool deferredImplementationFixCompletesBosonPredictions = false;
const bool launchableCodeOnlyPredictionFixFound = false;

var codeOnlyRepairRows = new[]
{
    new CodeOnlyRepairRow("quotient-aware-p3", "Implement P3 quotient-aware formulation.", "Would improve gauge-reduced spectral hygiene.", false, false, false, "Does not supply observed electroweak embedding, W/Z source rows, VEV/coupling source, or Higgs scalar source."),
    new CodeOnlyRepairRow("dispersion-relation-mass", "Implement true dispersion-relation mass extraction.", "Would improve mass-like scale estimation from multi-momentum data.", false, false, false, "Still requires observed-particle identification and source-derived normalization before physical W/Z/H comparison."),
    new CodeOnlyRepairRow("cuda-backend", "Build and link CUDA spectral kernels.", "Would improve backend stability and claim-class confidence.", false, false, false, "Backend parity is not a source-lineage theorem or target-independent mass source."),
    new CodeOnlyRepairRow("cosmological-environments", "Run larger, more physical environment campaigns.", "Would improve robustness testing.", false, false, false, "More data does not fill the missing source theorem, electroweak projection, or scalar-source lineage."),
    new CodeOnlyRepairRow("quartic-interaction-proxy", "Implement quartic and higher interaction proxy records.", "Would add a Higgs-like self-interaction diagnostic.", false, false, false, "A quartic proxy is not a solved scalar source/operator, Higgs identity envelope, massive scalar profile, or self-coupling value."),
    new CodeOnlyRepairRow("background-independent-convergence", "Implement Richardson/Cauchy convergence criteria.", "Would improve continuum-limit credibility.", false, false, false, "Convergence evidence cannot by itself identify observed W/Z/H rows or source constants."),
    new CodeOnlyRepairRow("symplectic-quantization-and-scattering", "Implement quantization and scattering/S-matrix infrastructure.", "Would open future amplitude tests.", false, false, false, "It is a later theoretical layer and does not replace the current tree-level mass source-lineage contracts."),
    new CodeOnlyRepairRow("symbolic-derivation-engine", "Integrate symbolic derivation support.", "Could help construct or verify future derivations.", false, false, false, "No current symbolic artifact exists; a tool is not itself the missing theorem/source artifact."),
};

var checks = new[]
{
    new Check(
        "phase3-open-issues-covered",
        issueCoverageRows.Length == 12
            && issueCoverageRows.All(row => row.PresentInOpenIssues)
            && fullParticleDictionaryRequiresFermionicSector
            && phase3ForbidsUniquePhysicalTruth
            && quotientAwareDeferred
            && dispersionMassDeferred
            && cudaBackendDeferred
            && quarticInteractionProxyDeferred
            && convergenceDeferred
            && quantizationOutOfScope
            && scatteringOutOfScope
            && symbolicDerivationOutOfScope,
        $"issueCount={issueCoverageRows.Length}; presentIssueCount={issueCoverageRows.Count(row => row.PresentInOpenIssues)}; fermionicSectorRequired={fullParticleDictionaryRequiresFermionicSector}; physicalTruthCaution={phase3ForbidsUniquePhysicalTruth}; quotientAwareDeferred={quotientAwareDeferred}; dispersionDeferred={dispersionMassDeferred}; cudaDeferred={cudaBackendDeferred}; quarticDeferred={quarticInteractionProxyDeferred}; convergenceDeferred={convergenceDeferred}; quantizationOutOfScope={quantizationOutOfScope}; scatteringOutOfScope={scatteringOutOfScope}; symbolicOutOfScope={symbolicDerivationOutOfScope}"),
    new Check(
        "current-interaction-proxy-is-cubic-only",
        interactionProxyRecordDefinesCubicResponse
            && !interactionProxyRecordDefinesQuarticResponse
            && simpleInteractionProxyComputerReturnsCubicOnly
            && interactionProxyComputerReturnsCubicOnly
            && registryInteractionEnvelopeIsCubicOnly,
        $"recordCubic={interactionProxyRecordDefinesCubicResponse}; recordQuartic={interactionProxyRecordDefinesQuarticResponse}; simpleCubicOnly={simpleInteractionProxyComputerReturnsCubicOnly}; operatorCubicOnly={interactionProxyComputerReturnsCubicOnly}; registryCubicOnly={registryInteractionEnvelopeIsCubicOnly}"),
    new Check(
        "quartic-proxy-is-diagnostic-not-higgs-source",
        quarticInteractionProxyDeferred
            && !canPromoteHiggsFromPotentialOrSelfCoupling
            && newHiggsScalarSourceStillRequired
            && !currentHiggsNumericalLeadPromotable
            && !currentRegistryHasScalarIdentityFeatures
            && !currentRegistryHasMassiveScalarProfile
            && !quarticProxyImplementationPromotesHiggsMass,
        $"quarticDeferred={quarticInteractionProxyDeferred}; p196CanPromote={canPromoteHiggsFromPotentialOrSelfCoupling}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}; currentHiggsNumericalLeadPromotable={currentHiggsNumericalLeadPromotable}; scalarIdentityFeatures={currentRegistryHasScalarIdentityFeatures}; massiveScalarProfile={currentRegistryHasMassiveScalarProfile}; quarticProxyPromotes={quarticProxyImplementationPromotesHiggsMass}"),
    new Check(
        "source-lineage-contracts-remain-unfilled",
        !allRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; existingEvidenceFound={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "observed-field-extraction-remains-unfilled-by-current-implementation",
        observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !currentImplementationCanFillObservedFieldExtractionContract
            && !deferredIssueImplementationCanFillPhase256ObservedFieldExtractionContract,
        $"phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}; phase257CurrentImplCanFill={currentImplementationCanFillObservedFieldExtractionContract}; deferredCanFillPhase256={deferredIssueImplementationCanFillPhase256ObservedFieldExtractionContract}"),
    new Check(
        "deferred-code-fixes-do-not-complete-boson-predictions",
        !codeOnlyFixCanDeriveGuLocalWzTheorem
            && !codeOnlyFixCanDeriveObservedElectroweakEmbedding
            && !codeOnlyFixCanDeriveVevOrCouplingSource
            && !codeOnlyFixCanDeriveHiggsScalarSource
            && !deferredIssueImplementationCanFillPhase201WzContract
            && !deferredIssueImplementationCanFillPhase201HiggsContract
            && !deferredImplementationFixCompletesBosonPredictions
            && !launchableCodeOnlyPredictionFixFound
            && codeOnlyRepairRows.All(row => !row.FillsPhase201Wz && !row.FillsPhase201Higgs && !row.CompletesBosonPredictions),
        $"guLocalWz={codeOnlyFixCanDeriveGuLocalWzTheorem}; observedEmbedding={codeOnlyFixCanDeriveObservedElectroweakEmbedding}; vevOrCoupling={codeOnlyFixCanDeriveVevOrCouplingSource}; higgsScalar={codeOnlyFixCanDeriveHiggsScalarSource}; canFillWz={deferredIssueImplementationCanFillPhase201WzContract}; canFillHiggs={deferredIssueImplementationCanFillPhase201HiggsContract}; completes={deferredImplementationFixCompletesBosonPredictions}; launchableFix={launchableCodeOnlyPredictionFixFound}"),
};

var deferredImplementationGapRepairabilityAuditPassed = checks.All(check => check.Passed)
    && !launchableCodeOnlyPredictionFixFound
    && !deferredImplementationFixCompletesBosonPredictions;
var terminalStatus = deferredImplementationGapRepairabilityAuditPassed
    ? "deferred-implementation-gap-repairability-audit-code-only-fix-not-sufficient"
    : "deferred-implementation-gap-repairability-audit-review-required";

var result = new
{
    phaseId = "phase318-deferred-implementation-gap-repairability-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    deferredImplementationGapRepairabilityAuditPassed,
    launchableCodeOnlyPredictionFixFound,
    deferredImplementationFixCompletesBosonPredictions,
    fullParticleDictionaryRequiresFermionicSector,
    phase3ForbidsUniquePhysicalTruth,
    quotientAwareDeferred,
    dispersionMassDeferred,
    cudaBackendDeferred,
    quarticInteractionProxyDeferred,
    convergenceDeferred,
    quantizationOutOfScope,
    scatteringOutOfScope,
    symbolicDerivationOutOfScope,
    interactionProxyImplementation = new
    {
        interactionProxyRecordDefinesCubicResponse,
        interactionProxyRecordDefinesQuarticResponse,
        simpleInteractionProxyComputerReturnsCubicOnly,
        interactionProxyComputerReturnsCubicOnly,
        registryInteractionEnvelopeIsCubicOnly,
    },
    contractImpact = new
    {
        codeOnlyFixCanDeriveGuLocalWzTheorem,
        codeOnlyFixCanDeriveObservedElectroweakEmbedding,
        codeOnlyFixCanDeriveVevOrCouplingSource,
        codeOnlyFixCanDeriveHiggsScalarSource,
        deferredIssueImplementationCanFillPhase201WzContract,
        deferredIssueImplementationCanFillPhase201HiggsContract,
        deferredIssueImplementationCanFillPhase256ObservedFieldExtractionContract,
        quarticProxyImplementationPromotesHiggsMass,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
    },
    upstreamEvidence = new
    {
        phase201 = new
        {
            allRequiredLineagesPromotable,
        },
        phase213 = new
        {
            existingEvidenceFound,
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase196 = new
        {
            canPromoteHiggsFromPotentialOrSelfCoupling,
        },
        phase248 = new
        {
            newHiggsScalarSourceStillRequired,
            currentHiggsNumericalLeadPromotable,
            currentRegistryHasScalarIdentityFeatures,
            currentRegistryHasMassiveScalarProfile,
        },
        phase256 = new
        {
            observedFieldExtractionFilledRequiredFieldCount,
            observedFieldExtractionContractPromotable,
        },
        phase257 = new
        {
            currentImplementationCanFillObservedFieldExtractionContract,
        },
    },
    issueRows = issueCoverageRows,
    codeOnlyRepairRows,
    checks,
    decision = "Do not treat deferred Phase III implementation gaps as the launchable fix for W/Z/H physical mass predictions. Implementing P3 quotient spectra, dispersion fits, CUDA parity, quartic interaction proxies, convergence extrapolation, quantization/scattering, or symbolic tooling would improve infrastructure, but none supplies the missing GU-local W/Z source theorem, observed electroweak projection, VEV/coupling sources, or Higgs scalar-source/self-coupling lineage required by Phase201/256.",
    nextRequiredArtifact = new[]
    {
        "A theorem/source artifact deriving target-independent W/Z source rows, observed electroweak embedding, photon/Z projection, and common source normalization.",
        "A solved Higgs scalar-sector artifact deriving the scalar source/operator, identity envelope, massive profile, and potential/self-coupling or excitation relation.",
        "Only after those artifacts exist should implementation work apply them through Phase201/209/210/213 and rerun the full boson generator.",
    },
    sourceEvidence = new
    {
        phase3OpenIssuesPath = Phase3OpenIssuesPath,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase196Path = Phase196Path,
        phase248Path = Phase248Path,
        phase256Path = Phase256Path,
        phase257Path = Phase257Path,
        interactionProxyRecordPath = InteractionProxyRecordPath,
        simpleInteractionProxyComputerPath = SimpleInteractionProxyComputerPath,
        interactionProxyComputerPath = InteractionProxyComputerPath,
        candidateBosonRecordPath = CandidateBosonRecordPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "deferred_implementation_gap_repairability_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "deferred_implementation_gap_repairability_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"deferredImplementationGapRepairabilityAuditPassed={deferredImplementationGapRepairabilityAuditPassed}");
Console.WriteLine($"launchableCodeOnlyPredictionFixFound={launchableCodeOnlyPredictionFixFound}");
Console.WriteLine($"quarticInteractionProxyDeferred={quarticInteractionProxyDeferred}");
Console.WriteLine($"quarticProxyImplementationPromotesHiggsMass={quarticProxyImplementationPromotesHiggsMass}");
Console.WriteLine($"deferredImplementationFixCompletesBosonPredictions={deferredImplementationFixCompletesBosonPredictions}");

static IssueRow IssueRow(string issueId, string area, string status, string contractAssessment) =>
    new(issueId, area, status, contractAssessment, false);

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;
}

public sealed record Check(
    string CheckId,
    bool Passed,
    string Detail);

public sealed record IssueRow(
    string IssueId,
    string Area,
    string Status,
    string ContractAssessment,
    bool PresentInOpenIssues);

public sealed record CodeOnlyRepairRow(
    string RepairId,
    string RepairDescription,
    string HelpsWith,
    bool FillsPhase201Wz,
    bool FillsPhase201Higgs,
    bool CompletesBosonPredictions,
    string WhyNotSufficient);
