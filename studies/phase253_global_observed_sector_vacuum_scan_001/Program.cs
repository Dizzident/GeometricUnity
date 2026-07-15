using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase253_global_observed_sector_vacuum_scan_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase230Path = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output/native_gu_vacuum_hessian_candidate_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE253_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase230 = JsonDocument.Parse(File.ReadAllText(Phase230Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));

var scannedRoots = new[] { "studies", "reports", "src", "docs" };
var searchableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".json",
    ".md",
    ".cs",
};
var excludedPathFragments = new[]
{
    "/bin/",
    "/obj/",
    "/output/",
    "studies/phase253_global_observed_sector_vacuum_scan_001/",
    // 2026-07-02: phase441 family-universality study legitimately DISCUSSES
    // the dimX >= 4 requirement as a recorded impossibility statement
    // (canonical Shiab not realizable on the 2D toy); it is a generated
    // fail-closed diagnostic artifact, not an observed-sector vacuum claim.
    "studies/phase441_toy_branch_family_universality_sweep_001/",
    // 2026-07-03: phase442 runs ON the dimension-four platform (necessary-not-
    // sufficient degree-lift probe); a generated fail-closed diagnostic
    // artifact, not an observed-sector vacuum claim.
    "studies/phase442_joint_omega_theta_hessian_degree_probe_001/",
    "studies/phase443_joint_effective_potential_saturation_probe_001/",
    "studies/phase444_mode_volume_scaled_saturation_probe_001/",
    "studies/phase445_rg_improved_joint_potential_probe_001/",
    "studies/phase446_rg_scheme_dependence_resolution_probe_001/",
    "studies/phase447_two_loop_saturation_probe_001/",
    "studies/phase448_torus_mode_volume_saturation_probe_001/",
    "studies/phase449_variational_gaussian_effective_potential_probe_001/",
    "studies/phase450_constraint_effective_potential_hmc_probe_001/",
    "studies/phase451_two_loop_unification_ledger_001/",
    "studies/phase453_wham_parity_error_model_repair_001/",
    "studies/phase454_beyond_ray_quadratic_certificate_probe_001/",
    "studies/phase460_source_corpus_units_equivariance_kernel_001/",
    "studies/phase461_dimensional_transmutation_reading_menu_001/",
    // 2026-07-12 Wave-2 STEP 0 skeletons: generated fail-closed diagnostic
    // artifacts (source/anchor/VEV audit phases), never observed-sector vacuum
    // claims.
    "studies/phase455_exact_fermionic_backreaction_probe_001/",
    "studies/phase456_consolidated_n4_launch_001/",
    "studies/phase457_upsilon_portal_stage_a_001/",
    "studies/phase458_binder_go_no_go_gate_001/",
    "studies/phase462_blocking_set_resolution_001/",
    "studies/phase463_transport_structure_theorems_001/",
    "studies/phase464_anchor_adjudication_contract_001/",
    "studies/phase465_anomaly_consistency_variety_kernel_001/",
    "studies/phase466_ws3_vev_completion_contract_001/",
    "studies/phase467_derived_operator_stabilizer_ray_census_001/",
    "studies/phase468_two_loop_content_row_closure_filter_001/",
    "studies/phase469_c_lift_representation_bookkeeping_gate_001/",
    "studies/phase470_c_permanence_five_limb_ledger_001/",
    "studies/phase471_b2_closure_ledger_001/",
    "studies/phase477_o4_adjudication_infrastructure_001/",
    "studies/phase478_phase458_gate_specification_closure_001/",
    "studies/phase479_phase457_post_ruling_readiness_001/",
    "studies/phase480_o4_physicist_adjudication_intake_001/",
    "studies/phase481_phase456_prospective_repair_preregistration_001/",
    "studies/phase482_a5_theorem_scout_001/",
    "studies/phase483_source_defined_reopening_intake_001/",
    "studies/phase484_exploratory_lane_governance_firewall_001/",
    "studies/phase485_o4_assumption_falsifier_census_001/",
    "studies/phase486_committed_evidence_sensitivity_triage_001/",
    "studies/phase487_independent_so3_haar_measure_control_001/",
    "studies/phase488_haar_proposal_invariance_control_001/",
    "studies/phase489_reduced_sampler_restart_equivalence_001/",
    "studies/phase490_zero_mode_quotient_audit_001/",
    "studies/phase491_committed_bosonic_model_family_sensitivity_001/",
    "studies/phase492_phase455_combined_robustness_adjudicator_001/",
    "studies/phase493_phase456_stored_artifact_failure_decomposition_001/",
    "studies/phase494_phase456_estimator_oracle_battery_001/",
    "studies/phase495_phase456_prospective_repair_readiness_adjudicator_001/",
    "studies/phase496_phase456_retained_data_information_census_001/",
    "studies/phase497_phase456_prospective_estimator_acquisition_oracle_001/",
    "studies/phase498_phase456_acquisition_repair_readiness_adjudicator_001/",
    "studies/phase499_phase456_retained_empirical_noise_information_audit_001/",
    "studies/phase500_phase456_adversarial_prospective_acquisition_stress_test_001/",
    "studies/phase501_phase456_robust_sampling_readiness_adjudicator_001/",
    "studies/phase502_phase456_adaptive_calibration_protocol_specification_001/",
    "studies/phase503_phase456_adaptive_calibration_protocol_validation_001/",
    "studies/phase504_phase456_calibration_repair_pack_readiness_adjudicator_001/",
    "studies/phase505_phase503_frozen_failure_localization_001/",
    "studies/phase506_phase456_selective_inference_protocol_validation_001/",
    "studies/phase507_phase456_selective_inference_pack_readiness_adjudicator_001/",
    "docs/Phases/Implementation/IMPLEMENTATION_P477.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P478.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P479.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P480.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P481.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P482.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P483.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P484.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P485.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P486.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P487.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P488.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P489.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P490.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P491.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P492.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P493.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P494.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P495.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P496.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P497.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P498.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P499.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P500.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P501.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P502.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P503.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P504.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P505.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P506.md",
    "docs/Phases/Implementation/IMPLEMENTATION_P507.md",
    "docs/Phases/EXPLORATORY_SELF_AUDIT_PLAN_2026-07-15.md",
    "docs/Phases/CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15.md",
    "docs/Phases/PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15.md",
    "docs/Phases/PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15.md",
};

