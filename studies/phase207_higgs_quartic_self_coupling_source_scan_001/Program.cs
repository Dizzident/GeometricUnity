using System.Text.Json;

const string DefaultOutputDir = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output";
const int MaxFindings = 240;

var outputDir = Environment.GetEnvironmentVariable("PHASE207_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var roots = new[]
{
    "studies",
    "docs",
    "TheoryCompletitionRevisions",
    "src",
};
var selfOutputPrefix = Path.Combine("studies", "phase207_higgs_quartic_self_coupling_source_scan_001", "output") + Path.DirectorySeparatorChar;

var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".md",
    ".cs",
    ".json",
};

var allCandidateFindings = new List<Finding>();
var retainedFindings = new List<Finding>();
var scannedFileCount = 0;
var relevantFileCount = 0;

foreach (var file in roots
    .Where(Directory.Exists)
    .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
    .Where(file => extensions.Contains(Path.GetExtension(file)))
    .Where(file => !IsReferenceTrackerFile(file.Replace('\\', '/').TrimStart('.', '/')))
    .Where(file => !file.StartsWith(selfOutputPrefix, StringComparison.Ordinal))
    .OrderBy(file => file, StringComparer.Ordinal))
{
    scannedFileCount++;
    string[] lines;
    try
    {
        lines = File.ReadAllLines(file);
    }
    catch
    {
        continue;
    }

    var fileHadRelevantLine = false;
    for (var i = 0; i < lines.Length; i++)
    {
        var line = lines[i].Trim();
        if (line.Length == 0)
            continue;

        var lower = line.ToLowerInvariant();
        if (!IsPotentialRelevant(lower))
            continue;

        fileHadRelevantLine = true;
        var blockers = ClassifyBlockers(lower, file);
        var evidenceSignals = ClassifyEvidenceSignals(lower);
        var intakeReady = blockers.Count == 0
            && evidenceSignals.Contains("higgs-sector")
            && evidenceSignals.Contains("source-or-operator")
            && evidenceSignals.Contains("potential-or-self-coupling")
            && evidenceSignals.Contains("massive-profile-or-excitation")
            && evidenceSignals.Contains("target-independent")
            && evidenceSignals.Contains("promotable-or-validated");

        var finding = new Finding(
            file,
            i + 1,
            DetermineKind(lower, file),
            intakeReady,
            evidenceSignals.ToArray(),
            blockers.ToArray(),
            line.Length > 360 ? line[..360] : line);

        allCandidateFindings.Add(finding);
        if (retainedFindings.Count < MaxFindings || intakeReady)
            retainedFindings.Add(finding);
    }

    if (fileHadRelevantLine)
        relevantFileCount++;
}

var intakeReadyFindings = allCandidateFindings.Where(finding => finding.IntakeReady).ToArray();
var blockerHistogram = allCandidateFindings
    .SelectMany(finding => finding.Blockers)
    .GroupBy(blocker => blocker, StringComparer.Ordinal)
    .Select(group => new { blocker = group.Key, count = group.Count() })
    .OrderByDescending(row => row.count)
    .ThenBy(row => row.blocker, StringComparer.Ordinal)
    .ToArray();

var signalHistogram = allCandidateFindings
    .SelectMany(finding => finding.EvidenceSignals)
    .GroupBy(signal => signal, StringComparer.Ordinal)
    .Select(group => new { signal = group.Key, count = group.Count() })
    .OrderByDescending(row => row.count)
    .ThenBy(row => row.signal, StringComparer.Ordinal)
    .ToArray();

var canPromoteHiggsQuarticSelfCouplingSource = intakeReadyFindings.Length > 0;
var terminalStatus = canPromoteHiggsQuarticSelfCouplingSource
    ? "higgs-quartic-self-coupling-source-scan-found-intake-ready-evidence"
    : "higgs-quartic-self-coupling-source-scan-no-source";

