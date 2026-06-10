using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase398: v29 VO-6/VO-7 control-branch completion ledger audit.
//
// The v29 completion document records two closing research obligations:
// VO-6 (complete the fermionic variational branch: operator domain, adjoint
// convention, coupling terms) and VO-7 (extend the fixed bosonic deformation
// complex to the coupled boson-fermion branch: precise mixed linearization
// blocks and their gauge-compatibility identities). Phases 389-397
// materialized each named component at the DISCRETE CONTROL-BRANCH level,
// piecemeal. This audit consolidates that work into a machine-checked
// component ledger:
//
//   - For every VO-6/VO-7 component (plus the post-VO-7 electroweak chain
//     the contracts require), it reads the corresponding phase summary,
//     verifies the relevant booleans, and records
//     controlBranchStatus = verified / partial together with the precise
//     physicalStatus = missing reason.
//   - The headline booleans separate what is DONE (every VO-6 component and
//     every VO-7 component verified on the control branch, with the
//     coupled-stationarity component PARTIAL: symmetric-occupation
//     first-order stationarity proven, self-consistent coupled solve absent)
//     from what is MISSING (every physical counterpart: physical M_psi
//     branch, completed GU fermionic action, physical mixed Hessian,
//     scalar/VEV sector, hypercharge/coupling lineage, 4D observed vacuum,
//     scale/pole/GeV lineage).
//
// This phase promotes nothing: it is the start-checklist for the physical
// VO-6/VO-7 derivation effort, and it keeps every contract gate closed.

const string DefaultOutputDir = "studies/phase398_vo6_vo7_control_branch_completion_ledger_audit_001/output";

var summaryPaths = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["phase371"] = "studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/discrete_connection_dirac_first_variation_coverage_audit_summary.json",
    ["phase372"] = "studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json",
    ["phase389"] = "studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/output/vo7_mixed_linearization_gauge_compatibility_identity_probe_summary.json",
    ["phase390"] = "studies/phase390_converged_control_branch_fermion_mode_rebuild_001/output/converged_control_branch_fermion_mode_rebuild_summary.json",
    ["phase392"] = "studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/output/coupled_mixed_hessian_fermion_induced_response_audit_summary.json",
    ["phase393"] = "studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/output/coupled_stationarity_fermionic_source_residual_probe_summary.json",
    ["phase394"] = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/positive_bosonic_spectrum_backreaction_construction_summary.json",
    ["phase395"] = "studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/output/source_current_axis_structure_gauge_covariance_probe_summary.json",
    ["phase396"] = "studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/output/gauge_invariant_neutral_charged_sector_separation_probe_summary.json",
    ["phase397"] = "studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/output/parametrized_u1_extension_neutral_mixing_underdetermination_probe_summary.json",
};

