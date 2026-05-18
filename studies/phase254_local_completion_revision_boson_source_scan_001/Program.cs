using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase254_local_completion_revision_boson_source_scan_001/output";
const string RevisionDir = "TheoryCompletitionRevisions";
const string Phase194Path = "studies/phase194_draft_boson_source_evidence_audit_001/output/draft_boson_source_evidence_audit_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase253Path = "studies/phase253_global_observed_sector_vacuum_scan_001/output/global_observed_sector_vacuum_scan_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE254_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase194 = JsonDocument.Parse(File.ReadAllText(Phase194Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase253 = JsonDocument.Parse(File.ReadAllText(Phase253Path));

var revisionFiles = Directory.Exists(RevisionDir)
    ? Directory.EnumerateFiles(RevisionDir, "*.md", SearchOption.TopDirectoryOnly)
        .Order(StringComparer.Ordinal)
        .ToArray()
    : Array.Empty<string>();

var bosonSignalPattern = new Regex("""\b(W\s*(boson|mass)|Z\s*(boson|mass)|Higgs|Yukawa|quartic|VEV|vacuum|mass[-\s]?matrix|boson mass|observed[-\s]?field|prediction registry|m_W|m_Z|m_H|MW|MZ|MH)\b""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var sourceContractPattern = new Regex("""sourceLineage|sourceRowId|theoremOrDerivationId|rawAmplitudeGatePassed|scalarSourceOperatorId|higgsIdentityEnvelopeId|massiveScalarProfileId|commonBridgeGatePassed|targetComparisonGatePassed""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var physicalNumberPattern = new Regex("""\b(80\.[0-9]+|91\.[0-9]+|125\.[0-9]+|246\.[0-9]+)\b|\bGeV\b""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var sourceDerivationPattern = new Regex("""deriv(ed|es|ation)|theorem|source|operator|lineage|proof|formal source|observable map|typed prediction""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var blockerPattern = new Regex("""open|blocked|conjectural|approximate|future work|requires|not yet|does not|missing|incomplete|speculative|phenomenological mapping|target|protocol|no observational comparison|not completed|fails|must""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var exactWzFormulaPattern = new Regex("""\b(m_W|MW)\s*=|\b(m_Z|MZ)\s*=|W/Z absolute|W/Z mass""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
var exactHiggsFormulaPattern = new Regex("""\b(m_H|MH)\s*=|Higgs mass|quartic|lambda""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

var allRows = new List<LineFinding>();
var totalLineCount = 0;

foreach (var file in revisionFiles)
{
    var lines = File.ReadAllLines(file);
    totalLineCount += lines.Length;

    for (var index = 0; index < lines.Length; index++)
    {
        var line = lines[index].Trim();
        if (line.Length == 0)
        {
            continue;
        }

        var hasBosonSignal = bosonSignalPattern.IsMatch(line);
        var hasSourceContractToken = sourceContractPattern.IsMatch(line);
        var hasPhysicalNumber = physicalNumberPattern.IsMatch(line);
        var hasSourceDerivation = sourceDerivationPattern.IsMatch(line);
        var hasBlockerLanguage = blockerPattern.IsMatch(line);
        var hasWzFormulaSignal = exactWzFormulaPattern.IsMatch(line);
        var hasHiggsFormulaSignal = exactHiggsFormulaPattern.IsMatch(line);

        if (!hasBosonSignal && !hasSourceContractToken && !hasPhysicalNumber)
        {
            continue;
        }

        allRows.Add(new LineFinding(
            NormalizePath(file),
            index + 1,
            hasBosonSignal,
            hasSourceContractToken,
            hasPhysicalNumber,
            hasSourceDerivation,
            hasBlockerLanguage,
            hasWzFormulaSignal,
            hasHiggsFormulaSignal,
            ClassifyLine(hasBosonSignal, hasSourceContractToken, hasPhysicalNumber, hasSourceDerivation, hasBlockerLanguage, hasWzFormulaSignal, hasHiggsFormulaSignal),
            Truncate(line, 360)));
    }
}

var sourceContractTokenLineCount = allRows.Count(row => row.HasSourceContractToken);
var bosonSignalLineCount = allRows.Count(row => row.HasBosonSignal);
var physicalNumberLineCount = allRows.Count(row => row.HasPhysicalNumber);
var wzFormulaSignalLineCount = allRows.Count(row => row.HasWzFormulaSignal);
var higgsFormulaSignalLineCount = allRows.Count(row => row.HasHiggsFormulaSignal);
var blockerLineCount = allRows.Count(row => row.HasBlockerLanguage);
var possibleSourceRows = allRows
    .Where(row => row.Classification == "possible-source-contract-line")
    .OrderBy(row => row.Path, StringComparer.Ordinal)
    .ThenBy(row => row.LineNumber)
    .ToArray();
var blockedRows = allRows
    .Where(row => row.Classification is "blocked-or-protocol-line" or "formula-or-number-without-source-contract")
    .OrderBy(row => row.Path, StringComparer.Ordinal)
    .ThenBy(row => row.LineNumber)
    .Take(40)
    .ToArray();
var versionSummaryRows = revisionFiles
    .Select(file =>
    {
        var normalizedPath = NormalizePath(file);
        var rows = allRows.Where(row => row.Path == normalizedPath).ToArray();
        return new VersionSummary(
            normalizedPath,
            File.ReadLines(file).Count(),
            rows.Length,
            rows.Count(row => row.HasSourceContractToken),
            rows.Count(row => row.HasWzFormulaSignal),
            rows.Count(row => row.HasHiggsFormulaSignal),
            rows.Count(row => row.HasBlockerLanguage));
    })
    .ToArray();

var p194NoCompletionSource = JsonBool(phase194.RootElement, "draftProvidesDirectWzLaw") is false
    && JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource") is false;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var globalObservedSectorVacuumCandidateFound = JsonBool(phase253.RootElement, "globalObservedSectorVacuumCandidateFound") is true;

var intakeReadyCompletionRevisionFindingCount = possibleSourceRows.Length;
var completionRevisionsProvideDirectWzLaw = intakeReadyCompletionRevisionFindingCount > 0
    && possibleSourceRows.Any(row => row.HasWzFormulaSignal);
var completionRevisionsProvideSolvedHiggsSource = intakeReadyCompletionRevisionFindingCount > 0
    && possibleSourceRows.Any(row => row.HasHiggsFormulaSignal);
var completionRevisionsFillSourceContracts = completionRevisionsProvideDirectWzLaw
    && completionRevisionsProvideSolvedHiggsSource
    && allRequiredLineagesPromotable
    && unlockContractFilled;
var newSourceEvidenceStillRequired = !completionRevisionsFillSourceContracts
    && !globalObservedSectorVacuumCandidateFound
    && wzMissingFieldCount == 15
    && higgsMissingFieldCount == 14;

var checks = new[]
{
    new Check(
        "completion-revision-corpus-scanned",
        revisionFiles.Length >= 20 && totalLineCount > 250000,
        $"revisionFileCount={revisionFiles.Length}; totalLineCount={totalLineCount}"),
    new Check(
        "latest-draft-audit-still-negative",
        p194NoCompletionSource,
        $"p194DraftProvidesDirectWzLaw={JsonBool(phase194.RootElement, "draftProvidesDirectWzLaw")}; p194DraftProvidesSolvedHiggsSource={JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource")}"),
    new Check(
        "no-source-contract-lines-in-completion-revisions",
        sourceContractTokenLineCount == 0 && intakeReadyCompletionRevisionFindingCount == 0,
        $"sourceContractTokenLineCount={sourceContractTokenLineCount}; intakeReadyCompletionRevisionFindingCount={intakeReadyCompletionRevisionFindingCount}"),
    new Check(
        "boson-lines-are-blockers-or-protocol-not-promotions",
        bosonSignalLineCount > 0 && blockerLineCount > 0 && !completionRevisionsProvideDirectWzLaw && !completionRevisionsProvideSolvedHiggsSource,
        $"bosonSignalLineCount={bosonSignalLineCount}; blockerLineCount={blockerLineCount}; completionRevisionsProvideDirectWzLaw={completionRevisionsProvideDirectWzLaw}; completionRevisionsProvideSolvedHiggsSource={completionRevisionsProvideSolvedHiggsSource}"),
    new Check(
        "existing-source-contracts-remain-unfilled",
        !allRequiredLineagesPromotable && !unlockContractFilled && wzMissingFieldCount == 15 && higgsMissingFieldCount == 14,
        $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; unlockContractFilled={unlockContractFilled}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "new-source-evidence-still-required",
        newSourceEvidenceStillRequired,
        $"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; completionRevisionsFillSourceContracts={completionRevisionsFillSourceContracts}"),
};

var localCompletionRevisionBosonSourceScanPassed = checks.All(check => check.Passed);
var terminalStatus = localCompletionRevisionBosonSourceScanPassed
    ? "local-completion-revision-boson-source-scan-no-contract-source"
    : "local-completion-revision-boson-source-scan-review-required";

var result = new
{
    phaseId = "phase254-local-completion-revision-boson-source-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    localCompletionRevisionBosonSourceScanPassed,
    completionRevisionFileCount = revisionFiles.Length,
    totalLineCount,
    bosonSignalLineCount,
    sourceContractTokenLineCount,
    physicalNumberLineCount,
    wzFormulaSignalLineCount,
    higgsFormulaSignalLineCount,
    blockerLineCount,
    intakeReadyCompletionRevisionFindingCount,
    completionRevisionsProvideDirectWzLaw,
    completionRevisionsProvideSolvedHiggsSource,
    completionRevisionsFillSourceContracts,
    newSourceEvidenceStillRequired,
    versionSummaryRows,
    possibleSourceRows,
    blockedOrProtocolSampleRows = blockedRows,
    currentBlockerEvidence = new
    {
        phase194 = new
        {
            p194NoCompletionSource,
        },
        phase201 = new
        {
            allRequiredLineagesPromotable,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase245 = new
        {
            unlockContractFilled,
        },
        phase253 = new
        {
            globalObservedSectorVacuumCandidateFound,
        },
    },
    checks,
    decision = localCompletionRevisionBosonSourceScanPassed
        ? "Do not promote older local completion revisions as W/Z/H mass sources. Across the scanned revision corpus, boson/Higgs/VEV/prediction language is protocol, target, blocked, approximate, or open-work language; no Phase201 source-lineage contract fields, separate W/Z source rows, raw gates, solved Higgs scalar-source fields, or registry-grade quantitative mass predictions are present."
        : "Review local completion revision source scan before relying on the draft-source boundary.",
    nextRequiredArtifact = new[]
    {
        "A completion-revision or source artifact with explicit Phase201/209 W/Z source-lineage fields, theorem/derivation id, separate W/Z source rows, raw gates, and target comparison after construction.",
        "A completion-revision or source artifact with explicit Higgs scalar-source operator, identity envelope, massive scalar profile, potential/self-coupling or excitation relation, stability sidecars, and prediction row.",
    },
    sourceEvidence = new
    {
        revisionDir = RevisionDir,
        phase194Path = Phase194Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase253Path = Phase253Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "local_completion_revision_boson_source_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "local_completion_revision_boson_source_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.localCompletionRevisionBosonSourceScanPassed,
        result.completionRevisionFileCount,
        result.totalLineCount,
        result.bosonSignalLineCount,
        result.sourceContractTokenLineCount,
        result.physicalNumberLineCount,
        result.wzFormulaSignalLineCount,
        result.higgsFormulaSignalLineCount,
        result.blockerLineCount,
        result.intakeReadyCompletionRevisionFindingCount,
        result.completionRevisionsProvideDirectWzLaw,
        result.completionRevisionsProvideSolvedHiggsSource,
        result.completionRevisionsFillSourceContracts,
        result.newSourceEvidenceStillRequired,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"localCompletionRevisionBosonSourceScanPassed={localCompletionRevisionBosonSourceScanPassed}");
Console.WriteLine($"completionRevisionFileCount={revisionFiles.Length}");
Console.WriteLine($"totalLineCount={totalLineCount}");
Console.WriteLine($"sourceContractTokenLineCount={sourceContractTokenLineCount}");
Console.WriteLine($"intakeReadyCompletionRevisionFindingCount={intakeReadyCompletionRevisionFindingCount}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static string ClassifyLine(
    bool hasBosonSignal,
    bool hasSourceContractToken,
    bool hasPhysicalNumber,
    bool hasSourceDerivation,
    bool hasBlockerLanguage,
    bool hasWzFormulaSignal,
    bool hasHiggsFormulaSignal)
{
    if (hasSourceContractToken && hasBosonSignal && hasSourceDerivation && !hasBlockerLanguage)
    {
        return "possible-source-contract-line";
    }

    if (hasBlockerLanguage)
    {
        return "blocked-or-protocol-line";
    }

    if ((hasPhysicalNumber || hasWzFormulaSignal || hasHiggsFormulaSignal) && !hasSourceContractToken)
    {
        return "formula-or-number-without-source-contract";
    }

    return "context-line";
}

static string NormalizePath(string path) => path.Replace('\\', '/');

static string Truncate(string text, int maxLength) =>
    text.Length <= maxLength ? text : text[..maxLength];

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record LineFinding(
    string Path,
    int LineNumber,
    bool HasBosonSignal,
    bool HasSourceContractToken,
    bool HasPhysicalNumber,
    bool HasSourceDerivation,
    bool HasBlockerLanguage,
    bool HasWzFormulaSignal,
    bool HasHiggsFormulaSignal,
    string Classification,
    string Text);

sealed record VersionSummary(
    string Path,
    int LineCount,
    int CandidateLineCount,
    int SourceContractTokenLineCount,
    int WzFormulaSignalLineCount,
    int HiggsFormulaSignalLineCount,
    int BlockerLineCount);

sealed record Check(string CheckId, bool Passed, string Detail);