var dimension4Pattern = new Regex("""
    ("baseDimension"\s*:\s*4)
    |("dimX"\s*:\s*4)
    |("dimension"\s*:\s*4)
    |(baseDimension\s*=\s*4)
    |(dimX\s*>=\s*4)
    """, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var observedVacuumPattern = new Regex("""observed[-\s]?sector|electroweak|vacuum|VEV|mass[-\s]?matrix|Hessian""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var productionArtifactPattern = new Regex("""^((studies|reports)/).*\.(json|md)$""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var documentationOrTemplatePattern = new Regex("""^(docs/|tests/)|/Tests/|/Gaps/|/Guides/|/Architecture/|HOW_TO\.md$""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var fourDimensionalReferenceRows = new List<ReferenceRow>();
var productionFourDimensionalRows = new List<ReferenceRow>();
var productionObservedSectorCandidateRows = new List<ReferenceRow>();
var hessianLikeModeRows = new List<string>();

foreach (var file in EnumerateCandidateFiles(scannedRoots, searchableExtensions, excludedPathFragments))
{
    var text = SafeReadText(file);
    if (text.Length == 0)
    {
        continue;
    }

    var normalizedPath = NormalizePath(file);
    var hasDimension4Signal = dimension4Pattern.IsMatch(text);
    var hasObservedVacuumSignal = observedVacuumPattern.IsMatch(text);
    var isProductionArtifact = productionArtifactPattern.IsMatch(normalizedPath) && !documentationOrTemplatePattern.IsMatch(normalizedPath);

    if (text.Contains("\"operatorType\": \"FullHessian\"", StringComparison.Ordinal)
        || text.Contains("\"operatorType\":\"FullHessian\"", StringComparison.Ordinal)
        || text.Contains("FullHessian", StringComparison.Ordinal))
    {
        if (normalizedPath.StartsWith("studies/", StringComparison.Ordinal))
        {
            hessianLikeModeRows.Add(normalizedPath);
        }
    }

    if (!hasDimension4Signal)
    {
        continue;
    }

    var row = new ReferenceRow(
        normalizedPath,
        isProductionArtifact,
        documentationOrTemplatePattern.IsMatch(normalizedPath),
        hasObservedVacuumSignal,
        ExtractSnippet(text, dimension4Pattern));
    fourDimensionalReferenceRows.Add(row);

    if (isProductionArtifact)
    {
        productionFourDimensionalRows.Add(row);
        if (hasObservedVacuumSignal)
        {
            productionObservedSectorCandidateRows.Add(row);
        }
    }
}

var phase230AuditPassed = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidateAuditPassed") is true;
var phase230Promotable = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidatePromotable") is true;
var phase228ObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;
var phase228Promotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var phase229ObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var phase229Promotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p248NewHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var productionFourDimensionalReferenceCount = productionFourDimensionalRows.Count;
var productionObservedSectorVacuumCandidateCount = productionObservedSectorCandidateRows.Count;
var hessianLikeModeArtifactCount = hessianLikeModeRows.Count;
var globalObservedSectorVacuumCandidateFound = productionObservedSectorVacuumCandidateCount > 0;
var globalScanFillsVacuumMassMatrixUnlock = globalObservedSectorVacuumCandidateFound
    && !phase228Promotable
    && !phase229Promotable
    && !p245UnlockContractFilled;
var newSourceEvidenceStillRequired = !globalScanFillsVacuumMassMatrixUnlock
    && wzMissingFieldCount == 15
    && higgsMissingFieldCount == 14
    && p248NewHiggsScalarSourceStillRequired;

var checks = new[]
{
    new Check(
        "phase230-local-native-route-still-blocked",
        phase230AuditPassed && !phase230Promotable,
        $"phase230AuditPassed={phase230AuditPassed}; phase230Promotable={phase230Promotable}"),
    new Check(
        "global-production-four-dimensional-vacuum-candidate-absent",
        productionObservedSectorVacuumCandidateCount == 0,
        $"productionFourDimensionalReferenceCount={productionFourDimensionalReferenceCount}; productionObservedSectorVacuumCandidateCount={productionObservedSectorVacuumCandidateCount}"),
    new Check(
        "four-dimensional-references-are-documentation-code-or-negative-requirement-guards",
        fourDimensionalReferenceRows.Count > 0
            && productionObservedSectorVacuumCandidateCount == 0
            && productionFourDimensionalRows.All(row => !row.HasObservedVacuumSignal),
        $"fourDimensionalReferenceCount={fourDimensionalReferenceRows.Count}; productionFourDimensionalReferenceCount={productionFourDimensionalReferenceCount}; productionObservedSectorVacuumCandidateCount={productionObservedSectorVacuumCandidateCount}"),
    new Check(
        "hessian-like-artifacts-do-not-clear-mass-matrix-gate",
        hessianLikeModeArtifactCount > 0 && phase228ObstructionCertified && !phase228Promotable,
        $"hessianLikeModeArtifactCount={hessianLikeModeArtifactCount}; phase228ObstructionCertified={phase228ObstructionCertified}; phase228Promotable={phase228Promotable}"),
    new Check(
        "gu-vev-source-still-absent",
        phase229ObstructionCertified && !phase229Promotable,
        $"phase229ObstructionCertified={phase229ObstructionCertified}; phase229Promotable={phase229Promotable}"),
    new Check(
        "source-lineage-blockers-preserved",
        wzMissingFieldCount == 15 && higgsMissingFieldCount == 14 && !p245UnlockContractFilled && p248NewHiggsScalarSourceStillRequired,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; p245UnlockContractFilled={p245UnlockContractFilled}; p248NewHiggsScalarSourceStillRequired={p248NewHiggsScalarSourceStillRequired}"),
    new Check(
        "new-source-evidence-still-required",
        newSourceEvidenceStillRequired,
        $"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; globalScanFillsVacuumMassMatrixUnlock={globalScanFillsVacuumMassMatrixUnlock}"),
};

var globalObservedSectorVacuumScanPassed = checks.All(check => check.Passed);
var terminalStatus = globalObservedSectorVacuumScanPassed
    ? "global-observed-sector-vacuum-scan-no-production-candidate"
    : "global-observed-sector-vacuum-scan-review-required";

var result = new
{
    phaseId = "phase253-global-observed-sector-vacuum-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    globalObservedSectorVacuumScanPassed,
    globalObservedSectorVacuumCandidateFound,
    productionFourDimensionalReferenceCount,
    productionObservedSectorVacuumCandidateCount,
    documentationOrCodeFourDimensionalReferenceCount = fourDimensionalReferenceRows.Count - productionFourDimensionalReferenceCount,
    hessianLikeModeArtifactCount,
    globalScanFillsVacuumMassMatrixUnlock,
    newSourceEvidenceStillRequired,
    objective = "Scan the whole repository for a production four-dimensional observed-sector GU vacuum or physical mass-matrix source artifact outside the narrower Phase230 local audit.",
    scanScope = new
    {
        scannedRoots,
        searchableExtensions = searchableExtensions.Order(StringComparer.Ordinal).ToArray(),
        excludedPathFragments,
    },
    referenceRows = fourDimensionalReferenceRows
        .OrderBy(row => row.Path, StringComparer.Ordinal)
        .ToArray(),
    productionFourDimensionalRows = productionFourDimensionalRows
        .OrderBy(row => row.Path, StringComparer.Ordinal)
        .ToArray(),
    productionObservedSectorCandidateRows = productionObservedSectorCandidateRows
        .OrderBy(row => row.Path, StringComparer.Ordinal)
        .ToArray(),
    hessianLikeModeArtifactSample = hessianLikeModeRows
        .Order(StringComparer.Ordinal)
        .Take(20)
        .ToArray(),
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase228 = new
        {
            phase228ObstructionCertified,
            phase228Promotable,
        },
        phase229 = new
        {
            phase229ObstructionCertified,
            phase229Promotable,
        },
        phase230 = new
        {
            phase230AuditPassed,
            phase230Promotable,
        },
        phase245 = new
        {
            p245UnlockContractFilled,
        },
        phase248 = new
        {
            p248NewHiggsScalarSourceStillRequired,
        },
    },
    checks,
    decision = globalObservedSectorVacuumScanPassed
        ? "Do not promote any current repository artifact as the missing four-dimensional observed-sector GU vacuum or physical W/Z/H mass matrix. Four-dimensional references found by the scan are documentation, guide, code-guard, or negative requirement-guard references; production artifacts remain lower-dimensional/source-diagnostic material and Phase228/229/230 blockers stay active."
        : "Review global observed-sector vacuum scan before relying on the local-route exhaustion boundary.",
    nextRequiredArtifact = new[]
    {
        "A production four-dimensional observed-sector GU vacuum/background artifact with a source-derived selection rule.",
        "A draft-aligned Shiab/Upsilon extraction theorem and gauge-consistent physical W/Z/H mass matrix.",
        "A GU-derived electroweak VEV/scale and Higgs scalar source satisfying Phase201/209/210/213.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase228Path = Phase228Path,
        phase229Path = Phase229Path,
        phase230Path = Phase230Path,
        phase245Path = Phase245Path,
        phase248Path = Phase248Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "global_observed_sector_vacuum_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "global_observed_sector_vacuum_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.globalObservedSectorVacuumScanPassed,
        result.globalObservedSectorVacuumCandidateFound,
        result.productionFourDimensionalReferenceCount,
        result.productionObservedSectorVacuumCandidateCount,
        result.documentationOrCodeFourDimensionalReferenceCount,
        result.hessianLikeModeArtifactCount,
        result.globalScanFillsVacuumMassMatrixUnlock,
        result.newSourceEvidenceStillRequired,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"globalObservedSectorVacuumScanPassed={globalObservedSectorVacuumScanPassed}");
Console.WriteLine($"productionFourDimensionalReferenceCount={productionFourDimensionalReferenceCount}");
Console.WriteLine($"productionObservedSectorVacuumCandidateCount={productionObservedSectorVacuumCandidateCount}");
Console.WriteLine($"hessianLikeModeArtifactCount={hessianLikeModeArtifactCount}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static IEnumerable<string> EnumerateCandidateFiles(IEnumerable<string> roots, ISet<string> extensions, string[] excludedPathFragments)
{
    foreach (var root in roots)
    {
        if (!Directory.Exists(root))
        {
            continue;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var normalized = NormalizePath(file);
            if (!extensions.Contains(Path.GetExtension(file)))
            {
                continue;
            }

            if (excludedPathFragments.Any(fragment => normalized.Contains(fragment, StringComparison.Ordinal)))
            {
                continue;
            }

            yield return file;
        }
    }
}

static string SafeReadText(string path)
{
    try
    {
        return File.ReadAllText(path);
    }
    catch
    {
        return "";
    }
}

static string NormalizePath(string path) => path.Replace('\\', '/').TrimStart('.', '/');

static string ExtractSnippet(string text, Regex pattern)
{
    var match = pattern.Match(text);
    if (!match.Success)
    {
        return "";
    }

    var start = Math.Max(0, match.Index - 80);
    var length = Math.Min(text.Length - start, match.Length + 160);
    return Regex.Replace(text.Substring(start, length), @"\s+", " ").Trim();
}

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ReferenceRow(string Path, bool IsProductionArtifact, bool IsDocumentationOrTemplate, bool HasObservedVacuumSignal, string Snippet);
sealed record Check(string CheckId, bool Passed, string Detail);
