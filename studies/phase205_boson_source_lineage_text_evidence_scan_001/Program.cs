using System.Text.Json;

const string DefaultOutputDir = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const int MaxFindings = 200;

var outputDir = Environment.GetEnvironmentVariable("PHASE205_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));

var roots = new[] { "docs", "studies", "src", "TheoryCompletitionRevisions" }
    .Where(Directory.Exists)
    .ToArray();
var textFiles = roots
    .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
    .Where(path => !path.Contains("/bin/", StringComparison.Ordinal) && !path.Contains("/obj/", StringComparison.Ordinal))
    .Where(path => !IsReferenceTrackerText(path.Replace('\\', '/').TrimStart('.', '/')))
    .Where(path => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();

var findings = new List<TextFinding>();
var wzReadyEvidenceCount = 0;
var higgsReadyEvidenceCount = 0;

foreach (var path in textFiles)
{
    var lines = File.ReadLines(path).ToArray();
    for (var i = 0; i < lines.Length; i++)
    {
        var line = lines[i];
        var lower = line.ToLowerInvariant();
        var kind = Classify(lower);
        if (kind is null)
            continue;

        var signals = Signals(lower);
        var blockers = Blockers(kind, lower, path);
        var intakeReady = blockers.Length == 0;
        if (kind == "wz-source-lineage" && intakeReady)
            wzReadyEvidenceCount++;
        if (kind == "higgs-source-lineage" && intakeReady)
            higgsReadyEvidenceCount++;

        if (intakeReady || findings.Count < MaxFindings)
        {
            findings.Add(new TextFinding(
                Path: path,
                LineNumber: i + 1,
                CandidateKind: kind,
                IntakeReady: intakeReady,
                Signals: signals,
                Blockers: blockers,
                Excerpt: line.Trim()));
        }
    }
}

var intakeReadyFindingCount = wzReadyEvidenceCount + higgsReadyEvidenceCount;
var terminalStatus = intakeReadyFindingCount > 0
    ? "boson-source-lineage-text-evidence-scan-found-intake-ready-evidence"
    : "boson-source-lineage-text-evidence-scan-no-intake-ready-evidence";

var result = new
{
    phaseId = "phase205-boson-source-lineage-text-evidence-scan",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    scannedTextFileCount = textFiles.Length,
    findingCount = findings.Count,
    storedFindingLimit = MaxFindings,
    wzReadyEvidenceCount,
    higgsReadyEvidenceCount,
    intakeReadyFindingCount,
    findings,
    decision = intakeReadyFindingCount > 0
        ? "Review text evidence and materialize it into Phase201 intake JSON before rerunning prediction gates."
        : "No markdown or C# source text line appears sufficient to fill the Phase201 W/Z or Higgs source-lineage intake contracts.",
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        wzTemplatePath = JsonString(phase201.RootElement, "wzTemplatePath"),
        higgsTemplatePath = JsonString(phase201.RootElement, "higgsTemplatePath"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_source_lineage_text_evidence_scan.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_text_evidence_scan_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.scannedTextFileCount,
        result.findingCount,
        result.storedFindingLimit,
        result.wzReadyEvidenceCount,
        result.higgsReadyEvidenceCount,
        result.intakeReadyFindingCount,
        topFindings = findings.Take(40).ToArray(),
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"scannedTextFileCount={textFiles.Length}");
Console.WriteLine($"findingCount={findings.Count}");
Console.WriteLine($"intakeReadyFindingCount={intakeReadyFindingCount}");

static string? Classify(string lower)
{
    var wz = lower.Contains("w/z", StringComparison.Ordinal)
        || lower.Contains("wz", StringComparison.Ordinal)
        || lower.Contains("w-boson", StringComparison.Ordinal)
        || lower.Contains("z-boson", StringComparison.Ordinal);
    var higgs = lower.Contains("higgs", StringComparison.Ordinal)
        || lower.Contains("scalar-sector source", StringComparison.Ordinal)
        || lower.Contains("scalar source/operator", StringComparison.Ordinal)
        || lower.Contains("massive scalar", StringComparison.Ordinal);
    var lineage = lower.Contains("source lineage", StringComparison.Ordinal)
        || lower.Contains("source-lineage", StringComparison.Ordinal)
        || lower.Contains("direct bridge", StringComparison.Ordinal)
        || lower.Contains("theorem", StringComparison.Ordinal)
        || lower.Contains("self-coupling", StringComparison.Ordinal)
        || lower.Contains("potential", StringComparison.Ordinal);

    if (wz && lineage)
        return "wz-source-lineage";
    if (higgs && lineage)
        return "higgs-source-lineage";
    return null;
}

static string[] Signals(string lower)
{
    var signals = new List<string>();
    if (lower.Contains("target-independent", StringComparison.Ordinal))
        signals.Add("target-independent");
    if (lower.Contains("theorem", StringComparison.Ordinal))
        signals.Add("theorem");
    if (lower.Contains("raw-amplitude", StringComparison.Ordinal) || lower.Contains("raw amplitude", StringComparison.Ordinal))
        signals.Add("raw-amplitude");
    if (lower.Contains("common-bridge", StringComparison.Ordinal) || lower.Contains("common bridge", StringComparison.Ordinal))
        signals.Add("common-bridge");
    if (lower.Contains("target-comparison", StringComparison.Ordinal) || lower.Contains("target comparison", StringComparison.Ordinal))
        signals.Add("target-comparison");
    if (lower.Contains("stability", StringComparison.Ordinal) || lower.Contains("sidecar", StringComparison.Ordinal))
        signals.Add("stability");
    if (lower.Contains("solved", StringComparison.Ordinal))
        signals.Add("solved");
    if (lower.Contains("identity", StringComparison.Ordinal))
        signals.Add("identity");
    return signals.ToArray();
}

static string[] Blockers(string kind, string lower, string path)
{
    var blockers = new List<string>();
    if (!lower.Contains("target-independent", StringComparison.Ordinal))
        blockers.Add("line does not assert target-independent construction");
    if (IsRequirementOrNegativeText(lower))
        blockers.Add("line is requirement/contract/negative text, not source evidence");
    if (IsGeneratedAuditOrImplementationText(path))
        blockers.Add("line is generated audit/implementation text, not source evidence");

    if (kind == "wz-source-lineage")
    {
        if (!lower.Contains("theorem", StringComparison.Ordinal) && !lower.Contains("derivation", StringComparison.Ordinal))
            blockers.Add("line does not assert W/Z theorem or derivation");
        if (!lower.Contains("w", StringComparison.Ordinal) || !lower.Contains("z", StringComparison.Ordinal))
            blockers.Add("line does not mention both W and Z");
        foreach (var gate in new[] { "raw", "common", "target", "stability" })
        {
            if (!lower.Contains(gate, StringComparison.Ordinal))
                blockers.Add($"line does not mention {gate} gate/evidence");
        }
    }
    else
    {
        foreach (var required in new[] { "source", "operator", "identity", "massive", "potential" })
        {
            if (!lower.Contains(required, StringComparison.Ordinal))
                blockers.Add($"line does not mention Higgs {required} evidence");
        }
        if (!lower.Contains("stability", StringComparison.Ordinal) && !lower.Contains("sidecar", StringComparison.Ordinal))
            blockers.Add("line does not mention Higgs stability sidecars");
    }

    return blockers.Distinct().ToArray();
}

static bool IsRequirementOrNegativeText(string lower)
{
    var negativeOrRequirementMarkers = new[]
    {
        "required",
        "requires",
        "require ",
        "must ",
        "template",
        "contract",
        "missing",
        "lack",
        "lacks",
        "not ",
        " no ",
        "next ",
        "blocker",
        "blocked",
        "failed",
        "do not",
    };
    return negativeOrRequirementMarkers.Any(marker => lower.Contains(marker, StringComparison.Ordinal));
}

static bool IsGeneratedAuditOrImplementationText(string path) =>
    path.Contains("docs/Phases/Implementation/", StringComparison.Ordinal)
    || path.Contains("docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md", StringComparison.Ordinal)
    || path.Contains("studies/phase200_", StringComparison.Ordinal)
    || path.Contains("studies/phase201_", StringComparison.Ordinal)
    || path.Contains("studies/phase202_", StringComparison.Ordinal)
    || path.Contains("studies/phase203_", StringComparison.Ordinal)
    || path.Contains("studies/phase204_", StringComparison.Ordinal)
    || path.Contains("studies/phase205_", StringComparison.Ordinal)
    || path.Contains("studies/phase206_", StringComparison.Ordinal)
    || path.Contains("studies/phase207_", StringComparison.Ordinal)
    || path.Contains("studies/phase208_", StringComparison.Ordinal)
    || path.Contains("studies/phase209_", StringComparison.Ordinal)
    || path.Contains("studies/phase210_", StringComparison.Ordinal)
    || path.Contains("studies/phase211_", StringComparison.Ordinal)
    || path.Contains("studies/phase212_", StringComparison.Ordinal)
    || path.Contains("studies/phase213_", StringComparison.Ordinal)
    || path.Contains("studies/phase214_", StringComparison.Ordinal)
    || path.Contains("studies/phase215_", StringComparison.Ordinal)
    || path.Contains("studies/phase216_", StringComparison.Ordinal)
    || path.Contains("studies/phase217_", StringComparison.Ordinal)
    || path.Contains("studies/phase218_", StringComparison.Ordinal)
    || path.Contains("studies/phase219_", StringComparison.Ordinal)
    || path.Contains("studies/phase220_", StringComparison.Ordinal)
    || path.Contains("studies/phase221_", StringComparison.Ordinal)
    || path.Contains("studies/phase222_", StringComparison.Ordinal)
    || path.Contains("studies/phase223_", StringComparison.Ordinal)
    || path.Contains("studies/phase224_", StringComparison.Ordinal)
    || path.Contains("studies/phase225_", StringComparison.Ordinal)
    || path.Contains("studies/phase226_", StringComparison.Ordinal)
    || path.Contains("studies/phase227_", StringComparison.Ordinal)
    || path.Contains("studies/phase228_", StringComparison.Ordinal)
    || path.Contains("studies/phase229_", StringComparison.Ordinal)
    || path.Contains("studies/phase230_", StringComparison.Ordinal)
    || path.Contains("studies/phase231_", StringComparison.Ordinal)
    || path.Contains("studies/phase232_", StringComparison.Ordinal)
    || path.Contains("studies/phase233_", StringComparison.Ordinal)
    || path.Contains("studies/phase234_", StringComparison.Ordinal)
    || path.Contains("studies/phase235_", StringComparison.Ordinal)
    || path.Contains("studies/phase236_", StringComparison.Ordinal)
    || path.Contains("studies/phase237_", StringComparison.Ordinal)
    || path.Contains("studies/phase238_", StringComparison.Ordinal)
    || path.Contains("studies/phase239_", StringComparison.Ordinal)
    || path.Contains("studies/phase240_", StringComparison.Ordinal)
    || path.Contains("studies/phase241_", StringComparison.Ordinal)
    || path.Contains("studies/phase242_", StringComparison.Ordinal)
    || path.Contains("studies/phase243_", StringComparison.Ordinal)
    || path.Contains("studies/phase244_", StringComparison.Ordinal)
    || path.Contains("studies/phase245_", StringComparison.Ordinal)
    || path.Contains("studies/phase246_", StringComparison.Ordinal)
    || path.Contains("studies/phase247_", StringComparison.Ordinal)
    || path.Contains("studies/phase248_", StringComparison.Ordinal)
    || path.Contains("studies/phase249_", StringComparison.Ordinal)
    || path.Contains("studies/phase250_", StringComparison.Ordinal)
    || path.Contains("studies/phase251_", StringComparison.Ordinal)
    || path.Contains("studies/phase252_", StringComparison.Ordinal)
    || path.Contains("studies/phase253_", StringComparison.Ordinal)
    || path.Contains("studies/phase254_", StringComparison.Ordinal)
    || path.Contains("studies/phase255_", StringComparison.Ordinal)
    || path.Contains("studies/phase256_", StringComparison.Ordinal)
    || path.Contains("studies/phase257_", StringComparison.Ordinal)
    || path.Contains("studies/phase258_", StringComparison.Ordinal)
    || path.Contains("studies/phase259_", StringComparison.Ordinal)
    || path.Contains("studies/phase260_", StringComparison.Ordinal)
    || path.Contains("studies/phase261_", StringComparison.Ordinal)
    || path.Contains("studies/phase262_", StringComparison.Ordinal)
    || path.Contains("studies/phase263_", StringComparison.Ordinal)
    || path.Contains("studies/phase264_", StringComparison.Ordinal)
    || path.Contains("studies/phase265_", StringComparison.Ordinal)
    || path.Contains("studies/phase266_", StringComparison.Ordinal)
    || path.Contains("studies/phase267_", StringComparison.Ordinal)
    || path.Contains("studies/phase268_", StringComparison.Ordinal)
    || path.Contains("studies/phase269_", StringComparison.Ordinal)
    || path.Contains("studies/phase270_", StringComparison.Ordinal)
    || path.Contains("studies/phase271_", StringComparison.Ordinal)
    || path.Contains("studies/phase272_", StringComparison.Ordinal)
    || path.Contains("studies/phase273_", StringComparison.Ordinal)
    || path.Contains("studies/phase274_", StringComparison.Ordinal)
    || path.Contains("studies/phase275_", StringComparison.Ordinal)
    || path.Contains("studies/phase276_", StringComparison.Ordinal)
    || path.Contains("studies/phase277_", StringComparison.Ordinal)
    || path.Contains("studies/phase278_", StringComparison.Ordinal)
    || path.Contains("studies/phase279_", StringComparison.Ordinal)
    || path.Contains("studies/phase280_", StringComparison.Ordinal)
    || path.Contains("studies/phase281_", StringComparison.Ordinal)
    || path.Contains("studies/phase282_", StringComparison.Ordinal)
    || path.Contains("studies/phase283_", StringComparison.Ordinal)
    || path.Contains("studies/phase284_", StringComparison.Ordinal)
    || path.Contains("studies/phase285_", StringComparison.Ordinal)
    || path.Contains("studies/phase286_", StringComparison.Ordinal)
    || path.Contains("studies/phase287_", StringComparison.Ordinal)
    || path.Contains("studies/phase288_", StringComparison.Ordinal)
    || path.Contains("studies/phase289_", StringComparison.Ordinal)
    || path.Contains("studies/phase290_", StringComparison.Ordinal)
    || path.Contains("studies/phase291_", StringComparison.Ordinal)
    || path.Contains("studies/phase292_", StringComparison.Ordinal)
    || path.Contains("studies/phase293_", StringComparison.Ordinal)
    || path.Contains("studies/phase294_", StringComparison.Ordinal)
    || path.Contains("studies/phase295_", StringComparison.Ordinal)
    || path.Contains("studies/phase296_", StringComparison.Ordinal)
    || path.Contains("studies/phase297_", StringComparison.Ordinal)
    || path.Contains("studies/phase298_", StringComparison.Ordinal)
    || path.Contains("studies/phase299_", StringComparison.Ordinal)
    || path.Contains("studies/phase300_", StringComparison.Ordinal)
    || path.Contains("studies/phase301_", StringComparison.Ordinal)
    || path.Contains("studies/phase302_", StringComparison.Ordinal)
    || path.Contains("studies/phase303_", StringComparison.Ordinal)
    || path.Contains("studies/phase304_", StringComparison.Ordinal)
    || path.Contains("studies/phase305_", StringComparison.Ordinal)
    || path.Contains("studies/phase306_", StringComparison.Ordinal)
    || path.Contains("studies/phase307_", StringComparison.Ordinal)
    || path.Contains("studies/phase308_", StringComparison.Ordinal)
    || path.Contains("studies/phase309_", StringComparison.Ordinal)
    || path.Contains("studies/phase310_", StringComparison.Ordinal)
    || path.Contains("studies/phase311_", StringComparison.Ordinal)
    || path.Contains("studies/phase312_", StringComparison.Ordinal)
    || path.Contains("studies/phase313_", StringComparison.Ordinal)
    || path.Contains("studies/phase314_", StringComparison.Ordinal)
    || path.Contains("studies/phase315_", StringComparison.Ordinal)
    || path.Contains("studies/phase316_", StringComparison.Ordinal)
    || path.Contains("studies/phase317_", StringComparison.Ordinal)
    || path.Contains("studies/phase318_", StringComparison.Ordinal)
    || path.Contains("studies/phase319_", StringComparison.Ordinal)
    || path.Contains("studies/phase320_", StringComparison.Ordinal)
    || path.Contains("studies/phase321_", StringComparison.Ordinal)
    || path.Contains("studies/phase322_", StringComparison.Ordinal)
    || path.Contains("studies/phase323_", StringComparison.Ordinal)
    || path.Contains("studies/phase324_", StringComparison.Ordinal)
    || path.Contains("studies/phase325_", StringComparison.Ordinal)
    || path.Contains("studies/phase326_", StringComparison.Ordinal)
    || path.Contains("studies/phase327_", StringComparison.Ordinal)
    || path.Contains("studies/phase328_", StringComparison.Ordinal)
    || path.Contains("studies/phase329_", StringComparison.Ordinal)
    || path.Contains("studies/phase330_", StringComparison.Ordinal)
    || path.Contains("studies/phase331_", StringComparison.Ordinal)
    || path.Contains("studies/phase332_", StringComparison.Ordinal)
    || path.Contains("studies/phase333_", StringComparison.Ordinal)
    || path.Contains("studies/phase334_", StringComparison.Ordinal)
    || path.Contains("studies/phase335_", StringComparison.Ordinal)
    || path.Contains("studies/phase336_", StringComparison.Ordinal)
    || path.Contains("studies/phase337_", StringComparison.Ordinal)
    || path.Contains("studies/phase338_", StringComparison.Ordinal)
    || path.Contains("studies/phase339_", StringComparison.Ordinal)
    || path.Contains("studies/phase340_", StringComparison.Ordinal)
    || path.Contains("studies/phase341_", StringComparison.Ordinal)
    || path.Contains("studies/phase342_", StringComparison.Ordinal)
    || path.Contains("studies/phase343_", StringComparison.Ordinal)
    || path.Contains("studies/phase344_", StringComparison.Ordinal)
    || path.Contains("studies/phase345_", StringComparison.Ordinal)
    || path.Contains("studies/phase346_", StringComparison.Ordinal)
    || path.Contains("studies/phase347_", StringComparison.Ordinal)
    || path.Contains("studies/phase348_", StringComparison.Ordinal)
    || path.Contains("studies/phase349_", StringComparison.Ordinal)
    || path.Contains("studies/phase350_", StringComparison.Ordinal)
    || path.Contains("studies/phase351_", StringComparison.Ordinal)
    || path.Contains("studies/phase352_", StringComparison.Ordinal)
    || path.Contains("studies/phase353_", StringComparison.Ordinal)
    || path.Contains("studies/phase354_", StringComparison.Ordinal);

static bool IsReferenceTrackerText(string normalizedPath) =>
    normalizedPath == "ExperimentReferences.md"
    || normalizedPath.StartsWith("docs/Reference/ExperimentReferences/", StringComparison.Ordinal);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

sealed record TextFinding(
    string Path,
    int LineNumber,
    string CandidateKind,
    bool IntakeReady,
    IReadOnlyList<string> Signals,
    IReadOnlyList<string> Blockers,
    string Excerpt);
