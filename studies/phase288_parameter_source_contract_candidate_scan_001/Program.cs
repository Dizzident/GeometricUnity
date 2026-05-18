using System.Text.Json;

const string DefaultOutputDir = "studies/phase288_parameter_source_contract_candidate_scan_001/output";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE288_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));

var scanRoots = new[] { "src", "TheoryCompletitionRevisions", "docs/Guides", "docs/Architecture", "docs/Phases/OpenIssues" };
var requirements = new[]
{
    new RequirementSpec(
        "gu-alpha-or-charge-source",
        "A GU-derived electromagnetic charge or alpha value at a declared reference scale.",
        new[] { "alpha", "fine structure", "fine-structure", "electromagnetic charge", "charge normalization", "electric charge" },
        new[] { "source", "derivation", "theorem", "target-independent" }),
    new RequirementSpec(
        "gu-charged-threshold-sources",
        "GU-derived charged thresholds sufficient for low-energy alpha transport.",
        new[] { "charged threshold", "charged-lepton threshold", "electron", "muon", "tau", "threshold correction", "threshold-correction" },
        new[] { "source", "derivation", "theorem", "target-independent" }),
    new RequirementSpec(
        "gu-rg-transport-and-scheme",
        "A GU low-energy running operator plus hadronic vacuum-polarization or renormalization-scheme closure.",
        new[] { "rg", "renormalization", "running", "hadronic", "vacuum polarization", "scheme closure" },
        new[] { "transport", "operator", "source", "derivation", "theorem" }),
    new RequirementSpec(
        "gu-vev-source",
        "A target-independent GU vacuum or VEV source replacing the external Phase54 Fermi scale.",
        new[] { "vev", "vacuum expectation", "fermi", "electroweak scale", "vacuum solution" },
        new[] { "source", "derivation", "theorem", "target-independent" }),
    new RequirementSpec(
        "gu-higgs-scalar-extraction",
        "A worked scalar-source/operator, identity envelope, massive profile, and potential/self-coupling extraction.",
        new[] { "scalar source", "scalar-source", "scalarSourceOperatorId", "higgsIdentityEnvelopeId", "massiveScalarProfileId", "potentialOrSelfCouplingSourceId", "self-coupling", "quartic" },
        new[] { "source", "operator", "derivation", "theorem", "profile", "target-independent" }),
};

var scannedFiles = EnumerateScanFiles(scanRoots).ToArray();
var lineHits = ScanFiles(scannedFiles, requirements);
var requirementResults = requirements
    .Select(requirement =>
    {
        var hits = lineHits.Where(hit => hit.RequirementId == requirement.RequirementId).ToArray();
        var strongestHits = hits
            .OrderByDescending(hit => hit.ReadinessScore)
            .ThenBy(hit => hit.Path, StringComparer.Ordinal)
            .ThenBy(hit => hit.LineNumber)
            .Take(8)
            .ToArray();
        var intakeReadyHits = hits
            .Where(hit => hit.IntakeReadyCandidate)
            .ToArray();

        return new RequirementScanResult(
            requirement.RequirementId,
            requirement.Detail,
            CandidateLineCount: hits.Length,
            IntakeReadyCandidateCount: intakeReadyHits.Length,
            Filled: intakeReadyHits.Length > 0,
            StrongestHits: strongestHits);
    })
    .ToArray();

var totalCandidateLineCount = requirementResults.Sum(result => result.CandidateLineCount);
var intakeReadyParameterSourceCandidateCount = requirementResults.Sum(result => result.IntakeReadyCandidateCount);
var anyParameterSourceCandidateFillsContract = requirementResults.Any(result => result.Filled);

var phase204IntakeReadyCandidateCount = JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? -1;
var phase205IntakeReadyFindingCount = JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? -1;
var phase207IntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? -1;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var officialDraftParameterSourceGapAuditPassed = JsonBool(phase287.RootElement, "officialDraftParameterSourceGapAuditPassed") is true;
var officialDraftFillsPhase286Gaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;

