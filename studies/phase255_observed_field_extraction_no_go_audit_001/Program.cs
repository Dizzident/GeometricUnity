using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase255_observed_field_extraction_no_go_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase230Path = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output/native_gu_vacuum_hessian_candidate_audit_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase253Path = "studies/phase253_global_observed_sector_vacuum_scan_001/output/global_observed_sector_vacuum_scan_summary.json";
const string Phase254Path = "studies/phase254_local_completion_revision_boson_source_scan_001/output/local_completion_revision_boson_source_scan_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE255_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase230 = JsonDocument.Parse(File.ReadAllText(Phase230Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase247 = JsonDocument.Parse(File.ReadAllText(Phase247Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase253 = JsonDocument.Parse(File.ReadAllText(Phase253Path));
using var phase254 = JsonDocument.Parse(File.ReadAllText(Phase254Path));

var scanRoots = new[]
{
    "README.md",
    "docs/Phases/OpenIssues",
    "docs/Phases/Gaps",
    "docs/Phases/Summaries",
    "docs/Phases/Plans",
    "reports/post_phase11_evidence_campaign",
    "TheoryCompletitionRevisions",
};
var searchableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".md", ".json", ".txt" };
var excludedPathFragments = new[]
{
    "/bin/",
    "/obj/",
    "/output/",
    "docs/Phases/Implementation/",
    "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md",
    "studies/phase255_observed_field_extraction_no_go_audit_001/",
};