var result = new
{
    phaseId = "phase207-higgs-quartic-self-coupling-source-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteHiggsQuarticSelfCouplingSource,
    scannedFileCount,
    relevantFileCount,
    candidateFindingCount = allCandidateFindings.Count,
    retainedFindingCount = retainedFindings.Count,
    intakeReadyFindingCount = intakeReadyFindings.Length,
    blockerHistogram,
    signalHistogram,
    intakeReadyFindings,
    findings = retainedFindings,
    decision = canPromoteHiggsQuarticSelfCouplingSource
        ? "At least one local text/source artifact appears to contain an intake-ready Higgs potential or self-coupling source. Review before promotion."
        : "Do not promote a Higgs mass prediction from local quartic/self-coupling material. The local hits are open issues, draft approximations/postdictions, generic gauge-lambda/eigenvalue notation, or already-blocked audit text rather than a solved target-independent Higgs potential/self-coupling source.",
    nextRequiredArtifact = "A solved target-independent Higgs scalar potential or self-coupling operator with quartic/higher interaction evidence, Higgs excitation identity, massive scalar profile, promotion gates, and stability sidecars.",
    sourceEvidence = new
    {
        scannedRoots = roots,
        scannedExtensions = extensions.OrderBy(extension => extension, StringComparer.Ordinal).ToArray(),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_quartic_self_coupling_source_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_quartic_self_coupling_source_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteHiggsQuarticSelfCouplingSource,
        result.scannedFileCount,
        result.relevantFileCount,
        result.candidateFindingCount,
        result.retainedFindingCount,
        result.intakeReadyFindingCount,
        result.blockerHistogram,
        result.signalHistogram,
        result.intakeReadyFindings,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteHiggsQuarticSelfCouplingSource={canPromoteHiggsQuarticSelfCouplingSource}");
Console.WriteLine($"candidateFindingCount={allCandidateFindings.Count}");
Console.WriteLine($"intakeReadyFindingCount={intakeReadyFindings.Length}");

static bool IsPotentialRelevant(string lower)
{
    var hasPotentialTerm = lower.Contains("quartic", StringComparison.Ordinal)
        || lower.Contains("self-coupling", StringComparison.Ordinal)
        || lower.Contains("self coupling", StringComparison.Ordinal)
        || lower.Contains("scalar potential", StringComparison.Ordinal)
        || lower.Contains("higgs potential", StringComparison.Ordinal)
        || lower.Contains("potential/self", StringComparison.Ordinal)
        || lower.Contains("massive scalar", StringComparison.Ordinal)
        || lower.Contains("scalar excitation", StringComparison.Ordinal)
        || lower.Contains("lambda", StringComparison.Ordinal)
        || lower.Contains("\\lambda", StringComparison.Ordinal)
        || lower.Contains("λ", StringComparison.Ordinal);

    if (!hasPotentialTerm)
        return false;

    var hasHiggsOrScalarContext = lower.Contains("higgs", StringComparison.Ordinal)
        || lower.Contains("scalar", StringComparison.Ordinal)
        || lower.Contains("quartic", StringComparison.Ordinal)
        || lower.Contains("self-coupling", StringComparison.Ordinal)
        || lower.Contains("self coupling", StringComparison.Ordinal)
        || lower.Contains("massive scalar", StringComparison.Ordinal);

    return hasHiggsOrScalarContext;
}

static bool IsReferenceTrackerFile(string normalizedPath) =>
    normalizedPath == "ExperimentReferences.md"
    || normalizedPath.StartsWith("docs/Reference/ExperimentReferences/", StringComparison.Ordinal);

static string DetermineKind(string lower, string file)
{
    if (lower.Contains("higgs", StringComparison.Ordinal))
        return "higgs-potential-or-self-coupling";
    if (lower.Contains("quartic", StringComparison.Ordinal) || lower.Contains("self-coupling", StringComparison.Ordinal) || lower.Contains("self coupling", StringComparison.Ordinal))
        return "quartic-or-self-coupling";
    if (lower.Contains("massive scalar", StringComparison.Ordinal) || lower.Contains("scalar excitation", StringComparison.Ordinal))
        return "scalar-excitation";
    if (file.Contains("Phase3", StringComparison.OrdinalIgnoreCase) || file.Contains("phase3", StringComparison.OrdinalIgnoreCase))
        return "phase3-scalar-spectrum";
    return "generic-scalar-or-lambda";
}

static List<string> ClassifyEvidenceSignals(string lower)
{
    var signals = new List<string>();
    AddIf(signals, lower.Contains("higgs", StringComparison.Ordinal), "higgs-sector");
    AddIf(signals, lower.Contains("source", StringComparison.Ordinal) || lower.Contains("operator", StringComparison.Ordinal), "source-or-operator");
    AddIf(signals, lower.Contains("potential", StringComparison.Ordinal)
        || lower.Contains("quartic", StringComparison.Ordinal)
        || lower.Contains("self-coupling", StringComparison.Ordinal)
        || lower.Contains("self coupling", StringComparison.Ordinal), "potential-or-self-coupling");
    AddIf(signals, lower.Contains("massive scalar", StringComparison.Ordinal)
        || lower.Contains("scalar excitation", StringComparison.Ordinal)
        || lower.Contains("excitation", StringComparison.Ordinal), "massive-profile-or-excitation");
    AddIf(signals, lower.Contains("target-independent", StringComparison.Ordinal)
        || lower.Contains("target independent", StringComparison.Ordinal)
        || lower.Contains("target-blind", StringComparison.Ordinal), "target-independent");
    AddIf(signals, lower.Contains("promotable", StringComparison.Ordinal)
        || lower.Contains("promoted", StringComparison.Ordinal)
        || lower.Contains("validated", StringComparison.Ordinal)
        || lower.Contains("passed", StringComparison.Ordinal), "promotable-or-validated");
    AddIf(signals, lower.Contains("stability", StringComparison.Ordinal)
        || lower.Contains("sidecar", StringComparison.Ordinal)
        || lower.Contains("stable", StringComparison.Ordinal), "stability-evidence");
    return signals;
}

static List<string> ClassifyBlockers(string lower, string file)
{
    var blockers = new List<string>();
    AddIf(blockers, lower.Contains("open issue", StringComparison.Ordinal)
        || lower.Contains("defer", StringComparison.Ordinal)
        || lower.Contains("deferred", StringComparison.Ordinal)
        || lower.Contains("not approximated", StringComparison.Ordinal), "open-or-deferred-material");
    AddIf(blockers, lower.Contains("approximate", StringComparison.Ordinal)
        || lower.Contains("postdiction", StringComparison.Ordinal)
        || lower.Contains("not fully derived", StringComparison.Ordinal)
        || lower.Contains("requires explicit", StringComparison.Ordinal)
        || lower.Contains("should be", StringComparison.Ordinal), "draft-approximate-or-postdictive");
    AddIf(blockers, lower.Contains("blocked", StringComparison.Ordinal)
        || lower.Contains("missing", StringComparison.Ordinal)
        || lower.Contains("no solved", StringComparison.Ordinal)
        || lower.Contains("do not promote", StringComparison.Ordinal)
        || lower.Contains("not promote", StringComparison.Ordinal)
        || lower.Contains("not a higgs", StringComparison.Ordinal), "negative-audit-text");
    AddIf(blockers, lower.Contains("gauge-lambda", StringComparison.Ordinal)
        || lower.Contains("gauge lambda", StringComparison.Ordinal)
        || lower.Contains("gauge penalty", StringComparison.Ordinal)
        || lower.Contains("generalized eigenproblem", StringComparison.Ordinal)
        || lower.Contains("eigenvalue", StringComparison.Ordinal), "generic-gauge-or-eigenvalue-lambda");
    AddIf(blockers, file.Contains("phase207_higgs_quartic_self_coupling_source_scan", StringComparison.Ordinal), "self-scan-artifact");
    AddIf(blockers, file.Contains("phase223_higgs_casimir_quartic_numerical_probe", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase224_electroweak_parameter_dependency_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase225_su2_normalization_representation_compatibility_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase226_official_gu_higgs_potential_notation_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase227_official_gu_shiab_upsilon_extraction_obstruction_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase228_boson_mass_matrix_extraction_obstruction_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase229_electroweak_vev_source_lineage_obstruction_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase230_native_gu_vacuum_hessian_candidate_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase231_external_cox_gu_paper_i_source_intake_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase232_external_cox_gu_paper_ii_source_intake_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase233_external_cox_gu_papers_iii_iv_source_intake_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase234_cox_ii_electroweak_formula_dependency_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase235_pati_salam_weak_mixing_normalization_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase236_low_energy_rg_transport_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase237_cox_ii_higgs_yukawa_texture_dependency_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase238_cox_ii_ready_to_fit_formula_dependency_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase239_cox_iv_gubc_single_parameter_boson_relevance_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase240_cox_iii_axial_contact_rg_boson_parameter_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase241_cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase242_post_p241_external_lead_consolidation_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase243_public_web_source_delta_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase244_electroweak_identifiability_rank_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase245_rank_deficit_minimal_unlock_contract", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase246_minimal_unlock_candidate_inventory", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase247_direct_bridge_repairability_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase248_higgs_scalar_repairability_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase249_invariant_origin_search_for_near_miss_constants", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase250_phase46_electroweak_feature_source_lineage_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase251_upstream_wz_identity_rule_source_chain_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase252_wz_normalization_closure_source_contract_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase253_global_observed_sector_vacuum_scan", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase254_local_completion_revision_boson_source_scan", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase255_observed_field_extraction_no_go_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase256_observed_field_extraction_intake_contract", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase257_observation_pipeline_physical_boson_capability_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase258_recent_electroweak_relation_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase259_recent_target_value_sensitivity_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase260_mass_definition_convention_sensitivity_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase261_electroweak_scheme_radiative_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase262_higgs_top_empirical_relation_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase263_top_yukawa_unity_higgs_closure_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase264_higgs_vacuum_criticality_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase265_gauge_higgs_boundary_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase266_veltman_naturalness_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase267_completion_revision_direct_bridge_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase268_spectral_action_boson_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase269_coleman_weinberg_scale_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase270_composite_higgs_pngb_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase271_asymptotic_safety_higgs_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase272_supersymmetric_higgs_boundary_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase273_boson_fermion_coupling_proxy_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase274_neutrino_option_electroweak_scale_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase275_multiple_point_principle_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase276_top_condensation_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase277_finite_unified_gauge_yukawa_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase278_relaxion_electroweak_scale_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase279_technicolor_walking_electroweak_scale_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase280_direct_bridge_analytic_variation_upgrade_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase281_geometric_refractive_unification_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase282_branch_local_direct_invariant_census", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase283_legacy_electroweak_bridge_source_survivability_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase284_predicted_ratio_alpha_gf_external_closure_diagnostic", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase285_recent_qtp_weak_geometry_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase286_alpha_running_threshold_source_viability_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase287_official_draft_parameter_source_gap_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase288_parameter_source_contract_candidate_scan", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase289_phase288_coverage_false_negative_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase290_charged_lepton_threshold_source_replacement_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase291_koide_charged_lepton_threshold_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase292_electromagnetic_alpha_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase293_fermi_vev_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase294_rg_scheme_transport_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase295_observed_field_extraction_contract_candidate_scan", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase296_source_lineage_contract_field_candidate_scan", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase297_wz_direct_bridge_source_contract_application_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase298_production_analytic_wz_source_row_replay_attempt", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase299_identity_split_production_wz_replay_attempt", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase300_identity_split_common_normalization_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase301_identity_split_production_transition_sweep", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase302_identity_split_particle_normalization_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase303_identity_split_branch_source_normalization_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase304_phase27_sector_aggregate_wz_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase305_phase27_charged_ladder_operator_wz_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase306_decoupled_charged_ladder_wz_row_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase307_target_independent_decoupled_wz_row_selection_law_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase309_source_mode_vector_length_measure_normalization_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase310_completion_variational_branch_to_wz_normalization_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase311_completion_observed_sector_wz_row_selector_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase312_current_public_gu_rvg_revision_delta_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase313_official_draft_electroweak_projection_map_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase314_dimension_casimir_wz_source_law_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase315_ucsd_dark_geometric_energy_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase316_ucsd_transcript_source_strength_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase317_electroweak_mass_matrix_bridge_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase318_deferred_implementation_gap_repairability_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase319_legacy_selector_spectrum_source_law_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase320_standard_electroweak_ladder_normalization_boundary_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase321_neutral_electroweak_mixing_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase322_higgs_upsilon_scalar_source_boundary_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase323_coupled_yang_mills_higgs_mass_extraction_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase324_custodial_rho_parameter_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase325_electroweak_unitarity_scattering_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase326_anomaly_hypercharge_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase327_oblique_precision_electroweak_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase328_superphysics_draft_energy_scale_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase329_seiberg_witten_monopole_electroweak_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase330_weyl_geometric_mass_generation_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase331_theta_omega_inhomogeneous_gauge_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase332_string_m_theory_compactification_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase333_kaluza_klein_internal_symmetry_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase334_su21_superconnection_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase335_graviweak_plebanski_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase336_heft_scalar_geometry_source_law_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase337_octonion_clifford_internal_space_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase338_metric_affine_torsion_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase339_macdowell_mansouri_cartan_breaking_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase340_bf_topological_mass_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase341_scherk_schwarz_twisted_compactification_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase342_higgsless_boundary_condition_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase343_stueckelberg_vector_mass_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase344_fms_gauge_invariant_spectrum_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase345_fradkin_shenker_complementarity_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase346_nielsen_pole_mass_gauge_independence_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase347_dispersive_electroweak_scale_mass_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase348_right_handed_weak_coupling_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase349_spin_exchange_preon_boson_mass_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase350_spin_charge_family_boson_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase351_weak_hypercharge_superselection_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase352_higgs_top_z_nnlo_matching_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("phase353_gauge_higgs_unification_source_audit", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P278.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P279.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P280.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P281.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P282.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P283.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P284.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P324.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P325.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P326.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P327.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P342.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P343.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P344.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P345.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P346.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P347.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P328.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P285.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P286.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P287.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P288.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P289.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P290.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P291.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P292.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P293.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P294.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P295.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P296.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P304.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P305.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P306.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P307.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P308.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P309.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P313.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P314.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P315.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P316.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P317.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P318.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P319.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P320.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P321.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P322.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P323.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P332.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P333.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P334.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P335.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("/output/", StringComparison.Ordinal) && lower.Contains("nextrequiredartifact", StringComparison.Ordinal), "requirement-output-not-source");
    return blockers;
}

static void AddIf(List<string> list, bool condition, string value)
{
    if (condition && !list.Contains(value, StringComparer.Ordinal))
        list.Add(value);
}

sealed record Finding(
    string File,
    int Line,
    string Kind,
    bool IntakeReady,
    IReadOnlyList<string> EvidenceSignals,
    IReadOnlyList<string> Blockers,
    string Excerpt);
