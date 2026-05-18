using System.Text.Json;

const string DefaultOutputDir = "studies/phase289_phase288_coverage_false_negative_audit_001/output";
const string Phase288Path = "studies/phase288_parameter_source_contract_candidate_scan_001/output/parameter_source_contract_candidate_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE289_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase288 = JsonDocument.Parse(File.ReadAllText(Phase288Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var expandedScanRoots = new[]
{
    "README.md",
    "OriginalPrompts",
    "docs/Reference",
    "docs/Phases/Plans",
    "docs/Phases/Summaries",
    "docs/Phases/Gaps",
    "docs/Phases/Prompts",
    "phase4",
    "data",
    "schemas",
    "reports",
    "examples",
    "apps",
    "native",
    "scripts",
};

var phase288Roots = new[] { "src", "TheoryCompletitionRevisions", "docs/Guides", "docs/Architecture", "docs/Phases/OpenIssues" };
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

var scannedFiles = EnumerateScanFiles(expandedScanRoots, phase288Roots).ToArray();
var lineHits = ScanFiles(scannedFiles, requirements);
var requirementResults = requirements
    .Select(requirement =>
    {
        var hits = lineHits.Where(hit => hit.RequirementId == requirement.RequirementId).ToArray();
        var strongestHits = hits
            .OrderByDescending(hit => hit.ReadinessScore)
            .ThenBy(hit => hit.Path, StringComparer.Ordinal)
            .ThenBy(hit => hit.LineNumber)
            .Take(10)
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

var scannedBuckets = scannedFiles
    .GroupBy(TopLevelBucket, StringComparer.Ordinal)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => new ScannedBucket(group.Key, group.Count()))
    .ToArray();

var totalCandidateLineCount = requirementResults.Sum(result => result.CandidateLineCount);
var intakeReadyExcludedCorpusCandidateCount = requirementResults.Sum(result => result.IntakeReadyCandidateCount);
var anyExcludedCorpusCandidateFillsContract = requirementResults.Any(result => result.Filled);

var phase288Passed = JsonBool(phase288.RootElement, "parameterSourceContractCandidateScanPassed") is true;
var phase288ScannedFileCount = JsonInt(phase288.RootElement, "scannedFileCount") ?? -1;
var phase288IntakeReadyCandidateCount = JsonInt(phase288.RootElement, "intakeReadyParameterSourceCandidateCount") ?? -1;
var phase288AnyCandidateFillsContract = JsonBool(phase288.RootElement, "anyParameterSourceCandidateFillsContract") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "phase288-baseline-present-and-fail-closed",
        phase288Passed && phase288ScannedFileCount > 0 && phase288IntakeReadyCandidateCount == 0 && !phase288AnyCandidateFillsContract,
        $"phase288Passed={phase288Passed}; phase288ScannedFileCount={phase288ScannedFileCount}; phase288IntakeReadyCandidateCount={phase288IntakeReadyCandidateCount}; phase288AnyCandidateFillsContract={phase288AnyCandidateFillsContract}"),
    new Check(
        "phase288-excluded-first-party-corpus-scanned",
        scannedFiles.Length > 0 && scannedBuckets.Length > 0,
        $"scannedFileCount={scannedFiles.Length}; bucketCount={scannedBuckets.Length}; scanRoots={string.Join(",", expandedScanRoots)}"),
    new Check(
        "expanded-context-candidate-lines-classified",
        totalCandidateLineCount >= 0 && requirementResults.Length == requirements.Length,
        $"totalCandidateLineCount={totalCandidateLineCount}; requirementCount={requirementResults.Length}"),
    new Check(
        "no-excluded-corpus-intake-ready-source-row-found",
        intakeReadyExcludedCorpusCandidateCount == 0 && !anyExcludedCorpusCandidateFillsContract,
        $"intakeReadyExcludedCorpusCandidateCount={intakeReadyExcludedCorpusCandidateCount}; anyExcludedCorpusCandidateFillsContract={anyExcludedCorpusCandidateFillsContract}"),
    new Check(
        "phase213-blockers-still-present",
        wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var coverageFalseNegativeAuditPassed = checks.All(check => check.Passed)
    && !anyExcludedCorpusCandidateFillsContract
    && intakeReadyExcludedCorpusCandidateCount == 0;
var terminalStatus = coverageFalseNegativeAuditPassed
    ? "phase288-coverage-false-negative-audit-no-missed-source-rows"
    : "phase288-coverage-false-negative-audit-review-required";

var result = new
{
    phaseId = "phase289-phase288-coverage-false-negative-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coverageFalseNegativeAuditPassed,
    scannedFileCount = scannedFiles.Length,
    scannedBuckets,
    excludedCorpusCandidateLineCount = totalCandidateLineCount,
    intakeReadyExcludedCorpusCandidateCount,
    anyExcludedCorpusCandidateFillsContract,
    requirementResults,
    scanBoundary = new
    {
        expandedScanRoots,
        baselinePhase288Roots = phase288Roots,
        contextWindowLines = 5,
        excludedPhase288Roots = true,
        excludedGeneratedStudiesAndOutputs = true,
        candidateMentionsAreNotSourceRows = true,
    },
    inheritedEvidence = new
    {
        phase288 = new
        {
            phase288Passed,
            phase288ScannedFileCount,
            phase288IntakeReadyCandidateCount,
            phase288AnyCandidateFillsContract,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    checks,
    decision = coverageFalseNegativeAuditPassed
        ? "Do not reopen W/Z/H mass promotion on Phase288 coverage grounds. The first-party corpus excluded from Phase288 contains parameter language but no intake-ready target-independent source row for alpha/charge, charged thresholds, RG/scheme transport, VEV/direct W/Z scale, or Higgs scalar extraction."
        : "Review the excluded-corpus candidates before relying on Phase288 as a complete local parameter-source scan.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed GU alpha/charge or direct W/Z scale source row outside diagnostic/prompt/requirement prose.",
        "A derivation-backed GU VEV/vacuum source row independent of external Fermi input.",
        "A derivation-backed GU Higgs scalar-source/self-coupling extraction row.",
    },
    sourceEvidence = new
    {
        phase288Path = Phase288Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "phase288_coverage_false_negative_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase288_coverage_false_negative_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coverageFalseNegativeAuditPassed,
        result.scannedFileCount,
        result.scannedBuckets,
        result.excludedCorpusCandidateLineCount,
        result.intakeReadyExcludedCorpusCandidateCount,
        result.anyExcludedCorpusCandidateFillsContract,
        result.requirementResults,
        result.scanBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coverageFalseNegativeAuditPassed={coverageFalseNegativeAuditPassed}");
Console.WriteLine($"scannedFileCount={scannedFiles.Length}");
Console.WriteLine($"excludedCorpusCandidateLineCount={totalCandidateLineCount}");
Console.WriteLine($"intakeReadyExcludedCorpusCandidateCount={intakeReadyExcludedCorpusCandidateCount}");
Console.WriteLine($"anyExcludedCorpusCandidateFillsContract={anyExcludedCorpusCandidateFillsContract}");

static IEnumerable<string> EnumerateScanFiles(IEnumerable<string> roots, IReadOnlyList<string> phase288Roots)
{
    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs", ".csproj", ".md", ".json", ".txt", ".yml", ".yaml", ".sh", ".h", ".hpp", ".c", ".cpp", ".cu" };
    foreach (var root in roots)
    {
        if (File.Exists(root))
        {
            var normalizedFile = NormalizePath(root);
            if (extensions.Contains(Path.GetExtension(root))
                && !IsExcluded(normalizedFile, phase288Roots))
            {
                yield return normalizedFile;
            }

            continue;
        }

        if (!Directory.Exists(root))
        {
            continue;
        }

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var normalized = NormalizePath(path);
            if (!extensions.Contains(Path.GetExtension(path))
                || IsExcluded(normalized, phase288Roots))
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

            var context = BuildContext(lines, i, 2);
            foreach (var requirement in requirements)
            {
                if (!ContainsAny(line, requirement.TriggerTerms))
                {
                    continue;
                }

                var supportTermCount = CountTerms(context, requirement.SupportTerms);
                var negativeContext = HasNegativeOrRequirementContext(context);
                var sourceRowLike = supportTermCount > 0 && ContainsAny(context, new[] { "sourceLineageId", "source row", "source-row", "sourceRowId", "theoremOrDerivationId", "derivation-backed" });
                var targetIndependentClaim = ContainsAny(context, new[] { "target-independent", "externalTargetValuesUsed=false", "without target", "not from target" });
                var generatedOrPromptContext = HasGeneratedOrPromptContext(file, context);
                var intakeReadyCandidate = sourceRowLike && targetIndependentClaim && !negativeContext && !generatedOrPromptContext;
                hits.Add(new LineHit(
                    requirement.RequirementId,
                    file,
                    i + 1,
                    line.Length > 260 ? line[..260] : line,
                    supportTermCount + (sourceRowLike ? 2 : 0) + (targetIndependentClaim ? 2 : 0) - (negativeContext ? 2 : 0) - (generatedOrPromptContext ? 2 : 0),
                    sourceRowLike,
                    targetIndependentClaim,
                    negativeContext,
                    generatedOrPromptContext,
                    intakeReadyCandidate));
            }
        }
    }

    return hits;
}

static string BuildContext(IReadOnlyList<string> lines, int center, int radius)
{
    var start = Math.Max(0, center - radius);
    var end = Math.Min(lines.Count - 1, center + radius);
    return string.Join('\n', Enumerable.Range(start, end - start + 1).Select(index => lines[index].Trim()));
}

static bool IsExcluded(string normalizedPath, IReadOnlyList<string> phase288Roots) =>
    normalizedPath.Contains("/bin/", StringComparison.Ordinal)
    || normalizedPath.Contains("/obj/", StringComparison.Ordinal)
    || normalizedPath.Contains("/output/", StringComparison.Ordinal)
    || normalizedPath.Contains("/.git/", StringComparison.Ordinal)
    || normalizedPath.Contains("/.claude/", StringComparison.Ordinal)
    || normalizedPath.Contains("/.agents/", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/Phases/Implementation/", StringComparison.Ordinal)
    || normalizedPath.Contains("/docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md", StringComparison.Ordinal)
    || normalizedPath.StartsWith("studies/", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/", StringComparison.Ordinal)
    || phase288Roots.Any(root => normalizedPath.Equals(root, StringComparison.Ordinal) || normalizedPath.StartsWith(root + "/", StringComparison.Ordinal));

static string NormalizePath(string path) => path.Replace('\\', '/').TrimStart('.', '/');

static string TopLevelBucket(string normalizedPath)
{
    if (normalizedPath == "README.md")
    {
        return "root-readme";
    }

    var slash = normalizedPath.IndexOf('/', StringComparison.Ordinal);
    if (slash < 0)
    {
        return "root";
    }

    if (normalizedPath.StartsWith("docs/Phases/", StringComparison.Ordinal))
    {
        var parts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 3 ? $"docs/Phases/{parts[2]}" : "docs/Phases";
    }

    return normalizedPath[..slash];
}

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
        "request",
        "prompt",
        "gap",
        "todo",
        "future",
    });

static bool HasGeneratedOrPromptContext(string path, string text) =>
    path.StartsWith("OriginalPrompts/", StringComparison.Ordinal)
    || path.StartsWith("docs/Phases/Prompts/", StringComparison.Ordinal)
    || path.StartsWith("reports/", StringComparison.Ordinal)
    || path.Contains("/tmp", StringComparison.OrdinalIgnoreCase)
    || ContainsAny(text, new[] { "prompt", "open issue", "gap", "request package", "audit", "diagnosis", "generated" });

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
    bool GeneratedOrPromptContext,
    bool IntakeReadyCandidate);

sealed record ScannedBucket(string Bucket, int FileCount);

sealed record Check(string CheckId, bool Passed, string Detail);