var observedExtractionPattern = new Regex(
    """observed[-\s]?field|observed[-\s]?boson|native[-\s]?to[-\s]?observed|observable extraction|extraction theorem|phenomenological mapping|observerse|observation mechanism|recovery""",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var shiabBranchPattern = new Regex(
    """Shiab|Upsilon|augmented torsion|swerve|swervature|branch[-\s]?local|operator[-\s]?choice|canonical""",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var massOperatorPattern = new Regex(
    """mass[-\s]?matrix|quadratic expansion|Hessian|VEV|vacuum|electroweak|Higgs potential|quartic|gauge[-\s]?boson mass""",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var unresolvedPattern = new Regex(
    """open|blocked|conjectural|requires|not yet|missing|unresolved|speculative|branch[-\s]?sensitive|non[-\s]?canonical|nonunique|underdetermined|not completed|does not prove|future work""",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var promotionPattern = new Regex(
    """sourceLineage|sourceRowId|theoremOrDerivationId|rawAmplitudeGatePassed|scalarSourceOperatorId|massiveScalarProfileId|targetComparisonGatePassed""",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

var findings = new List<ExtractionFinding>();
foreach (var file in EnumerateCandidateFiles(scanRoots, searchableExtensions, excludedPathFragments))
{
    var lines = SafeReadLines(file);
    for (var index = 0; index < lines.Length; index++)
    {
        var line = lines[index].Trim();
        if (line.Length == 0)
        {
            continue;
        }

        var hasObservedExtraction = observedExtractionPattern.IsMatch(line);
        var hasShiabBranch = shiabBranchPattern.IsMatch(line);
        var hasMassOperator = massOperatorPattern.IsMatch(line);
        var hasUnresolvedLanguage = unresolvedPattern.IsMatch(line);
        var hasPromotionToken = promotionPattern.IsMatch(line);

        if (!hasObservedExtraction && !hasShiabBranch && !hasMassOperator && !hasPromotionToken)
        {
            continue;
        }

        findings.Add(new ExtractionFinding(
            NormalizePath(file),
            index + 1,
            hasObservedExtraction,
            hasShiabBranch,
            hasMassOperator,
            hasUnresolvedLanguage,
            hasPromotionToken,
            Classify(hasObservedExtraction, hasShiabBranch, hasMassOperator, hasUnresolvedLanguage, hasPromotionToken),
            Truncate(line, 360)));
    }
}

var observedExtractionSignalCount = findings.Count(row => row.HasObservedExtraction);
var shiabBranchSignalCount = findings.Count(row => row.HasShiabBranch);
var massOperatorSignalCount = findings.Count(row => row.HasMassOperator);
var unresolvedExtractionSignalCount = findings.Count(row => row.HasUnresolvedLanguage);
var promotionContractSignalCount = findings.Count(row => row.HasPromotionContractToken);
var promotableExtractionContractCandidateCount = findings.Count(row => row.Classification == "possible-promotion-contract-token");
var strongestUnresolvedSamples = findings
    .Where(row => row.Classification is "observed-extraction-unresolved" or "shiab-branch-unresolved" or "mass-operator-unresolved")
    .OrderBy(row => row.Path, StringComparer.Ordinal)
    .ThenBy(row => row.LineNumber)
    .Take(80)
    .ToArray();

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var officialGuShiabUpsilonExtractionPromotable = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true;
var officialGuShiabUpsilonExtractionObstructionCertified = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionObstructionCertified") is true;
var bosonMassMatrixExtractionPromotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var bosonMassMatrixExtractionObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var electroweakVevSourceLineageObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var nativeGuVacuumHessianCandidatePromotable = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidatePromotable") is true;
var nativeGuVacuumHessianCandidateAuditPassed = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidateAuditPassed") is true;
var remainingNullity = JsonInt(phase244.RootElement, "remainingNullity") ?? -1;
var minimumAdditionalIndependentSourceConstraints = JsonInt(phase245.RootElement, "minimumAdditionalIndependentSourceConstraints") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var directBridgeRepairabilityAuditPassed = JsonBool(phase247.RootElement, "directBridgeRepairabilityAuditPassed") is true;
var currentDirectBridgeCandidatePromotable = JsonBool(phase247.RootElement, "currentDirectBridgeCandidatePromotable") is true;
var wzParticleSplitDerivableFromCurrentRegistry = JsonBool(phase247.RootElement, "wzParticleSplitDerivableFromCurrentRegistry") is true;
var higgsScalarRepairabilityAuditPassed = JsonBool(phase248.RootElement, "higgsScalarRepairabilityAuditPassed") is true;
var higgsScalarSourceRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var globalObservedSectorVacuumCandidateFound = JsonBool(phase253.RootElement, "globalObservedSectorVacuumCandidateFound") is true;
var completionRevisionsFillSourceContracts = JsonBool(phase254.RootElement, "completionRevisionsFillSourceContracts") is true;

var electroweakClosure = phase224.RootElement.TryGetProperty("closure", out var p224Closure)
    ? p224Closure.Clone()
    : default;
var wParameterClosure = electroweakClosure.ValueKind == JsonValueKind.Object
    && JsonBool(electroweakClosure, "wAbsoluteMassParameterClosure") is true;
var zParameterClosure = electroweakClosure.ValueKind == JsonValueKind.Object
    && JsonBool(electroweakClosure, "zAbsoluteMassParameterClosure") is true;
var higgsParameterClosure = electroweakClosure.ValueKind == JsonValueKind.Object
    && JsonBool(electroweakClosure, "higgsMassParameterClosure") is true;

var extractionTheoremMissing = officialGuShiabUpsilonExtractionObstructionCertified
    && !officialGuShiabUpsilonExtractionPromotable
    && nativeGuVacuumHessianCandidateAuditPassed
    && !nativeGuVacuumHessianCandidatePromotable
    && !globalObservedSectorVacuumCandidateFound;
var massOperatorMissing = bosonMassMatrixExtractionObstructionCertified
    && !bosonMassMatrixExtractionPromotable
    && electroweakVevSourceLineageObstructionCertified
    && !targetIndependentGuVevSourcePromotable;
var particleProjectionMissing = directBridgeRepairabilityAuditPassed
    && !currentDirectBridgeCandidatePromotable
    && !wzParticleSplitDerivableFromCurrentRegistry
    && higgsScalarRepairabilityAuditPassed
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry;
var electroweakMathStillUnderdetermined = electroweakParameterAuditPassed
    && !wParameterClosure
    && !zParameterClosure
    && !higgsParameterClosure
    && remainingNullity == 2
    && minimumAdditionalIndependentSourceConstraints == 2
    && !unlockContractFilled;
var localTextShowsUnresolvedExtractionRatherThanContracts = observedExtractionSignalCount > 0
    && shiabBranchSignalCount > 0
    && massOperatorSignalCount > 0
    && unresolvedExtractionSignalCount > 0
    && promotionContractSignalCount == 0
    && promotableExtractionContractCandidateCount == 0
    && !completionRevisionsFillSourceContracts;

var observedFieldExtractionNoGoPassed = extractionTheoremMissing
    && massOperatorMissing
    && particleProjectionMissing
    && electroweakMathStillUnderdetermined
    && localTextShowsUnresolvedExtractionRatherThanContracts
    && wzMissingFieldCount == 15
    && higgsMissingFieldCount == 14;
var observedFieldExtractionBridgePromotable = false;
var newObservedFieldExtractionArtifactRequired = !observedFieldExtractionBridgePromotable;

var checks = new[]
{
    new Check(
        "official-gu-extraction-theorem-still-missing",
        extractionTheoremMissing,
        $"officialGuShiabUpsilonExtractionPromotable={officialGuShiabUpsilonExtractionPromotable}; nativeGuVacuumHessianCandidatePromotable={nativeGuVacuumHessianCandidatePromotable}; globalObservedSectorVacuumCandidateFound={globalObservedSectorVacuumCandidateFound}"),
    new Check(
        "physical-electroweak-mass-operator-still-missing",
        massOperatorMissing,
        $"bosonMassMatrixExtractionPromotable={bosonMassMatrixExtractionPromotable}; targetIndependentGuVevSourcePromotable={targetIndependentGuVevSourcePromotable}"),
    new Check(
        "particle-projection-and-scalar-source-still-missing",
        particleProjectionMissing,
        $"currentDirectBridgeCandidatePromotable={currentDirectBridgeCandidatePromotable}; wzParticleSplitDerivableFromCurrentRegistry={wzParticleSplitDerivableFromCurrentRegistry}; higgsScalarSourceRepairPossibleFromCurrentRegistry={higgsScalarSourceRepairPossibleFromCurrentRegistry}"),
    new Check(
        "standard-electroweak-parameter-closure-still-underdetermined",
        electroweakMathStillUnderdetermined,
        $"wParameterClosure={wParameterClosure}; zParameterClosure={zParameterClosure}; higgsParameterClosure={higgsParameterClosure}; remainingNullity={remainingNullity}; minimumAdditionalIndependentSourceConstraints={minimumAdditionalIndependentSourceConstraints}; unlockContractFilled={unlockContractFilled}"),
    new Check(
        "local-sources-show-unresolved-extraction-not-contracts",
        localTextShowsUnresolvedExtractionRatherThanContracts,
        $"observedExtractionSignalCount={observedExtractionSignalCount}; shiabBranchSignalCount={shiabBranchSignalCount}; massOperatorSignalCount={massOperatorSignalCount}; unresolvedExtractionSignalCount={unresolvedExtractionSignalCount}; promotionContractSignalCount={promotionContractSignalCount}; promotableExtractionContractCandidateCount={promotableExtractionContractCandidateCount}"),
    new Check(
        "phase213-source-blockers-preserved",
        wzMissingFieldCount == 15 && higgsMissingFieldCount == 14,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = observedFieldExtractionNoGoPassed
    ? "observed-field-extraction-no-go-audit-new-artifact-required"
    : "observed-field-extraction-no-go-audit-review-required";

var result = new
{
    phaseId = "phase255-observed-field-extraction-no-go-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    observedFieldExtractionNoGoPassed,
    observedFieldExtractionBridgePromotable,
    newObservedFieldExtractionArtifactRequired,
    scanScope = new
    {
        scanRoots,
        searchableExtensions = searchableExtensions.Order(StringComparer.Ordinal).ToArray(),
        excludedPathFragments,
    },
    signalCounts = new
    {
        observedExtractionSignalCount,
        shiabBranchSignalCount,
        massOperatorSignalCount,
        unresolvedExtractionSignalCount,
        promotionContractSignalCount,
        promotableExtractionContractCandidateCount,
    },
    strongestUnresolvedSamples,
    researchBasis = new[]
    {
        new ResearchBasis(
            "pdg-2025-electroweak-model",
            "PDG 2025 Electroweak Model review",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
            "The Standard Model obtains W, Z, Higgs, and photon masses only after specifying the scalar vacuum, gauge couplings, weak mixing, and Higgs-potential parameters. This supplies the comparison-side dependency template, not GU source evidence."),
        new ResearchBasis(
            "pdg-2025-higgs-status",
            "PDG 2025 Higgs review",
            "https://pdgweb.lbl.gov/2025/reviews/rpp2025-rev-higgs-boson.pdf",
            "The electroweak VEV sets the symmetry-breaking scale, Goldstone modes are absorbed into W/Z, and the residual CP-even scalar is the Higgs. A GU prediction needs an analogous source-derived vacuum and physical eigenstate projection."),
        new ResearchBasis(
            "official-gu-2013-oxford-lecture",
            "Official Geometric Unity Oxford lecture transcript",
            "https://geometricunity.org/2013-oxford-lecture/",
            "The public GU material presents Shiab/Upsilon machinery and a generalized Yang-Mills/Higgs route, but leaves freedom in the operator and does not provide particle-specific W/Z/H mass rows."),
        new ResearchBasis(
            "public-gu-draft-appendix-location-table",
            "Geometric Unity Author's Working Draft, Appendix location table",
            "https://saismaran.org/geometricunity.pdf",
            "The public draft places Higgs potential at an inner product of Upsilon terms and lists field locations, but this is not an extraction theorem with a fixed physical mass matrix or W/Z/H source rows."),
    },
    noGoReason = "Generic GU field-location or Upsilon/Shiab action language is not enough to predict physical W/Z/H masses. The missing bridge is an observed-field extraction theorem: a canonical branch/operator choice, source-derived vacuum, quadratic physical mass operator, W/Z/photon/Higgs eigenstate projection, normalization/scale source, and Higgs scalar-source relation.",
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase224 = new
        {
            electroweakParameterAuditPassed,
            wParameterClosure,
            zParameterClosure,
            higgsParameterClosure,
        },
        phase227 = new
        {
            officialGuShiabUpsilonExtractionObstructionCertified,
            officialGuShiabUpsilonExtractionPromotable,
        },
        phase228 = new
        {
            bosonMassMatrixExtractionObstructionCertified,
            bosonMassMatrixExtractionPromotable,
        },
        phase229 = new
        {
            electroweakVevSourceLineageObstructionCertified,
            targetIndependentGuVevSourcePromotable,
        },
        phase230 = new
        {
            nativeGuVacuumHessianCandidateAuditPassed,
            nativeGuVacuumHessianCandidatePromotable,
        },
        phase244 = new
        {
            remainingNullity,
        },
        phase245 = new
        {
            minimumAdditionalIndependentSourceConstraints,
            unlockContractFilled,
        },
        phase247 = new
        {
            directBridgeRepairabilityAuditPassed,
            currentDirectBridgeCandidatePromotable,
            wzParticleSplitDerivableFromCurrentRegistry,
        },
        phase248 = new
        {
            higgsScalarRepairabilityAuditPassed,
            higgsScalarSourceRepairPossibleFromCurrentRegistry,
        },
        phase253 = new
        {
            globalObservedSectorVacuumCandidateFound,
        },
        phase254 = new
        {
            completionRevisionsFillSourceContracts,
        },
    },
    checks,
    decision = observedFieldExtractionNoGoPassed
        ? "Do not promote W/Z/H physical masses from generic GU Upsilon/Shiab/Higgs-location language. The current repo and local completion corpus contain unresolved observed-field extraction, branch/operator, and physical mass-operator obligations, not a source-lineage bridge that can fill W/Z or Higgs prediction rows."
        : "Review observed-field extraction audit before relying on it as a no-go boundary.",
    nextRequiredArtifact = new[]
    {
        "A theorem or source artifact with observedFieldExtractionTheoremId mapping GU variables to physical observed electroweak fields.",
        "A fixed canonical or branch-declared Shiab/Upsilon operator with normalization and stability sidecars.",
        "A source-derived four-dimensional observed-sector vacuum and quadratic electroweak mass operator.",
        "Particle eigenstate projection rows for photon, W, Z, and Higgs, including W/Z raw gates and Higgs scalar-source/self-coupling relation.",
        "A target-independent normalization/scale source that fills the Phase201/209/210/213 contracts before target comparison.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase227Path = Phase227Path,
        phase228Path = Phase228Path,
        phase229Path = Phase229Path,
        phase230Path = Phase230Path,
        phase244Path = Phase244Path,
        phase245Path = Phase245Path,
        phase247Path = Phase247Path,
        phase248Path = Phase248Path,
        phase253Path = Phase253Path,
        phase254Path = Phase254Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "observed_field_extraction_no_go_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "observed_field_extraction_no_go_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.observedFieldExtractionNoGoPassed,
        result.observedFieldExtractionBridgePromotable,
        result.newObservedFieldExtractionArtifactRequired,
        result.signalCounts,
        result.noGoReason,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"observedFieldExtractionNoGoPassed={observedFieldExtractionNoGoPassed}");
Console.WriteLine($"observedFieldExtractionBridgePromotable={observedFieldExtractionBridgePromotable}");
Console.WriteLine($"newObservedFieldExtractionArtifactRequired={newObservedFieldExtractionArtifactRequired}");

static IEnumerable<string> EnumerateCandidateFiles(IEnumerable<string> roots, ISet<string> extensions, IEnumerable<string> excludedPathFragments)
{
    foreach (var root in roots)
    {
        if (File.Exists(root))
        {
            if (ShouldInclude(root, extensions, excludedPathFragments))
            {
                yield return root;
            }

            continue;
        }

        if (!Directory.Exists(root))
        {
            continue;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (ShouldInclude(file, extensions, excludedPathFragments))
            {
                yield return file;
            }
        }
    }
}

static bool ShouldInclude(string path, ISet<string> extensions, IEnumerable<string> excludedPathFragments)
{
    var normalizedPath = NormalizePath(path);
    return extensions.Contains(Path.GetExtension(path))
        && !excludedPathFragments.Any(fragment => normalizedPath.Contains(fragment, StringComparison.Ordinal));
}

static string[] SafeReadLines(string path)
{
    try
    {
        return File.ReadAllLines(path);
    }
    catch
    {
        return [];
    }
}

static string Classify(bool hasObservedExtraction, bool hasShiabBranch, bool hasMassOperator, bool hasUnresolvedLanguage, bool hasPromotionContractToken)
{
    if (hasPromotionContractToken)
    {
        return "possible-promotion-contract-token";
    }

    if (hasObservedExtraction && hasUnresolvedLanguage)
    {
        return "observed-extraction-unresolved";
    }

    if (hasShiabBranch && hasUnresolvedLanguage)
    {
        return "shiab-branch-unresolved";
    }

    if (hasMassOperator && hasUnresolvedLanguage)
    {
        return "mass-operator-unresolved";
    }

    if (hasObservedExtraction || hasShiabBranch || hasMassOperator)
    {
        return "context-signal";
    }

    return "other";
}

static string NormalizePath(string path) => path.Replace(Path.DirectorySeparatorChar, '/');

static string Truncate(string value, int maxLength) =>
    value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength - 3), "...");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string CheckId, bool Passed, string Detail);

sealed record ResearchBasis(
    string SourceId,
    string Title,
    string Url,
    string Finding);

sealed record ExtractionFinding(
    string Path,
    int LineNumber,
    bool HasObservedExtraction,
    bool HasShiabBranch,
    bool HasMassOperator,
    bool HasUnresolvedLanguage,
    bool HasPromotionContractToken,
    string Classification,
    string Text);