var outputDir = Environment.GetEnvironmentVariable("PHASE398_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var documents = new Dictionary<string, JsonDocument>(StringComparer.Ordinal);
foreach (var (key, path) in summaryPaths)
    documents[key] = JsonDocument.Parse(File.ReadAllText(path));

bool Check(string phase, string property, bool expected = true) =>
    JsonBool(documents[phase].RootElement, property) == expected;

var components = new List<ComponentRow>
{
    // ----- VO-6: fermionic variational branch -----
    new()
    {
        Obligation = "VO-6",
        ComponentId = "vo6-first-variation-coverage",
        Description = "Connection-to-Dirac first variation delta_D[b] materialized and FD/analytic parity verified across the full variation set",
        EvidencePhase = "phase371",
        ControlBranchVerified = Check("phase371", "discreteConnectionDiracFirstVariationCoverageAuditPassed"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: variations are of the candidate bilinear at a toy background, not of a completed GU fermionic action",
    },
    new()
    {
        Obligation = "VO-6",
        ComponentId = "vo6-adjoint-convention",
        Description = "Hermitian/M-adjoint conventions verified (identity-weight control and mesh-volume M_psi generalized branch)",
        EvidencePhase = "phase390",
        ControlBranchVerified =
            Check("phase372", "discreteFermionicBilinearReciprocalMixedBlockAuditPassed") &&
            Check("phase390", "mPsiCompatibleGeneralizedControlBranchMaterialized"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: the physical M_psi convention on the GU branch is undetermined (mesh and weights are study-defined)",
    },
    new()
    {
        Obligation = "VO-6",
        ComponentId = "vo6-operator-domain",
        Description = "Operator domain characterized: 48-dim isolated-vertex structural kernel, converged nonzero spectrum, shell structure",
        EvidencePhase = "phase390",
        ControlBranchVerified = Check("phase390", "convergedControlBranchFermionModeRebuildPassed"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: toy 2D fiber-bundle mesh, not the GU 14-dimensional observerse domain",
    },
    new()
    {
        Obligation = "VO-6",
        ComponentId = "vo6-solved-fermionic-modes",
        Description = "Converged fermionic eigenmodes with sharp Ward verification (persisted Phase12 branch shown unconverged)",
        EvidencePhase = "phase390",
        ControlBranchVerified =
            Check("phase390", "wardZeroCurrentSharplyTested") &&
            Check("phase390", "persistedPhase12ModeBranchUnconverged"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: modes of the candidate bilinear, not of a derived GU fermionic operator with mass branch",
    },
    new()
    {
        Obligation = "VO-6",
        ComponentId = "vo6-coupling-terms",
        Description = "Boson-fermion coupling terms (reciprocal bilinear source blocks) materialized with central-difference and adjoint identities",
        EvidencePhase = "phase372",
        ControlBranchVerified = Check("phase372", "identityWeightControlBranchPassed"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: no explicit Yukawa functional or solved coupling map from the GU action",
    },

    // ----- VO-7: coupled boson-fermion branch -----
    new()
    {
        Obligation = "VO-7",
        ComponentId = "vo7-mixed-linearization-blocks",
        Description = "Candidate mixed-Hessian blocks 2 delta_D[e_k] psi_s materialized on converged modes and Schur-complemented exactly",
        EvidencePhase = "phase392",
        ControlBranchVerified =
            Check("phase392", "coupledMixedHessianFermionInducedResponseAuditPassed") &&
            Check("phase392", "mixedBlocksMaterializedOnConvergedModes"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: blocks of the candidate bilinear; the completed GU coupled action and its physical Hessian are absent",
    },
    new()
    {
        Obligation = "VO-7",
        ComponentId = "vo7-gauge-compatibility-identities",
        Description = "Exact discrete gauge-compatibility identity [D, X_hat] = delta_D[v(X)] + R(X) with characterized obstruction; conjugates exactly onto the M_psi branch",
        EvidencePhase = "phase389",
        ControlBranchVerified =
            Check("phase389", "discreteGaugeCompatibilityIdentityExact") &&
            Check("phase390", "mPsiWeightCommutesWithGaugeAction"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: physical-branch identities require the completed GU fermionic operator and physical M_psi",
    },
    new()
    {
        Obligation = "VO-7",
        ComponentId = "vo7-coupled-stationarity",
        Description = "First-order coupled stationarity characterized: exact plus/minus source cancellation (symmetric occupation stationary); asymmetric backreaction constructed from the recomputed positive bosonic spectrum",
        EvidencePhase = "phase393",
        ControlBranchVerified =
            Check("phase393", "shellAggregatedSourceCancels") &&
            Check("phase394", "firstOrderAsymmetricBackreactionConstructed"),
        ControlBranchStatus = "partial: first-order only; no self-consistent coupled critical-point solve has been performed",
        PhysicalStatus = "missing: a coupled GU critical point with fermionic backreaction at a physical background",
    },
    new()
    {
        Obligation = "VO-7",
        ComponentId = "vo7-effective-source-operator",
        Description = "Action-derived fermion-induced response operator (degenerate second-order perturbation) constructed; shown to diverge from the study-defined Gram (suppressed axis metric-dependent)",
        EvidencePhase = "phase392",
        ControlBranchVerified = Check("phase392", "coupledMixedHessianFermionInducedResponseAuditPassed"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: a physical effective-action Hessian replacing both study-defined response metrics",
    },

    // ----- Post-VO-7 electroweak chain (contract-required) -----
    new()
    {
        Obligation = "EW-chain",
        ComponentId = "ew-gauge-covariant-axis-structure",
        Description = "Carrier axis structure fully explained: rank-one response in the charged plane orthogonal to the symmetric-ansatz axis; exactly gauge-covariant, not canonical",
        EvidencePhase = "phase395",
        ControlBranchVerified =
            Check("phase395", "suppressedAxisIsGaugeCovariantNotCanonical") &&
            Check("phase395", "blockGramEffectivelyRankOne"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "n/a (structural result); physical analog requires the GU background, not the symmetric ansatz",
    },
    new()
    {
        Obligation = "EW-chain",
        ComponentId = "ew-sector-skeleton",
        Description = "Exact residual-U(1) sector separation: all 68 bosonic triplets split 1 neutral + 1 charged pair; gauge-invariant extraction at machine precision",
        EvidencePhase = "phase396",
        ControlBranchVerified = Check("phase396", "tripletNeutralChargedSplitObserved"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: observed-name identification requires the electroweak embedding and scalar sector",
    },
    new()
    {
        Obligation = "EW-chain",
        ComponentId = "ew-mixing-machinery-and-gap",
        Description = "u(1) extension machinery materialized; Z-like channel sourceless; fermion-bilinear neutral mixing vanishes (trace selection rule); photon/Z separation underdetermined",
        EvidencePhase = "phase397",
        ControlBranchVerified =
            Check("phase397", "parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed") &&
            Check("phase397", "neutralMixingElementVanishesInFermionBilinearChannel"),
        ControlBranchStatus = "verified",
        PhysicalStatus = "missing: hypercharge lineage, coupling-ratio lineage, and the symmetry-breaking scalar/VEV sector (welded to the Phase201 Higgs scalar source row)",
    },
};

int vo6Total = components.Count(component => component.Obligation == "VO-6");
int vo6Verified = components.Count(component => component.Obligation == "VO-6" && component.ControlBranchVerified);
int vo7Total = components.Count(component => component.Obligation == "VO-7");
int vo7Verified = components.Count(component => component.Obligation == "VO-7" && component.ControlBranchVerified);
int ewTotal = components.Count(component => component.Obligation == "EW-chain");
int ewVerified = components.Count(component => component.Obligation == "EW-chain" && component.ControlBranchVerified);

bool vo6ControlBranchComponentsComplete = vo6Verified == vo6Total;
bool vo7ControlBranchComponentsComplete = vo7Verified == vo7Total;
bool coupledStationarityRemainsPartial = components.Single(component => component.ComponentId == "vo7-coupled-stationarity").ControlBranchStatus.StartsWith("partial", StringComparison.Ordinal);
bool electroweakChainComponentsComplete = ewVerified == ewTotal;
bool physicalCompletionStillMissing = true;

var physicalGapLedger = new[]
{
    "physical M_psi-compatible GU fermionic branch (mesh, weights, operator all study-defined)",
    "completed GU fermionic action with explicit Yukawa functional (VO-6 physical)",
    "physical coupled effective-action Hessian (VO-7 physical)",
    "self-consistent coupled critical point with fermionic backreaction",
    "symmetry-breaking scalar/VEV sector (Phase201 Higgs scalar source row; also required for photon/Z mixing per Phase397)",
    "hypercharge assignments and coupling-ratio (weak angle) lineage",
    "four-dimensional observed vacuum (toy 2D fiber-bundle control branch only)",
    "source-scale, pole-extraction, and GeV-normalization lineage",
};

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
const string ApplicationSubjectKind = "vo6-vo7-control-branch-completion-ledger-audit";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    string.Join(",", components.Select(component => $"{component.ComponentId}:{component.ControlBranchVerified}")),
    string.Join(",", physicalGapLedger))))).ToLowerInvariant();

bool vo6Vo7ControlBranchCompletionLedgerAuditPassed =
    vo6ControlBranchComponentsComplete &&
    vo7ControlBranchComponentsComplete &&
    coupledStationarityRemainsPartial &&
    electroweakChainComponentsComplete &&
    physicalCompletionStillMissing &&
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

string terminalStatus = vo6Vo7ControlBranchCompletionLedgerAuditPassed
    ? "vo6-vo7-control-branch-components-complete-physical-completion-still-missing"
    : "vo6-vo7-control-branch-completion-ledger-audit-blocked";
string decision = vo6Vo7ControlBranchCompletionLedgerAuditPassed
    ? "Every component named by the v29 obligations VO-6 (operator domain, adjoint convention, coupling terms, solved modes, first-variation coverage) and VO-7 (mixed linearization blocks, gauge-compatibility identities, effective source operator) is now VERIFIED at the discrete control-branch level, with one component partial (coupled stationarity: first-order picture proven, self-consistent coupled solve absent), and the post-VO-7 electroweak chain (gauge-covariant axis structure, exact sector skeleton, mixing machinery with its named gap) is also verified. The physical completion remains entirely missing, with the gap ledger enumerated component by component. This audit is the start-checklist for the physical VO-6/VO-7 derivation effort: it promotes nothing, fills no contract field, and keeps every gate closed."
    : "Do not use the completion ledger until every component check against the underlying phase summaries passes.";

var result = new
{
    phaseId = "phase398-vo6-vo7-control-branch-completion-ledger-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    vo6Vo7ControlBranchCompletionLedgerAuditPassed,
    vo6ControlBranchComponentsComplete,
    vo7ControlBranchComponentsComplete,
    coupledStationarityRemainsPartial,
    electroweakChainComponentsComplete,
    physicalCompletionStillMissing,
    vo6ComponentCount = vo6Total,
    vo6VerifiedCount = vo6Verified,
    vo7ComponentCount = vo7Total,
    vo7VerifiedCount = vo7Verified,
    electroweakChainComponentCount = ewTotal,
    electroweakChainVerifiedCount = ewVerified,
    components = components.Select(component => new
    {
        obligation = component.Obligation,
        componentId = component.ComponentId,
        description = component.Description,
        evidencePhase = component.EvidencePhase,
        evidenceSummaryPath = summaryPaths[component.EvidencePhase],
        controlBranchVerified = component.ControlBranchVerified,
        controlBranchStatus = component.ControlBranchStatus,
        physicalStatus = component.PhysicalStatus,
    }).ToArray(),
    physicalGapLedger,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
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
        "control-branch completion ledger only; physical VO-6/VO-7 remain open obligations",
        "the coupled-stationarity component is partial (no self-consistent coupled solve)",
        "no contract fields filled; all promotion gates closed",
        "no physical predictions",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "vo6_vo7_control_branch_completion_ledger_audit.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "vo6_vo7_control_branch_completion_ledger_audit_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vo6Vo7ControlBranchCompletionLedgerAuditPassed={vo6Vo7ControlBranchCompletionLedgerAuditPassed}");
Console.WriteLine($"vo6ControlBranchComponentsComplete={vo6ControlBranchComponentsComplete} ({vo6Verified}/{vo6Total})");
Console.WriteLine($"vo7ControlBranchComponentsComplete={vo7ControlBranchComponentsComplete} ({vo7Verified}/{vo7Total})");
Console.WriteLine($"coupledStationarityRemainsPartial={coupledStationarityRemainsPartial}");
Console.WriteLine($"electroweakChainComponentsComplete={electroweakChainComponentsComplete} ({ewVerified}/{ewTotal})");
Console.WriteLine($"physicalCompletionStillMissing={physicalCompletionStillMissing}");
Console.WriteLine($"physicalGapLedgerCount={physicalGapLedger.Length}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

foreach (var document in documents.Values)
    document.Dispose();

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

public sealed class ComponentRow
{
    public required string Obligation { get; init; }
    public required string ComponentId { get; init; }
    public required string Description { get; init; }
    public required string EvidencePhase { get; init; }
    public required bool ControlBranchVerified { get; init; }
    public required string ControlBranchStatus { get; init; }
    public required string PhysicalStatus { get; init; }
}