var checks = new[]
{
    new Check(
        "non-generated-local-material-scanned",
        scannedFiles.Length > 0,
        $"scannedFileCount={scannedFiles.Length}; scanRoots={string.Join(",", scanRoots)}"),
    new Check(
        "parameter-source-candidate-lines-classified",
        totalCandidateLineCount >= 0 && requirementResults.Length == requirements.Length,
        $"totalCandidateLineCount={totalCandidateLineCount}; requirementCount={requirementResults.Length}"),
    new Check(
        "no-intake-ready-parameter-source-candidate-found",
        intakeReadyParameterSourceCandidateCount == 0 && !anyParameterSourceCandidateFillsContract,
        $"intakeReadyParameterSourceCandidateCount={intakeReadyParameterSourceCandidateCount}; anyParameterSourceCandidateFillsContract={anyParameterSourceCandidateFillsContract}"),
    new Check(
        "generic-source-scans-remain-empty",
        phase204IntakeReadyCandidateCount == 0 && phase205IntakeReadyFindingCount == 0 && phase207IntakeReadyFindingCount == 0,
        $"phase204IntakeReadyCandidateCount={phase204IntakeReadyCandidateCount}; phase205IntakeReadyFindingCount={phase205IntakeReadyFindingCount}; phase207IntakeReadyFindingCount={phase207IntakeReadyFindingCount}"),
    new Check(
        "phase287-gap-audit-remains-unfilled",
        officialDraftParameterSourceGapAuditPassed && !officialDraftFillsPhase286Gaps,
        $"officialDraftParameterSourceGapAuditPassed={officialDraftParameterSourceGapAuditPassed}; officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}"),
    new Check(
        "source-contracts-remain-unfilled",
        !unlockContractFilled && phase245NewSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; phase245NewSourceEvidenceStillRequired={phase245NewSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var parameterSourceContractCandidateScanPassed = checks.All(check => check.Passed)
    && !anyParameterSourceCandidateFillsContract
    && intakeReadyParameterSourceCandidateCount == 0
    && phase245NewSourceEvidenceStillRequired;
var terminalStatus = parameterSourceContractCandidateScanPassed
    ? "parameter-source-contract-candidate-scan-no-local-source-rows"
    : "parameter-source-contract-candidate-scan-review-required";

var result = new
{
    phaseId = "phase288-parameter-source-contract-candidate-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    parameterSourceContractCandidateScanPassed,
    scannedFileCount = scannedFiles.Length,
    totalCandidateLineCount,
    intakeReadyParameterSourceCandidateCount,
    anyParameterSourceCandidateFillsContract,
    requirementResults,
    scanBoundary = new
    {
        scanRoots,
        excludedGeneratedArtifacts = true,
        excludedOutputBinObj = true,
        candidateMentionsAreNotSourceRows = true,
    },
    inheritedBlockerEvidence = new
    {
        phase204IntakeReadyCandidateCount,
        phase205IntakeReadyFindingCount,
        phase207IntakeReadyFindingCount,
        phase245 = new
        {
            unlockContractFilled,
            phase245NewSourceEvidenceStillRequired,
        },
        phase287 = new
        {
            officialDraftParameterSourceGapAuditPassed,
            officialDraftFillsPhase286Gaps,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    checks,
    decision = parameterSourceContractCandidateScanPassed
        ? "Do not promote W/Z/H masses from local parameter-source mentions. The current non-generated local material contains mentions and requirements, but no intake-ready source row for alpha/charge, charged thresholds, RG/scheme transport, VEV, or Higgs scalar extraction."
        : "Review the parameter-source candidate scan before relying on its no-local-source-row classification.",
    nextRequiredArtifact = new[]
    {
        "A concrete GU alpha/charge source row or direct W/Z scale row with sourceLineageId and theoremOrDerivationId.",
        "A concrete GU low-energy transport/scheme source if alpha running remains part of the construction.",
        "A concrete GU VEV/vacuum source row replacing the external Fermi scale.",
        "A concrete GU Higgs scalar-source extraction row with the Phase201 Higgs fields filled.",
    },
    sourceEvidence = new
    {
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase287Path = Phase287Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "parameter_source_contract_candidate_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "parameter_source_contract_candidate_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.parameterSourceContractCandidateScanPassed,
        result.scannedFileCount,
        result.totalCandidateLineCount,
        result.intakeReadyParameterSourceCandidateCount,
        result.anyParameterSourceCandidateFillsContract,
        result.requirementResults,
        result.scanBoundary,
        result.inheritedBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"parameterSourceContractCandidateScanPassed={parameterSourceContractCandidateScanPassed}");
Console.WriteLine($"scannedFileCount={scannedFiles.Length}");
Console.WriteLine($"totalCandidateLineCount={totalCandidateLineCount}");
Console.WriteLine($"intakeReadyParameterSourceCandidateCount={intakeReadyParameterSourceCandidateCount}");
Console.WriteLine($"anyParameterSourceCandidateFillsContract={anyParameterSourceCandidateFillsContract}");

static IEnumerable<string> EnumerateScanFiles(IEnumerable<string> roots)
{
    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs", ".md", ".json", ".txt" };
    foreach (var root in roots)
    {
        if (!Directory.Exists(root))
        {
            continue;
        }

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var normalized = NormalizePath(path);
            if (!extensions.Contains(Path.GetExtension(path))
                || IsExcluded(normalized))
            {
                continue;
            }

            yield return normalized;
        }
    }
}

static IReadOnlyList<LineHit> ScanFiles(IReadOnlyList<string> files, IReadOnlyList<RequirementSpec> requirements)
{
    var hits = new List<LineHit>();
    foreach (var file in files)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(file);
        }
        catch
        {
            continue;
        }

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            foreach (var requirement in requirements)
            {
                if (!ContainsAny(line, requirement.TriggerTerms))
                {
                    continue;
                }

                var supportTermCount = CountTerms(line, requirement.SupportTerms);
                var negativeContext = HasNegativeOrRequirementContext(line);
                var sourceRowLike = supportTermCount > 0 && ContainsAny(line, new[] { "sourceLineageId", "source row", "source-row", "sourceRowId", "theoremOrDerivationId", "derivation-backed" });
                var targetIndependentClaim = ContainsAny(line, new[] { "target-independent", "externalTargetValuesUsed=false", "without target", "not from target" });
                var intakeReadyCandidate = sourceRowLike && targetIndependentClaim && !negativeContext;
                hits.Add(new LineHit(
                    requirement.RequirementId,
                    file,
                    i + 1,
                    line.Length > 260 ? line[..260] : line,
                    supportTermCount + (sourceRowLike ? 2 : 0) + (targetIndependentClaim ? 2 : 0) - (negativeContext ? 2 : 0),
                    sourceRowLike,
                    targetIndependentClaim,
                    negativeContext,
                    intakeReadyCandidate));
            }
        }
    }

    return hits;
}

static bool IsExcluded(string normalizedPath) =>
    normalizedPath.Contains("/bin/", StringComparison.Ordinal)
    || normalizedPath.Contains("/obj/", StringComparison.Ordinal)
    || normalizedPath.Contains("/output/", StringComparison.Ordinal)
    || normalizedPath.Contains("/.git/", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/Phases/Implementation/", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase288_", StringComparison.Ordinal);

static string NormalizePath(string path) => path.Replace('\\', '/');

static bool ContainsAny(string text, IEnumerable<string> terms) =>
    terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

static int CountTerms(string text, IEnumerable<string> terms) =>
    terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

static bool HasNegativeOrRequirementContext(string text) =>
    ContainsAny(text, new[]
    {
        "missing",
        "not ",
        "no ",
        "blocked",
        "required",
        "requires",
        "require ",
        "external",
        "loophole",
        "not promotable",
        "non-promotable",
        "diagnostic",
        "candidate",
    });

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
    {
        return null;
    }

    return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}

sealed record RequirementSpec(string RequirementId, string Detail, IReadOnlyList<string> TriggerTerms, IReadOnlyList<string> SupportTerms);

sealed record RequirementScanResult(
    string RequirementId,
    string Detail,
    int CandidateLineCount,
    int IntakeReadyCandidateCount,
    bool Filled,
    IReadOnlyList<LineHit> StrongestHits);

sealed record LineHit(
    string RequirementId,
    string Path,
    int LineNumber,
    string Excerpt,
    int ReadinessScore,
    bool SourceRowLike,
    bool TargetIndependentClaim,
    bool NegativeContext,
    bool IntakeReadyCandidate);

sealed record Check(string CheckId, bool Passed, string Detail);
